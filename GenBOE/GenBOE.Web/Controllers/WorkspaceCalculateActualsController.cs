// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System.Diagnostics;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using IES.Common;

	/// <summary>
	/// Workspace Actuals Controller
	/// </summary>
	public class WorkspaceCalculateActualsController : GenBOEController
    {
		/// <summary>
		/// Logger
		/// </summary>
        private readonly Logger _log = new Logger(typeof(WBSController));

		/// <summary>
		/// boe state machine
		/// </summary>
        private readonly BOEStateMachine _BOEStateMachine = null;

		/// <summary>
		/// boe mediator
		/// </summary>
        private readonly BoeMediator _BoeMediator = null;
        
        /// <summary>
        /// Constructor
        /// </summary>
		/// <param name="factory">Full object factory</param>
		/// <param name="inBoeMediator">Boe mediator</param>
		/// <param name="inBOEStateMachine">Boe state machine</param>
		/// <param name="inCommonDataMapper">Common data mapper</param>
		/// <param name="inControllerLogic">boe controller logic</param>
		/// <param name="inSecurityAccess">Security access</param>
		/// <param name="inSiteMasterUtilities">Site master utilities</param>
		/// <param name="inSystemMetrics">System metrics</param>
		/// <param name="permissionLoader">Permission loader</param>
		/// <param name="userLoader">User loader</param>
        public WorkspaceCalculateActualsController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            SiteMasterUtilities inSiteMasterUtilities,
            BOEStateMachine inBOEStateMachine,
            BoeMediator inBoeMediator,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic
            )
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
        {
            this._BOEStateMachine = inBOEStateMachine;
            this._BoeMediator = inBoeMediator;
        }

        /// <summary>
        /// Returns the WorkspaceCalculateActuals view
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>Main page for workspace calculate actuals</returns>
        public ViewResult Index(string workspace)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "Index", SecurityPage.WorkspaceCalculateActuals, SecurityAuthorization.ReadUpdate, ws, null);

            ViewData["WorkspaceID"] = ws.Id;

            // Perform Action
            ViewResult toReturn = GetMasterView(WebConstants.VIEW_INDEX, workspace);

            // Finalize Action
            FinalizeAction(_log, "Index", sw);
            return toReturn;
        }

		#region Display

		/// <summary>
		/// Get the Workspace Calculate Actuals Model for grid
		/// </summary>
		/// <param name="workspace">the workspace</param>
		/// <returns>The MV for the Workspace Calculate Actuals grid</returns>
		public JsonResult GetWorkspaceCalculateActualsModel(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_GET_WORKSPACE_CALCULATE_ACTUALS_MODEL, SecurityPage.WorkspaceCalculateActuals, SecurityAuthorization.ReadUpdate, ws, null);

			// TODO: we will need these eventually so we need to use them for the compiler not to complain
			this._BoeMediator.GetType();
			this._BOEStateMachine.GetType();

			// TODO: return the actual models
			object model = new object();
			// Finalize Action
			FinalizeAction(_log, WebConstants.ACTION_GET_WORKSPACE_CALCULATE_ACTUALS_MODEL, sw);

			return this.Json(model);
		}

		#endregion Display
	}
}
