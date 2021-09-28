// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    public interface IMSTZoneTravelDestinationDTODataLoader
    {
        /// <summary>
        /// Gets the Destination DTO using the destination's ID
        /// </summary>
        /// <param name="destinationID">ID of the destination</param>
        /// <returns>Destination DTO of the Destination with the given ID</returns>
        MSTZoneTravelDestinationDTO GetDestinationByDestinationID(int destinationID);

        /// <summary>
        /// Gets collection of Destination DTOs using a collection of IDs
        /// </summary>
        /// <param name="destinationIDs">IDs of the destinations</param>
        /// <returns>Collection of DTOs for the Destinations with the given IDs</returns>
        ICollection<MSTZoneTravelDestinationDTO> GetByIDs(ICollection<int> destinationIDs);

        /// <summary>
        /// Gets DTOs for all Destinations in the database
        /// </summary>
        /// <returns>Collection of DTOs for all Destinations</returns>
        ICollection<MSTZoneTravelDestinationDTO> GetAllDestinations();

        /// <summary>
        /// Saves an update to a destination
        /// </summary>
        /// <param name="inDestination">Destination to be updated</param>
        void SaveDestination(MSTZoneTravelDestinationDTO inDestination);
    }
}
