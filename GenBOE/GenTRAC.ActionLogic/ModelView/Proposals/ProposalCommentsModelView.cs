// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{

    public class ProposalCommentsModelView : PersistedDataModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalCommentsModelView()
        {
            Comments = string.Empty;
        }

        /// <summary>
        /// Get/Set Proposal Comments
        /// </summary>
        public string Comments { get; set; }
    }
}
