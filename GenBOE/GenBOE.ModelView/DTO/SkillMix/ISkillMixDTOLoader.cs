// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using System.Collections.Generic;

	public interface ISkillMixDTOLoader
	{
		/// <summary>
		/// Get all SkillMix values by MOQTypeSelection FK ID
		/// </summary>
		/// <param name="moqTypeSelectionID"></param>
		/// <returns>List of SkillMix</returns>
		ICollection<SkillMixDTO> GetByMOQTypeSelectionID(int moqTypeSelectionID);

		/// <summary>
		/// Get all SkillMix values by BOE FK ID
		/// </summary>
		/// <param name="boeID"></param>
		/// <returns>List of SkillMix</returns>
		ICollection<SkillMixDTO> GetByBOEID(int boeID);

		/// <summary>
		/// Get all SkillMix values by BOETaskElement FK ID
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns>List of SkillMix</returns>
		ICollection<SkillMixDTO> GetByBOETaskElementID(int boeTaskElementID);

		/// <summary>
		/// Get SkillMix by certain values
		/// </summary>
		/// <param name="skillMixIDs"></param>
		/// <returns>List of SkillMix</returns>
		ICollection<SkillMixDTO> GetByIds(ICollection<int> skillMixIDs);

		/// <summary>
		/// Get SkillMix value by a specific primary key
		/// </summary>
		/// <param name="skillMixID"></param>
		/// <returns>SkillMix</returns>
		SkillMixDTO GetById(int skillMixID);

		/// <summary>
		/// Delete Skill Mix by MOQ Type Selection ID
		/// </summary>
		/// <param name="moqTypeSelectionID"></param>
		/// <returns>Number of SkillMixes deleted</returns>
		int? DeleteSkillMixByMoqTypeSelection(int moqTypeSelectionID);

		/// <summary>
		/// Insert Skill Mix with Kill/Fill procedure
		/// </summary>
		/// <param name="skillMixes">SkillMixDTO</param>
		/// <returns>Num rows that were inserted</returns>
		int? InsertSkillMix(ICollection<SkillMixDTO> skillMixes);

		/// <summary>
		/// Get all Skill Mix values by Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>List of Skill Mix rows</returns>
		ICollection<SkillMixDTO> GetByWorkspaceId(int workspaceId);
	}
}
