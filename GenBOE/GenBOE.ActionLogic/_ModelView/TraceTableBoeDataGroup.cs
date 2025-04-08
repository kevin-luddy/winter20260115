// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using MoreLinq;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Grouped Trace Table Data from genBOE 
	/// </summary>
	public class TraceTableBoeDataGroup
	{
		/// <summary>
		/// Converts TraceTableBoeData to TraceTableBoeDataGroup model
		/// </summary>
		/// <param name="boeData">TraceTableBoeData model</param>
		public TraceTableBoeDataGroup(TraceTableBoeData boeData)
		{
			if (boeData == null)
			{
				throw new ArgumentNullException(nameof(boeData));
			}
			SummaryField = boeData.SummaryField;
			SummaryFieldValue = boeData.SummaryFieldValue;
			TotalValue = boeData.TotalValue;
			SpreadPrecision = boeData.SpreadPrecision;

			foreach (KeyValuePair<int, decimal> boeDataKvp in boeData.SpreadValuesForYear)
			{
				SpreadValuesForGroup.Add(boeDataKvp.Key.ToString(), boeDataKvp.Value);
			}

			boeData.ChildData.ForEach(x => ChildData.Add(new TraceTableBoeDataGroup(x)));
		}

		/// <summary>
		/// Blank constructor
		/// </summary>
		public TraceTableBoeDataGroup() { }

		/// <summary>
		/// Summary field name
		/// </summary>
		public string SummaryField { get; set; }

		/// <summary>
		/// Summary Field Value
		/// </summary>
		public string SummaryFieldValue { get; set; }

		/// <summary>
		/// Spread value for the group field
		/// key - Group field
		/// value - spread value
		/// </summary>
		public IDictionary<string, decimal> SpreadValuesForGroup { get; set; } = new Dictionary<string, decimal>();

		/// <summary>
		/// Total value for the current and parent summary fields
		/// </summary>
		public decimal TotalValue { get; set; }

		/// <summary>
		/// Decimal precision for the spread values
		/// </summary>
		public int SpreadPrecision { get; set; }

		/// <summary>
		/// Child trace table BOE data
		/// </summary>
		public ICollection<TraceTableBoeDataGroup> ChildData { get; set; } = new Collection<TraceTableBoeDataGroup>();
	}
}