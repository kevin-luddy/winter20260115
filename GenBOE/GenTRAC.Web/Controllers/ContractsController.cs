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
            this.ViewBag.ReadOnly = false; // todo.. fix me

            ContractsModelView model = this.contractsLogic.GetDataForProposalContracts(proposalId);

            return this.View(WebConstants.View.CONTRACTS_INDEX, model);
        }

        /// <summary>
        /// Calculates Offer Fields
        /// </summary>
        /// <param name="proposalId">Needed by MVC routing</param>
        /// <param name="counterOfferCost">Counter Offer Cost</param>
        /// <param name="counterOfferCom">Counter Offer COM</param>
        /// <param name="counterOfferProfitFee">Counter Offer Profit / Fee</param>
        /// <returns>Total Counter Offer Price and Counter Offer ROS</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "proposalId")]
        public JsonResult CalculateOfferFields(int proposalId, string counterOfferCost, string counterOfferCom, string counterOfferProfitFee)
        {
            int? cost = this.ProcessOfferInput(counterOfferCost, "LM Counter Offer Cost");
            int? com = this.ProcessOfferInput(counterOfferCom, "LM Counter Offer COM");
            int? profitFee = this.ProcessOfferInput(counterOfferProfitFee, "LM Counter Offer Profit/Fee");

            ContractsOfferModelView model = new ContractsOfferModelView()
            {
                LMCounterOfferCostInt = cost,
                LMCounterOfferCOMInt = com,
                LMCounterOfferProfitFeeInt = profitFee
            };

            return Json(new { Cost = model.LMCounterOfferCost, Com = model.LMCounterOfferCOM, ProfitFee = model.LMCounterOfferProfitFee, TotalPrice = model.LMCounterOfferTotalPrice, Ros = model.LMCounterOfferROS });
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
        /// Massages user inputs and attempts to convert them into a number
        /// </summary>
        /// <param name="fieldValue">Field value (to be converted into a number)</param>
        /// <param name="fieldDisplayLabel">Field label, in case of a problem</param>
        /// <returns>Null if blank, int if valid</returns>
        private int? ProcessOfferInput(string fieldValue, string fieldDisplayLabel)
        {
            int? result = null;

            if (!string.IsNullOrEmpty(fieldValue))
            {
                if (!decimal.TryParse(fieldValue.Replace("$", string.Empty).Replace(",", string.Empty), out decimal temp))
                {
                    throw new GenValidationException($"{fieldDisplayLabel} must be a valid number.");
                }

                result = decimal.ToInt32(temp);
            }

            return result;
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

            response.Status = true;

            this.contractsLogic.SaveContract(model);

            return this.Json(response);
        }

        /// <summary>
        /// Extracts the error messages from ModelState
        /// </summary>
        /// <param name="response">Generic response object</param>
        /// <typeparam name="T">Type of model included in the response.</typeparam>
        private void GetModelStateErrors<T>(IESResponse<T> response)
        {
            IEnumerable<ModelState> modelsWithErrors = ModelState.Values.Where(x => x.Errors.Count > 0).ToList();

            foreach (ModelState model in modelsWithErrors)
            {
                foreach (ModelError errorModel in model.Errors)
                {
                    response.Messages.Add(errorModel.ErrorMessage);
                }
            }
        }
    }
}