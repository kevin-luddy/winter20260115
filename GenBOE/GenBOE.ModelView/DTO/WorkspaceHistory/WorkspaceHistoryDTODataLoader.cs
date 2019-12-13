// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

//DTO that contains BOE History data
namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;

    public class WorkspaceHistoryDTODataLoader : GenBOE.DataBridge.DTO.IWorkspaceHistoryDTODataLoader
    {
        private Logger _log = new Logger(typeof(WorkspaceHistoryDTODataLoader));

        // default constructor
        public WorkspaceHistoryDTODataLoader() { }

        #region Retrieves

        /// <summary>
        /// Get the Workspace History DTO for a given Workspace ID
        /// </summary>
        /// <param name="inBoeID">Workspace ID</param>
        /// <returns>all the Workspace History logs</returns>
        [DbQuery]
        virtual public Collection<WorkspaceHistoryDTO> GetWorkspaceHistory(int inWorkspaceID)
        {
            Collection<WorkspaceHistoryDTO> toReturn = new Collection<WorkspaceHistoryDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    IEnumerable<WorkspaceHistoryDTO> resultsLinq = from b in gbe.WorkspaceStateHistories
                                                                   where b.WorkspaceID == inWorkspaceID
                                                                   select new WorkspaceHistoryDTO
                                                                   {
                                                                       OldValue = (WorkspaceState)b.CurrentWorkspaceStateID,
                                                                       NewValue = (WorkspaceState)b.UpdatedWorkspaceStateID,
                                                                       PerformedByETIUserId = b.ChangedByETIUserID,
                                                                       Date = b.UpdateDT,
                                                                       WorkspaceID = b.WorkspaceID
                                                                   };

                    toReturn = new Collection<WorkspaceHistoryDTO>(resultsLinq.ToArray());
                }
            }

            return toReturn;
        }

        #endregion Retrieves

    }
}
