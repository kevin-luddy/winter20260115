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
    /// Model View for the checklist Proposal Pricing Review (PPR) Document section
    /// </summary>
    public class ChecklistProposalPricingReviewDocumentModelView : PersistedDataModelView
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
        /// the time the document was last saved
        /// </summary>
        public string LastSavedDate { get; set; }

        /// <summary>
        /// determines what part of the checklist to show in read only mode and what to validate
        /// </summary>
        public ShowChecklistResponse ShowChecklistResponse { get; set; }

        /// <summary>
        /// Whether or not to use the old (GenTRAC) or new (PTM) checklist User Interface
        /// </summary>
        public bool? IsPTMChecklistUIEnabled { get; set; }

        /// <summary>
        /// should display this (PPR) section. this is based off of the "Is Certified Cost or Pricing Data Required?" response
        /// </summary>
        public string ShouldDisplayPPRSection { get; set; }

        /// <summary>
        /// Question 1's sort order.
        /// </summary>
        public int Question1SortOrder { get; set; }

        /// <summary>
        /// pricer comment
        /// </summary>
        public string PPRPricerComment { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChecklistProposalPricingReviewDocumentModelView()
        {
            this.ChecklistRows = new Collection<ChecklistRowModelView>();
            this.PPRPricerComment = string.Empty;
        }
    }
}
