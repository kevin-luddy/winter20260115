// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;
    using System.Collections.Generic;
    using IES.Standard;

    public interface IBOEFormPBOEDTODataLoader : IDataLoader<BOEFormPBOEDTO>
    {
        /// <summary>
        /// Gets BOE Forms for a Workspace.
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>BOE Forms used by Workspace.</returns>
        ICollection<BOEFormPBOEDTO> GetByWorkspaceId(int wsId);

        /// <summary>
        /// Returns a collection of PBOE BOE Form DTOs based on the Collection of Ids.
        /// </summary>
        /// <param name="workspaceId">Capture Ids.</param>
        /// <returns>The matching DTOs.</returns>
        ICollection<PBOEDataDTO> GetPBOEsForWorkspace(int workspaceId);

		/// <summary>
		/// Get a single PBOE from a Workspace ID and PBOE ID.
		/// </summary>
		/// <param name="workspaceId">Workspace ID</param>
		/// <param name="pboeId">PBOE ID</param>
		/// <returns>Single PBOE by Workspace ID and PBOE ID</returns>
		ICollection<PBOEDataDTO> GetPBOEByIDs(int workspaceId, int pboeId);

		/// <summary>
		/// Retrieves the latest version number of the form.
		/// </summary>
		/// <returns>Latest version number of the form.</returns>
		int GetCurrentFormVersion();

        /// <summary>
        /// Gets a list of currently used resources inside a workspace, excluding the boeForm passed in.
        /// </summary>
        /// <param name="wsId">Workspace Id.</param>
        /// <param name="boeFormId">The boe Form Id to not include when finding the in-use resources.</param>
        /// <returns>A list of resource ids used in a workspace.</returns>
        ICollection<int> CurrentlyUsedResources(int wsId, int boeFormId);
    }
}

