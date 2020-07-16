// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    public class WorkspaceSearchModelView : PersistedDataModelView
    {
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WorkspaceProposalName { get; set; }

        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string WorkspaceDescription { get; set; }

        [RegularExpression(@"([0]?[1-9]|1[0-2])/(0?[1-9]|[12][0-9]|3[01])/([1-2]\d{3})", ErrorMessage = "Start Date must be in mm/dd/yyyy format.")]
        public string ProposalStartDate { get; set; }

        [RegularExpression(@"([0]?[1-9]|1[0-2])/(0?[1-9]|[12][0-9]|3[01])/([1-2]\d{3})", ErrorMessage = "End Date must be in mm/dd/yyyy format.")]
        public string ProposalEndDate { get; set; }

        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string RFPNumber { get; set; }

        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        public string CostLead { get; set; }

        /// <summary>
        /// Gets or sets the ProjectMapType.
        /// </summary>
        public ProjectMapType? ProjectMapType { get; set; }
    }

}