// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;

    public interface IMSTZoneTravelResourceDTODataLoader
    {
        /// <summary>
        /// Gets a resource based on the Origin and Zone
        /// </summary>
        /// <param name="inOriginID">Origin ID</param>
        /// <param name="inZone">Zone number</param>
        /// <param name="isAirfare">bool to note if Airfare mode or not</param>
        /// <returns>Resource DTO for resource with the given origin and zone</returns>
        MSTZoneTravelResourceDTO GetResourceByOriginZoneAndMode(int inOriginID, int inZone, bool inIsAirfare);
    }
}
