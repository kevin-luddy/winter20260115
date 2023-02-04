/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System.Collections.Generic;
using System.Linq;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.ProPricer.Model;
using Microsoft.AspNetCore.Mvc;

namespace APTSPropricerApi.Controllers
{
    public class TravelRateTablesController : ProPricerController
    {
        /// <summary>
        /// Pool Manager
        /// </summary>
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public TravelRateTablesController(ILogger<TravelRateTablesController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/travelratetables
        /// <summary>
        /// Returns the list of Travel Rate Tables from the Global Library in PROPRICER.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// A collection of travel rate table entries.
        /// </returns>
        [HttpGet]
        [Route("{instanceId}")]
        public IEnumerable<TravelRateTableDto> Get(int instanceId)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                ppc.Workspace.GlobalLibrary.TravelRateTables.Open();
                IEnumerable<TravelRateTableDto> travelRateTablesResult =
                    from prop in ppc.Workspace.GlobalLibrary.TravelRateTables.Items().Cast<TravelRateTable>()
                    select new TravelRateTableDto
                    {
                        Id = prop.Id.ToString(),
                        Name = prop.Name,
                        Description = prop.Description,
                        EnableWeeklyRentalCarRate = prop.EnableWeeklyRentalCarRate,
                        IsCurrent = prop.IsCurrent
                    };
                ppc.Workspace.GlobalLibrary.TravelRateTables.Close();
                return travelRateTablesResult;
            }
        }
    }
}