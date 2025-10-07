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
	using GenBOE.ActionLogic.Validation;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using Microsoft.VisualBasic.Logging;

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
		/// Task Element Validation
		/// </summary>
		private TaskElementValidation taskElementValidation { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="homeControllerLogic">Home Controller Logic</param>
		public BOEController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader,
			IBOEControllerLogic boeControllerLogic, TaskElementValidation taskElementValidation)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.boeControllerLogic = boeControllerLogic;
			this.taskElementValidation = taskElementValidation;
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

		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public IESSingleResponse<GenericTaskElementGridModelView> GetTaskElementGrid(string workspaceShortname, int boeId)
		{
			IESSingleResponse<GenericTaskElementGridModelView> result = new IESSingleResponse<GenericTaskElementGridModelView>();

			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
			FullBoe boe = ws.Boes.First(x => x.Id == boeId);
			Stopwatch sw = InitializeAction(logger, WebConstants.GET_TASK_ELEMENT_GRID, SecurityPage.BOELaborGrid, SecurityAuthorization.Read, new List<WorkspaceDTO> { ws }, boeId);

			try
			{
				GenericTaskElementGridModelView theModelView = boeControllerLogic.GetTaskGridModelView(boe, ws);
				result.Data = theModelView;
				result.Data.TaskElements = theModelView.TaskElements.OrderBy(teOrder => teOrder.BOETaskElementOrder).ThenBy(teOrder => teOrder.TaskElementDetailID).ToCollection();

				ICollection<int> invalidTaskElementIds = this.taskElementValidation.GetInvalidTaskElementIds(ws, boe.TaskElements);
				theModelView.TaskElements
					.AsParallel()
					.Where(x => x.TaskElementDetailID.HasValue && invalidTaskElementIds.Contains(x.TaskElementDetailID.Value))
					.ForAll(z => z.FailedValidation = true);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			FinalizeAction(logger, WebConstants.GET_TASK_ELEMENT_GRID, sw);
			return result;
		}
	}
}