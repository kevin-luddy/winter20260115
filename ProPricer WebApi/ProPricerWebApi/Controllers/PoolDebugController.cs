/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using Microsoft.AspNetCore.Mvc;

	/// <summary>
	/// Pool Debug Controller
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class PoolDebugController : ControllerBase
	{
		/// <summary>
		/// Pool Manager
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// #ctor
		/// </summary>
		public PoolDebugController(PoolManagerList poolManagerList)
		{
			this.poolManagerList = poolManagerList;
		}

		// use PoolDebug.html
		/// <summary>
		/// Returns the list of pools and if they are in use.
		/// </summary>
		/// /// <returns>Returns a collection of pools.</returns>
		[Route("{instanceId}")]
		[HttpGet]
		public IEnumerable<PoolDebugDto> Get(int instanceId)
		{
			List<PoolDebugDto> poollist = new();
			foreach (PoolManager poolManager in poolManagerList.Instances)
			{
				int maxpool = poolManager.CurrentObjectsInPool;

				for (int i = 0; i < maxpool; i++)
				{
					PoolDebugDto pool = new()
					{
						InstanceId = poolManager.InstanceId.ToString(),
						PoolMaxNum = maxpool.ToString(),
						PoolNum = (i + 1).ToString(),
						PoolInUse = poolManager.IsPoolInUse(i).ToString()
					};
					poollist.Add(pool);
				}
			}

			return poollist;
		}
	}
}