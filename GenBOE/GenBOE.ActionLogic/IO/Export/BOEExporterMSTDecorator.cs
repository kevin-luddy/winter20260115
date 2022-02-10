// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using GenBOE.ActionLogic.Common;
using GenBOE.ActionLogic.IO.Export.BOE;
using GenBOE.Dtos;
using GenBOE.Objects;
using IES.Common;

namespace GenBOE.ActionLogic.IO.Export
{
    /// <summary>
    /// This decorator determines the methods to be used for exporting templates in MST based on the template ID.
    /// Due to ISGS templates being used in MST, it must be determined which version of certain methods is to be used due to certain ones being overridden for MST templates.
    /// If an ISGS template is used, the base versions of methods overridden in BOEExportMST need to be used. Otherwise the override method will be used as it was when there were only MST templates.
    /// </summary>
    public class BOEExporterMSTDecorator : IBOEExporter
    {
        /// <summary>
        /// The base exporter.
        /// </summary>
        private IBOEExporter baseExporter;
        
        /// <summary>
        /// The MST exporter.
        /// </summary>
        private IBOEExporter mstExporter;

        public BOEExporterMSTDecorator(IBOEExporter baseExporter, IBOEExporter mstExporter)
        {
            this.baseExporter = baseExporter;
            this.mstExporter = mstExporter;
        }

        /// <summary>
        /// Sets the WorkspaceDecimalPrecision and DefaultHoursFormat variables for the export
        /// </summary>
        /// <param name="workspace">Workspace containing BOE(s) in export</param>
        public void SetWorkspacePrecisionVariables(WorkspaceDTO workspace)
        {
            //Same for both ISGS and MST Templates
            this.mstExporter.SetWorkspacePrecisionVariables(workspace);
            this.baseExporter.SetWorkspacePrecisionVariables(workspace);
        }

        /// <summary>
        /// This function will populate the BOEExportModelViews based on the BOE DTOs.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// the BOE Export ModelViews.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">exportInputs</exception>
        public ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(BOEExportInputs exportInputs)
        {
            if(exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }
            // The workspace's export format doesn't have the correct template type if this is a user template so always use the exportFormatDTO
            WorkspaceExportFormatDTO exportFormatDTO = exportInputs.WorkspaceExportFormats.FirstOrDefault(x => x.Id == exportInputs.Workspace.TemplateID);

            if (exportFormatDTO != null && (int)exportFormatDTO.ExportFormat.TemplateType < 1001)
            {
                return this.baseExporter.ConvertBoeDTOsToExportMVs(exportInputs);
            }
            else
            {
                return this.mstExporter.ConvertBoeDTOsToExportMVs(exportInputs);
            }
        }

        /// <summary>
        /// Export data about the given BOE into a pre-formatted Word template and return the file path of
        /// the populated template.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
        /// <param name="ws">Full WS</param>
        /// <param name="response">the web response object to write the file back to for user download</param>
        /// <param name="fileNameToDisplayToBrowser">the file name to display to the browser in the download dialog</param>
        /// <param name="templatePath">Physical path of the template to copy and populate.</param>
        /// <param name="templateType">Template type</param>
        public void ExportBOEToWordFile(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews, 
            FullWorkspace ws, HttpResponseBase response, string fileNameToDisplayToBrowser, string templatePath, ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
        {
            if((int)templateType < 1001)
            {
                this.baseExporter.ExportBOEToWordFile(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, response, fileNameToDisplayToBrowser, templatePath, templateType);
            }
            else
            {
                this.mstExporter.ExportBOEToWordFile(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, response, fileNameToDisplayToBrowser, templatePath, templateType);
            }
        }

        /// <summary>
        /// Export data about the given BOE into a pre-formatted Word template and return the file path of
        /// the populated template.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="boeExportModelViews">Object to hold most of the BOE's data</param>
        /// <param name="boeSummaryGridModelViews">Object to hold data for the BOE Summary Grid</param>
        /// <param name="ws">Full WS</param>
        /// <param name="templatePath">Physical path of the template to copy and populate.</param>
        /// <param name="returnStream">Output stream</param>
        /// <param name="templateType">Template type</param>
        public bool ExportBOEToWordFileStream(BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
            FullWorkspace ws, string templatePath, Stream returnStream, ExcelReportTemplateType templateType)
        {
            if((int)templateType < 1001)
            {
                this.baseExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, templatePath, returnStream, templateType);
            }
            else
            {
                this.mstExporter.ExportBOEToWordFileStream(exportInputs, boeExportModelViews, boeSummaryGridModelViews, ws, templatePath, returnStream, templateType);
            }

            return true;
        }

        /// <summary>
        /// Exports the BOEs to Excel.
        /// </summary>
        /// <param name="templateFileLocation">Template file location.</param>
        /// <param name="workspace">Full Workspace.</param>
        /// <param name="blankTemplate">Bool to determine if template should be blank or contain all BOEs</param>
        /// <returns></returns>
        public string ExportToExcelFile(string templateFileLocation, FullWorkspace workspace, bool blankTemplate)
        {
            //Same for both ISGS and MST Templates
            return this.mstExporter.ExportToExcelFile(templateFileLocation, workspace, blankTemplate);
        }

        /// <summary>
        /// IES-707: Creates individual documents for reach BOE and compresses them into a single zip file.
        /// </summary>
        /// <param name="exportInputs">The export inputs</param>
        /// <param name="boeExportModelViews">Collection of BOE View Models</param>
        /// <param name="boeSummaryGridModelViews"></param>
        /// <param name="workSpace">Full workspace</param>
        /// <param name="response">What will ultimately be the response to the requester</param>
        /// <param name="returnFilename">File name that will be passed to browser (for download)</param>
        /// <param name="templatePath">Server path to the export template</param>
        /// <param name="templateType">Template type</param>
        /// <exception cref="ArgumentNullException">if response or workspace is null</exception>
        public void ExportBOEsToZipFile(
            BOEExportInputs exportInputs,
            ICollection<BOEExportModelView> boeExportModelViews,
            ICollection<BOESummaryGridModelView> boeSummaryGridModelViews,
            FullWorkspace workSpace,
            HttpResponseBase response,
            string returnFilename,
            string templatePath,
            ExcelReportTemplateType templateType = ExcelReportTemplateType.NotSet)
        {
            AllBOEExportHelper.ExportBOEsToZipFile<bool>(exportInputs, boeExportModelViews, boeSummaryGridModelViews, workSpace, response, returnFilename, templatePath, ExportBOEToWordFileStream);
        }
    }
}
