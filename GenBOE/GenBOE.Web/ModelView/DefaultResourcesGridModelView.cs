// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.Admin;

    /// <summary>
    /// Model view for default resources grid
    /// </summary>
    public class DefaultResourcesGridModelView : PagedResultsModelView<int>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultResourcesGridModelView()
        {
            CurrentPage = 1;
            PagedIndexes = new Collection<int>();
            ResourceResults = new Collection<DefaultResourceModelView>();
            ResultsPerPage = 200;
        }

        /// <summary>
        /// Gets or sets Total results
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// Gets or sets ResourceResults
        /// </summary>
        public ICollection<DefaultResourceModelView> ResourceResults { get; set; }
    }
}