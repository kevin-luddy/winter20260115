// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// proposal permmission loader interface
    /// </summary>
    public interface IProposalPermissionLoader : IDataLoader<ProposalPermissionDto>
    {
        /// <summary>
        /// Get all permissions
        /// </summary>
        /// <returns>all permissions</returns>
        System.Collections.Generic.ICollection<ProposalPermissionDto> GetAllProposalPermissions();

        /// <summary>
        /// Get a list of permissions by proposal id
        /// </summary>
        /// <param name="inProposalID">propsoal id</param>
        /// <returns>list of all permissions by that proposal in the db</returns>
        System.Collections.Generic.ICollection<int> GetIdsByProposalId(int inProposalID);

        /// <summary>
        /// Get a list of permissions by proposal ids.  Only includes permissions needed for Home Proposal grid (Pricer, Capture Manager, Peer)
        /// </summary>
        /// <param name="inProposalIDs">Collection of propsoal ids</param>
        /// <returns>list of all permissions by that proposal in the db</returns>
        System.Collections.Generic.ICollection<int> GetIdsForHomeProposalGrid(ICollection<int> inProposalIDs);

        /// <summary>
        /// Returns all proposal permission ids for the given user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>list of all proposal permission ids for that user</returns>
        System.Collections.Generic.ICollection<int> GetIdsByUserId(int userId);

        /// <summary>
        /// Delete permission IDs by proposal ID
        /// </summary>
        /// <param name="inProposalID">Proposal ID</param>
        void DeleteIdsByProposalId(int inProposalID);

        /// <summary>
        /// Delete permission DTO objects from database
        /// </summary>
        /// <param name="inProposalPermissions">Proposal Permission DTOs to delete</param>
        void Delete(ICollection<ProposalPermissionDto> inProposalPermissions);
    }
}
