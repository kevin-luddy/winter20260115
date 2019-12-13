// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System.Collections.Generic;

    /// <summary>
    /// Preview Data for a Pro Pricer Export
    /// </summary>
    public class ProPricerPreview
    {
        /// <summary>
        /// Gets or sets the task headers.
        /// </summary>
        public ICollection<string> TaskHeaders { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the task rows.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ICollection<ICollection<string>> TaskRows { get; set; } = new List<ICollection<string>>();

        /// <summary>
        /// Gets or sets the resource headers.
        /// </summary>
        public ICollection<string> ResourceHeaders { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the resource rows.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ICollection<ICollection<string>> ResourceRows { get; set; } = new List<ICollection<string>>();
    }
}
