// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ModelView for the Version Comparison partial page
    /// </summary>
    public class ReportsModelView : IESModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ReportsModelView()
        {
            this.AvailableVersions = new Collection<RevisionOptionModelView>();
            this.SelectedVersion = -1;
        }

        /// <summary>
        /// Gets/Sets Available Versions
        /// </summary>
        public ICollection<RevisionOptionModelView> AvailableVersions { get; set; }

        /// <summary>
        /// Gets/Sets the Selected Version number
        /// </summary>
        public int SelectedVersion { get; set; }
    }
}
