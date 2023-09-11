// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using Common;
    using DataBridge.Common;
    using IES.ActionLogic.Validation;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Logic for the Rate Controller.
    /// </summary>
    public class RateControllerLogic : RdmControllerLogic, IRateControllerLogic
    {
        /// <summary>
        /// Rate Detail Loader
        /// </summary>
        private readonly IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// The common data mapper
        /// </summary>
        private readonly ICommonDataMapper commonDataMapper;

        /// <summary>
        /// Home Controller Logic
        /// </summary>
        private readonly IHomeControllerLogic homeControllerLogic;

        /// <summary>
        /// Burden Pool Loader
        /// </summary>
        private readonly IBurdenPoolLoader burdenPoolLoader;

        /// <summary>
        /// Section Loader
        /// </summary>
        private readonly ISectionLoader sectionLoader;

        /// <summary>
        /// The replication loader
        /// </summary>
        private IRateCodeReplicationLoader replicationLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateControllerLogic"/> class.
        /// </summary>
        /// <param name="rateDetailLoader">The rate detail loader.</param>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="homeControllerLogic">The home controller logic.</param>
        /// <param name="burdenPoolLoader">The burden pool loader.</param>
        /// <param name="sectionLoader">The section loader.</param>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="revisionMediator">Revision Mediator</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        /// <param name="replicationLoader">The replication loader.</param>
        public RateControllerLogic(IRateDetailLoader rateDetailLoader, ICommonDataMapper commonDataMapper, IHomeControllerLogic homeControllerLogic, IBurdenPoolLoader burdenPoolLoader, ISectionLoader sectionLoader,
            IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo, IRateCodeReplicationLoader replicationLoader)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)

        {
            this.rateDetailLoader = rateDetailLoader;
            this.commonDataMapper = commonDataMapper;
            this.homeControllerLogic = homeControllerLogic;
            this.burdenPoolLoader = burdenPoolLoader;
            this.sectionLoader = sectionLoader;
            this.replicationLoader = replicationLoader;
        }

        /// <summary>
        /// Gets the rates and related data.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <param name="revisions">Collection of RDM revisions</param>
        /// <returns>Rates for the version requested</returns>
        public RateGridModelView GetRatesByVersion(int? id, ICollection<RevisionModelView> revisions)
        {
            RevisionModelView revision = id.HasValue ? revisions.FirstOrDefault(r => r.Id == id.Value) : revisions.LastOrDefault();
            if (revision == null)
            {
                throw new GenValidationException("Revision not found.");
            }

            ICollection<OptionModelView> commercialBurdenPoolOptions;
            ICollection<OptionModelView> governmentBurdenPoolOptions;
            this.burdenPoolLoader.GetBurdenPoolOptions(revision.Id, out commercialBurdenPoolOptions, out governmentBurdenPoolOptions);

            RateGridModelView model = new RateGridModelView
            {
                SelectedRevisionId = revision.Id,
                AdminUser = this.IsRDMAdminUser,
                CobraAdminUser = this.IsRDMCobraAdminUser,
                Versions = this.RevisionMediator.GetRevisionOptions(revisions),
                Rates = this.rateDetailLoader.GetRatesByRevision(revision),
                Sections = this.sectionLoader.RetrieveSectionsAsOptions(revision),
                RateCategories = ExtensionMethods.GetOptions<RateCategory>().OrderBy(x => x.Label).ToList(),
                LockInfo = this.homeControllerLogic.GetCurrentLockInfo(LockArea.RDMRates),
                RateTypes = ExtensionMethods.GetOptions<RateType>().OrderBy(x => x.Label).ToList(),
                ResourceClasses = this.commonDataMapper.GetResourceClassOptions(revision.Id),
                ResourceTypes = ExtensionMethods.GetOptions<DirectRateMappingResourceType>().OrderBy(x => x.Label).ToList(),
                CommercialBurdenPools = commercialBurdenPoolOptions,
                GovernmentBurdenPools = governmentBurdenPoolOptions
            };

            if (this.WipRevision.Id == revision.Id)
            {
                model.ReplicationValidationMessages = this.rateDetailLoader.VerifyRateCodeReplication(revision.Id);
            }

            return model;
        }

        /// <summary>
        /// Get the Version differences between the two given IDs for the Rates 
        /// </summary>
        /// <param name="revisionOptions">Revisions as an options list</param>
        /// <param name="firstId">ID of the first revision to compare</param>
        /// <param name="secondId">ID of the second revision to compare. -1 for previous revision.</param>
        /// <returns>Version differences for the rates between the two versions</returns>
        public ICollection<RateDetailModelView> GetRatesVersionDifferences(ICollection<RevisionOptionModelView> revisionOptions, int firstId, int secondId)
        {
            RevisionOptionModelView firstRevision = revisionOptions.FirstOrDefault(r => r.Id == firstId);
            RevisionOptionModelView secondRevision = revisionOptions.FirstOrDefault(r => secondId <= 0 ? r.Id < firstId : r.Id == secondId);

            int.TryParse(firstRevision.Revision, out int firstRevisionNumber);

            int secondRevisionNumber = -1;
            if (secondRevision != null)
            {
                int.TryParse(secondRevision.Revision, out secondRevisionNumber);
            }

            ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetComparableRates(firstRevision, secondRevision);

            return rates;
        }

        /// <summary>
        /// Validates all the imported rates exist. We will not load only part of an import file.
        /// Also populates the RateCategory and RateCategoryDescription fields for each model view.
        /// </summary>
        /// <param name="existingRates">Collection of Rates from DB.</param>
        /// <param name="importRateDetails">Collection of imported rate details to validate.</param>
        /// <returns>A list of validation errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateImportedRates(ICollection<RateDetailModelView> existingRates, ICollection<RateDetailModelView> importRateDetails)
        {
            if (existingRates == null)
            {
                throw new ArgumentNullException(nameof(existingRates));
            }

            if (importRateDetails == null)
            {
                throw new ArgumentNullException(nameof(importRateDetails));
            }

            // Make sure all the imported rates exist. We will not load only part of an import file.
            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            foreach (RateDetailModelView rdmv in importRateDetails)
            {
                try
                {
                    RateDetailModelView rate = existingRates.SingleOrDefault(x => x.RateCode.Equals(rdmv.RateCode));
                    if (rate == null)
                    {
                        string validationString =
                            string.Format(RateMappingValidationConstants.RATEDETAILS_RATECODE_MISSING,
                                rdmv.RateCode);
                        validationErrors.Add(new ValidationMessage(validationString));
                    }
                    else
                    {
                        // populate Rate Category to enable subsequent code (in ValidateRateDetailModelViews) to determine rate value precision.
                        rdmv.RateCategory = rate.RateCategory;
                        rdmv.RateCategoryDescription = rate.RateCategoryDescription;
                    }
                }
                catch (InvalidOperationException)
                {
                    string validationString =
                        string.Format(RateMappingValidationConstants.RATEDETAILS_DUPLICATE_RATECODE,
                            rdmv.RateCode);
                    validationErrors.Add(new ValidationMessage(validationString));
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// ValidateRateDetailModelViews when saving rate values
        /// </summary>
        /// <param name="rateDetailModelViews">Collection of RateDetailModelViews to validate.</param>
        /// <param name="startYear">Revision Start Year</param>
        /// <param name="endYear">Revision End Year</param>
        /// <returns>A list of validation errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateRateDetailModelViews(Collection<RateDetailModelView> rateDetailModelViews, int startYear, int endYear)
        {
            if (rateDetailModelViews == null)
            {
                throw new ArgumentNullException(nameof(rateDetailModelViews));
            }

            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            foreach (RateDetailModelView rdmv in rateDetailModelViews.Where(x => !x.IsDeleted))
            {
                // Get the precision before we loop through years.
                ValidationMessage precisionMsg = this.SetPrecision(rdmv);
                if (precisionMsg != null)
                {
                    validationErrors.Add(precisionMsg);
                }

                // Check each year against precision.
                if (rdmv.Values != null && rdmv.RatePrecision != -1)
                {
                    foreach (RateYearModelView rateYear in rdmv.Values)
                    {
                        // Make sure rate year is within configured range
                        if (rateYear.Year < startYear || rateYear.Year > endYear)
                        {
                            string validationString = string.Format(RateMappingValidationConstants.RATEDETAILS_RATEYEAR_ERROR,
                                rdmv.RateCode, rateYear.Year, startYear, endYear);

                            validationErrors.Add(new ValidationMessage(validationString));
                        }

                        if (rateYear.Value != null)
                        {
                            bool isNonLaborEscalation = rdmv.RateCategoryDescription == RateCategory.NonLaborEscalationFactor.GetDescription()
                                || rdmv.RateCategoryDescription == RateCategory.NonLaborEscalationPercentage.GetDescription();

                            // If the rounded value does not equal the actual value, we have too many decimal places.
                            if (decimal.Round((decimal)rateYear.Value, rdmv.RatePrecision) != rateYear.Value || 
                                (rateYear.Value < 0 && !isNonLaborEscalation))
                            {
                                string validationString = string.Format(isNonLaborEscalation ? RateMappingValidationConstants.RATEDETAILS_RATEPRECISION_ERROR_ALLOW_NEGATIVE : RateMappingValidationConstants.RATEDETAILS_RATEPRECISION_ERROR,
                                                                        rdmv.RateCode, rateYear.Year, rateYear.Value, rdmv.RateCategoryDescription, rdmv.RatePrecision);

                                validationErrors.Add(new ValidationMessage(validationString));
                            }
                        }
                    }
                }

                // Validate Rate ProPricer Mappings
                validationErrors.AddRange(this.ValidateRateProPricerMappings(rdmv));
            }

            return validationErrors;
        }

        /// <summary>
        /// Validate imported rate codes and associated mapping data.
        /// </summary>
        /// <param name="existingRateCodes">Existing Rate Code MVs from DB</param>
        /// <param name="importedRateCodes">Imported Rate Code MVs to validate.</param>
        /// <returns>A list of validation errors (if any)</returns>
        public ICollection<ValidationMessage> ValidateImportedRateCodes(RateDetailModelView[] existingRateCodes, RateDetailModelView[] importedRateCodes)
        {
            if (existingRateCodes == null)
            {
                throw new ArgumentNullException(nameof(existingRateCodes));
            }

            if (importedRateCodes == null)
            {
                throw new ArgumentNullException(nameof(importedRateCodes));
            }

            // validate rate codes are unique
            ICollection<ValidationMessage> validationErrors = this.ValidateRateCodeIsUnique(importedRateCodes);

            // perform additional validations against individual rate code rows 
            int rowIndex = -1;
            foreach (RateDetailModelView importedRateCode in importedRateCodes)
            {
                rowIndex++;
                // validate using model view annotations 
                ICollection<ValidationMessage> modelValidationErrors = this.ValidateRateCodeObject(importedRateCode);
                
                // validate ProPricer mappings
                ICollection<ValidationMessage> rowValidationErrors = this.ValidateRateProPricerMappings(importedRateCode);

                // Augment each validation message with the row index and add to the list of errors
                foreach (ValidationMessage validationError in modelValidationErrors.Concat(rowValidationErrors))
                {
                    validationError.RowIndex = rowIndex;
                    validationErrors.Add(validationError);
                }
            }

            return validationErrors.OrderBy(x => x.RowIndex).ToList();
        }

        /// <summary>
        /// Runs data validation using model view annotations
        /// </summary>
        /// <param name="rateDetailModelView">Object to validate</param>
        /// <returns>A list of validation errors (if any).</returns>
        private ICollection<ValidationMessage> ValidateRateCodeObject(RateDetailModelView rateDetailModelView)
        {
            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();
            ValidationContext ctx = new ValidationContext(rateDetailModelView, null, null);
            List<ValidationResult> errors = new List<ValidationResult>();
            Validator.TryValidateObject(rateDetailModelView, ctx, errors, true);

            foreach (ValidationResult error in errors)
            {
                validationMessages.Add(new ValidationMessage
                {
                    ValidationIssue = error.ErrorMessage.Replace(@"&nbsp;", string.Empty),
                    TreatAsWarning = false
                });
            }

            return validationMessages;
        }

        /// <summary>
        /// Validate no duplicate rate codes exist
        /// </summary>
        /// <param name="importedRateCodes">Imported Rate Code MVs to validate.</param>
        /// <returns>A list of validation errors (if any).</returns>
        private ICollection<ValidationMessage> ValidateRateCodeIsUnique(RateDetailModelView[] importedRateCodes)
        {
            ICollection<ValidationMessage> validationMessages = new Collection<ValidationMessage>();

            // check for duplicates within the imported rate codes
            var duplicates = importedRateCodes.Select((x, i) => new { index = i, value = x })
                .GroupBy(x => new { x.value.RateCode })
                .Where(x => x.Skip(1).Any()).ToArray();

            if (duplicates.Any())
            {
                // generate validation messages for each invalid record
                foreach (var duplicate in duplicates)
                {
                    foreach (var row in duplicate)
                    {
                        validationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = string.Format(RateMappingValidationConstants.RATEDETAILS_RATECODE_NOT_UNIQUE, row.value.RateCode),
                            RowIndex = row.index,
                            FieldName = "Rate Code=" + row.value.RateCode
                        });
                    }
                }
            }

            return validationMessages;
        }

        /// <summary>
        /// Looks up the precision of the rates based on RateCategory.
        /// </summary>
        /// <param name="rateDetailModelView">RateDetailModelView RateCategory to validate precision against.</param>
        /// <returns>A list of validation errors (if any).</returns>
        private ValidationMessage SetPrecision(RateDetailModelView rateDetailModelView)
        {
            if (string.IsNullOrEmpty(rateDetailModelView.RateCategoryDescription))
            {
                return new ValidationMessage(RateMappingValidationConstants.RATEDETAILS_RATECATEGORY_REQUIRED);
            }

            rateDetailModelView.RatePrecision = RateFormatter.GetRatePrecision(RateTarget.Rate, rateDetailModelView.RateCategoryDescription);

            if (rateDetailModelView.RatePrecision == -1)
            {
                return new ValidationMessage(RateMappingValidationConstants.RATEDETAILS_RATEPRECISION_REQUIRED, rateDetailModelView.RateCategoryDescription);
            }

            return null;
        }

        /// <summary>
        /// ValidateRateProPricerMappings when saving dialog values
        /// </summary>
        /// <param name="rateDetailModelView">RateDetailModelView</param>
        /// <returns>A list of validation errors (if any).</returns>
        private ICollection<ValidationMessage> ValidateRateProPricerMappings(RateDetailModelView rateDetailModelView)
        {
            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            bool commercialBurdenPoolSet = rateDetailModelView.CommercialBurdenPoolId != null && rateDetailModelView.CommercialBurdenPoolId > 0;
            bool governmentBurdenPoolSet = rateDetailModelView.GovernmentBurdenPoolId != null && rateDetailModelView.GovernmentBurdenPoolId > 0;
            bool rateTypeSet = rateDetailModelView.RateType != null && rateDetailModelView.RateType != RateType.NotSet;
            bool resourceTypeSet = rateDetailModelView.ResourceType != null && rateDetailModelView.ResourceType != DirectRateMappingResourceType.None;

            // if nothing is entered or all values have been cleared by user, nothing to validate
            if (!commercialBurdenPoolSet && !governmentBurdenPoolSet && 
                this.IsAllRateDescriptionsAndResourceClassesEmpty(rateDetailModelView) &&
                !rateTypeSet && !resourceTypeSet)
            {
                return validationErrors; 
            }

            IReadOnlyCollection<string> allowEmptyBurdenPools = RateMappingValidationConstants.ALLOW_EMPTY_BURDEN_POOLS;
            
            // Commercial and Government Burden Pools must both be populated (or null for certain rates)
            if (!commercialBurdenPoolSet && governmentBurdenPoolSet)
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_COMMERCIAL_BURDENPOOL_REQUIRED));
            }
            else if (!governmentBurdenPoolSet && commercialBurdenPoolSet)
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_GOVERNMENT_BURDENPOOL_REQUIRED));
            }
            else if (!commercialBurdenPoolSet && !governmentBurdenPoolSet &&
                     !rateDetailModelView.RateCode.EndsWith("NL") &&
                     !allowEmptyBurdenPools.Any(x => rateDetailModelView.RateCode.Contains(x)))
            {
                // Can only be blank for Resources ******NL, Mileage, NLBESCCH, Travel Escalation, Travel Factor, or TRAVLESC
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_COMMERCIAL_AND_GOVERNMENT_BURDENPOOL_REQUIRED));   
            }

            if (!rateTypeSet)
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RATETYPE_REQUIRED));
            }

            if (!resourceTypeSet)
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RESOURCETYPE_REQUIRED));
            }

            // if additional labor rates are selected, at least one of the descriptions needs to be filled in 
            if (rateDetailModelView.GenerateAdditionalDirectLaborRates && !this.IsAnyExtendedRateDescriptionPopulated(rateDetailModelView))
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_ADDITIONAL_DESCRIPTIONS_REQUIRED));
            }

            // if additional labor rates not selected, need single description 
            if (rateDetailModelView.GenerateAdditionalDirectLaborRates == false && string.IsNullOrEmpty(rateDetailModelView.RateDescription))
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_DESCRIPTION_REQUIRED));
            }

            // don't allow both base and extended rate descriptions to be filled in (this may happen during an import)
            if (!string.IsNullOrEmpty(rateDetailModelView.RateDescription) && this.IsAnyExtendedRateDescriptionPopulated(rateDetailModelView))
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_DESCRIPTION_INVALID));
            }

            // don't allow a resource class without a corresponding rate description (this may happen during an import)
            if (this.IsAnyResourceClassMissingCorrespondingRateDescription(rateDetailModelView))
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RESOURCE_CLASS_WITHOUT_RATE_DESCRIPTION));
            }

            // Resource Type must be labor when additional ProPricer rate descriptions are specified.
            if (!rateDetailModelView.ResourceType.Equals(DirectRateMappingResourceType.Labor) && this.IsAnyExtendedRateDescriptionPopulated(rateDetailModelView))
            {
                validationErrors.Add(this.FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RESOURCETYPE_INVALID));
            }

            return validationErrors;
        }

        /// <summary>
        /// Helper method to determine if all of the Rate Descriptions and Resource Classes are empty.
        /// </summary>
        /// <param name="rateDetailModelView">The rate detail mv</param>
        /// <returns>True if all of the Rate Descriptions and Resource Classes are empty; False otherwise.</returns>
        private bool IsAllRateDescriptionsAndResourceClassesEmpty(RateDetailModelView rateDetailModelView)
        {
            return string.IsNullOrEmpty(rateDetailModelView.RateDescription) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass, rateDetailModelView.ResourceClassId) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription1) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass1, rateDetailModelView.ResourceClassId1) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription2) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass2, rateDetailModelView.ResourceClassId2) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription3) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass3, rateDetailModelView.ResourceClassId3) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription4) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass4, rateDetailModelView.ResourceClassId4) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription5) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass5, rateDetailModelView.ResourceClassId5) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription6) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass6, rateDetailModelView.ResourceClassId6) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription7) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass7, rateDetailModelView.ResourceClassId7) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription8) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass8, rateDetailModelView.ResourceClassId8) &&
                   string.IsNullOrEmpty(rateDetailModelView.RateDescription9) && !this.IsResourceClassPopulated(rateDetailModelView.ResourceClass9, rateDetailModelView.ResourceClassId9);
        }

        /// <summary>
        /// Helper method to determine if any of the extended Resource Classes (i.e. 1-9) are populated without a corresponding Rate Description.
        /// </summary>
        /// <param name="rateDetailModelView">The rate detail mv</param>
        /// <returns>True if any of the extended Resource Classes (i.e. 1-9) are populated without a corresponding Rate Description; False otherwise.</returns>
        private bool IsAnyResourceClassMissingCorrespondingRateDescription(RateDetailModelView rateDetailModelView)
        {
            return (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass, rateDetailModelView.ResourceClassId)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription1) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass1, rateDetailModelView.ResourceClassId1)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription2) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass2, rateDetailModelView.ResourceClassId2)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription3) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass3, rateDetailModelView.ResourceClassId3)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription4) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass4, rateDetailModelView.ResourceClassId4)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription5) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass5, rateDetailModelView.ResourceClassId5)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription6) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass6, rateDetailModelView.ResourceClassId6)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription7) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass7, rateDetailModelView.ResourceClassId7)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription8) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass8, rateDetailModelView.ResourceClassId8)) ||
                   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription9) && this.IsResourceClassPopulated(rateDetailModelView.ResourceClass8, rateDetailModelView.ResourceClassId9));
        }

        /// <summary>
        /// Helper method to determine if any of the extended Rate Descriptions (i.e. 1-9) are populated.
        /// </summary>
        /// <param name="rateDetailModelView">The rate detail mv</param>
        /// <returns>True if any of the extended Rate Descriptions (i.e. 1-9) are populated; False otherwise.</returns>
        private bool IsAnyExtendedRateDescriptionPopulated(RateDetailModelView rateDetailModelView)
        {
            return !string.IsNullOrEmpty(rateDetailModelView.RateDescription1) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription2) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription3) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription4) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription5) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription6) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription7) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription8) ||
                   !string.IsNullOrEmpty(rateDetailModelView.RateDescription9);
        }

        /// <summary>
        /// Helper method to determine if a Resource Class is populated.
        /// </summary>
        /// <param name="resourceClass">Resource Class string</param>
        /// <param name="resourceClassId">Resource Class Id</param>
        /// <returns>True if Resource Class is populated; False otherwise.</returns>
        private bool IsResourceClassPopulated(string resourceClass, int? resourceClassId)
        {
            return !string.IsNullOrWhiteSpace(resourceClass) || (resourceClassId.HasValue && resourceClassId.Value > 0);
        }

        /// <summary>
        /// Formats the validation method with rate details
        /// </summary>
        /// <param name="rateDetailModelView">The rate detail mv</param>
        /// <param name="message">Validation message to format</param>
        /// <returns>Formatted validation message</returns>
        private ValidationMessage FormatValidationMessage(RateDetailModelView rateDetailModelView, string message)
        {
            string validationString = string.Format("{0} - {1}: " + message, rateDetailModelView.RateCode,
                rateDetailModelView.Description);

            return new ValidationMessage(validationString);
        }

        /// <summary>
        /// Load imported rates into database.
        /// </summary>
        /// <param name="existingRates">Collection of Rates from DB.</param>
        /// <param name="importedRates">Validated collection of rates to import.</param>
        /// <returns>Collection of imported rates for the current version.</returns>
        public ICollection<RateDetailModelView> LoadImportedRates(ICollection<RateDetailModelView> existingRates, ICollection<RateDetailModelView> importedRates)
        {
            if (existingRates == null)
            {
                throw new ArgumentNullException(nameof(existingRates));
            }

            Collection<RateDetailModelView> importResults = new Collection<RateDetailModelView>();

            if (importedRates == null)
            {
                throw new GenValidationException("Imported Rates are null.");
            }

            this.ReplicateRateCodes(importedRates);

            try
            {
                foreach (RateDetailModelView importedRateDetailMV in importedRates)
                {
                    // Find and update existing RateCode.
                    RateDetailModelView existingRateDetailMV = existingRates.FirstOrDefault(x => x.RateCode == importedRateDetailMV.RateCode);
                    if (existingRateDetailMV != null)
                    {
                        if (importedRateDetailMV.Values != null)
                        {
                            bool modified = false;

                            // Overwrite/Add RateCodeYears
                            foreach (RateYearModelView importedRateYearMV in importedRateDetailMV.Values.Where(x => x.Dirty))
                            {
                                RateYearModelView existingRateYearMV = existingRateDetailMV.Values.FirstOrDefault(x => x.Year == importedRateYearMV.Year);

                                if (existingRateYearMV != null)
                                {
                                    // RateYear already exists - update.
                                    if (existingRateYearMV.Value != importedRateYearMV.Value)
                                    {
                                        modified = true;
                                        existingRateYearMV.Value = importedRateYearMV.Value;
                                        existingRateYearMV.Dirty = true;
                                        existingRateYearMV.Updateable = UpdateType.Upsert;
                                        existingRateDetailMV.Updateable = UpdateType.Upsert;
                                    }
                                }
                                else
                                {
                                    modified = true;
                                    importedRateYearMV.Dirty = true;
                                    importedRateYearMV.Updateable = UpdateType.Upsert;
                                    importedRateYearMV.RateCodeId = existingRateDetailMV.Id;
                                    existingRateDetailMV.Updateable = UpdateType.Upsert;

                                    // New RateYear in import - insert
                                    existingRateDetailMV.Values.Add(importedRateYearMV);
                                }
                            }

                            // Only save Rates that have changes.
                            if (modified == true)
                            {
                                // Add to collection for bulk save.
                                importResults.Add(existingRateDetailMV);
                            }
                        }
                    }
                    else
                    {
                        string validationString =
                            string.Format(RateMappingValidationConstants.RATEDETAILS_RATECODE_MISSING,
                                importedRateDetailMV.RateCode);
                        throw new GenValidationException(validationString);
                    }
                }

                // Set to 5x Normal timeout (nominally 5 minutes total).
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 5 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    this.rateDetailLoader.BulkSave(importResults);
                    scope.Complete();
                }
            }
            catch (FileFormatException)
            {
                throw new GenValidationException("Imported file was an incorrect format.");
            }

            return importResults;
        }

        /// <summary>
        /// Replicates the rate codes.
        /// </summary>
        /// <param name="importedRates">The imported rates.</param>
        public void ReplicateRateCodes(ICollection<RateDetailModelView> importedRates)
        {
            if (importedRates == null)
            {
                throw new ArgumentNullException(nameof(importedRates));
            }

            ICollection<RateCodeModelView> replications = this.replicationLoader.GetAll();

            foreach (RateCodeModelView replication in replications)
            {
                RateDetailModelView importedRate = importedRates.FirstOrDefault(r => r.RateCode == replication.From);
                if (importedRate != null)
                {
                    // replicate the imported rate
                    RateDetailModelView duplicate = importedRate.DeepClone();
                    duplicate.RateCode = replication.To;
                    importedRates.Add(duplicate);
                }
            }
        }

        /// <summary>
        /// Load imported rate codes into database.
        /// </summary>
        /// <param name="importedRateCodes">Validated collection of rate codes to import.</param>
        public void LoadImportedRateCodes(ICollection<RateDetailModelView> importedRateCodes)
        {
            if (importedRateCodes == null)
            {
                throw new GenValidationException("Imported Rate Codes are null.");
            }

            // Set to 5x Normal timeout (nominally 5 minutes total).
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 5 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.rateDetailLoader.SaveDetails(importedRateCodes);
                scope.Complete();
            }
        }

        /// <summary>
        /// Get the imported rate codes to be updated.
        /// </summary>
        /// <param name="existingRateCodes">Existing Rate Code MVs from DB</param>
        /// <param name="importedRateCodes">Imported Rate Code MVs to validate.</param>
        /// <returns>Collection of updated rate codes</returns>
        public ICollection<RateDetailModelView> GetUpdatedRateCodes(ICollection<RateDetailModelView> existingRateCodes,
            ICollection<RateDetailModelView> importedRateCodes)
        {
            if (existingRateCodes == null)
            {
                throw new ArgumentNullException(nameof(existingRateCodes));
            }

            if (importedRateCodes == null)
            {
                throw new ArgumentNullException(nameof(importedRateCodes));
            }

            string rateCode = string.Empty;
            ICollection<RateDetailModelView> updatedRateCodes = new Collection<RateDetailModelView>();

            try
            {
                // Create a collection of updated rate codes.
                int id = -1;
                foreach (RateDetailModelView importedRateCode in importedRateCodes)
                {
                    rateCode = importedRateCode.RateCode;
                    RateDetailModelView existingRateCode = existingRateCodes.SingleOrDefault(x => x.RateCode.Equals(importedRateCode.RateCode));    // use SingleOrDefault since there should never be duplicate RateCode values
                    if (existingRateCode == null)
                    {
                        importedRateCode.Id = id--;
                        updatedRateCodes.Add(importedRateCode);
                    }
                    else
                    {
                        // Clone the existing rate code and update the imported properties
                        RateDetailModelView updatedRateCode = this.CloneAndUpdateImportedRateCodeProperties(existingRateCode, importedRateCode);

                        // If there are changes, add it to the collection
                        if (this.IsImportedRateCodeDirty(existingRateCode, updatedRateCode))
                        {
                            updatedRateCodes.Add(updatedRateCode);
                        }
                    }
                }

                // set dirty flag for each of the updated rate codes 
                foreach (RateDetailModelView updatedRateCode in updatedRateCodes)
                {
                    updatedRateCode.Dirty = true;
                    updatedRateCode.Updateable = UpdateType.Upsert;
                }
            }
            catch (InvalidOperationException)
            {
                string validationString = string.Format(RateMappingValidationConstants.RATEDETAILS_DUPLICATE_RATECODE, rateCode);
                throw new GenValidationException(validationString);
            }

            return updatedRateCodes;
        }

        /// <summary>
        /// Helper method to clone an existing rate code and update the imported rate code properties.
        /// </summary>
        /// <param name="existingRateCode">Existing Rate Code from DB</param>
        /// <param name="importedRateCode">Imported Rate Code</param>
        /// <returns>Cloned and updated rate code MV</returns>
        private RateDetailModelView CloneAndUpdateImportedRateCodeProperties(RateDetailModelView existingRateCode, RateDetailModelView importedRateCode)
        {
            RateDetailModelView updatedRateCode = existingRateCode.DeepClone();

            // update imported rate code properties
            updatedRateCode.RateCategory = importedRateCode.RateCategory;
            updatedRateCode.Description = importedRateCode.Description;
            updatedRateCode.Section = importedRateCode.Section;
            updatedRateCode.ResourceType = importedRateCode.ResourceType;
            updatedRateCode.RateType = importedRateCode.RateType;
            updatedRateCode.RateDescription = importedRateCode.RateDescription;
            updatedRateCode.RateDescription1 = importedRateCode.RateDescription1;
            updatedRateCode.RateDescription2 = importedRateCode.RateDescription2;
            updatedRateCode.RateDescription3 = importedRateCode.RateDescription3;
            updatedRateCode.RateDescription4 = importedRateCode.RateDescription4;
            updatedRateCode.RateDescription5 = importedRateCode.RateDescription5;
            updatedRateCode.RateDescription6 = importedRateCode.RateDescription6;
            updatedRateCode.RateDescription7 = importedRateCode.RateDescription7;
            updatedRateCode.RateDescription8 = importedRateCode.RateDescription8;
            updatedRateCode.RateDescription9 = importedRateCode.RateDescription9;
            updatedRateCode.ResourceClassId = importedRateCode.ResourceClassId;
            updatedRateCode.ResourceClassId1 = importedRateCode.ResourceClassId1;
            updatedRateCode.ResourceClassId2 = importedRateCode.ResourceClassId2;
            updatedRateCode.ResourceClassId3 = importedRateCode.ResourceClassId3;
            updatedRateCode.ResourceClassId4 = importedRateCode.ResourceClassId4;
            updatedRateCode.ResourceClassId5 = importedRateCode.ResourceClassId5;
            updatedRateCode.ResourceClassId6 = importedRateCode.ResourceClassId6;
            updatedRateCode.ResourceClassId7 = importedRateCode.ResourceClassId7;
            updatedRateCode.ResourceClassId8 = importedRateCode.ResourceClassId8;
            updatedRateCode.ResourceClassId9 = importedRateCode.ResourceClassId9;
            updatedRateCode.ResourceClass = string.Empty;
            updatedRateCode.ResourceClass1 = string.Empty;
            updatedRateCode.ResourceClass2 = string.Empty;
            updatedRateCode.ResourceClass3 = string.Empty;
            updatedRateCode.ResourceClass4 = string.Empty;
            updatedRateCode.ResourceClass5 = string.Empty;
            updatedRateCode.ResourceClass6 = string.Empty;
            updatedRateCode.ResourceClass7 = string.Empty;
            updatedRateCode.ResourceClass8 = string.Empty;
            updatedRateCode.ResourceClass9 = string.Empty;
            updatedRateCode.GovernmentBurdenPoolId = importedRateCode.GovernmentBurdenPoolId;
            updatedRateCode.GovernmentBurdenPool = string.Empty;
            updatedRateCode.CommercialBurdenPoolId = importedRateCode.CommercialBurdenPoolId;
            updatedRateCode.CommercialBurdenPool = string.Empty;

            return updatedRateCode;
        }

        /// <summary>
        /// Helper method to determine if an imported rate code is dirty (modified).
        /// Note: Does not compare rate values.
        /// </summary>
        /// <param name="existingRateCode">Existing Rate Code from DB</param>
        /// <param name="importedRateCode">Imported Rate Code</param>
        /// <returns>true if any of the imported properties have been modified; false otherwise;</returns>
        private bool IsImportedRateCodeDirty(RateDetailModelView existingRateCode, RateDetailModelView importedRateCode)
        {
            return existingRateCode.Id != importedRateCode.Id ||                    // sanity check (not modified by import)
                existingRateCode.RevisionId != importedRateCode.RevisionId ||       // sanity check (not modified by import)
                existingRateCode.RatePrecision != importedRateCode.RatePrecision || // sanity check (not modified by import)
                !this.IsNullableIdPropertyEquivalent(existingRateCode.CommercialBurdenPoolId, importedRateCode.CommercialBurdenPoolId) ||
                !this.IsStringPropertyEquivalent(existingRateCode.Description, importedRateCode.Description) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.GovernmentBurdenPoolId, importedRateCode.GovernmentBurdenPoolId) ||
                (int)existingRateCode.RateCategory != (int)importedRateCode.RateCategory ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription, importedRateCode.RateDescription) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription1, importedRateCode.RateDescription1) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription2, importedRateCode.RateDescription2) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription3, importedRateCode.RateDescription3) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription4, importedRateCode.RateDescription4) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription5, importedRateCode.RateDescription5) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription6, importedRateCode.RateDescription6) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription7, importedRateCode.RateDescription7) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription8, importedRateCode.RateDescription8) ||
                !this.IsStringPropertyEquivalent(existingRateCode.RateDescription9, importedRateCode.RateDescription9) ||
                !this.IsNullableIdPropertyEquivalent((int?)existingRateCode.RateType, (int?)importedRateCode.RateType) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId, importedRateCode.ResourceClassId) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId1, importedRateCode.ResourceClassId1) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId2, importedRateCode.ResourceClassId2) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId3, importedRateCode.ResourceClassId3) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId4, importedRateCode.ResourceClassId4) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId5, importedRateCode.ResourceClassId5) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId6, importedRateCode.ResourceClassId6) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId7, importedRateCode.ResourceClassId7) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId8, importedRateCode.ResourceClassId8) ||
                !this.IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId9, importedRateCode.ResourceClassId9) ||
                !this.IsNullableIdPropertyEquivalent((int?)existingRateCode.ResourceType, (int?)importedRateCode.ResourceType) ||
                existingRateCode.RevisionId != importedRateCode.RevisionId ||
                !this.IsNullableIdPropertyEquivalent((int?)existingRateCode.Section, (int?)importedRateCode.Section);
        }

        /// <summary>
        /// Helper method to determine if two strings are equivalent, i.e. both empty or both equal.
        /// </summary>
        /// <param name="string1">string 1</param>
        /// <param name="string2">string 2</param>
        /// <returns>true, if both strings are empty, or both are equal</returns>
        private bool IsStringPropertyEquivalent(string string1, string string2)
        {
            bool string1Null = string.IsNullOrWhiteSpace(string1);
            bool string2Null = string.IsNullOrWhiteSpace(string2);
            return (string1Null && string2Null) || (!string1Null && !string2Null && string1.Trim().Equals(string2.Trim()));
        }

        /// <summary>
        /// Helper method to determine if two nullable Ids are equivalent, i.e. both null or 0, or both equal.
        /// </summary>
        /// <param name="id1">id 1</param>
        /// <param name="id2">id 2</param>
        /// <returns>true, if both ids are null/not set, or both are equal</returns>
        private bool IsNullableIdPropertyEquivalent(int? id1, int? id2)
        {
            return ((!id1.HasValue || id1 == 0) && (!id2.HasValue || id2 == 0)) || (id1.HasValue && id2.HasValue && id1.Value == id2.Value);
        }
    }
}
