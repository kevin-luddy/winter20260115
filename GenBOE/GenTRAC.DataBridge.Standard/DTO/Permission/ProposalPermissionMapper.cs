// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Standard;

    /// <summary>
    /// Proposal Permission Mapper
    /// </summary>
    public class ProposalPermissionMapper : IInternalProposalPermissionMapper
    {
        /// <summary>
        /// Logger
        /// </summary>
        private Logger log = new Logger(typeof(ProposalPermissionMapper));

        /// <summary>
        /// Proposal Permission Loader
        /// </summary>
        protected IProposalPermissionLoader ProposalPermissionLoader { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="proposalPermissionLoader">Proposal Permission data loader</param>
        public ProposalPermissionMapper(IProposalPermissionLoader proposalPermissionLoader)
        {
            this.ProposalPermissionLoader = proposalPermissionLoader;
        }

        /// <summary>
        /// Get items by Id
        /// </summary>
        /// <param name="itemId">Item Id.</param>
        /// <returns>Dto for the corresponding Id.</returns>
        public ProposalPermissionDto GetById(int itemId)
        {
            if (itemId < 0)
            {
                // a null is returned here to prevent going to cache/db for a negative id which should never exist
                return null;
            }

            ProposalPermissionDto result;

            using (StopwatchTimer sw = new StopwatchTimer("ProposalPermissionMapper.GetById", this.log))
            {
                result = this.ProposalPermissionLoader.GetById(itemId);
            }

            return result;
        }

        /// <summary>
        /// Returns a list of Permissions DTOs for the given Proposal ID, null if none found.
        /// </summary>
        /// <param name="proposalId">proposal Id</param>
        /// <returns>Returns Permissions Dto</returns>
        public ICollection<ProposalPermissionDto> GetProposalPermissionsByProposalId(int proposalId)
        {
            ICollection<ProposalPermissionDto> toReturn = new List<ProposalPermissionDto>();

            using (StopwatchTimer sw = new StopwatchTimer("ProposalPermissionMapper.GetProposalPermissionsByProposalId", this.log))
            {
                ICollection<int> permissionIds = this.ProposalPermissionLoader.GetIdsByProposalId(proposalId);
                toReturn = this.GetDtos(permissionIds);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a list of Permissions DTOs for the given a User Id and proposal Id, null if none found.
        /// </summary>
        /// <param name="proposalId">proposal Id</param>
        /// <param name="userId">User Id</param>
        /// <returns>Returns Permissions Dto Collection</returns>
        public ICollection<ProposalPermissionDto> GetProposalPermissionsByUserId(int proposalId, int userId)
        {
            ICollection<ProposalPermissionDto> toReturn = this.GetProposalPermissionsByProposalId(proposalId);

            using (StopwatchTimer sw = new StopwatchTimer("ProposalPermissionMapper.GetProposalPermissionsByUserId", this.log))
            {
                toReturn = (from x in toReturn
                            where x.UserId == userId
                            select x).ToArray();
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a list of permissions for a single user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>List of permissions</returns>
        public ICollection<ProposalPermissionDto> GetByUserId(int userId)
        {
            List<ProposalPermissionDto> toReturn = new List<ProposalPermissionDto>();

            using (StopwatchTimer sw = new StopwatchTimer("ProposalPermissionMapper.GetSystemPermissions", this.log))
            {
                ICollection<int> proposalPermissionIds = this.ProposalPermissionLoader.GetIdsByUserId(userId);
                toReturn.AddRange(this.GetDtos(proposalPermissionIds));
            }

            return toReturn;
        }

        /// <summary>
        /// Saves a permissionsDto, returning the post save permissionsDto.
        /// </summary>
        /// <param name="permission">Permissions to save</param>
        /// <returns>Id of the saved Permissions</returns>
        int? IInternalProposalPermissionMapper.Save(ProposalPermissionDto permission)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ProposalPermissionMapper.SavePermissionsDto", this.log))
            {
                if (permission == null)
                {
                    throw new ArgumentNullException(nameof(permission));
                }

                toReturn = this.ProposalPermissionLoader.Save(permission);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a collection of DTOs from a collection of IDs
        /// </summary>
        /// <param name="ids">IDs to get</param>
        /// <returns>Collection of DTOs</returns>
        protected ICollection<ProposalPermissionDto> GetDtos(ICollection<int> ids)
        {
            List<ProposalPermissionDto> toReturn = new List<ProposalPermissionDto>();

            if (ids != null && ids.Any())
            {
                using (StopwatchTimer sw = new StopwatchTimer("PermissionMapper.GetDtos", this.log))
                {
                    toReturn = this.ProposalPermissionLoader.GetByIds(ids).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Delete permission IDs by proposal ID
        /// </summary>
        /// <param name="inProposalID">Proposal ID</param>
        public void DeleteIdsByProposalId(int inProposalID)
        {
            this.ProposalPermissionLoader.DeleteIdsByProposalId(inProposalID);
        }

        /// <summary>
        /// Delete permission DTO objects from database
        /// </summary>
        /// <param name="inProposalPermissions">Proposal Permission DTOs to delete</param>
        public void Delete(ICollection<ProposalPermissionDto> inProposalPermissions)
        {
            if (inProposalPermissions == null)
            {
                throw new ArgumentNullException(nameof(inProposalPermissions));
            }

            this.ProposalPermissionLoader.Delete(inProposalPermissions);
        }
    }
}
