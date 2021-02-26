// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the Version Controller Logic
    /// </summary>
    public interface IVersionControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Get the differences for the Version Comparison grid when a new Version is selected
        /// </summary>
        /// <param name="id">Selected Revision Id</param>
        /// <param name="secondId">Second selected revision ID, or -1 to return previous revision</param>
        /// <param name="isRdmAdminUser">true if user is RDM Admin user</param>
        /// <param name="activeUser">The active user.</param>
        /// <returns>Differences based on the selected version</returns>
        VersionComparisonModelView GetVersionDifferences(int id, int secondId, bool isRdmAdminUser, UserData activeUser);

        /// <summary>
        /// Gets any rates for the given revision for which the rate would have only zero values for all displayed years.
        /// </summary>
        /// <param name="id">The revision ID to check.</param>
        /// <returns>Collection of invalid rates - empty if all valid</returns>
        ICollection<string> GetInvalidRates(int? id);
        
        /// <summary>
        /// Sends the publish email.
        /// </summary>
        /// <param name="publishedRevision">The published revision.</param>
        /// <param name="currentUser">The current user</param>
        void SendPublishEmail(RevisionModelView publishedRevision, UserData currentUser);
    }
}
