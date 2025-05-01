// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Web.Controllers.Backend
{
	using System;
	using System.Web.Http;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;

	/// <summary>
	/// CLIN Controller for Manage CLIN Page
	/// </summary>
	public class CLINController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("CLINController");

		/// <summary>
		/// CLIN Controller Logic
		/// </summary>
		private CLINControllerLogic _clinControllerLogic { get; set; }

		public CLINController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader,
			IPermissionsDTODataLoader permissionsLoader, CLINControllerLogic clinControllerLogic)
		: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this._clinControllerLogic = clinControllerLogic;
		}

		/// <summary>
		/// Save CLIN
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="inUpdatedClin"></param>
		/// <returns></returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]

		public IESSingleResponse<ManageCLINModelView> SaveClin([FromBody] AddEditCLINModelView addEditCLINModelView)
		{
			IESSingleResponse<ManageCLINModelView> result = new IESSingleResponse<ManageCLINModelView>();

			try
			{
				if (addEditCLINModelView != null)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(addEditCLINModelView.workspaceShortName);
					result.Data = this._clinControllerLogic.SaveCLIN(ws, addEditCLINModelView.clin);
					result.IsSuccessful = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error saving CLIN: {ex.Message}");
			}

			return result;
		}
	}
}
