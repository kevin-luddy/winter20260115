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
    using System.Collections;
    using IES.Common.Interfaces;

    /// <summary>
    /// This class will mediate calls through non cache.
    /// </summary>
    public class NonCacheDataLoader : ICacheDataLoader
    {
        // logger
        private Logger _log = new Logger(typeof(NonCacheDataLoader));

        /// <summary>
        /// For non cache, dynamically invoke the delegate with the passed in parameters. There is
        /// no "Add" option in non cache
        /// </summary>
        /// <param name="inLoaderMethod">The method to invoke </param>
        /// <param name="inLoadMethodParams">The parameters to pass to the loader method</param>
        /// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
        /// <param name="inStoreInCacheByKey">true if storing result in cache, false if not storing in cache</param>
        /// <returns>the delegate</returns>
        public virtual object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache)
        {
            return this.GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, true);
        }

        /// <summary>
        /// For non cache, dynamically invoke the delegate with the passed in parameters. There is
        /// no "Add" option in non cache
        /// </summary>
        /// <param name="inLoaderMethod">The method to invoke </param>
        /// <param name="inLoadMethodParams">The parameters to pass to the loader method</param>
        /// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
        /// <param name="inStoreInCacheByKey">true if storing result in cache, false if not storing in cache</param>
        /// <returns>the delegate</returns>
        public virtual object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone)
        {
            return this.GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, inUseClone, -1);
        }

        /// <summary>
        /// For non cache, dynamically invoke the delegate with the passed in parameters. There is
        /// no "Add" option in non cache
        /// </summary>
        /// <param name="inLoaderMethod">The method to invoke </param>
        /// <param name="inLoadMethodParams">The parameters to pass to the loader method</param>
        /// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
        /// <param name="inStoreInCacheByKey">true if storing result in cache, false if not storing in cache</param>
        /// <returns>the delegate</returns>
        public virtual object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone, int secondsToCache)
        {

            if (inLoaderMethod == null)
            {
                throw new ArgumentNullException(nameof(inLoaderMethod), "Delegate passed to NonCacheDataLoader GetData method is null");
            }

            object toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(string.Format("NonCache GetData with delegate {0} and key {1}", inLoaderMethod.ToString(), inKeyForCache), this._log))
            {
                toReturn = inLoaderMethod.DynamicInvoke(inLoadMethodParams);

                if (inUseClone)
                {
                    toReturn = GenBOEUtilities.Clone<object>(toReturn);
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
        /// load it in BUT DO NOT STORE THE RESULT IN CACHE
        /// </summary>
        /// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
        /// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
        /// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
        /// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
        public virtual object GetDataNoCacheResult(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache)
        {
            return this.GetDataNoCacheResult(inLoaderMethod, inLoadMethodParams, inKeyForCache, true);
        }

        /// <summary>
        /// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
        /// load it in BUT DO NOT STORE THE RESULT IN CACHE
        /// </summary>
        /// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
        /// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
        /// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
        /// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
        public virtual object GetDataNoCacheResult(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone)
        {
            return GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, inUseClone);
        }
        public virtual ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary)
        {
            return GetData(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, true);
        }

        public virtual ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, bool inUseClone)
        {
            if (inLoaderMethod == null)
            {
                throw new ArgumentNullException(nameof(inLoaderMethod), "Delegate passed to NonCacheDataLoader GetData method is null");
            }
            if (inCacheKeyToIDDictionary == null)
            {
                throw new ArgumentNullException(nameof(inCacheKeyToIDDictionary));
            }

            Collection<object> toReturn = new Collection<object>();

            Collection<int> delegateParams = new Collection<int>();
            foreach (KeyValuePair<string, int> potentiallyCachedObject in inCacheKeyToIDDictionary)
            {
                delegateParams.Add(potentiallyCachedObject.Value);
            }

            if (delegateParams.Count > 0)
            {
                ICollection returnedObjects = inLoaderMethod.DynamicInvoke(delegateParams) as ICollection;
                foreach (ICachableDTO item in returnedObjects)
                {
                    if (inUseClone)
                    {
                        toReturn.Add(GenBOEUtilities.Clone<object>(item));
                    }
                    else
                    {
                        toReturn.Add(item);
                    }
                }
            }

            return toReturn;

        }

        /// <summary>
        /// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
        /// load it in BUT DO NOT STORE THE RESULT IN CACHE
        /// </summary>
        /// <param name="inLoaderMethod"></param>
        /// <param name="cacheKeyPrefix"></param>
        /// <param name="inCacheKeyToIDDictionary"></param>
        /// <returns></returns>
        public virtual ICollection GetDataNoCacheResult(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary)
        {
            return GetDataNoCacheResult(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, true);
        }

        /// <summary>
        /// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
        /// load it in BUT DO NOT STORE THE RESULT IN CACHE
        /// </summary>
        /// <param name="inLoaderMethod"></param>
        /// <param name="cacheKeyPrefix"></param>
        /// <param name="inCacheKeyToIDDictionary"></param>
        /// <returns></returns>
        public virtual ICollection GetDataNoCacheResult(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, bool inUseClone)
        {
            return GetData(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, inUseClone);
        }

        /// <summary>
        /// Remove value from non cache
        /// </summary>
        /// <param name="inKeyForCache">The key to look for in noncache to remove</param>
        public virtual void Remove(string inKeyForCache)
        {
            //  There are no keys to remove for non cache
        }

        /// <summary>
        /// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
        /// load it in BUT DO NOT STORE THE RESULT IN CACHE
        /// </summary>
        /// <param name="inLoaderMethod">in loader method</param>
        /// <param name="cacheKeyPrefix">cache key prefix</param>
        /// <param name="inCacheKeyToIDDictionary">cache key dictionary</param>
        /// <param name="inExtraParam"> extra paramter needed for loader call but necessarily part of the cache key</param>
        /// <param name="inUseClone">false</param>
        /// <returns></returns>
        public virtual ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, int inExtraParam, bool inUseClone)
        {
            return this._GetData(inLoaderMethod, inCacheKeyToIDDictionary, inExtraParam, inUseClone);
        }

        /// <summary>
        /// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
        /// load it in BUT DO NOT STORE THE RESULT IN CACHE
        /// </summary>
        /// <param name="inLoaderMethod">in loader method</param>
        /// <param name="cacheKeyPrefix">cache key prefix</param>
        /// <param name="inCacheKeyToIDDictionary">cache key dictionary</param>
        /// <param name="inExtraParam"> extra paramter needed for loader call but necessarily part of the cache key</param>
        /// <param name="inUseClone">false</param>
        /// <returns></returns>
        private ICollection _GetData(Delegate inLoaderMethod,  Dictionary<string, int> inCacheKeyToIDDictionary, int inExtraParam, bool inUseClone)
        {
            if (inLoaderMethod == null)
            {
                throw new ArgumentNullException(nameof(inLoaderMethod), "Delegate passed to CacheDataLoader GetData method is null");
            }
            if (inCacheKeyToIDDictionary == null)
            {
                throw new ArgumentNullException(nameof(inCacheKeyToIDDictionary), "Dictionary passed to CacheDataLoader GetData method is null");
            }

            Collection<object> toReturn = new Collection<object>();
            Collection<int> delegateParams = new Collection<int>();

            foreach (KeyValuePair<string, int> potentiallyCachedObject in inCacheKeyToIDDictionary)
            {
                delegateParams.Add(potentiallyCachedObject.Value);
            }

            if (delegateParams.Count > 0)
            {
                ICollection returnedObjects = inLoaderMethod.DynamicInvoke(delegateParams, inExtraParam) as ICollection;

                foreach (ICachableDTO item in returnedObjects)
                {
                    if (inUseClone)
                    {
                        toReturn.Add(GenBOEUtilities.Clone<object>(item));
                    }
                    else
                    {
                        toReturn.Add(item);
                    }
                }
            }

            return toReturn;
        }


    }
}
