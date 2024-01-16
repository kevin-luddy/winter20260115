// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common.Core;
    using IES.Common.Core.OfficeUtilities;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// The Cobra Exporter.
    /// </summary>
    public static class CobraExporter
    {
        /// <summary>
        /// Exports the Cobra data to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The template file path.</param>
        /// <param name="cobraExportRows">A collection of CobraExportRowModelViews to be exported.</param>
        /// <returns>The file path.</returns>
        public static string ExportToExcelFile(string templateFileLocation, ICollection<CobraExportRowModelView> cobraExportRows)
        {
            if (string.IsNullOrEmpty(templateFileLocation))
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            ExcelExportWorksheet worksheet = new ExcelExportWorksheet();

            // NOTE:  Formatting on the Date and Value columns is also being done directly in the template file itself.
            worksheet.AddRange(from cobraExportRow in cobraExportRows
                               select new Collection<string>
                               {
                                   cobraExportRow.RateSet,
                                   cobraExportRow.Code1,
                                   cobraExportRow.Description,
                                   cobraExportRow.Date.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR),
                                   cobraExportRow.Value == null ? string.Empty : cobraExportRow.Value.GetValueOrDefault().ToString(Constants.FIXED_POINT_FORMATTING_SIX_DECIMAL_PLACES)
                               });

            return ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);
        }
    }
}
