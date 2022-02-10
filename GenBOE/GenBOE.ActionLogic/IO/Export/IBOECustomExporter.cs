// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public interface IBOECustomExporter
    {
        /// <summary>
        /// Sets the WorkspaceDecimalPrecision and DefaultHoursFormat variables for the export
        /// </summary>
        /// <param name="ws">Workspace containing BOE(s) in export</param>
        void SetWorkspacePrecisionVariables(WorkspaceDTO ws);

        /// <summary>
        /// This function will populate the BOEExportModelViews based on the BOE DTOs.
        /// </summary>
        /// <param name="boes">Full Boes.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// the BOE Export ModelViews.
        /// </returns>
        ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(ICollection<FullBoe> boes, BOEExportInputs exportInputs);

        /// <summary>
        /// Export data about the given BOE into a pre-formatted Word template and return the file path of
        /// the populated template.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
        /// <param name="ws">Full WS</param>
        /// <param name="components">List of selected components</param>
        /// <param name="Response">the web response object to write the file back to for user download</param>
        /// <param name="fileNameToDisplayToBrowser">the file name to display to the browser in the download dialog</param>
        /// <param name="exportFormat">Export file info</param>
        void ExportBOEToWordFile(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
            FullWorkspace ws, ICollection<BoeCustomReportComponent> components, HttpResponseBase Response, string fileNameToDisplayToBrowser, WorkspaceExportFormatDTO exportFormat);

        /// <summary>
        /// Export data about the given BOE into a pre-formatted Word template and return the file path of
        /// the populated template.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
        /// <param name="ws">Full WS</param>
        /// <param name="components">List of selected components</param>
        /// <param name="returnStream">Output stream</param>
        /// <param name="exportFormat">Export file info</param>
        bool ExportBOEToWordFileStream(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews, FullWorkspace ws,
            ICollection<BoeCustomReportComponent> components, Stream returnStream, WorkspaceExportFormatDTO exportFormat);
        
        /// <summary>
        /// IES-707: Creates individual word documents for each BOE and compresses them into a single zip file.
        /// </summary>
        /// <param name="exportInputs">The export inputs</param>
        /// <param name="boeExportModelViews">Collection of BOE View Models</param>
        /// <param name="boeSummaryGridModelViews"></param>
        /// <param name="workSpace">Full workspace</param>
        /// <param name="selectedComponents"></param>
        /// <param name="httpResponse">What will ultimately be the response to the requester</param>
        /// <param name="returnFilename">File name that will be passed to browser (for download)</param>
        /// <param name="exportFormat">Export parameters</param>
        void ExportBOEsToZipFile(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
            FullWorkspace workSpace, ICollection<BoeCustomReportComponent> components, HttpResponseBase response, string returnFilename, WorkspaceExportFormatDTO exportFormat);
    }
}
