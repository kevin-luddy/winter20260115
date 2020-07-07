// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView.PostSubmittalAttachments;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Post Submittal Attachments controller
    /// </summary>
    public class PostSubmittalAttachmentsController : GenTRACController
    {
        /// <summary>
        /// Post Submittal Controller Logic
        /// </summary>
        private readonly PostSubmittalAttachmentsControllerLogic psaLogic;

        /// <summary>
        /// Security Information for Active User
        /// </summary>
        private readonly IES.Common.ISecurityInformation securityInformation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="postSubmittalAttachmentsControllerLogic">business logic for this controller</param>
        /// <param name="genTRACControllerLogic">Controller Logic</param>
        /// <param name="siteMasterUtilities">Site Master Utilities</param>
        /// <param name="securityInformation">security information about user and their context</param>
        public PostSubmittalAttachmentsController(
            PostSubmittalAttachmentsControllerLogic postSubmittalAttachmentsControllerLogic,
            GenTRACControllerLogic genTRACControllerLogic,
            SiteMasterUtilities siteMasterUtilities,
            IES.Common.ISecurityInformation securityInformation)
            : base(securityInformation, genTRACControllerLogic, siteMasterUtilities)
        {
            this.psaLogic = postSubmittalAttachmentsControllerLogic;
            this.securityInformation = securityInformation;
        }

        /// <summary>
        /// Display current user's approvals
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="psaVisibility">Indiciates the level of access to PSA the current user has</param>
        /// <returns>Displays all of the current user's approvals</returns>
        public ViewResult DisplayPostSubmittalAttachments(int proposalId, SecurityAuthorization psaVisibility)
        {
            PostSubmittalAttachmentModelView model = new PostSubmittalAttachmentModelView
            {
                PsaVisibility = psaVisibility,
                ProposalId = proposalId,
                MaxFileSize = SiteMasterUtilities.MaxFileSize,
                AllowedFileTypes = SiteMasterUtilities.AllowedFileTypes,
                PostSubmittalAttachments = this.psaLogic.GetPostSubmittalAttachments(proposalId),
                MaxOtherFileCount = SiteMasterUtilities.MaxOtherFileCount
            };

            this.ViewBag.IsRevision = this.psaLogic.GetFullProposalDto(proposalId).IsRevision;

            return this.View(WebConstants.View.POST_SUBMITTAL_ATTACHMENTS, model);
        }

        /// <summary>
        /// Uploads PSA file
        /// </summary>
        /// <param name="proposalId">Proposal to upload file to</param>
        /// <param name="file">File being uploaded</param>
        /// <param name="attachment">attachment data</param>
        /// <returns>File upload status</returns>
        [HttpPost]
        public JsonResult UploadAttachment(int proposalId, HttpPostedFileBase file, AttachmentDto attachment)
        {
            if (attachment == null || (file == null && !attachment.IsRevisionReference))
            {
                return this.Json("There was a problem uploading the file");
            }

            if (attachment.IsRevisionReference)
            {
                AttachmentDto result = this.psaLogic.SaveAttachmentReferenceForRevisedProposal(proposalId, attachment.AttachmentType);
                return this.Json(result);
            }
            else
            {
                ICollection<string> errors = this.psaLogic.ValidateFileTypeAndSize(file, SiteMasterUtilities.AllowedFileTypes, SiteMasterUtilities.MaxFileSize);
                errors = errors.Concat(this.psaLogic.ValidateOtherFileCount(proposalId, attachment, SiteMasterUtilities.MaxOtherFileCount)).ToList();

                if (errors.Any())
                {
                    return this.Json(string.Join("\n", errors.ToArray()));
                }

                attachment.UploadedBy = this.securityInformation.ActiveUserData.DisplayName;
                this.psaLogic.UploadAttachment(file, attachment);
                attachment.Contents = null;
                return this.Json(attachment);
            }
        }

        /// <summary>
        /// Downloads the attachment with fileId
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="fileId">File ID</param>
        /// <returns>File to download</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
        public FileResult DownloadAttachment(int proposalId, int fileId)
        {
            AttachmentDto attachment = this.psaLogic.DownloadAttachment(fileId);
            return this.File(attachment.Contents, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.Name);
        }

        /// <summary>
        /// Deletes the attachment with fileId
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="attachment">Attachment to delete</param>
        /// <returns>Action status</returns>
        [HttpPost]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
        public JsonResult DeleteAttachment(int proposalId, AttachmentDto attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (attachment.Id > 0)
            {
                this.psaLogic.DeleteAttachment(attachment);

                return this.Json(new AttachmentDto
                {
                    AttachmentType = attachment.AttachmentType
                });
            }

            return this.Json("File does not exist");
        }

        /// <summary>
        /// Validates that all of the requried attachments have been uploaded.
        /// </summary>
        /// <param name="proposalId">The ProposalId.</param>
        /// <returns>True, if successful.</returns>
        public JsonResult ValidateRequiredAttachments(int proposalId)
        {
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;

            this.psaLogic.ValidateRequiredAttachments(proposalId, validationErrors);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            return this.Json(new { Status = true });    // Success - no validation errors found.
        }
    }
}