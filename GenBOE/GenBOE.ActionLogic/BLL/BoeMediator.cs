// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class BoeMediator : IBoeMediator
    {
        private IUserDTODataLoader _userLoader;
        private IBoeDTODataLoader boeLoader;
        private IPermissionsDTODataLoader permissionLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public BoeMediator(
            IUserDTODataLoader inuserLoader,
            IBoeDTODataLoader boeLoader,
            IPermissionsDTODataLoader permissionLoader)
        {
            this._userLoader = inuserLoader;
            this.boeLoader = boeLoader;
            this.permissionLoader = permissionLoader;
        }

        /// <summary>
        /// Saves BOEs.
        /// 
        /// NOTE: this method requires callers to pass in the original boes. it helps with performance.
        /// </summary>
        /// <param name="workspace">Workspace that owns the inBOECollection</param>
        /// <param name="inModifiedBOECollection">BOEs to modify</param>
        /// <returns>Dictionary where key = old boe id and value = new boe id</returns>
        public IDictionary<int, int> MediatedSaveBOEs(FullWorkspace workspace, ICollection<BoeDTO> inModifiedBOECollection)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (inModifiedBOECollection == null)
            {
                throw new ArgumentNullException(nameof(inModifiedBOECollection));
            }

            // Make sure that RTE data is loaded, that way we do not wipe it out..
            this.boeLoader.LoadRTEFields(inModifiedBOECollection.Where(x => x.Updateable != UpdateType.Deleted).ToList());

            ICollection<BoeDTO> boesToSave = inModifiedBOECollection;

            int currentUserID = workspace.CurrentActiveUser.UserID;
            foreach (var boe in boesToSave)
            {
                boe.UpdatedByUserId = currentUserID;
            }

            ////********************************************************
            //// TODO - should create a BulkSave for this - ref WI 27989
            ////********************************************************
            IDictionary<int, int> toReturn = this.boeLoader.Save(boesToSave.ToCollection());

            // now tell the workspace to refresh the boes
            workspace.RefreshBoes();

            // now check to make sure that all users that have BOE permissions also have workspace level permissions
            if (boesToSave.Any())
            {
                int wsId = boesToSave.First().WorkspaceID;

                List<int> boeIds = boesToSave.Select(x => x.Id).Distinct().ToList();

                // To deal w/ "new" boes that had Ids < 0..
                if (boeIds.Any(x => x < 0))
                {
                    List<int> fixedBoeids = new List<int>();

                    foreach (int boeId in boeIds)
                    {
                        if (boeId >= 0)
                        {
                            fixedBoeids.Add(boeId);
                        }
                    }

                    boeIds = fixedBoeids;
                }

                Collection<PermissionsDTO> boePermissions = this.permissionLoader.GetBOEPermissions(boeIds);
                Collection<PermissionsDTO> wsPermissions = this.permissionLoader.GetBOEPotentialPermissionsForWorkspace(wsId);

                foreach (PermissionsDTO permission in boePermissions)
                {
                    Collection<PermissionsDTO> wsPermissionsForUser = wsPermissions.Where(x => x.ETIUserId == permission.ETIUserId).ToCollection();

                    // we need to insert a permission if one of the following is true..
                    bool needToInsert = (!wsPermissionsForUser.Any()) || // a) no WS permission for the user
                        (permission.Role == Role.Approver && !wsPermissionsForUser.Any(x => x.Role == Role.Approver)) ||
                        (permission.Role == Role.Author && !wsPermissionsForUser.Any(x => x.Role == Role.Author));

                    if (needToInsert)
                    {
                        // need to insert a potential WS permission based on the permission dto
                        permission.Id = -1;
                        permission.Updateable = UpdateType.Upsert;
                        permission.BOEId = null;
                        permission.WorkspaceId = wsId;
                        permission.PermissionId = -1;

                        this.permissionLoader.SavePermission(permission);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Saves BOEs.
        /// </summary>
        /// <param name="workspace">FullWorkspace object containing the Boe.</param>
        /// <param name="inModifiedBoeDTO">Boe to be updated.</param>
        /// <returns>Dictionary of old Boe Id to new Boe Id.</returns>
        public IDictionary<int, int> MediatedSave(FullWorkspace workspace, BoeDTO inModifiedBoeDTO)
        {
            return this.MediatedSaveBOEs(workspace, new Collection<BoeDTO>() { inModifiedBoeDTO });
        }

        /// <summary>
        /// When saving the BOE Header (description, sources of data, BOE level custom fields), do not go through the MediatedSaves
        /// There is no reason these updates will effect rates, recalculations, etc. Also, we do not want to go through the normal BOE
        /// Clear Cache method since we will be taking an unnecessary hit of clearing out keys that aren't applicable for this type of a save
        /// Every other BOE Save should follow the process of using the MediatedSaves
        /// </summary>
        /// <param name="inboe"></param>
        public void SaveEditBoeHeader(BoeDTO inboe)
        {
            if (inboe == null)
            {
                throw new ArgumentNullException(nameof(inboe));
            }

            inboe.UpdatedByUserId = this._userLoader.GetUserForActiveUser().UserID;

            this.boeLoader.Save(inboe);
        }
    }
}
