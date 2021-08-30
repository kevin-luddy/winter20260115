// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Dtos;
    using GenBOE.ActionLogic.ModelView;

    /// <summary>
    /// Model view for Offloading Rates.
    /// </summary>
    /// <seealso cref="GenBOE.ActionLogic.ModelView.PersistedDataModelView" />
    public class OffloadRateModelView : PersistedDataModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OffloadRateModelView"/> class.
        /// </summary>
        public OffloadRateModelView()
        {
            this.Deleted = false;
            this.OffloadRateID = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OffloadRateModelView"/> class.
        /// </summary>
        /// <param name="rate">The rate.</param>
        public OffloadRateModelView(OffloadRatesDTO rate)
        {
            if (ReferenceEquals(rate, null))
            {
                throw new ArgumentNullException(nameof(rate));
            }

            this.OffloadRateID = rate.Id;
            this.HourlyRate = rate.HourlyRate;
            this.PercentToOffload = rate.Percent;
            this.PerformingOrg = rate.PerformingOrg;
            this.Resource = rate.Resource;
            this.SubcontractorResource = rate.SubResource;
            this.UpdateDate = rate.UpdateDate;
            this.Year = rate.Year;
        }

        public int OffloadRateID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OffloadRateModelView"/> is deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        [Required]
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets the subcontractor resource.
        /// </summary>
        [Required(ErrorMessage = "Subcontractor Resource is required.")]
        public string SubcontractorResource { get; set; }

        /// <summary>
        /// Gets or sets the performing org.
        /// </summary>
        [Required(ErrorMessage = "Performing Org is required.")]
        public string PerformingOrg { get; set; }

        /// <summary>
        /// Gets or sets the percent to offload.
        /// </summary>
        [Required(ErrorMessage = "Percent To Offload is required.")]
        [RegularExpression(@"(^[0]?(?:\.[0-9]{1,3})$)", ErrorMessage = "Percent To Offload must be between 0.001 and 1.00 and contain only 3 decimal places.")]
        public decimal PercentToOffload { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        [Required]
        [Range(1990, 2400, ErrorMessage = "The Year must be a value from 1990 to 2400.")]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the hourly rate.
        /// </summary>
        [Required(ErrorMessage = "Hourly Rate is required.")]
        [RegularExpression(@"(^\d{0,3}([.]\d{1,2})?$)", ErrorMessage = "Hourly Rate must be between 0.01 and 999.99 and contain only 2 decimal places.")]
        public decimal HourlyRate { get; set; }

    }
}