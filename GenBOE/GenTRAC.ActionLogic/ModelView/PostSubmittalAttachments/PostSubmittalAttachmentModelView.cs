// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.PostSubmittalAttachments
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Model view containing post submittal attachment (PSA) information
    /// </summary>
    public class PostSubmittalAttachmentModelView
    {
        /// <summary>
        /// Required PSA attachments for proposal
        /// </summary>
        public ICollection<AttachmentDto> PostSubmittalAttachments { get; set; }

        /// <summary>
        /// Determines current user's authorization level
        /// </summary>
        public SecurityAuthorization PsaVisibility { get; set; }

        /// <summary>
        /// Indicates which proposal to display attachments for
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Max allowed attachment file size
        /// </summary>
        public int? MaxFileSize { get; set; }

        /// <summary>
        /// PSA allowed file types
        /// </summary>
        public string AllowedFileTypes { get; set; }

        /// <summary>
        /// Max number of allowed "Other" attachments.
        /// </summary>
        public int? MaxOtherFileCount { get; set; }
    }
}