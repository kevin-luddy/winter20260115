// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using GenBOE.DataBridge.DTO;
    using MoreLinq;
    using System;
    using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Trace Table Data from genBOE
	/// </summary>
	public class TraceTableBoeData
	{
        /// <summary>
        /// Converts TraceTableBoeDataGroup to TraceTableBoeData model
        /// </summary>
        /// <param name="boeDataGroup">TraceTableBoeData model</param>
        public TraceTableBoeData(TraceTableBoeDataGroup boeDataGroup)
        {
            if (boeDataGroup == null)
            {
                throw new ArgumentNullException(nameof(boeDataGroup));
            }
            SummaryField = boeDataGroup.SummaryField;
            SummaryFieldValue = boeDataGroup.SummaryFieldValue;
            TotalValue = boeDataGroup.TotalValue;
            SpreadPrecision = boeDataGroup.SpreadPrecision;

			if (boeDataGroup.SpreadValuesForGroup != null)
			{
                foreach (KeyValuePair<string, decimal> boeDataKvp in boeDataGroup.SpreadValuesForGroup)
                {
                    int year = 0;
                    int.TryParse(boeDataKvp.Key, out year);
                    SpreadValuesForYear.Add(year, boeDataKvp.Value);
                }
            }

            boeDataGroup.ChildData.ForEach(x => ChildData.Add(new TraceTableBoeData(x)));
        }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public TraceTableBoeData() { }

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
		/// Decimal precision for the spread values
		/// </summary>
		public int SpreadPrecision { get; set; }

		/// <summary>
		/// Child trace table BOE data
		/// </summary>
		public ICollection<TraceTableBoeData> ChildData { get; set; } = new Collection<TraceTableBoeData>();
	}
}