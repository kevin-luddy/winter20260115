// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public interface ISkillMixDTOLoader
	{
		/// <summary>
		/// Get all SkillMix values by MOQTypeSelection FK ID
		/// </summary>
		/// <param name="moqTypeSelectionID"></param>
		/// <returns></returns>
		ICollection<SkillMixDTO> GetByMOQTypeSelectionID(int moqTypeSelectionID);

		/// <summary>
		/// Get all SkillMix values by BOE FK ID
		/// </summary>
		/// <param name="boeID"></param>
		/// <returns></returns>
		ICollection<SkillMixDTO> GetByBOEID(int boeID);

		/// <summary>
		/// Get all SkillMix values by BOETaskElement FK ID
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns></returns>
		ICollection<SkillMixDTO> GetByBOETaskElementID(int boeTaskElementID);

		/// <summary>
		/// Get SkillMix by certain values
		/// </summary>
		/// <param name="skillMixIDs"></param>
		/// <returns></returns>
		ICollection<SkillMixDTO> GetByIds(ICollection<int> skillMixIDs);

		/// <summary>
		/// Get SkillMix value by a specific primary key
		/// </summary>
		/// <param name="skillMixID"></param>
		/// <returns></returns>
		SkillMixDTO GetById(int skillMixID);
	}
}
