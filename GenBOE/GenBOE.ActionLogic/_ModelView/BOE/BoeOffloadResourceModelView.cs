// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    /// <summary>
    /// Model View housing data for a specific Resource offload.
    /// </summary>
    public class BoeOffloadResourceModelView
    {
        /// <summary>
        /// Gets or sets the WBS for Multi-Clin only.
        /// </summary>
        public string Wbs { get; set; }

        /// <summary>
        /// Gets or sets the Clin for Multi-Clin only.
        /// </summary>
        public string Clin { get; set; }
        
        /// <summary>
        /// Gets or sets the existing resource.
        /// </summary>
        public string ExistingResource { get; set; }

        /// <summary>
        /// Gets or sets the offloaded resource.
        /// </summary>
        public string OffloadedResource { get; set; }

        /// <summary>
        /// Gets or sets the performing org.
        /// </summary>
        public string PerformingOrg { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the existing spread curve.
        /// </summary>
        public string ExistingSpreadCurve { get; set; }

        /// <summary>
        /// Gets or sets the existing modified hours.
        /// </summary>
        public string ExistingModifiedHours { get; set; }

        /// <summary>
        /// Gets or sets the total hours offloaded.
        /// </summary>
        public string TotalHoursOffloaded { get; set; }

        /// <summary>
        /// Gets or sets the total offloaded cost.
        /// </summary>
        public string TotalOffloadedCost { get; set; }

        /// <summary>
        /// Gets or sets the offload spreads.
        /// </summary>
        public ICollection<ResourceSpreadDto> OffloadSpreads { get; set; }

        /// <summary>
        /// Gets or sets the offload spreads.
        /// </summary>
        public ICollection<ResourceSpreadDto> OffloadHours { get; set; }

        /// <summary>
        /// Gets or sets the existing spreads.
        /// </summary>
        public ICollection<ResourceSpreadDto> ExistingSpreads { get; set; }
    }
}
