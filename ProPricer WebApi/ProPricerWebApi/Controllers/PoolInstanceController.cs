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
	using ACV.Shared;
	using APTSPropricerApi.Common;
	using APTSPropricerApi.Connection;
    using APTSPropricerApi.DTOs;
	using Microsoft.AspNetCore.Mvc;
	using Swashbuckle.AspNetCore.Annotations;

	/// <summary>
	/// The Pool Instance Controller
	/// </summary>
	public class PoolInstanceController : ProPricerController
    {
        /// <summary>
        /// Pool Manager
        /// </summary>
        private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// #ctor
		/// </summary>
		public PoolInstanceController(ILogger<PoolInstanceController> logger, PoolManagerList poolManagerList) : base(logger)
		{
			this.poolManagerList = poolManagerList;
		}

        // GET api/poolinstance
        /// <summary>
        /// Gets the Pool Manager instances.
        /// </summary>
        /// <returns>Returns a collection of the pool manager instances.</returns>
        [HttpGet]
		public IEnumerable<PoolInstanceDto> Get()
        {
			return Utility.GetAllPoolInstances(poolManagerList, Logger);
        }
    }
}