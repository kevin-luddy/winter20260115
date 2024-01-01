// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using IES.Standard;

    /// <summary>
    /// Revision Options that include start and end years to refresh rate grid.
    /// </summary>
    public class RevisionOptionModelView : OptionModelView
    {
        /// <summary>
        /// Gets or sets the start year for the revision.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the end year for the revision.
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        /// Revision is needed to display publish message in client.
        /// </summary>
        public string Revision { get; set; }
    }
}
