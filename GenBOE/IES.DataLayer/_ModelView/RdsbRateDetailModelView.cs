// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    /// <summary>
    /// The model view for Rate Details for use in RDSB Document Settings
    /// </summary>
    public class RdsbRateDetailModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RdsbRateDetailModelView()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the rate code.
        /// </summary>
        public string RateCode { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the section.
        /// </summary>
        public int Section { get; set; }
    }
}
