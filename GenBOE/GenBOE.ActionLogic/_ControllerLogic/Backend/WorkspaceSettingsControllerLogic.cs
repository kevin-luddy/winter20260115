// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ControllerLogic.Backend
{
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.PickList;

	public class WorkspaceSettingsControllerLogic
	{
		/// <summary>
		/// Service for Workspace Controller
		/// </summary>
		private IWorkspaceControllerLogic workspaceControllerLogic { get; set; }

		/// <summary>
		/// WorkspaceDTODataLoader
		/// </summary>
		private IWorkspaceDTODataLoader workspaceDTODataLoader { get; set; }

		/// <summary>
		/// BOE Pick List Mapper
		/// </summary>
		private BoePickListMapper boePickListMapper { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceControllerLogic"></param>
		/// <param name="workspaceDTODataLoader"></param>
		public WorkspaceSettingsControllerLogic(
			IWorkspaceControllerLogic workspaceControllerLogic, 
			IWorkspaceDTODataLoader workspaceDTODataLoader,
			BoePickListMapper boePickListMapper
		)
		{
			this.workspaceControllerLogic = workspaceControllerLogic;
			this.workspaceDTODataLoader = workspaceDTODataLoader;
			this.boePickListMapper = boePickListMapper;
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
	}
}
