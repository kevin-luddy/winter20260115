// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using System;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public class ClosedToInitializationTransition : DefaultWorkspaceTransition
    {
        private Logger _log = new Logger(typeof(ClosedToInitializationTransition));

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        public ClosedToInitializationTransition(IBoeEmailer inEmailer, IWorkspaceDTODataLoader workspaceLoader) :
            base(inEmailer, workspaceLoader)
        {
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void Action(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // restore each BOEs travel rates to the system's current rates
            this.WorkspaceLoader.RestoreTravelForWorkspace(workspace.Id);

            try
            {
                base.Action(workspace, inTransitionFrom, inTransitionTo);
            }
            catch (Exception ex)
            {
                // continue anyway
                this._log.Error(ex);
            }
        }

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// NOTE: Not logic currently present, throws NotImplementedException
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
