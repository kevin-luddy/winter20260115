// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    using System;

    /// <summary>
    /// Model View used for PRP Report.  Due to SSRS limitations, some of these fields will be identical
    /// across multiple or even all of the records in the ModelView.
    /// </summary>
    public class PRPModelView
    {
        /// <summary>
        /// CLIN
        /// </summary>
        public string CLIN { get; set; }

        /// <summary>
        /// WBS
        /// </summary>
        public string WBS { get; set; }

        /// <summary>
        /// Activity ID
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Resource Type (e.g. labor/material)
        /// This property will be driven by ResourceTypeDto.SpreadType
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Resource
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Cost Center
        /// </summary>
        public string CostCenter { get; set; }

        /// <summary>
        /// Cost Center Description
        /// </summary>
        public string CostCenterDescription { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Number between 1 and 204 representing the month 
        /// number within the 17 year report period
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Value for associated month (hours or dollars)
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Start Date for ResourceTypeDto (not specific month)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date for ResourceTypeDto (not specific month)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// January of report minimum year
        /// </summary>
        public DateTime ReportStartDate { get; set; }

        /// <summary>
        /// Workspace title displayed at top of report
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Used to format numbers with correct unit (e.g. $ or none for hours)
        /// </summary>
        public string ResourceUnit { get; set; }
    }
}