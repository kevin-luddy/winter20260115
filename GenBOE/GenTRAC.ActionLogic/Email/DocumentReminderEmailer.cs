// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Email
{
    using System;
    using System.Collections.Generic;
    using DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Emailer class for optional document reminder
    /// </summary>
    public class DocumentReminderEmailer
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger logger = new Logger(typeof(ApprovalEmailer));

        /// <summary>
        /// The email information loader.
        /// </summary>
        private readonly IEmailInformationLoader emailInformationLoader;

        /// <summary>
        /// The emailer.
        /// </summary>
        private readonly IPtmEmailer emailer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentReminderEmailer"/> class.
        /// </summary>
        public DocumentReminderEmailer()
        {
            // Only here for tests to mock
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentReminderEmailer"/> class.
        /// </summary>
        /// <param name="emailLoader">The email loader.</param>
        /// <param name="inEmailer">The in emailer.</param>
        public DocumentReminderEmailer(IEmailInformationLoader emailLoader, IPtmEmailer inEmailer)
        {
            this.emailInformationLoader = emailLoader;
            this.emailer = inEmailer;
        }

        /// <summary>
        /// This method sends the emails in the email list.
        /// </summary>
        public virtual void SendEmails()
        {
            ICollection<EmailInformationDto> emailList = this.emailInformationLoader.GetDocumentReminderEmailsToBeSent();
            this.logger.Debug(DateTime.Now.ToString() + " - Starting output to log.");

            foreach (EmailInformationDto email in emailList)
            {
                string backupText = email.EmailAddress.Contains(";") ? "/Backup" : string.Empty;

                this.emailer.SendEmail(Emails.MISSING_OPTIONAL_DOCUMENT_REMINDER_EMAIL, email.EmailAddress, email.ccUsers, 
                    new string[0] { }, new string[4] { backupText, email.TrackingNumber, email.ProposalTitle, email.ProposalPsaUrl.ToString() }, null, null);
            }

            // Write the final trace message to the console trace listener.
            this.logger.Debug(DateTime.Now.ToString() + " - Ending output to log.");
        }
    }
}
