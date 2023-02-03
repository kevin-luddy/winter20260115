/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
	using Microsoft.AspNetCore.Authentication.Negotiate;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	/// <summary>
	/// Abstract base class for Controllers connecting to Pro Pricer
	/// </summary>
	/// <seealso cref="System.Web.Http.ApiController" />
	//[APTSPropricerApi.HandleError]
	[ApiController, Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
	[Route("api/[controller]")]
    public abstract class ProPricerController : ControllerBase
    {

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; } //= new Logger(typeof(ProPricerController));

		/// <summary>
		/// #ctor
		/// </summary>
		/// <param name="logger">The logger</param>
		public ProPricerController(ILogger logger)
		{
			this.Logger = logger;
		}
    }
}