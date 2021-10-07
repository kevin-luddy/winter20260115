// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Web.Http;
    using GenBOE.DataBridge.DTO;
    using IES.Common;
    using GenBOE.Web.ModelView;

    /// <summary>
    /// BOE Data Controller, original intent is for it to be used by ACV to pull data in, but realistically, it is serving up BOE data, hence the name.
    /// </summary>
    [AllowAnonymous]
    public class BoeDataAPIController : ApiController
    {
        #region Properties & Ctor

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private IWorkspaceDTODataLoader loader;

        /// <summary>
        /// Token Handling
        /// </summary>
        private TokenHandling tokenHandler;

        /// <summary>
        /// Ctor
        /// </summary>
        public BoeDataAPIController() { }

        /// <summary>
        /// Ctor
        /// </summary>
        public BoeDataAPIController(IWorkspaceDTODataLoader loader, TokenHandling tokenHandler)
        {
            this.loader = loader;
            this.tokenHandler = tokenHandler;
        }

        #endregion

        /// <summary>
        /// Get Proposal data for ACV. Limits the number of records returned to 100.
        /// </summary>
        /// <param name="token">Token that will allow access</param>
        /// <param name="ptmTrackingNumber">PTM Tracking Number</param>
        /// <returns>Proposal Data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpGet]
        public ICollection<AcvWorkspaceData> GetWorkspaceDataForProposal(string token, string ptmTrackingNumber)
        {
            List<AcvWorkspaceData> result;

            try
            {
                AuthenticateUser(token);

                // Normal logic resumes
                ICollection<(int Id, string shortName, string longName)> data = this.loader.GetWorkspaceDataForProposal(ptmTrackingNumber);
                result = data.Select(x => new AcvWorkspaceData() { Id = x.Id, ShortName = x.shortName, LongName = x.longName }).ToList();
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