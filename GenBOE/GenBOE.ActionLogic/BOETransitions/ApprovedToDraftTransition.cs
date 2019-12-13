// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOETransitions
{
    using System;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public class ApprovedToDraftTransition : DefaultBOETransition
    {
        /// <summary>
        /// Workspace loader
        /// </summary>
        private IWorkspaceDTODataLoader _WorkspaceLoader;

        /// <summary>
        /// constructor for dependency injection
        /// </summary>
        /// <param name="inEmailer">the emailer to use</param>
        /// <param name="inBoeApproverLoader">BOE approval data tier</param>
        /// <param name="workspaceLoader">Workspace loader</param>
        /// constructor for dependency injection
        public ApprovedToDraftTransition(IBoeEmailer inEmailer, IBoeApproverResponseDTODataLoader inBoeApproverLoader, IWorkspaceDTODataLoader workspaceLoader)
            : base(inEmailer, inBoeApproverLoader)
        {
            this._WorkspaceLoader = workspaceLoader;
        }

        /// <summary>
        /// Action to take when transition from Approved state to Draft state.
        /// </summary>
        /// <param name="boe">boe to take action on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">from state</param>
        /// <param name="inTransitionTo">to state</param>
        public override void Action(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            this.ClearLockedRates(workspace);

            base.Action(boe, workspace, inTransitionFrom, inTransitionTo);

            this.ResetAllBOEApprovals(boe);

            this.SendEmail1(boe, workspace);
        }

        /// <summary>
        /// As is done for workspaces, when a BOE is "unlocked" for edit, any locked rates (that were stored when the workspace became
        /// locked) need to be cleared-out (across the entire workspace).
        /// </summary>
        /// <param name="workspace">workspace</param>
        private void ClearLockedRates(FullWorkspace workspace)
        {
            this._WorkspaceLoader.RestoreTravelForWorkspace(workspace.Id);
        }

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// NOTE: Not logic currently present, throws NotImplementedException
        /// </summary>
        /// <param name="boe">boe to validate</param>
        /// <param name="workspace">workspace</param>
        /// <param name="outErrorMessage">The error message, if one occurs</param>
        /// <returns>true/false depending on the validation success</returns>
        public override bool Validate(FullBoe boe, FullWorkspace workspace, out string outErrorMessage)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            bool toReturn = base.Validate(boe, workspace, out outErrorMessage);

            // BOE can only go from Approved to Draft if the Workspace is in Working or Locked state
            if (toReturn)
            {
               if (workspace.WorkspaceState == WorkspaceState.Working || workspace.WorkspaceState == WorkspaceState.Locked)
               {
                   toReturn = true;
               }
               else
               {
                   toReturn = false;

                   outErrorMessage = "Workspace must be in Working or Locked state, your workspace is in the " +
                                     workspace.WorkspaceStateName + " state.  Unable to perform action";
               }
            }

            return toReturn;

        }
    }
}
