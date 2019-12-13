// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.ActionLogic.Validation;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Logic for the Admin Controller.
    /// </summary>
    public class AdminControllerLogic : RdmControllerLogic, IAdminControllerLogic
    {
        /// <summary>
        /// The rate detail loader
        /// </summary>
        private IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// The replication loader
        /// </summary>
        private IRateCodeReplicationLoader replicationLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="revisionMediator">Revision Mediator</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        public AdminControllerLogic(IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, 
            IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo, IRateDetailLoader rateDetailLoader,
            IRateCodeReplicationLoader replicationLoader)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)
        {
            this.rateDetailLoader = rateDetailLoader;
            this.replicationLoader = replicationLoader;
        }


        /// <summary>
        /// Determines whether the provided Cobra mapping details are valid.
        /// </summary>
        /// <param name="collection">The collection of Cobra Details.</param>
        /// <returns>A list of validation errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateCobraDetailModelViews(ICollection<CobraDetailModelView> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            ICollection<ValidationMessage> validationErrors = new List<ValidationMessage>();

            // Look for RateCodes with COBRA Code1 but no Rate Set, or vice-versa.
            List<string> missingRateSet = collection.Where(x => x.Code1 > 0 && string.IsNullOrWhiteSpace(x.RateSet)).Select(x => x.RateCode).ToList();
            if (missingRateSet.Any())
            {
                string missingRateSetMessage = string.Join(", ", missingRateSet);
                validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.COBRA_MAPPING_MISSING_RATE_SET, missingRateSetMessage)));
            }
            List<string> missingCode1 = collection.Where(x => !string.IsNullOrWhiteSpace(x.RateSet) && (!x.Code1.HasValue || x.Code1.Value <= 0)).Select(x => x.RateCode).ToList();
            if (missingCode1.Any())
            {
                string missingCode1Message = string.Join(", ", missingCode1);
                validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.COBRA_MAPPING_MISSING_CODE_1, missingCode1Message)));
            }

            return validationErrors;
        }

        /// <summary>
        /// Determines whether the provided Cobra Year Configurations are valid.
        /// </summary>
        /// <param name="collection">The collection of Cobra Year Configurations.</param>
        /// <returns>A list of validation errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateCobraYearConfigurations(ICollection<CobraYearGridModelView> collection)
        {
            ICollection<ValidationMessage> validationErrors = new List<ValidationMessage>();

            // Ensure that the values are not being wiped-out completely.
            if (collection == null || !collection.Any())
            {
                validationErrors.Add(new ValidationMessage(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_REQUIRED));
            }
            else
            {
                // Look for Years that have a missing or invalid Cobra Date.
                List<int> missingOrInvalidCobraDate = collection.Where(config => config.CobraDate == DateTime.MinValue).Select(config => config.Year).ToList();
                if (missingOrInvalidCobraDate.Any())
                {
                    string missingOrInvalidCobraDateMessage = string.Join(", ", missingOrInvalidCobraDate);
                    validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_MISSING_OR_INVALID_COBRA_DATE, missingOrInvalidCobraDateMessage)));
                }

                // Look for Cobra Dates missing a Year.
                List<string> missingYear = collection.Where(config => config.Year <= 0).Select(config => config.CobraDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR)).ToList();
                if (missingYear.Any())
                {
                    string missingYearMessage = string.Join(", ", missingYear);
                    validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_MISSING_YEAR, missingYearMessage)));
                }

                // Look for duplicate Years.
                List<int> duplicateYears = collection.GroupBy(config => config.Year).Where(config => config.Count() > 1).Select(group => group.Key).ToList();
                if (duplicateYears.Any())
                {
                    string duplicateYearMessage = string.Join(", ", duplicateYears);
                    validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_DUPLICATE_YEARS, duplicateYearMessage)));
                }

                // Look for duplicate Cobra Dates.  Ignores blank and invalid dates, which are caught in an earlier message.
                List<string> duplicateCobraDates = collection.Where(config => config.CobraDate != DateTime.MinValue).GroupBy(config => config.CobraDate).Where(config => config.Count() > 1).Select(group => group.Key.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR)).ToList();
                if (duplicateCobraDates.Any())
                {
                    string duplicateCobraDateMessage = string.Join(", ", duplicateCobraDates);
                    validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_DUPLICATE_COBRA_DATES, duplicateCobraDateMessage)));
                }

                // Look for Year / Cobra Date combinations that do not match.  The year of the Cobra Date should be =(Year) or =(Year - 1).  Ignores blank and invalid dates, which are caught in an earlier message.
                List<int> dateCombinationErrors = collection.Where(config => config.CobraDate != DateTime.MinValue && config.CobraDate.Year != config.Year && config.CobraDate.Year != (config.Year - 1)).Select(config => config.Year).ToList();
                if (dateCombinationErrors.Any())
                {
                    string dateCombinationErrorMessage = string.Join(", ", dateCombinationErrors);
                    validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_INVALID_DATE_COMBINATION, dateCombinationErrorMessage)));
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Determine whether the provided start and end year values are valid.
        /// </summary>
        /// <param name="startYear">Start Year</param>
        /// <param name="endYear">End Year</param>
        /// <returns>A list of validation errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateRateYearConfiguration(string startYear, string endYear)
        {
            ICollection<ValidationMessage> validationErrors = new List<ValidationMessage>();
            int startYearInt, endYearInt;
            bool startYearResult = int.TryParse(startYear, out startYearInt);
            bool endYearResult = int.TryParse(endYear, out endYearInt);
            if (startYearResult && endYearResult)
            {
                if (startYearInt < AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR || startYearInt > AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR)
                {
                    validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEAR_OUT_OF_RANGE,
                        "Start", AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR, AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR)));
                }

                if (endYearInt < AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR || endYearInt > AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR)
                {
                    validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEAR_OUT_OF_RANGE,
                        "End", AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR, AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR)));
                }

                if (startYearInt > endYearInt)
                {
                    validationErrors.Add(new ValidationMessage(AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEARS_OUT_OF_ORDER));
                }
            }
            else
            {
                validationErrors.Add(new ValidationMessage(AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEAR_NOT_NUMERIC));
            }

            return validationErrors;
        }

        /// <summary>
        /// Gets the resource code replication model view.
        /// </summary>
        /// <returns>Model View.</returns>
        public RateCodeReplicationModelView GetRateCodeReplication()
        {
            RateCodeReplicationModelView modelView = new RateCodeReplicationModelView
            {
                RateCodes = this.rateDetailLoader.GetRateCodesForRevision(this.WipRevision.Id),
                LockInfo = this.GetCurrentLockInfo(LockArea.RateCodeReplication),
                ValidationMessages = this.rateDetailLoader.VerifyRateCodeReplication(this.WipRevision.Id),
                Replications = this.replicationLoader.GetAll()
            };

            return modelView;
        }

        /// <summary>
        /// Saves the rate code replications.
        /// </summary>
        /// <param name="rateCodes">The rate code replication to save.</param>
        public void SaveRateCodeReplication(ICollection<RateCodeModelView> rateCodes)
        {
            this.replicationLoader.Save(rateCodes);
        }

        /// <summary>
        /// Validates the rate code replications.
        /// </summary>
        /// <param name="rateCodes">The rate codes.</param>
        /// <returns>A list of Validation Errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateRateCodeReplication(ICollection<RateCodeModelView> rateCodes)
        {
            if (rateCodes == null)
            {
                throw new ArgumentNullException(nameof(rateCodes));
            }

            // Remove new ratecodes marked for deletion (created and deleted in UI without being saved)
            rateCodes = rateCodes.Where(r => !r.IsDeleted || r.Id > 0).ToList();

            List<ValidationMessage> errors = new List<ValidationMessage>();

            ICollection<RateDto> allRateCodes = this.rateDetailLoader.GetRateCodesForRevision(this.WipRevision.Id);
            ICollection<RateCodeModelView> dbRateCodes = this.replicationLoader.GetAll();

            // Create a merged list
            List<RateCodeModelView> mergedList = rateCodes.ToList();
            List<int> mergedListIds = mergedList.Where(r => r.Id > 0).Select(r => r.Id).ToList();
            mergedList.AddRange(dbRateCodes.Where(d => !mergedListIds.Contains(d.Id)));

            bool fromRequiredFlag = false;
            bool toRequiredFlag = false;

            foreach (RateCodeModelView rateCode in mergedList.ToList())
            {
                if (rateCode.IsDeleted)
                {
                    rateCode.Updateable = UpdateType.Deleted;

                    // no need to validate against deleted
                    continue;
                }

                // check for missing fields
                if (string.IsNullOrWhiteSpace(rateCode.From))
                {
                    fromRequiredFlag = true;
                }

                if (string.IsNullOrWhiteSpace(rateCode.To))
                {
                    toRequiredFlag = true;
                }

                // To column must be unique across entire table
                if (mergedList.Any(r => r.Id != rateCode.Id && !r.IsDeleted && r.To == rateCode.To))
                {
                    string message = string.Format(AdminValidationConstants.RATE_CODE_TO_UNIQUE, rateCode.To);

                    if (!errors.Any(e => e.ValidationIssue == message))
                    {
                        errors.Add(new ValidationMessage(message));
                    }
                }

                // A RateCode cannot be used in both the FROM and TO Columns
                if (!string.IsNullOrWhiteSpace(rateCode.From) && mergedList.Any(r => !r.IsDeleted && r.From == rateCode.To))
                {
                    errors.Add(new ValidationMessage(string.Format(AdminValidationConstants.RATE_CODE_FROM_TO_MATCH, rateCode.To)));
                }

                // check for invalid fields
                if (!string.IsNullOrWhiteSpace(rateCode.From) && !allRateCodes.Any(r => r.Rate == rateCode.From))
                {
                    errors.Add(new ValidationMessage(string.Format(AdminValidationConstants.RATE_CODE_INVALID, rateCode.From, AdminValidationConstants.RATE_CODE_FROM_COLUMN))); 
                }

                if (!string.IsNullOrWhiteSpace(rateCode.To) && !allRateCodes.Any(r => r.Rate == rateCode.To))
                {
                    errors.Add(new ValidationMessage(string.Format(AdminValidationConstants.RATE_CODE_INVALID, rateCode.To, AdminValidationConstants.RATE_CODE_TO_COLUMN)));
                }
            }

            if (fromRequiredFlag)
            {
                errors.Add(new ValidationMessage(string.Format(AdminValidationConstants.RATE_CODE_REQUIRED, AdminValidationConstants.RATE_CODE_FROM_COLUMN)));
            }

            if (toRequiredFlag)
            {
                errors.Add(new ValidationMessage(string.Format(AdminValidationConstants.RATE_CODE_REQUIRED, AdminValidationConstants.RATE_CODE_TO_COLUMN)));
            }

            return errors;
        }

        /// <summary>
        /// Clear the Revision cache.
        /// </summary>
        public void ClearRevisionCache()
        {
            this.RevisionMediator.ClearRevisionCache();
        }
    }
}
