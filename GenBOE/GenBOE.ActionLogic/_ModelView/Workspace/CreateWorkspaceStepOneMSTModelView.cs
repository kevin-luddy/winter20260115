// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// Model view for step one of create workspace for MST
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateWorkspaceStepOneMSTModelView : CreateWorkspaceStepOneModelView, ICreateWorkspaceStepOneModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CreateWorkspaceStepOneMSTModelView()
        {
            this.CostVolumeLeadPricerNTID = string.Empty;
        }

        /// <summary>
        /// Gets/Sets CostVolumeLeadPricerNTID
        /// </summary>
        [Required(ErrorMessage = "Estimating Lead/Pricer is required.")]
        public string CostVolumeLeadPricerNTID { get; set; }

        /// <summary>
        /// Gets/Sets LineOfBusinessID
        /// </summary>
        [Range(LineOfBusinessTypeConstants.MSTMinimumSelectableValue, LineOfBusinessTypeConstants.MSTMaximumSelectableValue, ErrorMessage = "Line of Business is required.")]
        public int LineOfBusinessID { get; set; }

    }
}
