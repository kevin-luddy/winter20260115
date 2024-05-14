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
    /// Represents the labor type information extracted from a labor type import excel file.
    /// </summary>
    public class ImportLaborTypeModelView
    {
        /// <summary>
        /// Id of the labor type
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id of the boe associated with the labor type
        /// </summary>
        public int BoeID { get; set; }

        /// <summary>
        /// Id of the spread curve used by the labor type
        /// </summary>
        public int SpreadCurveID { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Percent spread
        /// </summary>
        public decimal? PercentSpread { get; set; }

        /// <summary>
        /// Performing org id
        /// </summary>
        public int PerformingOrgID { get; set; }

        /// <summary>
        /// Id of the resource associated with the labor type
        /// </summary>
        public int ResourceID { get; set; }

		/// <summary>
		/// Id of the business resource code with the labor type
		/// </summary>
		public int BusinessResourceCodeID { get; set; }

        /// <summary>
        /// Id of the clin associated with the labor type
        /// </summary>
        public int? ClinID { get; set; }

        /// <summary>
        /// Id of the wbs associated with the labor type
        /// </summary>
        public int? WbsID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can offload.
        /// </summary>
        public bool CanOffload { get; set; }

        /// <summary>
        /// Gets or sets the tiered percentage value for project map workspaces.
        /// </summary>
        public decimal? TieredPercentage { get; set; }

        /// <summary>
        /// Value spread
        /// </summary>
        public decimal? ValueSpread { get; set; }

        /// <summary>
        /// One or more import types
        /// </summary>
        public Collection<int> ImportTypes { get; set; }

        /// <summary>
        ///  Percent Spread Locked, true is locked, false is unlocked
        /// </summary>
        public bool PercentSpreadLocked { get; set; }

        /// <summary>
        /// Hour Spread Locked, true if locked, false is unlocked
        /// </summary>
        public bool HourSpreadLocked { get; set; }

        /// <summary>
        /// Gets or sets the imported labor spreads.
        /// </summary>
        public Collection<ImportLaborSpreadModelView> ImportedLaborSpreads { get; set; }

        /// <summary>
        /// Gets or sets the custom field value containers.
        /// </summary>
        public Collection<CustomFieldImportModelView> CustomFieldValueContainers { get; set; }
    }
}
