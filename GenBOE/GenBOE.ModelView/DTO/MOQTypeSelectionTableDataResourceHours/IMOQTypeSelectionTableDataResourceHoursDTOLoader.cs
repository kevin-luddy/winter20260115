// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using System.Collections.Generic;

	public interface IMOQTypeSelectionTableDataResourceHoursDTOLoader
	{
		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by MOQTypeSelectionTableDataId FK Id.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataId"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByMOQTypeSelectionTableDataId(int moqTypeSelectionTableDataId);

		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by BOE FK ID.
		/// </summary>
		/// <param name="boeID"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByBOEID(int boeID);

		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by BOETaskElement FK ID.
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByBOETaskElementID(int boeTaskElementID);

		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByWorkspaceId(int workspaceId);

		/// <summary>
		/// Get MOQ Type Selection Table Data Resource Hours by Ids.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataResourceHoursIds"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByIds(ICollection<int> moqTypeSelectionTableDataResourceHoursIds);

		/// <summary>
		/// Get MOQ Type Selection Table Data Resource Hours value by a specific primary key.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataResourceHoursId"></param>
		/// <returns>MOQ Type Selection Table Data Resource Hours</returns>
		MOQTypeSelectionTableDataResourceHoursDTO GetById(int moqTypeSelectionTableDataResourceHoursId);

		/// <summary>
		/// Delete MOQ Type Selection Table Data Resource Hours by MOQ Type Selection ID.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataId"></param>
		/// <returns>Number of MOQ Type Selection Table Data Resource Hours deleted</returns>
		int? DeleteByMoqTypeSelectionTableDataId(int moqTypeSelectionTableDataId);

		/// <summary>
		/// Insert MOQ Type Selection Table Data Resource Hours with Kill/Fill procedure
		/// </summary>
		/// <param name="moqTypeSelectionTableDataResourceHours">MOQ Type Selection Table Data Resource Hours DTOs</param>
		/// <returns>Num rows that were inserted</returns>
		int? InsertMOQTypeSelectionTableDataResourceHours(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> moqTypeSelectionTableDataResourceHours);
	}
}
