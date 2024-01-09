using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Ui.Web.Authorization;

namespace IES.Core
{
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
