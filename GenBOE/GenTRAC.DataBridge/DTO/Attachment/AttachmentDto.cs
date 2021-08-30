// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Common;

    /// <summary>
    /// Attachment DTO
    /// </summary>
    [Serializable]
    public class AttachmentDto : IES.Common.UpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentDto"/> class.
        /// </summary>
        public AttachmentDto()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Gets or sets the Name of the attachment.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Attachment Contents.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] Contents { get; set; }

        /// <summary>
        /// Gets or sets who this attachment was uploaded by.
        /// </summary>
        public string UploadedBy { get; set; }

        /// <summary>
        /// Gets or sets the type of the attachment.
        /// </summary>
        public AttachmentType AttachmentType { get; set; }

        /// <summary>
        /// String used by the page to display the Attachment Type
        /// 
        /// For other we want to display the file name, without the extension
        /// </summary>
        public string AttachmentTypeDisplayString
        {
            get
            {
                string result = this.AttachmentType.GetDescription();

                if(this.AttachmentType == AttachmentType.Other && !string.IsNullOrEmpty(this.Name))
                {
                    string fileName = this.Name;

                    int positionLastPeriod = fileName.LastIndexOf('.');
                    if (positionLastPeriod > 0)
                    {
                        fileName = fileName.Substring(0, positionLastPeriod);
                    }

                    result += " - " + fileName;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the Proposal Id that this attachment is linked to.
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Used in the view to determine whether a file has been uploaded or not
        /// </summary>
        public bool FileHasBeenUploaded
        {
            get { return this.Id > 0; }
        }

        /// <summary>
        /// Is Revision Reference? 
        /// 
        /// False -> means you own the file
        /// True -> means you are referencing previous revision's file
        /// </summary>
        public bool IsRevisionReference { get; set; } = false;
    }
}
