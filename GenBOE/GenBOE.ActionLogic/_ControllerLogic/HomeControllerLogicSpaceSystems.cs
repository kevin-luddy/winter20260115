// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;

    public class HomeControllerLogicSpaceSystems : HomeControllerLogic
    {
        public HomeControllerLogicSpaceSystems(IActiveDirectoryUtilities activeDirectoryUtils) 
            : base(activeDirectoryUtils)
        {
            //nothing to do here
        }

        /// <summary>
        /// Populates the company specific properties of the <see cref="GenBOEMasterMenuItemModelView"/> for the Metric Admin menu item 
        /// </summary>
        /// <param name="theModelView">the <see cref="GenBOEMasterMenuItemModelView"/> to populate</param>
        public override void PopulateMetricsAdminCompanySpecificProperties(GenBOEMasterMenuItemModelView theModelView)
        {
            if (theModelView != null)
            {
                // Hide the Menu link for SSC
                theModelView.htmlAttributes = new { @class = "display-none" };
            }
        }
    }
}
