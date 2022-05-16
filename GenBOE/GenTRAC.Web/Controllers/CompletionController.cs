// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.ControllerLogic;
    using GenTRAC.Web.Common;
    using IES.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Mvc;

    /// <summary>
    /// Controler to support/control proposal completion operations
    /// </summary>
    public class CompletionController : GenTRACController
    {
        private readonly CompletionControllerLogic completionLogic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityInformation"></param>
        /// <param name="inGenTRACControllerLogic"></param>
        /// <param name="inSiteMasterUtilities"></param>
        public CompletionController(CompletionControllerLogic inLogic, ISecurityInformation inSecurityInformation, GenTRACControllerLogic inGenTRACControllerLogic, SiteMasterUtilities inSiteMasterUtilities)
            : base(inSecurityInformation, inGenTRACControllerLogic, inSiteMasterUtilities)
        {
            this.completionLogic = inLogic;
        }

        /// <summary>
        /// Set the proposal to complete and sends notification email(s)
        /// </summary>
        /// <param name="proposalId">proposal Id to update</param>
        /// <returns>JSON encoded response object</returns>
        public async Task<JsonResult> SetCompleteStatus(int proposalId)
        {
            IESResponse<bool> result = new IESResponse<bool>();
            List<string> messages = new List<string>();
                        
            //put the proposal into "Complete" status
            bool success = await this.SetProposalComplete(proposalId, messages);
            result.Data.Add(success);
            result.IsSuccessful = success;
            result.Messages.AddRange(messages);

            return Json(result);
        }

        /// <summary>
        /// Sets the status to proposal complete and sends notification email(s)
        /// </summary>
        /// <param name="proposalId">Proposal to be updated</param>
        /// <returns>Response object</returns>
        private async Task<bool> SetProposalComplete(int proposalId, List<string> errMessages)
        {
            bool response = false;

            await this.completionLogic.SetProposalStatusComplete(proposalId, errMessages);
            
            if (!errMessages.Any())
            {
                response = true;
            }

            return response;
        }
    }
}