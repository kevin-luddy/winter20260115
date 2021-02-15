// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// ModelView for the Version Comparison partial page
    /// </summary>
    public class VersionComparisonModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public VersionComparisonModelView()
        {
            this.AvailableVersions = new Collection<RevisionOptionModelView>();
            this.SelectedVersionNumber = -1;
            this.ActiveLocks = new Collection<AreaLockData>();
            this.AvailableCompareToVersions = new Collection<RevisionOptionModelView>();
        }

        /// <summary>
        /// Gets/Sets the Admin User flag.
        /// </summary>
        public bool AdminUser { get; set; }
        
        /// <summary>
        /// Gets/Sets Available Versions
        /// </summary>
        public IList<RevisionOptionModelView> AvailableVersions { get; set; }

        /// <summary>
        /// Gets/Sets Available Versions to compare to
        /// </summary>
        public IList<RevisionOptionModelView> AvailableCompareToVersions { get; set; }

        /// <summary>
        /// Gets/sets selected the first Revision
        /// </summary>
        public RevisionOptionModelView FirstSelectedRevision { get; set; }

        /// <summary>
        /// Gets/sets selected the second Revision
        /// </summary>
        public RevisionOptionModelView SecondSelectedRevision { get; set; }

        /// <summary>
        /// Gets/Sets the Selected Version number
        /// </summary>
        public int SelectedVersionNumber { get; set; }

        /// <summary>
        /// Gets previous version (Selected Version - 1)
        /// </summary>
        public int PreviousVersionNumber
        {
            get
            {
                return this.SelectedVersionNumber - 1;
            }
        }

        /// <summary>
        /// Gets/Sets the display value for the selected version, i.e. "Version 173" or "WIP Version".
        /// </summary>
        public string SelectedVersionNumberDisplay { get; set; }

        /// <summary>
        /// Gets/Sets the display value for the previous version 
        /// </summary>
        public string PreviousVersionNumberDisplay { get; set;  }

        /// <summary>
        /// Gets/Sets the rows of the Comparison Grid
        /// </summary>
        public ICollection<VersionComparisonGridRowModelView> PPRDDifferences { get; set; }

        /// <summary>
        /// Gets/Sets indicator whether the selected revision is the earliest revision or not
        /// </summary>
        public bool IsEarliestVersion { get; set; }

        /// <summary>
        /// Gets/Sets the active locks in RDM
        /// </summary>
        public ICollection<AreaLockData> ActiveLocks { get; set; }

        /// <summary>
        /// Gets or sets the work in progress history.
        /// </summary>
        public string WorkInProgressHistory { get; set; }

        /// <summary>
        /// Gets or sets the release notes.
        /// </summary>
        public string ReleaseNotes { get; set; }

        /// <summary>
        /// Gets or sets the validation messages.
        /// </summary>
        public ICollection<ValidationMessage> ReplicationValidationMessages { get; set; }
    }
}
