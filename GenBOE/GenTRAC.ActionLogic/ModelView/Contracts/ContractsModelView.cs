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
    using System.Web.Mvc;
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
        [Display(Name= "Previously Submitted ROM")]
        [Required(ErrorMessage = "Previously Submitted ROM is required.")]
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
        /// Customer Submittal Date String
        /// </summary>
        [Display(Name = "Customer Submittal Date")]
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
        /// Contracts Correspondence Log Number
        /// </summary>
        [Display(Name = "Contracts Correspondence Log Number")]
        [Required(ErrorMessage = "Contracts Correspondence Log Number is required.")]
        public string ContractsCorrespondenceLogNumber { get; set; }

        /// <summary>
        /// Final Negotiated Value String
        /// </summary>
        [Display(Name = "Final Negotiated Value")]
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
        /// EPP Delegation Authority ID
        /// </summary>
        public int? EPPDelegationAuthority { get; set; }

        /// <summary>
        /// Program EPP Date
        /// </summary>
        public DateTime? ProgramEppDate { get; set; }

        /// <summary>
        /// LOB EPP Date
        /// </summary>
        public DateTime? LobEppDate { get; set; }

        /// <summary>
        /// Pre Space EPP Date
        /// </summary>
        public DateTime? PreSpaceEppDate { get; set; }

        /// <summary>
        /// Space EPP Date
        /// </summary>
        public DateTime? SpaceEppDate { get; set; }

        /// <summary>
        /// Pre Corporate EPP Date
        /// </summary>
        public DateTime? PreCorporateEppDate { get; set; }

        /// <summary>
        /// Corporate Epp Date
        /// </summary>
        public DateTime? CorporateEppDate { get; set; }

        /// <summary>
        /// EPP ROS Delegation Notes
        /// </summary>
        public string EppRosDelegationNotes { get; set; }

        /// <summary>
        /// LM Won contract
        /// </summary>
        public bool? LmWon { get; set; }

        /// <summary>
        /// Mod Completed Date
        /// </summary>
        public DateTime? ModCompletedDate { get; set; }
    }
}
