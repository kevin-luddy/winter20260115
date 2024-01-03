// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Linq;
    using GenTRAC.Models;
    using IES.Core;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// PPR Checklist Content Loader
	/// </summary>
	public class PPRChecklistContentLoader : IChecklistContentLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Log { get; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PPRChecklistContentLoader(ILogger<PPRChecklistContentLoader> logger)
        {
            this.Log = logger;
        }

        /// <summary>
        /// Get checklist content by proposal Id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Checklist Content DTO</returns>
        public ChecklistContentDto GetChecklistByProposalId(int proposalId)
        {
            ChecklistContentDto toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("PPRChecklistContentLoader.GetPPRChecklistByProposalId", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalPPRChecklistXREFs.Where(x => x.ProposalID == proposalId).Select(entity => new ChecklistContentDto()
                    {
                        Version = entity.PPRChecklistContent.ProposalPricingReview.ChecklistVersion,
                        Content = entity.PPRChecklistContent.ProposalPricingReview.PPRChecklistContents.Select(x => new ChecklistContentItem()
                        {
                            ChecklistId = x.ProposalPricingReviewID,
                            ChecklistType = ChecklistType.ProposalPricingReview,
                            Id = x.PPRChecklistContentID,
                            SortOrder = x.SortOrder,
                            Text = x.ChecklistText,
                            TextType = (ChecklistTextType)x.TextTypeID,
                            ColumnOrder = x.ColumnOrder
                        }).ToList()
                    }).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get Checklist ID by given proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Id of checklist</returns>
        public int? GetChecklistIdByProposalId(int proposalId)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("PPRChecklistContentLoader.GetChecklistIdByProposalId", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    ProposalPPRChecklistXREF checklistEntry = dbModel.ProposalPPRChecklistXREFs.FirstOrDefault(x => x.ProposalID == proposalId);
                    if (checklistEntry != null)
                    {
                        toReturn = checklistEntry.PPRChecklistContentID;
                    }
                }
            }

            return toReturn;
        }
    }
}
