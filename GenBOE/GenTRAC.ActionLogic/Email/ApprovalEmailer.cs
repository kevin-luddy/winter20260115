// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Email
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Transactions;
    using DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Emailer class for Approvals.
    /// </summary>
    public class ApprovalEmailer
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
        /// Initializes a new instance of the <see cref="ApprovalEmailer"/> class.
        /// </summary>
        public ApprovalEmailer()
        {
            // Only here for tests to mock
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalEmailer"/> class.
        /// </summary>
        /// <param name="emailLoader">The email loader.</param>
        /// <param name="inEmailer">The in emailer.</param>
        public ApprovalEmailer(IEmailInformationLoader emailLoader, IPtmEmailer inEmailer)
        {
            this.emailInformationLoader = emailLoader;
            this.emailer = inEmailer;
        }

        /// <summary>
        /// This method sends the emails in the email list.
        /// </summary>
        /// <param name="proposalId">If a proposal Id is sent in, only send emails for that id.</param>
        public virtual void SendEmails(int? proposalId)
        {
            ICollection<EmailInformationDto> emailList = this.emailInformationLoader.GetAllEmailsToBeSent(proposalId);
            this.logger.Debug(DateTime.Now.ToString() + " - Starting output to log.");

            foreach (EmailInformationDto email in emailList)
            {
                string[] subjectReplaceTokens = new string[0] { };
                string proposalUrl;
                switch (email.ProposalEmailType)
                {
                    case EmailType.LOBApprovedEmail:
                        proposalUrl = email.ProposalChecklistUrl.ToString();
                        break;
                    case EmailType.ForecastAlertEmail:
                        proposalUrl = email.ProposalSetupUrl.ToString();
                        break;
                    case EmailType.CertificationTimelineEmail:
                        // no url for email
                        proposalUrl = string.Empty;
                        break;
                    default:
                        proposalUrl = email.ProposalApprovalUrl.ToString();
                        break;
                }

                string[] bodyReplaceTokens;
                
                if (email.ProposalEmailType == EmailType.CertificationTimelineEmail)
                {
                    subjectReplaceTokens = new string[2] { email.TrackingNumber, email.ProposalTitle };
                    bodyReplaceTokens = new string[2] { email.TrackingNumber, email.ProposalTitle };
                }
                else
                {
                    bodyReplaceTokens = new string[3] { email.TrackingNumber, email.ProposalTitle, proposalUrl };
                }

                EmailContent emailContent = GetEmailContent(email.ProposalEmailType, email.AdditionalText);

                // Send the email
                bool emailSent = this.emailer.SendEmail(emailContent, email.EmailAddress, new Collection<UserDTO>(), subjectReplaceTokens, bodyReplaceTokens, null, " for: " + email.TrackingNumber);

                if(emailSent)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        this.emailInformationLoader.UpdateEmailSent(email.ProposalId, email.ProposalEmailType);
                        scope.Complete();
                    }
                }
            }

            // Write the final trace message to the console trace listener.
            this.logger.Debug(DateTime.Now.ToString() + " - Ending output to log.");
        }

        /// <summary>
        /// Returns the email content for the given email type.
        /// </summary>
        /// <param name="emailType">The email type.</param>
        /// <param name="additionalText">The additional text for this proposal to include.</param>
        /// <returns>The email content.</returns>
        private static EmailContent GetEmailContent(EmailType emailType, string additionalText)
        {
            EmailContent returnValue;
            switch (emailType)
            {
                case EmailType.InitialApprovalEmail:
                case EmailType.SecondApprovalEmail:
                case EmailType.FinalApprovalEmail:
                case EmailType.InitialLOBApprovalEmail:
                case EmailType.SecondLOBApprovalEmail:
                case EmailType.FinalLOBApprovalEmail:
                    returnValue = Emails.APPROVER_EMAIL;
                    break;
                case EmailType.LeadAlertForApprovers:
                    returnValue = Emails.LEAD_ALERT_APPROVERS_EMAIL;
                    break;
                case EmailType.LeadAlertForLOBApprover:
                    returnValue = Emails.LEAD_ALERT_LOB_NOT_APPROVED_EMAIL;
                    break;
                case EmailType.LOBApprovedEmail:
                    returnValue = Emails.LEAD_ALERT_LOB_APPROVED_EMAIL;
                    break;
                case EmailType.ForecastAlertEmail:
                    returnValue = Emails.FORECAST_ALERT_EMAIL;
                    break;
                case EmailType.CertificationTimelineEmail:
                    returnValue = Emails.CERTIFICATION_TIMELINE_EMAIL;
                    break;
                default:
                    returnValue = Emails.APPROVAL_EMAIL;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(additionalText))
            {
                returnValue.Body = additionalText.Replace("\n", "<br/>")  + "<br/><br/>" + returnValue.Body;
            }

            return returnValue;
        }
    }
}
