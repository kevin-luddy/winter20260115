// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Home
{
    using System.Collections.Generic;

    /// <summary>
    /// Home My Proposals model view
    /// </summary>
    public class HomeProposalModelView : SortableModelView<HomeProposalGridModelView>
    {
        /// <summary>
        /// Home proposal model view
        /// </summary>
        public HomeProposalModelView()
        {
            this.DataRows = new List<HomeProposalGridModelView>();
        }

        /// <summary>
        /// Determines whether logged in user has the proper system roles to create a new proposal
        /// </summary>
        public bool AllowNewProposal { get; set; }

        /// <summary>
        /// The default sort field name
        /// </summary>
        public const string DEFAULT_SORT = "ProposalDueDate";

        /// <summary>
        /// Program Area help text
        /// </summary>
        public string ProgramAreaHelpText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can create BOE workspace.
        /// </summary>
        public bool CanCreateBOEWorkspace { get; set; }
    }
}
