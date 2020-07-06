// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// Approvals Loader
    /// </summary>
    public class AttachmentLoader : DataLoader<AttachmentDto>, IAttachmentLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AttachmentLoader()
        {
            this.Log = new Logger(typeof(AttachmentLoader));
        }

        /// <summary>
        /// Gets all Attachments for the proposal, excluding the File Contents.
        /// </summary>
        /// <param name="proposalId">The proposal ID.</param>
        /// <returns>
        /// A list of Attachments for the given proposal.
        /// </returns>
        public ICollection<AttachmentDto> GetAttachmentsForProposal(int proposalId)
        {
            ICollection<AttachmentDto> toReturn = new List<AttachmentDto>();
            using (StopwatchTimer sw = new StopwatchTimer("AttachmentLoader.GetAttachmentsForProposal", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalsAttachments.Where(x => x.ProposalId == proposalId)
                        .Select(x => new AttachmentDto()
                        {
                            Id = x.AttachmentId,
                            ProposalId = x.ProposalId,
                            IsRevisionReference = x.IsRevisionReference,
                            AttachmentType = (AttachmentType)x.AttachmentType,
                            Name = x.Attachment.Name,
                            UpdateDate = x.Attachment.UpdateDate,
                            UploadedBy = x.Attachment.UploadedBy
                        }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        /// <exception cref="System.NotImplementedException">This method should never be called.</exception>
        public override ICollection<AttachmentDto> GetByIds(ICollection<int> ids)
        {
            ICollection<AttachmentDto> toReturn;
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalsAttachments.Where(x => ids.Contains(x.AttachmentId))
                        .Select(x => new AttachmentDto
                        {
                            Id = x.AttachmentId,
                            ProposalId = x.ProposalId,
                            IsRevisionReference = x.IsRevisionReference,
                            AttachmentType = (AttachmentType)x.AttachmentType,
                            Name = x.Attachment.Name,
                            UpdateDate = x.Attachment.UpdateDate,
                            UploadedBy = x.Attachment.UploadedBy,
                            Contents = x.Attachment.Contents
                        }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the attachment metadata only.
        /// </summary>
        /// <param name="attachmentId">The attachment identifier.</param>
        /// <returns>AttachmentDto without byte[] representation of the attachment file.</returns>
        public AttachmentDto GetAttachmentMetadata(int attachmentId)
        {
            AttachmentDto toReturn;
            using (StopwatchTimer sw = new StopwatchTimer("AttachmentLoader.GetAttachment", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalsAttachments.Where(x => x.AttachmentId == attachmentId)
                        .Select(x => new AttachmentDto
                        {
                            Id = x.AttachmentId,
                            ProposalId = x.ProposalId,
                            IsRevisionReference = x.IsRevisionReference,
                            AttachmentType = (AttachmentType)x.AttachmentType,
                            Name = x.Attachment.Name,
                            UpdateDate = x.Attachment.UpdateDate,
                            UploadedBy = x.Attachment.UploadedBy
                        }).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns true if all required attachments have been uploaded
        /// </summary>
        /// <param name="proposalId">ID of proposal to check</param>
        /// <returns>true if all required attachments have been uploaded</returns>
        public bool AllRequiredAttachmentsHaveBeenUploaded(int proposalId)
        {
            bool toReturn;
            using (StopwatchTimer sw = new StopwatchTimer("AttachmentLoader.AllAttachmentsHaveBeenUploaded", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalsAttachments.Any(x => x.ProposalId == proposalId && x.AttachmentType == (int)AttachmentType.CostKickOffPackage)
                            && dbModel.ProposalsAttachments.Any(x => x.ProposalId == proposalId && x.AttachmentType == (int)AttachmentType.ResponsibilityAssignmentsMatrix);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns true if the optional DelegationOfAuthority attachment has been uploaded
        /// </summary>
        /// <param name="proposalId">ID of proposal to check</param>
        /// <returns>true if the optional DelegationOfAuthority attachment has been uploaded</returns>
        public bool OptionalAttachmentHasBeenUploaded(int proposalId)
        {
            bool toReturn;
            using (StopwatchTimer sw = new StopwatchTimer("AttachmentLoader.OptionalAttachmentHasBeenUploaded", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.ProposalsAttachments.Any(x => x.ProposalId == proposalId && x.AttachmentType == (int)AttachmentType.DelegationOfAuthority);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Delete an Attachment
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>
        /// Id of the deleted object
        /// </returns>
        protected override int? Delete(AttachmentDto dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    dbModel.deleteAttachment(dtoToDelete.Id, dtoToDelete.UpdateDate, dtoToDelete.ProposalId, !dtoToDelete.IsRevisionReference);
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert an Attachment.
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>
        /// Int representing the id of the upserted item.
        /// </returns>
        protected override int? Upsert(AttachmentDto dtoToUpsert)
        {
            int? toReturn = null;

            if (dtoToUpsert != null)
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.upsertAttachment(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.Name, dtoToUpsert.Contents, dtoToUpsert.UploadedBy, (int)dtoToUpsert.AttachmentType, dtoToUpsert.ProposalId, dtoToUpsert.IsRevisionReference).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Save only a reference to an attachment, for a revised proposal
        /// </summary>
        /// <param name="proposalId">Current Proposal Id</param>
        /// <param name="attachmentType">Attachment type that you want to reference</param>
        /// <returns>Attachment Id</returns>
        public int? SaveAttachmentReferenceForRevisedProposal(int proposalId, AttachmentType attachmentType)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("AttachmentLoader.SaveAttachmentReferenceForRevisedProposal", this.Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    // ToDo: Dusan / RJ - replace ".ProposalID" with OriginalProposalId, once it's ready
                    // will need to write unit test for it as well. 
                    int originalProposalId = dbModel.Proposals.First(x => x.ProposalID == proposalId).ProposalID;
originalProposalId = 74825;
                    int attachmentId = dbModel.ProposalsAttachments.First(x => x.AttachmentType == (int)attachmentType && x.Proposal.ProposalID == originalProposalId).AttachmentId;

                    toReturn = dbModel.upsertAttachment(attachmentId, null, null, null, null, (int)attachmentType, proposalId, true).FirstOrDefault();
                }
            }

            return toReturn;
        }
    }
}
