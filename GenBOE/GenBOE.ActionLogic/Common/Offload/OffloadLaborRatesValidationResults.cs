// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
    using System.Collections.Generic;
    using Dtos;

    /// <summary>
    /// The results from validating the Offloading of Labor Rates against a Labor Resource.
    /// </summary>
    public class OffloadLaborRatesValidationResults
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OffloadLaborRatesValidationResults"/> class.
        /// </summary>
        public OffloadLaborRatesValidationResults()
        {
            this.OffloadRateByYear = new Dictionary<int, OffloadRatesDTO>();
        }

        /// <summary>
        /// Returns true if the Labor Resource can be offloaded.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the Percent to offload for this labor resource.
        /// </summary>
        public decimal? Percent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [missing year].
        /// </summary>
        public bool MissingYear { get; set; }

        /// <summary>
        /// Gets or sets the sub resource.
        /// </summary>
        public ResourceDTO SubResource { get; set; }

        /// <summary>
        /// Gets or sets the offload rate by year.
        /// </summary>
        public Dictionary<int, OffloadRatesDTO> OffloadRateByYear { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the perf org.
        /// </summary>
        public string PerfOrgName { get; set; }

        /// <summary>
        /// Gets or sets the start year.
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        /// Gets or sets the end year.
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        /// Gets or sets the invalid warning text.
        /// </summary>
        public string InvalidWarningText { get; set; }
    }
}
