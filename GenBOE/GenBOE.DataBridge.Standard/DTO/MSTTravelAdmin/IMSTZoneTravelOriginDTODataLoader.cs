// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    public interface IMSTZoneTravelOriginDTODataLoader
    {
        /// <summary>
        /// Gets Origin DTO for the given ID
        /// </summary>
        MSTZoneTravelOriginDTO GetOriginByOriginID(int inOriginID);

        /// <summary>
        /// Gets Origin DTOs for the given IDs
        /// </summary>
        /// <param name="originIDs">IDs of the Origins</param>
        /// <returns>Collection of DTOs for the Origins of the given IDs</returns>
        ICollection<MSTZoneTravelOriginDTO> GetByIDs(ICollection<int> originIDs);

        /// <summary>
        /// Gets DTOs for all Origins in the database
        /// </summary>
        /// <returns>Collection of DTOs for all Origins in the database</returns>
        ICollection<MSTZoneTravelOriginDTO> GetAllOrigins();

        /// <summary>
        /// Gets the (12) resources for the given Origin
        /// </summary>
        /// <param name="inOriginID">ID of the Origin</param>
        /// <returns>Collection of resources for the given Origin</returns>
        ICollection<MSTZoneTravelResourceDTO> GetOriginResourcesByOriginID(int inOriginID);

        /// <summary>
        /// Checks if the Origin name already exists
        /// </summary>
        /// <param name="origin">Origin to check</param>
        /// <returns>True if an Origin already exists with that name, false if not</returns>
        bool originExists(MSTZoneTravelOriginDTO origin);

        /// <summary>
        /// Upserts/Deletes Origin and its resources
        /// </summary>
        /// <param name="inOrigin">Origin to be saved</param>
        /// <param name="inResources">Resources for the origin - can be null for deletes</param>
        /// <returns>Dictionary of old ID, new ID</returns>
        Dictionary<int, int> SaveOrigin(MSTZoneTravelOriginDTO inOrigin, ICollection<MSTZoneTravelResourceDTO> inResources);
    }
}
