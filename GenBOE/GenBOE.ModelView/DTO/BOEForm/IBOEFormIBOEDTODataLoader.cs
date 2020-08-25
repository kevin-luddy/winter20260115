// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;
    using System.Collections.Generic;
    using IES.Common;

    public interface IBOEFormIBOEDTODataLoader : IDataLoader<BOEFormIBOEDTO>
    {
        /// <summary>
        /// Gets BOE Forms for a Workspace.
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>BOE Forms used by Workspace.</returns>
        ICollection<BOEFormIBOEDTO> GetByWorkspaceId(int wsId);

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
