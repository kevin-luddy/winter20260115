// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using ActionLogic.ModelView.Proposals;
    using GenTRAC.ActionLogic;
    using GenTRAC.Web.Common;

    /// <summary>
    /// Certification Timeline controller
    /// </summary>
    public class CertificationTimelineController : GenTRACController
    {
        /// <summary>
        /// Proposal Controller Logic
        /// </summary>
        private ProposalControllerLogic proposalLogic = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="genTRACControllerLogic">Controller Logic</param>
        /// <param name="siteMasterUtilities">Site Master Utilities</param>
        /// <param name="securityInformation">security information about user and their context</param>
        /// <param name="proposalLogic">The proposal logic.</param>
        public CertificationTimelineController(
            GenTRACControllerLogic genTRACControllerLogic,
            SiteMasterUtilities siteMasterUtilities,
            IES.Common.ISecurityInformation securityInformation,
            ProposalControllerLogic proposalLogic)
            : base(securityInformation, genTRACControllerLogic, siteMasterUtilities)
        {
            this.proposalLogic = proposalLogic;
        }

        /// <summary>
        /// Display post proposal certification timeline info
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>Displays the post proposal certification timeline info.</returns>
        public ViewResult DisplayCertificationTimeline(int proposalId)
        {
            this.ViewBag.proposalid = proposalId.ToString();

            ProposalCertificationTimelineModelView model = this.proposalLogic.GetDataForProposalTimelineCertification(proposalId);
            return this.View(WebConstants.View.PROPOSAL_CERTIFICATION_TIMELINE, model);
        }

        /// <summary>
        /// Saves the certification timeline.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="model">The model.</param>
        /// <returns>Success or failure</returns>
        /// <exception cref="System.ArgumentNullException">model</exception>
        [HttpPost]
        public JsonResult SaveCertificationTimeline(int proposalId, ProposalCertificationTimelineModelView model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.proposalLogic.SaveCertificationTimeline(proposalId, model);

            return this.Json(new { Status = true });
        }

        /// <summary>
        /// Completes the certification timeline.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="model">The model.</param>
        /// <returns>Success or failure</returns>
        /// <exception cref="System.ArgumentNullException">model</exception>
        [HttpPost]
        public JsonResult CompleteCertificationTimeline(int proposalId, ProposalCertificationTimelineModelView model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.proposalLogic.CompleteCertificationTimeline(proposalId, model);

            return this.Json(new { Status = true });
        }
    }
}