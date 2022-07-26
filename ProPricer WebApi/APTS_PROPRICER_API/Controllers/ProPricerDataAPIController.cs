/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{
    using System;
    using System.Web.Http;
    using APTSPropricerApi.Common;

    /// <summary>
    /// The Pro Pricer Data API Controller for ACV to call
    /// </summary>
    [AllowAnonymous]
    public class ProPricerDataApiController : ApiController
    {
        /// <summary>
        /// The logger for the controller
        /// </summary>
        private readonly Logger logger = new Logger(typeof(ProPricerDataApiController));

        /// <summary>
        /// Token handler 
        /// </summary>
        private readonly TokenHandling tokenHandler = new TokenHandling();

        /// <summary>
        /// Test Method for ACV Tokens
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Test()
        {
            try
            {
                tokenHandler.AuthenticateUserFromAuthorizationToken();

                return "Hello World";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return "failed";
        }
    }
}