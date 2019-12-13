// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.Common
{
    using System;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;
    using System.Web.Http.Filters;
    using Elmah.Contrib.WebApi;

    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="System.ArgumentNullException">config</exception>
        public static void Register(HttpConfiguration config)
        {
            if (ReferenceEquals(config, null))
            {
                throw new ArgumentNullException(nameof(config));
            }

            // enable elmah
            config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());

            config.Filters.Add(new UnhandledExceptionFilterAttribute());
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Raises the exception event.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (ReferenceEquals(actionExecutedContext, null))
            {
                throw new ArgumentNullException(nameof(actionExecutedContext));
            }

            Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Elmah.Error(actionExecutedContext.Exception));
        }
    }
}