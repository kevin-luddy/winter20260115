// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.IO.Export;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using RDM.Web.Common;

    /// <summary>
    /// Controller for Reports.
    /// </summary>
    public class ReportsController : RDMController
    {
        /// <summary>
        /// Rate Detail Loader
        /// </summary>
        private readonly IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// Cobra Loader
        /// </summary>
        private readonly ICobraYearsLoader cobraLoader;

        /// <summary>
        /// burden pool loader
        /// </summary>
        private readonly IBurdenPoolLoader burdenPoolLoader;

        /// <summary>
        /// Reports controller logic
        /// </summary>
        private readonly IReportsControllerLogic reportsControllerLogic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reportsControllerLogic">Reports controller logic</param>
        /// <param name="rateDetailLoader">Rate Detail loader</param>
        /// <param name="cobraLoader">Cobra Loader</param>
        /// <param name="burdenPoolLoader">Burden Pool loader</param>
        /// <param name="whosOnlineLoader">Who's Online Loader</param>
        public ReportsController(IReportsControllerLogic reportsControllerLogic, 
            IRateDetailLoader rateDetailLoader, ICobraYearsLoader cobraLoader, IBurdenPoolLoader burdenPoolLoader, IWhosOnlineLoader whosOnlineLoader)
            : base(whosOnlineLoader, reportsControllerLogic)
        {
            this.reportsControllerLogic = reportsControllerLogic;
            this.rateDetailLoader = rateDetailLoader;
            this.cobraLoader = cobraLoader;
            this.burdenPoolLoader = burdenPoolLoader;
        }

        /// <summary>
        /// Index for Reports.
        /// </summary>
        /// <param name="id">A possible id to a revision, if passed in.</param>
        /// <returns>Index page for Reports.</returns>
        public ActionResult Index(int? id)
        {
            ReportsModelView modelView = new ReportsModelView
            {
                AvailableVersions = this.Logic.RevisionMediator.GetRevisionOptions(),
                AdminUser = this.Logic.IsRDMAdminUser,
                CobraAdminUser = this.Logic.IsRDMCobraAdminUser
            };

            if (!this.Logic.IsRDMAdminUser && !this.Logic.IsRDMCobraAdminUser)
            { 
                modelView.AvailableVersions.Remove(modelView.AvailableVersions.First(z => z.Label == CommonConstants.WorkInProgress));
            }

            if (modelView.AvailableVersions.Any())
            {
                // try to use the revision Id passed in
                RevisionOptionModelView revisionOption = modelView.AvailableVersions.FirstOrDefault(v => v.Id == id);

                if (revisionOption == null)
                {
                    revisionOption = modelView.AvailableVersions.FirstOrDefault();
                }

                if (revisionOption != null)
                {
                    modelView.SelectedVersion = revisionOption.Id;
                }
            }

            return this.View(modelView);
        }

        /// <summary>
        /// Exports the Cobra Data.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <returns>An ActionResult.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ActionResult ExportCobraData(int id)
        {
            try
            {
                string templateFileLocation = this.Server.MapPath("~/Templates/Export/CobraExport.xlsx");

                RevisionModelView revision = this.Logic.RevisionMediator.GetById(id);
                if (revision == null)
                {
                    throw new ArgumentException("Unable to export Cobra data - could not find selected revision.");
                }

                int startYear = 2011; // Start year will always be 2011 as per the customer (BOEJ-3286)
                ICollection<CobraExportRowModelView> cobraExportRows =
                    this.cobraLoader.GetCobraExportRows(id, startYear);
                string exportedFileName = CobraExporter.ExportToExcelFile(templateFileLocation, cobraExportRows);

                // The filename is hardcoded to make it more obvious what is being replace by string.format().
                string fileDownloadName =
                    $"Cobra_RDM-Rev{revision.Revision}_{DateTime.Today.ToString(Constants.DATE_FORMATTING_YEAR_MONTH_DAY)}.xlsx";

                // Generate a custom ActionResult to cause a file download to the client
                
                FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

                return File(
                    fileStream: fs,
                    contentType: ExportFileDownloadBase.GetContentType(fileDownloadName),
                    fileDownloadName: fileDownloadName);
            }
            catch (GeneralAppException ex)
            {
                this.Log.Error(ex);
                return this.CreateTextFileWithErrorMessage(ex);
            }
        }

        /// <summary>
        /// Exports the ProPricer Data.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <returns>An ActionResult.</returns>
        public ActionResult ExportProPricerData(string id)
        {
            try
            {
                string zipPathFile = this.Server.MapPath("~/Templates/Export/ProPricerExport.zip");                
                int revisionId;
                int.TryParse(id, out revisionId);
                BurdenPoolGridModelView burdenPoolGridModel = this.burdenPoolLoader.GetByRevision(revisionId);
                RevisionModelView selectedRevision = this.Logic.RevisionMediator.GetById(revisionId);
                ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetRatesByRevision(selectedRevision);

                // Generate the zip file containing the ProPricer direct and burden rate exports.
                return this.reportsControllerLogic.ExportProPricerData(zipPathFile, selectedRevision.Revision, rates, burdenPoolGridModel.BurdenPools, burdenPoolGridModel.BurdenElements);
            }
            catch (GeneralAppException ex)
            {
                this.Log.Error(ex);
                return this.CreateTextFileWithErrorMessage(ex);
            }
        }

        /// <summary>
        /// Generates the Full PPRD and returns a Word document.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <param name="portionMarkingRequired">Is Portion Marking Required</param>
        /// <returns>The Word file representing full PPRD.</returns>
        public ActionResult GenerateFullPPRD(string id, bool? portionMarkingRequired)
        {
            ActionResult result = new EmptyResult();
            try
            {
                string serverFileName = this.Server.MapPath("~/Templates/Export/PPRDTemplate.docx");

                this.reportsControllerLogic.GenerateFullPPRD(id, serverFileName, this.Response, portionMarkingRequired);
            }
            catch (GeneralAppException e)
            {
                this.Log.Error(e);
                result = this.CreateTextFileWithErrorMessage(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Exports a zip file containing the Revision data as XML.
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <returns>An ActionResult.</returns>
        public ActionResult ExportRevisionAsJson(string id)
        {
            try
            {
                // Create a new random file name in the specified directory
                string exportDirectory = this.Server.MapPath("~/Templates/Export/");
                string serverFileName = Path.GetDirectoryName(exportDirectory) + "\\" + Path.GetRandomFileName() + ".json";
                return this.reportsControllerLogic.ExportRevisionAsJson(id, serverFileName);
            }
            catch (GeneralAppException ex)
            {
                this.Log.Error(ex);
                return this.CreateTextFileWithErrorMessage(ex);
            }
        }
    }
}