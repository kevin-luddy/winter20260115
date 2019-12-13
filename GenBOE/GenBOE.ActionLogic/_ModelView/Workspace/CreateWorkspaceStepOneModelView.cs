// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    public class CreateWorkspaceStepOneModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CreateWorkspaceStepOneModelView()
        {
            this.WorkspaceName = null;
            this.Description = string.Empty;
            this.Shortname = string.Empty;
            this.ContractStartDate = "01/1900";
            this.ContractEndDate = "12/2099";
            this.ProposalSubmittalDate = DateTime.Now.ToString("MM/dd/yyyy");
            this.CostVolumeLeadPricerDisplayName = string.Empty;
            this.RFPNumber = string.Empty;
            this.TrackingNumber = string.Empty;
        }

        /// <summary>
        /// The ID of the workspace in the database
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// The name of the workspace
        /// </summary>
        [Required(ErrorMessage = "Workspace Name is required.")]
        [StringLength(100, ErrorMessage = "Workspace Name must not exceed 100 chars.")]
        [ServerValidation(ErrorMessage = "Workspace Name must be Unique", ValidationToPerform = ValidationType.WorkspaceUniqueName)]
        public string WorkspaceName { get; set; }

        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 chars.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "URL is required.")]
        [ServerValidation(ErrorMessage = "URL must be unique", ValidationToPerform = ValidationType.WorkspaceUniqueShortname)]
        // Verifies first character is alphanumeric and Verifies all characters are alphanumeric, -, or _
        [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9-_]*$", ErrorMessage = "URL must begin with a letter or number and URL format can only contain letters, numbers, underscores and hyphens.")]
        [StringLength(21, ErrorMessage = "URL must not exceed 21 chars.")]
        public string Shortname { get; set; }

        public string CostVolumeLeadPricerDisplayName { get; set; }

        [Required(ErrorMessage = "Contract Start Date is required.")]
        // Allows 01-12 for month, 1999-2000 for year
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Contract Start Date format must be in mm/yyyy format.")]
        public string ContractStartDate { get; set; }

        [Required(ErrorMessage = "Contract End Date is required.")]
        // Allows 01-12 for month, 1999-2000 for year
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Contract End Date format must be in mm/yyyy format.")]
        // Contract End Date must occur after the Contract Start Date.
        public string ContractEndDate { get; set; }

        // Allows 01-12 for month, 01-31 for day, 1999-2000 for year
        [RegularExpression(ValidationConstants.DATE_FULL, ErrorMessage = "Proposal Submittal Date format must be mm/dd/yyyy.")]
        public string ProposalSubmittalDate { get; set; }

        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the Tracking#")]
        public string TrackingNumber { get; set; }

        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the RFP#")]
        public string RFPNumber { get; set; }

        /// <summary>
        /// Used to decide whether we are doing an import of data as a part of WS creation
        /// </summary>
        public bool IsAttemptingToImport { get; set; }

        /// <summary>
        /// Number of decimal digits for labor hours precision.
        /// </summary>
        [DisplayName("Resource Decimal Precision")]
        [Range(0, 6, ErrorMessage = "Resource Decimal Precision must be between 0 and 6.")]
        public int? ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// Gets or sets that the new workspace is an exact copy of another workspace.
        /// </summary>
        public bool? WSExactCopy { get; set; }

        /// <summary>
        /// Gets or sets the Rich Text Editor Character Limit
        /// </summary>
        [Range(100, 100000, ErrorMessage = "Rich Text Editor Character Limit must be between 100 and 100,000.")]
        public int? RteSizeLimit { get; set; }

        /// <summary>
        /// Whether to create Sikorsky Custom Fields
        /// </summary>
        public bool CreateSikorskyCustomFields { get; set; }

        /// <summary>
        /// Get/Set the Workspace Type
        /// RMS Only, always None for SSC
        /// </summary>
        public ProjectMapType ProjectMapType { get; set; }
    }
}
