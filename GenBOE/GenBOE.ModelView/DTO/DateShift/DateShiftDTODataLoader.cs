// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using GenBOE.Models;
	using GenBOE.Objects;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.Entity;
	using System.Linq;

	/// <summary>
	/// Date shift DTO data loader.
	/// </summary>
	public class DateShiftDTODataLoader : DataLoader<DateShiftDTO>, IDateShiftDTODataLoader
	{
		/// <summary>
		/// Resource spread loader.
		/// </summary>
		private readonly IResourceSpreadLoader resourceSpreadLoader;

		/// <summary>
		/// Skill mix loader.
		/// </summary>
		private readonly ISkillMixDTOLoader skillMixLoader;

		/// <summary>
		/// Common discolsure loader.
		/// </summary>
		private readonly ICommonDisclosureSMDTODataLoader commonDisclosureLoader;

		/// <summary>
		/// Default constructor
		/// </summary>
		public DateShiftDTODataLoader(IResourceSpreadLoader resourceSpreadLoader, ISkillMixDTOLoader skillMixLoader, ICommonDisclosureSMDTODataLoader commonDisclosureLoader)
		{
			this.Log = new Logger(typeof(DateShiftDTODataLoader));
			this.resourceSpreadLoader = resourceSpreadLoader;
			this.skillMixLoader = skillMixLoader;
			this.commonDisclosureLoader = commonDisclosureLoader;
		}

		/// <summary>
		/// Updates the date shifted objects in the database.
		/// </summary>
		/// <param name="dateShiftDTOs">Date shifted DTOs.</param>
		public void Update(ICollection<DateShiftDTO> dateShiftDTOs)
		{
			if (dateShiftDTOs == null)
			{
				throw new ArgumentNullException(nameof(dateShiftDTOs));
			}

			List<DateShiftDTO> dateShiftDTOsToUpdate = new List<DateShiftDTO>();

			foreach (DateShiftDTO dateShiftDTO in dateShiftDTOs)
			{
				dateShiftDTOsToUpdate.AddRange(RecursivelyGetDateShiftDTOs(dateShiftDTO));
			}

			foreach (DateShiftDTO dateShiftDTOtoUpdate in dateShiftDTOsToUpdate)
			{
				// Update any labor spreads.
				if (dateShiftDTOtoUpdate.LaborSpreads != null && dateShiftDTOtoUpdate.Level == (int)Level.Labor)
				{
					resourceSpreadLoader.BulkSave(dateShiftDTOtoUpdate.LaborSpreads);
				}

				// Update any skill mixes and common disclosure for BOE Task Elements.
				if (dateShiftDTOtoUpdate.BOETaskElementId != null && dateShiftDTOtoUpdate.Level == (int)Level.Task)
				{
					if (dateShiftDTOtoUpdate.SkillMixTable != null && dateShiftDTOtoUpdate.SkillMixTable.Any())
					{
						List<SkillMixDTO> dtos = new List<SkillMixDTO>();
						foreach (SkillMixModelView skillMixModelView in dateShiftDTOtoUpdate.SkillMixTable)
						{
							SkillMixDTO dto = skillMixModelView.ToDto();
							dto.BOEID = dateShiftDTOtoUpdate.BoeId;
							dto.BOETaskElementID = dateShiftDTOtoUpdate.BOETaskElementId.Value;
							dtos.Add(dto);
						}

						this.skillMixLoader.InsertSkillMix(dtos);
					}
					else
					{
						// If the dto has no skill mix tables, it's possible they were cleared out, so make sure old data is deleted
						this.skillMixLoader.DeleteSkillMixByBOETaskElementID(dateShiftDTOtoUpdate.BOETaskElementId.Value);
					}

					// Save the Common Disclosure DTOs
					if (dateShiftDTOtoUpdate.CommonDisclosureTable != null && dateShiftDTOtoUpdate.CommonDisclosureTable.Any())
					{
						List<CommonDisclosureSkillMixDTO> dtos = new List<CommonDisclosureSkillMixDTO>();
						foreach (CommonDisclosureModelView commonDisclosure in dateShiftDTOtoUpdate.CommonDisclosureTable)
						{
							CommonDisclosureSkillMixDTO dto = commonDisclosure.ToDto();
							dto.BOEID = dateShiftDTOtoUpdate.BoeId;
							dto.BOETaskElementID = dateShiftDTOtoUpdate.BOETaskElementId.Value;
							dtos.Add(dto);
						}

						this.commonDisclosureLoader.InsertCommonDisclosureSM(dtos);
					}
					else
					{
						// If the dto has no common disclosure tables, it's possible they were cleared out, so make sure old data is deleted
						this.commonDisclosureLoader.DeleteCommonDisclosureSkillMixByBOETaskElementID(dateShiftDTOtoUpdate.Id);
					}
				}
			}

			// Update the dates in the database for all shifted items.
			this.UpdateDateShifts(dateShiftDTOsToUpdate);
		}

		#region Inherited
		/// <summary>
		/// Not implemented and not needed.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public override ICollection<DateShiftDTO> GetByIds(ICollection<int> ids)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Not implemented and not needed.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		protected override int? Delete(DateShiftDTO dtoToDelete)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Not implemented and not needed.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		protected override int? Upsert(DateShiftDTO dtoToUpsert)
		{
			throw new NotImplementedException();
		}
		#endregion

		/// <summary>
		/// Update date shifts in the database.
		/// </summary>
		/// <param name="dateShifts">Dateshift objects</param>
		public virtual void UpdateDateShifts(ICollection<DateShiftDTO> dateShifts)
		{
			if (dateShifts == null || dateShifts.Count == 0) { throw new ArgumentNullException(nameof(dateShifts)); }

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				Collection<string> dateShiftPropertiesToIncludeInTable = new Collection<string>()
				{
					"Level", "Id", "StartDate", "EndDate", "UpdateDate"
				};

				using (DbContext objectContext = new DbContext(Constants.BOE_DB_CONTEXT_NAME))
				{
					DataTable dateShiftsDataTable = StoredProcedureHelper.ToDataTable<DateShiftDTO>(dateShifts, dateShiftPropertiesToIncludeInTable);

					StoredProcedureHelper.ExecuteTableValueProcedure(
						objectContext,
						dateShiftsDataTable,
						"updateDateShiftviaTableParameter",
						"@DateShifts",
						"TT_DateShift",
						false
					);
				}
			}
		}

		/// <summary>
		/// Gets the date shift child DTOs recursively.
		/// </summary>
		/// <param name="dateShiftable">Date shiftable.</param>
		/// <returns>List of date shift dtos that were in the child property.</returns>
		private List<DateShiftDTO> RecursivelyGetDateShiftDTOs(DateShiftDTO dateShiftDTO)
		{
			if (dateShiftDTO == null)
			{
				throw new ArgumentNullException(nameof(dateShiftDTO));
			}

			List<DateShiftDTO> dateShiftDTOs = new List<DateShiftDTO>();

			if (dateShiftDTO != null)
			{
				dateShiftDTOs.Add(dateShiftDTO);
			}

			if (dateShiftDTO.Children != null)
			{
				foreach (DateShiftDTO child in dateShiftDTO.Children)
				{
					dateShiftDTOs.AddRange(RecursivelyGetDateShiftDTOs(child));
				}
			}

			return dateShiftDTOs;
		}

		/// <summary>
		/// Gets the Date Shift object by level and ID.
		/// </summary>
		/// <param name="level">Level.</param>
		/// <param name="id">Id.</param>
		/// <returns>Date Shift object</returns>
		public DateShiftDTO GetDateShiftObject(Level level, int id)
		{
			DateShiftDTO dateShift = null;

			switch (level)
			{
				case Level.BOE:
					dateShift = DateShiftDTO.FromIDateShiftable(GetBoeDateShiftDataById(id));
					// Sets parent to be CLIN if the BOE uses a clin, otherwise, if NO CLIN then set Workspace as parent.
					if (dateShift.ClinId != null)
					{
						dateShift.Parent = DateShiftDTO.FromIDateShiftable(GetClinDateShiftDataById(dateShift.ParentId.Value));
					}
					else
					{
						dateShift.Parent = DateShiftDTO.FromIDateShiftable(GetWorkspaceDateShiftDataById(dateShift.WorkspaceId));
					}

					// Add the Tasks as the children.
					dateShift.Children.AddRange(GetBoeChildrenByBoeId(id)?.ToList());
					break;
				case Level.CLIN:
					dateShift = DateShiftDTO.FromIDateShiftable(GetClinDateShiftDataById(id));
					dateShift.Parent = DateShiftDTO.FromIDateShiftable(GetWorkspaceDateShiftDataById(dateShift.WorkspaceId));

					// Add the Children
					dateShift.Children.AddRange(GetClinChildrenByClinId(id));
					break;
				case Level.Task:
					dateShift = DateShiftDTO.FromIDateShiftable(GetBoeTaskElementDateShiftDataById(id));
					dateShift.Parent = DateShiftDTO.FromIDateShiftable(GetBoeDateShiftDataById(dateShift.BoeId));
					break;
				case Level.Workspace:
					dateShift = DateShiftDTO.FromIDateShiftable(GetWorkspaceDateShiftDataById(id));
					// TODO Thomas: The issue with this is that we aren't getting the children of the children.
					dateShift.Children.AddRange(GetClinByWorkspaceId(id));
					dateShift.Children.AddRange(GetBoeByWorkspaceId(id));
					break;
				default:
					throw new NotSupportedException($"Unsupported level: {level}");
			}

			return dateShift;
		}

		/// <summary>
		/// Gets workspace by short name.
		/// </summary>
		/// <param name="workspaceShortName">Ws shortname.</param>
		/// <returns>Date shift object.</returns>
		public DateShiftDTO GetWorkspaceDateShiftDataObject(string workspaceShortName)
		{
			return DateShiftDTO.FromIDateShiftable(GetWorkspaceDateShiftDataByShortname(workspaceShortName));
		}

		/// <summary>
		/// Gets the required boe data by id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Boe data.</returns>
		public DateShiftDTO GetBoeDateShiftDataById(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				DateShiftDTO boe = (from b in gbe.BOEs
									where b.BOEID == id
									join xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID.HasValue) on b.BOEID equals xRef.BOEID into temp
									from xRef in temp.DefaultIfEmpty() // left outer joins for above..
									select new DateShiftDTO
									{
										Id = b.BOEID,
										BOEStateID = b.BOEStateID,
										StartDate = b.BOEStartDate,
										EndDate = b.BOEEndDate,
										UpdateDate = b.UpdateDT,
										WorkspaceId = b.WorkspaceID,
										ClinId = xRef == null ? null : xRef.CLINID,
										WbsId = xRef == null ? null : xRef.WBSID,
									}).FirstOrDefault();

				return boe;
			}
		}

		/// <summary>
		/// Gets the required clin data by id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Clin data.</returns>
		public DateShiftDTO GetClinDateShiftDataById(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				DateShiftDTO clin = (from c in gbe.CLINs
									 where c.CLINID == id
									 select new DateShiftDTO
									 {
										 Id = c.CLINID,
										 StartDate = c.CLINStartDate,
										 EndDate = c.CLINEndDate,
										 UpdateDate = c.UpdateDT,
										 WorkspaceId = c.WorkspaceID,
									 }).FirstOrDefault();

				return clin;
			}
		}

		/// <summary>
		/// Gets the required boe children data by clin id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Boe children data.</returns>
		public ICollection<DateShiftDTO> GetClinChildrenByClinId(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<DateShiftDTO> boes = (from b in gbe.BOEs
										   join xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => x.CLINID == id) on b.BOEID equals xRef.BOEID
										   select new DateShiftDTO
										   {
											   Id = b.BOEID,
											   BOEStateID = b.BOEStateID,
											   StartDate = b.BOEStartDate,
											   EndDate = b.BOEEndDate,
											   UpdateDate = b.UpdateDT,
											   WorkspaceId = b.WorkspaceID,
											   ClinId = xRef.CLINID,
											   WbsId = xRef.WBSID,
											   Level = (int)Level.BOE,
											   DateShiftLevel = Level.BOE,
											   ParentId = id,
										   }).ToList();

				return boes;
			}
		}

		/// <summary>
		/// Get BOE Task Element by Id.
		/// </summary>
		/// <param name="id">Boe Task Element Id</param>
		/// <returns>Boe Task Element.</returns>
		public DateShiftDTO GetBoeTaskElementDateShiftDataById(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				DateShiftDTO boeTaskElement = (from bT in gbe.BOETaskElements
											   where bT.BOETaskElementID == id
											   select new DateShiftDTO
											   {
												   Id = bT.BOETaskElementID,
												   StartDate = bT.TaskStartDate,
												   EndDate = bT.TaskEndDate,
												   UpdateDate = bT.UpdateDT,
												   BoeId = bT.BOEID,
											   }).FirstOrDefault();
				// Get resource types for task element labors.
				if (boeTaskElement != null)
				{
					boeTaskElement.TaskElementLabors = (from lT in gbe.BOELaborTypes
														where lT.BOETaskElementID == id
														select new ResourceTypeDto
														{
															Id = lT.BOELaborTypeID,
															TaskElementId = lT.BOETaskElementID,
															ResourceID = lT.ResourceID,
															StartDateValue = lT.BOELaborTypeStartDate,
															EndDateValue = lT.BOELaborTypeEndDate,
															SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
															UpdateDate = lT.UpdateDT,
															SpreadCurveIDValue = lT.SpreadCurveID,
															LaborSpreadsIEnum = lT.BOELaborSpreads
																.Select(lS => new ResourceSpreadDto
																{
																	Id = lS.BOELaborSpreadID,
																	LaborSpreadDate = lS.LaborSpreadDate,
																	LaborSpreadValue = lS.LaborSpreadValue ?? 0,
																	LaborTypeId = lS.BOELaborTypeID
																}),

														}).ToCollection();

					if (boeTaskElement.TaskElementLabors != null)
					{
						foreach (ResourceTypeDto resourceType in boeTaskElement.TaskElementLabors)
						{
							resourceType.LaborSpreads = resourceType.LaborSpreadsIEnum.ToCollection();
							resourceType.LaborSpreadsIEnum = null;
						}
					}
				}

				return boeTaskElement;
			}
		}

		/// <summary>
		/// Gets the required boe task element data by boe id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Boe task element data.</returns>
		public ICollection<DateShiftDTO> GetBoeChildrenByBoeId(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<DateShiftDTO> taskElements = (from te in gbe.BOETaskElements
												   where te.BOEID == id
												   select new DateShiftDTO
												   {
													   Id = te.BOETaskElementID,
													   BoeId = te.BOEID,
													   BOETaskElementId = te.BOETaskElementID,
													   StartDate = te.TaskStartDate,
													   EndDate = te.TaskEndDate,
													   Level = (int)Level.Task,
													   DateShiftLevel = Level.Task,
													   ParentId = te.BOEID,
												   }).ToList();

				foreach (DateShiftDTO taskElement in taskElements)
				{
					// Get resource types for task element labors.
					taskElement.TaskElementLabors = (from lT in gbe.BOELaborTypes
													 where lT.BOETaskElementID == taskElement.Id
													 select new ResourceTypeDto
													 {
														 Id = lT.BOELaborTypeID,
														 TaskElementId = lT.BOETaskElementID,
														 ResourceID = lT.ResourceID,
														 StartDateValue = lT.BOELaborTypeStartDate,
														 EndDateValue = lT.BOELaborTypeEndDate,
														 SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
														 UpdateDate = lT.UpdateDT,
														 SpreadCurveIDValue = lT.SpreadCurveID,
														 LaborSpreadsIEnum = lT.BOELaborSpreads
															 .Select(lS => new ResourceSpreadDto
															 {
																 Id = lS.BOELaborSpreadID,
																 LaborSpreadDate = lS.LaborSpreadDate,
																 LaborSpreadValue = lS.LaborSpreadValue ?? 0,
																 LaborTypeId = lS.BOELaborTypeID
															 }),

													 }).ToCollection();

					if (taskElement.TaskElementLabors != null)
					{
						foreach (ResourceTypeDto resourceType in taskElement.TaskElementLabors)
						{
							resourceType.LaborSpreads = resourceType.LaborSpreadsIEnum.ToCollection();
							resourceType.LaborSpreadsIEnum = null;
						}
					}
				}

				return taskElements;
			}
		}

		/// <summary>
		/// Gets the required workspace data by id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Workspace data.</returns>
		public DateShiftDTO GetWorkspaceDateShiftDataById(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				DateShiftDTO workspace = (from w in gbe.Workspaces
										  where w.WorkspaceID == id
										  select new DateShiftDTO
										  {
											  Id = w.WorkspaceID,
											  StartDate = w.ContractStartDate,
											  EndDate = w.ContractEndDate,
											  UpdateDate = w.UpdateDT,
										  }).FirstOrDefault();

				return workspace;
			}
		}

		/// <summary>
		/// Gets the required workspace data by workspaceShortName.
		/// </summary>
		/// <param name="workspaceShortName">workspace Short Name</param>
		/// <returns>Workspace data.</returns>
		public DateShiftDTO GetWorkspaceDateShiftDataByShortname(string workspaceShortName)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				DateShiftDTO workspace = (from w in gbe.Workspaces
										  where w.WorkspaceShortName == workspaceShortName
										  select new DateShiftDTO
										  {
											  Id = w.WorkspaceID,
											  StartDate = w.ContractStartDate,
											  EndDate = w.ContractEndDate,
											  UpdateDate = w.UpdateDT,
										  }).FirstOrDefault();

				return workspace;
			}
		}

		/// <summary>
		/// Gets the required clin children data by workspace id.
		/// </summary>
		/// <param name="workspaceId">id</param>
		/// <returns>Clin children data.</returns>
		public ICollection<DateShiftDTO> GetClinByWorkspaceId(int workspaceId)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<DateShiftDTO> clins = (from c in gbe.CLINs
							 where c.WorkspaceID == workspaceId
							 select new DateShiftDTO
							 {
								 Id = c.CLINID,
								 StartDate = c.CLINStartDate,
								 EndDate = c.CLINEndDate,
								 UpdateDate = c.UpdateDT,
								 WorkspaceId = c.WorkspaceID,
								 Level = (int)Level.CLIN,
								 DateShiftLevel = Level.CLIN,
								 ParentId = workspaceId,
							 }).ToList();

				return clins;
			}
		}

		/// <summary>
		/// Gets the required boe data by workspace id.
		/// </summary>
		/// <param name="workspaceId">id</param>
		/// <returns>Boe data.</returns>
		public ICollection<DateShiftDTO> GetBoeByWorkspaceId(int workspaceId)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<DateShiftDTO> boes = (from b in gbe.BOEs
							where b.WorkspaceID == workspaceId
							join xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID.HasValue) on b.BOEID equals xRef.BOEID into temp
							from xRef in temp.DefaultIfEmpty() // left outer joins for above..
							select new DateShiftDTO
							{
								Id = b.BOEID,
								BOEStateID = b.BOEStateID,
								StartDate = b.BOEStartDate,
								EndDate = b.BOEEndDate,
								UpdateDate = b.UpdateDT,
								WorkspaceId = b.WorkspaceID,
								ClinId = xRef == null ? null : xRef.CLINID,
								WbsId = xRef == null ? null : xRef.WBSID,
								Level = (int)Level.BOE,
								DateShiftLevel = Level.BOE,
								ParentId = workspaceId,
							}).ToList();

				return boes;
			}
		}


	}
}