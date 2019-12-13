// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The Model View used for Burden Pool Details.
    /// </summary>
    [Serializable]
    public class BurdenPoolDetailModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Gets or sets the Burden Pool.
        /// </summary>
        public string BurdenPool { get; set; }

        /// <summary>
        /// Gets or sets the Burden Pool description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets all of the Burden Pool mappings by Burden Element.
        /// </summary>
        [Required]
        public ICollection<BurdenElementIdToRateCodeModelView> BurdenElementRateCodeMappings { get; set; }

        /// <summary>
        /// Array of RateCodes in Burden Element Order for display.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [Required]
        public string[] BurdenElementRateCodeArray { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the G&amp;A T2 Burden Element is applicable for the ProPricer Mission Solutions export
        /// </summary>
        public bool IsGaT2ApplicableForMissionSolutions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the G&amp;A T2 Burden Element is applicable for the Burden Rate and Commercial Burden Rate exports
        /// </summary>
        public bool IncludeGaT2InBurdAndCommBurdTables { get; set; }

        /// <summary>
        /// Version for this model.
        /// </summary>
        public int RevisionID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this burden pool is Commercial or Government.
        /// </summary>
        public bool IsCommercial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to exclude fccom values when exporting to non-Commercial.
        /// </summary>
        public bool ExcludeFCCOMFromCommercial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
