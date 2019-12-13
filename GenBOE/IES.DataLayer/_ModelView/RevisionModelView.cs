// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;

    /// <summary>
    /// A Model View class for a PPR&amp;D Revision
    /// </summary>
    [Serializable]
    public class RevisionModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevisionModelView"/> class.
        /// </summary>
        public RevisionModelView()
        {
            this.Id = -1;
            this.Revision = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RevisionModelView"/> class from a RevisionOptionModelView.
        /// This is used to avoid a second call for a RevisionModelView when we already have retrieved the options for a select drop down.
        /// </summary>
        /// <param name="optionModelRevision">A RevisionOptionModelView</param>
        public RevisionModelView(RevisionOptionModelView optionModelRevision)
        {
            if (optionModelRevision != null)
            {
                this.Id = optionModelRevision.Id;
                this.Revision = optionModelRevision.Label;
                this.StartYear = optionModelRevision.StartYear;
                this.EndYear = optionModelRevision.EndYear;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RevisionModelView"/> class.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <param name="updateDate">Update Date</param>
        /// <param name="history">Revision history, e.g. changes since last revision</param>
        /// <param name="dateCreated">Date when revision was created.</param>
        /// <param name="createdBy">NT ID of person that created the revision</param>
        /// <param name="revision">PPR&amp;D revision number</param>
        /// <param name="datePublished">Date revision was published; null if WIP.</param>
        /// <param name="publishedBy">Name of user who published the revision</param>
        /// <param name="startYear">Start year for rate data</param>
        /// <param name="endYear">End year for rate data</param>
        /// <param name="releaseNotes">Release notes</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RevisionModelView(int id, DateTime updateDate, string history, DateTime dateCreated, string createdBy, string revision, DateTime? datePublished, string publishedBy, int startYear, int endYear, string releaseNotes)
        {
            this.Id = id;
            this.UpdateDate = updateDate;
            this.Revision = revision;
            this.History = history;
            this.DateCreated = dateCreated;
            this.CreatedBy = createdBy;
            this.DatePublished = datePublished;
            this.PublishedBy = publishedBy;
            this.StartYear = startYear;
            this.EndYear = endYear;
            this.ReleaseNotes = releaseNotes;
        }

        /// <summary>
        /// Gets or sets the PPR&amp;D Revision number.
        /// </summary>
        public string Revision { get; set; }

        /// <summary>
        /// Gets or sets the history, e.g. changes since last revision
        /// </summary>
        public string History { get; set; }

        /// <summary>
        /// Gets the date the PPR&amp;D revision was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the person that created the revision.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets the date the PPR&amp;D revision was published.
        /// </summary>
        public DateTime? DatePublished { get; set; }

        /// <summary>
        /// Gets or sets the name of the person that published the revision.
        /// </summary>
        public string PublishedBy { get; set; }

        /// <summary>
        /// Gets or sets the start year for the revision.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the end year for the revision.
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        /// Gets or sets the release notes for the revision
        /// </summary>
        public string ReleaseNotes { get; set; }

        /// <summary>
        /// Gets boolean value indicating if revision is work-in-progress or published.
        /// </summary>
        public bool IsWipRevision
        {
            get
            {
                return !this.DatePublished.HasValue;
            }
        }

        /// <summary>
        /// Gets the PPR&amp;D Revision number for grid display purposes.
        /// </summary>
        /// <returns>Revision number if the revision has been published; otherwise, "WIP" with the revision number in parenthesis.</returns>
        public string DisplayRevision
        {
            get
            {
                return this.IsWipRevision ? string.Format("WIP ({0})", this.Revision) : this.Revision;
            }
        }

        /// <summary>
        /// Revision Published information
        /// </summary>
        /// <returns>Published Date in MMMMM dd, yyyy format if the revision has been published; "(Work In Progress)" otherwise.</returns>
        public string RevisionPublishedInfo
        {
            get
            {
                return this.IsWipRevision ? "(Work In Progress)" : this.DatePublished.Value.ToString("MMMMM dd, yyyy");
            }
        }
    }
}
