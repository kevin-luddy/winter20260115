// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;

    /// <summary>
    /// Utility for generating and verifying a nonce.
    /// </summary>
    public static class NonceUtility
    {
        /// <summary>
        /// The cache in memory.
        /// </summary>
        private static MemoryCache cache = new MemoryCache();

        /// <summary>
        /// The number of seconds to store in cache - 5 minutes
        /// </summary>
        private const int SECONDS_TO_CACHE_NONCE = 300;

        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="action">The action/report name to generate for.</param>
        /// <returns>A newly generated nonce for a workspace/action combination.</returns>
        public static string GenerateNonce(int id, string action)
        {
            if (id < 1)
            {
                throw new ArgumentException("The id must be a positive integer.");
            }

            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException(nameof(action));
            }

            string nonce = Guid.NewGuid().ToString("N");
            string cacheKey = CreateCacheKey(nonce, id, action);

            cache.Add(cacheKey, nonce, SECONDS_TO_CACHE_NONCE);

            return nonce;
        }

        /// <summary>
        /// Determines whether a nonce is valid.
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <param name="id">Unique id.</param>
        /// <param name="action">The action.</param>
        /// <returns>
        ///   <c>true</c> if [is nonce valid] [the specified nonce]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">workspaceName or action</exception>
        public static bool IsNonceValid(string nonce, int id, string action)
        {
            if (id < 1)
            {
                throw new ArgumentException("The id must be a positive integer.");
            }

            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException(nameof(action));
            }

            bool isValid = false;
            string cacheKey = CreateCacheKey(nonce, id, action);

            if (cache.Contains(cacheKey))
            {
                isValid = true;
                cache.Remove(cacheKey);
            }

            return isValid;
        }

        /// <summary>
        /// Creates the cache key based on a nonce, workspace name, and an action.
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <param name="id">Unique id.</param>
        /// <param name="action">The action.</param>
        /// <returns>A key used for the Cache.</returns>
        private static string CreateCacheKey(string nonce, int id, string action)
        {
            return $"{id}::{action}::{nonce}";
        }
    }
}
