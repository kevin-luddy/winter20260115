// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for RateConfigLoader
    /// </summary>
    public interface IRateConfigLoader
    {
        /// <summary>
        /// Gets the Rate Format Configuration data.
        /// </summary>
        /// <returns>All rate format configuration rows.</returns>
        ICollection<RateConfigModelView> GetAll();
    }
}
