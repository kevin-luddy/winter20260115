// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using IES.Common;
    using GenBOE.Dtos;

    public interface IGenBOEMetricsDataLoader
    {
        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>Users</returns>
        int GetUsersTotal();

        GenBOEMetricsDTO GetGenBOEMetrics();
        void UpdateLastAccessTime(UserData userData);
        GenBOEUsersOnlineDTO GetOnlineUserDetails();
    }
}