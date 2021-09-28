// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.LoadersAndMappers
{
    /// <summary>
    /// Interface for mapper that allows cache warming to take place
    /// </summary>
    public interface ICacheWarmingMapper
    {
        /// <summary>
        /// Perform the warming operation
        /// </summary>
        void DoWarming();
    }
}
