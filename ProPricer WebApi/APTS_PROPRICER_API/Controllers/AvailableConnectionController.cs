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
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using APTSPropricerApi.Common;
    using APTSPropricerApi.DTOs;

    /// <summary>
    /// The Pro Pricer Data API Controller for ACV to call
    /// </summary>
    [AllowAnonymous]
    public class AvailableConnectionController : ApiController
    {
        /// <summary>
        /// The logger for the controller
        /// </summary>
        private readonly Logger logger = new Logger(typeof(AvailableConnectionController));

        /// <summary>
        /// Token handler 
        /// </summary>
        private readonly TokenHandling tokenHandler = new TokenHandling();

        /// <summary>
        /// Gets the Available Connections.
        /// </summary>
        /// <returns>Returns a collection of the pool manager instances.</returns>
        [HttpGet]
        public ProPricerResponse<ICollection<PoolInstanceDto>> Get()
        {
            ProPricerResponse<ICollection<PoolInstanceDto>> response = new ProPricerResponse<ICollection<PoolInstanceDto>>();

            try
            {
                tokenHandler.AuthenticateUserFromAuthorizationToken();

                using (PoolInstanceController pc = new PoolInstanceController())
                {
                    response.Data = pc.Get().ToList();
                    response.IsSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                response.Messages.Add("Error retrieving Pool Instances");
            }
            
            return response;
        }
    }
}