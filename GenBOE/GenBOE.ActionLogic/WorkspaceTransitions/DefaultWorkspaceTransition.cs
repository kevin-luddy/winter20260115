// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Default empty implementation of a WorkspaceTransition, sends an email to the WSAdmin
    /// that a state change occurred (for ANY state change)
    /// </summary>
    public class DefaultWorkspaceTransition : IWorkspaceStateTransition
    {
        protected IBoeEmailer _Emailer { get; set; }
        protected IWorkspaceDTODataLoader WorkspaceLoader { get; set; }

        /// <summary>
        /// constructor for dependency injection
        /// </summary>
        public DefaultWorkspaceTransition(IBoeEmailer inEmailer, IWorkspaceDTODataLoader workspaceLoader)
        {
            this._Emailer = inEmailer;
            this.WorkspaceLoader = workspaceLoader;
        }

        //TODO consider making this an abstract method that only accepts workspaceid, each transistion will know thier from and to states.
        // WI 31618
        /// <summary>
        /// Perform default behavior across any transition inheriting from this class.
        /// </summary>
        /// <param name="workspace">the full workspace</param>
        /// <param name="inTransitionFrom">the current workspace state</param>
        /// <param name="inTransitionTo">the state the workspace is changing to</param>
        public virtual void Action(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo)
        {
        }

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        /// <returns>true/false depending on the validation success</returns>
        public virtual bool Validate(FullWorkspace workspace, out string outErrorMessage)
        {
            outErrorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Send an email to authors and approvers depending on BOE states
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        protected virtual void SendEmail1(FullWorkspace workspace)
        {
            this._Emailer.SendWorkspaceAuthorsEmailOpenedForEdit(workspace);
        }

    }
}
