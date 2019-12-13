// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{
    using System;
    using System.Collections.Generic;
    using Common;

    /// <summary>
    /// Model View for Boe Offload page
    /// </summary>
    public class BoeOffloadModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoeOffloadModelView"/> class.
        /// </summary>
        public BoeOffloadModelView()
        {
            this.OffloadResources = new List<BoeOffloadResourceModelView>();
            this.OffloadWarnings = new List<OffloadLaborRatesValidationResults>();
            this.SpreadDates = new List<DateTime>();
        }

        /// <summary>
        /// Gets or sets the boe identifier.
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// Gets or sets the WBS.
        /// </summary>
        public string Wbs { get; set; }

        /// <summary>
        /// Gets or sets the clin.
        /// </summary>
        public string Clin { get; set; }

        /// <summary>
        /// Gets or sets the resource hours precision.
        /// </summary>
        public int? ResourceHoursPrecision { get; set; }

        /// <summary>
        /// Gets or sets the cost precision.
        /// </summary>
        public int CostPrecision { get; set; }

        /// <summary>
        /// Gets or sets the offload resources.
        /// </summary>
        public ICollection<BoeOffloadResourceModelView> OffloadResources { get; set; }

        /// <summary>
        /// Gets or sets the offload warnings.
        /// </summary>
        public ICollection<OffloadLaborRatesValidationResults> OffloadWarnings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi clin WBS.
        /// </summary>
        public bool IsMultiClinWbs { get; set; }

        /// <summary>
        /// Gets or sets the spread dates.
        /// </summary>
        public ICollection<DateTime> SpreadDates { get; set; }

        /// <summary>
        /// Gets or sets the no results message.
        /// </summary>
        public string NoResultsMessage { get; set; }
    }
}