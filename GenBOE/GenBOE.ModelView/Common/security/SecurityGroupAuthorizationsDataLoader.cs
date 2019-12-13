// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System.Web.Security;

    /// <summary>
    /// DataLoader responsible for loading the roles for the logged in user
    /// </summary>
    public class SecurityGroupAuthorizationsDataLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SecurityGroupAuthorizationsDataLoader()
        {
        }

        /// <summary>
        /// Get the roles from AD for the currently logged in user
        /// </summary>
        /// <returns>The AD roles for the logged in user</returns>
        public string[] GetRolesForLoggedInUser()
        {
            string[] roles = Roles.GetRolesForUser();

            return roles;
        }
    }
}
