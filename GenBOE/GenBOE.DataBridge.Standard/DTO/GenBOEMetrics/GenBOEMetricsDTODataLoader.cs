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
    using IES.Standard;
    using GenBOE.Models;
    using GenBOE.Dtos;
	using Microsoft.Extensions.Logging;

	public class GenBOEMetricsDTODataLoader : IGenBOEMetricsDataLoader
    {
        private readonly ILogger _log;

        /// <summary>
        /// Primary key for LineOfBusiness.LineOfBusinessName = 'None'
        /// </summary>
        private const int LINE_OF_BUSINESS_ID_NONE = 1;

        // default constructor
        public GenBOEMetricsDTODataLoader(ILogger<GenBOEMetricsDTODataLoader> logger)
		{
			this._log = logger;
		}

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>Users</returns>
        virtual public int GetUsersTotal()
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
        
        virtual public GenBOEMetricsDTO GetGenBOEMetrics()
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

                    var boeData = (from boes in gbe.BOEs
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
        virtual public void UpdateLastAccessTime(UserData userData)
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


        virtual public GenBOEUsersOnlineDTO GetOnlineUserDetails()
        {
            GenBOEUsersOnlineDTO toReturn = new GenBOEUsersOnlineDTO();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                ICollection<UserOnlineDetails> allUserDetails = new Collection<UserOnlineDetails>();

                // Upsert SP
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var users = (from allUsers in gbe.UserLogs
                                 select allUsers).ToCollection();
                    var ETIUsers = (from allETIUsers in gbe.ETIusers select allETIUsers).ToCollection();
                    
                    foreach (var user in users)
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
