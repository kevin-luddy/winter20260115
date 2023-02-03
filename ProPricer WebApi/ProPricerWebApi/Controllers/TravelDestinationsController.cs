/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.ProPricer.Model;
using Microsoft.AspNetCore.Mvc;

namespace APTSPropricerApi.Controllers
{
    public class TravelDestinationsController : ProPricerController
    {
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public TravelDestinationsController(ILogger<TravelDestinationsController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/traveldestinations
        /// <summary>
        /// Returns the list of Travel Destinations from the Global Library in PROPRICER.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// A collection of travel destinations.
        /// </returns>
        [HttpGet]
        [Route("{instanceId}")]
        public IEnumerable<TravelDestinationsDto> Get(int instanceId)
        {
            List<TravelDestinationsDto> trv = new List<TravelDestinationsDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    ppc.Workspace.GlobalLibrary.Travels.Open();
                    foreach (Travel trvdes in ppc.Workspace.GlobalLibrary.Travels.Items())
                    {
                        TravelDestinationsDto trvdto = new TravelDestinationsDto
                        {
                            Id = trvdes.Id.ToString(),
                            Name = trvdes.Name,
                            Description = trvdes.Description
                        };
                        trv.Add(trvdto);
                    }

                    ppc.Workspace.GlobalLibrary.Travels.Close();
                }
            }

            return trv;
        }
    }
}