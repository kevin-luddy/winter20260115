// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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

    /// <summary>
    /// Data Loader for MST Zone Travel Origin DTO
    /// </summary>
    public class MSTZoneTravelOriginDTODataLoader : IMSTZoneTravelOriginDTODataLoader
    {
        private Logger _log = new Logger(typeof(MSTZoneTravelOriginDTODataLoader));

        /// <summary>
        /// Constructor
        /// </summary>
        public MSTZoneTravelOriginDTODataLoader() { }

        /// <summary>
        /// Gets Origin DTO for the given ID
        /// </summary>
        /// <param name="inOriginID">ID of the Origin</param>
        /// <returns>DTO for Origin of the given ID</returns>
        [DbQuery]
        virtual public MSTZoneTravelOriginDTO GetOriginByOriginID(int inOriginID)
        {
            return this.GetByIDs(new Collection<int> { inOriginID }).FirstOrDefault();
        }

        /// <summary>
        /// Gets Origin DTOs for the given IDs
        /// </summary>
        /// <param name="originIDs">IDs of the Origins</param>
        /// <returns>Collection of DTOs for the Origins of the given IDs</returns>
        [DbQuery]
        virtual public ICollection<MSTZoneTravelOriginDTO> GetByIDs(ICollection<int> originIDs)
        {
            ICollection<MSTZoneTravelOriginDTO> toReturn = new Collection<MSTZoneTravelOriginDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from o in gbe.MSTZoneTravelOrigins
                           where originIDs.Contains(o.OriginID)
                           select new MSTZoneTravelOriginDTO
                           {
                               OriginID = o.OriginID,
                               Origin = o.Origin,
                               Site = o.Site
                           }).ToCollection();
            }

            return toReturn;
        }

        /// <summary>
        /// Gets DTOs for all Origins in the database
        /// </summary>
        /// <returns>Collection of DTOs for all Origins in the database</returns>
        [DbQuery]
        virtual public ICollection<MSTZoneTravelOriginDTO> GetAllOrigins()
        {
            ICollection<MSTZoneTravelOriginDTO> toReturn = new Collection<MSTZoneTravelOriginDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from o in gbe.MSTZoneTravelOrigins
                            select new MSTZoneTravelOriginDTO
                            {
                                OriginID = o.OriginID,
                                Origin = o.Origin,
                                Site = o.Site
                            }).ToCollection();
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the (12) resources for the given Origin
        /// </summary>
        /// <param name="inOriginID">ID of the Origin</param>
        /// <returns>Collection of resources for the given Origin</returns>
        [DbQuery]
        virtual public ICollection<MSTZoneTravelResourceDTO> GetOriginResourcesByOriginID(int inOriginID)
        {
            ICollection<MSTZoneTravelResourceDTO> toReturn = new Collection<MSTZoneTravelResourceDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from r in gbe.MSTZoneTravelResources
                            where inOriginID.Equals(r.OriginID)
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

        /// <summary>
        /// Checks if the Origin name already exists
        /// </summary>
        /// <param name="origin">Origin to check</param>
        /// <returns>True if an Origin already exists with that name, false if not</returns>
        [DbQuery]
        virtual public bool originExists(MSTZoneTravelOriginDTO origin)
        {
            bool toReturn = false;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from o in gbe.MSTZoneTravelOrigins
                              where origin.Origin == o.Origin && origin.OriginID != o.OriginID
                              select o.OriginID).Any();
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts/Deletes Origin and its resources
        /// </summary>
        /// <param name="inOrigin">Origin to be saved</param>
        /// <param name="inResources">Resources for the origin - can be null for deletes</param>
        /// <returns>Dictionary of old ID, new ID</returns>
        virtual public Dictionary<int, int> SaveOrigin(MSTZoneTravelOriginDTO inOrigin, ICollection<MSTZoneTravelResourceDTO> inResources)
        {
            if (inOrigin == null)
            {
                throw new ArgumentNullException(nameof(inOrigin));
            }
            if (inResources == null) { inResources = new List<MSTZoneTravelResourceDTO>(); }

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            if(inOrigin.Updateable == UpdateType.Upsert)
            {
                // Set any blank resources to "NO-RATE"
                foreach (MSTZoneTravelResourceDTO resource in inResources)
                {
                    if (string.IsNullOrEmpty(resource.Resource))
                    {
                        resource.Resource = Constants.ZONE_TRAVEL_RESOURCE_NO_RATE;
                    }
                }

                toReturn.Add(inOrigin.OriginID, UpsertOrigin(inOrigin, inResources.ToCollection()));
            }
            else if (inOrigin.Updateable == UpdateType.Deleted)
            {
                DeleteOrigin(inOrigin);
                toReturn.Add(inOrigin.OriginID, inOrigin.OriginID);
            }
            else
            {
                throw new ArgumentException("Please supply the Updateable argument");
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts (Updates or Inserts) Origin and its resources
        /// </summary>
        /// <param name="inOrigin">Origin to be upserted</param>
        /// <param name="inResources">Origin's resources</param>
        /// <returns>ID of Origin</returns>
        protected int UpsertOrigin(MSTZoneTravelOriginDTO inOrigin, Collection<MSTZoneTravelResourceDTO> inResources)
        {
            if (inOrigin == null)
            {
                throw new ArgumentNullException(nameof(inOrigin));
            }
            if (inResources == null)
            {
                throw new ArgumentNullException(nameof(inResources));
            }

            int id = 0;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    id = Convert.ToInt32(gbe.upsertMSTZoneTravelOrigin(inOrigin.OriginID, inOrigin.Origin, inOrigin.Site,
                        inResources[0].ResourceID, inResources[0].Resource,
                        inResources[1].ResourceID, inResources[1].Resource,
                        inResources[2].ResourceID, inResources[2].Resource,
                        inResources[3].ResourceID, inResources[3].Resource,
                        inResources[4].ResourceID, inResources[4].Resource,
                        inResources[5].ResourceID, inResources[5].Resource,
                        inResources[6].ResourceID, inResources[6].Resource,
                        inResources[7].ResourceID, inResources[7].Resource,
                        inResources[8].ResourceID, inResources[8].Resource,
                        inResources[9].ResourceID, inResources[9].Resource,
                        inResources[10].ResourceID, inResources[10].Resource,
                        inResources[11].ResourceID, inResources[11].Resource).FirstOrDefault());
                }
            }

            return id;
        }

        /// <summary>
        /// Deletes Origin and its Resources
        /// </summary>
        /// <param name="inOrigin">Origin to be deleted</param>
        protected void DeleteOrigin(MSTZoneTravelOriginDTO inOrigin)
        {
            if (inOrigin == null)
            {
                throw new ArgumentNullException(nameof(inOrigin));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteMSTZoneTravelOrigin(inOrigin.OriginID);
                }
            }
        }
    }
}
