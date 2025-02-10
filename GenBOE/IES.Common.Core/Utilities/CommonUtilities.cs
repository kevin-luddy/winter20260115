// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Utilities
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.DirectoryServices;
	using System.IO;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Text.RegularExpressions;
	using DocumentFormat.OpenXml.InkML;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Enums;
	using Microsoft.AspNetCore.Authentication;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Http.Features;
	using Microsoft.AspNetCore.Mvc.ModelBinding;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Primitives;
	using Microsoft.Net.Http.Headers;
	using PickList;

	/// <summary>
	/// Utility/helper methods that need a class to sit in
	/// </summary>
	public static class CommonUtilities
	{
		/// <summary>
		/// The business hours start time.
		/// </summary>
		private const int BUSINESS_HOURS_START = 9;

		/// <summary>
		/// The business hours end time.
		/// </summary>
		private const int BUSINESS_HOURS_END = 20;

		private static string versionAndUpdatedDate;
		private static readonly object lockObject = new();
		private static DateTime? sapSpaceStartDate;
		private static DateTime? skillMixStartDate;
		private static DateTime? oneLmxStartDate;

		/// <summary>
		/// Asserts the equality of decimal values within an epsilon error range.
		/// Equality check to account for potential internal floating-point precision errors (e.g., 99.99997 vs 100) when comparing decimal values.
		/// </summary>
		/// <param name="expected">expected value</param>
		/// <param name="actual">actual value</param>
		/// <param name="epsilon">epsilon range</param>
		public static bool EqualsEpsilon(this decimal actual, decimal expected, decimal epsilon = 0.001m)
		{
			return Math.Abs(expected - actual) < epsilon;
		}

		/// <summary>
		/// 1LMX boundary time
		/// </summary>
		public static DateTime OneLmxStartDate
		{
			get
			{
				if (!oneLmxStartDate.HasValue)
				{
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("OneLmxStartDate"), out DateTime sapTime))
					{
						oneLmxStartDate = DateTime.MaxValue;
					}
					else
					{
						oneLmxStartDate = sapTime.Normalize();
					}
				}

				return oneLmxStartDate.Value;
			}
		}

		/// <summary>
		/// Space cutoff time for workspaces
		/// </summary>
		public static DateTime SAPSpaceStartDate
		{
			get
			{
				if (!sapSpaceStartDate.HasValue)
				{
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("SAPSpaceStartDate"), out DateTime sapTime))
					{
						sapSpaceStartDate = DateTime.MaxValue;
					}
					else
					{
						sapSpaceStartDate = sapTime;
					}
				}

				return sapSpaceStartDate.Value;
			}
		}

		/// <summary>
		/// Cutoff time for workspaces for skill mix
		/// </summary>
		public static DateTime SkillMixStartDate
		{
			get
			{
				if (!skillMixStartDate.HasValue)
				{
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("SkillMixStartDate"), out DateTime skillMixTime))
					{
						skillMixStartDate = DateTime.MaxValue;
					}
					else
					{
						skillMixStartDate = skillMixTime;
					}
				}

				return skillMixStartDate.Value;
			}
		}

		/// <summary>
		/// Create static Regex object for NewLine - to remove all possible version of a new line.. <br>, <br />, <br > and so on.
		/// </summary>
		private static readonly Regex regexNewLine = new(@"<br( )*/*( )*>", RegexOptions.IgnoreCase, CommonConstants.REGEX_TIMEOUT);

		#region Adjusting precision of decimal numbers, based on workspace settings

		/// <summary>
		/// Adjusts precision of a decimal number based on the settings in the Workspace (rounds it appropriately)
		/// </summary>
		/// <param name="originalNumber">Original Number to be rounded</param>
		/// <param name="workspace">Workspace</param>
		/// <returns>Decimal number adjusted to precision specified in the workspace (based on WS settings)</returns>
		public static decimal AdjustPrecision(decimal originalNumber, int? numberOfDecimalPlaces)
		{
			int decimalPlaces = numberOfDecimalPlaces.HasValue ? numberOfDecimalPlaces.Value : 0;
			MidpointRounding roundingMethod = MidpointRounding.AwayFromZero;

			// return the rounded number
			return Math.Round(originalNumber, decimalPlaces, roundingMethod);
		}

		/// <summary>
		/// Used to adjust calculations based on decimal precision. This is used to deal with remainders and such, instead of using the 1 or 0.5 to break the numbers apart
		/// </summary>
		/// <param name="decimalPlacesAllowed">Decimal Places</param>
		/// <returns>Offset for either multiplication or for addition/subtraction</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "-1*decimalPlacesAllowed")]
		public static decimal MultiplicationFactorForCalculationsDueToPrecisionAdjustment(int decimalPlacesAllowed)
		{
			double power = Math.Pow(10, -1 * decimalPlacesAllowed);

			if (double.IsInfinity(power))
			{
				throw new ArithmeticException("The number would evaluate to an infinity");
			}

			return Convert.ToDecimal(power);
		}

		#endregion

		#region Formatting numbers as strings with precision

		/// <summary>
		/// Converts number to a string, formatted with decimal precision
		/// </summary>
		/// <param name="originalNumber">Number to be converted to string</param>
		/// <param name="numberOfDecimalPlaces">Number of decimal places to use for precision</param>
		/// <returns>Number as a string with decimal precision</returns>
		public static string FormatStringWithPrecision(decimal originalNumber, int? numberOfDecimalPlaces)
		{
			string format = GetFormattingString(numberOfDecimalPlaces);
			string toReturn = originalNumber.ToString(format);
			return toReturn;
		}

		/// <summary>
		/// Converts number to a string, formatted with decimal precision. This format string
		/// does not contain a comma in the thousands digit.
		/// </summary>
		/// <param name="originalNumber">Number to be converted to string.</param>
		/// <param name="numberOfDecimalPlaces">Number of decimal places to use for precision.</param>
		/// <returns>Number as a string with decimal precision and no commas.</returns>
		public static string FormatStringWithPrecisionNoComma(decimal originalNumber, int? numberOfDecimalPlaces)
		{
			string format = PrecisionFormattingStringNoComma(numberOfDecimalPlaces);
			return originalNumber.ToString(format);
		}

		/// <summary>
		/// Gets the formatting string for use in converting numbers to strings with decimal precision
		/// </summary>
		/// <param name="numberOfDecimalPlaces">Number of decimal places to use for precision</param>
		/// <returns>Formatting string for use in converting numbers to strings with decimal precision</returns>
		public static string PrecisionFormattingString(int? numberOfDecimalPlaces)
		{
			return GetFormattingString(numberOfDecimalPlaces);
		}

		/// <summary>
		/// Gets the formatting string for use in converting numbers to strings with decimal precision keeping trailing zeros
		/// </summary>
		/// <param name="numberOfDecimalPlaces">Number of decimal places to use for precision</param>
		/// <returns>Formatting string for use in converting numbers to strings with decimal precision</returns>
		public static string PrecisionFormattingStringWithTrailingZeros(int? numberOfDecimalPlaces)
		{
			string format = "#,##0.";

			for (int i = 0; i < (numberOfDecimalPlaces ?? 0); i++)
			{
				format += "0";
			}

			return format;
		}

		/// <summary>
		/// Gets the formatting string for use in converting cost numbers to strings with decimal precision
		/// </summary>
		/// <param name="numberOfDecimalPlaces">Number of decimal places to use for precision</param>
		/// <returns>Formatting string for use in converting numbers to strings with decimal precision</returns>
		public static string CostPrecisionFormattingString(int numberOfDecimalPlaces)
		{
			string format = "#,##0.##";

			if (numberOfDecimalPlaces == CommonConstants.DOLLARS_AND_CENTS_PRECISION)
			{
				format = "#,##0.00";
			}

			return format;
		}

		/// <summary>
		/// Gets the formatting string for use in converting numbers to strings with decimal precision. This format string
		/// does not contain a comma in the thousands digit.
		/// </summary>
		/// <param name="numberOfDecimalPlaces">Number of decimal places to use for precision</param>
		/// <returns>Formatting string for use in converting numbers to strings with decimal precision without a comma.</returns>
		public static string PrecisionFormattingStringNoComma(int? numberOfDecimalPlaces)
		{
			string format = GetFormattingString(numberOfDecimalPlaces);
			return format.Replace(",", "");
		}

		/// <summary>
		/// Gets the formatting string for use in converting numbers to strings with decimal precision
		/// </summary>
		/// <param name="numberOfDecimalPlaces">Number of decimal places to use for precision</param>
		/// <returns>Formatting string for use in converting numbers to strings with decimal precision</returns>
		private static string GetFormattingString(int? numberOfDecimalPlaces)
		{
			string format = "#,##0.";

			for (int i = 0; i < (numberOfDecimalPlaces ?? 0); i++)
			{
				format += "#";
			}

			return format;
		}

		/// <summary>
		/// Prepends a "COPY # " prefix to the title of a task.
		/// </summary>
		/// <param name="originalTitle">Existing task element title</param>
		/// <param name="duplicateNumber">Which number copy the new task is</param>
		/// <returns>Task title with prefix</returns>
		public static string appendCopyPrefixToTitle(string originalTitle, int duplicateNumber)
		{
			string prefix = "COPY " + duplicateNumber + " - ";
			string updatedTitle = prefix + originalTitle;
			//Truncate any characters over the max title length of 100
			if (updatedTitle.Length > 100)
			{
				updatedTitle = updatedTitle.Remove(100);
			}
			return updatedTitle;
		}
		#endregion

		/// <summary>
		/// Removes <br /> tags from the string
		/// </summary>
		/// <param name="input">Input (string to clean up)</param>
		/// <returns>Cleaned up string</returns>
		public static string RemoveBrTagsFromText(string input)
		{
			string result = input;

			if (!string.IsNullOrEmpty(result))
			{
				result = regexNewLine.Replace(result, string.Empty).RemoveCarriageReturns();
			}

			return result;
		}

		/// <summary>
		/// Creates the version and last updated date information, for our application
		/// </summary>
		/// <returns>Version and time stamp information</returns>
		public static string VersionAndUpdatedDate
		{
			get
			{
				if (string.IsNullOrEmpty(versionAndUpdatedDate))
				{
					lock (lockObject)
					{
						if (string.IsNullOrEmpty(versionAndUpdatedDate))
						{
							System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
							FileInfo file = new(assembly.Location);
							string lastModifiedDate = file.LastWriteTime.ToShortDateString();
							string lastModifiedTime = file.LastWriteTime.ToLongTimeString();

							string appVersion = ConfigurationUtilities.GetAppSetting("APPLICATION_VERSION");

							versionAndUpdatedDate = string.Format("revision {0}, deployed on {1} {2}", appVersion, lastModifiedDate, lastModifiedTime);
						}
					}
				}

				return versionAndUpdatedDate;
			}
		}

		/// <summary>
		/// Returns true/false indicating whether it is a production environment
		/// (created to show/hide roll-up piwik data since roll-up data is only tracked in production)
		/// </summary>
		/// <returns>true if environment is production</returns>
		public static bool IsProduction()
		{
			return !string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("IsProduction"))
				&& ConfigurationUtilities.GetAppSetting("IsProduction").ToLower().Equals("true");
		}

		/// <summary>
		/// Returns true/false indicating whether the application is read only mode
		/// </summary>
		/// <returns></returns>
		public static bool IsReadOnly()
		{
			return !string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("IsReadOnly"))
				&& ConfigurationUtilities.GetAppSetting("IsReadOnly").ToLower().Equals("true");
		}

		/// <summary>
		/// Returns true/false indicating whether the piwik should be disabled. This is used for classified installations.
		/// </summary>
		/// <returns>Bool whether the links should be shut off or not</returns>
		public static bool DisablePiwik()
		{
			return !string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("DisablePiwik"))
				&& ConfigurationUtilities.GetAppSetting("DisablePiwik").ToLower().Equals("true");
		}

		/// <summary>
		/// Gets the Piwik URL from web.config
		/// </summary>
		/// <returns>Piwik Url</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string PiwikUrl()
		{
			return ConfigurationUtilities.GetAppSetting("PiwikURL");
		}

		/// <summary>
		/// Returns PiwikId from the web.config; if it doesn't exist -> returns 0
		/// </summary>
		/// <returns>Piwik Id for the site</returns>
		public static int PiwikId()
		{

			if (!int.TryParse(ConfigurationUtilities.GetAppSetting("PiwikId"), out int result))
			{
				result = 0;
			}

			return result;
		}

		/// <summary>
		/// Gets support email address from web.config
		/// </summary>
		/// <returns>Helpdesk email</returns>
		public static string HelpdeskEmailAddress()
		{
			return ConfigurationUtilities.GetAppSetting("HelpdeskEmailAddress");
		}

		/// <summary>
		/// Gets Service Central Support Link for RMS
		/// </summary>
		/// <returns>Service Central Link (RMS)</returns>
		public static string ServiceCentralLink()
		{
			string supportLink = string.Empty;

			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
			{
				supportLink = "<a href=\"" + ConfigurationUtilities.GetAppSetting("ServiceCentralLinkMST") + "\" target=\"_blank\">" + "Service Central RMS Ticket</a>";
			}
			else if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				supportLink = "<a href=\"mailto:" + ConfigurationUtilities.GetAppSetting("ServiceCentralLinkSpaceSystems") + "\">" + "Service Central Space Email</a>";
			}

			return supportLink;
		}

		/// <summary>
		/// Gets PPR&D Disclosure Log URL from web.config
		/// </summary>
		/// <returns>PPR&D Disclosure Log URL</returns>
		public static string PPRDDisclosureLogURL()
		{
			return ConfigurationUtilities.GetAppSetting("PPRDDisclosureLogURL");
		}

		/// <summary>
		/// Gets the PTM URL from web.config
		/// </summary>
		/// <returns>PTM Url</returns>
		public static Uri PTMUrl()
		{
			return SafeUriUtility.safeUri(ConfigurationUtilities.GetAppSetting("PTMURL"));
		}

		/// <summary>
		/// Creates a string containing all of the errors in the Model State
		/// </summary>
		/// <param name="modelStateDictionary">The ModelState being checked</param>
		/// <returns>A string of errors</returns>
		public static Collection<ValidationMessage> CreateModelStateValidationErrorList(ModelStateDictionary modelStateDictionary)
		{
			if (modelStateDictionary == null)
			{
				throw new ArgumentNullException(nameof(modelStateDictionary));
			}

			Collection<ValidationMessage> errors = new();

			foreach (KeyValuePair<string, ModelStateEntry> state in modelStateDictionary)
			{
				foreach (ModelError error in state.Value.Errors)
				{
					if (!string.IsNullOrEmpty(error.ErrorMessage))
					{
						errors.Add(new ValidationMessage(state.Key, error.ErrorMessage));
					}
					else
					{
						errors.Add(new ValidationMessage(state.Key, "Field was not valid."));
					}
				}
			}

			// remove duplicate messages.
			errors = new Collection<ValidationMessage>(errors.GroupBy(x => x.ValidationIssue).Select(x => x.First()).ToCollection());

			return errors;
		}

		/// <summary>
		/// Concatenates number, seperator, and title into one string or returns UNIQUE_MULTI_NUMBER.
		/// </summary>
		/// <param name="number">Number string</param>
		/// <param name="title">Title string</param>
		/// <param name="seperator">Seperator string</param>
		/// <returns>Concatenated string or the value in UNIQUE_MULTI_NUMBER</returns>
		public static string FormatNumberTitleString(string number, string title, string seperator)
		{
			if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(title))
			{
				if (number.ToUpper() == CommonConstants.UNIQUE_MULTI_NUMBER && title.ToUpper() == CommonConstants.UNIQUE_MULTI_NUMBER)
				{
					return CommonConstants.UNIQUE_MULTI_NUMBER;
				}
			}
			return number + seperator + title;
		}

		/// <summary>
		/// Formats the resource names.
		/// </summary>
		/// <param name="resourceName">Name of the resource.</param>
		/// <param name="oldResource">The old resource.</param>
		/// <param name="isOffloadedResource">True if this is the offloaded resource</param>
		/// <returns>A properly formatted resource name.</returns>
		public static string FormatResourceNames(string resourceName, string oldResource, bool isOffloadedResource)
		{
			if (resourceName == null)
			{
				throw new ArgumentNullException(nameof(resourceName));
			}

			string formattedName = resourceName;

			if (!isOffloadedResource && !string.IsNullOrWhiteSpace(oldResource))
			{
				formattedName += $" ({oldResource.ToUpper()})";
			}

			return formattedName;
		}

		/// <summary>
		/// Gets the workflow cutoff date used for determining when to send out emails.
		/// </summary>
		/// <returns>The cutoff Date</returns>
		public static DateTime GetWorkflowCutoffDate()
		{
			// retrieve number of hours offset from the last business hour
			int numHoursCutoffOffset = ConfigurationUtilities.GetAppSetting<int>("CutoffHoursOffset");

			// find the last business hour
			DateTime currentTime = DateTime.Now;
			return GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
		}

		/// <summary>
		/// Gets the workflow post approval cutoff date.
		/// </summary>
		/// <returns>The workflow post approval cutoff date.</returns>
		public static DateTime GetWorkflowPostApprovalCutoffDate()
		{
			// Only send emails for post approval when workflow status hasn't been changed for a day
			return DateTime.Now.AddDays(-1);
		}

		/// <summary>
		/// Gets the workflow cutoff date.  This is refactored into a method like this for testing purposes.
		/// </summary>
		/// <param name="numHoursCutoffOffset">The number hours cutoff offset.</param>
		/// <param name="currentTime">The current time.</param>
		/// <returns>The cutoff date.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "-1*numHoursCutoffOffset")]
		public static DateTime GetWorkflowCutoffDate(int numHoursCutoffOffset, DateTime currentTime)
		{
			// Find the last (most recent) business hour
			DateTime lastBusinessHour = currentTime;

			// Set the Hour to 5pm if after BUSINESS_HOURS_END
			if (lastBusinessHour.Hour >= BUSINESS_HOURS_END)
			{
				lastBusinessHour = new DateTime(lastBusinessHour.Year, lastBusinessHour.Month, lastBusinessHour.Day, BUSINESS_HOURS_END, 0, 0);
			}

			// Set the Hour to BUSINESS_HOURS_END previous day if before BUSINESS_HOURS_START
			if (lastBusinessHour.Hour < BUSINESS_HOURS_START)
			{
				lastBusinessHour = lastBusinessHour.AddDays(-1);
				lastBusinessHour = new DateTime(lastBusinessHour.Year, lastBusinessHour.Month, lastBusinessHour.Day, BUSINESS_HOURS_END, 0, 0); // BUSINESS_HOURS_END Friday
			}

			// Determine the cutoff date
			DateTime cutoffDate = lastBusinessHour.AddHours(-1 * numHoursCutoffOffset);

			// Check that we are not before the start of the day
			if (cutoffDate.Hour < BUSINESS_HOURS_START)
			{
				cutoffDate = cutoffDate.AddDays(-1);
				cutoffDate = new DateTime(cutoffDate.Year, cutoffDate.Month, cutoffDate.Day, BUSINESS_HOURS_END - (BUSINESS_HOURS_START - cutoffDate.Hour), 0, 0); // the cutoff brings the hours to before BUSINESS_HOURS_START, go to day before
			}

			// Check for weekend days
			if (cutoffDate.DayOfWeek == DayOfWeek.Sunday)
			{
				// If cutoff Date is Sunday, roll back to Friday
				cutoffDate = cutoffDate.AddDays(-2);
			}
			else if (cutoffDate.DayOfWeek == DayOfWeek.Saturday)
			{
				// If cutoff Date is Saturday, roll back to Friday
				cutoffDate = cutoffDate.AddDays(-1);
			}

			return cutoffDate;
		}

		/// <summary>
		/// Determines whether current time is within business hours.
		/// </summary>
		/// <returns>
		///   <c>true</c> if [within business hours]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsWithinBusinessHours()
		{
			bool withinBusinessHours = false;
			DateTime currentTime = DateTime.Now;
			if (currentTime.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
			{
				if (currentTime.Hour is >= BUSINESS_HOURS_START and <= BUSINESS_HOURS_END)
				{
					withinBusinessHours = true;
				}
			}

			return withinBusinessHours;
		}

		/// <summary>
		/// Determines if the current time is the time to send the PTM Document Reminder Emails
		/// </summary>
		/// <returns>True if start of business hours on Monday, False otherwise</returns>
		public static bool IsTimeForDocumentReminderEmails()
		{
			DateTime currentTime = DateTime.Now;
			return currentTime.DayOfWeek == DayOfWeek.Monday && currentTime.Hour >= BUSINESS_HOURS_START && currentTime.Hour < BUSINESS_HOURS_START + 1;
		}

		/// <summary>
		/// Gets a value indicating whether PTM is integrated into creation process.
		/// </summary>
		public static bool IsPTMIntegrated
		{
			get
			{
				bool.TryParse(ConfigurationUtilities.GetAppSetting("IsPTMIntegrated"), out bool value);
				return value && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems;
			}
		}

		/// <summary>
		/// Gets the pick list text.
		/// </summary>
		/// <param name="selectedId">The selected identifier.</param>
		/// <param name="values">The values.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static string GetPickListText(int? selectedId, ICollection<PickListDto> values, string defaultValue = "")
		{
			string text = defaultValue;

			if (selectedId.HasValue)
			{
				PickListDto dto = values.FirstOrDefault(v => v.Id == selectedId.Value);
				if (dto != null)
				{
					text = dto.Text;
				}
			}

			return text;
		}

		/// <summary>
		/// Replaces spaces with underscores and removes invalid file name characters
		/// </summary>
		/// <param name="filename">Filename to clean</param>
		/// <returns>Filename with only valid characters</returns>
		public static string CleanFileName(string filename)
		{
			if (filename == null)
			{
				throw new ArgumentNullException(nameof(filename));
			}

			return string.Concat(filename.Replace(' ', '_').Split(Path.GetInvalidFileNameChars()));
		}

		/// <summary>
		/// Add Authorization Header to the HttpClient, if the token is provided
		/// </summary>
		/// <param name="client">HttpClient</param>
		/// <param name="token">Token</param>
		public static void AddAuthorizationHeader(this HttpClient client, string token)
		{
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			if (!string.IsNullOrEmpty(token))
			{
				string fullToken = CommonConstants.TOKEN_PREFIX + token;
				lock (lockObject)
				{
					if (!client.DefaultRequestHeaders.Any(h => h.Key == HeaderNames.Authorization && h.Value.Any(v => v == fullToken)))
					{
						client.DefaultRequestHeaders.Remove(HeaderNames.Authorization);
						client.DefaultRequestHeaders.Add(HeaderNames.Authorization, fullToken);
					}
				}
			}
		}

		/// <summary>
		/// Private for Is Sap Enabled, used for unit testing.. I know this is horrid design :(
		/// </summary>
		private static bool? isSapEnabled;

		/// <summary>
		/// Indicates whether SAP features are enabled
		/// </summary>
		public static bool IsSAPEnabledForSystem
		{
			get
			{
				if (isSapEnabled == null)
				{
					bool.TryParse(ConfigurationUtilities.GetAppSetting("EnableSAP"), out bool value);
					isSapEnabled = value;
				}

				return isSapEnabled.Value;
			}
			internal set // be able to override for unit test purposes
			{
				isSapEnabled = value;
			}
		}

		/// <summary>
		/// Is SAP Enabled for this workspace
		/// </summary>
		/// <param name="workspaceCreationDate">workspace creation date</param>
		/// <returns>True if SAP is enabled for this workspace</returns>
		public static bool IsSAPEnabledForWorkspace(bool workspaceEnabledSAPConnection, DateTime? workspaceCreationDate)
		{
			return workspaceEnabledSAPConnection && IsSAPEnabledForSystem &&
				!IsWorkspaceBeforeSAPCutoff(workspaceCreationDate);
		}

		/// <summary>
		/// Is the workspace before the SAP cutoff (always false for RMS)
		/// </summary>
		/// <param name="workspaceCreationDate">workspace creation date</param>
		/// <returns>True if workspace creation date is before SAP cutoff, always false if RMS.</returns>
		public static bool IsWorkspaceBeforeSAPCutoff(DateTime? workspaceCreationDate)
		{
			return SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems &&
				(!workspaceCreationDate.HasValue || workspaceCreationDate < SAPSpaceStartDate);
		}

		/// <summary>
		/// Is SAP connection shown to the user for this workspace
		/// </summary>
		/// <param name="workspaceCreationDate"></param>
		/// <returns></returns>
		public static bool ShowSAPForWorkspace(DateTime? workspaceCreationDate)
		{
			return IsSAPEnabledForSystem &&
				(SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST ||
				workspaceCreationDate >= SAPSpaceStartDate);
		}

		/// <summary>
		/// Private for Is BRC Enabled, used for unit testing
		/// </summary>
		private static bool? isBRCEnabled;

		/// <summary>
		/// Private for OverrideBRCValues
		/// </summary>
		private static string overrideBRCValues;

		/// <summary>
		/// Indicates whether BRC features are enabled
		/// </summary>
		public static bool IsBRCEnabledForSystem
		{
			get
			{
				if (isBRCEnabled == null)
				{
					bool.TryParse(ConfigurationUtilities.GetAppSetting("EnableBRC"), out bool value);
					isBRCEnabled = value;
				}

				return isBRCEnabled.Value;
			}
			internal set // be able to override for unit test purposes
			{
				isBRCEnabled = value;
			}
		}

		/// <summary>
		/// Overridden BRC Values
		/// </summary>
		public static string OverrideBRCValues
		{
			get
			{
				if (overrideBRCValues == null)
				{
					overrideBRCValues = ConfigurationUtilities.GetAppSetting("OverrideBRC");
				}

				return overrideBRCValues;
			}
			internal set // be able to override for unit test purposes
			{
				overrideBRCValues = value;
			}
		}

		/// <summary>
		/// Private for Is Confidence Report Enabled, used to cache value
		/// </summary>
		private static bool? isConfidenceReportEnabled;

		/// <summary>
		/// Indicates whether Confidence Report is enabled
		/// </summary>
		public static bool IsConfidenceReportEnabled
		{
			get
			{
				if (isConfidenceReportEnabled == null)
				{
					bool.TryParse(ConfigurationUtilities.GetAppSetting("EnableConfidenceReport"), out bool value);
					isConfidenceReportEnabled = value;
				}

				return isConfidenceReportEnabled.Value;
			}
			internal set => isConfidenceReportEnabled = value;
		}

		/// <summary>
		/// Private for Is Skill Mix Enabled, used for unit testing.. I know this is horrid design :(
		/// </summary>
		private static bool? isSkillMixEnabled;

		/// <summary>
		/// Indicates whether Skill Mix features are enabled
		/// </summary>
		public static bool IsSkillMixEnabledForSystem
		{
			get
			{
				if (isSkillMixEnabled == null)
				{
					bool.TryParse(ConfigurationUtilities.GetAppSetting("EnableSkillMix"), out bool value);
					isSkillMixEnabled = value;
				}

				return isSkillMixEnabled.Value;
			}
			internal set // be able to override for unit test purposes
			{
				isSkillMixEnabled = value;
			}
		}

		/// <summary>
		/// Private for Is Task Assign Author Enabled, used for unit testing. Following above design principle
		/// </summary>
		private static bool? isAssignTaskAuthorEnabled;

		/// <summary>
		/// Indicates whether Task Assign Author features are enabled
		/// </summary>
		public static bool IsAssignTaskAuthorEnabledForSystem
		{
			get
			{
				if (isAssignTaskAuthorEnabled == null)
				{
					if (bool.TryParse(ConfigurationUtilities.GetAppSetting("EnableTaskAssignAuthor"), out bool value))
					{
						isAssignTaskAuthorEnabled = value;
					}
				}

				return isAssignTaskAuthorEnabled ?? false;
			}

			// be able to override for unit test purposes
			internal set => isAssignTaskAuthorEnabled = value;
		}

		/// <summary>
		/// Is Skill Mix connection shown to the user for this workspace
		/// </summary>
		/// <param name="workspaceCreationDate"></param>
		/// <returns></returns>
		public static bool ShowSkillMixForWorkspace(DateTime? workspaceCreationDate)
		{
			return IsSkillMixEnabledForSystem && workspaceCreationDate >= SkillMixStartDate;
		}

		/// <summary>
		/// Returns true/false indicating whether the external help links should be shut off. This is used for classified installations, 
		/// to not point at unclassified locations that are not accessible.
		/// </summary>
		/// <returns>Bool whether the links should be shut off or not</returns>
		public static bool DisableExternalHelpLinksForClassifiedInstallations()
		{
			bool result = false;

			if (!string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("ShutOffExternalLinksForClassifiedInstall"))
				&& ConfigurationUtilities.GetAppSetting("ShutOffExternalLinksForClassifiedInstall").ToLower().Equals("true"))
			{
				result = true;
			}

			return result;
		}
		/// <summary>
		/// The Host Environment web path
		/// </summary>
		public static string HostEnvironmentWebpath { get; private set; }

		/// <summary>
		/// Initialize the Utilities class
		/// </summary>
		/// <param name="hostEnvironment"></param>
		public static void Initialize(string hostEnvironmentWebpath)
		{
			HostEnvironmentWebpath = hostEnvironmentWebpath;
		}

		/// <summary>
		/// Maps a filename to a server path
		/// </summary>
		/// <param name="fileName">the filename to map</param>
		/// <returns>The mapped filename path</returns>
		public static string MapPath(string fileName)
		{
			string path = Path.Combine(HostEnvironmentWebpath, fileName);

			return path;
		}

		/// <summary>
		/// Get Property Value if it exists
		/// </summary>
		/// <param name="propertyCollection">Collection of Properties to traverse through</param>
		/// <param name="propertyName">Property Value to retrieve</param>
		/// <param name="stringManipulation">Determines how to manipulate string for return</param>
		/// <returns>Property Value if it exists, empty string otherwise</returns>
		public static string TryGetPropertyValue(PropertyCollection propertyCollection, string propertyName, StringManipulation stringManipulation = StringManipulation.None)
		{
			string toReturn = string.Empty;

			if (propertyCollection != null)
			{
				toReturn = propertyCollection.Contains(propertyName) ? propertyCollection[propertyName][0].ToString() : string.Empty;
			}

			switch (stringManipulation)
			{
				case StringManipulation.ToLower:
					return string.IsNullOrEmpty(toReturn) ? toReturn : toReturn.ToLower();
				case StringManipulation.ToUpper:
					return string.IsNullOrEmpty(toReturn) ? toReturn : toReturn.ToUpper();
				case StringManipulation.None:
				default:
					return toReturn;
			}
		}

		/// <summary>
		/// Get Property Value if it exists
		/// </summary>
		/// <param name="propertyCollection">Collection of Properties to traverse through</param>
		/// <param name="propertyName">Property Value to retrieve</param>
		/// <param name="stringManipulation">Determines how to manipulate string for return</param>
		/// <returns>Property Value if it exists, empty string otherwise</returns>
		public static string TryGetPropertyValue(ResultPropertyCollection propertyCollection, string propertyName, StringManipulation stringManipulation = StringManipulation.None)
		{
			string toReturn = string.Empty;

			if (propertyCollection != null)
			{
				toReturn = propertyCollection.Contains(propertyName) ? propertyCollection[propertyName][0].ToString() : string.Empty;
			}

			switch (stringManipulation)
			{
				case StringManipulation.ToLower:
					return string.IsNullOrEmpty(toReturn) ? toReturn : toReturn.ToLower();
				case StringManipulation.ToUpper:
					return string.IsNullOrEmpty(toReturn) ? toReturn : toReturn.ToUpper();
				case StringManipulation.None:
				default:
					return toReturn;
			}
		}

		/// <summary>
		/// Get User phone number
		/// </summary>
		/// <param name="propertyCollection">Collection of Properties to traverse through</param>
		/// <returns>Business phone number by default but if that does not exist, then it returns mobile number</returns>
		public static string GetUserPhoneNumber(ResultPropertyCollection propertyCollection)
		{
			string phone = TryGetPropertyValue(propertyCollection, "telephonenumber");

			return !string.IsNullOrEmpty(phone) ? phone : TryGetPropertyValue(propertyCollection, "mobile");
		}

		/// <summary>
		/// Get User phone number
		/// </summary>
		/// <param name="propertyCollection">Collection of Properties to traverse through</param>
		/// <returns>Business phone number by default but if that does not exist, then it returns mobile number</returns>
		public static string GetUserPhoneNumber(PropertyCollection propertyCollection)
		{
			string phone = TryGetPropertyValue(propertyCollection, "telephonenumber");

			return !string.IsNullOrEmpty(phone) ? phone : TryGetPropertyValue(propertyCollection, "mobile");
		}

		/// <summary>
		/// Log environment variables and config app settings in elmah
		/// </summary>
		/// <param name="log">logger</param>
		public static void LogEnvironmentSettings(IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env, HttpContext context = null)
		{
			if (env.IsDevelopment() && config["EnableEnvironmentInfoLogging"] != null && config["EnableEnvironmentInfoLogging"] == "True")
			{
				StringBuilder sb = new StringBuilder();
				string nl = System.Environment.NewLine;
				string rule = string.Concat(nl, new string('-', 40), nl);
				IAuthenticationSchemeProvider authSchemeProvider = app.ApplicationServices.GetRequiredService<IAuthenticationSchemeProvider>();

				sb.Append($"Request{rule}");
				sb.Append($"{DateTimeOffset.Now}{nl}");
				if (context != null)
				{
					sb.Append($"{context.Request.Method} {context.Request.Path}{nl}");
					sb.Append($"Scheme: {context.Request.Scheme}{nl}");
					sb.Append($"Host: {context.Request.Headers["Host"]}{nl}");
					sb.Append($"PathBase: {context.Request.PathBase.Value}{nl}");
					sb.Append($"Path: {context.Request.Path.Value}{nl}");
					sb.Append($"Query: {context.Request.QueryString.Value}{nl}{nl}");

					sb.Append($"Connection{rule}");
					sb.Append($"RemoteIp: {context.Connection.RemoteIpAddress}{nl}");
					sb.Append($"RemotePort: {context.Connection.RemotePort}{nl}");
					sb.Append($"LocalIp: {context.Connection.LocalIpAddress}{nl}");
					sb.Append($"LocalPort: {context.Connection.LocalPort}{nl}");
					sb.Append($"ClientCert: {context.Connection.ClientCertificate}{nl}{nl}");

					sb.Append($"Identity{rule}");
					sb.Append($"User: {context.User.Identity.Name}{nl}");

					sb.Append($"Headers{rule}");
					foreach (KeyValuePair<string, StringValues> header in context.Request.Headers)
					{
						sb.Append($"{header.Key}: {header.Value}{nl}");
					}
					sb.Append(nl);

					sb.Append($"WebSockets{rule}");
					if (context.Features.Get<IHttpUpgradeFeature>() != null)
					{
						sb.Append($"Status: Enabled{nl}{nl}");
					}
					else
					{
						sb.Append($"Status: Disabled{nl}{nl}");
					}
				}

				sb.Append($"Configuration{rule}");
				foreach (KeyValuePair<string, string?> pair in config.AsEnumerable())
				{
					sb.Append($"{pair.Key}: {pair.Value}{nl}");
				}
				sb.Append(nl);
				sb.Append($"Environment Variables{rule}");
				IDictionary vars = System.Environment.GetEnvironmentVariables();
				foreach (var key in vars.Keys.Cast<string>().OrderBy(key => key,
					StringComparer.OrdinalIgnoreCase))
				{
					var value = vars[key];
					sb.Append($"{key}: {value}{nl}");
				}

				Serilog.Log.Debug("Begin LogEnvironmentInfo");
				Serilog.Log.Debug(sb.ToString());
				Serilog.Log.Debug("End LogEnvironmentInfo");
			}
		}
	}
}