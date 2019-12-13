// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Reports
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic.Validation;
    using IES.Common;

    /// <summary>
    /// Proposal log report model view
    /// </summary>
    public class ProposalLogReportModelView
    {
        /// <summary>
        /// List of Proposal Log status
        /// </summary>
        public ICollection<SelectListItem> ProposalLogStatusList { get; set; }

        /// <summary>
        /// List of Proposal log status IDs
        /// </summary>
        [Required(ErrorMessage = ValidationConstants.ProposalLogReportValidationConstants.PROPOSAL_STATUS_REQUIRED)]
        public ICollection<int> ProposalLogStatus { get; set; }

        /// <summary>
        /// List of allowed years
        /// </summary>
        public ICollection<SelectListItem> YearsList { get; set; }

        /// <summary>
        /// years
        /// </summary>
        public ICollection<string> Years { get; set; }

        /// <summary>
        /// List of allowed Lines of Business
        /// </summary>
        public ICollection<SelectListItem> LOBList { get; set; }

        /// <summary>
        /// Line of Business IDs
        /// </summary>
        public ICollection<int> LOBIds { get; set; }

        /// <summary>
        /// List of pricers that have pricer type as central estimator
        /// </summary>
        public ICollection<SelectListItem> LeadEstimatorList { get; set; }

        /// <summary>
        /// collection of the central estimators (pricer) ids
        /// </summary>
        public ICollection<int> CentralEstimators { get; set; }

        /// <summary>
        ///  submit start date range
        /// </summary>
        public string SubmitStartDate { get; set; }

        /// <summary>
        /// submit end date range
        /// </summary>
        public string SubmitEndDate { get; set; }

        /// <summary>
        /// tracking number text box
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// execution user ids..the user id (including groups) that the the user is running the report as
        /// </summary>
        public string ExecutionUserIds { get; set; }

        /// <summary>
        /// select a filter option
        /// </summary>
        public ProposalLogFilterOption? ProposalLogFilterOption { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public ProposalLogReportModelView()
        {
        }
    }
}
