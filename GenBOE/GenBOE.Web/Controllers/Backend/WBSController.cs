// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers.Backend
{
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic.Backend;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
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
	using System.Web;
	using ImportWbsResultsModelView = ActionLogic.ModelView.ImportWbsResultsModelView;

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
					Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_SAVE_MANAGE_WBS_UPDATES, SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);

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
		/// Create BOEs
		/// </summary>
		/// <param name="workspaceShortName">The workspace short name</param>
		/// <param name="selectedWbsIDs">Collection of the WBS IDs to create BOE for</param>
		/// <returns>IES Result whether or not operation was successful</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> CreateBOEs([FromBody] CreateBOEModelView createBOEModelView)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			if (createBOEModelView == null)
			{
				throw new ArgumentNullException(nameof(createBOEModelView));
			}

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(createBOEModelView.WorkspaceShortName);

				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_CREATE_BOES, SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);

				this.wbsControllerLogic.CreateBOEs(ws, createBOEModelView.WbsIDs);


				result.Data = true;
				result.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_CREATE_BOES, sw);

			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Data = false;
				result.Messages.Add($"Unknown error occurred creating BOEs: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Deletes a group of WBSs.
		/// </summary>
		/// <param name="deleteWBSModelView">Model containing workspace short name and WBSs to delete</param>
		/// <returns>IES Result whether or not deletion was successful</returns>
		[System.Web.Http.HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> DeleteWBS([FromBody] DeleteWBSModelView deleteWBSModelView)
		{
			if (deleteWBSModelView == null)
			{
				throw new ArgumentNullException(nameof(deleteWBSModelView));
			}

			IESSingleResponse<bool> result = new IESSingleResponse<bool>();

			try
			{
				if (deleteWBSModelView.Wbs.Any())
				{
					FullWorkspace ws = this.Factory.CreateFullWorkspace(deleteWBSModelView.WorkspaceShortName);

					// Initialize Action
					Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_SAVE_MANAGE_WBS_UPDATES, SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);

					foreach (ManageWBSModelView wbs in deleteWBSModelView.Wbs)
					{
						// Only delete WBS that have positive IDs
						if (wbs.WbsID > 0 && wbs.Deleted)
						{
							this.wbsControllerLogic.SaveWBS(ws, wbs);
						}
					}

					// Finalize Action
					FinalizeAction(logger, WebConstants.ACTION_SAVE_MANAGE_WBS_UPDATES, sw);
				}

				result.Data = true;
				result.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Data = false;
				result.Messages.Add($"Unknown error occurred deleting WBS data: {ex.Message}");
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
		public HttpResponseMessage ExportWBSs([FromBody] ExportFileModelView exportWBSModelView)
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
		public HttpResponseMessage ExportWBSTemplate([FromBody] ExportFileModelView exportWBSModelView)
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
					Content = new StringContent("Unknown error exporting WBS template")
				};
			}
		}

		/// <summary>
		/// Import WBS
		/// </summary>
		/// <returns>filestream</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IESSingleResponse<Collection<ImportWbsResultsModelView>> ImportWBS()
		{
			IESSingleResponse<Collection<ImportWbsResultsModelView>> response = new IESSingleResponse<Collection<ImportWbsResultsModelView>>();
			string workspaceShortName = HttpContext.Current.Request.Form["workspaceShortName"];
			int importType = int.Parse(HttpContext.Current.Request.Form["importType"]);
			HttpPostedFile importFile = HttpContext.Current.Request.Files[0];

			if (string.IsNullOrWhiteSpace(workspaceShortName))
			{
				throw new InvalidOperationException("Missing workspaceShortName in form data.");
			}

			if (importFile == null)
			{
				throw new InvalidOperationException("No file was uploaded.");
			}

			string fileName = HttpContext.Current.Request.Files[0].FileName;
			if (string.IsNullOrEmpty(fileName) || (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) && !fileName.EndsWith(".xlsm", StringComparison.OrdinalIgnoreCase)))
			{
				throw new InvalidOperationException($"Unsupported file type.");
			}

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortName);

				// Initialize Action
				Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_IMPORT_WBS, SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);
				Collection<ImportedWbs> importResults = this.wbsControllerLogic.ImportWBS(ws, importFile.InputStream);
				Collection<ImportWbsResultsModelView> importModel = new Collection<ImportWbsResultsModelView>();
				foreach (ImportedWbs importResult in importResults)
				{
					foreach (WbsImportResult resultType in importResult.ImportTypes)
					{
						importModel.Add(new ImportWbsResultsModelView()
						{
							ImportType = (int)resultType,
							WbsID = importResult.Id,
							WbsNumber = importResult.WbsNumber,
							WbsTitle = importResult.WbsTitle,
							ClinID = importResult.ClinID,
							ClinIDs = importResult.ClinIDs,
							ClinNumber = importResult.ClinNumber,
							ClinTitle = importResult.ClinTitle
						});
					}
				}

				// Find out what the user deleted from the Excel spreadsheet that is now going to be imported.
				if (importType == (int)WbsImportType.Existing)
				{
					this.wbsControllerLogic.FindWBSsToBeDeleted(ws, importFile.InputStream, importModel);
				}

				response.Data = importModel;
				response.IsSuccessful = true;

				// Finalize Action
				FinalizeAction(logger, WebConstants.ACTION_IMPORT_WBS, sw);

				return response;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				response.IsSuccessful = false;
				response.Messages.Add("An unexpected error occurred while processing the file.");
				return response;
			}
		}

		/// <summary>
		/// Save WBS updates
		/// </summary>
		/// <param name="addEditWBSModelView">wbs data to be saved</param>
		/// <returns>success indicator</returns>
		[System.Web.Http.HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESSingleResponse<bool> CompleteImportWBS([FromBody] CompleteWBSImportModelView completeWbsImport)
		{
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();
			result.Data = true;
			result.IsSuccessful = true;

			if (completeWbsImport == null)
			{
				throw new ArgumentNullException(nameof(completeWbsImport));
			}

			if (completeWbsImport.dataToSave.Any())
			{
				try
				{

					FullWorkspace ws = this.Factory.CreateFullWorkspace(completeWbsImport.workspaceShortName);

					// Initialize Action
					Stopwatch sw = InitializeAction(logger, WebConstants.ACTION_COMPLETE_IMPORT_WBS, SecurityPage.ManageWBS, SecurityAuthorization.CreateReadUpdateDelete, new Collection<WorkspaceDTO>() { ws }, null);

					this.wbsControllerLogic.CompleteImportWBS(ws, completeWbsImport.dataToSave);

					// Finalize Action
					FinalizeAction(logger, WebConstants.ACTION_COMPLETE_IMPORT_WBS, sw);
				}
				catch (GenValidationException ex)
				{
					result.Data = false;
					result.IsSuccessful = false;
					logger.Error(ex);
					result.Messages = ex.ValidationList.Select(x => x.ValidationIssue).ToList();
				}
				catch (Exception ex)
				{
					result.Data = false;
					result.IsSuccessful = false;
					logger.Error(ex);
					result.Messages.Add($"Unknown error completing WBS import: {ex.Message}");
				}
			}

			return result;
		}
	}
}