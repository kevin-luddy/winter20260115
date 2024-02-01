// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.DataBridge.ModelViews;
    using IES.Models;
	using IES.Common.Core;
	using Microsoft.Extensions.Logging;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Utilities;

	/// <summary>
	/// Revision Data Loader
	/// </summary>
	public class RevisionLoader : IRevisionLoader
    {
        /// <summary>
        /// The log
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="logger">logger</param>
        public RevisionLoader(ILogger<RevisionLoader> logger)
        {
            this.logger = logger;
        }

		#region Retrieves

			/// <summary>
			/// Get the specified revision.  
			/// If Id is null, return current WIP revision.
			/// </summary>
			/// <param name="id">Revision ID</param>
			/// <returns>If Id is null, return current WIP revision; othewise, return specified revision.</returns>
			internal RevisionModelView GetById(int? id)
        {
            RevisionModelView result = new();
            using (new StopwatchTimer(this.logger))
            {
                using (IESEntities context = new())
                {
                    Revision revision = id == null ?
                        context.Revisions.FirstOrDefault(r => r.DatePublished == null) :
                        context.Revisions.FirstOrDefault(r => r.ID == id.Value);

                    if (revision != null)
                    {
                        result = new RevisionModelView(revision.ID, revision.UpdateDate, revision.History,
                            revision.DateCreated, revision.CreatedBy, revision.Revision1, revision.DatePublished,
                            revision.PublishedBy, revision.StartYear, revision.EndYear, revision.ReleaseNotes);
                    }
                    else
                    {
                        throw new ArgumentException("Revision ID is invalid");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get All Revisions
        /// </summary>
        /// <returns>All Revisions in ascending order by Id</returns>
        public IList<RevisionModelView> GetAll()
        {
            IList<RevisionModelView> result;

            using (new StopwatchTimer(this.logger))
            {
                using (IESEntities context = new())
                {
                    result = context.Revisions.Select(r =>
                        new RevisionModelView()
                        {
                            Id = r.ID,
                            UpdateDate = r.UpdateDate,
                            History = r.History,
                            DateCreated = r.DateCreated,
                            CreatedBy = r.CreatedBy,
                            Revision = r.Revision1,
                            DatePublished = r.DatePublished,
                            PublishedBy = r.PublishedBy,
                            StartYear = r.StartYear,
                            EndYear = r.EndYear,
                            ReleaseNotes = r.ReleaseNotes
                        }).OrderBy(x => x.Id).ToList();     // Must be ordered ascending - used by GetPriorRevision().
                }
            }

            return result;
        }

        #endregion

        #region Commits

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        public int? Upsert(RevisionModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? result;

            using (IESEntities iesEntities = new())
            {
                result = iesEntities.upsertRevision(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.Revision,
                    dtoToUpsert.History, dtoToUpsert.CreatedBy, dtoToUpsert.StartYear, dtoToUpsert.EndYear, dtoToUpsert.ReleaseNotes).First();
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        public int? Delete(RevisionModelView dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn;

            using (IESEntities iesEntities = new())
            {
                toReturn = iesEntities.deleteRevision(dtoToDelete.Id, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }

        #endregion

        #region Publish and Rollback

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
            int? result;

            if (wipRevision == null || wipRevision.Id < 0)
            {
                throw new ArgumentException("Publish failed - No WIP revision.");
            }

            if (publishedBy == null)
            {
                throw new ArgumentException("Publish failed - Missing publishedBy parameter.");
            }

            using (IESEntities iesEntities = new())
            {
                publishRevision_Result publishRevisionResult = iesEntities.publishRevision(wipRevision.Id, wipRevision.UpdateDate, publishedBy, wipRevision.History, wipRevision.ReleaseNotes, datePublished).First();
                result = publishRevisionResult.Id;
            }

            return result;
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
            int? result = null;

            if (lastPublishedRevision == null || lastPublishedRevision.Id < 0)
            {
                throw new ArgumentException("Rollback failed - No published revision to roll back to.");
            }

            if (wipRevision == null || wipRevision.Id < 0)
            {
                throw new ArgumentException("Rollback failed - No WIP revision.");
            }

            using (IESEntities iesEntities = new())
            {
                // Preserve the revision, description, and createdBy values
                string revision = wipRevision.Revision;
                string history = wipRevision.History;
                string createdBy = wipRevision.CreatedBy;
                string releaseNotes = wipRevision.ReleaseNotes;

                // Rollback
                iesEntities.deleteRevision(wipRevision.Id, wipRevision.UpdateDate);
                copyRevision_Result copyRevisionResult = iesEntities.copyRevision(lastPublishedRevision.Id, revision, history, createdBy, releaseNotes).First();

                // Clear the DatePublished and PublishedBy fields
                wipRevision = this.GetById(copyRevisionResult.Id);  // Get the new WIP revision
                wipRevision.DatePublished = null;
                wipRevision.PublishedBy = string.Empty;
                this.Upsert(wipRevision);

                result = copyRevisionResult.Id;
            }

            return result;
        }

        #endregion

        #region CompareVersions
        /// <summary>
        /// GetVersionComparisonRows
        /// </summary>
        /// <param name="oldID">old revision id</param>
        /// <param name="newID">new revision id</param>
        /// <returns>result</returns>
        public ICollection<VersionComparisonGridRowModelView> GetVersionComparisonRows(int oldID, int newID)
        {
            ICollection<VersionComparisonGridRowModelView> result = new Collection<VersionComparisonGridRowModelView>();
            ICollection<VersionComparisonGridRowModelView> burdenPoolChanges = this.GetBurdenPoolRowsCompare(oldID, newID);
            ICollection<VersionComparisonGridRowModelView> sectionChanges = this.GetSectionRowsCompare(oldID, newID);

            result.AddRange(burdenPoolChanges);
            result.AddRange(sectionChanges);
            return result;
        }

        /// <summary>
        /// GetSectionRows
        /// </summary>
        /// <param name="oldID">old revision id</param>
        /// <param name="newID">new revision id</param>
        /// <returns>VersionComparisonGridRowModelView</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Collection<VersionComparisonGridRowModelView> GetSectionRowsCompare(int oldID, int newID)
        {
            Collection<VersionComparisonGridRowModelView> rows;

            using (new StopwatchTimer(this.logger))
            {
                using (IESEntities context = new())
                {
                    // We aren't calling .ToList() on the 3 vars to allow the EF to optimize this and only
                    // make a single DB call (on UNION) on the bottom.

                    var modifiedItems = context.Sections.Where(x => x.RevisionID == oldID
                                                                    && (x.SectionContentTypeID ==
                                                                        (int) SectionContentType.Text ||
                                                                        x.SectionContentTypeID ==
                                                                        (int)SectionContentType.Address ||
                                                                        x.SectionContentTypeID ==
                                                                        (int) SectionContentType.Section))
                        .Join(context.Sections.Where(x => x.RevisionID == newID),
                            oldSection => oldSection.RevisionUniqueSectionId,
                            newSection => newSection.RevisionUniqueSectionId,
                            (oldSection, newSection) => new { oldSection, newSection })
                        .Where(x => x.oldSection.Title != x.newSection.Title ||
                                    x.oldSection.TextContent != x.newSection.TextContent)
                        .Select(x => new
                        {
                            oldTitle = x.oldSection.Title,
                            newTitle = x.newSection.Title,
                            oldContent = (x.oldSection.TextContent == "")? "updated Address Content" : x.oldSection.TextContent,
                            newContent = (x.newSection.TextContent == "")? "Updated Address Content" : x.newSection.TextContent,
                            revisionUniqueSectionId = x.oldSection.RevisionUniqueSectionId
                        });

                    var deletedItems = context.Sections.Where(oldSection => oldSection.RevisionID == oldID
                                                                            && (oldSection.SectionContentTypeID ==
                                                                                (int)SectionContentType.Text ||
                                                                                oldSection.SectionContentTypeID ==
                                                                                (int)SectionContentType.Address ||
                                                                                oldSection.SectionContentTypeID ==
                                                                                (int)SectionContentType.Section))
                        .Where(x => !context.Sections.Where(newSection => newSection.RevisionID == newID)
                            .Select(z => z.RevisionUniqueSectionId).Contains(x.RevisionUniqueSectionId))
                        .Select(x => new
                        {
                            oldContent = x.SectionContentTypeID == (int)SectionContentType.Section ? x.Title : x.TextContent,
                            revisionUniqueSectionId = x.RevisionUniqueSectionId
                        });

                    var addedItems = context.Sections.Where(newSection => newSection.RevisionID == newID
                                                                          && (newSection.SectionContentTypeID ==
                                                                              (int)SectionContentType.Text ||
                                                                              newSection.SectionContentTypeID ==
                                                                              (int)SectionContentType.Address ||
                                                                              newSection.SectionContentTypeID ==
                                                                              (int)SectionContentType.Section))
                        .Where(x => !context.Sections.Where(oldSection => oldSection.RevisionID == oldID)
                            .Select(z => z.RevisionUniqueSectionId).Contains(x.RevisionUniqueSectionId))
                        .Select(x => new
                        {
                            newContent = x.SectionContentTypeID == (int)SectionContentType.Section ? x.Title : x.TextContent,
                            revisionUniqueSectionId = x.RevisionUniqueSectionId
                        });

                    rows =
                        modifiedItems.Where(sections => sections.oldTitle != sections.newTitle).Select(
                            sections => new VersionComparisonGridRowModelView()
                            {
                                RevisionUniqueSectionId = sections.revisionUniqueSectionId,
                                FieldChanged = "Section Title",
                                ChangeType = RDMChangeType.Edit,
                                OldValue = sections.oldTitle,
                                NewValue = sections.newTitle
                            }).Union(
                            modifiedItems.Where(sections => sections.oldContent != sections.newContent).Select(
                                sections => new VersionComparisonGridRowModelView()
                                {
                                    RevisionUniqueSectionId = sections.revisionUniqueSectionId,
                                    FieldChanged = "Section Content",
                                    ChangeType = RDMChangeType.Edit,
                                    OldValue = sections.oldContent,
                                    NewValue = sections.newContent
                                })).Union(
                            deletedItems.Select(deleted => new VersionComparisonGridRowModelView()
                            {
                                RevisionUniqueSectionId = deleted.revisionUniqueSectionId,
                                FieldChanged = "Section",
                                ChangeType = RDMChangeType.Delete,
                                OldValue = deleted.oldContent == "" ? "deleted Address Content" : deleted.oldContent,
                                NewValue = string.Empty
                            })).Union(
                            addedItems.Select(added => new VersionComparisonGridRowModelView()
                            {
                                RevisionUniqueSectionId = added.revisionUniqueSectionId,
                                FieldChanged = "Section",
                                ChangeType = RDMChangeType.Add,
                                OldValue = string.Empty,
                                NewValue = added.newContent == "" ? "Added Address Content" : added.newContent,
                            })).ToCollection();
                }
                           
            }

            return rows;
        }

        /// <summary>
        /// GetBurdenPoolRows
        /// </summary>
        /// <param name="oldID">old revision id</param>
        /// <param name="newID">new revision id</param>
        /// <returns>VersionComparisonGridRowModelView</returns>
        private Collection<VersionComparisonGridRowModelView> GetBurdenPoolRowsCompare(int oldID, int newID)
        {
            Collection<VersionComparisonGridRowModelView> rows;

            using (new StopwatchTimer(this.logger))
            {
                using (IESEntities context = new())
                {
                    // We aren't calling .ToList() on the 3 vars to allow the EF to optimize this and only
                    // make a single DB call (on UNION) on the bottom.

                    var modifiedItems = context.BurdenPoolLUs.Where(x => x.RevisionID == oldID)
                        .Join(context.BurdenPoolLUs.Where(x => x.RevisionID == newID),
                            oldBp => oldBp.BurdenPool,
                            newSection => newSection.BurdenPool,
                            (oldBp, newBp) => new { oldBP = oldBp, newBP = newBp })
                        .Where(x => x.oldBP.Description != x.newBP.Description)
                        .Select(x => new
                        {
                            oldDescription = x.oldBP.Description,
                            newDescription = x.newBP.Description
                        });

					IQueryable<string> deletedItems = context.BurdenPoolLUs.Where(oldBp => oldBp.RevisionID == oldID)
                        .Where(x => !context.BurdenPoolLUs.Where(newBp => newBp.RevisionID == newID)
                            .Select(z => z.BurdenPool).Contains(x.BurdenPool))
                        .Select(oldBp => oldBp.BurdenPool);

					IQueryable<string> addedItems = context.BurdenPoolLUs.Where(newBp => newBp.RevisionID == newID)
                        .Where(x => !context.BurdenPoolLUs.Where(oldBp => oldBp.RevisionID == oldID)
                            .Select(z => z.BurdenPool).Contains(x.BurdenPool))
                        .Select(newBp => newBp.BurdenPool);

                    rows =
                        modifiedItems.Select(bps => new VersionComparisonGridRowModelView()
                        {
                            FieldChanged = "Burden Pool",
                            ChangeType = RDMChangeType.Edit,
                            OldValue = bps.oldDescription,
                            NewValue = bps.newDescription
                        }).Union(
                            deletedItems.Select(deleted => new VersionComparisonGridRowModelView()
                            {
                                FieldChanged = "Burden Pool",
                                ChangeType = RDMChangeType.Delete,
                                OldValue = deleted,
                                NewValue = string.Empty
                            })).Union(
                            addedItems.Select(added => new VersionComparisonGridRowModelView()
                            {
                                FieldChanged = "Burden Pool",
                                ChangeType = RDMChangeType.Add,
                                OldValue = string.Empty,
                                NewValue = added
                            })).ToCollection();
                }
            }

            return rows;
        }

        #endregion
    }
}