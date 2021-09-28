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
    using System.Net.Mail;
    using System.Web.Configuration;
    using GenTRAC.ActionLogic.ModelView.Proposals;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
    /// </summary>
    public class PtmEmailer : Emailer, IPtmEmailer
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger log = new Logger(typeof(PtmEmailer));

        /// <summary>
        /// Security information
        /// </summary>
        private ISecurityInformation SecurityInformation { get; set; }

        /// <summary>
        /// Gets or sets the data fetching scheduler.
        /// </summary>
        private IDataFetchingScheduler DataFetchingScheduler { get; set; }

        /// <summary>
        /// Delegate for sending a nonpreferred tools Email.
        /// </summary>
        /// /// <param name="currentUser">The current user</param>
        /// <param name="LobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        private delegate void SendNonpreferredToolsEmailDelegate(UserData currentUser, UserDTO LobEstMgrDel, ProposalInformationModelView proposal);

        /// <summary>
        /// Delegate for sending a preferred tools Email.
        /// </summary>
        /// /// <param name="currentUser">The current user</param>
        /// <param name="LobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        private delegate void SendPreferredToolsEmailDelegate(UserData currentUser, UserDTO LobEstMgrDel, ProposalInformationModelView proposal);

        /// <summary>
        /// Delegate for sending out generic emails.
        /// </summary>
        /// <param name="inEmailTypeToSend">The email to send</param>
        /// <param name="inRecipient">recipient(s) of the email (use comma between names if multiple)</param>
        /// <param name="inCClist">CC list for the email</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="extraLoggingInfo">Extra info to include in logs</param> 
        /// <returns>Boolean if the email was successful or not</returns>
        private delegate bool SendEmailDelegate(EmailContent inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist,
            string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, UserData currentUser, string extraLoggingInfo);

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        /// <param name="inSecurityInformation">Security information</param>
        /// <param name="dataFetchingScheduler">Data fetching scheduler</param>
        public PtmEmailer(ISecurityInformation inSecurityInformation, IDataFetchingScheduler dataFetchingScheduler)
        {
            this.SecurityInformation = inSecurityInformation;
            this.DataFetchingScheduler = dataFetchingScheduler;
        }

        /// <summary>
        /// Sends email messages based on parameters and uses Host
        /// 
        /// Validation Note:
        ///   - The method will validate that the # of tokens in the subject/body are equal to the number of tokens sent in 
        ///         (i.e. "Hi, {0}, I'm a message" should have 1 value in the *ReplaceTokens array)
        /// 
        /// Special Logic:
        ///   - If the web.config contains a true value for "EmailsToCurrentlyLoggedInUser", 
        ///     this method will attempt to locate the currently logged in users' email.
        ///     
        /// General Note:
        ///   Check your spam folder as it appears messages will go there by default!
        /// </summary>
        /// <param name="inEmailTypeToSend">The email to send</param>
        /// <param name="inRecipient">recipient(s) of the email (use comma between names if multiple)</param>
        /// <param name="inCClist">CC list for the email</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="extraLoggingInfo">Extra info to include in logs</param> 
        /// <returns>true if email sent successfully, false if it failed</returns>
        public bool SendEmail(EmailContent inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist,
            string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, string extraLoggingInfo = null)
        {
            UserData currentUser = this.SecurityInformation.ActiveUserData;
            return this.SendEmailPrivate(inEmailTypeToSend, inRecipient, inCClist, inSubjectReplaceTokens, inBodyReplaceTokens, inAttachment, currentUser, extraLoggingInfo);
        }

        /// <summary>
        /// Sends the email as a delegate on another thread
        /// </summary>
        /// <param name="inEmailTypeToSend">The email to send</param>
        /// <param name="inRecipient">recipient(s) of the email (use comma between names if multiple)</param>
        /// <param name="inCClist">CC list for the email</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="extraLoggingInfo">Extra info to include in logs</param> 
        public void SendDelegateEmail(EmailContent inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist,
        string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, string extraLoggingInfo = null)
        {
            if (!ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false))
            {
                UserData currentUser = this.SecurityInformation.ActiveUserData;
                SendEmailDelegate emailDelegate = new SendEmailDelegate(this.SendEmailPrivate);
                this.DataFetchingScheduler.FetchEmails(emailDelegate, new object[] { inEmailTypeToSend, inRecipient, inCClist, inSubjectReplaceTokens, inBodyReplaceTokens, inAttachment, currentUser, extraLoggingInfo });
            }
            else
            {
                // if email is disabled by configuration setting
                this.log.Debug("Email is disabled by configuration setting in SendDelegateEmail");
            }
        }

        /// <summary>
        /// Sends a test email to ensure that all of the app configurations are correct including SMTP server and From email address
        /// </summary>
        /// <param name="inRecipient">email address to send to</param>
        /// <returns>Error String</returns>
        public string SendTestEmail(string inRecipient)
        {
            string returnVal = string.Empty;

            using (MailMessage message = new MailMessage())
            {
                using (SmtpClient smtp = new SmtpClient(WebConfigurationManager.AppSettings["EmailServer"]))
                {
                    string fromAddress = WebConfigurationManager.AppSettings["HelpdeskEmailAddress"];
                    this.log.Debug("EMAIL - Getting ready to send email to " + inRecipient);
                    message.Subject = "PTM Test Email";
                    message.Body = string.Format("<h3>Testing PTM email sending capabilities.</h3><p>Email Sent from {0} at {1} {2}</p><p>SMTP Server: {3}</p><p>Sincerely,<br>{4}", WebConfigurationManager.AppSettings["ServerURL"], DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), WebConfigurationManager.AppSettings["EmailServer"], fromAddress);
                    message.IsBodyHtml = true;
                    message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                    if (string.IsNullOrEmpty(inRecipient) || string.IsNullOrEmpty(inRecipient.Trim().Replace(",", string.Empty)))
                    {
                        // there are no valid recipients for the email ... this is a problem.  log an error message and abort
                        // the send since it's not going to anyone right now
                        returnVal = string.Format("Unable to send email as there are no recipients, recipients are [{0}]. Email subject is {1}.  Email body is {2}.", inRecipient, message.Subject, message.Body);
                        this.log.Warn(returnVal);
                        return returnVal;
                    }

                    message.From = new MailAddress(fromAddress);
                    message.To.Add(inRecipient);

                    // check to ensure the system doesn't have emails disabled.  Really should only be turned off
                    // for cases like security scanning the system and not wanting to flood anyone's inbox
                    this.log.Info("EMAIL - SmtpSend begin");
                    string email = string.Format("recipients are [{0}]. Email subject is {1}. Email body is {2}.", inRecipient, message.Subject, message.Body);
                    try
                    {
                        smtp.Send(message);
                        this.log.Info(string.Format("The following email was sent, {0}", email));
                    }
                    catch (SmtpException s)
                    {
                        returnVal = string.Format("There was an error sending email, {0}", email);
                        // We can continue here since sending an email is not critical to the operation
                        // This is necessary when running from a development boxes where email is blocked by McAfee
                        this.log.Error(s, returnVal);
                    }
                    finally
                    {
                        this.log.Info("EMAIL - SmtpSend end");
                    }
                }
            }

            return returnVal;
        }

        /// <summary>
        /// Sends the email to the LOB Estimating Manager/Delegate when a user selects a pricing tool other than ProPricer and/or a boe tool other than genBoe
        /// </summary>
        /// <param name="currentUser">The current user</param>
        /// <param name="lobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        public void SendNonpreferredToolsEmail(UserData currentUser, UserDTO lobEstMgrDel, ProposalInformationModelView proposal)
        {
            if (!ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false))
            {
                SendNonpreferredToolsEmailDelegate emailDelegate = new SendNonpreferredToolsEmailDelegate(this.PrivateSendNonpreferredToolsEmailDelegate);
                this.DataFetchingScheduler.FetchEmails(emailDelegate, new object[] { currentUser, lobEstMgrDel, proposal });
            }
            else
            {
                // if email is disabled by configuration setting
                this.log.Debug("Email is disabled by configuration setting in SendNonpreferredToolsEmail");
            }
        }

        /// <summary>
        /// This should be a private function but due to threading emails and testing, it needs to be public.
        /// Sends the Nonpreferred Tools email.
        /// </summary>
        /// <param name="currentUser">The current user</param>
        /// <param name="lobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        public void PrivateSendNonpreferredToolsEmailDelegate(UserData currentUser, UserDTO lobEstMgrDel, ProposalInformationModelView proposal)
        {
            if (currentUser == null)
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            if (lobEstMgrDel == null)
            {
                throw new ArgumentNullException(nameof(lobEstMgrDel));
            }

            if (proposal == null)
            {
                throw new ArgumentNullException(nameof(proposal));
            }

            EmailContent emailContent = Emails.NONPREFERRED_PRICING_ESTIMATING_TOOL_EMAIL;
            string[] subjectTokens = { };
            string[] bodyTokens = { currentUser.DisplayName, proposal.ProposalTrackingNumber, proposal.ProposalTitle };

            this.SendEmail(emailContent, lobEstMgrDel.EmailAddress, null, subjectTokens, bodyTokens, null, currentUser);
        }

        /// <summary>
        /// Sends the email to the LOB Estimating Manager/Delegate when a user adjusts a previously non-preferred a pricing and/or a boe tool to ProPricer and genBoe
        /// </summary>
        /// <param name="currentUser">The current user</param>
        /// <param name="lobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        public void SendPreferredToolsEmail(UserData currentUser, UserDTO lobEstMgrDel, ProposalInformationModelView proposal)
        {
            if (!ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false))
            {
                SendPreferredToolsEmailDelegate emailDelegate = new SendPreferredToolsEmailDelegate(this.PrivateSendPreferredToolsEmailDelegate);
                this.DataFetchingScheduler.FetchEmails(emailDelegate, new object[] { currentUser, lobEstMgrDel, proposal });
            }
            else
            {
                // if email is disabled by configuration setting
                this.log.Debug("Email is disabled by configuration setting in SendPreferredToolsEmail");
            }
        }

        /// <summary>
        /// This should be a private function but due to threading emails and testing, it needs to be public.
        /// Sends the Preferred Tools email.
        /// </summary>
        /// <param name="currentUser">The current user</param>
        /// <param name="lobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        public void PrivateSendPreferredToolsEmailDelegate(UserData currentUser, UserDTO lobEstMgrDel, ProposalInformationModelView proposal)
        {
            if (currentUser == null)
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            if (lobEstMgrDel == null)
            {
                throw new ArgumentNullException(nameof(lobEstMgrDel));
            }

            if (proposal == null)
            {
                throw new ArgumentNullException(nameof(proposal));
            }

            EmailContent emailContent = Emails.PREFERRED_PRICING_ESTIMATING_TOOL_EMAIL;
            string[] subjectTokens = { };
            string[] bodyTokens = { currentUser.DisplayName, proposal.ProposalTrackingNumber, proposal.ProposalTitle };

            this.SendEmail(emailContent, lobEstMgrDel.EmailAddress, null, subjectTokens, bodyTokens, null, currentUser);
        }

        /// <summary>
        /// Sends email messages based on parameters and uses Host
        /// 
        /// Validation Note:
        ///   - The method will validate that the # of tokens in the subject/body are equal to the number of tokens sent in 
        ///         (i.e. "Hi, {0}, I'm a message" should have 1 value in the *ReplaceTokens array)
        /// 
        /// Special Logic:
        ///   - If the web.config contains a true value for "EmailsToCurrentlyLoggedInUser", 
        ///     this method will attempt to locate the currently logged in users' email.
        ///     
        /// General Note:
        ///   Check your spam folder as it appears messages will go there by default!
        /// </summary>
        /// <param name="inEmailTypeToSend">The email to send</param>
        /// <param name="inRecipient">recipient(s) of the email (use comma between names if multiple)</param>
        /// <param name="inCClist">CC list for the email</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="extraLoggingInfo">Extra info to include in logs</param> 
        /// <returns>true if email sent successfully, false if it failed</returns>
        private bool SendEmailPrivate(EmailContent inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist,
            string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, UserData currentUser, string extraLoggingInfo = null)
        {
            if (inCClist == null)
            {
                inCClist = new Collection<UserDTO>();
            }

            ICollection<UserData> userDataForCc = new Collection<UserData>();
            foreach (UserDTO cc in inCClist)
            {
                userDataForCc.Add(new UserData()
                {
                    Email = cc.EmailAddress,
                    Ntid = cc.Ntid,
                    DisplayName = cc.DisplayName
                });
            }

            return base.SendEmail(inEmailTypeToSend, inRecipient, userDataForCc, inSubjectReplaceTokens, inBodyReplaceTokens, inAttachment, currentUser, extraLoggingInfo);
        }
    }
}
