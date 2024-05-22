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
			IList<DateTime> foundDates = new List<DateTime>();
			ICollection<string> rteFieldsRemovedDates = new Collection<string>();
			
			// Track the indexes and which RTE each single date is in
			IDictionary<int, int> dateRteIndex = new Dictionary<int, int>();
			int rteIndex = 0;
			int dateIndex = 0;

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
						dateRteIndex.Add(dateIndex++, rteIndex);
					}

					match = match.NextMatch();

					int removeDateIndex = rteFieldRemovedDates.IndexOf(date);
					if (removeDateIndex >= 0)
					{
						rteFieldRemovedDates = rteFieldRemovedDates.Remove(removeDateIndex, date.Length);
					}
				}

				rteFieldsRemovedDates.Add(rteFieldRemovedDates);
				rteIndex++;
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
					// Get the indexes of the start and end dates
					ICollection<int> startDateIndexes = foundDates.AllIndexesOf<DateTime>(dateRange.StartDate.Value);
					ICollection<int> endDateIndexes = foundDates.AllIndexesOf<DateTime>(dateRange.EndDate.Value);

					// Check if any indexes of end dates are 1 after any indexes of start dates and in the same RTE
					if (endDateIndexes.Any(x => startDateIndexes.Contains(x - 1)))
					{
						// If in the same RTE, it's a full match, otherwise partial
						int endDateIndex = endDateIndexes.FirstOrDefault(x => startDateIndexes.Contains(x - 1));

						if (dateRteIndex.TryGetValue(endDateIndex - 1, out int startDateRteIndex) && dateRteIndex.TryGetValue(endDateIndex, out int endDateRteIndex)
							&& startDateRteIndex == endDateRteIndex)
						{
							result.PoPDateResults.Add(dateRange, PoPMatchResult.Match);
						}
						else
						{
							result.PoPDateResults.Add(dateRange, PoPMatchResult.Partial);
						}
					}
					else
					{
						// Otherwise only partial
						result.PoPDateResults.Add(dateRange, PoPMatchResult.Partial);
					}

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

		/// <summary>
		/// Helper: Replaces variables in the string with their corresponding numerical value
		/// </summary>
		/// <param name="input">String to modify</param>
		/// <param name="replacements">Variables and their corresponding value</param>
		/// <returns>The original string modified to replace variables with their corresponding numeric value</returns>
		internal string ReplaceVariablesWithValues(string input, Dictionary<string, decimal> replacements)
		{
			foreach (KeyValuePair<string, decimal> item in replacements)
			{
				if (!input.Contains(item.Key))
				{
					throw new ArgumentException($"The key '{item.Key}' does not have a match in the input string.");
				}
				input = input.Replace(item.Key, item.Value.ToString());
			}
			return input;
		}

		/// <summary>
		/// Helper: Takes a collection of arbitrary strings and outputs a collection of decimals
		/// </summary>
		/// <param name="stringsContainingDecimals">Strings that contain decimals</param>
		/// <returns>A collection of decimals extracted from the string</returns>
		internal ICollection<decimal> ExtractDecimals(IEnumerable<string> stringsContainingDecimals)
		{
			ICollection<decimal> result = new Collection<decimal>();
			Regex decimalRegex = new Regex(@"\d+(\.\d+)?");
			foreach (string str in stringsContainingDecimals)
			{
				MatchCollection matches = decimalRegex.Matches(str);
				foreach (Match match in matches)
				{
					if (decimal.TryParse(match.Value, out decimal decimalValue))
					{
						result.Add(decimalValue);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Algorithm to parse RTE Fields for numbers to match against
		/// </summary>
		/// <param name="numbersToMatch">All numbers to look for matches</param>
		/// <param name="rteFields">RTE Fields to parse</param>
		/// <returns>Results containing match results for the input numbers and any other numbers that have no match</returns>
		internal ConfidenceReportMathResultDTO GetMathConfidenceResults(ICollection<string> numbersToMatch, ref ICollection<string> rteFields)
		{
			// To Do: replace MOQ equation variables with values HERE using the following helper
			// input = ReplaceVariablesWithValues(input, replacements);

			numbersToMatch = numbersToMatch.Select(str => str.Replace(",", "")).ToList();
			rteFields = rteFields.Select(str => str.Replace(",", "")).ToList();

			ICollection<decimal> numbersToMatchDecimals = ExtractDecimals(numbersToMatch);
			ICollection<decimal> rteFieldsDecimals = ExtractDecimals(rteFields);

			ConfidenceReportMathResultDTO result = new ConfidenceReportMathResultDTO();
			HashSet<decimal> rteFieldsDecimalsSet = new HashSet<decimal>(rteFieldsDecimals);

			foreach(decimal number in numbersToMatchDecimals)
			{
				result.Matches[number] = rteFieldsDecimalsSet.Contains(number);
			}

			return result;
		}
	}
}
