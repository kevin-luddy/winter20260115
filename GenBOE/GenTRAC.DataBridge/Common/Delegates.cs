// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;

    #region Generic

    /// <summary>
    /// Get Id By Id Delegate
    /// </summary>
    /// <param name="id">Id of the mapped object</param>
    /// <returns>Id of the target object</returns>
    public delegate int? GetIdByIdDelegate(int id);

    /// <summary>
    /// Get Dto By Id Delegate
    /// </summary>
    /// <param name="id">ID of the Dto</param>
    /// <returns>Dto Object</returns>
    public delegate object GetDtoByIdDelegate(int id);

    /// <summary>
    /// Get Dto By Id Delegate
    /// </summary>
    /// <param name="key">Key of the Dto</param>
    /// <returns>Dto Object</returns>
    public delegate object GetDtoByKeyDelegate(string key);

    /// <summary>
    /// Get Dtos By Parent Id Delegate
    /// </summary>
    /// <typeparam name="T">Type of the Dto</typeparam>
    /// <param name="id">Id of the parent object</param>
    /// <returns>A collection of Dtos</returns>
    public delegate ICollection<T> GetDtosByParentIdDelegate<T>(int id);

    /// <summary>
    /// Get Dtos By Ids Delegate
    /// </summary>
    /// <typeparam name="T">Type of the Dto</typeparam>
    /// <param name="ids">A collection of ids</param>
    /// <returns>A collection of Dtos</returns>
    public delegate ICollection<T> GetDtosByIdsDelegate<T>(ICollection<int> ids);

    /// <summary>
    /// Get Common Dtos Delegate
    /// </summary>
    /// <typeparam name="T">Dto Type</typeparam>
    /// <returns>A collection of dtos</returns>
    public delegate ICollection<T> GetCommonDtosDelegate<T>();

    #endregion
    #region User

    /// <summary>
    /// Get User By NTID Delegate
    /// </summary>
    /// <param name="inUserNTID">User NTID</param>
    /// <returns>Corresponding User</returns>
    public delegate UserDTO GetUserByNtidDelegate(string inUserNTID);

    /// <summary>
    /// Delegate to get the Online Users
    /// </summary>
    /// <returns>UsersOnlineDTO object</returns>
    public delegate UsersOnlineDTO GetUsersOnlineDelegate();

    #endregion
    #region Security

    /// <summary>
    /// Get Security Permissions for User Delegate
    /// </summary>
    /// <param name="inUserDTO">The user to retrieve permissions for</param>
    /// <returns>A collection of user permissions</returns>
    public delegate ICollection<SecurityPermissionsResponse> GetSecurityPermissionsForUserDelegate(UserDTO inUserDTO);

    #endregion

    #region Proposal

    /// <summary>
    /// Get proposal id by its title delegate
    /// </summary>
    /// <param name="title">proposal title</param>
    /// <returns>proposal id</returns>
    public delegate int GetProposalIDByTitle(string title);
    #endregion Proposal
}
