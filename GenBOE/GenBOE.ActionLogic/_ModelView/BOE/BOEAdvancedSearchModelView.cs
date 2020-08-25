// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.Validation;
    using IES.Common;

    /// <summary>
    /// ModelView for BOE Advanced Search
    /// </summary>
    public class BOEAdvancedSearchModelView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BOEs")]
        public BOEAdvancedSearchModelView()
        {
            this.Categories = new Collection<SelectListItem>() {
                new SelectListItem { Text = "Select Category", Value="" },
                new SelectListItem { Text = "BOEs in other Workspaces", Value = ((int)SearchCategory.BOEsInOtherWorkspaces).ToString() },
                new SelectListItem { Text = "BOE Content Templates", Value = ((int)SearchCategory.BOEContentTemplates).ToString() },
                new SelectListItem { Text = "BOEs in this Workspace", Value = ((int)SearchCategory.BOEsInThisWorkspace).ToString() },
                new SelectListItem { Text = "All", Value = ((int)SearchCategory.All).ToString() }
            };
        }

        /// <summary>
        /// Gets/Sets Categories
        /// </summary>
        public Collection<SelectListItem> Categories { get; set; }

        /// <summary>
        /// Gets/Sets SelectedCategory
        /// </summary>
        [Required(ErrorMessage = "A category must be selected.")]
        public SearchCategory SelectedCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this workspace is project map discrete.
        /// </summary>
        public bool IsProjectMapDiscrete { get; set; }

        /// <summary>
        /// Gets/Sets Workspace Name
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets/Sets WorkspaceDescription
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WorkspaceDescription { get; set; }

        /// <summary>
        /// Gets/Sets StartDate
        /// </summary>
        [RegularExpression(ValidationConstants.DATE_FULL, ErrorMessage = "Start Date must be in mm/dd/yyyy format.")]
        public string StartDate { get; set; }

        /// <summary>
        /// Gets/Sets EndDate
        /// </summary>
        [RegularExpression(ValidationConstants.DATE_FULL, ErrorMessage = "End Date must be in mm/dd/yyyy format.")]
        public string EndDate { get; set; }

        /// <summary>
        /// Gets/Sets RFP
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string RFP { get; set; }

        /// <summary>
        /// Gets/Sets BOETitle
        /// </summary>
        [StringLength(ValidationConstants.MAX_BOE_TITLE_LENGTH)]
        public string BOETitle { get; set; }

        /// <summary>
        /// Gets/Sets BoeDescription
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string BoeDescription { get; set; }

        /// <summary>
        /// Gets/Sets WBSNumber
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WBSNumber { get; set; }
        
        /// <summary>
        /// Gets/Sets WBSTitle
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WBSTitle { get; set; }

        /// <summary>
        /// Gets/Sets CLINNumber
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string CLINNumber { get; set; }

        /// <summary>
        /// Gets/Sets CLINTitle
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string CLINTitle { get; set; }

        /// <summary>
        /// Gets/Sets TaskTitle
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets/Sets TaskDescription
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string TaskDescription { get; set; }

        /// <summary>
        /// Gets/Sets SourcesOfData
        /// </summary>
        [StringLength(200, ErrorMessage = "A maximum of 200 characters are allowed")]
        public string SourcesOfData { get; set; }

        /// <summary>
        /// Gets/Sets PerformingOrg
        /// </summary>
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string PerformingOrg { get; set; }

        /// <summary>
        /// Gets/Sets CostVolumeLeadNTID
        /// </summary>
        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        public string CostVolumeLeadNTID { get; set; }

        /// <summary>
        /// Gets/Sets AuthorNTID
        /// </summary>
        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        public string AuthorNTID { get; set; }

        /// <summary>
        /// Gets/Sets ApproverNTID
        /// </summary>
        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        public string ApproverNTID { get; set; }

        /// <summary>
        /// When true, the search was initiated from the "Copy from Boe" button, otherwise, the search was initiated from
        /// "Copy MOQ from BOE" in the MOQ Hours Equation dropdown.
        /// </summary>
        public bool IsCopyFromBoeContext { get; set; }

        /// <summary>
        /// Gets/Sets the label text for the Lead/Pricer
        /// </summary>
        public String LabelLeadPricer { get; set; }

        /// <summary>
        /// Gets/Sets if the RFP field should be shown
        /// </summary>
        public bool ShowRFP { get; set; }
    }
}