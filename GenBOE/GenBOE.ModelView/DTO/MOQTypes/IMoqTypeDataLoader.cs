// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.ActionLogic.ModelView;

    /// <summary>
    /// Interface for the MOQ Type Data Loader
    /// </summary>
    public interface IMoqTypeDataLoader
    {
        /// <summary>
        /// Get all MOQ Type Selections for the given Workspace ID
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <returns>MOQ Type Selections for the given Workspace ID</returns>
        ICollection<MoqTypeSelection> GetByWorkspaceId(int workspaceId);

        /// <summary>
        /// Get all MOQ Type Selections for the given BOE ID
        /// </summary>
        /// <param name="boeId">BOE ID</param>
        /// <returns>MOQ Type Selections for the given BOE ID</returns>
        ICollection<MoqTypeSelection> GetByBoeId(int boeId);
    }
}
