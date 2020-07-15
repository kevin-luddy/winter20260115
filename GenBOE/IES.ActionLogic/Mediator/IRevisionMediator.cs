// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Mediator
{
    using System.Collections.Generic;
    using DataBridge.Loaders;
    using DataBridge.ModelViews;

    /// <summary>
    /// Interface for Revision Mediator
    /// </summary>
    public interface IRevisionMediator : IRevisionLoader
    {
        /// <summary>
        /// Get the specified revision.  
        /// If Id is null, return current WIP revision.
        /// </summary>
        /// <param name="id">Revision ID</param>
        /// <returns>If Id is null, return current WIP revision; othewise, return specified revision.</returns>
        RevisionModelView GetById(int? id);

        /// <summary>
        /// gets the prior revision or null if not found based on revisionId
        /// </summary>
        /// <param name="revisions">List of RevisionModelViews sorted in ascending order by Id</param>
        /// <param name="revisionId">selected revision Id</param>
        /// <returns>Prior Revision if found; null otherwise</returns>
        RevisionModelView GetPriorRevision(ICollection<RevisionModelView> revisions, int revisionId);

        /// <summary>
        /// Helper method to return the current WIP revision.
        /// </summary>
        /// <returns>Current WIP Revision</returns>
        RevisionModelView GetWipRevision();

        /// <summary>
        /// Get Revisions as an options list
        /// </summary>
        /// <returns>Revision options</returns>
        IList<RevisionOptionModelView> GetRevisionOptions();

        /// <summary>
        /// Get Revisions as an options list
        /// </summary>
        /// <param name="revisions">revisions</param>
        /// <returns>Revision options</returns>
        IList<RevisionOptionModelView> GetRevisionOptions(ICollection<RevisionModelView> revisions);

        /// <summary>
        /// Clear the Revision cache.
        /// </summary>
        void ClearRevisionCache();
    }
}
