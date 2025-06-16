// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Linq;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.Dtos;
	using GenBOE.Models;
	using IES.Common;
	using IES.Common.Exceptions;
	using static IES.Common.Constants;

	public class BoeTaskElementDTODataLoader : BulkDataLoader<BoeTaskElementDTO, BOETaskElement>, IBoeTaskElementDTODataLoader
	{
		private readonly Logger log = new Logger(typeof(BoeTaskElementDTODataLoader));

		private readonly IOrdinaryVariableLoader ordinaryVariableLoader = null;
		private readonly IResourceSpreadLoader resourceSpreadLoader = null;
		private readonly IResourceTypeLoader resourceTypeLoader = null;
		private readonly IBoeTaskElementCustomFieldValueXREFLoader boeTaskElementCustomFieldLoader = null;
		private readonly ILaborTypeCustomFieldValueXREFLoader boeLaborTypeCustomFieldLoader = null;
		private readonly ISkillMixDTOLoader skillMixDTOLoader = null;
		private readonly ICommonDisclosureSMDTODataLoader commonDisclosureSMDTODataLoader = null;

		/// <summary>
		/// Nominal constructor
		/// </summary>
		/// <param name="resourceTypeLoader">Resource type loader</param>
		/// <param name="resourceSpreadLoader">Resource spread loader</param>
		/// <param name="ordinaryVariableLoader">Ordinary variable loader</param>
		/// <param name="taskElementCustomFieldLoader">Custom field loader for tasks</param>
		/// <param name="laborTypeCustomFieldLoader">Custom field loader for labor types</param>
		public BoeTaskElementDTODataLoader(
			IResourceTypeLoader resourceTypeLoader,
			IResourceSpreadLoader resourceSpreadLoader,
			IOrdinaryVariableLoader ordinaryVariableLoader,
			IBoeTaskElementCustomFieldValueXREFLoader taskElementCustomFieldLoader,
			ILaborTypeCustomFieldValueXREFLoader laborTypeCustomFieldLoader,
			ISkillMixDTOLoader skillMixDTOLoader,
			ICommonDisclosureSMDTODataLoader commonDisclosureSMDTODataLoader)
		{
			this.resourceTypeLoader = resourceTypeLoader;
			this.resourceSpreadLoader = resourceSpreadLoader;
			this.ordinaryVariableLoader = ordinaryVariableLoader;
			this.boeTaskElementCustomFieldLoader = taskElementCustomFieldLoader;
			this.boeLaborTypeCustomFieldLoader = laborTypeCustomFieldLoader;
			this.skillMixDTOLoader = skillMixDTOLoader;
			this.commonDisclosureSMDTODataLoader = commonDisclosureSMDTODataLoader;
		}

		#region Retrieves

		/// <summary>
		/// Gets Task Elements by Boe Id
		/// </summary>
		/// <param name="boeId">Boe Id</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <param name="costPrecision">The Cost precision for the workspace.</param>
		/// <param name="hoursPrecision">The Hours precision for the workspace.</param>
		/// <returns>Fully Loaded Boe Task Element DTOs</returns>
		public ICollection<BoeTaskElementDTO> GetByBoeId(int boeId, int hoursPrecision, int costPrecision, bool includeRTEFields = false)
		{
			return this.GetByBoeIds(new List<int>() { boeId }, includeRTEFields, hoursPrecision, costPrecision);
		}

		/// <summary>
		/// (BULK LOAD) Get Boe Task Elements by Ids
		/// </summary>
		/// <param name="ids">Boe Task Element Ids</param>
		/// <returns>Boe Task Elements</returns>
		override public ICollection<BoeTaskElementDTO> GetByIds(ICollection<int> ids)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the by identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="hoursPrecision">The hours precision.</param>
		/// <param name="costPrecision">The cost precision.</param>
		/// <returns></returns>
		public BoeTaskElementDTO GetById(int id, int hoursPrecision, int costPrecision)
		{
			return this.GetByIds(new List<int>() { id }, false, hoursPrecision, costPrecision).FirstOrDefault();
		}

		/// <summary>
		/// (BULK LOAD) Get Boe Task Elements by Ids
		/// </summary>
		/// <param name="ids">Boe Task Element Ids</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <param name="costPrecision">The Cost precision for the workspace.</param>
		/// <param name="hoursPrecision">The Hours precision for the workspace.</param>
		/// <returns>Boe Task Elements</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[DbQuery(3)]
		public ICollection<BoeTaskElementDTO> GetByIds(ICollection<int> ids, bool includeRTEFields, int hoursPrecision, int costPrecision)
		{
			if (ids == null || !ids.Any()) { return new List<BoeTaskElementDTO>(); }

			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				List<BoeTaskElementDTO> result;
				List<OrdinaryVariableDto> ordinaryVariables;
				List<ResourceTypeDto> taskElementLabors;
				List<SkillMixDTO> skillMixDTOs = null;
				List<CommonDisclosureSkillMixDTO> commonDisclosureSkillMixDTOs = null;

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.Database.CommandTimeout = 360; // give queries enough time to execute

					result = gbe.BOETaskElements.Where(bT => ids.Contains(bT.BOETaskElementID))
								  .Select(bT => new BoeTaskElementDTO
								  {
									  // 2 RTE fields:
									  Description = includeRTEFields ? bT.TaskDescription : null,
									  MOQText = includeRTEFields ? bT.MOQText : null,
									  WasMoqTextSet = includeRTEFields,
									  WasDescriptionSet = includeRTEFields,

									  Id = bT.BOETaskElementID,
									  BOETaskID = bT.TaskID,
									  TaskTitle = bT.TaskTitle,
									  MOQHoursEquation = bT.MOQHoursEquation,
									  AuthorUserId = bT.AuthorUserId,
									  MOQType = bT.MOQTypeID.HasValue ? (MOQType)bT.MOQTypeID : MOQType.None,
									  IMS_ID = bT.IMS_ID,
									  StartDate = bT.TaskStartDate,
									  EndDate = bT.TaskEndDate,
									  LaborTypeWarningFlag = bT.LaborTypeWarningFlag,
									  UpdateDate = bT.UpdateDT,
									  BoeID = bT.BOEID,
									  TaskElementType = (TaskElementType)bT.TaskElementTypeID,
									  BOETaskElementOrder = bT.SortOrderID,

									  CustomFieldValueContainersIEnum = bT.BOETaskElementCustomFieldValueXREFs
											 .Select(cf => new CustomFieldValueContainer
											 {
												 ContainerID = cf.BTECFVID,
												 CustomFieldValueID = cf.CustomFieldValueID,
												 CustomFieldID = cf.CustomFieldValue.CustomFieldID,
												 UpdateDate = cf.UpdateDT,
												 IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
												 OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
											 }),

									  WorkspaceVariableIDsIEnum = bT.BOETaskElementWorkspaceVariableXREFs.Select(wsV => wsV.WorkspaceVariableID)
								  }).ToList();

					ordinaryVariables = GetOrdinaryVariables(ids, gbe);
					taskElementLabors = GetTaskLabors(ids, gbe);
					this.LoadSikorskyFields(gbe, result);
				}

				DoPostProcessing(result, ordinaryVariables, taskElementLabors, hoursPrecision, costPrecision, skillMixDTOs, commonDisclosureSkillMixDTOs, skillMixDTOLoader, commonDisclosureSMDTODataLoader);
				return result;
			}
		}

		/// <summary>
		/// Gets Task Elements by Boe Id
		/// </summary>
		/// <param name="boeIds">A collection of Boe Ids</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <param name="costPrecision">The Cost precision for the workspace.</param>
		/// <param name="hoursPrecision">The Hours precision for the workspace.</param>
		/// <returns>Fully Loaded Boe Task Element DTOs</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[DbQuery(3)]
		public ICollection<BoeTaskElementDTO> GetByBoeIds(ICollection<int> boeIds, bool includeRTEFields, int hoursPrecision, int costPrecision)
		{
			if (boeIds == null || !boeIds.Any()) { return new List<BoeTaskElementDTO>(); }

			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				List<BoeTaskElementDTO> result;
				List<OrdinaryVariableDto> ordinaryVariables;
				List<ResourceTypeDto> taskElementLabors;
				ICollection<SkillMixDTO> skillMixDTOs = this.skillMixDTOLoader.GetByBOEIDs(boeIds);
				ICollection<CommonDisclosureSkillMixDTO> commonDisclosureSkillMixDTOs = this.commonDisclosureSMDTODataLoader.GetByBOEIDs(boeIds);

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

					result = gbe.BOETaskElements.Where(bT => boeIds.Contains(bT.BOEID))
								  .Select(bT => new BoeTaskElementDTO
								  {
									  // 2 RTE fields:
									  Description = includeRTEFields ? bT.TaskDescription : null,
									  MOQText = includeRTEFields ? bT.MOQText : null,
									  WasMoqTextSet = includeRTEFields,
									  WasDescriptionSet = includeRTEFields,

									  Id = bT.BOETaskElementID,
									  BOETaskID = bT.TaskID,
									  TaskTitle = bT.TaskTitle,
									  MOQHoursEquation = bT.MOQHoursEquation,
									  AuthorUserId = bT.AuthorUserId,
									  MOQType = bT.MOQTypeID.HasValue ? (MOQType)bT.MOQTypeID : MOQType.None,
									  IMS_ID = bT.IMS_ID,
									  StartDate = bT.TaskStartDate,
									  EndDate = bT.TaskEndDate,
									  LaborTypeWarningFlag = bT.LaborTypeWarningFlag,
									  UpdateDate = bT.UpdateDT,
									  BoeID = bT.BOEID,
									  TaskElementType = (TaskElementType)bT.TaskElementTypeID,
									  BOETaskElementOrder = bT.SortOrderID,

									  CustomFieldValueContainersIEnum = bT.BOETaskElementCustomFieldValueXREFs
											 .Select(cf => new CustomFieldValueContainer
											 {
												 ContainerID = cf.BTECFVID,
												 CustomFieldValueID = cf.CustomFieldValueID,
												 CustomFieldID = cf.CustomFieldValue.CustomFieldID,
												 UpdateDate = cf.UpdateDT,
												 IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
												 OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
											 }),

									  WorkspaceVariableIDsIEnum = bT.BOETaskElementWorkspaceVariableXREFs.Select(wsV => wsV.WorkspaceVariableID)
								  }).ToList();

					List<int> ids = result.Select(bT => bT.Id).ToList();

					ordinaryVariables = GetOrdinaryVariables(ids, gbe);
					taskElementLabors = GetTaskLabors(ids, gbe);
					this.LoadSikorskyFields(gbe, result);
				}

				DoPostProcessing(result, ordinaryVariables, taskElementLabors, hoursPrecision, costPrecision, skillMixDTOs, commonDisclosureSkillMixDTOs, skillMixDTOLoader, commonDisclosureSMDTODataLoader);

				return result;
			}
		}

		/// <summary>
		/// Gets Task Elements by Workspace Id
		/// </summary>
		/// <param name="wsId">Workspace Id</param>
		/// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
		/// <param name="costPrecision">The Cost precision for the workspace.</param>
		/// <param name="hoursPrecision">The Hours precision for the workspace.</param>
		/// <returns>Fully Loaded Boe Task Element DTOs</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[DbQuery(3)]
		public ICollection<BoeTaskElementDTO> GetByWorkspaceId(int wsId, bool includeRTEFields, int hoursPrecision, int costPrecision)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				List<BoeTaskElementDTO> result;
				List<OrdinaryVariableDto> ordinaryVariables;
				List<ResourceTypeDto> taskElementLabors;
				ICollection<SkillMixDTO> skillMixDTOs = this.skillMixDTOLoader.GetByWorkspaceId(wsId);
				ICollection<CommonDisclosureSkillMixDTO> commonDisclosureSkillMixDTOs = this.commonDisclosureSMDTODataLoader.GetByWorkspaceId(wsId); ;

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

					result = gbe.BOETaskElements.Where(bT => bT.BOE.WorkspaceID == wsId)
								  .Select(bT => new BoeTaskElementDTO
								  {
									  // 2 RTE fields:
									  Description = includeRTEFields ? bT.TaskDescription : null,
									  MOQText = includeRTEFields ? bT.MOQText : null,
									  WasMoqTextSet = includeRTEFields,
									  WasDescriptionSet = includeRTEFields,

									  Id = bT.BOETaskElementID,
									  BOETaskID = bT.TaskID,
									  TaskTitle = bT.TaskTitle,
									  MOQHoursEquation = bT.MOQHoursEquation,
									  AuthorUserId = bT.AuthorUserId,
									  MOQType = bT.MOQTypeID.HasValue ? (MOQType)bT.MOQTypeID : MOQType.None,
									  IMS_ID = bT.IMS_ID,
									  StartDate = bT.TaskStartDate,
									  EndDate = bT.TaskEndDate,
									  LaborTypeWarningFlag = bT.LaborTypeWarningFlag,
									  UpdateDate = bT.UpdateDT,
									  BoeID = bT.BOEID,
									  TaskElementType = (TaskElementType)bT.TaskElementTypeID,
									  BOETaskElementOrder = bT.SortOrderID,

									  CustomFieldValueContainersIEnum = bT.BOETaskElementCustomFieldValueXREFs
											 .Select(cf => new CustomFieldValueContainer
											 {
												 ContainerID = cf.BTECFVID,
												 CustomFieldValueID = cf.CustomFieldValueID,
												 CustomFieldID = cf.CustomFieldValue.CustomFieldID,
												 UpdateDate = cf.UpdateDT,
												 IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
												 OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
											 }),

									  WorkspaceVariableIDsIEnum = bT.BOETaskElementWorkspaceVariableXREFs.Select(wsV => wsV.WorkspaceVariableID)
								  }).ToList();

					List<int> ids = result.Select(bT => bT.Id).ToList();

					ordinaryVariables = GetOrdinaryVariables(ids, gbe);
					taskElementLabors = GetTaskLabors(ids, gbe);
					this.LoadSikorskyFields(gbe, result);
				}

				DoPostProcessing(result, ordinaryVariables, taskElementLabors, hoursPrecision, costPrecision, skillMixDTOs, commonDisclosureSkillMixDTOs, skillMixDTOLoader, commonDisclosureSMDTODataLoader);

				return result;
			}
		}

		/// <summary>
		/// Loads Sikorsky Custom Fields into Task properties
		/// </summary>
		/// <param name="gbe">GenBOE Entities connected to the DB</param>
		/// <param name="taskElementsToLoad">Task elements to load</param>
		[DbQuery]
		private void LoadSikorskyFields(GenBoeEntities gbe, List<BoeTaskElementDTO> taskElementsToLoad)
		{
			List<int> taskIds = taskElementsToLoad.Select(z => z.Id).ToList();

			var sikorskyCfTaskData = gbe.BOETaskElementCustomFieldValueXREFs.Where(x => taskIds.Contains(x.BOETaskElementID)).Select(x => new
			{
				CfName = x.CustomFieldValue.CustomFieldValueName,
				CfValue = x.CustomFieldValue.CustomFieldValueDescription,
				FieldName = x.CustomFieldValue.CustomField.CustomFieldName.ToUpper(),
				TaskElementId = x.BOETaskElementID
			}).ToList();

			taskElementsToLoad.ForEach(task =>
			{
				task.SOW = sikorskyCfTaskData.FirstOrDefault(x => x.TaskElementId == task.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_SOW.ToUpper())?.CfName;
				task.SOWTitle = sikorskyCfTaskData.FirstOrDefault(x => x.TaskElementId == task.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_SOW.ToUpper())?.CfValue;
				task.Category = sikorskyCfTaskData.FirstOrDefault(x => x.TaskElementId == task.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_CATEGORY.ToUpper())?.CfValue;
				task.CamName = sikorskyCfTaskData.FirstOrDefault(x => x.TaskElementId == task.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_CAMNAME.ToUpper())?.CfValue;
				task.ClassOfCost = sikorskyCfTaskData.FirstOrDefault(x => x.TaskElementId == task.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_CLASSOFCOST.ToUpper())?.CfName?.GetEnumeratedValueNullable<ClassOfCost>() ?? ClassOfCost.None;
			});
		}

		#region RTE Load Methods

		/// <summary>
		/// Pulls RTE fields for the DTOs, and updates them as needed
		/// </summary>
		/// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
		[DbQuery]
		public void LoadRTEFields(ICollection<BoeTaskElementDTO> dtos)
		{
			if (dtos == null || !dtos.Any()) { return; }

			List<RteFieldsHelper> dataFromDb = null;

			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				// Only want to pull RTE for an object when:
				//   - the object exists (Id > 0)
				//   - at least one of the RTE fields has not been inserted into, or retrieved already
				List<int> dtoIds = dtos.Where(x => x.Id > 0 && (!x.WasMoqTextSet || !x.WasDescriptionSet) && x.Updateable != UpdateType.Deleted).Select(x => x.Id).ToList();

				if (dtoIds.Any())
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

						// get the basic BOE data from the sprocResults
						dataFromDb = (from x in gbe.BOETaskElements
									  where dtoIds.Contains(x.BOETaskElementID)
									  select new RteFieldsHelper
									  {
										  Id = x.BOETaskElementID,
										  Description = x.TaskDescription,
										  MoqText = x.MOQText
									  }).ToList();
					}

					dataFromDb.AsParallel().ForAll(dbData =>
					{
						BoeTaskElementDTO dto = dtos.First(b => b.Id == dbData.Id);

						// We do not want to overwrite existing data during, so we only fill missing data, if available
						dto.Description = dto.WasDescriptionSet ? dto.Description : dbData.Description;
						dto.MOQText = dto.WasMoqTextSet ? dto.MOQText : dbData.MoqText;
					});
				}
			}
		}

		#endregion

		#region Obsolete methods, only used by unit tests..

		/// <summary>
		/// Get Task Variable data by Task Variable ID
		/// This function should mainly be used when recalculating BOE LT's based on a select boe to sum change
		/// </summary>
		/// <param name="inTaskVariableID"><task variable ID/param>
		/// <returns>task variable data (no BOE)</returns>
		[DbQuery]
		[Obsolete("This is only used by unit tests")]
		virtual public OrdinaryVariableDto GetTaskVariableByTaskVariableID(int inTaskVariableID)
		{
			return this.ordinaryVariableLoader.GetById(inTaskVariableID);
		}

		#endregion

		#endregion Retrieves

		#region Commits

		/// <summary>
		/// Provides the metadata to support bulk save processing for BoeTaskElementDTOs
		/// </summary>
		/// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
		/// <returns>Meta data required for bulk save processing</returns>
		public override BulkSaveMetaData CreateBulkSaveMetaData()
		{
			BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

			metaData.BulkDeleteStoredProcedureName = "deleteBOETaskElementviaTableParameter";
			metaData.BulkInsertStoredProcedureName = "insertBOETaskElementviaTableParameter";
			metaData.BulkUpdateStoredProcedureName = "updateBOETaskElementviaTableParameter";

			metaData.BulkInsertStoredProcedureReturnsUpdateDate = true;
			metaData.BulkUpdateStoredProcedureReturnsUpdateDate = true;

			// This is the table type defined in the database for the input arg to the delete, insert, and update stored procedures
			metaData.DBTableTypeName = "TT_BOETaskElement";

			// Name of the stored procedure argument for all 3 bulk stored procedures
			metaData.StoredProcedureTableTypeParameterName = "@BOETaskElementTableParameter";

			// The order here matters. It must match exactly the order of the TT_BOETaskElement in the database.
			metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
			{
				"BOETaskElementID", "UpdateDT", "TaskID", "TaskTitle", "TaskDescription", "TaskStartDate", "TaskEndDate", "MOQHoursEquation", "MOQCostEquation",
				"MOQText", "MOQTypeID", "BOEID", "LaborTypeWarningFlag", "IMS_ID", "TaskElementTypeID", "SortOrderID", "AuthorUserId"
			};

			return metaData;
		}

		/// <summary>
		/// Performs a bulk save of the BoeTaskElementDTOs. This method is overridden because we're saving a bunch of related data with each BoeTaskElementDTO.
		/// </summary>
		/// <param name="dtosToSave">the set of dtos to bulk save</param>
		/// <returns>Dictionary where the key is the old dto id and the value is the new id.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public override IDictionary<int, int> BulkSave(ICollection<BoeTaskElementDTO> dtosToSave)
		{
			if (dtosToSave == null) { throw new ArgumentNullException(nameof(dtosToSave)); }
			if (dtosToSave.Any(d => d.Updateable == UpdateType.None)) { throw new ArgumentException("One or more Task Elements has UpdateType of None.", nameof(dtosToSave)); }

			IDictionary<int, int> toReturn = new Dictionary<int, int>();

			if (dtosToSave.Any())
			{
				if (this.log.DebugEnabled)
				{
					this.log.Debug(string.Format("BoeTaskElementDTODataLoader.BulkSave => Ntid: {2}, BoeId: {1}, Count: {0}",
						dtosToSave.Count, dtosToSave.First().BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name));

					foreach (BoeTaskElementDTO aDto in dtosToSave)
					{
						this.log.Debug(string.Format("BoeTaskElementDTODataLoader.BulkSave => Id: {0}, Hours: {2}, Cost: {3}. Moq: {1}",
							aDto.Id, aDto.MOQHoursEquation, aDto.TotalHours, aDto.TotalCost));
					}
				}

				using (StopwatchTimer sw = new StopwatchTimer(this.log))
				{
					// Make sure that RTE data is loaded, that way we do not wipe it out..
					this.LoadRTEFields(dtosToSave);

					ICollection<BoeTaskElementDTO> upsertableTaskElementCollection = dtosToSave.Where(d => d.Updateable != UpdateType.None).ToCollection();

					#region Bulk Save Task Element details
					// Bulk save task element details
					toReturn = base.BulkSave(dtosToSave);

					// remove the deleted task elements from the list before proceeding          
					upsertableTaskElementCollection = (from te in upsertableTaskElementCollection
													   where te.Updateable == UpdateType.None
													   select te).ToCollection();

					List<BOETaskElementWorkspaceVariableXREF> workspaceEntities = new List<BOETaskElementWorkspaceVariableXREF>();

					upsertableTaskElementCollection.ToList().ForEach(
						taskElement =>
						{
							if (taskElement.WorkspaceVariableIDs.Any())
							{
								taskElement.WorkspaceVariableIDs.ToList().ForEach(
									wsVarId =>
									{
										workspaceEntities.Add(new BOETaskElementWorkspaceVariableXREF()
										{
											BOETaskWSVarID = -1,
											BOETaskElementID = taskElement.Id,
											WorkspaceVariableID = wsVarId
										});
									});
							}
							else
							{
								// need to handle cases where there are no variables, so that way they would be removed correctly
								workspaceEntities.Add(new BOETaskElementWorkspaceVariableXREF()
								{
									BOETaskWSVarID = -1,
									BOETaskElementID = taskElement.Id,
									WorkspaceVariableID = -100 // this will indicate that there's no WS vars, so it won't be added, but will delete any existing ones for the task element
								});
							}
						});

					#region Bulk Save Workspace variables - in this case they're only inserts            

					if (workspaceEntities.Any())
					{
						using (GenBoeEntities gbe = new GenBoeEntities())
						{
							// convert to datatable
							// NOTE - normally, this code would reside in the data loader for this dto/entity combination but we don't have one for 
							//        workspace variables - These string literals are used only here so no need for constants.
							Collection<string> workspaceVariablesPropertiesToIncludeInTable = new Collection<string>()
							{
								"BOETaskWSVarID", "BOETaskElementID", "WorkspaceVariableID"
							};
							DataTable workspaceVariablesDataTable = StoredProcedureHelper.ToDataTable<BOETaskElementWorkspaceVariableXREF>(workspaceEntities, workspaceVariablesPropertiesToIncludeInTable);

							StoredProcedureHelper.ExecuteTableValueProcedure(
								gbe,
								workspaceVariablesDataTable,
								"insertBOETaskElementWorkspaceVariableviaTableParameter",
								"@BOETaskElementWorkspaceVariableXREF",
								"TT_BOETaskElementWorkspaceVariableXREF",
								false);
						}
					}

					#endregion

					#region Bulk Save Ordinary Variables
					// grab only the ordinary variable dtos that are upsertable. 
					ICollection<OrdinaryVariableDto> upsertableOrdinaryVariableDtos = (from taskElementDto in dtosToSave
																					   from ordinaryVariableDto in taskElementDto.OrdinaryVariables
																					   where ordinaryVariableDto.Updateable != UpdateType.None
																					   select ordinaryVariableDto).ToCollection();

					if (upsertableOrdinaryVariableDtos.Any())
					{
						this.ordinaryVariableLoader.BulkSave(upsertableOrdinaryVariableDtos);
					}
					#endregion

					#endregion

					#region Bulk Save Task Element Custom Field Value Containers

					// Before grabbing the task element containers, we need to iterate thru the task elements to make sure the new 'OwnerId' property on CustomFIeldValueContainer
					// is set to the owning task element id - this is to ensure the bulk save has the right info to process containers
					foreach (BoeTaskElementDTO taskElement in dtosToSave)
					{
						foreach (CustomFieldValueContainer container in taskElement.CustomFieldValueContainers)
						{
							// set the owner id
							container.OwnerID = taskElement.Id;
						}
					}

					// now bulk save the BoeTaskElementCustomFieldValues
					// These have to be done manually because there is no Loader class for it.
					ICollection<CustomFieldValueContainer> saveableTaskElementContainerCollection = (from taskElement in upsertableTaskElementCollection
																									 from taskElementCustomFieldValue in taskElement.CustomFieldValueContainers
																									 where taskElementCustomFieldValue.Updateable != UpdateType.None
																									 select taskElementCustomFieldValue).ToCollection();

					if (saveableTaskElementContainerCollection.Any())
					{
						this.boeTaskElementCustomFieldLoader.BulkSave(saveableTaskElementContainerCollection);
					}

					#endregion

					#region Bulk Save Task Element Labors
					// make sure each saveable labor has TaskElementID is set to parents id!!!!
					foreach (BoeTaskElementDTO taskElement in upsertableTaskElementCollection)
					{
						foreach (ResourceTypeDto resource in taskElement.taskElementLabors)
						{
							if (resource.Updateable != UpdateType.None && resource.TaskElementId == 0)
							{
								// make sure each labor is pointing to the correct parent
								resource.TaskElementId = taskElement.Id;
							}
						}
					}

					// Now grab all the labor resource types that are to be deleted, inserted, or updated
					ICollection<ResourceTypeDto> saveableLabors = (from upsertableTaskElement in upsertableTaskElementCollection
																   from resourceType in upsertableTaskElement.taskElementLabors
																   where resourceType.Updateable != UpdateType.None
																   select resourceType).ToCollection();

					if (saveableLabors.Any())
					{
						//Give labors with negative ids new ids to prevent duplicates
						int tempId = -1;
						saveableLabors.Where(x => x.Id < 0).ToList().ForEach(labor => { labor.Id = tempId--; });

						this.resourceTypeLoader.BulkSave(saveableLabors);

						// remove the deleted labors
						saveableLabors = (from labor in saveableLabors
										  where labor.Updateable == UpdateType.None
										  select labor).ToCollection();
					}

					#endregion

					#region Bulk Save Labor Type Custom Field Value Containers

					// Before grabbing the labor type containers, we need to iterate thru the labor types to make sure the new 'OwnerId' property on CustomFIeldValueContainer
					// is set to the owning labor type id - this is a hack to ensure the bulk save has the right info to process containers
					foreach (ResourceTypeDto resource in saveableLabors)
					{
						foreach (CustomFieldValueContainer container in resource.CustomFieldValueContainers)
						{
							// set the owner id - the OwnerId is a hack so we can get the labor type id into the Bulk Save call
							container.OwnerID = resource.Id;
						}
					}

					// Deleting a ResourceType deletes all of its related data so 
					// we only need to grab the related data from the newly inserted and updated resource types
					ICollection<CustomFieldValueContainer> saveableLaborTypeContainers = (from upsertableLabor in saveableLabors
																						  from aContainer in upsertableLabor.CustomFieldValueContainers
																						  where aContainer.Updateable != UpdateType.None
																						  select aContainer).ToCollection();

					if (saveableLaborTypeContainers.Any())
					{
						this.boeLaborTypeCustomFieldLoader.BulkSave(saveableLaborTypeContainers);
					}

					#endregion

					#region Bulk Save Labor Spreads

					// Before grabbing the labor spreads we need to make sure they have their parent id set
					foreach (ResourceTypeDto labor in saveableLabors)
					{
						foreach (ResourceSpreadDto spread in labor.LaborSpreads)
						{
							spread.LaborTypeId = labor.Id;
						}
					}

					// BulkSave the labor spreads
					// Since the resource spreads are inserted via kill/fill, deletes can be excluded from being processed except for non-Discrete in case we flipped from Discrete to non-Discrete
					ICollection<ResourceSpreadDto> upsertableLaborSpreads =
					(from upsertableLabor in saveableLabors
					 from laborSpread in upsertableLabor.LaborSpreads
					 where (upsertableLabor.SpreadCurveID == SpreadCurves.DiscreteCost || upsertableLabor.SpreadCurveID == SpreadCurves.DiscreteHours) &&
					 laborSpread.Updateable != UpdateType.None &&
					 laborSpread.Updateable != UpdateType.Deleted
					 select laborSpread).ToCollection();

					if (upsertableLaborSpreads.Any())
					{
						// now set the negative ids so they are all unique 
						for (int i = 0; i < upsertableLaborSpreads.Count(); i++)
						{
							ResourceSpreadDto dto = upsertableLaborSpreads.ElementAt(i);
							dto.Id = (-1) * (i + 1);
						}

						this.resourceSpreadLoader.BulkSave(upsertableLaborSpreads);
					}

					List<ResourceSpreadDto> deleteableLaborSpreads =
						(from upsertableLabor in saveableLabors
						 from laborSpread in upsertableLabor.LaborSpreads
						 where (upsertableLabor.SpreadCurveID != SpreadCurves.DiscreteCost && upsertableLabor.SpreadCurveID != SpreadCurves.DiscreteHours) &&
						 laborSpread.Updateable != UpdateType.None &&
						 laborSpread.Updateable != UpdateType.Deleted
						 select laborSpread).ToList();

					if (deleteableLaborSpreads.Any())
					{
						// Delete the labor spreads
						deleteableLaborSpreads.ForEach(ls => ls.Updateable = UpdateType.Deleted);
						this.resourceSpreadLoader.BulkSave(deleteableLaborSpreads);
					}

					#endregion

					// refresh Custom Field In Use flags for Workspaces
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						int[] boeIds = dtosToSave.Select(te => te.BoeID).Distinct().ToArray();

						List<int> workspaceIds = (from o in gbe.BOEs
												  where boeIds.Contains(o.BOEID)
												  select o.WorkspaceID).Distinct().ToList();

						foreach (int workspaceId in workspaceIds)
						{
							gbe.refreshCustomFieldInUseFlagByWorkspaceID(workspaceId);
						}
					}

					if (Utilities.IsSkillMixEnabledForSystem)
					{
						SaveSkillMixRationale(upsertableTaskElementCollection);
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Saves the skill mix rationale.
		/// </summary>
		/// <param name="dtosToSave">Boe Task Element Dto</param>
		private void SaveSkillMixRationale(ICollection<BoeTaskElementDTO> dtosToSave)
		{
			foreach (BoeTaskElementDTO inTaskDetail in dtosToSave)
			{
				// Save the Skill Mix tables
				if (inTaskDetail.SkillMixTable != null && inTaskDetail.SkillMixTable.Any())
				{
					List<SkillMixDTO> dtos = new List<SkillMixDTO>();
					foreach (SkillMixModelView skillMixModelView in inTaskDetail.SkillMixTable)
					{
						SkillMixDTO dto = skillMixModelView.ToDto();
						dto.BOEID = inTaskDetail.BoeID;
						dto.BOETaskElementID = inTaskDetail.Id;
						dtos.Add(dto);
					}

					this.skillMixDTOLoader.InsertSkillMix(dtos);
				}
				else
				{
					// If the dto has no skill mix tables, it's possible they were cleared out, so make sure old data is deleted
					foreach (BoeTaskElementDTO dto in dtosToSave)
					{
						this.skillMixDTOLoader.DeleteSkillMixByBOETaskElementID(dto.Id);
					}
				}

				// Save the Common Disclosure DTOs
				if (inTaskDetail.CommonDisclosureTable != null && inTaskDetail.CommonDisclosureTable.Any())
				{
					List<CommonDisclosureSkillMixDTO> dtos = new List<CommonDisclosureSkillMixDTO>();
					foreach (CommonDisclosureModelView commonDisclosure in inTaskDetail.CommonDisclosureTable)
					{
						CommonDisclosureSkillMixDTO dto = commonDisclosure.ToDto();
						dto.BOEID = inTaskDetail.BoeID;
						dto.BOETaskElementID = inTaskDetail.Id;
						dtos.Add(dto);
					}

					this.commonDisclosureSMDTODataLoader.InsertCommonDisclosureSM(dtos);
				}
				else
				{
					// If the dto has no common disclosure tables, it's possible they were cleared out, so make sure old data is deleted
					foreach (BoeTaskElementDTO dto in dtosToSave)
					{
						this.commonDisclosureSMDTODataLoader.DeleteCommonDisclosureSkillMixByBOETaskElementID(dto.Id);
					}
				}
			}
		}

		/// <summary>
		/// Save BOE Task Elements
		/// A user can only edit/insert one task element at a time, but the user can delete multiple task elements at a time which is why this function
		/// takes in a collection
		/// </summary>
		/// <param name="inBoeTaskElementDTOs">BOE Task Element DTOs to save</param>
		virtual public Dictionary<int, int> SaveBoeTaskElements(Collection<BoeTaskElementDTO> inBoeTaskElementDTOs)
		{
			if (inBoeTaskElementDTOs == null) { throw new ArgumentNullException(nameof(inBoeTaskElementDTOs)); }

			IDictionary<int, int> toReturn = new Dictionary<int, int>();

			if (inBoeTaskElementDTOs.Any())
			{
				using (StopwatchTimer sw = new StopwatchTimer(this.log))
				{
					// Make sure that RTE data is loaded, that way we do not wipe it out..
					this.LoadRTEFields(inBoeTaskElementDTOs);

					// The Task Elements should be the first items saved
					foreach (BoeTaskElementDTO taskElementDetail in inBoeTaskElementDTOs)
					{
						int taskElementId = taskElementDetail.Id;
						taskElementDetail.BoeID = taskElementDetail.BoeID;

						if (this.log.DebugEnabled)
						{
							this.log.Debug(string.Format("BoeTaskElementDTODataLoader.SaveBoeTaskElements => Ntid: {2}, BoeId: {1}, Count: {0}",
								inBoeTaskElementDTOs.Count, inBoeTaskElementDTOs.First().BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name));

							foreach (BoeTaskElementDTO aDto in inBoeTaskElementDTOs)
							{
								this.log.Debug(string.Format("BoeTaskElementDTODataLoader.SaveBoeTaskElements => Id: {0}, Hours: {2}, Cost: {3}. Moq: {1}",
									aDto.Id, aDto.MOQHoursEquation, aDto.TotalHours, aDto.TotalCost));
							}
						}

						if (taskElementDetail.Updateable == UpdateType.Deleted)
						{
							this.DeleteTaskElementDetail(taskElementDetail);
						}
						else
						{
							if (taskElementDetail.Updateable == UpdateType.Upsert)
							{
								int orgId = taskElementDetail.Id;
								taskElementId = this.CreateOrSaveTaskElementDetail(taskElementDetail.BoeID, taskElementDetail);

								// add old task element id & new task element id to dictionary
								toReturn.Add(orgId, taskElementId);
							}

							// save task element custom fields
							this.boeTaskElementCustomFieldLoader.SaveBOETaskElementCustomFieldValueContainers(taskElementDetail.CustomFieldValueContainers, taskElementId);
						}

						// check to see which labor types need to be saved
						foreach (ResourceTypeDto laborType in taskElementDetail.taskElementLabors)
						{
							int laborTypeID = laborType.Id;

							if (laborType.Updateable == UpdateType.Deleted)
							{
								this.DeleteLMLaborType(laborType);
							}
							else
							{
								if (laborType.Updateable == UpdateType.Upsert)
								{
									laborTypeID = this.CreateorSaveLMLaborType(taskElementId, laborType);
								}

								// save labor type custom fields
								this.SaveBOELaborTypeCustomFieldValueContainers(laborType.CustomFieldValueContainers, laborTypeID);
							}

							int deletedCnt = (from x in laborType.LaborSpreads
											  where x.Updateable == UpdateType.Deleted
											  select x).Count();

							if (deletedCnt > 0 && deletedCnt == laborType.LaborSpreads.Count())
							{
								this.DeleteLMLaborSpreadsByLaborTypeID(laborTypeID);
							}
							else
							{
								Boolean LSCleared = false;
								// check to see which labor spreads need to be saved
								foreach (ResourceSpreadDto laborSpread in laborType.LaborSpreads)
								{
									if (laborSpread.Updateable == UpdateType.Upsert)
									{
										/*this is strange logic but in the spirit of working with what we have:
                                         * LS are always saved as a group, so any time a LS is to be upserted
                                         * it means all LS for a LT will also be upserted.  Before we add a 
                                         * new group we need to clear out the old one.
                                        */
										if (!LSCleared)
										{
											this.DeleteLMLaborSpreadsByLaborTypeID(laborTypeID);
											LSCleared = true;
										}

										// only save the labor spread if it is discrete
										if (laborType.SpreadCurveID == SpreadCurves.DiscreteCost || laborType.SpreadCurveID == SpreadCurves.DiscreteHours)
										{
											this.CreateOrSaveLMLaborSpread(laborTypeID, laborSpread);
										}
									}
								}
							}
						}
					}
				}
			}

			return toReturn as Dictionary<int, int>;
		}

		/// <summary>
		/// Save/Create the Task Element Details
		/// </summary>
		/// <param name="dtoToUpsert">the element to save</param>
		/// <returns>the task element details</returns>
		override protected int? Upsert(BoeTaskElementDTO dtoToUpsert)
		{
			if (dtoToUpsert == null)
			{
				throw new ArgumentNullException(nameof(dtoToUpsert));
			}

			return this.CreateOrSaveTaskElementDetail(dtoToUpsert.BoeID, dtoToUpsert);
		}

		/// <summary>
		/// This function will handle an individual BOE Task Element delete
		/// </summary>
		/// <param name="dtoToDelete">the selected Task Element to delete</param>
		override protected int? Delete(BoeTaskElementDTO dtoToDelete)
		{
			int? toReturn = null;
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				if (dtoToDelete == null)
				{
					throw new ArgumentNullException(nameof(dtoToDelete));
				}

				this.Log.Debug(string.Format("BoeTaskElementDTODataLoader.Delete => Ntid: {1}, BoeId: {0}, Item Id: {2}, Hours: {3}, Cost: {4}, MOQ: {5}",
					dtoToDelete.BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name, dtoToDelete.Id, dtoToDelete.TotalHours, dtoToDelete.TotalCost, dtoToDelete.MOQHoursEquation));

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					// This delete will delete the task element, the labor types, and the labor spreads associated with TaskElementDetailID
					toReturn = gbe.deleteBOETaskElement(dtoToDelete.Id, dtoToDelete.UpdateDate);
				}
			}

			return toReturn;
		}

		#region Task Element detail

		/// <summary>
		/// Save/Create the Task Element Details
		/// </summary>
		/// <param name="inTaskDetail">the element to save</param>
		/// <returns>the task element details</returns>
		virtual public int CreateOrSaveTaskElementDetail(int BoeID, BoeTaskElementDTO inTaskDetail)
		{
			if (inTaskDetail == null)
			{
				throw new ArgumentNullException(nameof(inTaskDetail));
			}

			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				// Make sure that RTE data is loaded, that way we do not wipe it out..
				this.LoadRTEFields(new List<BoeTaskElementDTO>() { inTaskDetail });

				int TaskElementID = 0;

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					IEnumerable<int?> resultsLinq = from s in gbe.upsertBOETaskElement(
							inTaskDetail.Id,
							inTaskDetail.BOETaskID,
							inTaskDetail.TaskTitle,
							inTaskDetail.Description,
							inTaskDetail.StartDate,
							inTaskDetail.EndDate,
							inTaskDetail.MOQHoursEquation,
							string.Empty,
							inTaskDetail.MOQText,
							inTaskDetail.MOQType == MOQType.None ? (int?)null : (int)inTaskDetail.MOQType,
							BoeID,
							inTaskDetail.LaborTypeWarningFlag,
							inTaskDetail.IMS_ID,
							(int)inTaskDetail.TaskElementType,
							inTaskDetail.UpdateDate,
							inTaskDetail.BOETaskElementOrder,
							inTaskDetail.AuthorUserId
							)
													select s;

					TaskElementID = Convert.ToInt32(resultsLinq.SingleOrDefault());

					// Set the new ID on the DTO for later use, if necessary
					if (inTaskDetail.Id < 1)
					{
						inTaskDetail.Id = TaskElementID;
					}

					// Need to save any workspace variables that are used by this task element
					// It may be possible that the workspaceVariables count is 0, but we still need
					// to go through with the save because a workspace variable assocation may have
					// been deleted

					this.SaveWorkspaceVariables(inTaskDetail.WorkspaceVariableIDs, TaskElementID, gbe);

				} // end using gbe

				// Need to save any ordinary variables that are used by this task element
				if (inTaskDetail.OrdinaryVariables.Count > 0)
				{
					foreach (OrdinaryVariableDto variable in inTaskDetail.OrdinaryVariables)
					{
						variable.TaskElementId = TaskElementID;
						if (variable.Updateable == UpdateType.None)
						{
							variable.Updateable = UpdateType.Upsert;
						}
					}
					this.SaveOrdinaryVariables(inTaskDetail.OrdinaryVariables);
				}

				if (Utilities.IsSkillMixEnabledForSystem)
				{
					// Save the Skill Mix tables
					if (inTaskDetail.SkillMixTable != null && inTaskDetail.SkillMixTable.Any())
					{
						List<SkillMixDTO> dtos = new List<SkillMixDTO>();
						foreach (SkillMixModelView skillMixModelView in inTaskDetail.SkillMixTable)
						{
							SkillMixDTO dto = skillMixModelView.ToDto();
							dto.BOEID = inTaskDetail.BoeID;
							dto.BOETaskElementID = inTaskDetail.Id;
							dtos.Add(dto);
						}

						this.skillMixDTOLoader.InsertSkillMix(dtos);
					}

					// Save the Common Disclosure DTOs
					if (inTaskDetail.CommonDisclosureTable != null && inTaskDetail.CommonDisclosureTable.Any())
					{
						List<CommonDisclosureSkillMixDTO> dtos = new List<CommonDisclosureSkillMixDTO>();
						foreach (CommonDisclosureModelView commonDisclosure in inTaskDetail.CommonDisclosureTable)
						{
							CommonDisclosureSkillMixDTO dto = commonDisclosure.ToDto();
							dto.BOEID = inTaskDetail.BoeID;
							dto.BOETaskElementID = inTaskDetail.Id;
							dtos.Add(dto);
						}

						this.commonDisclosureSMDTODataLoader.InsertCommonDisclosureSM(dtos);
					}
				}

				return TaskElementID;

			}
		}

		/// <summary>
		/// delete the task element detail
		/// </summary>
		/// <param name="inTaskElementDetail">task element detail</param>
		virtual public void DeleteTaskElementDetail(BoeTaskElementDTO inTaskElementDetail)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				if (inTaskElementDetail == null)
				{
					throw new ArgumentNullException(nameof(inTaskElementDetail));
				}

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.deleteBOETaskElement(inTaskElementDetail.Id, inTaskElementDetail.UpdateDate);
				}
			}
		}

		/// <summary>
		/// This function will determine if the ordinary variable is being deleted or inserted/updated
		/// </summary>
		/// <param name="inOrdinaryVars">Ordinary variables that need to be adjusted in the database</param>
		public Dictionary<int, int> SaveOrdinaryVariables(Collection<OrdinaryVariableDto> inOrdinaryVars)
		{
			// HACK: This is only in place until the Full Object is complete.
			return this.ordinaryVariableLoader.Save(inOrdinaryVars);
		}

		/// <summary>
		/// Save all workspace variables that are currently being used by the MOQ Equation. The SP will handle any cleaning
		/// that is necessary (like a workspace variable that was being used but is no more)
		/// </summary>
		/// <param name="inWorkspaceVars">workspace variable IDs</param>
		/// <param name="inTaskElementID">the task element ID</param>
		virtual public void SaveWorkspaceVariables(Collection<int> inWorkspaceVars, int inTaskElementID, GenBoeEntities gbe)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				// The workspace variable comma delimited list
				// If there are no current workspace variables in use, clean up the table by sending a null to the SP
				string WorkspaceVars = null;

				if (inWorkspaceVars == null)
				{
					throw new ArgumentNullException(nameof(inWorkspaceVars));
				}
				if (gbe == null)
				{
					throw new ArgumentNullException(nameof(gbe));
				}

				// TODO the requirement for needing a hanging "," is very unintuitive. revisit this later.
				if (inWorkspaceVars.Count != 0)
				{
					WorkspaceVars = string.Join(",", inWorkspaceVars.Select(x => x).ToArray()) + ",";
				}

				gbe.insertBOETaskElementWorkspaceVariable(inTaskElementID, WorkspaceVars);
			}
		}

		/// <summary>
		/// Save BOE Task Element Labor warning
		/// </summary>
		/// <param name="inBOETaskElementID"></param>
		/// <param name="inLaborTypeWarningFlag"></param>
		virtual public void SaveBOETaskElementLaborTypeWarning(int inBOETaskElementID, bool inLaborTypeWarningFlag)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.updateBOETaskElementLaborTypeWarningFlag(inBOETaskElementID, inLaborTypeWarningFlag);

				}
			}
		}

		#endregion

		#region Labor Type

		/// <summary>
		/// This function will save an edited LM Labor Type or create a new one
		/// </summary>
		/// <param name="inLMLaborType"></param>
		/// <returns>a single LM void Type</returns>
		virtual public int CreateorSaveLMLaborType(int TaskElementID, ResourceTypeDto inSaveLMLaborType)
		{
			// HACK: This is only in place until the Full Object is complete.
			if (inSaveLMLaborType == null)
			{
				throw new ArgumentNullException(nameof(inSaveLMLaborType));
			}
			inSaveLMLaborType.TaskElementId = TaskElementID;
			return this.resourceTypeLoader.Save(inSaveLMLaborType).Value;
		}

		/// <summary>
		/// Delete a selected LM Labor type
		/// </summary>
		/// <param name="inLMLaborTypeID">the LM Labor type to delete</param>
		virtual public void DeleteLMLaborType(ResourceTypeDto inDeleteLaborType)
		{
			// HACK: This is only in place until the Full Object is complete.
			this.resourceTypeLoader.Save(inDeleteLaborType);
		}

		/// <summary>
		/// Save BOE Labor Type Custom Field Containers
		/// </summary>
		/// <param name="inBOELaborTypeCustomFieldValueContainers"></param>
		virtual public void SaveBOELaborTypeCustomFieldValueContainers(Collection<CustomFieldValueContainer> inBOELaborTypeCustomFieldValueContainers, int inLaborTypeID)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				if (inBOELaborTypeCustomFieldValueContainers == null)
				{
					throw new ArgumentNullException(nameof(inBOELaborTypeCustomFieldValueContainers));
				}

				foreach (CustomFieldValueContainer BOELaborTypeCustomFieldValueXref in inBOELaborTypeCustomFieldValueContainers)
				{
					this.SaveBOELaborTypeCustomFieldValueXref(BOELaborTypeCustomFieldValueXref, inLaborTypeID);
				}
			}
		}

		/// <summary>
		/// Save a single BOE Labor Type Field Container
		/// </summary>
		/// <param name="inCustomFieldValueXrefDTO"></param>
		virtual public void SaveBOELaborTypeCustomFieldValueXref(CustomFieldValueContainer inCustomFieldValueXrefDTO, int inLaborTypeID)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				if (inCustomFieldValueXrefDTO == null)
				{
					throw new ArgumentNullException(nameof(inCustomFieldValueXrefDTO));
				}

				if (inCustomFieldValueXrefDTO.Updateable == UpdateType.Deleted)
				{
					this.DeleteBoeLaborTypeCustomFieldValueContainer(inCustomFieldValueXrefDTO, inLaborTypeID);
				}
				else if (inCustomFieldValueXrefDTO.Updateable == UpdateType.Upsert)
				{
					this.UpdateBoeLaborTypeCustomFieldValueContainer(inCustomFieldValueXrefDTO, inLaborTypeID);
				}
			}
		}

		/// <summary>
		/// Upsert a BOE Labor Type Custom Field Value Container
		/// </summary>
		/// <param name="inCustomFieldValueContainer"></param>
		virtual public void UpdateBoeLaborTypeCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inLaborTypeID)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				if (inCustomFieldValueContainer == null)
				{
					throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
				}

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.upsertBOELaborTypeCustomFieldValue(inCustomFieldValueContainer.ContainerID,
											inLaborTypeID,
											inCustomFieldValueContainer.CustomFieldID,
											inCustomFieldValueContainer.CustomFieldValueID,
											inCustomFieldValueContainer.OpenEndedValue,
											inCustomFieldValueContainer.UpdateDate,
											inCustomFieldValueContainer.IsOpenEnded);

				}
			}
		}

		/// <summary>
		/// Delete a BOE Labor Type custom field container
		/// </summary>
		/// <param name="inCustomFieldValueContainer"></param>
		virtual public void DeleteBoeLaborTypeCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inLaborTypeID)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.log))
			{
				if (inCustomFieldValueContainer == null)
				{
					throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
				}

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.deleteBOELaborTypeCustomFieldValue(inCustomFieldValueContainer.ContainerID, inLaborTypeID, inCustomFieldValueContainer.CustomFieldValueID, inCustomFieldValueContainer.UpdateDate, inCustomFieldValueContainer.IsOpenEnded);
				}
			}
		}

		#endregion LaborType

		#region Labor Spread

		/// <summary>
		/// Insert or Save LM Labor Spread
		/// </summary>
		/// <param name="inSaveLaborSpread">Labor Spread Id</param>
		/// <returns>Labor Spread ID Created or Saved</returns>
		virtual public int CreateOrSaveLMLaborSpread(int LaborTypeID, ResourceSpreadDto inSaveLaborSpread)
		{
			// HACK: This is only in place until the Full Object is complete.
			if (inSaveLaborSpread == null)
			{
				throw new ArgumentNullException(nameof(inSaveLaborSpread));
			}
			inSaveLaborSpread.LaborTypeId = LaborTypeID;
			return this.resourceSpreadLoader.Save(inSaveLaborSpread).Value;
		}

		/// <summary>
		/// Insert or Save LM Labor Spread
		/// </summary>
		/// <param name="inSaveLaborSpread">Labor Spread Id</param>
		/// <returns>Labor Spread ID Created or Saved</returns>
		virtual public int DeleteLMLaborSpreadsByLaborTypeID(int LaborTypeID)
		{
			// HACK: This is only in place until the Full Object is complete.
			return this.resourceSpreadLoader.DeleteByResourceTypeId(LaborTypeID);
		}

		#endregion Labor Spread

		#endregion Commits

		#region Private Helpers

		/// <summary>
		/// Gets the Task Ordinary Variables.
		/// </summary>
		/// <param name="ids">The list of boe task element Ids to retrieve the ordinary variables against.</param>
		/// <param name="gbe">The database context.</param>
		/// <returns>A list of Task Ordinary Variables.</returns>
		private static List<OrdinaryVariableDto> GetOrdinaryVariables(ICollection<int> ids, GenBoeEntities gbe)
		{
			return gbe.OrdinaryVariables.Where(oV => ids.Contains(oV.BOETaskElementID))
									 .Select(oV => new OrdinaryVariableDto
									 {
										 Id = oV.OrdinaryVariableID,
										 OrdinaryVariableName = oV.OrdinaryVariableName,
										 OrdinaryVariableValue = oV.OrdinaryVariableValue,
										 IsPercentage = oV.IsPercentage,
										 ValueType = (VarValueType)oV.ValueTypeID,
										 SortBOEBy = (VarSortBOEBy)oV.SortByID,
										 UpdateDate = oV.UpdateDT,
										 TaskElementId = oV.BOETaskElementID,
										 SelectedBOEsToSumIEnum = oV.SumOfBOE_OrdinaryVariableXREF
											.Select(sXref => new SelectBOEsToSum
											{
												OVSumID = sXref.OVSumID,
												OrdinaryVariableID = sXref.OrdinaryVariableID,
												BoeID = sXref.BOEID,
												CLINID = sXref.CLINID,
												WBSID = sXref.WBSID
											}),
										 SumVariableResourceTypeIDsIEnum = oV.OrdinaryVariableSumVariableResourceTypeXREFs.Select(ovXref => ovXref.SumVariableResourceTypeID),
										 DefaultSize = oV.DefaultSize ?? string.Empty
									 }).ToList();
		}

		/// <summary>
		/// Gets the Task Labor Resources.
		/// </summary>
		/// <param name="ids">The list of boe task element Ids to retrieve the resources against.</param>
		/// <param name="gbe">The database context.</param>
		/// <returns>A list of Task Labor Resources.</returns>
		private static List<ResourceTypeDto> GetTaskLabors(ICollection<int> ids, GenBoeEntities gbe)
		{
			List<ResourceTypeDto> toReturn = gbe.BOELaborTypes.Where(lT => ids.Contains(lT.BOETaskElementID))
														.OrderBy(lT => lT.LaborSortId)
														.ThenBy(lT => lT.BOELaborTypeID)
														.Select(lT => new ResourceTypeDto
														{
															Id = lT.BOELaborTypeID,
															PerformingOrgID = lT.PerformingOrganizationID,
															ResourceID = lT.ResourceID,
															BusinessResourceCodeID = lT.BRCResourceID,
															ValueSpread = lT.ValueSpread,
															SpreadCurveIDValue = lT.SpreadCurveID,
															PercentSpread = lT.PercentSpread,
															StartDateValue = lT.BOELaborTypeStartDate,
															EndDateValue = lT.BOELaborTypeEndDate,
															TaskElementId = lT.BOETaskElementID,
															UpdateDate = lT.UpdateDT,
															PercentSpreadLocked = lT.PercentSpreadLocked,
															HourSpreadLocked = lT.HourSpreadLocked,
															WBSID = lT.WBSID,
															CLINID = lT.CLINID,
															SpreadType = lT.SpreadTypeID.HasValue ? (SpreadType)lT.SpreadTypeID.Value : SpreadType.NotSet,
															CanOffload = lT.CanOffload ?? false,
															LaborTypeOrder = lT.LaborSortId,
															LaborSpreadsIEnum = lT.BOELaborSpreads
																.Select(lS => new ResourceSpreadDto
																{
																	Id = lS.BOELaborSpreadID,
																	LaborSpreadDate = lS.LaborSpreadDate,
																	LaborSpreadValue = lS.LaborSpreadValue ?? 0,
																	LaborTypeId = lS.BOELaborTypeID
																}),
															CustomFieldValueContainersIEnum = lT.BOELaborTypeCustomFieldValueXREFs
																.Select(cf => new CustomFieldValueContainer
																{
																	ContainerID = cf.BLTCFVID,
																	CustomFieldValueID = cf.CustomFieldValueID,
																	CustomFieldID = cf.CustomFieldValue.CustomFieldID,
																	UpdateDate = cf.UpdateDT,
																	IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
																	OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
																})
														}).ToList();

			ResourceTypeLoader.LoadSikorskyFields(gbe, toReturn);

			return toReturn;
		}

		/// <summary>
		/// Does post processing after the retrieval of data
		/// </summary>
		/// <param name="result">Data to adjust</param>
		/// <param name="ordinaryVariables">Ordinary Variables</param>
		/// <param name="taskElementLabors">Task Element Labors</param>
		/// <param name="costPrecision">The Cost precision for the workspace.</param>
		/// <param name="hoursPrecision">The Hours precision for the workspace.</param>
		private static void DoPostProcessing(List<BoeTaskElementDTO> result, List<OrdinaryVariableDto> ordinaryVariables, List<ResourceTypeDto> taskElementLabors, int hoursPrecision, int costPrecision, ICollection<SkillMixDTO> skillMix, ICollection<CommonDisclosureSkillMixDTO> commonDisclosures, ISkillMixDTOLoader skillMixDTOLoader, ICommonDisclosureSMDTODataLoader commonDisclosureSMDTODataLoader)
		{
			result.AsParallel().ForAll(
				bT =>
				{
					bT.CustomFieldValueContainers = bT.CustomFieldValueContainersIEnum.ToCollection(); bT.CustomFieldValueContainersIEnum = null;
					bT.WorkspaceVariableIDs = bT.WorkspaceVariableIDsIEnum.ToCollection(); bT.WorkspaceVariableIDsIEnum = null;

					bT.OrdinaryVariables = ordinaryVariables.Where(oV => bT.Id == oV.TaskElementId).ToCollection();
					bT.OrdinaryVariables.ToList().ForEach(oV =>
					{
						oV.BoeID = bT.BoeID;
						oV.SelectedBOEsToSum = oV.SelectedBOEsToSumIEnum.ToCollection(); oV.SelectedBOEsToSumIEnum = null;
						oV.SumVariableResourceTypeIDs = oV.SumVariableResourceTypeIDsIEnum.ToCollection(); oV.SumVariableResourceTypeIDsIEnum = null;
					});

					bT.taskElementLabors = taskElementLabors.Where(lT => bT.Id == lT.TaskElementId).ToCollection();
					bT.taskElementLabors.ToList().ForEach(tL =>
					{
						tL.CustomFieldValueContainers = tL.CustomFieldValueContainersIEnum.ToCollection(); tL.CustomFieldValueContainersIEnum = null;

						if (tL.SpreadCurveID == SpreadCurves.DiscreteCost || tL.SpreadCurveID == SpreadCurves.DiscreteHours)
						{
							tL.LaborSpreads = tL.LaborSpreadsIEnum.ToCollection();
						}
						else
						{
							// auto-generate the Spreads
							if (tL.StartDate.HasValue && tL.EndDate.HasValue && tL.SpreadCurveID.HasValue)
							{
								int precision = tL.SpreadType == SpreadType.Cost ? costPrecision : hoursPrecision;
								LaborSpreadRequest request = new LaborSpreadRequest
								{
									CurveID = tL.SpreadCurveID,
									StartDate = tL.StartDateValue,
									EndDate = tL.EndDateValue,
									HourSpread = tL.ValueSpread ?? 0
								};
								try
								{
									tL.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(request, precision);
								}
								catch (GenValidationException)
								{
									// Catching exception when having negative range, just return empty list of spreads
									tL.LaborSpreads = new Collection<ResourceSpreadDto>();
								}
							}
						}

						// Always clear out the enumeration
						tL.LaborSpreadsIEnum = null;

						tL.LaborSpreads.ToList().ForEach(tS =>
						{
							tS.LaborSpreadDate = tS.LaborSpreadDate.Normalize();
							tS.BoeID = bT.BoeID;
						});

						tL.StartDateValue = tL.StartDateValue.Normalize();
						tL.EndDateValue = tL.EndDateValue.Normalize();
						tL.BoeID = bT.BoeID;
					});

					bT.TotalHours = bT.taskElementLabors.Where(z => z.SpreadType == SpreadType.Hours).Sum(l => l.ValueSpread);
					bT.TotalCost = bT.taskElementLabors.Where(z => z.SpreadType == SpreadType.Cost).Sum(l => l.ValueSpread);

					// recalculate unlocked Percentage Spread based on total hours
					bT.taskElementLabors.ToList().ForEach(tL =>
					{
						if (tL.HourSpreadLocked)
						{
							if (!bT.TotalHours.HasValue || bT.TotalHours.Value == 0 || !tL.ValueSpread.HasValue || tL.ValueSpread.Value == 0)
							{
								tL.PercentSpread = 0m;
							}
							else
							{
								tL.PercentSpread = Utilities.AdjustPrecision((tL.ValueSpread.Value / bT.TotalHours.Value) * 100m, hoursPrecision);
							}
						}
					});

					// labor task elements should be grabbing the date from the database
					bT.StartDate = bT.StartDate.Normalize();
					bT.EndDate = bT.EndDate.Normalize();

					if (skillMix == null)
					{
						bT.SkillMixTable = skillMixDTOLoader.GetByBOETaskElementID(bT.Id).Select(x => new SkillMixModelView(x)).OrderBy(x => x.ResourceOld).ThenBy(y => y.ResourceNew).ToCollection();
					}
					else
					{
						bT.SkillMixTable = skillMix.Where(r => r.BOETaskElementID == bT.Id).Select(x => new SkillMixModelView(x)).OrderBy(x => x.ResourceOld).ThenBy(y => y.ResourceNew).ToCollection();
					}

					if (commonDisclosures == null)
					{
						bT.CommonDisclosureTable = commonDisclosureSMDTODataLoader.GetByBOETaskElementID(bT.Id).Select(x => new CommonDisclosureModelView(x)).OrderBy(d => d.ResourceID).ThenBy(e => e.BusinessResourceID).ToCollection();
					}
					else
					{
						bT.CommonDisclosureTable = commonDisclosures.Where(r => r.BOETaskElementID == bT.Id).Select(x => new CommonDisclosureModelView(x)).OrderBy(d => d.ResourceID).ThenBy(e => e.BusinessResourceID).ToCollection();
					}
				});
		}

		/// <summary>
		/// Converts the Task Element DTO into a BOETaskElement entity.
		/// </summary>
		/// <param name="dtoToConvert">dto to convert</param>
		/// <returns>entity representing the dto</returns>
		protected override BOETaskElement ConvertDtoToEntity(BoeTaskElementDTO dtoToConvert)
		{
			if (dtoToConvert == null)
			{
				throw new ArgumentNullException(nameof(dtoToConvert));
			}

			BOETaskElement entity = new BOETaskElement()
			{
				BOETaskElementID = dtoToConvert.Id,
				TaskID = dtoToConvert.BOETaskID,
				TaskTitle = dtoToConvert.TaskTitle,
				TaskDescription = dtoToConvert.Description,
				TaskStartDate = dtoToConvert.StartDate,
				TaskEndDate = dtoToConvert.EndDate,
				MOQHoursEquation = dtoToConvert.MOQHoursEquation,
				AuthorUserId = dtoToConvert.AuthorUserId,
				MOQCostEquation = string.Empty,
				MOQText = dtoToConvert.MOQText,
				MOQTypeID = dtoToConvert.MOQType == MOQType.None ? (int?)null : (int)dtoToConvert.MOQType,
				BOEID = dtoToConvert.BoeID,
				LaborTypeWarningFlag = dtoToConvert.LaborTypeWarningFlag.HasValue ? dtoToConvert.LaborTypeWarningFlag.Value : false, // i'm assuming the default of false
				IMS_ID = dtoToConvert.IMS_ID,
				TaskElementTypeID = (int)dtoToConvert.TaskElementType,
				UpdateDT = dtoToConvert.UpdateDate,
				SortOrderID = dtoToConvert.BOETaskElementOrder
			};

			return entity;
		}

		#endregion
	}
}