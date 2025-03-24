// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Security.Policy;
	using System.Threading.Tasks;
	using System.Web;
    using System.Web.Mvc;
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.EMMA;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Presentation;
	using DocumentFormat.OpenXml.Wordprocessing;
	using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView.PostSubmittalAttachments;
    using GenTRAC.DataBridge.DTO;
	using GenTRAC.Objects.FullObject;
    using GenTRAC.Web.Common;
	using Glimpse.AspNet.Model;
	using IES.ActionLogic.Common;
	using IES.Common;
    using IES.Common.Exceptions;
	using Newtonsoft.Json;
	using Aspose.Html;
	using Aspose.Html.Converters;
	using Aspose.Html.IO;
	using Aspose.Html.Saving;

	//using static System.Net.Mime.MediaTypeNames;
	//using static System.Net.WebRequestMethods;

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

		///// <summary>
		///// Token Service
		///// </summary>
		//private readonly ITokenService tokenService;

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
			//ITokenService tokenService)
            : base(securityInformation, genTRACControllerLogic, siteMasterUtilities)
        {
            this.psaLogic = postSubmittalAttachmentsControllerLogic;
            this.securityInformation = securityInformation;
			this.proposalLoader = proposalLoader;
			//this.tokenService = tokenService;
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
                MaxOtherFileCount = SiteMasterUtilities.MaxOtherFileCount
            };

			// If there is an attachment of Delegation of Authority that already exists, show the current upload in the UI
			if (model.PostSubmittalAttachments.Any(x => x.AttachmentType == AttachmentType.DelegationOfAuthority && x.FileHasBeenUploaded))
			{
				AttachmentDto attachment = model.PostSubmittalAttachments.First(x => x.AttachmentType == AttachmentType.DelegationOfAuthority);
				attachment.ShowPTMUploadForDelegationOfAuthority = true;
			}
			// If unclassified and there is no existing attachment thru PTM, check the status of the associated eEPP record linked to the PTM tracking number, if any
			else
			{
				if (!SiteMasterUtilities.IsClassEnvironment)
				{
					ProposalDto proposal = this.proposalLoader.GetById(model.ProposalId);
					if (proposal != null)
					{
						// Check if this exists in eEPP
						// WIP - NEED TO FIX
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
								Result<int> deserializedResult = JsonConvert.DeserializeObject<Result<int>>(stringResult);

								if (deserializedResult != null && deserializedResult.Data != -1)
								{
									AttachmentDto doaDoc = model.PostSubmittalAttachments.FirstOrDefault(x => x.AttachmentType == AttachmentType.DelegationOfAuthority);
									if (doaDoc != null)
									{
										doaDoc.Name = WebConstants.ATTACHMENT_FROM_EEPP;
										doaDoc.UploadedBy = WebConstants.ATTACHMENT_UPLOADED_BY_EEPP;
										doaDoc.IsAttachmentFromeEPP = true;
										doaDoc.Id = 1;  // We need this to bypass the FileHasBeenUploaded flag in the UI, but the actual ID won't matter for eEPP docs
									}
								}
								// Do nothing (let the user upload a file) if there is no eEPP data with the tracking number
							}
							catch (HttpRequestException ex)
							{
								this.log.Error(ex);
							}
						}




						/*try
						{
							Result<byte[]> tempResult = new Result<byte[]>();
							tempResult.Messages.Add("This is a test.");

							AttachmentDto doaDoc = model.PostSubmittalAttachments.FirstOrDefault(x => x.AttachmentType == AttachmentType.DelegationOfAuthority);
							if (doaDoc != null)
							{
								doaDoc.Name = WebConstants.ATTACHMENT_FROM_EEPP;
								doaDoc.UploadedBy = WebConstants.ATTACHMENT_UPLOADED_BY_EEPP;
								doaDoc.IsAttachmentFromeEPP = true;
								doaDoc.Id = 1;  // We need this to bypass the FileHasBeenUploaded flag in the UI, but the actual ID won't matter for eEPP docs
							}
						}
						catch (Exception ex)
						{
							this.log.Error(ex);
							Result<byte[]> tempResult = new Result<byte[]>();
							MemoryStream stream = new MemoryStream();
							tempResult.Messages.Add("The requested action could not be completed. If the problem persists, please contact your application administrator.");
							HelperCreateErrorDocument(tempResult, stream);
						}*/

						


						//try
						//{
						//syncAPICall.Wait();
						// TODO KATIE: getting 401'd here
						//HttpResponseMessage response = syncAPICall.Result;

						/*Result<byte[]> tempResult = new Result<byte[]>();
						MemoryStream stream = new MemoryStream();
						tempResult.Messages.Add("This is a test.");
						HelperCreateErrorDocument(tempResult, stream);

						AttachmentDto doaDoc = model.PostSubmittalAttachments.Where(x => x.AttachmentType == AttachmentType.DelegationOfAuthority).FirstOrDefault();
						doaDoc.Name = WebConstants.ATTACHMENT_FROM_EEPP;
						doaDoc.Contents = stream.ToArray();	// This may not be needed if this is being handled in the Logic
						doaDoc.UploadedBy = WebConstants.ATTACHMENT_UPLOADED_BY_EEPP;
						doaDoc.IsAttachmentFromeEPP = true;
						// Using -1 for the ID will be used as a flag for retrieving the document
						doaDoc.Id = 1;*/
						//AttachmentDto attachment = new AttachmentDto()
						//{
						//	Name = WebConstants.ATTACHMENT_FROM_EEPP,
						//	Contents = ,
						//	UploadedBy = WebConstants.ATTACHMENT_UPLOADED_BY_EEPP,
						//	AttachmentType = AttachmentType.DelegationOfAuthority,
						//	ProposalId = model.ProposalId,
						//	IsRevisionReference = false,
						//	IsAttachmentFromeEPP = true,
						//	ShowPTMUploadForDelegationOfAuthority = false
						//};

						//}
						//catch (Exception ex)
						//{
						//	this.log.Error(ex);
						//	Result<byte[]> tempResult = new Result<byte[]>();
						//	MemoryStream stream = new MemoryStream();
						//	tempResult.Messages.Add("The requested action could not be completed. If the problem persists, please contact your application administrator.");
						//	HelperCreateErrorDocument(tempResult, stream);
						//}

						/*if ()
						{
							AttachmentDto attachment = new AttachmentDto()
							{
								Name = WebConstants.ATTACHMENT_FROM_EEPP,
								Contents = ,
								UploadedBy = WebConstants.ATTACHMENT_UPLOADED_BY_EEPP,
								AttachmentType = AttachmentType.DelegationOfAuthority,
								ProposalId = model.ProposalId,
								IsRevisionReference = false,
								IsAttachmentFromeEPP = true,
								ShowPTMUploadForDelegationOfAuthority = false
							};
						}
						// The record does not exist in eEPP, so keep the upload button
						else
						{
							// TODO Katie: Do we set anything here?
						}*/
					}
				}
			}

            FullProposal prop = this.psaLogic.GetFullProposalDto(proposalId);

            this.ViewBag.IsRevision = prop.IsRevision;
            this.ViewBag.ReadOnly = prop.ProposalStatus == ProposalStatus.Revised;

            return this.View(WebConstants.View.POST_SUBMITTAL_ATTACHMENTS, model);
        }

		/// <summary>
		/// Helper for creating a document to return an error message to the user. Returns the result as a byte stream.
		/// </summary>
		/// <param name="deserializedResult">Failed http response</param>
		/// <param name="stream">Stream into which to write the exported error Word document.</param>
		private void HelperCreateErrorDocument(Result<byte[]> deserializedResult, Stream stream)
		{
			string tempErrorFilename = Path.GetTempFileName();
			lock (CacheConstants.OPEN_XML_LOCK)
			{
				using (WordprocessingDocument errorDocument = WordprocessingDocument.Create(tempErrorFilename, WordprocessingDocumentType.Document))
				{
					MainDocumentPart mainPart = errorDocument.AddMainDocumentPart();
					mainPart.Document = new Document();
					Body body = mainPart.Document.AppendChild(new Body());
					foreach (string message in deserializedResult.Messages)
					{
						Paragraph para = body.AppendChild(new Paragraph());
						Run run = para.AppendChild(new Run());
						run.AppendChild(new DocumentFormat.OpenXml.Drawing.Text(message));
					}
				}
			}
			using (Stream errorStream = new FileStream(tempErrorFilename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose))
			{
				errorStream.Seek(0, SeekOrigin.Begin);
				errorStream.CopyTo(stream);
			}
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
		/// <param name="isFromeEPP">Is the file from eEPP?</param>
		/// <returns>File to download</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		public FileResult DownloadAttachment(int proposalId, int fileId, bool isFromeEPP)
        {
			if (isFromeEPP)
			{
				// Error handling - NEEDS TESTING
				//MemoryStream stream = new MemoryStream();
				//Result<byte[]> tempResult = new Result<byte[]>();
				//HelperCreateErrorDocument(tempResult, stream);

				//return this.File(stream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Octet, "error.docx");
				// END ERROR HANDLING


				// HTML download code
				/*string address = "https://localhost:44386/Print?proposalId=221"; // TODO KATIE: Change the address later
				WebClient client = new WebClient();
				Uri uri = new Uri(address);
				byte[] fileBytes = null; // assign bytesreturn File(fileBytes, "application/octet-stream");
				client.UseDefaultCredentials = true;


				fileBytes = client.DownloadData(uri);
				return this.File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, WebConstants.ATTACHMENT_FROM_EEPP + ".html");*/

				//return this.File(, System.Net.Mime.MediaTypeNames.Application.Octet, WebConstants.ATTACHMENT_FROM_EEPP);





				MemoryStream ms = psaLogic.ProcesseEPPAttachment();
				byte[] eeppBytes = new byte[ms.Length];

				ms.Read(eeppBytes, 0, (int)ms.Length);
				ms.Write(eeppBytes, 0, eeppBytes.Length);

				return this.File(eeppBytes, System.Net.Mime.MediaTypeNames.Application.Octet, WebConstants.ATTACHMENT_FROM_EEPP + ".pdf");



				// This commented block is moved to the logic, need to test
				/* HTMLDocument document = new HTMLDocument("https://localhost:44386/Print?proposalId=221");
				PdfSaveOptions options = new PdfSaveOptions();
				string tempFileName = Path.GetRandomFileName() + ".pdf";
				string tempFileLocation = Path.Combine(Server.MapPath("~/Templates/Export"), tempFileName);
				Converter.ConvertHTML(document, options, tempFileName);

				// Return the file
				//using (FileStream fs = new FileStream(WebConstants.ATTACHMENT_FROM_EEPP + ".pdf", FileMode.Open, FileAccess.Read))
				//{
				//	byte[] bytes = new byte[fs.Length];
				//	fs.Read(bytes, 0, (int)fs.Length);
				//	fs.Write(bytes, 0, bytes.Length);

				//}
				MemoryStream ms = new MemoryStream();
				using (FileStream file = new FileStream(tempFileLocation, FileMode.Open, FileAccess.Read))
				{
					file.CopyTo(ms);
				} */

				// MemoryStream attempt
				//MultipartMemoryStreamProvider streamProvider = new MultipartMemoryStreamProvider();
				//Converter.ConvertHTML(document, options, (ICreateStreamProvider)streamProvider);

				// Get access to the memory stream that contains the result data
				//return this.File(ms, System.Net.Mime.MediaTypeNames.Application.Octet, WebConstants.ATTACHMENT_FROM_EEPP + ".pdf");
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