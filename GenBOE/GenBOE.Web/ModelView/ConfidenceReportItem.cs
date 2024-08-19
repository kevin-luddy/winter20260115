// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using System;

	/// <summary>
	/// Class containing data for a row in the Confidence Report
	/// </summary>
	public class ConfidenceReportItem
	{
		/// <summary>
		/// ctor
		/// </summary>
		public ConfidenceReportItem()
		{			
		}

		/// <summary>
		/// ctor using ActionLogic model view
		/// </summary>
		/// <param name="item"></param>
		public ConfidenceReportItem(GenBOE.ActionLogic.ModelView.ConfidenceReportItem item)
		{
			_ = item ?? throw new ArgumentNullException(nameof(item));

			BoeId = item.BoeId;
			BoeTitle = item.BoeTitle;
			TaskId = item.TaskId;
			TaskTitle = item.TaskTitle;
			MoqTypes = item.MoqTypesString;
			RteFields = item.RteFields;
			HasPoPError = item.HasPoPError;
			HasMoqError = item.HasMoqError;
			HasHistoricalRefError = item.HasHistoricalRefError;
			ErrorText = item.ErrorText;
			WbsNumber = item.WbsNumber;
			WbsTitle = item.WbsTitle;
		}

		/// <summary>
		/// BOE ID
		/// </summary>
		public int BoeId { get; set; }

		/// <summary>
		/// BOE Title
		/// </summary>
		public string BoeTitle { get; set; }

		/// <summary>
		/// BOE Task Element ID
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// BOE Task Element Title
		/// </summary>
		public string TaskTitle { get; set; }

		/// <summary>
		/// MOQ Types
		/// </summary>
		public string MoqTypes { get; set; }

		/// <summary>
		/// Count of RTE fields in the task
		/// </summary>
		public int RteFields { get; set; }

		/// <summary>
		/// Does this Task have at least 1 PoP Error?
		/// </summary>
		public bool HasPoPError { get; set; }

		/// <summary>
		/// Does this Task have at least 1 MOQ Equation Error?
		/// </summary>
		public bool HasMoqError { get; set; }

		/// <summary>
		/// Does this Task have at least 1 Historical Reference Error?
		/// </summary>
		public bool HasHistoricalRefError { get; set; }

		/// <summary>
		/// Error type text to display
		/// </summary>
		public string ErrorText { get; set; }

		/// <summary>
		/// WBS Number
		/// </summary>
		public string WbsNumber { get; set; }

		/// <summary>
		/// WBS Title
		/// </summary>
		public string WbsTitle { get; set; }
	}
}