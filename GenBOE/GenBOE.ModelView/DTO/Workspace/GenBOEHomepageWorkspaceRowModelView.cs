// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Web.Script.Serialization;
    using IES.Common;

    /// <summary>
    /// Model View for a Workspace Row in the Metrics Grid on Homepage
    /// </summary>
    public class GenBOEHomepageWorkspaceRowModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenBOEHomepageWorkspaceRowModelView"/> class.
        /// </summary>
        public GenBOEHomepageWorkspaceRowModelView()
        {
            WorkspaceId = -1;
            NumBOEsUnassigned = -1;
            NumBOEsInDraft = -1;
            NumBOEsInAwaitingApproval = -1;
            NumBOEsApproved = -1;
            CurrentUserHasAdmin = false;
            HasBeenDeleted = false;
        }

        /// <summary>
        /// Gets or sets the workspace identifier.
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the workspace.
        /// </summary>
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets or sets the short name of the workspace.
        /// </summary>
        public string WorkspaceShortName { get; set; }

        /// <summary>
        /// Gets or sets the tracking number.
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the proposal submittal date inside LINQ.
        /// </summary>
        [ScriptIgnore]
        public DateTime? sub { get; set; }

        /// <summary>
        /// Gets or sets the proposal submittal date.
        /// </summary>
        public string ProposalSubmittal { get { return sub.HasValue ? sub.Value.ToShortDateString() : string.Empty; } }

        [ScriptIgnore]
        public int st { get; set; }

        /// <summary>
        /// Gets or sets the state of the workspace.
        /// </summary>
        /// <value>
        /// The state of the workspace.
        /// </value>
        public string WorkspaceState { get { return ((WorkspaceState)st).GetDescription(); } }

        /// <summary>
        /// Gets or sets the proposal status inside LINQ.
        /// </summary>
        [ScriptIgnore]
        public int ps { get; set; }

        /// <summary>
        /// Gets or sets the proposal status.
        /// </summary>
        public string ProposalStatus { get { return ((ProposalStatus)ps).GetDescription(); } }

        /// <summary>
        /// Gets or sets the number boes unassigned.
        /// </summary>
        public int NumBOEsUnassigned { get; set; }

        /// <summary>
        /// Gets or sets the number boes in draft.
        /// </summary>
        public int NumBOEsInDraft { get; set; }

        /// <summary>
        /// Gets or sets the number boes in awaiting approval.
        /// </summary>
        public int NumBOEsInAwaitingApproval { get; set; }

        /// <summary>
        /// Gets or sets the number boes approved.
        /// </summary>
        public int NumBOEsApproved { get; set; }

        /// <summary>
        /// Gets or sets the cost volume lead pricer user identifier.
        /// </summary>
        public string CostVolumeLeadPricer { get; set; }

        /// <summary>
        /// Gets or sets the cost volume lead pricer nt identifier.
        /// </summary>
        public string CostVolumeLeadPricerNtId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has admin rights.
        /// </summary>
        public bool CurrentUserHasAdmin { get; set; }

        /// <summary>
        /// Gets or sets the line of business.
        /// </summary>
        public string LineOfBusiness { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has been deleted.
        /// </summary>
        public bool HasBeenDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is favorite.
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Gets or sets the Last Accessed inside LINQ.
        /// </summary>
        public DateTime? la { get; set; }

        /// <summary>
        /// Gets or sets the last accessed.
        /// </summary>
        public string LastAccessed { get { return la.HasValue ? la.Value.ToShortDateString() : string.Empty; } }

        /// <summary>
        /// Gets or sets the Update Date inside LINQ.
        /// </summary>
        [ScriptIgnore]
        public DateTime up { get; set; }

        /// <summary>
        /// Gets or sets the update dt.
        /// </summary>
        public long updateDT
        {
            get { return up.Ticks; }
            set { up = new DateTime(value); }
        }
    }
}