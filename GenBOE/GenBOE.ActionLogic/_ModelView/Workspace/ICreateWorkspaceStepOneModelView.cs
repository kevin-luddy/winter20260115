// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using IES.Common;

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    public interface ICreateWorkspaceStepOneModelView
    {
        string ContractEndDate { get; set; }
        string ContractStartDate { get; set; }
        string CostVolumeLeadPricerDisplayName { get; set; }
        string CostVolumeLeadPricerNTID { get; set; }
        string Description { get; set; }
        bool IsAttemptingToImport { get; set; }
        string ProposalSubmittalDate { get; set; }
        int LineOfBusinessID { get; set; }
        string RFPNumber { get; set; }
        string Shortname { get; set; }
        string TrackingNumber { get; set; }
        int WorkspaceID { get; set; }
        string WorkspaceName { get; set; }

        /// <summary>
        /// Number of decimal digits for labor hours precision.
        /// </summary>
        int? ResourceDecimalPrecision { get; set; }

        // methods from parent class PersistedDataModelView
        DateTime UpdateDate { get; set; }
        string UpdateDateLong { get; set; }

        /// <summary>
        /// Gets or sets that the new workspace is an exact copy of another workspace.
        /// </summary>
        bool? WSExactCopy { get; set; }

        /// <summary>
        /// Gets or sets the Rich Text Editor Character Limit
        /// </summary>
        int? RteSizeLimit { get; set; }

        /// <summary>
        /// Get/Set the Workspace Type
        /// RMS Only, always None for SSC
        /// </summary>
        ProjectMapType ProjectMapType { get; set; }

        /// <summary>
        /// Get/Set whether using Template BOE
        /// </summary>
        bool UsingTemplateBoe { get; set; }

        /// <summary>
        /// Get/set whether using Enable SAP Connection is selected.
        /// </summary>
        bool EnableSAPConnection { get; set; }
    }
}
