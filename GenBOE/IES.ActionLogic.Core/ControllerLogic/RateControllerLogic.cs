// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.ControllerLogic
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
	using IES.ActionLogic.Core.Mediator;
	using IES.ActionLogic.Core.Validation;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.Extensions.Configuration;

	/// <summary>
	/// Logic for the Rate Controller.
	/// </summary>
	public class RateControllerLogic : RdmControllerLogic, IRateControllerLogic
	{
		/// <summary>
		/// Configuration for appsettings.json.
		/// </summary>
		private readonly IConfiguration configuration;

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
		private readonly IRateCodeReplicationLoader replicationLoader;

		/// <summary>
		/// The rate formatter
		/// </summary>
		private readonly RateFormatter rateFormatter;

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
			IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryService adUtils, ISecurityInformation securityInfo, IRateCodeReplicationLoader replicationLoader,
			RateFormatter rateFormatter, IConfiguration configuration)
			: base(areaLockingLoader, revisionMediator, adUtils, securityInfo, configuration)

		{
			this.rateDetailLoader = rateDetailLoader;
			this.commonDataMapper = commonDataMapper;
			this.homeControllerLogic = homeControllerLogic;
			this.burdenPoolLoader = burdenPoolLoader;
			this.sectionLoader = sectionLoader;
			this.replicationLoader = replicationLoader;
			this.rateFormatter = rateFormatter;
			this.configuration = configuration;
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

			burdenPoolLoader.GetBurdenPoolOptions(revision.Id, out ICollection<OptionModelView> commercialBurdenPoolOptions, out ICollection<OptionModelView> governmentBurdenPoolOptions);

			RateGridModelView model = new()
			{
				SelectedRevisionId = revision.Id,
				AdminUser = IsRDMAdminUser,
				CobraAdminUser = IsRDMCobraAdminUser,
				Versions = RevisionMediator.GetRevisionOptions(revisions),
				Rates = rateDetailLoader.GetRatesByRevision(revision),
				Sections = sectionLoader.RetrieveSectionsAsOptions(revision),
				RateCategories = ExtensionMethods.GetOptions<RateCategory>().OrderBy(x => x.Label).ToList(),
				LockInfo = homeControllerLogic.GetCurrentLockInfo(LockArea.RDMRates),
				RateTypes = ExtensionMethods.GetOptions<RateType>().OrderBy(x => x.Label).ToList(),
				DisclosureTypes = ExtensionMethods.GetOptions<DisclosureType>().OrderBy(x => x.Id).ToList(),
				ResourceClasses = commonDataMapper.GetResourceClassOptions(revision.Id),
				ResourceTypes = ExtensionMethods.GetOptions<DirectRateMappingResourceType>().OrderBy(x => x.Label).ToList(),
				CommercialBurdenPools = commercialBurdenPoolOptions,
				GovernmentBurdenPools = governmentBurdenPoolOptions
			};

			if (WipRevision.Id == revision.Id)
			{
				model.ReplicationValidationMessages = rateDetailLoader.VerifyRateCodeReplication(revision.Id);
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

			ICollection<RateDetailModelView> rates = rateDetailLoader.GetComparableRates(firstRevision, secondRevision);

			return rates;
		}

		/// <summary>
		/// Validates all the imported rates exist. We will not load only part of an import file.
		/// Also populates the RateCategory and RateCategoryDescription fields for each model view.
		/// </summary>
		/// <param name="existingRates">Collection of Rates from DB.</param>
		/// <param name="importRateDetails">Collection of imported rate details to validate.</param>
		/// <param name="isSave">Checks whether or not a Rate is being saved</param>
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
			Collection<ValidationMessage> validationErrors = new();
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
		public ICollection<ValidationMessage> ValidateRateDetailModelViews(ICollection<RateDetailModelView> rateDetailModelViews, int startYear, int endYear)
		{
			if (rateDetailModelViews == null)
			{
				throw new ArgumentNullException(nameof(rateDetailModelViews));
			}

			Collection<ValidationMessage> validationErrors = new();

			foreach (RateDetailModelView rdmv in rateDetailModelViews.Where(x => !x.IsDeleted))
			{
				// Get the precision before we loop through years.
				ValidationMessage precisionMsg = SetPrecision(rdmv);
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
				validationErrors.AddRange(ValidateRateProPricerMappings(rdmv));
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
			ICollection<ValidationMessage> validationErrors = ValidateRateCodeIsUnique(importedRateCodes);

			// perform additional validations against individual rate code rows 
			int rowIndex = -1;
			foreach (RateDetailModelView importedRateCode in importedRateCodes)
			{
				rowIndex++;
				// validate using model view annotations 
				ICollection<ValidationMessage> modelValidationErrors = ValidateRateCodeObject(importedRateCode);

				// validate ProPricer mappings
				ICollection<ValidationMessage> rowValidationErrors = ValidateRateProPricerMappings(importedRateCode);

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
			ValidationContext ctx = new(rateDetailModelView, null, null);
			List<ValidationResult> errors = new();
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

			rateDetailModelView.RatePrecision = rateFormatter.GetRatePrecision(RateTarget.Rate, rateDetailModelView.RateCategoryDescription);

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

			bool commercialBurdenPoolSet = rateDetailModelView.CommercialBurdenPoolId is not null and > 0;
			bool governmentBurdenPoolSet = rateDetailModelView.GovernmentBurdenPoolId is not null and > 0;
			bool rateTypeSet = rateDetailModelView.RateType is not null and not RateType.NotSet;
			bool resourceTypeSet = rateDetailModelView.ResourceType is not null and not DirectRateMappingResourceType.None;

			// if nothing is entered or all values have been cleared by user, nothing to validate
			if (!commercialBurdenPoolSet && !governmentBurdenPoolSet &&
				IsAllRateDescriptionsAndResourceClassesEmpty(rateDetailModelView) &&
				!rateTypeSet && !resourceTypeSet)
			{
				return validationErrors;
			}

			IReadOnlyCollection<string> allowEmptyBurdenPools = RateMappingValidationConstants.ALLOW_EMPTY_BURDEN_POOLS;

			// Commercial and Government Burden Pools must both be populated (or null for certain rates)
			if (!commercialBurdenPoolSet && governmentBurdenPoolSet)
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_COMMERCIAL_BURDENPOOL_REQUIRED));
			}
			else if (!governmentBurdenPoolSet && commercialBurdenPoolSet)
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_GOVERNMENT_BURDENPOOL_REQUIRED));
			}
			else if (!commercialBurdenPoolSet && !governmentBurdenPoolSet &&
					 !rateDetailModelView.RateCode.EndsWith("NL") &&
					 !allowEmptyBurdenPools.Any(x => rateDetailModelView.RateCode.Contains(x)))
			{
				// Can only be blank for Resources ******NL, Mileage, NLBESCCH, Travel Escalation, Travel Factor, or TRAVLESC
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_COMMERCIAL_AND_GOVERNMENT_BURDENPOOL_REQUIRED));
			}

			if (!rateTypeSet)
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RATETYPE_REQUIRED));
			}

			if (!resourceTypeSet)
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RESOURCETYPE_REQUIRED));
			}

			// if additional labor rates are selected, at least one of the descriptions needs to be filled in 
			if (rateDetailModelView.GenerateAdditionalDirectLaborRates && !IsAnyExtendedRateDescriptionPopulated(rateDetailModelView))
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_ADDITIONAL_DESCRIPTIONS_REQUIRED));
			}

			if (rateDetailModelView.GenerateAdditionalDirectLaborRates && rateDetailModelView.DisclosureType != DisclosureType.OneLMX && !IsAnyExtendedLegacyRateDescriptionPopulated(rateDetailModelView))
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_ADDITIONAL_DESCRIPTIONS_REQUIRED));
			}

			// if additional labor rates not selected, need single description 
			if (rateDetailModelView.GenerateAdditionalDirectLaborRates == false && string.IsNullOrEmpty(rateDetailModelView.RateDescription))
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_DESCRIPTION_REQUIRED));
			}

			// don't allow both base and extended rate descriptions to be filled in (this may happen during an import)
			if (!string.IsNullOrEmpty(rateDetailModelView.RateDescription) && IsAnyExtendedRateDescriptionPopulated(rateDetailModelView))
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_DESCRIPTION_INVALID));
			}

			// don't allow a resource class without a corresponding rate description (this may happen during an import)
			if (IsAnyResourceClassMissingCorrespondingRateDescription(rateDetailModelView))
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RESOURCE_CLASS_WITHOUT_RATE_DESCRIPTION));
			}

			// Resource Type must be labor when additional ProPricer rate descriptions are specified.
			if (!rateDetailModelView.ResourceType.Equals(DirectRateMappingResourceType.Labor) && IsAnyExtendedRateDescriptionPopulated(rateDetailModelView))
			{
				validationErrors.Add(FormatValidationMessage(rateDetailModelView, RateMappingValidationConstants.RATEMAPPING_RESOURCETYPE_INVALID));
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
			return string.IsNullOrEmpty(rateDetailModelView.RateDescription) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass, rateDetailModelView.ResourceClassId) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription1) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass1, rateDetailModelView.ResourceClassId1) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription2) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass2, rateDetailModelView.ResourceClassId2) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription3) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass3, rateDetailModelView.ResourceClassId3) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription4) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass4, rateDetailModelView.ResourceClassId4) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription5) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass5, rateDetailModelView.ResourceClassId5) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription6) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass6, rateDetailModelView.ResourceClassId6) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription7) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass7, rateDetailModelView.ResourceClassId7) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription8) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass8, rateDetailModelView.ResourceClassId8) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription9) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass9, rateDetailModelView.ResourceClassId9) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription11) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass11, rateDetailModelView.ResourceClassId11) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription12) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass12, rateDetailModelView.ResourceClassId12) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription13) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass13, rateDetailModelView.ResourceClassId13) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription14) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass14, rateDetailModelView.ResourceClassId14) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription15) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass15, rateDetailModelView.ResourceClassId15) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription21) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass21, rateDetailModelView.ResourceClassId21) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription22) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass22, rateDetailModelView.ResourceClassId22) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription23) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass23, rateDetailModelView.ResourceClassId23) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription24) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass24, rateDetailModelView.ResourceClassId24) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription25) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass25, rateDetailModelView.ResourceClassId25) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription31) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass31, rateDetailModelView.ResourceClassId31) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription32) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass32, rateDetailModelView.ResourceClassId32) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription33) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass33, rateDetailModelView.ResourceClassId33) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription34) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass34, rateDetailModelView.ResourceClassId34) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription35) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass35, rateDetailModelView.ResourceClassId35) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription41) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass41, rateDetailModelView.ResourceClassId41) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription42) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass42, rateDetailModelView.ResourceClassId42) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription43) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass43, rateDetailModelView.ResourceClassId43) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription44) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass44, rateDetailModelView.ResourceClassId44) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription45) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass45, rateDetailModelView.ResourceClassId45) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription51) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass51, rateDetailModelView.ResourceClassId51) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription52) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass52, rateDetailModelView.ResourceClassId52) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription53) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass53, rateDetailModelView.ResourceClassId53) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription54) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass54, rateDetailModelView.ResourceClassId54) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription55) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass55, rateDetailModelView.ResourceClassId55) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription61) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass61, rateDetailModelView.ResourceClassId61) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription62) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass62, rateDetailModelView.ResourceClassId62) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription63) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass63, rateDetailModelView.ResourceClassId63) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription64) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass64, rateDetailModelView.ResourceClassId64) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription65) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass65, rateDetailModelView.ResourceClassId65) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription71) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass71, rateDetailModelView.ResourceClassId71) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription72) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass72, rateDetailModelView.ResourceClassId72) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription73) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass73, rateDetailModelView.ResourceClassId73) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription74) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass74, rateDetailModelView.ResourceClassId74) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription75) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass75, rateDetailModelView.ResourceClassId75) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription81) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass81, rateDetailModelView.ResourceClassId81) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription82) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass82, rateDetailModelView.ResourceClassId82) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription83) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass83, rateDetailModelView.ResourceClassId83) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription84) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass84, rateDetailModelView.ResourceClassId84) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription85) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass85, rateDetailModelView.ResourceClassId85) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription91) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass91, rateDetailModelView.ResourceClassId91) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription92) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass92, rateDetailModelView.ResourceClassId92) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription93) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass93, rateDetailModelView.ResourceClassId93) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription94) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass94, rateDetailModelView.ResourceClassId94) &&
				   string.IsNullOrEmpty(rateDetailModelView.RateDescription95) && !IsResourceClassPopulated(rateDetailModelView.ResourceClass95, rateDetailModelView.ResourceClassId95);
		}

		/// <summary>
		/// Helper method to determine if any of the extended Resource Classes (i.e. 1-9) are populated without a corresponding Rate Description.
		/// </summary>
		/// <param name="rateDetailModelView">The rate detail mv</param>
		/// <returns>True if any of the extended Resource Classes (i.e. 1-9) are populated without a corresponding Rate Description; False otherwise.</returns>
		private bool IsAnyResourceClassMissingCorrespondingRateDescription(RateDetailModelView rateDetailModelView)
		{
			return (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription) && IsResourceClassPopulated(rateDetailModelView.ResourceClass, rateDetailModelView.ResourceClassId)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription1) && IsResourceClassPopulated(rateDetailModelView.ResourceClass1, rateDetailModelView.ResourceClassId1)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription2) && IsResourceClassPopulated(rateDetailModelView.ResourceClass2, rateDetailModelView.ResourceClassId2)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription3) && IsResourceClassPopulated(rateDetailModelView.ResourceClass3, rateDetailModelView.ResourceClassId3)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription4) && IsResourceClassPopulated(rateDetailModelView.ResourceClass4, rateDetailModelView.ResourceClassId4)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription5) && IsResourceClassPopulated(rateDetailModelView.ResourceClass5, rateDetailModelView.ResourceClassId5)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription6) && IsResourceClassPopulated(rateDetailModelView.ResourceClass6, rateDetailModelView.ResourceClassId6)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription7) && IsResourceClassPopulated(rateDetailModelView.ResourceClass7, rateDetailModelView.ResourceClassId7)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription8) && IsResourceClassPopulated(rateDetailModelView.ResourceClass8, rateDetailModelView.ResourceClassId8)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription9) && IsResourceClassPopulated(rateDetailModelView.ResourceClass9, rateDetailModelView.ResourceClassId9)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription11) && IsResourceClassPopulated(rateDetailModelView.ResourceClass11, rateDetailModelView.ResourceClassId11)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription12) && IsResourceClassPopulated(rateDetailModelView.ResourceClass12, rateDetailModelView.ResourceClassId12)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription13) && IsResourceClassPopulated(rateDetailModelView.ResourceClass13, rateDetailModelView.ResourceClassId13)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription14) && IsResourceClassPopulated(rateDetailModelView.ResourceClass14, rateDetailModelView.ResourceClassId14)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription15) && IsResourceClassPopulated(rateDetailModelView.ResourceClass15, rateDetailModelView.ResourceClassId15)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription21) && IsResourceClassPopulated(rateDetailModelView.ResourceClass21, rateDetailModelView.ResourceClassId21)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription22) && IsResourceClassPopulated(rateDetailModelView.ResourceClass22, rateDetailModelView.ResourceClassId22)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription23) && IsResourceClassPopulated(rateDetailModelView.ResourceClass23, rateDetailModelView.ResourceClassId23)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription24) && IsResourceClassPopulated(rateDetailModelView.ResourceClass24, rateDetailModelView.ResourceClassId24)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription25) && IsResourceClassPopulated(rateDetailModelView.ResourceClass25, rateDetailModelView.ResourceClassId25)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription31) && IsResourceClassPopulated(rateDetailModelView.ResourceClass31, rateDetailModelView.ResourceClassId31)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription32) && IsResourceClassPopulated(rateDetailModelView.ResourceClass32, rateDetailModelView.ResourceClassId32)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription33) && IsResourceClassPopulated(rateDetailModelView.ResourceClass33, rateDetailModelView.ResourceClassId33)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription34) && IsResourceClassPopulated(rateDetailModelView.ResourceClass34, rateDetailModelView.ResourceClassId34)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription35) && IsResourceClassPopulated(rateDetailModelView.ResourceClass35, rateDetailModelView.ResourceClassId35)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription41) && IsResourceClassPopulated(rateDetailModelView.ResourceClass41, rateDetailModelView.ResourceClassId41)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription42) && IsResourceClassPopulated(rateDetailModelView.ResourceClass42, rateDetailModelView.ResourceClassId42)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription43) && IsResourceClassPopulated(rateDetailModelView.ResourceClass43, rateDetailModelView.ResourceClassId43)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription44) && IsResourceClassPopulated(rateDetailModelView.ResourceClass44, rateDetailModelView.ResourceClassId44)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription45) && IsResourceClassPopulated(rateDetailModelView.ResourceClass45, rateDetailModelView.ResourceClassId45)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription51) && IsResourceClassPopulated(rateDetailModelView.ResourceClass51, rateDetailModelView.ResourceClassId51)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription52) && IsResourceClassPopulated(rateDetailModelView.ResourceClass52, rateDetailModelView.ResourceClassId52)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription53) && IsResourceClassPopulated(rateDetailModelView.ResourceClass53, rateDetailModelView.ResourceClassId53)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription54) && IsResourceClassPopulated(rateDetailModelView.ResourceClass54, rateDetailModelView.ResourceClassId54)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription55) && IsResourceClassPopulated(rateDetailModelView.ResourceClass55, rateDetailModelView.ResourceClassId55)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription61) && IsResourceClassPopulated(rateDetailModelView.ResourceClass61, rateDetailModelView.ResourceClassId61)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription62) && IsResourceClassPopulated(rateDetailModelView.ResourceClass62, rateDetailModelView.ResourceClassId62)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription63) && IsResourceClassPopulated(rateDetailModelView.ResourceClass63, rateDetailModelView.ResourceClassId63)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription64) && IsResourceClassPopulated(rateDetailModelView.ResourceClass64, rateDetailModelView.ResourceClassId64)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription65) && IsResourceClassPopulated(rateDetailModelView.ResourceClass65, rateDetailModelView.ResourceClassId65)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription71) && IsResourceClassPopulated(rateDetailModelView.ResourceClass71, rateDetailModelView.ResourceClassId71)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription72) && IsResourceClassPopulated(rateDetailModelView.ResourceClass72, rateDetailModelView.ResourceClassId72)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription73) && IsResourceClassPopulated(rateDetailModelView.ResourceClass73, rateDetailModelView.ResourceClassId73)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription74) && IsResourceClassPopulated(rateDetailModelView.ResourceClass74, rateDetailModelView.ResourceClassId74)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription75) && IsResourceClassPopulated(rateDetailModelView.ResourceClass75, rateDetailModelView.ResourceClassId75)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription81) && IsResourceClassPopulated(rateDetailModelView.ResourceClass81, rateDetailModelView.ResourceClassId81)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription82) && IsResourceClassPopulated(rateDetailModelView.ResourceClass82, rateDetailModelView.ResourceClassId82)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription83) && IsResourceClassPopulated(rateDetailModelView.ResourceClass83, rateDetailModelView.ResourceClassId83)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription84) && IsResourceClassPopulated(rateDetailModelView.ResourceClass84, rateDetailModelView.ResourceClassId84)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription85) && IsResourceClassPopulated(rateDetailModelView.ResourceClass85, rateDetailModelView.ResourceClassId85)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription91) && IsResourceClassPopulated(rateDetailModelView.ResourceClass91, rateDetailModelView.ResourceClassId91)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription92) && IsResourceClassPopulated(rateDetailModelView.ResourceClass92, rateDetailModelView.ResourceClassId92)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription93) && IsResourceClassPopulated(rateDetailModelView.ResourceClass93, rateDetailModelView.ResourceClassId93)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription94) && IsResourceClassPopulated(rateDetailModelView.ResourceClass94, rateDetailModelView.ResourceClassId94)) ||
				   (string.IsNullOrWhiteSpace(rateDetailModelView.RateDescription95) && IsResourceClassPopulated(rateDetailModelView.ResourceClass95, rateDetailModelView.ResourceClassId95));
		}

		/// <summary>
		/// Helper method to determine if any of the extended Rate Descriptions (i.e. 1-9 & [1-9][1-5]) are populated.
		/// </summary>
		/// <param name="rateDetailModelView">The rate detail mv</param>
		/// <returns>True if any of the extended Rate Descriptions (i.e. 1-9 & [1-9][1-5]) are populated; False otherwise.</returns>
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
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription9) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription11) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription12) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription13) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription14) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription15) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription21) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription22) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription23) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription24) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription25) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription31) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription32) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription33) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription34) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription35) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription41) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription42) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription43) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription44) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription45) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription51) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription52) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription53) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription54) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription55) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription61) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription62) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription63) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription64) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription65) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription71) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription72) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription73) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription74) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription75) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription81) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription82) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription83) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription84) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription85) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription91) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription92) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription93) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription94) ||
				   !string.IsNullOrEmpty(rateDetailModelView.RateDescription95);
		}

		/// <summary>
		/// Helper method to determine if any of the extended Legacy Rate Descriptions (i.e. 1-9) are populated.
		/// </summary>
		/// <param name="rateDetailModelView">The rate detail mv</param>
		/// <returns>True if any of the extended Rate Descriptions (i.e. 1-9) are populated; False otherwise.</returns>
		private bool IsAnyExtendedLegacyRateDescriptionPopulated(RateDetailModelView rateDetailModelView)
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

			Collection<RateDetailModelView> importResults = new();

			if (importedRates == null)
			{
				throw new GenValidationException("Imported Rates are null.");
			}

			// Changing from a fixed size array to a list
			importedRates = importedRates.ToList();

			ReplicateRateCodes(importedRates);

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
							if (modified)
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
				using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 5 * ConfigurationUtilities.GetAppSetting("TransactionTimeout", CommonConstants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					rateDetailLoader.BulkSave(importResults);
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
		/// Save Rates
		/// </summary>
		/// <param name="existingRates"></param>
		/// <param name="importedRates"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="GenValidationException"></exception>
		public ICollection<RateDetailModelView> SaveRates(ICollection<RateDetailModelView> existingRates, ICollection<RateDetailModelView> importedRates)
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
					// Save is different for import because it is possbile to Save a new Rate Code
					importedRateDetailMV.Updateable = UpdateType.Upsert;
					importResults.Add(importedRateDetailMV);
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
				throw new GenValidationException("Cannot Save Rates.");
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

			ICollection<RateCodeModelView> replications = replicationLoader.GetAll();

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
			using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, 5 * ConfigurationUtilities.GetAppSetting("TransactionTimeout", CommonConstants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				rateDetailLoader.SaveDetails(importedRateCodes);
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
						RateDetailModelView updatedRateCode = CloneAndUpdateImportedRateCodeProperties(existingRateCode, importedRateCode);

						// If there are changes, add it to the collection
						if (IsImportedRateCodeDirty(existingRateCode, updatedRateCode))
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
			updatedRateCode.GenerateAdditionalDirectLaborRates = importedRateCode.GenerateAdditionalDirectLaborRates;
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

			#region 1LMX Rates

			updatedRateCode.RateDescription11 = importedRateCode.RateDescription11;
			updatedRateCode.RateDescription12 = importedRateCode.RateDescription12;
			updatedRateCode.RateDescription13 = importedRateCode.RateDescription13;
			updatedRateCode.RateDescription14 = importedRateCode.RateDescription14;
			updatedRateCode.RateDescription15 = importedRateCode.RateDescription15;
			updatedRateCode.RateDescription21 = importedRateCode.RateDescription21;
			updatedRateCode.RateDescription22 = importedRateCode.RateDescription22;
			updatedRateCode.RateDescription23 = importedRateCode.RateDescription23;
			updatedRateCode.RateDescription24 = importedRateCode.RateDescription24;
			updatedRateCode.RateDescription25 = importedRateCode.RateDescription25;
			updatedRateCode.RateDescription31 = importedRateCode.RateDescription31;
			updatedRateCode.RateDescription32 = importedRateCode.RateDescription32;
			updatedRateCode.RateDescription33 = importedRateCode.RateDescription33;
			updatedRateCode.RateDescription34 = importedRateCode.RateDescription34;
			updatedRateCode.RateDescription35 = importedRateCode.RateDescription35;
			updatedRateCode.RateDescription41 = importedRateCode.RateDescription41;
			updatedRateCode.RateDescription42 = importedRateCode.RateDescription42;
			updatedRateCode.RateDescription43 = importedRateCode.RateDescription43;
			updatedRateCode.RateDescription44 = importedRateCode.RateDescription44;
			updatedRateCode.RateDescription45 = importedRateCode.RateDescription45;
			updatedRateCode.RateDescription51 = importedRateCode.RateDescription51;
			updatedRateCode.RateDescription52 = importedRateCode.RateDescription52;
			updatedRateCode.RateDescription53 = importedRateCode.RateDescription53;
			updatedRateCode.RateDescription54 = importedRateCode.RateDescription54;
			updatedRateCode.RateDescription55 = importedRateCode.RateDescription55;
			updatedRateCode.RateDescription61 = importedRateCode.RateDescription61;
			updatedRateCode.RateDescription62 = importedRateCode.RateDescription62;
			updatedRateCode.RateDescription63 = importedRateCode.RateDescription63;
			updatedRateCode.RateDescription64 = importedRateCode.RateDescription64;
			updatedRateCode.RateDescription65 = importedRateCode.RateDescription65;
			updatedRateCode.RateDescription71 = importedRateCode.RateDescription71;
			updatedRateCode.RateDescription72 = importedRateCode.RateDescription72;
			updatedRateCode.RateDescription73 = importedRateCode.RateDescription73;
			updatedRateCode.RateDescription74 = importedRateCode.RateDescription74;
			updatedRateCode.RateDescription75 = importedRateCode.RateDescription75;
			updatedRateCode.RateDescription81 = importedRateCode.RateDescription81;
			updatedRateCode.RateDescription82 = importedRateCode.RateDescription82;
			updatedRateCode.RateDescription83 = importedRateCode.RateDescription83;
			updatedRateCode.RateDescription84 = importedRateCode.RateDescription84;
			updatedRateCode.RateDescription85 = importedRateCode.RateDescription85;
			updatedRateCode.RateDescription91 = importedRateCode.RateDescription91;
			updatedRateCode.RateDescription92 = importedRateCode.RateDescription92;
			updatedRateCode.RateDescription93 = importedRateCode.RateDescription93;
			updatedRateCode.RateDescription94 = importedRateCode.RateDescription94;
			updatedRateCode.RateDescription95 = importedRateCode.RateDescription95;

			updatedRateCode.ResourceClassId11 = importedRateCode.ResourceClassId11;
			updatedRateCode.ResourceClassId12 = importedRateCode.ResourceClassId12;
			updatedRateCode.ResourceClassId13 = importedRateCode.ResourceClassId13;
			updatedRateCode.ResourceClassId14 = importedRateCode.ResourceClassId14;
			updatedRateCode.ResourceClassId15 = importedRateCode.ResourceClassId15;
			updatedRateCode.ResourceClassId21 = importedRateCode.ResourceClassId21;
			updatedRateCode.ResourceClassId22 = importedRateCode.ResourceClassId22;
			updatedRateCode.ResourceClassId23 = importedRateCode.ResourceClassId23;
			updatedRateCode.ResourceClassId24 = importedRateCode.ResourceClassId24;
			updatedRateCode.ResourceClassId25 = importedRateCode.ResourceClassId25;
			updatedRateCode.ResourceClassId31 = importedRateCode.ResourceClassId31;
			updatedRateCode.ResourceClassId32 = importedRateCode.ResourceClassId32;
			updatedRateCode.ResourceClassId33 = importedRateCode.ResourceClassId33;
			updatedRateCode.ResourceClassId34 = importedRateCode.ResourceClassId34;
			updatedRateCode.ResourceClassId35 = importedRateCode.ResourceClassId35;
			updatedRateCode.ResourceClassId41 = importedRateCode.ResourceClassId41;
			updatedRateCode.ResourceClassId42 = importedRateCode.ResourceClassId42;
			updatedRateCode.ResourceClassId43 = importedRateCode.ResourceClassId43;
			updatedRateCode.ResourceClassId44 = importedRateCode.ResourceClassId44;
			updatedRateCode.ResourceClassId45 = importedRateCode.ResourceClassId45;
			updatedRateCode.ResourceClassId51 = importedRateCode.ResourceClassId51;
			updatedRateCode.ResourceClassId52 = importedRateCode.ResourceClassId52;
			updatedRateCode.ResourceClassId53 = importedRateCode.ResourceClassId53;
			updatedRateCode.ResourceClassId54 = importedRateCode.ResourceClassId54;
			updatedRateCode.ResourceClassId55 = importedRateCode.ResourceClassId55;
			updatedRateCode.ResourceClassId61 = importedRateCode.ResourceClassId61;
			updatedRateCode.ResourceClassId62 = importedRateCode.ResourceClassId62;
			updatedRateCode.ResourceClassId63 = importedRateCode.ResourceClassId63;
			updatedRateCode.ResourceClassId64 = importedRateCode.ResourceClassId64;
			updatedRateCode.ResourceClassId65 = importedRateCode.ResourceClassId65;
			updatedRateCode.ResourceClassId71 = importedRateCode.ResourceClassId71;
			updatedRateCode.ResourceClassId72 = importedRateCode.ResourceClassId72;
			updatedRateCode.ResourceClassId73 = importedRateCode.ResourceClassId73;
			updatedRateCode.ResourceClassId74 = importedRateCode.ResourceClassId74;
			updatedRateCode.ResourceClassId75 = importedRateCode.ResourceClassId75;
			updatedRateCode.ResourceClassId81 = importedRateCode.ResourceClassId81;
			updatedRateCode.ResourceClassId82 = importedRateCode.ResourceClassId82;
			updatedRateCode.ResourceClassId83 = importedRateCode.ResourceClassId83;
			updatedRateCode.ResourceClassId84 = importedRateCode.ResourceClassId84;
			updatedRateCode.ResourceClassId85 = importedRateCode.ResourceClassId85;
			updatedRateCode.ResourceClassId91 = importedRateCode.ResourceClassId91;
			updatedRateCode.ResourceClassId92 = importedRateCode.ResourceClassId92;
			updatedRateCode.ResourceClassId93 = importedRateCode.ResourceClassId93;
			updatedRateCode.ResourceClassId94 = importedRateCode.ResourceClassId94;
			updatedRateCode.ResourceClassId95 = importedRateCode.ResourceClassId95;

			#endregion

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
				!IsNullableIdPropertyEquivalent(existingRateCode.CommercialBurdenPoolId, importedRateCode.CommercialBurdenPoolId) ||
				!IsStringPropertyEquivalent(existingRateCode.Description, importedRateCode.Description) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.GovernmentBurdenPoolId, importedRateCode.GovernmentBurdenPoolId) ||
				(int)existingRateCode.RateCategory != (int)importedRateCode.RateCategory ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription, importedRateCode.RateDescription) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription1, importedRateCode.RateDescription1) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription2, importedRateCode.RateDescription2) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription3, importedRateCode.RateDescription3) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription4, importedRateCode.RateDescription4) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription5, importedRateCode.RateDescription5) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription6, importedRateCode.RateDescription6) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription7, importedRateCode.RateDescription7) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription8, importedRateCode.RateDescription8) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription9, importedRateCode.RateDescription9) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription11, importedRateCode.RateDescription11) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription12, importedRateCode.RateDescription12) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription13, importedRateCode.RateDescription13) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription14, importedRateCode.RateDescription14) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription15, importedRateCode.RateDescription15) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription21, importedRateCode.RateDescription21) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription22, importedRateCode.RateDescription22) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription23, importedRateCode.RateDescription23) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription24, importedRateCode.RateDescription24) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription25, importedRateCode.RateDescription25) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription31, importedRateCode.RateDescription31) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription32, importedRateCode.RateDescription32) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription33, importedRateCode.RateDescription33) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription34, importedRateCode.RateDescription34) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription35, importedRateCode.RateDescription35) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription41, importedRateCode.RateDescription41) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription42, importedRateCode.RateDescription42) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription43, importedRateCode.RateDescription43) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription44, importedRateCode.RateDescription44) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription45, importedRateCode.RateDescription45) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription51, importedRateCode.RateDescription51) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription52, importedRateCode.RateDescription52) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription53, importedRateCode.RateDescription53) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription54, importedRateCode.RateDescription54) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription55, importedRateCode.RateDescription55) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription61, importedRateCode.RateDescription61) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription62, importedRateCode.RateDescription62) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription63, importedRateCode.RateDescription63) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription64, importedRateCode.RateDescription64) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription65, importedRateCode.RateDescription65) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription71, importedRateCode.RateDescription71) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription72, importedRateCode.RateDescription72) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription73, importedRateCode.RateDescription73) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription74, importedRateCode.RateDescription74) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription75, importedRateCode.RateDescription75) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription81, importedRateCode.RateDescription81) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription82, importedRateCode.RateDescription82) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription83, importedRateCode.RateDescription83) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription84, importedRateCode.RateDescription84) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription85, importedRateCode.RateDescription85) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription91, importedRateCode.RateDescription91) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription92, importedRateCode.RateDescription92) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription93, importedRateCode.RateDescription93) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription94, importedRateCode.RateDescription94) ||
				!IsStringPropertyEquivalent(existingRateCode.RateDescription95, importedRateCode.RateDescription95) ||
				!IsNullableIdPropertyEquivalent((int?)existingRateCode.RateType, (int?)importedRateCode.RateType) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId, importedRateCode.ResourceClassId) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId1, importedRateCode.ResourceClassId1) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId2, importedRateCode.ResourceClassId2) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId3, importedRateCode.ResourceClassId3) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId4, importedRateCode.ResourceClassId4) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId5, importedRateCode.ResourceClassId5) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId6, importedRateCode.ResourceClassId6) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId7, importedRateCode.ResourceClassId7) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId8, importedRateCode.ResourceClassId8) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId9, importedRateCode.ResourceClassId9) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId11, importedRateCode.ResourceClassId11) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId12, importedRateCode.ResourceClassId12) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId13, importedRateCode.ResourceClassId13) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId14, importedRateCode.ResourceClassId14) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId15, importedRateCode.ResourceClassId15) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId21, importedRateCode.ResourceClassId21) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId22, importedRateCode.ResourceClassId22) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId23, importedRateCode.ResourceClassId23) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId24, importedRateCode.ResourceClassId24) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId25, importedRateCode.ResourceClassId25) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId31, importedRateCode.ResourceClassId31) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId32, importedRateCode.ResourceClassId32) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId33, importedRateCode.ResourceClassId33) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId34, importedRateCode.ResourceClassId34) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId35, importedRateCode.ResourceClassId35) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId41, importedRateCode.ResourceClassId41) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId42, importedRateCode.ResourceClassId42) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId43, importedRateCode.ResourceClassId43) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId44, importedRateCode.ResourceClassId44) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId45, importedRateCode.ResourceClassId45) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId51, importedRateCode.ResourceClassId51) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId52, importedRateCode.ResourceClassId52) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId53, importedRateCode.ResourceClassId53) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId54, importedRateCode.ResourceClassId54) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId55, importedRateCode.ResourceClassId55) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId61, importedRateCode.ResourceClassId61) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId62, importedRateCode.ResourceClassId62) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId63, importedRateCode.ResourceClassId63) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId64, importedRateCode.ResourceClassId64) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId65, importedRateCode.ResourceClassId65) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId71, importedRateCode.ResourceClassId71) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId72, importedRateCode.ResourceClassId72) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId73, importedRateCode.ResourceClassId73) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId74, importedRateCode.ResourceClassId74) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId75, importedRateCode.ResourceClassId75) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId81, importedRateCode.ResourceClassId81) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId82, importedRateCode.ResourceClassId82) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId83, importedRateCode.ResourceClassId83) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId84, importedRateCode.ResourceClassId84) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId85, importedRateCode.ResourceClassId85) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId91, importedRateCode.ResourceClassId91) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId92, importedRateCode.ResourceClassId92) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId93, importedRateCode.ResourceClassId93) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId94, importedRateCode.ResourceClassId94) ||
				!IsNullableIdPropertyEquivalent(existingRateCode.ResourceClassId95, importedRateCode.ResourceClassId95) ||

				!IsNullableIdPropertyEquivalent((int?)existingRateCode.ResourceType, (int?)importedRateCode.ResourceType) ||
				existingRateCode.RevisionId != importedRateCode.RevisionId ||
				!IsNullableIdPropertyEquivalent(existingRateCode.Section, importedRateCode.Section);
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
