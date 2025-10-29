// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO.SkillMixSummary
{
	using GenBOE.Dtos;
	using GenBOE.Models;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Linq;

	public class SkillMixSummaryDTOLoader : ISkillMixSummaryDTOLoader
	{
		private readonly Logger _log = new Logger(typeof(SkillMixSummaryDTOLoader));

		/// <summary>
		/// Get all Skill Mix Summary values by BOE Task Element FK ID
		/// </summary>
		/// <param name="boeTaskElementID">BOE Task Element ID</param>
		/// <returns>List of Skill Mix Summaries</returns>
		[DbQuery]
		public ICollection<SkillMixSummaryDTO> GetByBOETaskElementID(int boeTaskElementID)
		{
			List<SkillMixSummaryDTO> result = new List<SkillMixSummaryDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.SkillMixSummaries
							  where cdsm.BOETaskElementID == boeTaskElementID
							  select new SkillMixSummaryDTO
							  {
								  SkillMixSummaryID = cdsm.SkillMixSummaryID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedLegacyResource = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  ResourceHours = cdsm.ResourceHours,
								  ProposedBrc = cdsm.BusinessResourceHours,
								  ProposedSkillMix = cdsm.BOESkillMix,
								  HistoricalSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  IsUserInput = cdsm.IsUserInput
							  }).ToList();
				}
				DoPostProcessing(result);

				return result;
			}
		}

		/// <summary>
		/// Get all Skill Mix Summary values by Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>List of Skill Mix Summaries</returns>
		[DbQuery]
		public ICollection<SkillMixSummaryDTO> GetByWorkspaceId(int workspaceId)
		{
			List<SkillMixSummaryDTO> result = new List<SkillMixSummaryDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.SkillMixSummaries
							  join b in gbe.BOEs on cdsm.BOEID equals b.BOEID
							  where b.WorkspaceID == workspaceId
							  select new SkillMixSummaryDTO
							  {
								  SkillMixSummaryID = cdsm.SkillMixSummaryID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedLegacyResource = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  ResourceHours = cdsm.ResourceHours,
								  ProposedBrc = cdsm.BusinessResourceHours,
								  ProposedSkillMix = cdsm.BOESkillMix,
								  HistoricalSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  IsUserInput = cdsm.IsUserInput
							  }).ToList();
				}
				DoPostProcessing(result);

				return result;
			}
		}

		/// <summary>
		/// Get all Skill Mix Summary values by BOE FK IDs
		/// </summary>
		/// <param name="boeIDs">BOE Ids</param>
		/// <returns>List of Skill Mix Summaries</returns>
		[DbQuery]
		public ICollection<SkillMixSummaryDTO> GetByBOEIDs(ICollection<int> boeIDs)
		{
			List<SkillMixSummaryDTO> result = new List<SkillMixSummaryDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.SkillMixSummaries
							  where boeIDs.Contains(cdsm.BOEID)
							  select new SkillMixSummaryDTO
							  {
								  SkillMixSummaryID = cdsm.SkillMixSummaryID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedLegacyResource = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  ResourceHours = cdsm.ResourceHours,
								  ProposedBrc = cdsm.BusinessResourceHours,
								  ProposedSkillMix = cdsm.BOESkillMix,
								  HistoricalSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  IsUserInput = cdsm.IsUserInput
							  }).ToList();
				}
				DoPostProcessing(result);

				return result;
			}
		}

		/// <summary>
		/// Get Skill Mix Summary by certain values
		/// </summary>
		/// <param name="skillMixSummaryIDs"></param>
		/// <returns>List of Skill Mix Summaries</returns>
		public ICollection<SkillMixSummaryDTO> GetByIds(ICollection<int> skillMixSummaryIDs)
		{
			List<SkillMixSummaryDTO> result = new List<SkillMixSummaryDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.SkillMixSummaries
							  where skillMixSummaryIDs.Contains(cdsm.SkillMixSummaryID)
							  select new SkillMixSummaryDTO
							  {
								  SkillMixSummaryID = cdsm.SkillMixSummaryID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedLegacyResource = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  ResourceHours = cdsm.ResourceHours,
								  ProposedBrc = cdsm.BusinessResourceHours,
								  ProposedSkillMix = cdsm.BOESkillMix,
								  HistoricalSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  IsUserInput = cdsm.IsUserInput
							  }).ToList();
				}
				DoPostProcessing(result);

				return result;
			}
		}

		/// <summary>
		/// Get Skill Mix Summary value by a specific primary key
		/// </summary>
		/// <param name="skillMixSummaryID"></param>
		/// <returns>Skill Mix Summary</returns>
		[DbQuery]
		public SkillMixSummaryDTO GetById(int skillMixSummaryID)
		{
			return this.GetByIds(new Collection<int>() { skillMixSummaryID }).FirstOrDefault();
		}

		/// <summary>
		/// Delete Skill Mix Summary by BOE Task Element ID
		/// </summary>
		/// <param name="boeTaskElementID">BOE Task Element ID</param>
		/// <returns>Number of Skill Mix Summaries deleted</returns>
		public int? DeleteSkillMixSummaryByBOETaskElementID(int boeTaskElementID)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				if (boeTaskElementID > 0)
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						toReturn = gbe.deleteSkillMixSummary(boeTaskElementID).FirstOrDefault();
					}
				}
				return toReturn;
			}
		}

		/// <summary>
		/// Insert Skill Mix Summary with Kill/Fill procedure
		/// </summary>
		/// <param name="SkillMixSummaryDTO">SkillMixSumaaryDTO</param>
		/// <returns>Num rows that were inserted</returns>
		public int? InsertSkillMixSummary(ICollection<SkillMixSummaryDTO> skillMixSummaries)
		{
			int? toReturn = null;
			if (skillMixSummaries == null || skillMixSummaries.Count == 0) { throw new ArgumentNullException(nameof(skillMixSummaries)); }

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				Collection<string> skillMixSummaryVariablesPropertiesToIncludeInTable = new Collection<string>()
				{
					"Rationale", "Included", "ProposedLegacyResource", "HistoricalHours", "ResourceHours", "ProposedBrc", "ProposedSkillMix", "HistoricalSkillMix", "ResourceID", "BusinessResourceID", "BOEID", "BOETaskElementID", "IsUserInput"
				};

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					DataTable skillMixSummaryVariablesDataTable = StoredProcedureHelper.ToDataTable<SkillMixSummaryDTO>(skillMixSummaries, skillMixSummaryVariablesPropertiesToIncludeInTable);

					toReturn = StoredProcedureHelper.ExecuteTableValueProcedure(
						gbe,
						skillMixSummaryVariablesDataTable,
						"insertSkillMixSummaryviaTableParameter",
						"@SkillMixSummaryTableParameter",
						"TT_SkillMixSummary",
						false
						).FirstOrDefault().Key;
				}
				return toReturn;
			}
		}

		/// <summary>
		/// Do post processing on the Skill Mix Summary dtos
		/// </summary>
		/// <param name="summaryDTOs">skill mix summary dtos</param>
		private void DoPostProcessing(ICollection<SkillMixSummaryDTO> summaryDTOs)
		{
			IEnumerable<IGrouping<int, SkillMixSummaryDTO>> groupedResourceHours =
				summaryDTOs.GroupBy(r => r.BOETaskElementID);

			foreach (IGrouping<int, SkillMixSummaryDTO> grouping in groupedResourceHours)
			{
				decimal totalGroupHours = grouping.Sum(g => g.HistoricalHours);
				if (totalGroupHours != 0m)
				{
					foreach (SkillMixSummaryDTO dto in grouping)
					{
						dto.HistoricalSkillMix = dto.HistoricalHours * 100m / totalGroupHours;
					}
				}
			}
		}
	}
}
