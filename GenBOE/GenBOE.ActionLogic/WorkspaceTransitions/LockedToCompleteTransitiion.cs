// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using System;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public class LockedToCompleteTransition : DefaultWorkspaceTransition
    {
        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        public LockedToCompleteTransition(IBoeEmailer inEmailer, IWorkspaceDTODataLoader workspaceLoader) :
            base(inEmailer, workspaceLoader)
        {
        }


        public override void Action(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            base.Action(workspace, inTransitionFrom, inTransitionTo);
        }

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// Validates that all BOEs are in the Approved State
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        /// <returns>true/false depending on the validation success</returns>
        public override bool Validate(FullWorkspace workspace, out string outErrorMessage)
        {
            if (workspace == null)
        {
                throw new ArgumentNullException(nameof(workspace));
            }

            bool toReturn = false;

            toReturn = base.Validate(workspace, out outErrorMessage);

            if (toReturn)
            {
                // check to see if we have any boe's in DRAFT state
                toReturn = workspace.Boes.Count(x => x.State == BOEState.Approved) == workspace.Boes.Count;

                if (!toReturn)
                {
                    outErrorMessage = "All BOEs must be in the Approved state.";
                }
            }

            return toReturn;
        }
    }
}
