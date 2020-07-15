// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;
    using IES.Common;

    /// <summary>
    /// The configuration for Routes.
    /// </summary>
    public static class RouteConfig
    {
        /// <summary>
        /// Registers the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <exception cref="System.ArgumentNullException">routes</exception>
        public static void RegisterRoutes(RouteCollection routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException(nameof(routes));
            }

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            string[] namespaces = new string[] { "RDM.Web.Controllers" };
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = IESWebConstants.CONTROLLER_HOME, action = "Index", id = UrlParameter.Optional },
                namespaces: namespaces );
        }
    }
}
