// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using System;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public class LockedToClosedTransition : DefaultWorkspaceTransition
    {

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        public LockedToClosedTransition(IBoeEmailer inEmailer, IWorkspaceDTODataLoader workspaceLoader) :
            base(inEmailer, workspaceLoader)
        {
        }


        public override void Action(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Check to ensure that rates are locked.  If not, then lock them.
            if (!this.WorkspaceLoader.LockedDataExists(workspace.Id))
            {
                this.WorkspaceLoader.LockTravelAndResourceRatesForWorkspace(workspace.Id);
            }

            base.Action(workspace, inTransitionFrom, inTransitionTo);
        }

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        /// <returns>true/false depending on the validation success</returns>
        public override bool Validate(FullWorkspace workspace, out string outErrorMessage)
        {
            bool toReturn = base.Validate(workspace, out outErrorMessage);

            return toReturn;
        }
    }
}
