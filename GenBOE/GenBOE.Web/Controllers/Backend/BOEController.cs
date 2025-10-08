// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Web.Http;
	using System.Web.Http.Cors;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;

	/// <summary>
	/// BOEController used for /boe/editboeindex/boe/
	/// </summary>
	[EnableCors("*", "*", "*", SupportsCredentials = true)]
	public class BOEController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("BOEController");

		/// <summary>
		/// BOE Controller Logic
		/// </summary>
		private IBOEControllerLogic boeControllerLogic { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="homeControllerLogic">Home Controller Logic</param>
		public BOEController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader,
			IBOEControllerLogic boeControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.boeControllerLogic = boeControllerLogic;
		}

		/// <summary>
		/// Get BOE Headers
		/// </summary>
		/// <param name="workspaceShortname">Workspace Short Name</param>
		/// <param name="boeId">BOE ID</param>
		/// <returns>BOEHeaderModelView</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<IBOEHeaderModelView> GetBOEHeader(string workspaceShortname, int boeId)
		{
			IESSingleResponse<IBOEHeaderModelView> result = new IESSingleResponse<IBOEHeaderModelView>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
			FullBoe boe = ws.Boes.First(x => x.Id == boeId);
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_BOE_HEADER, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, boeId);

			try
			{
				result.Data = boeControllerLogic.CreateBOEHeaderMV(boe, ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.GET_BOE_HEADER, sw);
			return result;
		}

		/// <summary>
		/// Get BOE Headers Description
		/// </summary>
		/// <param name="workspaceShortname">Workspace Short Name</param>
		/// <param name="boeId">BOE ID</param>
		/// <returns>BOEHeaderDescriptionModelView</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<BOEHeaderDescriptionModelView> GetBOEHeaderDescription(string workspaceShortname, int boeId)
		{
			IESSingleResponse<BOEHeaderDescriptionModelView> result = new IESSingleResponse<BOEHeaderDescriptionModelView>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
			FullBoe boe = ws.Boes.First(x => x.Id == boeId);
			Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_DISPLAY_BOE_HEADER_DESCRIPTION, SecurityPage.EditBOEHeader, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, boeId);

			try
			{
				result.Data = boeControllerLogic.GetBOEHeaderDescriptionMv(boe, ws);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.GET_BOE_HEADER, sw);
			return result;
		}
	}
}