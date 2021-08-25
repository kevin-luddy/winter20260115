// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for File Attachment Loader
    /// </summary>
    public interface IFileAttachmentLoader : IDataLoader<FileAttachmentRowModelView>
    {
        /// <summary>
        /// Get File Attachments for a revision.
        /// </summary>
        /// <param name="revisionId">The revision Id.</param>
        /// <returns>All File Attachment Data for a revision.</returns>
        ICollection<FileAttachmentRowModelView> GetByRevision(int revisionId);
    }
}
