// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// user loader interface
    /// </summary>
    public interface IUserLoader : IDataLoader<UserDTO>
    {
        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>all users</returns>
        ICollection<UserDTO> GetAll();

        /// <summary>
        /// Get all user ids
        /// </summary>
        /// <returns>all user ids</returns>
        ICollection<int> GetAllIds();

        /// <summary>
        /// Return a user by looking them up by their ntid
        /// </summary>
        /// <param name="inNtid">users ntid to locate them by</param>
        /// <returns>user found, null if not found</returns>
        UserDTO GetByNtid(string inNtid);

        /// <summary>
        /// Return a list of users who belong to a specific group
        /// </summary>
        /// <param name="inGroupId">group id to locate users for</param>
        /// <returns>user IDs found, null if not found</returns>
        System.Collections.Generic.ICollection<int> GetIdsByGroupId(int inGroupId);

        /// <summary>
        /// See if a user exists
        /// </summary>
        /// <param name="inUserNtid">user ntid to check for</param>
        /// <param name="outUserId">if the group exists, return the id</param>
        /// <returns>true/false user exists</returns>
        bool UserExists(string inUserNtid, out int outUserId);

        /// <summary>
        /// Get All Group User Ids
        /// </summary>
        /// <returns>All Group User Ids</returns>
        ICollection<int> GetAllGroupIds();

        /// <summary>
        /// Get a new instance of the UsersOnlineDTO
        /// </summary>
        /// <returns>instance of the UsersOnlineDTO</returns>
        UsersOnlineDTO GetUsersOnline();

        /// <summary>
        /// GetUserDTOsByADGroup
        /// </summary>
        /// <param name="inADGroup">inADGroup</param>
        /// <param name="inADDomain">inADDomain</param>
        /// <returns>userDTOs based on ADGroup</returns>
        ICollection<UserDTO> GetUserDTOsByADGroup(string inADGroup, string inADDomain);
    }
}
