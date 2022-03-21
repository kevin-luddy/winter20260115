// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ControllerLogic
{
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Proposals;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Web.Configuration;

    /// <summary>
    /// Business logic for completing a proposal
    /// </summary>
    public class CompletionControllerLogic : GenTRACControllerLogic
    {
        #region properties

        /// <summary>
        /// The logger
        /// </summary>
        private readonly Logger log = new Logger(typeof(ContractsControllerLogic));

        /// <summary>
        /// PTM Emailer
        /// </summary>
        // private readonly IPtmEmailer emailer;

        /// <summary>
        /// Injected Proposal Logic
        /// </summary>
        // private readonly ProposalControllerLogic proposalLogic;

        /// <summary>
        /// Injected proposal mediator
        /// </summary>
        // private readonly IProposalMediator proposalMediator;

        private readonly ContractsControllerLogic contractsLogic;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="securityAccess">asdf</param>
        /// <param name="proposalLoader">asdf</param>
        /// <param name="userMapper">asdf</param>
        /// <param name="objectFactory">asdf</param>
        /// <param name="approvalsLoader">asdf</param>
        /// <param name="proposalChecklistLoader">asdf</param>
        /// <param name="checklistMediator">asdf</param>
        /// <param name="inContractsLogic">Injected </param>
        public CompletionControllerLogic(
            ISecurityAccess securityAccess,
            IProposalLoader proposalLoader,
            IUserMapper userMapper,
            IFullObjectFactory objectFactory,
            IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader,
            IChecklistMediator checklistMediator,
            IProposalMediator proposalMediator,
            //IPtmEmailer inEmailer,
            //ProposalControllerLogic inProposalLogic,
            ContractsControllerLogic inContractsLogic
        ) : base(securityAccess, proposalLoader, userMapper, objectFactory, approvalsLoader, proposalChecklistLoader, checklistMediator, proposalMediator)
        {
            //this.emailer = inEmailer;
            //this.proposalLogic = inProposalLogic;
            //this.proposalMediator = proposalMediator;
            this.contractsLogic = inContractsLogic;
        }

        /// <summary>
        /// Logic related to setting a proposal to completed status
        /// </summary>
        /// <param name="proposalId">Id of the proposal</param>
        /// <param name="returnMessages">Error messages related to setting the status</param>
        /// <returns>true if successfully saved</returns>
        public async Task<bool> SetProposalStatusComplete(int proposalId, List<string> returnMessages)
        {
            if (proposalId < 1)
            {
                returnMessages.Add("The proposal ID invalid.");
            }
            
            log.Debug($"Setting proposal {proposalId} to Complete.");

            int? result;

            using (StopwatchTimer sw = new IES.Common.StopwatchTimer("CompletionControllerLogic.SetProposalStatusComplete", this.log))
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
                {
                    FullProposal fullProposal = await this.contractsLogic.GetFullProposalAsync(proposalId);
                    fullProposal.ProposalStatus = ProposalStatus.Completed;
                    fullProposal.CertificationTimelineCompleted = DateTime.Now;
                    fullProposal.Updateable = IES.Common.UpdateType.Upsert;
                    result = this.ProposalMediator.SaveProposal(fullProposal);

                    scope.Complete();
                }
            }

            if (result != null)
            {
                // send email
                this.contractsLogic.SendContractsStatusChangeEmails(proposalId);
            }
            else
            {
                returnMessages.Add("Saving the Completed status failed.");
            }

            return result == null ? false : true;
        }
    }
}
