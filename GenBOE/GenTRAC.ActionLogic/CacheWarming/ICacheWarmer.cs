// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.CacheWarming
{
    /// <summary>
    /// Cache Warming
    /// </summary>
    public interface ICacheWarmer
    {
        /// <summary>
        /// Warms Cache
        /// </summary>
        void DoWarmCache();
    }
}
