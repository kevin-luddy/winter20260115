// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using System.Web;
    using System.Web.Mvc;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.IO.Export;
    using IES.ActionLogic.IO.Import;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using RDM.Web.Common;

    /// <summary>
    /// The controller for Rates.
    /// </summary>
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
        public RateController(IRateControllerLogic controllerLogic, IRateDetailLoader rateDetailLoader, IWhosOnlineLoader whosOnlineLoader, IRateCodeReplicationLoader replicationLoader)
            : base(whosOnlineLoader, controllerLogic)
        {
            this.controllerLogic = controllerLogic;
            this.rateDetailLoader = rateDetailLoader;
            this.replicationLoader = replicationLoader;
        }

        /// <summary>
        /// Index for Rates
        /// </summary>
        /// <param name="id">Optional Revision Id</param>
        /// <returns>Index page for Rates.</returns>
        public ActionResult Index(int? id)
        {
            // Only set the Revision Id. Remaining grid data will be loaded via AJAX call to GetRatesByVersion().
            return this.View(new RateGridModelView { SelectedRevisionId = id });
        }

        /// <summary>
        /// Gets the rates.
        /// </summary>
        /// <param name="id">The revision Id.</param>
        /// <returns>Rates for the version requested</returns>
        [HttpPost]
        public ActionResult GetRatesByVersion(int? id)
        {
            ActionResult response;

            try
            {
                RateGridModelView model = this.controllerLogic.GetRatesByVersion(id, this.Logic.Revisions);

                response = this.Json(model);
            }
            catch (Exception ex)
            {
                this.Log.Error(ex);
                throw new GenValidationException(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Gets the WIP Rate Codes with section information.
        /// </summary>
        /// <returns>Rates with section information</returns>
        [HttpPost]
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
        [HttpPost]
        public ActionResult GetVersionDifferences(int id, int secondId)
        {
            ICollection<RevisionOptionModelView> revisionOptions = this.Logic.RevisionMediator.GetRevisionOptions(this.Logic.Revisions).OrderByDescending(r => r.Id).ToCollection();
            
            ICollection<RateDetailModelView> rates = this.controllerLogic.GetRatesVersionDifferences(revisionOptions, id, secondId);

            return this.Json(rates);
        }

        /// <summary>
        /// Validate the rate codes in the file.
        /// </summary>
        /// <returns>A list of validation errors (if any), and lists of rate codes to be inserted and updated</returns>
        [HttpPost]
        public ActionResult ValidateRateCodes()
        {
            return this.ValidateAndImportRateCodes(false);
        }

        /// <summary>
        /// Validate and import the rate codes in the file.
        /// </summary>
        /// <returns>All rate codes for the WIP version, including imported rate codes.</returns>
        [HttpPost]
        public ActionResult ImportRateCodes()
        {
            return this.ValidateAndImportRateCodes(true);
        }

        /// <summary>
        /// Helper method to validate and (optionally) import the rate codes in the file.
        /// </summary>
        /// <param name="doImport">if true, perform validation and import; otherwise, only perform validation.</param>
        /// <returns>All rate codes for the WIP version, including imported rate codes.</returns>
        private ActionResult ValidateAndImportRateCodes(bool doImport)
        {
            ActionResult response = new JsonResult();

            if (this.Request.Files.Count == 0)
            {
                throw new GenValidationException("No file was sent to Import.");
            }

            HttpPostedFileBase importFile = this.Request.Files[0];
            if (importFile != null && importFile.ContentLength > 0)
            {
                RevisionModelView revision = this.Logic.WipRevision;

                // Confirm that either user owns lock or area is unlocked
                this.Logic.VerifyLockForSaving(LockArea.RDMRates, revision.Id);

                RateGridModelView rates = this.controllerLogic.GetRatesByVersion(revision.Id, this.Logic.Revisions);
                ICollection<RateDetailModelView> importedRateCodes = RateCodeImporter.ImportFromExcelFile(this.Request.Files[0].InputStream, rates);

                // Load the existing RateDetails for all the imported Rates.
                ICollection<RateDetailModelView> existingRateCodes = rates.Rates;
                // Perform validation
                ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateImportedRateCodes(existingRateCodes.ToArray(), importedRateCodes.ToArray());
                // Get a collection of rate codes to be updated or inserted.
                ICollection<RateDetailModelView> updatedRateCodes = this.controllerLogic.GetUpdatedRateCodes(existingRateCodes, importedRateCodes);
                if (doImport)
                {
                    if (validationErrors.Any())
                    {
                        throw new GenValidationException(validationErrors);
                    }

                    // Save the updated rate codes to the Database for the work in progress version.
                    this.controllerLogic.LoadImportedRateCodes(updatedRateCodes);

                    // Refresh the rates from the database.
                    response = this.Json(this.rateDetailLoader.GetRatesByRevision(revision));
                }
                else
                {
                    // generate validation response
                    response = this.Json(new
                    {
                        ValidationErrors = validationErrors,
                        InsertRateCodes = updatedRateCodes.Where(x => x.Id < 0).OrderBy(x => x.RateCode).Select(x => new { RateCode = x.RateCode, Description = x.Description }).ToList(),
                        UpdateRateCodes = updatedRateCodes.Where(x => x.Id >= 0).OrderBy(x => x.RateCode).Select(x => new { RateCode = x.RateCode, Description = x.Description }).ToList()
                    });
                }
            }

            return response;
        }

        /// <summary>
        /// Import the rates in the file. Imports will overwrite existing rates. It will not create new rates.
        /// </summary>
        /// <returns>All rates for the WIP version, including imported rates.</returns>
        [HttpPost]
        public ActionResult ImportRates()
        {
            ActionResult response = new JsonResult();

            if (this.Request.Files.Count == 0)
            {
                throw new GenValidationException("No file was sent to Import.");
            }

            HttpPostedFileBase importFile = this.Request.Files[0];
            if (importFile != null && importFile.ContentLength > 0)
            {
                RevisionModelView revision = this.Logic.WipRevision;

                // Confirm that either user owns lock or area is unlocked
                this.Logic.VerifyLockForSaving(LockArea.RDMRates, revision.Id);

                Collection<string> importedRateCodes;
                Collection<RateDetailModelView> importRateDetails = RateImporter.GetRatesFromExcelFile(importFile, out importedRateCodes);

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
                response = this.Json(this.rateDetailLoader.GetRatesByRevision(revision));
            }

            return response;
        }

        /// <summary>
        /// Download the example rates import file.
        /// </summary>
        /// <returns>File contents</returns>
        public ActionResult DownloadRatesImportExample()
        {
            string filename = "RatesImportExample.xlsx";
            string path = this.Server.MapPath("~/Templates/Export");
            string file = Path.Combine(path, filename);
            file = Path.GetFullPath(file);

            return this.File(file, ExportFileDownloadBase.ContentType_XLSX, filename);
        }

        /// <summary>
        /// Download the example rate codes import file.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <returns>File contents</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ActionResult DownloadRateCodesImportExample(int? id)
        {
            try
            {
                string serverFileName = this.Server.MapPath("~/Templates/Export/RateCodesImportExample.xlsx");
                RateGridModelView rates = this.controllerLogic.GetRatesByVersion(id, this.Logic.Revisions);               
                rates.Rates.Clear();    // clear out Rates so example file is empty.

                // Call the export function and get back the file name of the populated file.
                string exportedFileName = RateCodeExporter.ExportToExcelFile(serverFileName, rates);

                // Generate a custom ActionResult to cause a file download to the client
                string fileName = "RateCodesImportExample.xlsx";
                // Generate a custom ActionResult to cause a file download to the client
                FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                return this.File(
                    fileStream: fs,
                    contentType: ExportFileDownloadBase.GetContentType(fileName),
                    fileDownloadName: fileName);
            }
            catch (GeneralAppException e)
            {
                this.Log.Error(e);
                return this.CreateTextFileWithErrorMessage(e.Message);
            }
        }

        /// <summary>
        /// Export rate codes file.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <returns>File contents</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ActionResult ExportRateCodes(int? id)
        {
            try
            {
                string serverFileName = this.Server.MapPath("~/Templates/Export/RateCodesImportExample.xlsx");
                RateGridModelView rates = this.controllerLogic.GetRatesByVersion(id, this.Logic.Revisions);

                // Call the export function and get back the file name of the populated file.
                string exportedFileName = RateCodeExporter.ExportToExcelFile(serverFileName, rates);

                // Generate a custom ActionResult to cause a file download to the client
                string fileName = "RateCodesExport.xlsx";
                // Generate a custom ActionResult to cause a file download to the client
                FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                return this.File(
                    fileStream: fs,
                    contentType: ExportFileDownloadBase.GetContentType(fileName),
                    fileDownloadName: fileName);
            }
            catch (GeneralAppException e)
            {
                this.Log.Error(e);
                return this.CreateTextFileWithErrorMessage(e.Message);
            }
        }

        /// <summary>
        /// Saves the specified collection.
        /// POST: Rate/Save
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>Json Result of the saved rates.</returns>
        [HttpPost]
        public ActionResult Save([ModelBinder(typeof(JsonNetModelBinder))] Collection<RateDetailModelView> collection)
        {
            if (collection == null || collection.None())
            {
                throw new GenValidationException("There are no Rates being saved.");
            }

            // Confirm that either user owns lock or area is unlocked
            this.Logic.VerifyLockForSaving(LockArea.RDMRates, collection[0].RevisionId);

            RevisionModelView revision = this.Logic.RevisionMediator.GetById(collection[0].RevisionId);

			Collection<string> importedRateCodes = collection.Select(r => r.RateCode).ToCollection();

			// Retrieve all the rate code replications as well
			importedRateCodes.AddRange(this.replicationLoader.GetAll().Select(r => r.To));

			// Load the existing RateDetails for all the imported Rates.
			ICollection<RateDetailModelView> existingRates = this.rateDetailLoader.GetRatesForImport(revision, importedRateCodes.ToArray());

			ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateImportedRates(existingRates, collection);
			if (validationErrors.Any())
			{
				throw new GenValidationException(validationErrors);
			}

			validationErrors = this.controllerLogic.ValidateRateDetailModelViews(collection, revision.StartYear, revision.EndYear);
			if (validationErrors.Any())
			{
				throw new GenValidationException(validationErrors);
			}

			// Send the updated rates to the Database for the work in progress version.
			this.controllerLogic.LoadImportedRates(existingRates, collection);
			ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetRatesByRevision(revision);

            return this.Json(rates);
        }
    }
}
