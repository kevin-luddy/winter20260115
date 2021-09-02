// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC
{
    using System;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web.Configuration;
    using System.Web.Http;
    using System.Web.Http.Cors;

    /// <summary>
    /// Class to add Web API to an MVC application
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Register API configuration
        /// </summary>
        public static void Register(HttpConfiguration config)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));

            if (WebConfigurationManager.AppSettings["EnableEppIntegration"] == "true")
            {
                config.EnableCors(new EnableCorsAttribute(WebConfigurationManager.AppSettings["eEPPUrl"], "*", "*") { SupportsCredentials = true });
            }

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            JsonMediaTypeFormatter formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            formatter.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}