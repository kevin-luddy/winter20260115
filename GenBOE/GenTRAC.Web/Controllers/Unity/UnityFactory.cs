// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers.Unity
{
    using System;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Routing;
    using IES.Common;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Unity Factory
    /// </summary>
    public class UnityFactory : DefaultControllerFactory
    {
        /// <summary>
        /// Logger
        /// </summary>
        private Logger logger = new Logger(typeof(UnityFactory));

        /// <summary>
        /// Unity Container
        /// </summary>
        private IUnityContainer container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">Container to pass in</param>
        public UnityFactory(IUnityContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// Gets an instance of the controller
        /// </summary>
        /// <param name="requestContext">Context</param>
        /// <param name="controllerType">Controller Type</param>
        /// <returns>An instance of a controller</returns>
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            // log the request context if it's not null
            if (requestContext != null && this.logger.DebugEnabled)
            {
                string x = requestContext.HttpContext.Request.Url.ToString();
                string y = string.Empty, z = string.Empty;

                foreach (var key in requestContext.RouteData.Values.Keys)
                {
                    y += "{" + key.ToString() + "} ";
                }

                foreach (var value in requestContext.RouteData.Values.Values)
                {
                    if (value != null)
                    {
                        z += "{" + value.ToString() + "} ";
                    }
                    else
                    {
                        z += "{ null } ";
                    }
                }

                this.logger.Debug(x + " Keys: " + y + " Values: " + z);
            }

            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            if (this.logger.DebugEnabled &&
                requestContext != null &&
                requestContext.HttpContext != null &&
                requestContext.HttpContext.Request != null &&
                requestContext.HttpContext.Request.InputStream != null &&
                /**suppress the form/multipart POST request that has binary file data
                and prevent it from writing to the log file */
                !requestContext.HttpContext.Request.ContentType.StartsWith("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
            {
                var bytes = new byte[requestContext.HttpContext.Request.InputStream.Length];
                requestContext.HttpContext.Request.InputStream.Read(bytes, 0, bytes.Length);
                requestContext.HttpContext.Request.InputStream.Position = 0;
                string content = Encoding.ASCII.GetString(bytes);

                this.logger.Debug("URL: " + requestContext.HttpContext.Request.Url + " POST : " + content);
            }

            return this.container.Resolve(controllerType) as IController;
        }
    }
}