// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOETransitions
{
    using System;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Perform state transition actions depending on what transition being taken.
    /// </summary>
    public class BOEStateMachine : IBOEStateMachine
    {
        private Logger _log = new Logger(typeof(BOEStateMachine));

        private IBOEStateTransition _NoneToUnassigned { get; set; }
        private IBOEStateTransition _UnassignedToDraft { get; set; }
        private IBOEStateTransition _DraftToAwaitingApproval { get; set; }
        private IBOEStateTransition _AwaitingApprovalToDraft { get; set; }

        private IBOEStateTransition _DraftLockedToAwaitingApproval { get; set; }
        private IBOEStateTransition _DraftLockedToDraft { get; set; }
        private IBOEStateTransition _DraftToDraftLocked { get; set; }
        private IBOEStateTransition _AwaitingApprovalToApproved { get; set; }
        private IBOEStateTransition _ApprovedToDraft { get; set; }
        private IBoeDTODataLoader _BoeDTODataLoader { get; set; }

        /// <summary>
        /// DI constructor
        /// </summary>
        public BOEStateMachine(IBOEStateTransition inNoneToUnassigned, IBOEStateTransition inUnassignedToDraft,
            IBOEStateTransition inDraftToAwaitingApproval, IBOEStateTransition inAwaitingApprovalToDraft,
            IBOEStateTransition inDraftLockedToAwaitingApproval,
            IBOEStateTransition inDraftLockedToDraft, IBOEStateTransition inDraftToDraftLocked,
            IBOEStateTransition inAwaitingApprovalToApproved, IBOEStateTransition inApprovedToDraft,
            IBoeDTODataLoader inBoeDTODataLoader)
        {
            this._NoneToUnassigned = inNoneToUnassigned;
            this._UnassignedToDraft = inUnassignedToDraft;
            this._DraftToAwaitingApproval = inDraftToAwaitingApproval;
            this._AwaitingApprovalToDraft = inAwaitingApprovalToDraft;
            this._DraftLockedToAwaitingApproval = inDraftLockedToAwaitingApproval;
            this._DraftLockedToDraft = inDraftLockedToDraft;
            this._DraftToDraftLocked = inDraftToDraftLocked;
            this._AwaitingApprovalToApproved = inAwaitingApprovalToApproved;
            this._ApprovedToDraft = inApprovedToDraft;
            this._BoeDTODataLoader = inBoeDTODataLoader;

            // Set properties so some transitions can kickoff other transitions
            this._AwaitingApprovalToDraft.BoeStateMachine = this;
            this._ApprovedToDraft.BoeStateMachine = this;
            this._AwaitingApprovalToDraft.BoeDTODataLoader = this._BoeDTODataLoader;
            this._ApprovedToDraft.BoeDTODataLoader = this._BoeDTODataLoader;
           
        }

        public BOEStateMachine() { }

        /// <summary>
        /// Perform the appropriate action(s) for the state transition.
        /// Throw InvalidOperationException if transition is not valid.
        /// </summary>
        /// <param name="boe">boe to perform state transition on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">The state transitioning from</param>
        /// <param name="inTransitionTo">The state transitioning to</param>
        public void PerformStateTransitionAction(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            this._log.Debug("Looking for boe transition from " + inTransitionFrom + " to " + inTransitionTo);

            IBOEStateTransition transition;

            if (inTransitionTo == BOEState.DateShiftDraft)
            {
                transition = this.BOETransitionFactory(inTransitionFrom, BOEState.Draft);
            }
            else
            {
                transition = this.BOETransitionFactory(inTransitionFrom, inTransitionTo);
            }

            // Throw Away
            string validationMessage = string.Empty;

            if (transition != null &&
                transition.Validate(boe, workspace, out validationMessage))
            {
                transition.Action(boe, workspace, inTransitionFrom, inTransitionTo);
            }
        }

        /// <summary>
        /// Perform the appropriate action(s) for the state transition.
        /// Throw InvalidOperationException if transition is not valid.
        /// </summary>
        /// <param name="boe">boe to perform state transition on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">The state transitioning from</param>
        /// <param name="inTransitionTo">The state transitioning to</param> 
        /// <param name="outErrorMessage">The error message, if one occurs</param>
        public Boolean PerformStateTransitionValidation(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo, out string outErrorMessage)
        {
            this._log.Debug("Looking for workspace transition from " + inTransitionFrom + " to " + inTransitionTo);

            outErrorMessage = string.Empty;

            IBOEStateTransition transition = this.BOETransitionFactory(inTransitionFrom, inTransitionTo);

            //if transition is null then there is no transition taking place. Else see if it is also valid
            if (transition == null ||
                transition.Validate(boe, workspace, out outErrorMessage))
            {
                return true;
            }

            return false;
        }

        //TODO this should be its own class, leaving it as a private method so we don't have to rewrite test right now.
        // 31753
        /// <summary>
        /// Gives you back an instance of a Transition
        /// </summary>
        /// <param name="inTransitionFrom"></param>
        /// <param name="inTransitionTo"></param>
        /// <returns>The Transition</returns>
        private IBOEStateTransition BOETransitionFactory(BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            IBOEStateTransition transition = null;

            if (inTransitionTo == inTransitionFrom)
            {
                // no transition ... nothing to do
            }
            else if (inTransitionFrom == BOEState.None && inTransitionTo == BOEState.Unassigned)
            {
                transition = this._NoneToUnassigned;
            }
            else if (inTransitionFrom == BOEState.Unassigned && inTransitionTo == BOEState.Draft)
            {
                transition = this._UnassignedToDraft;
            }
            else if (inTransitionFrom == BOEState.Draft && inTransitionTo == BOEState.AwaitingApproval)
            {
                transition = this._DraftToAwaitingApproval;
            }
            else if (inTransitionFrom == BOEState.AwaitingApproval && inTransitionTo == BOEState.Draft)
            {
                transition = this._AwaitingApprovalToDraft;
            }
            else if (inTransitionFrom == BOEState.DraftLocked && inTransitionTo == BOEState.AwaitingApproval)
            {
                transition = this._DraftLockedToAwaitingApproval;
            }
            else if (inTransitionFrom == BOEState.DraftLocked && inTransitionTo == BOEState.Draft)
            {
                transition = this._DraftLockedToDraft;
            }
            else if (inTransitionFrom == BOEState.Draft && inTransitionTo == BOEState.DraftLocked)
            {
                transition = this._DraftToDraftLocked;
            }
            else if (inTransitionFrom == BOEState.Approved && inTransitionTo == BOEState.Draft)
            {
                transition = this._ApprovedToDraft;
            }
            else if (inTransitionFrom == BOEState.AwaitingApproval && inTransitionTo == BOEState.Approved)
            {
                transition = this._AwaitingApprovalToApproved;
            }
            else
            {
                this._InvalidTransition(inTransitionFrom, inTransitionTo);
            }

            return transition;
        }

        /// <summary>
        /// Invalid transition ... throw exception
        /// </summary>
        /// <param name="inFrom">the state transitioned from</param>
        /// <param name="inTo">the state transitioned to</param>
        private void _InvalidTransition(BOEState inFrom, BOEState inTo)
        {
            throw new InvalidOperationException("Unable to transition from " + inFrom.ToString() + " to " + inTo.ToString());
        }
    }
}
