// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;

    public class SearchResultsModelView : PagedResultsModelView<int>
    {
        public SearchResultsModelView()
        {
            this.CurrentPage = 1;
            this.PagedIndexes = new Collection<int>();
            this.BOEResults = new Collection<BOESearchResult>();
            this.ResultsPerPage = 10;
            this.SearchResultsMessage = string.Empty;
        }

        /// <summary>
        /// Alternate constructor.
        /// </summary>
        /// <param name="isCopyFromBoeContext">When true, the search was initiated from the "Copy from another BOE" button,
        /// otherwise the search was initiated from "Copy MOQ from BOE" in the MOQ Hours Equation dropdown.</param>
        public SearchResultsModelView(bool isCopyFromBoeContext)
            : this()
        {
            this.IsCopyFromBoeContext = isCopyFromBoeContext;
        }

        public ICollection<BOESearchResult> BOEResults { get; set; }

        /// <summary>
        /// When true, the search was initiated from the "Copy from Boe" button, otherwise, the search was initiated from
        /// "Copy MOQ from BOE" in the MOQ Hours Equation dropdown.
        /// </summary>
        public bool IsCopyFromBoeContext { get; set; }

        /// <summary>
        /// Message to be displayed, e.g. if search results exceed threshold.
        /// </summary>
        public string SearchResultsMessage { get; set; }                
    }
}