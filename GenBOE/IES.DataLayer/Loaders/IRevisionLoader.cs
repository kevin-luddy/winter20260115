// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for Revision Loader
    /// </summary>
    public interface IRevisionLoader
    {
        /// <summary>
        /// Get All Revisions
        /// </summary>
        /// <returns>All Revisions in ascending order by Id</returns>
        IList<RevisionModelView> GetAll();

        /// <summary>
        /// Publish Revision 
        ///   - Mark the current WIP revision as published by setting the DatePublished and PublishedBy fields.
        ///   - Make a copy of the current WIP Revision and increment its revision number.
        /// </summary>
        /// <param name="wipRevision">Current WIP Revision</param>
        /// <param name="publishedBy">Display name for user that published the revision</param>
        /// <param name="datePublished">Date Published (optional)</param>
        /// <returns>Id of the new WIP revision.</returns>
        int? Publish(RevisionModelView wipRevision, string publishedBy, DateTime? datePublished = null);

        /// <summary>
        /// Rollback Revision 
        ///   - Delete the current WIP revision
        ///   - Make a copy of the last published revision and make it the new WIP Revision by clearing the DatePublished and PublishedBy fields.
        /// </summary>
        /// <param name="lastPublishedRevision">Last Published Revision</param>
        /// <param name="wipRevision">Current WIP Revision</param>
        /// <returns>Id of the new WIP revision.</returns>
        int? Rollback(IESUpdateableModelView lastPublishedRevision, RevisionModelView wipRevision);

        /// <summary>
        /// GetVersionComparisonRows
        /// </summary>
        /// <param name="oldID">old revision id</param>
        /// <param name="newID">new revision id</param>
        /// <returns>result</returns>
        ICollection<VersionComparisonGridRowModelView> GetVersionComparisonRows(int oldID, int newID);

        /// <summary>
        /// Delete Revision
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        int? Delete(RevisionModelView dtoToDelete);

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        int? Upsert(RevisionModelView dtoToUpsert);
    }
}
