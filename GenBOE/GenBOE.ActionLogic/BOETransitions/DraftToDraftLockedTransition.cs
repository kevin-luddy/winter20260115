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

    public class DraftToDraftLockedTransition : DefaultBOETransition
    {
        /// <summary>
        /// Workspace loader
        /// </summary>
        private IWorkspaceDTODataLoader _WorkspaceLoader;

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        /// <param name="inEmailer">Email features</param>
        /// <param name="inBoeApproverLoader">BOE approval data tier</param>
        /// <param name="workspaceLoader">Workspace loader</param>
        public DraftToDraftLockedTransition(IBoeEmailer inEmailer, IBoeApproverResponseDTODataLoader inBoeApproverLoader, IWorkspaceDTODataLoader workspaceLoader)
            : base(inEmailer, inBoeApproverLoader)
        {
            this._WorkspaceLoader = workspaceLoader;
        }

        /// <summary>
        /// Transition from Draft state to DraftLocked state.
        /// </summary>
        /// <param name="boe">boe to take action on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">from state</param>
        /// <param name="inTransitionTo">to state</param>
        public override void Action(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            base.Action(boe, workspace, inTransitionFrom, inTransitionTo);

            this.StoreLockedRates(boe, workspace);
        }

        /// <summary>
        /// As we do for workspaces, when a BOE is "locked" for edit, we need to store a copy of the current rates.
        /// Rates should not be locked-down unless/until ALL of the BOEs have been re-locked.
        /// </summary>
        /// <param name="boe">boe to take action on</param>
        /// <param name="workspace">workspace</param>
        private void StoreLockedRates(FullBoe boe, FullWorkspace workspace)
        {
            int? lastLockedBoeID = this._WorkspaceLoader.GetLastLockedBOEID(workspace.Id);

            if (lastLockedBoeID.HasValue && lastLockedBoeID.Value == boe.Id)
            {
                // Only trigger the rate lockdown if THIS BOE is the one that was the last one locked (to avoid duplicate and/or concurrent lockdowns)
                this._WorkspaceLoader.LockTravelAndResourceRatesForWorkspace(workspace.Id);
            }
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

            return base.Validate(boe, workspace, out outErrorMessage);
        }
    }
}
