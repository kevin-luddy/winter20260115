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
    /// <summary>
    /// Spread curves specify how to divide the number of hours/units/cost spread for a resource within a task across time.
    /// </summary>
    public class CurvesController : ProPricerController
    {
        /// <summary>
        /// Pool Manager
        /// </summary>
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public CurvesController(ILogger<CurvesController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/curves
        /// <summary>
        /// Return the list of available curves from the global library.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// A collection of curve information available from the global library.
        /// </returns>
        [HttpGet]
        [Route("{instanceId}")]
        public IEnumerable<CurvesDto> Get(int instanceId)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                ppc.Workspace.GlobalLibrary.Curves.Open();
                IEnumerable<CurvesDto> curvesResult =
                    from curve in ppc.Workspace.GlobalLibrary.Curves.Items().Cast<Curve>()
                    select new CurvesDto
                    {
                        Id = curve.Id.ToString(),
                        Name = curve.Name,
                        Description = curve.Description,
                        Type = curve.Type.ToString()
                    };
                ppc.Workspace.GlobalLibrary.Curves.Close();
                return curvesResult;
            }
        }
    }
}