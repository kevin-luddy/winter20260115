// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Http;
	using Serilog.Context;

	/// <summary>
	/// User logging middleware
	/// </summary>
	public class UserLoggingMiddleware
	{
		/// <summary>
		/// RequestDelegate
		/// </summary>
		private readonly RequestDelegate next;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="next">RequestDelegate</param>
		public UserLoggingMiddleware(RequestDelegate next)
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
