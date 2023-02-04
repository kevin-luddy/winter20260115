/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{
    using System.Linq;
    using APTSPropricerApi.Connection;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	/// <summary>
	/// Health Check Controller for Pro Pricer API
	/// </summary>
	[AllowAnonymous]
    // [APTSPropricerApi.HandleError]
	[ApiController]
	public class HealthCheckController : ControllerBase
	{
        /// <summary>
        /// Pool Manager
        /// </summary>
        private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// #ctor
		/// </summary>
		public HealthCheckController(PoolManagerList poolManagerList)
		{
			this.poolManagerList = poolManagerList;
		}

		/// <summary>
		/// Is Service Alive?
		/// </summary>
		/// <returns>True/false</returns>
		[HttpGet]
		[Route("api/HealthCheck/IsAlive")]
		public bool IsAlive()
        {
            bool result = false;

            try
            {
                // Check Pro pricer connection pool to see if there are any and that all are active
                if (poolManagerList.Instances != null && poolManagerList.Instances.Count > 0 &&
					poolManagerList.Instances.All(m => m.CurrentObjectsInPool > 0))
                {
                    result = true;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}