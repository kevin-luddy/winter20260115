// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// ModelView for BOE Project Map Advanced Search.
    /// </summary>
    public class BOEProjectMapAdvancedSearchModelView
    {
        /// <summary>
        /// Gets or sets Workspace Name
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceDescription
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WorkspaceDescription { get; set; }

        /// <summary>
        /// Gets or sets the Activity Id.
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the Activity Name.
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string ActivityName { get; set; }

        /// <summary>
        /// Gets or sets the Sow Title.
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string SowTitle { get; set; }

        /// <summary>
        /// Gets or sets the Task.
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string Task { get; set; }

        /// <summary>
        /// Gets or sets the Rationale.
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string Rationale { get; set; }

        /// <summary>
        /// Gets or sets the Cam Name.
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string CamName { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets WBSNumber
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WBSNumber { get; set; }
        
        /// <summary>
        /// Gets or sets WBSTitle
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WBSTitle { get; set; }

        /// <summary>
        /// When true, the search was initiated from the "Copy from Boe" button, otherwise, the search was initiated from
        /// "Copy MOQ from BOE" in the MOQ Hours Equation dropdown.
        /// </summary>
        public bool IsCopyFromBoeContext { get; set; }
    }
}