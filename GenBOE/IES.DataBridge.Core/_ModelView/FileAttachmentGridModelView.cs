// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System.Collections.Generic;
	using IES.Common.Core.Models;

	/// <summary>
	/// The Model for a grid in the File Attachment Grid/Table.
	/// </summary>
	public class FileAttachmentGridModelView  
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentGridModelView"/> class.
        /// </summary>
        public FileAttachmentGridModelView()
        {
            this.FileAttachments = new List<FileAttachmentRowModelView>();
            this.LockInfo = new LockModelView();
        }

        /// <summary>
        /// Gets or sets the selected revision Id.
        /// </summary>
        public int? SelectedRevisionId { get; set; }

        /// <summary>
        /// Gets or sets the sections.
        /// </summary>
        public ICollection<OptionModelView> Sections { get; set; }

        /// <summary>
        /// version collection
        /// </summary>
        public ICollection<RevisionOptionModelView> Versions { get; set; }

        /// <summary>
        /// Gets or sets the File Attachments.
        /// </summary>
        public ICollection<FileAttachmentRowModelView> FileAttachments { get; set; }

        /// <summary>
        /// Gets or sets lock information
        /// </summary>
        public LockModelView LockInfo { get; set; }
    }
}
