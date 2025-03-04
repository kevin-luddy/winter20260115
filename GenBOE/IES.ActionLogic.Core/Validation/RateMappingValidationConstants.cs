// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.Validation
{
	using System.Collections.ObjectModel;
	using System.Collections.Generic;

	/// <summary>
	/// Validation constants for Rate Mappings
	/// </summary>
	public static class RateMappingValidationConstants
	{
		/// <summary>
		/// No Commerical Burden Pool Selected, with Government Burden Pool selected
		/// </summary>
		public const string RATEMAPPING_COMMERCIAL_BURDENPOOL_REQUIRED = "Commercial Burden Pool is required because a Government Burden Pool was selected for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// No Government Burden Pool Selected, with Commerical Burden Pool selected
		/// </summary>
		public const string RATEMAPPING_GOVERNMENT_BURDENPOOL_REQUIRED = "Government Burden Pool is required because a Commercial Burden Pool was selected for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// No Commercial or Government Burden Pool selected and not in list of rates allowed to have both blank
		/// </summary>
		public const string RATEMAPPING_COMMERCIAL_AND_GOVERNMENT_BURDENPOOL_REQUIRED = "Commercial Burden Pool and Government Burden Pool are required for ProPricer Direct Rate Mapping. Can only be blank for Resources ******NL, Mileage, NLBESCCH, Travel Escalation, Travel Factor, or TRAVLESC";

		/// <summary>
		/// No Rate Type Selected
		/// </summary>
		public const string RATEMAPPING_RATETYPE_REQUIRED = "Rate Type Required for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// No Disclosure Type Selected
		/// </summary>
		public const string RATEMAPPING_DISCLOSURETYPE_REQUIRED = "Disclosure Type Required for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// No Resource Type Selected
		/// </summary>
		public const string RATEMAPPING_RESOURCETYPE_REQUIRED = "Resource Type Required for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// No Additional Descriptions at least one required
		/// </summary>
		public const string RATEMAPPING_ADDITIONAL_DESCRIPTIONS_REQUIRED = "One or more Additional ProPricer Rate Descriptions Required for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// No Description
		/// </summary>
		public const string RATEMAPPING_DESCRIPTION_REQUIRED = "ProPricer Rate Description Required for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// Not allowed to specify both base and extended ProPricer Rate Descriptions
		/// </summary>
		public const string RATEMAPPING_DESCRIPTION_INVALID = "You may not specify both a ProPricer Rate Description and Additional ProPricer Rate Descriptions, only one or the other is allowed for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// Resource Type must be labor when additional ProPricer rate descriptions are specified.
		/// </summary>
		public const string RATEMAPPING_RESOURCETYPE_INVALID = "Resource Type must be Labor when Additional ProPricer Rate Descriptions are specified.";

		/// <summary>
		/// Not allowed to specify both ProPricer Resource Class without a corresponding Rate Description
		/// </summary>
		public const string RATEMAPPING_RESOURCE_CLASS_WITHOUT_RATE_DESCRIPTION = "Each ProPricer Resource Class must have a corresponding ProPricer Rate Description for ProPricer Direct Rate Mapping.";

		/// <summary>
		/// No Rate Code 
		/// </summary>
		public const string RATEDETAILS_RATECODE_MISSING = "There are no existing rates for Rate Code '{0}'.";

		/// <summary>
		/// There should never be duplicate rate codes in a single revision.
		/// This causes a problem during the Rate import since there is no way to determine the proper precision for the specified rate code.
		/// </summary>
		public const string RATEDETAILS_DUPLICATE_RATECODE = "Unable to determine precision for Rate Code '{0}', due to duplicate rate codes in this revision.";

		/// <summary>
		/// There should never be duplicate rate codes in a single revision.
		/// </summary>
		public const string RATEDETAILS_RATECODE_NOT_UNIQUE = "Rate code {0} is not unique.";

		/// <summary>
		/// No Rate Category 
		/// </summary>
		public const string RATEDETAILS_RATECATEGORY_REQUIRED = "Unable to validate rates, no Rate Category available.";

		/// <summary>
		/// No Rate Precision defined 
		/// </summary>
		public const string RATEDETAILS_RATEPRECISION_REQUIRED = "Rate Precision {0} is not defined in the WebConfig file.";

		/// <summary>
		/// Rate Year error 
		/// </summary>
		public const string RATEDETAILS_RATEYEAR_ERROR = "Rate Code: {0}, Year: {1} is invalid. The rate year must be between {2} and {3} inclusive.";

		/// <summary>
		/// Rate Precision error 
		/// </summary>
		public const string RATEDETAILS_RATEPRECISION_ERROR = "Rate Code: {0}, Year: {1}, Value: {2} is invalid. The rate should be a non-negative number with "
															  + "decimal precision of {4} decimal places. This is due to its Rate Category being {3}.";

		/// <summary>
		/// Rate Precision error for rate categories that allow negative values
		/// </summary>
		public const string RATEDETAILS_RATEPRECISION_ERROR_ALLOW_NEGATIVE = "Rate Code: {0}, Year: {1}, Value: {2} is invalid. The rate should be a number with "
															  + "decimal precision of {4} decimal places. This is due to its Rate Category being {3}.";

		/// <summary>
		/// Get Rate Codes allowed to have empty burden pools
		/// </summary>
		public static IReadOnlyCollection<string> ALLOW_EMPTY_BURDEN_POOLS
		{
			get
			{
				return new ReadOnlyCollection<string>(new string[] { "Mileage", "NLBESCCH", "Travel Escalation", "Travel Factor", "TRAVLESC" });
			}
		}
	}
}
