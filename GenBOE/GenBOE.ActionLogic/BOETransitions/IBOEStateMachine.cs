// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.BOETransitions
{
    using System;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Class which performs logic for boe state transitions
    /// </summary>
    public interface IBOEStateMachine
    {
        /// <summary>
        /// Execute the logic for a boe state change
        /// </summary>
        /// <param name="boe">boe to perform state transition on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">the state the boe is in</param>
        /// <param name="inTransitionTo">the state the boe is transitioning to</param>
        void PerformStateTransitionAction(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo);

        /// <summary>
        /// Perform the appropriate action(s) for the state transition.
        /// Throw InvalidOperationException if transition is not valid.
        /// </summary>
        /// <param name="boe">The BOE we are working on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">The state transitioning from</param>
        /// <param name="inTransitionTo">The state transitioning to</param> 
        /// <param name="outErrorMessage">The error message, if one occurs</param>
        /// <returns>true/false depending on if validation passed</returns>
        Boolean PerformStateTransitionValidation(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo, out string outErrorMessage);
    }
}
