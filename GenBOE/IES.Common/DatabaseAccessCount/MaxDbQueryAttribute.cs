// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// see how many sql calls called during our method, but ignore entity inserts, updates, and deletes, because
    /// there might not be a whole lot we can do there
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class MaxDbQueryAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Set the new maximum
        /// </summary>
        /// <param name="maxQueries">max queries</param>
        public MaxDbQueryAttribute(int maxQueries)
        {
            int maxQueriesConfigured;

            if (maxQueries >= 0)
            {
                HttpContext.Current.Items[DbQueryConstants.MAX_QUERIES] = maxQueries;
            }
            else if ((maxQueriesConfigured = ConfigurationUtilities.GetAppSetting<int>(DbQueryConstants.MAX_QUERIES, -1)) >= 0)
            {
                HttpContext.Current.Items[DbQueryConstants.MAX_QUERIES] = maxQueriesConfigured;
            }
        }

        /// <summary>
        /// Get the maximum
        /// </summary>
        public int MaxQueries
        {
            get
            {
                return (int)HttpContext.Current.Items[DbQueryConstants.MAX_QUERIES];
            }
        }
    }
}
