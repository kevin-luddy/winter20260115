// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Objects
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// Retrieves child full objects / dtos for the Full Objects.
    /// </summary>
    internal class Retriever : GenTRAC.Objects.IRetriever
    {
        #region Fields and Constructor

        /// <summary>
        /// User Mapper
        /// </summary>
        private IUserMapper userMapper;

        /// <summary>
        /// Proposal Permissions Mapper
        /// </summary>
        private IProposalPermissionMapper proposalPermissionsMapper;

        /// <summary>
        /// Proposal Checklist Loader
        /// </summary>
        private IProposalChecklistLoader proposalChecklistLoader;

        /// <summary>
        /// proposal checklist PPR content mapper
        /// </summary>
        private IChecklistContentMapper proposalChecklistPPRContentMapper;

        /// <summary>
        /// proposal checklist PAR content mapper
        /// </summary>
        private IChecklistContentMapper proposalChecklistPARContentMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userMapper">user mapper</param>
        /// <param name="proposalPermissionsMapper">permissions mapper</param>
        /// <param name="proposalChecklistLoader">proposal checklist loader</param>
        /// <param name="checklistPPRContentMapper"> PPR checklist content mapper</param>
        /// <param name="checklistPARContentMapper">PAR checklist content mapper</param>
        public Retriever(
           IUserMapper userMapper,
           IProposalPermissionMapper proposalPermissionsMapper,
           IProposalChecklistLoader proposalChecklistLoader,
           IChecklistContentMapper checklistPPRContentMapper,
           IChecklistContentMapper checklistPARContentMapper)
        {
            this.userMapper = userMapper;
            this.proposalPermissionsMapper = proposalPermissionsMapper;
            this.proposalChecklistLoader = proposalChecklistLoader;
            this.proposalChecklistPPRContentMapper = checklistPPRContentMapper;
            this.proposalChecklistPARContentMapper = checklistPARContentMapper;
        }

        #endregion Fields and Constructor

        #region Proposal

        /// <summary>
        /// Returns the current user
        /// </summary>
        /// <returns>Current User</returns>
        public UserDTO GetCurrentUser()
        {
            return this.userMapper.GetActiveUser();
        }

        /// <summary>
        /// Returns the permissions for the proposal
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>permissions</returns>
        public ICollection<ProposalPermissionDto> GetProposalPermissions(int proposalId)
        {
            return this.proposalPermissionsMapper.GetProposalPermissionsByProposalId(proposalId);
        }

        #endregion Proposal

        #region Checklist

        /// <summary>
        /// Returns the proposal checklists for the proposal
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>proposal checklists</returns>
        public ICollection<ProposalChecklistDto> GetProposalChecklists(int proposalId)
        {
            return this.proposalChecklistLoader.GetByProposalIds(new Collection<int> { proposalId });
        }

        /// <summary>
        /// get par checklist content
        /// </summary>
        /// <param name="proposalId">proposal ID</param>
        /// <returns>PAR checklist content data</returns>
        public ChecklistContentDto GetPARChecklistContent(int proposalId)
        {
            return this.proposalChecklistPARContentMapper.GetChecklistByProposalId(proposalId);
        }

        /// <summary>
        /// get PPR checklist content
        /// </summary>
        /// <param name="proposalId">proposal ID</param>
        /// <returns>PPR checklist content data</returns>
        public ChecklistContentDto GetPPRChecklistContent(int proposalId)
        {
            return this.proposalChecklistPPRContentMapper.GetChecklistByProposalId(proposalId);
        }

        /// <summary>
        /// Get all checklist save info for the given proposal Id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Collection of Save Info objects</returns>
        public ICollection<ProposalChecklistSaveInfo> GetAllChecklistSaveInfo(int proposalId)
        {
            return this.proposalChecklistLoader.GetAllChecklistSaveInfo(proposalId);
        }

        #endregion Checklist
    }
}
