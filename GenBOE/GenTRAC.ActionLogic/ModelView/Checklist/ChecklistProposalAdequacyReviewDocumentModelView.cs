// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Checklist
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;

    /// <summary>
    /// Checklist Proposal Adequacy Review (PAR) Document model view
    /// </summary>
    public class ChecklistProposalAdequacyReviewDocumentModelView : PersistedDataModelView
    {
          /// <summary>
        /// Gets or sets Proposal ID
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Gets or sets Proposal Checklist ID
        /// </summary>
        public int ProposalChecklistID { get; set; }

        /// <summary>
        /// checklist rows
        /// </summary>
        public ICollection<ChecklistRowModelView> ChecklistRows { get; set; }

        /// <summary>
        /// the time the document was last saved by the pricer
        /// </summary>
        public string PricerLastSavedDate { get; set; }

        /// <summary>
        /// determines what part of the checklist to show in read only mode
        /// </summary>
        public ShowChecklistResponse ShowChecklistResponse { get; set; }

        /// <summary>
        /// should display this (PAR) section. this is based off of the PPR's Question 1 response
        /// or "Is Certified Cost or Pricing Data Required?" response
        /// </summary>
        public string ShouldDisplayPARSection { get; set; }

        /// <summary>
        /// Whether or not to use the old (GenTRAC) or new (PTM) checklist User Interface
        /// </summary>
        public bool? IsPTMChecklistUIEnabled { get; set; }

        /// <summary>
        /// the column span for the second header column
        /// </summary>
        public int SecondHeaderColSpan { get; set; }

        /// <summary>
        /// the column span for the comment row
        /// </summary>
        public int CommentColSpan { get; set; }

        /// <summary>
        /// the column span for the document's section final row 
        /// </summary>
        public int DocumentFinalRowColSpan { get; set; }

        /// <summary>
        /// the column span for the text row
        /// </summary>
        public int TextColSpan { get; set; }

        /// <summary>
        /// The column span for the radio response, either Yes, No, and sometimes N/A
        /// </summary>
        public int RadioResponseColSpan { get; set; }

        /// <summary>
        /// The column span class that will be used for the top header of the radio response columns.
        /// </summary>
        public string RadioResponseColSpanWidth { get; set; }

        /// <summary>
        /// pricer comment
        /// </summary>
        public string PARPricerComment { get; set; }

        /// <summary>
        /// peer comment
        /// </summary>
        public string PARPeerComment { get; set; }

        /// <summary>
        /// show the comment column
        /// </summary>
        public bool ShowComment { get; set; }

        /// <summary>
        /// Show the N/A column
        /// </summary>
        public bool ShowNA { get; set; }

        /// <summary>
        /// checklist version used for PAR Checklist Report
        /// </summary>
        public int? ChecklistVersion { get; set; }

        /// <summary>
        /// Proposal Adequacy Review ID
        /// </summary>
        public int? ProposalAdequacyReviewID { get; set; }

        /// <summary>
        /// Proposal status for the current proposal
        /// </summary>
        public bool ShowExportButton { get; set; }

        /// <summary>
        /// The type of Checklist, Default, International/commercial, etc.
        /// </summary>
        public ProposalChecklistType ProposalChecklistType { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChecklistProposalAdequacyReviewDocumentModelView()
        {
            this.ChecklistRows = new Collection<ChecklistRowModelView>();
            this.PricerLastSavedDate = string.Empty;
            this.SecondHeaderColSpan = 1;
            this.CommentColSpan = 1;
            this.TextColSpan = 1;
            this.RadioResponseColSpan = 1;
            this.DocumentFinalRowColSpan = 1;
            this.PARPricerComment = string.Empty;
            this.PARPeerComment = string.Empty;
            this.ShowComment = false;
            this.ShowNA = true;
            this.ChecklistVersion = null;
            this.ShowExportButton = false;
            this.ProposalChecklistType = ProposalChecklistType.Default;
            this.IsPTMChecklistUIEnabled = true;
        }
    }
}
