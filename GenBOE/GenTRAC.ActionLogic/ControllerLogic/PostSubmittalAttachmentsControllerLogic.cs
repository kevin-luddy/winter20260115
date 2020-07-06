// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using System.Web;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// The business logic for the Post Submittal Attachments controller
    /// </summary>
    public class PostSubmittalAttachmentsControllerLogic : GenTRACControllerLogic
    {
        /// <summary>
        /// User Loader
        /// </summary>
        private IAttachmentLoader attachmentLoader;

        /// <summary>
        /// PostSubmittalAttachmentsControllerLogic Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Security Access</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        /// <param name="inUserMapper">User Mapper</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="approvalsLoader">Approvals Loader</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        /// <param name="inChecklistMediator">Checklist Mediator</param>
        /// <param name="inProposalMediator">proposal mediator</param>
        /// <param name="attachmentLoader">PSA attachment loader</param>
        public PostSubmittalAttachmentsControllerLogic(
            ISecurityAccess inSecurityAccess, 
            IProposalLoader inProposalLoader, 
            IUserMapper inUserMapper, 
            IFullObjectFactory objectFactory, 
            IApprovalsLoader approvalsLoader, 
            IProposalChecklistLoader proposalChecklistLoader, 
            IChecklistMediator inChecklistMediator, 
            IProposalMediator inProposalMediator, 
            IAttachmentLoader attachmentLoader) 
            : base(inSecurityAccess, inProposalLoader, inUserMapper, objectFactory, approvalsLoader, proposalChecklistLoader, inChecklistMediator, inProposalMediator)
        {
            this.attachmentLoader = attachmentLoader;
        }

        /// <summary>
        /// Retrieves the list of post submittal attachments
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>List of post submittal attachments</returns>
        public ICollection<AttachmentDto> GetPostSubmittalAttachments(int proposalId)
        {
            ICollection<AttachmentDto> psaFiles = this.attachmentLoader.GetAttachmentsForProposal(proposalId);

            foreach (AttachmentType attachmentType in Enum.GetValues(typeof(AttachmentType)))
            {
                // All PSA attachment types are needed for the view so create an empty one if it doesn't already exist in the database
                if (psaFiles.All(file => file.AttachmentType != attachmentType))
                {
                    // Do not add blank rows for 'Other' attachments - these are added via the Add Attachment button.
                    if (attachmentType != AttachmentType.Other)
                    {
                        psaFiles.Add(new AttachmentDto
                        {
                            AttachmentType = attachmentType
                        });
                    }
                }
            }
            
            List<AttachmentDto> psaFilesOrdered = psaFiles.OrderBy(file => file.AttachmentType == AttachmentType.Other).ThenBy(file => file.AttachmentType.GetDescription()).ThenBy(file => file.UpdateDateLong).ToList();
            return psaFilesOrdered;
        }

        /// <summary>
        /// Ensures file has allowed extension and size is under the max allowed file size
        /// </summary>
        /// <param name="file">file to validate</param>
        /// <param name="allowedFileTypes">Allowed file types</param>
        /// <param name="maxFileSize">Max allowed file size</param>
        /// <returns>collection of validation errors</returns>
        public ICollection<string> ValidateFileTypeAndSize(HttpPostedFileBase file, string allowedFileTypes, int? maxFileSize)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (allowedFileTypes == null)
            {
                throw new ArgumentNullException(nameof(allowedFileTypes));
            }

            if (maxFileSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFileSize));
            }

            ICollection<string> errors = new Collection<string>();

            // Check file extension
            string[] allowedExtensions = allowedFileTypes.Split(',');

            string fileExtension = file.FileName.Substring(file.FileName.LastIndexOf('.')).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                errors.Add(string.Format("Only files with the following file extensions are allowed: {0}", allowedFileTypes));
            }

            // Check size
            if (file.ContentLength > maxFileSize)
            {
                errors.Add(string.Format("File cannot be larger than {0} MB", maxFileSize / 1000000));
            }

            return errors;
        }

        /// <summary>
        /// Ensures that the user does not exceed the maximum number of allowed 'Other' files.
        /// </summary>
        /// <param name="proposalId">The ProposalId.</param>
        /// <param name="attachment">The attachment being uploaded.</param>
        /// <param name="maxOtherFileCount">The maximum allowed number of 'Other' files.</param>
        /// <returns>A collection of validation errors.</returns>
        public ICollection<string> ValidateOtherFileCount(int proposalId, AttachmentDto attachment, int? maxOtherFileCount)
        {
            ICollection<string> errors = new Collection<string>();

            if (attachment != null)
            {
                if (attachment.AttachmentType == AttachmentType.Other && attachment.Id < 0)
                {
                    ICollection<AttachmentDto> psaFiles = this.attachmentLoader.GetAttachmentsForProposal(proposalId);
                    if (psaFiles != null && psaFiles.Count(file => file.AttachmentType == AttachmentType.Other) >= maxOtherFileCount)
                    {
                        errors.Add(string.Format("A maximum of <span style=\"font-weight:bold;\">{0}</span> files with the Attachment Type of \"Other\" are allowed for upload.  It is possible that another user has added additional files prior to you.  You must delete one of the existing attachments with a type of \"Other\" prior to adding a new one.", maxOtherFileCount));
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates that all the required attachments have been uploaded.
        /// </summary>
        /// <param name="proposalId">The ProposalId.</param>
        /// <param name="validationMessages">The collection of ValidationMessages to update.</param>
        public void ValidateRequiredAttachments(int proposalId, ICollection<ValidationMessage> validationMessages)
        {
            if (validationMessages == null)
            {
                validationMessages = new List<ValidationMessage>();
            }

            if (!this.attachmentLoader.AllRequiredAttachmentsHaveBeenUploaded(proposalId))
            {
                validationMessages.Add(new ValidationMessage("Not all required Post Submittal Attachments are present."));
            }
        }

        /// <summary>
        /// Saves new PSA attachment to database
        /// </summary>
        /// <param name="file">File to save</param>
        /// <param name="attachment">Attachment to save</param>
        public void UploadAttachment(HttpPostedFileBase file, AttachmentDto attachment)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            using (MemoryStream target = new MemoryStream())
            {
                file.InputStream.CopyTo(target);
                attachment.Contents = target.ToArray();
                attachment.Name = Path.GetFileName(file.FileName);
                attachment.Updateable = IES.Common.UpdateType.Upsert;

                using (TransactionScope scope = new TransactionScope())
                {
                    attachment.Id = this.attachmentLoader.Save(attachment).Value;
                    attachment.UpdateDate = this.attachmentLoader.GetAttachmentMetadata(attachment.Id).UpdateDate;
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Save only a reference to an attachment, for a revised proposal
        /// </summary>
        /// <param name="proposalId">Current Proposal Id</param>
        /// <param name="attachment">Attachment that you are saving</param>
        public void SaveAttachmentReferenceForRevisedProposal(int proposalId, AttachmentDto attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            using (TransactionScope scope = new TransactionScope())
            {
                attachment.Id = this.attachmentLoader.SaveAttachmentReferenceForRevisedProposal(proposalId, attachment.AttachmentType).Value;
                scope.Complete();
            }
        }

        /// <summary>
        /// Retrieves file from the database
        /// </summary>
        /// <param name="fileId">ID of file to retrieve</param>
        /// <returns>File with fileId</returns>
        public AttachmentDto DownloadAttachment(int fileId)
        {
            return this.attachmentLoader.GetById(fileId);
        }

        /// <summary>
        /// Deletes an attachment
        /// </summary>
        /// <param name="attachment">Attachment to delete</param>
        public void DeleteAttachment(AttachmentDto attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (attachment.Id > 0)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    attachment.Updateable = IES.Common.UpdateType.Deleted;
                    this.attachmentLoader.Save(attachment);
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Deletes all attachments for the proposal.
        /// </summary>
        /// <param name="proposalId">The ProposalId.</param>
        public void DeleteAllAttachments(int? proposalId)
        {
            if (proposalId != null)
            {
                Collection<AttachmentDto> attachments = this.GetPostSubmittalAttachments(proposalId.Value).ToCollection();

                if (attachments != null && attachments.Any())
                {
                    for (int i = 0; i < attachments.Count; i++)
                    {
                        this.DeleteAttachment(attachments[i]);
                    }
                }
            }
        }
    }
}