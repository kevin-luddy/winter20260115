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
    /// Data Loader for MST Zone Travel Destination DTO
    /// </summary>
    public class MSTZoneTravelDestinationDTODataLoader : IMSTZoneTravelDestinationDTODataLoader
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MSTZoneTravelDestinationDTODataLoader() { }

        /// <summary>
        /// Gets the Destination DTO using the destination's ID
        /// </summary>
        /// <param name="destinationID">ID of the destination</param>
        /// <returns>Destination DTO of the Destination with the given ID</returns>
        [DbQuery]
        virtual public MSTZoneTravelDestinationDTO GetDestinationByDestinationID(int destinationID)
        {
            return this.GetByIDs(new Collection<int>() { destinationID }).FirstOrDefault();
        }

        /// <summary>
        /// Gets collection of Destination DTOs using a collection of IDs
        /// </summary>
        /// <param name="destinationIDs">IDs of the destinations</param>
        /// <returns>Collection of DTOs for the Destinations with the given IDs</returns>
        [DbQuery]
        virtual public ICollection<MSTZoneTravelDestinationDTO> GetByIDs(ICollection<int> destinationIDs)
        {
            ICollection<MSTZoneTravelDestinationDTO> toReturn = new Collection<MSTZoneTravelDestinationDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from d in gbe.MSTZoneTravelDestinations
                            where destinationIDs.Contains(d.DestinationID)
                            select new MSTZoneTravelDestinationDTO
                            {
                                DestinationID = d.DestinationID,
                                Destination = d.Destination,
                                Abbreviation = d.Abbreviation,
                                Zone = d.Zone
                            }).ToCollection();
            }

            return toReturn;
        }

        /// <summary>
        /// Gets DTOs for all Destinations in the database
        /// </summary>
        /// <returns>Collection of DTOs for all Destinations</returns>
        [DbQuery]
        virtual public ICollection<MSTZoneTravelDestinationDTO> GetAllDestinations()
        {
            ICollection<MSTZoneTravelDestinationDTO> toReturn = new Collection<MSTZoneTravelDestinationDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from d in gbe.MSTZoneTravelDestinations
                            select new MSTZoneTravelDestinationDTO
                            {
                                DestinationID = d.DestinationID,
                                Destination = d.Destination,
                                Abbreviation = d.Abbreviation,
                                Zone = d.Zone
                            }).ToCollection();
            }

            return toReturn;
        }

        /// <summary>
        /// Saves an update to a destination
        /// </summary>
        /// <param name="inDestination">Destination to be updated</param>
        virtual public void SaveDestination(MSTZoneTravelDestinationDTO inDestination)
        {
            if (inDestination == null)
            {
                throw new ArgumentNullException(nameof(inDestination));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.UpdateMSTZoneTravelDestination(inDestination.DestinationID, inDestination.Zone);
            }
        }
    }
}
