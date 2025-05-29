// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Web.Controllers.Backend
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web;
	using System.Web.Http;
	using System.Web.Mvc;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.Exceptions;

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
		/// <param name="workspace">workspaceShortName</param>
		/// <param name="inUpdatedClin">CLIN model</param>
		/// <returns>Newly upserted CLIN</returns>
		[System.Web.Http.HttpPost]
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
			catch (GenValidationException ex)
			{
				logger.Error(ex);
				result.Messages = ex.ValidationList.Select(x => x.ValidationIssue).ToList();
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error saving CLIN: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Export CLIN
		/// </summary>
		/// <param name="exportCLINModelView">ExportCLINModelView (just workspaceShortName)</param>
		/// <returns>filestream</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public HttpResponseMessage ExportCLINs([FromBody] ExportCLINModelView exportCLINModelView)
		{
			try
			{
				if (exportCLINModelView != null)
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(exportCLINModelView.workspaceShortName);
					FileStream fs = this._clinControllerLogic.ExportCLINs(ws);

					HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StreamContent(fs);
					response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
					response.Content.Headers.ContentDisposition.FileName = "CLINs.xlsx";

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
					Content = new StringContent("Unknown error exporting CLINs")
				};
			}
		}

		/// <summary>
		/// Import CLIN to get confirmation response
		/// </summary>
		/// <returns>List of CLINs with types</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<ImportedClin> ImportCLINs()
		{
			IESResponse<ImportedClin> result = new IESResponse<ImportedClin>();

			try
			{
				string workspaceShortName = HttpContext.Current.Request.Form["workspaceShortName"];
				Stream importFile = HttpContext.Current.Request.Files[0].InputStream;

				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortName);

				result.Data = _clinControllerLogic.ImportCLINs(ws, importFile);
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred importing CLIN data: {ex.Message}");
			}

			return result;
		}
	}
}