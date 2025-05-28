// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers.Backend
{
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net.Http.Headers;
	using System.Net.Http;
	using System.Net;
	using System.Web.Http;

	/// <summary>
	/// WBS Controller for Manage WBS Page
	/// </summary>
	public class WBSController : BoeDataBaseAPIController
	{
		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("WBSController");

		/// <summary>
		/// WBS Controller Logic
		/// </summary>
		private WBSControllerLogic wbsControllerLogic { get; set; }

		public WBSController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader,
			IPermissionsDTODataLoader permissionsLoader, WBSControllerLogic wbsControllerLogic)
		: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.wbsControllerLogic = wbsControllerLogic;
		}

		/// <summary>
		/// Save WBS updates
		/// </summary>
		/// <param name="addEditWBSModelView">wbs data to be saved</param>
		/// <returns>success indicator</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> SaveWBS([FromBody] AddEditWBSModelView addEditWBSModelView)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			try
			{
				if (addEditWBSModelView != null)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(addEditWBSModelView.workspaceShortName);

					// Initialize Action
					Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_SAVE_MANAGE_WBS_UPDATES, SecurityPage.ManageWBS, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

					this.wbsControllerLogic.SaveWBS(ws, addEditWBSModelView.wbs);
					result.IsSuccessful = true;
					result.Data = true;

					// Finalize Action
					FinalizeAction(logger, WebConstants.ACTION_SAVE_MANAGE_WBS_UPDATES, sw);
				}
			}
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.ValidationList.Select(x => x.ValidationIssue).ToList();
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error saving WBS: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Export WBS
		/// </summary>
		/// <param name="exportWBSModelView">ExportWBSModelView (just workspaceShortName)</param>
		/// <returns>filestream</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public HttpResponseMessage ExportWBSs([FromBody] ExportWBSModelView exportWBSModelView)
		{
			try
			{
				if (exportWBSModelView != null)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(exportWBSModelView.workspaceShortName);

					// Initialize Action
					Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_EXPORT_WBS, SecurityPage.ManageWBS, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

					FileStream fs = this.wbsControllerLogic.ExportWBSs(ws);

					HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StreamContent(fs);
					response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
					response.Content.Headers.ContentDisposition.FileName = string.Format("GenBOE-{0}-WBSs.xlsx", ws.WorkspaceName);

					// Finalize Action
					FinalizeAction(logger, WebConstants.ACTION_EXPORT_WBS, sw);

					return response;
				}
				else
				{
					return new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = new StringContent("Invalid request body")
					};
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new StringContent("Unknown error exporting WBSs")
				};
			}
		}

		/// <summary>
		/// Export WBS Template
		/// </summary>
		/// <param name="exportWBSModelView">ExportWBSModelView (just workspaceShortName)</param>
		/// <returns>filestream</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public HttpResponseMessage ExportWBSTemplate([FromBody] ExportWBSModelView exportWBSModelView)
		{
			try
			{
				if (exportWBSModelView != null)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(exportWBSModelView.workspaceShortName);

					// Initialize Action
					Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_EXPORT_WBS_TEMPLATE, SecurityPage.ManageWBS, SecurityAuthorization.Read, new Collection<WorkspaceDTO>() { ws }, null);

					FileStream fs = this.wbsControllerLogic.ExportWBSTemplate();

					HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StreamContent(fs);
					response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
					response.Content.Headers.ContentDisposition.FileName = string.Format("GenBOE-{0}-WBSs.xlsx", ws.WorkspaceName);

					// Finalize Action
					FinalizeAction(logger, WebConstants.ACTION_EXPORT_WBS_TEMPLATE, sw);

					return response;
				}
				else
				{
					return new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = new StringContent("Invalid request body")
					};
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new StringContent("Unknown error exporting WBSs")
				};
			}
		}
	}
}