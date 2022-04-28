// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.Collections.Generic;
    using IES.Common.Exceptions;

    public interface IActiveDirectoryUtilities
    {
        /// <summary>
        /// Get members of an AD group
        /// </summary>
        /// <param name="inGroupName">The group's name</param>
        /// <returns>Collection of UserData objects</returns>
        /// <exception cref="GeneralAppException">Thrown if group DNE</exception>
        ICollection<UserData> GetAdGroupUsers(string inGroupName);

        /// <summary>
        /// Get user information from their Ntid
        /// </summary>
        /// <param name="inNtid">user's Ntid</param>
        /// <param name="isGroup">is the thing that we are looking for a group?</param>
        /// <returns>user data regarding account, null if user not found</returns>
        UserData GetUserByQualifiedAccount(string inNtid, bool isGroup);

        /// <summary>
        /// Returns a collection of groups for a user
        /// </summary>
        /// <param name="userNtid">user's ntid</param>
        /// <returns>Collection of groups</returns>
        ICollection<GroupData> GetGroupsForUser(string inNtid);

        /// <summary>
        /// Returns an XML string containing the user's NTID and all their AD group Ids.
        /// For example: &lt;ROOT&gt;&lt;id&gt;paliderd&lt;/id&gt;&lt;id&gt;ebs.estimationinitiative.devteam&lt;/id&gt;&lt;/ROOT&gt;
        /// </summary>
        /// <param name="ntid">user's ntid</param>
        /// <param name="groups">user's AD groups</param>
        /// <returns>XML string with user and group IDs</returns>
        string GetUserAndGroupIdsAsXml(string ntid, ICollection<IES.Common.GroupData> groups);

        /// <summary>
        /// Returns true if the group exists in AD
        /// </summary>
        /// <param name="inGroupName">The group name</param>
        /// <returns>Whether or not the AD group exists</returns>
        bool IsValidADGroup(string inGroupName);

        /// <summary>
        /// Returns true if the specified user/domain is a member of the group/domain.
        /// </summary>
        /// <param name="inUserName">The user name</param>
        /// <param name="inGroupName">The group name</param>
        /// <returns></returns>
        bool IsMemberOfADGroup(string inUserName, string inGroupName);

        /// <summary>
        /// Search Active Directory by user last name and display the results to the Active Directory search results view
        /// </summary>
        /// <param name="userSearchString">Search string - either the user's last name or his/her NT account name</param>
        /// <param name="searchBy">Search by last name or account</param>
        /// <param name="matchBy">Starts-with or exact match</param>
        /// <returns>Active Directory search results</returns>
        ICollection<UserData> SearchUsers(string userSearchString, ActiveDirectorySearchBy searchBy, ActiveDirectoryMatchType matchBy);

        /// <summary>
        /// Checks multiple users against multiple groups, to see if the user+domain is a member of the specified AD group+domain.
        /// </summary>
        /// <param name="usersToCheck">All of the users to check</param>
        /// <param name="groupsToCheckAgainst">All of the groups to check</param>
        /// <returns>A mapping of users and whether they belong to at least 1 group</returns>
        Dictionary<UserData, bool> CheckUsersBoeAccess(ICollection<UserData> usersToCheck, ICollection<GroupData> groupsToCheckAgainst);

        /// <summary>
        /// Gets the authorization groups from web configuration.
        /// </summary>
        /// <returns>A list of AD groups from web.config that are authorized to use the application.</returns>
        ICollection<GroupData> GetAuthorizationGroupsFromWebConfig();

        /// <summary>
        /// determine if this is a group or individual
        /// </summary>
        /// <param name="inNtID">group or user's NT ID</param>
        /// <returns>true if group, false if not</returns>
        bool IsGroup(string inNtID);

        /// <summary>
        /// Gets the NTIDs from the Web.config that should not be updated during ADSync
        /// </summary>
        /// <returns>As list of NTIDs</returns>
        ICollection<string> GetNoADSyncAccountsFromWebConfig();
    }
}
