// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;

	/// <summary>
	/// Class utilized to run some general validation on a BOE task.
	/// </summary>
	public static class BOETaskUtility
	{
		/// <summary>
		/// Whether to show Skill Mix for a Specific Task
		/// Note: Should only be used when working with existing data
		/// </summary>
		/// <param name="ws">Workspace</param>
		/// <param name="task">Task</param>
		/// <returns>Option to show skill mix for task.</returns>
		public static bool ShowSkillMixForTask(FullWorkspace ws, BoeTaskElementDTO task)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			return BOETaskUtility.ShowSkillMixForTask(ws.CreationDate, ws.UsingTemplateBOE, ws.EnableSAPConnection, task, ws.MoqTypeSelections,
				ws.ResourcesUsedInWsBoes, ws.TMResourceRatesForWorkspace, ws.Shortname);
		}

		/// <summary>
		/// Is Skill Mix connection shown to the user for this task
		/// </summary>
		/// <param name="workspaceCreationDate">Workspace creation date</param>
		/// <param name="workspaceUsingTemplateBOE">Is the Workspace using template BOEs</param>
		/// <param name="workspaceEnableSAPConnection">Does the workspace have SAP connection enabled</param>
		/// <param name="moqTypeSelections">Workspace MOQType selections</param>
		/// <param name="resourcesUsedInBOEs">Resources used in BOEs</param>
		/// <param name="tmRates">T&amp;M Rates</param>
		/// <param name="task">The Task</param>
		/// <param name="workspaceShortname">The Workspace shortname</param>
		/// <returns>Option to show skill mix for task.</returns>
		public static bool ShowSkillMixForTask(DateTime? workspaceCreationDate, bool workspaceUsingTemplateBOE, bool workspaceEnableSAPConnection, 
			BoeTaskElementDTO task, IReadOnlyCollection<MoqTypeSelection> moqTypeSelections, IReadOnlyCollection<ResourceDTO> resourcesUsedInBOEs, IReadOnlyCollection<TMResourceRateDTO> tmRates,
			string workspaceShortname)
		{
			bool hasTMRates = BOETaskUtility.IsUsingTMRates(task, resourcesUsedInBOEs, tmRates);

			return ShowSkillMixForTask(workspaceCreationDate, workspaceUsingTemplateBOE, workspaceEnableSAPConnection,
				moqTypeSelections, task.Id, hasTMRates, workspaceShortname);
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
		/// <param name="workspaceShortname">Workspace short name</param>
		/// <returns>Option to show skill mix for task.</returns>
		public static bool ShowSkillMixForTask(DateTime? workspaceCreationDate, bool workspaceUsingTemplateBOE, bool workspaceEnableSAPConnection,
			IEnumerable<MoqTypeSelection> moqTypeSelections, int boeTaskElementId, bool hasTMRates, string workspaceShortname)
		{
			bool showSkillMixRationale = false;

			if (Utilities.ShowSkillMixForWorkspace(workspaceCreationDate, workspaceShortname))
			{
				ICollection<MoqTypeSelection> moqTypes = moqTypeSelections.Where(m => m.TaskId == boeTaskElementId).ToList();
				// Only show SkillMix if there is 1 and only 1 MOQ Type
				// And using Template BOE
				if (moqTypes.Count == 1 && workspaceUsingTemplateBOE)
				{
					MoqTypeSelection moqType = moqTypes.First();
					if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
					{
						if (moqType.SelectedMOQType == MOQType.Comparative || moqType.SelectedMOQType == MOQType.Historical)
						{
							showSkillMixRationale = true;
						}
					}
					else if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
					{
						// Space requires SAP Connection
						// Comparative, Historical, or AR MOQ Type
						// And at least one MOQ Table needs to be connected to SAP Webi for a task
						if (workspaceEnableSAPConnection
							&& (moqType.SelectedMOQType == MOQType.Comparative || moqType.SelectedMOQType == MOQType.Historical || moqType.SelectedMOQType == MOQType.AnalogousRelationships) 
							&& moqType.TableData != null && moqType.TableData.Any(t => t.RepositoryName == RepositoryName.SapWebi.GetDescription()))
						{
							// For space only: Shows Skill Mix Rationale section when the workspace is NOT using T&M.
							showSkillMixRationale = !hasTMRates;
						}
					}
				}
			}

			return showSkillMixRationale;
		}

		/// <summary>
		/// Checks the usage of active T&M rates in the task.
		/// </summary>
		/// <param name="ws">Full workspace.</param>
		/// <param name="taskElement">Task element</param>
		/// <returns>If T&M Rates are being used in the Task.</returns>
		public static bool IsUsingTMRates(BoeTaskElementDTO taskElement, IReadOnlyCollection<ResourceDTO> resourcesUsedInBOEs,
			IReadOnlyCollection<TMResourceRateDTO> tmRates)
		{
			if (taskElement == null)
			{
				throw new ArgumentNullException(nameof(taskElement));
			}

			if (resourcesUsedInBOEs == null)
			{
				throw new ArgumentNullException(nameof(resourcesUsedInBOEs));
			}

			if (tmRates == null)
			{
				throw new ArgumentNullException(nameof(tmRates));
			}

			List<int> resourceIds = taskElement.taskElementLabors
				.Where(lt => lt.ResourceID.HasValue)
				.Select(lt => lt.ResourceID.Value)
				.Distinct()
				.ToList();

			List<int> businessResourceCodeIds = taskElement.taskElementLabors
				.Where(lt => lt.BusinessResourceCodeID.HasValue)
				.Select(lt => lt.BusinessResourceCodeID.Value)
				.Distinct()
				.ToList();

			if (!resourceIds.Any() && !businessResourceCodeIds.Any())
			{
				return false;
			}

			HashSet<int> allResourceIds = resourceIds.Concat(businessResourceCodeIds).ToHashSet();

			List<ResourceDTO> resources = resourcesUsedInBOEs
				.Where(r => allResourceIds.Contains(r.Id))
				.ToList();

			return CompareResources(tmRates, resources);
		}

		/// <summary>
		/// Checks the usage of active T&M rates in the task.
		/// </summary>
		/// <param name="tmResourceRates">The T&M resource rates.</param>
		/// <param name="resources">The resources.</param>
		/// <returns>If T&M Rates are being used in the Task.</returns>
		private static bool CompareResources(IReadOnlyCollection<TMResourceRateDTO> tmResourceRates, ICollection<ResourceDTO> resources)
		{
			if (tmResourceRates == null)
			{
				throw new ArgumentNullException(nameof(tmResourceRates));
			}

			if (resources == null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			if (tmResourceRates.Any() && resources.Any())
			{
				foreach (TMResourceRateDTO tmResourceRate in tmResourceRates)
				{
					if (resources.Any(r => r.ResourceName == tmResourceRate.ResourceName))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Refractored Code from Reports Controller => DisplayBOEStatusReport
		/// </summary>
		/// <param name="ws">Full Workspace</param>
		/// <param name="boes">Full BOEs</param>
		/// <param name="tasks">Task Elements</param>
		public static void GetBOEAndTaskDataForWorkspace(FullWorkspace ws, out List<FullBoe> boes, out List<BoeTaskElementDTO> tasks)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			// Call the BL to generate the status report
			// All BOEs for the workspace as a default
			boes = ws.Boes.ToList();
			tasks = ws.TaskElements.ToList();
			bool isOffloading = ws.ProjectMapType != ProjectMapType.StandardWithoutOffload;
			if (isOffloading)
			{
				OffloadLaborRates offloader = new OffloadLaborRates();
				List<int> selectedBoeIds = boes.Select(b => b.Id).ToList();
				OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes.Where(b => selectedBoeIds.Contains(b.Id)).ToList(), ws);

				boes = results.Boes.ToList();
				tasks = boes.SelectMany(b => b.TaskElements).ToList();
			}
		}
	}
}