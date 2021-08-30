// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    /// <summary>
    /// Interface for BOEHistoryDataLoader
    /// </summary>
    public interface IBOEHistoryDTODataLoader
    {
        /// <summary>
        /// Get the BOE History DTO for a given BOE ID
        /// </summary>
        /// <param name="inBoeID">BOE ID</param>
        /// <returns>all the BOE History logs</returns>
        ICollection<BOEHistoryDTO> GetBOEHistory(int inBoeID);

        /// <summary>
        /// Used during exports, this methods gets (for every BoeId in the WS) the last user to submit the Boe for approval
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>Mapping of BoeIds and UserIds</returns>
        Dictionary<int, UserDTO> GetBoeIdsAndAuthorsThatLastSubmittedItForApprovalForWs(int wsId);
    }
}
