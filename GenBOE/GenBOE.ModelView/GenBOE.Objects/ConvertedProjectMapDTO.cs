// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;

    /// <summary>
    /// Results from a converted Project Map Model View
    /// </summary>
    public class ConvertedProjectMapDTO 
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ConvertedProjectMapDTO()
        {
            this.Boes = new Collection<FullBoe>();
            this.Clins = new Collection<FullClin>();
            this.Wbs = new Collection<FullWbs>();
            this.Tasks = new Collection<BoeTaskElementDTO>();
        }

        /// <summary>
        /// Gets or sets the earliest start.
        /// </summary>
        public DateTime EarliestStart { get; set; }

        /// <summary>
        /// Gets or sets the latest end.
        /// </summary>
        public DateTime LatestEnd { get; set; }

        /// <summary>
        /// Gets the boes.
        /// </summary>
        public ICollection<FullBoe> Boes { get; private set; }

        /// <summary>
        /// Gets the clins.
        /// </summary>
        public ICollection<FullClin> Clins { get; private set; }

        /// <summary>
        /// Gets the WBS.
        /// </summary>
        public ICollection<FullWbs> Wbs { get; private set; }

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        public ICollection<BoeTaskElementDTO> Tasks { get; private set; }
    }
}
