// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Http;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;

	/// <summary>
	/// Manage Permissions Controller for getting workspace home data.
	/// </summary>
	public class ManagePermissionsController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Service for PermissionsController
		/// </summary>
		private PermissionsController PermissionsController { get; set; }

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("ManagePermissionsController");

		/// <summary>
		/// ctor
		/// </summary>
		public ManagePermissionsController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, PermissionsController PermissionsController)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.PermissionsController = PermissionsController;
		}

		/// <summary>
		/// Gets Permissions Model
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<PermissionModelView> GetManagePermissionModel(string workspace)
		{
			IESSingleResponse<PermissionModelView> result = new IESSingleResponse<PermissionModelView>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);
				result.Data = PermissionsController._GetPermissionsGrid(ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occured: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Delete workspaces
		/// </summary>
		///<param name="workspace">The workspace to delete permissions from.</param>
		///<param name="inUserID">The User ID to delete permissions from.</param>
		/// <returns>Object to indicate if operation is successfull</returns>
		[HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<Object> DeleteUserPermissions(string workspace, string inUserID)
		{
			IESSingleResponse<Object> result = new IESSingleResponse<Object>();

			if (inUserID == null || !inUserID.Any())
			{
				throw new ArgumentNullException(nameof(inUserID));
			}

			try
			{
				result.Data = PermissionsController.DeleteUserPermissions(workspace, Int32.Parse(inUserID)).Data;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			return result;
		}
	}
}