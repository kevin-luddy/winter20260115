// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web.Http;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Web.ModelView;
    using IES.Common;

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
            TokenHandling.AuthDomain = ConfigurationManager.AppSettings["oAuthDomain"];
        }

        #endregion

        /// <summary>
        /// Get Proposal data for ACV. Limits the number of records returned to 100.
        /// </summary>
        /// <param name="ptmTrackingNumber">PTM Tracking Number</param>
        /// <returns>Proposal Data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpGet]
        public ICollection<AcvWorkspaceData> GetWorkspaceDataForProposal(string ptmTrackingNumber)
        {
            List<AcvWorkspaceData> result;

            try
            {
                tokenHandler.AuthenticateUserFromAuthorizationToken();

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
        /// Is Service Alive?
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [HttpGet]
        public bool IsAlive()
        {
            bool result;

            try
            {
                // ToDo: add a DB grab, just to see if the DB is working.. To make the check more meaningful
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}