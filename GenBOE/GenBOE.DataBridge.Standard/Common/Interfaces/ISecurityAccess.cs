// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common.Interfaces
{

    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Standard;

    public interface ISecurityAccess
    {
        /// <summary>
        /// Return true/false depending on users authorizations for the roles requested
        /// </summary>
        /// <param name="inPermissions">Permissions to check</param>
        /// <param name="workspace">Optional Workspace</param>
        /// <returns>The access level the user has to this page (CRUD)</returns>
        SecurityAuthorization IsAuthorized(SecurityPermissionsRequested inPermissions, WorkspaceDTO workspace, IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser);
    }
}
