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
		/// Get all SkillMix values by BOE Task Element FK ID
		/// </summary>
		/// <param name="boeTaskElementID">BOE Task Element ID</param>
		/// <returns>List of SkillMix</returns>
		ICollection<SkillMixDTO> GetByBOETaskElementID(int boeTaskElementID);

		/// <summary>
		/// Get all SkillMix values by BOE FK IDs
		/// </summary>
		/// <param name="boeIDs"></param>
		/// <returns>List of SkillMix</returns>
		ICollection<SkillMixDTO> GetByBOEIDs(ICollection<int> boeIDs);

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
		/// Delete Skill Mix by BOE Task Element ID
		/// </summary>
		/// <param name="boeTaskElementID">BOE Task Element ID.</param>
		/// <returns>Number of SkillMixes deleted</returns>
		int? DeleteSkillMixByBOETaskElementID(int boeTaskElementID);

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
