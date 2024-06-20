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

	public class CommonDisclosureSMDTODataLoader : ICommonDisclosureSMDTODataLoader
	{
		private readonly Logger _log = new Logger(typeof(CommonDisclosureSMDTODataLoader));

		/// <summary>
		/// Get all Common Disclosure Skill Mix values by MOQTypeSelection FK ID
		/// </summary>
		/// <param name="moqTypeSelectionID"></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		virtual public ICollection<CommonDisclosureSkillMixDTO> GetByMOQTypeSelectionID(int moqTypeSelectionID)
		{
			List<CommonDisclosureSkillMixDTO> result = new List<CommonDisclosureSkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.CommonDisclosureSkillMixes
							  where cdsm.MOQTypeSelectionID == moqTypeSelectionID
							  select new CommonDisclosureSkillMixDTO
							  {
								  CommonDisclosureSkillMixID = cdsm.CommonDisclosureSkillMixID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedHours = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  BOESkillMix = cdsm.BOESkillMix,
								  LaborSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  SkillMixID = cdsm.SkillMixID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  MOQTypeSelectionID = cdsm.MOQTypeSelectionID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get all Common Disclosure Skill Mix values by BOE FK ID
		/// </summary>
		/// <param name="boeID">int</param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		virtual public ICollection<CommonDisclosureSkillMixDTO> GetByBOEID(int boeID)
		{
			List<CommonDisclosureSkillMixDTO> result = new List<CommonDisclosureSkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.CommonDisclosureSkillMixes
							  where cdsm.BOEID == boeID
							  select new CommonDisclosureSkillMixDTO
							  {
								  CommonDisclosureSkillMixID = cdsm.CommonDisclosureSkillMixID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedHours = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  BOESkillMix = cdsm.BOESkillMix,
								  LaborSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  SkillMixID = cdsm.SkillMixID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  MOQTypeSelectionID = cdsm.MOQTypeSelectionID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get all Common Disclosure Skill Mix values by BOETaskElement FK ID
		/// </summary>
		/// <param name="boeTaskElementID">int</param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		virtual public ICollection<CommonDisclosureSkillMixDTO> GetByBOETaskElementID(int boeTaskElementID)
		{
			List<CommonDisclosureSkillMixDTO> result = new List<CommonDisclosureSkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.CommonDisclosureSkillMixes
							  where cdsm.BOETaskElementID == boeTaskElementID
							  select new CommonDisclosureSkillMixDTO
							  {
								  CommonDisclosureSkillMixID = cdsm.CommonDisclosureSkillMixID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedHours = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  BOESkillMix = cdsm.BOESkillMix,
								  LaborSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  SkillMixID = cdsm.SkillMixID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  MOQTypeSelectionID = cdsm.MOQTypeSelectionID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get Common Disclosure Skill Mixes by certain values
		/// </summary>
		/// <param name="commonDisclosureSkillMixIDs">ICollection<int></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		virtual public ICollection<CommonDisclosureSkillMixDTO> GetByIds(ICollection<int> commonDisclosureSkillMixIDs)
		{
			List<CommonDisclosureSkillMixDTO> result = new List<CommonDisclosureSkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.CommonDisclosureSkillMixes
							  where commonDisclosureSkillMixIDs.Contains(cdsm.CommonDisclosureSkillMixID)
							  select new CommonDisclosureSkillMixDTO
							  {
								  CommonDisclosureSkillMixID = cdsm.CommonDisclosureSkillMixID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedHours = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  BOESkillMix = cdsm.BOESkillMix,
								  LaborSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  SkillMixID = cdsm.SkillMixID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  MOQTypeSelectionID = cdsm.MOQTypeSelectionID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get Common Disclosure Skill Mix value by a specific primary key
		/// </summary>
		/// <param name="commonDisclosureSkillMixID"></param>
		/// <returns>Common Disclosure Skill Mix</returns>
		virtual public CommonDisclosureSkillMixDTO GetById(int commonDisclosureSkillMixID)
		{
			return this.GetByIds(new Collection<int>() { commonDisclosureSkillMixID }).FirstOrDefault();
		}

		/// <summary>
		/// Get Common Disclosure Skill Mixes by skill mix values
		/// </summary>
		/// <param name="skillMixIDs">ICollection<int></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		virtual public ICollection<CommonDisclosureSkillMixDTO> GetBySkillMixIds(ICollection<int> skillMixIDs)
		{
			List<CommonDisclosureSkillMixDTO> result = new List<CommonDisclosureSkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.CommonDisclosureSkillMixes
							  where skillMixIDs.Contains(cdsm.SkillMixID)
							  select new CommonDisclosureSkillMixDTO
							  {
								  CommonDisclosureSkillMixID = cdsm.CommonDisclosureSkillMixID,
								  Rationale = cdsm.Rationale,
								  Included = cdsm.Included,
								  ProposedHours = cdsm.ProposedHours,
								  HistoricalHours = cdsm.HistoricalHours,
								  BOESkillMix = cdsm.BOESkillMix,
								  LaborSkillMix = cdsm.LaborSkillMix,
								  ResourceID = cdsm.ResourceID,
								  BusinessResourceID = cdsm.BusinessResourceID,
								  SkillMixID = cdsm.SkillMixID,
								  BOEID = cdsm.BOEID,
								  BOETaskElementID = cdsm.BOETaskElementID,
								  MOQTypeSelectionID = cdsm.MOQTypeSelectionID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get Common Disclosure Skill Mixes value by a skill mix id
		/// </summary>
		/// <param name="skillMixID"></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		virtual public ICollection<CommonDisclosureSkillMixDTO> GetBySkillMixId(int skillMixID)
		{
			return this.GetBySkillMixIds(new Collection<int>() { skillMixID });
		}

		/// <summary>
		/// Delete Common Disclosure Skill Mixes by MOQ Type Selection ID
		/// </summary>
		/// <param name="moqTypeSelectionID">int</param>
		/// <returns>Number of Common Disclosure Skill Mixes deleted</returns>
		virtual public int? DeleteCommonDisclosureSMByMoqTypeSelection(int moqTypeSelectionID)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				if (moqTypeSelectionID > 0)
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						toReturn = gbe.deleteCommonDisclosureSkillMix(moqTypeSelectionID).FirstOrDefault();
					}
				}
				return toReturn;
			}
		}

		/// <summary>
		/// Insert Common Disclosure Skill Mixes with Kill/Fill procedure
		/// </summary>
		/// <param name="skillMixes">SkillMixDTO</param>
		/// <returns>Num rows that were inserted</returns>
		virtual public int? InsertCommonDisclosureSM(ICollection<CommonDisclosureSkillMixDTO> commonDisclosureSkillMixes)
		{
			int? toReturn = null;
			if (commonDisclosureSkillMixes == null || commonDisclosureSkillMixes.Count == 0) { throw new ArgumentNullException(nameof(commonDisclosureSkillMixes)); }

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				Collection<string> skillMixVariablesPropertiesToIncludeInTable = new Collection<string>()
				{
					"Rationale", "Included", "ProposedHours", "HistoricalHours", "BOESkillMix", "LaborSkillMix", "ResourceID", "BusinessResourceID", "SkillMixID", "BOEID", "BOETaskElementID", "MOQTypeSelectionID"
				};
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					DataTable commonDisclosureSkillMixVariablesDataTable = StoredProcedureHelper.ToDataTable<CommonDisclosureSkillMixDTO>(commonDisclosureSkillMixes, skillMixVariablesPropertiesToIncludeInTable);

					toReturn = StoredProcedureHelper.ExecuteTableValueProcedure(
						gbe,
						commonDisclosureSkillMixVariablesDataTable,
						"insertCommonDisclosureSkillMixviaTableParameter",
						"@CommonDisclosureSkillMixTableParameter",
						"TT_CommonDisclosureSkillMix",
						false
						).FirstOrDefault().Key;
				}
				return toReturn;
			}
		}
	}
}
