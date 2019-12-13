// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public interface ICache
    {
        /// <summary>
        /// Adds an object to the cache using sliding expiration. 
        /// </summary>
        /// <param name="inKey">A unique value that is used to store and retrieve the object from the cache.</param>
        /// <param name="inValue">The object saved to the cache cluster.</param>
        /// <param name="inSecondsToCacheItems">Seconds to cache items in the cache.
        /// -1 indicates infinite which will allow .net to garbage collect as needed.</param>
        void Add(String inKey, Object inValue, int inSecondsToCacheItems);

        /// <summary>
        /// Adds an object to the cache using absolute expiration. 
        /// </summary>
        /// <param name="inKey">A unique value that is used to store and retrieve the object from the cache.</param>
        /// <param name="inValue">The object saved to the cache cluster.</param>
        /// <param name="inSecondsToCacheItems">Seconds to cache items in the cache.
        /// -1 indicates infinite which will allow .net to garbage collect as needed.</param>
        void AddAbsolute(String inKey, Object inValue, int inSecondsToCacheItems);

        /// <summary>
        /// Gets an object from the cache using the specified key. 
        /// </summary>
        /// <param name="inKey">The unique value that is used to identify the object in the cache.</param>
        /// <returns>The object that was cached by using the specified key. Null is returned if the key does not exist. </returns>
        object GetData(string inKey);

        /// <summary>
        /// Does a bulk load of data from cache
        /// </summary>
        /// <param name="inCacheKeyToIDDictionary">Dictionary containing keys and their corresponding IDs</param>
        /// <param name="retrievedData">Data retrieved from cache</param>
        /// <param name="retrievedIds">Ids retrieved from cache</param>
        void BulkGetData(Dictionary<string, int> inCacheKeyToIDDictionary, out Collection<object> retrievedData, out Collection<int> retrievedIds);

        /// <summary>
        /// Remove an object from the cache using the specified key
        /// </summary>
        /// <param name="inKey">The unique value that is used to identify the object in the cache.</param>
        void Remove(string inKey);

        /// <summary>
        /// Return true/false depending on if cache contains the passed in key
        /// </summary>
        /// <param name="inKey">The unique value that is used to identify the object in the cache.</param>
        /// <returns>true if cache contains key, false otherwise</returns>
        bool Contains(string inKey);

        /// <summary>
        /// Releases all resources that are used by the current instance of the cache.
        /// </summary>
        void ClearCache();
    }
}
