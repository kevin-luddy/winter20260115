namespace GenBOE.ActionLogic.ModelView
{
	using System.Collections.Generic;

	/// <summary>
	/// Response data from PTM Refresh for Angular GenBOE
	/// </summary>
	public class RefreshPTMResponseData
	{
		/// <summary>
		/// Tracking Number Revision
		/// </summary>
		public string TrackingNumberRevision {  get; set; }

		/// <summary>
		/// LOB Id
		/// </summary>
		public int LOBId { get; set; }

		/// <summary>
		/// RFP Number
		/// </summary>
		public string RFPNumber { get; set; }

		/// <summary>
		/// Contract Types
		/// </summary>
		public ICollection<int> ContractTypes { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Proposal Class Id
		/// </summary>
		public int ProposalClassId { get; set; }

		/// <summary>
		/// Anticipated Delivery Date
		/// </summary>
		public string AnticipatedDeliveryDate { get; set; }

		/// <summary>
		/// Revised Submittal Date
		/// </summary>
		public string RevisedSubmittalDate { get; set; }

		/// <summary>
		/// Using Template BOE
		/// </summary>
		public bool UsingTemplateBoe { get; set; }

		/// <summary>
		/// Is SAP Enabled
		/// </summary>
		public bool IsSAPEnabledConfig { get; set; }
	}
}