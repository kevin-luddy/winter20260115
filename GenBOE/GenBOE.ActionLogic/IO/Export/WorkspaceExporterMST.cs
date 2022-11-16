// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Exports a workspace to Excel
    /// </summary>
    public class WorkspaceExporterMST : WorkspaceExporter
    {
        public override string WORKSPACE_DATA_EXCEL_MAP_PATH
        {
            get { return "~/Templates/Export/WorkspaceDataMST.xlsx"; }
        }

        private TravelUnitCostExporterRMS travelUnitCostExporter;
        private TravelExtendedCostExporterRMS travelExtendedCostExporter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="permissionsDTOLoader">The permissions dto loader.</param>
        /// <param name="resourceDTODataLoader">The resource dto data loader.</param>
        /// <param name="perfOrgLoader">The perf org loader.</param>
        /// <param name="customFieldValueDTODataLoader">The custom field value dto data loader.</param>
        /// <param name="userDTODataLoader">The user dto data loader.</param>
        /// <param name="boeStatusReport">The boe status report.</param>
        /// <param name="wbsExporter">The WBS exporter.</param>
        /// <param name="travelTripCostCalculation">The travel trip cost calculation.</param>
        /// <param name="locationDtoDataLoader">The location dto data loader.</param>
        /// <param name="clinExporter">The clin exporter.</param>
        /// <param name="travelUnitCostExporter">Travel Unit Cost Exporter for RMS</param>
        /// <param name="travelExtendedCostExporter">Travel Extended Cost Exporter for RMS</param>
        public WorkspaceExporterMST(
            ICommonDataMapper commonDataMapper,
            IPermissionsDTODataLoader permissionsDTOLoader,
            IResourceDTODataLoader resourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            ICustomFieldValueDTODataLoader customFieldValueDTODataLoader,
            IUserDTODataLoader userDTODataLoader,
            IBOEStatusReport boeStatusReport,
            WbsExporter wbsExporter,
            TravelTripCostCalculation travelTripCostCalculation,
            ILocationDTODataLoader locationDtoDataLoader,
            ICLINExporter clinExporter,
            TravelUnitCostExporterRMS travelUnitCostExporter,
            TravelExtendedCostExporterRMS travelExtendedCostExporter)
            : base(commonDataMapper,
             permissionsDTOLoader,
             resourceDTODataLoader,
             perfOrgLoader,
             customFieldValueDTODataLoader,
             userDTODataLoader,
             boeStatusReport,
             wbsExporter,
             travelTripCostCalculation,
             locationDtoDataLoader,
             clinExporter)
        {
            this.travelUnitCostExporter = travelUnitCostExporter;
            this.travelExtendedCostExporter = travelExtendedCostExporter;
        }

        /// <summary>
        /// Creates the Workspace Identification sheet for MST
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// The populated <see cref="ExcelExportWorksheet" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">exportInputs</exception>
        
        protected override ExcelExportWorksheet GetWorkspaceIdentificationSheetExportData(BOEExportInputs exportInputs)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            var toReturn = new ExcelExportWorksheet("Workspace Identification");

            WorkspaceDTO workspace = exportInputs.Workspace;

            toReturn.Add("Workspace/Proposal Name", workspace.WorkspaceName);
            toReturn.Add("Description", workspace.Description);
            toReturn.Add("Line Of Business", workspace.LineOfBusiness.Text);
            toReturn.Add(CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC, this.UserDTODataLoader.GetUserByID(workspace.CostVolumeLeadPricerUserID).DisplayName);
            toReturn.Add("Contract Start Date", workspace.ContractStartDate.ToString("MM/yyyy"));
            toReturn.Add("Contract End Date", workspace.ContractEndDate.ToString("MM/yyyy"));
            toReturn.Add("Proposal Submittal Date", workspace.ProposalSubmittalDate.HasValue ? workspace.ProposalSubmittalDate.Value.ToString("MM/yyyy") : this.sEmpty);
            toReturn.Add("Resource Decimal Precision", workspace.DecimalPrecision.ToString());
            toReturn.Add("Cost Decimal Precision", workspace.CostDecimalPrecision.ToString());
            toReturn.Add("RFP", workspace.RFPNumber);
            toReturn.Add("OCI", workspace.ContainsOCI ? this.sYes : this.sNo);
            toReturn.Add("Proposal Status", this.CommonDataMapper.getProposalStatusTypesDictionary()[(int)workspace.ProposalStatus].ProposalStateType);
            toReturn.Add("Proposal Comments", workspace.StatusComment);
            // For some unknown reason these are needed so the file won't corrupt
            toReturn.Add(this.sEmpty, this.sEmpty);
            toReturn.Add(this.sEmpty, this.sEmpty);

            return toReturn;
        }

        /// <summary>
        /// Gets the export data for the Travel Unit Cost sheet
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>Export data</returns>
        protected override ExcelExportWorksheet GetTravelUnitCostSheetExportData(FullWorkspace workspace)
        {
            return this.travelUnitCostExporter.GetWorksheetData(workspace);
        }

        /// <summary>
        /// Gets the export data for the Travel Extended Cost sheet
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>Export data</returns>
        protected override ExcelExportWorksheet GetTravelExtendedCostSheetExportData(FullWorkspace workspace)
        {
            return this.travelExtendedCostExporter.GetWorksheetData(workspace);
        }

        /// <summary>
        /// Gets the start rows for the sheets in the Workspace Data Export
        /// </summary>
        /// <returns>int array of start rows</returns>
        /// <param name="usingTemplateBoe">Whether WS uses Template BOE</param>
        protected override int?[] GetStartRows(bool usingTemplateBoe)
        {
            if (usingTemplateBoe)
            {
                return new int?[] { null, null, 1, null, null, null, null, null, null, null, 1, 1 };
            }
            else
            {
                return new int?[] { null, null, 1, null, null, null, null, null, 1, 1 };
            }
        }

        /// <summary>
        /// Applies the remaining formatting to the Travel sheets
        /// </summary>
        /// <param name="filePath">The file path for the export</param>
        /// <param name="zoneTripRowCount">Zone Trip Row Count</param>
        protected override void ApplyNonzoneFormattingToTravelSheets(string filePath, int zoneTripRowCount)
        {
            this.travelUnitCostExporter.ApplyNonzoneFormatting(filePath, "Travel Unit Cost", zoneTripRowCount);
            this.travelExtendedCostExporter.ApplyNonzoneFormatting(filePath, "Travel Extended Cost", zoneTripRowCount);
        }

        /// <summary>
        /// Gets the count of Zone rows, which is the number of unique trips
        /// </summary>
        /// <param name="workspace">the workspace</param>
        /// <returns>The number of zone rows</returns>
        protected override int GetZoneRowCount(FullWorkspace workspace)
        {
            if(workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            
            return workspace.Boes.SelectMany(x => x.Travels)
                .SelectMany(x => x.MSTTravelTrips)
                .Where(x => x.ModeID == MSTTravelMode.ZoneAirfare || x.ModeID == MSTTravelMode.ZoneNoAirfare)
                .Select(x => new
                {
                    x.WbsNumber,
                    x.ClinNumber,
                    x.PerfOrgID,
                    x.ModeID,
                    x.ZoneOriginName,
                    x.ZoneDestinationName,
                    x.Purpose,
                    x.NumOfPeople,
                    x.NumOfDays
                }).Distinct().Count();
        }

        /// <summary>
        /// Get the MOQ Table Data row data
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="task">Task</param>
        /// <param name="selectedMoqType">Selected MOQ Type</param>
        /// <param name="table">MOQ Table</param>
        /// <param name="exportInputs">Export Inputs</param>
        /// <returns>MOQ Table Data row for the given data</returns>
        protected override IList<string> GetMOQTableDataRow(BoeDTO boe, BoeTaskElementDTO task, string selectedMoqType, MoqTableData table, BOEExportInputs exportInputs)
        {
            _ = boe ?? throw new ArgumentNullException(nameof(boe));
            _ = task ?? throw new ArgumentNullException(nameof(task));
            _ = table ?? throw new ArgumentNullException(nameof(table));
            _ = exportInputs ?? throw new ArgumentNullException(nameof(exportInputs));

            IList<string> toReturn = new List<string>()
            {
                boe.Id.ToString(),
                boe.Title ?? this.sEmpty,
                task.BOETaskID,
                task.TaskTitle,
                selectedMoqType,
                table.TableName,
                table.DateOfReport.ToShortDateString(),
                table.HistoricalProgramName,
                table.ContractNumber,
                table.WbsElement,
                table.PoPStartString,
                table.PoPEndString,
				table.PoPMonths.ToString(),
				table.TotalWbsHours.ToString(),
                table.AdditionalQueryFilters,
                table.TotalRelevantHours.ToString()
            };

            foreach (CustomFieldDTO customField in exportInputs.CustomFields.Where(x => x.CustomFieldDisplayID == CustomFieldType.MoqTypeTableDataDisplay))
            {
                CustomFieldValueContainer customFieldValue = table.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == customField.Id);
                toReturn.Add(customFieldValue != null ? customFieldValue.OpenEndedValue : this.sEmpty);
            }

            return toReturn;
        }
    }
}
