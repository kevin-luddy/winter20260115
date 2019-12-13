/*
    Copyright 2016-2018 Lockheed Martin Corporation.

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
using EBS.ProPricer.Model.General;

namespace APTSPropricerApi.Controllers
{
    public class ResourceFieldDefinitionsController : ProPricerController
    {
        // GET api/ResourceFieldDefinitions
        /// <summary>
        /// Returns the list of resources summary definitions in the instance of PROPRICER.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// Returns a collection of resources summary definitions from the instance of PROPRICER.
        /// </returns>
        public IEnumerable<ResourceFieldDefinitionsDto> Get(int instanceId)
        {
            List<ResourceFieldDefinitionsDto> resdfl = new List<ResourceFieldDefinitionsDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    ppc.Workspace.Open();
                    ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Open();

                    foreach (ResourceFieldDefinition rsd in ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions)
                    {
                        ResourceFieldDefinitionsDto rsddto = new ResourceFieldDefinitionsDto
                        {
                            Id = rsd.Id.ToString(),
                            Name = rsd.Name,
                            DataType = rsd.DataType.ToString()
                        };
                        resdfl.Add(rsddto);
                    }

                    ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Close();
                    ppc.Workspace.Close();
                }
            }
            return resdfl;
        }

        // GET api/ResourceFieldDefinitions/GUID
        /// <summary>
        /// Returns the list of resources summary definitions in the instance of PROPRICER.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Returns a collection of resources summary definitions from the instance of PROPRICER.
        /// </returns>
        public IEnumerable<ResourceFieldDefinitionsDto> Get(int instanceId, string id)
        {
            Proposal pr;
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (id.Contains("|"))
                {
                    // Name
                    string[] parts = id.Split('|');
                    pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value;
                }
                else
                {
                    // GUID
                    EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(id));
                    pr = ppc.Workspace.Proposals.Find(pEntityId).Value;
                }

                List<ResourceFieldDefinitionsDto> resdfl = new List<ResourceFieldDefinitionsDto>();
                if (pr != null)
                {
                    pr.Open();

                    foreach (IResourceFieldDefinition rsd in pr.ResourceFields())
                    {
                        ResourceFieldDefinitionsDto rsddto = new ResourceFieldDefinitionsDto
                        {
                            Id = rsd.Id.ToString(),
                            Name = rsd.Name,
                            DataType = rsd.DataType.ToString()
                        };
                        resdfl.Add(rsddto);
                    }

                    pr.Close();
                }

                return resdfl;
            }
        }

        // PUT api/ResourceFieldDefinitions/ResourceFieldsDto
        /// <summary>
        /// Checks the list of values for the given ResourceFieldDefinition and adds the value if it does not exists.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="rfddto">The rfddto.</param>
        public void Put(int instanceId, ResourceFieldsDto rfddto)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    //  ppc.workspace.Open();
                    //  ppc.workspace.GlobalLibrary.ResourceFieldDefinitions.Open();

                    ResourceFieldDefinition rfd = ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Find(rfddto.Key).Value;
                    if (rfd != null)
                    {
                        try
                        {
                            if (rfd.ValueList.Find(rfddto.Value))
                            {
                                ResourceFieldStandardValue rfdval = rfd.ValueList.Find(rfddto.Value).Value;
                            }
                            else
                            {
                                ResourceFieldStandardValue newRf = rfd.ValueList.AddNew();
                                newRf.Value = rfddto.Value;
                                newRf.EndEdit();
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(ex);
                        }
                    }

                    //                ppc.workspace.GlobalLibrary.ResourceFieldDefinitions.Close();
                    //   ppc.workspace.Close();
                }
            }
        }
    }
}