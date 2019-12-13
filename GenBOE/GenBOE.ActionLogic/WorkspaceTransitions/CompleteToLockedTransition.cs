// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using System;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;

    public class CompleteToLockedTransition : DefaultWorkspaceTransition
    {
        private Logger _log = new Logger(typeof(CompleteToLockedTransition));

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        public CompleteToLockedTransition(IBoeEmailer inEmailer, IWorkspaceDTODataLoader workspaceLoader) :
            base(inEmailer, workspaceLoader)
        {
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void Action(FullWorkspace workspace, WorkspaceState inTransitionFrom, WorkspaceState inTransitionTo)
        {
            // do database work first .. if error exception will stop emails from being sent
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
