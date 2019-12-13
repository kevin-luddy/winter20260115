// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Common
{
    using System;
    using System.Web.Mvc;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Attritbute for determining if user has access to the ProPricer Export
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ProPricerExportAccessAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Security information
        /// </summary>
        private ISecurityInformation securityInformation = null;

        /// <summary>
        /// Factory
        /// </summary>
        private IFullObjectFactory factory;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProPricerExportAccessAttribute()
        {
            // Get the object from the container since you can't pass custom objects to attributes
            securityInformation = GenBOEUnityContainer.Container.Resolve(typeof(ISecurityInformation)) as ISecurityInformation;
            factory = GenBOEUnityContainer.Container.Resolve(typeof(IFullObjectFactory)) as IFullObjectFactory;
        }
        
        /// <summary>
        /// When OnAuthorization is called, check if user has access to ProPricer Export and throw an exception if not
        /// </summary>
        /// <param name="filterContext">Filter Context</param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            string workspace = filterContext.RouteData.Values["workspace"].ToString();
            bool isProjectMapWs = this.factory.CreateFullWorkspace(workspace).IsProjectMapWorkspace;

            base.OnAuthorization(filterContext);

            // If it's not a project map & you are not authorized -> fail
            if (!isProjectMapWs && !this.securityInformation.IsAllowedProPricerAccess(this.securityInformation.ActiveUserNTID))
            {
                throw new AuthorizationException("You are not authorized to access the ProPricer Export.");
            }
        }
    }
}