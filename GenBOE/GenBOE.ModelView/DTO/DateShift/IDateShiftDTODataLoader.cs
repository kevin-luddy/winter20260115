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
		/// Update the respective levels in the database with the Date Shift data.
		/// </summary>
		/// <param name="dateShiftDTOs">Collection of Date Shift dates for updating.</param>
		void Update(ICollection<DateShiftDTO> dateShiftDTOs);
	}
}
