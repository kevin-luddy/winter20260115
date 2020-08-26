// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.SqlClient;
    using System.Net;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Handle Error Attribute, overridden to log errors to a file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class HandleErrorAttribute : System.Web.Mvc.HandleErrorAttribute
    {
        /// <summary>
        /// Instance of our logger
        /// </summary>
        private Logger log = new Logger("Unhandled Exception");

        /// <summary>
        /// Event called when an exception is thrown.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                string supportEmailLink = "<a href=\"mailto:" + Utilities.HelpdeskEmailAddress() + "\">" + Utilities.HelpdeskEmailAddress() + "</a>";

                string message = @"An error has occurred. Any changes you made recently might be lost. Please copy your changes, refresh the page and try again. If the error persists, please contact the Helpdesk at " + supportEmailLink;
                string title = "Application Error";
                string returnType = filterContext.Exception.GetType().ToString();
                ICollection<ValidationMessage> messageList = new List<ValidationMessage>();
                string details = 
                    (filterContext.Exception.Message == null ? "No Message" : filterContext.Exception.Message) + 
                    "<br />" + 
                    (filterContext.Exception.StackTrace == null ? "No StackTrace" : filterContext.Exception.StackTrace.ToString());

                if (filterContext.Exception.Message.Contains("has been updated and is out of sync with the data in your browser."))
                {
                    message = "A newer version was recently saved by another user. To prevent overriding information, please copy your changes, open the current version and reapply the changes as necessary.";
                    title = "Save Error";
                    details = filterContext.Exception.Message;
                }
                else if ((filterContext.Exception.GetType() == typeof(EntityCommandExecutionException) || filterContext.Exception.GetType() == typeof(EntityException) || filterContext.Exception.GetType() == typeof(SqlException)) && filterContext.Exception.Message.Contains("See the inner exception for details."))
                {
                    if (filterContext.Exception.InnerException != null && filterContext.Exception.InnerException.Message != null && filterContext.Exception.InnerException.Message.Contains("has been updated and is out of sync with the data in your browser."))
                    {
                        log.Warn(filterContext.Exception.InnerException);

                        message = "A newer version was recently saved by another user. To prevent overriding information, please copy your changes, open the current version and reapply the changes as necessary.";
                        title = "Save Error";
                        details = filterContext.Exception.InnerException.Message;
                    }
                    else
                    {
                        log.Error(filterContext.Exception.InnerException);

                        message = @"An error has occurred. Any changes you made recently might be lost. Please copy your changes, refresh the page and try again. If the error persists, please contact the GenBOE Helpdesk at " + supportEmailLink;
                        title = "Application Error";
                        details = message;
                    }
                }
                else if (filterContext.Exception.GetType() == typeof(EntityCommandExecutionException) || filterContext.Exception.GetType() == typeof(EntityException) || filterContext.Exception.GetType() == typeof(SqlException))
                {
                    log.Error(filterContext.Exception);
                    message = @"An error has occurred. Any changes you made recently might be lost. Please copy your changes, refresh the page and try again. If the error persists, please contact the GenBOE Helpdesk at " + supportEmailLink;
                    title = "Application Error";
                    details = message;
                }
                else if (filterContext.Exception.GetType() == typeof(ValidationException))
                {
                    message = filterContext.Exception.Message;
                    details = filterContext.Exception.Message;
                    if (!string.IsNullOrEmpty(((ValidationException)filterContext.Exception).Title))
                    {
                        title = ((ValidationException)filterContext.Exception).Title;
                    }
                    else
                    {
                        title = "Validation Error";
                    }
                }
                else if (filterContext.Exception.GetType() == typeof(GenValidationException))
                {
                    returnType = "GenValidationException";
                    message = filterContext.Exception.Message;
                    details = filterContext.Exception.Message;
                    messageList = ((GenValidationException)filterContext.Exception).ValidationList;
                    title = "";
                    
                }
                else if (filterContext.Exception.GetType() == typeof(AuthorizationException))
                {
                    message = @"You do not have permission to this page.  Please contact the workspace owner or the Helpdesk at " + supportEmailLink;
                    details = filterContext.Exception.Message;
                    title = "Authorization Error";
                }
                else if (filterContext.Exception.GetType() == typeof(DbQueryOverloadException))
                {
                    message = filterContext.Exception.Message;
                    title = "Database Query Overload Error";
                }
                else
                {
                    //unknown error type, log it
                    log.Error(filterContext.Exception);
                }

                filterContext.ExceptionHandled = true;
                filterContext.RequestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                filterContext.RequestContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                var error = new
                {
                    Message = message,
                    Details = details,
                    Title = title,
                    MessageList = messageList,
                    ReturnType  = returnType
                };
                filterContext.Result = new JsonResult() { Data = error, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                // The following IF statements are because exceptions thrown inside the RenderAction method get wrapped
                // inside a System.Web.HttpUnhandledException.  This will unwrap the exception if it's an Authorization
                // Exception, and correctly redirect the view to the SecurityError page.
                if (filterContext.Exception != null &&
                    filterContext.Exception.GetType() == typeof(AuthorizationException))
                {
                    View = "SecurityError";
                }
                else if (filterContext.Exception != null &&
                    filterContext.Exception.InnerException != null &&
                    filterContext.Exception.InnerException.GetType() == typeof(AuthorizationException))
                {
                    filterContext.Exception = filterContext.Exception.InnerException;
                    View = "SecurityError";
                }
                else if (filterContext.Exception != null &&
                    filterContext.Exception.InnerException != null &&
                    filterContext.Exception.InnerException.InnerException != null &&
                    filterContext.Exception.InnerException.InnerException.GetType() == typeof(AuthorizationException))
                {
                    filterContext.Exception = filterContext.Exception.InnerException.InnerException;
                    View = "SecurityError";
                }
                else  if (filterContext.Exception != null &&
                    filterContext.Exception.InnerException != null &&
                    filterContext.Exception.InnerException.InnerException != null &&
                    filterContext.Exception.InnerException.InnerException.InnerException != null &&
                    filterContext.Exception.InnerException.InnerException.InnerException.GetType() == typeof(AuthorizationException))
                {
                    filterContext.Exception = filterContext.Exception.InnerException.InnerException.InnerException;
                    View = "SecurityError";
                }
                else if (filterContext.Exception != null &&
                    filterContext.Exception.GetType() == typeof(InvalidDataRelationException))
                {
                    View = "InvalidParameters";
                }
            }

            // call the base method
            base.OnException(filterContext);
        }
    }
}