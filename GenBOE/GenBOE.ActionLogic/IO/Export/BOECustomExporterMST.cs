// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using DocumentFormat.OpenXml.Wordprocessing;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.ZoneTravel;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public class BOECustomExporterMST : BOECustomExporter
    {
        private MSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader;
        private RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader;

        public BOECustomExporterMST
            (
                IUserDTODataLoader inIUserDTODataLoader,
                ICommonDataMapper inICommonDataMapper,
                TravelTripCostCalculation inTravelTripCostCalculation,
                IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
                MSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader,
                RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader,
				IRetriever retriever
            ) : base(
                inIUserDTODataLoader,
                inICommonDataMapper,
                inTravelTripCostCalculation,
                inVariableSelectBOEtoSumCalculation,
				retriever
            )
        {
            // constructor
            this.DefaultCurrencyFormat = BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS;
            this.mstZoneTravelResourceDTODataLoader = mstZoneTravelResourceDTODataLoader;
            this.rmsZoneTravelRatesFeesDataLoader = rmsZoneTravelRatesFeesDataLoader;
        }

        #region Method Overrides

        #region Resource Summary

        /// <summary>
        /// Populate the Resource Summary by Resource ID Table
        /// </summary>
        /// <param name="tableContainerElement">container element for the table</param>
        /// <param name="boeExportModelView">Model View for the BOE Export with the resources to use</param>
        protected override void PopulateResourceSummaryByResourceIDTable(SdtElement tableContainerElement, BOEExportModelView boeExportModelView)
        {
            if (boeExportModelView != null)
            {
                ICollection<ResourceSummaryRowData> resourceData = new Collection<ResourceSummaryRowData>();

                foreach (BOEExportTaskElementLabor taskElementLabor in boeExportModelView.TaskElements.Where(e => e.ElementType != BOEExportTaskElementType.Travel).SelectMany(t => t.taskElementLabors))
                {
                    resourceData.Add(new ResourceSummaryRowData()
                    {
                        ResourceType = taskElementLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost],
                        ResourceName = taskElementLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] + " - " + taskElementLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription],
                        CostTotal = taskElementLabor.Cost.HasValue ? taskElementLabor.Cost.Value : 0m,
                        HoursTotal = taskElementLabor.Hours.HasValue ? taskElementLabor.Hours.Value : 0m
                    });
                }

                ICollection<ResourceSummaryRowData> rollupData =
                    resourceData
                        .GroupBy(x => x.GroupKey)
                        .Select(g => new ResourceSummaryRowData
                        {
                            ResourceType = g.First().ResourceType,
                            ResourceName = g.First().ResourceName,
                            PeopleTotal = g.Sum(x => x.PeopleTotal),
                            HoursTotal = g.Sum(x => x.HoursTotal),
                            CostTotal = g.Sum(x => x.CostTotal)
                        })
                        .OrderBy(x => x.ResourceType).ToList();

                this.PopulateResourceSummaryTable(tableContainerElement, rollupData);
            }
            else
            {
                this.RemoveElement(tableContainerElement);
            }
        }

        /// <summary>
        /// Populates the resource summary table
        /// </summary>
        /// <param name="tableContainerElement">container element for the table</param>
        /// <param name="rollupData">rollup data to be displayed in the table</param>
        protected override void PopulateResourceSummaryTable(SdtElement tableContainerElement, ICollection<ResourceSummaryRowData> rollupData)
        {
            if (rollupData != null && rollupData.Any())
            {
                // locate the table markers
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);
                SdtElement dataTotalsRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_TotalsRow);

                // initialize the "insertion" row
                TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
                if (templateDataRow != null)
                {
                    TableRow currentInsertionRow = templateDataRow;

                    // accumulate totals
                    decimal matSubIwtaCostTotal = 0m;
                    decimal otherCostTotal = 0m;
                    decimal hoursTotal = 0m;
                    decimal costTotal = 0m;

                    foreach (ResourceSummaryRowData rollupRowData in rollupData)
                    {
                        //create a new summary data row in the table
                        //clone marked template row
                        TableRow tableRow = templateDataRow.CloneNode(true) as TableRow;
                        Parallel.ForEach(tableRow.Descendants(), descendant =>
                        {
                            if (descendant is SdtId || descendant is SdtPlaceholder)
                            {
                                descendant.RemoveIt();
                            }
                        }
                        );

                        decimal hours = rollupRowData.HoursTotal.HasValue ? rollupRowData.HoursTotal.Value : 0m;
                        hoursTotal += hours;
                        
                        decimal matSubIwtaCost = 0m;
                        decimal otherCost = 0m;
                        decimal cost = rollupRowData.CostTotal.HasValue ? rollupRowData.CostTotal.Value : 0m;
                        if (rollupRowData.ResourceType == ElementOfCostType.Materials.ToString() 
                            || rollupRowData.ResourceType == ElementOfCostType.Sub.ToString() 
                            || rollupRowData.ResourceType == ElementOfCostType.IWTA.ToString())
                        {
                            matSubIwtaCost = cost;
                        }
                        else if (rollupRowData.ResourceType == ElementOfCostType.ODC.ToString()
                            || rollupRowData.ResourceType == ElementOfCostType.Travel.ToString())
                        {
                            otherCost = cost;
                        }

                        matSubIwtaCostTotal += matSubIwtaCost;
                        otherCostTotal += otherCost;
                        costTotal += cost;
                        
                        //populate the row
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), rollupRowData.ResourceName);
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Hours), hours.ToString(this.DefaultHoursFormat));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_MatSubIWTACost), matSubIwtaCost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this._CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_OtherCost), otherCost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this._CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Cost), cost.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this._CurrencyFormatter));

                        //add the row to the table
                        currentInsertionRow.InsertAfterSelf(tableRow);
                        currentInsertionRow = tableRow;
                    }

                    //total row doesn't need to be cloned
                    TableRow totalsRow = dataTotalsRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();

                    if (totalsRow != null)
                    {
                        //populate the overall totals
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_HoursTotal), hoursTotal.ToString(this.DefaultHoursFormat));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_MatSubIWTACostTotal), matSubIwtaCostTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this._CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_OtherCostTotal), otherCostTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this._CurrencyFormatter));
                        WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(totalsRow, BOEExporterConstants.FieldName_CostTotal), costTotal.ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this._CurrencyFormatter));
                    }

                    //remove template rows
                    templateDataRow.RemoveIt();
                }
            }
            else
            {
                this.RemoveElement(tableContainerElement);
            }
        }

        /// <summary>
        /// Prepare data for the Labor Task Resource Table before populating it
        /// </summary>
        /// <param name="tableElement">SdtElement for the Labor Task Resource Table</param>
        /// <param name="laborTaskElement">Element for the labor task</param>
        /// <param name="allLaborTaskElements">All task elements for the BOE</param>
        /// <param name="allWorkspaceCustomFields">All custom fields in the current workspace</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <exception cref="System.ArgumentNullException">laborTaskElement</exception>
        protected override void PrepareLaborTaskResourceTableData(SdtElement tableElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, IReadOnlyCollection<CustomFieldDTO> allWorkspaceCustomFields, BOEExportInputs exportInputs)
        {
            if (laborTaskElement == null)
            {
                throw new ArgumentNullException(nameof(laborTaskElement));
            }

            ICollection<ResourceSummaryRowData> resourceData = laborTaskElement.taskElementLabors.Select(r => new ResourceSummaryRowData
            {
                ResourceType = r.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost],
                ResourceName = r.ExportFields[BOEExporterConstants.FieldName_ResourceName] + " - " + r.ExportFields[BOEExporterConstants.FieldName_ResourceDescription],
                CostTotal = r.Cost.HasValue ? r.Cost.Value : 0m,
                HoursTotal = r.Hours.HasValue ? r.Hours.Value : 0m
            }).ToList();

            ICollection<ResourceSummaryRowData> rollupData =
                resourceData
                    .GroupBy(x => x.GroupKey)
                    .Select(g => new ResourceSummaryRowData
                    {
                        ResourceType = g.First().ResourceType,
                        ResourceName = g.First().ResourceName,
                        CostTotal = g.Sum(x => x.CostTotal),
                        HoursTotal = g.Sum(x => x.HoursTotal)
                    })
                    .OrderBy(x => x.ResourceType).ToList();

            this.PopulateResourceSummaryTable(tableElement, rollupData);
        }

        #endregion

        #region Spread Rollup Tables

        /// <summary>
        /// Prepare the data for the Labor Task Hours Rollup Table before populating it
        /// </summary>
        /// <param name="templateElement">Template element for the table</param>
        /// <param name="laborTaskElement">Element for the labor task</param>
        /// <param name="allLaborTaskElements">All task elements for the BOE</param>
        /// <param name="boeExportModelView">The boe export model view.</param>
        /// <param name="useGfy">Should Government Fiscal Years be used</param>
        /// <param name="byQuarter">If table is by quarter</param>
        /// <exception cref="System.ArgumentNullException">templateElement</exception>
        protected override void PrepareLaborTaskHoursRollupTableData(SdtElement templateElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportModelView boeExportModelView, bool useGfy, bool byQuarter)
        {
            if (templateElement == null)
            {
                throw new ArgumentNullException(nameof(templateElement));
            }

            BoeTaskElementDTO currentLaborTaskElement = allLaborTaskElements.FirstOrDefault(x => x.Id == laborTaskElement.BOETaskElementID.Value);
            ICollection<BoeTaskElementDTO> currentLaborTaskElementAsICollection = new Collection<BoeTaskElementDTO>();
            currentLaborTaskElementAsICollection.Add(currentLaborTaskElement);

            SdtElement currentInsertionElement = templateElement;

            List<LaborRollupByDateNew> laborRollupData = this.GetRollupByYear(currentLaborTaskElementAsICollection, null, RateType.Hours, useGfy);
            IList<RollupSummaryByYearTableRowData> laborHoursSummaryRollupData = laborRollupData.Convert();

            RollupSummaryByYearTableData laborRollupTableData = new RollupSummaryByYearTableData
            {
                SummaryTotalComplete = laborHoursSummaryRollupData.Sum(d => d.YearTotal),
                YearlyData = laborHoursSummaryRollupData
            };

            SdtElement HourRollupTableElement = templateElement.CloneNode(true) as SdtElement;
            if (this.PopulateRollupSummaryByYearTable(HourRollupTableElement, null, laborRollupTableData, this.DefaultHoursFormat, byQuarter, useGfy))
            {
                currentInsertionElement.InsertAfterSelf(HourRollupTableElement);
                currentInsertionElement = HourRollupTableElement;
            }

            this.RemoveElement(templateElement);
        }

        /// <summary>
        /// Process the Labor Task Cost Spread Rollup Table before populating it
        /// </summary>
        /// <param name="containerElement">Container element for task</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelView">The boe export model view.</param>
        /// <param name="laborTaskElement">Element for the labor task</param>
        /// <param name="allLaborTaskElements">All task elements for the BOE</param>
        /// <param name="selectedComponents">Components selected for the output</param>
        /// <param name="useGfy">Should Government Fiscal Years be used</param>
        /// <exception cref="System.ArgumentNullException">selectedComponents</exception>
        protected override void ProcessLaborTaskCostSpreadRollupTable(SdtElement containerElement, BOEExportInputs exportInputs, BOEExportModelView boeExportModelView, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, bool useGfy)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            #region Cost Spread Rollup Table

            SdtElement costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, 
                useGfy ? BOEExporterConstants.Table_GfyCostSpreadRollup : BOEExporterConstants.Table_CostSpreadRollup);

            bool byQuarter = false;
            if (useGfy && costSpreadRollupTableTemplateElement == null)
            {
                costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_CostSpreadRollupByQuarter);
                byQuarter = true;
            }

            if (costSpreadRollupTableTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
                {
                    BoeTaskElementDTO currentLaborTaskElement = allLaborTaskElements.FirstOrDefault(x => x.Id == laborTaskElement.BOETaskElementID.Value);
                    ICollection<BoeTaskElementDTO> currentLaborTaskElementAsICollection = new Collection<BoeTaskElementDTO>();
                    currentLaborTaskElementAsICollection.Add(currentLaborTaskElement);

                    SdtElement currentInsertionElement = costSpreadRollupTableTemplateElement;

                    List<LaborRollupByDateNew> laborRollupData = this.GetTaskCostRollup(currentLaborTaskElementAsICollection, exportInputs, null, useGfy);
                    IList<RollupSummaryByYearTableRowData> laborCostSummaryRollupData = laborRollupData.Convert();

                    RollupSummaryByYearTableData laborRollupTableData = new RollupSummaryByYearTableData
                    {
                        SummaryTotalComplete = laborCostSummaryRollupData.Sum(d => d.YearTotal),
                        YearlyData = laborCostSummaryRollupData
                    };

                    SdtElement CostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;

                    if (this.PopulateRollupSummaryByYearTable(CostRollupTableElement, null, laborRollupTableData, this.DefaultCurrencyFormat, byQuarter, useGfy))
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(CostRollupTableElement);
                    }
                }
                this.RemoveElement(costSpreadRollupTableTemplateElement);
            }
            #endregion
        }

        /// <summary>
        /// Process the ODC Task Direct Cost Rollup Table before populating it
        /// </summary>
        /// <param name="containerElement">Container element for task</param>
        /// <param name="odcTaskElement">Element for the ODC task</param>
        /// <param name="selectedComponents">Components selected for the output</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="useGfy">use government fiscal year?</param>
        /// <exception cref="System.ArgumentNullException">
        /// selectedComponents
        /// or
        /// exportInputs
        /// </exception>
        protected override void ProcessODCTaskDirectCostRollupTable(SdtElement containerElement, BOEExportTaskElement odcTaskElement, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            #region ODC Cost Rollup Table

            SdtElement costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, 
                useGfy ? BOEExporterConstants.Table_GfyDirectCostRollup : BOEExporterConstants.Table_DirectCostRollup);

            bool byQuarter = false;
            if (useGfy && costSpreadRollupTableTemplateElement == null)
            {
                costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
                byQuarter = true;
            }

            if (costSpreadRollupTableTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
                {
                    SdtElement currentInsertionElement = costSpreadRollupTableTemplateElement;

                    OtherDirectCostDTO currentODCTaskElement = exportInputs.Odcs.First(x => x.Id == odcTaskElement.BOETaskElementID.Value);
                    List<LaborRollupByDateNew> ODCRollupData = this.GetODCCostRollup(currentODCTaskElement, useGfy);
                    IList<RollupSummaryByYearTableRowData> ODCCostSummaryRollupData = ODCRollupData.Convert();

                    RollupSummaryByYearTableData ODCRollupTableData = new RollupSummaryByYearTableData
                    {
                        SummaryTotalComplete = ODCCostSummaryRollupData.Sum(d => d.YearTotal),
                        YearlyData = ODCCostSummaryRollupData
                    };

                    SdtElement CostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;

                    if (this.PopulateRollupSummaryByYearTable(CostRollupTableElement, null, ODCRollupTableData, this.DefaultCurrencyFormat, byQuarter, useGfy))
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(CostRollupTableElement);
                    }
                }
                this.RemoveElement(costSpreadRollupTableTemplateElement);
            }
            #endregion
        }

        #endregion

        #region Resource Container

        /// <summary>
        /// Process the Task Resources for a Labor Task
        /// </summary>
        /// <param name="containerElement">Container element for the task</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="laborTaskElement">Element for the labor task</param>
        /// <param name="allLaborTaskElements">All labor task elements in the workspace</param>
        /// <param name="selectedComponents">Components selected for the output</param>
        /// <param name="multiBoe">Bool to note if BOE is multi wbs/clin</param>
        /// <exception cref="System.ArgumentNullException">
        /// selectedComponents
        /// or
        /// laborTaskElement
        /// or
        /// exportInputs
        /// </exception>
        protected override void ProcessLaborTaskResources(SdtElement containerElement, BOEExportInputs exportInputs, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, bool multiBoe)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (laborTaskElement == null)
            {
                throw new ArgumentNullException(nameof(laborTaskElement));
            }

            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            #region Resource Container

            SdtElement laborResourceContainerTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.ResourceContainerPrefix + BOEExporterConstants.TaskType_Labor);

            if (laborResourceContainerTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.ResourceInfoAndSpreadTables))
                {
					IOrderedEnumerable<BOEExportTaskElementLabor> orderedResources;
					if (Utilities.IsBRCEnabledForWorkspace(exportInputs.Workspace.Shortname))
					{
						orderedResources = laborTaskElement.taskElementLabors
						.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_BrcID)
						&& c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID)
						&& c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgID))
						.OrderBy(o => o.ExportFields[BOEExporterConstants.FieldName_BrcID])
						.ThenByDescending(r => r.ExportFields[BOEExporterConstants.FieldName_ResourceID])
						.ThenBy(p => p.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]);
					} 
					else
					{
						orderedResources = laborTaskElement.taskElementLabors
						.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID)
						&& c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgID))
						.OrderBy(o => o.ExportFields[BOEExporterConstants.FieldName_ResourceID])
						.ThenBy(p => p.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]);
					}

                    SdtElement currentInsertionPoint = laborResourceContainerTemplateElement;

                    foreach (BOEExportTaskElementLabor resourceElement in orderedResources)
                    {
                        SdtElement laborResourceContainerElement = this.CloneContainerTemplate(laborResourceContainerTemplateElement);

                        this.ProcessResourceHeader(laborResourceContainerElement, resourceElement);
                        this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
                        this.ProcessResourceHoursRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);
                        this.ProcessResourceCostRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

                        currentInsertionPoint.InsertAfterSelf(laborResourceContainerElement);
                        currentInsertionPoint = laborResourceContainerElement;
                    }

                    this.RemoveElement(laborResourceContainerTemplateElement);
                }
                else
                {
                    this.RemoveElement(laborResourceContainerTemplateElement);
                }
            }

            #endregion
        }

        /// <summary>
        /// Process the Task Resources for an ODC Task
        /// </summary>
        /// <param name="containerElement">Container element for the task</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="odcTaskElement">Element for the odc task</param>
        /// <param name="selectedComponents">Components selected for the output</param>
        /// <exception cref="System.ArgumentNullException">
        /// selectedComponents
        /// or
        /// odcTaskElement
        /// or
        /// exportInputs
        /// </exception>
        protected override void ProcessODCTaskResources(SdtElement containerElement, BOEExportInputs exportInputs, BOEExportTaskElement odcTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (odcTaskElement == null)
            {
                throw new ArgumentNullException(nameof(odcTaskElement));
            }

            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            #region Resource Container

            SdtElement odcResourceContainerTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.ResourceContainerPrefix + BOEExporterConstants.TaskType_ODC);

            if (odcResourceContainerTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.ResourceInfoAndSpreadTables))
                {         
                    ICollection<OtherDirectCostType> ODCTypes = exportInputs.Odcs
                        .Where(t => t.Id == odcTaskElement.BOETaskElementID)
                        .SelectMany(o => o.ODCTypes)
                        .OrderBy(x => x.ResourceID)
                        .ThenBy(y => y.PerformingOrgID)
                        .ToList();
                    
                    HashSet<ResourceDTO> resourcesForOdcs = new HashSet<ResourceDTO>(exportInputs.ResourcesUsedInWsBoes);
                    HashSet<PerformingOrgDTO> performingOrgsFromDb = new HashSet<PerformingOrgDTO>(exportInputs.PerformingOrgsUsedInBoes);
                    SdtElement currentInsertionPoint = odcResourceContainerTemplateElement;

                    foreach (OtherDirectCostType odcType in ODCTypes)
                    {
                        /*
                         * Create a BOEExportTaskElementLabor for each ODC Type. The BOEExportTaskElementLabor 
                         * that exists inodcTaskElement is an aggregate of ODC types, so we can't use them. 
                         * Each individual ODC Type should be printed out for MST.
                        */
                        BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(odcTaskElement);
                        boeExportLabor.Cost = (odcType.SpreadCurve == SpreadCurves.Load) ?
                                                             odcType.Cost.Value * odcType.ODCSpreads.Count :
                                                             odcType.Cost.Value;

                        if (odcType.ResourceID.HasValue)
                        {
                            ResourceDTO resource = resourcesForOdcs.FirstOrDefault(x => x.Id == odcType.ResourceID.Value);
                            if (resource != null)
                            {
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_LaborTypes] = resource.LaborType;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.ResourceDesc;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.ResourceName;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceID] = resource.Id.ToString();
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceRateType] = resource.RateType.ToString();
                            }
                        }

                        if (odcType.PerformingOrgID.HasValue)
                        {
                            PerformingOrgDTO perfOrg = performingOrgsFromDb.FirstOrDefault(x => x.Id == odcType.PerformingOrgID.Value);
                            if (perfOrg != null)
                            {
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID] = perfOrg.Id.ToString();
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrg] = perfOrg.PerformingOrgName;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_PerformingOrgDescription] = perfOrg.PerformingOrgDesc;
                            }
                        }

                        boeExportLabor.StartDate = odcType.StartDate;
                        boeExportLabor.EndDate = odcType.EndDate;


                        SdtElement odcResourceContainerElement = this.CloneContainerTemplate(odcResourceContainerTemplateElement);

                        this.ProcessResourceHeader(odcResourceContainerElement, boeExportLabor);
                        this.ProcessODCResourceCostRollupTable(odcResourceContainerElement, odcTaskElement, odcType);
                        
                        currentInsertionPoint.InsertAfterSelf(odcResourceContainerElement);
                        currentInsertionPoint = odcResourceContainerElement;
                    }
                    this.RemoveElement(odcResourceContainerTemplateElement);
                }
                else
                {
                    this.RemoveElement(odcResourceContainerTemplateElement);
                }
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// Process Travel Task data for the export
        /// </summary>
        /// <param name="boe">BOE DTO for BOE containing Travel Task</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="BOEExportTaskElements">Collection of BOE Export Task Elements for the Travel Tasks</param>
        /// <param name="logEnabled">bool noting if log is enabled</param>
        /// <exception cref="System.ArgumentNullException">
        /// exportInputs
        /// or
        /// BOEExportTaskElements
        /// </exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected override void ProcessTravelTasks(BoeDTO boe, BOEExportInputs exportInputs, Collection<BOEExportTaskElement> BOEExportTaskElements, bool logEnabled)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            if (BOEExportTaskElements == null)
            {
                throw new ArgumentNullException(nameof(BOEExportTaskElements));
            }

            if (logEnabled)
            {
                this._log.Info("Exporting - BOECustomExporter - ProcessTravelTasks - travel tasks begin");
            }

            ICollection<TravelDTO> travelTasks = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();
            ICollection<MSTZoneTravelResourceDTO> zoneTravelResources = new Collection<MSTZoneTravelResourceDTO>();
            ICollection<ResourceDTO> nonzoneTravelResources = new Collection<ResourceDTO>();
            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = new Collection<WorkspaceRMSEscalationRatesDTO>();
            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = new Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO>();

            // only grab the travel resources, rates, and fees if there are travel tasks
            if (travelTasks.Any())
            {
                zoneTravelResources = this.mstZoneTravelResourceDTODataLoader.GetAllResources();
                nonzoneTravelResources = exportInputs.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel).ToCollection();

                //only get if there are nonzone trips, otherwise not needed
                if (travelTasks.Any(x => x.MSTTravelTrips.Any(y => y.ModeID == MSTTravelMode.NonZoneDomestic || y.ModeID == MSTTravelMode.NonZoneInternational)))
                {
                    escalationRates = this.rmsZoneTravelRatesFeesDataLoader.getAllEscalationRatesByWorkspace(exportInputs.Workspace.Id);
                    fees = this.rmsZoneTravelRatesFeesDataLoader.getAllFeesAndCostsByWorkspace(exportInputs.Workspace.Id).ToDictionary(f => f.ModeID);
                }
            }

            foreach (TravelDTO travelElement in travelTasks)
            {
                BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
                boeExportTaskElement.BoeID = boe.Id;
                boeExportTaskElement.BOETaskDesc = travelElement.Description;
                boeExportTaskElement.BOETaskElementID = travelElement.Id;
                boeExportTaskElement.BOETaskID = travelElement.TaskID;
                boeExportTaskElement.TaskTitle = travelElement.TaskTitle;
                boeExportTaskElement.StartDate = travelElement.StartDate.HasValue ? travelElement.StartDate : boe.StartDate;
                boeExportTaskElement.EndDate = travelElement.EndDate.HasValue ? travelElement.EndDate : boe.EndDate;
                boeExportTaskElement.ElementType = BOEExportTaskElementType.Travel;
                boeExportTaskElement.BOETaskElementOrder = travelElement.BOETaskElementOrder;

                // get Task Element Labors
                Collection<BOEExportTaskElementLabor> BoeExportLabors = new Collection<BOEExportTaskElementLabor>();
                
                foreach (MSTTravelTripType travelTrip in travelElement.MSTTravelTrips)
                {
                    BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);

                    decimal people = travelTrip.NumOfPeople ?? 0m;
                    decimal days = (travelTrip.NumOfDays ?? 0m) * people;

                    boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TravelTripID] = travelTrip.Id.ToString();
                    boeExportLabor.ExportFields[BOEExporterConstants.FieldName_GroupID] = travelTrip.GroupID.ToString();
                    boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Mode] = travelTrip.ModeID.ToDescription();
                    boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypePurpose] = travelTrip.Purpose;
                    boeExportLabor.ExportFields[BOEExporterConstants.FieldName_People] = Utilities.FormatStringWithPrecision(people, exportInputs.Workspace.DecimalPrecision);
                    boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Days] = Utilities.FormatStringWithPrecision(days, exportInputs.Workspace.DecimalPrecision);
                    boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TripDate] = boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDate] = travelTrip.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR);

                    if (boe.IsMultiClinWbs)
                    {
                        string wbsString = travelTrip.WbsId == null ? "None" : Utilities.FormatNumberTitleString(travelTrip.WbsNumber, travelTrip.WbsTitle, " - ");
                        string clinString = travelTrip.ClinId == null ? "None" : Utilities.FormatNumberTitleString(travelTrip.ClinNumber, travelTrip.ClinTitle, " - ");
                        boeExportLabor.ExportFields[BOEExporterConstants.FieldName_MultiLabel] = "WBS: " + wbsString + ", CLIN: " + clinString;
                    }

                    if (travelTrip.ModeID == MSTTravelMode.ZoneNoAirfare || travelTrip.ModeID == MSTTravelMode.ZoneAirfare)
                    {
                        if (travelTrip.ModeID == MSTTravelMode.ZoneAirfare)
                        {
                            MSTZoneTravelResourceDTO primaryResource = zoneTravelResources.FirstOrDefault(r => r.Resource == travelTrip.ResourceIdForExport && r.OriginID == travelTrip.ZoneOriginID);
                            if (primaryResource != null)
                            {
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Zone] = primaryResource.Zone.ToString();
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = primaryResource.Resource;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = primaryResource.Description;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = ElementOfCostType.Travel.ToString();
                            }
                            MSTZoneTravelResourceDTO secondaryResource = zoneTravelResources.FirstOrDefault(r => r.Resource == travelTrip.SecondaryResourceIdForExport);
                            if (secondaryResource != null)
                            {
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SecondaryResourceName] = secondaryResource.Resource;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_SecondaryResourceDescription] = secondaryResource.Description;
                            }
                        }
                        else
                        {
                            MSTZoneTravelResourceDTO resource = zoneTravelResources.FirstOrDefault(r => r.ResourceID == travelTrip.ZoneResourceID && r.OriginID == travelTrip.ZoneOriginID);
                            if (resource != null)
                            {
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Zone] = resource.Zone.ToString();
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.Resource;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.Description;
                                boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = ElementOfCostType.Travel.ToString();
                            }
                        }

                        boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDeparture] = travelTrip.ZoneOriginName;
                        boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDestination] = travelTrip.ZoneDestCity + ", " + travelTrip.ZoneDestinationName;
                        boeExportLabor.Cost = 0m;
                    }
                    else if (travelTrip.ModeID == MSTTravelMode.NonZoneDomestic || travelTrip.ModeID == MSTTravelMode.NonZoneInternational)
                    {
                        ResourceDTO resource = nonzoneTravelResources.FirstOrDefault(r => r.Id == travelTrip.NonZoneResourceID);
                        if(resource != null)
                        {
                            boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceName] = resource.ResourceName;
                            boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceDescription] = resource.ResourceDesc;
                            boeExportLabor.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
                        }

                        boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDeparture] = travelTrip.NonZoneFrom;
                        boeExportLabor.ExportFields[BOEExporterConstants.FieldName_TaskTypeDestination] = travelTrip.NonZoneTo;
                        boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Cars] = Utilities.FormatStringWithPrecision((decimal)travelTrip.NonZoneNumCars, exportInputs.Workspace.DecimalPrecision);
                        boeExportLabor.Cost = this.CalculateNonzoneTravelTripCost(exportInputs.Workspace, travelTrip, escalationRates, fees);
                        boeExportLabor.ExportFields[BOEExporterConstants.FieldName_Cost] = boeExportLabor.Cost.Value.ToString(BOEExporterConstants.CURRENCY_FORMAT_DEFAULT, this._CurrencyFormatter);
                    }
                                        
                    BoeExportLabors.Add(boeExportLabor);
                }

                boeExportTaskElement.taskElementLabors = BoeExportLabors;

                boeExportTaskElement.ExportFields[BOEExporterConstants.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString(BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS, this._CurrencyFormatter);

                BOEExportTaskElements.Add(boeExportTaskElement);
            }

            if (logEnabled) { this._log.Info("Exporting - BOECustomExporter - ProcessTravelTasks - travel tasks end"); }
        }

        /// <summary>
        /// Process the data for the Travel Task Resource Table
        /// </summary>
        /// <param name="containerElement">Container element for the table</param>
        /// <param name="travelTaskElement">BOE Export Task Element for the Travel Task</param>
        /// <param name="selectedComponents">Selected components for the custom export</param>
        protected override void ProcessTravelTaskResourceTable(SdtElement containerElement, BOEExportTaskElement travelTaskElement, ICollection<BoeCustomReportComponent> selectedComponents)
        {
            if (travelTaskElement == null) { throw new ArgumentNullException(nameof(travelTaskElement)); }
            if (selectedComponents == null) { throw new ArgumentNullException(nameof(selectedComponents)); }

            #region Resource Types Table

            //table elements
            SdtElement resourceTypesTableElement_Zone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes_ZoneTravel);
            SdtElement resourceTypesTableElement_Nonzone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceTypes_NonzoneTravel);

            //table label elements
            SdtElement resourceTypesTableLabelElement_Zone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_ZoneSummaryLabel);
            SdtElement resourceTypesTableLabelElement_Nonzone = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.FieldName_NonzoneSummaryLabel);

            bool isMulti = travelTaskElement.taskElementLabors.Any(x => x.ExportFields.ContainsKey(BOEExporterConstants.FieldName_MultiLabel));

            if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable) && resourceTypesTableElement_Zone != null)
            {
                Collection<BOEExportTaskElementLabor> travelTaskElementLabors = travelTaskElement.taskElementLabors.Where(x => x.ExportFields.ContainsValue(MSTTravelMode.ZoneNoAirfare.GetDescription()) || x.ExportFields.ContainsValue(MSTTravelMode.ZoneAirfare.GetDescription())).ToCollection();
                if (isMulti)
                {
                    this.populateTravelTripsTableForMultiBOEs(resourceTypesTableElement_Zone, resourceTypesTableLabelElement_Zone, travelTaskElementLabors, true);
                }
                else
                {
                    this.RemoveElement(WordUtilities.GetTaggedChildElement(resourceTypesTableElement_Zone, BOEExporterConstants.FieldName_MultiLabel));
                    RMSTravelResourceTypesTableData travelTableData = travelTaskElementLabors.ConvertRMSTravel();
                    this.PopulateTravelTripsTable(resourceTypesTableElement_Zone, resourceTypesTableLabelElement_Zone, travelTableData, true);
                }
            }
            else 
            {
                this.RemoveElement(resourceTypesTableLabelElement_Zone);
                this.RemoveElement(resourceTypesTableElement_Zone);
            }
            
            if (selectedComponents.Contains(BoeCustomReportComponent.TaskResourceTypesSummaryTable) && resourceTypesTableElement_Nonzone != null)
            {
                Collection<BOEExportTaskElementLabor> travelTaskElementLabors = travelTaskElement.taskElementLabors.Where(x => x.ExportFields.ContainsValue(MSTTravelMode.NonZoneDomestic.GetDescription()) || x.ExportFields.ContainsValue(MSTTravelMode.NonZoneInternational.GetDescription())).ToCollection();
                if (isMulti)
                {
                    this.populateTravelTripsTableForMultiBOEs(resourceTypesTableElement_Nonzone, resourceTypesTableLabelElement_Nonzone, travelTaskElementLabors, false);
                }
                else
                {
                    this.RemoveElement(WordUtilities.GetTaggedChildElement(resourceTypesTableElement_Nonzone, BOEExporterConstants.FieldName_MultiLabel));
                    RMSTravelResourceTypesTableData travelTableData = travelTaskElementLabors.ConvertRMSTravel();
                    this.PopulateTravelTripsTable(resourceTypesTableElement_Nonzone, resourceTypesTableLabelElement_Nonzone, travelTableData, false);
                }
            }
            else
            {
                this.RemoveElement(resourceTypesTableLabelElement_Nonzone);
                this.RemoveElement(resourceTypesTableElement_Nonzone);
            }

            #endregion
        }

        /// <summary>
        /// Process the Travel Task Direct Cost Rollup Table before populating it
        /// </summary>
        /// <param name="containerElement">Container element for task rollup table</param>
        /// <param name="travelTaskElement">Travel task element data</param>
        /// <param name="travelResources">Resources used by the travel task</param>
        /// <param name="selectedComponents">Components selected for the output</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="useGfy">use government fiscal year?</param>
        /// <exception cref="System.ArgumentNullException">
        /// selectedComponents
        /// or
        /// exportInputs
        /// </exception>
        protected override void ProcessTravelTaskDirectCostRollupTable(SdtElement containerElement, BOEExportTaskElement travelTaskElement, Collection<ResourceDTO> travelResources, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            #region Travel Direct Cost Rollup Table

            SdtElement travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, 
                useGfy ? BOEExporterConstants.Table_GfyDirectCostRollup : BOEExporterConstants.Table_DirectCostRollup);

            bool byQuarter = false;
            if (useGfy && travelCostRollupTableElement == null)
            {
                travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
                byQuarter = true;
            }

            if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables) && travelCostRollupTableElement != null)
            {
                TravelDTO currentTravelTaskElement = exportInputs.Travels.First(x => x.Id == travelTaskElement.BOETaskElementID.Value);
                List<LaborRollupByDateNew> currentTravelTaskRollupData = this.GetNonzoneTravelCostRollup(currentTravelTaskElement, travelTaskElement, useGfy);
                RollupSummaryByYearTableData travelSummaryRollupData = currentTravelTaskRollupData.ConvertToRollupSummaryByYear();
                this.PopulateRollupSummaryByYearTable(travelCostRollupTableElement, null, travelSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
            }
            else
            {
                this.RemoveElement(travelCostRollupTableElement);
            }

            #endregion
        }

        /// <summary>
        /// Remove MOQ related containers only used when Template BOE is set to "No"
        /// For RMS, this is the MOQ Type Container, but only if there are no MOQ RTE Templates
        /// </summary>
        /// <param name="wsHasMoqRteTemplate">Whether Worksace has RTE Templates for MOQ Rationale</param>
        /// <param name="containerElement">The container template</param>
        protected override void RemoveNonTemplateBoeContainers(bool wsHasMoqRteTemplate, SdtElement containerElement)
        {
            if (!wsHasMoqRteTemplate)
            {
                WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_MOQTypeContainer);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the cost rollup for an ODC task
        /// </summary>
        /// <param name="ODCElement">DTO for the ODC task</param>
        /// <param name="useGfy">Use Govt Fiscal Year</param>
        /// <returns>Cost rollup for an ODC task</returns>
        private List<LaborRollupByDateNew> GetODCCostRollup(OtherDirectCostDTO ODCElement, bool useGfy)
        {
            List<LaborRollupByDateNew> RollupList = new List<LaborRollupByDateNew>();
            DateRange ODCDateRange = this.GetODCTravelDateRange(new Collection<OtherDirectCostDTO> { ODCElement }, null, useGfy);
            if (ODCDateRange.StartDate.HasValue && ODCDateRange.EndDate.HasValue)
            {
                ICollection<int> odcResourceIds = ODCElement.ODCTypes.Where(t => t.ResourceID.HasValue).Select(t => t.ResourceID.Value).Distinct().ToList();

                for (int i = ODCDateRange.StartDate.Value.Year; i <= ODCDateRange.EndDate.Value.Year; i++)
                {
                    LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
                    Rollup.Resource = "";
                    Rollup.Year = i;

                    foreach(int resourceId in odcResourceIds)
                    {
                        ICollection<OtherDirectCostSpread> odcSpreads = ODCElement.ODCTypes.Where(t => t.ResourceID == resourceId).SelectMany(t => t.ODCSpreads).ToList().DeepClone();

                        if(useGfy)
                        {
                            foreach(OtherDirectCostSpread spread in odcSpreads.Where(x => x.ODCSpreadDate.HasValue))
                            {
                                spread.ODCSpreadDate = this.AdjustDateForGovtFiscalYear(spread.ODCSpreadDate.Value);
                            }
                        }

                        Rollup.January += this.GetRollupForMonth(odcSpreads, i, 1);
                        Rollup.February += this.GetRollupForMonth(odcSpreads, i, 2);
                        Rollup.March += this.GetRollupForMonth(odcSpreads, i, 3); 
                        Rollup.April += this.GetRollupForMonth(odcSpreads, i, 4); 
                        Rollup.May += this.GetRollupForMonth(odcSpreads, i, 5); 
                        Rollup.June += this.GetRollupForMonth(odcSpreads, i, 6); 
                        Rollup.July += this.GetRollupForMonth(odcSpreads, i, 7);
                        Rollup.August += this.GetRollupForMonth(odcSpreads, i, 8); 
                        Rollup.September += this.GetRollupForMonth(odcSpreads, i, 9);
                        Rollup.October += this.GetRollupForMonth(odcSpreads, i, 10); 
                        Rollup.November += this.GetRollupForMonth(odcSpreads, i, 11);
                        Rollup.December += this.GetRollupForMonth(odcSpreads, i, 12);
                    }
                    RollupList.Add(Rollup);
                }
            }
            return RollupList;
        }

        /// <summary>
        /// Process and populate the resource header with appropriate data
        /// </summary>
        /// <param name="containerElement">Container element for the resource</param>
        /// <param name="ResourceElement">Resource with data to populate the header</param>
        private void ProcessResourceHeader(SdtElement containerElement, BOEExportTaskElementLabor ResourceElement)
        {
            IDictionary<string, string> resourceHeaderDataValueMappings = new Dictionary<string, string>
            {
                {BOEExporterConstants.FieldName_ResourceStartDate, ResourceElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR)},
                {BOEExporterConstants.FieldName_ResourceEndDate, ResourceElement.EndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR)}
            };

            if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceDescription))
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceDescription]);
            }
            else
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, "NO JOB CODE");
            }

            if(ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrg) && ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgDescription))
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_PerformingOrg, ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrg] + " - " + ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgDescription]);
            }
            else
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, "NO PERF ORG");
            }

            foreach (KeyValuePair<string, string> entry in resourceHeaderDataValueMappings)
            {
                SdtElement headerDataElement = WordUtilities.GetTaggedChildElement(containerElement, entry.Key);
                WordUtilities.SetElementText(headerDataElement, entry.Value);
            }
        }
        
        /// <summary>
        /// Process and populate the Resource Hours Rollup Table
        /// </summary>
        /// <param name="containerElement">Container element for the resource</param>
        /// <param name="ResourceElement">Resource with the data to populate the table</param>
        /// <param name="allLaborTaskElements">All labor task elements in the workspace</param>
        /// <param name="currentLaborTaskElement">Labor task element containing the resource</param>
        private void ProcessResourceHoursRollupTable(SdtElement containerElement, BOEExportTaskElementLabor ResourceElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportTaskElement currentLaborTaskElement)
        {
            SdtElement resourceHoursTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceHoursRollup);

            if (resourceHoursTableElement != null)
            {
                BoeTaskElementDTO currentLaborTaskDto = allLaborTaskElements.FirstOrDefault(x => x.Id == currentLaborTaskElement.BOETaskElementID);
                if (currentLaborTaskDto != null)
                {
                    ResourceTypeDto currentResourceTypeDTO = currentLaborTaskDto.taskElementLabors
                        .FirstOrDefault(r => r.ResourceID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceID]
                            && r.Id.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_LaborTypeID]
                            && r.PerformingOrgID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]
                            && r.StartDate == ResourceElement.StartDate && r.EndDate == ResourceElement.EndDate);
                    
                    if (currentResourceTypeDTO != null && currentResourceTypeDTO.SpreadType.Equals(SpreadType.Hours))
                    {
                        List<LaborRollupByDateNew> rollupData = this.GetResourceRollupByYear(currentResourceTypeDTO, ResourceElement).ToList();
                        IList<RollupSummaryByYearTableRowData> hoursSummaryRollupData = rollupData.Convert();

                        RollupSummaryByYearTableData rollupTableData = new RollupSummaryByYearTableData
                        {
                            SummaryTotalComplete = hoursSummaryRollupData.Sum(s => s.YearTotal),
                            YearlyData = hoursSummaryRollupData
                        };

                        this.PopulateRollupSummaryByYearTable(resourceHoursTableElement, null, rollupTableData, this.DefaultHoursFormat, false);
                    }
                    else
                    {
                        this.RemoveElement(resourceHoursTableElement);
                    }
                }
                else
                {
                    this.RemoveElement(resourceHoursTableElement);
                }
            }
        }

        /// <summary>
        /// Process and populate the Resource Cost Rollup table
        /// </summary>
        /// <param name="containerElement">Container elment for the resource</param>
        /// <param name="ResourceElement">Resource with the data to populate the table</param>
        /// <param name="allLaborTaskElements">All labor task elements in the workspace</param>
        /// <param name="currentLaborTaskElement">Labor task element containing the resource</param>
        private void ProcessResourceCostRollupTable(SdtElement containerElement, BOEExportTaskElementLabor ResourceElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportTaskElement currentLaborTaskElement)
        {
            SdtElement resourceCostTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceCostRollup);

            if (resourceCostTableElement != null)
            {                
                BoeTaskElementDTO currentLaborTaskDto = allLaborTaskElements.FirstOrDefault(x => x.Id == currentLaborTaskElement.BOETaskElementID);
                if (currentLaborTaskDto != null)
                {
                    ResourceTypeDto currentResourceTypeDTO = currentLaborTaskDto.taskElementLabors
                            .FirstOrDefault(r => r.ResourceID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceID]
                            && r.Id.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_LaborTypeID]
                                && r.PerformingOrgID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]
                                && r.StartDate == ResourceElement.StartDate && r.EndDate == ResourceElement.EndDate);
                    
                    if (currentResourceTypeDTO != null && currentResourceTypeDTO.SpreadType.Equals(SpreadType.Cost))
                    {
                        List<LaborRollupByDateNew> rollupData = this.GetResourceRollupByYear(currentResourceTypeDTO, ResourceElement).ToList();
                        IList<RollupSummaryByYearTableRowData> costSummaryRollupData = rollupData.Convert();

                        RollupSummaryByYearTableData rollupTableData = new RollupSummaryByYearTableData
                        {
                            SummaryTotalComplete = costSummaryRollupData.Sum(s => s.YearTotal),
                            YearlyData = costSummaryRollupData
                        };

                        this.PopulateRollupSummaryByYearTable(resourceCostTableElement, null, rollupTableData, this.DefaultCurrencyFormat, false);
                    }
                    else
                    {
                        this.RemoveElement(resourceCostTableElement);
                    }
                }
                else
                {
                    this.RemoveElement(resourceCostTableElement);
                }
            }
        }

        /// <summary>
        /// Calculates the total cost for a Nonzone Travel Trip
        /// </summary>
        /// <param name="trip">Trip to calculate costs for</param>
        /// <param name="escalationRates">Collection of airfare escalation rates for the workspace</param>
        /// <param name="fees">Dictionary of all Travel Agency Fees and Misc/Other Costs for the Workspace</param>
        /// <returns>
        /// Total cost for the Trip
        /// </returns>
        private decimal CalculateNonzoneTravelTripCost(WorkspaceDTO workspace, MSTTravelTripType trip, ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates,
            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees)
        {
            Dictionary<int, decimal> airfareEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.AirfareRate);
            Dictionary<int, decimal> perDiemEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.PerDiemRate);
            Dictionary<int, decimal> miscEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.MiscRate);

            NonZoneTravelCalculation calculationClass = new NonZoneTravelCalculation(trip.NumOfDays ?? 0, trip.NumOfPeople ?? 0, trip.NonZoneNumCars ?? 0, trip.NonZonePerDiemDaily ?? 0,
                trip.NonZoneCarRentalTrans ?? 0, trip.NonZoneAirfareEstimate ?? 0, airfareEscalationRates, perDiemEscalationRates, miscEscalationRates,
                trip.EstimateDate.Year, trip.TripDate.Year, trip.ModeID == MSTTravelMode.NonZoneDomestic,
                fees[(int)trip.ModeID].TravelAgencyFee, fees[(int)trip.ModeID].MiscOther, workspace.CostDecimalPrecision);

            return calculationClass.TotalTripCost;
        }

        /// <summary>
        /// Populates the Travel Trips Tables
        /// </summary>
        /// <param name="tableContainerElement">Container element for the table</param>
        /// <param name="tableLabelElement">Container element for the table label</param>
        /// <param name="data">data to populate the table with</param>
        /// <param name="isZone">bool noting if trip is zone (true) or nonzone (false)</param>
        private void PopulateTravelTripsTable(SdtElement tableContainerElement, SdtElement tableLabelElement, RMSTravelResourceTypesTableData data, bool isZone)
        {
            if (data.ResourcesData.Any())
            {
                if (isZone)
                {
                    this.PopulateZoneTravelTripsTable(tableContainerElement, data.ResourcesData);
                }
                else
                {
                    this.PopulateNonzoneTravelTripsTable(tableContainerElement, data.ResourcesData);
                }
            }
            else
            {
                this.RemoveElement(tableLabelElement);
                this.RemoveElement(tableContainerElement);
            }
        }

        /// <summary>
        /// Populates the Zone Travel Trips table
        /// </summary>
        /// <param name="tableContainerElement">Container element for the table</param>
        /// <param name="resourcesData">The resources data.</param>
        private void PopulateZoneTravelTripsTable(SdtElement tableContainerElement, ICollection<RMSTravelResourceTypesTableRowData> resourcesData)
        {
            if (resourcesData.Any())
            {
                // different columns for both data and totals rows - logic will check for existence of tags to determine how to populate

                // locate the table markers
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);

                // initialize the "insertion" row
                TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
                this.SetCantSplit(templateDataRow);
                TableRow currentInsertionRow = templateDataRow;

                foreach (RMSTravelResourceTypesTableRowData rowData in resourcesData)
                {
                    // create a new data row in the table
                    TableRow tableRow = this.CloneMarkedTemplateRow(templateDataRow);

                    // populate the row with the data
                    this.PopulateZoneTravelTripsTableRow(tableRow, rowData, false);

                    // add the row to the table
                    currentInsertionRow.InsertAfterSelf(tableRow);
                    currentInsertionRow = tableRow;

                    // Add second row for airfare resource for airfare trips
                    if(rowData.IsAirfareTrip)
                    {
                        tableRow = this.CloneMarkedTemplateRow(templateDataRow);
                        this.PopulateZoneTravelTripsTableRow(tableRow, rowData, true);
                        currentInsertionRow.InsertAfterSelf(tableRow);
                        currentInsertionRow = tableRow;
                    }
                }

                // remove template rows
                templateDataRow.Remove();
            }
            else
            {
                this.RemoveElement(tableContainerElement);
            }
        }

        /// <summary>
        /// Populate a row of the Zone Travel Trips Table
        /// </summary>
        /// <param name="tableRow">Row to be populated</param>
        /// <param name="rowData">Data to populate the row with</param>
        /// <param name="isAirfareResource">Bool noting if resource is Airfare (true) or Per Diem (false)</param>
        private void PopulateZoneTravelTripsTableRow (TableRow tableRow, RMSTravelResourceTypesTableRowData rowData, bool isAirfareResource)
        {
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_GroupID), rowData.GroupID);
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), isAirfareResource ? rowData.SecondaryResourceName : rowData.ResourceName);
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Departure), rowData.Departure);
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Destination), rowData.Destination);
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Zone), rowData.Zone);
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Purpose), rowData.Purpose);
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TripDate), rowData.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
            // Empty for per diem resource in airfare trip
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_People), rowData.People.ToString());
            // Empty for airfare resource in airfare trip
            WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Days), isAirfareResource ? string.Empty : rowData.Days.ToString());
        }

        /// <summary>
        /// Populates the Nonzone Travel Trips table
        /// </summary>
        /// <param name="tableContainerElement">Container element for the table</param>
        /// <param name="resourcesData">The resources data.</param>
        private void PopulateNonzoneTravelTripsTable(SdtElement tableContainerElement, ICollection<RMSTravelResourceTypesTableRowData> resourcesData)
        {
            if (resourcesData.Any())
            {
                // different columns for both data and totals rows - logic will check for existence of tags to determine how to populate

                // locate the table markers
                SdtElement dataRowMarkerTag = WordUtilities.GetTaggedChildElement(tableContainerElement, BOEExporterConstants.Marker_DataRow);

                // initialize the "insertion" row
                TableRow templateDataRow = dataRowMarkerTag.Ancestors<TableRow>().FirstOrDefault();
                this.SetCantSplit(templateDataRow);
                TableRow currentInsertionRow = templateDataRow;

                foreach (RMSTravelResourceTypesTableRowData rowData in resourcesData)
                {
                    // create a new data row in the table
                    TableRow tableRow = this.CloneMarkedTemplateRow(templateDataRow);

                    // populate the row with the data
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_GroupID), rowData.GroupID);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_ResourceName), rowData.ResourceName);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Departure), rowData.Departure);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Destination), rowData.Destination);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Purpose), rowData.Purpose);
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_TripDate), rowData.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR));
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_People), rowData.People.ToString());
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Days), rowData.Days.ToString());
                    WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(tableRow, BOEExporterConstants.FieldName_Cars), rowData.Cars.ToString());

                    // add the row to the table
                    currentInsertionRow.InsertAfterSelf(tableRow);
                    currentInsertionRow = tableRow;
                }

                // remove template rows
                templateDataRow.Remove();
            }
            else
            {
                this.RemoveElement(tableContainerElement);
            }
        }

        /// <summary>
        /// Populates Travel Trips Tables for each WBS/CLIN combination in the Task
        /// </summary>
        /// <param name="tableElement">Sdt Element for the table (either zone or nonzone summary table)</param>
        /// <param name="tableLabelElement">Container element for the table label</param>
        /// <param name="travelTaskElementLabors">BOE Export Task Element for the Travel Task Labors (trips), filtered for zone or nonzone trips</param>
        /// <param name="isZone">bool noting is trip is zone (true) or nonzone (false)</param>
        private void populateTravelTripsTableForMultiBOEs(SdtElement tableElement, SdtElement tableLabelElement, Collection<BOEExportTaskElementLabor> travelTaskElementLabors, bool isZone)
        {
            SdtElement currentTable = tableElement;
			IEnumerable<IGrouping<string, BOEExportTaskElementLabor>> multiGroups = travelTaskElementLabors.GroupBy(x => x.ExportFields[BOEExporterConstants.FieldName_MultiLabel]);
            foreach (IGrouping<string, BOEExportTaskElementLabor> group in multiGroups)
            {
                SdtElement clonedTable = tableElement.CloneNode(true) as SdtElement;
                WordUtilities.SetElementText(WordUtilities.GetTaggedChildElement(clonedTable, BOEExporterConstants.FieldName_MultiLabel), group.First().ExportFields[BOEExporterConstants.FieldName_MultiLabel]);
                RMSTravelResourceTypesTableData travelTableData = group.ToCollection().ConvertRMSTravel();
                this.PopulateTravelTripsTable(clonedTable, tableLabelElement, travelTableData, isZone);
                if (travelTableData.ResourcesData.Any())
                {
                    currentTable.InsertAfterSelf(clonedTable);
                    currentTable = clonedTable;
                }
            }
            this.RemoveElement(tableElement);
        }

        /// <summary>
        /// Gets the nonzone travel cost rollup data
        /// </summary>
        /// <param name="TravelElement">Travel dto containing the trips to rollup</param>
        /// <param name="element">BOE Export Task Element for the Travel Task</param>
        /// <param name="useGfy">Use Govt Fiscal Year</param>
        /// <returns>The cost rollup data for the nonzone trips in the task</returns>
        private List<LaborRollupByDateNew> GetNonzoneTravelCostRollup(TravelDTO TravelElement, BOEExportTaskElement element, bool useGfy)
        {
            List<LaborRollupByDateNew> toReturn = new List<LaborRollupByDateNew>();

            DateRange TravelElementsDateRange = this.GetODCTravelDateRange(null, new Collection<TravelDTO> { TravelElement }, false);
            if (TravelElementsDateRange.StartDate.HasValue && TravelElementsDateRange.EndDate.HasValue)
            {
                ICollection<MSTTravelTripType> TravelTrips = TravelElement.MSTTravelTrips.DeepClone();

                if (useGfy)
                {
                    foreach(MSTTravelTripType trip in TravelTrips)
                    {
                        trip.TripDate = this.AdjustDateForGovtFiscalYear(trip.TripDate);
                    }
                }
                               
                for (int i = TravelElementsDateRange.StartDate.Value.Year; i <= TravelElementsDateRange.EndDate.Value.Year; i++)
                {
                    LaborRollupByDateNew Rollup = new LaborRollupByDateNew();
                    Rollup.Year = i;

                    Rollup.January = this.GetRollupForMonth(TravelTrips, element, i, 1);
                    Rollup.February = this.GetRollupForMonth(TravelTrips, element, i, 2);
                    Rollup.March = this.GetRollupForMonth(TravelTrips, element, i, 3);
                    Rollup.April = this.GetRollupForMonth(TravelTrips, element, i, 4);
                    Rollup.May = this.GetRollupForMonth(TravelTrips, element, i, 5);
                    Rollup.June = this.GetRollupForMonth(TravelTrips, element, i, 6);
                    Rollup.July = this.GetRollupForMonth(TravelTrips, element, i, 7);
                    Rollup.August = this.GetRollupForMonth(TravelTrips, element, i, 8);
                    Rollup.September = this.GetRollupForMonth(TravelTrips, element, i, 9);
                    Rollup.October = this.GetRollupForMonth(TravelTrips, element, i, 10);
                    Rollup.November = this.GetRollupForMonth(TravelTrips, element, i, 11);
                    Rollup.December = this.GetRollupForMonth(TravelTrips, element, i, 12);

                    toReturn.Add(Rollup);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Calculates the sum of the nonzone travel costs for the given month
        /// </summary>
        /// <param name="travelTrips">Collection of Trips for the Travel Task</param>
        /// <param name="travelExportElement">BOE Export Task Element for the Travel Task</param>
        /// <param name="year">Year of costs to rollup</param>
        /// <param name="month">Month of costs to roll up</param>
        /// <returns>The sum of the nonzone trip costs for the month</returns>
        private decimal GetRollupForMonth(ICollection<MSTTravelTripType> travelTrips, BOEExportTaskElement travelExportElement, int year, int month)
        {
            decimal sum =
                (from e in travelTrips
                 where e.TripDate.Year == year && e.TripDate.Month == month
                 select travelExportElement.taskElementLabors.First(f => Int32.Parse(f.ExportFields[BOEExporterConstants.FieldName_TravelTripID]) == e.Id).Cost.Value).Sum();

            return sum;
        }

        #endregion
    }
}