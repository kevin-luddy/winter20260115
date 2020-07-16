// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOETransitions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class DraftLockedToAwaitingApprovalTransition : DefaultBOETransition
    {
        private ISecurityInformation _SecurityInformation;
        private IUserDTODataLoader _userLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inEmailer">Email utilities</param>
        /// <param name="inSecurityInformation">Security authorization information</param>
        /// <param name="inuserLoader">User information loader</param>
        /// <param name="inBoeApproverLoader">BOE-approver response loader</param>
        public DraftLockedToAwaitingApprovalTransition(IBoeEmailer inEmailer, ISecurityInformation inSecurityInformation, IUserDTODataLoader inuserLoader, IBoeApproverResponseDTODataLoader inBoeApproverLoader)
            : base(inEmailer, inBoeApproverLoader)
        {
            this._Emailer = inEmailer;
            this._SecurityInformation = inSecurityInformation;
            this._userLoader = inuserLoader;
        }

        /// <summary>
        /// Transition BOE from Draft-Locked to AwaitingApproval state.
        /// </summary>
        /// <param name="boe">BOE to be transitioned</param>
        /// <param name="workspace">Workspace for the BOE</param>
        /// <param name="inTransitionFrom">Current BOE state: Draft-Locked</param>
        /// <param name="inTransitionTo">New BOE state: Awaiting Approval</param>
        public override void Action(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            base.Action(boe, workspace, inTransitionFrom, inTransitionTo);

            this.SendEmail1(boe, workspace);
        }

        /// <summary>
        /// Perform validation logic for the state change to occur
        /// </summary>
        /// <param name="boe">BOE to be transitioned</param>
        /// <param name="workspace">Workspace for the BOE</param>
        /// <param name="outErrorMessage">The error message, if one occurs</param>
        /// <returns>True, if BOE is OK to be transitioned; false if not.</returns>
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

            bool okToTransition;
            if (okToTransition = base.Validate(boe, workspace, out outErrorMessage))
            {
                // combine the Author and Sub Author IDs into one collection
                Collection<int> allAuthorIDs = boe.AuthorIDs.ToList().Union(boe.SubcontractorAuthorIDs).ToCollection();
                if (allAuthorIDs.Any())
                {
                    // grab all the authors at one time
                    ICollection<UserDTO> allAuthors = this._userLoader.GetByIds(allAuthorIDs);

                    // Is the current user an author?
                    if (!allAuthors.Any(a => a.NTID.Equals(this._SecurityInformation.ActiveUserNTID)))
                    {
                        okToTransition = false;
                        outErrorMessage = "Unable to perform action.  Only an Author can perform this action.";
                    }
                    // BOE can only go from Draft-Locked to Waiting Approval if the Workspace is in Working or Locked state
                    else if (workspace.WorkspaceState == WorkspaceState.Working || workspace.WorkspaceState == WorkspaceState.Locked)
                    {
                        okToTransition = true;
                    }
                    else
                    {
                        okToTransition = false;
                        outErrorMessage = "Unable to perform action.  Workspace must be in Working or Locked state.";
                    }
                }
                else
                {
                    okToTransition = false;
                    outErrorMessage = "Unable to perform action.  There are no Authors for the BOE.";
                }
            }

            return okToTransition;
        }
    }
}
