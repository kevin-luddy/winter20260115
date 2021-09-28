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
    /// Perform state transition actions depending on what transition being taken.
    /// </summary>
    public class WorkspaceStateMachine : IWorkspaceStateMachine
    {
        private Logger _log = new Logger(typeof(WorkspaceStateMachine));

        private IWorkspaceStateTransition _NoneToInitialization { get; set; }
        private IWorkspaceStateTransition _InitializationToWorking { get; set; }
        private IWorkspaceStateTransition _WorkingToInitialization { get; set; }
        private IWorkspaceStateTransition _InitializationToClosed { get; set; }
        private IWorkspaceStateTransition _ClosedToInitialization { get; set; }
        private IWorkspaceStateTransition _WorkingToLocked { get; set; }
        private IWorkspaceStateTransition _LockedToWorking { get; set; }
        private IWorkspaceStateTransition _WorkingToClosed { get; set; }
        private IWorkspaceStateTransition _LockedToComplete { get; set; }
        private IWorkspaceStateTransition _CompleteToLocked { get; set; }
        private IWorkspaceStateTransition _LockedToClosed { get; set; }
        private IWorkspaceStateTransition _CompleteToWorking { get; set; }

        /// <summary>
        /// DI constructor
        /// </summary>
        public WorkspaceStateMachine(IWorkspaceStateTransition inNoneToInitialization,
                                     IWorkspaceStateTransition inInitializationToWorking, IWorkspaceStateTransition inWorkingToInitialization,
                                     IWorkspaceStateTransition inInitializationToClosed, IWorkspaceStateTransition inClosedToInitialization,
                                     IWorkspaceStateTransition inWorkingToLocked, IWorkspaceStateTransition inLockedToWorking,
                                     IWorkspaceStateTransition inWorkingToClosed, IWorkspaceStateTransition inLockedToComplete,
                                     IWorkspaceStateTransition inCompleteToLocked, IWorkspaceStateTransition inLockedToClosed,
                                     IWorkspaceStateTransition inCompleteToWorking)
        {
            this._NoneToInitialization = inNoneToInitialization;
            this._InitializationToWorking = inInitializationToWorking;
            this._WorkingToInitialization = inWorkingToInitialization;
            this._InitializationToClosed = inInitializationToClosed;
            this._ClosedToInitialization = inClosedToInitialization;
            this._WorkingToLocked = inWorkingToLocked;
            this._LockedToWorking = inLockedToWorking;
            this._WorkingToClosed = inWorkingToClosed;
            this._LockedToComplete = inLockedToComplete;
            this._CompleteToLocked = inCompleteToLocked;
            this._LockedToClosed = inLockedToClosed;
            this._CompleteToWorking = inCompleteToWorking;
        }

        /// <summary>
        /// Perform the appropriate action(s) for the state transition.
        /// Throw InvalidOperationException if transition is not valid.
        /// </summary>
        /// <param name="inWorkspaceId">The workspace we are working on</param>
        /// <param name="inTransitionFrom">The state transitioning from</param>
        /// <param name="inTransitionTo">The state transitioning to</param>
        public void PerformStateTransitionAction(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo)
        {
            this._log.Debug("Looking for workspace transition from " + inTransitionFrom + " to " + inTransitionTo);

            IWorkspaceStateTransition transition = this.WorkspaceTransitionFactory(inTransitionFrom, inTransitionTo);
            // Throw Away
            string validationMessage = string.Empty;

            if (transition != null &&
                transition.Validate(workspace, out validationMessage))
            {
                transition.Action(workspace, inTransitionFrom, inTransitionTo);
            }
        }

        /// <summary>
        /// Perform the appropriate action(s) for the state transition.
        /// Throw InvalidOperationException if transition is not valid.
        /// </summary>
        /// <param name="inWorkspaceId">The workspace we are working on</param>
        /// <param name="inTransitionFrom">The state transitioning from</param>
        /// <param name="inTransitionTo">The state transitioning to</param> 
        public Boolean PerformStateTransitionValidation(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo, out string message)
        {
            this._log.Debug("Looking for workspace transition from " + inTransitionFrom + " to " + inTransitionTo);

            message = string.Empty;

            IWorkspaceStateTransition transition = this.WorkspaceTransitionFactory(inTransitionFrom, inTransitionTo);

            //if transition is null then there is no transition taking place. Else see if it is also valid
            if (transition == null ||
                transition.Validate(workspace, out message))
            {
                return true;
            }

            return false;
        }

        //TODO this should be its own class, leaving it as a private method so we don't have to rewrite test right now.
        /// <summary>
        /// Gives you back an instance of a Transition
        /// </summary>
        /// <param name="inTransitionFrom"></param>
        /// <param name="inTransitionTo"></param>
        /// <returns>The Transistion</returns>
        private IWorkspaceStateTransition WorkspaceTransitionFactory(WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo)
        {
            IWorkspaceStateTransition transition=null;

            if (inTransitionTo == inTransitionFrom)
            {
                // no transition ... nothing to do
            }
            else if (inTransitionFrom == WorkspaceState.None && inTransitionTo == WorkspaceState.Initialization)
            {
                transition = this._NoneToInitialization;
            }
            else if (inTransitionFrom == WorkspaceState.Initialization && inTransitionTo == WorkspaceState.Working)
            {
                transition = this._InitializationToWorking;
            }
            else if (inTransitionFrom == WorkspaceState.Working && inTransitionTo == WorkspaceState.Initialization)
            {
                transition = this._WorkingToInitialization;
            }
            else if (inTransitionFrom == WorkspaceState.Initialization && inTransitionTo == WorkspaceState.Closed)
            {
                transition = this._InitializationToClosed;
            }
            else if (inTransitionFrom == WorkspaceState.Closed && inTransitionTo == WorkspaceState.Initialization)
            {
                transition = this._ClosedToInitialization;
            }
            else if (inTransitionFrom == WorkspaceState.Working && inTransitionTo == WorkspaceState.Locked)
            {
                transition = this._WorkingToLocked;
            }
            else if (inTransitionFrom == WorkspaceState.Locked && inTransitionTo == WorkspaceState.Working)
            {
                transition = this._LockedToWorking;
            }
            else if (inTransitionFrom == WorkspaceState.Working && inTransitionTo == WorkspaceState.Closed)
            {
                transition = this._WorkingToClosed;
            }
            else if (inTransitionFrom == WorkspaceState.Locked && inTransitionTo == WorkspaceState.Complete)
            {
                transition = this._LockedToComplete;
            }
            else if (inTransitionFrom == WorkspaceState.Complete && inTransitionTo == WorkspaceState.Locked)
            {
                transition = this._CompleteToLocked;
            }
            else if (inTransitionFrom == WorkspaceState.Locked && inTransitionTo == WorkspaceState.Closed)
            {
                transition = this._LockedToClosed;
            }
            else if (inTransitionFrom == WorkspaceState.Complete && inTransitionTo == WorkspaceState.Working)
            {
                transition = this._CompleteToWorking;
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
        private void _InvalidTransition(WorkspaceState inFrom, WorkspaceState inTo)
        {
            throw new InvalidOperationException("Unable to transition from " + inFrom.ToString() + " to " + inTo.ToString());
        }
    }
}
