// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Configuration
{
	using System;
	using System.Configuration;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Utilities for accessing configuration settings
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class ConfigurationUtilities
	{
		#region AppSettings

		/// <summary>
		/// Retrieve a typed value from the application settings.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="key">Settings key</param>
		/// <returns>The application setting, as a typed value</returns>
		public static T GetAppSetting<T>(string key) where T : struct, IConvertible
		{
			return GetAppSetting(key, default(T));
		}

		/// <summary>
		/// Retrieve a string value from the application settings.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
		/// </summary>
		/// <param name="key">Settings key</param>
		/// <returns>The application setting</returns>
		public static string GetAppSetting(string key)
		{
			string value = SystemConfiguration.Instance().CompanyConfigurationSettings.AppSettings[key];

			if (string.IsNullOrEmpty(value))
			{
				// try to pull from appsettings.json
				value = ApplicationConfigurationBase.Configuration[key];
				if (string.IsNullOrEmpty(value))
				{
					// try to pull from app/web.config
					value = ConfigurationManager.AppSettings[key];
				}
			}

			return value;
		}

		/// <summary>
		/// Retrieve a typed value from the application settings.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="key">Settings key</param>
		/// <param name="defaultValue">Value to use if the setting does not exist</param>
		/// <returns>The application setting, as a typed value</returns>
		public static T GetAppSetting<T>(string key, T defaultValue) where T : IConvertible
		{
			T value = defaultValue;

			string stringValue = GetAppSetting(key);

			if (!string.IsNullOrEmpty(stringValue))
			{
				Type typeT = typeof(T);

				try
				{
					if (typeT.IsEnum)
					{
						value = (T)Enum.Parse(typeT, stringValue);
					}
					else
					{
						value = (T)Convert.ChangeType(stringValue, typeT);
					}
				}
				catch (InvalidCastException)
				{
					value = defaultValue;
				}
				catch (FormatException)
				{
					value = defaultValue;
				}
				catch (OverflowException)
				{
					value = defaultValue;
				}
			}

			return value;
		}

		#endregion

		#region ConnectionStrings

		/// <summary>
		/// Retrieve connection string settings from the web configuration.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
		/// </summary>
		/// <param name="key">Connection string key</param>
		/// <returns>The connection string settings</returns>
		public static ConnectionStringSettings GetConnectionString(string key)
		{
			ConnectionStringSettings value = SystemConfiguration.Instance().CompanyConfigurationSettings.ConnectionStrings[key];

			if (value == null)
			{
				value = ConfigurationManager.ConnectionStrings[key];
			}

			return value;
		}

		#endregion
	}
}
