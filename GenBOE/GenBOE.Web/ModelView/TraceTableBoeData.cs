// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using System.Collections.Generic;

	/// <summary>
	/// Trace Table Data from genBOE
	/// </summary>
	public class TraceTableBoeData
	{
		/// <summary>
		/// Summary field name
		/// </summary>
		string SummaryField { get; set; }

		/// <summary>
		/// Summary Field Value
		/// </summary>
		string SummaryFieldValue { get; set; }

		/// <summary>
		/// Spread value for the year
		/// key - year
		/// value - spread value
		/// </summary>
		IDictionary<int, decimal> SpreadValuesForYear { get; set; }

		/// <summary>
		/// Total vaue for the current and parent summary fields
		/// </summary>
		decimal TotalValue { get; set; }

		/// <summary>
		/// Child trace table BOE data
		/// </summary>
		ICollection<TraceTableBoeData> traceTableBoeData { get; set; }
	}
}