// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using System.Collections.Generic;

	public interface ICommonDisclosureSMDTODataLoader
	{
		/// <summary>
		/// Get all Common disclosure Skill Mix values by MOQTypeSelection FK ID
		/// </summary>
		/// <param name="moqTypeSelectionID"></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		ICollection<CommonDisclosureSkillMixDTO> GetByMOQTypeSelectionID(int moqTypeSelectionID);

		/// <summary>
		/// Get all Common disclosure Skill Mix values by Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		ICollection<CommonDisclosureSkillMixDTO> GetByWorkspaceId(int workspaceId);

		/// <summary>
		/// Get all Common disclosure Skill Mix values by BOE FK ID
		/// </summary>
		/// <param name="boeID"></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		ICollection<CommonDisclosureSkillMixDTO> GetByBOEID(int boeID);

		/// <summary>
		/// Get all Common disclosure Skill Mix values by BOETaskElement FK ID
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		ICollection<CommonDisclosureSkillMixDTO> GetByBOETaskElementID(int boeTaskElementID);

		/// <summary>
		/// Get Common disclosure Skill Mix by certain values
		/// </summary>
		/// <param name="commonDisclosureSkillMixIDs"></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		ICollection<CommonDisclosureSkillMixDTO> GetByIds(ICollection<int> commonDisclosureSkillMixIDs);

		/// <summary>
		/// Get Common disclosure Skill Mix value by a specific primary key
		/// </summary>
		/// <param name="commonDisclosureSkillMixID"></param>
		/// <returns>Common Disclosure Skill Mix</returns>
		CommonDisclosureSkillMixDTO GetById(int commonDisclosureSkillMixID);

		/// <summary>
		/// Delete Common disclosure Skill Mix by MOQ Type Selection ID
		/// </summary>
		/// <param name="moqTypeSelectionID"></param>
		/// <returns>Number of Common Disclosure Skill Mixes deleted</returns>
		int? DeleteCommonDisclosureSMByMoqTypeSelection(int moqTypeSelectionID);

		/// <summary>
		/// Insert Common disclosure Skill Mix with Kill/Fill procedure
		/// </summary>
		/// <param name="CommonDisclosureSkillMixDTO">SkillMixDTO</param>
		/// <returns>Num rows that were inserted</returns>
		int? InsertCommonDisclosureSM(ICollection<CommonDisclosureSkillMixDTO> commonDisclosureSkillMixes);
	}
}
