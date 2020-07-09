// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using IES.Common;
    using IES.Common.PickList;

    /// <summary>
    /// Model View for the Create Workspace page for Space
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateWorkspaceSpaceModelView : CreateWorkspaceModelView, ICreateWorkspaceModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CreateWorkspaceSpaceModelView()
        {
            this.CostVolumeLeadPricerNTID = string.Empty;
            this.ProposalClass = Constants.PROPOSAL_CLASS_TYPE_NOT_SET;
            this.ContractTypes = new Collection<PickListDto>();
            this.SelectedContractTypes = new Collection<int>();
            this.RevisedSubmittalDate = string.Empty;
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return base.ToString() +
                ", CostVolumeLeadPricerNTID=" + this.CostVolumeLeadPricerNTID +
                ", LineOfBusinessID=" + this.LineOfBusinessID;
        }

        /// <summary>
        /// Gets or sets the cost volumn lead pricer NTID
        /// </summary>
        [Required(ErrorMessage = "Estimating Lead/Pricer is required.")]
        public string CostVolumeLeadPricerNTID { get; set; }

        /// <summary>
        /// Gets or sets the line of business id
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Line of Business is required.")]
        public int LineOfBusinessID { get; set; }

        /// <summary>
        /// Gets or sets the proposal class
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = ValidationConstants.PROPOSAL_CLASS_REQUIRED)]
        public int ProposalClass { get; set; }

        /// <summary>
        /// Gets or sets the contract types
        /// </summary>
        public ICollection<PickListDto> ContractTypes { get; set; }

        /// <summary>
        /// Gets or sets the selected contract types
        /// </summary>
        [ContractTypesValidation(SelectedContractTypes = "SelectedContractTypes", ErrorMessage = "Contract Type is required.")]
        public ICollection<int> SelectedContractTypes { get; set; }

        /// <summary>
        /// Gets the segment
        /// </summary>
        public SegmentType Segment { get { return SegmentType.SSC; } }

        /// <summary>
        /// Summary Revised Submittal Date of the Proposal
        /// 
        /// Now labeled as Revised Anticipated Delivery Date
        /// </summary>
        public string RevisedSubmittalDate { get; set; }
    }
}
