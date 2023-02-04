/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.Core;
using EBS.ProPricer.Data;
using EBS.ProPricer.Model;
using Microsoft.AspNetCore.Mvc;

namespace APTSPropricerApi.Controllers
{
    /// <summary>
    /// Utility to copy an entire proposal 
    /// </summary>
    public class ProposalCopyController : ProPricerController
    {
        /// <summary>
        /// Pool Manager
        /// </summary>
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public ProposalCopyController(ILogger<ProposalCopyController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // Post api/proposalcopy
        /// <summary>
        /// Creates a new proposal from an existing one.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="newProp">Proposal object that must contain the following:
        /// "id" - The entity id of the proposal to be copied from.
        /// Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0
        /// "name" - The name of the new proposal.
        /// "version" - The version number of the new proposal.
        /// "description" - The description of the new proposal.
        /// 'folder" - The folder where the new proposal will be stored.
        /// All other fields will be ignored.</param>
        /// <returns>
        /// Returns the id of the newly created proposal if successful. If not, returns an error message.
        /// </returns>
        [HttpPost]
        [Route("{instanceId}")]
        public ReturnDto Post(int instanceId, [FromBody] ProposalDto newProp)
        {
            if (newProp?.Name == null || newProp.Name.Trim() == string.Empty)
            {
                ReturnDto retdto = new()
                {
                    Retcode = "500",
                    Retmsg = "New Proposal name cannot be blank"
                };
                return retdto;
            }

            if (newProp.Version.Trim() == string.Empty)
            {
                newProp.Version = "0";
            }

            string whichvar = "finding template proposal";
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                try
                {
                    Proposal pr;

                    if (newProp.Id.Contains('|'))
                    {
                        // Name
                        string[] parts = newProp.Id.Split('|');
                        pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value();
                    }
                    else
                    {
                        // GUID
                        EntityId pEntityId = new(new Guid(newProp.Id));
                        pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
                    }

                    pr.Open();

                    whichvar = "to folder";

                    Folder tofolder = ppc.Workspace.GlobalLibrary.Folders.Find(newProp.ParentFolderName, FolderCategory.Proposal, null).Value();

                    whichvar = "Copy command";
                    pr.Copy(newProp.Name, newProp.Version, newProp.Description, tofolder, false);

                    pr.Close();

                    whichvar = "new proposal id";
                    Proposal newpr = ppc.Workspace.Proposals.Find(newProp.Name, newProp.Version).Value();

                    ReturnDto retdto = new()
                    {
                        Retcode = "200",
                        Retmsg = newpr.Id.ToString()
                    };
                    return retdto;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error with " + whichvar);
                    System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.Message);

                    ReturnDto retdto = new()
                    {
                        Retcode = "500",
                        Retmsg = "Error with " + whichvar + " - " + ex.Message
                    };
                    return retdto;
                }
            }
        }
    }
}