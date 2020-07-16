// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenTRAC.ActionLogic.ModelView.Checklist
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using IES.Common;

    /// <summary>
    /// checklist row's model view
    /// </summary>
    public class ChecklistRowModelView
    {
        /// <summary>
        /// checklist row type is a CheckListTextType: Intro, Header, Text, Question, PricerComment, PeerComment
        /// </summary>
        public ChecklistTextType RowType { get; set; }

        /// <summary>
        /// peer response is a ChecklistResponseOption: Yes, No, NA, NotSet
        /// </summary>
        public ChecklistResponseOption PeerResponse { get; set; }

        /// <summary>
        /// pricer response is a ChecklistResponseOption: Yes, No, NA, NotSet
        /// </summary>
        public ChecklistResponseOption PricerResponse { get; set; }

        /// <summary>
        /// text to display can be a note, comment, intro, question
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// checklist content id
        /// </summary>
        public int ChecklistContentId { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Column Order
        /// </summary>
        public int ColumnOrder { get; set; }

        /// <summary>
        /// Submission Item
        /// </summary>
        public string SubmissionItem { get; set; }

        /// <summary>
        /// Pricer Page Number for each PAR checklist row, can be digits or numbers, 30 character max
        /// </summary>
        [StringLength(30, ErrorMessage = "The Lead Estimator Page Number cannot exceed 30 characters.")]
        public string PricerPageNumber { get; set; }

        /// <summary>
        /// Pricer Row comment for each PAR checklist row, 300 characters max
        /// </summary>
        [StringLength(300, ErrorMessage = "The Lead Estimator Row Comment cannot exceed 300 characters.")]
        public string PricerRowComment { get; set; }

        /// <summary>
        /// Peer Row comment for each PAR checklist row, 300 characters max
        /// </summary>
        [StringLength(300, ErrorMessage = "The Peer Row Comment cannot exceed 300 characters.")]
        public string PeerRowComment { get; set; }

        /// <summary>
        /// Yes Only
        /// </summary>
        public bool YesOnly { get; set; }

        /// <summary>
        /// Canned Responses
        /// </summary>
        public IDictionary<int, string> CannedResponses { get; set; }

        /// <summary>
        /// Selected Canned Response
        /// </summary>
        public int? SelectedCannedResponse { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public ChecklistRowModelView()
        {
            this.PricerResponse = ChecklistResponseOption.NotSet;
            this.PeerResponse = ChecklistResponseOption.NotSet;
            this.PricerPageNumber = string.Empty;
            this.PricerRowComment = string.Empty;
            this.PeerRowComment = string.Empty;
            this.CannedResponses = new Dictionary<int, string>();
        }
    }
}
