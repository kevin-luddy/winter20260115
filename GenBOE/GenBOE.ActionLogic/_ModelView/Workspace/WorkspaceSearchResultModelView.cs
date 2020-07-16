// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using IES.Common;

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    /// <summary>
    /// Model view for Workspace Search results
    /// </summary>
    public class WorkspaceSearchResultModelView : PagedResultsModelView<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkspaceSearchResultModelView()
        {
            this.CurrentPage = 1;
            this.PagedIndexes = new Collection<int>();
            this.WorkspaceResults = new Collection<WorkspaceSearchResult>();
            this.ResultsPerPage = 10;
        }

        /// <summary>
        /// Gets/Sets WorkspaceResults
        /// </summary>
        public Collection<WorkspaceSearchResult> WorkspaceResults { get; set; }

        /// <summary>
        /// Gets/Sets LabelLeadPricer
        /// </summary>
        public string LabelLeadPricer { get; set; }
    }

    /// <summary>
    /// Model view for a Workspace search result
    /// </summary>
    public class WorkspaceSearchResult
    {
        /// <summary>
        /// Gets/Sets WorkspaceID
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// Gets/Sets WorkspaceName
        /// </summary>
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets/Sets SubmittalDate
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? SubmittalDate { get; set; }

        /// <summary>
        /// Gets/Sets ContractStartDate
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? ContractStartDate { get; set; }

        /// <summary>
        /// Gets/Sets ContractEndDate
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? ContractEndDate { get; set; }
        
        /// <summary>
        /// Gets/Sets Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/Sets CostVolumeLeadPricer
        /// </summary>
        public string CostVolumeLeadPricer { get; set; }

        /// <summary>
        /// Gets or sets the ProjectMapType.
        /// </summary>
        public ProjectMapType ProjectMapType { get; set; }

        /// <summary>
        /// Gets or sets whether this is a Project Map Workspace.
        /// </summary>
        public bool IsProjectMapWorkspace { get; set; }
    }
}