using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Net;
using IES.Common.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IES.Common.Core.Exceptions
{
	public static class ExceptionMiddlewareExtensions
	{
		//
		// Summary:
		//     Determines whether the specified HTTP request is an AJAX request.
		//
		// Parameters:
		//   request:
		//     The HTTP request.
		//
		// Returns:
		//     true if the specified HTTP request is an AJAX request; otherwise, false.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     The request parameter is null (Nothing in Visual Basic).
		public static bool IsAjaxRequest(this HttpRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			if (request.Headers != null)
			{
				return request.Headers["X-Requested-With"] == "XMLHttpRequest";
			}

			return false;
		}

		public static void ConfigureExceptionHandler(this IApplicationBuilder app)
		{
			var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger("ExceptionMiddlewareExtensions");

			app.UseExceptionHandler(appError =>
			{
				appError.Run(async filterContext =>
				{
				var contextFeature = filterContext.Features.Get<IExceptionHandlerFeature>();

				if (filterContext.Request.IsAjaxRequest())
				{
					// Separate out the ticket link since for classified it will not exist
					string helpDeskLink = "If the error persists, please open a ticket with the Helpdesk at: ";
					string supportLink = CommonUtilities.ServiceCentralLink();

					helpDeskLink = helpDeskLink + supportLink;

					string message = @"An error has occurred. Any changes you made recently might be lost. Please copy your changes, refresh the page and try again.";

					if (!CommonUtilities.DisableExternalHelpLinksForClassifiedInstallations())
					{
						message = message + " " + helpDeskLink;
					}

					string title = "Application Error";
					string returnType = contextFeature.Error.GetType().ToString();
					ICollection<ValidationMessage> messageList = new List<ValidationMessage>();
					string details =
						(contextFeature.Error.Message == null ? "No Message" : contextFeature.Error.Message) +
						"<br />" +
						(contextFeature.Error.StackTrace == null ? "No StackTrace" : contextFeature.Error.StackTrace.ToString());

					if (contextFeature.Error.Message.Contains("has been updated and is out of sync with the data in your browser."))
					{
						message = "A newer version was recently saved by another user. To prevent overriding information, please copy your changes, open the current version and reapply the changes as necessary.";
						title = "Save Error";
						details = contextFeature.Error.Message;
					}
					else if ((contextFeature.Error.GetType() == typeof(EntityCommandExecutionException) || contextFeature.Error.GetType() == typeof(EntityException) || contextFeature.Error.GetType() == typeof(SqlException)) && contextFeature.Error.Message.Contains("See the inner exception for details."))
					{
						if (contextFeature.Error.InnerException != null && contextFeature.Error.InnerException.Message != null && contextFeature.Error.InnerException.Message.Contains("has been updated and is out of sync with the data in your browser."))
						{
							message = "A newer version was recently saved by another user. To prevent overriding information, please copy your changes, open the current version and reapply the changes as necessary.";
							logger.LogWarning(contextFeature.Error.InnerException, message);
							title = "Save Error";
							details = contextFeature.Error.InnerException.Message;
						}
						else
						{
							message = @"An error has occurred. Any changes you made recently might be lost. Please copy your changes, refresh the page and try again.";

							if (!CommonUtilities.DisableExternalHelpLinksForClassifiedInstallations())
							{
								message = message + " " + helpDeskLink;
							}

							logger.LogError(contextFeature.Error.InnerException, message);

							title = "Application Error";
							details = message;
						}
					}
					else if (contextFeature.Error.GetType() == typeof(EntityCommandExecutionException) || contextFeature.Error.GetType() == typeof(EntityException) || contextFeature.Error.GetType() == typeof(SqlException))
					{
						message = @"An error has occurred. Any changes you made recently might be lost. Please copy your changes, refresh the page and try again.";
						logger.LogError(contextFeature.Error, message);

						if (!CommonUtilities.DisableExternalHelpLinksForClassifiedInstallations())
						{
							message = message + " " + helpDeskLink;
						}

						title = "Application Error";
						details = message;
					}
					else if (contextFeature.Error.GetType() == typeof(ValidationException))
					{
						message = contextFeature.Error.Message;
						details = contextFeature.Error.Message;
						if (!string.IsNullOrEmpty(((ValidationException)contextFeature.Error).Title))
						{
							title = ((ValidationException)contextFeature.Error).Title;
						}
						else
						{
							title = "Validation Error";
						}
					}
					else if (contextFeature.Error.GetType() == typeof(GenValidationException))
					{
						returnType = "GenValidationException";
						message = contextFeature.Error.Message;
						details = contextFeature.Error.Message;
						messageList = ((GenValidationException)contextFeature.Error).ValidationList;
						title = "";

					}
					else if (contextFeature.Error.GetType() == typeof(AuthorizationException))
					{

						message = @"You do not have permission to this page. Please contact the workspace owner or open a ticket with Helpdesk at " + supportLink;
						details = contextFeature.Error.Message;
						title = "Authorization Error";
					}
					else
					{
						//unknown error type, log it
						logger.LogError(contextFeature.Error, "Unknown Error");
					}

					//contextFeature.ErrorHandled = true;
					filterContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					// filterContext.Response.TrySkipIisCustomErrors = true;
					var statusCodePagesFeature = filterContext.Features.Get<IStatusCodePagesFeature>();

					if (statusCodePagesFeature is not null)
					{
						statusCodePagesFeature.Enabled = false;
					}

					var error = new
					{
						Message = message,
						Details = details,
						Title = title,
						MessageList = messageList,
						ReturnType = returnType
					};

						await filterContext.Response.WriteAsJsonAsync(error);
						//filterContext.Result = new JsonResult() { Data = error, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
					}
					else
					{
						// The following IF statements are because exceptions thrown inside the RenderAction method get wrapped
						// inside a System.Web.HttpUnhandledException.  This will unwrap the exception if it's an Authorization
						// Exception, and correctly redirect the view to the SecurityError page.
						if (contextFeature.Error != null &&
							contextFeature.Error.GetType() == typeof(AuthorizationException))
						{
							filterContext.Response.Redirect("SecurityError");
						}
						else if (contextFeature.Error != null &&
							contextFeature.Error.InnerException != null &&
							contextFeature.Error.InnerException.GetType() == typeof(AuthorizationException))
						{
							filterContext.Response.Redirect("SecurityError");
						}
						else if (contextFeature.Error != null &&
							contextFeature.Error.InnerException != null &&
							contextFeature.Error.InnerException.InnerException != null &&
							contextFeature.Error.InnerException.InnerException.GetType() == typeof(AuthorizationException))
						{
							filterContext.Response.Redirect("SecurityError");
						}
						else if (contextFeature.Error != null &&
							contextFeature.Error.InnerException != null &&
							contextFeature.Error.InnerException.InnerException != null &&
							contextFeature.Error.InnerException.InnerException.InnerException != null &&
							contextFeature.Error.InnerException.InnerException.InnerException.GetType() == typeof(AuthorizationException))
						{
							filterContext.Response.Redirect("SecurityError");
						}
						else if (contextFeature.Error != null &&
							contextFeature.Error.GetType() == typeof(InvalidDataRelationException))
						{
							filterContext.Response.Redirect("InvalidParameters");
						}
						else
						{
							filterContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
							filterContext.Response.ContentType = "application/json";
							if (contextFeature != null)
							{
								logger.LogError($"Something went wrong: {contextFeature.Error}");
								await filterContext.Response.WriteAsync(new ErrorDetails()
								{
									StatusCode = filterContext.Response.StatusCode,
									Message = "Internal Server Error."
								}.ToString());
							}
						}
					}
				});
			});
		}
	}
}
