// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Logging
{
	using IES.Common.Core.Interfaces;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;
	using Serilog.Ui.Web.Authorization;

	public class CustomAuthorizeFilter : IUiAuthorizationFilter
	{
		/// <summary>
		/// Authorizes only Admins for this custom filter
		/// </summary>
		/// <param name="httpContext">The http context</param>
		/// <returns>True if Admin; otherwise false.</returns>
		public bool Authorize(HttpContext httpContext)
		{
			ISecurityInformation securityInformation = httpContext.RequestServices.GetService<ISecurityInformation>();
			string userNtId = securityInformation.ActiveUserNTID;
			if (!string.IsNullOrWhiteSpace(userNtId))
			{
				return securityInformation.IsIESPortalAdminUser(securityInformation.ActiveUserNTID);
			}

			return false;
		}
	}
}
