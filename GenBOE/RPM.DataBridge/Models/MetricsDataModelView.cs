// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace RPM.DataBridge.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The metrics data model.
    /// </summary>
    public class MetricsDataModelView
    {
        /// <summary>
        /// Gets or sets the LOBs used in LOB filter dropdown.
        /// </summary>
        public ICollection<string> LOBs { get; set; }

        /// <summary>
        /// Gets or sets the program areas used in PA filter dropdown keyed by PA to LOB.
        /// </summary>
        public ICollection<ProgramAreaModelView> ProgramAreas { get; set; }

        /// <summary>
        /// Gets or sets the PTM proposals.
        /// </summary>
        public ICollection<MetricsModelView> Proposals { get; set; }
    }
}
