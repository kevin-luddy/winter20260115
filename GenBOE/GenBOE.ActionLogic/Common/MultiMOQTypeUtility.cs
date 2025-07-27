// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;

	/// <summary>
	/// Utility class used for checking whether a workspace or task has multiple MOQ Types (Space only)
	/// </summary>
	public static class MultiMOQTypeUtility
	{
		/// <summary>
		/// Space only - will check the current BOE for multiple MOQ Types for a single task, and if any of those have the following MOQ Types
		/// </summary>
		/// <param name="fullBoe">The full BOE</param>
		/// <param name="workspaceCreationDate">Workspace creation date</param>
		/// <param name="workspaceShortname">Workspace short name</param>
		/// <returns>Whether or not the task has multiple MOQ Types, and what tasks if so</returns>
		public static MultiMOQTypeResult DoTasksHaveMultipleMOQTypes(FullBoe fullBoe, DateTime? workspaceCreationDate, 
			string workspaceShortname)
		{
			_ = fullBoe ?? throw new ArgumentNullException(nameof(fullBoe));
			MultiMOQTypeResult result = new MultiMOQTypeResult();

			if (Utilities.ShowUCOTForWorkspace(workspaceCreationDate, workspaceShortname) && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				foreach (BoeTaskElementDTO task in fullBoe.TaskElements)
				{
					ICollection<MoqTypeSelection> moqTypeSelectionsForTask = fullBoe.MoqTypeSelections.Where(x => x.TaskId == task.Id).ToList();
					bool doesSpecifiedMoqTypeExist = moqTypeSelectionsForTask.Any(x => x.SelectedMOQType == MOQType.Comparative || x.SelectedMOQType == MOQType.Historical
						|| x.SelectedMOQType == MOQType.AnalogousRelationships);
					if (moqTypeSelectionsForTask.Count > 1 && doesSpecifiedMoqTypeExist)
					{
						result.Tasks.Add(task.TaskTitle);
					}
				}
			}

			return result;
		}
	}
}
