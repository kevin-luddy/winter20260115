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

    public interface IProPricerDTODataLoader
    {
        /// <summary>
        /// Gets a ProPricerDto for the given export Id.
        /// </summary>
        /// <param name="inExportID">Export Id.</param>
        /// <param name="scope">The scope for the id.</param>
        /// <returns>ProPricerDto or null if it does not exist.</returns>
        ProPricerDTO GetById(int inExportID, ProPricerScope scope);

        /// <summary>
        /// Gets all ProPricer Exports for a workspace.  This includes all system and workspace level exports.
        /// </summary>
        /// <param name="workspaceId">Workspace Id.</param>
        /// <returns>ProPricerDtos for the system and given workspace.</returns>
        ICollection<ProPricerDTO> GetByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets all System Exports.
        /// </summary>
        /// <returns>ProPricerDtos for the system only.</returns>
        ICollection<ProPricerDTO> GetAllSystemExports();

        /// <summary>
        /// Saves a ProPricerDto.
        /// </summary>
        /// <param name="inProPricer">ProPricerDto</param>
        void SaveProPricerExport(ProPricerDTO inProPricer);

        /// <summary>
        /// Saves a System ProPricerDto.
        /// </summary>
        /// <param name="inProPricer">ProPricerDto</param>
        void SaveSystemProPricerExport(ProPricerDTO inProPricer);

        /// <summary>
        /// Saves an update for a System ProPricer Custom Field Name.
        /// </summary>
        /// <param name="originalName">The original name.</param>
        /// <param name="updatedName">The updated name.</param>
        void SaveSystemProPricerCustomField(string originalName, string updatedName);
    }
}
