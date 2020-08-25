// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System.Collections.Generic;
    using System.Linq;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ExcelExportWorksheet : List<ICollection<string>>
    {
        public string WorksheetName { get; set; }

        public ExcelExportWorksheet()
            : base()
        {
            WorksheetName = string.Empty;
        }

        public ExcelExportWorksheet(string inWorksheetName)
            : this()
        {
            WorksheetName = inWorksheetName;
        }

        public void Add(params string[] cells)
        {
            this.Add(cells.ToList());
        }
    }
}
