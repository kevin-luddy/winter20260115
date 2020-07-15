// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Common
{
    using System.Collections.Generic;
    using IES.Common;
    using Loaders;

    /// <summary>
    /// This class will load the common lookup/reference data from the Loaders
    /// and pass the results through the CacheDataLoader which will cache
    /// if injected with the Cached data loader.
    /// 
    /// Callers should NOT invoke these methods inside of loops. Instead, grab the entire collection or dictionary outside
    /// of the loop. Inside of the loop, you can reference that variable to either lookup values using the dictionary or 
    /// search for names in the collection. This eliminates multiple calls to the cache to re-create the same objects over
    /// and over again.
    /// 
    /// </summary>
    public class CommonDataMapper : ICommonDataMapper
    {
        /// <summary>
        /// The rate detail loader
        /// </summary>
        private IRateDetailLoader rateDetailLoader;

        /// <summary>
        /// The cache data loader
        /// </summary>
        private ICacheDataLoader cacheDataLoader;

        /// <summary>
        /// Constructor with Common Data and Cache Data loader
        /// </summary>
        /// <param name="rateDetailLoader">The Rate Detail data loader.</param>
        /// <param name="cacheDataLoader">the Cache data loader.</param>
        public CommonDataMapper(IRateDetailLoader rateDetailLoader, ICacheDataLoader cacheDataLoader)
        {
            this.cacheDataLoader = cacheDataLoader;
            this.rateDetailLoader = rateDetailLoader;
        }

        /// <summary>
        /// Gets all Resource Classes as an options list.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <returns>Resource Class options list.</returns>
        public ICollection<OptionModelView> GetResourceClassOptions(int revisionId)
        {
            GetResourceClassOptionsDelegate resourceClassDelegate = new GetResourceClassOptionsDelegate(this.rateDetailLoader.GetResourceClassOptions);
            object toReturn = this.cacheDataLoader.GetData(resourceClassDelegate, new object[] { revisionId }, CacheConstants.RESOURCE_CLASS_TYPE, false);
            return toReturn as ICollection<OptionModelView>;
        }
    }

    /// <summary>
    /// Delegate for Retrieving the ResourceClass Options
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <returns>A Collection of ResourceClass Options.</returns>
    public delegate ICollection<OptionModelView> GetResourceClassOptionsDelegate(int revisionId);
}
