// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Checklist
{
	using System;
	using System.ComponentModel.DataAnnotations;
    using GenTRAC.ActionLogic.Validation;
    using IES.Common;

    /// <summary>
    /// Checklist's general information model view
    /// </summary>
    public class ChecklistGeneralInformationModelView : PersistedDataModelView
    {
		/// <summary>
		/// Proposal Date Created
		/// </summary>
		public DateTime? ProposalDateCreated { get; set; }

        /// <summary>
        /// Gets or sets Proposal ID
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// Gets or sets Proposal Checklist ID
        /// </summary>
        public int ProposalChecklistID { get; set; }

        /// <summary>
        /// Gets or sets Proposal submittal date
        /// </summary>
        public string EstimatingSubmitsToContractsDate { get; set; }

        /// <summary>
        /// Gets or sets pricer Displays Name
        /// </summary>
        public string PricerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets pricer ID
        /// </summary>
        public int PricerId { get; set; }

        /// <summary>
        /// Gets or sets submitted value (SSC Total Price)
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.TOTAL_PRICE_WHOLE_NUMBERS)]
        public string SubmittedValue { get; set; }

        /// <summary>
        /// Gets or sets absolute value
        /// </summary>
        [RegularExpression(Validation.ValidationConstants.ABSOLUTE_VALUE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ChecklistValidationConstants.ABSOLUTE_VALUE_WHOLE_NUMBERS)]
        public string AbsoluteValue { get; set; }   

        /// <summary>
        /// determines what part of the checklist to show in read only mode and which part to validate against
        /// </summary>
        public ShowChecklistResponse ShowChecklistResponse { get; set; }

        /// <summary>
        /// Boolean flag used to turn on/off PAR checklist ProposalPageNumber validation
        /// </summary>
        public bool DeliverChecklistDFARS { get; set; }

        /// <summary>
        /// Boolean flag used to display DFAR
        /// </summary>
        public bool ShowDFARQuestion { get; set; }

        /// <summary>
        /// Whether or not to use the old (GenTRAC) or new (PTM) checklist User Interface
        /// </summary>
        public bool? IsPTMChecklistUIEnabled { get; set; }

		/// <summary>
		/// Does this proposal include international costs estimated by Global Mobility?
		/// </summary>
		public bool? IncludeInternationalCosts { get; set; }

		/// <summary>
		/// Should we display the Include International Costs Section?
		/// </summary>
		public bool ShowIncludeInternationalCosts => (ProposalDateCreated ?? DateTime.Now) >= ConfigurationUtilities.GetAppSetting<DateTime>("InternationalCostStartDate");

		/// <summary>
		/// constructor
		/// </summary>
		public ChecklistGeneralInformationModelView()
        {
            this.ProposalID = -1;
            this.ProposalChecklistID = -1;
            this.DeliverChecklistDFARS = false;
            this.ShowDFARQuestion = true;
            this.IsPTMChecklistUIEnabled = true;
		}
    }
}
