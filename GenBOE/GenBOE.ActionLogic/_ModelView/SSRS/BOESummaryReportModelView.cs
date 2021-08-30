// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    /// <summary>
    /// Model View used for BOE Summary Report.  Due to SSRS limitations, some of these fields will be identical
    /// across multiple or even all of the records in the ModelView.
    /// </summary>
    public class BOESummaryReportModelView
    {
        /// <summary>
        /// The name of the Project.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// The Report Start Year (all year-based columns will be in reference to this year).
        /// </summary>
        public int ReportStartYear { get; set; }

        /// <summary>
        /// The HTML for header column 1 for the BOE.  This contains the PerformingOrgDescriptions, plus the Task Start CY label and the CLIN label.
        /// </summary>
        public string HeaderHTMLColumn1{ get; set; }

        /// <summary>
        /// The HTML for header column 2 for the BOE.  This contains the PerformingOrgNames, plus the Task Start CY and the CLIN ID.
        /// </summary>
        public string HeaderHTMLColumn2 { get; set; }

        /// <summary>
        /// The HTML for header column 3 for the BOE.  This contains the Start Date for each Task.
        /// </summary>
        public string HeaderHTMLColumn3 { get; set; }

        /// <summary>
        /// The HTML for header column 4 for the BOE.  This contains the End Date for each Task.
        /// </summary>
        public string HeaderHTMLColumn4 { get; set; }

        /// <summary>
        /// The BOE ID.
        /// </summary>
        public string BoeID{ get; set; }

        /// <summary>
        /// The Activity ID.
        /// </summary>
        public string ActivityID{ get; set; }

        /// <summary>
        /// The Activity Name.
        /// </summary>
        public string ActivityName{ get; set; }

        /// <summary>
        /// The Task Description.
        /// </summary>
        public string TaskDescription{ get; set; }

        /// <summary>
        /// The total Salary Hours for all resources for all years for this BOE.
        /// </summary>
        public decimal SalaryHoursTotal{ get; set; }

        /// <summary>
        /// The total Cost Dollars for all resources for all years for this BOE.
        /// </summary>
        public decimal CostDollarsTotal{ get; set; }

        /// <summary>
        /// The Rationale.
        /// </summary>
        public string Rationale{ get; set; }

        /// <summary>
        /// The unit represented by the resource (e.g. Hours, Cost Dollars, etc.).  Should match one of the options from
        /// the IES.Common.Constants class, such as BOE_SUMMARY_RESOURCE_UNIT_HOURS or BOE_SUMMARY_RESOURCE_UNIT_COST_DOLLARS.
        /// </summary>
        public string ResourceUnit{ get; set; }

        /// <summary>
        /// The Value for Year 1, which is represented by the ReportStartYear.  This is a summary of all of the associated hours or cost dollars
        /// for all resources for that year.
        /// </summary>
        public decimal Year01 { get; set; }

        /// <summary>
        /// The Value for Year 2.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year02 { get; set; }

        /// <summary>
        /// The Value for Year 3.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year03 { get; set; }

        /// <summary>
        /// The Value for Year 4.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year04 { get; set; }

        /// <summary>
        /// The Value for Year 5.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year05 { get; set; }

        /// <summary>
        /// The Value for Year 6.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year06 { get; set; }

        /// <summary>
        /// The Value for Year 7.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year07 { get; set; }

        /// <summary>
        /// The Value for Year 8.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year08 { get; set; }

        /// <summary>
        /// The Value for Year 9.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year09 { get; set; }

        /// <summary>
        /// The Value for Year 10.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year10 { get; set; }

        /// <summary>
        /// The Value for Year 11.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year11 { get; set; }

        /// <summary>
        /// The Value for Year 12.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year12 { get; set; }

        /// <summary>
        /// The Value for Year 13.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year13 { get; set; }

        /// <summary>
        /// The Value for Year 14.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year14 { get; set; }

        /// <summary>
        /// The Value for Year 15.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year15 { get; set; }

        /// <summary>
        /// The Value for Year 16.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year16 { get; set; }

        /// <summary>
        /// The Value for Year 17.  This is a summary of all of the associated hours or cost dollars for all resources for that year.
        /// </summary>
        public decimal Year17 { get; set; }
    }
}
