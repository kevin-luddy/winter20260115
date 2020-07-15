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

    public class DraftToAwaitingApprovalTransition : DefaultBOETransition
    {
        private ISecurityInformation _SecurityInformation;
        private IUserDTODataLoader _userLoader;

        /// <summary>
        /// Workspace loader
        /// </summary>
        private IWorkspaceDTODataLoader _WorkspaceLoader;

        /// <summary>
        /// constructor for dependency injection
        /// </summary>
        /// <param name="inEmailer">the emailer to use</param>
        /// <param name="inSecurityInformation">Security info</param>
        /// <param name="inuserLoader">User data loader</param>
        /// <param name="inBoeApproverLoader">Boe approver response loader.</param>
        /// <param name="workspaceLoader">Workspace data loader</param>
        /// constructor for dependency injection
        public DraftToAwaitingApprovalTransition(IBoeEmailer inEmailer,
            ISecurityInformation inSecurityInformation, IUserDTODataLoader inuserLoader, IBoeApproverResponseDTODataLoader inBoeApproverLoader, IWorkspaceDTODataLoader workspaceLoader)
            : base(inEmailer, inBoeApproverLoader)
        {
            this._SecurityInformation = inSecurityInformation;
            this._userLoader = inuserLoader;
            this._WorkspaceLoader = workspaceLoader;
        }

        /// <summary>
        /// Send the email
        /// </summary>
        /// <param name="boe">boe to take action on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">Draft</param>
        /// <param name="inTransitionTo">Awaiting Approval</param>
        public override void Action(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (workspace.WorkspaceState == WorkspaceState.Locked)
            {
                this.StoreLockedRates(boe, workspace);
            }

            base.Action(boe, workspace, inTransitionFrom, inTransitionTo);

            this.SendEmail1(boe, workspace); 
        }

        /// <summary>
        /// As we do for workspaces, when a BOE is "locked" for edit, we need to store a copy of the current rates.
        /// Rates should not be locked-down unless/until ALL of the BOEs have been re-locked.
        /// </summary>
        /// <param name="boe">boe to take action on</param>
        /// <param name="workspace">workspace</param>
        private void StoreLockedRates(FullBoe boe, FullWorkspace workspace)
        {
            // If there are any BOEs (besides THIS one) that are still in Draft, then defer rate-locking (i.e. those BOEs will handle it).
            if (!workspace.Boes.Where(b => b.Id != boe.Id).Any(b => b.State == BOEState.Draft))  // there are no unlocked draft BOEs		
            {
                this._WorkspaceLoader.LockTravelAndResourceRatesForWorkspace(workspace.Id);
            }
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
                    // BOE can only go from Draft to Waiting Approval if the Workspace is in Working or Locked state
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
