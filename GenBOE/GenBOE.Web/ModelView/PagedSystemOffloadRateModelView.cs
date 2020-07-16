// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using ActionLogic.ModelView.Admin;
    using GenBOE.ActionLogic.ModelView;

    /// <summary>
    /// Paging class for Managing System Offload Rates.
    /// </summary>
    /// <seealso cref="GenBOE.ActionLogic.ModelView.PagedResultsModelView{System.Int32}" />
    public class PagedSystemOffloadRateModelView : PagedResultsModelView<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedSystemOffloadRateModelView"/> class.
        /// </summary>
        public PagedSystemOffloadRateModelView()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            Results = new Collection<OffloadRateModelView>();
            ResultsPerPage = 100;
            SearchFilter = "";
        }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        public ICollection<OffloadRateModelView> Results { get; set; }
    }
}