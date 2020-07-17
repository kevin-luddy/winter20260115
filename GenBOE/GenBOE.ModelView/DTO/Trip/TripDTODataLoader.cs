// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class TripDTODataLoader : GenBOE.DataBridge.DTO.ITripDTODataLoader
    {
        private Logger _log = new Logger(typeof(TripDTODataLoader));
        public TripDTODataLoader() { }

        /// <summary>
        /// Gets a single trip by its Trip Id.  Returns null if trip does not exist.
        /// </summary>
        /// <param name="inTripID">Trip Id.</param>
        /// <returns>Trip or null if not found.</returns>
        [DbQuery]
        virtual public TripDTO GetTripByTripID(int inTripID)
        {
            return this.GetByIds(new Collection<int> { inTripID }).FirstOrDefault();
        }

        /// <summary>
        /// Gets a collection of trips by a list of trip ids.
        /// </summary>
        /// <param name="tripIds">Trip Ids.</param>
        /// <returns>Trips for the given Ids.</returns>
        [DbQuery]
        public ICollection<TripDTO> GetByIds(ICollection<int> tripIds)
        {
            ICollection<TripDTO> trips = new Collection<TripDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                trips = ConvertToDto((from t in gbe.Trips.Include("TravelTrips")
                                      where tripIds.Contains(t.TripID)
                                      select t).ToCollection<Trip>());
            }
            return trips;
        }

        /// <summary>
        /// Gets all trips from the DB.
        /// </summary>
        /// <returns>All trips from the DB.</returns>
        [DbQuery(2)]
        virtual public ICollection<TripDTO> GetAllTrips()
        {
            ICollection<TripDTO> toReturn = new Collection<TripDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var rawCounts = (from t in gbe.Trips
                                     join tt in gbe.TravelTrips on t.TripID equals tt.TripID
                                     group tt by tt.TripID into grp
                                     select new { key = grp.Key, value = grp.Count() });

                    Dictionary<int, int> counts = new Dictionary<int, int>();
                    foreach (var count in rawCounts)
                    {
                        counts.Add(count.key, count.value);
                    }

                    toReturn = ConvertToDto(gbe.Trips.ToCollection(), counts);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get trip by unique trip data but instead of using location IDs, using location names. This function can be removed once Location Code has been removed from the Location db table
        /// </summary>
        /// <param name="ModeID">mode/rate</param>
        /// <param name="DestinationName">destination location name</param>
        /// <param name="DepartureName">departure location name</param>
        /// <param name="Qualification">qualification</param>
        /// <returns></returns>
        [DbQuery(3)]
        virtual public ICollection<TripData> GetTripByUniqueTripDataUsingLocationNames(int? ModeID, string DestinationName, string DepartureName, string Qualification)
        {
            List<TripData> tripListResults = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {

                var allLocations = (from l in gbe.Locations
                                   select l).ToArray();

                var allTrip = (from t in gbe.Trips
                              select t).ToArray();

                // PerDiem is where qualification is stored
                var allPerDiem = (from p in gbe.PerDiems                                  
                                  select p).ToArray();


                // this function should always be at least called with Mode ID
                if (ModeID.HasValue)
                {                    
                    tripListResults = new List<TripData>((from destination in allLocations.Where(x => x.LocationName == DestinationName)
                                                          from departure in allLocations.Where(x => x.LocationName == DepartureName)
                                                          from trip in allTrip.Where(x => x.TravelMiscRateID == ModeID.Value &&
                                                                                          x.DepartureLocationID == departure.LocationID &&
                                                                                          x.DestinationLocationID == destination.LocationID)
                                                          from perdiem in allPerDiem.Where(x => TwoStringCompare(x.Qualification, Qualification) &&
                                                                                                x.PerDiemID == trip.PerDiemID)
                                                          select new TripData
                                                          {
                                                              ModeID = trip.TravelMiscRateID,
                                                              DepartureID = departure.LocationID,
                                                              DepartureName = departure.LocationName,
                                                              DestinationID = destination.LocationID,
                                                              DestinationName = destination.LocationName,
                                                              Qualifciation = perdiem.Qualification,
                                                              TripID = trip.TripID
                                                          }).ToArray());
                }

            }

            return tripListResults;
        }

        private bool TwoStringCompare(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
            {
                return true;
            }
            else if (!string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right))
            {
                return left.ToLower().Equals(right.ToLower());
            }
            else
            {
                // only case left is if 1 is valued and the other is null .. in which case it's false
                return false;
            }
        }

        /// <summary>
        /// Retrieves the correct <see cref="TripDTO"/>.  Locked/System depending on the state of the <see cref="WorkspaceDTO"/>
        /// </summary>
        /// <param name="inTripID">The ID of the requested <see cref="TripDTO"/></param>
        /// <param name="inWorkspace">The <see cref="WorkspaceDTO"/> to determine the state.</param>
        /// <returns>The requested <see cref="TripDTO"/></returns>
        virtual public TripDTO GetTripDTOByTripID(int inTripID, WorkspaceDTO inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            TripDTO toReturn;

                if (inWorkspace.WorkspaceState == WorkspaceState.Locked || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        toReturn = ConvertToDto((from t in gbe.WorkspaceLockedTrips
                                                 where t.TripID == inTripID && t.WorkspaceID == inWorkspace.Id
                                                 select t).ToCollection<WorkspaceLockedTrip>(), gbe).FirstOrDefault();
                    }
                }
                else
                {
                    //get the system level rates
                    toReturn = this.GetTripByTripID(inTripID);
                }

                // With enhancements 35365/35366, BOEs in a Locked workspace can now be editable (and new/unsaved trips created)
                if (toReturn == null && inWorkspace.WorkspaceState == WorkspaceState.Locked)
                {
                    toReturn = this.GetTripByTripID(inTripID);
                }

                 return toReturn;
            }

        /// <summary>
        /// Retrieves the correct collection <see cref="TripDTO"/> by thier Ids.  Locked/System depending on the state of the <see cref="WorkspaceDTO"/>
        /// </summary>
        /// <param name="inTripID">The Ids of the requested <see cref="TripDTO"/></param>
        /// <param name="inWorkspace">The <see cref="WorkspaceDTO"/> to determine the state.</param>
        /// <returns>The requested collection of <see cref="TripDTO"/></returns>
        [DbQuery]
        public ICollection<TripDTO> GetByIds(ICollection<int> inTripIds, WorkspaceDTO inWorkspace)
        {
            if (inWorkspace == null) { throw new ArgumentNullException(nameof(inWorkspace)); }

            ICollection<TripDTO> lockedTrips = new Collection<TripDTO>();
            ICollection<TripDTO> toReturn = new Collection<TripDTO>();

            if (inWorkspace.WorkspaceState == WorkspaceState.Locked || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    lockedTrips = ConvertToDto((from t in gbe.WorkspaceLockedTrips
                                            where inTripIds.Contains(t.TripID) && t.WorkspaceID == inWorkspace.Id
                                            select t).ToCollection<WorkspaceLockedTrip>(), gbe);
                }
            }

            if (lockedTrips.Any() || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
            {
                //Always use locked WS rates in Complete/Closed workspaces
                //When a WS is locked, there will not be WS rates if it contains an unlocked BOE, so system rates should be used
                toReturn = lockedTrips;
            }
            else
            {
                //Workspace rates are not locked
                toReturn = this.GetByIds(inTripIds);
            }

            return toReturn;
        }

        /// <summary>
        /// Saves a collection of trips.
        /// </summary>
        /// <param name="inTrips">Trips to save.</param>
        /// <returns>Dictionary of original Id by new Id.</returns>
        virtual public Dictionary<int, int> SaveTrips(Collection<TripDTO> inTrips)
      
        {
            if (inTrips == null)
            {
                throw new ArgumentNullException(nameof(inTrips));
            }

            Dictionary<int, int> toReturn = new Dictionary<int, int>();
            foreach (TripDTO trip in inTrips)
            {
                if (trip.Updateable == UpdateType.Upsert)
                {
                    toReturn.Add(trip.TripID, UpsertTrip(trip));
                }
                else if (trip.Updateable == UpdateType.Deleted)
                {
                    DeleteTrip(trip);
                    toReturn.Add(trip.TripID, trip.TripID);
                }
                else
                {
                    throw new ArgumentException("please supply the Updateable argument");
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert a trip
        /// </summary>
        /// <param name="inTripData"></param>
        protected int UpsertTrip(TripDTO inTripData)
        {
            // check if the input is null
            if (inTripData == null)
            {
                throw new ArgumentNullException(nameof(inTripData));
            }
            int id = 0;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    id = Convert.ToInt32(gbe.upsertTrip(inTripData.TripID, inTripData.MiscTravelRateID, inTripData.DepartureLocationID, inTripData.DestinationLocationID, inTripData.PerDiemID, inTripData.Fare, inTripData.RTMiles, inTripData.FareUpdatedByUserID, inTripData.UpdateDate, inTripData.RentalCarRate, inTripData.DestinationLocationCode, inTripData.DepartureLocationCode).FirstOrDefault());
                }
            }

            return id;
        }

        /// <summary>
        /// Delete the trip
        /// </summary>
        /// <param name="inTrip"></param>
        protected void DeleteTrip(TripDTO inTrip)
        {
            // check if the input is null
            if (inTrip == null)
            {
                throw new ArgumentNullException(nameof(inTrip));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteTrip(inTrip.TripID, inTrip.UpdateDate);
                }
            }
        }

        #region privates

        /// <summary>
        /// Converts Trip entities to TripDtos.
        /// </summary>
        /// <param name="entities">Trip Entities.</param>
        /// <returns>Converted TripDTOs.</returns>
        private ICollection<TripDTO> ConvertToDto(ICollection<Trip> entities)
        {
            ICollection<TripDTO> trips = new Collection<TripDTO>();

            if (entities.Any())
            {
                foreach (Trip entity in entities)
                {
                    trips.Add(new TripDTO
                    {
                        TripID = entity.TripID,
                        MiscTravelRateID = entity.TravelMiscRateID,
                        DepartureLocationID = entity.DepartureLocationID,
                        DestinationLocationID = entity.DestinationLocationID,
                        Fare = entity.TransportationFare,
                        RTMiles = entity.RoundTripMiles,
                        FareUpdatedByUserID = entity.FareLastUpdateETIUserID,
                        FareLastUpdatedDate =  GenBOEUtilities.AdjustDateTimePrecision((DateTime)entity.UpdateDT, DateTimePrecision.Day),
                        UpdateDate = entity.UpdateDT,
                        InUse = entity.TripInUse,
                        LastUsedDate = entity.LastUsedDT != null ? GenBOEUtilities.AdjustDateTimePrecision((DateTime)entity.LastUsedDT, DateTimePrecision.Day) : entity.LastUsedDT,
                        PerDiemID = entity.PerDiemID,
                        // Indicates the object is from the system Trip table. Locked rates are store in the WorkspaceLockedTrips table.
                        LockedRate = false,
                        RentalCarRate = entity.RentalCarRate,
                        TripCount = entity.TravelTrips.Count,
                        DepartureLocationCode = entity.DepartureLocationCode,
                        DestinationLocationCode = entity.DestinationLocationCode
                    });
                }
            }
            return trips;
        }

        /// <summary>
        /// Converts Trip entities to TripDtos.
        /// </summary>
        /// <param name="entities">Trip Entities.</param>
        /// <returns>Converted TripDTOs.</returns>
        private ICollection<TripDTO> ConvertToDto(ICollection<Trip> entities, Dictionary<int, int> counts)
        {
            ICollection<TripDTO> trips = new Collection<TripDTO>();

            if (entities.Any())
            {
                foreach (Trip entity in entities)
                {
                    int count = 0;
                    if (!counts.TryGetValue(entity.TripID, out count))
                    {
                        count = 0;
                    }

                    trips.Add(new TripDTO
                    {
                        TripID = entity.TripID,
                        MiscTravelRateID = entity.TravelMiscRateID,
                        DepartureLocationID = entity.DepartureLocationID,
                        DestinationLocationID = entity.DestinationLocationID,
                        Fare = entity.TransportationFare,
                        RTMiles = entity.RoundTripMiles,
                        FareUpdatedByUserID = entity.FareLastUpdateETIUserID,
                        FareLastUpdatedDate = GenBOEUtilities.AdjustDateTimePrecision((DateTime)entity.UpdateDT, DateTimePrecision.Day),
                        UpdateDate = entity.UpdateDT,
                        InUse = entity.TripInUse,
                        LastUsedDate = entity.LastUsedDT != null ? GenBOEUtilities.AdjustDateTimePrecision((DateTime)entity.LastUsedDT, DateTimePrecision.Day) : entity.LastUsedDT,
                        PerDiemID = entity.PerDiemID,
                        // Indicates the object is from the system Trip table. Locked rates are store in the WorkspaceLockedTrips table.
                        LockedRate = false,
                        RentalCarRate = entity.RentalCarRate,
                        TripCount = count,
                        DepartureLocationCode = entity.DepartureLocationCode,
                        DestinationLocationCode = entity.DestinationLocationCode
                    });
                }
            }
            return trips;
        }

        /// <summary>
        /// Converts WorkspaceLockedTrip entities to TripDtos.
        /// </summary>
        /// <param name="entities">Trip Entities.</param>
        /// <returns>Converted TripDTOs.</returns>
        private ICollection<TripDTO> ConvertToDto(ICollection<WorkspaceLockedTrip> entities, GenBoeEntities gbe)
        {
            ICollection<TripDTO> trips = new Collection<TripDTO>();

            if (entities.Any())
            {
                ICollection<int> allTripIds = entities.Select(i => i.TripID).ToCollection<int>();
                var travelTrips = (from tp in gbe.TravelTrips
                                   where allTripIds.Contains(tp.TripID)
                                   select new
                                   {
                                       tripId = tp.TripID
                                   }).ToList();

                foreach (WorkspaceLockedTrip entity in entities)
                {
                    trips.Add(new TripDTO
                    {
                        TripID = entity.TripID,
                        MiscTravelRateID = entity.TravelMiscRateID,
                        DepartureLocationID = entity.DepartureLocationID,
                        DestinationLocationID = entity.DestinationLocationID,
                        Fare = entity.TransportationFare,
                        RTMiles = entity.RoundTripMiles,
                        FareUpdatedByUserID = entity.FareLastUpdateETIUserID,
                        FareLastUpdatedDate = GenBOEUtilities.AdjustDateTimePrecision((DateTime)entity.UpdateDT, DateTimePrecision.Day),
                        UpdateDate = entity.UpdateDT,
                        InUse = entity.TripInUse,
                        LastUsedDate = entity.LastUsedDT != null ? GenBOEUtilities.AdjustDateTimePrecision((DateTime)entity.LastUsedDT, DateTimePrecision.Day) : entity.LastUsedDT,
                        PerDiemID = entity.PerDiemID,
                        // Indicates the object is from the system Trip table. Locked rates are store in the WorkspaceLockedTrips table.
                        LockedRate = true,
                        RentalCarRate = entity.RentalCarRate,
                        TripCount = travelTrips.Count(x => x.tripId == entity.TripID),
                        DepartureLocationCode = entity.DepartureLocationCode,
                        DestinationLocationCode = entity.DestinationLocationCode
                    });
                }
            }
            return trips;
        }

        #endregion
    }
}
