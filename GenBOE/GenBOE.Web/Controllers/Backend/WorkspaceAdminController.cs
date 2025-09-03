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
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	/// <summary>
	/// Workspace Admin Controller
	/// </summary>
	public class WorkspaceAdminController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Contract Type loader
		/// </summary>
		private readonly ContractTypeLoader contractTypeLoader;

		/// <summary>
		/// Workspace Admin Controller Logic
		/// </summary>
		private readonly WorkspaceAdminControllerLogic workspaceAdminControllerLogic;

		/// <summary>
		/// BOE Controller Logic
		/// </summary>
		private readonly IBOEControllerLogic boeControllerLogic;

		/// <summary>
		/// logger
		/// </summary>
		private readonly Logger logger = new Logger("WorkspaceAdminController");

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="securityAccess"></param>
		/// <param name="factory"></param>
		/// <param name="userLoader"></param>
		/// <param name="permissionsLoader"></param>
		/// <param name="workspaceAdminControllerLogic"></param>
		/// <param name="contractTypeLoader"></param>
		/// <param name="boeControllerLogic"></param>
		public WorkspaceAdminController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, WorkspaceAdminControllerLogic workspaceAdminControllerLogic, ContractTypeLoader contractTypeLoader, IBOEControllerLogic boeControllerLogic)
	: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.workspaceAdminControllerLogic = workspaceAdminControllerLogic;
			this.contractTypeLoader = contractTypeLoader;
			this.boeControllerLogic = boeControllerLogic;
		}

		/// <summary>
		/// Get CLINS for Workspace in Manage CLINS page
		/// </summary>
		/// <param name="workspaceShortName"></param>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<ManageCLINGridModelView> GetManageCLINs(string workspaceShortName)
		{
			IESSingleResponse<ManageCLINGridModelView> result = new IESSingleResponse<ManageCLINGridModelView>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortName);

				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_GET_MANAGE_CLIN_MODEL, SecurityPage.ManageCLINs, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

				ManageCLINGridModelView theModelView = new ManageCLINGridModelView();
				theModelView.HideContractType = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST;
				theModelView.ContractTypeList = this.workspaceAdminControllerLogic.BuildContractTypeDropdownOptions(ws.Id);
				ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();
				// convert the DTOs to model views
				IOrderedEnumerable<FullClin> clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinPaddedNumber);

				foreach (FullClin clin in clins)
				{
					string contract = Utilities.GetPickListText(clin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);
					theModelView.ClinResults.Add(new ManageCLINModelView(clin, contract));
				}

				result.Data = theModelView;
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_GET_MANAGE_CLIN_MODEL, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning CLIN data: {ex.Message}");
			}

			return result;
		}

        /// <summary>
        /// Get WBSs for Workspace in Manage WBS page
        /// </summary>
        /// <param name="workspaceShortName"> the workspace shortname</param>
        /// <returns>The MV for the Manage WBS grid</returns>
        [HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<ManageWBSGridModelView> GetManageWBS(string workspaceShortName)
		{
			IESSingleResponse<ManageWBSGridModelView> result = new IESSingleResponse<ManageWBSGridModelView>();

			try
			{
				ManageWBSGridModelView theModelView = new ManageWBSGridModelView();
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortName);

				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_GET_MANAGE_WBS_MODEL, SecurityPage.ManageWBS, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

				theModelView.ContainsOCI = ws.ContainsOCI;
				theModelView.AvailableClins = ws.ClinsNoMultiClin.OrderBy(x => x.ClinNumber).Select(c => new ManageCLINModelView(c, string.Empty)).ToList();
				theModelView.WbsResults = workspaceAdminControllerLogic.GetManageWBSModel(ws);
				result.Data = theModelView;
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_GET_MANAGE_WBS_MODEL, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning WBS data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Get WBSs for Workspace in Manage WBS page
		/// </summary>
		/// <param name="workspaceShortName"> the workspace shortname</param>
		/// <returns>The MV for the Manage WBS grid</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<ManageBOEGridWidgetModelView> GetManageBOE(string workspaceShortName)
		{
			IESSingleResponse<ManageBOEGridWidgetModelView> result = new IESSingleResponse<ManageBOEGridWidgetModelView>();

			try
			{
				ManageBOEGridWidgetModelView theModelView = new ManageBOEGridWidgetModelView();
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortName);

				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_GET_MANAGE_BOE_MODEL, SecurityPage.ManageBOEs, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

				theModelView.ContainsOCI = ws.ContainsOCI;


				boeControllerLogic.CalculateManageBOEDefaults(theModelView, ws);

				theModelView.ManageBoeHeaderInfo = boeControllerLogic.GetCompanySpecificManageBoeHeaderInfo;
				theModelView.WorkspaceState = ws.WorkspaceState;
				theModelView.AllowBOEStateChanges = CheckPermission(SecurityPage.EditBoeLockedState, ws, null) == SecurityAuthorization.CreateReadUpdateDelete;
				result.Data = theModelView;
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_GET_MANAGE_BOE_MODEL, sw);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning BOE data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Saves a BOE(s) from the Manage BOE page.
		/// </summary>
		/// <param name="workspace">Workspace name.</param>
		/// <param name="boes">List of BOEs to be saved.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<ManageBOEModelView> SaveManageBOE(string workspace, [FromBody] Collection<ManageBOEModelView> boes)
		{
			if (boes == null)
			{
				throw new ArgumentNullException(nameof(boes));
			}

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace, true);
			IESSingleResponse<ManageBOEModelView> result = new IESSingleResponse<ManageBOEModelView>();

			// Initialize Action
			Stopwatch sw = this.InitializeAction(logger, WebConstants.ACTION_SAVE_MANAGE_BOE, SecurityPage.ManageBOEs, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);

			try
			{
				System.Web.Mvc.ModelStateDictionary modelState = new System.Web.Mvc.ModelStateDictionary();

				result.Data = boeControllerLogic.SaveManageBOE(boes, modelState, ws);;
				result.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			// Finalize Action
			this.FinalizeAction(logger, WebConstants.ACTION_SAVE_MANAGE_BOE, sw);

			return result;
		}
	}
}