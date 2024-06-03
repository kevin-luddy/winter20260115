// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System.Collections.Generic;

	/// <summary>
	/// DTO For Math Results for the Confidence Report
	/// </summary>
	public class ConfidenceReportMathResultDTO
	{
		/// <summary>
		/// ctor
		/// </summary>
		public ConfidenceReportMathResultDTO()
		{
			Matches = new Dictionary<decimal, bool>();
		}

		/// <summary>
		/// Dictionary of the numbers to match and their match results
		/// </summary>
		public IDictionary<decimal, bool> Matches { get; set; }

	}
}
