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
    using IES.Standard;
    using GenBOE.Models;
    using GenBOE.Dtos;

    /// <summary>
    /// This loader is exclusively used for retrievals only
    /// </summary>
    public class LocationDTODataLoader : DataLoader<LocationDTO>, ILocationDTODataLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LocationDTODataLoader(ILogger logger)
		{
			this.Log = logger;
		}

        /// <summary>
        /// Gets a collection of LocationDTOs matching the ids.
        /// </summary>
        /// <param name="ids">LocationIDs to find</param>
        /// <returns>Collection of matching LocationDTOs</returns>
        public override ICollection<LocationDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<LocationDTO> toReturn = new Collection<LocationDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from l in gbe.Locations.Where(x => ids.Contains(x.LocationID))
                                select new LocationDTO
                                {
                                    Id = l.LocationID,
                                    LastUpdatedBy = l.UpdatedByETIUserID,
                                    LocationName = l.LocationName,
                                    UpdateDate = l.UpdateDT
                                }).ToCollection<LocationDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get all Locations
        /// </summary>
        /// <returns></returns>
        
        virtual public ICollection<LocationDTO> GetAllLocations()
        {
            ICollection<LocationDTO> locations = new Collection<LocationDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    locations = (from l in gbe.Locations
                                 select new LocationDTO
                                 {
                                     Id = l.LocationID,
                                     LastUpdatedBy = l.UpdatedByETIUserID,
                                     LocationName = l.LocationName,
                                     UpdateDate = l.UpdateDT
                                 }).ToCollection<LocationDTO>();
                }
            }

            return locations;

        }

        /// <summary>
        /// Get location name given location ID
        /// </summary>
        /// <param name="inLocationId"></param>
        /// <returns></returns>
        
        virtual public string GetLocationName(int inLocationId)
        {
            string LocationName = string.Empty;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                LocationName = (from l in gbe.Locations
                                where l.LocationID == inLocationId
                                select l.LocationName).First();
            }

            return LocationName;
        }
        
        /// <summary>
        /// Get all Location IDs
        /// </summary>
        /// <returns></returns>
        
        virtual public ICollection<int> GetAllLocationIds()
        {
            ICollection<int> LocationIds = new Collection<int>();
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                LocationIds = new Collection<int>((from p in gbe.Locations
                                                   select p.LocationID).ToCollection());
            }

            return LocationIds;
        }

        #region Protected

        /// <summary>
        /// Upsert a LocationDTO
        /// </summary>
        /// <param name="dtoToUpsert">dto to upsert</param>
        /// <returns>id of upserted entry</returns>
        protected override int? Upsert(LocationDTO dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                if (dtoToUpsert.Id < 0)
                {
                    // save
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        int passedBackId = Convert.ToInt32(gbe.insertLocation(dtoToUpsert.LocationName, dtoToUpsert.LastUpdatedBy).FirstOrDefault());
                        dtoToUpsert.Id = passedBackId;
                        toReturn = passedBackId;
                    }
                }
                else
                {
                    throw new NotSupportedException("Cannot edit a location");
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletion of a location is not supported.
        /// </summary>
        /// <param name="dtoToDelete">the element to delete</param>
        /// <returns>id of the deleted item</returns>
        protected override int? Delete(LocationDTO dtoToDelete)
        {
            throw new NotSupportedException("Cannot delete a location");
        }

        #endregion
    }
}
