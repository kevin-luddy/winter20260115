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

namespace APTSPropricerApi.Controllers
{
    public class ResourcesController : ProPricerController
    {
        // GET api/resources
        /// <summary>
        /// Returns the list of resources in the instance of PROPRICER.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// Returns a collection of resource information from the instance of PROPRICER.
        /// </returns>
        public IEnumerable<ResourcesDto> Get(int instanceId)
        {
            List<ResourcesDto> res = new List<ResourcesDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    ppc.Workspace.GlobalLibrary.Resources.Open();
                    foreach (Resource resrc in ppc.Workspace.GlobalLibrary.Resources.Items())
                    {
                        ResourcesDto resdto = new ResourcesDto
                        {
                            Id = resrc.Id.ToString(),
                            Name = resrc.Name,
                            Description = resrc.Description,
                            Type = resrc.Type.ToString(),
                            Rclass = resrc.ResourceClass != null ? resrc.ResourceClass.Name : string.Empty,
                            AccountingCalendar = resrc.Calendar != null ? resrc.Calendar.ToString() : string.Empty
                        };
                        res.Add(resdto);
                    }

                    ppc.Workspace.GlobalLibrary.Resources.Close();
                }
            }

            return res;
        }
    }
}