// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Common
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Turn off client side cache
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class NoCacheAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// OnResultExecuting
        /// </summary>
        /// <param name="filterContext">ResultExecutingContext</param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext != null)
            {
                filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
                filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Private);
            }

            base.OnResultExecuting(filterContext);
        }
    }
}