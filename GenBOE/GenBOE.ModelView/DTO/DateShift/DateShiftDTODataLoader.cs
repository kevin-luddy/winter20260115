// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
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
		/// The workspace version data loader.
		/// </summary>
		private readonly IWorkspaceVersionMetaDataDTODataLoader workspaceVersionMetaDataDTODataLoader;

		/// <summary>
		/// Default constructor
		/// </summary>
		public DateShiftDTODataLoader(IResourceSpreadLoader resourceSpreadLoader, ISkillMixDTOLoader skillMixLoader, ICommonDisclosureSMDTODataLoader commonDisclosureLoader, IWorkspaceVersionMetaDataDTODataLoader workspaceVersionMetaDataDTODataLoader)
		{
			this.Log = new Logger(typeof(DateShiftDTODataLoader));
			this.resourceSpreadLoader = resourceSpreadLoader;
			this.skillMixLoader = skillMixLoader;
			this.commonDisclosureLoader = commonDisclosureLoader;
			this.workspaceVersionMetaDataDTODataLoader = workspaceVersionMetaDataDTODataLoader;
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

			// Create a local dictionary to store unique date shift objects
			Dictionary<(int, Level), DateShiftDTO> uniqueDateShiftObjects = new Dictionary<(int, Level), DateShiftDTO>();

			foreach (DateShiftDTO dateShiftDTO in dateShiftDTOs)
			{
				FlattenListFromChildrenObjects(dateShiftDTO, uniqueDateShiftObjects);
			}

			foreach (DateShiftDTO dateShiftDTOtoUpdate in uniqueDateShiftObjects.Values)
			{
				// Update any labor spreads.
				if (dateShiftDTOtoUpdate.LaborSpreads.Any() && dateShiftDTOtoUpdate.DateShiftLevel == Level.Labor)
				{
					resourceSpreadLoader.BulkSave(dateShiftDTOtoUpdate.LaborSpreads);
				}

				// Update any skill mixes and common disclosure for BOE Task Elements.
				if (dateShiftDTOtoUpdate.BOETaskElementId != null && dateShiftDTOtoUpdate.DateShiftLevel == Level.Task)
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
			this.UpdateDateShifts(uniqueDateShiftObjects.Values);
		}

		/// <summary>
		/// Gets the date shift child DTOs recursively.
		/// </summary>
		/// <param name="dateShiftDTO">Date shift dtos.</param>
		/// <param name="uniqueDateShiftObjects">Dictionary to store unique date shift objects.</param>
		private void FlattenListFromChildrenObjects(DateShiftDTO dateShiftDTO, Dictionary<(int, Level), DateShiftDTO> uniqueDateShiftObjects)
		{
			if (dateShiftDTO == null)
			{
				throw new ArgumentNullException(nameof(dateShiftDTO));
			}

			if (!uniqueDateShiftObjects.ContainsKey((dateShiftDTO.Id, dateShiftDTO.DateShiftLevel)))
			{
				uniqueDateShiftObjects.Add((dateShiftDTO.Id, dateShiftDTO.DateShiftLevel), dateShiftDTO);
			}

			if (dateShiftDTO.Children != null)
			{
				foreach (DateShiftDTO child in dateShiftDTO.Children)
				{
					FlattenListFromChildrenObjects(child, uniqueDateShiftObjects);
				}
			}
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
				case Level.Workspace:
					dateShift = GetWorkspaceDateShiftDataById(id);
					break;
				case Level.CLIN:
					dateShift = GetClinDateShiftDataById(id, true);
					//dateShift.Parent = GetWorkspaceDateShiftDataById(dateShift.WorkspaceId);
					break;
				case Level.BOE:
					dateShift = GetBoeDateShiftDataById(id, true);

					// Sets parent to be CLIN if the BOE uses a clin, otherwise, if NO CLIN then set Workspace as parent.
					if (dateShift.ClinId != null)
					{
						dateShift.Parent = GetClinDateShiftDataById(dateShift.ParentId.Value, false);
					}
					else
					{
						//dateShift.Parent = GetWorkspaceDateShiftDataById(dateShift.WorkspaceId);
					}
					break;
				case Level.Task:
					dateShift = GetBoeTaskElementDateShiftDataById(id);
					dateShift.Parent = GetBoeDateShiftDataById(dateShift.BoeId, false);
					break;
				default:
					throw new NotSupportedException($"Unsupported level: {level}");
			}

			return dateShift;
		}

		/// <summary>
		/// Gets the required boe data by id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Boe data.</returns>
		public DateShiftDTO GetBoeDateShiftDataById(int id, bool getChildren)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				DateShiftDTO boeDateShift = (from b in gbe.BOEs
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
												 ParentId = xRef.CLINID ?? b.WorkspaceID,
												 DateShiftLevel = Level.BOE
											 }).FirstOrDefault();

				if (getChildren)
				{
					boeDateShift.Children.AddRange(GetBoeTaskElementDateShiftDataByBoeId(id));
				}

				return boeDateShift;
			}
		}

		/// <summary>
		/// Gets the required clin data by id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Clin data.</returns>
		public DateShiftDTO GetClinDateShiftDataById(int id, bool getChildren)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				DateShiftDTO clinDateShift = (from c in gbe.CLINs
											  where c.CLINID == id
											  select new DateShiftDTO
											  {
												  Id = c.CLINID,
												  StartDate = c.CLINStartDate,
												  EndDate = c.CLINEndDate,
												  UpdateDate = c.UpdateDT,
												  WorkspaceId = c.WorkspaceID,
												  ParentId = c.WorkspaceID,
												  DateShiftLevel = Level.CLIN
											  }).FirstOrDefault();

				if (getChildren)
				{
					clinDateShift.Children.AddRange(GetBoeDateShiftDataByClinId(clinDateShift.Id));
				}

				return clinDateShift;
			}
		}

		/// <summary>
		/// Gets the required boe children data by clin id.
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>Boe children data.</returns>
		public ICollection<DateShiftDTO> GetBoeDateShiftDataByClinId(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<DateShiftDTO> boeDateShifts = (from b in gbe.BOEs
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
														DateShiftLevel = Level.BOE,
														ParentId = xRef.CLINID,
													}).ToList();

				foreach (DateShiftDTO boe in boeDateShifts)
				{
					boe.Children.AddRange(GetBoeTaskElementDateShiftDataByBoeId(boe.Id));
				}

				return boeDateShifts;
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

				DateShiftDTO boeTaskElementDateShift = (from bT in gbe.BOETaskElements
														where bT.BOETaskElementID == id
														select new DateShiftDTO
														{
															Id = bT.BOETaskElementID,
															BOETaskElementId = bT.BOETaskElementID,
															StartDate = bT.TaskStartDate,
															EndDate = bT.TaskEndDate,
															UpdateDate = bT.UpdateDT,
															BoeId = bT.BOEID,
															DateShiftLevel = Level.Task,
															ParentId = bT.BOEID
														}).FirstOrDefault();

				boeTaskElementDateShift.SkillMixTable = skillMixLoader.GetByBOETaskElementID(id).Select(dto => new SkillMixModelView(dto)).ToList();
				boeTaskElementDateShift.CommonDisclosureTable = commonDisclosureLoader.GetByBOETaskElementID(id).Select(dto => new CommonDisclosureModelView(dto)).ToList();

				// Get resource types for task element labors.
				if (boeTaskElementDateShift != null)
				{
					boeTaskElementDateShift.TaskElementLabors = (from lT in gbe.BOELaborTypes
																 where lT.BOETaskElementID == id
																 select new ResourceTypeDto
																 {
																	 Id = lT.BOELaborTypeID,
																	 TaskElementId = lT.BOETaskElementID,
																	 ResourceID = lT.ResourceID,
																	 StartDate = lT.BOELaborTypeStartDate,
																	 EndDate = lT.BOELaborTypeEndDate,
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

					if (boeTaskElementDateShift.TaskElementLabors != null)
					{
						foreach (ResourceTypeDto resourceType in boeTaskElementDateShift.TaskElementLabors)
						{
							resourceType.LaborSpreads = resourceType.LaborSpreadsIEnum.ToCollection();
							resourceType.LaborSpreadsIEnum = null;

							boeTaskElementDateShift.Children.Add(new DateShiftDTO()
							{
								Id = resourceType.Id,
								StartDate = resourceType.StartDateValue,
								EndDate = resourceType.EndDateValue,
								UpdateDate = resourceType.UpdateDate,
								DateShiftLevel = Level.Labor,
								ParentId = resourceType.TaskElementId,
								LaborSpreads = resourceType.LaborSpreads,
								SpreadCurveID = resourceType.SpreadCurveID,
								SpreadType = resourceType.SpreadType,
								HasSpread = resourceType.HasSpread
							});
						}
					}
				}

				return boeTaskElementDateShift;
			}
		}

		/// <summary>
		/// Gets the required boe task element data by boe id.
		/// </summary>
		/// <param name="boeId">id</param>
		/// <returns>Boe task element data.</returns>
		public ICollection<DateShiftDTO> GetBoeTaskElementDateShiftDataByBoeId(int boeId)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<DateShiftDTO> boeTaskElementDateShifts = (from te in gbe.BOETaskElements
															   where te.BOEID == boeId
															   select new DateShiftDTO
															   {
																   Id = te.BOETaskElementID,
																   BoeId = te.BOEID,
																   BOETaskElementId = te.BOETaskElementID,
																   StartDate = te.TaskStartDate,
																   EndDate = te.TaskEndDate,
																   DateShiftLevel = Level.Task,
																   UpdateDate = te.UpdateDT,
																   ParentId = te.BOEID,
															   }).ToList();

				foreach (DateShiftDTO taskElement in boeTaskElementDateShifts)
				{
					taskElement.SkillMixTable = skillMixLoader.GetByBOETaskElementID(taskElement.Id).Select(dto => new SkillMixModelView(dto)).ToList();
					taskElement.CommonDisclosureTable = commonDisclosureLoader.GetByBOETaskElementID(taskElement.Id).Select(dto => new CommonDisclosureModelView(dto)).ToList();

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

							taskElement.Children.Add(new DateShiftDTO()
							{
								Id = resourceType.Id,
								StartDate = resourceType.StartDateValue,
								EndDate = resourceType.EndDateValue,
								UpdateDate = resourceType.UpdateDate,
								DateShiftLevel = Level.Labor,
								ParentId = resourceType.TaskElementId,
								LaborSpreads = resourceType.LaborSpreads,
								SpreadCurveID = resourceType.SpreadCurveID,
								SpreadType = resourceType.SpreadType,
								HasSpread = resourceType.HasSpread
							}
							);
						}
					}
				}

				return boeTaskElementDateShifts;
			}
		}

		/// <summary>
		/// Gets the workspace date shift object and sets up all of its children (and nested children).
		/// </summary>
		/// <param name="id">Workspace Id.</param>
		/// <returns>Completel workspace date shift object.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public DateShiftDTO GetWorkspaceDateShiftDataById(int id)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				// Retrieve workspace data with CLINs, BOEs, and BOE Task Elements
				var workspaceData = (from w in gbe.Workspaces
									 join c in gbe.CLINs on w.WorkspaceID equals c.WorkspaceID into clinGroup
									 from c in clinGroup.DefaultIfEmpty()
									 join xRef in gbe.WBS_CLIN_BOE_XREF on c == null ? (int?)null : c.CLINID equals xRef.CLINID into xRefGroup
									 from xRef in xRefGroup.DefaultIfEmpty()
									 join b in gbe.BOEs on xRef == null ? w.WorkspaceID : xRef.BOEID equals b.WorkspaceID into boeGroup
									 from b in boeGroup.DefaultIfEmpty()
									 join te in gbe.BOETaskElements on b.BOEID equals te.BOEID into teGroup
									 from te in teGroup.DefaultIfEmpty()
									 where w.WorkspaceID == id
									 select new
									 {
										 Workspace = w,
										 CLINs = clinGroup,
										 BOEs = boeGroup,
										 BOETaskElements = teGroup,
										 XRefs = xRefGroup
									 }).FirstOrDefault();

				// Construct DateShiftDTO hierarchy
				DateShiftDTO workspaceDateShift = new DateShiftDTO
				{
					Id = workspaceData.Workspace.WorkspaceID,
					StartDate = workspaceData.Workspace.ContractStartDate,
					EndDate = workspaceData.Workspace.ContractEndDate,
					UpdateDate = workspaceData.Workspace.UpdateDT,
					DateShiftLevel = Level.Workspace,
				};

				workspaceDateShift.WorkspaceVersionMetaData = workspaceVersionMetaDataDTODataLoader.GetByWorkspaceID(workspaceDateShift.WorkspaceId);

				// Add CLINs as workspace children
				foreach (CLIN clin in workspaceData.CLINs)
				{
					DateShiftDTO clinDateShift = new DateShiftDTO
					{
						Id = clin.CLINID,
						StartDate = clin.CLINStartDate,
						EndDate = clin.CLINEndDate,
						UpdateDate = clin.UpdateDT,
						WorkspaceId = clin.WorkspaceID,
						DateShiftLevel = Level.CLIN,
						ParentId = workspaceDateShift.Id,
					};

					workspaceDateShift.Children.Add(clinDateShift);

					// Add BOEs as CLIN children
					foreach (WBS_CLIN_BOE_XREF xRef in workspaceData.XRefs.Where(x => x.CLINID == clin.CLINID))
					{
						BOE boe = workspaceData.BOEs.FirstOrDefault(b => b.BOEID == xRef.BOEID);
						if (boe != null)
						{
							DateShiftDTO boeDateShift = new DateShiftDTO
							{
								Id = boe.BOEID,
								BOEStateID = boe.BOEStateID,
								StartDate = boe.BOEStartDate,
								EndDate = boe.BOEEndDate,
								UpdateDate = boe.UpdateDT,
								WorkspaceId = boe.WorkspaceID,
								ClinId = clin.CLINID,
								DateShiftLevel = Level.BOE,
								ParentId = clinDateShift.Id,
							};

							clinDateShift.Children.Add(boeDateShift);

							// Add BOE Task Elements as BOE children
							foreach (BOETaskElement te in workspaceData.BOETaskElements.Where(t => t.BOEID == boe.BOEID))
							{
								boeDateShift.Children.Add(new DateShiftDTO
								{
									Id = te.BOETaskElementID,
									BoeId = te.BOEID,
									BOETaskElementId = te.BOETaskElementID,
									StartDate = te.TaskStartDate,
									EndDate = te.TaskEndDate,
									DateShiftLevel = Level.Task,
									ParentId = boeDateShift.Id,
								});
							}
						}
					}
				}

				// Add BOEs without CLIN as workspace children
				foreach (BOE boe in workspaceData.BOEs.Where(b => !workspaceData.XRefs.Any(x => x.BOEID == b.BOEID)))
				{
					DateShiftDTO boeDateShift = new DateShiftDTO
					{
						Id = boe.BOEID,
						BOEStateID = boe.BOEStateID,
						StartDate = boe.BOEStartDate,
						EndDate = boe.BOEEndDate,
						UpdateDate = boe.UpdateDT,
						WorkspaceId = boe.WorkspaceID,
						DateShiftLevel = Level.BOE,
						ParentId = workspaceDateShift.Id,
					};

					workspaceDateShift.Children.Add(boeDateShift);

					// Add BOE Task Elements as BOE children
					foreach (BOETaskElement te in workspaceData.BOETaskElements.Where(t => t.BOEID == boe.BOEID))
					{
						boeDateShift.Children.Add(new DateShiftDTO
						{
							Id = te.BOETaskElementID,
							BoeId = te.BOEID,
							BOETaskElementId = te.BOETaskElementID,
							StartDate = te.TaskStartDate,
							EndDate = te.TaskEndDate,
							DateShiftLevel = Level.Task,
							ParentId = boeDateShift.Id,
						});
					}
				}

				return workspaceDateShift;
			}
		}
	}
}