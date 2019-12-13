// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Lookup_Table
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Parent class for lookup data
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LookUpDataDTO
    {
        /// <summary>
        /// Type Id 
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Type Name
        /// </summary>
        public string TypeName { get; set; }
    }
}
