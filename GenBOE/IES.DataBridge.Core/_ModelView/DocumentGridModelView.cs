// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using IES.Common.Core.Enums;
	using System;

    /// <summary>
    /// The model for a row in the Document Grid
    /// </summary>
    public class DocumentGridModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Gets or sets the proposal title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Gets or sets the proposal tracking number
        /// </summary>
        public string ProposalTrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Status
        /// </summary>
        public string ProposalStatus { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who created the document
        /// </summary>
        public string DocumentCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the document create date
        /// </summary>
        public DateTime DocumentCreated { get; set; }

        /// <summary>
        /// Gets or sets the RDM revision identifier.
        /// </summary>
        public int RDMRevisionId { get; set; }

        /// <summary>
        /// Gets or sets the PPR&amp;D version used
        /// </summary>
        public string PPRDVersion { get; set; }

        /// <summary>
        /// Gets or sets the PPR&amp;D version date
        /// </summary>
        public DateTime PPRDVersionDate { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Id.
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Gets or sets the Start Year.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the End Year.
        /// </summary>
        public int EndYear { get; set; }

		/// <summary>
		/// Revision segment.
		/// </summary>
		public RevisionSegment RevisionSegment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using latest PPRD Revision.
        /// </summary>
        public bool IsUsingLatest { get; set; }
    }
}
