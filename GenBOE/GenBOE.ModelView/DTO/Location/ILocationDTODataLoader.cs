// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Interface for LocationDTODataLoader
    /// </summary>
    public interface ILocationDTODataLoader : IDataLoader<LocationDTO>
    {
        /// <summary>
        /// Gets all locations
        /// </summary>
        /// <returns>Collection of LocationDTO</returns>
        ICollection<LocationDTO> GetAllLocations();

        /// <summary>
        /// Gets the location name of the entry with the id
        /// </summary>
        /// <param name="inLocationId">id of the location entry</param>
        /// <returns>location name</returns>
        string GetLocationName(int inLocationId);

        /// <summary>
        /// Gets ids of all location entries
        /// </summary>
        /// <returns>Collection of location ids</returns>
        ICollection<int> GetAllLocationIds();
    }
}
