// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.Dtos;
	using IES.Common;

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
			MoqTypes = new Collection<MOQType>();
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
		/// MOQ Types used in the Task
		/// </summary>
		public ICollection<MOQType> MoqTypes { get; set; }

		/// <summary>
		/// MOQ Types as a comma separated list for display in the report
		/// </summary>
		public string MoqTypesString => string.Join(", ", MoqTypes.Select(x => x.GetDescription()));

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

		/// <summary>
		/// WBS ID
		/// </summary>
		public int WbsId { get; set; }

		/// <summary>
		/// WBS Title
		/// </summary>
		public string WbsTitle { get; set; }
	}
}