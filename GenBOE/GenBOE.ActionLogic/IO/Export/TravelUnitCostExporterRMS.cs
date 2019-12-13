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
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ZoneTravel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Exports Travel Unit Cost Data for RMS.
    /// </summary>
    /// <seealso cref="ITravelUnitCostExporter" />
    [ExcludeFromCodeCoverage]
    public class TravelUnitCostExporterRMS : ITravelUnitCostExporter
    {
        private const string NOT_APPLICABLE = "N/A";
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private static readonly string sEmpty = string.Empty;
        private const int STATIC_HEADER_ROW_COUNT = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelUnitCostExporterRMS"/> class.
        /// </summary>
        /// <param name="zoneTravelRatesFeesLoader">The zone travel rates fees loader.</param>
        /// <param name="perfOrgLoader">The perf org loader.</param>
        public TravelUnitCostExporterRMS(RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader, IPerformingOrgDTODataLoader perfOrgLoader)
        {
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
            this.perfOrgLoader = perfOrgLoader;
        }

        /// <summary>
        /// Exports to excel file.
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>A string location of an excel file that has travel unit costs.</returns>
        /// <exception cref="ArgumentNullException">workspace</exception>
        /// <exception cref="ArgumentNullException">templateFileLocation</exception>
        public string ExportToExcelFile(string templateFileLocation, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (String.IsNullOrEmpty(templateFileLocation))
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            string toReturn = null;
            int zoneTripRowCount = 0;
            ExcelExportWorksheet firstSheet = this.GetWorksheetData(workspace, out zoneTripRowCount);

            ExcelExportWorksheet[] sheets = new ExcelExportWorksheet[1];
            sheets[0] = firstSheet;
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, sheets, new int?[] { 1 });

            // Now we have to set the header rows for the Non-Zone Headers to be Bold
            this.ApplyNonzoneFormatting(toReturn, "Sheet1", zoneTripRowCount);

            return toReturn;
        }

        /// <summary>
        /// Public method to get worksheet data without needing to pass the zone trip row count out param
        /// </summary>
        /// <param name="workspace">The workspace</param>
        /// <returns>the worksheet</returns>
        public ExcelExportWorksheet GetWorksheetData(FullWorkspace workspace)
        {
            int temp;
            return this.GetWorksheetData(workspace, out temp);
        }

        /// <summary>
        /// Get the worksheet data
        /// </summary>
        /// <param name="workspace">workspace</param>
        /// <param name="zoneTripRowCount"></param>
        /// <returns>worksheet data</returns>
        private ExcelExportWorksheet GetWorksheetData(FullWorkspace workspace, out int zoneTripRowCount)
        {
            if(workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ExcelExportWorksheet firstSheet = new ExcelExportWorksheet();

            IReadOnlyCollection<WorkspaceHistoryDTO> workspaceHistory = workspace.WorkspaceHistory;

            string workspaceName = workspace.WorkspaceName;
            string workspaceTrackingNumber = workspace.TrackingNumber;
            string travelRatesDate = DateTime.Now.ToString();
            if (workspace.WorkspaceState == WorkspaceState.Closed)
            {
                travelRatesDate = (from r in workspaceHistory
                                   where r.NewValue == WorkspaceState.Closed
                                   select r.Date).LastOrDefault().ToString();

            }
            else if (workspace.WorkspaceState == WorkspaceState.Locked || workspace.WorkspaceState == WorkspaceState.Complete)
            {
                travelRatesDate = (from r in workspaceHistory
                                   where r.NewValue == WorkspaceState.Locked
                                   select r.Date).LastOrDefault().ToString();
            }
            else
            {
                travelRatesDate = DateTime.Now.ToString();
            }

            string submittalDate = DateTime.Now.ToString();

            firstSheet.Add("Proposal Name: " + workspaceName, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, "Unit Cost Report");
            firstSheet.Add("Proposal Tracking Number: " + workspaceTrackingNumber);
            firstSheet.Add("Travel rates as of: " + travelRatesDate);
            firstSheet.Add("Submittal Date: " + submittalDate);
            firstSheet.Add(sEmpty);
            firstSheet.Add("Zone");
            AddHeaders(firstSheet);

            Collection<TravelUnitRow> allZoneRows = new Collection<TravelUnitRow>();
            Collection<TravelUnitRow> allNonZoneRows = new Collection<TravelUnitRow>();

            // Bulk load all travel data for the workspace.
            List<FullBoe> boes = workspace.Boes.ToList();
            HashSet<ClinDTO> allClins = new HashSet<ClinDTO>(workspace.Clins);
            HashSet<WbsDTO> allWbs = new HashSet<WbsDTO>(workspace.WbsElements);
            HashSet<TravelDTO> allTravels = new HashSet<TravelDTO>(workspace.Travels);
            Collection<PerformingOrgDTO> allPerfOrgs = this.perfOrgLoader.GetByListId(workspace.PerfOrgListID);

            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.zoneTravelRatesFeesLoader.getAllFeesAndCostsByWorkspace(workspace.Id).ToDictionary(f => f.ModeID);

            ICollection<WorkspaceRMSEscalationRatesDTO> rates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(workspace.Id);
            Dictionary<int, decimal> airfareEscalationRates = rates.ToDictionary(x => x.Year, y => y.AirfareRate);
            Dictionary<int, decimal> perDiemEscalationRates = rates.ToDictionary(x => x.Year, y => y.PerDiemRate);
            Dictionary<int, decimal> miscEscalationRates = rates.ToDictionary(x => x.Year, y => y.MiscRate);

            foreach (FullBoe boe in boes)
            {
                ICollection<TravelDTO> travelCollection = allTravels.Where(x => x.BoeID == boe.Id).ToCollection<TravelDTO>();

                WbsDTO boe_WBS = boe.IsMultiClinWbs ? null : allWbs.FirstOrDefault(i => i.Id == boe.WBSID);
                ClinDTO boe_CLIN = boe.IsMultiClinWbs ? null : allClins.FirstOrDefault(i => i.Id == boe.CLINID);

                string clinNumber = boe_CLIN != null ? boe_CLIN.ClinNumber : sEmpty;
                string wbsNumber = boe_WBS != null ? boe_WBS.WbsNumber : sEmpty;

                foreach (TravelDTO travel in travelCollection)
                {
                    foreach (MSTTravelTripType tripType in travel.MSTTravelTrips)
                    {
                        if (boe.IsMultiClinWbs)
                        {
                            wbsNumber = tripType.WbsNumber;
                            clinNumber = tripType.ClinNumber;
                        }

                        PerformingOrgDTO perfOrg = allPerfOrgs.FirstOrDefault(p => p.Id == tripType.PerfOrgID);

                        TravelUnitRow row = new TravelUnitRow()
                        {
                            WBSNum = wbsNumber,
                            CLINNum = clinNumber,
                            PerformingOrg = perfOrg != null ? perfOrg.PerformingOrgName : sEmpty,
                            Mode = tripType.ModeID.GetDescription(),
                            Purpose = tripType.Purpose,
                            TripDate = tripType.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR),
                            People = tripType.NumOfPeople.ToString(),
                            Days = tripType.NumOfDays.ToString(),
                            TravelAgencyFee = 0
                        };

                        if (tripType.ModeID == MSTTravelMode.NonZoneDomestic || tripType.ModeID == MSTTravelMode.NonZoneInternational)
                        {
                            NonZoneTravelCalculation calculationClass = new NonZoneTravelCalculation(
                                tripType.NumOfDays ?? 0,
                                tripType.NumOfPeople ?? 0,
                                tripType.NonZoneNumCars ?? 0,
                                tripType.NonZonePerDiemDaily ?? 0,
                                tripType.NonZoneCarRentalTrans ?? 0,
                                tripType.NonZoneAirfareEstimate ?? 0,
                                airfareEscalationRates,
                                perDiemEscalationRates,
                                miscEscalationRates,
                                tripType.EstimateDate.Year,
                                tripType.TripDate.Year,
                                tripType.ModeID == MSTTravelMode.NonZoneDomestic,
                                fees[(int)tripType.ModeID].TravelAgencyFee,
                                fees[(int)tripType.ModeID].MiscOther,
                                workspace.DecimalPrecision);

                            row.Departure = tripType.NonZoneFrom;
                            row.DestinationLoc = tripType.NonZoneTo;
                            row.Cars = calculationClass.NumberOfCars;
                            row.Fare = calculationClass.AirfareCost;
                            row.MIERate = calculationClass.DailyPerDiemRate;
                            row.MiscRate = calculationClass.MiscOtherDailyFee;
                            row.RentalCarFee = calculationClass.DailyCarRentalRate;
                            row.TravelAgencyFee = calculationClass.TotalTravelAgencyFee;

                            // find the escalation dollars by subtracting the original total cost (found by dividing by the escalation rate multiplier) from the total cost.
                            row.EscalationDollars = calculationClass.EscalationDollars;

                            allNonZoneRows.Add(row);
                        }
                        else
                        {
                            row.Departure = tripType.ZoneOriginName;
                            row.DestinationLoc = string.Format("{0}, {1}", tripType.ZoneDestCity, tripType.ZoneDestinationName);
                            row.ZoneNumber = tripType.ZoneDestinationZone.ToString();

                            allZoneRows.Add(row);
                        }
                    }
                }
            }

            zoneTripRowCount = AddZoneRows(firstSheet, allZoneRows);
            firstSheet.Add(sEmpty);
            firstSheet.Add("Non-Zone");
            AddHeaders(firstSheet);
            AddNonZoneRows(firstSheet, allNonZoneRows);

            return firstSheet;
        }

        /// <summary>
        /// Applies the remaining formatting to the nonzone rows
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="worksheetName">the worksheet name</param>
        public void ApplyNonzoneFormatting(string filePath, string worksheetName, int zoneTripRowCount)
        {
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(filePath, true))
            {
                // Get the specified worksheet part
                WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(spreadsheet, worksheetName);

                // checked block due to potential for overflow
                int firstHeaderIndex = 0;
                checked
                {
                    firstHeaderIndex = STATIC_HEADER_ROW_COUNT + zoneTripRowCount;
                }

                int secondheaderIndex = firstHeaderIndex + 1;
                Cell boldCell = ExcelUtilities.GetCell(worksheetPart, "A6");
                for (int rowIndex = firstHeaderIndex; rowIndex <= secondheaderIndex; rowIndex++)
                {
                    for (long col = 1; col <= 21; col++)
                    {
                        string cellAddress = TableRange.GetColumnName(col) + rowIndex.ToString();
                        Cell cell = ExcelUtilities.GetCell(worksheetPart, cellAddress);
                        if (cell != null && cell.CellValue != null)
                        {
                            cell.StyleIndex = boldCell.StyleIndex;
                        }
                    }
                }

                worksheetPart.Worksheet.Save();
            }
        }

        /// <summary>
        /// Adds the zone rows.
        /// </summary>
        /// <param name="firstSheet">The first sheet.</param>
        /// <param name="allRows">All rows.</param>
        private static int AddZoneRows(ExcelExportWorksheet firstSheet, Collection<TravelUnitRow> allRows)
        {
            Collection<TravelUnitRow> newCollection = new Collection<TravelUnitRow>();
            int totalTrips = 0;
            foreach (TravelUnitRow itemToAdd in allRows)
            {
                TravelUnitRow existingItem = (from x in newCollection
                                              where x.WBSNum == itemToAdd.WBSNum &&
                                                    x.CLINNum == itemToAdd.CLINNum &&
                                                    x.PerformingOrg == itemToAdd.PerformingOrg &&
                                                    x.Mode == itemToAdd.Mode &&
                                                    x.Departure == itemToAdd.Departure &&
                                                    x.DestinationLoc == itemToAdd.DestinationLoc &&
                                                    x.Purpose == itemToAdd.Purpose &&
                                                    x.Year == itemToAdd.Year &&
                                                    x.People == itemToAdd.People &&
                                                    x.Days == itemToAdd.Days
                                              select x).FirstOrDefault();

                if (existingItem == null)
                {
                    newCollection.Add(itemToAdd);
                }
                else
                {
                    existingItem.Trips = existingItem.Trips + itemToAdd.Trips;
                }
                totalTrips += itemToAdd.Trips;
            }
            foreach (TravelUnitRow rowAdded in newCollection)
            {
                firstSheet.Add(rowAdded.WBSNum, 
                    rowAdded.CLINNum,
                    rowAdded.PerformingOrg,
                    rowAdded.Mode,
                    rowAdded.Departure,
                    rowAdded.DestinationLoc,
                    rowAdded.Purpose,
                    rowAdded.TripDate,
                    rowAdded.Trips.ToString(),
                    rowAdded.People,
                    rowAdded.Days,
                    rowAdded.ZoneNumber,
                    NOT_APPLICABLE,
                    NOT_APPLICABLE,
                    NOT_APPLICABLE,
                    NOT_APPLICABLE,
                    NOT_APPLICABLE,
                    NOT_APPLICABLE,
                    NOT_APPLICABLE);
            }

            firstSheet.Add(sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                "Totals",
                sEmpty,
                totalTrips.ToString(),
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                "");

            return newCollection.Count;
        }

        /// <summary>
        /// Adds the non zone rows.
        /// </summary>
        /// <param name="firstSheet">The first sheet.</param>
        /// <param name="allRows">All rows.</param>
        private static void AddNonZoneRows(ExcelExportWorksheet firstSheet, Collection<TravelUnitRow> allRows)
        {
            Collection<TravelUnitRow> newCollection = new Collection<TravelUnitRow>();
            int totalTrips = 0;
            foreach (TravelUnitRow itemToAdd in allRows)
            {
                TravelUnitRow existingItem = (from x in newCollection
                                              where x.WBSNum == itemToAdd.WBSNum &&
                                                    x.CLINNum == itemToAdd.CLINNum &&
                                                    x.PerformingOrg == itemToAdd.PerformingOrg &&
                                                    x.Mode == itemToAdd.Mode &&
                                                    x.Departure == itemToAdd.Departure &&
                                                    x.DestinationLoc == itemToAdd.DestinationLoc &&
                                                    x.Purpose == itemToAdd.Purpose &&
                                                    x.Year == itemToAdd.Year &&
                                                    x.People == itemToAdd.People &&
                                                    x.Days == itemToAdd.Days &&
                                                    x.Cars == itemToAdd.Cars
                                              select x).FirstOrDefault();

                if (existingItem == null)
                {
                    newCollection.Add(itemToAdd);
                }
                else
                {
                    existingItem.Trips = existingItem.Trips + itemToAdd.Trips;
                }
                totalTrips += itemToAdd.Trips;
            }
            foreach (TravelUnitRow rowAdded in newCollection)
            {
                firstSheet.Add(rowAdded.WBSNum,
                    rowAdded.CLINNum,
                    rowAdded.PerformingOrg,
                    rowAdded.Mode,
                    rowAdded.Departure,
                    rowAdded.DestinationLoc,
                    rowAdded.Purpose,
                    rowAdded.TripDate,
                    rowAdded.Trips.ToString(),
                    rowAdded.People,
                    rowAdded.Days,
                    NOT_APPLICABLE,
                    rowAdded.Cars.ToString(),
                    Math.Round(rowAdded.Fare, 2).ToString(),
                    Math.Round(rowAdded.TravelAgencyFee, 2).ToString(),
                    Math.Round(rowAdded.MIERate, 2).ToString(),
                    Math.Round(rowAdded.RentalCarFee, 2).ToString(),
                    Math.Round(rowAdded.MiscRate, 2).ToString(),
                    Math.Round(rowAdded.EscalationDollars, 2).ToString());
            }

            firstSheet.Add(sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                "Totals",
                sEmpty,
                totalTrips.ToString(),
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty);
        }

        /// <summary>
        /// Adds the headers.
        /// </summary>
        /// <param name="firstSheet">The first sheet.</param>
        private static void AddHeaders(ExcelExportWorksheet firstSheet)
        {
            firstSheet.Add(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER,
                                        ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER,
                                        ImportExportConstants.PERF_ORG_COLUMN_HEADER,
                                        "Mode",
                                        "Departure",
                                        "Destination Loc",
                                        "Purpose",
                                        "Trip Date",
                                        "Trips",
                                        "People",
                                        "Days",
                                        "Zone #",
                                        "Cars",
                                        "Fare",
                                        "Travel Agency Fee",
                                        "Per Diem Daily",
                                        "Rental Car Fee",
                                        "Misc Rate",
                                        "Escalation Dollars");
        }
    }
}
