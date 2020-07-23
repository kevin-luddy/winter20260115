/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Controllers
{

    using System.Web.Http;
    using APTSPropricerApi.DTOs;

    /// <summary>
    /// Method to obtain a proposal and it's tasks and resources
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    [APTSPropricerApi.HandleError]
    public class ProposalandTasksController : ApiController
    {
        // GET api/proposalandtasks/id
        /// <summary>
        /// Returns the general proposal data and tasks for a given proposal
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
        /// <returns>
        /// Returns the general proposal data and tasks for a given proposal
        /// </returns>
        public ProposalDto Get(int instanceId, string id)
        {
            ProposalDto pDto;

            using (ProposalsController pc = new ProposalsController())
            {
                pDto = pc.Get(instanceId, id);
            }

            using (TasksController tc = new TasksController())
            {
                pDto.Tasks = tc.Get(instanceId, id);
            }

            return pDto;
        }
    }
}