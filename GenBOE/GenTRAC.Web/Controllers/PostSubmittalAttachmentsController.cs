// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
	using GenTRAC.ActionLogic;
	using GenTRAC.ActionLogic.ModelView.PostSubmittalAttachments;
	using GenTRAC.DataBridge.DTO;
	using GenTRAC.Objects.FullObject;
	using GenTRAC.Web.Common;
	using GenTRAC.Web.ModelView;
	using IES.ActionLogic.Common;
	using IES.Common;
	using IES.Common.Exceptions;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;

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
		/// Proposal Loader
		/// </summary>
		private readonly IProposalLoader proposalLoader;

		/// <summary>
		/// Logger
		/// </summary>
		private IES.Common.Logger log = new IES.Common.Logger(typeof(PostSubmittalAttachmentsController));

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
            IES.Common.ISecurityInformation securityInformation,
			IProposalLoader proposalLoader)
            : base(securityInformation, genTRACControllerLogic, siteMasterUtilities)
        {
            this.psaLogic = postSubmittalAttachmentsControllerLogic;
            this.securityInformation = securityInformation;
			this.proposalLoader = proposalLoader;
        }

        /// <summary>
        /// Display current user's approvals
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <param name="psaVisibility">Indiciates the level of access to PSA the current user has</param>
        /// <returns>Displays all of the current user's approvals</returns>
        public async Task<ViewResult> DisplayPostSubmittalAttachments(int proposalId, SecurityAuthorization psaVisibility)
        {
            PostSubmittalAttachmentModelView model = new PostSubmittalAttachmentModelView
            {
                PsaVisibility = psaVisibility,
                ProposalId = proposalId,
                MaxFileSize = SiteMasterUtilities.MaxFileSize,
                AllowedFileTypes = SiteMasterUtilities.AllowedFileTypes,
                PostSubmittalAttachments = this.psaLogic.GetPostSubmittalAttachments(proposalId),
                MaxOtherFileCount = SiteMasterUtilities.MaxOtherFileCount,
				IsEPPIntegrationEnabled = SiteMasterUtilities.IsEPPIntegrationEnabled
            };

			// If there is an attachment of Delegation of Authority that already exists, show the current upload in the UI
			if (model.PostSubmittalAttachments.Any(x => x.AttachmentType == AttachmentType.DelegationOfAuthority && x.FileHasBeenUploaded))
			{
				AttachmentDto attachment = model.PostSubmittalAttachments.First(x => x.AttachmentType == AttachmentType.DelegationOfAuthority);
				attachment.ShowPTMUploadForDelegationOfAuthority = true;
			}
			// If EPP is integrated and there is no existing attachment thru PTM, check the status of the associated eEPP record linked to the PTM tracking number, if any
			else if (SiteMasterUtilities.IsEPPIntegrationEnabled)
			{
				ProposalDto proposal = this.proposalLoader.GetById(model.ProposalId);
				if (proposal != null)
				{
					// Check if this exists in eEPP
					using (HttpClient httpClient = new HttpClient(new HttpClientHandler()
					{
						UseDefaultCredentials = true
					}))
					{
						string eeppAPI = IES.Common.ConfigurationUtilities.GetAppSetting("eEPPUrl");
						string url = $"{eeppAPI}/api/eEPP/EPPController/GeteEPPDataByTrackingNumber/{proposal.TrackingNumber}";

						try
						{
							HttpResponseMessage response = await httpClient.GetAsync(url);
							response.EnsureSuccessStatusCode();

							// Process the response
							string stringResult = await response.Content.ReadAsStringAsync();
							Result<EeppProposal> deserializedResult = JsonConvert.DeserializeObject<Result<EeppProposal>>(stringResult);

							if (deserializedResult != null && deserializedResult.Data != null && deserializedResult.Data.Id != -1)
							{
								AttachmentDto doaDoc = model.PostSubmittalAttachments.FirstOrDefault(x => x.AttachmentType == AttachmentType.DelegationOfAuthority);
								if (doaDoc != null)
								{
									doaDoc.Name = WebConstants.ATTACHMENT_FROM_EEPP;
									doaDoc.UploadedBy = WebConstants.ATTACHMENT_UPLOADED_BY_EEPP;
									doaDoc.IsAttachmentFromeEPP = true;

									// We need this to bypass the FileHasBeenUploaded flag in the UI
									// We'll also utilize the Id field to send over to eEPP for the proposal ID there
									doaDoc.Id = deserializedResult.Data.Id;
									doaDoc.EeppStatus = deserializedResult.Data.Status;
								}
							}
							// Do nothing (let the user upload a file) if there is no eEPP data with the tracking number
						}
						catch (HttpRequestException ex)
						{
							this.log.Error(ex);
						}
					}
				}
			}

            FullProposal prop = this.psaLogic.GetFullProposalDto(proposalId);

            this.ViewBag.IsRevision = prop.IsRevision;
            this.ViewBag.ReadOnly = prop.ProposalStatus == ProposalStatus.Revised;

            return this.View(WebConstants.View.POST_SUBMITTAL_ATTACHMENTS, model);
        }

		/// <summary>
		/// This method can be used to stream a text file to the browser that contains a series of error messages separated
		/// by newlines. This is a quick and easy way to alert the user that there was a problem (e.g. an exception thrown)
		/// during file download processing.
		/// </summary>
		/// <param name="errorMessages">List of error messages</param>
		/// <returns>Text file</returns>
		protected ActionResult CreateTextFileWithErrorMessage(params string[] errorMessages)
		{
			this.Response.ClearHeaders();
			this.Response.ClearContent();
			this.Response.Clear();

			this.Response.ContentType = "text/plain";
			this.Response.ContentEncoding = Encoding.ASCII;
			this.Response.AppendHeader("Content-Disposition", "attachment;filename=error.txt");

			byte[] newline = Encoding.ASCII.GetBytes("\r\n");

			if (errorMessages != null)
			{
				foreach (string errorMessage in errorMessages)
				{
					byte[] errorContent = Encoding.ASCII.GetBytes(errorMessage);
					this.Response.OutputStream.Write(errorContent, 0, errorContent.Length);
					this.Response.OutputStream.Write(newline, 0, newline.Length);
				}
			}

			this.Response.OutputStream.Flush();

			return new EmptyResult();
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
		/// <param name="fileId">File ID, or the proposal ID (in the case of an eEPP doc)</param>
		/// <param name="isFromeEPP">Is the file from eEPP?</param>
		/// <returns>File to download</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public FileResult DownloadAttachment(int proposalId, int fileId, bool isFromeEPP)
        {
			if (isFromeEPP)
			{
				try
				{
					MemoryStream ms = psaLogic.ProcesseEPPAttachment(fileId);
					return this.File(ms.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf, WebConstants.ATTACHMENT_FROM_EEPP + ".pdf");
				}
				catch (Exception ex)
				{
					string[] errors = { "An error occurred downloading the file from eEPP: " + ex.Message };
					return this.CreateTextFileWithErrorMessage(errors) as FileResult;
				}
			}
			else
			{
				AttachmentDto attachment = this.psaLogic.DownloadAttachment(fileId);
				return this.File(attachment.Contents, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.Name);
			}
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