// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using IES.Common.PickList;

    [ExcludeFromCodeCoverage]
    public class CLINExporter : ICLINExporter
    {
        /// <summary>
        /// Exports CLINs to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Clin Excel file template</param>
        /// <param name="clinDTOs">The collection of CLINs to export</param>
        /// <param name="workspace">The workspace for the CLINs.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns>Path to the exported Clin file</returns>
        public string ExportToExcelFile(string templateFileLocation, Collection<FullClin> clinDTOs, WorkspaceDTO workspace, ICollection<PickListDto> contractTypes)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }
            if (clinDTOs == null)
            {
                throw new ArgumentNullException(nameof(clinDTOs));
            }

            var toReturn = string.Empty;

            var worksheet = this.GetExcelExportWorksheet(clinDTOs, contractTypes);

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);            

            // Return the file path
            return toReturn;
        }

        /// <summary>
        /// Gets the excel export worksheet.
        /// </summary>
        /// <param name="clinDTOs">The clin dt os.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "ExcelExportWorksheet's base class is List<T> but Collection<T> does not have AddRange()")]
        public ExcelExportWorksheet GetExcelExportWorksheet(Collection<FullClin> clinDTOs, ICollection<PickListDto> contractTypes)
        {
            if (clinDTOs == null)
            {
                throw new ArgumentNullException(nameof(clinDTOs));
            }

            var toReturn = new ExcelExportWorksheet();

            if (clinDTOs.Count > 0)
            {
                foreach (var clin in clinDTOs)
                {
                    toReturn.Add(
                        clin.Id.ToString(),
                        CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinNumber,
                        CommonConstants.FORCE_AS_STRING_VALUE + clin.ClinTitle,
                        String.Format("{0:M/yyyy}", clin.StartDate),
                        String.Format("{0:M/yyyy}", clin.EndDate)
                        );
                }
            }

            return toReturn;
        }
    }
}
