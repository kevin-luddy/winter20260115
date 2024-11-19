// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Logging
{
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Security;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Serilog.Ui.Core.Interfaces;
	using Serilog.Ui.Web.Authorization;

	/// <summary>
	/// Custom Authorize Handler 
	/// </summary>
	public class CustomAuthorizeFilter : IUiAsyncAuthorizationFilter
	{
		/// <summary>
		/// Http Context Accessor
		/// </summary>
		private readonly IHttpContextAccessor httpContextAccessor;
		
		/// <summary>
		/// Token Authentication Scheme Handler
		/// </summary>
		private ILogger logger;

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="logger">The logger</param>
		public CustomAuthorizeFilter(ILogger<CustomAuthorizeFilter> logger, IHttpContextAccessor httpContextAccessor)
		{
			this.logger = logger;
			this.httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// Authorizes only Admins for this custom filter
		/// </summary>
		/// <param name="httpContext">The http context</param>
		/// <returns>True if Admin; otherwise false.</returns>
		public async Task<bool> AuthorizeAsync()
		{
			HttpContext httpContext = httpContextAccessor.HttpContext;
			if (httpContext != null)
			{
				string jwt = httpContext.Request.Headers["Authorization"];

				if (!string.IsNullOrWhiteSpace(jwt) && jwt != "null")
				{
					string userNtId = await TokenAuthenticationSchemeHandler.GetNtidIfTokenIsValid(jwt, logger);
					if (!string.IsNullOrWhiteSpace(userNtId))
					{
						ISecurityInformation securityInformation = httpContext.RequestServices.GetService<ISecurityInformation>();
						return securityInformation.IsIESPortalAdminUser(userNtId);
					}
				}
			}

			return false;
		}
	}
}
