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
    using GenBOE.Dtos;
    using IES.Common;

    public interface IWorkspaceDTODataLoader : IDataLoader<WorkspaceDTO>
    {
        #region Retrieves
        
        /// <summary>
        /// Retrieves Short and Long names for validation purposes ONLY
        /// </summary>
        /// <returns>Partially filled wsDtos</returns>
        Collection<WorkspaceDTO> GetAllWsNamesAndTrackingNumberInfo();

        /// <summary>
        /// This method retrieves PARTIAL workspace data: Short and Long names, Ws Id, State, and the Tracking Number
        /// </summary>
        /// <returns>Partially filled wsDtos</returns>
        ICollection<WorkspaceDTO> GetAllWsNamesForTrackingNumber(string trackingNumber);

        /// <summary>
        /// Get All Workspaces
        /// </summary>
        /// <param name="userId">The user Id.</param>
        /// <returns>workspaces</returns>
        ICollection<GenBOEHomepageWorkspaceRowModelView> GetAllWsForHomepageGrid(int userId);

        /// <summary>
        /// Gets the Workspaces for homepage grid.
        /// </summary>
        /// <param name="workspaceIds">The workspace ids.</param>
        /// <param name="workspaceIdsWithAdmin">The workspace ids with admin.</param>
        /// <param name="userId">The user Id.</param>
        /// <returns>workspaces</returns>
        ICollection<GenBOEHomepageWorkspaceRowModelView> GetWsForHomepageGrid(ICollection<int> workspaceIds, ICollection<int> workspaceIdsWithAdmin, int userId);

        /// <summary>
        /// Get Workspace Data for a given workspace
        /// </summary>
        /// <param name="shortName">shortname</param>
        /// <returns>workspace DTO</returns>
        WorkspaceDTO GetByShortname(string shortName);

        /// <summary>
        /// Gets the workspace email overrides.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>
        /// A collection of workspace email overrides.
        /// </returns>
        ICollection<WorkspaceEmailOverrideDTO> GetWorkspaceEmailOverrides(int workspaceId);

        /// <summary>
        /// Gets the rte fields exceeding limit.
        /// </summary>
        /// <param name="wsId">The ws id.</param>
        /// <returns>A list of fields that exceed the RTE limit</returns>
        ICollection<RTEValidationMV> GetRteFieldsExceedingLimit(int wsId);

        #endregion

        #region Restores and Copies

        void RestoreTravelForWorkspace(int inWorkspaceId);

        void LockTravelAndResourceRatesForWorkspace(int inWorkspaceId);

        /// <summary>
        /// Determine whether there is currently locked rate and/or travel data stored for a workspace.
        /// </summary>
        /// <param name="workspaceId">PKID for the workspace</param>
        /// <returns>True if there is locked data in the database; false if not.</returns>
        bool LockedDataExists(int workspaceId);

        /// <summary>
        /// Check states for all BOEs in the workspace.  If all draft BOEs are locked, then return the BOEID for the last-locked BOE.
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <returns>BOEID for the last-locked BOE, or null if there are BOEs still in (unlocked) draft</returns>
        int? GetLastLockedBOEID(int workspaceId);

        /// <summary>
        /// The ExactCopyWorkspace will copy every aspect of a workspace.
        /// </summary>
        /// <param name="inWorkspaceIDtoCopy">Id of workspace to exact copy</param>
        /// <param name="newWorkspaceName">New unique workspace name.</param>
        /// <param name="newShortName">New unique short name for the workspace.</param>
        int ExactCopyWorkspace(int inWorkspaceIDtoCopy, string newWorkspaceName, string newShortName);

        /// <summary>
        /// Soft Deletes or Restores Workspace
        /// </summary>
        /// <param name="inWorkspaceID">workspace id</param>
        /// <param name="inUpdateDT">Update Date</param>
        /// <param name="inSoftDelete">soft delete workspace/param>
        /// <param name="userID">The User Id.</param>
        void UpdateDeletedStatus(int inWorkspaceID, DateTime inUpdateDT, bool inSoftDelete, int userID);

        #endregion

        #region Commits

        /// <summary>
        /// The SaveWorkspaceSettings will save the following items:
        /// - Inserts/Edits/Deletes to Workspace variables
        /// - Inserts/Edits Workspace Identification and Output Format Template
        /// </summary>
        /// <param name="userID">Curent user ID</param>
        /// <param name="wsToSave">workspace to save</param>
        int SaveWorkspaceSettings(int userID, WorkspaceDTO wsToSave);

        /// <summary>
        /// Save the data within Workspace Identification and the Output Format Template
        /// </summary>
        /// <param name="userID">Curent user ID</param>
        /// <param name="wsToSave">the workspace identification and output format to save</param>
        void SaveIdentificationAndExportFormat(int userID, WorkspaceDTO wsToSave);

        /// <summary>
        /// Save Allow Search
        /// </summary>
        /// <param name="inWorkspace">workspace to save</param>
        void SaveAllowSearch(WorkspaceDTO inWorkspace);

        /// <summary>
        /// If the workspace performing organization list has been restored, set performing organization change flag to false
        /// </summary>
        /// <param name="inWorkspace"></param>
        /// <param name="inPerfOrgChangeFlag"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        void UpdatePerfOrgChangeFlag(WorkspaceDTO inWorkspace, bool inPerfOrgChangeFlag);

        /// <summary>
        /// Inserts a Multi Clin and WBS into the workspace if they dont already have one. 
        /// </summary>
        /// <param name="workspaceID">Workspace ID</param>
        void InsertDefaultMutliValues(int workspaceID);

        /// <summary>
        /// Inserts the report XML.
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <param name="reportXml">The report XML.</param>
        void InsertReportXml(string nonce, string reportXml);

        /// <summary>
        /// Saves the workspace email overrides.
        /// </summary>
        /// <param name="emailOverrides">The email overrides.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        void SaveWorkspaceEmailOverrides(ICollection<WorkspaceEmailOverrideDTO> emailOverrides, int workspaceId);

        /// <summary>
        /// Updates the favorite.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isFavorite">if set to <c>true</c> [is favorite].</param>
        void UpdateFavorite(int workspaceId, int userId, bool isFavorite);

        /// <summary>
        /// Updates the last accessed.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="userId">The user identifier.</param>
        void UpdateLastAccessed(int workspaceId, int userId);

        #endregion
    }
}
