// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    /// Model view for permissions grid
    /// </summary>
    public class PermissionsModelView : PersistedDataModelView
    {
        /// <summary>
        /// Initializes a new instance of the PermissionsGridModelView class
        /// </summary>
        public PermissionsModelView()
        {
            this.UserName = null;
            this.UserId = -1;
            this.Roles = new Collection<PtmRole>();
            this.ViewerLinesOfBusiness = new Collection<int>();
            this.ProposalSetupAdminLinesOfBusiness = new Collection<int>();
            this.UserType = IES.Common.UserType.User;
            this.UsersInAGroup = new Collection<string>();
            this.UserNtId = null;
        }

        /// <summary>
        /// Gets or sets the user string
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user string
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the boolean if the UserId is a group or user
        /// </summary>
        public IES.Common.UserType UserType { get; set; }

        /// <summary>
        /// Gets or sets the user nt Id
        /// </summary>
        public string UserNtId { get; set; }

        /// <summary>
        /// Gets or sets the roles belonging to the users contained in the element
        /// </summary>
        public Collection<PtmRole> Roles { get; set; }

        /// <summary>
        /// If it's a group, this will give me the users in the group
        /// </summary>
        public Collection<string> UsersInAGroup { get; set; }

        /// <summary>
        /// Lines Of Business for the Proposal Setup Admin role
        /// </summary>
        public ICollection<int> ProposalSetupAdminLinesOfBusiness { get; set; }

        /// <summary>
        /// Lines of business for the viewer role
        /// </summary>
        public ICollection<int> ViewerLinesOfBusiness { get; set; }

        /// <summary>
        /// Comma-delimited list of lines of business
        /// </summary>
        public string ViewerLinesOfBusinessString
        {
            get
            {
                return string.Join(",", this.ViewerLinesOfBusiness);
            }
        }

        /// <summary>
        /// Comma-delimited list of lines of business
        /// </summary>
        public string ProposalSetupAdminLinesOfBusinessString
        {
            get
            {
                return string.Join(",", this.ProposalSetupAdminLinesOfBusiness);
            }
        }

        /// <summary>
        /// Lines of business for viewer role, formatted for display
        /// </summary>
        public string ViewerLinesOfBusinessDisplay { get; set; }

        /// <summary>
        /// Lines of Business for proposal setup Admin role, formatted for display
        /// </summary>
        public string ProposalSetupAdminLinesOfBusinessDisplay { get; set; }
    }
}
