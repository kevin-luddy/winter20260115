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
        private readonly int?[] numbers = { null, 1, 2, 3, 4, 5, 6, 7, 8 };

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

            string[] descs = { dtoToUpsert.RateDescription, dtoToUpsert.RateDescription1, dtoToUpsert.RateDescription2, dtoToUpsert.RateDescription3, dtoToUpsert.RateDescription4, dtoToUpsert.RateDescription5, dtoToUpsert.RateDescription6, dtoToUpsert.RateDescription7, dtoToUpsert.RateDescription8 };
            int?[] resourceClassIds = { dtoToUpsert.ResourceClassId, dtoToUpsert.ResourceClassId1, dtoToUpsert.ResourceClassId2, dtoToUpsert.ResourceClassId3, dtoToUpsert.ResourceClassId4, dtoToUpsert.ResourceClassId5, dtoToUpsert.ResourceClassId6, dtoToUpsert.ResourceClassId7, dtoToUpsert.ResourceClassId8 };
            var numbersAndDescs = this.numbers.Zip(descs, (first, second) => new Tuple<int?, string>(first, second));

            foreach (Tuple<int?, string> numberAndDesc in numbersAndDescs)
            {
                int index = numberAndDesc.Item1.HasValue ? numberAndDesc.Item1.Value : 0;
                string newDescription = string.Empty;
                int? newResourceClassId = 0;
                ProPricerRateCodeXrefModelView previousXref = null;
                
                // Get previous and new descriptions for this Xref.
                if (dtoToUpsert.ProPricerMappings != null)
                {
                    previousXref = dtoToUpsert.ProPricerMappings.FirstOrDefault(x => x.RateCodeExtensionId == numberAndDesc.Item1);
                }

                if (numberAndDesc.Item2 != null)
                {
                    newDescription = numberAndDesc.Item2.ToString().Trim(' ');
                    // only set resource class if corresponding description is populated
                    newResourceClassId = resourceClassIds[index];
                }

                // The first part of this if statement deals with the rates we are keeping based on the GenerateAdditionalDirectLaborRates flag.
                // Everything else is deleted.
                if ((dtoToUpsert.GenerateAdditionalDirectLaborRates != true && !numberAndDesc.Item1.HasValue) ||
                    (dtoToUpsert.GenerateAdditionalDirectLaborRates && numberAndDesc.Item1.HasValue && numberAndDesc.Item1.Value > 0))
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
                            RateCodeExtensionId = numberAndDesc.Item1
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
                "RateTypeID"
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
    }
}