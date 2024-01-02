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
    using GenTRAC.Models;
    using IES.Standard;
    using IES.Standard.Exceptions;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Permission Dto Data Loader
	/// </summary>
	public class ProposalPermissionLoader : DataLoader<ProposalPermissionDto>, IProposalPermissionLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalPermissionLoader(ILogger logger) : base(logger)
		{
		}

        /// <summary>
        /// Get a Permission by Permission Id
        /// </summary>
        /// <param name="ids">PermissionsId</param>
        /// <returns>Permission DTO.</returns>
        public override ICollection<ProposalPermissionDto> GetByIds(ICollection<int> ids)
        {
            ICollection<ProposalPermissionDto> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("ProposalPermissionLoader.GetById", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalUserRoles.Where(x => ids.Contains(x.ProposalUserRoleID)).Select(entity => new ProposalPermissionDto()
                    {
                        Id = entity.ProposalUserRoleID,
                        UserId = entity.UserID,
                        Role = (PtmRole) entity.RoleID,
                        ProposalID = entity.ProposalID,
                        UpdateDate = entity.UpdateDate,
                        ResourceType = entity.RoleTypeID.HasValue && 
                                       ((PtmRole)entity.RoleID == PtmRole.AdditionalPricingResource1 || 
                                        (PtmRole)entity.RoleID == PtmRole.AdditionalPricingResource2)
                            ? (ResourceType) entity.RoleTypeID.Value
                            : ResourceType.NotSet
                    }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get all Proposal Permissions
        /// </summary>
        /// <returns>All permissions from the DB for all proposals.</returns>
        public ICollection<ProposalPermissionDto> GetAllProposalPermissions()
        {
            ICollection<ProposalPermissionDto> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("ProposalPermissionLoader.GetAllProposalPermissions", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // select the permission IDs from the database
                    toReturn = dbModel.ProposalUserRoles.Select(entity => new ProposalPermissionDto()
                    {
                        Id = entity.ProposalUserRoleID,
                        UserId = entity.UserID,
                        Role = (PtmRole) entity.RoleID,
                        ProposalID = entity.ProposalID,
                        UpdateDate = entity.UpdateDate,
                        ResourceType = entity.RoleTypeID.HasValue && 
                                       ((PtmRole)entity.RoleID == PtmRole.AdditionalPricingResource1 || 
                                        (PtmRole)entity.RoleID == PtmRole.AdditionalPricingResource2)
                            ? (ResourceType) entity.RoleTypeID.Value
                            : ResourceType.NotSet
                    }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get all Proposal Permissions by Proposal Id
        /// </summary>
        /// <param name="inProposalID">proposal id</param>
        /// <returns>All permission ids from the DB for a given proposal.</returns>
        public ICollection<int> GetIdsByProposalId(int inProposalID)
        {
            ICollection<int> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("ProposalPermissionLoader.GetIdsByProposalId", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = (from cur in dbModel.ProposalUserRoles
                                    where cur.ProposalID == inProposalID
                                    select cur.ProposalUserRoleID).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get a list of permissions by proposal ids.  Only includes permissions needed for Home Proposal grid (Pricer, Capture Manager, Peer)
        /// </summary>
        /// <param name="inProposalIDs">Collection of propsoal ids</param>
        /// <returns>list of all permissions by that proposal in the db</returns>
        public ICollection<int> GetIdsForHomeProposalGrid(ICollection<int> inProposalIDs)
        {
            ICollection<int> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("ProposalPermissionLoader.GetIdsByProposalIds", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = (from cur in dbModel.ProposalUserRoles
                                                  where (inProposalIDs.Contains(cur.ProposalID) &&
                                                         (cur.RoleID == (int)PtmRole.Pricer ||
                                                          cur.RoleID == (int)PtmRole.CaptureManager
                                                          || cur.RoleID == (int)PtmRole.PeerReviewer
                                                          || cur.RoleID == (int)PtmRole.CostVolumeLead))
                                                  select cur.ProposalUserRoleID).ToList();
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
            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("ProposalPermissionLoader.DeleteIdsByProposalId", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    dbModel.deleteProposalRolesByProposalID(inProposalID);
                }
            }
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

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("ProposalPermissionLoader.DeleteIdsByProposalId", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    foreach (ProposalPermissionDto permission in inProposalPermissions)
                    {
                        int? roleType = null;
                        if (permission.Role == PtmRole.AdditionalPricingResource1 || permission.Role == PtmRole.AdditionalPricingResource2)
                        {
                            roleType = (int)permission.ResourceType;
                        }

                        dbModel.deleteProposalRolesByUserID(permission.UserId, permission.UpdateDate, permission.ProposalID, (int)permission.Role, roleType);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all proposal permission ids for the given user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>list of all permission ids for that user</returns>
        public ICollection<int> GetIdsByUserId(int userId)
        {
            ICollection<int> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("ProposalPermissionLoader.GetIdsByUserId", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = (from cur in dbModel.ProposalUserRoles
                                    where cur.UserID == userId
                                    select cur.ProposalUserRoleID).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts a PermissionsDto
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert</param>
        /// <returns>Id of the saved capture</returns>
        protected override int? Upsert(ProposalPermissionDto dtoToUpsert)
        {
            int? toReturn = null;

            if (dtoToUpsert != null)
            {
                if (dtoToUpsert.Id > 0)
                {
                    throw new NotImplementedException("You cannot update a permission, only insert or delete");
                }

                // certain permissions may have an additonal role type associated with it
                int? roleTypeID = null;

                // insert new permissions
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    switch (dtoToUpsert.Role)
                    {
                        case PtmRole.AdditionalPricingResource1:
                        case PtmRole.AdditionalPricingResource2:
                            // when saving an additional pricer resource, we need to save the resource type associated with it
                            roleTypeID = Convert.ToInt32(dtoToUpsert.ResourceType);
                            break;
                        case PtmRole.Pricer:
                        case PtmRole.PeerReviewer:
                        case PtmRole.CaptureManager:
                        case PtmRole.CostVolumeLead:
                        case PtmRole.AdditionalUser:
                        case PtmRole.SupplyChainPOCMatl:
                        case PtmRole.SupplyChainPOCSubs:
                        case PtmRole.ContractsPOC:
                        case PtmRole.BackupContractsPOC:
                        case PtmRole.CoverSheetApprover:
                        case PtmRole.LOBEstLead:
                        case PtmRole.PricingVerification:
                        case PtmRole.ProposalMgr:
                        case PtmRole.SystemPricer:
                        case PtmRole.TechLead:
                        case PtmRole.BackupPricer:
                        case PtmRole.GenBoeWorkspaceCreator:
                            roleTypeID = null;
                            break;
                        default:
                            // this is for all permissions that do not get saved to the ProposalUserRole table (i.e. Admin)
                            throw new GeneralAppException("A proposal permission requires a proposal role to be saved");
                    }

                    toReturn = dbModel.insertProposalUserRole(dtoToUpsert.ProposalID, Convert.ToInt32(dtoToUpsert.Role), roleTypeID, dtoToUpsert.UserId).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes a PermissionsDto
        /// </summary>
        /// <param name="dtoToDelete">Permissions to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        protected override int? Delete(ProposalPermissionDto dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                // certain permissions may have an additonal role type associated with it
                int? roleTypeID = null;
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    switch (dtoToDelete.Role)
                    {
                        case PtmRole.AdditionalPricingResource1:
                        case PtmRole.AdditionalPricingResource2:
                            // when deleting an additional pricer resource, we need to pass in the resource type associated with it
                            roleTypeID = Convert.ToInt32(dtoToDelete.ResourceType);
                            break;
                        case PtmRole.Pricer:
                        case PtmRole.PeerReviewer:
                        case PtmRole.CaptureManager:
                        case PtmRole.CostVolumeLead:
                        case PtmRole.AdditionalUser:
                        case PtmRole.SupplyChainPOCMatl:
                        case PtmRole.SupplyChainPOCSubs:
                        case PtmRole.ContractsPOC:
                        case PtmRole.CoverSheetApprover:
                        case PtmRole.LOBEstLead:
                        case PtmRole.PricingVerification:
                        case PtmRole.ProposalMgr:
                        case PtmRole.SystemPricer:
                        case PtmRole.TechLead:
                        case PtmRole.BackupPricer:
                            // these roles do not have a roletype associated with it, the db expects a null
                            roleTypeID = null;
                            break;
                        default:
                            // this is for all permissions that do not get saved to the ProposalUserRole table (i.e. Admin)
                            throw new GeneralAppException("A proposal permission requires a proposal role to be saved");
                    }

                    dbModel.deleteProposalRolesByUserID(dtoToDelete.UserId, dtoToDelete.UpdateDate, dtoToDelete.ProposalID, Convert.ToInt32(dtoToDelete.Role), roleTypeID);
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }
    }
}