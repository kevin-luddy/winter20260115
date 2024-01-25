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
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public class TravelExtendedCostExporter : ITravelExtendedCostExporter
    {
        private TripDTODataLoader _TripDTODataLoader;
        private MiscTravelRateDTOLoader miscTravelRateDTOLoader;
        private LocationDTODataLoader _LocationDTODataLoader;
        private TravelTripCostCalculation _TravelTripCostCalculator;
        private PerDiemDTODataLoader _PerDiemDTODataLoader;
        private IEscalationRatesDTOLoader _EscalationLoader;

        private readonly string sEmpty = string.Empty;

        public TravelExtendedCostExporter(
            TripDTODataLoader inTripDTOLoader,
            LocationDTODataLoader inLocationDTODataLoader,
            MiscTravelRateDTOLoader inMiscTravelRateDTOLoader,
            TravelTripCostCalculation inTravelTripCostCalculator,
            PerDiemDTODataLoader inPerDiemDTODataLoader,
            IEscalationRatesDTOLoader inEsclationLoader)
        {
            this._TripDTODataLoader = inTripDTOLoader;
            this._LocationDTODataLoader = inLocationDTODataLoader;
            this.miscTravelRateDTOLoader = inMiscTravelRateDTOLoader;
            this._TravelTripCostCalculator = inTravelTripCostCalculator;
            this._PerDiemDTODataLoader = inPerDiemDTODataLoader;
            this._EscalationLoader = inEsclationLoader;
        }


        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public string ExportToExcelFile(string templateFileLocation, FullWorkspace inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            string toReturn = null;
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

            firstSheet.Add("Proposal Name: " + workspaceName, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "Extended Cost Report");
            firstSheet.Add("Proposal Tracking Number: " + workspaceTrackingNumber);
            firstSheet.Add("Travel rates as of: " + travelRatesDate);
            firstSheet.Add("Submittal Date: " + submittalDate);
            firstSheet.Add("");
            firstSheet.Add(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER,
                            ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER,
                            "Segment",
                            "Mode",
                            "Departure",
                            "Destination Loc",
                            "Purpose",
                            "Year",
                            "Trips",
                            "People",
                            "Days",
                            "Cars",
                            "Fare Total",
                            "Hotel Total",
                            "MIE Total",
                            "Rental Car Total",
                            "Misc Total",
                            "Trip Total",
                            "Escalation Dollars",
                            ImportExportConstants.TOTAL_COST_COLUMN_HEADER);

            Collection<TravelExtendedRow> allRows = new Collection<TravelExtendedRow>();
			// Bulk load all travel data for the workspace.
			IReadOnlyCollection<FullBoe> boes = inWorkspace.Boes;
            HashSet<ClinDTO> allClins = new HashSet<ClinDTO>(inWorkspace.Clins);
            HashSet<WbsDTO> allWbs = new HashSet<WbsDTO>(inWorkspace.WbsElements);
            HashSet<TravelDTO> allTravels = new HashSet<TravelDTO>(inWorkspace.Travels);
            HashSet<TripDTO> allTravelTrips = new HashSet<TripDTO>(this._TripDTODataLoader.GetByIds(allTravels.SelectMany(i => i.TravelTrips).Select(i => i.SystemTripID).ToCollection<int>(), workspace));
            HashSet<PerDiemDTO> allPerDiems = new HashSet<PerDiemDTO>(this._PerDiemDTODataLoader.GetByIds(allTravelTrips.Select(i => i.PerDiemID).ToCollection<int>(), workspace));
            HashSet<LocationDTO> allDepartures = new HashSet<LocationDTO>(this._LocationDTODataLoader.GetByIds(allTravelTrips.Select(i => i.DepartureLocationID).ToCollection<int>()));
            HashSet<LocationDTO> allDestinations = new HashSet<LocationDTO>(this._LocationDTODataLoader.GetByIds(allTravelTrips.Select(i => i.DestinationLocationID).ToCollection<int>()));
            HashSet<EscalationRatesDTO> allEscalations = new HashSet<EscalationRatesDTO>(this._EscalationLoader.GetByWorkspace(workspace));
            HashSet<MiscTravelRateDTO> allMiscTravelRates = new HashSet<MiscTravelRateDTO>(this.miscTravelRateDTOLoader.GetByIds(allTravelTrips.Select(i => i.MiscTravelRateID).ToCollection<int>(), workspace));


            foreach (FullBoe boe in boes)
            {
                ICollection<TravelDTO> travelCollection = allTravels.Where(i => i.BoeID == boe.Id).ToCollection<TravelDTO>();

                WbsDTO boe_WBS = allWbs.FirstOrDefault(i => i.Id == boe.WBSID);
                ClinDTO boe_CLIN = allClins.FirstOrDefault(i => i.Id == boe.CLINID);

                foreach (TravelDTO travel in travelCollection)
                {
                    foreach (TravelTripType tripType in travel.TravelTrips)
                    {
                        TripDTO trip = allTravelTrips.First(i => i.TripID == tripType.SystemTripID);
                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == trip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == trip.PerDiemID);

                        TravelExtendedRow row = new TravelExtendedRow();
                        TravelTripCostData TravelTotals = this._TravelTripCostCalculator.CalculateTravelCost(tripType, inWorkspace, trip, miscRateDTO.MiscTravelRate, perDiem, allEscalations);
                        row.totalCostTotal = TravelTotals.CostTotal;
                        
                        string mode = miscRateDTO.MiscTravelRateMode;
                        LocationDTO departureLocation = allDepartures.First(i => i.Id == trip.DepartureLocationID);
                        LocationDTO destinationLocation = allDestinations.First(i => i.Id == trip.DestinationLocationID); 

                        row.WBSNum = boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty;
                        row.CLINNum = boe_CLIN != null ? boe_CLIN.ClinNumber : this.sEmpty;
                        row.Segment = tripType.Segment.ToString();
                        row.Mode = mode;
                        row.Departure = departureLocation.LocationName;
                        row.DestinationLoc = destinationLocation.LocationName;
                        row.Purpose = tripType.Purpose;
                        row.Year = tripType.TripDate.Year;
                        row.People = tripType.NumOfPeople;
                        row.Days = tripType.NumOfDays;
                        row.Cars = Convert.ToInt32(TravelTotals.NumberOfCars);
                        row.FareTotal = TravelTotals.FareTotal;
                        row.HotelTotal = TravelTotals.HotelTotal;
                        row.MIERateTotal = TravelTotals.MIETotal;
                        row.RentalCarFeeTotal = TravelTotals.RentalCarTotal;
                        row.MiscRateTotal = TravelTotals.MiscTotal;
                        row.tripCostTotal = TravelTotals.TotalCostNoEscalation;
                        row.escalationDollars = TravelTotals.EscalatinDollars;
                        row.Trips = tripType.NumOfTrips;

                        allRows.Add(row);
                    }
                }
            }

            Collection<TravelExtendedRow> newCollection = new Collection<TravelExtendedRow>();
            int tripsTotal = 0;
            decimal fareTotal = 0;
            decimal hotelTotal = 0;
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
                                                        x.Segment == itemToAdd.Segment &&
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
                    existingItem.FareTotal += itemToAdd.FareTotal;
                    existingItem.HotelTotal += itemToAdd.HotelTotal;
                    existingItem.MIERateTotal += itemToAdd.MIERateTotal;
                    existingItem.RentalCarFeeTotal += itemToAdd.RentalCarFeeTotal;
                    existingItem.MiscRateTotal += itemToAdd.MiscRateTotal;
                    existingItem.tripCostTotal += itemToAdd.tripCostTotal;
                    existingItem.escalationDollars += itemToAdd.escalationDollars;
                    existingItem.totalCostTotal += itemToAdd.totalCostTotal;
                }
                tripsTotal += itemToAdd.Trips;
                fareTotal += itemToAdd.FareTotal;
                hotelTotal += itemToAdd.HotelTotal;
                mieTotal += itemToAdd.MIERateTotal;
                RentalTotal += itemToAdd.RentalCarFeeTotal;
                miscTotal += itemToAdd.MiscRateTotal;
                tripTotal += itemToAdd.tripCostTotal;
                escalationTotal += itemToAdd.escalationDollars;
                totalCostTotal += itemToAdd.totalCostTotal;
            }


            foreach (TravelExtendedRow rowAdded in newCollection)
            {
                firstSheet.Add(rowAdded.WBSNum,
                                rowAdded.CLINNum,
                                rowAdded.Segment,
                                rowAdded.Mode,
                                rowAdded.Departure,
                                rowAdded.DestinationLoc,
                                rowAdded.Purpose,
                                rowAdded.Year.HasValue ? rowAdded.Year.Value.ToString() : string.Empty,
                                rowAdded.Trips.ToString(),
                                rowAdded.People.ToString(),
                                rowAdded.Days.ToString(),
                                rowAdded.Cars.ToString(),
                                Math.Round(rowAdded.FareTotal, 2).ToString(),
                                Math.Round(rowAdded.HotelTotal, 2).ToString(),
                                Math.Round(rowAdded.MIERateTotal, 2).ToString(),
                                Math.Round(rowAdded.RentalCarFeeTotal, 2).ToString(),
                                Math.Round(rowAdded.MiscRateTotal, 2).ToString(),
                                Math.Round(rowAdded.tripCostTotal, 2).ToString(),
                                Math.Round(rowAdded.escalationDollars, 2).ToString(),
                                Math.Round(rowAdded.totalCostTotal, 2).ToString());
            }
            firstSheet.Add("",
                "",
                "",
                "",
                "",
                "",
                "Totals",
                "",
                tripsTotal.ToString(),
                "",
                "",
                "",
                Math.Round(fareTotal, 2).ToString(),
                Math.Round(hotelTotal, 2).ToString(),
                Math.Round(mieTotal, 2).ToString(),
                Math.Round(RentalTotal, 2).ToString(),
                Math.Round(miscTotal, 2).ToString(),
                Math.Round(tripTotal, 2).ToString(),
                Math.Round(escalationTotal, 2).ToString(),
                Math.Round(totalCostTotal, 2).ToString());


            ExcelExportWorksheet[] sheets = new ExcelExportWorksheet[1];
            sheets[0] = firstSheet;
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, sheets, new int?[] { 1 });

            return toReturn;
        }
    }

    /// <summary>
    /// Class used for setting up export rows inside Travel Extended Exporter.
    /// </summary>
    public class TravelExtendedRow
    {
        public TravelExtendedRow()
        {
            this.WBSNum = string.Empty;
            this.CLINNum = string.Empty;
            this.Segment = string.Empty;
            this.Mode = string.Empty;
            this.Departure = string.Empty;
            this.DestinationLoc = string.Empty;
            this.Purpose = string.Empty;
            this.Year = null;
            this.Trips = 1;
            this.People = 0;
            this.Days = 0;
            this.Cars = 0;
            this.FareTotal = 0;
            this.HotelTotal = 0;
            this.MIERateTotal = 0;
            this.RentalCarFeeTotal = 0;
            this.MiscRateTotal = 0;
            this.totalCostTotal = 0;
            this.PerformingOrg = string.Empty;
            this.TripDate = string.Empty;
            this.ZoneNumber = string.Empty;
        }

        public string WBSNum { get; set; }
        public string CLINNum { get; set; }
        public string Segment { get; set; }
        public string PerformingOrg { get; set; }
        public string Mode { get; set; }
        public string Departure { get; set; }
        public string DestinationLoc { get; set; }
        public string Purpose { get; set; }
        public int? Year { get; set; }
        public string TripDate { get; set; }
        public int Trips { get; set; }
        public decimal People { get; set; }
        public decimal Days { get; set; }
        public string ZoneNumber { get; set; }
        public decimal Cars { get; set; }
        public decimal FareTotal { get; set; }
        public decimal HotelTotal { get; set; }
        public decimal MIERateTotal { get; set; }
        public decimal RentalCarFeeTotal { get; set; }
        public decimal MiscRateTotal { get; set; }
        public decimal tripCostTotal { get; set; }
        public decimal escalationDollars { get; set; }
        public decimal totalCostTotal { get; set; }

        public decimal travelAgencyFeeTotal { get; set; }
    }
}
