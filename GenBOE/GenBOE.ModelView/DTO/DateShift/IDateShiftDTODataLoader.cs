// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using IES.Common;
	using System.Collections.Generic;

	/// <summary>
	/// Interface for the Date Shift loader.
	/// </summary>
	public interface IDateShiftDTODataLoader : IDataLoader<DateShiftDTO>
	{
		/// <summary>
		/// Gets the Date Shift object by level and ID.
		/// </summary>
		/// <param name="level">Level.</param>
		/// <param name="id">Id.</param>
		/// <returns>Date Shift object</returns>
		DateShiftDTO GetDateShiftObject(Level level, int id);

		///// <summary>
		///// Gets workspace by short name.
		///// </summary>
		///// <param name="workspaceShortName">Ws shortname.</param>
		///// <returns>Date shifto bject.</returns>
		//DateShiftDTO GetWorkspaceDateShiftDataObject(string workspaceShortName);

		/// <summary>
		/// Update the respective levels in the database with the Date Shift data.
		/// </summary>
		/// <param name="dateShiftDTOs">Collection of Date Shift dates for updating.</param>
		void Update(ICollection<DateShiftDTO> dateShiftDTOs);
	}
}
