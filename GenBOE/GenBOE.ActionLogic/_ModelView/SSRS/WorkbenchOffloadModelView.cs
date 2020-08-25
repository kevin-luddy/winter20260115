// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    /// <summary>
    /// Model View used for SSRS Workbench Offload Report
    /// </summary>
    public class WorkbenchOffloadModelView
    {
        /// <summary>
        /// Gets or sets the CLIN
        /// </summary>
        public string Clin { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets the materials cost value.
        /// </summary>
        public decimal Value { get; set; }
    }
}
