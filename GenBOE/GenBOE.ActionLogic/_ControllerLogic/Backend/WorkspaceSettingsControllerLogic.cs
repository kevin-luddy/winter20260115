// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ControllerLogic.Backend
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenTRAC.DataBridge.DTO;
	using IES.Common;
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
	}
}
