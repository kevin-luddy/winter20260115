// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;

	/// <summary>
	/// Interface for when BOE needs validation
	/// </summary>
	public interface IValidateWorkspaceDataLoader
	{
		/// <summary>
		/// Check the Workspace and the children to see if they all fall within the PoP
		/// </summary>
		/// <param name="workspaceID">Workspace Id</param>
		/// <param name="contractStartDate">Workspace Start Date</param>
		/// <param name="contractEndDate">Workspace End Date</param>
		/// <returns>isValid boolean</returns>
		bool ValidateWorkspacePoP(int workspaceID, DateTime contractStartDate, DateTime contractEndDate);
	}
}
