// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public class TripsExporter
    {
        private ITripDTODataLoader _tripLoader;
        private IMiscTravelRateDTOLoader miscTravelRateLoader;
        private IUserDTODataLoader _userLoader;
        private IPerDiemDTODataLoader _perDiemLoader;
        private ILocationDTODataLoader _locationLoader;

        public TripsExporter(ITripDTODataLoader inTripLoader,
            IMiscTravelRateDTOLoader inMiscTravelRateLoader,
            IUserDTODataLoader inuserLoader,
            IPerDiemDTODataLoader inPerDiemMLoader,
            ILocationDTODataLoader inLocationLoader)
        {
            this._tripLoader = inTripLoader;
            this.miscTravelRateLoader = inMiscTravelRateLoader;
            this._userLoader = inuserLoader;
            this._perDiemLoader = inPerDiemMLoader;
            this._locationLoader = inLocationLoader;
        }

        /// <summary>
        /// Provides a blank template for Export
        /// </summary>
        /// <param name="templateFileLocation"></param>
        /// <returns></returns>
        public string ExportTemplate(string templateFileLocation)
        {
            return this.ExportToExcelFile(templateFileLocation, true);
        }

        /// <summary>
        /// Exports all data to the Excel file
        /// </summary>
        /// <param name="templateFileLocation"></param>
        /// <returns></returns>
        public string ExportToExcelFile(string templateFileLocation)
        {
            return this.ExportToExcelFile(templateFileLocation, false);
        }

        /// <summary>
        /// Exports to an excel file.
        /// </summary>
        /// <param name="templateFileLocation"></param>
        /// <param name="isTemplate"></param>
        /// <returns></returns>
        private string ExportToExcelFile(string templateFileLocation, bool isTemplate)
        {
            string toReturn = null;

            Collection<MiscTravelRateDTO> miscTravelRates = this.miscTravelRateLoader.GetAll();

            int maxRows = miscTravelRates.Count;

            // Create collections of strings for each row in the export file
            var optionsListWorksheet = new ExcelExportWorksheet("Options Lists");
            var firstWorksheet = new ExcelExportWorksheet();

            for (int i = 0; i < maxRows; i++)
            {
                optionsListWorksheet.Add(
                    miscTravelRates.ElementAtOrDefault(i) != null ? miscTravelRates[i].MiscTravelRateMode : string.Empty);
            }

            if (!isTemplate)
            {
                HashSet<TripDTO> allTrips = new HashSet<TripDTO>(this._tripLoader.GetAllTrips());
                HashSet<PerDiemDTO> allPerDiems = new HashSet<PerDiemDTO>(this._perDiemLoader.GetAllPerDiem());
                HashSet<LocationDTO> allLocations = new HashSet<LocationDTO>(this._locationLoader.GetAllLocations());
                HashSet<UserDTO> allFareUpdaters = new HashSet<UserDTO>(this._userLoader.GetByIds(allTrips.Where(t => t.FareLastUpdatedDate.HasValue).Select(t => t.FareUpdatedByUserID).ToCollection<int>()));
                HashSet<UserDTO> allPerDiemUpdaters = new HashSet<UserDTO>(this._userLoader.GetByIds(allPerDiems.Where(t => t.LastUpdatedBy.HasValue).Select(t => t.LastUpdatedBy.Value).ToCollection<int>()));

                foreach (TripDTO trip in allTrips)
                {
                    if (!trip.LockedRate)
                    {
                        MiscTravelRateDTO miscTravelRate = miscTravelRates.First(x => x.Id == trip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(x => x.Id == trip.PerDiemID);
                        LocationDTO destination = allLocations.First(x => x.Id == trip.DestinationLocationID);
                        LocationDTO departure = allLocations.First(x => x.Id == trip.DepartureLocationID);
                        UserDTO fareUpdater = allFareUpdaters.FirstOrDefault(x => x.UserID == trip.FareUpdatedByUserID);
                        UserDTO perDiemUpdater = allPerDiemUpdaters.FirstOrDefault(x => x.UserID == perDiem.LastUpdatedBy);

                        firstWorksheet.Add(
                            trip.TripID.ToString(),
                            miscTravelRate.MiscTravelRateMode,
                            departure.LocationName,
                            trip.DepartureLocationCode,
                            perDiem.PerDiemDestination,
                            destination.LocationName,
                            trip.DestinationLocationCode,
                            perDiem.Qualification,
                            trip.Fare.ToString(),
                            perDiem.HotelRate.ToString(),
                            perDiem.MIERate.ToString(),
                            perDiem.PerDiemNotes,
                            trip.RentalCarRate.ToString(),
                            trip.RTMiles.ToString(),
                            trip.FareLastUpdatedDate.HasValue ? trip.FareLastUpdatedDate.Value.ToShortDateString() : string.Empty,
                            perDiem.PerDiemLastUpdatedDate.ToShortDateString(),
                            fareUpdater != null ? fareUpdater.DisplayName : string.Empty,
                            perDiem.LastUpdatedBy.HasValue && perDiemUpdater != null ? perDiemUpdater.DisplayName : string.Empty,
                            trip.LastUsedDate.HasValue ? trip.LastUsedDate.Value.ToShortDateString() : string.Empty,
                            trip.TripCount.ToString()
                            );
                    }
                }
            }

            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, optionsListWorksheet, firstWorksheet);

            // Adjust Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { "Mode", miscTravelRates.Count }
            };

            ExcelExporter.AdjustDefinedNames(toReturn, lengths);

            return toReturn;
        }
    }
}
