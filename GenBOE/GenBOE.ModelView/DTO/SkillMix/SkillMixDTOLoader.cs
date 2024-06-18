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

	public class SkillMixDTOLoader : ISkillMixDTOLoader
	{
		private readonly Logger _log = new Logger(typeof(SkillMixDTOLoader));

		/// <summary>
		/// Get all SkillMix values by MOQTypeSelection FK ID
		/// </summary>
		/// <param name="moqTypeSelectionID"></param>
		/// <returns>List of SkillMix</returns>
		[DbQuery]
		virtual public ICollection<SkillMixDTO> GetByMOQTypeSelectionID(int moqTypeSelectionID)
		{
			List<SkillMixDTO> result = new List<SkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log)) 
			{ 
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from sm in gbe.SkillMixes
							  where sm.MOQTypeSelectionID == moqTypeSelectionID
							  select new SkillMixDTO
							  {
								  SkillMixID = sm.SkillMixID,
								  Rationale = sm.Rationale,
								  ProposedHours = sm.ProposedHours ?? 0,
								  HistoricalHours = sm.HistoricalHours ?? 0,
								  BOESkillMix = sm.BOESkillMix ?? 0,
								  LaborSkillMix = sm.LaborSkillMix ?? 0,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID ?? -1,
								  BOETaskElementID = sm.BOETaskElementID ?? -1,
								  MOQTypeSelectionID = sm.MOQTypeSelectionID ?? -1
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get all SkillMix values by BOE FK ID
		/// </summary>
		/// <param name="boeID">int</param>
		/// <returns>List of SkillMix</returns>
		[DbQuery]
		virtual public ICollection<SkillMixDTO> GetByBOEID(int boeEID) 
		{
			List<SkillMixDTO> result = new List<SkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log)) 
			{ 
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from sm in gbe.SkillMixes
							  where sm.BOEID == boeEID
							  select new SkillMixDTO
							  {
								  SkillMixID = sm.SkillMixID,
								  Rationale = sm.Rationale,
								  ProposedHours = sm.ProposedHours ?? 0,
								  HistoricalHours = sm.HistoricalHours ?? 0,
								  BOESkillMix = sm.BOESkillMix ?? 0,
								  LaborSkillMix = sm.LaborSkillMix ?? 0,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID ?? -1,
								  BOETaskElementID = sm.BOETaskElementID ?? -1,
								  MOQTypeSelectionID = sm.MOQTypeSelectionID ?? -1
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get all SkillMix values by BOETaskElement FK ID
		/// </summary>
		/// <param name="boeTaskElementID">int</param>
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
								  ProposedHours = sm.ProposedHours ?? 0,
								  HistoricalHours = sm.HistoricalHours ?? 0,
								  BOESkillMix = sm.BOESkillMix ?? 0,
								  LaborSkillMix = sm.LaborSkillMix ?? 0,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID ?? -1,
								  BOETaskElementID = sm.BOETaskElementID ?? -1,
								  MOQTypeSelectionID = sm.MOQTypeSelectionID ?? -1
							  }).ToList();
				}
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
								  ProposedHours = sm.ProposedHours ?? 0,
								  HistoricalHours = sm.HistoricalHours ?? 0,
								  BOESkillMix = sm.BOESkillMix ?? 0,
								  LaborSkillMix = sm.LaborSkillMix ?? 0,
								  ResourceOld = sm.ResourceOld,
								  ResourceNew = sm.ResourceNew,
								  BOEID = sm.BOEID ?? -1,
								  BOETaskElementID = sm.BOETaskElementID ?? -1,
								  MOQTypeSelectionID = sm.MOQTypeSelectionID ?? -1
							  }).ToList();
				}
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
		/// <param name="moqTypeSelectionID">int</param>
		/// <returns>Number of SkillMixes deleted</returns>
		virtual public int? DeleteSkillMixByMoqTypeSelection(int moqTypeSelectionID)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				if (moqTypeSelectionID > 0)
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						toReturn = gbe.deleteSkillMix(moqTypeSelectionID).FirstOrDefault();
					}
				}
				return toReturn;
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

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				if (skillMixes != null || skillMixes.Count > 0)
				{
					Collection<string> skillMixVariablesPropertiesToIncludeInTable = new Collection<string>()
					{
						"Rationale", "Included", "ProposedHours", "HistoricalHours", "BOESkillMix", "LaborSkillMix", "ResourceOld", "ResourceNew", "BOEID", "BOETaskElementID", "MOQTypeSelectionID"
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
				}
				return toReturn;
			}
		}
	}
}
