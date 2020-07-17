// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class MSTZoneTravelResourceDTODataLoader : IMSTZoneTravelResourceDTODataLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MSTZoneTravelResourceDTODataLoader() { }

        /// <summary>
        /// Gets a resource based on the Origin and Zone
        /// </summary>
        /// <param name="inOriginID">Origin ID</param>
        /// <param name="inZone">Zone number</param>
        /// <param name="isAirfare">bool to note if Airfare mode or not</param>
        /// <returns>Resource DTO for resource with the given origin and zone</returns>
        [DbQuery]
        virtual public MSTZoneTravelResourceDTO GetResourceByOriginZoneAndMode(int inOriginID, int inZone, bool inIsAirfare)
        {
            MSTZoneTravelResourceDTO toReturn = new MSTZoneTravelResourceDTO();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from r in gbe.MSTZoneTravelResources
                            where r.OriginID == inOriginID && r.Zone == inZone && r.isAirfare == inIsAirfare
                            select new MSTZoneTravelResourceDTO
                            {
                                ResourceID = r.ResourceID,
                                Resource = r.Resource,
                                OriginID = r.OriginID,
                                Zone = r.Zone,
                                IsAirfare = r.isAirfare,
                                LookupValue = r.LookupValue,
                                Description = r.Description
                            }).FirstOrDefault();
            }
            
            return toReturn;
        }

        /// <summary>
        /// Gets All Zone Travel Resources from the database
        /// </summary>
        /// <returns>A collection of All Zone Travel Resources from the database</returns>
        [DbQuery]
        public ICollection<MSTZoneTravelResourceDTO> GetAllResources()
        {
            ICollection<MSTZoneTravelResourceDTO> toReturn = new Collection<MSTZoneTravelResourceDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from r in gbe.MSTZoneTravelResources
                            select new MSTZoneTravelResourceDTO
                            {
                                ResourceID = r.ResourceID,
                                Resource = r.Resource,
                                OriginID = r.OriginID,
                                Zone = r.Zone,
                                IsAirfare = r.isAirfare,
                                LookupValue = r.LookupValue,
                                Description = r.Description
                            }).ToCollection();
            }

            return toReturn;
        }
    }
}
