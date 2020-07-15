// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System.Collections.ObjectModel;
    using System.IO;
    using GenBOE.Objects;

    public interface IBOEImporter
    {
        Collection<ImportedBoe> ImportBoeFromExcelFile(Stream excelFileStream, FullWorkspace workspace);
    }
}
