// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using GenBOE.Objects;
    using IES.Common;

    public interface IWorkspaceStateTransition
    {
        /// <summary>
        /// Perform default behavior across any transition inheriting from this class
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        /// <param name="inTransitionFrom">the current workspace state</param>
        /// <param name="inTransitionTo">the state the workspace is changing to</param>
        void Action(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo);

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        /// <param name="outErrorMessage">The error message, if one occurs</param>
        /// <returns>true/false depending on the validation success</returns>
        bool Validate(FullWorkspace workspace, out string outErrorMessage);
    }
}
