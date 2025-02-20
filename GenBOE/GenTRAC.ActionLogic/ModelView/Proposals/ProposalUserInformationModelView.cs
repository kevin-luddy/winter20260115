// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Proposals
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Proposal information model view
    /// </summary>
    public class ProposalUserInformationModelView : PersistedDataModelView
    {
        /// <summary>
        /// Gets or sets Capture Manager's NTID
        /// </summary>
        public string CaptureManagerNtid { get; set; }

        /// <summary>
        /// Gets or sets Capture Manager's account display name
        /// </summary>
        public string CaptureManagerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets Cost Volume Lead's NTID
        /// </summary>
        public string CostVolumeLeadNtid { get; set; }

        /// <summary>
        /// Gets or sets Cost Volume Lead's account display name
        /// </summary>
        public string CostVolumeLeadDisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets Additional Pricing Resource 1 Display Name
        /// </summary>        
        public string AdditionalPricingResource1DisplayName { get; set; }

        /// <summary>
        /// Gets or sets Additional Pricing Resource 1 nt id
        /// </summary>
        public string AdditionalPricingResource1NtId { get; set; }

        /// <summary>
        /// Gets or sets Additional Pricing Resource Type 1
        /// </summary>
        public ResourceType AdditionalPricingResource1Type { get; set; }

        /// <summary>
        /// Gets or sets Additional Pricing Resource 2 display name
        /// </summary>        
        public string AdditionalPricingResource2DisplayName { get; set; }

        /// <summary>
        /// Gets or sets Additional Pricing Resource 2 Nt ID
        /// </summary>
        public string AdditionalPricingResource2NtId { get; set; }

        /// <summary>
        /// Gets or sets Additional Pricing Resource Type 2
        /// </summary>
        public ResourceType AdditionalPricingResource2Type { get; set; }

        /// <summary>
        /// Gets or sets Additional Pricing Resource Type ist
        /// </summary>
        public ICollection<SelectListItem> AdditionalPricingResourceTypeList { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Materials Display Name
		/// </summary>
		public string SupplyChainPOCMaterialsDisplayName { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Materials NT id
		/// </summary>
		public string SupplyChainPOCMaterialsNtId { get; set; }

		/// <summary>
		/// Gets or sets the list of Supply Chain POC Materials Leads
		/// </summary>
		public List<SelectListItem> SupplyChainPOCMaterialsLeadsList { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Materials Backup Display Name
		/// </summary>
		public string SupplyChainPOCMaterialsBackupDisplayName { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Materials Backup NT id
		/// </summary>
		public string SupplyChainPOCMaterialsBackupNtId { get; set; }

		/// <summary>
		/// Gets or sets the list of Supply Chain POC Materials Backup Leads
		/// </summary>
		public List<SelectListItem> SupplyChainPOCMaterialsBackupLeadsList { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Subs Display Name
		/// </summary>
		public string SupplyChainPOCSubsDisplayName { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Subs NT id
		/// </summary>
		public string SupplyChainPOCSubsNtId { get; set; }

		/// <summary>
		/// Gets or sets the list of Supply Chain POC Subs Leads
		/// </summary>
		public List<SelectListItem> SupplyChainPOCSubsLeadsList { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Subs Backup Display Name
		/// </summary>
		public string SupplyChainPOCSubsBackupDisplayName { get; set; }

		/// <summary>
		/// Gets or sets Supply Chain POC Subs Backup NT id
		/// </summary>
		public string SupplyChainPOCSubsBackupNtId { get; set; }

		/// <summary>
		/// Gets or sets the list of Supply Chain POC Subs Backup Leads
		/// </summary>
		public List<SelectListItem> SupplyChainPOCSubsBackupLeadsList { get; set; }

		/// <summary>
		/// Gets or sets Contracts POC Display name
		/// </summary>
		public string ContractsPOCDisplayName { get; set; }

        /// <summary>
        /// Gets or sets Backup Contracts POC Display name
        /// </summary>
        public string BackupContractsPOCDisplayName { get; set; }

        /// <summary>
        /// Gets or sets Contracts POC Nt id
        /// </summary>
        public string ContractsPOCNtId { get; set; }

        /// <summary>
        /// Gets or sets Backup Contracts POC Nt id
        /// </summary>
        public string BackupContractsPOCNtId { get; set; }

        /// <summary>
        /// Gets or sets Backup pricer Display Name
        /// </summary>
        public string BackupPricerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets Backup pricer NT id
        /// </summary>
        public string BackupPricerNtId { get; set; }
       
        /// <summary>
        /// Gets or sets TechLead
        /// </summary>
        public string TechLeadNtid { get; set; }

        /// <summary>
        /// Gets or sets TechLead's account display name
        /// </summary>
        public string TechLeadDisplayName { get; set; }

        /// <summary>
        /// Gets or sets PropsalMgr
        /// </summary>
        public string ProposalMgrNtid { get; set; }

        /// <summary>
        /// Gets or sets ProposalMgr's account display name
        /// </summary>
        public string ProposalMgrDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the NTID of the GenBOE Workspace Creator
        /// </summary>
        public string GenBoeWorkspaceCreatorNtid { get; set; }

        /// <summary>
        /// Gets or sets the Display Name of the GenBOE Workspace Creator
        /// </summary>
        public string GenBoeWorkspaceCreatorDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the list of GenBOE Workspace Creators
        /// </summary>
        public ICollection<SelectListItem> GenBoeWorkspaceCreatorList { get; set; }

        /// <summary>
        /// Contract Leads
        /// </summary>
        public List<SelectListItem> ContractLeadList { get; set; }

        /// <summary>
        /// Backup Contracts Leads
        /// </summary>
        public List<SelectListItem> BackupContractLeadList { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalUserInformationModelView()
        {
        }
    }
}
