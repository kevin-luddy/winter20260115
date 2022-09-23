// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace GenBOE.ActionLogic.ModelView.Workspace
{

	/// <summary>
	/// Workspace Calculate Actuals Model View
	/// </summary>
	public class WorkspaceCalculateActualsModelView
	{
		/// <summary>
		/// The Boe Title
		/// </summary>
		public string BoeTitle { get; set; }

		/// <summary>
		/// The Task (title)
		/// </summary>
		public string Task { get; set; }

		/// <summary>
		/// The affected Table Name
		/// </summary>
		public string TableName { get; set; }

		/// <summary>
		/// The previous BOE State
		/// </summary>
		public string BoeStatePrevious { get; set; }

		/// <summary>
		/// The previous Total Relevant Hours
		/// </summary>
		public decimal TotalRelevantHoursPrevious { get; set; }

		/// <summary>
		/// The new Total Relevant Hours
		/// </summary>
		public decimal TotalRelevantHours { get; set; }

		/// <summary>
		/// The original sort order (boe, task, then table order)
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// The Previous Wbs Hours (RMS Only)
		/// </summary>
		public decimal? WbsHoursPrevious { get; set; }

		/// <summary>
		/// The Current Wbs Hours (RMS Only)
		/// </summary>
		public decimal? WbsHours { get; set; }

		/// <summary>
		/// Whether the call was successful
		/// </summary>
		public bool IsSuccessful { get; set; }

		/// <summary>
		/// Validation messages
		/// </summary>
		public ICollection<string> Messages { get; set; } = new List<string>();
	}
}
