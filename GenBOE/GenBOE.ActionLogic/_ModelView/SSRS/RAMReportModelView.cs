// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    /// <summary>
    /// Model View used for SSRS RAM Report
    /// </summary>
    public class RAMReportModelView
    {
        /// <summary>
        /// Gets/sets the CLIN
        /// </summary>
        public string Clin { get; set; }

        /// <summary>
        /// Gets/sets the WBS
        /// </summary>
        public string Wbs { get; set; }

        /// <summary>
        /// Gets/sets the Activity ID
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets/sets the Activity Name
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Gets/sets the Resource and Cost Center
        /// Should be formatted as "[Resource], [Cost Center]"
        /// </summary>
        public string ResourceCostCenter { get; set; }

        /// <summary>
        /// Gets/sets the hour or cost value
        /// </summary>
        public string Value { get; set; }
    }
}
