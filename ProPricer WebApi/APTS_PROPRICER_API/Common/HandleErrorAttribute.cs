/*
    Copyright 2016-2018 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
    using System;
    using System.Web.Mvc;

    /// <summary>
    /// Handle Error Attribute, overridden to log errors to a file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class HandleErrorAttribute : System.Web.Mvc.HandleErrorAttribute
    {
        /// <summary>
        /// Instance of our logger
        /// </summary>
        private readonly Logger log = new Logger("Unhandled Exception");

        /// <summary>
        /// Event called when an exception is thrown.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            //unknown error type, log it
            this.log.Error(filterContext.Exception);

            // call the base method
            base.OnException(filterContext);
        }
    }
}