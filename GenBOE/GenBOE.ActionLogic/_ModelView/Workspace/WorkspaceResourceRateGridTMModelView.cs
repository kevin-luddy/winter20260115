// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// WorkspaceResourceRateGridTMModelView
    /// </summary>
    public class WorkspaceResourceRateGridTMModelView : SortablePagedResultsModelView<int>
    {
        /// <summary>
        /// WorkspaceResourceRateGridTMModelView constructor
        /// </summary>
        public WorkspaceResourceRateGridTMModelView()
        {
            this.CurrentPage = 1;
            this.PagedIndexes = new Collection<int>();
            this.WorkspaceResourceRateTMResults = new Collection<WorkspaceResourceRateTMModelView>();
            this.ResultsPerPage = 100;
        }

        /// <summary>
        /// WorkspaceResourceRateTMResults
        /// </summary>
        public ICollection<WorkspaceResourceRateTMModelView> WorkspaceResourceRateTMResults { get; set; }
    }
}
