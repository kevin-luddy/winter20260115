/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
    using System.Web.Http;
    using System.Web.Http.Cors;

    /// <summary>
    /// The Web API Configuration
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            EnableCorsAttribute cors = new EnableCorsAttribute("*", "*", "*");
            cors.SupportsCredentials = true;
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{instanceId}/{id}",
                new {instanceId = RouteParameter.Optional, id = RouteParameter.Optional}
            );

            // DataPower will call these routes
            config.Routes.MapHttpRoute(
                "GetApi",
                "get_api/{controller}/{instanceId}/{id}",
                new { instanceId = RouteParameter.Optional, id = RouteParameter.Optional}
            );
            config.Routes.MapHttpRoute(
                "PostApi",
                "post_api/{controller}/{instanceId}/{id}",
                new {id = RouteParameter.Optional}
            );
            config.Routes.MapHttpRoute(
                "DeleteApi",
                "delete_api/{controller}/{instanceId}/{id}",
                new {id = RouteParameter.Optional}
            );
            config.Routes.MapHttpRoute(
                "PutApi",
                "put_api/{controller}/{instanceId}/{id}",
                new {id = RouteParameter.Optional}
            );
        }
    }
}