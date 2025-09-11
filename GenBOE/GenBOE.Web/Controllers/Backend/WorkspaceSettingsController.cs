// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.Controllers.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Web.Http;
	using System.Web.Mvc;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.ActionLogic._ModelView.Backend;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.PickList;
	using HttpGetAttribute = System.Web.Http.HttpGetAttribute;

	/// <summary>
	/// Workspace Settings Controller
	/// </summary>
	public class WorkspaceSettingsController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Service for Workspace Controller
		/// </summary>
		private WorkspaceSettingsControllerLogic workspaceSettingsControllerLogic { get; set; }

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("WorkspaceSettingsController");

		/// <summary>
		/// ctor
		/// </summary>
		public WorkspaceSettingsController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader,
			WorkspaceSettingsControllerLogic workspaceSettingsControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.workspaceSettingsControllerLogic = workspaceSettingsControllerLogic;
		}
		#endregion

		/// <summary>
		/// Get the Workspace Identification for a specific Workspace
		/// </summary>
		/// <param name="workspaceShortName">workspace short name</param>
		/// <returns>Workspace Identification</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<IWorkspaceIdentificationModelView> GetWorkspaceIdentification(string workspaceShortName)
		{
			IESSingleResponse<IWorkspaceIdentificationModelView> result = new IESSingleResponse<IWorkspaceIdentificationModelView>();

			try
			{
				FullWorkspace ws = Factory.CreateFullWorkspace(workspaceShortName);

				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_DISPLAY_WORKSPACE_IDENTIFICATION, SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

				IWorkspaceIdentificationModelView workspaceIdentificationModelView = workspaceSettingsControllerLogic.GetWorkspaceIdentification(ws);
				result.Data = workspaceIdentificationModelView;
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_DISPLAY_WORKSPACE_IDENTIFICATION, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unkown error returning Workspace Identification: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Pick List Values
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<PickListDto> GetLineOfBusinessType()
		{
			IESResponse<PickListDto> result = new IESResponse<PickListDto>();

			try
			{
				result.Data = workspaceSettingsControllerLogic.GetLineOfBusinessType();
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Line of Business data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Proposal Class Options
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<PickListDto> GetProposalClassOptionList()
		{
			IESResponse<PickListDto> result = new IESResponse<PickListDto>();

			try
			{
				ICollection<PickListDto> proposalClassTypes = workspaceSettingsControllerLogic.GetProposalClassOptionList();
				result.Data = proposalClassTypes.Where(p => p.Id != Constants.PROPOSAL_CLASS_TYPE_NOT_SET).ToList();
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Line of Business data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Contract Type Options
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<PickListDto> GetContractTypeOptionList()
		{
			IESResponse<PickListDto> result = new IESResponse<PickListDto>();

			try
			{
				ICollection<PickListDto> contractTypes = workspaceSettingsControllerLogic.GetContractTypeOptionList();
				result.Data = contractTypes;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning list of Contract Types: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get Tracking Numbers
		/// </summary>
		/// <param name="trackingNumber"></param>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<SelectListItem> GetTrackingNumberOptionList(string trackingNumber)
		{
			IESResponse<SelectListItem> result = new IESResponse<SelectListItem>();
			if (trackingNumber != null && Utilities.IsPTMIntegrated)
			{
				try
				{
					ICollection<SelectListItem> trackingNumbers = workspaceSettingsControllerLogic.GetTrackingNumberOptionList(trackingNumber);
					result.Data = trackingNumbers;
				}
				catch (Exception ex)
				{
					logger.Error(ex);
					result.Messages.Add($"Unknown error occurred returning list of Tracking Numbers: {ex.Message}");
				}
			}
			return result;
		}

		/// <summary>
		/// Update Workspace Data when tracking number is updated
		/// </summary>
		/// <param name="trackingNumberRevisionModelView"></param>
		/// <returns></returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<RefreshPTMResponseData> GetNextTrackingNumberRevision([FromBody] TrackingNumberRevisionModelView trackingNumberRevisionModelView)
		{
			IESSingleResponse<RefreshPTMResponseData> result = new IESSingleResponse<RefreshPTMResponseData>();
			if (trackingNumberRevisionModelView != null && trackingNumberRevisionModelView.TrackingNumber != null)
			{
				try
				{
					RefreshPTMResponseData data = workspaceSettingsControllerLogic.GetNextTrackingNumberRevision(trackingNumberRevisionModelView.TrackingNumber);
					result.Data = data;
				}
				catch (Exception ex)
				{
					logger.Error(ex);
					result.Messages.Add($"Unknown error occurred GetNextTrackingNumberRevision: {ex.Message}");
				}
			}
			return result;
		}

		/// <summary>
		/// Save the Workspace Identification (Space)
		/// A separate Space API is needed because IWorkspaceIdentificationModelView is not compatible
		/// </summary>
		/// <param name="workspaceIdentificationModelView"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<ActionResult> SaveWorkspaceIdentificationSpace([FromBody] WorkspaceIdentificationSpaceModelView workspaceIdentificationModelView)
		{
			_ = workspaceIdentificationModelView ?? throw new ArgumentNullException(nameof(workspaceIdentificationModelView));

			IESSingleResponse<ActionResult> result = new IESSingleResponse<ActionResult>();

			FullWorkspace ws = Factory.CreateFullWorkspace(workspaceIdentificationModelView.ShortName);

			// Initialize Action
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_SAVE_WORKSPACE_IDENTIFICATION, SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);

			string returnMessage = string.Empty;

			try
			{
				returnMessage = this.workspaceSettingsControllerLogic.SaveWorkspaceIdentification(Factory, ws, workspaceIdentificationModelView);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred SaveWorkspaceIdentificationSpace: {ex.Message}");
			}

			result.IsSuccessful = true;
			result.Messages.Add(returnMessage);

			// Finalize Action
			FinalizeAction(logger, WebConstants.ACTION_SAVE_WORKSPACE_IDENTIFICATION, sw);

			return result;
		}

		/// <summary>
		/// Save the Workspace Identification (MST)
		/// A separate RMS API is needed because IWorkspaceIdentificationModelView is not compatible
		/// </summary>
		/// <param name="workspaceIdentificationModelView"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<ActionResult> SaveWorkspaceIdentificationMST([FromBody] WorkspaceIdentificationMSTModelView workspaceIdentificationModelView)
		{
			_ = workspaceIdentificationModelView ?? throw new ArgumentNullException(nameof(workspaceIdentificationModelView));

			IESSingleResponse<ActionResult> result = new IESSingleResponse<ActionResult>();

			FullWorkspace ws = Factory.CreateFullWorkspace(workspaceIdentificationModelView.ShortName);

			// Initialize Action
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_SAVE_WORKSPACE_IDENTIFICATION, SecurityPage.WorkspaceSettings, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);

			string returnMessage = string.Empty;

			try
			{
				returnMessage = this.workspaceSettingsControllerLogic.SaveWorkspaceIdentification(Factory, ws, workspaceIdentificationModelView);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred SaveWorkspaceIdentificationMST: {ex.Message}");
			}

			result.IsSuccessful = true;
			result.Messages.Add(returnMessage);

			// Finalize Action
			FinalizeAction(logger, WebConstants.ACTION_SAVE_WORKSPACE_IDENTIFICATION, sw);

			return result;
		}

		/// <summary>
		/// Get the Workspace Status History Grid
		/// </summary>
		/// <param name="workspaceShortName">workspace shortname string</param>
		/// <returns>Workspace Status History Grid</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<WorkspaceStatusHistoryModelView> GetWorkspaceStatusHistory(string workspace)
		{
			IESResponse<WorkspaceStatusHistoryModelView> result = new IESResponse<WorkspaceStatusHistoryModelView>();

			try
			{
				FullWorkspace ws = Factory.CreateFullWorkspace(workspace);

				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_DISPLAY_WORKSPACE_STATUS_HISTORY_GRID, SecurityPage.WorkspaceSettings, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

				ICollection<WorkspaceStatusHistoryModelView> workspaceStatusHistoryGrid = workspaceSettingsControllerLogic.GetWorkspaceStatusHistory(ws);
				result.Data = workspaceStatusHistoryGrid;
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_DISPLAY_WORKSPACE_STATUS_HISTORY_GRID, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unkown error returning Workspace Identification: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Save the Workspace Status
		/// </summary>
		/// <param name="workspaceShortname">Workspace Shortname</param>
		/// <returns>Workspace Status</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<WorkspaceStatusModelView> GetWorkspaceStatus(string workspaceShortname)
		{
			IESSingleResponse<WorkspaceStatusModelView> result = new IESSingleResponse<WorkspaceStatusModelView>();
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);

			// Initialize Action
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_WORKSPACE_STATUS, SecurityPage.WorkspaceSettingsStatus, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, null);

			try
			{
				result.Data = new WorkspaceStatusModelView(ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.GET_WORKSPACE_STATUS, sw);
			return result;
		}

		/// <summary>
		/// Get the Workspace Status
		/// </summary>
		/// <param name="workspaceStatusMV">Workspace Status Model View</param>
		/// <returns>True if Workspace Status is updated Successfully, False otherwise</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062: Validate arguments of public methods")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<bool> SaveWorkspaceStatus([FromBody] WorkspaceStatusModelView workspaceStatusMV)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();
			FullWorkspace ws = Factory.CreateFullWorkspace(workspaceStatusMV.WorkspaceName);

			// Initialize Action
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_SAVE_WORKSPACE_STATUS, SecurityPage.WorkspaceSettingsStatus, SecurityAuthorization.CreateReadUpdateDelete, new List<WorkspaceDTO> { ws }, null);

			try
			{
				result.Data = this.workspaceSettingsControllerLogic.SaveWorkspaceStatus(workspaceStatusMV, ws, logger);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.ACTION_SAVE_WORKSPACE_STATUS, sw);
			return result;
		}
	}
}