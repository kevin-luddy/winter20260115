// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace APTSPropricerApi
{
    /// <summary>
    /// Enum for How ProPricer export duplicates should be handled
    /// </summary>
    public enum ProPricerExportOption
    {
        /// <summary>
        /// Overwrite duplicates
        /// </summary>
        OverwriteDuplicates = 1,

        /// <summary>
        /// Do not overwrite duplicates
        /// </summary>
        DoNotOverwriteDuplicates = 2,

        /// <summary>
        /// Add value to duplicate
        /// </summary>
        AddValueToDuplicate = 3,

        /// <summary>
        /// Replace all existing
        /// </summary>
        ReplaceAllExisting = 4
    }
}