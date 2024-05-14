// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;

    /// <summary>
    /// MST specific Logic Controller
    /// Since this class inherits from the base (IS&amp;GS) implementation when we want to use another derived class we can repeat the code (only if a line or two)
    /// If the exact implementation is needed from another derived controller logic class please use the decorator pattern so code isn't repeated.
    /// </summary>
    public class HomeControllerLogicMST : HomeControllerLogic
    {
        public HomeControllerLogicMST(IActiveDirectoryUtilities activeDirectoryUtils)
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
                // MST uses metrics from external PMM site
                theModelView.linkUrl = SafeUriUtility.safeUri(ConfigurationUtilities.GetAppSetting("MstMetricsUrl"));
                theModelView.securityPage = SecurityPage.SystemAdmin;
            }
        }
    }
}
