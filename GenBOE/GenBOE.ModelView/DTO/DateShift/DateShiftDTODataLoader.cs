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
					dateShift = GetWorkspaceDateShiftDataById(id, true);
					break;
				case Level.CLIN:
					dateShift = GetClinDateShiftDataById(id, true);
					dateShift.Parent = GetWorkspaceDateShiftDataById(dateShift.WorkspaceId, false);
					break;
				case Level.BOE:
					dateShift = GetBoeDateShiftDataById(id, true);
					// Sets parent to be CLIN if the BOE uses a clin, otherwise, if NO CLIN then set Workspace as parent.
					if (dateShift.ClinId != null)
					{
						dateShift.Parent = GetClinDateShiftDataById(dateShift.ClinId.Value, false);
					}
					else
					{
						dateShift.Parent = GetWorkspaceDateShiftDataById(dateShift.WorkspaceId, false);
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
		/// Gets the complete date shift data by workspace Id and sets up the hierarchy.
		/// </summary>
		/// <param name="workspaceId">Workspace id.</param>
		/// <param name="getChildren">Get children objects.</param>
		/// <returns>Complete workspace date shift data.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public DateShiftDTO GetWorkspaceDateShiftDataById(int workspaceId, bool getChildren)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute
				Workspace workspace = (from w in gbe.Workspaces
									   where w.WorkspaceID == workspaceId
									   select w).FirstOrDefault();

				List<CLIN> clinData = (from c in gbe.CLINs
									   where c.WorkspaceID == workspaceId
									   select c).ToList();

				var boeData = (from b in gbe.BOEs
							   where b.WorkspaceID == workspaceId
							   join xRef in gbe.WBS_CLIN_BOE_XREF on b.BOEID equals xRef.BOEID into xRefGroup
							   from xRef in xRefGroup.DefaultIfEmpty()
							   join c in gbe.CLINs on xRef.CLINID equals c.CLINID into clinGroup
							   from c in clinGroup.DefaultIfEmpty()
							   select new
							   {
								   BOE = b,
								   CLIN = c
							   }).ToList();

				var teData = (from te in gbe.BOETaskElements
											   join b in gbe.BOEs on te.BOEID equals b.BOEID
											   where b.WorkspaceID == workspaceId
											   select new 
											   {
												   te.BOETaskElementID,
												   te.BOEID,
												   te.TaskStartDate,
												   te.TaskEndDate,
												   te.UpdateDT
											   }
											   ).ToList();

				List<Models.SkillMix> skillMixes = (from sm in gbe.SkillMixes
													join b in gbe.BOEs on sm.BOEID equals b.BOEID
													where b.WorkspaceID == workspaceId
													select sm).ToList();

				List<CommonDisclosureSkillMix> commonDisclosures = (from cd in gbe.CommonDisclosureSkillMixes
														   join b in gbe.BOEs on cd.BOEID equals b.BOEID
													where b.WorkspaceID == workspaceId
													select cd).ToList();

				List<BOELaborType> boeLaborTypes = (from blt in gbe.BOELaborTypes
													join te in gbe.BOETaskElements on blt.BOETaskElementID equals te.BOETaskElementID
													join b in gbe.BOEs on te.BOEID equals b.BOEID
													where b.WorkspaceID == workspaceId
													select blt).ToList();

				List<BOELaborSpread> boeLaborSpreads = (from bls in gbe.BOELaborSpreads
														join blt in gbe.BOELaborTypes on bls.BOELaborTypeID equals blt.BOELaborTypeID
														join te in gbe.BOETaskElements on blt.BOETaskElementID equals te.BOETaskElementID
														join b in gbe.BOEs on te.BOEID equals b.BOEID
														where b.WorkspaceID == workspaceId
														select bls).ToList(); 

				// Construct DateShiftDTO hierarchy
				DateShiftDTO workspaceDateShift = new DateShiftDTO
				{
					Id = workspace.WorkspaceID,
					StartDate = workspace.ContractStartDate,
					EndDate = workspace.ContractEndDate,
					UpdateDate = workspace.UpdateDT,
					DateShiftLevel = Level.Workspace,
				};

				if (getChildren)
				{
					// Add CLINs as workspace children
					foreach (CLIN clin in clinData)
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

						workspaceDateShift.WorkspaceVersionMetaData = workspaceVersionMetaDataDTODataLoader.GetByWorkspaceID(workspaceDateShift.WorkspaceId);

						// Add BOEs as CLIN children
						List<BOE> clinBOEs = boeData.Where(b => b.CLIN != null && b.CLIN.CLINID == clin.CLINID).Select(b => b.BOE).ToList();

						foreach (BOE boe in clinBOEs)
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
							var boeTaskElements = teData.Where(t => t.BOEID == boe.BOEID).ToList();

							foreach (var te in boeTaskElements)
							{
								DateShiftDTO taskElementDateShift = new DateShiftDTO
								{
									Id = te.BOETaskElementID,
									BoeId = te.BOEID,
									BOETaskElementId = te.BOETaskElementID,
									StartDate = te.TaskStartDate,
									EndDate = te.TaskEndDate,
									UpdateDate = te.UpdateDT,
									DateShiftLevel = Level.Task,
									ParentId = boeDateShift.Id,
								};

								// Get skill mixes and common disclosures for task element
								List<SkillMixDTO> taskElementSkillMixes = skillMixes.Where(sm => sm.BOETaskElementID == te.BOETaskElementID)
									.Select(sm => new SkillMixDTO
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

								DoPostProcessing(taskElementSkillMixes);

								List<CommonDisclosureSkillMixDTO> taskElementCommonDisclosures = commonDisclosures.Where(cdsm => cdsm.BOETaskElementID == te.BOETaskElementID)
									.Select(cdsm => new CommonDisclosureSkillMixDTO
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

								DoPostProcessing(taskElementCommonDisclosures);

								// Assign the skill mix and common disclosure
								taskElementDateShift.SkillMixTable = taskElementSkillMixes.Select(dto => new SkillMixModelView(dto)).ToList();
								taskElementDateShift.CommonDisclosureTable = taskElementCommonDisclosures.Select(dto => new CommonDisclosureModelView(dto)).ToList();

								// Get resource types for task element labors.
								Collection<ResourceTypeDto> taskElementLabors = boeLaborTypes.Where(lT => lT.BOETaskElementID == te.BOETaskElementID)
									.Select(lT => new ResourceTypeDto
									{
										Id = lT.BOELaborTypeID,
										TaskElementId = lT.BOETaskElementID,
										ResourceID = lT.ResourceID,
										StartDateValue = lT.BOELaborTypeStartDate,
										EndDateValue = lT.BOELaborTypeEndDate,
										SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
										UpdateDate = lT.UpdateDT,
										SpreadCurveIDValue = lT.SpreadCurveID,
										LaborSpreadsIEnum = boeLaborSpreads.Where(lS => lS.BOELaborTypeID == lT.BOELaborTypeID)
											.Select(lS => new ResourceSpreadDto
											{
												Id = lS.BOELaborSpreadID,
												LaborSpreadDate = lS.LaborSpreadDate,
												LaborSpreadValue = lS.LaborSpreadValue ?? 0,
												LaborTypeId = lS.BOELaborTypeID
											}),
									}).ToCollection();

								taskElementDateShift.TaskElementLabors = taskElementLabors;

								if (taskElementLabors != null)
								{
									foreach (ResourceTypeDto resourceType in taskElementLabors)
									{
										resourceType.LaborSpreads = resourceType.LaborSpreadsIEnum.ToCollection();
										resourceType.LaborSpreadsIEnum = null;

										taskElementDateShift.Children.Add(new DateShiftDTO()
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

								boeDateShift.Children.Add(taskElementDateShift);
							}
						}
					}

					// Add BOEs without CLIN as workspace children
					List<BOE> workspaceBOEs = boeData.Where(b => b.CLIN == null).Select(b => b.BOE).ToList();

					foreach (BOE boe in workspaceBOEs)
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
						var boeTaskElements = teData.Where(t => t.BOEID == boe.BOEID).ToList();

						foreach (var te in boeTaskElements)
						{
							DateShiftDTO taskElementDateShift = new DateShiftDTO
							{
								Id = te.BOETaskElementID,
								BoeId = te.BOEID,
								BOETaskElementId = te.BOETaskElementID,
								StartDate = te.TaskStartDate,
								EndDate = te.TaskEndDate,
								UpdateDate = te.UpdateDT,
								DateShiftLevel = Level.Task,
								ParentId = boeDateShift.Id,
							};

							// Get skill mixes and common disclosures for task element
							List<SkillMixDTO> taskElementSkillMixes = skillMixes.Where(sm => sm.BOETaskElementID == te.BOETaskElementID)
								.Select(sm => new SkillMixDTO
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

							DoPostProcessing(taskElementSkillMixes);

							List<CommonDisclosureSkillMixDTO> taskElementCommonDisclosures = commonDisclosures.Where(cdsm => cdsm.BOETaskElementID == te.BOETaskElementID)
								.Select(cdsm => new CommonDisclosureSkillMixDTO
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

							DoPostProcessing(taskElementCommonDisclosures);

							// Assign the skill mix and common disclosure
							taskElementDateShift.SkillMixTable = taskElementSkillMixes.Select(dto => new SkillMixModelView(dto)).ToList();
							taskElementDateShift.CommonDisclosureTable = taskElementCommonDisclosures.Select(dto => new CommonDisclosureModelView(dto)).ToList();

							// Get resource types for task element labors.
							Collection<ResourceTypeDto> taskElementLabors = boeLaborTypes.Where(lT => lT.BOETaskElementID == te.BOETaskElementID)
								.Select(lT => new ResourceTypeDto
								{
									Id = lT.BOELaborTypeID,
									TaskElementId = lT.BOETaskElementID,
									ResourceID = lT.ResourceID,
									StartDateValue = lT.BOELaborTypeStartDate,
									EndDateValue = lT.BOELaborTypeEndDate,
									SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
									UpdateDate = lT.UpdateDT,
									SpreadCurveIDValue = lT.SpreadCurveID,
									LaborSpreadsIEnum = boeLaborSpreads.Where(lS => lS.BOELaborTypeID == lT.BOELaborTypeID)
										.Select(lS => new ResourceSpreadDto
										{
											Id = lS.BOELaborSpreadID,
											LaborSpreadDate = lS.LaborSpreadDate,
											LaborSpreadValue = lS.LaborSpreadValue ?? 0,
											LaborTypeId = lS.BOELaborTypeID
										}),
								}).ToCollection();

							taskElementDateShift.TaskElementLabors = taskElementLabors;

							if (taskElementLabors != null)
							{
								foreach (ResourceTypeDto resourceType in taskElementLabors)
								{
									resourceType.LaborSpreads = resourceType.LaborSpreadsIEnum.ToCollection();
									resourceType.LaborSpreadsIEnum = null;

									taskElementDateShift.Children.Add(new DateShiftDTO()
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

							boeDateShift.Children.Add(taskElementDateShift);
						}
					}
				}

				return workspaceDateShift;
			}
		}

		/// <summary>
		/// Gets the required clin data by id.
		/// </summary>
		/// <param name="clinId">clinId</param>
		/// <param name="getChildren">Should we get children objects?</param>
		/// <returns>Clin data.</returns>
		public DateShiftDTO GetClinDateShiftDataById(int clinId, bool getChildren)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<Models.SkillMix> skillMixes = (from sm in gbe.SkillMixes
													join cl in gbe.WBS_CLIN_BOE_XREF on sm.BOEID equals cl.BOEID
													where cl.CLINID == clinId 
													select sm).ToList();

				List<CommonDisclosureSkillMix> commonDisclosures = (from cd in gbe.CommonDisclosureSkillMixes
																	join cl in gbe.WBS_CLIN_BOE_XREF on cd.BOEID equals cl.BOEID
																	where cl.CLINID == clinId
																	select cd).ToList();

				List<BOELaborType> boeLaborTypes = (from blt in gbe.BOELaborTypes
													join te in gbe.BOETaskElements on blt.BOETaskElementID equals te.BOETaskElementID
													join cl in gbe.WBS_CLIN_BOE_XREF on te.BOEID equals cl.BOEID
													where cl.CLINID == clinId
													select blt).ToList();

				List<BOELaborSpread> boeLaborSpreads = (from bls in gbe.BOELaborSpreads
														join blt in gbe.BOELaborTypes on bls.BOELaborTypeID equals blt.BOELaborTypeID
														join te in gbe.BOETaskElements on blt.BOETaskElementID equals te.BOETaskElementID
														join cl in gbe.WBS_CLIN_BOE_XREF on te.BOEID equals cl.BOEID
														where cl.CLINID == clinId
														select bls).ToList();

				DateShiftDTO clinDateShift = (from c in gbe.CLINs
											  where c.CLINID == clinId
											  select new DateShiftDTO
											  {
												  Id = c.CLINID,
												  StartDate = c.CLINStartDate,
												  EndDate = c.CLINEndDate,
												  UpdateDate = c.UpdateDT,
												  WorkspaceId = c.WorkspaceID,
												  DateShiftLevel = Level.CLIN,
												  ParentId = c.WorkspaceID,
											  }).FirstOrDefault();

				if (getChildren)
				{
					// Add BOEs as CLIN children
					foreach (WBS_CLIN_BOE_XREF xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => x.CLINID == clinDateShift.Id))
					{
						BOE boe = gbe.BOEs.FirstOrDefault(b => b.BOEID == xRef.BOEID);
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
								ClinId = clinDateShift.Id,
								DateShiftLevel = Level.BOE,
								ParentId = clinDateShift.Id,
							};

							clinDateShift.Children.Add(boeDateShift);

							// Add BOE Task Elements as BOE children
							foreach (BOETaskElement te in gbe.BOETaskElements.Where(t => t.BOEID == boe.BOEID))
							{
								DateShiftDTO taskElementDateShift = new DateShiftDTO
								{
									Id = te.BOETaskElementID,
									BoeId = te.BOEID,
									BOETaskElementId = te.BOETaskElementID,
									StartDate = te.TaskStartDate,
									EndDate = te.TaskEndDate,
									UpdateDate = te.UpdateDT,
									DateShiftLevel = Level.Task,
									ParentId = boeDateShift.Id,
								};

								// Get skill mixes and common disclosures for task element
								List<SkillMixDTO> taskElementSkillMixes = skillMixes.Where(sm => sm.BOETaskElementID == te.BOETaskElementID)
									.Select(sm => new SkillMixDTO
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

								DoPostProcessing(taskElementSkillMixes);

								List<CommonDisclosureSkillMixDTO> taskElementCommonDisclosures = commonDisclosures.Where(cdsm => cdsm.BOETaskElementID == te.BOETaskElementID)
									.Select(cdsm => new CommonDisclosureSkillMixDTO
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

								DoPostProcessing(taskElementCommonDisclosures);

								// Assign the skill mix and common disclosure
								taskElementDateShift.SkillMixTable = taskElementSkillMixes.Select(dto => new SkillMixModelView(dto)).ToList();
								taskElementDateShift.CommonDisclosureTable = taskElementCommonDisclosures.Select(dto => new CommonDisclosureModelView(dto)).ToList();

								// Get resource types for task element labors.
								Collection<ResourceTypeDto> taskElementLabors = boeLaborTypes.Where(lT => lT.BOETaskElementID == te.BOETaskElementID)
									.Select(lT => new ResourceTypeDto
									{
										Id = lT.BOELaborTypeID,
										TaskElementId = lT.BOETaskElementID,
										ResourceID = lT.ResourceID,
										StartDateValue = lT.BOELaborTypeStartDate,
										EndDateValue = lT.BOELaborTypeEndDate,
										SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
										UpdateDate = lT.UpdateDT,
										SpreadCurveIDValue = lT.SpreadCurveID,
										LaborSpreadsIEnum = boeLaborSpreads.Where(lS => lS.BOELaborTypeID == lT.BOELaborTypeID)
											.Select(lS => new ResourceSpreadDto
											{
												Id = lS.BOELaborSpreadID,
												LaborSpreadDate = lS.LaborSpreadDate,
												LaborSpreadValue = lS.LaborSpreadValue ?? 0,
												LaborTypeId = lS.BOELaborTypeID
											}),
									}).ToCollection();

								taskElementDateShift.TaskElementLabors = taskElementLabors;

								if (taskElementLabors != null)
								{
									foreach (ResourceTypeDto resourceType in taskElementLabors)
									{
										resourceType.LaborSpreads = resourceType.LaborSpreadsIEnum.ToCollection();
										resourceType.LaborSpreadsIEnum = null;

										taskElementDateShift.Children.Add(new DateShiftDTO()
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

								boeDateShift.Children.Add(taskElementDateShift);
							}
						}
					}
				}

				return clinDateShift;
			}
		}

		/// <summary>
		/// Gets the required boe data by id.
		/// </summary>
		/// <param name="id">id</param>
		/// <param name="getChildren">Get child objects.</param>
		/// <returns>Boe data.</returns>
		public DateShiftDTO GetBoeDateShiftDataById(int id, bool getChildren)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<Models.SkillMix> skillMixes = (from sm in gbe.SkillMixes
													where sm.BOEID == id
													select sm).ToList();

				List<CommonDisclosureSkillMix> commonDisclosures = (from cd in gbe.CommonDisclosureSkillMixes
																	where cd.BOEID == id
																	select cd).ToList();

				List<BOELaborType> boeLaborTypes = (from blt in gbe.BOELaborTypes
													join te in gbe.BOETaskElements on blt.BOETaskElementID equals te.BOETaskElementID
													where te.BOEID == id
													select blt).ToList();

				List<BOELaborSpread> boeLaborSpreads = (from bls in gbe.BOELaborSpreads
														join blt in gbe.BOELaborTypes on bls.BOELaborTypeID equals blt.BOELaborTypeID
														join te in gbe.BOETaskElements on blt.BOETaskElementID equals te.BOETaskElementID
														where te.BOEID == id
														select bls).ToList();

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
					// Add BOE Task Elements as BOE children
					foreach (BOETaskElement te in gbe.BOETaskElements.Where(t => t.BOEID == boeDateShift.Id))
					{
						DateShiftDTO taskElementDateShift = new DateShiftDTO
						{
							Id = te.BOETaskElementID,
							BoeId = te.BOEID,
							BOETaskElementId = te.BOETaskElementID,
							StartDate = te.TaskStartDate,
							EndDate = te.TaskEndDate,
							UpdateDate = te.UpdateDT,
							DateShiftLevel = Level.Task,
							ParentId = boeDateShift.Id,
						};

						// Get skill mixes and common disclosures for task element
						List<SkillMixDTO> taskElementSkillMixes = skillMixes.Where(sm => sm.BOETaskElementID == te.BOETaskElementID)
							.Select(sm => new SkillMixDTO
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

						DoPostProcessing(taskElementSkillMixes);

						List<CommonDisclosureSkillMixDTO> taskElementCommonDisclosures = commonDisclosures.Where(cdsm => cdsm.BOETaskElementID == te.BOETaskElementID)
							.Select(cdsm => new CommonDisclosureSkillMixDTO
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

						DoPostProcessing(taskElementCommonDisclosures);

						// Assign the skill mix and common disclosure
						taskElementDateShift.SkillMixTable = taskElementSkillMixes.Select(dto => new SkillMixModelView(dto)).ToList();
						taskElementDateShift.CommonDisclosureTable = taskElementCommonDisclosures.Select(dto => new CommonDisclosureModelView(dto)).ToList();

						// Get resource types for task element labors.
						Collection<ResourceTypeDto> taskElementLabors = boeLaborTypes.Where(lT => lT.BOETaskElementID == te.BOETaskElementID)
							.Select(lT => new ResourceTypeDto
							{
								Id = lT.BOELaborTypeID,
								TaskElementId = lT.BOETaskElementID,
								ResourceID = lT.ResourceID,
								StartDateValue = lT.BOELaborTypeStartDate,
								EndDateValue = lT.BOELaborTypeEndDate,
								SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
								UpdateDate = lT.UpdateDT,
								SpreadCurveIDValue = lT.SpreadCurveID,
								LaborSpreadsIEnum = boeLaborSpreads.Where(lS => lS.BOELaborTypeID == lT.BOELaborTypeID)
											.Select(lS => new ResourceSpreadDto
											{
												Id = lS.BOELaborSpreadID,
												LaborSpreadDate = lS.LaborSpreadDate,
												LaborSpreadValue = lS.LaborSpreadValue ?? 0,
												LaborTypeId = lS.BOELaborTypeID
											}),
							}).ToCollection();

						boeDateShift.TaskElementLabors = taskElementLabors;

						if (taskElementLabors != null)
						{
							foreach (ResourceTypeDto resourceType in taskElementLabors)
							{
								resourceType.LaborSpreads = resourceType.LaborSpreadsIEnum.ToCollection();
								resourceType.LaborSpreadsIEnum = null;

								boeDateShift.Children.Add(new DateShiftDTO()
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

						boeDateShift.Children.Add(taskElementDateShift);
					}
				}

				return boeDateShift;
			}
		}

		/// <summary>
		/// Get BOE Task Element by Id.
		/// </summary>
		/// <param name="taskId">Boe Task Element Id</param>
		/// <returns>Boe Task Element.</returns>
		public DateShiftDTO GetBoeTaskElementDateShiftDataById(int taskId)
		{
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

				List<Models.SkillMix> skillMixes = (from sm in gbe.SkillMixes
													where sm.BOETaskElementID == taskId
													select sm).ToList();

				List<CommonDisclosureSkillMix> commonDisclosures = (from cd in gbe.CommonDisclosureSkillMixes
																	where cd.BOETaskElementID == taskId
																	select cd).ToList();

				List<BOELaborType> boeLaborTypes = (from blt in gbe.BOELaborTypes
													where blt.BOETaskElementID == taskId
													select blt).ToList();

				List<BOELaborSpread> boeLaborSpreads = (from bls in gbe.BOELaborSpreads
														join blt in gbe.BOELaborTypes on bls.BOELaborTypeID equals blt.BOELaborTypeID
														where blt.BOETaskElementID == taskId
														select bls).ToList();

				DateShiftDTO boeTaskElementDateShift = (from bT in gbe.BOETaskElements
														where bT.BOETaskElementID == taskId
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

				// Get skill mixes and common disclosures for task element
				List<SkillMixDTO> taskElementSkillMixes = skillMixes.Where(sm => sm.BOETaskElementID == boeTaskElementDateShift.Id)
					.Select(sm => new SkillMixDTO
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

				DoPostProcessing(taskElementSkillMixes);

				List<CommonDisclosureSkillMixDTO> taskElementCommonDisclosures = commonDisclosures.Where(cdsm => cdsm.BOETaskElementID == boeTaskElementDateShift.Id)
					.Select(cdsm => new CommonDisclosureSkillMixDTO
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

				DoPostProcessing(taskElementCommonDisclosures);

				// Assign the skill mix and common disclosure
				boeTaskElementDateShift.SkillMixTable = taskElementSkillMixes.Select(dto => new SkillMixModelView(dto)).ToList();
				boeTaskElementDateShift.CommonDisclosureTable = taskElementCommonDisclosures.Select(dto => new CommonDisclosureModelView(dto)).ToList();

				// Get resource types for task element labors.
				Collection<ResourceTypeDto> taskElementLabors = boeLaborTypes.Where(lT => lT.BOETaskElementID == boeTaskElementDateShift.Id)
					.Select(lT => new ResourceTypeDto
					{
						Id = lT.BOELaborTypeID,
						TaskElementId = lT.BOETaskElementID,
						ResourceID = lT.ResourceID,
						StartDateValue = lT.BOELaborTypeStartDate,
						EndDateValue = lT.BOELaborTypeEndDate,
						SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
						UpdateDate = lT.UpdateDT,
						SpreadCurveIDValue = lT.SpreadCurveID,
						LaborSpreadsIEnum = boeLaborSpreads.Where(lS => lS.BOELaborTypeID == lT.BOELaborTypeID)
							.Select(lS => new ResourceSpreadDto
							{
								Id = lS.BOELaborSpreadID,
								LaborSpreadDate = lS.LaborSpreadDate,
								LaborSpreadValue = lS.LaborSpreadValue ?? 0,
								LaborTypeId = lS.BOELaborTypeID
							}),
					}).ToCollection();

				boeTaskElementDateShift.TaskElementLabors = taskElementLabors;

				if (taskElementLabors != null)
				{
					foreach (ResourceTypeDto resourceType in taskElementLabors)
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

				return boeTaskElementDateShift;
			}
		}

		/// <summary>
		/// Do post processing on the skill mixes.
		/// </summary>
		/// <param name="skillMixes">Skill mixes.</param>
		private void DoPostProcessing(ICollection<SkillMixDTO> skillMixes)
		{
			IEnumerable<IGrouping<int, SkillMixDTO>> groupedResourceHours =
				skillMixes.GroupBy(r => r.BOETaskElementID);

			foreach (IGrouping<int, SkillMixDTO> grouping in groupedResourceHours)
			{
				decimal totalGroupHours = grouping.Sum(g => g.HistoricalHours);

				if (totalGroupHours != 0m)
				{
					foreach (SkillMixDTO dto in grouping)
					{
						dto.LaborSkillMix = dto.HistoricalHours * 100m / totalGroupHours;
					}
				}
			}
		}

		/// <summary>
		/// Do post processing on the common disclosures.
		/// </summary>
		/// <param name="commonDisclosures">Common disclosures.</param>
		private void DoPostProcessing(ICollection<CommonDisclosureSkillMixDTO> commonDisclosures)
		{
			IEnumerable<IGrouping<int, CommonDisclosureSkillMixDTO>> groupedResourceHours =
				commonDisclosures.GroupBy(r => r.BOETaskElementID);

			foreach (IGrouping<int, CommonDisclosureSkillMixDTO> grouping in groupedResourceHours)
			{
				decimal totalGroupHours = grouping.Sum(g => g.HistoricalHours);
				if (totalGroupHours != 0m)
				{
					foreach (CommonDisclosureSkillMixDTO dto in grouping)
					{
						dto.LaborSkillMix = dto.HistoricalHours * 100m / totalGroupHours;
					}
				}
			}
		}
	}
}