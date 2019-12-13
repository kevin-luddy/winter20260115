// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    /// <summary>
    /// Model View used for RMS SSRS Summary Reports.  Due to SSRS limitations, some of these fields will be identical
    /// across multiple or even all of the records in the ModelView.
    /// </summary>
    public class SummaryReportRMSModelView
    {
        /// <summary>
        /// The label  for the Major Grouping on the report.
        /// </summary>
        public string MajorGroupingLabel { get; set; }

        /// <summary>
        /// The label for the Minor Grouping on the report.
        /// </summary>
        public string MinorGroupingLabel { get; set; }

        /// <summary>
        /// The Report Title.
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// The name of the Project.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// The text for the Major Grouping on the report.
        /// </summary>
        public string MajorGroupingText { get; set; }

        /// <summary>
        /// The text for the Minor Grouping on the report.
        /// </summary>
        public string MinorGroupingText { get; set; }

        /// <summary>
        /// The Activity ID.
        /// </summary>
        public string ActivityID { get; set; }

        /// <summary>
        /// The Activity Name.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// The Resource.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The Resource Name.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// The Start Date.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// The End Date.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// The Labor Hours.
        /// </summary>
        public string LaborHrs { get; set; }

        /// <summary>
        /// The Material Cost.
        /// </summary>
        public string MatlCost { get; set; }
    }
}
