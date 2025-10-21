// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.Controllers.Backend
{
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using Glimpse.AspNet.Tab;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.PickList;
	using Microsoft.VisualBasic.Logging;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Http;
	using System.Web.Mvc;

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
		[System.Web.Http.HttpGet]
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
        [System.Web.Http.HttpGet]
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
		[System.Web.Http.HttpGet]
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
		[System.Web.Http.HttpPost]
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
				System.Web.Mvc.ModelStateDictionary modelState = ModelState.ToMVC();

				//Removing errors except the boes model
				foreach (string key in modelState.Keys)
				{
					if(key != "boes[0]")
					{
						modelState[key].Errors.Clear();
					}
				}

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

		// ExportManageBOE in BOEController.cs
		/// <summary>
		/// Perform actions to export BOEs from Manage BOEs page
		/// </summary>
		/// <param name="workspace">Workspace containing BOEs</param>
		/// <returns></returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public HttpResponseMessage ExportBOEs([FromBody] ExportFileModelView exportBOEModelView)
		{
			try
			{
				if (exportBOEModelView != null)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(exportBOEModelView.workspaceShortName);
					FileStream fs = this.boeControllerLogic.ExportBOEs(ws, exportBOEModelView.isBlankTemplate);

					HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StreamContent(fs);
					response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
					response.Content.Headers.ContentDisposition.FileName = "BOEs.xlsm";

					return response;
				}
				else
				{
					return new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = new StringContent("Invalid request body")
					};
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new StringContent("Unknown error exporting BOEs")
				};
			}
		}

		/// <summary>
		/// Import BOEs to get confirmation response
		/// </summary>
		/// <returns>List of imported BOEs</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<ImportedBoe> ImportBOEs()
		{
			IESResponse<ImportedBoe> result = new IESResponse<ImportedBoe>();

			try
			{
				string workspaceShortName = HttpContext.Current.Request.Form["workspaceShortName"];
				Stream importFile = HttpContext.Current.Request.Files[0].InputStream;

				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortName);

				result.Data = boeControllerLogic.ImportBOEs(ws, importFile);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred importing CLIN data: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Finalize importing the BOEs into the workspace
		/// </summary>
		/// <param name="importBoeResults">The POST body with the workspace shortname and imported BOE metadata</param>
		/// <returns>Success or failure</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<bool> CompleteImportBOEs([FromBody] ImportBOEModelView importBoeResults)
		{
			IESResponse<bool> result = new IESResponse<bool>();

			try
			{
				if (importBoeResults != null)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(importBoeResults.workspaceShortName);
					boeControllerLogic.CompleteImportBOEs(ws, importBoeResults.importedBOEs);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error completing import for BOE data: {ex.Message}");
			}

			return result;
		}
	}
}