// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System.IO;
    using GenBOE.Objects;

    public interface IWorkOfflineImporter
    {
        /// <summary>
        /// Imports from excel file
        /// </summary>
        /// <param name="excelFileStream">excel file stream</param>
        /// <param name="ws">ws for which the import is happening</param>
        /// <returns>imported data</returns>
        WorkofflineImport ImportFromExcelFile(Stream excelFileStream, FullWorkspace ws);
    }
}
