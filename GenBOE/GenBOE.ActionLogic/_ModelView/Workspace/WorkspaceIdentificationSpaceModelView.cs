// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using System.Collections.Generic;

    /// <summary>
    /// Workspace Identification (Space Systems Company) ModelView that extends WorkspaceIdentificationModelView
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WorkspaceIdentificationSpaceModelView : WorkspaceIdentificationModelView, IWorkspaceIdentificationModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WorkspaceIdentificationSpaceModelView()
        {
            this.Initialize();
        }

        /// <summary>
        /// Constructor to take in workspace and user DTOs
        /// </summary>
        public WorkspaceIdentificationSpaceModelView(FullWorkspace workspaceDTO, UserDTO costVolumeLeadDTO)
            : base(workspaceDTO, costVolumeLeadDTO)
        {
            this.Initialize();
            if (workspaceDTO != null)
            {
                this.LineOfBusinessTypeID = workspaceDTO.LineOfBusiness.Id;
                this.ProposalClassType = workspaceDTO.ProposalClass.Id;
                this.ContractTypes = new Collection<int>();
                this.SelectedContractTypes = workspaceDTO.SelectedContractTypes;
                this.TrackingNumber = workspaceDTO.TrackingNumber;
                this.IsUsingTM = workspaceDTO.IsUsingTM;
                this.ProposalTitle = workspaceDTO.ProposalTitle;
                this.RevisedSubmittalDate = workspaceDTO.RevisedSubmittalDate.HasValue?
                    workspaceDTO.RevisedSubmittalDate.Value.ToString("MM/dd/yyyy") : 
                    string.Empty;
            }
            if (costVolumeLeadDTO != null)
            {
                this.CostVolumeLeadPricerNTID = costVolumeLeadDTO.NTID;
            }
        }

        /// <summary>
        /// Sets properties to default values
        /// </summary>
        private void Initialize()
        {
            this.LineOfBusinessTypeID = 0;
            this.ProposalClassType = Constants.PROPOSAL_CLASS_TYPE_NOT_SET;
            this.ContractTypes = new Collection<int>();
            this.SelectedContractTypes = new Collection<int>();
            this.CostVolumeLeadPricerNTID = string.Empty;
            this.IsUsingTM = false;
            this.RevisedSubmittalDate = string.Empty;
        }

        /// <summary>
        /// Gets or sets the Cost volumn lead pricer NTID
        /// </summary>
        [Required(ErrorMessage = "Estimating Lead/Pricer is required.")]
        public string CostVolumeLeadPricerNTID { get; set; }

        /// <summary>
        /// Line Of Business Type
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Line of Business is required.")]
        public int LineOfBusinessTypeID { get; set; }

        /// <summary>
        /// Proposal Class Type
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = ValidationConstants.PROPOSAL_CLASS_REQUIRED)]
        public int ProposalClassType { get; set; }

        /// <summary>
        /// Contract Types
        /// </summary>
        public ICollection<int> ContractTypes { get; set; }

        /// <summary>
        /// Selected Contract Types
        /// </summary>
        [ContractTypesValidation(SelectedContractTypes = "SelectedContractTypes", ErrorMessage = "Contract Type is required.")]
        public ICollection<int> SelectedContractTypes { get; set; }

        /// <summary>
        /// Tracking Number
        /// </summary>
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the Tracking Number")]
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the revision workspace.
        /// </summary>
        [StringLength(88, ErrorMessage = "A maximum of 88 characters are allowed for the Workspace Name")]
        public string RevisionWorkspaceName { get; set; }

        /// <summary>
        /// Proposal title.  
        /// </summary>
        public string ProposalTitle { get; set; }

        public String LabelLeadPricer
        {
            get
            {
                return CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using T&amp;M.
        /// </summary>
        public bool IsUsingTM { get; set; }
        
        /// <summary>
        /// Revised Submitted Date. Allows 01-12 for month, 01-31 for day, 1999-2000 for year
        /// </summary>
        public string RevisedSubmittalDate { get; set; }
    }
}
