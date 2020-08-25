// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Web.Caching;
    using System.Web;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Simple class stub so any data fetched can be placed into simple cache
    /// until we determine if Azure "Velocity" is going to be utilized or not.
    /// </summary>
    public class MemoryCache : ICache
    {
        private Logger _log = new Logger(typeof(MemoryCache));
        private Cache _cache = null;
        private object LOCK = new object();
        public MemoryCache()
        {
            this._log.Debug("Starting new MemoryCache");
            this._cache = HttpRuntime.Cache;
        }

        /// <summary>
        /// Adds an object to the cache using sliding expiration. 
        /// </summary>
        /// <param name="inKey">A unique value that is used to store and retrieve the object from the cache.</param>
        /// <param name="inValue">The object saved to the cache cluster.</param>
        /// <param name="inSecondsToCacheItems">Seconds to cache items in the cache.
        /// -1 indicates infinite which will allow .net to garbage collect as needed.</param>
        public void Add(String inKey, Object inValue, int inSecondsToCacheItems)
        {
            if (inKey == null)
            {
                throw new ArgumentNullException(nameof(inKey));
            }
            if (inValue == null)
            {
                throw new ArgumentNullException(nameof(inValue), "Object passed to cache Add method is null");
            }

            string key = inKey.ToLower();
            this._log.Debug(string.Format("Adding key {0} for value {1} and will cache for {2} seconds", key, inValue.ToString(), inSecondsToCacheItems));

            TimeSpan cacheTime = (inSecondsToCacheItems < 1) ? new TimeSpan(365, 0, 0, 0, 0) : new TimeSpan(0, 0, inSecondsToCacheItems);

            lock (LOCK)
            {
                this._cache.Insert(key, inValue,
                    null,
                    Cache.NoAbsoluteExpiration,
                    cacheTime,
                    CacheItemPriority.Default,
                    new CacheItemRemovedCallback(this.CacheEntryRemoved));
            }
        }

        /// <summary>
        /// Adds an object to the cache using absolute expiration. 
        /// </summary>
        /// <param name="inKey">A unique value that is used to store and retrieve the object from the cache.</param>
        /// <param name="inValue">The object saved to the cache cluster.</param>
        /// <param name="inSecondsToCacheItems">Seconds to cache items in the cache.
        /// -1 indicates infinite which will allow .net to garbage collect as needed.</param>
        public void AddAbsolute(String inKey, Object inValue, int inSecondsToCacheItems)
        {
            if (inKey == null)
            {
                throw new ArgumentNullException(nameof(inKey));
            }

            if (inValue == null)
            {
                throw new ArgumentNullException(nameof(inValue), "Object passed to cache Add method is null");
            }

            string key = inKey.ToLower();
            this._log.Debug($"Adding key {key} for value {inValue.ToString()} and will cache for {inSecondsToCacheItems} seconds");
            
            lock (this.LOCK)
            {
                this._cache.Insert(key, inValue,
                    null,
                    DateTime.Now.AddSeconds(inSecondsToCacheItems),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Default,
                    new CacheItemRemovedCallback(this.CacheEntryRemoved));
            }
        }

        /// <summary>
        /// Gets an object from the cache using the specified key. 
        /// </summary>
        /// <param name="inKey">The unique value that is used to identify the object in the cache.</param>
        /// <returns>The object that was cached by using the specified key. Null is returned if the key does not exist. </returns>
        public Object GetData(string inKey)
        {
            if (inKey == null)
            {
                throw new ArgumentNullException(nameof(inKey));
            }

            object data;
            string key = inKey.ToLower();

            lock (LOCK)
            {
                data = this._cache.Get(key);
            }

            return data;
        }

        /// <summary>
        /// Does a bulk load of data from cache
        /// </summary>
        /// <param name="inCacheKeyToIDDictionary">Dictionary containing keys and their corresponding IDs</param>
        /// <param name="retrievedData">Data retrieved from cache</param>
        /// <param name="retrievedIds">Ids retrieved from cache</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void BulkGetData(Dictionary<string, int> inCacheKeyToIDDictionary, out Collection<object> retrievedData, out Collection<int> retrievedIds)
        {
            retrievedData = new Collection<object>();
            retrievedIds = new Collection<int>();

            foreach (KeyValuePair<string, int> dataPoint in inCacheKeyToIDDictionary)
            {
                if (this.Contains(dataPoint.Key))
                {
                    retrievedData.Add(this.GetData(dataPoint.Key));
                    retrievedIds.Add(dataPoint.Value);
                }
            }
        }

        /// <summary>
        /// Remove an object from the cache using the specified key
        /// </summary>
        /// <param name="inKey">The unique value that is used to identify the object in the cache.</param>
        public void Remove(string inKey)
        {
            if (inKey == null)
            {
                throw new ArgumentNullException(nameof(inKey));
            }

            string key = inKey.ToLower();
            this._log.Debug(string.Format("Removing cached item for key {0}", key));

            lock (LOCK)
            {
                this._cache.Remove(key);
            }
        }

        /// <summary>
        /// Return true/false depending on if cache contains the passed in key
        /// </summary>
        /// <param name="inKey">The unique value that is used to identify the object in the cache.</param>
        /// <returns>true if cache contains key, false otherwise</returns>
        public bool Contains(string inKey)
        {
            if (inKey == null)
            {
                throw new ArgumentNullException(nameof(inKey));
            }

            object data;
            string key = inKey.ToLower();

            lock (LOCK)
            {
                data = this._cache.Get(key);
            }

            return (data != null);
        }

        /// <summary>
        /// Releases all resources that are used by the current instance of the cache.
        /// </summary>
        public void ClearCache()
        {
            lock (LOCK)
            {
                IDictionaryEnumerator enumerator = _cache.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    _cache.Remove(enumerator.Key.ToString().ToLower());
                }
            }
        }

        public void CacheEntryRemoved(string inKey, object inValue, CacheItemRemovedReason inReason)
        {
            if (inKey == null)
            {
                throw new ArgumentNullException(nameof(inKey));
            }

            if (inReason != CacheItemRemovedReason.Removed)
            {
                string key = inKey.ToLower();
                this._log.Debug("CACHE REMOVED : entry key " + key + " removed because " + inReason.ToString());
            }
        }
    }
}
