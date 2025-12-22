// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;
	using System.Linq;
	using GenBOE.Models;
	using IES.Common;

	/// <summary>
	/// BOE Validation Stored Proc
	/// </summary>
	public class ValidateWorkspaceDataLoader : IValidateWorkspaceDataLoader
	{
		/// <summary>
		/// Check the Workspace and the children to see if they all fall within the PoP
		/// </summary>
		/// <param name="workspaceID">Workspace Id</param>
		/// <param name="contractStartDate">Workspace Start Date</param>
		/// <param name="contractEndDate">Workspace End Date</param>
		/// <returns>isValid boolean</returns>
		[DbQuery]
		public bool ValidateWorkspacePoP(int workspaceID, DateTime contractStartDate, DateTime contractEndDate)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				bool? sprocResult = gbe.validateWorkspacePoP(workspaceID, contractStartDate, contractEndDate).FirstOrDefault();
				return sprocResult != null && (bool)sprocResult;
			}
		}
	}
}
