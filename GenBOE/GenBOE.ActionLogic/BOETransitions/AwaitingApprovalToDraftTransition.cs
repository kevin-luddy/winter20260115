// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOETransitions
{
    using System;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public class AwaitingApprovalToDraftTransition : DefaultBOETransition
    {
        /// <summary>
        /// Common data mapper
        /// </summary>
        private ICommonDataMapper _CommonDataMapper;

        /// <summary>
        /// Workspace loader
        /// </summary>
        private IWorkspaceDTODataLoader _WorkspaceLoader;

        /// <summary>
        /// constructor for dependency injection
        /// </summary>
        /// <param name="inEmailer">the emailer to use</param>
        /// <param name="inCommonDataMapper">Common data mapper</param>
        /// <param name="inboeApproverLoader">Boe Approver response loader</param>
        /// <param name="workspaceLoader">Workspace loader</param>
        /// constructor for dependency injection
        public AwaitingApprovalToDraftTransition(IBoeEmailer inEmailer, ICommonDataMapper inCommonDataMapper, IBoeApproverResponseDTODataLoader inboeApproverLoader, IWorkspaceDTODataLoader workspaceLoader)
            : base(inEmailer, inboeApproverLoader)
        {
            this._CommonDataMapper = inCommonDataMapper;
            this._WorkspaceLoader = workspaceLoader;
        }

        /// <summary>
        /// Action to take when transition from AwaitingApproval to Draft state.
        /// </summary>
        /// <param name="boe">boe to take action on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">from state</param>
        /// <param name="inTransitionTo">to state</param>
        public override void Action(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            bool sendEmail = true;

            if (inTransitionTo == BOEState.DateShiftDraft)
            {
                sendEmail = false;
                inTransitionTo = BOEState.Draft;
            }

            this.ClearLockedRates(workspace);

            base.Action(boe, workspace, inTransitionFrom, inTransitionTo);

            this.ResetAllBOEApprovals(boe);

            //  Don't want to send email here if the transition is caused by a Date Shift aka BOEState.DateShiftDraft.
            //  Also, The Clin is loaded here when a date shift occurs and it is not grabbing
            //  the correct date since the clin date change has not yet been committed to the database.
            if (boe.State == BOEState.Draft && sendEmail)
            {
                this._Emailer.SendBOEAuthorsEmailOpenedForEdit(boe, workspace);
            }
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
            else if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            bool toReturn = base.Validate(boe, workspace, out outErrorMessage);

            // BOE can only go from Awaiting-Approval to Draft if the Workspace is in Working or Locked state
            if (toReturn)
            {
                if (workspace.WorkspaceState == WorkspaceState.Working || workspace.WorkspaceState == WorkspaceState.Locked)
                {
                    toReturn = true;
                }
                else
                {
                    toReturn = false;

                    outErrorMessage = "Workspace must be in Working or Locked state, your workspace is in the " + this._CommonDataMapper.getWorkspaceStateName(workspace.WorkspaceState) + " state.  Unable to perform action";
                }
            }

            return toReturn;
        }
    }
}
