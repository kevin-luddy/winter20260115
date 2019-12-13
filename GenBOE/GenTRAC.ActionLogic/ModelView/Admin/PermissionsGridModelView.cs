// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Grid View
    /// </summary>
    public class PermissionsGridModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public PermissionsGridModelView()
        {
            this.PermissionData = new Collection<PermissionsModelView>();
        }

        /// <summary>
        /// Holds on to the underlying data
        /// </summary>
        public ICollection<PermissionsModelView> PermissionData { get; set; }
    }
}
