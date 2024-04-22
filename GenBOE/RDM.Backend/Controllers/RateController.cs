// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Transactions;
	using Azure;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.ActionLogic.Core.IO.Export;
	using IES.ActionLogic.Core.IO.Import;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.Common.Core.OfficeUtilities;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using RDM.Backend.Models;
	using RDM.Backend.Common;

	/// <summary>
	/// The controller for Rates.
	/// </summary>
	[Authorize]
	[Route("api/Rate")]
	public class RateController : RDMController
	{
		/// <summary>
		/// Rate Detail Loader
		/// </summary>
		private readonly IRateDetailLoader rateDetailLoader;

		/// <summary>
		/// The Rate Controller Logic.
		/// </summary>
		private readonly IRateControllerLogic controllerLogic;

		/// <summary>
		/// The replication loader
		/// </summary>
		private readonly IRateCodeReplicationLoader replicationLoader;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controllerLogic">RatecontrollerLogic</param>
		/// <param name="rateDetailLoader">Rate Detail Loader</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		/// <param name="replicationLoader">The replication loader.</param>
		public RateController(IRateControllerLogic controllerLogic, IRateDetailLoader rateDetailLoader,
			IWhosOnlineLoader whosOnlineLoader, IRateCodeReplicationLoader replicationLoader,
			ISecurityInformation securityInformation, IConfiguration configuration, ILogger<RateController> logger)
			: base(securityInformation, whosOnlineLoader, controllerLogic, configuration, logger)
		{
			this.controllerLogic = controllerLogic;
			this.rateDetailLoader = rateDetailLoader;
			this.replicationLoader = replicationLoader;
		}

		/// <summary>
		/// Gets the rates.
		/// </summary>
		/// <param name="id">The revision Id.</param>
		/// <returns>Rates for the version requested</returns>
		[HttpGet("[action]")]
		public ActionResult GetRatesByVersion(int? id)
		{
			IESResponse<RateGridModelView> response = new();
			try
			{
				response.Data = this.controllerLogic.GetRatesByVersion(id, this.Logic.Revisions);
				response.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}
			catch (Exception ex)
			{
				this.log.LogError(ex, "Unknown Exception.");
				response.Messages.Add("Unknown Exception");
			}

			return this.Json(response);
		}

		/// <summary>
		/// Gets the WIP Rate Codes with section information.
		/// </summary>
		/// <returns>Rates with section information</returns>
		[HttpGet("[action]")]
		public ActionResult GetWIPRateSections()
		{
			return this.Json(this.rateDetailLoader.GetWIPRateSections(this.Logic.WipRevision));
		}

		/// <summary>
		/// Gets the rates for the rate compare page.
		/// </summary>
		/// <param name="id">The revision id.</param>
		/// <param name="secondId">The id of the second revision to compare to. -1 for previous revision</param>
		/// <returns>Collection of rates with changes between the requested revisions and the previous revision.</returns>
		[HttpGet("[action]")]
		public ActionResult GetVersionDifferences(int id, int secondId)
		{
			ICollection<RevisionOptionModelView> revisionOptions = this.Logic.RevisionMediator.GetRevisionOptions(this.Logic.Revisions).OrderByDescending(r => r.Id).ToList();
			ICollection<RateDetailModelView> rates = this.controllerLogic.GetRatesVersionDifferences(revisionOptions, id, secondId);

			return this.Json(rates);
		}

		/// <summary>
		/// Validate the rate codes in the file.
		/// </summary>
		/// <returns>A list of validation errors (if any), and lists of rate codes to be inserted and updated</returns>
		[HttpPost("[action]")]
		public ActionResult ValidateRateCodes()
		{
			IESResponse <ICollection <RateDetailModelView >>  validationResponse = this.ValidateRateCodesInternal();
			IESResponse<ValidateRateCodesViewModel> response = new(); 
			
			response.Data = new()
			{
				InsertRateCodes = validationResponse.Data.Where(x => x.Id < 0).OrderBy(x => x.RateCode).Select(x => new RateCodeValidationViewModel() { RateCode = x.RateCode, Description = x.Description }).ToList(),
				UpdateRateCodes = validationResponse.Data.Where(x => x.Id >= 0).OrderBy(x => x.RateCode).Select(x => new RateCodeValidationViewModel() { RateCode = x.RateCode, Description = x.Description }).ToList()
			};
			return this.Json(response);
		}

		/// <summary>
		/// Validate and import the rate codes in the file.
		/// </summary>
		/// <returns>All rate codes for the WIP version, including imported rate codes.</returns>
		[HttpPost("[action]")]
		public ActionResult ImportRateCodes()
		{
			IESResponse<ICollection<RateDetailModelView>> response = new();

			IESResponse<ICollection<RateDetailModelView>> validationResponse = this.ValidateRateCodesInternal();
			if (validationResponse.IsSuccessful)
			{
				try
				{
					// Save the updated rate codes to the Database for the work in progress version.
					this.controllerLogic.LoadImportedRateCodes(validationResponse.Data);

					// Refresh the rates from the database.
					RevisionModelView revision = this.Logic.WipRevision;
					response.Data = this.rateDetailLoader.GetRatesByRevision(revision);
					response.IsSuccessful = true;
				}
				catch (GenValidationException ex)
				{
					response.Messages = ex.GetValidationMessages(ex.ValidationList);
				}
			}
			else
			{
				response.Messages = validationResponse.Messages;
			}

			return this.Json(response);
		}

		/// <summary>
		/// Helper method to import and validate the rate codes in the file.
		/// </summary>
		/// <returns>All rate codes for the WIP version, including imported rate codes.</returns>
		[NonAction]
		private IESResponse<ICollection<RateDetailModelView>> ValidateRateCodesInternal()
		{
			IESResponse<ICollection<RateDetailModelView>> response = new();

			try
			{
				if (this.Request.Form.Files.Count == 0)
				{
					throw new GenValidationException("No file was sent to Import.");
				}

				IFormFile importFile = this.Request.Form.Files[0];
				if (importFile != null && importFile.Length > 0)
				{
					RevisionModelView revision = this.Logic.WipRevision;

					// Confirm that either user owns lock or area is unlocked
					this.Logic.VerifyLockForSaving(LockArea.RDMRates, revision.Id);

					RateGridModelView rates = this.controllerLogic.GetRatesByVersion(revision.Id, this.Logic.Revisions);
					ICollection<RateDetailModelView> importedRateCodes = RateCodeImporter.ImportFromExcelFile(this.Request.Form.Files[0].OpenReadStream(), rates);

					// Load the existing RateDetails for all the imported Rates.
					ICollection<RateDetailModelView> existingRateCodes = rates.Rates;
					// Perform validation
					ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateImportedRateCodes(existingRateCodes.ToArray(), importedRateCodes.ToArray());

					if (validationErrors.Any())
					{
						throw new GenValidationException(validationErrors);
					}

					// Get a collection of rate codes to be updated or inserted.
					response.Data = this.controllerLogic.GetUpdatedRateCodes(existingRateCodes, importedRateCodes);
					response.IsSuccessful = true;
				}
				else
				{
					throw new GenValidationException("Empty file was sent to Import.");
				}
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return response;
		}

		/// <summary>
		/// Import the rates in the file. Imports will overwrite existing rates. It will not create new rates.
		/// </summary>
		/// <returns>All rates for the WIP version, including imported rates.</returns>
		[HttpPost("[action]")]
		public ActionResult ImportRates()
		{
			IESResponse<ICollection<RateDetailModelView>> response = new();

			try
			{
				if (this.Request.Form.Files.Count == 0)
				{
					throw new GenValidationException("No file was sent to Import.");
				}

				IFormFile importFile = this.Request.Form.Files[0];
				if (importFile != null && importFile.Length > 0)
				{
					RevisionModelView revision = this.Logic.WipRevision;

					// Confirm that either user owns lock or area is unlocked
					this.Logic.VerifyLockForSaving(LockArea.RDMRates, revision.Id);

					Collection<RateDetailModelView> importRateDetails = RateImporter.GetRatesFromExcelFile(importFile.OpenReadStream(), out List<string> importedRateCodes);

					// Retrieve all the rate code replications as well
					importedRateCodes.AddRange(this.replicationLoader.GetAll().Select(r => r.To));

					// Load the existing RateDetails for all the imported Rates.
					ICollection<RateDetailModelView> existingRates = this.rateDetailLoader.GetRatesForImport(revision, importedRateCodes.ToArray());

					ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateImportedRates(existingRates, importRateDetails);
					if (validationErrors.Any())
					{
						throw new GenValidationException(validationErrors);
					}

					validationErrors = this.controllerLogic.ValidateRateDetailModelViews(importRateDetails, revision.StartYear, revision.EndYear);
					if (validationErrors.Any())
					{
						throw new GenValidationException(validationErrors);
					}

					// Send the updated rates to the Database for the work in progress version.
					this.controllerLogic.LoadImportedRates(existingRates, importRateDetails);

					// Refresh the rates from the database.
					response.Data = this.rateDetailLoader.GetRatesByRevision(revision);
					response.IsSuccessful = true;
				}
				else
				{
					throw new GenValidationException("Empty file was sent to Import.");
				}
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Download the example rates import file.
		/// </summary>
		/// <returns>File contents</returns>
		[HttpGet("[action]")]
		public ActionResult DownloadRatesImportExample()
		{
			string filename = "RatesImportExample.xlsx";
			string path = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export");
			string file = Path.Combine(path, filename);
			file = Path.GetFullPath(file);

			return this.File(file, ExportFileDownloadBase.ContentType_XLSX, filename);
		}

		/// <summary>
		/// Download the example rate codes import file.
		/// </summary>
		/// <param name="id">Revision Id</param>
		/// <returns>File contents</returns>
		[HttpGet("[action]")]
		public IActionResult DownloadRateCodesImportExample(int? id)
		{
			try
			{
				string serverFileName = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/RateCodesImportExample.xlsx");
				RateGridModelView rates = this.controllerLogic.GetRatesByVersion(id, this.Logic.Revisions);
				rates.Rates.Clear();    // clear out Rates so example file is empty.

				// Call the export function and get back the file name of the populated file.
				string exportedFileName = RateCodeExporter.ExportToExcelFile(serverFileName, rates);

				// Generate a custom ActionResult to cause a file download to the client
				string fileName = "RateCodesImportExample.xlsx";
				// Generate a custom ActionResult to cause a file download to the client
				FileStream fs = new(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				return this.File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileName),
					fileDownloadName: fileName);
			}
			catch (GeneralAppException e)
			{
				this.log.LogError(e, "Unknown Exception.");
				return this.CreateTextFileWithErrorMessage(e.Message);
			}
		}

		/// <summary>
		/// Export rate codes file.
		/// </summary>
		/// <param name="id">Revision Id</param>
		/// <returns>File contents</returns>
		[HttpGet("[action]")]
		public IActionResult ExportRateCodes(int? id)
		{
			try
			{
				//string serverFileName = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/RateCodesImportExample.xlsx");
				string projectDirectory = Directory.GetCurrentDirectory();
				string serverFileName = Path.Combine(projectDirectory, "Templates", "Export", "RateCodesImportExample.xlsx");
				RateGridModelView rates = this.controllerLogic.GetRatesByVersion(id, this.Logic.Revisions);

				// Call the export function and get back the file name of the populated file.
				string exportedFileName = RateCodeExporter.ExportToExcelFile(serverFileName, rates);

				// Generate a custom ActionResult to cause a file download to the client
				string fileName = "RateCodesExport.xlsx";
				// Generate a custom ActionResult to cause a file download to the client
				FileStream fs = new(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				return this.File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileName),
					fileDownloadName: fileName);
			}
			catch (GeneralAppException e)
			{
				this.log.LogError(e, "Unknown Exception.");
				return this.CreateTextFileWithErrorMessage(e.Message);
			}
		}

		/// <summary>
		/// Saves the specified collection.
		/// POST: Rate/Save
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <returns>Json Result of the saved rates.</returns>
		[HttpPost("[action]")]
		public ActionResult Save(RateDetailModelView[] collection)
		{
			IESResponse<ICollection<RateDetailModelView>> response = new();

			try
			{
				if (collection == null || collection.None())
				{
					throw new GenValidationException("There are no Rates being saved.");
				}

				// Confirm that either user owns lock or area is unlocked
				this.Logic.VerifyLockForSaving(LockArea.RDMRates, collection[0].RevisionId);

				RevisionModelView revision = this.Logic.RevisionMediator.GetById(collection[0].RevisionId);
				ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateRateDetailModelViews(collection, revision.StartYear, revision.EndYear);
				if (validationErrors.Any())
				{
					throw new GenValidationException(validationErrors);
				}

				using (TransactionScope scope = new(TransactionScopeOption.Required,
					new TransactionOptions
					{
						IsolationLevel = IsolationLevel.Snapshot,
						Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", CommonConstants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
					}))
				{
					this.rateDetailLoader.SaveDetails(collection);
					scope.Complete();
				}

				response.Data = this.rateDetailLoader.GetRatesByRevision(revision);
				response.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}
	}
}
