// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Web.Common
{
    using System;
    using System.Web.Mvc;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;

    // inspired by http://geekswithblogs.net/tyarmer/archive/2010/02/25/strongly-typed-roles-in-mvc-with-authorize-attribute.aspx
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class AuthorizationCoreAttribute : AuthorizeAttribute
    {
        private ISecurityInformation _SecInfo = null;

        protected AuthorizationCoreAttribute()
        {
            // Get the object from the container since you can't pass custom objects to attributes
            _SecInfo = GenBOEUnityContainer.Container.Resolve(typeof(ISecurityInformation)) as ISecurityInformation;
        }

        public ISecurityInformation SecurityInformation { get { return _SecInfo; } }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            base.OnAuthorization(filterContext);

            // for an unauthorized attempt, just return nothing to the user
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new EmptyResult();
            }
        }
    }
}