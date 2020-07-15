// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// BOE Project Map search DTO.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BOEProjectMapSearchDTO
    {
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
        /// Gets or sets WorkspaceID
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceDescription
        /// </summary>
        public string WorkspaceDescription { get; set; }

        /// <summary>
        /// Gets or sets the Activity Id.
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the Activity Name.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Gets or sets the Sow Title.
        /// </summary>
        public string SowTitle { get; set; }

        /// <summary>
        /// Gets or sets the Task.
        /// </summary>
        public string Task { get; set; }

        /// <summary>
        /// Gets or sets the Rationale.
        /// </summary>
        public string Rationale { get; set; }

        /// <summary>
        /// Gets or sets the Cam Name.
        /// </summary>
        public string CamName { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets/Sets WBSNumber
        /// </summary>
        public string WBSNumber { get; set; }

        /// <summary>
        /// Gets/Sets WBSTitle
        /// </summary>
        public string WBSTitle { get; set; }

        /// <summary>
        /// Gets or sets SearchResultsThreshold
        /// </summary>
        public int SearchResultsThreshold { get; set; }

    }
}