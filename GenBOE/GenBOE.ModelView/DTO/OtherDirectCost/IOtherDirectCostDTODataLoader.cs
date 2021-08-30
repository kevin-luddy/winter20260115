// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;

    public interface IOtherDirectCostDTODataLoader
    {
        /// <summary>
        /// get ODC by ODC ID
        /// </summary>
        /// <param name="inODCID">ODC Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        OtherDirectCostDTO GetById(int inODCID, bool includeRTEFields = false);
        
        /// <summary>
        /// Gets data by Ids
        /// </summary>
        /// <param name="odcIds">Odc Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        ICollection<OtherDirectCostDTO> GetByIds(ICollection<int> odcIds, bool includeRTEFields = false);

        /// <summary>
        /// Gets data by Boe Ids
        /// </summary>
        /// <param name="odcIds">Odc Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        ICollection<OtherDirectCostDTO> GetByBoeIds(ICollection<int> boeIds, bool includeRTEFields = false);

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        void LoadRTEFields(ICollection<OtherDirectCostDTO> dtos);
    }
}
