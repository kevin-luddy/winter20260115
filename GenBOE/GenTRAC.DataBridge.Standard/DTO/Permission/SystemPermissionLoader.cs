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

    /// <summary>
    /// System Permissions Loader
    /// </summary>
    public class SystemPermissionLoader : DataLoader<SystemPermissionDto>, ISystemPermissionLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SystemPermissionLoader(ILogger logger)
        {
            this.Log = logger;
        }

        /// <summary>
        /// Get a Permission by Permission Id
        /// </summary>
        /// <param name="ids">PermissionsId</param>
        /// <returns>Permission DTO.</returns>
        public override ICollection<SystemPermissionDto> GetByIds(ICollection<int> ids)
        {
            ICollection<SystemPermissionDto> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("SystemPermissionLoader.GetById", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.SystemUserRoles.Where(x => ids.Contains(x.SystemUserRoleID))
                        .Select(entity => new
                        {
                            Id = entity.SystemUserRoleID,
                            UserId = entity.UserID,
                            Role = (PtmRole)entity.RoleID,
                            UpdateDate = entity.UpdateDT,
                            LineOfBusinessIDs = entity.LineOfBusinessLUs.Select(x => x.LineOfBusinessID)
                        }).ToList().Select(entity => new SystemPermissionDto()
                        {
                            Id = entity.Id,
                            UserId = entity.UserId,
                            Role = entity.Role,
                            UpdateDate = entity.UpdateDate,
                            LineOfBusinessIDs = entity.LineOfBusinessIDs.ToList()
                        }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns all system permission DTOs
        /// </summary>
        /// <returns>System Permission DTOs</returns>
        public ICollection<SystemPermissionDto> GetAllSystemPermissions()
        {
            ICollection<SystemPermissionDto> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("SystemPermissionLoader.GetAllSystemPermissions", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // select the permission IDs from the database
                    var resultLinq = dbModel.SystemUserRoles.Select(entity => new
                                                {
                                                    Id = entity.SystemUserRoleID,
                                                    UserId = entity.UserID,
                                                    Role = (PtmRole)entity.RoleID,
                                                    UpdateDate = entity.UpdateDT,
                                                    LineOfBusinessIDs = entity.LineOfBusinessLUs.Select(x => x.LineOfBusinessID)
                                                }).ToList().Select(entity => new SystemPermissionDto()
                                                {
                                                    Id = entity.Id,
                                                    UserId = entity.UserId,
                                                    Role = entity.Role,
                                                    UpdateDate = entity.UpdateDate,
                                                    LineOfBusinessIDs = entity.LineOfBusinessIDs.ToList()
                                                }).ToList();

                    toReturn = resultLinq.ToArray();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets all system permission IDs
        /// </summary>
        /// <returns>Collection of IDs</returns>
        public ICollection<int> GetAllIds()
        {
            ICollection<int> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("SystemPermissionLoader.GetAllIds", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    IEnumerable<int> resultLinq = from cur in dbModel.SystemUserRoles
                                                  select cur.SystemUserRoleID;

                    toReturn = resultLinq.ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns all system permission ids for the given user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>list of all permission ids for that user</returns>
        public ICollection<int> GetIdsByUserId(int userId)
        {
            ICollection<int> toReturn = null;

            using (IES.Standard.StopwatchTimer sw = new IES.Standard.StopwatchTimer("SystemPermissionLoader.GetIdsByUserId", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    IEnumerable<int> resultLinq = from cur in dbModel.SystemUserRoles
                                                  where cur.UserID == userId
                                                  select cur.SystemUserRoleID;

                    toReturn = resultLinq.ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts a PermissionsDto
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert</param>
        /// <returns>Id of the saved capture</returns>
        protected override int? Upsert(SystemPermissionDto dtoToUpsert)
        {
            int? toReturn = null;

            if (dtoToUpsert != null)
            {
                if (dtoToUpsert.Id > 0)
                {
                    throw new NotImplementedException("You cannot update a permission, only insert or delete");
                }

                // insert new permissions
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    switch (dtoToUpsert.Role)
                    {
                        case PtmRole.Admin:
                        case PtmRole.Viewer:
                        case PtmRole.SystemPricer:
                        case PtmRole.ProposalSetupAdmin:
                            var currentlyExists = dbModel.SystemUserRoles.SingleOrDefault(x => x.UserID == dtoToUpsert.UserId && x.RoleID == (int)dtoToUpsert.Role);

                            if (currentlyExists == null)
                            {
                                toReturn = dbModel.insertSystemUserRole(dtoToUpsert.UserId, (int)dtoToUpsert.Role, string.Join(",", dtoToUpsert.LineOfBusinessIDs)).FirstOrDefault();
                            }
                            else
                            {
                                toReturn = currentlyExists.SystemUserRoleID;
                            }

                            break;
                        case PtmRole.NotSet:
                        default:
                            // this is for all permissions that do not get saved to the SystemUserRole table (i.e. Pricer)
                            throw new GeneralAppException("A system permission requires a system role to be saved");
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes a PermissionsDto
        /// </summary>
        /// <param name="dtoToDelete">Permissions to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        protected override int? Delete(SystemPermissionDto dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    switch (dtoToDelete.Role)
                    {
                        case PtmRole.Admin:
                        case PtmRole.Viewer:
                        case PtmRole.SystemPricer:
                        case PtmRole.ProposalSetupAdmin:
                            dbModel.deleteSystemRoleByID(dtoToDelete.UpdateDate, dtoToDelete.Id);
                            break;
                        case PtmRole.NotSet:
                        default:
                            throw new GeneralAppException("A system permission requires a system role to be saved");
                    }
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }
    }
}
