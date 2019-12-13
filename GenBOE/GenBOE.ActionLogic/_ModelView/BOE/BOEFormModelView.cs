// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// The base ModelView for BOE Forms.
    /// </summary>
    /// <seealso cref="GenBOE.ActionLogic.ModelView.PersistedDataModelView" />
    public class BOEFormModelView : PersistedDataModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BOEFormModelView"/> class.
        /// </summary>
        public BOEFormModelView()
        {
            this.BOEFormId = -1;
            this.BOEFormName = string.Empty;
            this.ClinContractTypes = new List<BoeFormClinContractTypeDTO>();
            this.Description = string.Empty;
            this.Approver = string.Empty;
            this.ApproverPhone = string.Empty;
            this.BasisAndRationale = string.Empty;
            this.Poc = string.Empty;
            this.PocPhone = string.Empty;
            this.ProposalDate = string.Empty;
            this.ProposalTitle = string.Empty;
            this.ResourceIds = new List<int>();
            this.HasValidTMRates = false;
            this.Revision = "0";
            this.UpdateType = UpdateType.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BOEFormModelView"/> class.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <exception cref="System.ArgumentNullException">dto</exception>
        public BOEFormModelView(BOEFormDTO dto)
        {
            if (ReferenceEquals(dto, null))
            {
                throw new ArgumentNullException(nameof(dto));
            }

            this.BOEFormId = dto.Id;
            this.BOEFormName = dto.FormName;
            this.ClinContractTypes = dto.ClinContractTypes;
            this.Description = dto.Description;
            this.Approver = dto.Approver;
            this.ApproverPhone = dto.ApproverPhone;
            this.BasisAndRationale = dto.BasisAndRationale;
            this.BOEFormType = dto.BOEFormType;
            this.Poc = dto.Poc;
            this.PocPhone = dto.PocPhone;
            this.ProposalDate = dto.ProposalDate;
            this.ProposalTitle = dto.ProposalTitle;
            this.ResourceIds = dto.ResourceIds;
            this.HasValidTMRates = false;
            this.Revision = dto.Revision.ToString();
            this.UpdateDate = dto.UpdateDate;
            this.UpdateDateLong = dto.UpdateDateLong;
            this.Version = dto.Version;
            this.UpdateType = UpdateType.None;
            
            this.TotalCost = 0;
            this.TMCost = 0;

        }

        /// <summary>
        /// Gets or sets the boe form identifier.
        /// </summary>
        [Required]
        public int BOEFormId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the type of the boe form.
        /// </summary>
        [Required]
        public BOEFormType BOEFormType
        {
            get; set;
        }

        /// <summary>
        /// Form Name
        /// </summary>
        [Required]
        [StringLength(200, ErrorMessage = "A maximum of 200 characters are allowed for Form Name.")]
        public string BOEFormName { get; set; }

        /// <summary>
        /// Total Cost
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// TM Cost
        /// </summary>
        public decimal TMCost { get; set; }

        /// <summary>
        /// The collection of Resource Ids.
        /// </summary>
        public ICollection<int> ResourceIds { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether there are valid T&amp;M rates for all the resources in this form.
        /// </summary>
        public bool HasValidTMRates { get; set; }

        /// <summary>
        /// Mark for Deletion
        /// </summary>
        public UpdateType UpdateType { get; set; }

        /// <summary>
        /// Gets or sets the version number of the BOE Form.
        /// </summary>
        [Required]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the revision number.
        /// </summary>
        [Required(ErrorMessage = "Revision is a required numeric input.")]
        [Range(0, int.MaxValue, ErrorMessage = "Revision must be a number >= 0.")]
        public string Revision { get; set; }

        /// <summary>
        /// Gets or sets the proposal title.
        /// </summary>
        [StringLength(200, ErrorMessage = "A maximum of 200 characters are allowed for Proposal Title.")]
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Gets or sets the proposal date.
        /// </summary>
        [RegularExpression(ValidationConstants.DATE_FULL, ErrorMessage = "Proposal Date must be in mm/dd/yyyy format.")]
        public string ProposalDate { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_DESCRIPTION, "PBOEFormID")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the basis and rationale.
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_RTE_LENGTH)]
        [RichText(RichTextDbColumn.PBOE_BASIS_RATIONALE, "PBOEFormID")]
        public string BasisAndRationale { get; set; }

        /// <summary>
        /// Gets or sets the Point of Contact.
        /// </summary>
        [StringLength(65, ErrorMessage = "A maximum of 65 characters are allowed for Point of Contact.")]
        public string Poc { get; set; }

        /// <summary>
        /// Gets or sets the POC phone.
        /// </summary>
        [RequiredIf("PocSet", true, ErrorMessage = "Invalid Point of Contact, please either use Lookup or use Check Name.")]
        [StringLength(60, ErrorMessage = "A maximum of 60 characters are allowed for Point of Contact Phone.")]
        public string PocPhone { get; set; }

        /// <summary>
        /// Gets a value indicating whether [poc is set].
        /// </summary>
        public bool PocSet { get { return !string.IsNullOrWhiteSpace(this.Poc); } }

        /// <summary>
        /// Gets or sets the Approver.
        /// </summary>
        [StringLength(65, ErrorMessage = "A maximum of 65 characters are allowed for Approver.")]
        public string Approver { get; set; }

        /// <summary>
        /// Gets or sets the Approver phone.
        /// </summary>
        [RequiredIf("ApproverSet", true, ErrorMessage = "Invalid Point of Contact, please either use Lookup or use Check Name.")]
        [StringLength(60, ErrorMessage = "A maximum of 60 characters are allowed for Approver Phone.")]
        public string ApproverPhone { get; set; }

        /// <summary>
        /// Gets a value indicating whether [approver is set].
        /// </summary>
        public bool ApproverSet { get { return !string.IsNullOrWhiteSpace(this.Approver); } }

        /// <summary>
        /// Gets or sets the Clin Contract Types.
        /// </summary>
        public ICollection<BoeFormClinContractTypeDTO> ClinContractTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is incomplete.
        /// </summary>
        public bool IsIncomplete { get; set; }

        /// <summary>
        /// Gets or sets the incomplete messages.
        /// </summary>
        public ICollection<string> IncompleteMessages { get; set; }
    }
}