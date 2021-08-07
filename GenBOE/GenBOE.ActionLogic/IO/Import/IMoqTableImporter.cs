// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System.Collections.Generic;
    using System.IO;
    using GenBOE.Objects;

    public interface IMoqTableImporter
    {
        /// <summary>
        /// Import MOQ Tables from Excel file
        /// </summary>
        /// <param name="excelFileStream">Excel file stream</param>
        /// <param name="ws">workspace</param>
        /// <returns>imported MOQ Tables</returns>
        ICollection<ImportedMoqTable> ImportMoqTableFromExcelFile(Stream excelFileStream, FullWorkspace ws);
    }
}
