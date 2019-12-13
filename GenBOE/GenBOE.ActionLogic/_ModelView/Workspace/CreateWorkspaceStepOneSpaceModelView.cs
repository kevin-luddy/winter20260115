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

    [ExcludeFromCodeCoverage]
    public class CreateWorkspaceStepOneSpaceModelView : CreateWorkspaceStepOneModelView, ICreateWorkspaceStepOneModelView
    {
        public CreateWorkspaceStepOneSpaceModelView()
        {
            this.CostVolumeLeadPricerNTID = string.Empty;
            this.ProposalClass = Constants.PROPOSAL_CLASS_TYPE_NOT_SET;
            this.ContractTypes = new Collection<PickListDto>();
            this.SelectedContractTypes = new Collection<int>();
        }

        [Required(ErrorMessage = "Estimating Lead/Pricer is required.")]
        public string CostVolumeLeadPricerNTID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Line of Business is required.")]
        public int LineOfBusinessID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = ValidationConstants.PROPOSAL_CLASS_REQUIRED)]
        public int ProposalClass { get; set; }

        public ICollection<PickListDto> ContractTypes { get; set; }

        [ContractTypesValidation(SelectedContractTypes = "SelectedContractTypes", ErrorMessage = "Contract Type is required.")]
        public ICollection<int> SelectedContractTypes { get; set; }

        /// <summary>
        /// Proposal Title.  Required for ISGS.  N/A for space.
        /// </summary>
        public string ProposalTitle { get; set; }
    }
}
