namespace APTSPropricerApi.Common
{
    using ACV.Common;
    using ACV.Shared;
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
