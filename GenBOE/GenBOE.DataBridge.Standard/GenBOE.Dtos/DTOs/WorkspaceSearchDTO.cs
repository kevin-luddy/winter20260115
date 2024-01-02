// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using IES.Standard;

    /// <summary>
    /// Workspace Search DTO
    /// </summary>
    public class WorkspaceSearchDTO
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WorkspaceSearchDTO()
        {
            this.WorkspaceName = null;
            this.WorkspaceDesc = null;
            this.ProposalSubmitStartDate = null;
            this.ProposalSubmitEndDate = null;
            this.RFPNumber = null;
            this.CostVolumeLead = null;
            this.ProjectMapType = null;
        }

        /// <summary>
        /// Gets or sets WorkspaceName
        /// </summary>
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceDesc
        /// </summary>
        public string WorkspaceDesc { get; set; }
        
        /// <summary>
        /// Gets or sets ProposalSubmitStartDate
        /// </summary>
        public DateTime? ProposalSubmitStartDate { get; set; }

        /// <summary>
        /// Gets or sets ProposalSubmitEndDate
        /// </summary>
        public DateTime? ProposalSubmitEndDate { get; set; }

        /// <summary>
        /// Gets or sets RFPNumber
        /// </summary>
        public string RFPNumber { get; set; }

        /// <summary>
        /// Gets or sets CostVolumnLead
        /// </summary>
        public string CostVolumeLead { get; set; }

        /// <summary>
        /// Gets or sets the ProjectMapType.
        /// </summary>
        public ProjectMapType? ProjectMapType { get; set; }
    }
}
