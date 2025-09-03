namespace GenBOE.Dtos
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel.DataAnnotations;

	/// <summary>
	/// BOEStatusReportGrid used for genBOE Angular
	/// </summary>
	public class BOEStatusReportGrid
	{
		/// <summary>
		/// Approvers
		/// </summary>
		public Collection<string> Approvers { get; set; }

		/// <summary>
		/// Authors
		/// </summary>
		public Collection<string> Authors { get; set; }

		/// <summary>
		/// BOEID
		/// </summary>
		public int BOEID { get; set; }

		/// <summary>
		/// BOETitle
		/// </summary>
		public string BOETitle { get; set; }

		/// <summary>
		/// CLIN Number
		/// </summary>
		public string CLINNumber { get; set; }

		/// <summary>
		/// CLIN Padded Number
		/// </summary>
		public string CLINPaddedNumber { get; set; }

		/// <summary>
		/// CLIN Title
		/// </summary>
		public string CLINTitle { get; set; }

		/// <summary>
		/// Decimal Precision String Format
		/// </summary>
		public string DecimalPrecisionStringFormat { get; set; }

		/// <summary>
		/// End Date
		/// </summary>
		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime EndDate { get; set; }

		/// <summary>
		/// Is Material
		/// </summary>
		public Boolean IsMaterial { get; set; }

		/// <summary>
		/// Is Multi CLIN WBS
		/// </summary>
		public Boolean IsMultiClinWbs { get; set; }

		/// <summary>
		/// Is UCOT Enabled for Workspace
		/// </summary>
		public bool IsUCOTEnabledForWorkspace { get; set; }

		/// <summary>
		/// Number of decimal places for resources.
		/// </summary>
		public int ResourceDecimalPrecision { get; set; }

		/// <summary>
		/// Start Date
		/// </summary>
		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Status
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// Total Cost
		/// </summary>
		public decimal TotalCost { get; set; }

		/// <summary>
		/// Total Hours
		/// </summary>
		public decimal TotalHours { get; set; }

		/// <summary>
		/// Gets Total Hours in the correct format as defined for the workspace.
		/// </summary>
		public string TotalHoursFormatted { get; set; }

		/// <summary>
		/// Total Hours with UCOT (TotalHours + TotalUCOTHours)
		/// </summary>
		public decimal TotalHoursWithUCOT { get; set; }

		/// <summary>
		/// Gets Total Hours with UCOT Hours in the correct format as defined for the workspace.
		/// </summary>
		public string TotalHoursWithUCOTFormatted { get; set; }

		/// <summary>
		/// Total UCOT Hours 
		/// </summary>
		public decimal TotalUCOTHours { get; set; }

		/// <summary>
		/// Gets Total UCOT Hours in the correct format as defined for the workspace.
		/// </summary>
		public string TotalUCOTHoursFormatted { get; set; }

		/// <summary>
		/// WBS Number
		/// </summary>
		public string WBSNumber { get; set; }

		/// <summary>
		/// WBS Padded Number
		/// </summary>
		public string WBSPaddedNumber { get; set; }

		/// <summary>
		/// WBS Title
		/// </summary>
		public string WBSTitle { get; set; }

		/// <summary>
		/// WorkspaceID
		/// </summary>
		public int WorkspaceID { get; set; }
	}
}