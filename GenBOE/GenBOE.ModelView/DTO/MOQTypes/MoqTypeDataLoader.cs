// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Models;
    using IES.Common;

    /// <summary>
    /// Loader for MOQ Type Selection and MOQ Table Data
    /// </summary>
    public class MoqTypeDataLoader : DataLoader<MoqTypeSelection>, IMoqTypeDataLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MoqTypeDataLoader()
        {
            this.Log = new Logger(typeof(MoqTypeDataLoader));
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
                                    BoeId = t.BOEID
                                }).OrderBy(x => x.Order).ToCollection<MoqTypeSelection>();

                    this.GetTableDataForMoqTypes(toReturn, gbe);
                }
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
                                    BoeId = t.BOEID
                                }).OrderBy(x => x.Order).ToCollection<MoqTypeSelection>();

                    this.GetTableDataForMoqTypes(toReturn, gbe);
                }
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
                                    BoeId = t.BOEID
                                }).OrderBy(x => x.Order).ToCollection<MoqTypeSelection>();

                    this.GetTableDataForMoqTypes(toReturn, gbe);
                }
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
                    foreach(MoqTableData table in dtoToDelete.TableData)
                    {
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
                        dtoToUpsert.SmeHoursLogic, dtoToUpsert.SmeDurationLogic, dtoToUpsert.SmeTaskEstimates, dtoToUpsert.Rationale, dtoToUpsert.SkillMixRationale).FirstOrDefault();

                    foreach(MoqTableData table in dtoToUpsert.TableData)
                    {
                        gbe.upsertMOQTypeSelectionTableData(table.Id, toReturn, table.UpdateDate, table.Order, table.TableName, table.RepositoryName, table.QueryType,
                            table.DateOfReport, table.HistoricalProgramName, table.ContractNumber, table.WbsElement, table.PoPStart, table.PoPEnd, table.TotalWbsHours,
                            table.AdditionalQueryFilters, table.TotalRelevantHours);
                    }
                }
            }

            return toReturn;
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
                                       TotalRelevantHours = td.TotalRelevantHoursAfterQueryFilters
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
    }
}
