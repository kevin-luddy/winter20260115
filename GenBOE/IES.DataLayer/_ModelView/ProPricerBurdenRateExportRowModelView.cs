// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    /// <summary>
    /// The Model View used for a ProPricer Burden Pool Export Row.
    /// </summary>
    public class ProPricerBurdenRateExportRowModelView : ProPricerExportModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProPricerBurdenRateExportRowModelView"/> class.
        /// </summary>
        public ProPricerBurdenRateExportRowModelView()
        {
            this.BurdenPool = string.Empty;
            this.Description = string.Empty;
            this.EffectiveDate = string.Empty;
            this.Date = string.Empty;
            this.Rates = new List<string>();
        }

        /// <summary>
        /// Gets/sets the Burden Pool.
        /// </summary>
        [Required]
        public string BurdenPool { get; set; }

        /// <summary>
        /// Gets/sets the Description.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets the EffectiveDate.
        /// </summary>
        [Required]
        public string EffectiveDate { get; set; }

        /// <summary>
        /// Gets/sets the Date.
        /// </summary>
        [Required]
        public string Date { get; set; }

        /// <summary>
        /// Gets/sets the Rate values (one for each Burden Element).
        /// </summary>
        [Required]
        public ICollection<string> Rates { get; set; }

        /// <summary>
        /// Generates string representation of this ModelView object to attach to the StringBuilder.
        /// </summary>
        /// <param name="stringBuilder">A stringbuilder to append this objects representation into.</param>
        public override void ExportString(StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException(nameof(stringBuilder));
            }

            // shorten names for brevity
            const string DQ = ProPricerExportModelView.DOUBLE_QUOTE;
            const string EF = ProPricerExportModelView.END_FIELD;
            stringBuilder.Append(this.BurdenPool ?? string.Empty);
            stringBuilder.Append(EF);
            stringBuilder.Append(DQ);
            stringBuilder.Append(this.Description ?? string.Empty);
            stringBuilder.Append(DQ);
            stringBuilder.Append(EF);
            stringBuilder.Append(this.EffectiveDate ?? string.Empty);
            stringBuilder.Append(EF);
            stringBuilder.Append(this.Date ?? string.Empty);
            foreach (string rate in this.Rates)
            {
                stringBuilder.Append(EF);
                stringBuilder.Append(rate);
            }

            stringBuilder.AppendLine();
        }
    }
}
