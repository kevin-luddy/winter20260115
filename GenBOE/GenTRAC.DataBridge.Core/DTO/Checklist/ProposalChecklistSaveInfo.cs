// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Core;

    /// <summary>
    /// Represents proposal checklist save information for a single user
    /// </summary>
    [Serializable]
    public class ProposalChecklistSaveInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalChecklistSaveInfo()
        {
            this.Comment = string.Empty;
        }

        /// <summary>
        /// User ID of user saving/submitting data
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// Last save date
        /// </summary>
        public DateTime? LastSaveDate { get; set; }

        /// <summary>
        /// Submit date
        /// </summary>
        public DateTime? SubmitDate { get; set; }

        /// <summary>
        /// Checklist comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Checklist response type
        /// </summary>
        public ChecklistResponseType ResponseType { get; set; }

        /// <summary>
        /// Checklist type
        /// </summary>
        public ChecklistType ChecklistType { get; set; }
    }
}
