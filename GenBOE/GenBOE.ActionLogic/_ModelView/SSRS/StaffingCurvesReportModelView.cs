// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    using System;

    /// <summary>
    /// Model View used for SSRS Staffing Curves Reports.
    /// </summary>
    public class StaffingCurvesReportModelView
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the perf org.
        /// </summary>
        public string PerfOrg { get; set; }


        /// <summary>
        /// Gets or sets the Number of Full Time Equivalents.
        /// </summary>
        public decimal FTE { get; set; }
    }
}
