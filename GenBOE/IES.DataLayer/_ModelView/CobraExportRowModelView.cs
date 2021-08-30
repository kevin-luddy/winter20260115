// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The Model View used for a Cobra Export Row.
    /// </summary>
    public class CobraExportRowModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CobraExportRowModelView"/> class.
        /// </summary>
        public CobraExportRowModelView()
        {
            this.RateSet = string.Empty;
            this.Code1 = string.Empty;
            this.RateCode = string.Empty;
            this.Description = string.Empty;
            this.Date = DateTime.MinValue;
            this.Value = 0;
        }

        /// <summary>
        /// Gets/sets the RateSet.
        /// </summary>
        [Required]
        public string RateSet { get; set; }

        /// <summary>
        /// Gets/sets the Code1.
        /// </summary>
        [Required]
        public string Code1 { get; set; }

        /// <summary>
        /// Gets/sets the RateCode.
        /// </summary>
        public string RateCode { get; set; }
        
        /// <summary>
        /// Gets/sets the Description.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets the Date.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets/sets the Value.
        /// </summary>
        [Required]
        public decimal? Value { get; set; }
    }
}
