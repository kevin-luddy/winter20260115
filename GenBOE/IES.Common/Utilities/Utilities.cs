// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.DirectoryServices;
	using System.IO;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Web.Mvc;
	using classes;
	using Exceptions;
	using IES.Common.Enums;
	using Microsoft.Net.Http.Headers;
	using PickList;

	/// <summary>
	/// Utility/helper methods that need a class to sit in
	/// </summary>
	public static class Utilities
	{
		/// <summary>
		/// The business hours start time.
		/// </summary>
		private const int BUSINESS_HOURS_START = 9;

		/// <summary>
		/// The business hours end time.
		/// </summary>
		private const int BUSINESS_HOURS_END = 20;

		private static string versionAndUpdatedDate = null;
		private static object lockObject = new object();
		private static DateTime? sapSpaceStartDate;
		private static DateTime? skillMixStartDate;
		private static DateTime? oneLmxStartDate;
		private static DateTime? datepickerRestrictionRMS;
		private static DateTime? historicalReferenceExplanationStartDate;
		private static DateTime? showINLCutoffDate;
		private static readonly IActiveDirectoryUtilities activeDirectoryUtilities = GenBOEUnityContainer.Resolve<IActiveDirectoryUtilities>();
		
		/// <summary>
		/// Private for UCOT Start Date
		/// </summary>
		private static DateTime? ucotStartDate;

		/// <summary>
		/// Private for Is UCOT Enabled
		/// </summary>
		internal static bool? isUCOTEnabled;

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
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("OneLmxStartDate"), out DateTime startDate))
					{
						oneLmxStartDate = DateTime.MaxValue;
					}
					else
					{
						oneLmxStartDate = startDate.Normalize();
					}
				}

				return oneLmxStartDate.Value;
			}
		}

		/// <summary>
		/// Get the restricted date set in the config for RMS
		/// </summary>
		public static DateTime DatepickerRestrictionRMS
		{
			get
			{
				if (!datepickerRestrictionRMS.HasValue)
				{
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("DatepickerRestrictionRMS"), out DateTime dateRestriction))
					{
						datepickerRestrictionRMS = DateTime.MinValue;
					}
					else
					{
						datepickerRestrictionRMS = dateRestriction;
					}
				}

				return datepickerRestrictionRMS.Value;
			}
		}

		/// <summary>
		/// get user JobTitle from active directories utility
		/// </summary>
		public static string JobTitle
		{
			get
			{
				return GetUser()?.Title;
			}
		}

		/// <summary>
		/// get user department from active directories utility
		/// </summary>
		public static string DepartmentTitle
		{
			get
			{
				return GetUser()?.Department;
			}
		}

		/// <summary>
		/// get user data from active directories utility
		/// </summary>
		private static UserData GetUser()
		{
			string currentUserNtid = Thread.CurrentPrincipal.Identity.Name;
			if ((!string.IsNullOrEmpty(currentUserNtid)) && (currentUserNtid.Contains('\\')))
			{
				currentUserNtid = currentUserNtid.Split('\\').Last();
			}
			return activeDirectoryUtilities.GetUserByQualifiedAccount(currentUserNtid, false);
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
		/// Is UCOT/Uncompensated Overtime enabled?
		/// </summary>
		public static bool IsUCOTEnabledForSystem
		{
			get
			{
				if (isUCOTEnabled == null)
				{
					bool.TryParse(ConfigurationUtilities.GetAppSetting("EnableUCOT"), out bool ucotEnabled);
					isUCOTEnabled = ucotEnabled;
				}

				return isUCOTEnabled.Value;
			}
		}

		/// <summary>
		/// Cutoff time for workspaces for UCOT
		/// </summary>
		public static DateTime UCOTStartDate
		{
			get
			{
				if (!ucotStartDate.HasValue)
				{
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("UCOTStartDate"), out DateTime ucotTime))
					{
						ucotStartDate = DateTime.MaxValue;
					}
					else
					{
						ucotStartDate = ucotTime;
					}
				}

				return ucotStartDate.Value;
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
		/// Date to begin using Historical Reference Explanation
		/// </summary>
		public static DateTime HistoricalReferenceExplanationStartDate
		{
			get
			{
				if (!historicalReferenceExplanationStartDate.HasValue)
				{
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("HistoricalReferenceExplanationStartDate"), out DateTime startDate))
					{
						historicalReferenceExplanationStartDate = DateTime.MaxValue;
					}
					else
					{
						historicalReferenceExplanationStartDate = startDate;
					}
				}

				return historicalReferenceExplanationStartDate.Value;
			}
		}

		/// <summary>
		/// Cutoff date to show PBOE/IBOE forms for workspace
		/// </summary>
		public static DateTime ShowINLCutoffDate
		{
			get
			{
				if (!showINLCutoffDate.HasValue)
				{
					if (!DateTime.TryParse(ConfigurationUtilities.GetAppSetting("ShowINLCutoffDate"), out DateTime cutoffDate))
					{
						showINLCutoffDate = DateTime.MaxValue;
					}
					else
					{
						showINLCutoffDate = cutoffDate;
					}
				}

				return showINLCutoffDate.Value;
			}
		}

		/// <summary>
		/// Create static Regex object for NewLine - to remove all possible version of a new line.. <br>, <br />, <br > and so on.
		/// </summary>
		private static Regex regexNewLine = new Regex(@"<br( )*/*( )*>", RegexOptions.IgnoreCase, Constants.REGEX_TIMEOUT);

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
			double power = Math.Pow(10, (-1 * decimalPlacesAllowed));

			if (Double.IsInfinity(power))
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
			string toReturn = "";

			toReturn = originalNumber.ToString(format);

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

			if (numberOfDecimalPlaces == Constants.DOLLARS_AND_CENTS_PRECISION)
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
			String prefix = "COPY " + duplicateNumber + " - ";
			String updatedTitle = prefix + originalTitle;
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
							FileInfo file = new FileInfo(assembly.Location);
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
			int result;

			if (!int.TryParse(ConfigurationUtilities.GetAppSetting("PiwikId"), out result))
			{
				result = 0;
			}

			return result;
		}

		/// <summary>
		/// Returns the Current user.
		/// </summary>
		/// <returns>The current user.</returns>
		public static string CurrentUser()
		{
			string[] splitDomainAndNtid = Thread.CurrentPrincipal.Identity.Name.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
			return string.Join(" ", splitDomainAndNtid);
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

			Collection<ValidationMessage> errors = new Collection<ValidationMessage>();

			foreach (KeyValuePair<string, ModelState> state in modelStateDictionary)
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
				if (number.ToUpper() == IES.Common.Constants.UNIQUE_MULTI_NUMBER && title.ToUpper() == IES.Common.Constants.UNIQUE_MULTI_NUMBER)
				{
					return IES.Common.Constants.UNIQUE_MULTI_NUMBER;
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
			if (currentTime.DayOfWeek != DayOfWeek.Saturday && currentTime.DayOfWeek != DayOfWeek.Sunday)
			{
				if (currentTime.Hour >= BUSINESS_HOURS_START && currentTime.Hour <= BUSINESS_HOURS_END)
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
				bool value = false;
				bool.TryParse(ConfigurationUtilities.GetAppSetting("IsPTMIntegrated"), out value);
				return value && (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems);
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
		/// Removes characters that cause issues when present in file names; Some may look like duplicates, but they actually aren't
		/// </summary>
		/// <param name="target">String to be cleaned</param>
		/// <param name="replacementValue">Character to replace the illegal characters with. Defaults to "."</param>
		/// <returns>String with illegal characters replaced</returns>
		public static string StripIllegalFileNameCharacters(string target, string replacementValue = ".")
		{
			if (string.IsNullOrEmpty(target))
			{
				throw new ArgumentNullException("target");
			}

			char[] illegalCharacters = new[] { ' ', ' ', '/', '\\', '\n', '\r', '\'', '"', '–', '-', '%', '#', '$', '&', ')', '(', '!', ',', ':', ';', '{', '}', '`', '~', '^', '/', '<', '>' };
			string[] cleanedParts = target.Split(illegalCharacters, StringSplitOptions.RemoveEmptyEntries);

			return string.Join(replacementValue, cleanedParts);
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

			return StripIllegalFileNameCharacters(string.Concat(filename.Replace(' ', '_').Split(Path.GetInvalidFileNameChars())));
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
				string fullToken = Constants.TOKEN_PREFIX + token;
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
		///   Private for IsPLD 
		/// </summary>
		private static bool? isPLDIntegrated;

		/// <summary>
		///  Is PLD Integrated
		/// </summary>
		public static bool ShowPLDIsIntegrated
		{
			get
			{
				if (isPLDIntegrated == null)
				{
					bool.TryParse(ConfigurationUtilities.GetAppSetting("IsPLDIntegrated"), out bool pldIntegrated);
					isPLDIntegrated = pldIntegrated;
				}

				return isPLDIntegrated.Value;
			}
		}


		/// <summary>
		/// Private for Is BRC Enabled, used for unit testing
		/// </summary>
		private static bool? isBRCEnabled;

		/// <summary>
		/// Private for OverrideBRCValues
		/// </summary>
		private static string[] overrideBRCValues;

		/// <summary>
		/// Indicates whether BRC features are enabled
		/// </summary>
		private static bool IsBRCEnabledForSystem
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
		}

		/// <summary>
		/// Private for Is Report Generation External
		/// </summary>
		private static bool? isReportGenerationExternal;

		/// <summary>
		/// Is the generation of Reports External; specifically BOE Export to Word (and future to Excel)
		/// </summary>
		public static bool IsReportGenerationExternal
		{
			get
			{
				if (isReportGenerationExternal == null)
				{
					if (bool.TryParse(ConfigurationUtilities.GetAppSetting("IsReportGenerationExternal"), out bool value))
					{
						isReportGenerationExternal = value;
					}
					else
					{
						// default to false
						isReportGenerationExternal = false;
					}
				}

				return isReportGenerationExternal.Value;
			}
		}



		/// <summary>
		/// Be able to override for unit test purposes
		/// </summary>
		/// <param name="value"></param>
		static internal void SetBRCEnabled(bool value)
		{
			isBRCEnabled = value;
		}

		/// <summary>
		/// Is BRC Enabled for Workspace
		/// </summary>
		/// <param name="workspaceShortName">workspace short name</param>
		/// <returns>True if BRC enabled, false if disabled or workspace overridden to be disabled</returns>
		public static bool IsBRCEnabledForWorkspace(string workspaceShortName)
		{
			if (OverrideBRCValues != null && OverrideBRCValues.Contains(workspaceShortName))
			{
				return false;
			}

			return IsBRCEnabledForSystem;
		}

		/// <summary>
		/// Overridden BRC Values
		/// </summary>
		public static ICollection<string> OverrideBRCValues
		{
			get
			{
				if (overrideBRCValues == null)
				{
					string values = ConfigurationUtilities.GetAppSetting("OverrideBRC");
					if (!string.IsNullOrWhiteSpace(values))
					{
						overrideBRCValues = values.Split(',');
					}
				}

				return overrideBRCValues;
			}
			internal set // be able to override for unit test purposes
			{
				overrideBRCValues = value.ToArray();
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

			// be able to override for unit test purposes
			internal set => isSkillMixEnabled = value;
		}

		/// <summary>
		/// Private for Is Assign Task Author Enabled, used for unit testing. Following above design principle
		/// </summary>
		private static bool? isAssignTaskAuthorEnabled;

		/// <summary>
		/// Indicates whether Assign Task Author features are enabled
		/// </summary>
		public static bool IsAssignTaskAuthorEnabledForSystem
		{
			get
			{
				if (isAssignTaskAuthorEnabled == null 
					&& bool.TryParse(ConfigurationUtilities.GetAppSetting("EnableAssignTaskAuthor"), out bool value))
				{
					isAssignTaskAuthorEnabled = value;
				}

				return isAssignTaskAuthorEnabled ?? false;
			}

			// be able to override for unit test purposes
			internal set => isAssignTaskAuthorEnabled = value;
		}

		/// <summary>
		/// Private for Is Skill Mix Whitelist Enabled
		/// </summary>
		private static bool? isSkillMixWhitelistEnabled;

		/// <summary>
		/// Private for Skill Mix whitelisted workspaces
		/// </summary>
		private static Collection<string> skillMixWhitelistWorkspaces;

		/// <summary>
		/// Update the Skill Mix Whitelist settings--currently utilized by Space only
		/// </summary>
		/// <param name="whiteListEnabled">Is the whitelist for Skill Mix enabled? (set as a boolean-parseable string)</param>
		/// <param name="whiteListWorkspaces">Comma-separated string of whitelisted workspaces (by short name)</param>
		public static void UpdateSkillMixWhitelistSettings(string whiteListEnabled, string whiteListWorkspaces)
		{
			// Parse the disabled/enabled whitelist value
			if (bool.TryParse(whiteListEnabled, out bool value))
			{
				isSkillMixWhitelistEnabled = value;
			}

			// Update the list of whitelisted Skill Mix workspaces (short name)
			skillMixWhitelistWorkspaces = whiteListWorkspaces.Split(',').ToCollection();
		}

		/// <summary>
		/// Is Skill Mix connection shown to the user for this workspace?
		/// </summary>
		/// <param name="workspaceCreationDate">Workspace creation date.</param>
		/// <param name="workspaceShortName">The workspace short name</param>
		/// <returns>Option to show skill mix for workspace.</returns>
		public static bool ShowSkillMixForWorkspace(DateTime? workspaceCreationDate, string workspaceShortName)
		{
			// Whitelist config is for Space-only
			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				if (IsSkillMixEnabledForSystem && workspaceCreationDate >= skillMixStartDate
					&& isSkillMixWhitelistEnabled.HasValue && isSkillMixWhitelistEnabled.Value
					&& skillMixWhitelistWorkspaces.Contains(workspaceShortName))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return IsSkillMixEnabledForSystem && workspaceCreationDate >= SkillMixStartDate;
			}
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
		/// Is HistoricalReferenceExplanation required and displayed
		/// </summary>
		/// <param name="workspaceCreationDate">workspace creation date</param>
		/// <returns>True if workspace creation date after the start date for Historical Reference Explanation</returns>
		public static bool IsHistoricalReferenceExplanationRequired(DateTime? workspaceCreationDate)
		{
			return workspaceCreationDate.HasValue && workspaceCreationDate.Value.Date >= HistoricalReferenceExplanationStartDate.Date;
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
		public static void LogEnvironmentSettings(Logger log)
		{
			_ = log ?? throw new ArgumentNullException(nameof(log));

			string loggingEnabled = ConfigurationUtilities.GetAppSetting("EnableEnvionmentInfoLogging");
			if (loggingEnabled != null && loggingEnabled == "true")
			{
				log.Debug("Begin Environment Variables");
				foreach (DictionaryEntry envVariable in Environment.GetEnvironmentVariables())
				{
					log.Debug(envVariable.Key + " - " + envVariable.Value);
				}
				log.Debug("End Environment Variables");

				log.Debug("Begin Configuration Manager App Settings");
				foreach (string configSettingKey in ConfigurationUtilities.GetAppSettingsKeys())
				{
					log.Debug(configSettingKey + " - " + ConfigurationUtilities.GetAppSetting(configSettingKey));
				}
				log.Debug("End Configuration Manager App Settings");
			}
		}

		/// <summary>
		/// Method to Populate Model from Dictionary of <Property, Value> Pair
		/// </summary>
		/// <typeparam name="T">Type of Model being passed in</typeparam>
		/// <param name="model">Model to Populate</param>
		/// <param name="valuesForModel">Values to populate the Model with</param>
		public static void PopulateModel<T>(T model, Dictionary<string, string> valuesForModel)
		{
			_ = valuesForModel ?? throw new ArgumentNullException(nameof(valuesForModel));

			// Iterate through the Model Properties
			// Since this is static method, we only get access to the Array.ForEach
			// Hence after the .GetProperties() we have to add the .ToList() before the .ForEach()
			model.GetType().GetProperties().ToList().ForEach(p =>
			{
				TypeConverter converter = TypeDescriptor.GetConverter(p.PropertyType);
				Object convertedObject = converter.ConvertFromString(valuesForModel[p.Name]);
				p.SetValue(model, convertedObject, null);
			});
		}

		/// <summary>
		/// Method to populate values passed HTTP Content to a Dictionary
		/// </summary>
		/// <param name="contents">List of HTTP Content</param>
		/// <param name="valuesToPopulate">List that will be populated with <key, value> Pair from content values</param>
		public static void GetModelValuesFromContent(IEnumerable<HttpContent> contents, Dictionary<string, string> valuesToPopulate)
		{
			_ = contents ?? throw new ArgumentNullException(nameof(contents));
			_ = valuesToPopulate ?? throw new ArgumentNullException(nameof(valuesToPopulate));

			foreach (HttpContent content in contents)
			{
				// We want to get the Values Passed in the body as Dictionary items so further reduce complexity
				string key = content.Headers.ContentDisposition.Name.Replace("\"", "");
				string value = content.ReadAsStringAsync().Result;
				valuesToPopulate.Add(key, value);
			}
		}

		/// <summary>
		/// Is UCOT shown to the user for this workspace
		/// </summary>
		/// <param name="workspaceCreationDate">Workspace creation date</param>
		/// <param name="shortname">Workspace shortname.</param>
		/// <returns>True to show UCOT</returns>
		public static bool ShowUCOTForWorkspace(DateTime? workspaceCreationDate, string shortname)
		{
			// UCOT is space only, no need to run logic if it isn't space.
			if (SystemConfiguration.Instance().CompanyMode != CompanyConfiguration.SpaceSystems)
			{
				return false;
			}

			// Exclude UCOT showing for specific Workspace Shortnames.
			string excludedShortspaces = ConfigurationUtilities.GetAppSetting("UcotExcludedWorkspaces");
			string[] excludedShortspacesArray = excludedShortspaces?.Split(',').Select(s => s.Trim()).ToArray();

			if (excludedShortspacesArray != null && excludedShortspacesArray.Any() && excludedShortspacesArray.Contains(shortname))
			{
				return false;
			}
			else
			{
				return IsUCOTEnabledForSystem && workspaceCreationDate >= UCOTStartDate;
			}
		}
	}
}