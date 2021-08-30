// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// This class generates a dictionary that will be used by the controllers to check access authorization
    /// for each controller's action
    /// </summary>
    internal static class AuthorizationDictionarySetup
    {
        /// <summary>
        /// Generate a dictionary containing controllers and actions that allow read-only (viewer) access.
        /// </summary>
        /// <returns>Dictionary containing controllers and actions that allow read-only access.</returns>
        internal static Dictionary<string, List<string>> GenerateViewerDictionary()
        {
            // Notes: The RDM application is primarily a tool for Rates Administrators.  As such, most of the
            // controller actions are only available to RDM Admins.  However, there are a handful of actions
            // that are available to RDM Viewers.
            // Therefore, the RDM Viewer Authorization Dictionary will simply be a list of actions for each controller that  
            // are available to the RDM Viewers with the assumption that all other controller/actions are
            // for RDM Admins and RDM COBRA Admins only.  
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            result.Add(IESWebConstants.CONTROLLER_RATE.ToLower(), new List<string>()
            {
                "index", // remove if non-admin users aren't allowed access to page at all
                IESWebConstants.ACTION_VIEW_RATES.ToLower(),    // provides read-only menu access to manage rates
                IESWebConstants.ACTION_GET_RATES_BY_VERSION.ToLower(),
                IESWebConstants.ACTION_GET_VERSION_DIFFERENCES.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_HOME.ToLower(), new List<string>()
            {
                "index", // remove if non-admin users aren't allowed access to page at all
                IESWebConstants.ACTION_GET_REVISIONS.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_PPRD.ToLower(), new List<string>()
            {
                IESWebConstants.ACTION_GET_PPRD.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_VERSION.ToLower(), new List<string>()
            {
                "index", // remove if non-admin users aren't allowed access to page at all
                IESWebConstants.VIEW_VERSION.ToLower(),
                IESWebConstants.ACTION_GET_VERSION_DIFFERENCES.ToLower(),
                IESWebConstants.VIEW_VERSION_DIFF.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_BURDEN_POOL.ToLower(), new List<string>());

            result.Add(IESWebConstants.CONTROLLER_REPORTS.ToLower(), new List<string>()
            {
                "index", // remove if non-admin users aren't allowed access to page at all
                IESWebConstants.VIEW_REPORTS.ToLower(),
                IESWebConstants.ACTION_GENERATE_FULL_PPRD.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_ADMIN.ToLower(), new List<string>()
            {
                IESWebConstants.ACTION_GET_MENU_OPTIONS.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_FILE_ATTACHMENTS.ToLower(), new List<string>()
            {
                "index",
                IESWebConstants.ACTION_GET_FILE_ATTACHMENTS.ToLower()
            });

            return result;
        }

        /// <summary>
        /// Generate a dictionary containing controllers and actions that allow COBRA Administrator access.
        /// </summary>
        /// <returns>Dictionary containing controllers and actions that allow COBRA Administrator access.</returns>
        internal static Dictionary<string, List<string>> GenerateCobraAdminDictionary()
        {
            // Notes: The RDM application is primarily a tool for Rates Administrators.  As such, most of the
            // controller actions are only available to RDM Admins.  However, there are a handful of actions
            // that are available to RDM COBRA Administrators.
            // Therefore, the RDM COBRA Admin Authorization Dictionary will simply be a list of actions for each controller that  
            // are available to the RDM COBRA Administrators with the assumption that all other controller/actions are
            // for RDM Admins only.  
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            
            result.Add(IESWebConstants.CONTROLLER_REPORTS.ToLower(), new List<string>()
            {
                IESWebConstants.ACTION_EXPORT_COBRA_DATA.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_ADMIN.ToLower(), new List<string>()
            {
                IESWebConstants.ACTION_GET_COBRA_DETAILS_BY_VERSION.ToLower(),
                IESWebConstants.ACTION_GET_COBRA_YEAR_CONFIGURATION.ToLower(),
                IESWebConstants.ACTION_SAVE_COBRA_DETAILS.ToLower(),
                IESWebConstants.ACTION_SAVE_COBRA_YEAR_CONFIGURATION.ToLower(),
                IESWebConstants.VIEW_COBRA_DETAILS.ToLower(),
                IESWebConstants.VIEW_COBRA_YEAR_CONFIGURATION.ToLower()
            });

            return result;
        }

        /// <summary>
        /// Generate a dictionary containing controller and actions that require regular or COBRA Administrator access (i.e. Lock/Unlock/RefreshLock/Who's Online).
        /// </summary>
        /// <returns>Dictionary containing controller and actions that require regular or COBRA Administrator access.</returns>
        internal static Dictionary<string, List<string>> GenerateCommonAdminDictionary()
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            result.Add(IESWebConstants.CONTROLLER_HOME.ToLower(), new List<string>()
            {
                IESWebConstants.ACTION_LOCK.ToLower(),
                IESWebConstants.ACTION_UNLOCK.ToLower(),
                IESWebConstants.ACTION_REFRESH_LOCK.ToLower()
            });

            result.Add(IESWebConstants.CONTROLLER_ADMIN.ToLower(), new List<string>()
            {
                IESWebConstants.ACTION_GET_WHOS_ONLINE.ToLower()
            });

            return result;
        }
    }
}