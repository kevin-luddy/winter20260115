// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Enums
{
	/// <summary>
	/// Results of parsing RTE Fields for Task/MOQ Table Period of Performance ranges for the BOE Confidence Report
	/// </summary>
	public enum PoPMatchResult
	{
		/// <summary>
		/// Both the Start Date and End Date match text in the RTE field
		/// </summary>
		Match = 0,

		/// <summary>
		/// Only one of the Start Date or End Date match text in the RTE field
		/// </summary>
		Partial = 1,

		/// <summary>
		/// Neither Start Date nor End Date match match in the RTE field
		/// </summary>
		Missing = 2
	}
}
