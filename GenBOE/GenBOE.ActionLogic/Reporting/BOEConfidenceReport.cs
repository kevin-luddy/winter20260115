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
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Enums;

	/// <summary>
	/// class containing logic for the BOE Confidence Report
	/// </summary>
	public class BOEConfidenceReport : IBOEConfidenceReport
	{
		/// <summary>
		/// BOE Loader
		/// </summary>
		private IBoeDTODataLoader boeLoader;

		/// <summary>
		/// MOQ Type Loader
		/// </summary>
		private IMoqTypeDataLoader moqTypeLoader;

		/// <summary>
		/// RTE Template Loader
		/// </summary>
		private IRteTemplateDataLoader rteTemplateLoader;

		/// <summary>
		/// ctor
		/// </summary>
		public BOEConfidenceReport(IBoeDTODataLoader boeLoader, IMoqTypeDataLoader moqTypeLoader, IRteTemplateDataLoader rteTemplateLoader)
		{
			this.boeLoader = boeLoader;
			this.moqTypeLoader = moqTypeLoader;
			this.rteTemplateLoader = rteTemplateLoader;
		}

		/// <summary>
		/// Generate the Confidence Report
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="boeId">BOE ID if running for a specific BOE, null if running for all BOEs</param>
		/// <returns>Confidence Report View Model</returns>
		public ConfidenceReportModelView GenerateConfidenceReport(FullWorkspace workspace, int? boeId = null)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));

			ConfidenceReportModelView confidencereportModelView = new ConfidenceReportModelView();

			// Get BOE if ID provided, otherwise all BOEs in the Workspace
			ICollection<FullBoe> boes = new Collection<FullBoe>();

			if (boeId != null)
			{
				BoeDTO boe = boeLoader.GetById(boeId.Value);
				boes.Add(new FullBoe(boe));
			}
			else
			{
				boes = workspace.Boes.ToCollection();
			}

			// Get the workspace variables so we don't need to repeat for every task
			Dictionary<string, decimal> wsVariableReplacements = new Dictionary<string, decimal>();
			foreach (WorkspaceVariableDTO wsVariable in workspace.WorkspaceVariables)
			{
				wsVariableReplacements.Add(wsVariable.WorkspaceVariableName, wsVariable.WorkspaceVariableValue);
			}

			// Get the RTE Template values for the Workspace if the workspace is using them
			ICollection<RTECustomTemplateAnswerModelView> rteTemplateValues = new Collection<RTECustomTemplateAnswerModelView>();
			if (workspace.RteOverrides.Any())
			{
				rteTemplateValues = rteTemplateLoader.GetAnswersByWorkspaceId(workspace.Id);
			}

			// Loop though each task in each boe to generate the report
			foreach (FullBoe boe in boes)
			{
				ICollection<MoqTypeSelection> moqTypes = workspace.UsingTemplateBOE ? moqTypeLoader.GetByBoeId(boe.Id) : null;
				boe.LoadTaskElementRTEData();

				foreach (BoeTaskElementDTO task in boe.TaskElements)
				{
					ConfidenceReportItem reportItem = new ConfidenceReportItem()
					{
						BoeId = boe.Id,
						BoeTitle = boe.Title,
						TaskId = task.Id,
						TaskTitle = task.TaskTitle,
						WbsNumber = boe.Wbs.WbsNumber,
						WbsTitle = boe.Wbs.WbsTitle
					};

					// get rte fields and the PoPs and Numbers to check them for
					ICollection<string> rteFields = new Collection<string>
					{
						task.Description
					};

					// Get the Task Description RTE Template answers if they're being used
					if (workspace.RteOverrides.Any(x => x == RteTemplateSource.TaskDescription || x == RteTemplateSource.TaskMOQ))
					{
						rteFields.AddRange(rteTemplateValues.Where(x => x.BoeId == boe.Id && x.TaskId == task.Id 
								&& (x.Source == RteTemplateSource.TaskDescription || x.Source == RteTemplateSource.TaskMOQ))
							.Select(x => x.AnswerText));
					}

					ICollection<DateRange> popDates = new Collection<DateRange>()
					{
						new DateRange(task.StartDate, task.EndDate)
					};

					// replace variables in the MOQ Equation
					string moqEquation = ReplaceVariablesWithValues(task, wsVariableReplacements);

					// Get the MOQ numeric values - the equation and the result
					ICollection<string> moqNumericValues = new Collection<string>()
					{
						moqEquation,
						task.TotalHours.ToString()
					};

					ICollection<string> historicalRefNumericValues = new Collection<string>();

					if (workspace.UsingTemplateBOE)
					{
						// Get RTE fields and Historical Reference numeric values from MOQ Types
						foreach (MoqTypeSelection moqType in moqTypes.Where(x => x.TaskId == task.Id).OrderBy(x => x.SelectedMOQType))
						{
							reportItem.MoqTypes.Add(moqType.SelectedMOQType);
							switch (moqType.SelectedMOQType)
							{
								case MOQType.Historical:
								case MOQType.Comparative:
									rteFields.Add(moqType.Rationale);
									rteFields.Add(moqType.SkillMixRationale);

									if (Utilities.IsHistoricalReferenceExplanationRequired(workspace.CreationDate))
									{
										rteFields.Add(moqType.HistoricalReferenceExplanation);
									}

									foreach (MoqTableData table in moqType.TableData)
									{
										if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
										{
											historicalRefNumericValues.Add(table.TotalWbsHours.ToString());
										}

										historicalRefNumericValues.Add(table.TotalRelevantHours.ToString());
									}

									break;
								case MOQType.CostEstimatingRelationships:
								case MOQType.ParametricEstimates:
								case MOQType.AnalogousRelationships:
									rteFields.Add(moqType.Rationale);
									rteFields.Add(moqType.SkillMixRationale);
									break;
								case MOQType.SOW:
								case MOQType.LOE:
									rteFields.Add(moqType.DescriptionHoursRequired);
									rteFields.Add(moqType.Rationale);
									rteFields.Add(moqType.SkillMixRationale);
									break;
								case MOQType.SME:
									rteFields.Add(moqType.SmeReason);
									rteFields.Add(moqType.SmeHoursLogic);
									rteFields.Add(moqType.SmeDurationLogic);
									rteFields.Add(moqType.SmeTaskEstimates);
									rteFields.Add(moqType.SkillMixRationale);
									break;
								case MOQType.NonLabor:
									rteFields.Add(moqType.Rationale);
									break;
								default:
									break;
							}
						}
					}
					else
					{
						// if not using Template BOE, just add MOQText to the RTE fields
						rteFields.Add(task.MOQText);
						reportItem.MoqTypes.Add(task.MOQType);
					}

					// process and populate data
					rteFields = rteFields.Where(x => x != null).ToCollection();
					reportItem.RteFields = rteFields.Count;
					reportItem.PoPResults = GetPoPConfidenceResults(popDates, ref rteFields);
					reportItem.MoqResults = GetMathConfidenceResults(moqNumericValues, ref rteFields);
					reportItem.HistoricalRefResults = GetMathConfidenceResults(historicalRefNumericValues, ref rteFields);

					PopulateErrorText(reportItem);
					UpdateConfidenceValuesForTask(reportItem, historicalRefNumericValues.Any(), confidencereportModelView);

					// Add the data to the report if there are errors
					if (reportItem.HasPoPError || reportItem.HasMoqError || reportItem.HasHistoricalRefError)
					{
						confidencereportModelView.ConfidenceReportData.Add(reportItem);
					}
				}
			}

			return confidencereportModelView;
		}

		#region PoP methods

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
			string yearRegex = @"(\d{4}|\d{2})";

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

		#endregion PoP methods

		#region Math methods

		/// <summary>
		/// Algorithm to parse RTE Fields for numbers to match against
		/// </summary>
		/// <param name="numbersToMatch">All numbers to look for matches</param>
		/// <param name="rteFields">RTE Fields to parse</param>
		/// <returns>Results containing match results for the input numbers and any other numbers that have no match</returns>
		internal ConfidenceReportMathResultDTO GetMathConfidenceResults(ICollection<string> numbersToMatch, ref ICollection<string> rteFields)
		{
			numbersToMatch = numbersToMatch.Select(str => str.Replace(",", "")).ToList();
			rteFields = rteFields.Select(str => str.Replace(",", "")).ToList();

			ICollection<decimal> numbersToMatchDecimals = ExtractDecimals(numbersToMatch);
			ICollection<decimal> rteFieldsDecimals = ExtractDecimals(rteFields);

			ConfidenceReportMathResultDTO result = new ConfidenceReportMathResultDTO();
			HashSet<decimal> rteFieldsDecimalsSet = new HashSet<decimal>(rteFieldsDecimals);

			foreach (decimal number in numbersToMatchDecimals)
			{
				result.Matches[number] = rteFieldsDecimalsSet.Contains(number);
			}

			return result;
		}

		/// <summary>
		/// Helper: Replaces variables in the MOQ Equation with their corresponding numerical value
		/// 
		/// We have to process WS variables first, then Task variables, to prevent a situation where the variables may have the same name.. just in case
		/// </summary>
		/// <param name="task">Task containing MOQ Equation and ordinary variables</param>
		/// <param name="wsReplacements">Workspace Variables and their corresponding value</param>
		/// <returns>MOQ Equation modified to replace variables with their corresponding numeric value</returns>
		internal string ReplaceVariablesWithValues(BoeTaskElementDTO task, Dictionary<string, decimal> wsReplacements)
		{
			string toReturn = task.MOQHoursEquation;

			// replace the variables with their value - WS variables first
			foreach (KeyValuePair<string, decimal> item in wsReplacements)
			{
				toReturn = toReturn.Replace(item.Key, item.Value.ToString());
			}

			// replace the variables with their value - task variables next
			foreach (KeyValuePair<string, decimal> item in task.OrdinaryVariables.Select(x => new { x.OrdinaryVariableName, x.OrdinaryVariableValue }).ToDictionary(x => x.OrdinaryVariableName, x => x.OrdinaryVariableValue ?? 0))
			{
				toReturn = toReturn.Replace(item.Key, item.Value.ToString());
			}

			return toReturn;
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

		#endregion Math methods

		/// <summary>
		/// Populate the error text for the report item. If there are no errors, also increment the tasks without errors for the full report
		/// </summary>
		/// <param name="reportItem">Report item</param>
		private void PopulateErrorText(ConfidenceReportItem reportItem)
		{
			ICollection<string> errorMessages = new Collection<string>();

			if (reportItem.PoPResults.PoPDateResults.Any(x => x.Value == PoPMatchResult.Missing || x.Value == PoPMatchResult.Partial))
			{
				reportItem.HasPoPError = true;
				errorMessages.Add(ConfidenceReportConstants.POP_ERROR);
			}

			if (reportItem.MoqResults.Matches.Any(x => !x.Value))
			{
				reportItem.HasMoqError = true;
				errorMessages.Add(ConfidenceReportConstants.MOQ_ERROR);
			}

			if (reportItem.HistoricalRefResults.Matches.Any(x => !x.Value))
			{
				reportItem.HasHistoricalRefError = true;
				errorMessages.Add(ConfidenceReportConstants.RELEVANT_HOURS_ERROR);
			}

			if (errorMessages.Any())
			{
				reportItem.ErrorText = string.Join(", ", errorMessages);
			}
			else
			{
				reportItem.ErrorText = ConfidenceReportConstants.NO_ERRORS;
			}
		}

		/// <summary>
		/// Update the ConfidenceValue and MaximumConfidenceValue for the results of a given task report item
		/// </summary>
		/// <param name="reportItem">ConfidenceReportItem for the Task</param>
		/// <param name="hasHistoricalRef">If the Task has any Historical Reference Values to check</param>
		/// <param name="confidencereportModelView">ModelView for the Confidence Report containing the Confidence Values</param>
		private void UpdateConfidenceValuesForTask(ConfidenceReportItem reportItem, bool hasHistoricalRef, ConfidenceReportModelView confidencereportModelView)
		{
			// Increment Maximum Confidence Value by 2 since all Tasks will have a PoP and MOQ to check
			confidencereportModelView.MaximumConfidenceValue += 2;

			// Only increment the maximum value for Historical Reference values if they are being checked
			if (hasHistoricalRef)
			{
				confidencereportModelView.MaximumConfidenceValue++;
			}

			// Increment ConfidenceValue when there's not an error of each type
			if (!reportItem.HasPoPError)
			{
				confidencereportModelView.ConfidenceValue++;
			}

			if (!reportItem.HasMoqError)
			{
				confidencereportModelView.ConfidenceValue++;
			}

			if (hasHistoricalRef && !reportItem.HasHistoricalRefError)
			{
				confidencereportModelView.ConfidenceValue++;
			}
		}
	}
}
