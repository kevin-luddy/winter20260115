// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.Dtos;

namespace GenBOE.DataBridge.DTO
{
    public interface ITripDTODataLoader
    {
        /// <summary>
        /// Gets all trips from the DB.
        /// </summary>
        /// <returns>All trips from the DB.</returns>
        ICollection<TripDTO> GetAllTrips();

        /// <summary>
        /// Gets a single trip by its Trip Id.  Returns null if trip does not exist.
        /// </summary>
        /// <param name="inTripID">Trip Id.</param>
        /// <returns>Trip or null if not found.</returns>
        TripDTO GetTripByTripID(int inTripID);

        /// <summary>
        /// Gets a collection of trips by a list of trip ids.
        /// </summary>
        /// <param name="tripIds">Trip Ids.</param>
        /// <returns>Trips for the given Ids.</returns>
        ICollection<TripDTO> GetByIds(ICollection<int> tripIds);

        /// <summary>
        /// Saves a collection of trips.
        /// </summary>
        /// <param name="inTrips">Trips to save.</param>
        /// <returns>Dictionary of original Id by new Id.</returns>
        Dictionary<int, int> SaveTrips(Collection<TripDTO> inTrips);

        /// <summary>
        /// Get trip by unique trip data but instead of using location IDs, using location names. This function can be removed once Location Code has been removed from the Location db table
        /// </summary>
        /// <param name="ModeID">mode/rate</param>
        /// <param name="DestinationName">destination location name</param>
        /// <param name="DepartureName">departure location name</param>
        /// <param name="Qualification">qualification</param>
        /// <returns>trips that match criteria</returns>
        ICollection<TripData> GetTripByUniqueTripDataUsingLocationNames(int? ModeID, string DestinationName, string DepartureName, string Qualification);

        /// <summary>
        /// Retrieves the correct <see cref="TripDTO"/>.  Locked/System depending on the state of the <see cref="WorkspaceDTO"/>
        /// </summary>
        /// <param name="inTripID">The ID of the requested <see cref="TripDTO"/></param>
        /// <param name="inWorkspace">The <see cref="WorkspaceDTO"/> to determine the state.</param>
        /// <returns>The requested <see cref="TripDTO"/></returns>
        TripDTO GetTripDTOByTripID(int inTripID, WorkspaceDTO inWorkspace);

        /// <summary>
        /// Retrieves the correct collection <see cref="TripDTO"/> by thier Ids.  Locked/System depending on the state of the <see cref="WorkspaceDTO"/>
        /// </summary>
        /// <param name="inTripID">The Ids of the requested <see cref="TripDTO"/></param>
        /// <param name="inWorkspace">The <see cref="WorkspaceDTO"/> to determine the state.</param>
        /// <returns>The requested collection of <see cref="TripDTO"/></returns>
        ICollection<TripDTO> GetByIds(ICollection<int> inTripIds, WorkspaceDTO inWorkspace);
    }
}
