// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers.Backend
{
	using System;
	using System.Diagnostics;
	using System.Web.Http;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.Controllers;
	using IES.Common;

	/// <summary>
	/// BOELaborController
	/// </summary>
	public class BOELaborController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("BOEController");

		/// <summary>
		/// BOE Labor Controller Logic
		/// </summary>
		private IBOELaborControllerLogic boeLaborControllerLogic { get; set; }

		public BOELaborController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader,
				IBOELaborControllerLogic boeLaborControllerLogic)
				: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.boeLaborControllerLogic = boeLaborControllerLogic;
		}

		/// <summary>
		/// Get BOE Task Element Data
		/// </summary>
		/// <param name="workspaceShortname">Workspace Short Name</param>
		/// <param name="boeId">BOE ID</param>
		/// <param name="taskElementId">Task Element ID</param>
		/// <returns>LaborTaskDataModelView</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<LaborTaskDataModelView> GetTaskDataModel(string workspaceShortname, int boeId, int taskElementId)
		{
			IESSingleResponse<LaborTaskDataModelView> result = new IESSingleResponse<LaborTaskDataModelView>();
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(logger, "GetTaskDataModel", SecurityPage.TaskElements, SecurityAuthorization.Read, ws, boeId);
			FullBoe boe = this.Factory.CreateFullBoe(boeId);

			try
			{
				result.Data = this.boeLaborControllerLogic.GetLaborTaskData(ws, boe, taskElementId);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add(ex.Message);
			}

			// Finalize Action
			this.FinalizeAction(logger, "GetTaskDataModel", sw);

			return result;
		}
	}
}