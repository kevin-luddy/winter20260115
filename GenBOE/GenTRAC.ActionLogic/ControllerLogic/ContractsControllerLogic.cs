// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;

    /// <summary>
    /// Contracts controller logic
    /// </summary>
    public class ContractsControllerLogic : GenTRACControllerLogic
    {
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
        public ContractsControllerLogic(ISecurityAccess securityAccess, IProposalLoader proposalLoader, IUserMapper userMapper, IFullObjectFactory objectFactory, IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader, IChecklistMediator checklistMediator, IProposalMediator proposalMediator)
            : base(securityAccess, proposalLoader, userMapper, objectFactory, approvalsLoader, proposalChecklistLoader, checklistMediator, proposalMediator)
        {
        }

        /// <summary>
        /// Get Contracts Data
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Contracts Tab Data</returns>
        public ContractsModelView GetContractsData(int proposalId)
        {
            if(proposalId < 0)
            {
                throw new ArgumentNullException(nameof(proposalId));
            }
            
            int i = 1;
            ContractsModelView model = new ContractsModelView()
            {
                ContractsCorrespondenceLogNumber = "log #",
                CustomerSubmittalDate = DateTime.Now.AddDays(-1).ToString(),
                ContractOffers = new List<ContractsOfferModelView>()
                {
                    new ContractsOfferModelView()
                    {
                        Id = ++i,
                        CustomerOfferAmmountInt = 100000,
                        CustomerOfferDate = DateTime.Now.AddDays(-30).ToString(),
                        LMCounterOfferCOMInt = 70000,
                        LMCounterOfferCostInt = 20000,
                        LMCounterOfferProfitFeeInt = 19000,
                        LMCounterOfferDate = DateTime.Now.AddDays(-10).ToString()
                    },
                    new ContractsOfferModelView()
                    {
                        Id = ++i,
                        CustomerOfferAmmountInt = 110000,
                        CustomerOfferDate = DateTime.Now.AddDays(-3).ToString()
                    }
                },
                FinalNegotiatedValueInt = 114000,
                NegotiationsSubmitted = DateTime.Now.AddDays(-1).ToString(),
                PreviouslySubmittedROM = 15212
            };

            // Load additional values
            model.PreviouslySubmittedRoms = this.ProposalLoader.GetRomProposalOptions(model.PreviouslySubmittedROM);

            if (model.PreviouslySubmittedROM.HasValue)
            {
                Tuple<DateTime?, decimal?> previousRomDateAndValue = this.GetRomDateAndValue(model.PreviouslySubmittedROM.Value);
                model.previousROMDt = previousRomDateAndValue?.Item1;
                model.PreviousROMValueDecimal = previousRomDateAndValue?.Item2;
            }

            // Add an extra dummy offer, for UI clone purposes. We will be throwing this one out when the data comes back into a save
            model.ContractOffers.Add(new ContractsOfferModelView() { Id = ContractsOfferModelView.OFFER_TO_IGNORE_ID });

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
            return this.ProposalLoader.GetRomDateAndValue(proposalId);
        }
    }
}
