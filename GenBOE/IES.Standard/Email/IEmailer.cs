// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard
{
    using System.Collections.Generic;
    using System.Net.Mail;

    /// <summary>
    /// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
    /// </summary>
    public interface IEmailer
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
        /// <param name="inCClist">CC list for the email</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="inUserData">Current User</param>
        /// <param name="extraLoggingInfo">Extra info to include in logs</param> 
        /// <returns>true if email sent successfully, false if it failed</returns>
        bool SendEmail(EmailContent inEmailTypeToSend, string inRecipient, ICollection<UserData> inCClist,
            string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment,
            UserData inUserData, string extraLoggingInfo = null);
    }

}
