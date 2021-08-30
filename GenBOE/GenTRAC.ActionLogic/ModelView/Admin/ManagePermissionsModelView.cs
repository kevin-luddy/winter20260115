// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web.Mvc;

    /// <summary>
    /// Model view for manage permissions page
    /// </summary>
    public class ManagePermissionsModelView : PersistedDataModelView
    {
        /// <summary>
        /// Initializes a new instance of the ManagePermissionsModelView class
        /// </summary>
        public ManagePermissionsModelView()
        {
            this.ViewerLinesOfBusinessList = new Collection<SelectListItem>();
            this.ProposalSetupAdminLinesOfBusinessList = new Collection<SelectListItem>();
        }

        /// <summary>
        /// List of allowed lines of business for viewer role
        /// </summary>
        public ICollection<SelectListItem> ViewerLinesOfBusinessList { get; set; }

        /// <summary>
        /// List of allowed lines of business for Proposal Setup Admin role
        /// </summary>
        public ICollection<SelectListItem> ProposalSetupAdminLinesOfBusinessList { get; set; }
    }
}
