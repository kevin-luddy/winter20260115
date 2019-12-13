// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    /// <summary>
    /// Model View for one File Attachment
    /// </summary>
    public class FileAttachmentRowModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the section identifier.
        /// </summary>
        public int? SectionId { get; set; }

        /// <summary>
        /// Version for this model.
        /// </summary>
        public int RevisionID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
