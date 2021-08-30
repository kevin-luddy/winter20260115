// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;

    /// <summary>
    /// Approvals Loader
    /// </summary>
    public interface IApprovalsLoader
    {
        /// <summary>
        /// Gets all approvals for the user
        /// </summary>
        /// <param name="userId">The users genTrac user ID.</param>
        /// <returns>A list of proposals for the given user.</returns>
        ICollection<ApprovalsDto> GetApprovalsForUser(int userId);

        /// <summary>
        /// Gets # of pending approvals for the user
        /// </summary>
        /// <param name="userId">The users genTrac user ID.</param>
        /// <returns>The number of pending proposals.</returns>
        int GetPendingApprovalsCountForUser(int userId);
    }
}
