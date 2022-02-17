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
    using System.Transactions;
    using System.Web.Configuration;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
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
        public ContractsControllerLogic(ISecurityAccess securityAccess, IProposalLoader proposalLoader, IUserMapper userMapper, IFullObjectFactory objectFactory, IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader, IChecklistMediator checklistMediator, IProposalMediator proposalMediator, IContractsLoader contractsLoader)
            : base(securityAccess, proposalLoader, userMapper, objectFactory, approvalsLoader, proposalChecklistLoader, checklistMediator, proposalMediator)
        {
            this.contractsLoader = contractsLoader;
        }

        /// <summary>
        /// Get Contracts Data
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Contracts Tab Data</returns>
        public ContractsModelView GetDataForProposalContracts(int proposalId)
        {
            if (proposalId < 0)
            {
                throw new ArgumentNullException(nameof(proposalId));
            }

            ContractsDto dto = this.contractsLoader.GetContractForProposal(proposalId);
            ContractsModelView model = ConvertContractsDtoToModel(dto);

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

        //private void ValidateContract(ContractsModelView model, bool isComplete)
        //{
        //    _ = model ?? throw new ArgumentNullException(nameof(model));

        //    // TODO: Add Validation here
        //}

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

            return model;
        }

        #endregion Contract Validate / Save
    }
}
