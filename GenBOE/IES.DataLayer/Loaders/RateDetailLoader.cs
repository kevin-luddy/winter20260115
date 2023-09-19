// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.DataBridge.Loaders
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Linq.Expressions;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.DataBridge.ModelViews;
	using IES.Models;

    /// <summary>
    /// Rate Grid Loader
    /// </summary>
    public class RateDetailLoader : BulkDataLoader<RateDetailModelView, RateCode>, IRateDetailLoader
    {
        /// <summary>
        /// The Rate Description numbers.
        /// </summary>
        private readonly int?[] numbers = { null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 21, 22, 23, 24, 25, 31, 32, 33, 34, 35, 41, 42, 43, 44, 45, 51, 52, 53, 54, 55, 61, 62, 63, 64, 65, 71, 72, 73, 74, 75, 81, 82, 83, 84, 85, 91, 92, 93, 94, 95 };

		/// <summary>
		/// RateYearLoader.
		/// </summary>
		private readonly IRateCodeYearLoader rateYearLoader;

		/// <summary>
		/// RateCodeXrefLoader.
		/// </summary>
		private readonly IProPricerRateCodeXrefLoader proPricerXrefLoader;

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RateDetailLoader"/> class.
		/// </summary>
		/// <param name="rateYearLoader">The rate year loader.</param>
		/// <param name="proPricerXrefLoader">The proPricerXrefLoader loader.</param>
		public RateDetailLoader(IRateCodeYearLoader rateYearLoader, IProPricerRateCodeXrefLoader proPricerXrefLoader)
		{
			this.Log = new Logger(typeof(RateDetailLoader));
			this.rateYearLoader = rateYearLoader;
			this.proPricerXrefLoader = proPricerXrefLoader;
		}

		#endregion

		#region Retrieves

		/// <summary>
		/// Gets the wip Rates with section information.
		/// </summary>
		/// <param name="wipRevision">The WIP revision.</param>
		/// <returns>All of the Rates with Section detail for the WIP.</returns>
		public ICollection<RateSectionModelView> GetWIPRateSections(RevisionModelView wipRevision)
		{
			ICollection<RateSectionModelView> rateDetails;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (IESEntities context = new IESEntities())
				{
					rateDetails = context.RateCodes.Where(x => x.RevisionID == wipRevision.Id)
					.Select(r =>
					new RateSectionModelView()
					{
						Id = r.ID,
						RateCode = r.RateCode1,    // Entity Framework adds 1 to avoid name collision.
						Section = r.SectionID ?? 0 // special case because of angular dropdowns such that a zero comes back instead of null
					}).ToList();
				}
			}

			return rateDetails;
		}

		/// <summary>
		/// Get Rates with various filters.
		/// </summary>
		/// <param name="revision">Revision to retrieve.</param>
		/// <param name="rateCodes">Optional parameter to filter by Rate Code (used in import).</param>
		/// <param name="compareRevision">Optional parameter to get two compare sets with one DB call.</param>
		/// <returns>All Rate Details for the given revision.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private ICollection<RateDetailModelView> GetRates(RevisionModelView revision, string[] rateCodes = null, RevisionModelView compareRevision = null)
		{
			ICollection<RateDetailModelView> rateDetails;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (IESEntities context = new IESEntities())
				{
					// Define filter - limit with specific rate codes if string array is provided (Import).
					Expression<Func<RateCode, bool>> detailFilter;
					Expression<Func<RateCodeYear, bool>> yearFilter;
					if (rateCodes != null)
					{
						// For Imports return all the rows, even outside the revision years.
						detailFilter = x => x.RevisionID == revision.Id && rateCodes.Contains(x.RateCode1);
						yearFilter = ry => ry.ID >= 0;      // Should return all years for imports.
					}
					else if (compareRevision != null)
					{
						// Only compare overlapping years.
						int startYear = revision.StartYear > compareRevision.StartYear ? revision.StartYear : compareRevision.StartYear;
						int endYear = revision.EndYear < compareRevision.EndYear ? revision.EndYear : compareRevision.EndYear;

						// For Compare send all Rates in both versions for performance.
						detailFilter = x => x.RevisionID == revision.Id || x.RevisionID == compareRevision.Id;
						yearFilter = ry => ry.Year >= startYear && ry.Year <= endYear;
					}
					else
					{
						// For UI just send the rates within the Revision years.
						detailFilter = x => x.RevisionID == revision.Id;
						yearFilter = ry => ry.Year >= revision.StartYear && ry.Year <= revision.EndYear;
					}

					// Grab all the RateDetails for this revision and rate code list if provided.
					rateDetails = context.RateCodes.Where(detailFilter)
					.OrderBy(y => y.CategoryID)
					.Select(r =>
					new RateDetailModelView()
					{
						Description = r.Description,
						CommercialBurdenPoolId = r.CommercialBurdenPoolID ?? 0, // special case because of angular dropdowns such that a zero comes back instead of null
						CommercialBurdenPool = r.CommercialBurdenPoolLU == null ? string.Empty : r.CommercialBurdenPoolLU.BurdenPool,
						GovernmentBurdenPoolId = r.GovernmentBurdenPoolID ?? 0, // special case because of angular dropdowns such that a zero comes back instead of null
						GovernmentBurdenPool = r.GovernmentBurdenPoolLU == null ? string.Empty : r.GovernmentBurdenPoolLU.BurdenPool,
						Id = r.ID,
						ProPricerMappings = r.ProPricerRateCodeXrefs.Select(m =>
							new ProPricerRateCodeXrefModelView()
							{
								Id = m.ID,
								UpdateDate = m.UpdateDate,
								RateCodeId = m.RateCodeID,
								Description = m.Description,
								RateCodeExtensionId = m.RateCodeExtensionID,
								ResourceClassId = m.ResourceClassID ?? 0,  // special case because of angular dropdowns such that a zero comes back instead of null
								ResourceClass = m.ResourceClassLU == null ? string.Empty : m.ResourceClassLU.Description
							}).ToList(),
						HasProPricerBurdenRateMappings = r.ProPricerBurdenRateMaps.Any(),
						RateCategory = (RateCategory)r.CategoryID,
						RateCode = r.RateCode1,                     // Entity Framework adds 1 to avoid name collision.
						RateType = r.RateTypeID == null ? RateType.NotSet : (RateType)r.RateTypeID,
						DisclosureType = r.DisclosureTypeId == null ? DisclosureType.None : (DisclosureType)r.DisclosureTypeId,
						ResourceType = r.ResourceTypeID == null ? DirectRateMappingResourceType.None : (DirectRateMappingResourceType)r.ResourceTypeID,
						RevisionId = r.RevisionID,
						Section = r.SectionID ?? 0, // special case because of angular dropdowns such that a zero comes back instead of null
						UpdateDate = r.UpdateDate,
						Values = r.RateCodeYears.AsQueryable().Where(yearFilter)
												.OrderBy(y => y.Year).Select(m =>
							new RateYearModelView()
							{
								Id = m.ID,
								UpdateDate = m.UpdateDate,
								RateCodeId = m.RateCodeID,
								Year = m.Year,
								Value = m.Rate
							}).ToList()
					}).ToList();
				}
			}

			// Fill in missing RateYears and initialize ProPricer values.
			foreach (RateDetailModelView rate in rateDetails)
			{
				// Fill in missing years
				List<RateYearModelView> fullYearList = new List<RateYearModelView>();
				int lastYear = revision.StartYear;
				foreach (RateYearModelView rateYear in rate.Values)
				{
					// Year, Rate and Id should not be null.
					if (rateYear.Year != lastYear)
					{
						for (int i = lastYear; i < rateYear.Year; i++)
						{
							fullYearList.Add(new RateYearModelView()
							{
								RateCodeId = rate.Id,
								Year = i,
								Value = null
							});
							lastYear++;
						}
					}

					fullYearList.Add(rateYear);
					lastYear++;
				}
				// Fill in any missing years after last one in DB.
				if (lastYear < revision.EndYear)
				{
					for (int i = lastYear; i <= revision.EndYear; i++)
					{
						fullYearList.Add(new RateYearModelView()
						{
							RateCodeId = rate.Id,
							Year = i,
							Value = null
						});
					}
				}

				// Attach the filled in List to the Model.
				rate.Values = fullYearList;

				// Get the CategoryDescription.
				rate.RateCategoryDescription = rate.RateCategory.GetDescription();

                // If we have generated mappings set Description to empty and set Generate to true, otherwise load the description.
                foreach(ProPricerRateCodeXrefModelView mapping in rate.ProPricerMappings)
                {
                    if (mapping.RateCodeExtensionId == null)
                    {
                        rate.RateDescription = mapping.Description;
                        rate.ResourceClass = mapping.ResourceClass;
                        rate.ResourceClassId = mapping.ResourceClassId;
                    }
                    else
                    {
                        // Load Description into the property based on Id.
                        switch (mapping.RateCodeExtensionId)
                        {
                            case 1:
                                rate.RateDescription1 = mapping.Description;
                                rate.ResourceClass1 = mapping.ResourceClass;
                                rate.ResourceClassId1 = mapping.ResourceClassId;
                                break;
                            case 2:
                                rate.RateDescription2 = mapping.Description;
                                rate.ResourceClass2 = mapping.ResourceClass;
                                rate.ResourceClassId2 = mapping.ResourceClassId;
                                break;
                            case 3:
                                rate.RateDescription3 = mapping.Description;
                                rate.ResourceClass3 = mapping.ResourceClass;
                                rate.ResourceClassId3 = mapping.ResourceClassId;
                                break;
                            case 4:
                                rate.RateDescription4 = mapping.Description;
                                rate.ResourceClass4 = mapping.ResourceClass;
                                rate.ResourceClassId4 = mapping.ResourceClassId;
                                break;
                            case 5:
                                rate.RateDescription5 = mapping.Description;
                                rate.ResourceClass5 = mapping.ResourceClass;
                                rate.ResourceClassId5 = mapping.ResourceClassId;
                                break;
                            case 6:
                                rate.RateDescription6 = mapping.Description;
                                rate.ResourceClass6 = mapping.ResourceClass;
                                rate.ResourceClassId6 = mapping.ResourceClassId;
                                break;
                            case 7:
                                rate.RateDescription7 = mapping.Description;
                                rate.ResourceClass7 = mapping.ResourceClass;
                                rate.ResourceClassId7 = mapping.ResourceClassId;
                                break;
                            case 8:
                                rate.RateDescription8 = mapping.Description;
                                rate.ResourceClass8 = mapping.ResourceClass;
                                rate.ResourceClassId8 = mapping.ResourceClassId;
                                break;
                            case 9:
                                rate.RateDescription9 = mapping.Description;
                                rate.ResourceClass9 = mapping.ResourceClass;
                                rate.ResourceClassId9 = mapping.ResourceClassId;
                                break;
							case 11:
								rate.RateDescription11 = mapping.Description;
								rate.ResourceClass11 = mapping.ResourceClass;
								rate.ResourceClassId11 = mapping.ResourceClassId;
								break;
							case 12:
								rate.RateDescription12 = mapping.Description;
								rate.ResourceClass12 = mapping.ResourceClass;
								rate.ResourceClassId12 = mapping.ResourceClassId;
								break;
							case 13:
								rate.RateDescription13 = mapping.Description;
								rate.ResourceClass13 = mapping.ResourceClass;
								rate.ResourceClassId13 = mapping.ResourceClassId;
								break;
							case 14:
								rate.RateDescription14 = mapping.Description;
								rate.ResourceClass14 = mapping.ResourceClass;
								rate.ResourceClassId14 = mapping.ResourceClassId;
								break;
							case 15:
								rate.RateDescription15 = mapping.Description;
								rate.ResourceClass15 = mapping.ResourceClass;
								rate.ResourceClassId15 = mapping.ResourceClassId;
								break;
							case 21:
								rate.RateDescription21 = mapping.Description;
								rate.ResourceClass21 = mapping.ResourceClass;
								rate.ResourceClassId21 = mapping.ResourceClassId;
								break;
							case 22:
								rate.RateDescription22 = mapping.Description;
								rate.ResourceClass22 = mapping.ResourceClass;
								rate.ResourceClassId22 = mapping.ResourceClassId;
								break;
							case 23:
								rate.RateDescription23 = mapping.Description;
								rate.ResourceClass23 = mapping.ResourceClass;
								rate.ResourceClassId23 = mapping.ResourceClassId;
								break;
							case 24:
								rate.RateDescription24 = mapping.Description;
								rate.ResourceClass24 = mapping.ResourceClass;
								rate.ResourceClassId24 = mapping.ResourceClassId;
								break;
							case 25:
								rate.RateDescription25 = mapping.Description;
								rate.ResourceClass25 = mapping.ResourceClass;
								rate.ResourceClassId25 = mapping.ResourceClassId;
								break;
							case 31:
								rate.RateDescription31 = mapping.Description;
								rate.ResourceClass31 = mapping.ResourceClass;
								rate.ResourceClassId31 = mapping.ResourceClassId;
								break;
							case 32:
								rate.RateDescription32 = mapping.Description;
								rate.ResourceClass32 = mapping.ResourceClass;
								rate.ResourceClassId32 = mapping.ResourceClassId;
								break;
							case 33:
								rate.RateDescription33 = mapping.Description;
								rate.ResourceClass33 = mapping.ResourceClass;
								rate.ResourceClassId33 = mapping.ResourceClassId;
								break;
							case 34:
								rate.RateDescription34 = mapping.Description;
								rate.ResourceClass34 = mapping.ResourceClass;
								rate.ResourceClassId34 = mapping.ResourceClassId;
								break;
							case 35:
								rate.RateDescription35 = mapping.Description;
								rate.ResourceClass35 = mapping.ResourceClass;
								rate.ResourceClassId35 = mapping.ResourceClassId;
								break;
							case 41:
								rate.RateDescription41 = mapping.Description;
								rate.ResourceClass41 = mapping.ResourceClass;
								rate.ResourceClassId41 = mapping.ResourceClassId;
								break;
							case 42:
								rate.RateDescription42 = mapping.Description;
								rate.ResourceClass42 = mapping.ResourceClass;
								rate.ResourceClassId42 = mapping.ResourceClassId;
								break;
							case 43:
								rate.RateDescription43 = mapping.Description;
								rate.ResourceClass43 = mapping.ResourceClass;
								rate.ResourceClassId43 = mapping.ResourceClassId;
								break;
							case 44:
								rate.RateDescription44 = mapping.Description;
								rate.ResourceClass44 = mapping.ResourceClass;
								rate.ResourceClassId44 = mapping.ResourceClassId;
								break;
							case 45:
								rate.RateDescription45 = mapping.Description;
								rate.ResourceClass45 = mapping.ResourceClass;
								rate.ResourceClassId45 = mapping.ResourceClassId;
								break;
							case 51:
								rate.RateDescription51 = mapping.Description;
								rate.ResourceClass51 = mapping.ResourceClass;
								rate.ResourceClassId51 = mapping.ResourceClassId;
								break;
							case 52:
								rate.RateDescription52 = mapping.Description;
								rate.ResourceClass52 = mapping.ResourceClass;
								rate.ResourceClassId52 = mapping.ResourceClassId;
								break;
							case 53:
								rate.RateDescription53 = mapping.Description;
								rate.ResourceClass53 = mapping.ResourceClass;
								rate.ResourceClassId53 = mapping.ResourceClassId;
								break;
							case 54:
								rate.RateDescription54 = mapping.Description;
								rate.ResourceClass54 = mapping.ResourceClass;
								rate.ResourceClassId54 = mapping.ResourceClassId;
								break;
							case 55:
								rate.RateDescription55 = mapping.Description;
								rate.ResourceClass55 = mapping.ResourceClass;
								rate.ResourceClassId55 = mapping.ResourceClassId;
								break;
							case 61:
								rate.RateDescription61 = mapping.Description;
								rate.ResourceClass61 = mapping.ResourceClass;
								rate.ResourceClassId61 = mapping.ResourceClassId;
								break;
							case 62:
								rate.RateDescription62 = mapping.Description;
								rate.ResourceClass62 = mapping.ResourceClass;
								rate.ResourceClassId62 = mapping.ResourceClassId;
								break;
							case 63:
								rate.RateDescription63 = mapping.Description;
								rate.ResourceClass63 = mapping.ResourceClass;
								rate.ResourceClassId63 = mapping.ResourceClassId;
								break;
							case 64:
								rate.RateDescription64 = mapping.Description;
								rate.ResourceClass64 = mapping.ResourceClass;
								rate.ResourceClassId64 = mapping.ResourceClassId;
								break;
							case 65:
								rate.RateDescription65 = mapping.Description;
								rate.ResourceClass65 = mapping.ResourceClass;
								rate.ResourceClassId65 = mapping.ResourceClassId;
								break;
							case 71:
								rate.RateDescription71 = mapping.Description;
								rate.ResourceClass71 = mapping.ResourceClass;
								rate.ResourceClassId71 = mapping.ResourceClassId;
								break;
							case 72:
								rate.RateDescription72 = mapping.Description;
								rate.ResourceClass72 = mapping.ResourceClass;
								rate.ResourceClassId72 = mapping.ResourceClassId;
								break;
							case 73:
								rate.RateDescription73 = mapping.Description;
								rate.ResourceClass73 = mapping.ResourceClass;
								rate.ResourceClassId73 = mapping.ResourceClassId;
								break;
							case 74:
								rate.RateDescription74 = mapping.Description;
								rate.ResourceClass74 = mapping.ResourceClass;
								rate.ResourceClassId74 = mapping.ResourceClassId;
								break;
							case 75:
								rate.RateDescription75 = mapping.Description;
								rate.ResourceClass75 = mapping.ResourceClass;
								rate.ResourceClassId75 = mapping.ResourceClassId;
								break;
							case 81:
								rate.RateDescription81 = mapping.Description;
								rate.ResourceClass81 = mapping.ResourceClass;
								rate.ResourceClassId81 = mapping.ResourceClassId;
								break;
							case 82:
								rate.RateDescription82 = mapping.Description;
								rate.ResourceClass82 = mapping.ResourceClass;
								rate.ResourceClassId82 = mapping.ResourceClassId;
								break;
							case 83:
								rate.RateDescription83 = mapping.Description;
								rate.ResourceClass83 = mapping.ResourceClass;
								rate.ResourceClassId83 = mapping.ResourceClassId;
								break;
							case 84:
								rate.RateDescription84 = mapping.Description;
								rate.ResourceClass84 = mapping.ResourceClass;
								rate.ResourceClassId84 = mapping.ResourceClassId;
								break;
							case 85:
								rate.RateDescription85 = mapping.Description;
								rate.ResourceClass85 = mapping.ResourceClass;
								rate.ResourceClassId85 = mapping.ResourceClassId;
								break;
							case 91:
								rate.RateDescription91 = mapping.Description;
								rate.ResourceClass91 = mapping.ResourceClass;
								rate.ResourceClassId91 = mapping.ResourceClassId;
								break;
							case 92:
								rate.RateDescription92 = mapping.Description;
								rate.ResourceClass92 = mapping.ResourceClass;
								rate.ResourceClassId92 = mapping.ResourceClassId;
								break;
							case 93:
								rate.RateDescription93 = mapping.Description;
								rate.ResourceClass93 = mapping.ResourceClass;
								rate.ResourceClassId93 = mapping.ResourceClassId;
								break;
							case 94:
								rate.RateDescription94 = mapping.Description;
								rate.ResourceClass94 = mapping.ResourceClass;
								rate.ResourceClassId94 = mapping.ResourceClassId;
								break;
							case 95:
								rate.RateDescription95 = mapping.Description;
								rate.ResourceClass95 = mapping.ResourceClass;
								rate.ResourceClassId95 = mapping.ResourceClassId;
								break;
						}

						rate.GenerateAdditionalDirectLaborRates = true;
					}
				}
			}

			return rateDetails;
		}

		/// <summary>
		/// Gets the rate codes for a revision.
		/// </summary>
		/// <param name="revisionId">The revision identifier.</param>
		/// <returns>A collection of Rate Codes</returns>
		public ICollection<RateDto> GetRateCodesForRevision(int revisionId)
		{
			List<RateDto> rateCodes;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (IESEntities context = new IESEntities())
				{
					rateCodes = context.RateCodes.Where(r => r.RevisionID == revisionId)
					.OrderBy(y => y.RateCode1)
					.Select(r =>
					new RateDto()
					{
						Rate = r.RateCode1,
						Desc = r.Description
					}).ToList();
				}
			}

			return rateCodes;
		}

		/// <summary>
		/// Verifies the rate code replication rate codes.
		/// </summary>
		/// <param name="revisionId">The revision identifier for WIP.</param>
		/// <returns>A list of validation errors if there are any.</returns>
		public ICollection<ValidationMessage> VerifyRateCodeReplication(int revisionId)
		{
			List<ValidationMessage> messages;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (IESEntities context = new IESEntities())
				{
					messages = context.verifyRateCodeReplication(revisionId)
					.Select(r =>
					new ValidationMessage(string.Format("Invalid Rate Code {0} found in '{1}' Rate Code Replication Column", r.RateCode, r.ColumnName)))
					.ToList();
				}
			}

			return messages;
		}

		/// <summary>
		/// Get rates by Revision.
		/// </summary>
		/// <param name="revision">Revision to retrieve.</param>
		/// <returns>All Rate Details for the given revision.</returns>
		public ICollection<RateDetailModelView> GetRatesByRevision(RevisionModelView revision)
		{
			return this.GetRates(revision);
		}

		/// <summary>
		/// Get the Rate Details for the rates being imported.
		/// </summary>
		/// <param name="revision">Revision to retrieve.</param>
		/// <param name="rateCodes">Rate Codes being imported.</param>
		/// <returns>All Rate Details for the given revision.</returns>
		public ICollection<RateDetailModelView> GetRatesForImport(RevisionModelView revision, string[] rateCodes)
		{
			return this.GetRates(revision, rateCodes: rateCodes);
		}

		/// <summary>
		/// Get Rates with changes between revisions.
		/// </summary>
		/// <param name="revision">Revision to retrieve.</param>
		/// <param name="previousRevision">Optional parameter get two compare sets with one DB call.</param>
		/// <returns>All Rate Details for the given revision.</returns>
		public ICollection<RateDetailModelView> GetComparableRates(RevisionOptionModelView revision, RevisionOptionModelView previousRevision)
		{
			ICollection<RateDetailModelView> comparableRateDetails = new Collection<RateDetailModelView>();

			if (previousRevision != null)
			{
				ICollection<RateDetailModelView> allRates = this.GetRates(new RevisionModelView(revision), compareRevision: new RevisionModelView(previousRevision));
				ICollection<RateDetailModelView> currentRates = allRates.Where(r => r.RevisionId == revision.Id).ToList();
				ICollection<RateDetailModelView> previousRates = allRates.Where(r => r.RevisionId == previousRevision.Id).ToList();

				// Include full range of years from both revisions.
				int startYear = revision.StartYear < previousRevision.StartYear ? revision.StartYear : previousRevision.StartYear;
				int endYear = revision.EndYear > previousRevision.EndYear ? revision.EndYear : previousRevision.EndYear;
				int minDifferenceYear = endYear;
				int maxDifferenceYear = startYear;

				HashSet<string> allRateCodes = new HashSet<string>(currentRates.Concat(previousRates).Select(r => r.RateCode).ToList());

				foreach (string rateCode in allRateCodes.OrderBy(s => s))
				{
					RateDetailModelView currentRate = currentRates.FirstOrDefault(r => r.RateCode == rateCode);
					RateDetailModelView previousRate = previousRates.FirstOrDefault(r => r.RateCode == rateCode);
					bool rateHasChanged = false;

					// Mark adds and deletes.
					if (currentRate == null)
					{
						// if both current and previous are null, nothing to compare
						if (previousRate != null)
						{
							previousRate.CompareState = RateCompareState.Deleted.GetDescription();
							comparableRateDetails.Add(previousRate);
						}
					}
					else if (previousRate == null)
					{
						currentRate.CompareState = RateCompareState.Added.GetDescription();
						comparableRateDetails.Add(currentRate);
					}
					else
					{
						// Both rate codes exist, check rate values.
						for (int i = startYear; i <= endYear; i++)
						{
							RateYearModelView previousRateYear = previousRate.Values.FirstOrDefault(r => r.Year == i);
							RateYearModelView currentRateYear = currentRate.Values.FirstOrDefault(r => r.Year == i);

							// null check, if null then create but leave .Value null for below checks
							if (previousRateYear == null)
							{
								previousRateYear = new RateYearModelView();
							}

							if (currentRateYear == null)
							{
								currentRateYear = new RateYearModelView();
							}

							if (previousRateYear.Value != currentRateYear.Value)
							{
								rateHasChanged = true;

								// Find the range of years that have changes.
								if (i < minDifferenceYear)
								{
									minDifferenceYear = i;
								}

								if (i > maxDifferenceYear)
								{
									maxDifferenceYear = i;
								}

								currentRateYear.PercentChange = (currentRateYear.Value.HasValue && previousRateYear.Value.HasValue && previousRateYear.Value.Value != 0) ?
																Math.Abs((currentRateYear.Value.Value / previousRateYear.Value.Value - 1) * 100) :
																(decimal)0.01;
							}
						}
					}

					if (rateHasChanged)
					{
						if (string.IsNullOrEmpty(currentRate.CompareState))
						{
							currentRate.CompareState = RateCompareState.Edit.GetDescription();
						}

						comparableRateDetails.Add(currentRate);
						comparableRateDetails.Add(previousRate);
					}
				}

				// Remove years that have no differences.
				foreach (RateDetailModelView rate in comparableRateDetails)
				{
					rate.Values = rate.Values.Where(v => v.Year >= minDifferenceYear && v.Year <= maxDifferenceYear).ToList();
				}
			}

			return comparableRateDetails;
		}

		/// <summary>
		/// Gets all Resource Classes as an options list.
		/// </summary>
		/// <param name="revisionId">Revision Id</param>
		/// <returns>Resource Class options list.</returns>
		public ICollection<OptionModelView> GetResourceClassOptions(int revisionId)
		{
			ICollection<OptionModelView> result;
			using (IESEntities context = new IESEntities())
			{
				// get Resource Classes for this revision
				result = context.ResourceClassLUs
					.Select(rc => new OptionModelView()
					{
						Id = rc.ID,
						Label = rc.Description
					})
					.ToCollection();
			}

			// Add blank option
			result.Add(new OptionModelView() { Id = 0, Label = string.Empty });
			return result.OrderBy(x => x.Label).ToList();
		}

		/// <summary>
		/// Gets the necessary details of Rate Codes attached to Sections for a given Revision for the RDSB Edit Document dropdowns.
		/// Only retrieve rates for Direct Labor.
		/// </summary>
		/// <param name="revisionId">Revision ID</param>
		/// <returns>necessary details of Rate Codes for the RDSB Edit Document dropdown</returns>
		public ICollection<RdsbRateDetailModelView> GetRatesForRdsbDocument(int revisionId)
		{
			ICollection<RdsbRateDetailModelView> toReturn;

			int directLaborId = (int)RateCategory.DirectLabor;
			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (IESEntities context = new IESEntities())
				{
					toReturn = context.RateCodes.Where(x => x.RevisionID == revisionId && x.SectionID.HasValue && x.CategoryID == directLaborId)
						.OrderBy(y => y.CategoryID)
						.Select(r => new RdsbRateDetailModelView()
						{
							Id = r.ID,
							RateCode = r.RateCode1,
							Description = r.Description,
							Section = r.SectionID.Value
						}).OrderBy(x => x.RateCode).ToCollection();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets all objects based on the IDs that were passed in
		/// </summary>
		/// <param name="ids">IDs</param>
		/// <returns>
		/// Corresponding Data
		/// </returns>
		public override ICollection<RateDetailModelView> GetByIds(ICollection<int> ids)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Commits

		/// <summary>
		/// Upsert
		/// </summary>
		/// <param name="dtoToUpsert">Dto that is upserted</param>
		/// <returns>Id of the dto after the modification</returns>
		/// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
		protected override int? Upsert(RateDetailModelView dtoToUpsert)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// RateDescriptionsToMappings
		/// </summary>
		/// <param name="newRateCode">Rate Code for new objects.</param>
		/// <param name="dtoToUpsert">The RateDetail object.</param>
		public void RateDescriptionsToMappings(int? newRateCode, RateDetailModelView dtoToUpsert)
		{
			if (dtoToUpsert == null)
			{
				throw new ArgumentNullException(nameof(dtoToUpsert));
			}

			if (dtoToUpsert.ProPricerMappings == null)
			{
				dtoToUpsert.ProPricerMappings = this.proPricerXrefLoader.GetByRateCodeId(dtoToUpsert.Id);
			}

            string[] descs = GetRateDescriptions(dtoToUpsert);
            int?[] resourceClassIds = GetResourceIds(dtoToUpsert);

			for (int i = 0; i < this.numbers.Length; i++)
			{
				string desc = descs[i];
				int? number = this.numbers[i];
				//bool isOneLMX = number.HasValue && number.Value > 10;
				string newDescription = string.Empty;
				int? newResourceClassId = 0;
				ProPricerRateCodeXrefModelView previousXref = null;

				// Get previous and new descriptions for this Xref.
				if (dtoToUpsert.ProPricerMappings != null)
				{
					previousXref = dtoToUpsert.ProPricerMappings.FirstOrDefault(x => x.RateCodeExtensionId == number);
				}

				if (desc != null)
				{
					newDescription = desc.Trim(' ');
					// only set resource class if corresponding description is populated
					newResourceClassId = resourceClassIds[i];
				}

				// The first part of this if statement deals with the rates we are keeping based on the GenerateAdditionalDirectLaborRates flag.
				// Everything else is deleted.
				if ((dtoToUpsert.GenerateAdditionalDirectLaborRates != true && !number.HasValue) ||
					(dtoToUpsert.GenerateAdditionalDirectLaborRates && number.HasValue && number.Value > 0))
				{
					// There has been a change to an existing xref.
					if (previousXref != null &&
						(newDescription != previousXref.Description || newResourceClassId != previousXref.ResourceClassId))
					{
						// If user blanked out the description, delete the existing row.
						if (newDescription.Length == 0)
						{
							previousXref.Updateable = UpdateType.Deleted;
						}
						else
						{
							// Update the existing row.
							previousXref.Updateable = UpdateType.Upsert;
							previousXref.Description = newDescription;
							previousXref.ResourceClassId = newResourceClassId;
						}
					}
					else if (previousXref == null && newDescription.Length > 0)
					{
						// No previous xref, upsert the new xref.
						ProPricerRateCodeXrefModelView newXref = new ProPricerRateCodeXrefModelView()
						{
							Id = -1,
							Updateable = UpdateType.Upsert,
							UpdateDate = DateTime.Now,
							RateCodeId = (int)newRateCode,
							Description = newDescription,
							ResourceClassId = newResourceClassId,
							RateCodeExtensionId = number
						};

						// Add the new xref to the dto.
						dtoToUpsert.ProPricerMappings.Add(newXref);
					}
				}
				else
				{
					if (previousXref != null)
					{
						previousXref.Updateable = UpdateType.Deleted;
					}
				}
			}
		}

		/// <summary>
		/// Delete
		/// </summary>
		/// <param name="dtoToDelete">Dto that is deleted</param>
		/// <returns>Id of the deleted dto</returns>
		/// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
		protected override int? Delete(RateDetailModelView dtoToDelete)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Provides the metadata to support bulk save processing for RateCodes
		/// </summary>
		/// <exception cref="NotImplementedException">Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
		/// <returns>Meta data required for bulk save processing</returns>
		public override BulkSaveMetaData CreateBulkSaveMetaData()
		{
			BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.IES_DB_CONTEXT_NAME);

			metaData.BulkDeleteStoredProcedureName = "deleteRateCodeviaTableParameter";
			metaData.BulkInsertStoredProcedureName = "insertRateCodeviaTableParameter";
			metaData.BulkUpdateStoredProcedureName = "updateRateCodeviaTableParameter";

			metaData.BulkInsertStoredProcedureReturnsUpdateDate = true;
			metaData.BulkUpdateStoredProcedureReturnsUpdateDate = true;

			metaData.DBTableTypeName = "TT_RateCode";
			metaData.StoredProcedureTableTypeParameterName = "@RateCodeParam";

			// Entity Framework renamed RateCode column to RateCode1 to avoid name collision.
			metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
			{
				"ID", "UpdateDate", "RevisionID", "CategoryID", "Description",
				"SectionID", "RateCode1", "ResourceTypeID", "GovernmentBurdenPoolID", "CommercialBurdenPoolID",
				"RateTypeID", "DisclosureTypeId"
			};

			return metaData;
		}

		/// <summary>
		/// Bulk save Rate Details.
		/// </summary>
		/// <param name="dtosToSave">dtos to save</param>
		/// <returns>Dictionary where the key is the old dto id and the value is the new id.</returns>
		public override IDictionary<int, int> BulkSave(ICollection<RateDetailModelView> dtosToSave)
		{
			IDictionary<int, int> toReturn = new Dictionary<int, int>();

			if (dtosToSave == null)
			{
				throw new ArgumentNullException(nameof(dtosToSave));
			}

			if (dtosToSave.Any(d => d.Updateable == UpdateType.None))
			{
				throw new ArgumentException("One or more Task Elements has UpdateType of None.", nameof(dtosToSave));
			}

			if (dtosToSave.Any())
			{
				this.Log.Debug(string.Format("RateDetailLoader.BulkSave => Ntid: {2}, RateCodeId: {1}, Count: {0}",
				dtosToSave.Count, dtosToSave.First().Id, System.Threading.Thread.CurrentPrincipal.Identity.Name));

				foreach (RateDetailModelView dto in dtosToSave)
				{
					this.Log.Debug(string.Format("RateDetailLoader.BulkSave => Item Id: {0}, Value: {1}", dto.Id, dto.RateCode));
				}

				using (StopwatchTimer sw = new StopwatchTimer(this.Log))
				{
					ICollection<RateDetailModelView> upsertableRateDetailCollection = dtosToSave.Where(d => d.Updateable != UpdateType.None).ToCollection();

					// Set the RateType, ResourceType, Code1, GovernmentBurdenPoolId, CommercialBurdenPoolId, and ResourceClassId values to null if the selected value is empty (not set)
					foreach (RateDetailModelView rateDetailModelView in dtosToSave)
					{
						rateDetailModelView.RateType = rateDetailModelView.RateType.HasValue && rateDetailModelView.RateType.Value == RateType.NotSet ? null : rateDetailModelView.RateType;
						rateDetailModelView.DisclosureType = rateDetailModelView.DisclosureType.HasValue && rateDetailModelView.DisclosureType.Value == DisclosureType.None ? null : rateDetailModelView.DisclosureType;
						rateDetailModelView.ResourceType = rateDetailModelView.ResourceType.HasValue && rateDetailModelView.ResourceType.Value == DirectRateMappingResourceType.None ? null : rateDetailModelView.ResourceType;
						rateDetailModelView.GovernmentBurdenPoolId = rateDetailModelView.GovernmentBurdenPoolId.HasValue && rateDetailModelView.GovernmentBurdenPoolId.Value < 1 ? null : rateDetailModelView.GovernmentBurdenPoolId;
						rateDetailModelView.CommercialBurdenPoolId = rateDetailModelView.CommercialBurdenPoolId.HasValue && rateDetailModelView.CommercialBurdenPoolId.Value < 1 ? null : rateDetailModelView.CommercialBurdenPoolId;
						if (rateDetailModelView.ProPricerMappings != null && rateDetailModelView.ProPricerMappings.Any())
						{
							foreach (ProPricerRateCodeXrefModelView xrefModelView in rateDetailModelView.ProPricerMappings)
							{
								xrefModelView.ResourceClassId = xrefModelView.ResourceClassId.HasValue && xrefModelView.ResourceClassId.Value < 1 ? null : xrefModelView.ResourceClassId;
							}
						}
					}

					// Bulk save Rate Details
					toReturn = base.BulkSave(dtosToSave);

					// remove the deleted Rate Details from the list before proceeding          
					upsertableRateDetailCollection = (from te in upsertableRateDetailCollection
													  where te.Updateable == UpdateType.None
													  select te).ToCollection();

					ICollection<ProPricerRateCodeXrefModelView> saveableProPricerXrefs = (from rateDetail in upsertableRateDetailCollection
																						  where rateDetail.ProPricerMappings != null
																						  from proPricerMapping in rateDetail.ProPricerMappings
																						  where proPricerMapping.Updateable != UpdateType.None
																						  select proPricerMapping).ToCollection();

					if (saveableProPricerXrefs.Any())
					{
						// Give ProPricerXref with negative ids new ids to prevent duplicates
						int tempId = -1;
						foreach (var proPricerXref in saveableProPricerXrefs)
						{
							if (proPricerXref.Id < 0)
							{
								proPricerXref.Id = tempId;
								tempId--;
							}
						}

						this.proPricerXrefLoader.BulkSave(saveableProPricerXrefs);
					}

					// Grab all the RateYears that are to be deleted, inserted, or updated
					ICollection<RateYearModelView> saveableRateYears = (from rateDetail in upsertableRateDetailCollection
																		where rateDetail.Values != null
																		from rateYear in rateDetail.Values
																		where rateYear != null
																		&& rateYear.Updateable != UpdateType.None
																		select rateYear).ToCollection();

					if (saveableRateYears.Any())
					{
						// Give Rate Years with negative ids new ids to prevent duplicates
						int tempId = -1;
						foreach (var rateYear in saveableRateYears)
						{
							if (rateYear.Id < 0)
							{
								rateYear.Id = tempId;
								tempId--;
							}
						}

						this.rateYearLoader.BulkSave(saveableRateYears);
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Converts the DTO into an entity.
		/// </summary>
		/// <param name="dtoToConvert">dto to convert</param>
		/// <returns>entity representing the dto</returns>
		protected override RateCode ConvertDtoToEntity(RateDetailModelView dtoToConvert)
		{
			RateCode entity = new RateCode();

			if (dtoToConvert == null)
			{
				throw new ArgumentNullException(nameof(dtoToConvert));
			}

			entity.ID = dtoToConvert.Id;
			entity.UpdateDate = dtoToConvert.UpdateDate;
			entity.RevisionID = dtoToConvert.RevisionId;
			entity.CategoryID = (int)dtoToConvert.RateCategory;
			entity.Description = dtoToConvert.Description;
			if (dtoToConvert.Section == 0)
			{
				// this is a special case because of angular dropdowns such that a zero comes back instead of null
				dtoToConvert.Section = null;
			}

			entity.SectionID = dtoToConvert.Section;
			entity.RateCode1 = dtoToConvert.RateCode;
			entity.ResourceTypeID = (int?)dtoToConvert.ResourceType;
			if (dtoToConvert.GovernmentBurdenPoolId == 0)
			{
				// this is a special case because of angular dropdowns such that a zero comes back instead of null
				dtoToConvert.GovernmentBurdenPoolId = null;
			}

			entity.GovernmentBurdenPoolID = dtoToConvert.GovernmentBurdenPoolId;
			if (dtoToConvert.CommercialBurdenPoolId == 0)
			{
				// this is a special case because of angular dropdowns such that a zero comes back instead of null
				dtoToConvert.CommercialBurdenPoolId = null;
			}

			entity.CommercialBurdenPoolID = dtoToConvert.CommercialBurdenPoolId;
			entity.RateTypeID = (int?)dtoToConvert.RateType;
			entity.DisclosureTypeId = (int?)dtoToConvert.DisclosureType;

			return entity;
		}

		/// <summary>
		/// Save RateGridModelView to database and reset Dirty flags.
		/// </summary>
		/// <param name="dirtyRateDetails">The collection of RateDetailModelViews with changed.</param>
		public void SaveDetails(ICollection<RateDetailModelView> dirtyRateDetails)
		{
			if (dirtyRateDetails == null)
			{
				throw new ArgumentNullException(nameof(dirtyRateDetails));
			}

			// Save all the Dirty RateDetails to the database.
			foreach (RateDetailModelView rdmv in dirtyRateDetails)
			{
				if (rdmv.Dirty)
				{
					if (rdmv.IsDeleted)
					{
						rdmv.Updateable = UpdateType.Deleted;
					}
					else
					{
						rdmv.Updateable = UpdateType.Upsert;

						// Move RateDescriptions into the ProPricerMappings collection.
						this.RateDescriptionsToMappings(rdmv.Id, rdmv);

						foreach (RateYearModelView rateYear in rdmv.Values)
						{
							if (rateYear.Dirty)
							{
								rateYear.Updateable = UpdateType.Upsert;
							}
						}
					}
				}
			}

			// Using Bulk Save
			this.BulkSave(dirtyRateDetails);
		}
		#endregion

		#region Private Functions

		private string[] GetRateDescriptions(RateDetailModelView dtoToUpsert)
		{
			return new string [] { 
				dtoToUpsert.RateDescription, dtoToUpsert.RateDescription1, dtoToUpsert.RateDescription2, dtoToUpsert.RateDescription3, dtoToUpsert.RateDescription4, dtoToUpsert.RateDescription5, 
				dtoToUpsert.RateDescription6, dtoToUpsert.RateDescription7, dtoToUpsert.RateDescription8, dtoToUpsert.RateDescription9,

				dtoToUpsert.RateDescription11, dtoToUpsert.RateDescription12, dtoToUpsert.RateDescription13, dtoToUpsert.RateDescription14, dtoToUpsert.RateDescription15,
				dtoToUpsert.RateDescription21, dtoToUpsert.RateDescription22, dtoToUpsert.RateDescription23, dtoToUpsert.RateDescription24, dtoToUpsert.RateDescription25,
				dtoToUpsert.RateDescription31, dtoToUpsert.RateDescription32, dtoToUpsert.RateDescription33, dtoToUpsert.RateDescription34, dtoToUpsert.RateDescription35,
				dtoToUpsert.RateDescription41, dtoToUpsert.RateDescription42, dtoToUpsert.RateDescription43, dtoToUpsert.RateDescription44, dtoToUpsert.RateDescription45,
				dtoToUpsert.RateDescription51, dtoToUpsert.RateDescription52, dtoToUpsert.RateDescription53, dtoToUpsert.RateDescription54, dtoToUpsert.RateDescription55,
				dtoToUpsert.RateDescription61, dtoToUpsert.RateDescription62, dtoToUpsert.RateDescription63, dtoToUpsert.RateDescription64, dtoToUpsert.RateDescription65,
				dtoToUpsert.RateDescription71, dtoToUpsert.RateDescription72, dtoToUpsert.RateDescription73, dtoToUpsert.RateDescription74, dtoToUpsert.RateDescription75,
				dtoToUpsert.RateDescription81, dtoToUpsert.RateDescription82, dtoToUpsert.RateDescription83, dtoToUpsert.RateDescription84, dtoToUpsert.RateDescription85,
				dtoToUpsert.RateDescription91, dtoToUpsert.RateDescription92, dtoToUpsert.RateDescription93, dtoToUpsert.RateDescription94, dtoToUpsert.RateDescription95,
			};
		}

		private int?[] GetResourceIds(RateDetailModelView dtoToUpsert)
		{
			return new int?[] {
				dtoToUpsert.ResourceClassId, dtoToUpsert.ResourceClassId1, dtoToUpsert.ResourceClassId2, dtoToUpsert.ResourceClassId3, dtoToUpsert.ResourceClassId4, dtoToUpsert.ResourceClassId5,
				dtoToUpsert.ResourceClassId6, dtoToUpsert.ResourceClassId7, dtoToUpsert.ResourceClassId8, dtoToUpsert.ResourceClassId9,

				dtoToUpsert.ResourceClassId11, dtoToUpsert.ResourceClassId12, dtoToUpsert.ResourceClassId13, dtoToUpsert.ResourceClassId14, dtoToUpsert.ResourceClassId15,
				dtoToUpsert.ResourceClassId21, dtoToUpsert.ResourceClassId22, dtoToUpsert.ResourceClassId23, dtoToUpsert.ResourceClassId24, dtoToUpsert.ResourceClassId25,
				dtoToUpsert.ResourceClassId31, dtoToUpsert.ResourceClassId32, dtoToUpsert.ResourceClassId33, dtoToUpsert.ResourceClassId34, dtoToUpsert.ResourceClassId35,
				dtoToUpsert.ResourceClassId41, dtoToUpsert.ResourceClassId42, dtoToUpsert.ResourceClassId43, dtoToUpsert.ResourceClassId44, dtoToUpsert.ResourceClassId45,
				dtoToUpsert.ResourceClassId51, dtoToUpsert.ResourceClassId52, dtoToUpsert.ResourceClassId53, dtoToUpsert.ResourceClassId54, dtoToUpsert.ResourceClassId55,
				dtoToUpsert.ResourceClassId61, dtoToUpsert.ResourceClassId62, dtoToUpsert.ResourceClassId63, dtoToUpsert.ResourceClassId64, dtoToUpsert.ResourceClassId65,
				dtoToUpsert.ResourceClassId71, dtoToUpsert.ResourceClassId72, dtoToUpsert.ResourceClassId73, dtoToUpsert.ResourceClassId74, dtoToUpsert.ResourceClassId75,
				dtoToUpsert.ResourceClassId81, dtoToUpsert.ResourceClassId82, dtoToUpsert.ResourceClassId83, dtoToUpsert.ResourceClassId84, dtoToUpsert.ResourceClassId85,
				dtoToUpsert.ResourceClassId91, dtoToUpsert.ResourceClassId92, dtoToUpsert.ResourceClassId93, dtoToUpsert.ResourceClassId94, dtoToUpsert.ResourceClassId95
			};
		} 

		#endregion
	}
}