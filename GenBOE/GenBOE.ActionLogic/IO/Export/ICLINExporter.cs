// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using GenBOE.Dtos;
	using IES.Common.OfficeUtilities;
	using IES.Common.PickList;

	/// <summary>
	/// Interface for CLIN exporters
	/// </summary>
	public interface ICLINExporter
    {
        /// <summary>
        /// Exports CLINs to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Clin Excel file template</param>
        /// <param name="clinDTOs">The collection of CLINs to export</param>
        /// <param name="workspace">The workspace for the CLINs.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns>
        /// Path to the exported Clin file
        /// </returns>
        string ExportToExcelFile(string templateFileLocation, Collection<ClinDTO> clinDTOs, WorkspaceDTO workspace, ICollection<PickListDto> contractTypes);

        /// <summary>
        /// Gets the excel export worksheet.
        /// </summary>
        /// <param name="clinDTOs">The clin dtos.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns>New Excel Export worksheet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "ExcelExportWorksheet's base class is List<T> but Collection<T> does not have AddRange()")]
        ExcelExportWorksheet GetExcelExportWorksheet(Collection<ClinDTO> clinDTOs, ICollection<PickListDto> contractTypes);
    }        
}