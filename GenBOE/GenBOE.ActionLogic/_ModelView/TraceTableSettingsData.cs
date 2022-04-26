// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System.Collections.Generic;

	/// <summary>
	/// Trace Table Settings Data
	/// </summary>
	public class TraceTableSettingsData
	{
		/// <summary>
		/// Collection of ints representing the element of cost enum
		/// </summary>
		public ICollection<int> ElementsOfCost { get; set; } = new List<int>();

		/// <summary>
		/// Resource Rate Type
		/// </summary>
		public int RateType { get; set; }

		/// <summary>
		/// bool for showing years
		/// </summary>
		public bool ShowYears { get; set; }

		/// <summary>
		/// Collection of Summary Field Names
		/// </summary>
		public ICollection<string> SummaryFields { get; set; }
	}
}