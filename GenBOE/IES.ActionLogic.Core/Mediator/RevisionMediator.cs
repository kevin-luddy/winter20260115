// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.Mediator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DataBridge.ModelViews;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Utilities;
	using IES.DataBridge.Loaders;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// The mediator for loading/caching Revisions.
	/// </summary>
	/// <seealso cref="IRevisionMediator" />
	public class RevisionMediator : IRevisionMediator
	{
		/// <summary>
		/// The revision loader
		/// </summary>
		private readonly IRevisionLoader revisionLoader;

		/// <summary>
		/// The cache
		/// </summary>
		private readonly ICacheDataLoader cacheDataLoader;

		/// <summary>
		/// The logger
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="RevisionMediator"/> class.
		/// </summary>
		/// <param name="revisionLoader">The revision loader.</param>
		/// <param name="cacheDataLoader">The cache data loader.</param>
		public RevisionMediator(ILogger<RevisionMediator> logger, IRevisionLoader revisionLoader, ICacheDataLoader cacheDataLoader)
		{
			this.logger = logger;
			this.revisionLoader = revisionLoader;
			this.cacheDataLoader = cacheDataLoader;
		}

		/// <summary>
		/// Get the specified revision.
		/// If Id is null or -1 for new Id, return current WIP revision.
		/// </summary>
		/// <param name="id">Revision ID</param>
		/// <returns>If Id is null or -1 for new Id, return current WIP revision; othewise, return specified revision.</returns>
		public RevisionModelView GetById(int? id)
		{
			IList<RevisionModelView> revisions = GetAll();

			RevisionModelView revision = (id == null || id == -1) ?
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
			return GetById(null);
		}

		/// <summary>
		/// Get All Revisions
		/// </summary>
		/// <returns>All Revisions in ascending order by Id</returns>
		public IList<RevisionModelView> GetAll()
		{
			IList<RevisionModelView> revisions;
			using (StopwatchTimer sw = new("DataMapper.GetById", logger))
			{
				GetAllRevisionsDelegate cacheDelegate = new(revisionLoader.GetAll);
				revisions = cacheDataLoader.GetData(cacheDelegate, Array.Empty<object>(), CacheConstants.GET_ALL_REVISIONS) as IList<RevisionModelView>;
			}

			return revisions;
		}

		/// <summary>
		/// Get Revisions as an options list
		/// </summary>
		/// <returns>Revision options</returns>
		public IList<RevisionOptionModelView> GetRevisionOptions()
		{
			return GetRevisionOptions(GetAll());
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
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
			int? id = revisionLoader.Delete(dtoToDelete);
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

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
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
			int? id = revisionLoader.Publish(wipRevision, publishedBy, datePublished);
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

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
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
			int? id = revisionLoader.Rollback(lastPublishedRevision, wipRevision);
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

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
			return revisionLoader.GetVersionComparisonRows(oldID, newID);
		}

		/// <summary>
		/// Upsert
		/// </summary>
		/// <param name="dtoToUpsert">Dto that is upserted</param>
		/// <returns>Id of the dto after the modification</returns>
		public int? Upsert(RevisionModelView dtoToUpsert)
		{
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
			int? id = revisionLoader.Upsert(dtoToUpsert);
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);

			return id;
		}

		/// <summary>
		/// Clear the Revision cache.
		/// </summary>
		public void ClearRevisionCache()
		{
			cacheDataLoader.Remove(CacheConstants.GET_ALL_REVISIONS);
		}
	}

	/// <summary>
	/// Delegate for retrieving all of the revisions.
	/// </summary>
	/// <returns>All of the revisions.</returns>
	public delegate IList<RevisionModelView> GetAllRevisionsDelegate();
}
