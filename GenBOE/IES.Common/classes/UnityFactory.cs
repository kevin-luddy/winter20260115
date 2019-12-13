// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Text;
    using System.Web.Mvc;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// A factory class for Unity containers.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.DefaultControllerFactory" />
    public class UnityFactory : DefaultControllerFactory
    {
        Logger _log = new Logger(typeof(UnityFactory));

        IUnityContainer _container;
        public UnityFactory(IUnityContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Retrieves the controller instance for the specified request context and controller type.
        /// </summary>
        /// <param name="requestContext">The context of the HTTP request, which includes the HTTP context and route data.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>
        /// The controller instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// requestContext
        /// or
        /// controllerType
        /// </exception>
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException(nameof(requestContext));
            }

            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            try
            {
                if (_log.DebugEnabled)
                {
                    string x = requestContext.HttpContext.Request.Url.ToString();
                    StringBuilder y = new StringBuilder();
                    StringBuilder z = new StringBuilder();

                    foreach (var key in requestContext.RouteData.Values.Keys)
                    {
                        y.Append("{");
                        y.Append(key == null ? "null" : key.ToString());
                        y.Append("} ");
                    }
                    foreach (var value in requestContext.RouteData.Values.Values)
                    {
                        z.Append("{");
                        z.Append(value == null ? "null" : value.ToString());
                        z.Append("} ");
                    }

                    _log.Debug(x + " Keys: " + y.ToString() + " Values: " + z.ToString());


                    if (requestContext != null &&
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
                        _log.Debug("URL: " + requestContext.HttpContext.Request.Url + " POST : " + content);
                    }
                }

                return _container.Resolve(controllerType) as IController;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }
    }
}
