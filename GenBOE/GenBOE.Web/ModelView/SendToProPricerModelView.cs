// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{

    /// <summary>
    /// ModelView for the data in the SendToProPricer pop-up
    /// </summary>
    public class SendToProPricerModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SendToProPricerModelView()
        {
            this.SelectedInstance = -1;
            this.SelectedProposalId = string.Empty;
        }

        /// <summary>
        /// Gets/Sets the selected instance
        /// </summary>
        public int? SelectedInstance { get; set; }

        /// <summary>
        /// Gets or sets the selected proposal.
        /// </summary>
        public string SelectedProposalId { get; set; }
    }
}