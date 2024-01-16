// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common.Core;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the ProPricerRateCodeXrefLoader
    /// </summary>
    public interface IProPricerRateCodeXrefLoader : IBulkDataLoader<ProPricerRateCodeXrefModelView>
    {
        /// <summary>
        /// Get ProPricer Rate Code Xrefs for Rate Code ID
        /// </summary>
        /// <param name="rateCodeId">Rate Code ID</param>
        /// <returns>ProPricer Rate Code Xrefs for Rate Code ID</returns>
        ICollection<ProPricerRateCodeXrefModelView> GetByRateCodeId(int rateCodeId);
    }
}
