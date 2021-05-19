// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using IES.Common;

    /// <summary>
    /// Contracts Tab MV
    /// </summary>
    public class ContractsModelView
    {
        /// <summary>
        /// Previously Submitted ROM (PTM record)
        /// </summary>
        [Display(Name= "Previously Submitted ROM")]
        public string PreviouslySubmittedROM { get; set; }

        /// <summary>
        /// Previous ROM Date
        /// </summary>
        public DateTime? previousROMDt { get; private set; }

        /// <summary>
        /// Previous ROM Date String
        /// </summary>
        [Display(Name = "Previously Submitted ROM Date")]
        public string PreviousROMDate
        {
            get => this.previousROMDt?.Date.ToShortDateString(); 
            
            set
            {
                this.previousROMDt = null;
                if(DateTime.TryParse(value, out DateTime result))
                {
                    this.previousROMDt = result.Normalize(DateTimePrecision.Day);
                }
            }
        }

        /// <summary>
        /// Previous ROM Value
        /// </summary>
        public decimal? PreviousROMValueDecimal { get; set; }

        /// <summary>
        /// Previous ROM Value
        /// </summary>
        [Display(Name = "Previously Submitted ROM Value")]
        public string PreviousROMValue => this.PreviousROMValueDecimal?.ToString("C");

        /// <summary>
        /// Customer Submittal Date
        /// </summary>
        public DateTime? customerSubmittalDt { get; private set; }

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
    }
}
