// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Loaders
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using IES.Common.Core;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// This class will mediate calls through the cache and, if not found, will
	/// invoke a delete to obtain the data (typically from the database but could be
	/// anywhere the caller wants based on the delegate they pass in).
	/// </summary>
	public class CacheDataLoader : ICacheDataLoader
	{
		private readonly ILogger _log;

		/// <summary>
		/// The proxy for the cache itself
		/// </summary>
		private readonly ICacheService _CacheProxy;

		/// <summary>
		/// Seconds to cache items in the cache.  -1 indicates infinite which will
		/// allow .net to garbage collect as needed.
		/// </summary>
		private readonly int _SecondsToCacheItems = -1;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger">loggger</param>
		/// <param name="inCacheProxy">The cache repository</param>
		/// <param name="inSecondsToCacheItems">Seconds to cache items in the cache.
		/// -1 indicates infinite which will allow .net to garbage collect as needed.</param>
		public CacheDataLoader(ILogger<CacheDataLoader> logger, ICacheService inCacheProxy, int inSecondsToCacheItems = 120)
		{
			_log = logger;
			_CacheProxy = inCacheProxy;
			_SecondsToCacheItems = inSecondsToCacheItems;
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key.
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		public virtual object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache)
		{
			return _GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, true, true, _SecondsToCacheItems);
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key.
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <param name="inUseClone">Make a clone of the object before returning from cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		public virtual object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone, int secondsToCache)
		{
			return _GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, true, inUseClone, secondsToCache);
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key.
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <param name="inUseClone">Make a clone of the object before returning from cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		public virtual object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone)
		{
			return _GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, true, inUseClone, _SecondsToCacheItems);
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in BUT DO NOT STORE THE RESULT IN CACHE
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <param name="inUseClone">Make a clone of the object before returning from cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		public virtual object GetDataNoCacheResult(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone)
		{
			return _GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, false, inUseClone, _SecondsToCacheItems);
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
			return _GetData(inLoaderMethod, inLoadMethodParams, inKeyForCache, false, true, _SecondsToCacheItems);
		}


		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key.
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <param name="inUseClone">Make a clone of the object before returning from cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		/// SUPPRESSION NOTE: Added prefix "LOCK" to avoid any collisions on the string itself .. the string is already pretty unique
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		public virtual object _GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inStoreInCacheByKey, bool inUseClone, int secondsToCache)
		{
			if (inLoaderMethod == null)
			{
				throw new ArgumentNullException(nameof(inLoaderMethod), "Delegate passed to CacheDataLoader GetData method is null");
			}
			if (inKeyForCache == null)
			{
				throw new ArgumentNullException(nameof(inKeyForCache), "Cache Key passed into CacheDataLoader GetData method is null");
			}

			inKeyForCache = inKeyForCache.ToLower(); // force lowercase strings in the event we're caching on strings (ntid, shortname, etc)

			object toReturn = _CacheProxy.GetData(inKeyForCache);

			if (toReturn == null)
			{
				using (StopwatchTimer sw = new(_log, "MISS - " + inLoaderMethod.Method.Name + " -> Key: " + inKeyForCache))
				{
					// lock since we are going to overwrite cache key
					lock (string.Intern("LOCK" + inKeyForCache))
					{
						// double check now that we have lock that someone else
						// didn't load the cache up with our requested value
						// .. this avoid unnecessary database retrievals
						toReturn = _CacheProxy.GetData(inKeyForCache);

						if (toReturn == null)
						{
							// still didn't have any data in cache ... load it from database
							// while we have exclusive lock!
							_log.LogDebug("Cache MISS for key " + inKeyForCache);

							toReturn = inLoaderMethod.DynamicInvoke(inLoadMethodParams);

							if (toReturn == null)
							{
								// NULL returned from dataloader is a problem .. and we can't cache it (nor should we want to)
								_log.LogError("Cannot set value [NULL] returned from DataLoader delegate into cache for key " + inKeyForCache + ". Continuing but value [NULL] is NOT CACHED");
							}
							else
							{
								// push value back into cache for next caller
								if (inStoreInCacheByKey)
								{
									_CacheProxy.Add(inKeyForCache, toReturn, secondsToCache);
								}
							}
						}
					}
				}
			}

			// make a clone of the object if directed to do so
			if (toReturn != null && inUseClone)
			{
				toReturn = toReturn.DeepClone();
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
		/// <param name="inUseClone">Make a clone of the object before returning from cache</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		public virtual ICollection GetDataNoCacheResult(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, bool inUseClone)
		{
			return _GetData(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, false, inUseClone);
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in BUT DO NOT STORE THE RESULT IN CACHE
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		public virtual ICollection GetDataNoCacheResult(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary)
		{
			return _GetData(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, false, true);
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object.
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <param name="inUseClone">Make a clone of the object before returning from cache</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		public virtual ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, bool inUseClone)
		{
			return _GetData(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, true, inUseClone);
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object.
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		public virtual ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary)
		{
			return _GetData(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, true, true);
		}

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object.
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <param name="inUseClone">Make a clone of the object before returning from cache</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		private ICollection _GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, bool inStoreInCacheByKey, bool inUseClone)
		{
			if (inLoaderMethod == null)
			{
				throw new ArgumentNullException(nameof(inLoaderMethod), "Delegate passed to CacheDataLoader GetData method is null");
			}
			if (inCacheKeyToIDDictionary == null)
			{
				throw new ArgumentNullException(nameof(inCacheKeyToIDDictionary), "Dictionary passed to CacheDataLoader GetData method is null");
			}

			Collection<object> toReturn = new();
			Collection<int> delegateParams = new();

			foreach (KeyValuePair<string, int> potentiallyCachedObject in inCacheKeyToIDDictionary)
			{
				string key = potentiallyCachedObject.Key.ToLower();

				if (_CacheProxy.Contains(key))
				{
					if (inUseClone)
					{
						toReturn.Add(_CacheProxy.GetData(key).DeepClone());
					}
					else
					{
						toReturn.Add(_CacheProxy.GetData(key));
					}
				}
				else
				{
					_log.LogDebug("Cache MISS for key " + key);
					delegateParams.Add(potentiallyCachedObject.Value);
				}
			}

			if (delegateParams.Count > 0)
			{
				ICollection returnedObjects = inLoaderMethod.DynamicInvoke(delegateParams) as ICollection;

				foreach (ICachableDTO item in returnedObjects)
				{
					string cacheKey = (cacheKeyPrefix + "_" + item.GetPrimaryKeyID()).ToLower();

					// lock since we are going to overwrite cache key
					lock (string.Intern("LOCK" + cacheKey))
					{
						// push value back into cache for next caller
						if (inStoreInCacheByKey)
						{
							_CacheProxy.Add(cacheKey, item, _SecondsToCacheItems);
						}
					}

					if (inUseClone)
					{
						toReturn.Add(item.DeepClone());
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
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object. For this case, an extra parameter is needed
		/// </summary>
		/// <param name="inLoaderMethod">loader method</param>
		/// <param name="cacheKeyPrefix">cache key prefix</param>
		/// <param name="inCacheKeyToIDDictionary">cache key dictionary</param>
		/// <param name="inExtraParam">extra parameter needed for loader call but necessarily part of the cache key</param>
		/// <param name="inUseClone">to use clone or not</param>
		/// <returns></returns>
		public virtual ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, int inExtraParam, bool inUseClone)
		{
			return _GetData(inLoaderMethod, cacheKeyPrefix, inCacheKeyToIDDictionary, inExtraParam, true, inUseClone);
		}


		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object. For this case, an extra parameter is needed
		/// </summary>
		/// <param name="inLoaderMethod">loader method</param>
		/// <param name="cacheKeyPrefix">cache key prefix</param>
		/// <param name="inCacheKeyToIDDictionary">cahce key dictionary</param>
		/// <param name="inExtraParam">extra paramter needed for loader call but necessarily part of the cache key</param>
		/// <param name="inStoreInCacheByKey">store in cache by key</param>
		/// <param name="inUseClone">to use clone or not</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
		private ICollection _GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, int inExtraParam, bool inStoreInCacheByKey, bool inUseClone)
		{
			if (inLoaderMethod == null)
			{
				throw new ArgumentNullException(nameof(inLoaderMethod), "Delegate passed to CacheDataLoader GetData method is null");
			}
			if (inCacheKeyToIDDictionary == null)
			{
				throw new ArgumentNullException(nameof(inCacheKeyToIDDictionary), "Dictionary passed to CacheDataLoader GetData method is null");
			}

			Collection<object> toReturn = new();
			Collection<int> delegateParams = new();

			foreach (KeyValuePair<string, int> potentiallyCachedObject in inCacheKeyToIDDictionary)
			{
				string key = potentiallyCachedObject.Key.ToLower();

				if (_CacheProxy.Contains(key))
				{
					if (inUseClone)
					{
						toReturn.Add(_CacheProxy.GetData(key).DeepClone());
					}
					else
					{
						toReturn.Add(_CacheProxy.GetData(key));
					}
				}
				else
				{
					_log.LogDebug("Cache MISS for key " + key);
					delegateParams.Add(potentiallyCachedObject.Value);
				}
			}

			if (delegateParams.Count > 0)
			{
				ICollection returnedObjects = inLoaderMethod.DynamicInvoke(delegateParams, inExtraParam) as ICollection;

				foreach (ICachableDTO item in returnedObjects)
				{
					string cacheKey = (cacheKeyPrefix + "_" + item.GetPrimaryKeyID()).ToLower();
					// lock since we are going to overwrite cache key
					lock (string.Intern("LOCK" + cacheKey))
					{
						// push value back into cache for next caller
						if (inStoreInCacheByKey)
						{
							_CacheProxy.Add(cacheKey, item, _SecondsToCacheItems);
						}
					}

					if (inUseClone)
					{
						toReturn.Add(item.DeepClone());
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
		/// Remove value from cache
		/// </summary>
		/// <param name="inKeyForCache">The key to look for in cache to remove</param>
		public virtual void Remove(string inKeyForCache)
		{
			if (inKeyForCache != null)
			{
				_CacheProxy.Remove(inKeyForCache.ToLower());
			}
		}
	}
}
