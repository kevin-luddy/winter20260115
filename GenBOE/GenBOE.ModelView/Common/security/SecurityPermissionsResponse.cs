// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System;
    using IES.Common;

    /// <summary>
    /// Immutable class to capture the response for use in code to understand
    /// a specific set of permissions based on a specific set of
    /// object IDs in the system.
    /// </summary>
    [Serializable()]
    public class SecurityPermissionsResponse
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SecurityPermissionsResponse(Role authorizedRole, int? workspaceId, int? boeId)
        {
            AuthorizedRole = authorizedRole;
            WorkspaceId = workspaceId;
            BOEId = boeId;
        }

        /// <summary>
        /// The roles the user is authorized for
        /// </summary>
        public Role AuthorizedRole { get; private set; }

        /// <summary>
        /// Optional workspaceId the user is authorized for
        /// </summary>
        public int? WorkspaceId { get; private set; }
        
        /// <summary>
        /// Optional BOEId the user is authorized for
        /// </summary>
        public int? BOEId { get; private set; }
    }
}
