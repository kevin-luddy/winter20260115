// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    public interface ITMResourceRateDTODataLoader : IDataLoader<TMResourceRateDTO>
    {
        /// <summary>
        /// Gets all T&M resource rates for the workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>T&M resource rates for a given workspace.</returns>
        ICollection<TMResourceRateDTO> GetByWorkspaceId(int workspaceId);

        /// <summary>
        /// Saves a collection of T&M resource rate DTOs.
        /// </summary>
        /// <param name="inResourceRates">T&M Resource rate DTOs to save.</param>
        /// <returns>Mapping of old Id to new Id.</returns>
        Dictionary<int, int> SaveTMResourceRates(ICollection<TMResourceRateDTO> inResourceRates);
    }
}
