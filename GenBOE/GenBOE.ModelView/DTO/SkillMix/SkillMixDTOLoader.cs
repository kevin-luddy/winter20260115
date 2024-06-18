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

							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get all SkillMix values by BOE FK ID
		/// </summary>
		/// <param name="boeID"></param>
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

							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get all SkillMix values by BOETaskElement FK ID
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

							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get SkillMix by certain values
		/// </summary>
		/// <param name="skillMixIDs"></param>
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

							  }).ToList();
				}
			}

			return result;
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
	}
}
