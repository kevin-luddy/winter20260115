// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    public interface IMSTTravelNonzoneFeesAndCostsDTODataLoader
    {
        /// <summary>
        /// Gets the Fees And Costs DTO based on the Travel Mode ID
        /// </summary>
        /// <param name="modeID">Travel Mode ID</param>
        /// <returns>DTO for the Mode ID</returns>
        MSTTravelNonzoneFeesAndCostsDTO getFeesAndCostsByModeID(int modeID);

        /// <summary>
        /// Gets all Fees And Costs DTOs
        /// </summary>
        /// <returns>Collection of all Fees and Costs DTOs</returns>
        ICollection<MSTTravelNonzoneFeesAndCostsDTO> getAllFeesAndCosts();

        /// <summary>
        /// Save updates to a Travel Mode's Fee and Cost
        /// </summary>
        /// <param name="feeAndCost">Fee And Cost DTO to be saved</param>
        void saveFeesAndCosts(MSTTravelNonzoneFeesAndCostsDTO feeAndCost);
    }
}
