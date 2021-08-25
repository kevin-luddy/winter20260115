// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;

    public interface IHomeControllerLogic
    {
        /// <summary>
        /// Search Active Directory by user last name and display the results to the Active Directory search results view
        /// </summary>
        /// <param name="userSearchString">Search string - either the user's last name or his/her NT account name</param>
        /// <param name="searchBy">Search by last name or account</param>
        /// <param name="matchBy">Starts-with or exact match</param>
        /// <returns>Active Directory search results view</returns>
        ICollection<UserData> SearchUsers(string userSearchString, ActiveDirectorySearchBy searchBy, ActiveDirectoryMatchType matchBy);

        /// <summary>
        /// Populates the company specific properties of the <see cref="GenBOEMasterMenuItemModelView"/> for the Metric Admin menu item 
        /// </summary>
        /// <param name="theModelView">the <see cref="GenBOEMasterMenuItemModelView"/> to populate</param>
        void PopulateMetricsAdminCompanySpecificProperties(GenBOEMasterMenuItemModelView theModelView);
        
        /// <summary>
        /// Gets a collection of strings from the available <see cref="WorkspaceState"/> enum
        /// </summary>
        /// <returns></returns>
        Collection<string> GetWorkspaceStateStrings { get; }
    }
}
