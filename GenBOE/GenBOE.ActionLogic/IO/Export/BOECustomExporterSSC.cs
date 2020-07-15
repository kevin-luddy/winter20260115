// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DocumentFormat.OpenXml.Wordprocessing;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public class BOECustomExporterSSC : BOECustomExporter
    {
        public BOECustomExporterSSC
            (
                IUserDTODataLoader inIUserDTODataLoader,
                ICommonDataMapper inICommonDataMapper,
                TravelTripCostCalculation inTravelTripCostCalculation,
                IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation
            ) : base(
                inIUserDTODataLoader,
                inICommonDataMapper,
                inTravelTripCostCalculation,
                inVariableSelectBOEtoSumCalculation
            )
        {
            // constructor
            this.DefaultCurrencyFormat = BOEExporterConstants.CURRENCY_FORMAT_NO_DECIMALS;
        }

        #region Method overrides

        #region Resource summary

        protected override void PopulateResourceSummaryByElementOfCostTable(SdtElement tableContainerElement, BOEExportModelView boeExportModelView, ICollection<BOESummaryGridModelView> data)
        {
            if (boeExportModelView == null)
            {
                throw new ArgumentNullException(nameof(boeExportModelView));
            }

            ICollection<ResourceSummaryRowData> resourceData = boeExportModelView.TaskElements
                .SelectMany(t => t.taskElementLabors)
                .Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceName) && c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceElementOfCost))                
                .Select(r => new ResourceSummaryRowData
                {
                    ResourceType = r.ExportFields[BOEExporterConstants.FieldName_ResourceElementOfCost],
                    ResourceName = r.ExportFields[BOEExporterConstants.FieldName_ResourceName],
                    ResourceDescription = r.ExportFields[BOEExporterConstants.FieldName_ResourceDescription],
                    CostTotal = r.Cost.HasValue ? r.Cost.Value : 0m,
                    HoursTotal = r.Hours.HasValue ? r.Hours.Value : 0m
                }).ToList();

            if (resourceData.Any())
            {
                // derive the rollup data
                ICollection<ResourceSummaryRowData> rollupData =
                    resourceData
                        .GroupBy(x => x.GroupKey)
                        .Select(g => new ResourceSummaryRowData
                        {
                            ResourceType = g.First().ResourceType,
                            ResourceName = g.First().ResourceName,
                            ResourceDescription = g.First().ResourceDescription,
                            CostTotal = g.Sum(x => x.CostTotal),
                            HoursTotal = g.Sum(x => x.HoursTotal)
                        })
                        .OrderBy(x => x.GroupKey).ToList();

                this.PopulateResourceSummaryTable(tableContainerElement, rollupData);
            }
            else
            {
                this.RemoveElement(tableContainerElement);
            }
        }

        #endregion

        #region Spread rollup tables

        /// <summary>
        /// Process the Labor Task Hours Rollup Table before populating it
        /// </summary>
        /// <param name="containerElement">Container element for task</param>
        /// <param name="laborTaskElement">Element for the labor task</param>
        /// <param name="allLaborTaskElements">All task elements for the BOE</param>
        /// <param name="selectedComponents">Components selected for the output</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelView">The boe export model view.</param>
        /// <param name="useGfy">Should Government Fiscal Years be used</param>
        /// <exception cref="System.ArgumentNullException">
        /// selectedComponents
        /// or
        /// exportInputs
        /// </exception>
        protected override void ProcessLaborTaskHoursRollupTable(SdtElement containerElement, BOEExportTaskElement laborTaskElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, BOEExportModelView boeExportModelView, bool useGfy)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }
            
            #region Labor Hours Rollup Table

            bool byQuarter = false;
            SdtElement laborHoursRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_LaborHoursRollup); ;
            SdtElement gfyLaborHoursRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyLaborHoursRollup);

            if (laborHoursRollupTableTemplateElement == null)
            {
                laborHoursRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_LaborHoursRollupByYear);
                if (laborHoursRollupTableTemplateElement == null)
                {
                    laborHoursRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_LaborHoursRollupByQuarter);
                    if (laborHoursRollupTableTemplateElement != null)
                    {
                        byQuarter = true;
                    }
                }
            }

            if (gfyLaborHoursRollupTableTemplateElement == null)
            {
                gfyLaborHoursRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyLaborHoursRollupByQuarter);

                if (gfyLaborHoursRollupTableTemplateElement != null)
                {
                    byQuarter = true;
                }
            }
            
            if (laborHoursRollupTableTemplateElement != null || gfyLaborHoursRollupTableTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
                {
                    string hoursLabel = FullObjectHelper.HoursLabel(exportInputs.Workspace);
                    string tag = laborHoursRollupTableTemplateElement != null ? laborHoursRollupTableTemplateElement.Descendants<Tag>().FirstOrDefault().Val?.Value : string.Empty;

                    BoeTaskElementDTO currentLaborTaskElement = allLaborTaskElements.FirstOrDefault(x => x.Id == laborTaskElement.BOETaskElementID.Value);

                    SdtElement currentInsertionElement = laborHoursRollupTableTemplateElement != null ? laborHoursRollupTableTemplateElement : gfyLaborHoursRollupTableTemplateElement;

                    #region Repeat for each element of cost

                    // if description is present, then it should be used instead of resource id.
                    bool useDescriptionInsteadOfName = laborHoursRollupTableTemplateElement != null ? WordUtilities.GetTaggedChildElement(WordUtilities.GetTaggedChildElement(currentInsertionElement, BOEExporterConstants.Marker_DataRow)
                                                                                            .Ancestors<TableRow>().FirstOrDefault(), BOEExporterConstants.FieldName_ResourceDescription) 
                                                                                            != null : false;

                    Dictionary<int, List<LaborRollupByDateNew>> currentLMLaborTaskRollupData = this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useDescriptionInsteadOfName, false, ElementOfCostType.LMLabor, null);
                    SdtElement lmLaborHoursRollupTableElement = laborHoursRollupTableTemplateElement != null ? laborHoursRollupTableTemplateElement.CloneNode(true) as SdtElement : null;
                    bool populated = false;

                    if (tag == BOEExporterConstants.Table_LaborHoursRollupByYear)
                    {
                        RollupSummaryByYearTableData lmLaborSummaryRollupData = currentLMLaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(lmLaborHoursRollupTableElement, "LM Labor " + hoursLabel + " Spread", lmLaborSummaryRollupData, this.DefaultHoursFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData lmLaborSummaryRollupData = currentLMLaborTaskRollupData.Convert();
                        populated = this.PopulateRollupSummaryByGroupByYearTable(lmLaborHoursRollupTableElement, "LM Labor " + hoursLabel + " Spread", lmLaborSummaryRollupData, this.DefaultHoursFormat, byQuarter);
                    }

                    if (populated)
                    {
                        currentInsertionElement.InsertAfterSelf(lmLaborHoursRollupTableElement);
                        currentInsertionElement = lmLaborHoursRollupTableElement;
                    }

                    if(gfyLaborHoursRollupTableTemplateElement != null)
                    {
                        Dictionary<int, List<LaborRollupByDateNew>> gfyCurrentLMLaborTaskRollupData = this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useDescriptionInsteadOfName, true, ElementOfCostType.LMLabor, null);
                        SdtElement gfyLaborHoursRollupTableElement = gfyLaborHoursRollupTableTemplateElement.CloneNode(true) as SdtElement;
                        RollupSummaryByGroupByYearTableData gfyLmLaborSummaryRollupData = gfyCurrentLMLaborTaskRollupData.Convert();
                        string tableTitle = "LM Labor " + hoursLabel + " Spread" + (byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(gfyLaborHoursRollupTableElement, tableTitle, gfyLmLaborSummaryRollupData, this.DefaultHoursFormat, byQuarter, true);
                        if (populated)
                        {
                            currentInsertionElement.InsertAfterSelf(gfyLaborHoursRollupTableElement);
                            currentInsertionElement = gfyLaborHoursRollupTableElement;
                        }
                    }

                    Dictionary<int, List<LaborRollupByDateNew>> currentSubLaborTaskRollupData = this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useDescriptionInsteadOfName, false, ElementOfCostType.Sub, null);
                    SdtElement subLaborHoursRollupTableElement = laborHoursRollupTableTemplateElement != null ? laborHoursRollupTableTemplateElement.CloneNode(true) as SdtElement : null;
                    populated = false;

                    if (tag == BOEExporterConstants.Table_LaborHoursRollupByYear)
                    {
                        RollupSummaryByYearTableData subLaborSummaryRollupData = currentSubLaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(subLaborHoursRollupTableElement, "Subcontractor Labor " + hoursLabel + " Spread", subLaborSummaryRollupData, this.DefaultHoursFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData subLaborSummaryRollupData = currentSubLaborTaskRollupData.Convert();
                        populated = this.PopulateRollupSummaryByGroupByYearTable(subLaborHoursRollupTableElement, "Subcontractor Labor " + hoursLabel + " Spread", subLaborSummaryRollupData, this.DefaultHoursFormat, byQuarter);
                    }

                    if (populated)
                    {
                        currentInsertionElement.InsertAfterSelf(subLaborHoursRollupTableElement);
                        currentInsertionElement = subLaborHoursRollupTableElement;
                    }

                    if (gfyLaborHoursRollupTableTemplateElement != null)
                    {
                        Dictionary<int, List<LaborRollupByDateNew>> gfyCurrentSubLaborTaskRollupData = this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useDescriptionInsteadOfName, true, ElementOfCostType.Sub, null);
                        SdtElement gfySubHoursRollupTableElement = gfyLaborHoursRollupTableTemplateElement.CloneNode(true) as SdtElement;
                        RollupSummaryByGroupByYearTableData gfySubLaborSummaryRollupData = gfyCurrentSubLaborTaskRollupData.Convert();
                        string tableTitle = "Subcontractor Labor " + hoursLabel + " Spread" + (byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(gfySubHoursRollupTableElement, tableTitle, gfySubLaborSummaryRollupData, this.DefaultHoursFormat, byQuarter, true);
                        if (populated)
                        {
                            currentInsertionElement.InsertAfterSelf(gfySubHoursRollupTableElement);
                            currentInsertionElement = gfySubHoursRollupTableElement;
                        }
                    }

                    Dictionary<int, List<LaborRollupByDateNew>> currentIWTALaborTaskRollupData = this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useDescriptionInsteadOfName, false, ElementOfCostType.IWTA, null);    
                    SdtElement iwtaLaborHoursRollupTableElement = laborHoursRollupTableTemplateElement != null ? laborHoursRollupTableTemplateElement.CloneNode(true) as SdtElement : null;
                    populated = false;

                    if (tag == BOEExporterConstants.Table_LaborHoursRollupByYear)
                    {
                        RollupSummaryByYearTableData iwtaLaborSummaryRollupData = currentIWTALaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(iwtaLaborHoursRollupTableElement, "IWTA Labor " + hoursLabel + " Spread", iwtaLaborSummaryRollupData, this.DefaultHoursFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData iwtaLaborSummaryRollupData = currentIWTALaborTaskRollupData.Convert();
                        populated = this.PopulateRollupSummaryByGroupByYearTable(iwtaLaborHoursRollupTableElement, "IWTA Labor " + hoursLabel + " Spread", iwtaLaborSummaryRollupData, this.DefaultHoursFormat, byQuarter);
                    }

                    if (populated)
                    {
                        currentInsertionElement.InsertAfterSelf(iwtaLaborHoursRollupTableElement);
                        currentInsertionElement = iwtaLaborHoursRollupTableElement;
                    }

                    if (gfyLaborHoursRollupTableTemplateElement != null)
                    {
                        Dictionary<int, List<LaborRollupByDateNew>> gfyCurrentIwtaLaborTaskRollupData = this.GetTaskHourRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useDescriptionInsteadOfName, true, ElementOfCostType.IWTA, null);
                        SdtElement gfyIwtaHoursRollupTableElement = gfyLaborHoursRollupTableTemplateElement.CloneNode(true) as SdtElement;
                        RollupSummaryByGroupByYearTableData gfyIwtaLaborSummaryRollupData = gfyCurrentIwtaLaborTaskRollupData.Convert();
                        string tableTitle = "IWTA Labor " + hoursLabel + " Spread" + (byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(gfyIwtaHoursRollupTableElement, tableTitle, gfyIwtaLaborSummaryRollupData, this.DefaultHoursFormat, byQuarter, true);

                        if (populated)
                        {
                            currentInsertionElement.InsertAfterSelf(gfyIwtaHoursRollupTableElement);
                            currentInsertionElement = gfyIwtaHoursRollupTableElement;
                        }
                    }

                    #endregion
                }

                this.RemoveElement(laborHoursRollupTableTemplateElement);
                this.RemoveElement(gfyLaborHoursRollupTableTemplateElement);
            }

            #endregion
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

            bool byQuarter = false;
            SdtElement costSpreadRollupTableTemplateElement;

            if (useGfy)
            {
                costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyCostSpreadRollup);
                if (costSpreadRollupTableTemplateElement == null)
                {
                    costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyCostSpreadRollupByQuarter);
                    byQuarter = true;
                }
            }
            else
            {
                costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_CostSpreadRollup);
                if (costSpreadRollupTableTemplateElement == null)
                {
                    costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_CostSpreadRollupByYear);
                    if (costSpreadRollupTableTemplateElement == null)
                    {
                        costSpreadRollupTableTemplateElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_CostSpreadRollupByQuarter);
                        byQuarter = true;
                    }
                }
            }

            if (costSpreadRollupTableTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables))
                {
                    string tag = costSpreadRollupTableTemplateElement.Descendants<Tag>().FirstOrDefault().Val.Value;

                    BoeTaskElementDTO currentLaborTaskElement = allLaborTaskElements.FirstOrDefault(x => x.Id == laborTaskElement.BOETaskElementID.Value);

                    SdtElement currentInsertionElement = costSpreadRollupTableTemplateElement;

                    #region Repeat for each element of cost

                    Dictionary<int, List<LaborRollupByDateNew>> currentLMLaborTaskRollupData = this.GetLaborTaskCostRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useGfy, ElementOfCostType.LMLabor, RateType.Cost);
                    SdtElement lmLaborCostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;
                    bool populated = false;
                    if (tag == BOEExporterConstants.Table_CostSpreadRollupByYear)
                    {
                        RollupSummaryByYearTableData lmLaborSummaryRollupData = currentLMLaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(lmLaborCostRollupTableElement, "LM Labor Cost Spread", lmLaborSummaryRollupData, this.DefaultCurrencyFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData lmLaborSummaryRollupData = currentLMLaborTaskRollupData.Convert();
                        string tableTitle = "LM Labor Cost Spread" + (useGfy && byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(lmLaborCostRollupTableElement, tableTitle, lmLaborSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                    }

                    if (populated)
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(lmLaborCostRollupTableElement);
                    }

                    Dictionary<int, List<LaborRollupByDateNew>> currentSubLaborTaskRollupData = this.GetLaborTaskCostRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useGfy, ElementOfCostType.Sub, RateType.Cost);
                    SdtElement subLaborCostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;                    
                    populated = false;
                    if (tag == BOEExporterConstants.Table_CostSpreadRollupByYear)
                    {
                        RollupSummaryByYearTableData subLaborSummaryRollupData = currentSubLaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(subLaborCostRollupTableElement, "Subcontractor Cost Spread", subLaborSummaryRollupData, this.DefaultCurrencyFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData subLaborSummaryRollupData = currentSubLaborTaskRollupData.Convert();
                        string tableTitle = "Subcontractor Cost Spread" + (useGfy && byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(subLaborCostRollupTableElement, tableTitle, subLaborSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                    }
                    
                    if (populated)
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(subLaborCostRollupTableElement);
                    }

                    Dictionary<int, List<LaborRollupByDateNew>> currentIWTALaborTaskRollupData = this.GetLaborTaskCostRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useGfy, ElementOfCostType.IWTA, RateType.Cost);
                    SdtElement iwtaLaborCostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;
                    populated = false;
                    if (tag == BOEExporterConstants.Table_CostSpreadRollupByYear)
                    {
                        RollupSummaryByYearTableData iwtaLaborSummaryRollupData = currentIWTALaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(iwtaLaborCostRollupTableElement, "IWTA Cost Spread", iwtaLaborSummaryRollupData, this.DefaultCurrencyFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData iwtaLaborSummaryRollupData = currentIWTALaborTaskRollupData.Convert();
                        string tableTitle = "IWTA Cost Spread" + (useGfy && byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(iwtaLaborCostRollupTableElement, tableTitle, iwtaLaborSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                    }

                    if (populated)
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(iwtaLaborCostRollupTableElement);
                    }

                    Dictionary<int, List<LaborRollupByDateNew>> currentMaterialLaborTaskRollupData = this.GetLaborTaskCostRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useGfy, ElementOfCostType.Materials, RateType.Cost);
                    SdtElement materialLaborCostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;
                    populated = false;
                    if (tag == BOEExporterConstants.Table_CostSpreadRollupByYear)
                    {
                        RollupSummaryByYearTableData materialLaborSummaryRollupData = currentMaterialLaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(materialLaborCostRollupTableElement, "Material Cost Spread", materialLaborSummaryRollupData, this.DefaultCurrencyFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData materialLaborSummaryRollupData = currentMaterialLaborTaskRollupData.Convert();
                        string tableTitle = "Material Cost Spread" + (useGfy && byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(materialLaborCostRollupTableElement, tableTitle, materialLaborSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                    }

                    if (populated)
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(materialLaborCostRollupTableElement);
                    }

                    Dictionary<int, List<LaborRollupByDateNew>> currentODCLaborTaskRollupData = this.GetLaborTaskCostRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useGfy, ElementOfCostType.ODC, RateType.Cost);
                    SdtElement odcLaborCostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;
                    populated = false;
                    if (tag == BOEExporterConstants.Table_CostSpreadRollupByYear)
                    {
                        RollupSummaryByYearTableData odcLaborSummaryRollupData = currentODCLaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(odcLaborCostRollupTableElement, "ODC Cost Spread", odcLaborSummaryRollupData, this.DefaultCurrencyFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData odcLaborSummaryRollupData = currentODCLaborTaskRollupData.Convert();
                        string tableTitle = "ODC Cost Spread" + (useGfy && byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(odcLaborCostRollupTableElement, tableTitle, odcLaborSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                    }

                    if (populated)
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(odcLaborCostRollupTableElement);
                    }

                    Dictionary<int, List<LaborRollupByDateNew>> currentTravelLaborTaskRollupData = this.GetLaborTaskCostRollup(new Collection<BoeTaskElementDTO> { currentLaborTaskElement }, laborTaskElement.taskElementLabors, boeExportModelView, useGfy, ElementOfCostType.Travel, RateType.Cost);
                    SdtElement travelLaborCostRollupTableElement = costSpreadRollupTableTemplateElement.CloneNode(true) as SdtElement;
                    populated = false;
                    if (tag == BOEExporterConstants.Table_CostSpreadRollupByYear)
                    {
                        RollupSummaryByYearTableData travelLaborSummaryRollupData = currentTravelLaborTaskRollupData.ConvertToRollupSummaryByYear();
                        populated = this.PopulateRollupSummaryByYearTable(travelLaborCostRollupTableElement, "Travel Cost Spread", travelLaborSummaryRollupData, this.DefaultCurrencyFormat, false);
                    }
                    else
                    {
                        RollupSummaryByGroupByYearTableData travelLaborSummaryRollupData = currentTravelLaborTaskRollupData.Convert();
                        string tableTitle = "Travel Cost Spread" + (useGfy && byQuarter ? BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : string.Empty);
                        populated = this.PopulateRollupSummaryByGroupByYearTable(travelLaborCostRollupTableElement, tableTitle, travelLaborSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                    }

                    if (populated)
                    {
                        currentInsertionElement = currentInsertionElement.InsertAfterSelf(travelLaborCostRollupTableElement);
                    }

                    #endregion
                }

                this.RemoveElement(costSpreadRollupTableTemplateElement);
            }

            #endregion
        }

        /// <summary>
        /// Process the ODC Task Direct Cost Rollup Table before populating it
        /// </summary>
        /// <param name="containerElement">Container element for task rollup table</param>
        /// <param name="odcTaskElement">ODC task element data</param>
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

            #region ODC Direct Cost Rollup Table

            bool byQuarter = false;
            SdtElement odcCostRollupTableElement;

            if (useGfy)
            {
                odcCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollup);
                if (odcCostRollupTableElement == null)
                {
                    odcCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
                    byQuarter = true;
                }
            }
            else
            {
                odcCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollup);
                if (odcCostRollupTableElement == null)
                {
                    odcCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollupByYear);
                    if (odcCostRollupTableElement == null)
                    {
                        odcCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollupByQuarter);
                        byQuarter = true;
                    }
                }
            }

            if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables) && odcCostRollupTableElement!=null)
            {
                string tag = odcCostRollupTableElement.Descendants<Tag>().FirstOrDefault().Val.Value;
                
                OtherDirectCostDTO currentODCTaskElement = exportInputs.Odcs.First(x => x.Id == odcTaskElement.BOETaskElementID.Value);
                Dictionary<int, List<LaborRollupByDateNew>> currentODCTaskRollupData = this.GetODCCostRollup(new Collection<OtherDirectCostDTO> { currentODCTaskElement }, odcTaskElement.taskElementLabors, useGfy);
                if (tag == BOEExporterConstants.Table_DirectCostRollupByYear)
                {
                    RollupSummaryByYearTableData odcSummaryRollupData = currentODCTaskRollupData.ConvertToRollupSummaryByYear();
                    this.PopulateRollupSummaryByYearTable(odcCostRollupTableElement, null, odcSummaryRollupData, this.DefaultCurrencyFormat, false);
                }
                else
                {
                    RollupSummaryByGroupByYearTableData odcSummaryRollupData = currentODCTaskRollupData.Convert();
                    this.PopulateRollupSummaryByGroupByYearTable(odcCostRollupTableElement, null, odcSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                }
            }
            else
            {
                this.RemoveElement(odcCostRollupTableElement);
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

            bool byQuarter = false;
            SdtElement travelCostRollupTableElement;
            if (useGfy)
            {
                travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollup);
                if (travelCostRollupTableElement == null)
                {
                    travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
                    byQuarter = true;
                }
            }
            else
            {
                travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollup);
                if (travelCostRollupTableElement == null)
                {
                    travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollupByYear);
                    if (travelCostRollupTableElement == null)
                    {
                        travelCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollupByQuarter);
                        byQuarter = true;
                    }
                }
            }

            if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables) && travelCostRollupTableElement != null)
            {
                string tag = travelCostRollupTableElement.Descendants<Tag>().FirstOrDefault().Val.Value;

                TravelDTO currentTravelTaskElement = exportInputs.Travels.First(x => x.Id == travelTaskElement.BOETaskElementID.Value);
                Dictionary<int, List<LaborRollupByDateNew>> currentTravelTaskRollupData = this.GetTravelCostRollup(new Collection<TravelDTO> { currentTravelTaskElement }, new Collection<BOEExportTaskElement> { travelTaskElement }, travelResources, useGfy);
                if (tag == BOEExporterConstants.Table_DirectCostRollupByYear)
                {
                    RollupSummaryByYearTableData travelSummaryRollupData = currentTravelTaskRollupData.ConvertToRollupSummaryByYear();
                    this.PopulateRollupSummaryByYearTable(travelCostRollupTableElement, null, travelSummaryRollupData, this.DefaultCurrencyFormat, false);
                }
                else
                {
                    RollupSummaryByGroupByYearTableData travelSummaryRollupData = currentTravelTaskRollupData.Convert();
                    this.PopulateRollupSummaryByGroupByYearTable(travelCostRollupTableElement, null, travelSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                }
            }
            else
            {
                this.RemoveElement(travelCostRollupTableElement);
            }

            #endregion
        }

        /// <summary>
        /// Process the Material Task Direct Cost Rollup Table before populating it
        /// </summary>
        /// <param name="containerElement">Container element for task rollup table</param>
        /// <param name="materialTaskElement">Material task element data</param>
        /// <param name="selectedComponents">Components selected for the output</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="useGfy">use government fiscal year?</param>
        /// <exception cref="System.ArgumentNullException">
        /// selectedComponents
        /// or
        /// exportInputs
        /// </exception>
        protected override void ProcessMaterialTaskDirectCostRollupTable(SdtElement containerElement, BOEExportTaskElement materialTaskElement, ICollection<BoeCustomReportComponent> selectedComponents, BOEExportInputs exportInputs, bool useGfy)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            #region Material Direct Cost Rollup Table

            bool byQuarter = false;
            SdtElement materialCostRollupTableElement;
            if (useGfy)
            {
                materialCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollup);
                if (materialCostRollupTableElement == null)
                {
                    materialCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_GfyDirectCostRollupByQuarter);
                    byQuarter = true;
                }
            }
            else
            {
                materialCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollup);
                if (materialCostRollupTableElement == null)
                {
                    materialCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollupByYear);
                    if (materialCostRollupTableElement == null)
                    {
                        materialCostRollupTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_DirectCostRollupByQuarter);
                        byQuarter = true;
                    }
                }
            }

            if (selectedComponents.Contains(BoeCustomReportComponent.TaskSpreadTables) && materialCostRollupTableElement != null)
            {
                string tag = materialCostRollupTableElement.Descendants<Tag>().FirstOrDefault().Val.Value;

                Dictionary<int, List<LaborRollupByDateNew>> currentMaterialRollupData = new Dictionary<int, List<LaborRollupByDateNew>>();
                if (tag == BOEExporterConstants.Table_DirectCostRollupByYear)
                {
                    RollupSummaryByYearTableData travelSummaryRollupData = currentMaterialRollupData.ConvertToRollupSummaryByYear();
                    this.PopulateRollupSummaryByYearTable(materialCostRollupTableElement, null, travelSummaryRollupData, this.DefaultCurrencyFormat, false);
                }
                else
                {
                    RollupSummaryByGroupByYearTableData travelSummaryRollupData = currentMaterialRollupData.Convert();
                    this.PopulateRollupSummaryByGroupByYearTable(materialCostRollupTableElement, null, travelSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy);
                }
            }
            else
            {
                this.RemoveElement(materialCostRollupTableElement);
            }

            #endregion
        }

        #endregion

        #region Hours and cost summary tables

        /// <summary>
        /// Process the data for the Labor Hours Summary Table before populating it
        /// </summary>
        /// <param name="boeContainer">Container element for the BOE</param>
        /// <param name="taskElementCollection">Collection of the BOE's task elements</param>
        /// <param name="resourcesByElementOfCost">Resources used by the BOE</param>
        /// <param name="selectedComponents">Components to be included in the export</param>
        /// <param name="useGfy">use government fiscal year?</param>
        protected override void ProcessLaborHoursSummaryTable(SdtElement boeContainer, ICollection<BoeTaskElementDTO> taskElementCollection, IDictionary<ElementOfCostType, Collection<ResourceDTO>> resourcesByElementOfCost, ICollection<BoeCustomReportComponent> selectedComponents, bool isUsingEquivalentPerson, bool useGfy)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (resourcesByElementOfCost == null)
            {
                throw new ArgumentNullException(nameof(resourcesByElementOfCost));
            }

            #region Labor Hours Summary By Date Table

            bool byQuarter = false;
            SdtElement laborHoursSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_LaborHoursSummaryByDate); ;
            SdtElement gfyLaborHoursSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_GfyLaborHoursSummaryByDate);

            if (laborHoursSummaryTableTemplateElement == null)
            {
                laborHoursSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_LaborHoursSummaryByQuarter);
                if (laborHoursSummaryTableTemplateElement != null)
                {
                    byQuarter = true;
                }
            }

            if (gfyLaborHoursSummaryTableTemplateElement == null)
            {
                gfyLaborHoursSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_GfyLaborHoursSummaryByQuarter);
                if (gfyLaborHoursSummaryTableTemplateElement != null)
                {
                    byQuarter = true;
                }
            }

            if (laborHoursSummaryTableTemplateElement != null || gfyLaborHoursSummaryTableTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.BOESpreadSummaryTables) && taskElementCollection.Any())
                {
                    SdtElement currentInsertionElement = gfyLaborHoursSummaryTableTemplateElement != null ? gfyLaborHoursSummaryTableTemplateElement : laborHoursSummaryTableTemplateElement;

                    #region Use one Summary table for each element of cost

                    IDictionary<ElementOfCostType, string> rollupTableTitles;

                    if (isUsingEquivalentPerson)
                    {
                        rollupTableTitles = new Dictionary<ElementOfCostType, string>
                            {
                                { ElementOfCostType.IWTA, "IWTA Labor EPs Summary" },
                                { ElementOfCostType.Sub, "Subcontractor Labor EPs Summary" },
                                { ElementOfCostType.LMLabor, "LM Labor EPs Summary" }
                            };
                    }
                    else
                    {
                        rollupTableTitles = new Dictionary<ElementOfCostType, string>
                            {
                                { ElementOfCostType.IWTA, "IWTA Labor Hours Summary" },
                                { ElementOfCostType.Sub, "Subcontractor Labor Hours Summary" },
                                { ElementOfCostType.LMLabor, "LM Labor Hours Summary" }
                            };
                    }

                    foreach (KeyValuePair<ElementOfCostType, string> entry in rollupTableTitles)
                    {
                        Collection<ResourceDTO> laborResources = resourcesByElementOfCost[entry.Key];

                        // compile the rollup data
                        this.PopulateLaborHoursSummaryTable(taskElementCollection, laborResources, gfyLaborHoursSummaryTableTemplateElement, currentInsertionElement, entry, byQuarter, true);
                        this.PopulateLaborHoursSummaryTable(taskElementCollection, laborResources, laborHoursSummaryTableTemplateElement, currentInsertionElement, entry, byQuarter, false);
                    }
                    
                    #endregion
                }

                this.RemoveElement(laborHoursSummaryTableTemplateElement);
                this.RemoveElement(gfyLaborHoursSummaryTableTemplateElement);
            }

            #endregion
        }

        /// <summary>
        /// Populate the Labor Hours Summary Table
        /// </summary>
        /// <param name="taskElementCollection">Task Elements</param>
        /// <param name="laborResources">Labor Resources</param>
        /// <param name="templateElement">Table template element</param>
        /// <param name="currentInsertionElement">current insertion element</param>
        /// <param name="entry">table title entry</param>
        /// <param name="byQuarter">table by Quarter?</param>
        /// <param name="useGfy">Use Govt Fiscal Year?</param>
        private void PopulateLaborHoursSummaryTable(ICollection<BoeTaskElementDTO> taskElementCollection, Collection<ResourceDTO> laborResources, SdtElement templateElement, SdtElement currentInsertionElement, KeyValuePair<ElementOfCostType, string> entry, bool byQuarter, bool useGfy)
        {
            if (templateElement != null)
            {
                List<LaborRollupByDateNew> laborRollupData = this.GetRollupByYear(taskElementCollection, laborResources, null, useGfy);
                IList<RollupSummaryByYearTableRowData> laborHoursSummaryRollupData = laborRollupData.Convert();

                RollupSummaryByYearTableData rollupTableData = new RollupSummaryByYearTableData
                {
                    SummaryTotalComplete = laborHoursSummaryRollupData.Sum(d => d.YearTotal),
                    YearlyData = laborHoursSummaryRollupData
                };

                bool printTable = true;
                if (rollupTableData.SummaryTotalComplete == 0)
                {
                    //check for any nonzero values in case the table totals 0 using negative values
                    ICollection<int> resourceIds = laborResources.Select(r => r.Id).ToList();
                    ResourceSpreadDto nonzero = taskElementCollection.SelectMany(x => x.taskElementLabors)
                        .Where(r => r.ResourceID.HasValue && resourceIds.Contains(r.ResourceID.Value) && r.SpreadType == SpreadType.Hours)
                        .SelectMany(y => y.LaborSpreads)
                        .FirstOrDefault(z => z.LaborSpreadValue != 0);
                    if (nonzero == null)
                    {
                        printTable = false;
                    }
                }

                if (printTable)//only print if there are hours
                {
                    SdtElement laborHoursSummaryByDateTableElement = templateElement.CloneNode(true) as SdtElement;

                    string tableTitle = useGfy && byQuarter ? entry.Value + BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : entry.Value;

                    if (this.PopulateRollupSummaryByYearTable(laborHoursSummaryByDateTableElement, tableTitle, rollupTableData, this.DefaultHoursFormat, byQuarter, useGfy))
                    {
                        currentInsertionElement.InsertAfterSelf(laborHoursSummaryByDateTableElement);
                        currentInsertionElement = laborHoursSummaryByDateTableElement;
                    }
                }
            }
        }

        /// <summary>
        /// Processes the labor cost summary table.
        /// </summary>
        /// <param name="boeContainer">The boe container.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boe">The boe.</param>
        /// <param name="taskElementDtos">The task element dtos.</param>
        /// <param name="resourcesByElementOfCost">The resources by element of cost.</param>
        /// <param name="selectedComponents">The selected components.</param>
        /// <param name="useGfy">use government fiscal year?</param>
        /// <returns>List of Labor Cost Summary Table</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        protected override List<LaborRollupByDateNew> ProcessLaborCostSummaryTable(SdtElement boeContainer, BOEExportInputs exportInputs, BoeDTO boe, ICollection<BoeTaskElementDTO> taskElementDtos, IDictionary<ElementOfCostType, Collection<ResourceDTO>> resourcesByElementOfCost, ICollection<BoeCustomReportComponent> selectedComponents, bool useGfy)
        {
            if (selectedComponents == null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (resourcesByElementOfCost == null)
            {
                throw new ArgumentNullException(nameof(resourcesByElementOfCost));
            }

            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            #region Labor Cost Summary By Date Table

            bool byQuarter = false;
            SdtElement costSummaryTableTemplateElement;
            if (useGfy)
            {
                costSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_GfyLaborCostSummaryByDate);
                if (costSummaryTableTemplateElement == null)
                {
                    costSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_GfyLaborCostSummaryByQuarter);
                    byQuarter = true;
                }
            }
            else
            {
                costSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_LaborCostSummaryByDate);
                if (costSummaryTableTemplateElement == null)
                {
                    costSummaryTableTemplateElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_LaborCostSummaryByQuarter);
                    byQuarter = true;
                }
            }
            List<LaborRollupByDateNew> taskRollupCostData = null;

            if (costSummaryTableTemplateElement != null)
            {
                if (selectedComponents.Contains(BoeCustomReportComponent.BOESpreadSummaryTables))
                {
                    SdtElement currentInsertionElement = costSummaryTableTemplateElement;

                    IDictionary<ElementOfCostType, string> rollupTableTitles = new Dictionary<ElementOfCostType, string>
                    {
                        { ElementOfCostType.Sub, "Subcontractor Cost Summary" },
                        { ElementOfCostType.LMLabor, "LM Labor Cost Summary" },
                        { ElementOfCostType.IWTA, "IWTA Cost Summary" },
                        { ElementOfCostType.Materials, "Material Cost Summary" },
                        { ElementOfCostType.Travel, "Travel Cost Summary" },
                        { ElementOfCostType.ODC, "Other Direct Cost Summary" }
                    };

                    foreach (KeyValuePair<ElementOfCostType, string> entry in rollupTableTitles)
                    {
                        RollupSummaryByYearTableData costSummaryRollupData = new RollupSummaryByYearTableData();

                        bool printTable = true;
                        taskRollupCostData = this.GetTaskCostRollup(taskElementDtos, exportInputs, resourcesByElementOfCost[entry.Key].Where(r => r.RateType == RateType.Cost).ToCollection(), useGfy);
                        costSummaryRollupData = taskRollupCostData.ConvertToRollupSummaryByYear();
                        if (costSummaryRollupData.SummaryTotalComplete == 0)
                        {
                            //check for any nonzero values in case the table totals 0 using negative values
                            List<int> resourceIds = resourcesByElementOfCost[entry.Key].Where(r => r.RateType == RateType.Cost).Select(r => r.Id).ToList();
                            ResourceSpreadDto nonzero = taskElementDtos.SelectMany(x => x.taskElementLabors)
                                .Where(r => r.ResourceID.HasValue && resourceIds.Contains(r.ResourceID.Value) && r.SpreadType == SpreadType.Cost)
                                .SelectMany(y => y.LaborSpreads)
                                .FirstOrDefault(z => z.LaborSpreadValue != 0);
                            if (nonzero == null)
                            {
                                printTable = false;
                            }
                        }
                        

                        if (printTable)//only print if there are costs
                        {
                            SdtElement costSummaryByDateTableElement = costSummaryTableTemplateElement.CloneNode(true) as SdtElement;
                            string tableTitle = useGfy && byQuarter ? entry.Value + BOEExporterConstants.GFY_QUARTERLY_TABLE_SUFFIX : entry.Value;

                            if (this.PopulateRollupSummaryByYearTable(costSummaryByDateTableElement, tableTitle, costSummaryRollupData, this.DefaultCurrencyFormat, byQuarter, useGfy))
                            {
                                currentInsertionElement.InsertAfterSelf(costSummaryByDateTableElement);
                                currentInsertionElement = costSummaryByDateTableElement;
                            }
                        }                        
                    }
                    this.RemoveElement(costSummaryTableTemplateElement);
                }
            }
            // TODO:SJR - need to return a dictionary of these, indexed by Element of Cost?
            return taskRollupCostData;

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
        protected override void ProcessLaborTaskResources(SdtElement containerElement, BOEExportInputs exportInputs, BOEExportTaskElement laborTaskElement, 
            ICollection<BoeTaskElementDTO> allLaborTaskElements, ICollection<BoeCustomReportComponent> selectedComponents, bool multiBoe)
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
                    var orderedResources = laborTaskElement.taskElementLabors
                        .Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID) &&
                        c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgID))
                        .OrderBy(r => r.ExportFields[BOEExporterConstants.FieldName_ResourceID])
                        .ThenBy(p => p.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]).ToList();

                    var LMLaborResources = orderedResources.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_LaborTypes)
                        && c.ExportFields[BOEExporterConstants.FieldName_LaborTypes] == ElementOfCostType.LMLabor.ToDescription()).ToList();
                    var SubResources = orderedResources.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_LaborTypes)
                        && c.ExportFields[BOEExporterConstants.FieldName_LaborTypes] == ElementOfCostType.Sub.ToString()).ToList();
                    var IWTAResources = orderedResources.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_LaborTypes)
                        && c.ExportFields[BOEExporterConstants.FieldName_LaborTypes] == ElementOfCostType.IWTA.ToString()).ToList();
                    var TravelResources = orderedResources.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_LaborTypes)
                        && c.ExportFields[BOEExporterConstants.FieldName_LaborTypes] == ElementOfCostType.Travel.ToString()).ToList();
                    var MaterialsResources = orderedResources.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_LaborTypes)
                        && c.ExportFields[BOEExporterConstants.FieldName_LaborTypes] == ElementOfCostType.Materials.ToString()).ToList();
                    var ODCResources = orderedResources.Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_LaborTypes)
                        && c.ExportFields[BOEExporterConstants.FieldName_LaborTypes] == ElementOfCostType.ODC.ToString()).ToList();

                    SdtElement currentInsertionPoint = laborResourceContainerTemplateElement;
                    
                    foreach (var resourceElement in LMLaborResources)
                    {
                        SdtElement laborResourceContainerElement = this.CloneContainerTemplate(laborResourceContainerTemplateElement);

                        this.ProcessResourceHeader(laborResourceContainerElement, resourceElement, multiBoe, exportInputs.Workspace);
                        this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
                        this.ProcessResourceRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

                        currentInsertionPoint.InsertAfterSelf(laborResourceContainerElement);
                        currentInsertionPoint = laborResourceContainerElement;
                    }
                    foreach (var resourceElement in SubResources)
                    {
                        SdtElement laborResourceContainerElement = this.CloneContainerTemplate(laborResourceContainerTemplateElement);

                        this.ProcessResourceHeader(laborResourceContainerElement, resourceElement, multiBoe, exportInputs.Workspace);
                        this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
                        this.ProcessResourceRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

                        currentInsertionPoint.InsertAfterSelf(laborResourceContainerElement);
                        currentInsertionPoint = laborResourceContainerElement;
                    }
                    foreach (var resourceElement in IWTAResources)
                    {
                        SdtElement laborResourceContainerElement = this.CloneContainerTemplate(laborResourceContainerTemplateElement);

                        this.ProcessResourceHeader(laborResourceContainerElement, resourceElement, multiBoe, exportInputs.Workspace);
                        this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
                        this.ProcessResourceRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

                        currentInsertionPoint.InsertAfterSelf(laborResourceContainerElement);
                        currentInsertionPoint = laborResourceContainerElement;
                    }
                    foreach (var resourceElement in TravelResources)
                    {
                        SdtElement laborResourceContainerElement = this.CloneContainerTemplate(laborResourceContainerTemplateElement);

                        this.ProcessResourceHeader(laborResourceContainerElement, resourceElement, multiBoe, exportInputs.Workspace);
                        this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
                        this.ProcessResourceRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

                        currentInsertionPoint.InsertAfterSelf(laborResourceContainerElement);
                        currentInsertionPoint = laborResourceContainerElement;
                    }
                    foreach (var resourceElement in MaterialsResources)
                    {
                        SdtElement laborResourceContainerElement = this.CloneContainerTemplate(laborResourceContainerTemplateElement);

                        this.ProcessResourceHeader(laborResourceContainerElement, resourceElement, multiBoe, exportInputs.Workspace);
                        this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
                        this.ProcessResourceRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

                        currentInsertionPoint.InsertAfterSelf(laborResourceContainerElement);
                        currentInsertionPoint = laborResourceContainerElement;
                    }
                    foreach (var resourceElement in ODCResources)
                    {
                        SdtElement laborResourceContainerElement = this.CloneContainerTemplate(laborResourceContainerTemplateElement);

                        this.ProcessResourceHeader(laborResourceContainerElement, resourceElement, multiBoe, exportInputs.Workspace);
                        this.ProcessResourceCustomFields(laborResourceContainerElement, resourceElement, exportInputs.CustomFields, exportInputs);
                        this.ProcessResourceRollupTable(laborResourceContainerElement, resourceElement, allLaborTaskElements, laborTaskElement);

                        currentInsertionPoint.InsertAfterSelf(laborResourceContainerElement);
                        currentInsertionPoint = laborResourceContainerElement;
                    }
                    this.RemoveElement(laborResourceContainerTemplateElement);
                }
                else
                {
                    this.RemoveElement(laborResourceContainerTemplateElement);
                    WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.FieldName_ResourceSectionLabel);
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
                    var orderedResources = odcTaskElement.taskElementLabors
                        .Where(c => c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceID)
                            && c.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgID))
                            .OrderBy(o => o.ExportFields[BOEExporterConstants.FieldName_ResourceID])
                            .ThenBy(t => t.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]);

                    ICollection<OtherDirectCostType> ODCTypes = exportInputs.Odcs.Where(t => t.TaskID == odcTaskElement.BOETaskID).SelectMany(o => o.ODCTypes).ToList();

                    SdtElement currentInsertionPoint = odcResourceContainerTemplateElement;

                    foreach (var resourceElement in orderedResources)
                    {
                        SdtElement odcResourceContainerElement = this.CloneContainerTemplate(odcResourceContainerTemplateElement);

                        OtherDirectCostType currentODCType = ODCTypes
                            .FirstOrDefault(r => r.ResourceID.ToString() == resourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceID]
                            && r.PerformingOrgID.ToString() == resourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]);

                        if (currentODCType != null)
                        {
                            resourceElement.StartDate = currentODCType.StartDate;
                            resourceElement.EndDate = currentODCType.EndDate;

                            //Multi-BOEs bool always false because Multi-BOEs do not have ODC
                            this.ProcessResourceHeader(odcResourceContainerElement, resourceElement, false, exportInputs.Workspace);
                            this.ProcessODCResourceCostRollupTable(odcResourceContainerElement, odcTaskElement, currentODCType);
                        }

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

        #region Private methods

        /// <summary>
        /// Process and populate the resource header with appropriate data
        /// </summary>
        /// <param name="containerElement">Container element for the resource</param>
        /// <param name="ResourceElement">Resource with data to populate the header</param>
        /// <param name="multiBoe">Bool to note if BOE is multi</param>
        private void ProcessResourceHeader(SdtElement containerElement, BOEExportTaskElementLabor ResourceElement, bool multiBoe, WorkspaceDTO workspace)
        {
            IDictionary<string, string> resourceHeaderDataValueMappings = new Dictionary<string, string>
            {
                {BOEExporterConstants.FieldName_ResourceStartDate, ResourceElement.StartDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR)},
                {BOEExporterConstants.FieldName_ResourceEndDate, ResourceElement.EndDate.Value.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR)}
            };

            if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceRateType)
                && ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceRateType] != RateType.NotSet.ToString())
            {
                if (ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceRateType] == RateType.Hours.ToString())
                {
                    if (FullObjectHelper.ShowEquivalentPersonsOption && workspace.IsUsingEquivalentPerson)
                    {
                        resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceCostHoursLabel, "EPs:");
                    }
                    else
                    {
                        resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceCostHoursLabel, "Hours:");
                    }
                    resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceCostHours, Utilities.FormatStringWithPrecision((decimal)ResourceElement.Hours, this.WorkspaceDecimalPrecision));
                }
                else //Cost
                {
                    resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceCostHoursLabel, "Cost:");                    
                    resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceCostHours, "$" + ((int)ResourceElement.Cost).ToString());
                }
            }
            else
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceCostHoursLabel, "");
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceCostHours, "NO TOTAL");
            }

            if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceName))
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceName, ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceName]);
            }
            else
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceName, "NO RESOURCE NAME");
            }

            if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceDescription))
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceDescription]);
            }
            else
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_ResourceDescription, "NO RESOURCE DESCRIPTION");
            }

            if(multiBoe)
            {
                if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_WBSString))
                {
                    resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_WBSString, ResourceElement.ExportFields[BOEExporterConstants.FieldName_WBSString]);
                }
                else
                {
                    resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_WBSString, "NO WBS");
                }

                if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_CLINString))
                {
                    resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_CLINString, ResourceElement.ExportFields[BOEExporterConstants.FieldName_CLINString]);
                }
                else
                {
                    resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_CLINString, "NO CLIN");
                }
            }
            else
            {
                //remove WBS and CLIN for non-multi-boes
                WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_MultiWbsClin);
            }

            //remove Reference for non-summary boes
            WordUtilities.RemoveTaggedElement(containerElement, BOEExporterConstants.Container_SummaryReference);

            if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_LaborTypes))
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_LaborTypes, ResourceElement.ExportFields[BOEExporterConstants.FieldName_LaborTypes]);
            }
            else
            {
                resourceHeaderDataValueMappings.Add(BOEExporterConstants.FieldName_LaborTypes, "NO ELEMENT OF COST");
            }

            if (ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrg) && ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_PerformingOrgDescription))
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
        /// Process and populate the Resource Hours or Cost Rollup Table
        /// </summary>
        /// <param name="containerElement">Container elment for the resource</param>
        /// <param name="ResourceElement">Resource with the data to populate the table</param>
        /// <param name="allLaborTaskElements">All labor task elements in the workspace</param>
        /// <param name="currentLaborTaskElement">Labor task element containing the resource</param>
        private void ProcessResourceRollupTable(SdtElement containerElement, BOEExportTaskElementLabor ResourceElement, ICollection<BoeTaskElementDTO> allLaborTaskElements, BOEExportTaskElement currentLaborTaskElement)
        {
            SdtElement resourceTableElement = WordUtilities.GetTaggedChildElement(containerElement, BOEExporterConstants.Table_ResourceRollup);

            if (resourceTableElement != null && ResourceElement.ExportFields.ContainsKey(BOEExporterConstants.FieldName_ResourceRateType))
            {
                BoeTaskElementDTO currentLaborTaskDto = allLaborTaskElements.FirstOrDefault(x => x.Id == currentLaborTaskElement.BOETaskElementID);

                if (currentLaborTaskDto != null)
                {
                    ResourceTypeDto currentResourceTypeDTO = currentLaborTaskDto.taskElementLabors
                        .FirstOrDefault(r => r.ResourceID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_ResourceID] &&
                            r.Id.ToString()==ResourceElement.ExportFields[BOEExporterConstants.FieldName_LaborTypeID]
                            && r.PerformingOrgID.ToString() == ResourceElement.ExportFields[BOEExporterConstants.FieldName_PerformingOrgID]
                            && r.StartDate == ResourceElement.StartDate && r.EndDate == ResourceElement.EndDate);
                    if (currentResourceTypeDTO != null)
                    {                        
                        List<LaborRollupByDateNew> rollupData = this.GetResourceRollupByYear(currentResourceTypeDTO, ResourceElement).ToList();
                        IList<RollupSummaryByYearTableRowData> summaryRollupData = rollupData.Convert();

                        RollupSummaryByYearTableData rollupTableData = new RollupSummaryByYearTableData
                        {
                            SummaryTotalComplete = summaryRollupData.Sum(s => s.YearTotal),
                            YearlyData = summaryRollupData
                        };

                        this.PopulateRollupSummaryByYearTable(resourceTableElement, null, rollupTableData, this.DefaultHoursFormat, false);
                    }
                    else
                    {
                        this.RemoveElement(resourceTableElement);
                    }
                }
                else
                {
                    this.RemoveElement(resourceTableElement);
                }
            }
        }

        #endregion

        protected override void ProcessBOEHoursSummaryTable(SdtElement boeContainer, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents)
        {
            // SSC template does not include this table
            SdtElement boeHoursSummaryTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_BOEHoursSummary);
            if (boeHoursSummaryTableElement != null)
            {
                this.RemoveElement(boeHoursSummaryTableElement);
            }
        }

        protected override void ProcessBOECostSummaryTable(SdtElement boeContainer, WorkspaceDTO workspaceData, Collection<ResourceDTO> travelResources, BOEExportModelView boeExportModelView, ICollection<BoeCustomReportComponent> selectedComponents)
        {
            // SSC template does not include this table
            SdtElement boeCostSummaryTableElement = WordUtilities.GetTaggedChildElement(boeContainer, BOEExporterConstants.Table_BOECostSummary);
            if (boeCostSummaryTableElement != null)
            {
                this.RemoveElement(boeCostSummaryTableElement);
            }
        }

        #endregion
    }
}
