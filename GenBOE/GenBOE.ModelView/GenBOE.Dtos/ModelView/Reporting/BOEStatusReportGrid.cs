namespace GenBOE.Dtos
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel.DataAnnotations;
	using IES.Common;

	public class BOEStatusReportGrid
	{
		public Collection<string> Approvers { get; set; }

		public Collection<string> Authors { get; set; }

		public int BOEID { get; set; }

		public string BOETitle { get; set; }

		public string CLINNumber { get; set; }

		public string CLINPaddedNumber { get; set; }

		public string CLINTitle { get; set; }

		public string DecimalPrecisionStringFormat { get; set; }

		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime EndDate { get; set; }

		public Boolean IsMaterial { get; set; }

		public Boolean IsMultiClinWbs { get; set; }

		/// <summary>
		/// Is UCOT Enabled for Workspace
		/// </summary>
		public bool IsUCOTEnabledForWorkspace { get; set; }

		/// <summary>
		/// Number of decimal places for resources.
		/// </summary>
		public int ResourceDecimalPrecision { get; set; }

		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime StartDate { get; set; }

		public string Status { get; set; }

		public decimal TotalCost { get; set; }

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

		public string WBSNumber { get; set; }

		public string WBSPaddedNumber { get; set; }

		public string WBSTitle { get; set; }

		public int WorkspaceID { get; set; }
	}
}