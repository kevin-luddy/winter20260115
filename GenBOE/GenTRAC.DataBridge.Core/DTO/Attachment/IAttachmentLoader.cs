// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using IES.Core;

    /// <summary>
    /// Attachment Loader.
    /// </summary>
    public interface IAttachmentLoader : IDataLoader<AttachmentDto>
    {
        /// <summary>
        /// Gets all Attachments for the proposal, not including the File Contents.
        /// </summary>
        /// <param name="proposalId">The proposal ID.</param>
        /// <returns>A list of Attachments for the given proposal.</returns>
        ICollection<AttachmentDto> GetAttachmentsForProposal(int proposalId);

        /// <summary>
        /// Gets the attachment metadata only.
        /// </summary>
        /// <param name="attachmentId">The attachment identifier.</param>
        /// <returns>AttachmentDto without byte[] representation of the attachment file.</returns>
        AttachmentDto GetAttachmentMetadata(int attachmentId);

        /// <summary>
        /// Returns true if all required attachments have been uploaded
        /// </summary>
        /// <param name="proposalId">ID of proposal to check</param>
        /// <returns>true if all required attachments have been uploaded</returns>
        bool AllRequiredAttachmentsHaveBeenUploaded(int proposalId);

        /// <summary>
        /// Returns true if the optional DelegationOfAuthority attachment has been uploaded
        /// </summary>
        /// <param name="proposalId">ID of proposal to check</param>
        /// <returns>true if the optional DelegationOfAuthority attachment has been uploaded</returns>
        bool OptionalAttachmentHasBeenUploaded(int proposalId);

        /// <summary>
        /// Save only a reference to an attachment, for a revised proposal
        /// </summary>
        /// <param name="proposalId">Current Proposal Id</param>
        /// <param name="attachmentType">Attachment type that you want to reference</param>
        /// <returns>Attachment Id</returns>
        int? SaveAttachmentReferenceForRevisedProposal(int proposalId, AttachmentType attachmentType);
    }
}
