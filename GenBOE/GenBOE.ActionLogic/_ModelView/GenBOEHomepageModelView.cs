// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using DataBridge.DTO;

    /// <summary>
    /// Model View for Home Page Metrics
    /// </summary>
    public class GenBOEHomepageModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenBOEHomepageModelView"/> class.
        /// </summary>
        public GenBOEHomepageModelView()
        {
            this.isSysAdmin = false;
            this.canCreateWS = false;
            this.lastUpdatedTime = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToLongTimeString();
            this.workspaceGridRows = new Collection<GenBOEHomepageWorkspaceRowModelView>();
        }

        /// <summary>
        /// Gets or sets the last updated time.
        /// </summary>
        public string lastUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is system admin.
        /// </summary>
        public bool isSysAdmin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this user can create ws.
        /// </summary>
        public bool canCreateWS { get; set; }

        /// <summary>
        /// Gets or sets the workspace grid rows.
        /// </summary>
        public ICollection<GenBOEHomepageWorkspaceRowModelView> workspaceGridRows { get; set; }
    }
}