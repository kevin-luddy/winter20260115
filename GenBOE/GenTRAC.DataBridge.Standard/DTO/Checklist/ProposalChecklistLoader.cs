// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using GenTRAC.Models;
    using IES.Standard;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Proposal dto data loader
	/// </summary>
	public class ProposalChecklistLoader : DataLoader<ProposalChecklistDto>, IProposalChecklistLoader
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProposalChecklistLoader(ILogger<ProposalChecklistLoader> logger) : base(logger)
		{
		}

        /// <summary>
        /// Returns a collection of checklist DTOs by checklist IDs
        /// </summary>
        /// <param name="ids">Collection of checklist IDs</param>
        /// <returns>Collection of ChecklistDtos</returns>
        public override ICollection<ProposalChecklistDto> GetByIds(ICollection<int> ids)
        {
            ICollection<ProposalChecklistDto> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ChecklistLoader.GetByIds", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // select the checklist IDs from the database
                    toReturn = this.ConvertEntityToDto(dbModel.ProposalChecklists.Where(x => ids.Contains(x.ProposalChecklistID)).ToList(), dbModel);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns the Checklist DTOs for a given Proposal IDs.
        /// </summary>
        /// <param name="inProposalIds">Collection of Proposal IDs</param>
        /// <returns>Collection of checklist DTOs</returns>
        public ICollection<ProposalChecklistDto> GetByProposalIds(ICollection<int> inProposalIds)
        {
            ICollection<ProposalChecklistDto> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ChecklistLoader.GetIdsByProposalId", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = this.ConvertEntityToDto(dbModel.ProposalChecklists.Where(x => inProposalIds.Contains(x.ProposalID)).ToList(), dbModel);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts a Checklist
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert</param>
        /// <returns>Id of the saved Proposal</returns>
        protected override int? Upsert(ProposalChecklistDto dtoToUpsert)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ChecklistLoader.Upsert", Log))
            {
                if (dtoToUpsert != null)
                {
                    // save
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        // only pricer can edit general info and PPR responses
                        if (dtoToUpsert.ResponseType == ChecklistResponseType.Pricer)
                        {
                            toReturn = dbModel.upsertProposalChecklist(dtoToUpsert.Id, 
                                dtoToUpsert.UpdateDate, 
                                dtoToUpsert.ProposalID,
                                dtoToUpsert.EstimatingSubmitsToContractsDate,
                                dtoToUpsert.SubmittedValue,
                                dtoToUpsert.Profit, 
                                dtoToUpsert.Com,
                                dtoToUpsert.ProfitFeeWithCom, 
                                dtoToUpsert.ROSPercentage,
                                dtoToUpsert.LMLaborHrs, 
                                dtoToUpsert.LMLaborCost,
                                dtoToUpsert.SubcontractorCost, 
                                dtoToUpsert.MaterialCost,
                                dtoToUpsert.IWTACost,
                                dtoToUpsert.TravelCost, 
                                dtoToUpsert.OtherDirectCosts, 
                                dtoToUpsert.DeliverChecklistDFARS,
                                dtoToUpsert.AbsoluteValue,
                                dtoToUpsert.CostThroughCom
                                ).FirstOrDefault();

                            ProposalChecklistSaveInfo pprUserSaveInfo = dtoToUpsert.UserSaveInfo[dtoToUpsert.ResponseType][ChecklistType.ProposalPricingReview];
                            int pprUserId = pprUserSaveInfo.UserID;
                            string pprComment = pprUserSaveInfo.Comment;

                            // update PPR responses
                            StringBuilder pprResponseBuilder = new StringBuilder();

                            // filter responses based on user
                            ICollection<ChecklistResponseItem> pprResponses = dtoToUpsert.PPRResponses.Where(x => x.ResponseType == dtoToUpsert.ResponseType).ToList();
                            foreach (ChecklistResponseItem responseItem in pprResponses)
                            {
                                pprResponseBuilder.Append(string.Format("{0},{1};", responseItem.ChecklistContentId, (int)responseItem.Response));
                            }

                            dbModel.updateProposalPPRChecklist(dtoToUpsert.ProposalID, pprResponseBuilder.ToString(),
                                (int)dtoToUpsert.ResponseType, pprComment, pprUserId, dtoToUpsert.IsSubmit);
                        }

                        if (dtoToUpsert.UserSaveInfo[dtoToUpsert.ResponseType].ContainsKey(ChecklistType.ProposalAdequacyReview))
                        {
                            ProposalChecklistSaveInfo parUserSaveInfo = dtoToUpsert.UserSaveInfo[dtoToUpsert.ResponseType][ChecklistType.ProposalAdequacyReview];
                            int parUserId = parUserSaveInfo.UserID;
                            string parComment = parUserSaveInfo.Comment;

                            // update PAR responses
                            StringBuilder parResponseBuilder = new StringBuilder();

                            // filter responses based on user
                            ICollection<ChecklistResponseItem> parResponses = dtoToUpsert.PARResponses.Where(x => x.ResponseType == dtoToUpsert.ResponseType).ToList();
                            foreach (ChecklistResponseItem responseItem in parResponses)
                            {
                                string pageNum = string.IsNullOrEmpty(responseItem.PricerPageNumber) ? string.Empty : responseItem.PricerPageNumber.TrimEnd();
                                parResponseBuilder.Append($"{responseItem.ChecklistContentId}~{(int)responseItem.Response}~{responseItem.RowComment}~{pageNum}~{responseItem.CannedResponseId}^");
                            }

                            dbModel.updateProposalPARChecklist(dtoToUpsert.ProposalID, parResponseBuilder.ToString(),
                                (int)dtoToUpsert.ResponseType, parComment, parUserId, dtoToUpsert.IsSubmit);

                            toReturn = (int?)(from x in dbModel.ProposalChecklists
                                        where dtoToUpsert.ProposalID == x.ProposalID
                                        select x.ProposalChecklistID).FirstOrDefault();
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes all Checklist data for the corresponding Proposal.  Usually handled from the ProposalLoader Delete.  Can also be called when changing the ProposalClass to Forecasted.
        /// </summary>
        /// <param name="dtoToDelete">Checklist to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        protected override int? Delete(ProposalChecklistDto dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    dbModel.deleteChecklist(dtoToDelete.ProposalID, dtoToDelete.UpdateDate);
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }

        /// <summary>
        /// Get PPR checklist responses for given proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Collection of PPR checklist response items</returns>
        public ICollection<ChecklistResponseItem> GetPPRResponses(int proposalId)
        {
            ICollection<ChecklistResponseItem> toReturn = new List<ChecklistResponseItem>();

            using (StopwatchTimer sw = new StopwatchTimer("ChecklistLoader.GetPPRResponses", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalPPRChecklistXREFs.Where(x => x.ProposalID == proposalId).Select(entity => new ChecklistResponseItem()
                    {
                        ChecklistContentId = entity.PPRChecklistContentID,
                        ChecklistType = ChecklistType.ProposalPricingReview,
                        ProposalId = entity.ProposalID,
                        Response = (ChecklistResponseOption) entity.ResponseID,
                        ResponseType = (ChecklistResponseType) entity.ResponseTypeID
                    }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get PAR checklist responses for given proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Collection of PAR checklist response items</returns>
        public ICollection<ChecklistResponseItem> GetPARResponses(int proposalId)
        {
            ICollection<ChecklistResponseItem> toReturn = new List<ChecklistResponseItem>();

            using (StopwatchTimer sw = new StopwatchTimer("ChecklistLoader.GetPARResponses", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalPARChecklistXREFs.Where(x => x.ProposalID == proposalId).Select(entity => new ChecklistResponseItem()
                    {
                        ChecklistContentId = entity.PARChecklistContentID,
                        ChecklistType = ChecklistType.ProposalAdequacyReview,
                        ProposalId = entity.ProposalID,
                        Response = (ChecklistResponseOption)entity.ResponseID,
                        ResponseType = (ChecklistResponseType)entity.ResponseTypeID,
                        PricerPageNumber = entity.PageNumber,
                        RowComment = entity.Comment,
                        CannedResponseId = entity.CannedResponseId,
                        CannedResponseText = entity.CannedResponsesPAR.Text
                    }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Converts from a collection of Checklist entity to a list of ProposalChecklistDtos.
        /// </summary>
        /// <param name="entities">Entities to convert.</param>
        /// <param name="dbModel">db model</param>
        /// <returns>A collection of ChecklistDtos, based on the entities.</returns>
        private ICollection<ProposalChecklistDto> ConvertEntityToDto(ICollection<ProposalChecklist> entities, genTRACEntities dbModel)
        {
            List<ProposalChecklistDto> result = new List<ProposalChecklistDto>();

            if (entities != null)
            {
                foreach (ProposalChecklist entity in entities)
                {
                    ProposalChecklistDto proposalChecklist = new ProposalChecklistDto()
                    {
                        Id = entity.ProposalChecklistID,
                        UpdateDate = entity.UpdateDate,
                        ProposalID = entity.ProposalID,
                        Profit = entity.Profit,
                        Com = entity.Com,
                        ProfitFeeWithCom = entity.ProfitFeeWithCom,
                        EstimatingSubmitsToContractsDate = entity.ProposalSubmittalDate,
                        ROSPercentage = entity.ROSPercentage,
                        SubmittedValue = entity.ISGSTotalPrice,
                        AbsoluteValue = entity.AbsoluteValue,
                        LMLaborHrs = entity.LMLaborHours,
                        LMLaborCost = entity.LMLaborCost,
                        SubcontractorCost = entity.SubcontractorCost,
                        MaterialCost = entity.MaterialCost,
                        IWTACost = entity.IWTACost,
                        TravelCost = entity.TravelCost,
                        OtherDirectCosts = entity.OtherDirectCost,
                        DeliverChecklistDFARS = entity.DeliverChecklistDFARS,
                        CostThroughCom = entity.CostThroughCom
                    };

                    proposalChecklist.PARResponses = dbModel.ProposalPARChecklistXREFs.Where(x => x.ProposalID == entity.ProposalID).Select(x => new ChecklistResponseItem()
                        {
                            ChecklistContentId = x.PARChecklistContentID,
                            ChecklistType = ChecklistType.ProposalAdequacyReview,
                            ProposalId = entity.ProposalID,
                            Response = (ChecklistResponseOption) x.ResponseID,
                            ResponseType = (ChecklistResponseType) x.ResponseTypeID,
                            PricerPageNumber = x.PageNumber,
                            RowComment = x.Comment,
                            CannedResponseId = x.CannedResponseId,
                            CannedResponseText = x.CannedResponsesPAR.Text
                        }).ToList(); 

                    proposalChecklist.PPRResponses = dbModel.ProposalPPRChecklistXREFs.Where(x => x.ProposalID == entity.ProposalID).Select(x => new ChecklistResponseItem()
                        {
                            ChecklistContentId = x.PPRChecklistContentID,
                            ChecklistType = ChecklistType.ProposalPricingReview,
                            ProposalId = x.ProposalID,
                            Response = (ChecklistResponseOption) x.ResponseID,
                            ResponseType = (ChecklistResponseType) x.ResponseTypeID
                        }).ToList();

                    ICollection<ProposalChecklistSaveInfo> saveInfo = dbModel.ProposalChecklistCompletes.Where(x => x.ProposalID == entity.ProposalID).Select(z => new ProposalChecklistSaveInfo()
                    {
                        UserID = z.CompletedByUserID,
                        LastSaveDate = z.LastSaveDate,
                        SubmitDate = z.SubmitDate,
                        Comment = z.Comment,
                        ResponseType = (ChecklistResponseType) z.ResponseTypeID,
                        ChecklistType = (ChecklistType) z.ChecklistTypeID
                    }).ToList();

                    ProposalChecklistSaveInfo pricerPprInfo = saveInfo.FirstOrDefault(x => x.ResponseType == ChecklistResponseType.Pricer && x.ChecklistType == ChecklistType.ProposalPricingReview);
                    if (pricerPprInfo != null)
                    {
                        proposalChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, pricerPprInfo);
                    }

                    ProposalChecklistSaveInfo pricerParInfo = saveInfo.FirstOrDefault(x => x.ResponseType == ChecklistResponseType.Pricer && x.ChecklistType == ChecklistType.ProposalAdequacyReview);
                    if (pricerParInfo != null)
                    {
                        proposalChecklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalAdequacyReview, pricerParInfo);
                    }

                    ProposalChecklistSaveInfo peerParInfo = saveInfo.FirstOrDefault(x => x.ResponseType == ChecklistResponseType.Peer && x.ChecklistType == ChecklistType.ProposalAdequacyReview);
                    if (peerParInfo != null)
                    {
                        proposalChecklist.UserSaveInfo[ChecklistResponseType.Peer].Add(ChecklistType.ProposalAdequacyReview, peerParInfo);
                    }

                    result.Add(proposalChecklist);
                }
            }

            return result;
        }

        /// <summary>
        /// Get all checklist save info for the given proposal Id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Collection of Save Info objects</returns>
        public ICollection<ProposalChecklistSaveInfo> GetAllChecklistSaveInfo(int proposalId)
        {
            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>();
            using (genTRACEntities dbModel = new genTRACEntities())
            {
                saveInfo = dbModel.ProposalChecklistCompletes.Where(x => x.ProposalID == proposalId).Select(z => new ProposalChecklistSaveInfo()
                {
                    UserID = z.CompletedByUserID,
                    LastSaveDate = z.LastSaveDate,
                    SubmitDate = z.SubmitDate,
                    Comment = z.Comment,
                    ResponseType = (ChecklistResponseType) z.ResponseTypeID,
                    ChecklistType = (ChecklistType) z.ChecklistTypeID
                }).ToList();
            }

            return saveInfo;
        }

        /// <summary>
        /// Unlock checklist for the selected user(s)
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="updateDate">Proposal update date</param>
        /// <param name="unlockOption">Indicates pricer, peer, or both users</param>
        /// <returns>Id of the saved Proposal</returns>
        public int? UnlockChecklist(int proposalId, DateTime updateDate, UnlockChecklistOption unlockOption)
        {
            int? toReturn = null;

            using (genTRACEntities dbModel = new genTRACEntities())
            {
                toReturn = dbModel.unlockProposal(proposalId, updateDate, (int)unlockOption).FirstOrDefault();
            }

            return toReturn;
        }

        /// <summary>
        /// Get the proposal submittal date for given proposal ids.
        /// </summary>
        /// <param name="proposalIds">Collection of proposal ids</param>
        /// <returns>Dictionary of proposal ids mapped to submittal date</returns>
        public IDictionary<int, DateTime?> GetProposalSubmittalDate(ICollection<int> proposalIds)
        {
            IDictionary<int, DateTime?> toReturn = new Dictionary<int, DateTime?>();

            using (genTRACEntities dbModel = new genTRACEntities())
            {
                toReturn = (from x in dbModel.ProposalChecklists
                            where proposalIds.Contains(x.ProposalID)
                            select x).ToDictionary(x => x.ProposalID, x => x.ProposalSubmittalDate);
            }

            return toReturn;
        }
    }
}
