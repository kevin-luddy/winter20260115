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

    public class ContractsModelView
    {
        [Display(Name= "Previously Submitted ROM")]
        public string PreviouslySubmittedROM { get; set; }

        private DateTime? previousROMDate { get; set; }

        [Display(Name = "Previously Submitted ROM Date")]
        public DateTime? PreviousROMDate { get => this.previousROMDate?.Date; set => this.previousROMDate = value?.Normalize(DateTimePrecision.Day); }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public decimal? PreviousROMValueDecimal { private get; set; }

        [Display(Name = "Previously Submitted ROM Value")]
        public string PreviousROMValue => this.PreviousROMValueDecimal?.ToString("C");

        private DateTime? customerSubmittalDate { get; set; }

        [Display(Name = "Customer Submittal Date")]
        public DateTime? CustomerSubmittalDate { get => this.customerSubmittalDate?.Date; set => this.customerSubmittalDate = value?.Normalize(DateTimePrecision.Day); }

        [Display(Name = "Contracts Correspondence Log Number")]
        public string ContractsCorrespondenceLogNumber { get; set; }
    }
}
