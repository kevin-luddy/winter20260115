// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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

    [ExcludeFromCodeCoverage]
    public class TravelExtendedCostExporterRMS : ITravelExtendedCostExporter
    {
        private static readonly string sEmpty = string.Empty;
        private const string NOT_APPLICABLE = "N/A";
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;
        private IPerformingOrgDTODataLoader perfOrgLoader;
        private const int STATIC_HEADER_ROW_COUNT = 9;

        /// <summary>
        /// Initializes a new instance of the <see cref="TravelExtendedCostExporterRMS"/> class.
        /// </summary>
        public TravelExtendedCostExporterRMS(RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader, IPerformingOrgDTODataLoader perfOrgLoader)
        {
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
            this.perfOrgLoader = perfOrgLoader;
        }

        /// <summary>
        /// Exports the report to an excel file
        /// </summary>
        /// <param name="templateFileLocation">template file location</param>
        /// <param name="inWorkspace">the workspace</param>
        /// <returns>export file location</returns>
        public string ExportToExcelFile(string templateFileLocation, FullWorkspace inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            string toReturn = null;
            int zoneTripRowCount;
            ExcelExportWorksheet firstSheet = this.GetWorksheetData(inWorkspace, out zoneTripRowCount);

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
        /// <param name="inWorkspace">workspace</param>
        /// <param name="zoneTripRowCount"></param>
        /// <returns>worksheet data</returns>
        private ExcelExportWorksheet GetWorksheetData(FullWorkspace inWorkspace, out int zoneTripRowCount)
        {
            if(inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            ExcelExportWorksheet firstSheet = new ExcelExportWorksheet();

            WorkspaceDTO workspace = inWorkspace;

            IReadOnlyCollection<WorkspaceHistoryDTO> workspaceHistory = inWorkspace.WorkspaceHistory;

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

            firstSheet.Add("Proposal Name: " + workspaceName, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, sEmpty, "Extended Cost Report");
            firstSheet.Add("Proposal Tracking Number: " + workspaceTrackingNumber);
            firstSheet.Add("Travel rates as of: " + travelRatesDate);
            firstSheet.Add("Submittal Date: " + submittalDate);
            firstSheet.Add("Zone");
            AddHeaders(firstSheet);

            Collection<TravelExtendedRow> allZoneRows = new Collection<TravelExtendedRow>();
            Collection<TravelExtendedRow> allNonZoneRows = new Collection<TravelExtendedRow>();

            // Bulk load all travel data for the workspace.
            List<FullBoe> boes = inWorkspace.Boes.ToList();
            HashSet<ClinDTO> allClins = new HashSet<ClinDTO>(inWorkspace.Clins);
            HashSet<WbsDTO> allWbs = new HashSet<WbsDTO>(inWorkspace.WbsElements);
            HashSet<TravelDTO> allTravels = new HashSet<TravelDTO>(inWorkspace.Travels);
            Collection<PerformingOrgDTO> allPerfOrgs = this.perfOrgLoader.GetByListId(workspace.PerfOrgListID);

            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.zoneTravelRatesFeesLoader.getAllFeesAndCostsByWorkspace(workspace.Id).ToDictionary(f => f.ModeID);

            ICollection<WorkspaceRMSEscalationRatesDTO> rates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(workspace.Id);
            Dictionary<int, decimal> airfareEscalationRates = rates.ToDictionary(x => x.Year, y => y.AirfareRate);
            Dictionary<int, decimal> perDiemEscalationRates = rates.ToDictionary(x => x.Year, y => y.PerDiemRate);
            Dictionary<int, decimal> miscEscalationRates = rates.ToDictionary(x => x.Year, y => y.MiscRate);

            foreach (FullBoe boe in boes)
            {
                ICollection<TravelDTO> travelCollection = allTravels.Where(i => i.BoeID == boe.Id).ToCollection<TravelDTO>();

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

                        TravelExtendedRow row = new TravelExtendedRow()
                        {
                            WBSNum = wbsNumber,
                            CLINNum = clinNumber,
                            PerformingOrg = perfOrg != null ? perfOrg.PerformingOrgName : sEmpty,
                            Mode = tripType.ModeID.GetDescription(),
                            Purpose = tripType.Purpose,
                            TripDate = tripType.TripDate.ToString(BOEExporterConstants.DATE_FORMAT_MONTH_YEAR),
                            People = tripType.NumOfPeople ?? 0,
                            Days = tripType.NumOfDays ?? 0
                        };

                        row.Trips = 1;

                        if (tripType.ModeID == MSTTravelMode.NonZoneDomestic || tripType.ModeID == MSTTravelMode.NonZoneInternational)
                        {
                            row.Departure = tripType.NonZoneFrom;
                            row.DestinationLoc = tripType.NonZoneTo;
                            row.Cars = tripType.NonZoneNumCars ?? 0m;

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

                            row.FareTotal = calculationClass.UnescalatedTotalAirfare;
                            row.MIERateTotal = calculationClass.UnescalatedTotalPerDiem;
                            row.MiscRateTotal = calculationClass.UnescalatedTotalOtherMiscDailyFee;
                            row.RentalCarFeeTotal = calculationClass.UnescalatedTotalCarRental;
                            row.travelAgencyFeeTotal = calculationClass.TotalTravelAgencyFee;

                            row.tripCostTotal = calculationClass.TotalTripCost - calculationClass.EscalationDollars;
                            row.escalationDollars = calculationClass.EscalationDollars;
                            row.totalCostTotal = calculationClass.TotalTripCost;

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
            AddNonZoneRowsAndTotals(firstSheet, allNonZoneRows);

            return firstSheet;
        }

        /// <summary>
        /// Applies the remaining formatting to the nonzone rows
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="worksheetName">the worksheet name</param>
        /// <param name="zoneTripRowCount">the zone row count</param>
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
        /// Adds the Non-Zone rows and totals row to the worksheet.
        /// </summary>
        /// <param name="firstSheet">The first sheet.</param>
        /// <param name="allRows">All rows.</param>
        private static void AddNonZoneRowsAndTotals(ExcelExportWorksheet firstSheet, Collection<TravelExtendedRow> allRows)
        {
            Collection<TravelExtendedRow> newCollection = new Collection<TravelExtendedRow>();
            int tripsTotal = 0;
            decimal fareTotal = 0;
            decimal travelAgencyFeeTotal = 0;
            decimal mieTotal = 0;
            decimal RentalTotal = 0;
            decimal miscTotal = 0;
            decimal tripTotal = 0;
            decimal escalationTotal = 0;
            decimal totalCostTotal = 0;

            foreach (TravelExtendedRow itemToAdd in allRows)
            {
                TravelExtendedRow existingItem = (from x in newCollection
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
                    existingItem.FareTotal += itemToAdd.FareTotal;
                    existingItem.MIERateTotal += itemToAdd.MIERateTotal;
                    existingItem.RentalCarFeeTotal += itemToAdd.RentalCarFeeTotal;
                    existingItem.MiscRateTotal += itemToAdd.MiscRateTotal;
                    existingItem.tripCostTotal += itemToAdd.tripCostTotal;
                    existingItem.escalationDollars += itemToAdd.escalationDollars;
                    existingItem.totalCostTotal += itemToAdd.totalCostTotal;
                    existingItem.travelAgencyFeeTotal += itemToAdd.travelAgencyFeeTotal;
                }
                tripsTotal += itemToAdd.Trips;
                fareTotal += itemToAdd.FareTotal;
                mieTotal += itemToAdd.MIERateTotal;
                RentalTotal += itemToAdd.RentalCarFeeTotal;
                miscTotal += itemToAdd.MiscRateTotal;
                tripTotal += itemToAdd.tripCostTotal;
                escalationTotal += itemToAdd.escalationDollars;
                totalCostTotal += itemToAdd.totalCostTotal;
                travelAgencyFeeTotal += itemToAdd.travelAgencyFeeTotal;
            }


            foreach (TravelExtendedRow rowAdded in newCollection)
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
                                rowAdded.People.ToString(),
                                rowAdded.Days.ToString(),
                                NOT_APPLICABLE,
                                rowAdded.Cars.ToString(),
                                Math.Round(rowAdded.FareTotal, 2).ToString(),
                                Math.Round(rowAdded.travelAgencyFeeTotal, 2).ToString(),
                                Math.Round(rowAdded.MIERateTotal, 2).ToString(),
                                Math.Round(rowAdded.RentalCarFeeTotal, 2).ToString(),
                                Math.Round(rowAdded.MiscRateTotal, 2).ToString(),
                                Math.Round(rowAdded.tripCostTotal, 2).ToString(),
                                Math.Round(rowAdded.escalationDollars, 2).ToString(),
                                Math.Round(rowAdded.totalCostTotal, 2).ToString());
            }
            firstSheet.Add(sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                "Totals",
                sEmpty,
                tripsTotal.ToString(),
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                Math.Round(fareTotal, 2).ToString(),
                Math.Round(travelAgencyFeeTotal, 2).ToString(),
                Math.Round(mieTotal, 2).ToString(),
                Math.Round(RentalTotal, 2).ToString(),
                Math.Round(miscTotal, 2).ToString(),
                Math.Round(tripTotal, 2).ToString(),
                Math.Round(escalationTotal, 2).ToString(),
                Math.Round(totalCostTotal, 2).ToString());
        }

        /// <summary>
        /// Adds the rows and totals row to the worksheet.
        /// </summary>
        /// <param name="firstSheet">The first sheet.</param>
        /// <param name="allRows">All rows.</param>
        private static int AddZoneRows(ExcelExportWorksheet firstSheet, Collection<TravelExtendedRow> allRows)
        {
            Collection<TravelExtendedRow> newCollection = new Collection<TravelExtendedRow>();
            int tripsTotal = 0;
            
            foreach (TravelExtendedRow itemToAdd in allRows)
            {
                TravelExtendedRow existingItem = (from x in newCollection
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
                tripsTotal += itemToAdd.Trips;
            }


            foreach (TravelExtendedRow rowAdded in newCollection)
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
                                rowAdded.People.ToString(),
                                rowAdded.Days.ToString(),
                                rowAdded.ZoneNumber,
                                NOT_APPLICABLE,
                                NOT_APPLICABLE,
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
                tripsTotal.ToString(),
                sEmpty,
                sEmpty,
                sEmpty,
                sEmpty,
                NOT_APPLICABLE,
                NOT_APPLICABLE,
                NOT_APPLICABLE,
                NOT_APPLICABLE,
                NOT_APPLICABLE,
                NOT_APPLICABLE,
                NOT_APPLICABLE,
                NOT_APPLICABLE);

            return newCollection.Count;
        }

        /// <summary>
        /// Adds the headers to the specified first sheet.
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
                                        "Fare Total",
                                        "Travel Agency Fee Total",
                                        "Per Diem Total",
                                        "Rental Car Total",
                                        "Misc Total",
                                        "Trip Total",
                                        "Escalation Dollars",
                                        ImportExportConstants.TOTAL_COST_COLUMN_HEADER);
        }
    }
}
