// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;

    public interface ITravelDTODataLoader
    {
        /// <summary>
        /// Get travel data by travel Id.
        /// </summary>
        /// <param name="inTravelID">Travel Id.</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Travel Dto for given Id.</returns>
        TravelDTO GetById(int inTravelID, bool includeRTEFields = false);

        /// <summary>
        /// Gets Travel data by Ids
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        ICollection<TravelDTO> GetByIds(ICollection<int> ids, bool includeRTEFields = false);

        /// <summary>
        /// Gets Travel data by Workspace Id
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        ICollection<TravelDTO> GetByWorkspaceId(int wsId, bool includeRTEFields = false);

        /// <summary>
        /// Gets Travel data by Boe Ids
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        ICollection<TravelDTO> GetByBoeIds(ICollection<int> boeIds, bool includeRTEFields = false);
        
        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        void LoadRTEFields(ICollection<TravelDTO> dtos);

        /// <summary>
        /// Saves a list of Travel elements.
        /// </summary>
        /// <param name="inTravels">Collection of travel elements.</param>
        /// <returns>Old id to New id mapping.</returns>
        Dictionary<int, int> SaveTravels(ICollection<TravelDTO> inTravels);

        /// <summary>
        /// Deletes All Travel Tasks and Trips for a BOE 
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        void DeleteAllTravelTripTaskElements(int boeId);
    }
}