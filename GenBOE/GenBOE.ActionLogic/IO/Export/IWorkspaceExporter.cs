// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using IES.Common.OfficeUtilities;

    public interface IWorkspaceExporter
    {
        string WORKSPACE_DATA_EXCEL_MAP_PATH { get; }

        string ExportToExcelFile(string inTemplateFileLocation, int inWorkspaceID);
        
        ExcelExportWorksheet GetWorkspaceIdentificationSheetExportData(int inWorkspaceID);

    }
}
