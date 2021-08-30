// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// BOE search DTO
    /// </summary>
    public class BOESearchDTO
    {
        /// <summary>
        /// Gets or sets IsQuickSearch
        /// </summary>
        public bool IsQuickSearch { get; set; }

        /// <summary>
        /// Gets or sets SelectedCategory
        /// </summary>
        public SearchCategory SelectedCategory { get; set; }

        /// <summary>
        /// Gets or sets QuickSearchText
        /// </summary>
        public string QuickSearchText { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceName
        /// </summary>
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceDescription
        /// </summary>
        public string WorkspaceDescription { get; set; }

        /// <summary>
        /// Gets or sets StartDate
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets EndDate
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets RFP
        /// </summary>
        public string RFP { get; set; }

        /// <summary>
        /// Gets or sets BoeTitle
        /// </summary>
        public string BoeTitle { get; set; }

        /// <summary>
        /// Gets or sets BoeDescription
        /// </summary>
        public string BoeDescription { get; set; }

        /// <summary>
        /// Gets or sets TaskTitle
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets or sets TaskDescription
        /// </summary>
        public string TaskDescription { get; set; }

        /// <summary>
        /// Gets or sets SourcesOfData
        /// </summary>
        public string SourcesOfData { get; set; }

        /// <summary>
        /// Gets or sets PerformingOrg
        /// </summary>
        public string PerformingOrg { get; set; }

        /// <summary>
        /// Gets or sets CostVolumeLeadUser
        /// </summary>
        public string CostVolumeLeadUser { get; set; }

        /// <summary>
        /// Gets or sets AuthorUser
        /// </summary>
        public string AuthorUser { get; set; }

        /// <summary>
        /// Gets or sets ApproverUser
        /// </summary>
        public string ApproverUser { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceID
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets BOEID
        /// </summary>
        public int BOEID { get; set; }

        /// <summary>
        /// Gets or sets WBSNumber
        /// </summary>
        public string WBSNumber { get; set; }

        /// <summary>
        /// Gets or sets WBSTitle
        /// </summary>
        public string WBSTitle { get; set; }

        /// <summary>
        /// Gets or sets CLINNumber
        /// </summary>
        public string CLINNumber { get; set; }

        /// <summary>
        /// Gets or sets CLINTitle
        /// </summary>
        public string CLINTitle { get; set; }

        /// <summary>
        /// Gets or sets SearchResultsThreshold
        /// </summary>
        public int SearchResultsThreshold { get; set; }

    }
}