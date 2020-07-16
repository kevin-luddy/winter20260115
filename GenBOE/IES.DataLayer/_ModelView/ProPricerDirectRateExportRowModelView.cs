// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using IES.Common;

    /// <summary>
    /// The Model View used for a ProPricer Direct Rate Export Row.
    /// </summary>
    public class ProPricerDirectRateExportRowModelView : ProPricerExportModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProPricerDirectRateExportRowModelView"/> class.
        /// </summary>
        public ProPricerDirectRateExportRowModelView()
        {
            this.ResourceType = string.Empty;
            this.StartDate = string.Empty;
            this.EndDate = string.Empty;
            this.Resource = string.Empty;
            this.Description = string.Empty;
            this.ResourceClass = string.Empty;
            this.BurdenPool = string.Empty;
            this.Calendar = string.Empty;
            this.RateType = string.Empty;
            this.EffectiveDate = string.Empty;
            this.BaseRate = string.Empty;
            this.Step = "0";
            this.Factor = "0";
        }

        /// <summary>
        /// Gets/sets the Resource Type.
        /// </summary>
        [Required]
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets/sets the Start Date.
        /// </summary>
        [Required]
        public string StartDate { get; set; }

        /// <summary>
        /// Gets/sets the End Date.
        /// </summary>
        [Required]
        public string EndDate { get; set; }

        /// <summary>
        ///  Gets/sets the Resource.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets/sets the Description.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets the Resource Class.
        /// </summary>
        [Required]
        public string ResourceClass { get; set; }

        /// <summary>
        /// Gets/sets the Burden Pool.
        /// </summary>
        [Required]
        public string BurdenPool { get; set; }

        /// <summary>
        ///  Gets/sets the Calendar.
        /// </summary>
        public string Calendar { get; set; }

        /// <summary>
        ///  Gets/sets the Rate Type.
        /// </summary>
        public string RateType { get; set; }

        /// <summary>
        ///  Gets/sets the Effective Date.
        /// </summary>
        public string EffectiveDate { get; set; }

        /// <summary>
        ///  Gets/sets the Base Rate.
        /// </summary>
        public string BaseRate { get; set; }

        /// <summary>
        ///  Gets/sets the Step.
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        ///  Gets/sets the Factor.
        /// </summary>
        public string Factor { get; set; }

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
            stringBuilder.Append(DQ);
            stringBuilder.Append(this.ResourceType.GetDescription());
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.StartDate);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.EndDate);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.Resource);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.Description);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.ResourceClass);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.BurdenPool);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.Calendar);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.RateType);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.EffectiveDate);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.BaseRate);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.Step);
            stringBuilder.Append(ProPricerExportModelView.DOUBLEQUOTE_ENDFIELD_DOUBLEQUOTE);
            stringBuilder.Append(this.Factor);
            stringBuilder.AppendLine(DQ);
        }

        /// <summary>
        /// returns ModelView for first row of direct rate pro pricer exports w/column headers
        /// </summary>
        /// <returns>ProPricerDirectRateExportRowModelView</returns>   
        public static ProPricerDirectRateExportRowModelView GetDirectRateHeaderModelView()
        {
            // Create a model view for the header row
            return new ProPricerDirectRateExportRowModelView()
            {
                ResourceType = "Resource Type",
                StartDate = "Start Date",
                EndDate = "End Date",
                Resource = "Resource",
                Description = "Description",
                ResourceClass = "ResourceClass",
                BurdenPool = "Burden Pool",
                Calendar = "Calendar",
                RateType = "Rate Type",
                EffectiveDate = "Effective Date",
                BaseRate = "Base Rate",
                Step = "Step",
                Factor = "Factor"
            };
        }
    }
}
