// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;
    using IES.Common;

    /// <summary>
    /// eEPP Data Controller
    /// 
    /// This is going to be a REST controller that the eEPP application will use to pull proposal data
    /// </summary>
    public class EeppDataController : ApiController
    {
        /// <summary>
        /// Security Information
        /// </summary>
        ISecurityInformation security;

        /// <summary>
        /// Ctor
        /// </summary>
        public EeppDataController(ISecurityInformation security)
        {
            this.security = security;
        }

        /// <summary>
        /// Get Proposals for eEPP
        /// </summary>
        /// <returns>Proposal Data</returns>
        public List<string> GetProposalData()
        {
            List<string> result = new List<string>();

            result.Add(this.security.ActiveUserNTID);

            return result;
        }
    }
}
