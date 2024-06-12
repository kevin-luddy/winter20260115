// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Utilities
{
	using Microsoft.Extensions.Logging;
	using System;

    public static class SafeUriUtility
    {
		/// <summary>
		/// Logger
		/// </summary>
		private static ILogger logger;

		public static void Initialize(ILogger logger)
		{
			SafeUriUtility.logger = logger;
		}

        /// <summary>
        /// Create a Uri without throwing an exception
        /// </summary>
        /// <param name="uriString">Uri String</param>
        /// <returns>A Uri object or null if the uri string is invalid</returns>
        public static Uri safeUri(string uriString)
        {
			Uri result = null;
			try
			{
				result = new Uri(uriString);
			}
			catch (UriFormatException e)
			{
				logger?.LogError(e, uriString + " is not a valid URL");
			}
			return result;
        }
    }
}
