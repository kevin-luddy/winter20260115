// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Http;
	using Serilog.Context;

	/// <summary>
	/// Correlation middleware
	/// </summary>
	public class CorrelationMiddleware
	{
		/// <summary>
		/// Request delegate
		/// </summary>
		private readonly RequestDelegate next;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="next">Request delegate</param>
		public CorrelationMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		/// <summary>
		/// InvokeAsync
		/// </summary>
		/// <param name="context">HttpContext</param>
		public async Task InvokeAsync(HttpContext context)
		{
			string correlationId = context.GetCorrelationId();

			if (string.IsNullOrEmpty(correlationId))
			{
				correlationId = Guid.NewGuid().ToString();
				context.Request.Headers.Add(Constants.CORRELATION_HEADER_NAME, correlationId);
			}

			LogContext.PushProperty(Constants.CORRELATION_HEADER_NAME, correlationId);
			await next(context);
		}
	}
}
