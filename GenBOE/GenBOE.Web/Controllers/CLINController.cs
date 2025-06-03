// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Web.Mvc;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using GenBOE.Web.ModelView;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;
	using IES.Common.PickList;

	public class CLINController : GenBOEController
	{
		private Logger _log = new Logger(typeof(CLINController));

		private ICLINExporter _ClinExporter;
		private IClinDTODataLoader clinLoader;
		private ICommonDataLoader _CommonDataLoader;
		private ContractTypeLoader contractTypeLoader;
		private CLINControllerLogic _clinControllerLogic;

		/// <summary>
		/// The constructor
		/// </summary>
		public CLINController(ISecurityAccess inSecurityAccess,
			ICommonDataMapper inCommonDataMapper,
			SiteMasterUtilities inSiteMasterUtilities,
			ICLINExporter inClinExporter,
			SystemMetrics inSystemMetrics,
			IFullObjectFactory factory,
			IClinDTODataLoader clinLoader,
			IUserDTODataLoader userLoader,
			IPermissionsDTODataLoader permissionLoader,
			IGenBOEControllerLogic inControllerLogic,
			ICommonDataLoader inCommonDataLoader,
			ContractTypeLoader contractTypeLoader,
			CLINControllerLogic clinControllerLogic
			)
			: base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
		{
			this._ClinExporter = inClinExporter;
			this.clinLoader = clinLoader;
			this._CommonDataLoader = inCommonDataLoader;
			this.contractTypeLoader = contractTypeLoader;
			this._clinControllerLogic = clinControllerLogic;
		}

		/// <summary>
		/// Returns the ManageCLINs view
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ViewResult Index(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "Index", SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

			// Perform Action
			ViewResult toReturn = this.GetMasterView(WebConstants.VIEW_INDEX, workspace);

			// Finalize Action
			this.FinalizeAction(this._log, "Index", sw);
			return toReturn;
		}

		public JsonResult GetManageClinGridModel(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, WebConstants.ACTION_GET_MANAGE_CLIN_MODEL, SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

			ManageCLINGridModelView theModelView = new ManageCLINGridModelView();
			theModelView.HideContractType = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST;
			theModelView.ContractTypeList = this.BuildContractTypeDropdownOptions(ws.Id);
			ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();
			// convert the DTOs to model views
			IOrderedEnumerable<FullClin> clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinPaddedNumber);

			foreach (FullClin clin in clins)
			{
				string contract = Utilities.GetPickListText(clin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);
				theModelView.ClinResults.Add(new ManageCLINModelView(clin, contract));
			}

			// Finalize Action
			this.FinalizeAction(this._log, WebConstants.ACTION_GET_MANAGE_CLIN_MODEL, sw);

			return this.Json(theModelView);
		}

		/// <summary>
		/// Builds sorted SelectList with only Contract Types selected at the workspace level and default option
		/// </summary>
		/// <param name="workspaceId"></param>
		/// <returns>sorted SelectList with workspace Contract Types only</returns>
		private ICollection<PickListDto> BuildContractTypeDropdownOptions(int workspaceId)
		{
			//Contract Type dropdown includes workspace contract types only
			List<PickListDto> selectedContractTypesList = this._CommonDataLoader.GetSelectedContractTypes(workspaceId)
				.OrderBy(ct => ct.Text)
				.ToList();
			//Add 'Not Set' option for CLINs without contract type
			selectedContractTypesList.Insert(0, new PickListDto { Id = Constants.CONTRACT_TYPE_NOT_SET, Text = Constants.CONTRACT_TYPE_NOT_SET_STRING });
			return selectedContractTypesList;
		}

		/// <summary>
		/// Get the BOE count associated with the CLIN
		/// </summary>
		/// <param name="workspace">The Workspace.</param>
		/// <param name="ClinID">CLIN ID</param>
		/// <returns>number of BOEs associated with the CLIN</returns>
		public ActionResult GetBOECountForClin(string workspace, int ClinID)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = this.InitializeAction(this._log, "GetBOECountForClin", SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

			JsonResult toReturn;

			/** Valid Model Check */
			if (this.ModelState.IsValid)
			{
				// Perform Action
				DataRelationshipVerifier.VerifyDataRelation(this.Factory.CreateFullClin(ClinID), ws.Id);
				int result = this.clinLoader.GetBoeCountByClinID(ClinID);
				toReturn = this.Json(new { Status = result });
			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(this.ModelState));
			}

			// Finalize Action
			this.FinalizeAction(this._log, "GetBOECountForClin", sw);
			return toReturn;
		}

		/// <summary>
		/// Deletes a group of CLINs.
		/// </summary>
		/// <param name="workspace">The workspace the CLINs are assoicated with</param>
		/// <param name="inDeletedClins">Collection of deleted CLINs</param>
		/// <returns>If successfull,empty string is return. Otherwise, exception error text to be handled in the post:error </returns>
		public virtual JsonResult DeleteCLINs(string workspace, Collection<ManageCLINModelView> inDeletedClins)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "DeleteCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = this.Json(new { Status = true });

			// inDeletedClins could be null if user tries to only delete CLINS that are in use.
			// in that case, we will not do anything.  There is already a warning that is displayed
			// to the user.
			if (inDeletedClins != null)
			{
				foreach (ManageCLINModelView deletedCLIN in inDeletedClins)
				{
					// Only delete CLINs that have positive IDs
					if (deletedCLIN.ClinID > 0 && deletedCLIN.Deleted == true)
					{
						this.SaveClin(workspace, deletedCLIN);
					}
				}
			}
			// Finalize Action
			this.FinalizeAction(this._log, "DeleteCLINs", sw);

			return toReturn;
		}

		/// <summary>
		/// This function is called from the ManageCLINGrid partial view when the user selects "Save."
		/// This function will check if the input is valid and clean up any of the data if any newly 
		/// created CLINs is deleted=true which is not valid
		/// </summary>
		/// <param name="workspace">the workspace the CLINs are assoicated with</param>
		/// <param name="inUpdatedClin">collection of new or edited CLINs</param>
		/// <returns>If successfull,empty string is return. Otherwise, exception error text to be handled in the post:error </returns>
		public virtual JsonResult SaveClin(string workspace, ManageCLINModelView inUpdatedClin)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "SaveClin", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			// Perform Action
			// check if the input is null
			if (inUpdatedClin == null)
			{
				throw new ArgumentNullException(nameof(inUpdatedClin));
			}

			JsonResult toReturn;

			// only validate data if it exists
			if (inUpdatedClin.ClinID > 0 || (inUpdatedClin.ClinID < 0 && !inUpdatedClin.Deleted))
			{
				if (this.ModelState.IsValid)
				{
					ManageCLINModelView modifiedClin = this._clinControllerLogic.SaveCLIN(ws, inUpdatedClin);
					toReturn = this.Json(modifiedClin);
				}
				else
				{
					throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(this.ModelState));
				}
			}
			else
			{
				this._log.Info("No CLINs have been updated as part of SaveEditCLIN");
				toReturn = this.Json(new { Status = false });
			}

			// Finalize Action
			this.FinalizeAction(this._log, "SaveClin", sw);
			return toReturn;
		}

		/// <summary>
		/// Imports an Excel file and parses out CLINs to be imported. The results are passed back to the UI
		/// for acceptance by the user. They can then be passed back to CompleteImportCLINs to save the imported
		/// data into the system.
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns>JSON formatted results from import operation</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public JsonResult ImportCLINs(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = this.InitializeAction(this._log, "ImportCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = null;

			// If a file was uploaded successfully
			if (this.Request.Files.Count > 0 && this.Request.Files[0].FileName.Length > 0)
			{
				try
				{
					ICollection<ImportedClin> results = this._clinControllerLogic.ImportCLINs(ws, this.Request.Files[0].InputStream);

					toReturn = this.GenerateJsonUploadResponse(true, results, this.Request.Files[0].FileName);
				}
				// Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
				// exception messages to display for the user
				catch (NotExcelFileException)
				{
					toReturn = this.GenerateJsonUploadResponse(false, "File is an invalid format. File must be in a MS Excel (.xlsx) format.");
				}
				catch (ColumnMissingException ex1)
				{
					toReturn = this.GenerateJsonUploadResponse(false, "File does not contain all of the required columns. The following columns are missing: {0}.", ex1.Message);
				}
				catch (Exception ex0)
				{
					this._log.Error(ex0, "Unknown Import Resources Error.");
					toReturn = this.GenerateJsonUploadResponse(false, "A general error occurred. Please ensure that your import file follows the format of the import template and retry the import.");
				}
			}
			// If no file was uploaded, tell the user about it. Client validation should keep this from being hit.
			else
			{
				toReturn = this.GenerateJsonUploadResponse(false, "No file selected for upload");
			}

			// Finalize Action
			this.FinalizeAction(this._log, "ImportCLINs", sw);

			return toReturn;
		}

		/// <summary>
		/// Completes an import operation by saving valid imported CLINs into the DB.
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="importResults">The CLIN import data to be saved</param>
		/// <returns>True</returns>
		public JsonResult CompleteImportCLINs(string workspace, Collection<ImportedClin> importResults)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			// Initialize Action
			Stopwatch sw = this.InitializeAction(this._log, "CompleteImportCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.CreateReadUpdateDelete, ws, null);

			JsonResult toReturn = this.Json(new { Status = true });

			this._clinControllerLogic.CompleteImportCLIN(ws, importResults);

			// Finalize Action
			this.FinalizeAction(this._log, "CompleteImportCLINs", sw);

			return toReturn;
		}

		/// <summary>
		/// Exports all CLINs for the workspace
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns>An ActionResult for the file being exported</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ActionResult ExportCLINs(string workspace)
		{
			FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

			Stopwatch sw = this.InitializeAction(this._log, "ExportCLINs", SecurityPage.ManageCLINs, SecurityAuthorization.Read, ws, null);

			ActionResult toReturn = null;

			/** Valid Model Check */
			if (this.ModelState.IsValid)
			{
				//filter out multi boe's to hide from user. 
				Collection<ClinDTO> clins = ws.ClinsNoMultiClin.OrderBy(c => c.ClinNumber).ToCollection<ClinDTO>();

				// Get the CLIN template file name
				string templateFileName = this.Server.MapPath("~/Templates/Export/CLINs.xlsx");

				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
				{
					templateFileName = this.Server.MapPath("~/Templates/Export/CLINsRMS.xlsx");
				}
				ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();

				string exportFile = this._ClinExporter.ExportToExcelFile(templateFileName, clins, ws, contractTypes);

				if (exportFile.Length > 0)
				{
					string fileName = string.Format("GenBOE-{0}-CLINs.xlsx", ws.WorkspaceName);
					// Generate a custom ActionResult to cause a file download to the client
					FileStream fs = new FileStream(exportFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

					// Finalize Action
					FinalizeAction(_log, "ExportCLINs", sw);

					toReturn = File(
						fileStream: fs,
						contentType: ExportFileDownloadBase.GetContentType(fileName),
						fileDownloadName: fileName);
				}

			}
			else
			{
				throw new GenValidationException(Utilities.CreateModelStateValidationErrorList(this.ModelState));
			}

			// Finalize Action
			this.FinalizeAction(this._log, "PageManageCLIN", sw);

			return toReturn;
		}
	}
}
