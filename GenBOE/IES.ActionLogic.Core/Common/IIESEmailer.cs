// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
    using IES.Core;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
    /// </summary>
    public interface IIESEmailer
    {
        /// <summary>
        /// Sends the publish email.
        /// </summary>
        /// <param name="publishedRevision">The published revision.</param>
        /// <param name="currentUser">The current user</param>
        void SendPublishEmail(RevisionModelView publishedRevision, UserData currentUser);

        /// <summary>
        /// Sends the email when a revision has been successfully deployed to the classified environment.
        /// </summary>
        /// <param name="deployedRevision">The deployed revision</param>
        /// <param name="currentUser">The current user</param>
        void SendClassifiedDeploymentSuccessEmail(RevisionModelView deployedRevision, UserData currentUser);

        /// <summary>
        /// Sends the email when a revision fails to deploy to the classified environment.
        /// </summary>
        /// <param name="failureMessage">The failure message</param>
        /// <param name="currentUser">The current user</param>
        void SendClassifiedDeploymentFailedEmail(string failureMessage, UserData currentUser);
    }
}
