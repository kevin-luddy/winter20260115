// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Home
{
    /// <summary>
    /// Export Proposal Report Model View
    /// </summary>
    public class ExportProposalReportModelView
    {
        /// <summary>
        /// FilterOption - InProgress (0), Completed (1), All (2)
        /// </summary>
        public string FilterOption { get; set; }

        /// <summary>
        /// FilterStartDate - the assigned proposal start date
        /// </summary>
        public string FilterStartDate { get; set; }

        /// <summary>
        /// FilterEndDate - the assigned proposal end date
        /// </summary>
        public string FilterEndDate { get; set; }

        /// <summary>
        /// SearchText - the optional proposal search string
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// Gets or sets the filter proposal class.
        /// </summary>
        public string FilterProposalClass { get; set; }

        /// <summary>
        ///  default constructor
        /// </summary>
        public ExportProposalReportModelView()
        {
        }
    }
}
