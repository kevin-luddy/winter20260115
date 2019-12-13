// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2013 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenProfit.Web.Common
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using GenProfit.Common;
    using GenProfit.Common.UserLookup;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// inspired by http://geekswithblogs.net/tyarmer/archive/2010/02/25/strongly-typed-roles-in-mvc-with-authorize-attribute.aspx
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class AuthorizeDomesticUsersAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The security info
        /// </summary>
        private ISecurityInformation secInfo = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public AuthorizeDomesticUsersAttribute()
        {
            // Get the object from the container since you can't pass custom objects to attributes
            this.secInfo = GenProfitUnityContainer.Container.Resolve(typeof(ISecurityInformation)) as ISecurityInformation;
        }

        /// <summary>
        /// Checks authorization
        /// </summary>
        /// <param name="filterContext">The current context</param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            base.OnAuthorization(filterContext);

            // for an unauthorized attempt, just return nothing to the user
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new EmptyResult();
            }
        }

        /// <summary>
        /// Checks if a user is authorized
        /// </summary>
        /// <param name="httpContext">The current context</param>
        /// <returns>Authorization boolean</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return this.secInfo.CanCreateProposal(httpContext.User);
        }
    }
}