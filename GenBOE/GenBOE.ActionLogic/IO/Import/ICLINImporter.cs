// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using GenBOE.Objects;
    using IES.Common.PickList;

    /// <summary>
    /// Interface for importing CLINs from Excel document.
    /// </summary>
    public interface ICLINImporter
    {
        /// <summary>
        /// Imports the clins from excel file.
        /// </summary>
        /// <param name="excelFileStream">The excel file stream.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="allContractTypes">All of the Contract Types.</param>
        /// <returns>A collection of imported CLINs.</returns>
        Collection<ImportedClin> ImportClinsFromExcelFile(Stream excelFileStream, FullWorkspace workspace, ICollection<PickListDto> allContractTypes);
    }
}
