// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.Web.Common;
    using IES.Common;

    /// <summary>
    /// Contracts Controller
    /// </summary>
    public class ContractsController : GenTRACController
    {
        /// <summary>
        /// Contracts Logic
        /// </summary>
        private ContractsControllerLogic logic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logic">Controller Logic</param>
        /// <param name="siteMasterUtils">Site Master Utilities</param>
        /// <param name="securityInformation">security information about user and their context</param>
        public ContractsController(ContractsControllerLogic logic, SiteMasterUtilities siteMasterUtils, ISecurityInformation securityInformation)
            : base(securityInformation, logic, siteMasterUtils)
        {
            this.logic = logic;
        }

        /// <summary>
        /// Display checklist index
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>create new proposal approval view</returns>
        public ViewResult DisplayContractsIndex(int proposalId)
        {
            this.ViewBag.ProposalId = proposalId.ToString();
            this.ViewBag.ReadOnly = false; // todo.. fix me

            ContractsModelView model = this.logic.GetContractsData(proposalId);

            return this.View(WebConstants.View.CONTRACTS_INDEX, model);
        }
    }
}