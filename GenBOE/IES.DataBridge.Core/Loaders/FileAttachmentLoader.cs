// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common.Core;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Utilities;
	using IES.DataBridge.ModelViews;
    using IES.Models;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// File Attachment Loader
	/// </summary>
    public class FileAttachmentLoader : DataLoader<FileAttachmentRowModelView>, IFileAttachmentLoader
    {
		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="logger">logger</param>
		public FileAttachmentLoader(ILogger<FileAttachmentLoader> logger) : base(logger)
		{ }

		#region Retrieves

		/// <summary>
		/// Get File Attachments for a revision.
		/// </summary>
		/// <param name="revisionId">The revision Id.</param>
		/// <returns>All File Attachment Data for a revision.</returns>
		public ICollection<FileAttachmentRowModelView> GetByRevision(int revisionId)
        {
            ICollection<FileAttachmentRowModelView> result;

            using (StopwatchTimer sw = new(this.Log))
            {
                using (IESEntities context = new())
                {
                    result = context.FileAttachments.Where(f => f.RevisionId == revisionId).Select(e =>
                        new FileAttachmentRowModelView()
                        {
                            Id = e.FileAttachmentId,
                            UpdateDate = e.UpdateDate,
                            Name = e.Name,
                            Link = e.Link,
                            SectionId = e.SectionId ?? 0, // special case because of angular dropdowns such that a zero comes back instead of null,
                            RevisionID = e.RevisionId
                        }).OrderBy(x => x.Name).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<FileAttachmentRowModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Commits

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        protected override int? Upsert(FileAttachmentRowModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? result;
            int? sectionId = dtoToUpsert.SectionId;
            if (sectionId == 0)
            {
                sectionId = null;
            }

            using (IESEntities iesEntities = new())
            {
                result = iesEntities.upsertFileAttachment(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.Name, dtoToUpsert.Link, sectionId, dtoToUpsert.RevisionID).First();
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        protected override int? Delete(FileAttachmentRowModelView dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn;

            using (IESEntities iesEntities = new())
            {
                toReturn = iesEntities.deleteFileAttachment(dtoToDelete.Id, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }

        #endregion
    }
} 