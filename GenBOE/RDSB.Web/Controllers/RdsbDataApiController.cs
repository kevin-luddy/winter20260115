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
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
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
	using IES.DataBridge.ModelViews;

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
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
				toReturn.Messages.Add($"Error occurred while checking for RDSB Record.");
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
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
				string serverFileName = HttpContext.Current.Server.MapPath("~/Templates/Export/PPRDTemplate.docx");
				MemoryStream stream = new MemoryStream();
				this.documentControllerLogic.GenerateRDD(proposalId, serverFileName, stream, null, parentSectionNumber, false);

				stream.Position = 0;
				responseMessage.Content = new StreamContent(stream);
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
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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

		/// <summary>
		/// Gets data necessary for automation of a coversheet. Specifically sections that contain 1) CASB and 2) Non-Compliance data
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a Cover Sheet creation</returns>
		[HttpGet]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<(string CasbSection, string NonComplianceSection)> GetCoverSheetData(int proposalId)
		{
			IESResponse<(string CasbSection, string NonComplianceSection)> toReturn = new IESResponse<(string CasbSection, string NonComplianceSection)>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				toReturn.Data = new List<(string CasbSection, string NonComplianceSection)>() { documentControllerLogic.GetCoverSheetData(proposalId) };
				toReturn.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				toReturn.Messages.Add($"Error occurred while retrieving data from RDSB");
			}

			return toReturn;
		}

		/// <summary>
		/// Gets data necessary for CPS Reports
		/// </summary>
		/// <param name="rateCodes">List of rate codes</param>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a CPS Report</returns>
		[HttpGet]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<(string rateCode, string parentSectionNumber)> GetSectionsForRateCodes(ICollection<string> rateCodes, int proposalId)
		{
			IESResponse<(string rateCode, string parentSectionNumber)> toReturn = new IESResponse<(string rateCode, string parentSectionNumber)>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				toReturn.Data = documentControllerLogic.GetTopLevelSectionsForRateCodes(rateCodes, proposalId);
				toReturn.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				toReturn.Messages.Add($"Error occurred while retrieving data from RDSB");
			}

			return toReturn;
		}

		/// <summary>
		/// Gets data necessary for CPS Reports
		/// </summary>
		/// <param name="rateDescriptions">List of rate descriptions</param>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a CPS Report</returns>
		[HttpGet]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<(string rateDescription, string parentSectionNumber)> GetSectionsForRateDescriptions(ICollection<string> rateDescriptions, int proposalId)
		{
			IESResponse<(string rateDescription, string parentSectionNumber)> toReturn = new IESResponse<(string rateDescription, string parentSectionNumber)>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				toReturn.Data = documentControllerLogic.GetTopLevelSectionsForRateDescriptions(rateDescriptions, proposalId);
				toReturn.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				toReturn.Messages.Add($"Error occurred while retrieving data from RDSB");
			}

			return toReturn;
		}

		/// <summary>
		/// Get all of the addresses based on restricting it to the Include In Cover Sheet property and for the specific PPR&D version
		/// </summary>
		/// <param name="revision">The specific version ID of PPR&D</param>
		/// <returns>A collection of addresses</returns>
		[HttpGet]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<SectionAddressModelView> GetAddresses(int revision)
		{
			IESResponse<SectionAddressModelView> addresses = new IESResponse<SectionAddressModelView>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				addresses.Data = this.documentControllerLogic.GetAddresses(revision);
				addresses.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				addresses.Messages.Add($"Error occurred while retrieving address data from RDSB");
			}

			return addresses;
		}
	}
}