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
    using GenBOE.ActionLogic.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Exports Travel Unit Costs to an Excel Spreadsheet.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TravelUnitCostExporter : ITravelUnitCostExporter
    {
        private TripDTODataLoader _TripDTODataLoader;
        private MiscTravelRateDTOLoader miscTravelRateDTOLoader;
        private PerDiemDTODataLoader _PerDiemDTODataLoader;
        private LocationDTODataLoader _LocationDTODataLoader;
        private IEscalationRatesDTOLoader _EscalationLoader;

        private readonly string sEmpty = string.Empty;

        public TravelUnitCostExporter(
            TripDTODataLoader inTripDTOLoader,
            PerDiemDTODataLoader inPerDiemDTODataLoader,
            LocationDTODataLoader inLocationDTODataLoader,
            MiscTravelRateDTOLoader inMiscTravelRateDTOLoader,
            IEscalationRatesDTOLoader inEsclationLoader)
        {
            this._TripDTODataLoader = inTripDTOLoader;
            this._PerDiemDTODataLoader = inPerDiemDTODataLoader;
            this._LocationDTODataLoader = inLocationDTODataLoader;
            this.miscTravelRateDTOLoader = inMiscTravelRateDTOLoader;
            this._EscalationLoader = inEsclationLoader;
        }


        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public string ExportToExcelFile(string templateFileLocation, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            string toReturn = null;
            ExcelExportWorksheet firstSheet = new ExcelExportWorksheet();

            IReadOnlyCollection<WorkspaceHistoryDTO> workspaceHistory = workspace.WorkspaceHistory;

            var workspaceName = workspace.WorkspaceName;
            var workspaceTrackingNumber = workspace.TrackingNumber;
            var travelRatesDate = DateTime.Now.ToString();
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
            var submittalDate = DateTime.Now.ToString();

            firstSheet.Add("Proposal Name: " + workspaceName, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, "Unit Cost Report");
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
                            "Fare",
                            "Hotel",
                            "MIE",
                            "Rental Car Fee",
                            "Misc Rate",
                            "Escalation Rate");

            Collection<TravelUnitRow> allRows = new Collection<TravelUnitRow>();
            HashSet<ClinDTO> allClins = new HashSet<ClinDTO>(workspace.Clins);
            HashSet<WbsDTO> allWbs = new HashSet<WbsDTO>(workspace.WbsElements);
            // Bulk load all travel data for the workspace.
            HashSet<TravelDTO> allTravels = new HashSet<TravelDTO>(workspace.Travels);
            HashSet<TripDTO> allTravelTrips = new HashSet<TripDTO>(this._TripDTODataLoader.GetByIds(allTravels.SelectMany(i => i.TravelTrips).Select(i => i.SystemTripID).ToCollection<int>(), workspace));
            HashSet<PerDiemDTO> allPerDiems = new HashSet<PerDiemDTO>(this._PerDiemDTODataLoader.GetByIds(allTravelTrips.Select(i => i.PerDiemID).ToCollection<int>(), workspace));
            HashSet<LocationDTO> allDepartures = new HashSet<LocationDTO>(this._LocationDTODataLoader.GetByIds(allTravelTrips.Select(i => i.DepartureLocationID).ToCollection<int>()));
            HashSet<LocationDTO> allDestinations = new HashSet<LocationDTO>(this._LocationDTODataLoader.GetByIds(allTravelTrips.Select(i => i.DestinationLocationID).ToCollection<int>()));
            HashSet<EscalationRatesDTO> allEscalations = new HashSet<EscalationRatesDTO>(this._EscalationLoader.GetByWorkspace(workspace));
            HashSet<MiscTravelRateDTO> allMiscTravelRates = new HashSet<MiscTravelRateDTO>(this.miscTravelRateDTOLoader.GetByIds(allTravelTrips.Select(i => i.MiscTravelRateID).ToCollection<int>(), workspace));

            foreach (var boe in workspace.Boes)
            {
                ICollection<TravelDTO> travelCollection = allTravels.Where(x => x.BoeID == boe.Id).ToCollection<TravelDTO>();

                var boe_WBS = allWbs.FirstOrDefault(i => i.Id == boe.WBSID); 
                var boe_CLIN = allClins.FirstOrDefault(i => i.Id == boe.CLINID);     

                foreach (TravelDTO travel in travelCollection)
                {
                    foreach (TravelTripType tripType in travel.TravelTrips)
                    {
                        TravelUnitRow row = new TravelUnitRow();

                        TripDTO trip = allTravelTrips.First(i => i.TripID == tripType.SystemTripID);

                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == trip.MiscTravelRateID);
                        decimal miscRate = miscRateDTO.MiscTravelRate;
                        string mode = miscRateDTO.MiscTravelRateMode;

                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == trip.PerDiemID);
                        LocationDTO departureLocation = allDepartures.First(i => i.Id == trip.DepartureLocationID);
                        LocationDTO destinationLocation = allDestinations.First(i => i.Id == trip.DestinationLocationID); 

                        int numOfCars = (tripType.NumOfPeople / 2);

                        if (tripType.NumOfPeople % 2 != 0)
                        {
                            numOfCars++;
                        }

                        decimal escalationRate = 0.0m;

                        if (tripType.Segment == SegmentType.LS)
                        {
                            escalationRate = (from e in allEscalations
                                              where tripType.TripDate.Year == e.Year
                                              select e.LMSIEscalation).FirstOrDefault();
                        }
                        else
                        {
                            escalationRate = (from e in allEscalations
                                              where tripType.TripDate.Year == e.Year
                                              select e.DevEscalation).FirstOrDefault();
                        }

                        row.WBSNum = boe_WBS != null ? boe_WBS.WbsNumber : this.sEmpty;
                        row.CLINNum = boe_CLIN != null ? boe_CLIN.ClinNumber : this.sEmpty;
                        row.Segment = tripType.Segment.ToString();
                        row.Mode = mode;
                        row.Departure = departureLocation.LocationName;
                        row.DestinationLoc = destinationLocation.LocationName;
                        row.Purpose = tripType.Purpose;
                        row.Year = tripType.TripDate.Year.ToString();
                        row.People = tripType.NumOfPeople.ToString();
                        row.Days = tripType.NumOfDays.ToString();
                        row.Cars = numOfCars;
                        row.Fare = trip.Fare;
                        row.Hotel = perDiem.HotelRate;
                        row.MIERate = perDiem.MIERate;
                        row.RentalCarFee = trip.RentalCarRate;
                        row.MiscRate = miscRate;
                        row.Trips = tripType.NumOfTrips;
                        row.EscalationRate = escalationRate;

                        allRows.Add(row);
                    }
                }
            }

            Collection<TravelUnitRow> newCollection = new Collection<TravelUnitRow>();
            int totalTrips = 0;
            foreach (TravelUnitRow itemToAdd in allRows)
            {
                TravelUnitRow existingItem = (from x in newCollection
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
                }
                totalTrips += itemToAdd.Trips;
            }


            foreach (TravelUnitRow rowAdded in newCollection)
            {
                firstSheet.Add(rowAdded.WBSNum,
                                rowAdded.CLINNum,
                                rowAdded.Segment,
                                rowAdded.Mode,
                                rowAdded.Departure,
                                rowAdded.DestinationLoc,
                                rowAdded.Purpose,
                                rowAdded.Year,
                                rowAdded.Trips.ToString(),
                                rowAdded.People,
                                rowAdded.Days,
                                rowAdded.Cars.ToString(),
                                rowAdded.Fare.ToString(),
                                rowAdded.Hotel.ToString(),
                                rowAdded.MIERate.ToString(),
                                rowAdded.RentalCarFee.ToString(),
                                rowAdded.MiscRate.ToString(),
                                string.Format("{0:0.000%}", rowAdded.EscalationRate));
            }

            firstSheet.Add(this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty,
                            "Totals", this.sEmpty,
                            totalTrips.ToString(), this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty, this.sEmpty,
                            "");

            ExcelExportWorksheet[] sheets = new ExcelExportWorksheet[1];
            sheets[0] = firstSheet;
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, sheets, new int?[] { 1 });

            return toReturn;
      }

     
    }

public class TravelUnitRow
     {
        public TravelUnitRow()
        {
            this.WBSNum = string.Empty;
            this.CLINNum = string.Empty;
            this.Segment = string.Empty;
            this.Mode = string.Empty;
            this.Departure = string.Empty;
            this.DestinationLoc = string.Empty;
            this.Purpose = string.Empty;
            this.Year = string.Empty;
            this.Trips = 1;
            this.People = string.Empty;
            this.Days = string.Empty;
        }

        public string WBSNum { get; set; }
        public string CLINNum { get; set; }
        public string Segment { get; set; }
        public string Mode { get; set; }
        public string PerformingOrg { get; set; }
        public string Departure { get; set; }
        public string DestinationLoc { get; set; }
        public string Purpose { get; set; }
        public string ZoneNumber { get; set; }
        public string TripDate { get; set; }
        public string Year { get; set; }
        public int Trips { get; set; }
        public string People { get; set; }
        public string Days { get; set; }
        public decimal Cars { get; set; }
        public decimal Fare { get; set; }
        public decimal Hotel { get; set; }

        public decimal TravelAgencyFee { get; set; }
        public decimal MIERate { get; set; }
        public decimal RentalCarFee { get; set; }
        public decimal MiscRate { get; set; }
        public decimal EscalationRate { get; set; }
        public decimal EscalationDollars { get; set; }
    }


}
