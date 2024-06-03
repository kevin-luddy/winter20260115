// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.Objects;

	/// <summary>
	/// Interface for BOEConfidenceReport
	/// </summary>
	public interface IBOEConfidenceReport
	{
		/// <summary>
		/// Generate the Confidence Report
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeId">BOE ID if running for a specific BOE, null if running for all BOEs</param>
		/// <returns>Confidence Report View Model</returns>
		ConfidenceReportModelView GenerateConfidenceReport(FullWorkspace workspace, int? boeId = null);
	}
}