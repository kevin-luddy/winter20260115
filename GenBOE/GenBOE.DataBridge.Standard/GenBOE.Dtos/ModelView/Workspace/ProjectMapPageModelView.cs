// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System.Collections.Generic;
    using IES.Standard;

    /// <summary>
    /// Model View housing the paged data as well as Totals
    /// </summary>
    public class ProjectMapPageModelView
    {
        /// <summary>
        /// Gets or sets the total number of project map rows for this workspace.
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Gets or sets the total hours.
        /// </summary>
        public decimal TotalHours { get; set; }

        /// <summary>
        /// Gets or sets the total dollars.
        /// </summary>
        public decimal TotalDollars { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public IReadOnlyCollection<ProjectMapModelView> Data { get; set; }
    }
}
