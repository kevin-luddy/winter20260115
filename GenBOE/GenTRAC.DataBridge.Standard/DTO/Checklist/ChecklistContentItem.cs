// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using IES.Standard;

    /// <summary>
    /// Checklist Item
    /// </summary>
    [Serializable]
    public class ChecklistContentItem
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Checklist type
        /// </summary>
        public ChecklistType ChecklistType { get; set; }

        /// <summary>
        /// Text of content
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Text type
        /// </summary>
        public ChecklistTextType TextType { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Checklist Id
        /// </summary>
        public int ChecklistId { get; set; }

        /// <summary>
        /// Column Order
        /// </summary>
        public int ColumnOrder { get; set; }

        /// <summary>
        /// Submission Item
        /// </summary>
        public string SubmissionItem { get; set; }

        /// <summary>
        /// Canned Responses
        /// </summary>
        public IDictionary<int, string> CannedResponses { get; set; }

        /// <summary>
        /// Is the question a yes-only?
        /// </summary>
        public bool YesOnly { get; set; }
    }
}
