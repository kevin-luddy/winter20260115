// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;

namespace IES.Common
{
    /// <summary>
    /// Routing utilities
    /// </summary>
    /// <see cref="http://bradwilson.typepad.com/blog/2010/07/testing-routing-and-url-generation-in-aspnet-mvc.html"/>
    /// <remarks>Primarily used to identify workspace-based vs. BOE-based image routes for rich-text image support.</remarks>
    public static class RoutingUtilities
    {
        /// <summary>
        /// Determine (mapped) route data values for a URL
        /// </summary>
        /// <param name="routeUrl">URL to be resolved</param>
        /// <returns>Route data info</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
        public static RouteData ResolveRoute(string routeUrl)
        {
            string url = String.Format("/{0}", routeUrl);  // prepend additional slash or first segment will not resolve correctly

            HttpContextBase context = new StubHttpContextForRouting(requestUrl: url);

            return RouteTable.Routes.GetRouteData(context);
        }

        /// <summary>
        /// HTTP context surrogate
        /// </summary>
        private class StubHttpContextForRouting : HttpContextBase
        {
            /// <summary>
            /// The web request
            /// </summary>
            StubHttpRequestForRouting _request;

            /// <summary>
            /// The web response
            /// </summary>
            StubHttpResponseForRouting _response;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="appPath">The virtual root path of the application on the server</param>
            /// <param name="requestUrl">URL of the current request</param>
            public StubHttpContextForRouting(string appPath = "/", string requestUrl = "~/")
            {
                _request = new StubHttpRequestForRouting(appPath, requestUrl);
                _response = new StubHttpResponseForRouting();
            }

            /// <summary>
            /// The web request
            /// </summary>
            public override HttpRequestBase Request
            {
                get { return _request; }
            }

            /// <summary>
            /// The web response
            /// </summary>
            public override HttpResponseBase Response
            {
                get { return _response; }
            }
        }

        /// <summary>
        /// HTTP request surrogate
        /// </summary>
        private class StubHttpRequestForRouting : HttpRequestBase
        {
            /// <summary>
            /// The virtual root path of the application on the server
            /// </summary>
            string _appPath;

            /// <summary>
            /// URL of the current request
            /// </summary>
            string _requestUrl;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="appPath">The virtual root path of the application on the server</param>
            /// <param name="requestUrl">URL of the current request</param>
            public StubHttpRequestForRouting(string appPath, string requestUrl)
            {
                _appPath = appPath;
                _requestUrl = requestUrl;
            }

            /// <summary>
            /// The virtual root path of the application on the server
            /// </summary>
            public override string ApplicationPath
            {
                get { return _appPath; }
            }

            /// <summary>
            /// URL of the current request
            /// </summary>
            public override string AppRelativeCurrentExecutionFilePath
            {
                get { return _requestUrl; }
            }

            /// <summary>
            /// Path information for a resource that has a URL extension
            /// </summary>
            public override string PathInfo
            {
                get { return string.Empty; }
            }

            /// <summary>
            /// The collection of web server variables
            /// </summary>
            public override NameValueCollection ServerVariables
            {
                get { return new NameValueCollection(); }
            }
        }

        /// <summary>
        /// HTTP response surrogate
        /// </summary>
        private class StubHttpResponseForRouting : HttpResponseBase
        {
            /// <summary>
            /// When overridden in a derived class, adds a session ID to the virtual path
            /// if the session is using System.Web.Configuration.SessionStateSection.Cookieless
            /// session state, and returns the combined path.
            /// </summary>
            /// <param name="virtualPath">The virtual path of a resource</param>
            /// <returns>The virtual path, with the session ID inserted.</returns>
            public override string ApplyAppPathModifier(string virtualPath)
            {
                return virtualPath;
            }
        }
    }
}
