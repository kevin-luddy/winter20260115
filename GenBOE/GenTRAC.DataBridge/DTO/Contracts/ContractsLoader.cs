// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// Contracts Loader
    /// </summary>
    class ContractsLoader : DataLoader<ContractsDto>, IContractsLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ContractsLoader()
        {
            this.Log = new Logger(typeof(ContractsLoader));
        }

        /// <summary>
        /// Saves a Proposal Contract
        /// </summary>
        /// <param name="dtoToUpsert">Proposal Contract to save.</param>
        /// <returns>Id of the upserted item.</returns>
        protected override int? Upsert(ContractsDto dtoToUpsert)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ContractsLoader.Upsert", Log))
            {
                if (dtoToUpsert != null)
                {
                    // save
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        toReturn = dbModel.upsertProposalContractsData(
                            dtoToUpsert.Id,
                            dtoToUpsert.UpdateDate,
                            dtoToUpsert.ProposalId,
                            dtoToUpsert.PreviouslySubmittedROM,
                            dtoToUpsert.CustomerSubmittalDate,
                            dtoToUpsert.ContractsCorrespondenceLogNumber,
                            dtoToUpsert.FinalNegotiatedValue,
                            dtoToUpsert.NegotiationsSubmitted
                            ).FirstOrDefault();
                    }

                    foreach (ContractsOffersDto contractOffer in dtoToUpsert.ContractOffers)
                    {
                        contractOffer.ContractsDataId = toReturn.Value;

                        if (contractOffer.Updateable == UpdateType.Upsert)
                        {
                            UpsertContractsOffer(contractOffer);
                        }
                        else if (contractOffer.Updateable == UpdateType.Deleted)
                        {
                            DeleteContractsOffer(contractOffer);
                        }
                        else
                        {
                            throw new ArgumentException("The dto did not specify the Updateable type.");
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes a Proposal Contract
        /// Note: This is not implemented, is deleted when Proposal is deleted.
        /// </summary>
        /// <param name="dtoToDelete">Proposal Contract to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        /// <exception cref="System.NotImplementedException">This method should never be called.</exception>
        protected override int? Delete(ContractsDto dtoToDelete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all Contracts for the proposal.
        /// </summary>
        /// <param name="proposalId">The proposal ID.</param>
        /// <returns>
        /// A Contract for the given proposal.
        /// </returns>
        public ContractsDto GetContractForProposal(int proposalId)
        {
            ContractsDto toReturn = new ContractsDto();

            using (StopwatchTimer sw = new StopwatchTimer("ContractsLoader.GetContractForProposal", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalContractsDatas.Include("ProposalContractsOffers").Where(x => x.ProposalID == proposalId)
                        .Select(x => new ContractsDto()
                        {
                            Id = x.ProposalContractsDataId,
                            UpdateDate = x.UpdateDT,
                            ProposalId = x.ProposalID,
                            PreviouslySubmittedROM = x.PreviouslySubmittedROM,
                            CustomerSubmittalDate = x.CustomerSubmittalDate,
                            ContractsCorrespondenceLogNumber = x.ContractsCorrespondLogNumber,
                            FinalNegotiatedValue = x.FinalNegotiatedValue,
                            NegotiationsSubmitted = x.FinalNegotiatedDate,
                            ContractOffers = x.ProposalContractsOffers.Select(y => new ContractsOffersDto
                            {
                                Id = y.ProposalContractsOffersId,
                                UpdateDate = y.UpdateDT,
                                ContractsDataId = y.ContractsDataId,
                                CustomerOfferAmount = y.CustomerOfferAmount,
                                CustomerOfferDate = y.CustomerOfferDate,
                                LMCounterOfferDate = (DateTime)y.LMCounterOfferDate,
                                LMCounterOfferCost = (long)y.LMCounterOfferCost,
                                LMCounterOfferCOM = (long)y.LMCounterOfferCOM,
                                LMCounterOfferProfitFee = (long)y.LMCounterOfferProfitFee
                            }).ToList()
                        }).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<ContractsDto> GetByIds(ICollection<int> ids)
        {
            ICollection<ContractsDto> toReturn;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalContractsDatas.Include("ProposalContractsOffers").Where(x => ids.Contains(x.ProposalContractsDataId))
                        .Select(x => new ContractsDto
                        {
                            Id = x.ProposalContractsDataId,
                            UpdateDate = x.UpdateDT,
                            ProposalId = x.ProposalID,
                            PreviouslySubmittedROM = x.PreviouslySubmittedROM,
                            CustomerSubmittalDate = x.CustomerSubmittalDate,
                            ContractsCorrespondenceLogNumber = x.ContractsCorrespondLogNumber,
                            FinalNegotiatedValue = x.FinalNegotiatedValue,
                            NegotiationsSubmitted = x.FinalNegotiatedDate,
                            ContractOffers = x.ProposalContractsOffers.Select(y => new ContractsOffersDto
                            {
                                Id = y.ProposalContractsOffersId,
                                UpdateDate = y.UpdateDT,
                                ContractsDataId = y.ContractsDataId,
                                CustomerOfferAmount = y.CustomerOfferAmount,
                                CustomerOfferDate = y.CustomerOfferDate,
                                LMCounterOfferDate = (DateTime)y.LMCounterOfferDate,
                                LMCounterOfferCost = (long)y.LMCounterOfferCost,
                                LMCounterOfferCOM = (long)y.LMCounterOfferCOM,
                                LMCounterOfferProfitFee = (long)y.LMCounterOfferProfitFee
                            }).ToList()
                        }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Saves a Proposal Contract Offer
        /// </summary>
        /// <param name="contractOfferDtoToUpsert">Proposal Contract Offer to save.</param>
        /// <returns>Id of the upserted item.</returns>
        private int? UpsertContractsOffer(ContractsOffersDto contractOfferDtoToUpsert)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ContractsLoader.UpsertContractsOffers", Log))
            {
                if (contractOfferDtoToUpsert != null)
                {
                    // save
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        toReturn = dbModel.upsertProposalContractsOffers(
                            contractOfferDtoToUpsert.Id,
                            contractOfferDtoToUpsert.UpdateDate,
                            contractOfferDtoToUpsert.ContractsDataId,
                            contractOfferDtoToUpsert.CustomerOfferAmount,
                            contractOfferDtoToUpsert.CustomerOfferDate,
                            contractOfferDtoToUpsert.LMCounterOfferDate,
                            contractOfferDtoToUpsert.LMCounterOfferCost,
                            contractOfferDtoToUpsert.LMCounterOfferCOM,
                            contractOfferDtoToUpsert.LMCounterOfferProfitFee
                            ).FirstOrDefault();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes a Proposal Contract
        /// </summary>
        /// <param name="contractOfferDtoToDelete">Proposal Contract Offer to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        private int? DeleteContractsOffer(ContractsOffersDto contractOfferDtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ContractsLoader.DeleteContractsOffers", Log))
            {
                if (contractOfferDtoToDelete != null)
                {
                    // delete
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        toReturn = dbModel.deleteProposalContractsOffers(
                            contractOfferDtoToDelete.Id,
                            contractOfferDtoToDelete.UpdateDate
                            );
                    }
                }
            }

            return toReturn;
        }
    }
}
