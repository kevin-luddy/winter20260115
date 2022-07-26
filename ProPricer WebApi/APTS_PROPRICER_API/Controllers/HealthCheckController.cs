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
    using System.Web.Http;
    using APTSPropricerApi.Connection;

    /// <summary>
    /// Health Check Controller for Pro Pricer API
    /// </summary>
    [AllowAnonymous]
    [APTSPropricerApi.HandleError]
    public class HealthCheckController : ApiController
    {
        /// <summary>
		/// Is Service Alive?
		/// </summary>
		/// <returns>True/false</returns>
		[HttpGet]
        public bool IsAlive()
        {
            bool result = false;

            try
            {
                // Check Pro pricer connection pool to see if there are any and that all are active
                if (PoolManager.Instances != null && PoolManager.Instances.Count > 0 &&
                    PoolManager.Instances.All(m => m.CurrentObjectsInPool > 0))
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