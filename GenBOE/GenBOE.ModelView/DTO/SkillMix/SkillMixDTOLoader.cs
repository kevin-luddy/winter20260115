// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using IES.Common;
	using GenBOE.Dtos;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.Models;
	using System.Data;
	using System;
	using IES.Common.classes;

	public class SkillMixDTOLoader : ISkillMixDTOLoader
	{
		private readonly Logger _log = new Logger(typeof(SkillMixDTOLoader));

		/// <summary>
		/// Get all SkillMix values by MOQTypeSelection FK ID
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns>List of SkillMix</returns>
		[DbQuery]
		virtual public ICollection<SkillMixDTO> GetByBOETaskElementID(int boeTaskElementID)
		{
			List<SkillMixDTO> result = new List<SkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log)) 
			{ 
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from sm in gbe.SkillMixes
							  where sm.BOETaskElementID == boeTaskElementID
							  select new SkillMixDTO
							  {
								  SkillMixID = sm.SkillMixID,
								  Rationale = sm.Rationale,
								  ProposedHours = sm.ProposedHours,
								  HistoricalHours = sm.HistoricalHours,
								  BOESkillMix = sm.BOESkillMix,
								  LaborSkillMix = sm.LaborSkillMix,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID,
								  Included = sm.Included ?? false,
								  BOETaskElementID = sm.BOETaskElementID,
								  IsUserInput = sm.IsUserInput
							  }).ToList();
				}
				DoPostProcessiong(result);

				return result;
			}
		}

		/// <summary>
		/// Get all SkillMix values by BOE FK ID
		/// </summary>
		/// <param name="boeID">int</param>
		/// <returns>List of SkillMix</returns>
		[DbQuery]
		virtual public ICollection<SkillMixDTO> GetByBOEID(int boeID) 
		{
			List<SkillMixDTO> result = new List<SkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log)) 
			{ 
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from sm in gbe.SkillMixes
							  where sm.BOEID == boeID
							  select new SkillMixDTO
							  {
								  SkillMixID = sm.SkillMixID,
								  Rationale = sm.Rationale,
								  ProposedHours = sm.ProposedHours,
								  HistoricalHours = sm.HistoricalHours,
								  BOESkillMix = sm.BOESkillMix,
								  LaborSkillMix = sm.LaborSkillMix,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID,
								  Included = sm.Included,
								  BOETaskElementID = sm.BOETaskElementID,
								  IsUserInput = sm.IsUserInput
							  }).ToList();
				}
				DoPostProcessiong(result);

				return result;
			}
		}

		/// <summary>
		/// Get SkillMix by certain values
		/// </summary>
		/// <param name="skillMixIDs">ICollection<int></param>
		/// <returns>List of SkillMix</returns>
		[DbQuery]
		virtual public ICollection<SkillMixDTO> GetByIds(ICollection<int> skillMixIDs)
		{
			List<SkillMixDTO> result = new List<SkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from sm in gbe.SkillMixes
							  where skillMixIDs.Contains(sm.SkillMixID)
							  select new SkillMixDTO
							  {
								  SkillMixID = sm.SkillMixID,
								  Rationale = sm.Rationale,
								  ProposedHours = sm.ProposedHours,
								  HistoricalHours = sm.HistoricalHours,
								  BOESkillMix = sm.BOESkillMix,
								  LaborSkillMix = sm.LaborSkillMix,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID,
								  Included = sm.Included,
								  BOETaskElementID = sm.BOETaskElementID,
								  IsUserInput = sm.IsUserInput
							  }).ToList();
				}
				DoPostProcessiong(result);

				return result;
			}
		}

		/// <summary>
		/// Get SkillMix value by a specific primary key
		/// </summary>
		/// <param name="skillMixID"></param>
		/// <returns>SkillMix</returns>
		virtual public SkillMixDTO GetById(int skillMixID)
		{
			return this.GetByIds(new Collection<int>() { skillMixID }).FirstOrDefault();
		}

		/// <summary>
		/// Delete Skill Mix by MOQ Type Selection ID
		/// </summary>
		/// <param name="boeTaskElementID">int</param>
		/// <returns>Number of SkillMixes deleted</returns>
		virtual public int? DeleteSkillMixByBOETaskElementID(int boeTaskElementID)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				if (boeTaskElementID > 0)
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						toReturn = gbe.deleteSkillMix(boeTaskElementID).FirstOrDefault();
					}
				}
				return toReturn;
			}
		}

		/// <summary>
		/// Get all Skill Mix values by Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>List of Skill Mix rows</returns>
		[DbQuery]
		public ICollection<SkillMixDTO> GetByWorkspaceId(int workspaceId)
		{
			List<SkillMixDTO> result = new List<SkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from sm in gbe.SkillMixes
							  join b in gbe.BOEs on sm.BOEID equals b.BOEID
							  where b.WorkspaceID == workspaceId
							  select new SkillMixDTO
							  {
								  SkillMixID = sm.SkillMixID,
								  Rationale = sm.Rationale,
								  ProposedHours = sm.ProposedHours,
								  HistoricalHours = sm.HistoricalHours,
								  BOESkillMix = sm.BOESkillMix,
								  LaborSkillMix = sm.LaborSkillMix,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID,
								  Included = sm.Included,
								  BOETaskElementID = sm.BOETaskElementID,
								  IsUserInput = sm.IsUserInput
							  }).ToList();
				}
				DoPostProcessiong(result);

				return result;
			}
		}

		/// <summary>
		/// Insert Skill Mix with Kill/Fill procedure
		/// </summary>
		/// <param name="skillMixes">SkillMixDTO</param>
		/// <returns>Num rows that were inserted</returns>
		virtual public int? InsertSkillMix(ICollection<SkillMixDTO> skillMixes)
		{
			int? toReturn = null;
			if (skillMixes == null || skillMixes.Count == 0) { throw new ArgumentNullException(nameof(skillMixes)); }

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				Collection<string> skillMixVariablesPropertiesToIncludeInTable = new Collection<string>()
				{
					"Rationale", "Included", "ProposedHours", "HistoricalHours", "BOESkillMix", "LaborSkillMix", "ResourceOld", "ResourceNew", "BOEID", "BOETaskElementID", "IsUserInput"
				};
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					DataTable skillMixVariablesDataTable = StoredProcedureHelper.ToDataTable<SkillMixDTO>(skillMixes, skillMixVariablesPropertiesToIncludeInTable);

					toReturn = StoredProcedureHelper.ExecuteTableValueProcedure(
						gbe,
						skillMixVariablesDataTable,
						"insertSkillMixviaTableParameter",
						"@SkillMixTableParameter",
						"TT_SkillMix",
						false
						).FirstOrDefault().Key;
				}
				return toReturn;
			}
		}

		/// <summary>
		/// Do post processing on the skill mix dtos
		/// </summary>
		/// <param name="skillMixes">skill mix dtos</param>
		private void DoPostProcessiong(ICollection<SkillMixDTO> skillMixes)
		{
			IEnumerable<IGrouping<int, SkillMixDTO>> groupedResourceHours =
				skillMixes.GroupBy(r => r.MOQTypeSelectionID);

			foreach (IGrouping<int, SkillMixDTO> grouping in groupedResourceHours)
			{
				decimal totalGroupHours = grouping.Sum(g => g.HistoricalHours);

				if (totalGroupHours != 0m)
				{
					foreach (SkillMixDTO dto in grouping)
					{
						dto.LaborSkillMix = dto.HistoricalHours / totalGroupHours;
					}
				}
			}
		}
	}
}
