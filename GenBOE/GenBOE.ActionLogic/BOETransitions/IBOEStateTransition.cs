// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOETransitions
{
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public interface IBOEStateTransition
    {
        /// <summary>
        /// Gets or sets the Boe State Machine.
        /// </summary>
        IBOEStateMachine BoeStateMachine { get; set; }

        /// <summary>
        /// Gets or sets the boe data loader.
        /// </summary>
        IBoeDTODataLoader BoeDTODataLoader { get; set; }

        /// <summary>
        /// Perform default behavior across any transition inheriting from this class
        /// </summary>
        /// <param name="boe">the boe</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">the current boe state</param>
        /// <param name="inTransitionTo">the state the boe is changing to</param>
        void Action(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo);

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// </summary>
        /// <param name="boe">the boe</param>
        /// <param name="workspace">workspace</param>
        /// <param name="outErrorMessage">The error message, if one occurs</param>
        /// <returns>true/false depending on the validation success</returns>
        bool Validate(FullBoe boe, FullWorkspace workspace, out string outErrorMessage);
    }
}
