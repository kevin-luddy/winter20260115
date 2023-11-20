// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
	using GenTRAC.Objects;
	using GenTRAC.Objects.FullObject;
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
        private IProposalLoader loader;

        /// <summary>
        /// Cover Sheet Loader
        /// </summary>
        private ICoverSheetDataLoader coverSheetLoader;

		/// <summary>
		/// Proposal Checklist Loader
		/// </summary>
		private IProposalChecklistLoader proposalChecklistLoader;

		/// <summary>
		/// Proposal Loader
		/// </summary>
		private IProposalLoader proposalLoader;

		/// <summary>
		/// The user mapper
		/// </summary>
		private IUserMapper userMapper { get; set; }

		/// <summary>
		/// Object Factory
		/// </summary>
		private IFullObjectFactory objectFactory { get; set; }

		/// <summary>
		/// Workspace Data Loader
		/// </summary>
		private IWorkspaceDTODataLoader workspaceDataLoader { get; set; }

		/// <summary>
		/// User Data Loader
		/// </summary>
		private IUserDTODataLoader userDataLoader { get; set; }

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
			IProposalChecklistLoader proposalChecklistLoader, IProposalLoader proposalLoader, IFullObjectFactory objectFactory, IUserMapper userMapper, IWorkspaceDTODataLoader workspaceDataLoader,
			IUserDTODataLoader userDataLoader)
        {
            this.security = security;
            this.loader = loader;
            this.coverSheetLoader = coverSheetLoader;
            this.securityAccess = securityAccess;
            this.tokenHandler = tokenHandler;
			this.proposalChecklistLoader = proposalChecklistLoader;
			this.proposalLoader = proposalLoader;
			this.objectFactory = objectFactory;
			this.userMapper = userMapper;
			this.workspaceDataLoader = workspaceDataLoader;
			this.userDataLoader = userDataLoader;
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
                ICollection<(string PtmTrackingNumber, string ProposalTitle, int ProposalId)> data = this.loader.GetCostVolumeProposalData(security.ActiveUserNTID, isAdmin, searchString);
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

                result.Data.Add(loader.GetAcvHeaderDataByProposalId(proposalId));
                result.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                result.Messages.Add($"Unknown error occurred returning Header data: {ex.Message}");
            }

            return result;
        }

		[HttpGet]
		public IESResponse<PBOEDataDTO> GetProposalDataForNlfExport(string ptmTrackingNumber)
		{
			IESResponse<PBOEDataDTO> result = new IESResponse<PBOEDataDTO>();

			try
			{
				ICollection<int> proposalId = new List<int> { this.proposalLoader.GetIdByTrackingNumber(ptmTrackingNumber) };

                // POC's
                DataBridge.DTO.UserDTO contractsPocDto = new DataBridge.DTO.UserDTO();
				GenBOE.Dtos.UserDTO leadEstimatorPocDto = new GenBOE.Dtos.UserDTO();

				// Proposal Variables
				ProposalDto proposal = this.proposalLoader.GetByIds(proposalId).FirstOrDefault();
				PBOEDataDTO pBOEDataDTO = new PBOEDataDTO();
				FullProposal fullProposalDto = this.objectFactory.CreateFullProposal(proposal);

				// Workspace DTO
				WorkspaceDTO workspaceDto = new WorkspaceDTO();

				pBOEDataDTO.ProposalTitle = proposal.ProposalTitle;
				pBOEDataDTO.ProposalSubmittalDate = proposalChecklistLoader.GetProposalSubmittalDate(proposalId).FirstOrDefault().Value;

				// Get Contracts POC
				ProposalPermissionDto permissionsContractsPOC = fullProposalDto.Permissions.FirstOrDefault(x => x.Role == PtmRole.ContractsPOC);

				if (permissionsContractsPOC != null)
				{
					contractsPocDto = this.userMapper.GetById(permissionsContractsPOC.Id);
				}
				else
				{
					contractsPocDto.DisplayName = "User not found";
					contractsPocDto.EmailAddress = string.Empty;
				}

				// Set Contracts Lead on PBOE Data DTO
				pBOEDataDTO.ContractsLeadDisplayName = contractsPocDto.DisplayName;
				pBOEDataDTO.ContractsLeadEmail = contractsPocDto.EmailAddress;

				// Get Lead Estimator POC
				workspaceDto = this.workspaceDataLoader.GetAllWsNamesAndTrackingNumberInfo().Where(x => x.TrackingNumber == ptmTrackingNumber).FirstOrDefault();

				if (workspaceDto != null)
				{
					leadEstimatorPocDto = this.userDataLoader.GetUserByID(workspaceDto.CostVolumeLeadPricerUserID);

					if (leadEstimatorPocDto != null)
					{
						pBOEDataDTO.LeadEstimatorDisplayName = leadEstimatorPocDto.DisplayName;
						pBOEDataDTO.LeadEstimatorEmail = leadEstimatorPocDto.EmailAddress;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured returning PBOEDataDTO data: {ex.Message}");
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