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

	/// <summary>
	/// Common Disclosure Skill Mix DTO Data Loader
	/// </summary>
	public class CommonDisclosureSMDTODataLoader : ICommonDisclosureSMDTODataLoader
	{
		private readonly Logger _log = new Logger(typeof(CommonDisclosureSMDTODataLoader));

		/// <summary>
		/// Get all Common Disclosure Skill Mix values by BOETaskElement FK ID
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		public virtual ICollection<CommonDisclosureSkillMixDTO> GetByBOETaskElementID(int boeTaskElementID)
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
		/// Get all Common disclosure Skill Mix values by Workspace Id
		/// </summary>
		/// <param name="workspaceId">Workspace Id</param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		public ICollection<CommonDisclosureSkillMixDTO> GetByWorkspaceId(int workspaceId)
		{
			List<CommonDisclosureSkillMixDTO> result = new List<CommonDisclosureSkillMixDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from cdsm in gbe.CommonDisclosureSkillMixes
							  join b in gbe.BOEs on cdsm.BOEID equals b.BOEID
							  where b.WorkspaceID == workspaceId
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
		/// Get all Common Disclosure Skill Mix values by BOE FK ID
		/// </summary>
		/// <param name="boeID">int</param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		public virtual ICollection<CommonDisclosureSkillMixDTO> GetByBOEID(int boeID)
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
		/// Get Common Disclosure Skill Mixes by certain values
		/// </summary>
		/// <param name="commonDisclosureSkillMixIDs">ICollection<int></param>
		/// <returns>List of Common Disclosure Skill Mixes</returns>
		[DbQuery]
		public virtual ICollection<CommonDisclosureSkillMixDTO> GetByIds(ICollection<int> commonDisclosureSkillMixIDs)
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
		/// Get Common Disclosure Skill Mix value by a specific primary key
		/// </summary>
		/// <param name="commonDisclosureSkillMixID"></param>
		/// <returns>Common Disclosure Skill Mix</returns>
		public virtual CommonDisclosureSkillMixDTO GetById(int commonDisclosureSkillMixID)
		{
			return this.GetByIds(new Collection<int>() { commonDisclosureSkillMixID }).FirstOrDefault();
		}


		/// <summary>
		/// Delete Common Disclosure Skill Mixes by BOE Task Element ID
		/// </summary>
		/// <param name="boeTaskElementID">int</param>
		/// <returns>Number of Common Disclosure Skill Mixes deleted</returns>
		public virtual int? DeleteCommonDisclosureSkillMixByBOETaskElementID(int boeTaskElementID)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				if (boeTaskElementID > 0)
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						toReturn = gbe.deleteCommonDisclosureSkillMix(boeTaskElementID).FirstOrDefault();
					}
				}
				return toReturn;
			}
		}

		/// <summary>
		/// Insert Common Disclosure Skill Mixes with Kill/Fill procedure
		/// </summary>
		/// <param name="commonDisclosureSkillMixes">SkillMixDTO</param>
		/// <returns>Num rows that were inserted</returns>
		public virtual int? InsertCommonDisclosureSM(ICollection<CommonDisclosureSkillMixDTO> commonDisclosureSkillMixes)
		{
			int? toReturn = null;
			if (commonDisclosureSkillMixes == null || commonDisclosureSkillMixes.Count == 0) { throw new ArgumentNullException(nameof(commonDisclosureSkillMixes)); }

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				Collection<string> commonDisclosureVariablesPropertiesToIncludeInTable = new Collection<string>()
				{
					"Rationale", "Included", "ProposedHours", "HistoricalHours", "BOESkillMix", "LaborSkillMix", "ResourceID", "BusinessResourceID", "BOEID", "BOETaskElementID", "IsUserInput"
				};
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					DataTable commonDisclosureSkillMixVariablesDataTable = StoredProcedureHelper.ToDataTable<CommonDisclosureSkillMixDTO>(commonDisclosureSkillMixes, commonDisclosureVariablesPropertiesToIncludeInTable);

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

		/// <summary>
		/// Do post processing on the Common Disclosure dtos
		/// </summary>
		/// <param name="commonDisclosures">Common disclosure dtos</param>
		private void DoPostProcessing(ICollection<CommonDisclosureSkillMixDTO> commonDisclosures)
		{
			IEnumerable<IGrouping<int, CommonDisclosureSkillMixDTO>> groupedResourceHours =
				commonDisclosures.GroupBy(r => r.MOQTypeSelectionID);
			
			foreach (IGrouping<int, CommonDisclosureSkillMixDTO> grouping in groupedResourceHours)
			{
				decimal totalGroupHours = grouping.Sum(g => g.HistoricalHours);
				if (totalGroupHours != 0m)
				{
					foreach (CommonDisclosureSkillMixDTO dto in grouping)
					{
						dto.LaborSkillMix = dto.HistoricalHours / totalGroupHours;
					}
				}
			}
		}
	}
}
