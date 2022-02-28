// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.ActionLogic.ModelView.Proposals;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;

    /// <summary>
    /// Contracts controller logic
    /// </summary>
    public class ContractsControllerLogic : GenTRACControllerLogic
    {
        #region properties

        /// <summary>
        /// The logger
        /// </summary>
        private Logger log = new Logger(typeof(ContractsControllerLogic));

        /// <summary>
        /// the name of the contracts information form, needed for validation
        /// </summary>
        public const string CONTRACTS_INFO_FORM = "contractsInfoForm";

        /// <summary>
        /// The Contracts Loader
        /// </summary>
        private IContractsLoader contractsLoader = null;

        /// <summary>
        /// The emailer.
        /// </summary>
        private readonly IPtmEmailer emailer;

        /// <summary>
        /// Injected Proposal Logic
        /// </summary>
        private ProposalControllerLogic proposalLogic;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="securityAccess'">Security Access</param>
        /// <param name="proposalLoader">Proposal Loader</param>
        /// <param name="userMapper">User Mapper</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="approvalsLoader">Approvals Loader</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        /// <param name="checklistMediator">Checklist Mediator</param>
        /// <param name="proposalMediator">Proposal Mediator</param>
        /// <param name="contractsLoader">Contracts Loader</param>
        /// <param name="inEmailer">Emailer</param>
        /// <param name="inProposalLogic">Proposal logic</param>
        public ContractsControllerLogic(
            ISecurityAccess securityAccess,
            IProposalLoader proposalLoader,
            IUserMapper userMapper,
            IFullObjectFactory objectFactory,
            IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader,
            IChecklistMediator checklistMediator,
            IProposalMediator proposalMediator,
            IContractsLoader contractsLoader,
            IPtmEmailer inEmailer,
            ProposalControllerLogic inProposalLogic)
            : base(securityAccess, proposalLoader, userMapper, objectFactory, approvalsLoader, proposalChecklistLoader, checklistMediator, proposalMediator)
        {
            this.contractsLoader = contractsLoader;
            this.emailer = inEmailer;
            this.proposalLogic = inProposalLogic;
        }

        /// <summary>
        /// Get Contracts Data
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Contracts Tab Data</returns>
        public async Task<ContractsModelView> GetDataForProposalContracts(int proposalId)
        {
            if (proposalId < 0)
            {
                throw new ArgumentNullException(nameof(proposalId));
            }

            ContractsDto dto = this.contractsLoader.GetContractForProposal(proposalId);
            ContractsModelView model = ConvertContractsDtoToModel(dto);
            model.EppOptions = this.GetEppSelectOptions(model.EppDelegationAuthority);
            model.SetLostButtonEnabled = await this.IsValidForLostStatus(proposalId);

            // Load additional values
            model.PreviouslySubmittedRoms = this.ProposalLoader.GetRomProposalOptions(model.PreviouslySubmittedROM);

            if (model.PreviouslySubmittedROM.HasValue)
            {
                Tuple<DateTime?, decimal?> previousRomDateAndValue = this.GetRomDateAndValue(model.PreviouslySubmittedROM.Value);
                model.previousROMDt = previousRomDateAndValue?.Item1;
                model.PreviousROMValueDecimal = previousRomDateAndValue?.Item2;
            }

            return model;
        }

        /// <summary>
        /// Get ROM Date and Value
        /// </summary>
        /// <param name="proposalId">(previously) selected rom (proposal id)</param>
        /// <returns>Submittal date and value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Tuple<DateTime?, decimal?> GetRomDateAndValue(int proposalId)
        {
            if (proposalId < 0)
            {
                throw new ArgumentNullException(nameof(proposalId));
            }

            return this.ProposalLoader.GetRomDateAndValue(proposalId);
        }

        #region Contract Validate / Save

        /// <summary>
        /// Saves the contract.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="model">The model.</param>
        public int? SaveContract(ContractsModelView model)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));

            int? contractId = null;

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ContractsControllerLogic.SaveContract", this.log))
            {
                // TODO: this.ValidateContract(model, false);

                ContractsDto contract = this.ConvertContractsModelToDto(model);

                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
                {
                    contractId = this.contractsLoader.Save(contract);
                    scope.Complete();
                }
            }

            return contractId;
        }

        /// <summary>
        /// Generate the options for the EPP option select box
        /// </summary>
        /// <param name="eppDelegationAuthority">The current value for the contract</param>
        /// <returns>Collection of SelectListItems</returns>
        public ICollection<SelectListItem> GetEppSelectOptions(EppDelegationAuthority eppDelegationAuthority)
        {
            return this.contractsLoader.GetEppSelectValues(eppDelegationAuthority);
        }
        
        /// <summary>
        /// Orchestrates the setting of the proposal status to "Lost"
        /// </summary>
        /// <param name="proposalId">Proposal Id for status update</param>
        /// <param name="messages">List of validation errors (if applicable)</param>
        /// <returns></returns>
        public async Task SetProposalLost(int proposalId, List<string> messages)
        {
            if (! await this.CanUserSaveLostProposalStatus(proposalId, messages))
            {
                return;
            }

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ContractsControllerLogic.SetProposalLost", this.log))
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
                {
                    this.SetProposalStatusToLost(proposalId);
                    scope.Complete();
                }
            }

            // return control and send email async
            SendProposalLostEmail(proposalId);

        }

        /// <summary>
        /// Converts the page viewmodel into the Dto
        /// </summary>
        /// <param name="model">Contracts Model View</param>
        /// <param name="isComplete">Are we completing the proposal</param>
        /// <returns>Contracts DTO</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "*")]
        private ContractsDto ConvertContractsModelToDto(ContractsModelView model)
        {
            if (model == null)
            {
                return new ContractsDto();
            }

            ContractsDto dto = new ContractsDto();
            dto.Updateable = UpdateType.Upsert;

            dto.Id = model.Id;
            dto.ProposalId = model.ProposalId;
            dto.PreviouslySubmittedROM = model.PreviouslySubmittedROM;
            dto.CustomerSubmittalDate = DateTime.Parse(model.CustomerSubmittalDate); 
            dto.ContractsCorrespondenceLogNumber = model.ContractsCorrespondenceLogNumber;
            dto.FinalNegotiatedValue = model.FinalNegotiatedValueLong;
            dto.NegotiationsSubmitted = DateTime.Parse(model.NegotiationsSubmitted);
            dto.UpdateDateLong = model.LastUpdatedDateLong;
            dto.EppDelegationAuthority = (int)model.EppDelegationAuthority;
            dto.ProgramEppDate = model.ProgramEppDate;
            dto.LobEppDate = model.LobEppDate;
            dto.PreSpaceEppDate = model.PreSpaceEppDate;
            dto.SpaceEppDate = model.SpaceEppDate;
            dto.PreCorporateEppDate = model.PreCorporateEppDate;
            dto.CorporateEppDate = model.CorporateEppDate;
            dto.EppRosDelegationNotes = model.EppRosDelegationNotes;
            dto.LmWon = model.LmWon;
            dto.ModCompletedDate = model.ModCompletedDate;

            return dto;
        }

        /// <summary>
        /// Converts the dto to a page view model
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Contracts view model</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "*")]
        private ContractsModelView ConvertContractsDtoToModel(ContractsDto dto)
        {
            if (dto == null)
            {
                return new ContractsModelView();
            }

            ContractsModelView model = new ContractsModelView();

            model.Id = dto.Id;
            model.ProposalId = dto.ProposalId;
            model.PreviouslySubmittedROM = dto.PreviouslySubmittedROM;
            model.CustomerSubmittalDt = dto.CustomerSubmittalDate;
            model.ContractsCorrespondenceLogNumber = dto.ContractsCorrespondenceLogNumber;
            model.FinalNegotiatedValueLong = dto.FinalNegotiatedValue == null ? dto.FinalNegotiatedValue : long.Parse(dto.FinalNegotiatedValue.ToString());
            model.NegotiationsSubmittedDt = dto.NegotiationsSubmitted;
            model.LastUpdatedDateLong = dto.UpdateDateLong;
            model.EppDelegationAuthority = (EppDelegationAuthority)dto.EppDelegationAuthority;
            model.ProgramEppDate = dto.ProgramEppDate;
            model.LobEppDate = dto.LobEppDate;
            model.PreSpaceEppDate = dto.PreSpaceEppDate;
            model.SpaceEppDate = dto.SpaceEppDate;
            model.PreCorporateEppDate = dto.PreCorporateEppDate;
            model.CorporateEppDate = dto.CorporateEppDate;
            model.EppRosDelegationNotes = dto.EppRosDelegationNotes;
            model.LmWon = dto.LmWon;
            model.ModCompletedDate = dto.ModCompletedDate;

            return model;
        }

        /// <summary>
        /// Determines whether the proposal is in a valid state to have "Set Lost" status set.
        /// </summary>
        /// <param name="proposalId">Proposal Id to be considered</param>
        /// <returns>true if valid for Lost status</returns>
        private async Task<bool> IsValidForLostStatus(int proposalId)
        {
            bool valid = false;
            FullProposal fullProposal = await GetFullProposalAsync(proposalId);
            ContractsDto contractInfo = this.contractsLoader.GetContractForProposal(proposalId);

            if ((fullProposal.ProposalStatus == ProposalStatus.PendingCertification || fullProposal.ProposalStatus == ProposalStatus.PendingAward)
                && (!contractInfo.LmWon.HasValue || !contractInfo.LmWon.Value)) // null or false
            {
                valid = true;
            }

            return valid;
        }

        /// <summary>
        /// Sends templated email regarding the Lost status to the estimators
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        private async Task SendProposalLostEmail(int proposalId)
        {
            FullProposal fullProposal = await GetFullProposalAsync(proposalId);
            EmailInformationDto emailInfo = new EmailInformationDto();
            emailInfo.ProposalId = proposalId;

            EmailContent emailContent = new EmailContent();
            emailContent.Body = Emails.STATUS_LOST_SET.Body;
            emailContent.Subject = Emails.STATUS_LOST_SET.Subject;

            // Set data to be used in the replacements
            string[] subjectReplaceTokens = new string[] { fullProposal.ProposalTitle };
            string[] bodyReplaceTokens = new string[] { fullProposal.ProposalTitle, emailInfo.ProposalContractsUrl.ToString() };

            ProposalApprovalsModelView modelApprovals = this.proposalLogic.GetDataForProposalApprovals(proposalId, false);
            ProposalUserInformationModelView modelUserInfo = this.proposalLogic.GetDataForProposalUserInformation(proposalId);

            var leadEstimator = UserMapper.GetByNtid(modelApprovals.LeadEstimatorNtid);
            var backupEstimator = UserMapper.GetByNtid(modelUserInfo.BackupPricerNtId);

            // Send the email notifications
            this.emailer.SendEmail(emailContent, leadEstimator.EmailAddress, new Collection<UserDTO>(), subjectReplaceTokens, bodyReplaceTokens, null, " for: " + fullProposal.TrackingNumber);
            this.emailer.SendEmail(emailContent, backupEstimator.EmailAddress, new Collection<UserDTO>(), subjectReplaceTokens, bodyReplaceTokens, null, " for: " + fullProposal.TrackingNumber);
        }

        /// <summary>
        /// Loads the Full Proposal data using thread pool versus the main
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        private async Task<FullProposal> GetFullProposalAsync(int proposalId)
        {
            Task<FullProposal> task = Task<FullProposal>.Run(() => this.GetFullProposalDto(proposalId));            
            return task.Result;
        }

        /// <summary>
        /// Set proposal to Lost status
        /// </summary>
        /// <param name="proposalId">ID of Proposal</param>
        private void SetProposalStatusToLost(int proposalId)
        {
            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("ContractsControllerLogic.SetProposalStatusToLost", this.log))
            {
                FullProposal fullProposal = GetFullProposalAsync(proposalId).Result;
                fullProposal.ProposalStatus = ProposalStatus.Lost;
                fullProposal.CertificationTimelineCompleted = DateTime.Now;
                fullProposal.Updateable = IES.Common.UpdateType.Upsert;
                this.ProposalMediator.SaveProposal(fullProposal);
            }
        }

        /// <summary>
        /// Checks permissions and status to ensure that the proposal is valid for setting "Lost"
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="messages">Response object to be returned</param>
        /// <returns>true if valid for setting the status</returns>
        private async Task<bool> CanUserSaveLostProposalStatus(int proposalId, List<string> messages)
        {
            messages = messages ?? new List<string>();
            bool isValid = true;

            FullProposal fullProposal = await GetFullProposalAsync(proposalId);

            // check permissions? (Is this right?)
            bool isLeadOrBackup = fullProposal.Permissions.Any(x => (x.Role == PtmRole.ContractsPOC || x.Role == PtmRole.BackupContractsPOC) && x.UserId == fullProposal.CurrentUser.Id);
            bool isAdmin = this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null);

            if (!isLeadOrBackup && !isAdmin)
            {
                messages.Add("Insufficient permissions to set proposal as Lost.");
                isValid = false;
            }

            if (fullProposal.ProposalStatus != ProposalStatus.PendingCertification && fullProposal.ProposalStatus != ProposalStatus.PendingAward)
            {
                messages.Add("The proposal status must be in 'Pending Certification' or 'Pending Contractual Award' in order to set it to 'Proposal Lost'");
                isValid = false;
            }

            return isValid;
        }

        #endregion Contract Validate / Save
    }
}
