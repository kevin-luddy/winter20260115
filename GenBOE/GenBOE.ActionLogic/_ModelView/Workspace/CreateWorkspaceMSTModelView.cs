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
    /// Model View for the Create Workspace page for MST
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateWorkspaceMSTModelView : CreateWorkspaceModelView, ICreateWorkspaceModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CreateWorkspaceMSTModelView()
        {
            this.CostVolumeLeadPricerNTID = string.Empty;
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
        [Range(LineOfBusinessTypeConstants.MSTMinimumSelectableValue, LineOfBusinessTypeConstants.MSTMaximumSelectableValue, ErrorMessage = "Line of Business is required.")]
        public int LineOfBusinessID { get; set; }
        
        /// <summary>
        /// Gets or sets the segment
        /// </summary>
        public SegmentType Segment { get { return SegmentType.RMS; } }

        /// <summary>
        /// Gets or sets the proposal class.
        /// </summary>
        public int ProposalClass { get; set; }
    }
}
