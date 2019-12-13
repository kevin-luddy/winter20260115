// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Linq;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class WorkOfflineImporterSpaceSystems : WorkOfflineImporter
    {
        public WorkOfflineImporterSpaceSystems(
            IPermissionsDTODataLoader inIPermissionsDTOLoader,
            ICommonDataMapper inCommonDataMapper,
            IResourceDTODataLoader inResourceDataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IVariableCircularReferenceChecker inVariableCircularReferenceChecker
            )
            : base(inIPermissionsDTOLoader, inCommonDataMapper, inResourceDataLoader, perfOrgLoader, inVariableCircularReferenceChecker)
        {
        }

        /// <summary>
        /// Determines if the current user is a workspace author or has author permissions
        /// </summary>
        /// <returns></returns>
        protected override Boolean IsUserAuthor(BoeDTO boeDTO, FullWorkspace workspace)
        {
            if (boeDTO == null)
            {
                throw new ArgumentNullException(nameof(boeDTO), "boeDTO can't be null");
            }

            // Admins can import as authors in Space Systems
            Boolean toReturn = base.IsUserAuthor(boeDTO, workspace);

            if (!toReturn)
            {
                UserDTO activeUser = workspace.CurrentActiveUser;

                // retrieve the list of authors on the BOE
                var WSAdminList = from x in workspace.WorkspacePermissions
                                  where x.Role == Role.WorkspaceAdmin
                                  select x.ETIUserId;

                toReturn = WSAdminList.Contains(activeUser.UserID);   // check if the current user is a workspace admin
            }

            return toReturn;
        }

        protected override void ValidateBoeTitle(WorkofflineImportedBoe importedBoe)
        {
            if (importedBoe == null)
            {
                throw new ArgumentNullException(nameof(importedBoe), "Parameter importedBoe is required.");
            }

            base.ValidateBoeTitle(importedBoe);

            if (importedBoe.Title == null || String.IsNullOrEmpty(importedBoe.Title))
            {
                importedBoe.ImportTypes.Add(BoeImportResult.BoeTitleRequired);
            }
        }  
    }
}
