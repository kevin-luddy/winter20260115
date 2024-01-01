// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DataBridge.Loaders;
	using DataBridge.ModelViews;
	using IES.Standard;
	using Newtonsoft.Json;

	/// <summary>
	/// Rate formatting information.
	/// </summary>
	public static class RateFormatter
    {
        /// <summary>
        /// Lookup table for PPR&amp;D and Rates format specifications.
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> formats;

        /// <summary>
        /// The lock object
        /// </summary>
        private static object lockObject = new object();

        /// <summary>
        /// Default rate category
        /// </summary>
        internal const string RATE_CATEGORY_NONE = "None";

        /// <summary>
        /// prefix key for rate formater dictionary
        /// </summary>
        internal const string PREFIX = "prefix";

        /// <summary>
        /// suffix key for rate formater dictionary
        /// </summary>
        internal const string SUFFIX = "suffix";

        /// <summary>
        /// precision key for rate formater dictionary
        /// </summary>
        internal const string PRECISION = "precision";

        /// <summary>
        /// multiplier key for rate formater dictionary
        /// </summary>
        internal const string MULTIPLIER = "multiplier";

        /// <summary>
        /// Rate format definition.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Dictionary<string, Dictionary<string, string>> Formats
        {
            get
            {
                if (formats == null)
                {
                    lock (lockObject)
                    {
                        if (formats == null)
                        {
                            Dictionary<string, Dictionary<string, string>> tempFormats = new Dictionary<string, Dictionary<string, string>>();
                            // Load format settings from DB.
                            IRateConfigLoader rateConfigLoader = GenBOEUnityContainer.Resolve<IRateConfigLoader>();
                            ICollection<RateConfigModelView> rateConfigs = rateConfigLoader.GetAll();
                            foreach (RateConfigModelView rateConfig in rateConfigs)
                            {
                                string category = rateConfig.RateCategory == null ? RATE_CATEGORY_NONE : rateConfig.RateCategory.GetDescription();
                                string formatKey = rateConfig.RateTarget + category;
                                tempFormats[formatKey] = new Dictionary<string, string>();
                                tempFormats[formatKey][PREFIX] = rateConfig.Prefix ?? string.Empty;
                                tempFormats[formatKey][SUFFIX] = rateConfig.Suffix ?? string.Empty;
                                tempFormats[formatKey][PRECISION] = rateConfig.Precision.ToString();
                                tempFormats[formatKey][MULTIPLIER] = rateConfig.Multiplier.HasValue ? rateConfig.Multiplier.ToString() : string.Empty;
                            }

                            formats = tempFormats;
                        }
                    }
                }

                return formats;
            }
        }

        /// <summary>
        /// Returns all the formats in JSON for client.
        /// </summary>
        /// <returns>All formats as JSON</returns>
        public static string JSONFormats()
        {
            return JsonConvert.SerializeObject(Formats);
        }

        /// <summary>
        /// Return the RateFormat precision for the Target and RateCategory.
        /// </summary>
        /// <param name="target">target, i.e. PPR&amp;D or Rate</param>
        /// <param name="categoryDescription">rate category description, e.g. "Direct Labor"</param>
        /// <returns>Rate precision</returns>
        public static int GetRatePrecision(RateTarget target, string categoryDescription)
        {
            string key = target.GetDescription() + categoryDescription;
            return Convert.ToInt32(Formats[key][PRECISION]);
        }

        /// <summary>
        /// Gets the default rate precision.
        /// </summary>
        /// <returns>Default rate precision</returns>
        public static string DefaultRatePrecision()
        {
            string key = RateTarget.Rate.GetDescription() + RATE_CATEGORY_NONE;
            return Formats[key][PRECISION];
        }

        /// <summary>
        /// Formats a rate value with precision and prefix/suffix
        /// </summary>
        /// <param name="target">target, i.e. PPR&amp;D or Rate</param>
        /// <param name="categoryDescription">rate category description, e.g. "Direct Labor"</param>
        /// <param name="rate">rate value</param>
        /// <returns>formatted string</returns>
        public static string FormatRate(RateTarget target, string categoryDescription, decimal? rate)
        {
            if (!rate.HasValue)
            {
                return target == RateTarget.PPRD ? Constants.NOT_APPLICABLE : string.Empty;
            }

            string key = target.GetDescription() + categoryDescription;
            KeyValuePair<string, Dictionary<string, string>> formatKeyValuePair = Formats.FirstOrDefault(x => x.Key == key);

            // if not found in dictionary, throw an exception
            if (formatKeyValuePair.Equals(default(KeyValuePair<string, Dictionary<string, string>>)))
            {
                throw new ArgumentOutOfRangeException(categoryDescription, 
                    string.Format("Unable to format rate value for Category '{0}'. Rate format is not defined in the Rate Configuration table.", categoryDescription));
            }

            Dictionary<string, string> format = formatKeyValuePair.Value;

            string prefix = format[PREFIX];
            string suffix = format[SUFFIX];

            int precision;
            int.TryParse(format[PRECISION], out precision);

            int multiplier;
            if (!int.TryParse(format[MULTIPLIER], out multiplier))
            {
                multiplier = 1;
            }

            // Adjust precision based on the multiplier (ex. subtract 2 if multiplying by 100)
            int newPrecision = (int)(precision - Math.Log10(multiplier));

            // Get value, apply multiplier, and adjust it with precision
            decimal valueWithPrecision = decimal.Round(rate.Value * multiplier, newPrecision, MidpointRounding.AwayFromZero);

            // Generate format string to remove trailing zeros (except for currency amounts where prefix is "$" and precision is 2).
            char padChar = prefix.Equals("$") && precision == 2 ? '0' : '#';
            string formatStr = "#0." + string.Empty.PadLeft(newPrecision, padChar);

            return prefix + valueWithPrecision.ToString(formatStr) + suffix;
        }
    }
}