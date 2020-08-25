// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;

    public class WorkspaceToCopyModelView
    {
        public WorkspaceToCopyModelView()
        {
            WorkspaceID = -1;
            WorkspaceName = String.Empty;
            Description = String.Empty;
            ShortName = String.Empty;
            ContractStartDate = String.Empty;
            ContractEndDate = String.Empty;
            ProposalSubmittalDate = String.Empty;
            TrackingNumber = String.Empty;
            RFPNumber = String.Empty;
            ContainsOCI = false;
            ContainsContentTemplates = false;
            ShareAndAllowSearch = false;
            CostDecimalPrecision = 2;
            IsUsingEquivalentPerson = false;
            IsUsingTM = false;
            ProjectMapType = (int)IES.Common.ProjectMapType.StandardWithoutOffload;
            LineOfBusinessID = -1;
        }
        
        public int LineOfBusinessID { get; set; }
        public int WorkspaceID { get; set; }
        public string WorkspaceName { get; set; }
        public string Description { get; set; }
        public string ShortName { get; set; }
        public string ContractStartDate { get; set; }
        public string ContractEndDate { get; set; }
        public string ProposalSubmittalDate { get; set; }
        public string TrackingNumber { get; set; }
        public string RFPNumber { get; set; }
        public Boolean ContainsOCI  { get; set; }
        public Boolean ContainsContentTemplates { get; set; }
        public Boolean ShareAndAllowSearch { get; set; }
        
        /// <summary>
        /// Number of decimal digits to use for resource hours.
        /// </summary>
        public int? ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// Number of decimal digits to use for resource costs.
        /// </summary>
        public int CostDecimalPrecision { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using equivalent person (or hours).
        /// </summary>
        public bool IsUsingEquivalentPerson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using T&M.
        /// </summary>
        public bool IsUsingTM { get; set; }

        /// <summary>
        /// Gets or sets Project Map Type (RMS Only)
        /// </summary>
        public int ProjectMapType { get; set; }

        /// <summary>
        /// Get/Set AllowGridEdit flag (enables/disables in app grid edit functionality)
        /// RMS Only, always false for SSC
        /// </summary>
        public bool AllowGridEdit { get; set; }

        /// <summary>
        /// Get/Set the RTE character size limit
        /// </summary>
        public int? RteSizeLimit { get; set; }
    }
}