// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// PAR Checklist Content Loader
    /// </summary>
    public class PARChecklistContentLoader : IChecklistContentLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected Logger Log { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PARChecklistContentLoader()
        {
            this.Log = new Logger(typeof(PARChecklistContentLoader));
        }

        /// <summary>
        /// Get checklist content by proposal Id
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Checklist Content DTO</returns>
        [DbQuery]
        public ChecklistContentDto GetChecklistByProposalId(int proposalId)
        {
            ChecklistContentDto toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("PARChecklistContentLoader.GetChecklistByProposalId", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    var temp = dbModel.ProposalPARChecklistXREFs.Where(x => x.ProposalID == proposalId)
                        .Select(x => new
                        {
                            Version = x.PARChecklistContent.ProposalAdequacyReview.ChecklistVersion,
                            ProposalAdequacyReviewID = x.PARChecklistContent.ProposalAdequacyReview.ProposalAdequacyReviewID,
                            ChecklistType = (ProposalChecklistType)x.PARChecklistContent.ProposalAdequacyReview.ProposalChecklistTypeID,
                            Content = x.PARChecklistContent.ProposalAdequacyReview.PARChecklistContents.Select(entity => new
                            {
                                ChecklistId = entity.ProposalAdequacyReviewID,
                                ChecklistType = ChecklistType.ProposalAdequacyReview,
                                Id = entity.PARChecklistContentID,
                                SortOrder = entity.SortOrder,
                                Text = entity.ChecklistText,
                                TextType = (ChecklistTextType)entity.TextTypeID,
                                ColumnOrder = entity.ColumnOrder,
                                SubmissionItem = entity.SubmissionItem,
                                CannedResponses = entity.CannedResponsesPARs.Select(c => new { c.CannedResponseId, c.Text }),
                                YesOnly = entity.YesOnly
                            }).ToList()
                        }).FirstOrDefault();

                    toReturn = new ChecklistContentDto()
                    {
                        Version = temp.Version,
                        ProposalAdequacyReviewID = temp.ProposalAdequacyReviewID,
                        ChecklistType = temp.ChecklistType,
                        Content = new Collection<ChecklistContentItem>()
                    };

                    foreach(var content in temp.Content)
                    {
                        toReturn.Content.Add(new ChecklistContentItem()
                        {
                            ChecklistId = content.ChecklistId,
                            ChecklistType = content.ChecklistType,
                            Id = content.Id,
                            SortOrder = content.SortOrder,
                            Text = content.Text,
                            TextType = content.TextType,
                            ColumnOrder = content.ColumnOrder,
                            SubmissionItem = content.SubmissionItem,
                            YesOnly = content.YesOnly,
                            CannedResponses = content.CannedResponses.ToDictionary(c => c.CannedResponseId, c => c.Text)
                        });
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get Checklist ID by given proposal ID
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Id of checklist</returns>
        [DbQuery]
        public int? GetChecklistIdByProposalId(int proposalId)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("PARChecklistContentLoader.GetChecklistIdByProposalId", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    ProposalPARChecklistXREF checklistEntry = dbModel.ProposalPARChecklistXREFs.FirstOrDefault(x => x.ProposalID == proposalId);
                    if (checklistEntry != null)
                    {
                        toReturn = checklistEntry.PARChecklistContentID;
                    }
                }
            }

            return toReturn;
        }
    }
}
