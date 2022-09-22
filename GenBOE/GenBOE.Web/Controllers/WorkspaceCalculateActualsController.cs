// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using ActionLogic.ModelView.Clin;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.WBS;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
    using GenBOE.Web.ModelView;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;

    public class WorkspaceCalculateActualsController : GenBOEController
    {
        private readonly Logger _log = new Logger(typeof(WBSController));

        private readonly BOEStateMachine _BOEStateMachine = null;
        private readonly BoeMediator _BoeMediator = null;
        
        /// <summary>
        /// Constructor
        /// </summary>
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
        /// <returns></returns>
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
		/// Get the Manage WBS Grid MV
		/// </summary>
		/// <param name="workspace">the workspace</param>
		/// <returns>The MV for the Manage WBS grid</returns>
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

		#region AJAX Calls


		#endregion
	}
}
