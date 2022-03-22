// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Contracts Controller
    /// </summary>
    public class ContractsController : GenTRACController
    {
        /// <summary>
        /// Contracts Logic
        /// </summary>
        private ContractsControllerLogic contractsLogic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logic">Controller Logic</param>
        /// <param name="siteMasterUtils">Site Master Utilities</param>
        /// <param name="securityInformation">security information about user and their context</param>
        public ContractsController(ContractsControllerLogic logic, SiteMasterUtilities siteMasterUtils, ISecurityInformation securityInformation)
            : base(securityInformation, logic, siteMasterUtils)
        {
            this.contractsLogic = logic;
        }

        /// <summary>
        /// Display Contracts index
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Create new proposal approval view</returns>
        public ViewResult DisplayContractsIndex(int proposalId)
        {
            this.ViewBag.ProposalId = proposalId.ToString();
            
            ContractsModelView model = this.contractsLogic.GetDataForProposalContracts(proposalId).Result;

            return this.View(WebConstants.View.CONTRACTS_INDEX, model);
        }

        /// <summary>
        /// Get ROM Date and Value
        /// </summary>
        /// <param name="proposalId">Needed by MVC routing</param>
        /// <param name="previousRomProposalId">Previous Selected ROM (proposal Id)</param>
        /// <returns>Date and Value of the proposal</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
        public JsonResult GetPreviouslySelectedRomData(int proposalId, int previousRomProposalId)
        {
            Tuple<DateTime?, decimal?> data = this.contractsLogic.GetRomDateAndValue(previousRomProposalId);

            return Json(new { Date = data?.Item1?.ToShortDateString() ?? "N/A", Value = data?.Item2?.ToString("C") ?? "N/A" });
        }

        /// <summary>
        /// Set Proposal to No Bid Status
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>json result</returns>
        public JsonResult SetProposalAsNoBid(int proposalId)
        {
            IESResponse<ContractsModelView> response = new IESResponse<ContractsModelView>();

            if (this.contractsLogic.ValidForNoBidProposalStatusSave(this.contractsLogic.GetFullProposalAsync(proposalId).Result, (List<string>)response.Messages))
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
                {
                    this.contractsLogic.SetProposalToNoBid(proposalId);
                    scope.Complete();
                    response.IsSuccessful = true;
                }

                // email outside of transaction to ensure email send failure doesn't roll-back transaction.
                if (response.IsSuccessful)
                {
                    // send email notifications (no await purposely)
                    this.contractsLogic.SendContractsStatusChangeEmails(proposalId);
                }
            }

            return this.Json(response);
        }

        /// <summary>
        /// Set Proposal to No Bid Status
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>json result</returns>
        public JsonResult RevertProposalNoBidStatus(int proposalId)
        {
            IESResponse<ContractsModelView> response = new IESResponse<ContractsModelView>();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                this.contractsLogic.RevertProposalFromNoBid(proposalId);
                scope.Complete();
                response.IsSuccessful = true;
            }

            return this.Json(response);
        }

        /// <summary>
        /// Sets the status to proposal lost and sends notification email(s)
        /// </summary>
        /// <param name="proposalId">Proposal to be updated</param>
        /// <returns>Response object</returns>
        public async Task<JsonResult> SetProposalLost(int proposalId)
        {
            IESResponse<ContractsModelView> response = new IESResponse<ContractsModelView>();
            List<string> errMessages = new List<string>();

            await this.contractsLogic.SetProposalLost(proposalId, errMessages);
            response.Messages.AddRange(errMessages);

            if (!errMessages.Any())
            {
                response.IsSuccessful = true;
            }

            return Json(response);
        }

        /// <summary>
        /// Saves the Contract.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <param name="model">The model.</param>
        /// <returns>Success or failure</returns>
        /// <exception cref="System.ArgumentNullException">model</exception>
        [HttpPost]
        public JsonResult SaveContract(int proposalId, ContractsModelView model)
        {
            if (proposalId < 0)
            {
                throw new ArgumentNullException(nameof(proposalId));
            }

            _ = model ?? throw new ArgumentNullException(nameof(model));

            IESResponse<ContractsModelView> response = new IESResponse<ContractsModelView>();

            if (!ModelState.IsValid)
            {
                GetModelStateErrors(response);
                return Json(response);
            }
            else
            {
                this.contractsLogic.SaveContract(model);
                response.IsSuccessful = true;
            }            

            return this.Json(response);
        }

        /// <summary>
        /// Extracts the error messages from ModelState
        /// </summary>
        /// <param name="response">Generic response object</param>
        /// <typeparam name="T">Type of model included in the response.</typeparam>
        private void GetModelStateErrors<T>(IESResponse<T> response)
        {
            List<ModelState> modelsWithErrors = ModelState.Values.Where(x => x.Errors.Any()).ToList();

            foreach (ModelError model in modelsWithErrors.SelectMany(x => x.Errors))
            {
                response.Messages.Add(model.ErrorMessage);                
            }
        }
    }
}