// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Checklist
{
    using System.ComponentModel.DataAnnotations;
    using GenTRAC.ActionLogic.Validation;
    using IES.Common;

    /// <summary>
    /// Checklist proposal pricing data's section grid
    /// </summary>
    public class ChecklistProposalPricingDataModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ChecklistProposalPricingDataModelView()
        {
        }

        /// <summary>
        /// Gets or sets Proposal ID
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Gets or sets Proposal Checklist ID
        /// </summary>
        public int ProposalChecklistID { get; set; }

        /// <summary>
        /// Gets or sets LM labor hrs
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.DECIMAL_WITH_COMMAS_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.LM_LABOR_HRS_INVALID_RANGE)]
        public string LMLaborHrs { get; set; }

        /// <summary>
        /// Gets or sets LM labor cost
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.LM_LABOR_COST_WHOLE_NUMBERS)]
        public string LMLaborCost { get; set; }

        /// <summary>
        /// Gets or sets subcontractor cost
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.SUBCONTRACTOR_COST_WHOLE_NUMBERS)]
        public string SubcontractorCost { get; set; }

        /// <summary>
        /// Gets or sets material cost
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.MATERIAL_COST_WHOLE_NUMBERS)]
        public string MaterialCost { get; set; }

        /// <summary>
        /// Gets or sets iwta cost
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.IWTA_COST_WHOLE_NUMBERS)]
        public string IWTACost { get; set; }

        /// <summary>
        /// Gets or sets travel cost
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.TRAVEL_COST_WHOLE_NUMBERS)]
        public string TravelCost { get; set; }

        /// <summary>
        /// Gets or sets other direct costs
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.OTHER_DIRECT_COSTS_WHOLE_NUMBERS)]
        public string OtherDirectCosts { get; set; }

        /// <summary>
        /// Gets or sets profit/fee + com value
        /// 
        /// This field is being split into 2, but this is required for historical data
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.PROFIT_FEE_COM_WHOLE_NUMBER)]
        public string ProfitFeeComTotal { get; set; }

        /// <summary>
        /// Gets or sets profit/fee
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.PROFIT_FEE_WHOLE_NUMBER)] 
        public string ProfitFeeTotal { get; set; }

        /// <summary>
        /// Gets or sets COM
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.COM_WHOLE_NUMBER)] 
        public string ComTotal { get; set; }

        /// <summary>
        /// Gets or sets ROS %
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PERCENT_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.ROS_PERCENT_RANGE)]
        public string ROSPercent { get; set; }

        /// <summary>
        /// Gets or sets the submitted value.
        /// </summary>
        public string SubmittedValue { get; set; }

        /// <summary>
        /// Gets or sets the Absolute value.
        /// </summary>
        public string AbsoluteValue { get; set; }

        /// <summary>
        /// determines what part of the checklist to show in read only mode
        /// </summary>
        public ShowChecklistResponse ShowChecklistResponse { get; set; }
    }
}
