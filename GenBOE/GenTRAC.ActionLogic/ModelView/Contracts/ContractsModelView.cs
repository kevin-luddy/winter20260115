// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
        /// Previous ROM Date
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public DateTime? previousROMDt { private get; set; }

        /// <summary>
        /// Customer Submittal Date
        /// </summary>
        private DateTime? customerSubmittalDt { get; set; }

        /// <summary>
        /// Date Confirmation of Negotiations Submitted
        /// </summary>
        private DateTime? negotiationsSubmittedDt { get; set; }

        /// <summary>
        /// Final Negotiated Value
        /// </summary>
        public int? FinalNegotiatedValueInt { get; set; }

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
            get => this.customerSubmittalDt?.Date.ToShortDateString();

            set
            {
                this.customerSubmittalDt = null;
                if (DateTime.TryParse(value, out DateTime result))
                {
                    this.customerSubmittalDt = result.Normalize(DateTimePrecision.Day);
                }
            }
        }

        /// <summary>
        /// Contracts Correspondence Log Number
        /// </summary>
        [Display(Name = "Contracts Correspondence Log Number")]
        public string ContractsCorrespondenceLogNumber { get; set; }

        /// <summary>
        /// Final Negotiated Value String
        /// </summary>
        [Display(Name = "Final Negotiated Value")]
        public string FinalNegotiatedValue => this.FinalNegotiatedValueInt?.ToString("C");

        /// <summary>
        /// Date Confirmation of Negotiations Submitted String
        /// </summary>
        [Display(Name = "Date Confirmation of Negotiations Submitted")]
        public string NegotiationsSubmitted
        {
            get => this.negotiationsSubmittedDt?.Date.ToShortDateString();

            set
            {
                this.negotiationsSubmittedDt = null;
                if (DateTime.TryParse(value, out DateTime result))
                {
                    this.negotiationsSubmittedDt = result.Normalize(DateTimePrecision.Day);
                }
            }
        }

        /// <summary>
        /// Contract Offers
        /// </summary>
        public ICollection<ContractsOfferModelView> ContractOffers { get; set; } = new List<ContractsOfferModelView>();
    }
}
