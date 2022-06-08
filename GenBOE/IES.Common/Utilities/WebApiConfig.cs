// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.Net.Http.Formatting;
	using System.Net.Http.Headers;
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
            _ = config ?? throw new ArgumentNullException(nameof(config));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            JsonMediaTypeFormatter formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            formatter.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

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