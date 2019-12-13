// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.DataBridge.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The OTIS Opportunity Model.
    /// </summary>
    public class OTISOpportunityDataModelView
    {
        /// <summary>
        /// Gets or sets the opportunities.
        /// </summary>
        public ICollection<OTISOpportunityModelView> Opportunities { get; set; }
    }
}