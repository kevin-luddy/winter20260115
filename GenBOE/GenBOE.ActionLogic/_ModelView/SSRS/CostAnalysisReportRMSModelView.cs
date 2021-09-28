// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    /// <summary>
    /// Model View used for RMS SSRS Cost Analysis Reports.  Due to SSRS limitations, some of these fields will be identical
    /// across multiple or even all of the records in the ModelView.
    /// </summary>
    public class CostAnalysisReportRMSModelView
    {
        /// <summary>
        /// The Report Title.
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// The name of the Project.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// The CLIN.
        /// </summary>
        public string CLIN { get; set; }

        /// <summary>
        /// The Resource ID.
        /// </summary>
        public string ResourceID { get; set; }

        /// <summary>
        /// The Resource Description.
        /// </summary>
        public string ResourceDescription { get; set; }

        /// <summary>
        /// Gets or sets the cost center.
        /// </summary>
        public string CostCenter { get; set; }

        /// <summary>
        /// The Performing Org Description.
        /// </summary>
        public string PerformingOrgDescription { get; set; }

        /// <summary>
        /// The unit represented by the resource (e.g. Hours, Direct Dollars, etc.).  Should match one of the options from
        /// the IES.Common.Constants class, such as COST_ANALYSIS_RESOURCE_UNIT_HOURS or COST_ANALYSIS_RESOURCE_UNIT_DIRECT_DOLLARS.
        /// </summary>
        public string ResourceUnit { get; set; }

        /// <summary>
        /// The Category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The Activity ID.
        /// </summary>
        public string ActivityID { get; set; }

        /// <summary>
        /// The WBS.
        /// </summary>
        public string WBS { get; set; }

        /// <summary>
        /// Gets or sets the WBS title.
        /// </summary>
        public string WbsTitle { get; set; }

        /// <summary>
        /// Gets or sets the percent tiered.
        /// </summary>
        public string TieredPercentage { get; set; }

        /// <summary>
        /// The Activity Name.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// The Class of Cost.
        /// </summary>
        public string ClassOfCost { get; set; }

        /// <summary>
        /// The Start Year, which corresponds to Year01.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        /// <value>
        /// The type of the resource.
        /// </value>
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the legacy resource.
        /// </summary>
        public string LegacyResource { get; set; }

        /// <summary>
        /// Gets or sets the name of the legacy resource.
        /// </summary>
        /// <value>
        /// The name of the legacy resource.
        /// </value>
        public string LegacyResourceName { get; set; }

        /// <summary>
        /// Gets or sets the sow title.
        /// </summary>
        public string SOWTitle { get; set; }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        public string Task { get; set; }

        /// <summary>
        /// Gets or sets the add delete.
        /// </summary>
        public string AddDelete { get; set; }

        /// <summary>
        /// The Value for Year 1, which is the StartYear.
        /// </summary>
        public decimal Year01 { get; set; }

        /// <summary>
        /// The Value for Year 2.
        /// </summary>
        public decimal Year02 { get; set; }

        /// <summary>
        /// The Value for Year 3.
        /// </summary>
        public decimal Year03 { get; set; }

        /// <summary>
        /// The Value for Year 4.
        /// </summary>
        public decimal Year04 { get; set; }

        /// <summary>
        /// The Value for Year 5.
        /// </summary>
        public decimal Year05 { get; set; }

        /// <summary>
        /// The Value for Year 6.
        /// </summary>
        public decimal Year06 { get; set; }

        /// <summary>
        /// The Value for Year 7.
        /// </summary>
        public decimal Year07 { get; set; }

        /// <summary>
        /// The Value for Year 8.
        /// </summary>
        public decimal Year08 { get; set; }

        /// <summary>
        /// The Value for Year 9.
        /// </summary>
        public decimal Year09 { get; set; }

        /// <summary>
        /// The Value for Year 10.
        /// </summary>
        public decimal Year10 { get; set; }

        /// <summary>
        /// The Value for Year 11.
        /// </summary>
        public decimal Year11 { get; set; }

        /// <summary>
        /// The Value for Year 12.
        /// </summary>
        public decimal Year12 { get; set; }

        /// <summary>
        /// The Value for Year 13.
        /// </summary>
        public decimal Year13 { get; set; }

        /// <summary>
        /// The Value for Year 14.
        /// </summary>
        public decimal Year14 { get; set; }

        /// <summary>
        /// The Value for Year 15.
        /// </summary>
        public decimal Year15 { get; set; }

        /// <summary>
        /// The Value for Year 16.
        /// </summary>
        public decimal Year16 { get; set; }

        /// <summary>
        /// The Value for Year 17.
        /// </summary>
        public decimal Year17 { get; set; }
    }
}
