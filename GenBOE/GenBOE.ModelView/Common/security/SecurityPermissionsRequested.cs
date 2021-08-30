// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// Class to capture the request for permissions
    /// for a specific set of roles.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SecurityPermissionsRequested
    {
        /// <summary>
        /// The page to check, required
        /// </summary>
        public SecurityPage PageToCheck { get; set; }

        
        /// <summary>
        /// The optional workspaceId to look at specific permissions for (required if using Workspace Roles)
        /// </summary>
        public int? WorkspaceId { get; set; }

        /// <summary>
        /// The optional BOEId to look at specific permissions for (required if using BOE Roles)
        /// </summary>
        public int? BOEId { get; set; }

        /// <summary>
        /// Default constructor.  Set all enums to 'None' values by default.
        /// </summary>
        public SecurityPermissionsRequested()
        {
            PageToCheck = SecurityPage.None;
        }

        /// <summary>
        /// Pretty print details about object
        /// </summary>
        /// <returns>string representing object</returns>
        public override string ToString()
        {
            return String.Format("Page [{0}] WorkspaceID [{1}] BOEID [{2}]",
                                        PageToCheck,
                                        WorkspaceId.HasValue ? WorkspaceId.Value.ToString() : "<null>",
                                        BOEId.HasValue ? BOEId.Value.ToString() : "<null>");
        }
    }
}
