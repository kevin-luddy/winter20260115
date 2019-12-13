// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// Proposal Permission Mediator
    /// </summary>
    public class ProposalPermissionMediator : IProposalPermissionMediator
    {
        /// <summary>
        /// Proposal Permissions Mapper
        /// </summary>
        private IInternalProposalPermissionMapper proposalPermissionMapper = null;

        /// <summary>
        /// Constructor, requires a reference to the internal mapper
        /// </summary>
        /// <param name="inProposalPermissionsMapper">Permissions mapper</param>
        public ProposalPermissionMediator(IProposalPermissionMapper inProposalPermissionsMapper)
        {
            this.proposalPermissionMapper = inProposalPermissionsMapper as IInternalProposalPermissionMapper;
        }

        /// <summary>
        /// Saves a collection of ProposalPermissionDto
        /// </summary>
        /// <param name="inProposalPermissionDtos">Permissions to save</param>
        /// <returns>Collection of the DTOs of the saved Permissions</returns>
        public ICollection<ProposalPermissionDto> SaveProposalPermissionDtos(ICollection<ProposalPermissionDto> inProposalPermissionDtos)
        {
            if (inProposalPermissionDtos == null)
            {
                throw new ArgumentNullException(nameof(inProposalPermissionDtos));
            }

            Collection<ProposalPermissionDto> toReturn = new Collection<ProposalPermissionDto>();

            foreach (ProposalPermissionDto permission in inProposalPermissionDtos)
            {
                ProposalPermissionDto afterSave = this.SaveProposalPermissionDto(permission);

                if (afterSave != null)
                {
                    toReturn.Add(afterSave);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Saves a permission dto
        /// </summary>
        /// <param name="inProposalPermissionDto">dto to save</param>
        /// <returns>saved dto</returns>
        public ProposalPermissionDto SaveProposalPermissionDto(ProposalPermissionDto inProposalPermissionDto)
        {
            // Pre save business logic
            if (inProposalPermissionDto == null)
            {
                throw new ArgumentNullException(nameof(inProposalPermissionDto));
            }

            // Save
            int? toReturn = this.proposalPermissionMapper.Save(inProposalPermissionDto);

            // Return
            return this.proposalPermissionMapper.GetById(toReturn.Value);
        }

        /// <summary>
        /// Delete permission IDs by proposal ID
        /// </summary>
        /// <param name="inProposalID">Proposal ID</param>
        public void DeleteIdsByProposalId(int inProposalID)
        {
            this.proposalPermissionMapper.DeleteIdsByProposalId(inProposalID);
        }

        /// <summary>
        /// Delete permission DTO objects from database
        /// </summary>
        /// <param name="inProposalPermissions">Proposal Permission DTOs to delete</param>
        public void Delete(ICollection<ProposalPermissionDto> inProposalPermissions)
        {
            this.proposalPermissionMapper.Delete(inProposalPermissions);
        }
    }
}
