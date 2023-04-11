/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{
	using System.Collections.Generic;
	using System.Linq;
	using APTSPropricerApi.Connection;
	using APTSPropricerApi.DTOs;
	using EBS.ProPricer.Model;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	/// <summary>
	/// Controller for SysCds
	/// </summary>
	public class SysCdsController : ProPricerController
	{
		/// <summary>
		/// Pool Manager
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// #ctor
		/// </summary>
		public SysCdsController(ILogger<SysCdsController> logger, PoolManagerList poolManagerList) : base(logger)
		{
			this.poolManagerList = poolManagerList;
		}

		// GET api/syscds
		/// <summary>
		/// Returns the list of SysCds in the instance of PROPRICER.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <returns>
		/// Returns a collection of SysCds from the instance of PROPRICER.
		/// </returns>
		[HttpGet]
		[Authorize]
		[Route("{instanceId}")]
		public IEnumerable<SysCdsDto> Get(int instanceId)
		{
			List<SysCdsDto> sysCds = new();
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				if (ppc.Workspace != null)
				{
					ppc.Workspace.Open();
					ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Open();

					foreach (TitleTable rsd in ppc.Workspace.GlobalLibrary.TitleTables.Items())
					{
						if (rsd.Name == "F35 System Code")
						{
							rsd.Open();
							foreach (Title sysCd in rsd.Elements.Items())
							{
								SysCdsDto sysCdDto = new()
								{
									Name = sysCd.Name,
									Description = sysCd.Description
								};
								sysCds.Add(sysCdDto);
							}

							rsd.Close();
						}
					}

					ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Close();
					ppc.Workspace.Close();
				}
			}

			IEnumerable<SysCdsDto> ordered = sysCds.OrderBy(sysCdList => sysCdList.Name);

			return ordered;
		}
	}
}