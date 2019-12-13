/*
    Copyright 2016-2018 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System;
using System.Collections.Generic;
using System.Web.Http;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;

namespace APTSPropricerApi.Controllers
{
    public class PoolInstanceController : ProPricerController
    {
        // GET api/poolinstance
        /// <summary>
        /// Gets the Pool Manager instances.
        /// </summary>
        /// <returns>Returns a collection of the pool manager instances.</returns>
        public IEnumerable<PoolInstanceDto> Get()
        {
            List<PoolInstanceDto> instances = new List<PoolInstanceDto>();

            try
            { 
                bool isUsingBackup = ConfigurationUtilities.GetAppSetting<bool>("UseProPricerBackup");
                foreach(PoolManager poolManager in PoolManager.Instances)
                {
                    instances.Add(new PoolInstanceDto
                    {
                        Id = poolManager.InstanceId,
                        IsBackup = isUsingBackup,
                        FriendlyName = poolManager.FriendlyName
                    });
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "GetAllProposals");
                throw new Exception("Operation failed.");
            }

            return instances;
        }
    }
}