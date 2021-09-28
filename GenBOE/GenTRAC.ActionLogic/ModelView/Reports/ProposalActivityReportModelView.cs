// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Reports
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Proposal Activity Report Model View
    /// </summary>
    public class ProposalActivityReportModelView
    {
        /// <summary>
        ///  create start date range
        /// </summary>
        public string CreateStartDate { get; set; }

        /// <summary>
        /// create end date range
        /// </summary>
        public string CreateEndDate { get; set; }

        /// <summary>
        /// tracking number text box
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// execution user ids..the user id (including groups) that the the user is running the report as
        /// </summary>
        public string ExecutionUserIds { get; set; }

        /// <summary>
        /// List of allowed Program Areas
        /// </summary>
        public ICollection<SelectListItem> ProgramAreaList { get; set; }

        /// <summary>
        /// Program Area IDs
        /// </summary>
        public ICollection<int> ProgramAreaIds { get; set; }

        /// <summary>
        /// List of Proposal status IDs
        /// </summary>
        public ProposalReportStatus ProposalStatus { get; set; }

        /// <summary>
        /// List of proposal status
        /// </summary>
        public ICollection<SelectListItem> ProposalStatusList { get; set; }

        /// <summary>
        /// select a filter option
        /// </summary>
        public ProposalActivityFilterOption? ProposalActivityFilterOption { get; set; }

        /// <summary>
        /// Gets or sets Lead Estimator NTID
        /// </summary>
        [System.ComponentModel.DisplayName("Lead Estimator")]
        public string LeadEstimatorNtid { get; set; }

        /// <summary>
        /// LeadEstimatorList
        /// </summary>
        public ICollection<UserData> LeadEstimatorList { get; set; }

        /// <summary>
        /// List of customer types
        /// </summary>
        public ICollection<SelectListItem> CustomerTypeList { get; set; }

        /// <summary>
        /// customer type IDs
        /// </summary>
        public ICollection<int> CustomerTypeIds { get; set; }

        /// <summary>
        /// Returns the DisplayName attribute for the LeadEstimatorNtid field for use in views.
        /// </summary>
        public string LeadEstimatorNtidDisplayName
        {
            get
            {
                if (typeof(ProposalActivityReportModelView).GetProperty("LeadEstimatorNtid") != null &&
                    typeof(ProposalActivityReportModelView).GetProperty("LeadEstimatorNtid").GetCustomAttribute(typeof(DisplayNameAttribute)) != null)
                {
                    return ((DisplayNameAttribute) typeof(ProposalActivityReportModelView).GetProperty("LeadEstimatorNtid").GetCustomAttribute(typeof(DisplayNameAttribute))).DisplayName;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public ProposalActivityReportModelView()
        {
        }
    }
}
