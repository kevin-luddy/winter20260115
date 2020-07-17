// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    using System;

    /// <summary>
    /// Model View used for RPS Report.  Due to SSRS limitations, some of these fields will be identical
    /// across multiple or even all of the records in the ModelView.
    /// </summary>
    public class RPSReportModelView
    {
        /// <summary>
        /// The name of the Project.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// The Resource.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets the cost center.
        /// </summary>
        public string CostCenter { get; set; }

        /// <summary>
        /// The Cost Center Description.
        /// </summary>
        public string CostCenterDescription { get; set; }

        /// <summary>
        /// The Resource Type.
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// The Offload Rate.
        /// </summary>
        public decimal OffloadRate { get; set; }

        /// <summary>
        /// The Month of the corresponding data.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// The Value of the corresponding data.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// The Start Date corresponding to the first monthly summary column (January of the year of the earliest Resource Start Date).  Other column headers will be calculated off of this.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The unit represented by the resource (e.g. Hours, Dollars, etc.).  Should match one of the options from
        /// the IES.Common.Constants class, such as RPS_RESOURCE_UNIT_HOURS or RPS_RESOURCE_UNIT_DOLLARS.
        /// </summary>
        public string ResourceUnit { get; set; }
    }
}
