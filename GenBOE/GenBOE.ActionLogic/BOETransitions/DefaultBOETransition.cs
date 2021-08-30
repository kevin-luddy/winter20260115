// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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

    public class DefaultBOETransition : IBOEStateTransition
    {
        protected IBoeEmailer _Emailer { get; set; }
        protected IBoeApproverResponseDTODataLoader _BoeApproverLoader { get; set; }
        /// <summary>
        /// Gets or sets the Boe State Machine.
        /// </summary>
        public IBOEStateMachine BoeStateMachine { get; set; }

        /// <summary>
        /// Gets or sets the boe dto data loader.
        /// </summary>
        public IBoeDTODataLoader BoeDTODataLoader { get; set; }

        /// <summary>
        /// constructor for dependency injection
        /// </summary>
        /// <param name="inEmailer">the emailer to use</param>
        /// <param name="inBoeApproverLoader">Boe Approver response loader.</param>
        public DefaultBOETransition(IBoeEmailer inEmailer, IBoeApproverResponseDTODataLoader inBoeApproverLoader)
        {
            this._Emailer = inEmailer;
            this._BoeApproverLoader = inBoeApproverLoader;
        }

        /// <summary>
        /// Perform common actions associated with all transitions
        /// </summary>
        /// <param name="boe">boe to take action on</param>
        /// <param name="workspace">workspace</param>
        /// <param name="inTransitionFrom">the transition from</param>
        /// <param name="inTransitionTo">the transition to</param>
        public virtual void Action(FullBoe boe, FullWorkspace workspace, BOEState inTransitionFrom, BOEState inTransitionTo)
        {
            // no default Action
        }

        /// <summary>
        /// Validate if the transition if able to be performed
        /// </summary>
        /// <param name="boe">the boe id to check</param>
        /// <param name="workspace">workspace</param>
        /// <param name="outErrorMessage">The error message, if one occurs</param>
        /// <returns></returns>
        public virtual bool Validate(FullBoe boe, FullWorkspace workspace, out string outErrorMessage)
        {
            outErrorMessage = string.Empty;

            return true;
        }

        /// <summary>
        /// Send an email to authors and approvers depending on BOE states
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <param name="inWorkspace">The workspace the BOE belongs to.</param>
        protected virtual void SendEmail1(FullBoe boe, FullWorkspace inWorkspace)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (boe.State == BOEState.Draft)
            {
                this._Emailer.SendBOEAuthorsEmailOpenedForEdit(boe, inWorkspace);
            }

            else if (boe.State == BOEState.AwaitingApproval)
            {
                this._Emailer.SendBOEApproversEmailAwaitingApproval(boe);
            }
        }

        /// <summary>
        /// Resets all BOE approvals to NULL
        /// </summary>
        /// <param name="boe">The BOE to reset</param>
        protected virtual void ResetAllBOEApprovals(FullBoe boe)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            Collection<BoeApproverResponseDTO> boeApproverResponsesCopy = new Collection<BoeApproverResponseDTO>();

            if (boe.Id > 0)
            {
                IReadOnlyCollection<BoeApproverResponseDTO> boeApps = boe.ApproverResponses;
                // If there are approvers for the BOE, let's continue
                if (boeApps.Any())
                {
                    // Reset each approver response to NULL
                    foreach (BoeApproverResponseDTO approver in boeApps)
                    {
                        // Set the approval to Upsert to tell the back end to update
                        approver.ApproverResponse = ApproverReponseType.None;
                        approver.Updateable = UpdateType.Upsert;

                        // Add the updated approver to the BOE to save
                        boeApproverResponsesCopy.Add(approver);
                    }

                    // Save the approvers with the BOE
                    this._BoeApproverLoader.Save(boeApproverResponsesCopy);
                }
            }
        }
    }
}
