// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;

    /// <summary>
    /// Boe Discrepancy Report
    /// </summary>
    public class BOEDiscrepancyReport
    {
        #region Properties and Constructor

        /// <summary>
        /// WS Recalc 
        /// </summary>
        private IFullWorkspaceRecalculation wsRecalc { get; set; }

        /// <summary>
        /// User Data Loader
        /// </summary>
        private IUserDTODataLoader userLoader { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wsRecalc">Ws recalculation</param>
        public BOEDiscrepancyReport(IFullWorkspaceRecalculation wsRecalc, IUserDTODataLoader userLoader)
        {
            this.wsRecalc = wsRecalc;
            this.userLoader = userLoader;
        }

        #endregion

        /// <summary>
        /// Gets all the data for the report -> all BOEs and their Task Elements where either the labor or the cost is 
        /// not correct.
        /// </summary>
        /// <param name="ws">WS which we'll be checking</param>
        /// <returns>Data w/ discrepancies</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "secondTask", Justification="Not used, but it's created to make sure the data matches what we are looking for")]
        public ICollection<BoeDiscrepancyReportModelView> GetReport(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            List<BoeDiscrepancyReportModelView> allModelViews = new List<BoeDiscrepancyReportModelView>();
            
            HashSet<BoeTaskElementDTO> laborHourTaskElements = new HashSet<BoeTaskElementDTO>(this.wsRecalc.GetTaskElementsWithNonZeroDeltaLabor(ws));
            HashSet<BoeTaskElementDTO> costTaskElements = new HashSet<BoeTaskElementDTO>(this.wsRecalc.GetTaskElementsWithInconsistentCosts(ws));
            HashSet<OtherDirectCostDTO> odcTaskElements = new HashSet<OtherDirectCostDTO>(this.wsRecalc.GetElementsWithInconsistentODCs(ws));
			HashSet<BoeTaskElementDTO> resourceTaskElements = new HashSet<BoeTaskElementDTO>(this.wsRecalc.GetTaskElementsWithMissingResource(ws));

            if (laborHourTaskElements.Any() || costTaskElements.Any() || odcTaskElements.Any() || resourceTaskElements.Any())
            {
                // preload data
                HashSet<FullBoe> boes = new HashSet<FullBoe>(ws.Boes);
                ws.WbsElements.Any();
                ws.Clins.Any();
                HashSet<UserDTO> authorsForAllBoes = new HashSet<UserDTO>(this.userLoader.GetByIds(boes.SelectMany(x => x.AuthorIDs).Distinct().ToList()));

                ProcessTaskElements( allModelViews, boes, authorsForAllBoes, BoeInconsistencyEnum.Hours, laborHourTaskElements, ws);
                ProcessTaskElements( allModelViews, boes, authorsForAllBoes, BoeInconsistencyEnum.Cost, costTaskElements, ws);
                ProcessTaskElements( allModelViews, boes, authorsForAllBoes, BoeInconsistencyEnum.ODC, odcTaskElements, ws);
				ProcessTaskElements( allModelViews, boes, authorsForAllBoes, BoeInconsistencyEnum.Resource, resourceTaskElements, ws);
            }

            #region Grouping/Consolidating things into matching BOEs and Task Elements

            List<BoeDiscrepancyReportModelView> consolidatedModelViews = new List<BoeDiscrepancyReportModelView>();

            List<int> boeIdsFromModelViews = allModelViews.Select(x => x.BoeId).Distinct().OrderBy(x => x).ToList();

            foreach (int boeId in boeIdsFromModelViews)
            {
                BoeDiscrepancyReportModelView firstItem = allModelViews.First(x => x.BoeId == boeId);

                if(allModelViews.Count(x => x.BoeId == boeId) > 1)
                {
                    #region Task Element Grouping/Consolidation

                    List<BoeTaskDetailsMV> taskElementsForBoe = allModelViews.Where(x => x.BoeId == boeId).SelectMany(x => x.ElementsWithIssues).ToList();
                    List<BoeTaskDetailsMV> consolidatedTaskElements = new List<BoeTaskDetailsMV>();
                    List<int> taskElementIds = taskElementsForBoe.Select(x => x.TaskId).Distinct().ToList();

                    foreach (int id in taskElementIds)
                    {
                        if (taskElementsForBoe.Where(x => x.DiscrepancyEnum == BoeInconsistencyEnum.Cost 
                            || x.DiscrepancyEnum == BoeInconsistencyEnum.Hours).Count(x => x.TaskId == id) == 2)
                        {
                            // We are expecting that if we are here, we have one cost and one hours
                            // If that's not the case -> problem..
                            BoeTaskDetailsMV firstTask = taskElementsForBoe.Single(x => x.TaskId == id && x.DiscrepancyEnum == BoeInconsistencyEnum.Cost);
                            BoeTaskDetailsMV secondTask = taskElementsForBoe.Single(x => x.TaskId == id && x.DiscrepancyEnum == BoeInconsistencyEnum.Hours);

                            BoeTaskDetailsMV newElement = new BoeTaskDetailsMV()
                            {
                                TaskId = id,
                                DisplayedTaskId = firstTask.DisplayedTaskId,
                                TaskTitle = firstTask.TaskTitle,
                                DiscrepancyEnum = BoeInconsistencyEnum.HoursAndCost,
                                IsUsingEquivalentPerson = ws.IsUsingEquivalentPerson
                            };

                            consolidatedTaskElements.Add(newElement);
                        }
                        else
                        {
                            // either just a single item for the Id, or ODC and (Hours or Cost) task elements have the same Id. Either way, we should add them in
                            consolidatedTaskElements.AddRange(taskElementsForBoe.Where(x => x.TaskId == id).ToList());
                        }
                    }

                    #endregion

                    BoeDiscrepancyReportModelView newMVWithConsolidatedData = new BoeDiscrepancyReportModelView()
                    {
                        BoeId = boeId,
                        BoeAuthors = firstItem.BoeAuthors,
                        BoeTitle = firstItem.BoeTitle,
                        Clin = firstItem.Clin,
                        Wbs = firstItem.Wbs,
                        ElementsWithIssues = consolidatedTaskElements.OrderBy(x => x.TaskTitle).ToList()
                    };

                    consolidatedModelViews.Add(newMVWithConsolidatedData);
                }
                else
                {
                    consolidatedModelViews.Add(firstItem);
                }
            }

            #endregion

            return consolidatedModelViews;
        }

        /// <summary>
        /// Task in task elements and sets up data for the report
        /// </summary>
        /// <param name="modelViewsForReport">Data for the report</param>
        /// <param name="boes">BOEs for the WS</param>
        /// <param name="authorsForAllBoes">authors for the BOEs</param>
        /// <param name="inconsistency">Inconsistency type</param>
        /// <param name="elementsToProcess">Task elements to process</param>
        private static void ProcessTaskElements( List<BoeDiscrepancyReportModelView> modelViewsForReport, HashSet<FullBoe> boes, HashSet<UserDTO> authorsForAllBoes, 
            BoeInconsistencyEnum inconsistency, HashSet<BoeTaskElementDTO> elementsToProcess, WorkspaceDTO workspace)
        {
            if (elementsToProcess.Any())
            {
                Collection<int> boeIds = elementsToProcess.Select(x => x.BoeID).Distinct().ToCollection();

                object LOCK = new object();

                Parallel.ForEach(boeIds, new ParallelOptions { MaxDegreeOfParallelism = 5 }, boeId =>
                {
                    BoeDiscrepancyReportModelView item = GenerateAnItem( elementsToProcess, boes, boeId, authorsForAllBoes, inconsistency, workspace);

                    lock (LOCK) // add it to the data..
                    {
                        modelViewsForReport.Add(item);
                    }
                });
            }
        }

        /// <summary>
        /// Task in task elements and sets up data for the report
        /// </summary>
        /// <param name="modelViewsForReport">Data for the report</param>
        /// <param name="boes">BOEs for the WS</param>
        /// <param name="authorsForAllBoes">authors for the BOEs</param>
        /// <param name="inconsistency">Inconsistency type</param>
        /// <param name="elementsToProcess">elements to process</param>
        private static void ProcessTaskElements( List<BoeDiscrepancyReportModelView> modelViewsForReport, HashSet<FullBoe> boes, HashSet<UserDTO> authorsForAllBoes,
            BoeInconsistencyEnum inconsistency, HashSet<OtherDirectCostDTO> elementsToProcess, WorkspaceDTO workspace)
        {
            if (elementsToProcess.Any())
            {
                Collection<int> boeIds = elementsToProcess.Select(x => x.BoeID).Distinct().ToCollection();

                object LOCK = new object();

                Parallel.ForEach(boeIds, new ParallelOptions { MaxDegreeOfParallelism = 5 }, boeId =>
                {
                    BoeDiscrepancyReportModelView item = GenerateAnItem( elementsToProcess, boes, boeId, authorsForAllBoes, inconsistency, workspace);

                    lock (LOCK) // add it to the data..
                    {
                        modelViewsForReport.Add(item);
                    }
                });
            }
        }

        /// <summary>
        /// Generates an item based on the boeId
        /// </summary>
        /// <param name="elementsToProcess">Task Elements</param>
        /// <param name="boes">Boes for the WS</param>
        /// <param name="boeId">Boe Id</param>
        /// <param name="authorsForAllBoes">Boes Authors</param>
        /// <param name="inconsistencyType">Inconsistency Type</param>
        /// <returns>Data for the report</returns>
        private static BoeDiscrepancyReportModelView GenerateAnItem( HashSet<BoeTaskElementDTO> elementsToProcess, HashSet<FullBoe> boes,
            int boeId, HashSet<UserDTO> authorsForAllBoes, BoeInconsistencyEnum inconsistencyType, WorkspaceDTO workspace)
        {
            FullBoe boe = boes.First(x => x.Id == boeId);
            BoeDiscrepancyReportModelView item = ProcessBoeLevelData( boe, authorsForAllBoes);

            // Task Elements
            item.ElementsWithIssues = new List<BoeTaskDetailsMV>();
            foreach (BoeTaskElementDTO task in elementsToProcess.Where(x => x.BoeID == boe.Id).ToCollection())
            {
                item.ElementsWithIssues.Add(new BoeTaskDetailsMV()
                {
                    TaskId = task.Id,
                    DisplayedTaskId = task.BOETaskID,
                    TaskTitle = task.TaskTitle,
                    DiscrepancyEnum = inconsistencyType,
                    IsUsingEquivalentPerson = FullObjectHelper.ShowEquivalentPersonsOption && workspace.IsUsingEquivalentPerson
                });
            }


            return item;
        }

        /// <summary>
        /// Generates an item based on the boeId
        /// </summary>
        /// <param name="elementsToProcess">Task Elements</param>
        /// <param name="boes">Boes for the WS</param>
        /// <param name="boeId">Boe Id</param>
        /// <param name="authorsForAllBoes">Boes Authors</param>
        /// <param name="inconsistencyType">Inconsistency Type</param>
        /// <returns>Data for the report</returns>
        private static BoeDiscrepancyReportModelView GenerateAnItem( HashSet<OtherDirectCostDTO> elementsToProcess, HashSet<FullBoe> boes,
            int boeId, HashSet<UserDTO> authorsForAllBoes, BoeInconsistencyEnum inconsistencyType, WorkspaceDTO workspace)
        {
            FullBoe boe = boes.First(x => x.Id == boeId);
            BoeDiscrepancyReportModelView item = ProcessBoeLevelData( boe, authorsForAllBoes);

            // Task Elements
            item.ElementsWithIssues = new List<BoeTaskDetailsMV>();
            foreach (OtherDirectCostDTO element in elementsToProcess.Where(x => x.BoeID == boe.Id).ToCollection())
            {
                item.ElementsWithIssues.Add(new BoeTaskDetailsMV()
                {
                    TaskId = element.Id,
                    DisplayedTaskId = element.BOETaskID,
                    TaskTitle = element.TaskTitle,
                    DiscrepancyEnum = inconsistencyType,
                    IsUsingEquivalentPerson = FullObjectHelper.ShowEquivalentPersonsOption && workspace.IsUsingEquivalentPerson
                });
            }

            return item;
        }

        /// <summary>
        /// Processes all BOE data for the report item
        /// </summary>
        /// <param name="boe">Boe</param>
        /// <param name="authorsForAllBoes">Authors for all BOEs for the WS</param>
        /// <returns>Item w/ the BOE level data filled out</returns>
        private static BoeDiscrepancyReportModelView ProcessBoeLevelData(FullBoe boe, HashSet<UserDTO> authorsForAllBoes)
        {
            BoeDiscrepancyReportModelView result = new BoeDiscrepancyReportModelView();

            // Boe info
            result.BoeId = boe.Id;
            result.BoeTitle = boe.Title;

          
            result.Wbs = boe.Wbs != null ? boe.Wbs.WbsString : CommonConstants.Unassigned_WBS_Display_Text;


            result.Clin = boe.Clin != null ? boe.Clin.ClinString : CommonConstants.Unassigned_CLIN_Display_Text;

            // Authors
            List<UserDTO> authorsForThisBoe = authorsForAllBoes.Where(x => boe.AuthorIDs.Contains(x.UserID)).ToList();
            List<string> authorNames = authorsForThisBoe.Select(x => x.DisplayName).ToList();
            result.BoeAuthors = authorNames.Count > 0 ? string.Join("<br/>", authorNames.Select(x => x)) : string.Empty;

            return result;
        }
    }
}