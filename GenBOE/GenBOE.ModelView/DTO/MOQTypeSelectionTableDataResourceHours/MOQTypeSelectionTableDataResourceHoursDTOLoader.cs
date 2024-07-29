// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using GenBOE.Models;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Linq;

	public class MOQTypeSelectionTableDataResourceHoursDTOLoader : IMOQTypeSelectionTableDataResourceHoursDTOLoader
	{
		private readonly Logger _log = new Logger(typeof(MOQTypeSelectionTableDataResourceHoursDTOLoader));

		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by MOQTypeSelectionTableDataId FK Id.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataId"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		[DbQuery]
		virtual public ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByMOQTypeSelectionTableDataId(int moqTypeSelectionTableDataId)
		{
			List<MOQTypeSelectionTableDataResourceHoursDTO> result = new List<MOQTypeSelectionTableDataResourceHoursDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from moq in gbe.MOQTypeSelectionTableDataResourceHours
							  where moq.MOQTypeSelectionTableDataId == moqTypeSelectionTableDataId
							  select new MOQTypeSelectionTableDataResourceHoursDTO
							  {
								  MOQTypeSelectionTableDataResourceHoursId = moq.MOQTypeSelectionTableDataResourceHoursId,
								  ResourceName = moq.ResourceName,
								  BRCName = moq.BRCName,
								  WbsHours = moq.WbsHours,
								  TotalHours = moq.TotalHours,
								  MOQTypeSelectionTableDataId = moq.MOQTypeSelectionTableDataId,
								  BOETaskElementID = moq.BOETaskElementID,
								  BOEID = moq.BOEID
							  }).ToList();
				}

				return result;
			}
		}

		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by BOETaskElement FK ID.
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		[DbQuery]
		public virtual ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByWorkspaceId(int workspaceId)
		{
			List<MOQTypeSelectionTableDataResourceHoursDTO> result = new List<MOQTypeSelectionTableDataResourceHoursDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from moq in gbe.MOQTypeSelectionTableDataResourceHours
								join b in gbe.BOEs on moq.BOEID equals b.BOEID
								where b.WorkspaceID == workspaceId
								select new MOQTypeSelectionTableDataResourceHoursDTO
								{
									MOQTypeSelectionTableDataResourceHoursId = moq.MOQTypeSelectionTableDataResourceHoursId,
									ResourceName = moq.ResourceName,
									BRCName = moq.BRCName,
									WbsHours = moq.WbsHours,
									TotalHours = moq.TotalHours,
									MOQTypeSelectionTableDataId = moq.MOQTypeSelectionTableDataId,
									BOETaskElementID = moq.BOETaskElementID,
									BOEID = moq.BOEID
								}).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by BOE FK ID.
		/// </summary>
		/// <param name="boeID"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		[DbQuery]
		virtual public ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByBOEID(int boeID)
		{
			List<MOQTypeSelectionTableDataResourceHoursDTO> result = new List<MOQTypeSelectionTableDataResourceHoursDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from moq in gbe.MOQTypeSelectionTableDataResourceHours
							  where moq.BOEID == boeID
							  select new MOQTypeSelectionTableDataResourceHoursDTO
							  {
								  MOQTypeSelectionTableDataResourceHoursId = moq.MOQTypeSelectionTableDataResourceHoursId,
								  ResourceName = moq.ResourceName,
								  BRCName = moq.BRCName,
								  WbsHours = moq.WbsHours,
								  TotalHours = moq.TotalHours,
								  MOQTypeSelectionTableDataId = moq.MOQTypeSelectionTableDataId,
								  BOETaskElementID = moq.BOETaskElementID,
								  BOEID = moq.BOEID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get all MOQ Type Selection Table Data Resource Hours values by BOETaskElement FK ID.
		/// </summary>
		/// <param name="boeTaskElementID"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		[DbQuery]
		virtual public ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByBOETaskElementID(int boeTaskElementID)
		{
			List<MOQTypeSelectionTableDataResourceHoursDTO> result = new List<MOQTypeSelectionTableDataResourceHoursDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from moq in gbe.MOQTypeSelectionTableDataResourceHours
							  where moq.BOETaskElementID == boeTaskElementID
							  select new MOQTypeSelectionTableDataResourceHoursDTO
							  {
								  MOQTypeSelectionTableDataResourceHoursId = moq.MOQTypeSelectionTableDataResourceHoursId,
								  ResourceName = moq.ResourceName,
								  BRCName = moq.BRCName,
								  WbsHours = moq.WbsHours,
								  TotalHours = moq.TotalHours,
								  MOQTypeSelectionTableDataId = moq.MOQTypeSelectionTableDataId,
								  BOETaskElementID = moq.BOETaskElementID,
								  BOEID = moq.BOEID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get MOQ Type Selection Table Data Resource Hours by Ids.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataResourceHoursIds"></param>
		/// <returns>List of MOQ Type Selection Table Data Resource Hours</returns>
		[DbQuery]
		virtual public ICollection<MOQTypeSelectionTableDataResourceHoursDTO> GetByIds(ICollection<int> moqTypeSelectionTableDataResourceHoursIds)
		{
			List<MOQTypeSelectionTableDataResourceHoursDTO> result = new List<MOQTypeSelectionTableDataResourceHoursDTO>();

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from moq in gbe.MOQTypeSelectionTableDataResourceHours
							  where moqTypeSelectionTableDataResourceHoursIds.Contains(moq.MOQTypeSelectionTableDataResourceHoursId)
							  select new MOQTypeSelectionTableDataResourceHoursDTO
							  {
								  MOQTypeSelectionTableDataResourceHoursId = moq.MOQTypeSelectionTableDataResourceHoursId,
								  ResourceName = moq.ResourceName,
								  BRCName = moq.BRCName,
								  WbsHours = moq.WbsHours,
								  TotalHours = moq.TotalHours,
								  MOQTypeSelectionTableDataId = moq.MOQTypeSelectionTableDataId,
								  BOETaskElementID = moq.BOETaskElementID,
								  BOEID = moq.BOEID
							  }).ToList();
				}
				return result;
			}
		}

		/// <summary>
		/// Get MOQ Type Selection Table Data Resource Hours value by a specific primary key.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataResourceHoursId"></param>
		/// <returns>MOQ Type Selection Table Data Resource Hours</returns>
		virtual public MOQTypeSelectionTableDataResourceHoursDTO GetById(int moqTypeSelectionTableDataResourceHoursId)
		{
			return this.GetByIds(new Collection<int>() { moqTypeSelectionTableDataResourceHoursId }).FirstOrDefault();
		}

		/// <summary>
		/// Delete MOQ Type Selection Table Data Resource Hours by MOQ Type Selection ID.
		/// </summary>
		/// <param name="moqTypeSelectionTableDataId"></param>
		/// <returns>Number of MOQ Type Selection Table Data Resource Hours deleted</returns>
		virtual public int? DeleteByMoqTypeSelectionTableDataId(int moqTypeSelectionTableDataId)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				if (moqTypeSelectionTableDataId > 0)
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						toReturn = gbe.deleteMOQTypeSelectionTableDataResourceHours(moqTypeSelectionTableDataId).FirstOrDefault();
					}
				}
				return toReturn;
			}
		}

		/// <summary>
		/// Insert MOQ Type Selection Table Data Resource Hours with Kill/Fill procedure
		/// </summary>
		/// <param name="moqTypeSelectionTableDataResourceHours">MOQ Type Selection Table Data Resource Hours DTOs</param>
		/// <returns>Num rows that were inserted</returns>
		virtual public int? InsertMOQTypeSelectionTableDataResourceHours(ICollection<MOQTypeSelectionTableDataResourceHoursDTO> moqTypeSelectionTableDataResourceHours)
		{
			int? toReturn = null;
			if (moqTypeSelectionTableDataResourceHours == null || moqTypeSelectionTableDataResourceHours.Count == 0) { throw new ArgumentNullException(nameof(moqTypeSelectionTableDataResourceHours)); }

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				Collection<string> resourceHoursVariablesPropertiesToIncludeInTable = new Collection<string>()
				{
					"ResourceName", "BRCName", "WbsHours", "TotalHours", "MOQTypeSelectionTableDataId", "BOETaskElementID", "BOEID"
				};
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					DataTable moqTypeSelectionTableDataResourceHoursVariablesDataTable = StoredProcedureHelper.ToDataTable<MOQTypeSelectionTableDataResourceHoursDTO>(moqTypeSelectionTableDataResourceHours, resourceHoursVariablesPropertiesToIncludeInTable);

					toReturn = StoredProcedureHelper.ExecuteTableValueProcedure(
						gbe,
						moqTypeSelectionTableDataResourceHoursVariablesDataTable,
						"insertMOQTypeSelectionTableDataResourceHoursviaTableParameter",
						"@MOQTypeSelectionTableDataResourceHoursTableParameter",
						"TT_MOQTypeSelectionTableDataResourceHours",
						false
						).FirstOrDefault().Key;
				}
				return toReturn;
			}
		}
	}
}