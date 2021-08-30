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

    public class InitializationToWorkingTransition : DefaultWorkspaceTransition
    {
        private Logger _log = new Logger(typeof(InitializationToWorkingTransition));

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        public InitializationToWorkingTransition(IBoeEmailer inEmailer, IWorkspaceDTODataLoader workspaceLoader) :
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

            try
            {
                base.Action(workspace, inTransitionFrom, inTransitionTo);
                this.SendEmail1(workspace);
            }
            catch (Exception ex)
            {
                // continue anyway
                this._log.Error(ex);
            }
        }

        /// <summary>
        /// Perform validation logic for the state change to occur. verify that atleast one BOE is in the draft state
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
                // check to see that at least 1 BOEs is in draft or above
                toReturn = workspace.Boes.Any(x => x.State >= BOEState.Draft);

                if (!toReturn)
                {
                    outErrorMessage = "At least 1 BOE must be in the Draft state";
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Send an email to authors depending on if this is the first or a subsequent time making this transistion
        /// </summary>
        /// <param name="inWorkspaceId">the workspace id</param>
        protected override void SendEmail1(FullWorkspace workspace)
        {
            if(workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            bool firstTransition = workspace.WorkspaceHistory.Count(x => x.OldValue == WorkspaceState.Initialization && x.NewValue == WorkspaceState.Working) == 1;

            if (firstTransition)
            {
                this._Emailer.SendWorkspaceAuthorsEmailOpenedForEdit(workspace);
            }
            else
            {
                this._Emailer.SendWorkspaceInitializationToWorkingSubsequent(workspace);
            }
        }
    }
}
