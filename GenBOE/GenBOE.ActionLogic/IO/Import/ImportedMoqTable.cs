// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;

    /// <summary>
    /// MOQ Table for import, including Import Types
    /// </summary>
    public class ImportedMoqTable : MoqTableData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportedMoqTable()
            : base()
        {
            ImportTypes = new Collection<MoqTableImportType>();
        }

        /// <summary>
        /// Import types for the MOQ Table
        /// </summary>
        public ICollection<MoqTableImportType> ImportTypes { get; set; }
    }
}
