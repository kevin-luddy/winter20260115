/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
	using System;
	using System.IO;
	using System.Reflection;

	/// <summary>
	/// The constants class, containing shared constants to be used across the entire solution.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// The IES database context name
		/// </summary>
		public const string IES_DB_CONTEXT_NAME = "name=IESEntities";

		/// <summary>
		/// String that gets logged when an activity starts.
		/// </summary>
		public const string LOG_ACTIVITY_START = "Begin {0}.";

		/// <summary>
		/// String that gets logged when an activity ends.
		/// </summary>
		public const string LOG_ACTIVITY_END = "Finished {0}: {1} milliseconds.";

		/// <summary>
		/// Currency Formatting String
		/// </summary>
		public const string MONEY_FORMATTING = "{0:$#,##0.00}";

		/// <summary>
		/// Percentage Formatting String
		/// </summary>
		public const string PERCENTAGE_FORMATTING = "P";

		/// <summary>
		/// Number with Commas Formatting String
		/// </summary>
		public const string NUMBER_WITH_COMMAS_FORMATTING = "N";

		/// <summary>
		/// Number with Commas Formatting String, no decimal places
		/// </summary>
		public const string NUMBER_WITH_COMMAS_FORMATTING_NO_DECIMAL_PLACES = "N0";

		/// <summary>
		/// Decimal Formatting String
		/// </summary>
		public const string DECIMAL_FORMATTING = "G";

		/// <summary>
		/// Fixed Point Formatting String (to 5 decimal places)
		/// </summary>
		public const string FIXED_POINT_FORMATTING_FIVE_DECIMAL_PLACES = "F5";

		/// <summary>
		/// Date Formatting - MM/dd/yyyy.
		/// </summary>
		public const string DATE_FORMATTING_MONTH_DAY_YEAR = "MM/dd/yyyy";

		/// <summary>
		/// Date Formatting - yyyy-MM-dd.
		/// </summary>
		public const string DATE_FORMATTING_YEAR_MONTH_DAY = "yyyy-MM-dd";

		/// <summary>
		/// The default number of seconds for a TransactionScope timeout
		/// </summary>
		public const int DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT = 60;

		/// <summary>
		/// Used to generate a unique identifier string based on the current system time
		/// </summary>
		public const string UNIQUE_IDENTIFIER_TIMESTAMP_FORMAT = "yyyyMMddHHmmssffff";

		/// <summary>
		/// Cost resource rate and spread decimal precision when tracking dollars and cents. Valid 
		/// precisions for cost are 0 and 2.
		/// </summary>
		public const int DOLLARS_AND_CENTS_PRECISION = 2;

		/// <summary>
		/// Timeout for Regex matching
		/// 
		/// It is set to 5 minutes.. This is not meant as a performance benchmark, just as a fail-safe to prevent the application from locking up IIS
		/// </summary>
		public static readonly TimeSpan REGEX_TIMEOUT = new(0, 5, 0);

		/// <summary>
		/// Temporary directory for files
		/// </summary>
		public static readonly string TEMP_DIRECTORY = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DeleteMe");

		/// <summary>
		/// Total price for a Burden Cost
		/// </summary>
		public const string TOTAL_PRICE = "Total Prc";

		/// <summary>
		/// Total Cost for a Burden Cost
		/// </summary>
		public const string TOTAL_COST = "Total Cst";

		/// <summary>
		/// Fee/Profit for a Burden Cost
		/// </summary>
		public const string FEE_PROFIT = "Fee/Prft";

		/// <summary>
		/// Clin name inside a Summary Field
		/// </summary>
		public const string CLIN = "CLIN";

		/// <summary>
		/// Clin number inside a Summary Field
		/// </summary>
		public const string CLIN_NUMBER = "CLIN #";

		/// <summary>
		/// Clin Description inside a Summary Field
		/// </summary>
		public const string CLIN_DESC = "CLIN DESC";

		/// <summary>
		/// Clin Title inside a Summary Field
		/// </summary>
		public const string CLIN_TITLE = "CLIN TITLE";

		/// <summary>
		/// The IES Token Scheme name
		/// </summary>
		public const string IES_TOKEN_SCHEME = "IES_TOKEN";

		/// <summary>
		/// Report Type Word
		/// </summary>
		public static readonly string REPORT_TYPE_WORD = "Word";

		/// <summary>
		/// Report Type Pdf
		/// </summary>
		public static readonly string REPORT_TYPE_PDF = "Pdf";

		/// <summary>
		/// Report Type Excel
		/// </summary>
		public static readonly string REPORT_TYPE_EXCEL = "Excel";
	}
}