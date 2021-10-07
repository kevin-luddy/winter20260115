// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Web.Http;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// PTM Data Controller, original intent is for it to be used by ACV to pull data in, but realistically, it is serving up PTM data, hence the name.
    /// </summary>
    [AllowAnonymous]
    public class PtmDataAPIController : ApiController
    {
        #region Properties & Ctor

        /// <summary>
        /// Security Information
        /// </summary>
        private ISecurityInformation security;

        /// <summary>
        /// Security Access
        /// </summary>
        private ISecurityAccess securityAccess;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private IProposalLoader loader;

        /// <summary>
        /// Token Handling
        /// </summary>
        private TokenHandling tokenHandler;

        /// <summary>
        /// Ctor
        /// </summary>
        public PtmDataAPIController(ISecurityInformation security, IProposalLoader loader, ISecurityAccess securityAccess, TokenHandling tokenHandler)
        {
            this.security = security;
            this.loader = loader;
            this.securityAccess = securityAccess;
            this.tokenHandler = tokenHandler;
        }

        #endregion

        /// <summary>
        /// Get Proposal data for ACV. Limits the number of records returned to 100.
        /// </summary>
        /// <param name="token">Token that will allow access</param>
        /// <param name="searchString">Search String</param>
        /// <returns>Proposal Data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpGet]
        public ICollection<AcvProposalData> GetProposalDataForCostVolume(string token, string searchString)
        {
            List<AcvProposalData> result;

            try
            {
                AuthenticateUser(token);

                // Normal logic resumes
                bool isAdmin = this.securityAccess.CurrentUserHasRole(PtmRole.Admin, null);
                ICollection<(string PtmTrackingNumber, string ProposalTitle)> data = this.loader.GetCostVolumeProposalData(security.ActiveUserNTID, isAdmin, searchString);
                result = data.Select(x => new AcvProposalData() { PtmTrackingNumber = x.PtmTrackingNumber, ProposalTitle = x.ProposalTitle }).ToList();
            }
            catch
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Authenticates the user based on the token that is coming in. 
        ///     The token is first validated, and if it is valid, then the user's NTID will be retrieved from it. 
        ///     Finally, the NTID will be set into the System's Current Principal
        /// </summary>
        /// <param name="token">Incoming token</param>
        private void AuthenticateUser(string token)
        {
            // Authenticate the call, and pull out the user's ntid.
            string ntid = tokenHandler.GetNtidIfTokenIsValid(token);

            if(string.IsNullOrEmpty(ntid))
            {
                throw new UnauthorizedAccessException();
            }

            // Set the current user to the NTID that is coming in.
            GenericIdentity identity = new GenericIdentity(ntid);
            System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
        }
    }
}