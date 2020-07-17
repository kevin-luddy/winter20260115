// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView.Reports;
    using GenTRAC.Web.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Controller for Reports
    /// </summary>
    public class ReportsController : GenTRACController
    {
        /// <summary>
        /// Reports Controller Logic
        /// </summary>
        private ReportsControllerLogic reportsControllerLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inGenTRACControllerLogic">GenTRAC Controller Logic</param>
        /// <param name="inSiteMasterUtilities">Site utilities</param>
        /// <param name="inReportsControllerLogic">Reports Controller Logic</param>
        /// <param name="inSecurityInformation">security information about user and their context</param>
        public ReportsController(GenTRACControllerLogic inGenTRACControllerLogic, SiteMasterUtilities inSiteMasterUtilities, ReportsControllerLogic inReportsControllerLogic,
            IES.Common.ISecurityInformation inSecurityInformation)
            : base(inSecurityInformation, inGenTRACControllerLogic, inSiteMasterUtilities)
        {
            this.reportsControllerLogic = inReportsControllerLogic;
        }

        /// <summary>
        /// Display Proposal log
        /// </summary>
        /// <returns>proposal log</returns>
        public ViewResult DisplayProposalLog()
        {
            ProposalLogReportModelView model = this.reportsControllerLogic.GetDataForProposalLogReport();
            return this.View(WebConstants.View.REPORT_PROPOSAL_LOG, model);
        }

        /// <summary>
        /// Display proposal log report parameters section
        /// </summary>
        /// <returns>proposal log report parameters section</returns>
        public PartialViewResult DisplayProposalLogReportParameters()
        {
            ProposalLogReportModelView model = this.reportsControllerLogic.GetDataForProposalLogReport();
            return this.PartialView(WebConstants.View.REPORT_PROPOSAL_LOG_PARAMETERS, model);
        }

        /// <summary>
        /// View the proposal log report
        /// </summary>
        /// <param name="reportParameters">report parameters to use</param>
        /// <returns>true if successful, no if not</returns>
        public JsonResult ViewProposalLogReport(ProposalLogReportModelView reportParameters)
        {
            // validate the data
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            this.reportsControllerLogic.ValidateProposalLogParameters(reportParameters, validationErrors);
            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.ReportsControllerLogic.PROPOSAL_LOG_REPORT_PARAMETER_FORM);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            Uri reportUri = this.reportsControllerLogic.PopulateProposalLogSSRSParameters(reportParameters);

            return this.Json(new { Status = true, Url = reportUri });
        }

        /// <summary>
        /// Display Proposal Activity
        /// </summary>
        /// <returns>proposal activity</returns>
        public ViewResult DisplayProposalActivity()
        {
            ProposalActivityReportModelView model = this.reportsControllerLogic.GetDataForProposalActivityReport();
            return this.View(WebConstants.View.REPORT_PROPOSAL_ACTIVITY, model);
        }

        /// <summary>
        /// Display proposal activity report parameters section
        /// </summary>
        /// <returns>proposal activity report parameters section</returns>
        public PartialViewResult DisplayProposalActivityReportParameters()
        {
            ProposalActivityReportModelView model = this.reportsControllerLogic.GetDataForProposalActivityReport();
            return this.PartialView(WebConstants.View.REPORT_PROPOSAL_ACTIVITY_PARAMETERS, model);
        }

        /// <summary>
        /// View the proposal activity report
        /// </summary>
        /// <param name="reportParameters">report parameters to use</param>
        /// <returns>true if successful, no if not</returns>
        public JsonResult ViewProposalActivityReport(ProposalActivityReportModelView reportParameters)
        {
            // validate the data
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            this.reportsControllerLogic.ValidateProposalActivityParameters(reportParameters, validationErrors);
            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.ReportsControllerLogic.PROPOSAL_ACTIVITY_REPORT_PARAMETER_FORM);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            Uri reportUri = this.reportsControllerLogic.PopulateProposalActivitySSRSParameters(reportParameters);

            return this.Json(new { Status = true, Url = reportUri });
        }

        /// <summary>
        /// Display DFARS Checklist Response Report page
        /// </summary>
        /// <returns>dfars report model</returns>
        public ViewResult DisplayDfars()
        {
            DfarsReportModelView model = this.reportsControllerLogic.GetDataForDfarsReport();
            return this.View(WebConstants.View.REPORT_DFARS, model);
        }

        /// <summary>
        /// Display DFARS Checklist Response Report parameters section
        /// </summary>
        /// <returns>dfars report parameters section</returns>
        public PartialViewResult DisplayDfarsReportParameters()
        {
            DfarsReportModelView model = this.reportsControllerLogic.GetDataForDfarsReport();
            return this.PartialView(WebConstants.View.REPORT_DFARS_PARAMETERS, model);
        }

        /// <summary>
        /// View the DFARS Checklist Response Report
        /// </summary>
        /// <param name="reportParameters">Report Parameters to use</param>
        /// <returns>true if successful, false if not</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "reportParameters")]
        public JsonResult ViewDfarsReport(DfarsReportModelView reportParameters)
        {
            // validate the data
            List<ValidationMessage> validationErrors = HttpContext.Items["ValidationErrors"] as List<ValidationMessage>;
            this.reportsControllerLogic.ValidateDfarsReportParameters(reportParameters, validationErrors);
            validationErrors.ForEach(x => x.FormIDToTarget = GenTRAC.ActionLogic.ReportsControllerLogic.DFARS_REPORT_PARAMETER_FORM);

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            Uri reportUri = this.reportsControllerLogic.PopulateDfarsSSRSParameters(reportParameters);

            return this.Json(new { Status = true, Url = reportUri });
        }

        /// <summary>
        /// Will get the Program Areas for the selected LOBs
        /// </summary>
        /// <param name="lobs">Selected LOBs</param>
        /// <returns>Html for Program Area options</returns>
        public string LoadFilteredProgramAreas(ICollection<string> lobs)
        {
            ICollection<int> lobIds = new Collection<int>();

            if(lobs != null)
            {
                foreach (string lob in lobs)
                {
                    int lobId;
                    if (int.TryParse(lob, out lobId))
                    {
                        lobIds.Add(lobId);
                    }
                }
            }
            
            return this.reportsControllerLogic.GetProgramAreasForLOBs(lobIds);
        }
    }
}