// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using IES.Common;

	/// <summary>
	/// Workspace Recalculate Actuals Controller
	/// </summary>
	public class WorkspaceRecalculateActualsController : GenBOEController
	{
		/// <summary>
		/// logger
		/// </summary>
		private readonly Logger _log = new Logger(typeof(WBSController));

		/// <summary>
		/// Workspace controller logic
		/// </summary>
		private readonly IWorkspaceControllerLogic workspaceControllerLogic;

		/// <summary>
		/// Constructor
		/// </summary>
		public WorkspaceRecalculateActualsController(ISecurityAccess inSecurityAccess,
			CommonDataMapper inCommonDataMapper,
			SiteMasterUtilities inSiteMasterUtilities,
			SystemMetrics inSystemMetrics,
			IFullObjectFactory factory,
			IUserDTODataLoader userLoader,
			IPermissionsDTODataLoader permissionLoader,
			IGenBOEControllerLogic inControllerLogic,
			IWorkspaceControllerLogic workspaceControllerLogic
			)
			: base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
		{
			this.workspaceControllerLogic = workspaceControllerLogic;
		}

		/// <summary>
		/// Returns the WorkspaceRecalculateActuals view
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns>Main page for workspace recalculate actuals</returns>
		public ViewResult Index(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, "Index", SecurityPage.WorkspaceRecalculateActuals, SecurityAuthorization.ReadUpdate, ws, null);

			ViewData["WorkspaceID"] = ws.Id;

			// Perform Action
			ViewResult toReturn = GetMasterView(WebConstants.VIEW_INDEX, workspace);

			// Finalize Action
			FinalizeAction(_log, "Index", sw);
			return toReturn;
		}

		#region Display

		/// <summary>
		/// Get the Workspace Recalculate Actuals Model for grid
		/// </summary>
		/// <param name="workspace">the workspace</param>
		/// <returns>The MV for the Workspace Calculate Actuals grid</returns>
		public async Task<JsonResult> GetWorkspaceRecalculateActualsModel(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_WORKSPACE_RECALCULATE_ACTUALS_MODEL, SecurityPage.WorkspaceRecalculateActuals, SecurityAuthorization.ReadUpdate, ws, null);

			ICollection<WorkspaceCalculateActualsModelView> actuals = await this.workspaceControllerLogic.RecalculateActuals(ws);

			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_GET_WORKSPACE_RECALCULATE_ACTUALS_MODEL, sw);

			return this.Json(actuals);
		}

		#endregion Display
	}
}
