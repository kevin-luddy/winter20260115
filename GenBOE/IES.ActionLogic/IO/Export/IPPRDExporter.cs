// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Export
{
    using System.Collections.Generic;
    using System.Web;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for PPRD Exporter
    /// </summary>
    public interface IPPRDExporter
    {
        /// <summary>
        /// Generate a Word document containing the full PPRD.
        /// </summary>
        /// <param name="sections">Collection of Section MVs</param>
        /// <param name="rates">Collection of RateDetail MVs</param>
        /// <param name="fileAttachments">Collection of File Attachment MVs</param>
        /// <param name="serverFileName">Server path to new file to generate.</param>
        /// <param name="clientFileName">the file name to display to the browser in the download dialog</param>
        /// <param name="revision">Revision modelview</param>
        /// <param name="rateTableYears">Number of years to include in the rate tables</param>
        /// <param name="response">the web response object to write the file back to for user download</param>
        void ExportFullPPRDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, string clientFileName, RevisionModelView revision, int rateTableYears, HttpResponseBase response);

        /// <summary>
        /// Generate a Word document containing the RDD sections and rates.
        /// </summary>
        /// <param name="sections">Collection of Section MVs</param>
        /// <param name="rates">Collection of RateDetail MVs</param>
        /// <param name="fileAttachments">Collection of File Attachment MVs</param>
        /// <param name="serverFileName">Server path to new file to generate.</param>
        /// <param name="clientFileName">the file name to display to the browser in the download dialog</param>
        /// <param name="revision">Revision modelview</param>
        /// <param name="rddDocument">The RDD modelview to create the Word Export from.</param>
        /// <param name="response">the web response object to write the file back to for user download</param>
        /// <param name="includeDocumentDetails">If document details (introduction, clarification, table of contents) should be included in the export</param>
        void ExportRDDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, string clientFileName, RevisionModelView revision, DocumentDetailModelView rddDocument, HttpResponseBase response, bool includeDocumentDetails = true);
    }
}
