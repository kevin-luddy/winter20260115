// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    /// <summary>
    /// The Model View used for Rate Code with Section detail.
    /// </summary>
    public class RateSectionModelView
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the section.
        /// </summary>
        public int? Section { get; set; }

        /// <summary>
        /// Gets or sets the rate code.
        /// </summary>
        public string RateCode { get; set; }
    }
}
