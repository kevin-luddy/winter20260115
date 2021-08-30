// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.Permissions
{
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using IES.Common;

    public class PermissionsWorkspaceAdminValidation
    {
        private IPermissionsDTODataLoader _permissionsDTOLoader;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PermissionsWorkspaceAdminValidation(IPermissionsDTODataLoader inPermissionDTOLoader)
        {
            this._permissionsDTOLoader = inPermissionDTOLoader;
        }

        /// <summary>
        /// Checks to see how many Workspace Administrators
        /// are assigned to a workspace.
        /// </summary>
        /// <param name="inWorkspaceID">Workspace ID</param>
        /// <returns>Boolean value based on the # of workspace administrators</returns>
        public bool ValidateWorkspaceAdmin(int inWorkspaceID)
        {
            bool multipleAdmins = false;
            
            var linqResults = (from p in this._permissionsDTOLoader.GetWorkspacePermissions(inWorkspaceID)
                               where p.Role == Role.WorkspaceAdmin
                               select p).ToList();

            var groups = linqResults.Where(x => x.NTID.Contains(".")).ToList();

            int nullGroupCount = linqResults.Except(groups).Select(x => x.NTID).Count();

            int GroupCount = groups.Select(x => x.NTID).Count();
                        
            if (nullGroupCount > 1 || GroupCount > 1)
            {
                multipleAdmins = true;
            }

            return multipleAdmins;
        }

    }
}
