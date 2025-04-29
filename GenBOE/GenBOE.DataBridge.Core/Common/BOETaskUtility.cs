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

			if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
			{
				showSkillMixRationale = CommonUtilities.ShowSkillMixForWorkspace(workspaceCreationDate);
			}
			else if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				bool hasSapWebi = moqTypeSelections.Any(x => x.TableData.Any(y => y.RepositoryName == RepositoryName.SapWebi.GetDescription()));

				bool isComparativeHistoricalAnalogous = moqTypeSelections.Any(x =>
				x.SelectedMOQType == MOQType.Comparative ||
				x.SelectedMOQType == MOQType.Historical ||
				x.SelectedMOQType == MOQType.AnalogousRelationships);

				showSkillMixRationale = CommonUtilities.ShowSkillMixForWorkspace(workspaceCreationDate) && !hasTMRates && hasSapWebi && isComparativeHistoricalAnalogous;
			}

			return showSkillMixRationale;
		}
	}
}