// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    ///  Interface for ProjectMapDataLoader
    /// </summary>
    public interface IProjectMapDataLoader : IBulkDataLoader<ProjectMapModelView>
    {
        /// <summary>
        /// Gets all ProjectMapModelViews for the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>ProjectMapModelViews for the specified WS</returns>
        ICollection<ProjectMapModelView> GetByWorkspaceId(int workspaceId);

        /// <summary>
        /// Delete all data in a WS. Used by project map, for kill / fill
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="updateDate">Last Update Date</param>
        void DeleteAllInWs(int workspaceId, DateTime updateDate);

        /// <summary>
        /// Gets the paged data.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="page">The page.</param>
        /// <returns>A page of data for the front-end.</returns>
        ProjectMapPageModelView GetProjectMapPagedData(int workspaceId, int page);
    }
}