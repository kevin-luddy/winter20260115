// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.ModelView
{
    public class UsageMetricsModelView
    {
        /// <summary>
        /// Number of All Workspaces
        /// </summary>
        public int WorkspacesAll { get; set; }

        /// <summary>
        /// Number of Workspaces in Initialization State
        /// </summary>
        public int WorkspacesInitialization { get; set; }

        /// <summary>
        /// Number of Workspaces in Working State
        /// </summary>
        public int WorkspacesWorking { get; set; }

        /// <summary>
        /// Number of Workspaces in Locked State
        /// </summary>
        public int WorkspacesLocked { get; set; }

        /// <summary>
        /// Number of Workspaces in Complete State
        /// </summary>
        public int WorkspacesComplete { get; set; }

        /// <summary>
        /// Number of Workspaces in Closed State
        /// </summary>
        public int WorkspacesClosed { get; set; }
        
        /// <summary>
        /// Number of All Boes
        /// </summary>
        public int BoesAll { get; set; }

        /// <summary>
        /// Number of Unassigned Boes
        /// </summary>
        public int BoesUnassigned { get; set; }

        /// <summary>
        /// Number of Boes in Draft
        /// </summary>
        public int BoesDraft { get; set; }

        /// <summary>
        /// Number of Boes Awaiting Approval
        /// </summary>
        public int BoesAwaitingApproval { get; set; }

        /// <summary>
        /// Number of Boes Approved
        /// </summary>
        public int BoesApproved { get; set; }
    }
}