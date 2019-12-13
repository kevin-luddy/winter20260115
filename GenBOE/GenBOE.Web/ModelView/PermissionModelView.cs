// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class PermissionModelView
    {
        /// <summary>
        /// Permissions
        /// </summary>
        public ICollection<PermissionsGridModelView> Permissions { get; set; }

        /// <summary>
        /// Current User ID
        /// </summary>
        public int CurrentUserId { get; set; }

        /// <summary>
        /// Current User Display Name
        /// </summary>
        public string CurrentUserDisplayName { get; set; }
    }
}