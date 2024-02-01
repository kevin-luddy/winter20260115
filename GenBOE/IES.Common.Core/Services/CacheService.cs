// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Services
{
	using System.Collections.ObjectModel;
	using System.Reflection;
	using IES.Common.Core.Interfaces;
	using Microsoft.Extensions.Caching.Memory;
	using Microsoft.Extensions.Logging;

	public class CacheService : ICacheService
	{
		private readonly ILogger _log;
		private readonly object LOCK = new();
		private readonly IMemoryCache _cache;

		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="cache">memory cache</param>
		public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
		{
			_cache = cache;
			_log = logger;
		}

		public void CacheEntryRemoved(object key, object value, EvictionReason reason, object state)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (reason != EvictionReason.Removed)
			{
				string keyString = key.ToString().ToLower();
				_log.LogDebug("CACHE REMOVED : entry key " + keyString + " removed because " + reason.ToString());
			}
		}

		/// <summary>
		/// Adds an object to the cache using sliding expiration. 
		/// </summary>
		/// <param name="inKey">A unique value that is used to store and retrieve the object from the cache.</param>
		/// <param name="inValue">The object saved to the cache cluster.</param>
		/// <param name="inSecondsToCacheItems">Seconds to cache items in the cache.
		/// -1 indicates infinite which will allow .net to garbage collect as needed.</param>
		public void Add(string inKey, object inValue, int inSecondsToCacheItems)
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
			_log.LogDebug(string.Format("Adding key {0} for value {1} and will cache for {2} seconds", key, inValue.ToString(), inSecondsToCacheItems));

			TimeSpan cacheTime = inSecondsToCacheItems < 1 ? new TimeSpan(365, 0, 0, 0, 0) : new TimeSpan(0, 0, inSecondsToCacheItems);

			MemoryCacheEntryOptions options = new()
			{
				SlidingExpiration = cacheTime,
				Priority = CacheItemPriority.Normal
			};

			options.RegisterPostEvictionCallback(CacheEntryRemoved);

			lock (LOCK)
			{
				_cache.Set(key, inValue, options);
			}
		}

		/// <summary>
		/// Adds an object to the cache using absolute expiration. 
		/// </summary>
		/// <param name="inKey">A unique value that is used to store and retrieve the object from the cache.</param>
		/// <param name="inValue">The object saved to the cache cluster.</param>
		/// <param name="inSecondsToCacheItems">Seconds to cache items in the cache.
		/// -1 indicates infinite which will allow .net to garbage collect as needed.</param>
		public void AddAbsolute(string inKey, object inValue, int inSecondsToCacheItems)
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
			_log.LogDebug($"Adding key {key} for value {inValue.ToString()} and will cache for {inSecondsToCacheItems} seconds");

			MemoryCacheEntryOptions options = new()
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(inSecondsToCacheItems),
				Priority = CacheItemPriority.Normal
			};

			options.RegisterPostEvictionCallback(CacheEntryRemoved);

			lock (LOCK)
			{
				_cache.Set(key, inValue, options);
			}
		}

		/// <summary>
		/// Does a bulk load of data from cache
		/// </summary>
		/// <param name="inCacheKeyToIDDictionary">Dictionary containing keys and their corresponding IDs</param>
		/// <param name="retrievedData">Data retrieved from cache</param>
		/// <param name="retrievedIds">Ids retrieved from cache</param>
		public void BulkGetData(Dictionary<string, int> inCacheKeyToIDDictionary, out Collection<object> retrievedData, out Collection<int> retrievedIds)
		{
			retrievedData = new Collection<object>();
			retrievedIds = new Collection<int>();

			foreach (KeyValuePair<string, int> dataPoint in inCacheKeyToIDDictionary)
			{
				if (Contains(dataPoint.Key))
				{
					retrievedData.Add(GetData(dataPoint.Key));
					retrievedIds.Add(dataPoint.Value);
				}
			}
		}

		/// <summary>
		/// Releases all resources that are used by the current instance of the cache.
		/// </summary>
		public void ClearCache()
		{
			lock (LOCK)
			{
				// .Net 7
				//if (_cache is MemoryCache cache)
				//{
				//	cache.Clear();
				//}

				PropertyInfo prop = _cache.GetType().GetProperty("EntriesCollection", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public);
				if (prop is null)
				{
					return;
				}

				object innerCache = prop.GetValue(_cache);
				MethodInfo clearMethod = innerCache.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public);
				clearMethod.Invoke(innerCache, null);
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
				data = _cache.Get(key);
			}

			return data != null;
		}

		/// <summary>
		/// Gets an object from the cache using the specified key. 
		/// </summary>
		/// <param name="inKey">The unique value that is used to identify the object in the cache.</param>
		/// <returns>The object that was cached by using the specified key. Null is returned if the key does not exist. </returns>
		public object GetData(string inKey)
		{
			if (inKey == null)
			{
				throw new ArgumentNullException(nameof(inKey));
			}

			object data;
			string key = inKey.ToLower();

			lock (LOCK)
			{
				data = _cache.Get(key);
			}

			return data;
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
			_log.LogDebug(string.Format("Removing cached item for key {0}", key));

			lock (LOCK)
			{
				_cache.Remove(key);
			}
		}
	}
}
