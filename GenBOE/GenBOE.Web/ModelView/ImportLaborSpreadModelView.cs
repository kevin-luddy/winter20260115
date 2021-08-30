// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;

namespace GenBOE.Web.ModelView
{
    /// <summary>
    /// Represents the labor spread information.
    /// </summary>
    public class ImportLaborSpreadModelView
    {
        /// <summary>
        /// Id of the labor type
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// One or more import types
        /// </summary>
        public Collection<int> ImportTypes { get; set; }

        /// <summary>
        /// Labor Spread Date
        /// </summary>
        public DateTime LaborSpreadDate { get; set; }

        /// <summary>
        /// Gets or sets the labor type identifier for import.
        /// </summary>
        public int? LaborTypeIDForImport { get; set; }

        /// <summary>
        /// Gets or sets the labor spread value.
        /// </summary>
        public decimal LaborSpreadValue { get; set; }
    }
}
