// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    ///  Interface for BoeDTODataloader
    /// </summary>
    public interface IBoeDTODataLoader : IDataLoader<BoeDTO>
    {
        /// <summary>
        /// Get Boes by their Ids
        /// </summary>
        /// <param name="ids">Ids of the BOEs to retrieve</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Collection of BOE DTOs</returns>
        ICollection<BoeDTO> GetByIds(ICollection<int> ids, bool includeRTEFields = false);

        /// <summary>
        /// Gets all BOEs for the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>BOEs for the specified WS</returns>
        ICollection<BoeDTO> GetByWorkspaceId(int workspaceId, bool includeRTEFields = false);

        /// <summary>
        /// Gets BOEs by Wbs Id
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Corresponding Boe Elements</returns>
        ICollection<BoeDTO> GetByWbsId(int wbsId);

        bool CheckIfBoeExistsByCustomFieldId(int inBOEID, int inCustomFieldID);

        bool BoeContainsMaterialElement(int inBoeID);

        bool BoeContainsLaborCostElement(int inBoeID);

        int GetWbsClinBoeXrefId(int? wbsID, int? clinID, int? boeID);

        /// <summary>
        /// Get BOE ids by CLIN ids
        /// </summary>
        /// <param name="clinIds">CLIN ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Collection of BOE Ids</returns>
        ICollection<BoeDTO> GetByClinIds(ICollection<int> clinIds, bool includeRTEFields = false);

        /// <summary>
        /// Get All BOEs
        /// </summary>
        /// <returns>workspaces</returns>
        Collection<BoeDTO> GetAllBoesForMetrics();

        /// <summary>
        /// Get a collection of BOE IDs that are using the resource ID
        /// </summary>
        /// <param name="inResourceID"></param>
        /// <returns>BOE IDs whose children elements use a given resource ID</returns>
        ICollection<int> GetIdsByResourceId(int inResourceID);

        /// <summary>
        /// Get workspace variable IDs that are affect by the BOE for Sum Of Boe variables.
        /// </summary>
        /// <param name="inBoeID">BOE ID</param>
        /// <returns>workspace variable IDs</returns>
        ICollection<int> GetWorkspaceVariableIdsByBoeId(int inBoeID);

        ICollection<int> GetTaskVariableIdsByBoeId(int inBoeID);

        /// <summary>
        /// Returns true if the Boe belongs to the given workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="boeId">Boe Id</param>
        /// <returns>True/False</returns>
        bool DoesWorkspaceContainBoe(int workspaceId, int boeId);

        /// <summary>
        /// Gets Boe's state
        /// </summary>
        /// <param name="boeId">Boe Id</param>
        /// <returns>Boe's State</returns>
        BOEState GetBoeState(int boeId);

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        void LoadRTEFields(ICollection<BoeDTO> dtos);

		/// <summary>
		/// Get MultiClin BOE based on matching Clin
		/// </summary>
		/// <param name="clinId"></param>
		/// <returns>BOEIds that are found</returns>
		ICollection<int> GetMultiClinBOEIdsByClins(int clinId);

	}
}