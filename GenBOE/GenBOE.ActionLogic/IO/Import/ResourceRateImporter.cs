// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    public abstract class ResourceRateImporter
    {
        public Collection<ResourceRatesImportResult> sequentialDateCheck(ICollection<ResourceRateDTO> inExistingDates, ICollection<ResourceRateDTO> datesToModify)
        {
            ICollection<ResourceRateDTO> ratesToDelete = datesToModify.Where(x => x.Updateable == UpdateType.Deleted).ToList();
            ICollection<ResourceRateDTO> ratesToAdd = datesToModify.Where(x => x.Updateable == UpdateType.Upsert && x.ResourceRateID < 0).ToList();

            HashSet<int> rateIDsToDelete = new HashSet<int>(ratesToDelete.Select(x => x.ResourceRateID));
            // remove all deleted rates, and those without start/end dates
            List<ResourceRateDTO> finalDateRangeToAssess = inExistingDates.Where(x => !rateIDsToDelete.Contains(x.ResourceRateID) && x.StartDate.HasValue && x.EndDate.HasValue).ToList();
            finalDateRangeToAssess.AddRange(ratesToAdd);

            return this.sequentialDateCheck(finalDateRangeToAssess);
        }

        public Collection<ResourceRatesImportResult> sequentialDateCheck(ICollection<ResourceRateDTO> resourceRates)
        {
            Collection<ResourceRatesImportResult> results = new Collection<ResourceRatesImportResult>();

            IList<ResourceRateDTO> resultingResourceRates = resourceRates.OrderBy(x => x.StartDate).ToList<ResourceRateDTO>();

            for (int i = 1; i < resultingResourceRates.Count; i++)
            {
                if (results.Contains(ResourceRatesImportResult.MissingDateRanges) && results.Contains(ResourceRatesImportResult.OverlappingDateRanges))
                {
                    break;
                }

                // start and end dates should always have values, if they don't then there is a logic error in the above code
                DateTime endDatePlusOneMonth = resultingResourceRates[i - 1].EndDate.Value.Normalize().AddMonths(1);
                DateTime nextStartDate = resultingResourceRates[i].StartDate.Value.Normalize();
                if (endDatePlusOneMonth != nextStartDate)
                {
                    if (resultingResourceRates[i - 1].EndDate.Value.Normalize().AddMonths(1) < resultingResourceRates[i].StartDate.Value.Normalize())
                    {
                        results.Add(ResourceRatesImportResult.MissingDateRanges);
                    }
                    else
                    {
                        results.Add(ResourceRatesImportResult.OverlappingDateRanges);
                    }
                }
            }

            return results;
        }
    }

    public enum ResourceRatesImportResult
    {
        Updated = 0,
        Added = 1,
        MissingCol = 2,
        MissingResource = 3,
        InvalidStartDateFormat = 4,
        InvalidEndDateFormat = 5,
        InvalidRateFormat = 6,
        OverlappingDateRanges = 7,
        MissingDateRanges = 8,
        RateIsInUse = 9,
        NotUpdateable = 10,
        InvalidDateRange = 11,
        MissingRequiredData = 12,
        InvalidLaborCatagory = 13,
        InvalidRateRange = 14,
        CanNotImportMappedRates = 15
    }
}