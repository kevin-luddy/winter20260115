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

namespace APTSPropricerApi.Controllers
{
    public class UserController : ProPricerController
    {
        // GET api/user
        /// <summary>
        /// Returns the list of users in the instance of PROPRICER.
        /// </summary>
        /// <returns>Returns a collection of user information from the instance of PROPRICER</returns>
        public IEnumerable<UserDto> Get(int instanceId)
        {
            List<UserDto> ul = new List<UserDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    ppc.Workspace.Users.Open();
                    foreach (User user in ppc.Workspace.Users)
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

                    ppc.Workspace.Users.Close();
                }
            }

            return ul;
        }

        // GET api/user/5
        /// <summary>
        /// Returns the role information for a given user.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The EntityId of the user in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
        /// <returns>
        /// Returns the role information for a given user.
        /// </returns>
        public RolesDto Get(int instanceId, string id)
        {
            RolesDto rldto = new RolesDto();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(id));
                    User user = ppc.Workspace.Users.Find(pEntityId).Value;
                    //  var u2 = ppc.workspace.Users.FindName("Mcbride, Mike");
                    ppc.Workspace.Roles.Open();
                    foreach (Role role in ppc.Workspace.Roles)
                    {
                        if (user.Role.Id == role.Id) // should only be one but I will just let it loop 
                        {
                            rldto.Id = role.Id.ToString();
                            rldto.Name = role.Name;
                            rldto.Description = role.Description;
                            rldto.ProposalAccess = role.ProposalAccess.ToString();
                        }
                    }

                    ppc.Workspace.Roles.Close();
                }
            }
            return rldto;
        }
    }
}