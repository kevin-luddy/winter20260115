// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using IES.Common.classes;
	using IES.Common.Enums;

	/// <summary>
	/// DTO For PoP Results for the Confidence Report
	/// </summary>
	public class ConfidenceReportPoPResultDTO
	{
		/// <summary>
		/// ctor
		/// </summary>
		public ConfidenceReportPoPResultDTO()
		{
			PoPDateResults = new Dictionary<DateRange, PoPMatchResult>();
			NoMatchDates = new Collection<DateTime>();
		}

		/// <summary>
		/// Dictionary of the Task/MOQ Table PoP Date Ranges and their match results
		/// </summary>
		public IDictionary<DateRange, PoPMatchResult> PoPDateResults { get; set; }

		/// <summary>
		/// Dates found in the RTE Fields that do not match any PoP Dates
		/// </summary>
		public ICollection<DateTime> NoMatchDates { get; set; }
	}
}
