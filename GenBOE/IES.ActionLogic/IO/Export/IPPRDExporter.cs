// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Export
{
	using System.Collections.Generic;
	using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
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
		/// <param name="refNumberPrefixLevel">The prefix Level for the Reference Numbers.</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		Task ExportFullPPRDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, string clientFileName, RevisionModelView revision, int rateTableYears, HttpResponseBase response, int refNumberPrefixLevel, bool? portionMarkingRequired);

        /// <summary>
        /// Generate a Word document containing the RDD sections and rates.
        /// </summary>
        /// <param name="sections">Collection of Section MVs</param>
        /// <param name="rates">Collection of RateDetail MVs</param>
        /// <param name="fileAttachments">Collection of File Attachment MVs</param>
        /// <param name="serverFileName">Server path to new file to generate.</param>
        /// <param name="revision">Revision modelview</param>
        /// <param name="rddDocument">The RDD modelview to create the Word Export from.</param>
        /// <param name="stream">the stream to write the file back to for user download</param>
        /// <param name="refNumberPrefixLevel">The prefix Level for the Reference Numbers.</param>
        /// <param name="includeDocumentDetails">If document details (introduction, clarification, table of contents) should be included in the export</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
        Task ExportRDDToWordFile(ICollection<SectionModelView> sections, ICollection<RateDetailModelView> rates, ICollection<FileAttachmentRowModelView> fileAttachments, string serverFileName, RevisionModelView revision, DocumentDetailModelView rddDocument, Stream stream, int refNumberPrefixLevel, bool? portionMarkingRequired, bool includeDocumentDetails = true);
    }
}
