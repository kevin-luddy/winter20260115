// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic.Validation;
    using IES.Common;

    /// <summary>
    /// Proposal information model view
    /// </summary>
    public class ProposalGeneralInformationModelView : PersistedDataModelView
    {
        /// <summary>
        /// Gets or sets Proposal ID
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Gets or sets Line of Business
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.LINE_OF_BUSINESS_REQUIRED)]
        public string LineOfBusiness { get; set; }

        /// <summary>
        /// Gets or sets Program Area
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalValidationConstants.PROGRAM_AREA_REQUIRED)]
        public string ProgramArea { get; set; }

        /// <summary>
        /// Gets or sets Program Name
        /// </summary>
        [StringLength(50)]
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets OTIS opportunity ID
        /// </summary>
        [StringLength(25)]
        public string OTISOpportunityID { get; set; }

        /// <summary>
        /// Proposal Location
        /// </summary>
        public ProposalLocation ProposalLocation { get; set; }

        /// <summary>
        /// When the Proposal Location field is set to Other, then populate the ProposalLocationName field.
        /// </summary>
        [StringLength(50)]
        public string ProposalLocationName { get; set; }

        /// <summary>
        /// Gets or sets Pricing Tool
        /// </summary>
        public PricingTool PricingTool { get; set; }

        /// <summary>
        /// When the Pricing Tool is set to Other, then populate the PricingToolName field.
        /// </summary>
        [StringLength(50)]
        public string PricingToolName { get; set; }

        /// <summary>
        /// Gets or sets BOE Tool
        /// </summary>
        public BOETool BOETool { get; set; }

        /// <summary>
        /// When the BOE Tool is set to Other, then populate the BOEToolName field
        /// </summary>
        [StringLength(50)]
        public string BOEToolName { get; set; }

        /// <summary>
        /// Gets or sets Cost Volume Tool
        /// </summary>
        public CostVolumeTool CostVolumeTool { get; set; }

        /// <summary>
        /// When Cost Volume Tool is set to Other, then populate CostVolumeToolName field
        /// </summary>
        [StringLength(50)]
        public string CostVolumeToolName { get; set; }

        /// <summary>
        /// Gets or sets text for selected Line of Business
        /// </summary>
        public string LineOfBusinessSelectedText { get; set; }

        /// <summary>
        /// Gets or sets text for selected Program Area
        /// </summary>
        public string ProgramAreaSelectedText { get; set; }

        /// <summary>
        /// List of allowed lines of business
        /// </summary>
        public ICollection<SelectListItem> LinesOfBusinessList { get; set; }

        /// <summary>
        /// Options for Program Areas
        /// </summary>
        public string ProgramAreaHtmlOptions { get; set; }

        /// <summary>
        /// List of allowed proposal locations
        /// </summary>
        public ICollection<SelectListItem> ProposalLocationsList { get; set; }

        /// <summary>
        /// List of allowed pricing tools
        /// </summary>
        public ICollection<SelectListItem> PricingToolsList { get; set; }

        /// <summary>
        /// List of allowed BOE tools
        /// </summary>
        public ICollection<SelectListItem> BOEToolsList { get; set; }

        /// <summary>
        /// List of allowed Cost Volume tools
        /// </summary>
        public ICollection<SelectListItem> CostVolumeToolsList { get; set; }

        /// <summary>
        /// Program Area help text
        /// </summary>
        public string ProgramAreaHelpText { get; set; }

        /// <summary>
        /// Gets or sets Program/Proposal Status
        /// </summary>
        public ProgramProposalStatus ProgramProposalStatus { get; set; }

        /// <summary>
        /// Whether or not certified cost and pricing data is required
        /// </summary>
        public bool? IsCCPDRequired { get; set; }

        /// <summary>
        /// Whether or not cost volume is classified
        /// </summary>
        public bool? IsCostVolumeClassified { get; set; }

        /// <summary>
        /// Whether or not to use the old (GenTRAC) or new (PTM) checklist User Interface
        /// </summary>
        public bool? IsPTMChecklistUIEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CCPD is read only.
        /// </summary>
        public bool IsCCPDReadOnly { get; set; }

		/// <summary>
		/// Higher level clearance required beyond standard clearance requirements
		/// </summary>
		public bool? AdditionalClassification { get; set; }

		/// <summary>
		/// When CCOPD = no, a reason needs to be provided
		/// </summary>
		public CcopdOptionalReason? CcopdNoReason { get; set; }

		/// <summary>
		/// When CCOPD = no, and reason is selected as other, this is the text for it
		/// </summary>
		[StringLength(50)]
		public string CcopdNoOtherReason { get; set; }

		/// <summary>
		/// List of CCOPD = No reasons
		/// </summary>
		public ICollection<SelectListItem> ReasonsForCcopdBeingNo { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProposalGeneralInformationModelView()
        {
            this.ProposalID = -1;
            this.LineOfBusiness = string.Empty;
            this.ProgramArea = string.Empty;
            this.ProgramProposalStatus = ProgramProposalStatus.LMRetainedSSC;
            this.IsCCPDRequired = null;
            this.IsPTMChecklistUIEnabled = true;
        }
    }
}
