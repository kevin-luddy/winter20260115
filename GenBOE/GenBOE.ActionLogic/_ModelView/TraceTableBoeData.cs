// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Trace Table Data from genBOE
	/// </summary>
	public class TraceTableBoeData
	{
		/// <summary>
		/// Summary field name
		/// </summary>
		public string SummaryField { get; set; }

		/// <summary>
		/// Summary Field Value
		/// </summary>
		public string SummaryFieldValue { get; set; }

		/// <summary>
		/// Spread value for the year
		/// key - year
		/// value - spread value
		/// </summary>
		public IDictionary<int, decimal> SpreadValuesForYear { get; set; } = new Dictionary<int, decimal>();

		/// <summary>
		/// Total value for the current and parent summary fields
		/// </summary>
		public decimal TotalValue { get; set; }

		/// <summary>
		/// Child trace table BOE data
		/// </summary>
		public ICollection<TraceTableBoeData> ChildData { get; set; } = new Collection<TraceTableBoeData>();
	}
}