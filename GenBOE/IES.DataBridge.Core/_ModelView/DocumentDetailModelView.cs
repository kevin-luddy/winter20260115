// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;

    /// <summary>
    /// The model for RDSB document detail
    /// </summary>
    public class DocumentDetailModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDetailModelView"/> class.
        /// </summary>
        public DocumentDetailModelView()
        {
            this.AvailableRevisions = new List<RevisionModelView>();
            this.SelectedRateCodeIds = new List<int>();
            this.SelectedSectionIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the proposal title.
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Gets or sets the proposal tracking number.
        /// </summary>
        public string ProposalTrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the proposal status.
        /// </summary>
        public string ProposalStatus { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Id.
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who created the document
        /// </summary>
        public string DocumentCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the selected revision Id.
        /// </summary>
        public int? SelectedRevisionId { get; set; }

        /// <summary>
        /// Gets or sets the Start Year.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the End Year.
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        /// Gets or sets a collection of associated Rate Code IDs
        /// </summary>
        public ICollection<int> SelectedRateCodeIds { get; set; }

        /// <summary>
        /// Gets or sets a collection of associated Section IDs
        /// </summary>
        public ICollection<int> SelectedSectionIds { get; set; }
        
        /// <summary>
        /// Gets or sets a collection of available revisions
        /// </summary>
        public ICollection<RevisionModelView> AvailableRevisions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using latest PPRD Revision.
        /// </summary>
        public bool IsUsingLatest { get; set; }

        /// <summary>
        /// Gets or sets the parent section to be put in front of section numbers.
        /// </summary>
        public string ParentSection { get; set; }
    }
}
