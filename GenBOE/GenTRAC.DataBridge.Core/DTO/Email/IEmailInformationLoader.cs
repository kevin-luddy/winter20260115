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
    /// Email information loader interface
    /// </summary>
    public interface IEmailInformationLoader
    {
        /// <summary>
        /// Polls the database to return all Proposals that require an email to be sent.
        /// </summary>
        /// <param name="proposalId">If set, this should get emails that are only during this approval</param>
        /// <returns>A list of Proposal information that require an email.</returns>
        ICollection<EmailInformationDto> GetAllEmailsToBeSent(int? proposalId);

        /// <summary>
        /// Updates the DB table tracking if an email has been sent.
        /// </summary>
        /// <param name="proposalId">The Proposal Id.</param>
        /// <param name="emailType">The email type.</param>
        void UpdateEmailSent(int proposalId, EmailType emailType);

        /// <summary>
        /// Gets the optional document missing reminder emails to be sent
        /// </summary>
        /// <returns>A collection of the email info dtos for the emails to be sent</returns>
        ICollection<EmailInformationDto> GetDocumentReminderEmailsToBeSent();
    }
}
