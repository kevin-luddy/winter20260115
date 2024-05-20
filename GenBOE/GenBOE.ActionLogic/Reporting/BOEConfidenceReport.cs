// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text.RegularExpressions;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Enums;

	/// <summary>
	/// class containing logic for the BOE Confidence Report
	/// </summary>
	public class BOEConfidenceReport
	{
		/// <summary>
		/// ctor
		/// </summary>
		public BOEConfidenceReport()
		{

		}

		/// <summary>
		/// Algorithm to parse RTE Fields for PoP date ranges
		/// </summary>
		/// <param name="popDates">PoP date ranges</param>
		/// <param name="rteFields">RTE Fields to parse</param>
		/// <returns>Results containing match results for the input PoP Dates and any other dates found that have no PoP match</returns>
		internal ConfidenceReportPoPResultDTO GetPoPConfidenceResults(ICollection<DateRange> popDates, ref ICollection<string> rteFields)
		{
			ConfidenceReportPoPResultDTO result = new ConfidenceReportPoPResultDTO();

			// Normalize dates to the 15th to ensure they match the normalized parsed dates
			foreach (DateRange popDate in popDates)
			{
				popDate.EndDate = popDate.EndDate.Value.Normalize();
				popDate.StartDate = popDate.StartDate.Value.Normalize();
			}

			ICollection<DateRange> foundDateRanges = new Collection<DateRange>();
			ICollection<DateTime> foundDates = new Collection<DateTime>();
			ICollection<string> rteFieldsRemovedDates = new Collection<string>();

			// Regex strings that will be used multiple times in the full regex strings
			string monthRegex = @"(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?|0?[1-9]|1[0-2])";
			string monthYearSeparatorRegex = @"(\s|\/|-)";
			string yearRegex = @"\d{2,4}";

			// Regex strings to match date ranges and individual dates
			// For reference, the full regexes should look like the following:
			// dateRangeRegex:(?!(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?|0?[1-9]|1[0-2])(\s|\/|-))\b(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?|0?[1-9]|1[0-2])\s?(\s|\/|-)\s?(\d{2,4})?\s?(-|to|through|thru)\s?(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?|0?[1-9]|1[0-2])\s?(\s|\/|-)\s?(\d{2,4}(?!(\s|\/|-)(\d{2,4})))
			// singleDateRegex:(?<!(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?|0?[1-9]|1[0-2])(\s|\/|-))\b(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?|0?[1-9]|1[0-2])\s?(\s|\/|-)\s?(\d{2,4}(?!(\s|\/|-)(\d{2,4})))
			string dateRangeRegex = $@"(?<!{monthRegex}{monthYearSeparatorRegex})\b{monthRegex}\s?{monthYearSeparatorRegex}\s?({yearRegex})?\s?(-|to|through|thru)\s?{monthRegex}\s?{monthYearSeparatorRegex}\s?({yearRegex}(?!{monthYearSeparatorRegex}({yearRegex})))";
			string singleDateRegex = $@"(?<!{monthRegex}{monthYearSeparatorRegex})\b{monthRegex}\s?{monthYearSeparatorRegex}\s?({yearRegex}(?!{monthYearSeparatorRegex}({yearRegex})))";
			
			// pull out date ranges and individual dates found in RTE fields
			foreach (string rteField in rteFields)
			{
				// match and remove the date ranges from the strings
				string rteFieldRemovedDates = rteField;
				Match match = Regex.Match(rteField, dateRangeRegex, RegexOptions.IgnoreCase, Constants.REGEX_SHORT_TIMEOUT);

				while (match.Success)
				{
					string dateRange = match.Value;
					foundDateRanges.Add(new DateRange(dateRange));
					match = match.NextMatch();

					int dateRangeIndex = rteFieldRemovedDates.IndexOf(dateRange);
					if (dateRangeIndex >= 0)
					{
						rteFieldRemovedDates = rteFieldRemovedDates.Remove(dateRangeIndex, dateRange.Length);
					}
				}

				// match and remove the individual dates from the strings
				match = Regex.Match(rteFieldRemovedDates, singleDateRegex, RegexOptions.IgnoreCase, Constants.REGEX_SHORT_TIMEOUT);

				while (match.Success)
				{
					string date = match.Value;

					if (date.TryParseMonthYear(out DateTime parsedStartDate))
					{
						foundDates.Add(parsedStartDate);
					}

					match = match.NextMatch();

					int dateIndex = rteFieldRemovedDates.IndexOf(date);
					if (dateIndex >= 0)
					{
						rteFieldRemovedDates = rteFieldRemovedDates.Remove(dateIndex, date.Length);
					}
				}

				rteFieldsRemovedDates.Add(rteFieldRemovedDates);
			}

			// create list of all dates for the "No Match" list to be removed from once dates are found
			ICollection<DateTime> noMatchDates = new Collection<DateTime>();
			noMatchDates.AddRange(foundDateRanges.Select(x => x.StartDate.Value));
			noMatchDates.AddRange(foundDateRanges.Select(x => x.EndDate.Value));
			noMatchDates.AddRange(foundDates);
			noMatchDates = noMatchDates.Distinct().ToCollection();

			// compare each pop date range to the found dates and date ranges
			foreach (DateRange dateRange in popDates)
			{
				// check found date ranges for full matches
				if (foundDateRanges.Any(x => x.Equals(dateRange)))
				{
					result.PoPDateResults.Add(dateRange, PoPMatchResult.Match);
					noMatchDates.Remove(dateRange.StartDate.Value);
					noMatchDates.Remove(dateRange.EndDate.Value);
				}
				// If PoP start and end dates are the same, a single date will be a match
				else if (dateRange.StartAndEndSameDate() && foundDates.Any(x => dateRange.HasDateAsStartOrEndDate(x)))
				{
					result.PoPDateResults.Add(dateRange, PoPMatchResult.Match);
					noMatchDates.Remove(dateRange.StartDate.Value);
				}
				// check found date ranges for partial matches
				else if (foundDateRanges.Any(x => dateRange.HasDateAsStartOrEndDate(x.StartDate.Value)))
				{
					result.PoPDateResults.Add(dateRange, PoPMatchResult.Partial);
					noMatchDates.Remove(dateRange.StartDate.Value);

					// If both start and end dates are in separate date ranges, it's still partial, but remove both from No Match dates
					if (foundDateRanges.Any(x => dateRange.HasDateAsStartOrEndDate(x.EndDate.Value)))
					{
						noMatchDates.Remove(dateRange.EndDate.Value);
					}
				}
				else if (foundDateRanges.Any(x => dateRange.HasDateAsStartOrEndDate(x.EndDate.Value)))
				{
					result.PoPDateResults.Add(dateRange, PoPMatchResult.Partial);
					noMatchDates.Remove(dateRange.EndDate.Value);
				}
				// check individual dates for partial matches
				else if (foundDates.Any(x => dateRange.HasDateAsStartOrEndDate(x)))
				{
					result.PoPDateResults.Add(dateRange, PoPMatchResult.Partial);

					// Different found dates could match both start and end (still a partial match in that case),
					// so we want to ensure both are removed from No Match dates
					foreach (DateTime foundDate in foundDates.Where(x => dateRange.HasDateAsStartOrEndDate(x)))
					{
						noMatchDates.Remove(foundDate);
					}
				}
				// dates not found
				else
				{
					result.PoPDateResults.Add(dateRange, PoPMatchResult.Missing);
				}
			}

			result.NoMatchDates = noMatchDates;

			// set rteFields to the collection with dates removed so it can be passed to the Math algorithm
			rteFields = rteFieldsRemovedDates;

			return result;
		}
	}
}
