namespace IES.Common.Core.Interfaces
{
	using System.Collections;

	public interface ICacheDataLoader
	{
		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key.
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone, int secondsToCache);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key.
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key.
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		object GetData(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in BUT DO NOT STORE THE RESULT IN CACHE
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		object GetDataNoCacheResult(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache, bool inUseClone);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in BUT DO NOT STORE THE RESULT IN CACHE
		/// </summary>
		/// <param name="inLoaderMethod">The method to invoke if the data is not in cache</param>
		/// <param name="inLoadMethodParams">The parameters to pass to the loader method, if data not in cache</param>
		/// <param name="inKeyForCache">The key to look in cache for and store back into cache</param>
		/// <returns>the object from cache or the delegates return (invoked if not found in cache)</returns>
		object GetDataNoCacheResult(Delegate inLoaderMethod, object[] inLoadMethodParams, string inKeyForCache);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in BUT DO NOT STORE THE RESULT IN CACHE
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <returns></returns>
		ICollection GetDataNoCacheResult(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, bool inUseClone);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in BUT DO NOT STORE THE RESULT IN CACHE
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <returns></returns>
		ICollection GetDataNoCacheResult(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object.
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <returns></returns>
		ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, bool inUseClone);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object.
		/// </summary>
		/// <param name="inLoaderMethod"></param>
		/// <param name="cacheKeyPrefix"></param>
		/// <param name="inCacheKeyToIDDictionary"></param>
		/// <returns></returns>
		ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary);

		/// <summary>
		/// Remove an object from cache using an unique key
		/// </summary>
		/// <param name="inKeyForCache">the unique key for cache</param>
		void Remove(string inKeyForCache);

		/// <summary>
		/// Get the data from cache, if it exists there.  Otherwise invoke the delegate to
		/// load it in and store the result from the delegate into cache according to the
		/// passed in key prefix and the primary key of the object. 
		/// </summary>
		/// <param name="inLoaderMethod">loader method</param>
		/// <param name="cacheKeyPrefix">cache key prefix</param>
		/// <param name="inCacheKeyToIDDictionary">cahce key dictionary</param>
		/// <param name="inExtraParam">extra paramter needed for loader call but necessarily part of the cache key</param>
		/// <param name="inUseClone">to use clone or not</param>
		/// <returns></returns>
		ICollection GetData(Delegate inLoaderMethod, string cacheKeyPrefix, Dictionary<string, int> inCacheKeyToIDDictionary, int inExtraParam, bool inUseClone);
	}
}
