/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace APTSPropricerApi.Controllers
{
    public class BurdenRateTablesController : ProPricerController
    {
        /// <summary>
        /// Pool Manager
        /// </summary>
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public BurdenRateTablesController(ILogger<BurdenRateTablesController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/burdenratetables
        /// <summary>
        /// Returns the list of Burden Rate Tables from the Global Library in PROPRICER.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// A collection of burden rate table entries.
        /// </returns>
        [HttpGet]
        [Route("{instanceId}")]
        public IEnumerable<BurdenRateTableDto> Get(int instanceId)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                ppc.Workspace.GlobalLibrary.BurdenRateTables.Open();
                IEnumerable<BurdenRateTableDto> burdenRateTablesResult =
                    from prop in ppc.Workspace.GlobalLibrary.BurdenRateTables.Items()
                    select new BurdenRateTableDto
                    {
                        Id = prop.Id.ToString(),
                        Name = prop.Name,
                        Description = prop.Description
                    };
                ppc.Workspace.GlobalLibrary.BurdenRateTables.Close();
                return burdenRateTablesResult;
            }
        }
    }
}