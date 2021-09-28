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
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public class WbsExporter
    {
        /// <summary>
        /// Exports Wbs to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the Wbs Excel file template</param>
        /// <param name="wbsDTOs">The WBS dt os.</param>
        /// <param name="clinStrings">full workspace</param>
        /// <returns>Path to the exported Wbs file</returns>
       public string ExportToExcelFile(string templateFileLocation, Collection<WbsDTO> wbsDTOs, Dictionary<int, string> clinStrings)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (wbsDTOs == null)
            {
                throw new ArgumentNullException(nameof(wbsDTOs));
            }

            if (clinStrings == null)
            {
                throw new ArgumentNullException(nameof(clinStrings));
            }
            
            // Create collections of strings for each row in the export file
            ExcelExportWorksheet worksheet = this.GetExcelExportWorksheet(wbsDTOs, clinStrings);

            // Pass the rows to the generic Excel exporter           
            string toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file path
            return toReturn;
        }

        /// <summary>
        /// Gets the excel export worksheet.
        /// </summary>
        /// <param name="wbsDTOs">The WBS dt os.</param>
        /// <param name="clinStrings">The clin strings.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// wbsDTOs or clinStrings</exception>
        
        public ExcelExportWorksheet GetExcelExportWorksheet(Collection<WbsDTO> wbsDTOs, Dictionary<int, string> clinStrings)
        {
            if (wbsDTOs == null)
            {
                throw new ArgumentNullException(nameof(wbsDTOs));
            }

            if (clinStrings == null)
            {
                throw new ArgumentNullException(nameof(clinStrings));
            }

            ExcelExportWorksheet toReturn = new ExcelExportWorksheet();

            if (wbsDTOs.Any())
            {
                foreach (WbsDTO wbs in wbsDTOs)
                {
                    string clinString;
                    clinStrings.TryGetValue(wbs.Id, out clinString);
                    
                    toReturn.Add(
                        wbs.Id.ToString(),
                        wbs.WbsNumber,
                        CommonConstants.FORCE_AS_STRING_VALUE + wbs.WbsTitle,
                        CommonConstants.FORCE_AS_STRING_VALUE + clinString
                    );
                }
            }

            return toReturn;
        }
    }
}
