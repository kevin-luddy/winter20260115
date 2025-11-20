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
    using IES.Common;
    using GenBOE.Models;
    using GenBOE.Dtos;

    public class GenBOEMetricsDTODataLoader : GenBOE.DataBridge.DTO.IGenBOEMetricsDataLoader
    {
        private Logger _log = new Logger(typeof(GenBOEMetricsDTODataLoader));

        // default constructor
        public GenBOEMetricsDTODataLoader( )
        {
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>Users</returns>
        public virtual int GetUsersTotal()
        {
            int result = -1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                result = (from b in gbe.WorkspaceUserRoles select b.ETIUserID).Union(
                    from c in gbe.BOEUserRoles select c.ETIUserID).Union(
                    from d in gbe.BOEPotentialRoles select d.ETIUserID).Distinct().Count();
            }

            return result;
        }

        /// <summary>
        /// Gets GenBoe Metrics Data
        /// </summary>
        /// <returns>Data for the Boe Metrics Table</returns>
        [DbQuery(2)]
        public virtual GenBOEMetricsDTO GetGenBOEMetrics()
        {
            GenBOEMetricsDTO toReturn = new GenBOEMetricsDTO();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var wsData = (from ws in gbe.Workspaces
                                  where ws.IsDeleted != true
                                  select new { ws.WorkspaceID, ws.WorkspaceStateID, ws.LineOfBusinessID }).ToCollection();

                    Collection<int> validWorkspaceIds = wsData.Select(x => x.WorkspaceID).ToCollection();

					Collection<int> boeData = (from boes in gbe.BOEs
                                   where validWorkspaceIds.Contains(boes.WorkspaceID)
                                   select boes.BOEStateID).ToCollection();

                    toReturn.WorkspacesAll = wsData.Count();
                    toReturn.WorkspacesClosed = wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Closed).Count();
                    toReturn.WorkspacesComplete = wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Complete).Count();
                    toReturn.WorkspacesInitialization = wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Initialization).Count();
                    toReturn.WorkspacesLocked = wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Locked).Count();
                    toReturn.WorkspacesWorking = wsData.Where(x => x.WorkspaceStateID == (int)WorkspaceState.Working).Count();

                    toReturn.BoesAll = boeData.Count();
                    toReturn.BoesApproved = boeData.Where(x => x == (int)BOEState.Approved).Count();
                    toReturn.BoesAwaitingApproval = boeData.Where(x => x == (int)BOEState.AwaitingApproval).Count();
                    toReturn.BoesDraft = boeData.Where(x => x == (int)BOEState.Draft).Count() + boeData.Where(x => x == (int)BOEState.DraftLocked).Count();
                    toReturn.BoesUnassigned = boeData.Where(x => x == (int)BOEState.Unassigned).Count();

                    toReturn.usersTotal = this.GetUsersTotal();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Updates the database with the latest access time of the current user.
        /// </summary>
        public virtual void UpdateLastAccessTime(UserData userData)
        {
            if (userData == null)
            {
                throw new ArgumentNullException(nameof(userData), "userData cannot be null.");
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {

                // Upsert SP
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.upsertUserLog(userData.Ntid);
                } // end using gbe
            }
        }


        public virtual GenBOEUsersOnlineDTO GetOnlineUserDetails()
        {
            GenBOEUsersOnlineDTO toReturn = new GenBOEUsersOnlineDTO();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                ICollection<UserOnlineDetails> allUserDetails = new Collection<UserOnlineDetails>();

                // Upsert SP
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
					Collection<UserLog> users = (from allUsers in gbe.UserLogs
                                 select allUsers).ToCollection();
					Collection<ETIuser> ETIUsers = (from allETIUsers in gbe.ETIusers select allETIUsers).ToCollection();
                    
                    foreach (UserLog user in users)
                    {
                        UserOnlineDetails userDetails = new UserOnlineDetails();
                        userDetails.TimeLastAccessed = user.LogInUpdateDT;
                        userDetails.Ntid = user.NTID;
                        userDetails.DisplayName = ETIUsers.Where(x => x.NTID == user.NTID).Select(x => x.DisplayName).FirstOrDefault();
                        allUserDetails.Add(userDetails);
                    }

                    toReturn.UserOnlineDetailsCollection = allUserDetails;

                } // end using gbe
            }

            return toReturn;

        }
    }
}
