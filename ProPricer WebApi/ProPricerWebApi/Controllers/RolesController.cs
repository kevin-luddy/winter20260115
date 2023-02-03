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
using EBS.Core;
using EBS.ProPricer.Model;
using Microsoft.AspNetCore.Mvc;

namespace APTSPropricerApi.Controllers
{
    public class RolesController : ProPricerController
    {
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public RolesController(ILogger<RolesController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/roles
        /// <summary>
        /// Looks up the list of current roles in PROPRICER.
        /// </summary>
        /// <returns>Returns a collection of roles information.</returns>
        [HttpGet]
        [Route("{instanceId}")]
        public IEnumerable<RolesDto> Get(int instanceId)
        {
            List<RolesDto> rl = new List<RolesDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    ppc.Workspace.Roles.Open();
                    foreach (Role role in ppc.Workspace.Roles.Items())
                    {
                        RolesDto rldto = new RolesDto
                        {
                            Id = role.Id.ToString(),
                            Name = role.Name,
                            Description = role.Description,
                            ProposalAccess = role.ProposalAccess.ToString()
                        };
                        rl.Add(rldto);
                    }

                    ppc.Workspace.Roles.Close();
                }
            }

            return rl;
        }

        // GET api/roles/GUID
        /// <summary>
        /// Returns user information for users who are members in the specified role.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The EntityId of the role in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
        /// <returns>
        /// Returns a collection of user information who are members in the specified role.
        /// </returns>
        [HttpGet]
        [Route("{instanceId}/{id}")]
        public IEnumerable<UserDto> Get(int instanceId, string id)
        {
            List<UserDto> ul = new List<UserDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(id));
                    Role role = ppc.Workspace.Roles.Find(pEntityId).Value();
                    ppc.Workspace.Users.Open();
                    foreach (User user in ppc.Workspace.Users.Items())
                    {
                        if (user.Role.Id == role.Id)
                        {
                            UserDto uldto = new UserDto
                            {
                                Id = user.Id.ToString(),
                                LoginType = user.LoginType.ToString(),
                                LoginName = user.LoginName,
                                Name = user.Name,
                                Description = user.Description,
                                Role = user.Role.Name,
                                Logins = user.Logins,
                                LastLogin = user.LastLogin.ToString()
                            };

                            ul.Add(uldto);
                        }
                    }

                    ppc.Workspace.Users.Close();
                }
            }

            return ul;
        }
    }
}