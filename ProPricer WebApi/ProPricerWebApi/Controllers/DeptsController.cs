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
	/// Controller for departments
	/// </summary>
	public class DeptsController : ProPricerController
	{
		/// <summary>
		/// Pool Manager
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// #ctor
		/// </summary>
		public DeptsController(ILogger<DeptsController> logger, PoolManagerList poolManagerList) : base(logger)
		{
			this.poolManagerList = poolManagerList;
		}

		// GET api/Depts
		/// <summary>
		/// Returns the list of Depts in the instance of PROPRICER.
		/// </summary>
		/// <returns>Returns a collection of Depts from the instance of PROPRICER.</returns>
		[HttpGet]
		[Authorize]
		[Route("{instanceId}")]
		public IEnumerable<DeptsDto> Get(int instanceId)
		{
			List<DeptsDto> depts = new();
			using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
			{
				if (ppc.Workspace != null)
				{
					ppc.Workspace.Open();
					ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Open();

					foreach (ResourceFieldDefinition rsd in ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Items())
					{
						if (rsd.Name == "DEPT")
						{
							foreach (ResourceFieldStandardValue dept in rsd.ValueList.Items())
							{
								DeptsDto deptdto = new()
								{
									Value = dept.Value.ToString(),
									Description = dept.Description
								};
								depts.Add(deptdto);
							}
						}
					}

					ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Close();
					ppc.Workspace.Close();
				}
			}
			IEnumerable<DeptsDto> ordered = depts.OrderBy(deptlist => deptlist.Value);

			return ordered;
		}
	}
}