// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using IES.ActionLogic.IO.Export;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Logic for the RDM Reports Controller.
    /// </summary>
    public class ReportsControllerLogic : RdmControllerLogic, IReportsControllerLogic
    {
        /// <summary>
        /// PPRD Exporter
        /// </summary>
        private readonly IPPRDExporter pprdExporter;

        /// <summary>
        /// RDM Revision Exporter
        /// </summary>
        private readonly IRdmRevisionExporter rdmRevisionExporter;

        /// <summary>
        /// Rate Detail Loader
        /// </summary>
        private readonly IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// COBRA Detail Loader
        /// </summary>
        private readonly ICobraDetailLoader cobraDetailLoader;

        /// <summary>
        /// Section Loader
        /// </summary>
        private readonly ISectionLoader sectionLoader;

        /// <summary>
        /// The file attachment loader
        /// </summary>
        private readonly IFileAttachmentLoader fileAttachmentLoader;

        /// <summary>
        /// The burden pool loader
        /// </summary>
        private readonly IBurdenPoolLoader burdenPoolLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pprdExporter">PPRD Exporter</param>
        /// <param name="rdmRevisionExporter">RDM Revision Exporter</param>
        /// <param name="rateDetailLoader">Rate Detail Loader</param>
        /// <param name="cobraDetailLoader">COBRA Detail Loader</param>
        /// <param name="sectionLoader">Section Loader</param>
        /// <param name="fileAttachmentLoader">The file attachment loader.</param>
        /// <param name="burdenPoolLoader">The burden pool loader.</param>
        /// <param name="revisionMediator">Revision Mediator</param>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        public ReportsControllerLogic(IPPRDExporter pprdExporter, IRdmRevisionExporter rdmRevisionExporter, IRateDetailLoader rateDetailLoader, ICobraDetailLoader cobraDetailLoader, ISectionLoader sectionLoader, IFileAttachmentLoader fileAttachmentLoader,
            IBurdenPoolLoader burdenPoolLoader, IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)
        {
            this.pprdExporter = pprdExporter;
            this.rdmRevisionExporter = rdmRevisionExporter;
            this.rateDetailLoader = rateDetailLoader;
            this.cobraDetailLoader = cobraDetailLoader;
            this.sectionLoader = sectionLoader;
            this.fileAttachmentLoader = fileAttachmentLoader;
            this.burdenPoolLoader = burdenPoolLoader;
        }

        /// <summary>
        /// Generate a zip file containing the ProPricer direct and burden rate exports.
        /// </summary>
        /// <param name="zipPathFile">The server path where the zip file will be created</param>
        /// <param name="versionNumber">PPR&amp;D version number</param>
        /// <param name="rates">PPR&amp;D rates</param>
        /// <param name="burdenPools">PPR&amp;D ProPricer burden pools</param>
        /// <param name="burdenElements">PPR&amp;D ProPricer burden elements</param>
        /// <returns>The ExportFileDownloadResult.</returns>
        public ExportFileDownloadResult ExportProPricerData(string zipPathFile, string versionNumber,
            ICollection<RateDetailModelView> rates, ICollection<BurdenPoolDetailModelView> burdenPools, 
            ICollection<BurdenElementModelView> burdenElements)
        {
            RdmProPricerExporter rdmProPricerExporter =
                new RdmProPricerExporter(rates, zipPathFile, burdenPools, burdenElements);

            string exportedFileName = rdmProPricerExporter.ExportReport();

            // The filename is hardcoded to make it more obvious what is being replaced by string.format().
            string fileDownloadName = $"ProPricer_RDM_Rev{versionNumber}_{DateTime.Today.ToString(Constants.DATE_FORMATTING_YEAR_MONTH_DAY)}.zip";

            return new ExportFileDownloadResult(exportedFileName, fileDownloadName);
        }

        /// <summary>
        /// Generates the Full PPRD document
        /// </summary>
        /// <param name="id">Revision ID</param>
        /// <param name="serverFileName">Server File Name</param>
        /// <param name="httpResponse">HTTP response object</param>
        public void GenerateFullPPRD(string id, string serverFileName, HttpResponseBase httpResponse)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Get Revision MV
            int revisonId;
            RevisionModelView revisionMV;

            if (int.TryParse(id, out revisonId))
            {
                revisionMV = this.RevisionMediator.GetById(revisonId);
            }
            else if (id.Equals(PPRDExporterConstants.WORKINPROGRESSID))
            {
                revisionMV = this.WipRevision;
            }
            else
            {
                throw new ArgumentException("Revision ID is invalid");
            }

            string clientFileName = $"FullPPRD_RDM_Rev{revisionMV.Revision}_{DateTime.Today.ToString(Constants.DATE_FORMATTING_YEAR_MONTH_DAY)}.docx";

            // Get SectionsMVs
            ICollection<SectionModelView> sections = this.sectionLoader.GetAll(revisionMV);

            // No prefix
            int refNumberPrefixLevel = 0;

            // Get Rates
            ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetRatesByRevision(revisionMV);

            // Get File Attachments
            ICollection<FileAttachmentRowModelView> fileAttachments = this.fileAttachmentLoader.GetByRevision(revisionMV.Id);

            // TODO - RDM 1.0 - Update to allow user to select number of years
            this.pprdExporter.ExportFullPPRDToWordFile(sections, rates, fileAttachments, serverFileName, clientFileName, revisionMV, CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY, httpResponse, refNumberPrefixLevel);
        }

        /// <summary>
        /// Generates a file containing revision data as JSON.
        /// </summary>
        /// <param name="id">Revision Id to export</param>
        /// <param name="jsonFilePath">The server path where the JSON file will be created</param>
        public ExportFileDownloadResult ExportRevisionAsJson(string id, string jsonFilePath)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Get Revision MV
            int revisonId;
            RevisionModelView revision;

            if (int.TryParse(id, out revisonId))
            {
                revision = this.RevisionMediator.GetById(revisonId);
            }
            else
            {
                throw new ArgumentException("Revision ID is invalid");
            }

            string currentUserDisplayName = this.AdUtils.GetUserByQualifiedAccount(this.SecurityInformation.ActiveUserNTID, false).DisplayName;
            ICollection<SectionModelView> sections = this.sectionLoader.GetAll(revision);
            ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetRatesByRevision(revision);
            ICollection<CobraDetailModelView> cobraDetails = this.cobraDetailLoader.GetCobraDetailsByRevision(revision);
            BurdenPoolGridModelView burdenPoolGridModel = this.burdenPoolLoader.GetByRevision(revision.Id);

            this.rdmRevisionExporter.ExportRevisionAsJson(currentUserDisplayName, revision, sections, rates, cobraDetails, burdenPoolGridModel, jsonFilePath);
            string fileDownloadName = $"Revision{revision.Revision}_{DateTime.Today.ToString(Constants.DATE_FORMATTING_YEAR_MONTH_DAY)}.json";
            return new ExportFileDownloadResult(jsonFilePath, fileDownloadName);
        }
    }
}
