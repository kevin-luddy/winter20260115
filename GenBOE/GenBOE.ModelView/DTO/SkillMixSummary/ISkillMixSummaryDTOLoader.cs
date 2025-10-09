// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO.SkillMixSummary
{
	using GenBOE.Dtos;
	using System.Collections.Generic;

	public interface ISkillMixSummaryDTOLoader
	{
		/// <summary>
		/// Get all Skill Mix Summary values by BOE Task Element FK ID
		/// </summary>
		/// <param name="boeTaskElementID">BOE Task Element ID</param>
		/// <returns>List of Skill Mix Summaries</returns>
		ICollection<SkillMixSummaryDTO> GetByBOETaskElementID(int boeTaskElementID);

		/// <summary>
		/// Get all Skill Mix Summary values by Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>List of Skill Mix Summaries</returns>
		ICollection<SkillMixSummaryDTO> GetByWorkspaceId(int workspaceId);

		/// <summary>
		/// Get all Skill Mix Summary values by BOE FK IDs
		/// </summary>
		/// <param name="boeIDs">BOE Ids</param>
		/// <returns>List of Skill Mix Summaries</returns>
		ICollection<SkillMixSummaryDTO> GetByBOEIDs(ICollection<int> boeIDs);

		/// <summary>
		/// Get Skill Mix Summary by certain values
		/// </summary>
		/// <param name="skillMixSummaryIDs">IDs of skill mix summaries to retrieve</param>
		/// <returns>List of Skill Mix Summaries</returns>
		ICollection<SkillMixSummaryDTO> GetByIds(ICollection<int> skillMixSummaryIDs);

		/// <summary>
		/// Get Skill Mix Summary value by a specific primary key
		/// </summary>
		/// <param name="skillMixSummaryID">ID of skill mix summary to retrieve</param>
		/// <returns>Skill Mix Summary</returns>
		SkillMixSummaryDTO GetById(int skillMixSummaryID);

		/// <summary>
		/// Delete Skill Mix Summary by BOE Task Element ID
		/// </summary>
		/// <param name="boeTaskElementID">BOE Task Element ID</param>
		/// <returns>Number of Skill Mix Summaries deleted</returns>
		int? DeleteSkillMixSummaryByBOETaskElementID(int boeTaskElementID);

		/// <summary>
		/// Insert Skill Mix Summary with Kill/Fill procedure
		/// </summary>
		/// <param name="SkillMixSummaryDTO">SkillMixSummaryDTO to insert</param>
		/// <returns>Num rows that were inserted</returns>
		int? InsertSkillMixSummary(ICollection<SkillMixSummaryDTO> skillMixSummaries);
	}
}
