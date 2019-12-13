// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2017 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web.Controllers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Mvc;
    using IES.ActionLogic.ModelView;
    using IES.Common;
    using RDM.Web.Common;

    /// <summary>
    /// Controller for the Versions.
    /// </summary>
    public class PPRDExportController : RDMController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inAdUtils">Active Directory Utilities</param>
        /// <param name="inSecurityInformation">Security information</param>
        public PPRDExportController(IActiveDirectoryUtilities inAdUtils, ISecurityInformation inSecurityInformation)
            : base(inAdUtils, inSecurityInformation)
        {
        }

        /// <summary>
        /// Index for Versions.
        /// </summary>
        /// <returns>Index page for Versions.</returns>
        public ActionResult Index()
        {
            PPRDExportModelview modelView = new PPRDExportModelview();

            // TODO - hook up real DataLoader for versions
            modelView.AvailableVersions = this.GetAvailableVersions();

            modelView.SelectedVersion = modelView.AvailableVersions.FirstOrDefault(x => x.WIP).VersionNumber;

            return this.View(modelView);
        }

        /// <summary>
        /// Gets available Versions for dropdown
        /// </summary>
        /// <returns>Collection of Versions for dropdown</returns>
        private ICollection<VersionSelectModelView> GetAvailableVersions()
        {
            return new Collection<VersionSelectModelView>()
            {
                new VersionSelectModelView() { VersionNumber = 164, WIP = true },
                new VersionSelectModelView() { VersionNumber = 163 },
                new VersionSelectModelView() { VersionNumber = 162 },
                new VersionSelectModelView() { VersionNumber = 161 },
                new VersionSelectModelView() { VersionNumber = 160 },
                new VersionSelectModelView() { VersionNumber = 159 }
            };
        }
    }
}
