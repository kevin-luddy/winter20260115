// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;

    public class HomeControllerLogic : IHomeControllerLogic
    {
        /// <summary>
        /// The AD utils
        /// </summary>
        private IActiveDirectoryUtilities activeDirectoryUtils;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeDirectoryUtils">AD lookup utilities</param>
        public HomeControllerLogic(
            IActiveDirectoryUtilities activeDirectoryUtils)
        {
            this.activeDirectoryUtils = activeDirectoryUtils;
        }
            
        /// <summary>
        /// Search Active Directory by user last name and display the results to the Active Directory search results view
        /// </summary>
        /// <param name="userSearchString">Search string - either the user's last name or his/her NT account name</param>
        /// <param name="searchBy">Search by last name or account</param>
        /// <param name="matchBy">Starts-with or exact match</param>
        /// <returns>Active Directory search results view</returns>
        public ICollection<UserData> SearchUsers(string userSearchString, ActiveDirectorySearchBy searchBy, ActiveDirectoryMatchType matchBy)
        {
            return this.activeDirectoryUtils.SearchUsers(userSearchString, searchBy, matchBy);
        }

        /// <summary>
        /// Populates the company specific properties of the <see cref="GenBOEMasterMenuItemModelView"/> for the Metric Admin menu item 
        /// </summary>
        /// <param name="theModelView">the <see cref="GenBOEMasterMenuItemModelView"/> to populate</param>
        public virtual void PopulateMetricsAdminCompanySpecificProperties(GenBOEMasterMenuItemModelView theModelView)
        {
            if (theModelView != null)
            {
                // IS&GS no longer used metrics.
                theModelView.linkUrl = null;
            }
        }
        
        /// <summary>
        /// Gets a collection of strings from the available <see cref="WorkspaceState"/> enum
        /// </summary>
        /// <returns></returns>
        public virtual Collection<string> GetWorkspaceStateStrings
        {
            get
            {
                Collection<string> toReturn = new Collection<string>();

                // Add these states
                //    "Initialization",
                //    "Working",
                //    "Locked",
                //    "Complete",
                //    "Closed"
                foreach (WorkspaceState item in (WorkspaceState[])Enum.GetValues(typeof(WorkspaceState)))
                {
                    if ((int)item > 0)
                    {
                        toReturn.Add(item.GetDescription());
                    }
                }

                return toReturn;
            }
        }
    }
}
