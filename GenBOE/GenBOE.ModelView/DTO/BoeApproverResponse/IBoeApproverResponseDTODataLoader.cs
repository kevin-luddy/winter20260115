// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IBoeApproverResponseDTODataLoader : IDataLoader<BoeApproverResponseDTO>
    {
        /// <summary>
        /// Gets a list of approver responses associated with a Boe.
        /// </summary>
        /// <param name="boeId">Boe Id.</param>
        /// <returns>Approver responses associated to a Boe.</returns>
        ICollection<BoeApproverResponseDTO> GetByBoeId(int boeId);
        
        /// <summary>
        /// Gets a list of approver responses associated with a Boe int the boeIds.
        /// </summary>
        /// <param name="boeIds">Boe Ids to get approver responses for.</param>
        /// <returns>Approver responses associated to the Boes.</returns>
        ICollection<BoeApproverResponseDTO> GetByBoeIds(ICollection<int> boeIds);

        /// <summary>
        /// Gets a list of approver user ids associated with Boes inside a workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace to get approval user Ids for.</param>
        /// <returns>List of approver user ids associated with Boes inside a workspace.</returns>
        ICollection<int> GetApprovalUserIdsByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets a list of approver responses associated with a Workspace.
        /// </summary>
        /// <param name="workspaceId">The Workspace.</param>
        /// <returns>Approver responses associated to a Workspace.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<int, ICollection<BoeApproverResponseDTO>> GetByWorkspaceId(int workspaceId);
    }
}
