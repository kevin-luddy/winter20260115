// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.Common;

    /// <summary>
    /// user mapper interface
    /// </summary>
    public interface IUserMapper : IDataMapper<UserDTO>
    {
        /// <summary>
        /// Return the user dto for the actively logged in user
        /// </summary>
        /// <returns>user found, null if not found</returns>
        UserDTO GetActiveUser();

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>all users</returns>
        ICollection<UserDTO> GetAll();

        /// <summary>
        /// Get Users Online from Cache
        /// </summary>
        /// <returns>UsersOnlineDTO</returns>
        UsersOnlineDTO GetUsersOnline();

        /// <summary>
        /// Updates the cached version of the online user status object
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="inTrackingNumber">proposal tracking number</param>
        void UpdateUsersStatus(IES.Standard.UserData user, string inTrackingNumber);

        /// <summary>
        /// Get all user IDs
        /// </summary>
        /// <returns>returns all user ids in the system.</returns>
        ICollection<int> GetAllIds();

        /// <summary>
        /// Return a user by looking them up by their ntid
        /// </summary>
        /// <param name="inNtid">users ntid to locate them by</param>
        /// <returns>user found, null if not found</returns>
        UserDTO GetByNtid(string inNtid);

        /// <summary>
        /// Return a user by looking them up with their AD information
        /// </summary>
        /// <param name="inUserData">users AD information</param>
        /// <returns>user found, null if not found</returns>
        UserDTO GetByUserData(IES.Standard.UserData inUserData);

        /// <summary>
        /// See if a user exists
        /// </summary>
        /// <param name="inUserNtid">user ntid to check for</param>
        /// <param name="outUserId">if the user exists, return the id</param>
        /// <returns>true/false user exists</returns>
        bool UserExists(string inUserNtid, out int outUserId);

        /// <summary>
        /// Get all groups
        /// </summary>
        /// <returns>all groups</returns>
        ICollection<UserDTO> GetAllGroups();

        /// <summary>
        /// Get User Dtos by User Ids
        /// </summary>
        /// <param name="userIds">collection of user ids</param>
        /// <returns>collection of user dtos</returns>
        ICollection<UserDTO> GetUserDtosByUserIds(ICollection<int> userIds);
    }

    /// <summary>
    /// Internal User Interface
    /// </summary>
    internal interface IInternalUserMapper : IUserMapper, IInternalDataMapper<UserDTO>
    {
    }
}
