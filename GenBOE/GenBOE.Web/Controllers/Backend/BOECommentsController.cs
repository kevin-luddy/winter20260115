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
	using System.Web.Http;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using IES.Common;

	/// <summary>
	/// BOE Comments Controller
	/// </summary>
	public class BOECommentsController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("BOECommentsController");

		/// <summary>
		/// BOE Comments Controller Logic
		/// </summary>
		private BOECommentsControllerLogic boeCommentsControllerLogic { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="securityAccess"></param>
		/// <param name="factory"></param>
		/// <param name="userLoader"></param>
		/// <param name="permissionsLoader"></param>
		/// <param name="boeCommentsControllerLogic"></param>

		public BOECommentsController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader,
			IPermissionsDTODataLoader permissionsLoader, BOECommentsControllerLogic boeCommentsControllerLogic)
		: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.boeCommentsControllerLogic = boeCommentsControllerLogic;
		}

		/// <summary>
		/// Get BOEComments
		/// </summary>
		/// <param name="workspaceShortname">Workspace Short Name</param>
		/// <param name="boeId">BOE ID</param>
		/// <returns>BOECommentsModelView</returns>
		[System.Web.Http.HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<BOECommentsModelView> GetBOEComments(string workspaceShortname, int boeID)
		{
			IESSingleResponse<BOECommentsModelView> result = new IESSingleResponse<BOECommentsModelView>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);

			// Initialize Action
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_DISPLAY_BOE_COMMENTS, SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeID);

			try
			{
				Collection<SecurityPage> pagesToCheck = new Collection<SecurityPage>() 
				{
					SecurityPage.BOEApproval,
					SecurityPage.BOEComment,
					SecurityPage.BOECommentResponse
				};
				Dictionary<SecurityPage, SecurityAuthorization> extraPermissionDictionary = new Dictionary<SecurityPage, SecurityAuthorization>();
				foreach (SecurityPage page in pagesToCheck)
				{
					SecurityAuthorization securityAuthorization = CheckPermission(page, ws, boeID);
					extraPermissionDictionary.Add(page, securityAuthorization);
				}
				if (Utilities.IsReadOnly())
				{
					SecurityAuthorization securityAuthorization = CheckPermission(SecurityPage.SystemAdmin, null, null);
					extraPermissionDictionary.Add(SecurityPage.SystemAdmin, securityAuthorization);
				}
				result.Data = boeCommentsControllerLogic.GetBOEComments(ws, boeID, extraPermissionDictionary);
				result.Data.ContainsOCI = ws.ContainsOCI;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.IsSuccessful = false;
			}

			// Finalize Action
			FinalizeAction(logger, WebConstants.ACTION_DISPLAY_BOE_COMMENTS, sw);
			return result;
		}

		/// <summary>
		/// Saves a list of updated or new BOE Comments from the UI
		/// </summary>
		/// <param name="boeComments">Collection of BOE Comment Model Views that have been
		/// added or updated by the user</param>
		/// <returns></returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> SaveBoeComments([FromBody] BOECommentsModelView boeComments)
		{
			if (boeComments == null)
			{
				throw new ArgumentNullException(nameof(boeComments));
			}

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(boeComments.WorkspaceShortName);
			FullBoe fullBOE = this.Factory.CreateFullBoe(boeComments.BoeId);

			// Initialize Action
			Stopwatch sw = InitializeActionWithAnyPermission(logger, WebConstants.ACTION_SAVE_BOE_COMMENTS,
				new Collection<SecurityPage>() { SecurityPage.BOEApproval, SecurityPage.BOEComment, SecurityPage.BOECommentResponse },
				SecurityAuthorization.CreateReadUpdateDelete, ws, boeComments.BoeId);
			try
			{
				boeCommentsControllerLogic.SaveBOEComments(ws, fullBOE, boeComments);

				result.IsSuccessful = true;
				result.Data = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.IsSuccessful = false;
			}

			// Finalize Action
			FinalizeAction(logger, WebConstants.ACTION_SAVE_BOE_COMMENTS, sw);
			return result;
		}
	}
}