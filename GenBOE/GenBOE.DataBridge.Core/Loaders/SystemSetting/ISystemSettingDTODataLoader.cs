// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Loaders.SystemSetting
{
	using System.Collections.Generic;
	using GenBOE.DataBridge.Core.DTO;

	public interface ISystemSettingDTODataLoader
	{
		/// <summary>
		/// Get all system setting DTOs
		/// </summary>
		/// <returns>All system settings</returns>
		ICollection<SystemSettingDTO> GetSystemSettings();

		/// <summary>
		/// Get the system setting DTO
		/// </summary>
		/// <param name="key">system setting key to retrieve</param>
		/// <returns>system setting value</returns>
		SystemSettingDTO GetSystemSetting(string key);

		/// <summary>
		/// Save the System Setting
		/// </summary>
		/// <param name="systemSetting">system setting to save</param>
		string SaveSystemSetting(SystemSettingDTO systemSetting);

		/// <summary>
		/// Clear (delete) the system setting
		/// </summary>
		/// <param name="key">system setting key to clear</param>
		void ClearSystemSetting(string key);

		/// <summary>
		/// Get Skill Mix setting DTOs only
		/// </summary>
		/// <returns>All Skill Mix settings</returns>
		ICollection<SystemSettingDTO> GetSkillMixSettings();
	}
}
