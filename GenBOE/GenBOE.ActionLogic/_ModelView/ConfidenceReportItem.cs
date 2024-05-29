// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using GenBOE.Dtos;

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
		/// Count of RTE fields in the task
		/// </summary>
		public int rteFields { get; set; }

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
		/// POP Algorithm Result DTO for PoPs
		/// </summary>
		public ConfidenceReportPoPResultDTO PoPResults { get; set; }

		/// <summary>
		/// Math Algorithm Result DTO for MOQ Equation
		/// </summary>
		public ConfidenceReportMathResultDTO MoqResults { get; set; }

		/// <summary>
		/// Math Algorithm Result DTO for Historical References
		/// </summary>
		public ConfidenceReportMathResultDTO HistoricalRefResults { get; set; }
	}
}