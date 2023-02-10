/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Common
{
	using ACV.Common;
	using Serilog.Context;

	/// <summary>
	/// Middleware to add User name from Negotiate and IES_Authorization Authentication Schemes
	/// </summary>
	public class ProPricerUserLoggingMiddleware
	{
		/// <summary>
		/// RequestDelegate
		/// </summary>
		private readonly RequestDelegate next;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="next">RequestDelegate</param>
		public ProPricerUserLoggingMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		/// <summary>
		/// Invoke
		/// </summary>
		/// <param name="context">HttpContext</param>
		public async Task Invoke(HttpContext context)
		{
			LogContext.PushProperty(LoggerConstants.UserNTIDProperty, GetNTID(context));
			await next.Invoke(context);
		}

		/// <summary>
		/// Gets the NTID of the user
		/// </summary>
		/// <param name="context">HttpContext</param>
		private static string GetNTID(HttpContext context)
		{
			string ntid = null;

			if (context.User != null && context.User.Identity.IsAuthenticated)
			{
				ntid = context.User.Identity.Name;
			}

			return ntid;
		}
	}
}
