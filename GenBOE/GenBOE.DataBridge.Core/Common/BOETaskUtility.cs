// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Common
{
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.ModelView;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Class utilized to run some general validation on a BOE task.
	/// </summary>
	public static class BOETaskUtility
	{
		/// <summary>
		/// Helper method to decide if Skill Mix should be shown for a task.
		/// </summary>
		/// <param name="workspaceCreationDate">Workspace creation date.</param>
		/// <param name="hasTMRates">If the task contains any used T&M rates.</param>
		/// <param name="moqTypeSelections">Moq type selections.</param>
		/// <returns>To show skill mix for a task.</returns>
		public static bool ShowSkillMixForTask(DateTime? workspaceCreationDate, bool hasTMRates, ICollection<MoqTypeSelection> moqTypeSelections)
		{
			bool showSkillMixRationale = false;

			// Skill mix will be disabled if there is not exactly one MOQ Type selected.
			if (moqTypeSelections.Count() == 1)
			{
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
				{
					showSkillMixRationale = CommonUtilities.ShowSkillMixForWorkspace(workspaceCreationDate);
				}
				else if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
				{
					bool hasSapWebi = moqTypeSelections.Any(x => x.TableData.Any(y => y.RepositoryName == RepositoryName.SapWebi.GetDescription()));

					showSkillMixRationale = CommonUtilities.ShowSkillMixForWorkspace(workspaceCreationDate) && !hasTMRates && hasSapWebi &&
						(moqTypeSelections.First().SelectedMOQType == MOQType.Comparative ||
						moqTypeSelections.First().SelectedMOQType == MOQType.Historical ||
						moqTypeSelections.First().SelectedMOQType == MOQType.AnalogousRelationships);

				}
			}

			return showSkillMixRationale;
		}

		/// <summary>
		/// Is Skill Mix connection shown to the user for this task
		/// </summary>
		/// <param name="workspaceCreationDate">Workspace creation date</param>
		/// <param name="workspaceUsingTemplateBOE">Is the Workspace using template BOEs</param>
		/// <param name="workspaceEnableSAPConnection">Does the workspace have SAP connection enabled</param>
		/// <param name="moqTypeSelections">Workspace MOQType selections</param>
		/// <param name="hasTMRates">Has T&amp;M Rates</param>
		/// <param name="task">The Task</param>
		/// <returns>Option to show skill mix for task.</returns>
		public static bool ShowSkillMixForTask(DateTime? workspaceCreationDate, bool workspaceUsingTemplateBOE, bool workspaceEnableSAPConnection,
			IEnumerable<MoqTypeSelection> moqTypeSelections, int boeTaskElementId, bool hasTMRates)
		{
			bool showSkillMixRationale = false;

			if (CommonUtilities.ShowSkillMixForWorkspace(workspaceCreationDate))
			{
				ICollection<MoqTypeSelection> moqTypes = moqTypeSelections.Where(m => m.TaskId == boeTaskElementId).ToList();
				// Only show SkillMix if there is 1 and only 1 MOQ Type
				// And using Template BOE
				if (moqTypes != null && moqTypes.Count == 1 && workspaceUsingTemplateBOE)
				{
					MoqTypeSelection moqType = moqTypes.First();
					if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
					{
						if (moqType.SelectedMOQType == MOQType.Comparative || moqType.SelectedMOQType == MOQType.Historical)
						{
							showSkillMixRationale = true;
						}
					}
					// For space only: Shows Skill Mix Rationale section when the workspace is NOT using T&M.
					else if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
					{
						// Space requires SAP Connection
						if (workspaceEnableSAPConnection)
						{
							if (moqType.SelectedMOQType == MOQType.Comparative || moqType.SelectedMOQType == MOQType.Historical || moqType.SelectedMOQType == MOQType.AnalogousRelationships)
							{
								// At least one MOQ Table needs to be connected to SAP Webi for a task
								if (moqType.TableData != null && moqType.TableData.Any(t => t.RepositoryName == RepositoryName.SapWebi.GetDescription()))
								{
									showSkillMixRationale = !hasTMRates;
								}
							}
						}
					}
				}
			}

			return showSkillMixRationale;
		}
	}
}