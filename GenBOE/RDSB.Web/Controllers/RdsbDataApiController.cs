// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web;
	using System.Web.Http;
	using GenTRAC.DataBridge.Common.Security;
	using IES.ActionLogic.ControllerLogic;
	using IES.Common;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// RDSB Data API Controller - used to serve up RDSB data for ACV (or other applications as needed)
	/// </summary>
	[AllowAnonymous]
	public class RdsbDataApiController : ApiController
	{
		#region Properties & Ctor

		/// <summary>
		/// PTM Security Mapper
		/// </summary>
		private ISecurityMapper securityMapper;

		/// <summary>
		/// Token Handling
		/// </summary>
		private TokenHandling tokenHandler;

		/// <summary>
		/// Document Controller Logic
		/// </summary>
		private IDocumentControllerLogic documentControllerLogic;

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("RdsbDataAPIController");

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="securityMapper">PTM Security Mapper</param>
		/// <param name="tokenHandler">Token Handling</param>
		/// <param name="documentControllerLogic">Document COntroller Logic</param>
		public RdsbDataApiController(ISecurityMapper securityMapper, TokenHandling tokenHandler, IDocumentControllerLogic documentControllerLogic)
		{
			this.securityMapper = securityMapper;
			this.tokenHandler = tokenHandler;
			this.documentControllerLogic = documentControllerLogic;
		}

		#endregion

		/// <summary>
		/// Check if an RDSB record exists for the given PTM Tracking Number
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>true if record exists, otherwise false</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<bool> DoesRdsbRecordExist(int proposalId)
		{
			IESResponse<bool> toReturn = new IESResponse<bool>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				toReturn.Data = new Collection<bool>() { documentControllerLogic.DoesRdsbRecordExistForProposalId(proposalId) };
				toReturn.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				toReturn.Messages.Add($"Error occurred checking for RDSB Record: {ex.Message}");
			}

			return toReturn;
		}

		/// <summary>
		/// Export the RDSB Document
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <param name="parentSectionNumber">Parent Section Number</param>
		/// <returns>RDSB Document in HTTP Response Message</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public HttpResponseMessage ExportRdsbDocument(int proposalId, string parentSectionNumber)
		{
			HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

			try
			{
				// validate token and check permissions
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				ICollection<SecurityPermissionsResponse> roles = this.securityMapper.GetRolesForLoggedInUser().ToList();
				if (!roles.Any(x => x.ProposalID == proposalId || x.AuthorizedRole == PtmRole.Admin))
				{
					return new HttpResponseMessage(HttpStatusCode.Unauthorized);
				}

				// perform export
				HttpResponse response = HttpContext.Current.Response;
				string serverFileName = HttpContext.Current.Server.MapPath("~/Templates/Export/PPRDTemplateACV.docx");
				this.documentControllerLogic.GenerateRDD(proposalId, serverFileName, new HttpResponseWrapper(response), parentSectionNumber, false);

				response.OutputStream.Position = 0;
				responseMessage.Content = new StreamContent(response.OutputStream);
				responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(ExportFileDownloadBase.ContentType_DOCX);
				responseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
				{
					FileName = $"RDSB-Export-ProposalId{proposalId}.docx"
				};
			}
			catch (Exception ex)
			{
				this.logger.Error(ex);
				responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			return responseMessage;
		}

		/// <summary>
		/// Is Service Alive?
		/// </summary>
		/// <returns>True/false</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool IsAlive()
		{
			bool result;

			try
			{
				// ToDo: add a DB grab, just to see if the DB is working.. To make the check more meaningful
				result = true;
			}
			catch
			{
				result = false;
			}

			return result;
		}
	}
}