// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Threading.Tasks;
	using GenTRAC.DataBridge.Core.Common.Security;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.Common.Core;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.Common.Core.OfficeUtilities;
	using IES.Common.Core.Utilities;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using RDSB.Backend.Models;

	/// <summary>
	/// RDSB Data API Controller - used to serve up RDSB data for ACV (or other applications as needed)
	/// </summary>
	[Authorize(AuthenticationSchemes = CommonConstants.IES_TOKEN_SCHEME)]
	[Route("api/RdsbDataApi")]
	public class RdsbDataApiController : IESController
	{
		#region Properties & Ctor

		/// <summary>
		/// PTM Security Mapper
		/// </summary>
		private readonly ISecurityMapper securityMapper;

		/// <summary>
		/// Document Controller Logic
		/// </summary>
		private readonly IDocumentControllerLogic documentControllerLogic;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="securityMapper">PTM Security Mapper</param>
		/// <param name="tokenHandler">Token Handling</param>
		/// <param name="documentControllerLogic">Document COntroller Logic</param>
		public RdsbDataApiController(ISecurityMapper securityMapper, IDocumentControllerLogic documentControllerLogic,
			ILogger<RdsbDataApiController> logger, ISecurityInformation securityInformation, IConfiguration configuration) : base(logger, securityInformation, configuration)
		{
			this.securityMapper = securityMapper;
			this.documentControllerLogic = documentControllerLogic;
		}

		#endregion

		/// <summary>
		/// Check if an RDSB record exists for the given PTM Tracking Number
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>true if record exists, otherwise false</returns>
		[HttpGet("[action]")]
		public IESResponse<bool> DoesRdsbRecordExist(int proposalId)
		{
			IESResponse<bool> toReturn = new();

			try
			{
				toReturn.Data = documentControllerLogic.DoesRdsbRecordExistForProposalId(proposalId);
				toReturn.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				log.LogError(ex, "Error occurred while checking for RDSB Record.");
				toReturn.Messages.Add("Error occurred while checking for RDSB Record.");
			}

			return toReturn;
		}

		/// <summary>
		/// Export the RDSB Document
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <param name="parentSectionNumber">Parent Section Number</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		/// <returns>RDSB Document in HTTP Response Message</returns>
		[HttpGet("[action]")]
		public async Task<IActionResult> ExportRdsbDocument(int proposalId, string parentSectionNumber, bool portionMarkingRequired = false)
		{
			try
			{
				// perform export
				string serverFileName = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/PPRDTemplate.docx");
				Stream stream = await this.documentControllerLogic.GenerateRDD(proposalId, serverFileName, null, parentSectionNumber, false, portionMarkingRequired);
				stream.Position = 0;
				return new FileStreamResult(stream, ExportFileDownloadBase.ContentType_DOCX)
				{
					FileDownloadName = $"RDSB-Export-ProposalId{proposalId}.docx"
				};

			}
			catch (Exception ex)
			{
				this.log.LogError(ex, "Error exporting RDSB Document");
				return this.StatusCode((int)HttpStatusCode.InternalServerError);
			}
		}

		/// <summary>
		/// Is Service Alive?
		/// </summary>
		/// <returns>True/false</returns>
		[HttpGet("[action]")]
		[AllowAnonymous]
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
		/// Gets data necessary for automation of a coversheet. Specifically sections that contain 1) CASB, 2) Non-Compliance data, and 3) Disclosure Statements
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a Cover Sheet creation</returns>
		[HttpGet("[action]")]
		public IESResponse<ICollection<(string CasbSection, string NonComplianceSection, bool AdequateDisclosure, bool NoncomplianceNotification)>> GetCoverSheetData(int proposalId)
		{
			IESResponse<ICollection<(string CasbSection, string NonComplianceSection, bool AdequateDisclosure, bool NoncomplianceNotification)>> toReturn = new();

			try
			{
				toReturn.Data = new List<(string CasbSection, string NonComplianceSection, bool AdequateDisclosure, bool NoncomplianceNotification)>() {
					documentControllerLogic.GetCoverSheetData(proposalId) };
				toReturn.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				log.LogError(ex, "Error occurred while retrieving data from RDSB");
				toReturn.Messages.Add("Error occurred while retrieving data from RDSB");
			}

			return toReturn;
		}

		/// <summary>
		/// Gets data necessary for CPS Reports
		/// </summary>
		/// <param name="requestData">Request data for RDSB API.</param>
		/// <returns>Data to support a CPS Report</returns>
		[HttpPost("[action]")]
		public IESResponse<ICollection<string>> GetSectionsForRateCodes(RDSBRateCodesRequest requestData)
		{
			IESResponse<ICollection<string>> toReturn = new();

			try
			{
				if (requestData != null)
				{
					toReturn.Data = documentControllerLogic.GetTopLevelSectionsForRateCodes(requestData.RateCodes, requestData.PtmProposalId);
					toReturn.IsSuccessful = true;
				}
				else
				{
					toReturn.IsSuccessful = false;
				}
			}
			catch (Exception ex)
			{
				log.LogError(ex, "Error occurred while retrieving data from RDSB");
				toReturn.Messages.Add("Error occurred while retrieving data from RDSB");
			}

			return toReturn;
		}

		/// <summary>
		/// Gets data necessary for CPS Reports
		/// </summary>
		/// <param name="requestData">Request data for RDSB API.</param>
		/// <returns>Data to support a CPS Report</returns>
		[HttpPost("[action]")]
		public IESResponse<ICollection<string>> GetSectionsForRateDescriptions(RDSBRateDescriptionsRequest requestData)
		{
			IESResponse<ICollection<string>> toReturn = new();

			try
			{
				if (requestData != null)
				{
					toReturn.Data = documentControllerLogic.GetTopLevelSectionsForRateDescriptions(requestData.RateDescriptions, requestData.PtmProposalId);
					toReturn.IsSuccessful = true;
				}
				else
				{
					toReturn.IsSuccessful = false;
				}
			}
			catch (Exception ex)
			{
				log.LogError(ex, "Error occurred while retrieving data from RDSB");
				toReturn.Messages.Add("Error occurred while retrieving data from RDSB");
			}

			return toReturn;
		}

		/// <summary>
		/// Get all of the addresses based on restricting it to the Include In Cover Sheet property and for the specific PPR&D version
		/// </summary>
		/// <param name="ptmTrackingId">The PTM Tracking #/Proposal ID</param>
		/// <returns>A collection of addresses</returns>
		[HttpGet("[action]")]
		public IESResponse<ICollection<SectionAddressModelView>> GetAddresses(int ptmTrackingId)
		{
			IESResponse<ICollection<SectionAddressModelView>> addresses = new();

			try
			{
				addresses.Data = this.documentControllerLogic.GetAddresses(ptmTrackingId);
				addresses.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				log.LogError(ex, "Error occurred while retrieving address data from RDSB");
				addresses.Messages.Add("Error occurred while retrieving address data from RDSB");
			}

			return addresses;
		}
	}
}