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
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.PickList;

	/// <summary>
	/// Workspace Admin Controller
	/// </summary>
	public class WorkspaceAdminController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Contract Type loader
		/// </summary>
		private ContractTypeLoader contractTypeLoader;

		/// <summary>
		/// Workspace Admin Controller Logic
		/// </summary>
		private WorkspaceAdminControllerLogic workspaceAdminControllerLogic;

		/// <summary>
		/// logger
		/// </summary>
		private Logger logger = new Logger("WorkspaceAdminController");

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="securityAccess"></param>
		/// <param name="factory"></param>
		/// <param name="userLoader"></param>
		/// <param name="permissionsLoader"></param>
		public WorkspaceAdminController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, WorkspaceAdminControllerLogic workspaceAdminControllerLogic, ContractTypeLoader contractTypeLoader)
	: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.workspaceAdminControllerLogic = workspaceAdminControllerLogic;
			this.contractTypeLoader = contractTypeLoader;
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
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_GET_MANAGE_CLIN_MODEL, SecurityPage.ManageWBS, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

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
	}
}