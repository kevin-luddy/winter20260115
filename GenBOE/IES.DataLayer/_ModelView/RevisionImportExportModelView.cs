// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Model View used for a Revision import/export.
    /// </summary>
    public class RevisionImportExportModelView
    {
        #region Constructors

        #endregion

        /// <summary>
        /// Gets or sets the user that generated the export
        /// </summary>
        public string GeneratedBy { get; set; }

        /// <summary>
        /// Gets or sets the Date/Time the export was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }

        /// <summary>
        /// Gets or sets the Revision
        /// </summary>
        public RevisionModelView Revision { get; set; }

        /// <summary>
        /// Gets or sets the Sections
        /// </summary>
        public ICollection<SectionModelView> Sections { get; set; }

        /// <summary>
        /// Gets or sets the Rates
        /// </summary>
        public ICollection<RateDetailModelView> Rates { get; set; }

        /// <summary>
        /// Gets or sets the COBRA details.
        /// </summary>
        public ICollection<CobraDetailModelView> CobraDetails { get; set; }

        /// <summary>
        /// Gets or sets the Burden Pool data
        /// </summary>
        public BurdenPoolGridModelView BurdenPoolGridModel { get; set; }
    }
}
