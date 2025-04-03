// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Configuration
{
	using System.Configuration;
	using IES.Common.Core.Enums;
	using Microsoft.Extensions.Configuration;

	/// <summary>
	/// Singleton that stores system configurations from web.config
	/// </summary>
	public sealed class SystemConfiguration
	{
		#region Private Members

		// static instance of this object
		static volatile private SystemConfiguration _UniqueInstance;

		// static lock object used to make methods thread safe
		private static readonly object mLock = new();

		#endregion Private Members

		#region private methods

		/// <summary>
		/// Default private constructor due to singleton class instance
		/// </summary>
		private SystemConfiguration()
		{
			//get the company configuration string if it doesn't contain a valid value set to IS&GS
			// Note: This MUST NOT use the ConfigurationUtilities methods or an infinite loop condition will occur
			string sCompany = System.Configuration.ConfigurationManager.AppSettings["CompanyConfiguration"];
			if (string.IsNullOrEmpty(sCompany))
			{
				// try to pull from BuildConfiguration first
				if (BuildConfiguration != null)
				{
					sCompany = BuildConfiguration["CompanyConfiguration"];
				}

				// try to pull from appsettings.json
				if (string.IsNullOrEmpty(sCompany) && ApplicationConfigurationBase.Configuration != null)
				{
					sCompany = ApplicationConfigurationBase.Configuration["CompanyConfiguration"];
				}
			}

			CompanyMode = string.IsNullOrEmpty(sCompany) ? CompanyConfiguration.ISGS : sCompany.GetEnumeratedValue<CompanyConfiguration>(CompanyConfiguration.ISGS);
			CompanyConfigurationSettings = SystemConfigurationSection.Section[CompanyMode];
		}

		#endregion private methods

		/// <summary>
		/// gets Company Configuration
		/// </summary>
		public CompanyConfiguration CompanyMode
		{
			get; internal set // for unit testing only
													 ;
		}

		/// <summary>
		/// Company-specific configuration override settings
		/// </summary>
		public CompanyConfigurationSection CompanyConfigurationSettings { get; }

		public static IConfigurationManager BuildConfiguration { get; set; }

		#region public methods

		/// <summary>
		/// The instance of the singleton
		/// </summary>
		/// <returns>the instance of the singleton</returns>
		public static SystemConfiguration Instance()
		{
			// double lock in case many threads hit first if check and then blocked on lock ... they will be filtered out on second if check
			if (_UniqueInstance == null)
			{
				lock (mLock)
				{
					if (_UniqueInstance == null)
					{
						// get singleton reference
						_UniqueInstance = new SystemConfiguration();

					}
				}
			}

			return _UniqueInstance;
		}


		#endregion public methods

	}
}
