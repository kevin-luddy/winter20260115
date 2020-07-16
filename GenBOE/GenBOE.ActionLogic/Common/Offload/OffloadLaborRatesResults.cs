// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
    using System.Collections.Generic;
    using GenBOE.Objects;

    /// <summary>
    /// The results from Offloading Labor Rates.
    /// </summary>
    public class OffloadLaborRatesResults
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OffloadLaborRatesResults"/> class.
        /// </summary>
        public OffloadLaborRatesResults()
        {
            this.Boes = new Queue<FullBoe>();
            this.TotalHoursOffloaded = 0m;
            this.TotalCostOffloaded = 0m;
        }

        /// <summary>
        /// Gets or sets the boes.
        /// </summary>
        public Queue<FullBoe> Boes { get; set; }

        /// <summary>
        /// Gets or sets the total hours offloaded.
        /// </summary>
        public decimal TotalHoursOffloaded { get; set; }

        /// <summary>
        /// Gets or sets the total cost offloaded.
        /// </summary>
        public decimal TotalCostOffloaded { get; set; }
    }
}
