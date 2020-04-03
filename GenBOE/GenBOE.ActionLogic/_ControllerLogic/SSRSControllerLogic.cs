// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Text;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.SSRS;
    using GenBOE.ActionLogic.Workspace;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// The Logic class for the SSRS Controller.
    /// </summary>
    /// <seealso cref="GenBOE.ActionLogic.ControllerLogic.ISSRSControllerLogic" />
    public class SSRSControllerLogic : ISSRSControllerLogic
    {
        /// <summary>
        /// The clin label
        /// </summary>
        private const string CLIN_LABEL = "CLIN";

        /// <summary>
        /// The category label
        /// </summary>
        private const string CATEGORY_LABEL = "Category";

        /// <summary>
        /// The offload rates loader.
        /// </summary>
        private readonly IOffloadRatesDTOLoader offloadRatesLoader;

        /// <summary>
        /// The resource loader
        /// </summary>
        private readonly IResourceDTODataLoader resourceLoader;

        /// <summary>
        /// The workspace loader.
        /// </summary>
        private readonly IWorkspaceDTODataLoader workspaceLoader;

        /// <summary>
        /// The common data mapper.
        /// </summary>
        private readonly ICommonDataMapper commonDataMapper;

        /// <summary>
        /// The logger
        /// </summary>
        private Logger logger = new Logger(typeof(SSRSControllerLogic));


        /// <summary>
        /// SSRSControllerLogic Constructer
        /// </summary>
        /// <param name="offloadRatesDTOLoader">The offload rates dto loader.</param>
        /// <param name="resourceLoader">The resource loader.</param>
        /// <param name="workspaceLoader">The workspace loader.</param>
        /// <param name="commonDataMapper">The common data mapper</param>
        public SSRSControllerLogic(IOffloadRatesDTOLoader offloadRatesDTOLoader, IResourceDTODataLoader resourceLoader, IWorkspaceDTODataLoader workspaceLoader, ICommonDataMapper commonDataMapper)
        {
            this.offloadRatesLoader = offloadRatesDTOLoader;
            this.resourceLoader = resourceLoader;
            this.workspaceLoader = workspaceLoader;
            this.commonDataMapper = commonDataMapper;
        }

        /// <summary>
        /// Get the ModelViews for a Cost Analysis report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="fullWS">The FullWorkspace object.</param>
        /// <param name="reportType">The report type.</param>
        /// <returns>The ModelViews for the report.</returns>
        public ICollection<CostAnalysisReportRMSModelView> GetCostAnalysisReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS, SSRSReportType reportType)
        {
            Collection<CostAnalysisReportRMSModelView> modelViews = new Collection<CostAnalysisReportRMSModelView>();
            string reportTitle = reportType.GetDescription();

            if (fullWS != null && offloadedBoes != null && offloadedBoes.Any())
            {
                ICollection<BoeTaskElementDTO> allTaskElements = SSRSControllerLogic.FilterGoodTasks(offloadedBoes.SelectMany(x => x.TaskElements).ToList());
                IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(fullWS.IsProjectMapWorkspace);

                // Determine the start year for the entire report.
                int reportStartYear = this.GetMinimumResourceStartYear(offloadedBoes, allTaskElements);

                // Retrieve data and build the ModelViews.
                foreach (FullBoe boe in offloadedBoes)
                {
                    foreach (BoeTaskElementDTO task in boe.TaskElements)
                    {
                        string activityId = fullWS.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;
                        string activityName = fullWS.IsProjectMapWorkspace ? boe.Description : task.TaskTitle;

                        foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                        {
                            PerformingOrgDTO perfOrg = fullWS.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
                            ResourceDTO resource = fullWS.ResourcesForWsResourceListId.First(r => r.Id == laborResource.ResourceID);
                            string resourceName = reportType == SSRSReportType.CostAnalysis8YearsFlat ? resource.ResourceName : Utilities.FormatResourceNames(resource.ResourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources), laborResource is SubResourceTypeDto);

                            SikorskyLegacyResourceDTO legacyResource = null;
                            if (allLegacyResources != null && laborResource.LegacyID.HasValue)
                            {
                                legacyResource = allLegacyResources[laborResource.LegacyID.Value];
                            }

                            WbsDTO wbs = fullWS.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                            ClinDTO clin = fullWS.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                            string clinText = fullWS.IsProjectMapWorkspace ? clin?.ClinTitle : clin?.ClinNumber ?? string.Empty;

                            // Get the Labor Spread summaries for each year.
                            Dictionary<int, decimal> laborSpreadYearSummaries = this.GetCostAnalysisReportLaborSpreadSummariesByYear(reportStartYear, laborResource.LaborSpreads);
                            int yearIncrementer = 0;
                            string resourceType = resource.ElementOfCost.GetDescription();
                            if (resource.ElementOfCost == ElementOfCostType.LMLabor)
                            {
                                resourceType = "Labor";
                            }

                            modelViews.Add(new CostAnalysisReportRMSModelView()
                            {
                                ReportTitle = reportTitle,
                                Project = fullWS.WorkspaceName,
                                CLIN = clinText,
                                CostCenter = perfOrg.PerformingOrgName ?? string.Empty,
                                ResourceID = resourceName ?? string.Empty,
                                ResourceDescription = resource.ResourceDesc ?? string.Empty,
                                PerformingOrgDescription = perfOrg.PerformingOrgDesc ?? string.Empty,
                                ResourceUnit = laborResource.SpreadType == SpreadType.Cost ? Constants.COST_ANALYSIS_RESOURCE_UNIT_DIRECT_DOLLARS : Constants.COST_ANALYSIS_RESOURCE_UNIT_HOURS,
                                Category = boe.Category ?? task.Category ?? string.Empty,
                                ActivityID = activityId ?? string.Empty,
                                ClassOfCost = (boe.ClassOfCost == ClassOfCost.None ? task.ClassOfCost : boe.ClassOfCost).GetDescription(),
                                WBS = (wbs == null) ? string.Empty : wbs.WbsNumber,
                                WbsTitle = wbs?.WbsTitle ?? string.Empty,
                                TieredPercentage = laborResource.TieredPercentage.HasValue ? (laborResource.TieredPercentage.Value / 100m).ToString(Constants.PERCENTAGE_FORMATTING) : string.Empty,
                                ActivityName = RTEUtilities.TurnHTMLIntoPlainText(activityName),
                                StartYear = reportStartYear,
                                ResourceType = resourceType ?? string.Empty,
                                SOWTitle = boe.SOWTitle ?? task.SOWTitle ?? string.Empty,
                                Task = RTEUtilities.TurnHTMLIntoPlainText(task.Description),
                                AddDelete = laborResource.AddOrDelete ?? string.Empty,
                                LegacyResource = legacyResource?.LegacyResourceID ?? string.Empty,
                                LegacyResourceName = legacyResource?.LegacyResourceName ?? string.Empty,
                                Year01 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year02 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year03 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year04 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year05 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year06 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year07 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year08 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year09 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year10 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year11 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year12 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year13 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year14 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year15 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year16 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++],
                                Year17 = laborSpreadYearSummaries[reportStartYear + yearIncrementer++]
                            });
                        }
                    }
                }
            }

            return modelViews;
        }

        /// <summary>
        /// Gets model views for both the offload cost by year and offload cost summary reports
        /// </summary>
        /// <param name="offloadedBoes">Collection of BOEs including off-loaded BOEs</param>
        /// <param name="fullWS">Full workspace</param>
        /// <returns>Collection of objects used to populate data in offload cost summary and by year reports</returns>
        public ICollection<OffloadCostByYearReportRMSModelView> GetOffloadCostByYearReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (fullWS == null)
            {
                throw new ArgumentNullException(nameof(fullWS));
            }

            Collection<OffloadCostByYearReportRMSModelView> modelViews = new Collection<OffloadCostByYearReportRMSModelView>();
            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(fullWS.IsProjectMapWorkspace);

            // Retrieve data and build the ModelViews.
            foreach (FullBoe boe in offloadedBoes)
            {
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    string activityId = fullWS.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;

                    foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                    {
                        if (!(laborResource is SubResourceTypeDto))
                        {
                            continue;
                        }

                        SubResourceTypeDto subResourceTypeDto = laborResource as SubResourceTypeDto;

                        Dictionary<int, decimal> inHouseLaborSpreads = GetTotalsByYear(subResourceTypeDto.InHouseLaborSpreads);
                        Dictionary<int, decimal> offloadedHourSpreads = GetTotalsByYear(subResourceTypeDto.OffLoadedHourSpreads);
                        Dictionary<int, decimal> offloadedDollarSpreads = GetTotalsByYear(subResourceTypeDto.LaborSpreads);

                        PerformingOrgDTO perfOrg = fullWS.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
                        ResourceDTO inHouseResourceDto = fullWS.ResourcesUsedInWsBoes.First(r => r.Id == subResourceTypeDto.InHouseResource);
                        WbsDTO wbs = fullWS.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                        ClinDTO clin = fullWS.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                        string clinText = fullWS.IsProjectMapWorkspace ? clin?.ClinTitle : clin?.ClinNumber ?? string.Empty;

                        string resourceName = Utilities.FormatResourceNames(inHouseResourceDto.ResourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources), false);

                        decimal totalHours = inHouseLaborSpreads.Sum(x => x.Value) + offloadedHourSpreads.Sum(x => x.Value);

                        foreach (KeyValuePair<int, decimal> inHouseLaborSpread in inHouseLaborSpreads)
                        {
                            int year = inHouseLaborSpread.Key;
                            decimal offloadHours = offloadedHourSpreads.First(x => x.Key == year).Value;
                            decimal offloadDollars = offloadedDollarSpreads.First(x => x.Key == year).Value;
                            decimal inHouseHours = inHouseLaborSpread.Value;

                            OffloadCostByYearReportRMSModelView modelView = modelViews.FirstOrDefault(m => (string.IsNullOrEmpty(m.Wbs) || (wbs != null && m.Wbs == wbs.WbsNumber)) && m.ActivityId == activityId && m.CostCenter == perfOrg.PerformingOrgName && m.OffloadYear == year.ToString());
                            if (modelView != null)
                            {
                                // add to existing grouping
                                modelView.TotalInHouseHours += inHouseHours;
                                modelView.TotalOffLoadHours += offloadHours;
                                modelView.SumOfOLCost += offloadDollars;
                                modelView.TotalHoursInYear += offloadHours + inHouseHours;
                                modelView.TotalHours += totalHours;

                            }
                            else
                            {
                                // create a new one
                                modelViews.Add(new OffloadCostByYearReportRMSModelView()
                                {
                                    ActivityId = activityId ?? string.Empty,
                                    CostCenter = perfOrg.PerformingOrgName ?? string.Empty,
                                    Resource = resourceName ?? string.Empty,
                                    Wbs = wbs?.WbsNumber ?? string.Empty,
                                    Clin = clinText,
                                    TotalInHouseHours = inHouseHours,
                                    TotalOffLoadHours = offloadHours,
                                    SumOfOLCost = offloadDollars,
                                    OffloadYear = year.ToString(),
                                    TotalHoursInYear = offloadHours + inHouseHours,
                                    TotalHours = totalHours
                                });
                            }
                        }
                    }
                }
            }

            // Calculate OL % and OL Rate on the fly
            foreach (OffloadCostByYearReportRMSModelView modelView in modelViews)
            {
                if (modelView.TotalHoursInYear != 0m)
                {
                    modelView.OffLoadPercent = modelView.TotalOffLoadHours / modelView.TotalHoursInYear;
                }

                if (modelView.TotalOffLoadHours != 0m)
                {
                    modelView.HourlyOffLoadRate = modelView.SumOfOLCost / modelView.TotalOffLoadHours;
                }
            }

            return modelViews;
        }

        /// <summary>
        /// Gets model views for both the offload cost by year and offload cost summary reports
        /// </summary>
        /// <param name="offloadedBoes">Collection of BOEs including off-loaded BOEs</param>
        /// <param name="fullWS">Full workspace</param>
        /// <returns>Collection of objects used to populate data in offload cost summary and by year reports</returns>
        public ICollection<OffloadCostByYearReportRMSModelView> GetOffloadDetailedReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (fullWS == null)
            {
                throw new ArgumentNullException(nameof(fullWS));
            }

            ICollection<OffloadRatesDTO> offloadRates = this.offloadRatesLoader.GetByWorkspaceId(fullWS.Id);
            Collection<OffloadCostByYearReportRMSModelView> modelViews = new Collection<OffloadCostByYearReportRMSModelView>();
            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(fullWS.IsProjectMapWorkspace);

            // Retrieve data and build the ModelViews.
            foreach (FullBoe boe in offloadedBoes)
            {
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    string activityId = fullWS.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;

                    foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                    {
                        if (!(laborResource is SubResourceTypeDto))
                        {
                            continue;
                        }

                        SubResourceTypeDto subResourceTypeDto = laborResource as SubResourceTypeDto;

                        Dictionary<int, decimal> inHouseLaborSpreads = GetTotalsByYear(subResourceTypeDto.InHouseLaborSpreads);
                        Dictionary<int, decimal> offloadedHourSpreads = GetTotalsByYear(subResourceTypeDto.OffLoadedHourSpreads);
                        Dictionary<int, decimal> offloadedDollarSpreads = GetTotalsByYear(subResourceTypeDto.LaborSpreads);

                        PerformingOrgDTO perfOrg = fullWS.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
                        ResourceDTO inHouseResourceDto = fullWS.ResourcesUsedInWsBoes.First(r => r.Id == subResourceTypeDto.InHouseResource);
                        WbsDTO wbs = fullWS.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                        ClinDTO clin = fullWS.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                        string clinText = fullWS.IsProjectMapWorkspace ? clin?.ClinTitle : clin?.ClinNumber ?? string.Empty;
                        string resourceName = inHouseResourceDto.ResourceName;
                        string legacyName = this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources);

                        decimal totalHours = inHouseLaborSpreads.Sum(x => x.Value) + offloadedHourSpreads.Sum(x => x.Value);

                        foreach (KeyValuePair<int, decimal> inHouseLaborSpread in inHouseLaborSpreads)
                        {
                            int year = inHouseLaborSpread.Key;
                            decimal offloadHours = offloadedHourSpreads.First(x => x.Key == year).Value;
                            decimal offloadDollars = offloadedDollarSpreads.First(x => x.Key == year).Value;
                            decimal inHouseHours = inHouseLaborSpread.Value;

                            modelViews.Add(new OffloadCostByYearReportRMSModelView()
                            {
                                ActivityId = activityId ?? string.Empty,
                                CostCenter = perfOrg.PerformingOrgName ?? string.Empty,
                                OffloadedResource = legacyName ?? string.Empty,
                                Resource = resourceName ?? string.Empty,
                                Wbs = wbs?.WbsNumber ?? string.Empty,
                                Clin = clinText,
                                TotalInHouseHours = inHouseHours,
                                TotalOffLoadHours = offloadHours,
                                SumOfOLCost = offloadDollars,
                                OffloadYear = year.ToString(),
                                TotalHoursInYear = offloadHours + inHouseHours,
                                TotalHours = totalHours,
                                OffLoadPercent = offloadRates.First(x => x.Resource == inHouseResourceDto.ResourceName && x.PerformingOrg == perfOrg.PerformingOrgName && x.Year == year).Percent,
                                HourlyOffLoadRate = offloadRates.First(x => x.Resource == inHouseResourceDto.ResourceName && x.PerformingOrg == perfOrg.PerformingOrgName && x.Year == year).HourlyRate
                            });
                        }
                    }
                }
            }

            return modelViews;
        }


        /// <summary>
        /// Gets model views for the offload cost summary report by consolidating the costs by year.
        /// </summary>
        /// <param name="offloadCostByYearReportRMSModelViews">model views broken out by year.</param>
        /// <returns>consolidated model views</returns>
        public ICollection<OffloadCostByYearReportRMSModelView> ConsolidateOffloadCostByYear(ICollection<OffloadCostByYearReportRMSModelView> offloadCostByYearReportRMSModelViews)
        {
            return offloadCostByYearReportRMSModelViews
                .GroupBy(x => new { x.Wbs, x.ActivityId, x.Resource, x.CostCenter, x.OffloadedResource, x.OffLoadPercent, x.TotalHours })
                .Select(x => new OffloadCostByYearReportRMSModelView
                {
                    Wbs = x.Key.Wbs,
                    ActivityId = x.Key.ActivityId,
                    CostCenter = x.Key.CostCenter,
                    OffloadedResource = x.Key.OffloadedResource,
                    Resource = x.Key.Resource,
                    OffLoadPercent = x.Key.OffLoadPercent,
                    SumOfOLCost = x.Sum(xs => xs.SumOfOLCost),
                    TotalHours = x.Key.TotalHours,
                    TotalOffLoadHours = x.Sum(xs => xs.TotalOffLoadHours),
                    TotalInHouseHours = x.Sum(xs => xs.TotalInHouseHours)
                }).ToCollection();
        }

        /// <summary>
        /// Aggregates labor spreads by year.
        /// </summary>
        /// <param name="spreadsByMonth">labor spreads by month</param>
        /// <returns>Labor spreads by year.</returns>
        private static Dictionary<int, decimal> GetTotalsByYear(Collection<ResourceSpreadDto> spreadsByMonth)
        {
            Dictionary<int, decimal> spreadsByYear = new Dictionary<int, decimal>();
            foreach (ResourceSpreadDto monthSpread in spreadsByMonth)
            {
                if (spreadsByYear.ContainsKey(monthSpread.LaborSpreadDate.Year))
                {
                    spreadsByYear[monthSpread.LaborSpreadDate.Year] += monthSpread.LaborSpreadValue;
                }
                else
                {
                    spreadsByYear[monthSpread.LaborSpreadDate.Year] = monthSpread.LaborSpreadValue;
                }
            }

            return spreadsByYear;
        }

        /// <summary>
        /// Gets the offloaded BOEs from a FullWorkspace, ordering them by the standard Project Map sort.
        /// </summary>
        /// <param name="fullWS">The FullWorkspace.</param>
        /// <returns>The offloaded BOEs, ordered by standard Project Map sort.</returns>
        public ICollection<FullBoe> GetOffloadBOEsFromFullWorkspace(FullWorkspace fullWS)
        {
            if (fullWS == null)
            {
                throw new ArgumentNullException(nameof(fullWS));
            }

            OffloadLaborRates offloader = new OffloadLaborRates();
            // Set the task elements before Filtering good boes
            SetTaskElements(fullWS);

            ICollection<FullBoe> filteredBoes = FilterGoodBOEs(fullWS.Boes);
            if (fullWS.IsProjectMapWorkspace || fullWS.ProjectMapType == ProjectMapType.StandardWithOffload)
            {
                filteredBoes = offloader.OffloadWorkspace(filteredBoes, fullWS).Boes.ToList();
            }

            return ProjectMapSorter.OrderBoes(filteredBoes, fullWS);
        }

        /// <summary>
        /// Sets the task elements for each BOE
        /// </summary>
        /// <param name="fullWS">The full workspace</param>
        private void SetTaskElements(FullWorkspace fullWS)
        {
            foreach (FullBoe boe in fullWS.Boes)
            {
                boe.SetTaskElements(fullWS.TaskElements.Where(t => t.BoeID == boe.Id).ToList());
            }
        }

        /// <summary>
        /// Filters the good boes out of the list.
        /// </summary>
        /// <param name="boes">The boes.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ICollection<FullBoe> FilterGoodBOEs(IEnumerable<FullBoe> boes)
        {
            return boes.Where(b => b.TaskElements.Any()).ToList();
        }

        public static ICollection<BoeTaskElementDTO> FilterGoodTasks(ICollection<BoeTaskElementDTO> tasks)
        {
            return tasks.Where(t => t.taskElementLabors.Any()).ToList();
        }

        /// <summary>
        /// Gets the minimum start year that exists for any LaborSpread across all LaborResources aned across all the offloaded BOEs .
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="allTaskElements">All of the TaskElements for the offloaded BOEs.</param>
        /// <returns>The minimum start year for all LaborSpreads.</returns>
        internal int GetMinimumResourceStartYear(ICollection<FullBoe> offloadedBoes, ICollection<BoeTaskElementDTO> allTaskElements)
        {
            int minimumResourceStartYear = int.MaxValue;

            if (offloadedBoes != null && offloadedBoes.Any())
            {
                foreach (FullBoe boe in offloadedBoes)
                {
                    foreach (BoeTaskElementDTO task in allTaskElements.Where(x => x.BoeID == boe.Id))
                    {
                        // Find the minimum start year for any individual LaborSpread.
                        foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                        {
                            int startYearForResource = laborResource.LaborSpreads.OrderBy(ls => ls.LaborSpreadDate).First().LaborSpreadDate.Year;
                            minimumResourceStartYear = startYearForResource < minimumResourceStartYear ? startYearForResource : minimumResourceStartYear;
                        }
                    }
                }
            }

            return minimumResourceStartYear;
        }

        /// <summary>
        /// Gets the summary of LaborSpreads for each year of the report.
        /// </summary>
        /// <param name="startYear">The start year for the report.</param>
        /// <param name="laborSpreads">The LaborSpreads to be broken out and summed by year.</param>
        /// <returns>A dictionary of years with the corresponding LaborSpread summary for each year.</returns>
        internal Dictionary<int, decimal> GetCostAnalysisReportLaborSpreadSummariesByYear(int startYear, Collection<ResourceSpreadDto> laborSpreads)
        {
            Dictionary<int, decimal> spreadYearSummaries = new Dictionary<int, decimal>();

            // Initialize the dictionary with 0 values for each year, up to the maximum number of years needed for any of the Cost Analysis reports.
            //  Years beyond the range of the individual report will be ignored at report generation time.
            const int maxYears = 17;
            for (int i = 0; i < maxYears; i++)
            {
                spreadYearSummaries.Add(startYear + i, 0);
            }

            // Sum the LaborSpreadValues for each year and add them to the dictionary in the appropriate entry.
            foreach (ResourceSpreadDto laborSpread in laborSpreads)
            {
                if (spreadYearSummaries.ContainsKey(laborSpread.LaborSpreadDate.Year))
                {
                    spreadYearSummaries[laborSpread.LaborSpreadDate.Year] += laborSpread.LaborSpreadValue;
                }
            }

            return spreadYearSummaries;
        }

        /// <summary>
        /// Gets the ModelViews for a Staffing Curves report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="workspace">The FullWorkspace object.</param>
        /// <returns>The ModelViews for the report.</returns>
        public ICollection<StaffingCurvesReportModelView> GetStaffingCurvesReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<BoeTaskElementDTO> allTaskElements = offloadedBoes.SelectMany(x => x.TaskElements).ToList();
            List<StaffingCurvesReportModelView> staffingCurves = new List<StaffingCurvesReportModelView>();

            // Add missing dates (jan thru starting month of first year, end month to december of last year)
            DateTime minMonth = allTaskElements.SelectMany(t => t.taskElementLabors).Min(p => p.StartDateValue);
            DateTime maxMonth = allTaskElements.SelectMany(t => t.taskElementLabors).Max(p => p.EndDateValue);

            minMonth = new DateTime(minMonth.Year, 1, 1).Normalize();
            maxMonth = new DateTime(maxMonth.Year, 12, 1).Normalize();

            Dictionary<string, Dictionary<DateTime, decimal>> curvesByPerfOrgAndMonth = new Dictionary<string, Dictionary<DateTime, decimal>>();
            foreach (BoeTaskElementDTO task in allTaskElements)
            {
                foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                {
                    if (laborResource.SpreadType == SpreadType.Hours)
                    {
                        PerformingOrgDTO perfOrg = workspace.PerformingOrgsUsedInBoes.First(p => p.Id == laborResource.PerformingOrgID);
                        Dictionary<DateTime, decimal> curvesByMonth;
                        if (!curvesByPerfOrgAndMonth.TryGetValue(perfOrg.PerformingOrgDesc, out curvesByMonth))
                        {
                            curvesByMonth = CreateDefaultMonthDictionary(minMonth, maxMonth);
                            curvesByPerfOrgAndMonth.Add(perfOrg.PerformingOrgDesc, curvesByMonth);
                        }

                        foreach (ResourceSpreadDto spread in laborResource.LaborSpreads)
                        {
                            // for each labor spread month, add it to the dictionary keyed by the month
                            if (curvesByMonth.ContainsKey(spread.LaborSpreadDate))
                            {
                                curvesByMonth[spread.LaborSpreadDate] += spread.LaborSpreadValue;
                            }
                            else
                            {
                                throw new ArgumentException($"The date {spread.LaborSpreadDate} is not inside the min/max dates set for the resources {minMonth} - {maxMonth}");
                            }
                        }
                    }
                }
            }

            // compress the Dictionaries down to ModelViews
            foreach (KeyValuePair<string, Dictionary<DateTime, decimal>> perfOrgs in curvesByPerfOrgAndMonth)
            {
                foreach (KeyValuePair<DateTime, decimal> monthValues in perfOrgs.Value)
                {
                    staffingCurves.Add(new StaffingCurvesReportModelView
                    {
                        Date = new DateTime(monthValues.Key.Year, monthValues.Key.Month, 1), // Adjust to first of the month so that SSRS shows the dates correctly
                        FTE = Utilities.AdjustPrecision(monthValues.Value / 157m, 2),  // 157 hours per month equals one Full Time Employee
                        PerfOrg = perfOrgs.Key
                    });
                }
            }

            return staffingCurves;
        }

        /// <summary>
        /// Gets the Boe Summary report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>The ModelViews for the report.</returns>
        public ICollection<BOESummaryReportModelView> GetBOESummaryReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            
            List<BOESummaryReportModelView> boeSummaryReport = new List<BOESummaryReportModelView>();

            if (offloadedBoes.Any())
            {
                string projectName = workspace.WorkspaceName;
                int reportStartYear = offloadedBoes.Select(b => b.StartDate).Min().Year;
                IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(workspace.IsProjectMapWorkspace);

                // Label constants for the header columns.
                const string taskStartYearLabel = "Task Start CY";

                foreach (FullBoe boe in offloadedBoes)
                {
                    decimal salaryHoursTotal = boe.TaskElements.SelectMany(t => t.taskElementLabors).Where(l => l.SpreadType == SpreadType.Hours).Sum(le => le.ValueSpread ?? 0m);
                    decimal costDollarsTotal = boe.TaskElements.SelectMany(t => t.taskElementLabors).Where(l => l.SpreadType == SpreadType.Cost).Sum(le => le.ValueSpread ?? 0m);

                    int taskStartYear = boe.TaskElements.Select(t => t.StartDate.Value).Min().Year;

                    // Resource names
                    List<string> boeColumn1Data = new List<string>();

                    // Performing Org Descriptions
                    List<string> boeColumn2Data = new List<string>();

                    // Start dates
                    List<string> boeColumn3Data = new List<string>();

                    // End Dates
                    List<string> boeColumn4Data = new List<string>();

                    Dictionary<int, decimal> hoursByYear = SetupInitialSpreadDictionary(reportStartYear);
                    Dictionary<int, decimal> costByYear = SetupInitialSpreadDictionary(reportStartYear);

                    foreach (BoeTaskElementDTO task in boe.TaskElements)
                    {
                        // Add Labor Resource header data
                        foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                        {
                            string resourceName = Utilities.FormatResourceNames(workspace.ResourcesForWsResourceListId.First(r => r.Id == laborResource.ResourceID).ResourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources), laborResource is SubResourceTypeDto);
                            boeColumn1Data.Add(resourceName);
                            boeColumn2Data.Add(workspace.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID).PerformingOrgDesc);
                            boeColumn3Data.Add(laborResource.StartDateValue.ToMonthString());
                            boeColumn4Data.Add(laborResource.EndDateValue.ToMonthString());
                            foreach (ResourceSpreadDto spread in laborResource.LaborSpreads)
                            {
                                if (laborResource.SpreadType == SpreadType.Hours)
                                {
                                    AddToSpreadDictionary(hoursByYear, spread);
                                }
                                else if (laborResource.SpreadType == SpreadType.Cost)
                                {
                                    AddToSpreadDictionary(costByYear, spread);
                                }
                            }
                        }
                    }

                    ClinDTO clin = workspace.Clins.FirstOrDefault(x => x.Id == boe.CLINID);

                    // Add Task and Clin data
                    boeColumn1Data.Add(taskStartYearLabel);
                    boeColumn2Data.Add(taskStartYear.ToString());
                    boeColumn1Data.Add(CLIN_LABEL);
                    boeColumn2Data.Add(clin?.ClinNumber ?? string.Empty);

                    // Build BOE Column Headers.
                    string boeHeaderHTMLColumn1 = GetBOESummaryHeaderHTMLColumn(boeColumn1Data);
                    string boeHeaderHTMLColumn2 = GetBOESummaryHeaderHTMLColumn(boeColumn2Data);
                    string boeHeaderHTMLColumn3 = GetBOESummaryHeaderHTMLColumn(boeColumn3Data);
                    string boeHeaderHTMLColumn4 = GetBOESummaryHeaderHTMLColumn(boeColumn4Data);
                    string taskDescription = boe.TaskElements.FirstOrDefault()?.Description ?? string.Empty;

                    // Build BOE Labor Resource ModelViews
                    boeSummaryReport.Add(new BOESummaryReportModelView()
                    {
                        Project = projectName,
                        BoeID = boe.Id.ToString(),
                        HeaderHTMLColumn1 = boeHeaderHTMLColumn1,
                        HeaderHTMLColumn2 = boeHeaderHTMLColumn2,
                        HeaderHTMLColumn3 = boeHeaderHTMLColumn3,
                        HeaderHTMLColumn4 = boeHeaderHTMLColumn4,
                        ReportStartYear = reportStartYear,
// ToDo: Dusan -> This is an issue.. cannot use task, since it's summing things up.. yuck 2x
                        ActivityID = boe.Title ?? string.Empty,
                        ActivityName = RTEUtilities.TurnHTMLIntoPlainText(boe.Description),

                        TaskDescription = RTEUtilities.TurnHTMLIntoPlainText(taskDescription),
                        SalaryHoursTotal = salaryHoursTotal,
                        CostDollarsTotal = costDollarsTotal,
                        ResourceUnit = Constants.BOE_SUMMARY_RESOURCE_UNIT_HOURS,
                        Year01 = hoursByYear[reportStartYear],
                        Year02 = hoursByYear[reportStartYear + 1],
                        Year03 = hoursByYear[reportStartYear + 2],
                        Year04 = hoursByYear[reportStartYear + 3],
                        Year05 = hoursByYear[reportStartYear + 4],
                        Year06 = hoursByYear[reportStartYear + 5],
                        Year07 = hoursByYear[reportStartYear + 6],
                        Year08 = hoursByYear[reportStartYear + 7],
                        Year09 = hoursByYear[reportStartYear + 8],
                        Year10 = hoursByYear[reportStartYear + 9],
                        Year11 = hoursByYear[reportStartYear + 10],
                        Year12 = hoursByYear[reportStartYear + 11],
                        Year13 = hoursByYear[reportStartYear + 12],
                        Year14 = hoursByYear[reportStartYear + 13],
                        Year15 = hoursByYear[reportStartYear + 14],
                        Year16 = hoursByYear[reportStartYear + 15],
                        Year17 = hoursByYear[reportStartYear + 16]
                    });
                    
                    boeSummaryReport.Add(new BOESummaryReportModelView()
                    {
                        Project = projectName,
                        BoeID = boe.Id.ToString(),
                        HeaderHTMLColumn1 = boeHeaderHTMLColumn1,
                        HeaderHTMLColumn2 = boeHeaderHTMLColumn2,
                        HeaderHTMLColumn3 = boeHeaderHTMLColumn3,
                        HeaderHTMLColumn4 = boeHeaderHTMLColumn4,
                        ReportStartYear = reportStartYear,
// ToDo: Dusan -> PROBLEM 2x
                        ActivityID = boe.Title ?? string.Empty,
                        ActivityName = RTEUtilities.TurnHTMLIntoPlainText(boe.Description),

                        TaskDescription = RTEUtilities.TurnHTMLIntoPlainText(taskDescription),
                        SalaryHoursTotal = salaryHoursTotal,
                        CostDollarsTotal = costDollarsTotal,
                        ResourceUnit = Constants.BOE_SUMMARY_RESOURCE_UNIT_COST_DOLLARS,
                        Year01 = costByYear[reportStartYear],
                        Year02 = costByYear[reportStartYear + 1],
                        Year03 = costByYear[reportStartYear + 2],
                        Year04 = costByYear[reportStartYear + 3],
                        Year05 = costByYear[reportStartYear + 4],
                        Year06 = costByYear[reportStartYear + 5],
                        Year07 = costByYear[reportStartYear + 6],
                        Year08 = costByYear[reportStartYear + 7],
                        Year09 = costByYear[reportStartYear + 8],
                        Year10 = costByYear[reportStartYear + 9],
                        Year11 = costByYear[reportStartYear + 10],
                        Year12 = costByYear[reportStartYear + 11],
                        Year13 = costByYear[reportStartYear + 12],
                        Year14 = costByYear[reportStartYear + 13],
                        Year15 = costByYear[reportStartYear + 14],
                        Year16 = costByYear[reportStartYear + 15],
                        Year17 = costByYear[reportStartYear + 16]
                    });
                }
            }

            return boeSummaryReport;
        }

        /// <summary>
        /// Sets up the initial spread dictionary.
        /// </summary>
        /// <param name="taskStartYear">The task start year.</param>
        /// <returns>Spread dictionary with zeroes for all 17 years.</returns>
        private static Dictionary<int, decimal> SetupInitialSpreadDictionary(int taskStartYear)
        {
            Dictionary<int, decimal> spreads = new Dictionary<int, decimal>();
            for (int i = 0; i < 17; i++)
            {
                spreads[taskStartYear + i] = 0m;
            }

            return spreads;
        }

        /// <summary>
        /// Adds the spread's value to the spread dictionary.
        /// </summary>
        /// <param name="valueByYear">The value by year.</param>
        /// <param name="spread">The spread.</param>
        private static void AddToSpreadDictionary(Dictionary<int, decimal> valueByYear, ResourceSpreadDto spread)
        {
            if (valueByYear.ContainsKey(spread.LaborSpreadDate.Year))
            {
                valueByYear[spread.LaborSpreadDate.Year] += spread.LaborSpreadValue;
            }
            else
            {
                valueByYear[spread.LaborSpreadDate.Year] = spread.LaborSpreadValue;
            }
        }

        /// <summary>
        /// This builds a column of data for the header based on a string array.  Each item in the array will appear on its own line.
        /// </summary>
        /// <param name="itemsToAdd">String array of items to be added to the header column.</param>
        /// <returns>The HTML required to display the items properly in the column on the report.</returns>
        private static string GetBOESummaryHeaderHTMLColumn(ICollection<string> itemsToAdd)
        {
            StringBuilder html = new StringBuilder();

            // Add items, if they exist.
            foreach(string itemToAdd in itemsToAdd)
            {
                html.Append("<div>");
                html.Append(itemToAdd);
                html.Append("</div>");
            }

            return html.ToString();
        }

        /// <summary>
        /// Creates the default month dictionary.
        /// </summary>
        /// <param name="minMonth">The minimum month.</param>
        /// <param name="maxMonth">The maximum month.</param>
        /// <returns>A dictionary of all the months.</returns>
        private static Dictionary<DateTime, decimal> CreateDefaultMonthDictionary(DateTime minMonth, DateTime maxMonth)
        {
            Dictionary<DateTime, decimal> months = new Dictionary<DateTime, decimal>();

            for (DateTime currentMonth = minMonth; currentMonth <= maxMonth; currentMonth = currentMonth.AddMonths(1))
            {
                months.Add(currentMonth, 0);
            }

            return months;
        }

        /// <summary>
        /// Gets the ModelViews for Pre Vs Post Offload Totals (Diagnostics Report)
        /// </summary>
        /// <param name="workspace">Workspace for the report</param>
        /// <returns>The ModelViews for the report</returns>
        public ICollection<PreVsPostOffloadTotalsModelView> GetPreVsPostOffloadTotalsModelViews(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            PreVsPostOffloadTotalsModelView modelView = new PreVsPostOffloadTotalsModelView();

            // input total
            decimal hoursInputTotal = workspace.TaskElements.SelectMany(t => t.taskElementLabors)
                .Where(l => l.SpreadType == SpreadType.Hours).Select(tl => tl.ValueSpread ?? 0).Sum();
            decimal costInputTotal = workspace.TaskElements.SelectMany(t => t.taskElementLabors)
                .Where(l => l.SpreadType == SpreadType.Cost).Select(tl => tl.ValueSpread ?? 0).Sum();

            modelView.HoursInputTotal = hoursInputTotal.ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(workspace.DecimalPrecision));
            modelView.CostInputTotal = "$" + costInputTotal.ToString(Utilities.CostPrecisionFormattingString(workspace.CostDecimalPrecision));
            if (workspace.IsProjectMapWorkspace || workspace.ProjectMapType == ProjectMapType.StandardWithOffload)
            {

                // offload total
                OffloadLaborRates offloader = new OffloadLaborRates();
                OffloadLaborRatesResults offloadResults = offloader.OffloadWorkspace(workspace.Boes.ToList(), workspace);

                modelView.HoursOffloadTotal = offloadResults.TotalHoursOffloaded.ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(workspace.DecimalPrecision));
                modelView.CostOffloadTotal = "$" + offloadResults.TotalCostOffloaded.ToString(Utilities.CostPrecisionFormattingString(workspace.CostDecimalPrecision));

                // output total
                ICollection<FullBoe> offloadBoes = offloadResults.Boes.ToCollection();

                decimal hoursOutputTotal = offloadBoes.SelectMany(b => b.TaskElements).SelectMany(t => t.taskElementLabors)
                    .Where(l => l.SpreadType == SpreadType.Hours).Select(tl => tl.ValueSpread ?? 0).Sum();
                decimal costOutputTotal = offloadBoes.SelectMany(b => b.TaskElements).SelectMany(t => t.taskElementLabors)
                    .Where(l => l.SpreadType == SpreadType.Cost).Select(tl => tl.ValueSpread ?? 0).Sum();

                modelView.HoursOutputTotal = hoursOutputTotal.ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(workspace.DecimalPrecision));
                modelView.CostOutputTotal = "$" + costOutputTotal.ToString(Utilities.CostPrecisionFormattingString(workspace.CostDecimalPrecision));

                // variance
                modelView.HoursVariance = (hoursInputTotal - offloadResults.TotalHoursOffloaded - hoursOutputTotal).ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(workspace.DecimalPrecision));
                modelView.CostVariance = "$" + (costInputTotal + offloadResults.TotalCostOffloaded - costOutputTotal).ToString(Utilities.CostPrecisionFormattingString(workspace.CostDecimalPrecision));
            }
            else
            {
                modelView.HoursOffloadTotal = "N/A";
                modelView.CostOffloadTotal = "N/A";

                modelView.HoursOutputTotal = "N/A";
                modelView.CostOutputTotal = "N/A";

                // variance
                modelView.HoursVariance = "N/A";
                modelView.CostVariance = "N/A";
            }

            ICollection<PreVsPostOffloadTotalsModelView> preVsPostOffloadTotals = new Collection<PreVsPostOffloadTotalsModelView>()
            {
                modelView
            };

            return preVsPostOffloadTotals;
        }

        /// <summary>
        /// Get the Model Views for the RAM (Responsibility Assignment Matrix)
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="workspace">Workspace for the report</param>
        /// <returns>The ModelViews for the report</returns>
        public ICollection<RAMReportModelView> GetRamReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            
            ICollection<RAMReportModelView> ramReport = new Collection<RAMReportModelView>();

            HashSet<PerformingOrgDTO> performingOrgs = new HashSet<PerformingOrgDTO>(workspace.PerformingOrgsUsedInBoes);
            HashSet<ResourceDTO> resources = new HashSet<ResourceDTO>(workspace.ResourcesUsedInWsBoes);
            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(workspace.IsProjectMapWorkspace);

            foreach (FullBoe boe in offloadedBoes)
            {
                ClinDTO clin = workspace.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                string clinText = workspace.IsProjectMapWorkspace ? clin?.ClinTitle : clin?.ClinNumber ?? string.Empty;
                WbsDTO wbs = workspace.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    // Project map -> activity is fed from BOE. Standard -> fed from task. (facepalm)
                    string activityId = workspace.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;
                    string activityName = workspace.IsProjectMapWorkspace ? boe.Description : task.TaskTitle;

                    var groupedResourceTypes = task.taskElementLabors.GroupBy(x => new { x.ResourceID, x.LegacyID, x.PerformingOrgID }).ToCollection();
                    foreach (var groupedResourceType in groupedResourceTypes)
                    {
                        // if we are missing the resource, we'll grab it by the id.. this is likely going to trip for the offload resource
                        // we'll add it into the hashset, to make sure we don't have to keep pulling the same thing... :)
                        if (!resources.Any(x => x.Id == groupedResourceType.First().ResourceID))
                        {
                            resources.Add(this.resourceLoader.GetById(groupedResourceType.First().ResourceID.Value));
                        }

                        string resourceName = Utilities.FormatResourceNames(resources.First(x => x.Id == groupedResourceType.First().ResourceID).ResourceName, 
                            this.commonDataMapper.GetSikorskyLegacyResourceID(groupedResourceType.First().LegacyID, allLegacyResources), groupedResourceType.First() is SubResourceTypeDto);

                        string costCenterName = performingOrgs.First(x => x.Id == groupedResourceType.First().PerformingOrgID).PerformingOrgName;

                        decimal resourceValue = groupedResourceType.Sum(x => x.ValueSpread ?? 0);
                        string resourceValueString = groupedResourceType.First().SpreadType == SpreadType.Cost
                            ? "$" + resourceValue.ToString(
                                  Utilities.CostPrecisionFormattingString(workspace.CostDecimalPrecision))
                            : resourceValue.ToString(Utilities.PrecisionFormattingString(workspace.DecimalPrecision));

                        ramReport.Add(new RAMReportModelView()
                        {
                            Clin = clinText,
                            Wbs = wbs?.WbsNumber ?? string.Empty,
                            ActivityId = activityId ?? string.Empty,
                            ActivityName = RTEUtilities.TurnHTMLIntoPlainText(activityName),
                            ResourceCostCenter = resourceName + ", " + costCenterName,
                            Value = resourceValueString
                        });
                    }
                }
            }
            
            return ramReport;
        }

        /// <summary>
        /// Get the ModelViews for the Workbench Offload report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="workspace">The FullWorkspace object.</param>
        /// <returns>The ModelViews for the report.</returns>
        public ICollection<WorkbenchOffloadModelView> GetWorkbenchOffloadModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace workspace)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            Dictionary<string, WorkbenchOffloadModelView> modelViews = new Dictionary<string, WorkbenchOffloadModelView>();
            foreach (FullBoe boe in offloadedBoes)
            {
                ClinDTO clin = workspace.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                string clinText = workspace.IsProjectMapWorkspace ? clin?.ClinTitle : clin?.ClinNumber ?? string.Empty;
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    foreach (ResourceTypeDto laborType in task.taskElementLabors)
                    {
                        if (laborType is SubResourceTypeDto)
                        {
                            foreach (ResourceSpreadDto spread in laborType.LaborSpreads)
                            {
                                string key = clinText + "#@$" + spread.LaborSpreadDate.Year.ToString();
                                if (!modelViews.TryGetValue(key, out WorkbenchOffloadModelView modelView))
                                {
                                    modelView = new WorkbenchOffloadModelView
                                    {
                                        Clin = clinText,
                                        Year = spread.LaborSpreadDate.Year.ToString()
                                    };

                                    modelViews.Add(key, modelView);
                                }

                                modelView.Value += spread.LaborSpreadValue;
                            }
                        }
                    }
                }
            }

            return modelViews.Values.ToList();
        }

        /// <summary>
        /// Gets the Engineering and Non-Engineering Project WBS Cost Summary By CLIN report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="fullWS">The workspace.</param>
        /// <returns>
        /// The ModelViews for the report.
        /// </returns>
        public ICollection<ProjectWbsCostSummaryByClinModelView> GetProjectCLINCostSummaryModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (fullWS == null)
            {
                throw new ArgumentNullException(nameof(fullWS));
            }

            ICollection<ProjectWbsCostSummaryByClinModelView> retProjectWbsCostSummaryByClin = new Collection<ProjectWbsCostSummaryByClinModelView>();

            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(fullWS.IsProjectMapWorkspace);

            // Retrieve data and build the ModelViews.
            foreach (FullBoe boe in offloadedBoes)
            {
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    // Project map -> activity is fed from BOE. Standard -> fed from task. (facepalm)
                    string activityId = fullWS.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;
                    string activityName = fullWS.IsProjectMapWorkspace ? boe.Description : task.TaskTitle;

                    foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                    {
                        string resourceName = this.GetResourceName(laborResource, fullWS.ResourcesUsedInWsBoes);
                        resourceName = Utilities.FormatResourceNames(resourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources), laborResource is SubResourceTypeDto);
                        PerformingOrgDTO perfOrg = fullWS.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
                        WbsDTO wbs = fullWS.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                        ClinDTO clin = fullWS.Clins.FirstOrDefault(x => x.Id == boe.CLINID);

                        retProjectWbsCostSummaryByClin.Add(new ProjectWbsCostSummaryByClinModelView
                        {
                            WorkspaceName = fullWS.WorkspaceName,
                            Clin = clin?.ClinNumber ?? string.Empty,
                            ActivityID = activityId ?? string.Empty,
                            ActivityName = RTEUtilities.TurnHTMLIntoPlainText(activityName),
                            CostCenter = perfOrg.PerformingOrgName ?? string.Empty,
                            Resource = resourceName ?? string.Empty,
                            StartDate = laborResource.StartDate,
                            EndDate = laborResource.EndDate,
                            LaborHrs = laborResource.SpreadType == SpreadType.Hours ? laborResource.ValueSpread : 0,
                            MatlCost = laborResource.SpreadType == SpreadType.Cost ? laborResource.ValueSpread : 0,
                            Wbs = wbs?.WbsNumber ?? string.Empty,
                            WbsTitle = wbs?.WbsTitle ?? string.Empty
                        });
                    }
                }
            }

            return retProjectWbsCostSummaryByClin;
        }

        /// <summary>
        /// Because FullWorkspace.ResourcesUsedInWsBoes does not contain offloaded resources
        /// this method checks whether a resource is an offloaded resource and if so extracts
        /// the resource name from the SubResourceTypeDto
        /// </summary>
        /// <param name="laborResource">ResourceTypeDto to get the resource name for</param>
        /// <param name="resourcesUsedInWsBoes">Collection of non-offloaded resources for the workspace</param>
        /// <returns>Returns a sub resource type's resource name.</returns>
        private string GetResourceName(ResourceTypeDto laborResource, IReadOnlyCollection<ResourceDTO> resourcesUsedInWsBoes)
        {
            SubResourceTypeDto subResourceTypeDto = laborResource as SubResourceTypeDto;
            return subResourceTypeDto != null ? subResourceTypeDto.SubResourceName : resourcesUsedInWsBoes.First(r => r.Id == laborResource.ResourceID).ResourceName;
        }

        /// <summary>
        /// Gets the summary report RMS.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="ws">The workspace.</param>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>A list of model views for the summary report.</returns>
        public ICollection<SummaryReportRMSModelView> GetSummaryReportRMSModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace ws, SSRSReportType reportType)
        {
            if (offloadedBoes == null)
            {
                throw new ArgumentNullException(nameof(offloadedBoes));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (reportType != SSRSReportType.ProjectCategoryCLINCostSummary && reportType != SSRSReportType.ProjectCLINCategoryCostSummary)
            {
                throw new ArgumentException($"Invalid report type passed into Get Summary Report: {reportType.ToDescription()}");
            }

            List<SummaryReportRMSModelView> summaryReport = new List<SummaryReportRMSModelView>();
            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(ws.IsProjectMapWorkspace);

            string projectName = $"{ws.WorkspaceName} - {ws.Id}";
            string majorGroupingLabel = reportType == SSRSReportType.ProjectCLINCategoryCostSummary ? CLIN_LABEL : CATEGORY_LABEL;
            string minorGroupingLabel = reportType == SSRSReportType.ProjectCLINCategoryCostSummary ? CATEGORY_LABEL : CLIN_LABEL;
            string reportTitle = reportType == SSRSReportType.ProjectCLINCategoryCostSummary ? "Project CLIN Category Cost Summary" : "Project Category CLIN Cost Summary";

            foreach (FullBoe boe in offloadedBoes)
            {
                string majorGroupingText, minorGroupingText;
                ClinDTO clin = ws.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                // Assuming there are no multi boes
                string clinText = ws.IsProjectMapWorkspace ? clin?.ClinTitle : clin?.ClinNumber ?? string.Empty;
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    string activityId = ws.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;
                    string activityName = ws.IsProjectMapWorkspace ? boe.Description : task.TaskTitle;
                    majorGroupingText = reportType == SSRSReportType.ProjectCLINCategoryCostSummary ? clinText : boe.Category ?? task.Category;
                    minorGroupingText = reportType == SSRSReportType.ProjectCLINCategoryCostSummary ? boe.Category ?? task.Category : clinText;

                    foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                    {
                        ResourceDTO resource = ws.ResourcesForWsResourceListId.First(r => r.Id == laborResource.ResourceID);
                        PerformingOrgDTO perfOrg = ws.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
                        summaryReport.Add(new SummaryReportRMSModelView()
                        {
                            MajorGroupingLabel = majorGroupingLabel,
                            MinorGroupingLabel = minorGroupingLabel,
                            ReportTitle = reportTitle,
                            Project = projectName,
                            MajorGroupingText = majorGroupingText,
                            MinorGroupingText = minorGroupingText,
                            ActivityID = activityId ?? string.Empty,
                            ActivityName = RTEUtilities.TurnHTMLIntoPlainText(activityName),
                            Resource = Utilities.FormatResourceNames(resource.ResourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources), laborResource is SubResourceTypeDto),
                            ResourceName = perfOrg.PerformingOrgDesc,
                            StartDate = laborResource.StartDateValue.ToMonthString(),
                            EndDate = laborResource.EndDateValue.ToMonthString(),
                            LaborHrs = laborResource.SpreadType == SpreadType.Hours ? laborResource.ValueSpread.ToString() : "0",
                            MatlCost = laborResource.SpreadType == SpreadType.Cost ? laborResource.ValueSpread.ToString() : "0"
                        });
                    }
                }
            }

            return summaryReport;
        }

        /// <summary>
        /// Builds model views for PRP report
        /// </summary>
        /// <param name="boes">All BOEs including offloaded</param>
        /// <param name="fullWS">Full Workspace</param>
        /// <returns>A list of PRP model views.</returns>
        public ICollection<PRPModelView> GetPRPModelViews(ICollection<FullBoe> boes, FullWorkspace fullWS)
        {
            Collection<PRPModelView> modelViews = new Collection<PRPModelView>();
            
            if (fullWS != null && boes != null && boes.Any())
            {
                // Determine the start year for the entire report and create the start date.
                int reportStartYear = this.GetMinimumResourceStartYear(boes, boes.SelectMany(x => x.TaskElements).ToList());
                DateTime reportStartDate = new DateTime(reportStartYear, 1, 1);  // Always starts on January 1st of the start year.

                IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(fullWS.IsProjectMapWorkspace);

                // Retrieve data and build the ModelViews.
                foreach (FullBoe boe in boes)
                {
                    foreach (BoeTaskElementDTO task in boe.TaskElements)
                    {
                        // Project map -> activity is fed from BOE. Standard -> fed from task. (facepalm)
                        string activityId = fullWS.IsProjectMapWorkspace ? boe.Title : task.BOETaskID;

                        foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                        {
                            PerformingOrgDTO perfOrg = fullWS.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
                            ResourceDTO resource = fullWS.ResourcesForWsResourceListId.First(r => r.Id == laborResource.ResourceID);
                            WbsDTO wbs = fullWS.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                            ClinDTO clin = fullWS.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                            string clinText = fullWS.IsProjectMapWorkspace ? clin?.ClinTitle : clin?.ClinNumber ?? string.Empty;

                            // Get the Labor Spread summaries for each year.
                            foreach (ResourceSpreadDto spread in laborResource.LaborSpreads)
                            {
                                modelViews.Add(new PRPModelView()
                                {
                                    Project = fullWS.WorkspaceName,
                                    CLIN = clinText,
                                    WBS = wbs?.WbsNumber ?? string.Empty,
                                    ActivityId = activityId ?? string.Empty,
                                    ResourceType = resource.ElementOfCost.GetDescription(),
                                    Resource = Utilities.FormatResourceNames(resource.ResourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources), laborResource is SubResourceTypeDto),
                                    CostCenter = perfOrg.PerformingOrgName,
                                    CostCenterDescription = perfOrg.PerformingOrgDesc,
                                    Category = boe.Category ?? task.Category ?? string.Empty,
                                    StartDate = laborResource.StartDate,
                                    EndDate = laborResource.EndDate,
                                    Month = this.GetMonthRelativeToStartYear(reportStartDate, spread.LaborSpreadDate),
                                    Value = spread.LaborSpreadValue,
                                    ReportStartDate = reportStartDate,
                                    ResourceUnit = resource.RateType == RateType.Cost ? Constants.RPS_RESOURCE_UNIT_DOLLARS : Constants.RPS_RESOURCE_UNIT_HOURS
                                });
                            }
                        }
                    }
                }
            }

            // Create empty records for all of the missing months so that the corresponding columns are created and visible in the report.
            this.PRPReportCreateMissingMonths(modelViews);

            // Only return ModelViews that have month values that fall within the range for this report.
            return modelViews.Where(r => r.Month > 0 && r.Month <= Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS).ToList();
        }

        /// <summary>
        /// Gets the Cost By Pricing Code/Resource Report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded Boes</param>
        /// <param name="fullWS">Workspace for the report</param>
        /// <returns>The ModelViews for the report</returns>
        public ICollection<CostByPricingCodeReportModelView> GetCostByPriceCodeResource(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS)
        {
            ICollection<CostByPricingCodeReportModelView> costByPriceCodeResourceModelView = this.GetDataForPricingCodeReports(fullWS, offloadedBoes, false);

            return costByPriceCodeResourceModelView;
        }

        /// <summary>
        /// Gets the Cost By Category/Pricing Code Report model views.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded boes.</param>
        /// <param name="ws">The workspace.</param>
        /// <returns>The ModelViews for the report.</returns>
        public ICollection<CostByPricingCodeReportModelView> GetCostByCategoryPricingCodeReportModelView(ICollection<FullBoe> offloadedBoes, FullWorkspace ws)
        {
            ICollection<CostByPricingCodeReportModelView> costByCategoryPricingCodeReport = this.GetDataForPricingCodeReports(ws, offloadedBoes, true);

            return costByCategoryPricingCodeReport;
        }

        /// <summary>
        /// Get the modelview data for the Cost By Pricing Code and Cost By Category/Pricing Code Reports
        /// </summary>
        /// <param name="ws">Full WS</param>
        /// <param name="boes">offloaded boes for the workspace</param>
        /// <param name="includeCategory">bool noting if this report contains Category</param>
        /// <returns>Collection of modelviews for the report</returns>
        private ICollection<CostByPricingCodeReportModelView> GetDataForPricingCodeReports(FullWorkspace ws, ICollection<FullBoe> boes, bool includeCategory)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            ICollection<CostByPricingCodeReportModelView> toReturn = new Collection<CostByPricingCodeReportModelView>();

            int reportStartYear = boes.Select(b => b.StartDate).Min().Year;

            HashSet<PerformingOrgDTO> performingOrgs = new HashSet<PerformingOrgDTO>(ws.PerformingOrgsUsedInBoes);
            HashSet<ResourceDTO> resources = new HashSet<ResourceDTO>(ws.ResourcesUsedInWsBoes);
            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(ws.IsProjectMapWorkspace);

            foreach (FullBoe boe in boes)
            {
                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    foreach (ResourceTypeDto resourceType in task.taskElementLabors)
                    {
                        // if we are missing the resource, we'll grab it by the id.. this is likely going to trip for the offload resource
                        // we'll add it into the hashset, to make sure we don't have to keep pulling the same thing... :)
                        if (!resources.Any(x => x.Id == resourceType.ResourceID))
                        {
                            resources.Add(this.resourceLoader.GetById(resourceType.ResourceID.Value));
                        }

                        string resourceName = Utilities.FormatResourceNames(resources.First(x => x.Id == resourceType.ResourceID).ResourceName,
                            this.commonDataMapper.GetSikorskyLegacyResourceID(resourceType.LegacyID, allLegacyResources), resourceType is SubResourceTypeDto);

                        PerformingOrgDTO costCenter = performingOrgs.First(x => x.Id == resourceType.PerformingOrgID);

                        // Check for existing MV with this Resource Type's resource, cost center, pricing code, and (if applicable) category
                        ICollection<CostByPricingCodeReportModelView> existingResourceTypes = toReturn.Where(x => x.Resource == resourceName
                            && x.CostCenter == costCenter.PerformingOrgName && x.PricingCode == costCenter.PerformingOrgDesc).ToCollection();

                        if (includeCategory)
                        {
                            existingResourceTypes = existingResourceTypes.Where(x => x.Category == (boe.Category ?? task.Category)).ToCollection();
                        }

                        CostByPricingCodeReportModelView existingResourceType = existingResourceTypes.FirstOrDefault();

                        // If there is an existing MV, add the spread values to it rather than making a new MV
                        if (existingResourceType != null)
                        {
                            existingResourceType.Year01 += this.GetSpreadValueByYear(resourceType, reportStartYear);
                            existingResourceType.Year02 += this.GetSpreadValueByYear(resourceType, reportStartYear + 1);
                            existingResourceType.Year03 += this.GetSpreadValueByYear(resourceType, reportStartYear + 2);
                            existingResourceType.Year04 += this.GetSpreadValueByYear(resourceType, reportStartYear + 3);
                            existingResourceType.Year05 += this.GetSpreadValueByYear(resourceType, reportStartYear + 4);
                            existingResourceType.Year06 += this.GetSpreadValueByYear(resourceType, reportStartYear + 5);
                            existingResourceType.Year07 += this.GetSpreadValueByYear(resourceType, reportStartYear + 6);
                            existingResourceType.Year08 += this.GetSpreadValueByYear(resourceType, reportStartYear + 7);
                            existingResourceType.Year09 += this.GetSpreadValueByYear(resourceType, reportStartYear + 8);
                            existingResourceType.Year10 += this.GetSpreadValueByYear(resourceType, reportStartYear + 9);
                            existingResourceType.Year11 += this.GetSpreadValueByYear(resourceType, reportStartYear + 10);
                            existingResourceType.Year12 += this.GetSpreadValueByYear(resourceType, reportStartYear + 11);
                            existingResourceType.Year13 += this.GetSpreadValueByYear(resourceType, reportStartYear + 12);
                            existingResourceType.Year14 += this.GetSpreadValueByYear(resourceType, reportStartYear + 13);
                            existingResourceType.Year15 += this.GetSpreadValueByYear(resourceType, reportStartYear + 14);
                            existingResourceType.Year16 += this.GetSpreadValueByYear(resourceType, reportStartYear + 15);
                            existingResourceType.Year17 += this.GetSpreadValueByYear(resourceType, reportStartYear + 16);
                        }
                        else
                        {
                            // If there is no existing MV, create one
                            toReturn.Add(new CostByPricingCodeReportModelView()
                            {
                                Project = ws.WorkspaceName,
                                Resource = resourceName ?? string.Empty,
                                ResourceType = resourceType.SpreadType == SpreadType.Cost ? "Cost Dollars" : "Labor Hours",
                                CostCenter = costCenter.PerformingOrgName ?? string.Empty,
                                PricingCode = costCenter.PerformingOrgDesc ?? string.Empty,
                                Category = boe.Category ?? task.Category ?? string.Empty,
                                StartYear = reportStartYear,
                                Year01 = this.GetSpreadValueByYear(resourceType, reportStartYear),
                                Year02 = this.GetSpreadValueByYear(resourceType, reportStartYear + 1),
                                Year03 = this.GetSpreadValueByYear(resourceType, reportStartYear + 2),
                                Year04 = this.GetSpreadValueByYear(resourceType, reportStartYear + 3),
                                Year05 = this.GetSpreadValueByYear(resourceType, reportStartYear + 4),
                                Year06 = this.GetSpreadValueByYear(resourceType, reportStartYear + 5),
                                Year07 = this.GetSpreadValueByYear(resourceType, reportStartYear + 6),
                                Year08 = this.GetSpreadValueByYear(resourceType, reportStartYear + 7),
                                Year09 = this.GetSpreadValueByYear(resourceType, reportStartYear + 8),
                                Year10 = this.GetSpreadValueByYear(resourceType, reportStartYear + 9),
                                Year11 = this.GetSpreadValueByYear(resourceType, reportStartYear + 10),
                                Year12 = this.GetSpreadValueByYear(resourceType, reportStartYear + 11),
                                Year13 = this.GetSpreadValueByYear(resourceType, reportStartYear + 12),
                                Year14 = this.GetSpreadValueByYear(resourceType, reportStartYear + 13),
                                Year15 = this.GetSpreadValueByYear(resourceType, reportStartYear + 14),
                                Year16 = this.GetSpreadValueByYear(resourceType, reportStartYear + 15),
                                Year17 = this.GetSpreadValueByYear(resourceType, reportStartYear + 16)
                            });
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the spread value for a resource type for a particular year
        /// </summary>
        /// <param name="resourceType">the resource type</param>
        /// <param name="year">the year</param>
        /// <returns>spread value for the resource type for the given year</returns>
        private decimal GetSpreadValueByYear(ResourceTypeDto resourceType, int year)
        {
            return resourceType.LaborSpreads.Where(x => x.LaborSpreadDate.Year == year).Sum(y => y.LaborSpreadValue);
        }

        /// <summary>
        /// Adds elements to the collection with a zero value for each month that does not currently exist to ensure that the report
        /// has columns for every month, even if none of the resources have a value for that given month.
        /// </summary>
        /// <param name="prpReport">The collection of report ModelViews.</param>
        public void PRPReportCreateMissingMonths(ICollection<PRPModelView> prpReport)
        {
            if (prpReport == null || !prpReport.Any())
            {
                return;
            }

            // Get the distinct list of month numbers.  This is all of the months for the report that currently have an associated value.
            // These months will NOT have an empty (zero) value created for them.
            Collection<int> monthsWithValue = new Collection<int>((from mv in prpReport group mv by mv.Month into g orderby g.Key select g.Key).ToList());
            PRPModelView firstMV = prpReport.First();

            // For all applicable months for the report...
            for (int month = 1; month <= Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS; month++)
            {
                // ...if the report does not already contain a value for that month...
                if (monthsWithValue.None() || !monthsWithValue.Contains(month))
                {
                    // ...create a dummy element with a value of 0 for that month. This will ensure that all applicable
                    //    month columns get created for the report and that none are skipped due to not having a resource with a value for that month.
                    prpReport.Add(new PRPModelView()
                    {
                        Project = firstMV.Project,
                        CLIN = firstMV.CLIN,
                        WBS = firstMV.WBS,
                        ActivityId = firstMV.ActivityId,
                        ResourceType = firstMV.ResourceType,
                        Resource = firstMV.Resource,
                        CostCenter = firstMV.CostCenter,
                        CostCenterDescription = firstMV.CostCenterDescription,
                        Category = firstMV.Category,
                        StartDate = firstMV.StartDate,
                        EndDate = firstMV.EndDate,
                        Month = month,
                        Value = Convert.ToDecimal(0),
                        ReportStartDate = firstMV.ReportStartDate,
                        ResourceUnit = firstMV.ResourceUnit
                    });
                }
            }
        }

        /// <summary>
        /// Adds elements to the collection with a zero value for each month that does not currently exist to ensure that the report
        /// has columns for every month, even if none of the resources have a value for that given month.
        /// </summary>
        /// <param name="rpsReport">The collection of report ModelViews.</param>
        public void RPSReportCreateMissingMonths(ICollection<RPSReportModelView> rpsReport)
        {
            if (rpsReport != null && rpsReport.Any())
            {
                // Get a dummy ModelView for the first item in the collection.
                RPSReportModelView firstMV = rpsReport.First();

                // Get the distinct list of month numbers.  This is all of the months for the report that currently have an associated value.
                // These months will NOT have an empty (zero) value created for them.
                Collection<int> months = new Collection<int>((from mv in rpsReport group mv by mv.Month into g orderby g.Key ascending select g.Key).ToList());
                bool noMonthData = months.None();

                // For all applicable months for the report...
                for (int i = 1; i <= Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS; i++)
                {
                    // ...if the report does not already contain a value for that month...
                    if (noMonthData || (!months.Contains(i)))
                    {
                        // ...create a dummy element with a value of 0 for that month using the dummy ModelView data.  This will ensure that all applicable
                        //    month columns get created for the report and that none are skipped due to not having a resource with a value for that month.
                        rpsReport.Add(new RPSReportModelView()
                        {
                            Project = firstMV.Project,
                            Resource = firstMV.Resource,
                            CostCenter = firstMV.CostCenter,
                            CostCenterDescription = firstMV.CostCenterDescription,
                            ResourceType = firstMV.ResourceType,
                            OffloadRate = firstMV.OffloadRate,
                            Month = i,
                            Value = Convert.ToDecimal(0),
                            StartDate = firstMV.StartDate,
                            ResourceUnit = firstMV.ResourceUnit
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Get the ModelViews for the RPS report.
        /// </summary>
        /// <param name="offloadedBoes">The offloaded BOEs.</param>
        /// <param name="fullWS">The FullWorkspace object.</param>
        /// <returns>The ModelViews for the report.</returns>
        public ICollection<RPSReportModelView> GetRPSReportModelViews(ICollection<FullBoe> offloadedBoes, FullWorkspace fullWS)
        {
            Collection<RPSReportModelView> modelViews = new Collection<RPSReportModelView>();

            if (fullWS != null && offloadedBoes != null && offloadedBoes.Any())
            {
                ICollection<BoeTaskElementDTO> allTaskElements = offloadedBoes.SelectMany(x => x.TaskElements).ToList();
                ICollection<OffloadRatesDTO> offloadRates = this.offloadRatesLoader.GetByWorkspaceId(fullWS.Id);
                IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(fullWS.IsProjectMapWorkspace);

                // Determine the start year for the entire report and create the start date.
                int reportStartYear = this.GetMinimumResourceStartYear(offloadedBoes, allTaskElements);
                DateTime reportStartDate = new DateTime(reportStartYear, 1, 1);  // Always starts on January 1st of the start year.

                // Retrieve data and build the ModelViews.
                foreach (FullBoe boe in offloadedBoes)
                {
                    foreach (BoeTaskElementDTO task in boe.TaskElements)
                    {
                        foreach (ResourceTypeDto laborResource in task.taskElementLabors)
                        {
                            PerformingOrgDTO perfOrg = fullWS.PerformingOrgsForWsList.First(p => p.Id == laborResource.PerformingOrgID);
                            ResourceDTO resource = fullWS.ResourcesForWsResourceListId.First(r => r.Id == laborResource.ResourceID);
                            string resourceName = Utilities.FormatResourceNames(resource.ResourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborResource.LegacyID, allLegacyResources), laborResource is SubResourceTypeDto);

                            // Get the Labor Spread summaries for each year.
                            // Dictionary<int, decimal> laborSpreadYearSummaries = this.GetCostAnalysisReportLaborSpreadSummariesByYear(reportStartYear, laborResource.LaborSpreads);
                            foreach (ResourceSpreadDto spread in laborResource.LaborSpreads)
                            {
                                OffloadRatesDTO offloadRate = offloadRates.FirstOrDefault(o => o.Resource == resource.ResourceName && o.PerformingOrg == perfOrg.PerformingOrgName && o.Year == reportStartYear);
                                decimal offloadPercent = offloadRate != null ? offloadRate.Percent : 0;

                                modelViews.Add(new RPSReportModelView()
                                {
                                    Project = fullWS.WorkspaceName,
                                    Resource = resourceName,
                                    CostCenter = perfOrg.PerformingOrgName,
                                    CostCenterDescription = perfOrg.PerformingOrgDesc,
                                    ResourceType = resource.ElementOfCost.GetDescription(),
                                    OffloadRate = offloadPercent * 100,
                                    Month = this.GetMonthRelativeToStartYear(reportStartDate, spread.LaborSpreadDate),
                                    Value = spread.LaborSpreadValue,
                                    StartDate = reportStartDate,
                                    ResourceUnit = resource.RateType == RateType.Cost ? Constants.RPS_RESOURCE_UNIT_DOLLARS : Constants.RPS_RESOURCE_UNIT_HOURS
                                });
                            }
                        }
                    }
                }
            }

            // Only return ModelViews that have month values that fall within the range for this report.
            return modelViews.Where(r => r.Month > 0 && r.Month <= Constants.SSRS_RPS_REPORT_NUMBER_OF_MONTH_COLUMNS).ToList();
        }

        /// <summary>
        /// Determine the the Month for the Resource, which is the number of the month relative to the starting Month for the report.
        /// The starting month is 1.
        /// </summary>
        /// <param name="reportStartDate">The Start Date for the report.</param>
        /// <param name="laborSpreadStartDate">The Start Date for the Labor Spread Resource.</param>
        /// <returns>The Month for the Resource.</returns>
        internal int GetMonthRelativeToStartYear(DateTime? reportStartDate, DateTime? laborSpreadStartDate)
        {
            int resourceMonth = -1;

            if (reportStartDate != null && laborSpreadStartDate != null)
            {
                // Calculate the number of months relative to the starting month of the report.  The starting month is 1.
                resourceMonth = laborSpreadStartDate.Value.Month + ((laborSpreadStartDate.Value.Year - reportStartDate.Value.Year) * 12);
            }

            return resourceMonth;
        }

        /// <summary>
        /// Generates the report with a nonce string that relates to that report..
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="reportType">Type of the report.</param>
        /// <returns>The nonce string related to the generated report.</returns>
        public string GenerateReportNonce(FullWorkspace workspace, Reports reportType)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            string nonce = Guid.NewGuid().ToString("N");

            // Only get reports for RMS
            if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
            {
                // Pre vs Post Offload totals does the offload inside of the controllerlogic call
                ICollection<FullBoe> boes = reportType == Reports.PreVsPostOffloadTotals ? null : this.GetOffloadBOEsFromFullWorkspace(workspace);

                string reportXml;

                // generate the report Dto and transform Dto into Xml
                switch (reportType)
                {
                    case Reports.StaffingCurves:
                        List<StaffingCurvesReportModelView> staffingCurvesReport = this.GetStaffingCurvesReportModelViews(boes, workspace).ToList();
                        reportXml = staffingCurvesReport.ToXmlString();
                        break;
                    case Reports.BOESummaryReport:
                        List<BOESummaryReportModelView> summaryReport = this.GetBOESummaryReportModelViews(boes, workspace).ToList();
                        reportXml = summaryReport.ToXmlString();
                        break;
                    case Reports.CostByClinResActYrFlat:
                        List<CostAnalysisReportRMSModelView> flatCostReport = this.GetCostAnalysisReportModelViews(boes, workspace, SSRSReportType.CostAnalysis8YearsFlat).ToList();

                        flatCostReport = flatCostReport.OrderBy(x => x.CLIN).ThenBy(x => x.AddDelete).ThenBy(x => x.ActivityID).ToList();

                        reportXml = flatCostReport.ToXmlString();
                        break;
                    case Reports.CostByClinResActYr:
                        List<CostAnalysisReportRMSModelView> costReport = this.GetCostAnalysisReportModelViews(boes, workspace, SSRSReportType.CostAnalysis8Years).ToList();
                        reportXml = costReport.ToXmlString();
                        break;
                    case Reports.CostByClinActYr:
                        List<CostAnalysisReportRMSModelView> costClinReport = this.GetCostAnalysisReportModelViews(boes, workspace, SSRSReportType.CostAnalysis17Years).ToList();
                        reportXml = costClinReport.ToXmlString();
                        break;
                    case Reports.OffloadCostByYear:
                        List<OffloadCostByYearReportRMSModelView> costByYearReport = this.GetOffloadCostByYearReportModelViews(boes, workspace).ToList();
                        reportXml = costByYearReport.ToXmlString();
                        break;
                    case Reports.OffloadDetailedReport:
                        List<OffloadCostByYearReportRMSModelView> offloadDetailedReport = this.GetOffloadDetailedReportModelViews(boes, workspace).ToList();
                        reportXml = offloadDetailedReport.ToXmlString();
                        break;
                    case Reports.OffloadCostSummary:
                        ICollection<OffloadCostByYearReportRMSModelView> offloadCostByYearReportRows = this.GetOffloadCostByYearReportModelViews(boes, workspace).ToList();
                        List<OffloadCostByYearReportRMSModelView> offloadCostSummaryReportRows = this.ConsolidateOffloadCostByYear(offloadCostByYearReportRows).ToList();
                        reportXml = offloadCostSummaryReportRows.ToXmlString();
                        break;
                    case Reports.PreVsPostOffloadTotals:
                        List<PreVsPostOffloadTotalsModelView> preVsPostOffloadTotals = this.GetPreVsPostOffloadTotalsModelViews(workspace).ToList();
                        reportXml = preVsPostOffloadTotals.ToXmlString();
                        break;
                    case Reports.Prp:
                        List<PRPModelView> prpModelViews = this.GetPRPModelViews(boes, workspace).ToList();
                        reportXml = prpModelViews.ToXmlString();
                        break;
                    case Reports.Rps:
                        List<RPSReportModelView> rpsReport = this.GetRPSReportModelViews(boes, workspace).ToList();
                        reportXml = rpsReport.ToXmlString();
                        break;
                    case Reports.Ram:
                        List<RAMReportModelView> ramReport = this.GetRamReportModelViews(boes, workspace).ToList();
                        reportXml = ramReport.ToXmlString();
                        break;
                    case Reports.CategoryClinSummary:
                        List<SummaryReportRMSModelView> categoryClinReport = this.GetSummaryReportRMSModelViews(boes, workspace, SSRSReportType.ProjectCategoryCLINCostSummary).ToList();
                        reportXml = categoryClinReport.ToXmlString();
                        break;
                    case Reports.ClinCategorySummary:
                        List<SummaryReportRMSModelView> clinCategoryReport = this.GetSummaryReportRMSModelViews(boes, workspace, SSRSReportType.ProjectCLINCategoryCostSummary).ToList();
                        reportXml = clinCategoryReport.ToXmlString();
                        break;
                    case Reports.ProjectClinCostSummary:
                        List<ProjectWbsCostSummaryByClinModelView> projectCLINCostSummaryModelViews = this.GetProjectCLINCostSummaryModelViews(boes, workspace).ToList();
                        reportXml = projectCLINCostSummaryModelViews.ToXmlString();
                        break;
                    case Reports.ByCatPricingCode:
                        List<CostByPricingCodeReportModelView> byCategoryPricingCodeReport = this.GetCostByCategoryPricingCodeReportModelView(boes, workspace).ToList();
                        reportXml = byCategoryPricingCodeReport.ToXmlString();
                        break;
                    case Reports.ByPricingCode:
                        List<CostByPricingCodeReportModelView> costByCategoryPriceResource = this.GetCostByPriceCodeResource(boes, workspace).ToList();
                        reportXml = costByCategoryPriceResource.ToXmlString();
                        break;
                    case Reports.WorkbenchOffload:
                        List<WorkbenchOffloadModelView> workbenchOffload = this.GetWorkbenchOffloadModelViews(boes, workspace).ToList();
                        reportXml = workbenchOffload.ToXmlString();
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // remove the xml version/encoding tag since that will cause problems inside SSRS
                reportXml = reportXml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", string.Empty);
                reportXml = reportXml.Replace("\r\n", string.Empty);

                // Save xml
                this.workspaceLoader.InsertReportXml(nonce, reportXml);
            }

            return nonce;
        }

        /// <summary>
        /// Bulk downloads reports.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="downloadReports">The download reports.</param>
        /// <param name="zipFilePath">The zip file path to save into.</param>
        /// <returns>The location of the bulk file to send back.</returns>
        public string BulkDownloadReport(FullWorkspace workspace, ICollection<DownloadReportModelView> downloadReports, string zipFilePath)
        {
            if (downloadReports == null)
            {
                throw new ArgumentNullException(nameof(downloadReports));
            }
            
            string zippedFileName;

            string tempDirectory = Path.Combine(zipFilePath, Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);

            try
            {
                Collection<ReportDTO> reportsAvailable = this.commonDataMapper.getReports();

                foreach (DownloadReportModelView downloadReport in downloadReports)
                {
                    ReportDTO realReport = reportsAvailable.FirstOrDefault(r => r.ReportID == (int)downloadReport.ReportType);
                    string nonce;
                    using (StopwatchTimer sw = new StopwatchTimer($"Generating Report {downloadReport.ReportType.ToDescription()}", this.logger))
                    {
                        nonce = this.GenerateReportNonce(workspace, downloadReport.ReportType);
                    }

                    string link = downloadReport.SSRSLink + nonce + @"&rs:format=" + downloadReport.FormatType;

                    string tempFileName = Path.GetRandomFileName();
                    string tempFileLocation = Path.Combine(tempDirectory, tempFileName);
                    using (StopwatchTimer sw = new StopwatchTimer($"Downloading Report {downloadReport.ReportType.ToDescription()}", this.logger))
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.UseDefaultCredentials = true;
                            client.DownloadFile(link, tempFileLocation);

                            var header = new ContentDisposition(client.ResponseHeaders["Content-Disposition"]);
                            string realFilename = header.FileName;
                            if (realReport != null)
                            {
                                string ext = realFilename.Split('.').Last();
                                realFilename = realReport.ReportName + "." + ext;
                                realFilename = realFilename.Replace(' ', '_');
                            }

                            string realFileLocation = Path.Combine(tempDirectory, realFilename);
                            if (File.Exists(realFileLocation))
                            {
                                // this can happen since we have some reports that use the same SSRS report file
                                string duplicateName = Path.GetFileNameWithoutExtension(realFilename) + "(1)" + Path.GetExtension(realFilename);
                                realFileLocation = Path.Combine(tempDirectory, duplicateName);
                            }

                            File.Move(tempFileLocation, realFileLocation);
                        }
                    }
                }

                // Zip files and return zip file to user as a Download
                zippedFileName = Path.Combine(zipFilePath, Path.GetRandomFileName() + ".zip");
                using (StopwatchTimer sw = new StopwatchTimer("Zipping all files", this.logger))
                {
                    ZipFile.CreateFromDirectory(tempDirectory, zippedFileName);
                }
            }
            finally
            {
                // Delete the temp directory
                Directory.Delete(tempDirectory, true);
            }
            
            return zippedFileName;
        }
    }
}
