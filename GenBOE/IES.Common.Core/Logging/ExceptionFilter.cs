// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Logging
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc.Filters;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Log unhandled exceptions
	/// </summary>
	public class ExceptionFilter : IExceptionFilter
	{
		/// <summary>
		/// Logger for the class
		/// </summary>
		private ILogger logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger">The logger</param>
		public ExceptionFilter(ILogger<ExceptionFilter> logger)
		{
			this.logger = logger;
		}

		/// <summary>
		/// On Exception callback
		/// </summary>
		/// <param name="context">Exception context</param>
		public void OnException(ExceptionContext context)
		{
			this.logger.LogError(context.Exception, $"An exception occurred in {context.RouteData.Values["controller"]}.{context.RouteData.Values["action"]}().");
		}
	}
}
