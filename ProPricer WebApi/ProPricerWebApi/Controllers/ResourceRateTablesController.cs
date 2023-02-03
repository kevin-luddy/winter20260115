/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System;
using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.ProPricer.Model;
using Microsoft.AspNetCore.Mvc;

namespace APTSPropricerApi.Controllers
{
    /// <summary>
    /// The Resource Rate Tables Controller.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class ResourceRateTablesController : ProPricerController
    {
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public ResourceRateTablesController(ILogger<ResourceRateTablesController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/resourceratetables
        /// <summary>
        /// Returns the list of resource rate tables from the PROPRICER Global Library.
        /// </summary>
        /// <returns>A collection of resource rate tables.</returns>
        [HttpGet]
        [Route("{instanceId}")]
        public IEnumerable<ResourceRateTableDto> Get(int instanceId)
        {
            using (IProPricerConnection ppc = (Connection.IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                ppc.Workspace.GlobalLibrary.ResourceRateTables.Open();
                List<ResourceRateTableDto> resourceRateTablesResult = new List<ResourceRateTableDto>();
                if (ppc.Workspace != null)
                {
                    try
                    {
                        DirectRateTableCollection dircol = ppc.Workspace.GlobalLibrary.ResourceRateTables.DirectRateTables;

                        foreach (DirectRateTable rattbl in dircol.Items())
                        {
                            ResourceRateTableDto rttdto = new ResourceRateTableDto
                            {
                                Id = rattbl.Id.ToString(),
                                Name = rattbl.Name,
                                Description = rattbl.Description,
                                IsCurrent = rattbl.IsCurrent,
                                BurdenRateTable = rattbl.BurdenRateTable.Name
                            };
                            resourceRateTablesResult.Add(rttdto);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Error retrieving Resource Rate Tables");
                        System.Diagnostics.Debug.WriteLine("Error - " + ex.Message);
                    }
                }

                ppc.Workspace.GlobalLibrary.ResourceRateTables.Close();
                return resourceRateTablesResult;
            }
        }
    }
}