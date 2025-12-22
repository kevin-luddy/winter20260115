// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView.BOE;
    using IES.Common;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using GenBOE.Web.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic;

	public class BOECommentsController : GenBOEController
    {
        Logger _log = new Logger(typeof(BOECommentsController));
		private readonly BOECommentsControllerLogic _BOECommentsLogic = null;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="inSecurityAccess"></param>
		/// <param name="inCommonDataMapper"></param>
		/// <param name="inEmailer">Emailer</param>
		public BOECommentsController(ISecurityAccess inSecurityAccess, 
            CommonDataMapper inCommonDataMapper, SiteMasterUtilities inSiteMasterUtilities,
            UserDTODataLoader inUserDTODataLoader,
            SystemMetrics inSystemMetrics, 
            IFullObjectFactory factory, 
            BOECommentsControllerLogic inBOECommentsLogic,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDTODataLoader, permissionLoader, inControllerLogic)
        {
            _BOECommentsLogic = inBOECommentsLogic;
        }


        /// <summary>
        /// Displays the BOE Comments view
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <returns></returns>
        [HttpPost]
		public ViewResult DisplayBOEComments(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, WebConstants.ACTION_DISPLAY_BOE_COMMENTS, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			int currentUserID = ws.CurrentActiveUser.UserID;

            ViewData["BOEID"] = boeID;
            ViewData["CurrentUserID"] = currentUserID;

            Collection<SecurityPage> pagesToCheck = new Collection<SecurityPage>() { 
                SecurityPage.BOEApproval, 
                SecurityPage.BOEComment, 
                SecurityPage.BOECommentResponse 
            };

            Dictionary<SecurityPage, SecurityAuthorization> extraPermissionDictionary = CheckPermissions(pagesToCheck, ws, boeID);

            if (SiteMasterUtilities.IsReadOnly())
            {
				extraPermissionDictionary.Add(SecurityPage.SystemAdmin, CheckPermissions(SecurityPage.SystemAdmin, null, null));
			}

            ViewData["WorkspaceState"] = (int)ws.WorkspaceState;

			BOECommentsModelView theModelView = _BOECommentsLogic.GetBOEComments(ws,boeID, extraPermissionDictionary);

			this.ViewData["Approvals_ReadOnly"] = theModelView.ApprovalsReadOnly;
			this.ViewData["Comments_ReadOnly"] = theModelView.CommentsReadOnly;
			this.ViewData["Responses_ReadOnly"] = theModelView.ResponsesReadOnly;

			// Perform Action
			ViewResult toReturn = View(WebConstants.VIEW_BOE_COMMENTS_GRID, theModelView);

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_DISPLAY_BOE_COMMENTS, sw);
            return toReturn;
        }

		/// <summary>
		/// Saves a list of updated or new BOE Comments from the UI
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="boeID"></param>
		/// <param name="boeComments">Collection of BOE Comment Model Views that have been
		/// added or updated by the user</param>
		/// <returns></returns>
		[HttpPost]
		public virtual JsonResult SaveBoeComments(string workspace, int boeID, BOECommentsModelView boeComments)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullBoe fullBOE = this.Factory.CreateFullBoe(boeID);

            // Initialize Action
            Stopwatch sw = InitializeActionWithAnyPermission(_log, WebConstants.ACTION_SAVE_BOE_COMMENTS,
                new Collection<SecurityPage>() { SecurityPage.BOEApproval, SecurityPage.BOEComment, SecurityPage.BOECommentResponse },
                SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            _BOECommentsLogic.SaveBOEComments(ws, fullBOE, boeComments);

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, WebConstants.ACTION_SAVE_BOE_COMMENTS, sw);
            return toReturn;
        }
    }
}
