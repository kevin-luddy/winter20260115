// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    /// Checklist Content DTO
    /// </summary>
    [Serializable]
    public class ChecklistContentDto
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ChecklistContentDto()
        {
            this.Content = new Collection<ChecklistContentItem>();
        }

        /// <summary>
        /// Checklist version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Proposal Adequacy Review ID
        /// </summary>
        public int ProposalAdequacyReviewID { get; set; }

        /// <summary>
        /// Checklist Type
        /// </summary>
        public ProposalChecklistType ChecklistType { get; set; }

        /// <summary>
        /// Collection of content items
        /// </summary>
        public ICollection<ChecklistContentItem> Content { get; set; }
    }
}
