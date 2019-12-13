// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
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

    public class MSTTravelNonzoneFeesAndCostsDTODataLoader : IMSTTravelNonzoneFeesAndCostsDTODataLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MSTTravelNonzoneFeesAndCostsDTODataLoader () { }

        /// <summary>
        /// Gets the Fees And Costs DTO based on the Travel Mode ID
        /// </summary>
        /// <param name="modeID">Travel Mode ID</param>
        /// <returns>DTO for the Mode ID</returns>
        [DbQuery]
        virtual public MSTTravelNonzoneFeesAndCostsDTO getFeesAndCostsByModeID(int modeID)
        {
            MSTTravelNonzoneFeesAndCostsDTO toReturn = new MSTTravelNonzoneFeesAndCostsDTO();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from f in gbe.MSTTravelNonzoneFeesAndCosts
                            where f.ModeID == modeID
                            select new MSTTravelNonzoneFeesAndCostsDTO
                            {
                                Id = f.FeesAndCostsID,
                                ModeID = f.ModeID,
                                TravelAgencyFee = f.TravelAgencyFee,
                                MiscOther = f.MiscOther,
                                UpdateDate = f.UpdateDT
                            }).FirstOrDefault();
            }

            return toReturn;
        }

        /// <summary>
        /// Gets all Fees And Costs DTOs
        /// </summary>
        /// <returns>Collection of all Fees and Costs DTOs</returns>
        [DbQuery]
        virtual public ICollection<MSTTravelNonzoneFeesAndCostsDTO> getAllFeesAndCosts()
        {
            ICollection<MSTTravelNonzoneFeesAndCostsDTO> toReturn = new Collection<MSTTravelNonzoneFeesAndCostsDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from f in gbe.MSTTravelNonzoneFeesAndCosts
                            select new MSTTravelNonzoneFeesAndCostsDTO
                            {
                                Id = f.FeesAndCostsID,
                                ModeID = f.ModeID,
                                TravelAgencyFee = f.TravelAgencyFee,
                                MiscOther = f.MiscOther,
                                UpdateDate = f.UpdateDT
                            }).ToCollection();
            }

            return toReturn;
        }

        /// <summary>
        /// Save updates to a Travel Mode's Fee and Cost
        /// </summary>
        /// <param name="feeAndCost">Fee And Cost DTO to be saved</param>
        [DbQuery]
        virtual public void saveFeesAndCosts(MSTTravelNonzoneFeesAndCostsDTO feeAndCost)
        {
            if (feeAndCost == null)
            {
                throw new ArgumentNullException(nameof(feeAndCost));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.UpdateMSTTravelNonzoneFeesAndCosts(feeAndCost.ModeID, feeAndCost.TravelAgencyFee, feeAndCost.MiscOther);
            }
        }
    }
}
