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
    using System.Linq;
    using System.Web.Mvc;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.Metrics;
    using GenBOE.ActionLogic.ModelView.BOE;
    using IES.Common;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenBOE.Web.Common;

    public class BOECommentsController : GenBOEController
    {
        Logger _log = new Logger(typeof(BOECommentsController));

        private BOEHistoryDTODataLoader _boeHistoryLoader = null;
        private BOECommentsControllerLogic _BOECommentsLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess"></param>
        /// <param name="inCommonDataMapper"></param>
        /// <param name="inEmailer">Emailer</param>
        public BOECommentsController(ISecurityAccess inSecurityAccess, 
            CommonDataMapper inCommonDataMapper, SiteMasterUtilities inSiteMasterUtilities,
            UserDTODataLoader inUserDTODataLoader,
            BOEHistoryDTODataLoader inBoeHistoryLoader,
            SystemMetrics inSystemMetrics, 
            IFullObjectFactory factory, 
            BOECommentsControllerLogic inBOECommentsLogic,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic)
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, inUserDTODataLoader, permissionLoader, inControllerLogic)
        {
            _boeHistoryLoader = inBoeHistoryLoader;
            _BOECommentsLogic = inBOECommentsLogic;
        }


        /// <summary>
        /// Displays the BOE Comments view
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="boeID"></param>
        /// <returns></returns>
        public ViewResult DisplayBOEComments(string workspace, int boeID)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullBoe boe = this.Factory.CreateFullBoe(boeID);

            // Initialize Action
            Stopwatch sw = InitializeAction(_log, "DisplayBOEComments", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			int currentUserID = ws.CurrentActiveUser.UserID;

            ViewData["BOEID"] = boeID;
            ViewData["CurrentUserID"] = currentUserID;

            Collection<SecurityPage> pagesToCheck = new Collection<SecurityPage>() { 
                SecurityPage.BOEApproval, 
                SecurityPage.BOEComment, 
                SecurityPage.BOECommentResponse 
            };

            Dictionary<SecurityPage, SecurityAuthorization> extraPermissionDictionary = CheckPermissions(pagesToCheck, ws, boeID);

            SecurityAuthorization permission;
            
            if (extraPermissionDictionary.TryGetValue(SecurityPage.BOEApproval, out permission))
            {
                this.ViewData["Approvals_ReadOnly"] = this.GetReadOnlyAttribute(permission);
            }
            if (extraPermissionDictionary.TryGetValue(SecurityPage.BOEComment, out permission))
            {
                this.ViewData["Comments_ReadOnly"] = this.GetReadOnlyAttribute(permission);
            }
            if (extraPermissionDictionary.TryGetValue(SecurityPage.BOECommentResponse, out permission))
            {
                this.ViewData["Responses_ReadOnly"] = this.GetReadOnlyAttribute(permission);
            }

            if (SiteMasterUtilities.IsReadOnly())
            {
                if (CheckPermissions(SecurityPage.SystemAdmin, null, null) != SecurityAuthorization.CreateReadUpdateDelete)
                {
					this.ViewData["Approvals_ReadOnly"] = true;
					this.ViewData["Comments_ReadOnly"] = true;
					this.ViewData["Responses_ReadOnly"] = true;
				}
			}

            ViewData["WorkspaceState"] = (int)ws.WorkspaceState;

            BOECommentsModelView theModelView = new BOECommentsModelView(currentUserID, boe.ApproverResponses);

            // Get all comments and responses for the current BOE
            theModelView.Comments = _BOECommentsLogic.GetCommentsByBOEId(boeID);

            // Get all History entries
            ICollection<BOEHistoryDTO> boeHistories = _boeHistoryLoader.GetBOEHistory(boeID);
            
            // Initialize a list of field types that we want to display in the comments grid
            Collection<FieldType> fieldTypes = new Collection<FieldType>
            {
                FieldType.ApproverResponse
            };

            // Add each history item with a desired field type to the list of comments to render
            foreach (BOEHistoryDTO boeHistory in boeHistories)
            {
                if (fieldTypes.Contains(boeHistory.Field))
                {
                    BOEComment comment = new BOEComment
                    {
                        CommentType = BOECommentType.Approval,
                        ReviewerComment = boeHistory.NewValue,
                        ReviewerName = this.UserLoader.GetUserByID(boeHistory.PerformedByETIUserId).DisplayName,
                        ReviewerCommentUpdateDT = boeHistory.Date
                    };
                    theModelView.Comments.Add(comment);
                }
            }

            // Sort the Comments so that Approvals/Rejections are interweaved with true reviewer comments by date
            theModelView.Comments = new Collection<BOEComment>(theModelView.Comments.OrderBy(c => c.ReviewerCommentUpdateDT).ToArray());

            // Perform Action
            ViewResult toReturn = View(WebConstants.VIEW_BOE_COMMENTS_GRID, theModelView);

            // Finalize Action
            FinalizeAction(_log, "DisplayBOEComments", sw);
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
        virtual public JsonResult SaveBoeComments(string workspace, int boeID, BOECommentsModelView boeComments)
        {
            FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
            FullBoe fullBOE = this.Factory.CreateFullBoe(boeID);

            // Initialize Action
            Stopwatch sw = InitializeActionWithAnyPermission(_log, "SaveBoeComments",
                new Collection<SecurityPage>() { SecurityPage.BOEApproval, SecurityPage.BOEComment, SecurityPage.BOECommentResponse },
                SecurityAuthorization.CreateReadUpdateDelete, ws, boeID);

            _BOECommentsLogic.SaveBOEComments(ws, fullBOE, boeComments);

            JsonResult toReturn = Json(new { Status = true });

            // Finalize Action
            FinalizeAction(_log, "SaveBoeComments", sw);
            return toReturn;
        }
    }
}
