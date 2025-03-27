// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ControllerLogic.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Transactions;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using GenTRAC.DataBridge.DTO;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	public class WorkspaceSettingsControllerLogic
	{
		/// <summary>
		/// Security Information
		/// </summary>
		private ISecurityInformation _securityInformation { get; set; }

		/// <summary>
		/// Service for Workspace Controller
		/// </summary>
		private IWorkspaceControllerLogic workspaceControllerLogic { get; set; }

		/// <summary>
		/// WorkspaceDTODataLoader
		/// </summary>
		private IWorkspaceDTODataLoader workspaceDTODataLoader { get; set; }

		/// <summary>
		/// Proposal Loader
		/// </summary>
		private GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader { get; set; }

		/// <summary>
		/// BOE Pick List Mapper
		/// </summary>
		private BoePickListMapper boePickListMapper { get; set; }

		/// <summary>
		/// PTM Security Mapper
		/// </summary>
		private GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceControllerLogic"></param>
		/// <param name="workspaceDTODataLoader"></param>
		public WorkspaceSettingsControllerLogic(
			ISecurityInformation _securityInformation,
			IWorkspaceControllerLogic workspaceControllerLogic,
			IWorkspaceDTODataLoader workspaceDTODataLoader,
			BoePickListMapper boePickListMapper,
			GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader,
			GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper
		)
		{
			this._securityInformation = _securityInformation;
			this.workspaceControllerLogic = workspaceControllerLogic;
			this.workspaceDTODataLoader = workspaceDTODataLoader;
			this.boePickListMapper = boePickListMapper;
			this.proposalLoader = proposalLoader;
			this.ptmSecurityMapper = ptmSecurityMapper;
		}

		/// <summary>
		/// Logic to get Workspace Identification
		/// </summary>
		/// <param name="ws"></param>
		/// <returns></returns>
		public IWorkspaceIdentificationModelView GetWorkspaceIdentification(FullWorkspace ws)
		{
			IWorkspaceIdentificationModelView workspaceIdentificationModelView = this.workspaceControllerLogic.GetWorkspaceIdentificationModelView(ws);
			workspaceIdentificationModelView.ApplicationURL = new System.Uri(ConfigurationUtilities.GetAppSetting("ServerURL") + "/" + workspaceIdentificationModelView.ShortName);

			workspaceIdentificationModelView.HoursLabel = FullObjectHelper.HoursLabel(ws);
			workspaceIdentificationModelView.EnableSAP = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
			workspaceIdentificationModelView.ShowSAP = Utilities.ShowSAPForWorkspace(ws.CreationDate);

			ICollection<WorkspaceDTO> workspaceChecks = this.workspaceDTODataLoader.GetAllWsNamesForTrackingNumber(ws.TrackingNumber)
				.Where(x => x.CurrentPTMWorkspace).ToList();

			if (workspaceChecks.Count() == 1 && workspaceChecks.Any(x => x.Id != ws.Id))
			{
				workspaceIdentificationModelView.DoesPTMMultipleWorkspaces = true;
			} 
			else if (workspaceChecks.Count() > 1)
			{
				workspaceIdentificationModelView.DoesPTMMultipleWorkspaces = false;
			}

			workspaceIdentificationModelView.ContractTypes = boePickListMapper.GetPickListValues(PickListEnum.ContractType).PickLists
				.Where(p => p.IsActive || ws.SelectedContractTypes.Contains(p.Id))
				.Select(p => p.Id)
				.ToList();

			workspaceIdentificationModelView.SelectedContractTypes = ws.SelectedContractTypes;

			return workspaceIdentificationModelView;
		}

		/// <summary>
		/// Get Pick List Values
		/// </summary>
		/// <returns></returns>
		public ICollection<PickListDto> GetLineOfBusinessType()
		{
			return boePickListMapper.GetPickListValues(PickListEnum.LineOfBusiness).PickLists;
		}

		/// <summary>
		/// Get Proposal Class Options
		/// </summary>
		/// <returns></returns>
		public ICollection<PickListDto> GetProposalClassOptionList()
		{
			return boePickListMapper.GetPickListValues(PickListEnum.ProposalClass).PickLists;
		}

		/// <summary>
		/// Get Contract Type List
		/// </summary>
		/// <returns></returns>
		public ICollection<PickListDto> GetContractTypeOptionList()
		{
			return boePickListMapper.GetPickListValues(PickListEnum.ContractType).PickLists;
		}

		/// <summary>
		/// Get Tracking Number List
		/// </summary>
		/// <param name="trackingNumber"></param>
		/// <returns></returns>
		public ICollection<SelectListItem> GetTrackingNumberOptionList(string trackingNumber)
		{
			Collection<SelectListItem> trackingNumbers = new Collection<SelectListItem>();
			IReadOnlyCollection<GenTRAC.DataBridge.Common.Security.SecurityPermissionsResponse> roles = this.ptmSecurityMapper.GetRolesForLoggedInUser();
			bool isAdmin = roles.Any(r => r.AuthorizedRole == PtmRole.Admin);

			ICollection<ProposalDto> proposals = (isAdmin ? this.proposalLoader.GetAllSlim() : this.proposalLoader.GetProposalsByUser(this._securityInformation.ActiveUserNTID, true))
																					.Where(p => !p.IsForecastProposal && p.ProposalStatus != ProposalStatus.NoBid && p.ProposalStatus != ProposalStatus.Revised).ToList();

			if (!string.IsNullOrWhiteSpace(trackingNumber) && !proposals.Any(p => p.TrackingNumber == trackingNumber))
			{
				// Currently has a bad Tracking Number saved in DB, but we will let that slide
				trackingNumbers.Add(new SelectListItem
				{
					Text = trackingNumber,
					Value = trackingNumber
				});
			}

			foreach (ProposalDto proposal in proposals)
			{
				trackingNumbers.Add(new SelectListItem
				{
					Text = proposal.TrackingNumber + " - " + proposal.ProposalTitle,
					Value = proposal.TrackingNumber
				});
			}

			return trackingNumbers;
		}

		/// <summary>
		/// Gets the next Tracking Number details when workspace tracking number is updated
		/// </summary>
		/// <param name="trackingNumber"></param>
		/// <returns></returns>
		public RefreshPTMResponseData GetNextTrackingNumberRevision(string trackingNumber)
		{
			RefreshPTMResponseData response = new RefreshPTMResponseData();

			string nextRevision = this.GetNextTrackingNumber(trackingNumber);

			int proposalId = this.proposalLoader.GetIdByTrackingNumber(trackingNumber);
			if (proposalId > 0)
			{
				response.TrackingNumberRevision = nextRevision;
				ProposalDto proposal = this.proposalLoader.GetById(proposalId);
				response.RFPNumber = proposal.RFPNumber;
				response.Title = proposal.ProposalTitle;
				if (proposal.ContractTypeIds.Any())
				{
					List<int> contractTypeIds = new List<int>();
					foreach (int contractTypeId in proposal.ContractTypeIds)
					{
						int convertedContractTypeId = workspaceControllerLogic.ConvertPTMContractTypeId(contractTypeId);
						if (convertedContractTypeId > 0)
						{
							contractTypeIds.Add(convertedContractTypeId);
						}
					}
					response.ContractTypes = contractTypeIds.ToArray();
				}

				response.ProposalClassId = workspaceControllerLogic.ConvertPTMProposalClassId(proposal.ProposalClass);

				response.LOBId = workspaceControllerLogic.ConvertPTMLineOfBusiness(proposal.LineOfBusinessID);

				response.AnticipatedDeliveryDate = proposal.DeliveryDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR);

				response.RevisedSubmittalDate = proposal.RevisedSubmittalDate.HasValue
					? proposal.RevisedSubmittalDate.Value.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR)
					: string.Empty;

				// Default Template BOE switch to Yes if CCoPD is set to true
				response.UsingTemplateBoe = proposal.IsCCPDRequired.HasValue ? proposal.IsCCPDRequired.Value : false;

				response.IsSAPEnabledConfig = Utilities.IsSAPEnabledForSystem;
			}
			else
			{
				throw new GenValidationException("PTM Tracking Number is invalid. Please delete the current Tracking Number and choose another from the dropdown list.");
			}

			return response;
		}

		/// <summary>
		/// Gets the next tracking number
		/// </summary>
		/// <param name="trackingNumber"></param>
		/// <returns></returns>
		private string GetNextTrackingNumber(string trackingNumber)
		{
			string nextRevision = trackingNumber;
			ICollection<WorkspaceDTO> trackingNameData = this.workspaceDTODataLoader.GetAllWsNamesAndTrackingNumberInfo().Where(w => w.TrackingNumber == trackingNumber || w.Shortname.StartsWith(trackingNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
			if (trackingNameData.Any())
			{
				// extract revision numbers - short names should be of the format [TrackingNumber] or [TrackingNumber]_XX, where XX is the revision number
				IList<string> revisionStrings = trackingNameData.Where(x => x.Shortname.StartsWith(nextRevision + "_")).Select(x => x.Shortname.Substring(x.Shortname.IndexOf("_") + 1, 2)).ToList();
				IList<int> revisions = new List<int>();

				// confirm the extracted values are numbers and convert them to ints
				foreach (string revision in revisionStrings)
				{
					int revisionNumber;
					if (int.TryParse(revision, out revisionNumber))
					{
						revisions.Add(revisionNumber);
					}
				}

				// Get highest number or 0 if there are none
				int highestRevision = revisions.Any() ? revisions.OrderByDescending(x => x).First() : 0;

				nextRevision = nextRevision + "_" + (highestRevision + 1).ToString("00");
			}

			return nextRevision;
		}
	}
}
