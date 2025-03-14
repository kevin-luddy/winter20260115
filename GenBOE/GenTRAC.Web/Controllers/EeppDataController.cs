// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
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
        private ISecurityInformation securityInformation;

        /// <summary>
        /// Security Access
        /// </summary>
        private ISecurityAccess securityAccess;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private IProposalLoader loader;

        /// <summary>
        /// Ctor
        /// </summary>
        public EeppDataController(ISecurityInformation security, IProposalLoader loader, ISecurityAccess securityAccess)
        {
            this.securityInformation = security;
            this.loader = loader;
            this.securityAccess = securityAccess;
        }

        /// <summary>
        /// Get Proposals for eEPP
        /// </summary>
        /// <param name="searchString">Search String</param>
        /// <returns>Proposal Data</returns>
        public ICollection<EppProposalData> GetProposalData(string searchString)
        {
            bool isAdmin = this.securityAccess.CurrentUserHasRole(PtmRole.Admin, null);

            ICollection<EppProposalData> result = this.loader.GetEppProposalData(this.securityInformation.ActiveUserNTID, isAdmin, searchString);

            return result;
        }

        /// <summary>
        /// Get Single Proposal for eEPP
        /// 
        /// Returns null if the proposal doesn't exist, or the user isn't allowed to access it
        /// </summary>
        /// <param name="trackingNumber">Proposal Tracking Number</param>
        /// <returns>Proposal Data</returns>
        public EppProposalData GetProposalDataByProposal(string trackingNumber)
        {
            bool isAdmin = this.securityAccess.CurrentUserHasRole(PtmRole.Admin, null);

            EppProposalData result = this.loader.GetEppProposalDataByProposalTrackingNumber(this.securityInformation.ActiveUserNTID, isAdmin, trackingNumber);

            return result;
        }

		/// <summary>
		/// Get 
		/// </summary>
    }
}
