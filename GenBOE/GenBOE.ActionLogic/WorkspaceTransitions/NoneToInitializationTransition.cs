// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WorkspaceTransitions
{
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;

    public class NoneToInitializationTransition : DefaultWorkspaceTransition
    {
        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        public NoneToInitializationTransition(IBoeEmailer inEmailer, IWorkspaceDTODataLoader workspaceLoader) :
            base(inEmailer, workspaceLoader)
        {
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
