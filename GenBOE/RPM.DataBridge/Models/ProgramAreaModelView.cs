// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.DataBridge.Models
{
    /// <summary>
    /// The Model view for a Program Area.
    /// </summary>
    public class ProgramAreaModelView
    {
        /// <summary>
        /// Gets or sets the Program Area
        /// </summary>
        public string ProgramArea { get; set; }

        /// <summary>
        /// Gets or sets the Line of Business associated with the Program Area
        /// </summary>
        public string LineOfBusiness { get; set; }
    }
}
