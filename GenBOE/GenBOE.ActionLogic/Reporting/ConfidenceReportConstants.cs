// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
	/// <summary>
	/// Constants for the BOE Confidence Report
	/// </summary>
	public static class ConfidenceReportConstants
	{
		/// <summary>
		/// Error message for a PoP error
		/// </summary>
		public const string POP_ERROR = "PoP Error";

		/// <summary>
		/// Error message for a MOQ Equation error
		/// </summary>
		public const string MOQ_ERROR = "MOQ Equation Error";

		/// <summary>
		/// Error message for a historical reference error
		/// </summary>
		public const string HISTORICAL_REF_ERROR = "Relevant Hours Error";

		/// <summary>
		/// Message for when a task has no errors
		/// </summary>
		public const string NO_ERRORS = "No Errors";
	}
}
