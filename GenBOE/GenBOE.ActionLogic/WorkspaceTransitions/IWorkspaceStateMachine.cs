// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using System;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Class which performs logic for workspace state transitions
    /// </summary>
    public interface IWorkspaceStateMachine
    {
        /// <summary>
        /// Execute the logic for a workspace state change
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        /// <param name="inTransitionFrom">the state the workspace is in</param>
        /// <param name="inTransitionTo">the state the workspace is transitioning to</param>
        void PerformStateTransitionAction(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo);
        
        /// <summary>
        /// Perform the appropriate action(s) for the state transition.
        /// Throw InvalidOperationException if transition is not valid.
        /// </summary>
        /// <param name="inWorkspaceId">The workspace we are working on</param>
        /// <param name="inTransitionFrom">The state transitioning from</param>
        /// <param name="inTransitionTo">The state transitioning to</param> 
        Boolean PerformStateTransitionValidation(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo, out string message);
    }
}
