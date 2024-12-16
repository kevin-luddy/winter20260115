// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using System.Web.Mvc;
	using GenTRAC.ActionLogic.ModelView.Contracts;
	using GenTRAC.ActionLogic.Validation;
	using IES.Common;

	/// <summary>
	/// Contracts Tab MV
	/// </summary>
	public class ContractsModelView
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ContractsModelView()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Id that this Contract is linked to.
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Previous ROM Date
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public DateTime? previousROMDt { private get; set; }

        /// <summary>
        /// Customer Due Date
        /// </summary>
        public DateTime? CustomerDueDt { get; set; }

        /// <summary>
        /// Customer Submittal Date
        /// </summary>
        public DateTime? CustomerSubmittalDt { get; set; }

        /// <summary>
        /// Date Confirmation of Negotiations Submitted
        /// </summary>
        public DateTime? NegotiationsSubmittedDt { get; set; }

        /// <summary>
        /// Final Negotiated Value
        /// </summary>
        public long? FinalNegotiatedValueLong { get; set; }

        /// <summary>
        /// Previous ROM Value
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public decimal? PreviousROMValueDecimal { private get; set; }

        /// <summary>
        /// A list of options for the previously submitted roms
        /// </summary>
        public ICollection<SelectListItem> PreviouslySubmittedRoms { get; set; }

        /// <summary>
        /// Previously Submitted ROM (PTM record)
        /// </summary>
        [Display(Name = "Previously Submitted ROM")]
        public int? PreviouslySubmittedROM { get; set; }

        /// <summary>
        /// Previous ROM Date String
        /// </summary>
        [Display(Name = "Previously Submitted ROM Date")]
        public string PreviousROMDate => this.previousROMDt?.Date.ToShortDateString();

        /// <summary>
        /// Previous ROM Value
        /// </summary>
        [Display(Name = "Previously Submitted ROM Value")]
        public string PreviousROMValue => this.PreviousROMValueDecimal?.ToString("C");

        /// <summary>
        /// Customer Due Date
        /// </summary>
        [Display(Name = "Customer Due Date")]
		public string CustomerDueDate
		{
			get => this.CustomerDueDt?.Date.ToShortDateString();

			set
			{
				this.CustomerDueDt = null;
				if (DateTime.TryParse(value, out DateTime result))
				{
					this.CustomerDueDt = result.Normalize(DateTimePrecision.Day);
				}
			}
		}

		/// <summary>
		/// Proposal Submittal Date to Customer string representation.
		/// </summary>
		[Display(Name = "Proposal Submittal Date to Customer")]
        public string CustomerSubmittalDate
        {
            get => this.CustomerSubmittalDt?.Date.ToShortDateString();

            set
            {
                this.CustomerSubmittalDt = null;
                if (DateTime.TryParse(value, out DateTime result))
                {
                    this.CustomerSubmittalDt = result.Normalize(DateTimePrecision.Day);
                }
            }
        }

        /// <summary>
        /// Cage Code #
        /// </summary>
		[Display(Name = "Cage #")]
        public string CageCode { get; set; }

        /// <summary>
        /// Collection of Cage Code View Models
        /// </summary>
        public ICollection<CageCodeModelView> CageCodes {get; set;}

        /// <summary>
        /// Select List for Cage Codes, including address
        /// </summary>
        public ICollection<SelectListItem> CageCodeSelectListItems
		{
            get
			{
                return CageCodes.Select(x => new SelectListItem() { 
                    Text = $"{x.CageCode} - {x.Address1}{(!string.IsNullOrEmpty(x.Address2) ? ", " + x.Address2 : String.Empty)}, {x.City}, {x.State} {x.Zip}", 
                    Value = x.CageCode, 
                    Selected = x.CageCode == CageCode 
                }).ToList();
			}
		}

        /// <summary>
        /// Contracts Correspondence Log Number
        /// </summary>
        [Display(Name = "Contracts Correspondence Log Number")]
        [Required(ErrorMessage = "Contracts Correspondence Log Number is required.")]
        public string ContractsCorrespondenceLogNumber { get; set; }

        /// <summary>
        /// Final Negotiated Value String
        /// </summary>
        [Display(Name = "Final Negotiated Value ($)")]
        public string FinalNegotiatedValue => this.FinalNegotiatedValueLong?.ToString("C");

        /// <summary>
        /// Date Confirmation of Negotiations Submitted String
        /// </summary>
        [Display(Name = "Date Confirmation of Negotiations Submitted")]
        public string NegotiationsSubmitted
        {
            get => this.NegotiationsSubmittedDt?.Date.ToShortDateString();

            set
            {
                this.NegotiationsSubmittedDt = null;
                if (DateTime.TryParse(value, out DateTime result))
                {
                    this.NegotiationsSubmittedDt = result.Normalize(DateTimePrecision.Day);
                }
            }
        }

        /// <summary>
        /// The date/time of the last update to the record
        /// </summary>
        [Display(Name = "The date and time of the last update to the record.")]
        public string LastUpdatedDateLong { get; set; }

        /// <summary>
        /// EPP Delegation Authority Level
        /// </summary>
        [Display(Name = "EPP Delegation Authority")]
        public EppDelegationAuthority? EppDelegationAuthority { get; set; }

        /// <summary>
        /// Program EPP Date
        /// </summary>
        [Display(Name = "Program EPP Date")]
        public DateTime? ProgramEppDate { get; set; }

        /// <summary>
        /// Line of Business EPP Date
        /// </summary>
        [Display(Name = "Line of Business EPP Date")]
        public DateTime? LobEppDate { get; set; }

        /// <summary>
        /// Pre-Space EPP Date
        /// </summary>
        [Display(Name = "Pre-Space EPP Date")]
        public DateTime? PreSpaceEppDate { get; set; }

        /// <summary>
        /// Space EPP Date
        /// </summary>
        [Display(Name = "Space EPP Date")]
        public DateTime? SpaceEppDate { get; set; }

        /// <summary>
        /// Pre-Corporate EPP Date
        /// </summary>
        [Display(Name = "Pre-Corporate EPP Date")]
        public DateTime? PreCorporateEppDate { get; set; }

        /// <summary>
        /// Corporate EPP Date
        /// </summary>
        [Display(Name = "Corporate EPP Date")]
        public DateTime? CorporateEppDate { get; set; }

        /// <summary>
        /// EPP ROS Delegation Notes
        /// </summary>
        [Display(Name = "Comments - Please Note EPP ROS Delegation Notes Here as Well")]
        [MaxLength(1000)]
        public string EppRosDelegationNotes { get; set; }

        /// <summary>
        /// Lockheed Martin Contract Won?
        /// </summary>
        [Display(Name = "Contract Won?")]
        public bool? LmWon { get; set; }

		/// <summary>
		/// Is Insurance Direct
		/// </summary>
		[Display(Name = "Has insurance been proposed direct?")]
		public TripleBooleanState? IsInsuranceDirect { get; set; }

		/// <summary>
		/// Insurance Type
		/// </summary>
		public InsuranceType? InsuranceType { get; set; }

		/// <summary>
		/// Proposed Insurance Value
		/// </summary>
		[RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ProposalValidationConstants.PROPOSED_INSURANCE_VALUE_IN_DOLLARS)]
		[Display(Name = "Proposed Insurance Value ($)")]
		public string ProposedInsurance { get; set; }

		/// <summary>
		/// Negotiated Insurance Value
		/// </summary>
		[RegularExpression(Validation.ValidationConstants.PRICE_RANGE_FORMAT, ErrorMessage = ValidationConstants.ProposalValidationConstants.NEGOTIATED_INSURANCE_VALUE_IN_DOLLARS)]
		[Display(Name = "Negotiated Insurance Value ($)")]
		public string NegotiatedInsurance { get; set; }


		/// <summary>
		/// MOD Completed Date
		/// </summary>
		[Display(Name = "MOD Completed Date")]
        public DateTime? ModCompletedDate { get; set; }

        /// <summary>
        /// Select options for EPP
        /// </summary>
        public ICollection<SelectListItem> EppOptions { get; set; }

		/// <summary>
		/// Select options for Insurance Proposed Direct
		/// </summary>
		public ICollection<SelectListItem> InsuranceProposedDirectOptions { get; set; }

		/// <summary>
		/// Select options for Insurance Type
		/// </summary>
		public ICollection<SelectListItem> InsuranceTypeOptions { get; set; }

		#region Buttons
		/// <summary>
		/// Should the "Set Lost" button be enabled
		/// </summary>
		public bool SetLostButtonEnabled { get; set; }

        /// <summary>
        /// Should the "No Bid" button be enabled
        /// </summary>
        public bool NoBidButtonEnabled { get; set; }

        /// <summary>
        /// Should the "Complete" button be enabled
        /// </summary>
        public bool CompleteButtonEnabled { get; set; }

 #endregion

        /// <summary>
        /// Has No Bid been set?
        /// </summary>
        public bool IsNoBid { get; set; }

        /// <summary>
        /// Should the data be read only, i.e. the proposal is completed
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Does user have access to set No Bid?
        /// </summary>
        public bool HasAccessToSetNoBid { get; set; }

		/// <summary>
		/// Is the Proposal Class ROM or NTE?  if so, need to hide a bunch of stuff in the UI, and change text in some buttons/labels
		/// </summary>
		public bool IsRomNte { get; set; }

		/// <summary>
		/// Button text for No Bid button
		/// </summary>
		public string NoBidButtonText
		{
			get
			{
				if (this.IsRomNte)
				{
					return "Mark ROM/NTE as No Bid";
				}
				else
				{
					return "Mark Proposal as No Bid";
				}
			}
		}
    }
}
