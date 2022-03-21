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
        /// 
        /// </summary>
        /// <param name="proposalId"></param>
        /// <returns></returns>
        public async Task<JsonResult> SetCompleteStatus(int proposalId)
        {
            IESResponse<bool> result = new IESResponse<bool>();
            List<string> messages = new List<string>();
                        
            //put the proposal into "Complete" status
            bool success = await this.SetProposalComplete(proposalId, messages);
            result.Data.Add(success);
            result.IsSuccessful = success;
            result.Messages.AddRange(messages);

            //record the date the way we used to(IES - 855 removed the code)
            //contracts tab goes read only(this will need to be implemented)
            //Certification tab remains editable, but disable "Reset Certification Not Required" button.
            //send out an email:
            //  Recipients: Est Lead and Backup EL.
            //  Subject: [PTM Entry Number] Notification
            //  Body: [PTM Entry Number] was completed and placed on contract. [Link to specific PTM entry]

            return Json(result);
        }

        /// <summary>
        /// Sets the status to proposal lost and sends notification email(s)
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