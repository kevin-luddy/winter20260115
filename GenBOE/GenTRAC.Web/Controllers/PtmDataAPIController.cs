// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Runtime.Remoting.Messaging;
	using System.Web.Http;
	using GenTRAC.DataBridge.Common.Security;
	using GenTRAC.DataBridge.DTO;
	using GenTRAC.DataBridge.DTO.Permission;
	using GenTRAC.Objects;
	using GenTRAC.Objects.FullObject;
	using GenTRAC.Web.ModelView;
	using IES.Common;

	/// <summary>
	/// PTM Data Controller, original intent is for it to be used by ACV to pull data in, but realistically, it is serving up PTM data, hence the name.
	/// </summary>
	[AllowAnonymous]
	public class PtmDataAPIController : ApiController
	{
		#region Properties & Ctor

		/// <summary>
		/// Security Information
		/// </summary>
		private ISecurityInformation security;

		/// <summary>
		/// Security Access
		/// </summary>
		private ISecurityAccess securityAccess;

		/// <summary>
		/// Proposal Loader
		/// </summary>
		private IProposalLoader proposalLoader;

		/// <summary>
		/// Cover Sheet Loader
		/// </summary>
		private ICoverSheetDataLoader coverSheetLoader;

		/// <summary>
		/// The user mapper
		/// </summary>
		private IUserMapper userMapper { get; set; }

		/// <summary>
		/// Object Factory
		/// </summary>
		private IFullObjectFactory objectFactory { get; set; }

		/// <summary>
		/// Contracts Loader
		/// </summary>
		private IContractsLoader contractsLoader { get; set; }

		/// <summary>
		/// Token Handling
		/// </summary>
		private TokenHandling tokenHandler;

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("PtmDataAPIController");

		/// <summary>
		/// Ctor
		/// </summary>
		public PtmDataAPIController(ISecurityInformation security, IProposalLoader loader, ISecurityAccess securityAccess, TokenHandling tokenHandler, ICoverSheetDataLoader coverSheetLoader,
			IFullObjectFactory objectFactory, IUserMapper userMapper, IContractsLoader contractsLoader)
		{
			this.security = security;
			this.proposalLoader = loader;
			this.coverSheetLoader = coverSheetLoader;
			this.securityAccess = securityAccess;
			this.tokenHandler = tokenHandler;
			this.objectFactory = objectFactory;
			this.userMapper = userMapper;
			this.contractsLoader = contractsLoader;
		}

		#endregion

		/// <summary>
		/// Get Proposal data for ACV. Limits the number of records returned to 100.
		/// </summary>
		/// <param name="searchString">Search String</param>
		/// <returns>Proposal Data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<AcvProposalData> GetProposalDataForCostVolume(string searchString)
		{
			IESResponse<AcvProposalData> result = new IESResponse<AcvProposalData>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				bool isAdmin = this.securityAccess.CurrentUserHasRole(PtmRole.Admin, null);
				ICollection<(string PtmTrackingNumber, string ProposalTitle, int ProposalId)> data = this.proposalLoader.GetCostVolumeProposalData(security.ActiveUserNTID, isAdmin, searchString);
				result.Data = data.Select(x => new AcvProposalData() { PtmTrackingNumber = x.PtmTrackingNumber, ProposalTitle = x.ProposalTitle, ProposalId = x.ProposalId }).ToList();
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Proposal data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Cover Sheet data.
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <returns>Cover Sheet Data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<CoverSheetDataDto> GetCoverSheetData(int proposalId)
		{
			IESResponse<CoverSheetDataDto> result = new IESResponse<CoverSheetDataDto>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				result.Data.Add(coverSheetLoader.GetCoverSheetDataById(proposalId));
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Cover Sheet data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get header data for ACV
		/// </summary>
		/// <param name="proposalId">Proposal ID</param>
		/// <returns>Header data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<AcvHeaderDataDto> GetHeaderData(int proposalId)
		{
			IESResponse<AcvHeaderDataDto> result = new IESResponse<AcvHeaderDataDto>();

			try
			{
				tokenHandler.AuthenticateUserFromAuthorizationToken();

				result.Data.Add(proposalLoader.GetAcvHeaderDataByProposalId(proposalId));
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Header data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Additional Proposal Data for NLF Export
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>PBOE Data</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<PBOEData> GetProposalDataForNlfExport(string ptmTrackingNumber)
		{
			IESResponse<PBOEData> result = new IESResponse<PBOEData>();
			PBOEData pboeData = new PBOEData();

			try
			{
				int proposalId = this.proposalLoader.GetIdByTrackingNumber(ptmTrackingNumber);

				if (proposalId > 0) 
				{
					ICollection<int> ids = new List<int> { proposalId };
					// POC's
					DataBridge.DTO.UserDTO contractsPocDto = new DataBridge.DTO.UserDTO();

					// Proposal Variables
					ProposalDto proposal = this.proposalLoader.GetByIds(ids).FirstOrDefault();
					FullProposal fullProposalDto = null;

					if (proposal != null)
					{
						fullProposalDto = this.objectFactory.CreateFullProposal(proposal);

						pboeData.ProposalTitle = proposal.ProposalTitle;
						pboeData.AgreementDate = proposal.AgreementDate;

						ContractsDto contract = contractsLoader.GetContractForProposal(proposalId);

						if (contract != null)
						{
							pboeData.ProposalSubmittalDate = contract.CustomerSubmittalDate;
						}

						// Get Contracts POC
						ProposalPermissionDto permissionsContractsPOC = fullProposalDto.Permissions.FirstOrDefault(x => x.Role == PtmRole.ContractsPOC);

						if (permissionsContractsPOC != null)
						{
							contractsPocDto = this.userMapper.GetById(permissionsContractsPOC.UserId);
						}
						else
						{
							contractsPocDto.DisplayName = "User not found";
							contractsPocDto.EmailAddress = string.Empty;
						}

						// Set Contracts Lead on PBOE Data DTO
						pboeData.ContractsLeadDisplayName = contractsPocDto.DisplayName;
						pboeData.ContractsLeadEmail = contractsPocDto.EmailAddress;
					}
				}
				

				result.Data.Add(pboeData);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured returning PBOE data: {ex.Message}");
				result.IsSuccessful = false;
			}

			return result;
		}

		/// <summary>
		/// Get Proposal Permissions for NLF for the user retrieved from the authorization token and optional ptm tracking number
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>Proposal Permissions for the current user</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<ICollection<ProposalRoleDto>> GetProposalPermissions(string ptmTrackingNumber = "")
		{
			IESResponse<ICollection<ProposalRoleDto>> result = new IESResponse<ICollection<ProposalRoleDto>>();

			try
			{
				string ntid = tokenHandler.AuthenticateUserFromAuthorizationToken();

				result.Data.Add(proposalLoader.GetProposalRolesForNlfByNtid(ntid, ptmTrackingNumber));
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured returning Proposal Permissions: {ex.Message}");
				result.IsSuccessful = false;
			}

			return result;
		}

		/// <summary>
		/// Get the Lead Estimator and Backup Estimator names from PTM given a PTM tracking number
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM tracking number</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
		public IESResponse<AcvPtmAuthors> GetEstimatorNamesByTrackingNumber(string ptmTrackingNumber)
		{
			IESResponse<AcvPtmAuthors> result = new IESResponse<AcvPtmAuthors>();
			ICollection<ProposalRoleDto> roles;

			try
			{
				roles = proposalLoader.GetEstimatorNames(ptmTrackingNumber);
				if (roles == null)
				{
					logger.Warn($"No users found for tracking number");
					result.Messages.Add("No users found for tracking number");
				}
				else
				{
					AcvPtmAuthors author = new AcvPtmAuthors();
					ProposalRoleDto backupPricer = roles.FirstOrDefault(r => r.Role == PtmRole.BackupPricer);
					ProposalRoleDto leadEstimator = roles.FirstOrDefault(r => r.Role == PtmRole.Pricer);
					author.BackupEstimator = backupPricer != null ? backupPricer.NTID : string.Empty;
					author.LeadEstimator = leadEstimator != null ? leadEstimator.NTID : string.Empty;

					result.Data.Add(author);
					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Error occurred attempting to retrieve estimator data by tracking number {ptmTrackingNumber}: {ex.Message}");
				result.IsSuccessful = false;
			}

			return result;
		}

		/// <summary>
		/// Is Service Alive?
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[HttpGet]
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