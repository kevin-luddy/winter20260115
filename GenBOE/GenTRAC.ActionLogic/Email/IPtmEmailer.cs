// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Email
{
    using System.Collections.ObjectModel;
    using System.Net.Mail;
    using GenTRAC.ActionLogic.ModelView.Proposals;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
    /// </summary>
    public interface IPtmEmailer
    {
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
        /// <param name="inCClist">CC list for the email, null if none</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="extraLoggingInfo">Extra info to include in logs</param>
        /// <returns>true if email sent successfully, false if it failed</returns>
        bool SendEmail(EmailContent inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist, string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, string extraLoggingInfo = null);

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
        void SendDelegateEmail(EmailContent inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist, string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, string extraLoggingInfo = null);

        /// <summary>
        /// Sends the email to the LOB Estimating Manager/Delegate when a user selects a pricing tool other than ProPricer and/or a boe tool other than genBoe
        /// </summary>
        /// <param name="currentUser">The current user</param>
        /// <param name="lobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        void SendNonpreferredToolsEmail(UserData currentUser, UserDTO lobEstMgrDel, ProposalInformationModelView proposal);

        /// <summary>
        /// Sends the email to the LOB Estimating Manager/Delegate when a user adjusts a previously non-preferred a pricing and/or a boe tool to ProPricer and genBoe
        /// </summary>
        /// <param name="currentUser">The current user</param>
        /// <param name="lobEstMgrDel">The LOB Estimating Manager/Delegate</param>
        /// <param name="proposal">The proposal</param>
        void SendPreferredToolsEmail(UserData currentUser, UserDTO lobEstMgrDel, ProposalInformationModelView proposal);
    }
}
