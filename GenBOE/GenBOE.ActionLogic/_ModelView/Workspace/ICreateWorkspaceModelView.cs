// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using IES.Common;
using System;

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    public interface ICreateWorkspaceModelView
    {
        // Only fields used by all company configurations should be declared here

        /// <summary>
        /// Number of decimal digits for labor hours precision.
        /// </summary>
        int? ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// Number of decimal digits for labor costs precision.
        /// </summary>
        int CostDecimalPrecision { get; set; }

        /// <summary>
        /// Get/Set the ApplicationURL
        /// </summary>
        Uri ApplicationURL { get; set; }

        /// <summary>
        /// Get/Set the BOEsToCopy
        /// </summary>
        System.Collections.ObjectModel.Collection<int> BOEsToCopy { get; set; }

        /// <summary>
        /// Get/Set the ContainsContentTemplates
        /// </summary>
        bool ContainsContentTemplates { get; set; }

        /// <summary>
        /// Get/Set the ContainsOCI
        /// </summary>
        bool ContainsOCI { get; set; }

        /// <summary>
        /// Get/Set the ContractEndDate
        /// </summary>
        string ContractEndDate { get; set; }

        /// <summary>
        /// Get/Set the ContractStartDate
        /// </summary>
        string ContractStartDate { get; set; }

        /// <summary>
        /// Get/Set the CopyLaborSpreads
        /// </summary>
        bool CopyLaborSpreads { get; set; }

        /// <summary>
        /// Get/Set the CopyPermissions
        /// </summary>
        bool CopyPermissions { get; set; }

        /// <summary>
        /// Get/Set the CopyTasks
        /// </summary>
        bool CopyTasks { get; set; }

        /// <summary>
        /// Get/Set the CostVolumeLeadPricerDisplayName
        /// </summary>
        string CostVolumeLeadPricerDisplayName { get; set; }

        /// <summary>
        /// Get/Set the CostVolumeLeadPricerNTID
        /// </summary>
        string CostVolumeLeadPricerNTID { get; set; }

        /// <summary>
        /// Get/Set the Description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Get/Set the ProposalSubmittalDate
        /// </summary>
        string ProposalSubmittalDate { get; set; }

        /// <summary>
        /// Get/Set the LineOfBusinessID
        /// </summary>
        int LineOfBusinessID { get; set; }
        
        /// <summary>
        /// Get/Set the RFPNumber
        /// </summary>
        string RFPNumber { get; set; }

        /// <summary>
        /// Gets or sets the proposal class.
        /// </summary>
        int ProposalClass { get; set; }

        /// <summary>
        /// Gets or sets the proposal title.
        /// </summary>
        string ProposalTitle { get; set; }

        /// <summary>
        /// Get/Set the Segment
        /// </summary>
        SegmentType Segment { get; }

        /// <summary>
        /// Get/Set the ShareAndAllowSearch
        /// </summary>
        bool ShareAndAllowSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using equivalent person.
        /// </summary>
        bool IsUsingEquivalentPerson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using T&amp;M.
        /// </summary>
        bool IsUsingTM { get; set; }

        /// <summary>
        /// Get/Set the Shortname
        /// </summary>
        string Shortname { get; set; }

        /// <summary>
        /// Get/Set the TrackingNumber
        /// </summary>
        string TrackingNumber { get; set; }

        /// <summary>
        /// Get/Set the WorkspaceID
        /// </summary>
        int WorkspaceID { get; set; }

        /// <summary>
        /// Get/Set the WorkspaceName
        /// </summary>
        string WorkspaceName { get; set; }

        /// <summary>
        /// Get/Set the WorkspaceToCopyID
        /// </summary>
        int WorkspaceToCopyID { get; set; }

        /// <summary>
        /// Get/Set the WSExactCopy
        /// </summary>
        bool? WSExactCopy { get; set; }

        /// <summary>
        /// Get/Set the UpdateDate
        /// </summary>
        DateTime UpdateDate { get; set; }

        /// <summary>
        /// Get/Set the UpdateDateLong
        /// </summary>
        string UpdateDateLong { get; set; }

        /// <summary>
        /// Get/Set the Workspace Type
        /// RMS Only, always None for SSC
        /// </summary>
        ProjectMapType ProjectMapType { get; set; }

        /// <summary>
        /// Get/Set AllowGridEdit flag (enables/disables in-app grid edit functionality)
        /// RMS Only, always false for SSC
        /// </summary>
        bool AllowGridEdit { get; set; }

        /// <summary>
        /// Get/Set the RTE Size Limit (number of characters)
        /// </summary>
        int? RteSizeLimit { get; set; }

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
