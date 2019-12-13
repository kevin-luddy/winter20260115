// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.DataBridge.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The model view for Proposal Data.
    /// </summary>
    public class ProposalDataModelView
    {
        /// <summary>
        /// Gets or sets the LOBs used in LOB filter dropdown.
        /// </summary>
        public ICollection<string> LOBs { get; set; }

        /// <summary>
        /// Gets or sets the contract types used in filter dropdown.
        /// </summary>
        public ICollection<string> ContractTypes { get; set; }

        /// <summary>
        /// Gets or sets the program areas used in PA filter dropdown keyed by PA to LOB.
        /// </summary>
        public ICollection<ProgramAreaModelView> ProgramAreas { get; set; }

        /// <summary>
        /// Gets or sets the PTM proposals.
        /// </summary>
        public ICollection<ProposalModelView> Proposals { get; set; }

        /// <summary>
        /// Gets or sets the minimum year to show in the filter dropdown.
        /// </summary>
        public int MinYear { get; set; } 
    }
}