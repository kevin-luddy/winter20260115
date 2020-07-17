// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Mediator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataBridge.ModelViews;
    using IES.Common;
    using IES.Common.classes;
    using IES.DataBridge.Loaders;

    /// <summary>
    /// The mediator for loading/caching Revisions.
    /// </summary>
    /// <seealso cref="IES.ActionLogic.Mediator.IRevisionMediator" />
    public class RevisionMediator : IRevisionMediator
    {
        /// <summary>
        /// The revision loader
        /// </summary>
        private IRevisionLoader revisionLoader;

        /// <summary>
        /// The cache
        /// </summary>
        private ICacheDataLoader cacheDataLoader;

        /// <summary>
        /// The logger
        /// </summary>
        private Logger logger = new Logger(typeof(RevisionMediator));

        /// <summary>
        /// Initializes a new instance of the <see cref="RevisionMediator"/> class.
        /// </summary>
        /// <param name="revisionLoader">The revision loader.</param>
        /// <param name="cacheDataLoader">The cache data loader.</param>
        public RevisionMediator(IRevisionLoader revisionLoader, ICacheDataLoader cacheDataLoader)
        {
            this.revisionLoader = revisionLoader;
            this.cacheDataLoader = cacheDataLoader;
        }

        /// <summary>
        /// Get the specified revision.
        /// If Id is null, return current WIP revision.
        /// </summary>
        /// <param name="id">Revision ID</param>
        /// <returns>If Id is null, return current WIP revision; othewise, return specified revision.</returns>
        public RevisionModelView GetById(int? id)
        {
            IList<RevisionModelView> revisions = this.GetAll();

            RevisionModelView revision = id == null ?
                        revisions.FirstOrDefault(r => r.DatePublished == null) :
                        revisions.FirstOrDefault(r => r.Id == id.Value);

            if (revision == null)
            {
                throw new ArgumentException("Revision ID is invalid");
            }

            return revision;
        }

        /// <summary>
        /// gets the prior revision or null if not found based on revisionId
        /// </summary>
        /// <param name="revisions">List of RevisionModelView sorted in ascending order by Id</param>
        /// <param name="revisionId">selected revision Id</param>
        /// <returns>Prior Revision if found; null otherwise</returns>
        public RevisionModelView GetPriorRevision(ICollection<RevisionModelView> revisions, int revisionId)
        {
            if (revisions == null)
            {
                throw new ArgumentNullException(nameof(revisions));
            }

            RevisionModelView revision = revisions.FirstOrDefault(x => x.Id == revisionId);
            if (revision == null)
            {
                return null;    // revision not found
            }

            int idx = revisions.ToList().IndexOf(revision);
            return idx > 0 ? revisions.ToList()[idx - 1] : null; // return previous revision; or null if not found
        }

        /// <summary>
        /// Helper method to return the current WIP revision.
        /// </summary>
        /// <returns>Current WIP Revision</returns>
        public RevisionModelView GetWipRevision()
        {
            return this.GetById(null);
        }

        /// <summary>
        /// Get All Revisions
        /// </summary>
        /// <returns>All Revisions in ascending order by Id</returns>
        public IList<RevisionModelView> GetAll()
        {
            IList<RevisionModelView> revisions;
            using (StopwatchTimer sw = new StopwatchTimer("DataMapper.GetById", this.logger))
            {
                GetAllRevisionsDelegate cacheDelegate = new GetAllRevisionsDelegate(this.revisionLoader.GetAll);
                revisions = this.cacheDataLoader.GetData(cacheDelegate, new object[0], CacheConstants.GET_ALL_REVISIONS) as IList<RevisionModelView>;
            }

            return revisions;
        }

        /// <summary>
        /// Get Revisions as an options list
        /// </summary>
        /// <returns>Revision options</returns>
        public IList<RevisionOptionModelView> GetRevisionOptions()
        {
            return this.GetRevisionOptions(this.GetAll());
        }

        /// <summary>
        /// Get Revisions as an options list
        /// </summary>
        /// <param name="revisions">revisions</param>
        /// <returns>Revision options</returns>
        public IList<RevisionOptionModelView> GetRevisionOptions(ICollection<RevisionModelView> revisions)
        {
            IList<RevisionOptionModelView> result = revisions.Select(r => new RevisionOptionModelView()
            {
                Id = r.Id,
                Label = r.DatePublished == null ? CommonConstants.WorkInProgress : r.Revision,
                StartYear = r.StartYear,
                EndYear = r.EndYear,
                Revision = r.Revision
            })
                .OrderByDescending(r => r.Id)           // Must be ordered by descending - used in client.
                .ToList();

            return result;
        }

        /// <summary>
        /// Delete Revision
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        public int? Delete(RevisionModelView dtoToDelete)
        {
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
            int? id = this.revisionLoader.Delete(dtoToDelete);
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

            return id;
        }

        /// <summary>
        /// Publish Revision 
        ///   - Mark the current WIP revision as published by setting the DatePublished and PublishedBy fields.
        ///   - Make a copy of the current WIP Revision and increment its revision number.
        /// </summary>
        /// <param name="wipRevision">Current WIP Revision</param>
        /// <param name="publishedBy">Display name for user that published the revision</param>
        /// <param name="datePublished">Date Published (optional)</param>
        /// <returns>Id of the new WIP revision.</returns>
        public int? Publish(RevisionModelView wipRevision, string publishedBy, DateTime? datePublished = null)
        {
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
            int? id = this.revisionLoader.Publish(wipRevision, publishedBy, datePublished);
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

            return id;
        }

        /// <summary>
        /// Rollback Revision 
        ///   - Delete the current WIP revision
        ///   - Make a copy of the last published revision and make it the new WIP Revision by clearing the DatePublished and PublishedBy fields.
        /// </summary>
        /// <param name="lastPublishedRevision">Last Published Revision</param>
        /// <param name="wipRevision">Current WIP Revision</param>
        /// <returns>Id of the new WIP revision.</returns>
        public int? Rollback(IESUpdateableModelView lastPublishedRevision, RevisionModelView wipRevision)
        {
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
            int? id = this.revisionLoader.Rollback(lastPublishedRevision, wipRevision);
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

            return id;
        }

        /// <summary>
        /// GetVersionComparisonRows
        /// </summary>
        /// <param name="oldID">old revision id</param>
        /// <param name="newID">new revision id</param>
        /// <returns>result</returns>
        public ICollection<VersionComparisonGridRowModelView> GetVersionComparisonRows(int oldID, int newID)
        {
            return this.revisionLoader.GetVersionComparisonRows(oldID, newID);
        }

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        public int? Upsert(RevisionModelView dtoToUpsert)
        {
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
            int? id = this.revisionLoader.Upsert(dtoToUpsert);
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

            return id;
        }

        /// <summary>
        /// Clear the Revision cache.
        /// </summary>
        public void ClearRevisionCache()
        {
            this.cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
        }
    }

    /// <summary>
    /// Delegate for retrieving all of the revisions.
    /// </summary>
    /// <returns>All of the revisions.</returns>
    public delegate IList<RevisionModelView> GetAllRevisionsDelegate();
}
