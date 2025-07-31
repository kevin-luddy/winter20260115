// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO.SkillMix
{
	using GenBOE.Dtos;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public interface ISkillMixSettingDTODataLoader
	{
		/// <summary>
		/// Get all Skill Mix setting DTOs
		/// </summary>
		/// <returns>All system settings</returns>
		ICollection<SystemSettingDTO> GetSkillMixSettings();

		/// <summary>
		/// Get the Skill Mix setting DTO
		/// </summary>
		/// <param name="key">system setting key to retrieve</param>
		/// <returns>system setting value</returns>
		SystemSettingDTO GetSkillMixSetting(string key);

		/// <summary>
		/// Save the Skill Mix Setting
		/// </summary>
		/// <param name="systemSetting">system setting to save</param>
		string SaveSkillMixSetting(SystemSettingDTO systemSetting);
	}
}
