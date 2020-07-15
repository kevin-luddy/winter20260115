// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    /// <summary>
    /// Model View used for SSRS Pre Vs Post Offload Totals
    /// </summary>
    public class PreVsPostOffloadTotalsModelView
    {
        /// <summary>
        /// Gets/Sets the Project Map Hours Input Total
        /// </summary>
        public string HoursInputTotal { get; set; }

        /// <summary>
        /// Gets/Sets the Hours Offload Total
        /// </summary>
        public string HoursOffloadTotal { get; set; }

        /// <summary>
        /// Gets/Sets the ProPricer File (Offloaded Data) Hours Output Total 
        /// </summary>
        public string HoursOutputTotal { get; set; }

        /// <summary>
        /// Gets/Sets the Variance between Input, Offload, and Output Hours Totals
        /// </summary>
        public string HoursVariance { get; set; }

        /// <summary>
        /// Gets/Sets the Project Map Cost Input Total
        /// </summary>
        public string CostInputTotal { get; set; }

        /// <summary>
        /// Gets/Sets the Cost Offload Total
        /// </summary>
        public string CostOffloadTotal { get; set; }

        /// <summary>
        /// Gets/Sets the ProPricer File (Offloaded Data) Cost Output Total 
        /// </summary>
        public string CostOutputTotal { get; set; }

        /// <summary>
        /// Gets/Sets the Variance between Input, Offload, and Output Cost Totals
        /// </summary>
        public string CostVariance { get; set; }
    }
}
