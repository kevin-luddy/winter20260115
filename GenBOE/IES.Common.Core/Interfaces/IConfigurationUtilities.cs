namespace IES.Common.Core.Interfaces
{
	using System;

	interface IConfigurationUtilities
	{
		/// <summary>
		/// Retrieve a string value from AppSettings
		/// </summary>
		/// <param name="key">AppSettings key</param>
		/// <returns>The application setting</returns>
		string GetAppSetting(string key);

		/// <summary>
		/// Retrieve a typed value from AppSettings
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="key">AppSettings key</param>
		/// <returns>The application setting, as a typed value</returns>
		T GetAppSetting<T>(string key) where T : struct, IConvertible;

		/// <summary>
		/// Retrieve a typed value from AppSettings
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="key">AppSettings key</param>
		/// <param name="defaultValue">Value to use if the setting does not exist</param>
		/// <returns>The application setting, as a typed value</returns>
		T GetAppSetting<T>(string key, T defaultValue) where T : struct, IConvertible;
	}
}
