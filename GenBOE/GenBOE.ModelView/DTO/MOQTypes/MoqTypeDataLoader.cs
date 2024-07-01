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
    using System.Linq;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;

    /// <summary>
    /// Loader for MOQ Type Selection and MOQ Table Data
    /// </summary>
    public class MoqTypeDataLoader : DataLoader<MoqTypeSelection>, IMoqTypeDataLoader
    {
        /// <summary>
        /// MOQ Type Table Custom Field Value XREF Loader
        /// </summary>
        private IMoqTypeTableCustomFieldValueXREFLoader moqTypeTableCustomFieldValueXREFLoader;

		/// <summary>
		/// MOQ Type resource hours loader
		/// </summary>
		private IMOQTypeSelectionTableDataResourceHoursDTOLoader moqTypeSelectionTableDataResourceHoursDTOLoader;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="moqTypeTableCustomFieldValueXREFLoader">MOQ Type Table Custom Field Value XREF Loader</param>
		/// <param name="moqTypeSelectionTableDataResourceHoursDTOLoader">MOQ Type Resource Hours Loader</param>
		public MoqTypeDataLoader(IMoqTypeTableCustomFieldValueXREFLoader moqTypeTableCustomFieldValueXREFLoader, IMOQTypeSelectionTableDataResourceHoursDTOLoader moqTypeSelectionTableDataResourceHoursDTOLoader)
        {
            this.Log = new Logger(typeof(MoqTypeDataLoader));
            this.moqTypeTableCustomFieldValueXREFLoader = moqTypeTableCustomFieldValueXREFLoader;
			this.moqTypeSelectionTableDataResourceHoursDTOLoader = moqTypeSelectionTableDataResourceHoursDTOLoader;

		}

        /// <summary>
        /// Get MOQ Type Selections by MOQ Type Selection IDs
        /// </summary>
        /// <param name="ids">MOQ Type Selection IDs</param>
        /// <returns>MOQ Type Selections and Table Data for the given IDs</returns>
        [DbQuery]
        public override ICollection<MoqTypeSelection> GetByIds(ICollection<int> ids)
        {
            ICollection<MoqTypeSelection> toReturn = new Collection<MoqTypeSelection>();
			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from m in gbe.MOQTypeSelections.Where(mt => ids.Contains(mt.MOQTypeSelectionId))
                                join t in gbe.BOETaskElements on m.TaskId equals t.BOETaskElementID
                                select new MoqTypeSelection
                                {
                                    Id = m.MOQTypeSelectionId,
                                    TaskId = m.TaskId,
                                    SelectedMOQType = (MOQType)m.MOQTypeSelection1,
                                    UpdateDate = m.UpdateDT,
                                    Order = m.Order,
                                    CerName = m.CERName,
                                    DescriptionHoursRequired = m.HoursDescription,
                                    SmeReason = m.SubjectMatterExpert,
                                    SmeHoursLogic = m.HoursLogicAndAssumptions,
                                    SmeDurationLogic = m.DurationLogicAndAssumptions,
                                    SmeTaskEstimates = m.EstimateTasks,
                                    Rationale = m.Rationale,
                                    SkillMixRationale = m.SkillMix,
									HistoricalReferenceExplanation = m.HistoricalReferenceExplanation,
                                    BoeId = t.BOEID
                                }).OrderBy(x => x.Order).ToCollection<MoqTypeSelection>();

                    this.GetTableDataForMoqTypes(toReturn, gbe);
				}

				this.DoPostProcessing(toReturn, null);
            }

            return toReturn;
        }

        /// <summary>
        /// Get all MOQ Type Selections for the given Workspace ID
        /// </summary>
        /// <param name="workspaceId">Workspace ID</param>
        /// <returns>MOQ Type Selections for the given Workspace ID</returns>
        [DbQuery]
        public ICollection<MoqTypeSelection> GetByWorkspaceId(int workspaceId)
        {
            ICollection<MoqTypeSelection> toReturn = new Collection<MoqTypeSelection>();
			ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from m in gbe.MOQTypeSelections
                                join t in gbe.BOETaskElements on m.TaskId equals t.BOETaskElementID
                                join b in gbe.BOEs on t.BOEID equals b.BOEID
                                where b.WorkspaceID == workspaceId
                                select new MoqTypeSelection
                                {
                                    Id = m.MOQTypeSelectionId,
                                    TaskId = m.TaskId,
                                    SelectedMOQType = (MOQType)m.MOQTypeSelection1,
                                    UpdateDate = m.UpdateDT,
                                    Order = m.Order,
                                    CerName = m.CERName,
                                    DescriptionHoursRequired = m.HoursDescription,
                                    SmeReason = m.SubjectMatterExpert,
                                    SmeHoursLogic = m.HoursLogicAndAssumptions,
                                    SmeDurationLogic = m.DurationLogicAndAssumptions,
                                    SmeTaskEstimates = m.EstimateTasks,
                                    Rationale = m.Rationale,
                                    SkillMixRationale = m.SkillMix,
									HistoricalReferenceExplanation = m.HistoricalReferenceExplanation,
									BoeId = t.BOEID
                                }).OrderBy(x => x.Order).ToCollection<MoqTypeSelection>();

                    this.GetTableDataForMoqTypes(toReturn, gbe);
                }

				resourceHours = this.moqTypeSelectionTableDataResourceHoursDTOLoader.GetByWorkspaceId(workspaceId);

				this.DoPostProcessing(toReturn, resourceHours);
            }

            return toReturn;
        }

        /// <summary>
        /// Get all MOQ Type Selections for the given BOE ID
        /// </summary>
        /// <param name="boeId">BOE ID</param>
        /// <returns>MOQ Type Selections for the given BOE ID</returns>
        [DbQuery]
        public ICollection<MoqTypeSelection> GetByBoeId(int boeId)
        {
            ICollection<MoqTypeSelection> toReturn = new Collection<MoqTypeSelection>();
			ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours = new Collection<MOQTypeSelectionTableDataResourceHoursDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from m in gbe.MOQTypeSelections
                                join t in gbe.BOETaskElements on m.TaskId equals t.BOETaskElementID
                                where t.BOEID == boeId
                                select new MoqTypeSelection
                                {
                                    Id = m.MOQTypeSelectionId,
                                    TaskId = m.TaskId,
                                    SelectedMOQType = (MOQType)m.MOQTypeSelection1,
                                    UpdateDate = m.UpdateDT,
                                    Order = m.Order,
                                    CerName = m.CERName,
                                    DescriptionHoursRequired = m.HoursDescription,
                                    SmeReason = m.SubjectMatterExpert,
                                    SmeHoursLogic = m.HoursLogicAndAssumptions,
                                    SmeDurationLogic = m.DurationLogicAndAssumptions,
                                    SmeTaskEstimates = m.EstimateTasks,
                                    Rationale = m.Rationale,
                                    SkillMixRationale = m.SkillMix,
									HistoricalReferenceExplanation = m.HistoricalReferenceExplanation,
									BoeId = t.BOEID
                                }).OrderBy(x => x.Order).ToCollection<MoqTypeSelection>();

                    this.GetTableDataForMoqTypes(toReturn, gbe);
					resourceHours = this.moqTypeSelectionTableDataResourceHoursDTOLoader.GetByBOEID(boeId);

				}

                this.DoPostProcessing(toReturn, resourceHours);
            }

            return toReturn;
        }

        /// <summary>
        /// Delete a MOQ Type Selection and related Table Data
        /// </summary>
        /// <param name="dtoToDelete">MOQ Type Selection to delete</param>
        /// <returns>ID of deleted MOQ Type Selection</returns>
        protected override int? Delete(MoqTypeSelection dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    foreach(MoqTableData table in dtoToDelete.TableData.Where(x => x.Id > 0))
                    {
						gbe.deleteMOQTypeSelectionTableDataResourceHours(table.Id);
                        gbe.deleteMOQTypeSelectionTableData(table.Id, table.UpdateDate);
                    }

                    toReturn = gbe.deleteMOQTypeSelection(dtoToDelete.Id, dtoToDelete.UpdateDate);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert a MOQ Type Selection and related Table Data
        /// </summary>
        /// <param name="dtoToUpsert">MOQ Type Selection to upsert</param>
        /// <returns>ID of upserted MOQ Type Selection</returns>
        protected override int? Upsert(MoqTypeSelection dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.upsertMOQTypeSelection(dtoToUpsert.Id, dtoToUpsert.TaskId, (int)dtoToUpsert.SelectedMOQType, dtoToUpsert.UpdateDate, 
                        dtoToUpsert.Order, dtoToUpsert.CerName, dtoToUpsert.DescriptionHoursRequired, dtoToUpsert.SmeReason, 
                        dtoToUpsert.SmeHoursLogic, dtoToUpsert.SmeDurationLogic, dtoToUpsert.SmeTaskEstimates, dtoToUpsert.Rationale, dtoToUpsert.SkillMixRationale, 
						dtoToUpsert.HistoricalReferenceExplanation).FirstOrDefault();

                    foreach(MoqTableData table in dtoToUpsert.TableData.Where(x => x.DateOfReport.Date != DateTime.MinValue.Date))
                    {
                        int? tableId = gbe.upsertMOQTypeSelectionTableData(table.Id, toReturn, table.UpdateDate, table.Order, table.TableName, table.RepositoryName, table.QueryType,
                            table.DateOfReport, table.HistoricalProgramName, table.ContractNumber, table.WbsElement, table.PoPStart ?? DateTime.MinValue, table.PoPEnd ?? DateTime.MinValue, table.TotalWbsHours,
                            table.AdditionalQueryFilters, table.TotalRelevantHours).FirstOrDefault();
                        
                        // Save custom fields
                        if (tableId.HasValue)
                        {
                            this.moqTypeTableCustomFieldValueXREFLoader.SaveMoqTypeTableCustomFieldValueContainers(table.CustomFieldValueContainers, tableId.Value);

							if (table.ResourceHours != null && table.ResourceHours.Count > 0)
							{
								// Set the ID for the new MOQ Table
								foreach (MOQTypeSelectionTableDataResourceHoursDTO resourceHours in table.ResourceHours)
								{
									resourceHours.MOQTypeSelectionTableDataId = tableId.Value;
									resourceHours.BOEID = dtoToUpsert.BoeId;
									resourceHours.BOETaskElementID = dtoToUpsert.TaskId;
								}

								this.moqTypeSelectionTableDataResourceHoursDTOLoader.InsertMOQTypeSelectionTableDataResourceHours(table.ResourceHours);
							}
						}
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Save the MOQ Type Tables from an import by deleting the original tables and saving the new ones
        /// </summary>
        /// <param name="moqType">MOQ Type containing the tables</param>
        public void SaveImportedMoqTypeTables(MoqTypeSelection moqType)
        {
            _ = moqType ?? throw new ArgumentNullException(nameof(moqType));

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // Delete original tables
                    foreach(MoqTableData table in moqType.TableData.Where(x => x.Updateable == UpdateType.Deleted))
                    {
						gbe.deleteMOQTypeSelectionTableDataResourceHours(table.Id);
                        gbe.deleteMOQTypeSelectionTableData(table.Id, table.UpdateDate);
                    }

                    // Save new imported tables
                    foreach (MoqTableData table in moqType.TableData.Where(x => x.Updateable == UpdateType.Upsert))
                    {
                        int? tableId = gbe.upsertMOQTypeSelectionTableData(table.Id, moqType.Id, table.UpdateDate, table.Order, table.TableName, table.RepositoryName, table.QueryType,
                            table.DateOfReport, table.HistoricalProgramName, table.ContractNumber, table.WbsElement, table.PoPStart ?? DateTime.MinValue, table.PoPEnd ?? DateTime.MinValue, table.TotalWbsHours,
                            table.AdditionalQueryFilters, table.TotalRelevantHours).FirstOrDefault();

                        // Save custom fields
                        if (tableId.HasValue)
                        {
                            this.moqTypeTableCustomFieldValueXREFLoader.SaveMoqTypeTableCustomFieldValueContainers(table.CustomFieldValueContainers, tableId.Value);
							if (table.ResourceHours != null && table.ResourceHours.Count > 0)
							{
								// Set the ID for the new MOQ Table
								foreach (MOQTypeSelectionTableDataResourceHoursDTO resourceHours in table.ResourceHours)
								{
									resourceHours.MOQTypeSelectionTableDataId = tableId.Value;
									resourceHours.BOEID = moqType.BoeId;
									resourceHours.BOETaskElementID = moqType.TaskId;
								}

								this.moqTypeSelectionTableDataResourceHoursDTOLoader.InsertMOQTypeSelectionTableDataResourceHours(table.ResourceHours);
							}
						}
                    }
                }
            }
        }

        /// <summary>
        /// Get the MOQ Tyoe Table Data  for the MOQ Type Selections
        /// </summary>
        /// <param name="moqTypeSelections"></param>
        /// <param name="gbe"></param>
        [DbQuery]
        private void GetTableDataForMoqTypes(ICollection<MoqTypeSelection> moqTypeSelections, GenBoeEntities gbe)
        {
            ICollection<int> moqTypeIds = moqTypeSelections.Select(x => x.Id).ToCollection();
            ICollection<MoqTableData> allTableData = (from td in gbe.MOQTypeSelectionTableDatas
                                                      .Where(d => moqTypeIds.Contains(d.MOQTypeSelectionId))
                                   select new MoqTableData
                                   {
                                       Id = td.MOQTypeSelectionTableDataId,
                                       MOQTypeSelectionId = td.MOQTypeSelectionId,
                                       UpdateDate = td.UpdateDT,
                                       Order = td.Order,
                                       TableName = td.TableName,
                                       RepositoryName = td.RepositoryName,
                                       QueryType = td.QueryType,
                                       DateOfReport = td.DateOfReport,
                                       HistoricalProgramName = td.HistoricalProgramName,
                                       ContractNumber = td.ContractNumber,
                                       WbsElement = td.WbsElement,
                                       PoPStart = td.PeriodOfPerformanceStartDate,
                                       PoPEnd = td.PeriodOfPerformanceEndDate,
                                       TotalWbsHours = td.TotalWbsHours,
                                       AdditionalQueryFilters = td.AdditionalQueryFilters,
                                       TotalRelevantHours = td.TotalRelevantHoursAfterQueryFilters,
                                       CustomFieldValueContainersIEnum = td.MoqTypeTableCustomFieldValueXREFs
                                            .Select(cf => new CustomFieldValueContainer
                                            {
                                                ContainerID = cf.Id,
                                                CustomFieldValueID = cf.CustomFieldValueId,
                                                CustomFieldID = cf.CustomFieldValue.CustomFieldID,
                                                UpdateDate = cf.UpdateDT,
                                                IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription
                                            })
                                   }).OrderBy(x => x.Order).ToCollection<MoqTableData>();

            foreach (MoqTypeSelection selection in moqTypeSelections)
            {
                selection.TableData.AddRange(allTableData.Where(x => x.MOQTypeSelectionId == selection.Id));

                if((selection.SelectedMOQType == MOQType.Historical || selection.SelectedMOQType == MOQType.Comparative) && selection.TableData.None())
                {
                    selection.TableData.Add(new MoqTableData());
                }
            }
        }

        /// <summary>
        /// Do post processing after saving MOQ Type Selections
        /// </summary>
        /// <param name="moqTypeSelections"></param>
        private void DoPostProcessing(ICollection<MoqTypeSelection> moqTypeSelections, ICollection<MOQTypeSelectionTableDataResourceHoursDTO> resourceHours)
        {
            // Handle custom field enums and resource Hours
            foreach (MoqTypeSelection selection in moqTypeSelections.Where(x => x.TableData.Any()))
            {
                foreach (MoqTableData table in selection.TableData)
                {
                    table.CustomFieldValueContainers = (table.CustomFieldValueContainersIEnum ?? new List<CustomFieldValueContainer>()).ToCollection();
                    table.CustomFieldValueContainersIEnum = null;

					if (resourceHours == null)
					{
						resourceHours = this.moqTypeSelectionTableDataResourceHoursDTOLoader.GetByMOQTypeSelectionTableDataId(table.Id);
					}
					else
					{
						table.ResourceHours = resourceHours.Where(r => r.MOQTypeSelectionTableDataId == table.Id).ToCollection();
					}
                }
            }
        }
    }
}
