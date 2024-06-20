// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.ActionLogic.Common;

	/// <summary>
	/// View Model for the Confidence Report
	/// </summary>
	public class ConfidenceReportModelView : GenBOEMasterModelView
	{
		/// <summary>
		/// ctor
		/// </summary>
		public ConfidenceReportModelView()
		{
			ConfidenceReportData = new Collection<ConfidenceReportItem>();
		}

		/// <summary>
		/// ctor using ActionLogic model view
		/// </summary>
		/// <param name="mv"></param>
		public ConfidenceReportModelView(GenBOE.ActionLogic.ModelView.ConfidenceReportModelView mv, string proposalName, string workspaceState, bool isOci)
		{
			_ = mv ?? throw new ArgumentNullException(nameof(mv));

			TotalTaskCount = mv.TotalTaskCount;
			TasksWithoutErrors = mv.TasksWithoutErrors;
			ConfidenceReportData = new Collection<ConfidenceReportItem>();

			foreach (ActionLogic.ModelView.ConfidenceReportItem item in mv.ConfidenceReportData)
			{
				ConfidenceReportData.Add(new ConfidenceReportItem(item));
			}

			ProposalName = proposalName;
			WorkspaceState = workspaceState;
			HeaderFooter = isOci ? WebConstants.LMPI_OCI_LABEL_TEXT : WebConstants.LMPI_LABEL_TEXT;
		}

		/// <summary>
		/// Total number of tasks in the report
		/// </summary>
		public int TotalTaskCount { get; set; }

		/// <summary>
		/// Total number of tasks with no errors in the report
		/// </summary>
		public int TasksWithoutErrors { get; set; }

		/// <summary>
		/// Confidence Score for the report
		/// </summary>
		public string ConfidenceScore => $"{TasksWithoutErrors}/{TotalTaskCount}";

		/// <summary>
		/// Collection of data for the report
		/// </summary>
		public ICollection<ConfidenceReportItem> ConfidenceReportData { get; set; }
	}
}