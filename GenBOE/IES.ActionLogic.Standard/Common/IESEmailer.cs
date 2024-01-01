// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Common
{
	using System;
	using IES.DataBridge.ModelViews;
	using IES.Standard;

	/// <summary>
	/// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
	/// </summary>
	public class IESEmailer : Emailer, IIESEmailer
    {

        /// <summary>
        /// Gets or sets the data fetching scheduler.
        /// </summary>
        private IDataFetchingScheduler DataFetchingScheduler { get; set; }

        #region Email Delegates

        /// <summary>
        /// Delegate for sending a published Email.
        /// </summary>
        /// <param name="publishedRevision">The published revision.</param>
        /// <param name="currentUser">The current user.</param>
        private delegate void SendPublishEmailDelegate(RevisionModelView publishedRevision, UserData currentUser);

        /// <summary>
        /// Delegate for sending a classified deployment success Email.
        /// </summary>
        /// <param name="deployedRevision">The deployed revision.</param>
        /// <param name="currentUser">The current user.</param>
        private delegate void SendClassifiedDeploymentSuccessEmailDelegate(RevisionModelView deployedRevision, UserData currentUser);

        /// <summary>
        /// Delegate for sending a classified deployment failed Email.
        /// </summary>
        /// <param name="failureMessage">The failure message</param>
        /// <param name="currentUser">The current user</param>
        private delegate void SendClassifiedDeploymentFailedEmailDelegate(string failureMessage, UserData currentUser);

        #endregion Email Delegates

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        /// <param name="dataFetchingScheduler">The data fetching scheduler.</param>
        public IESEmailer(IDataFetchingScheduler dataFetchingScheduler, ILogger logger) : base(logger)
		{
            this.DataFetchingScheduler = dataFetchingScheduler;
        }

        /// <summary>
        /// Sends the publish email.
        /// </summary>
        /// <param name="publishedRevision">The published revision.</param>
        /// <param name="currentUser">The current user</param>
        public void SendPublishEmail(RevisionModelView publishedRevision, UserData currentUser)
        {
            if (!ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false))
            {
                SendPublishEmailDelegate emailDelegate = new SendPublishEmailDelegate(this.PrivateSendPublishEmailDelegate);
                this.DataFetchingScheduler.FetchEmails(emailDelegate, new object[] { publishedRevision, currentUser });
            }
            else 
            {
                // if email is disabled by configuration setting
                this.log.Debug("Email is disabled by configuration setting in SendPublishEmail");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public.
        /// Sends the publish email.
        /// </summary>
        /// <param name="publishedRevision">The published revision.</param>
        /// <param name="currentUser">The current user</param>
        public void PrivateSendPublishEmailDelegate(RevisionModelView publishedRevision, UserData currentUser)
        {
            if (publishedRevision == null)
            {
                throw new ArgumentNullException(nameof(publishedRevision));
            }

            EmailContent emailContent = Emails.PUBLISH_EMAIL;
            string commonUrl = ConfigurationUtilities.GetAppSetting("ServerURL");

            string homeUrl = string.Format("{0}/", commonUrl);
            string exportUrl = string.Format("{0}/{1}/{2}/{3}", commonUrl, IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_GENERATE_FULL_PPRD, publishedRevision.Id);
            string summaryUrl = string.Format("{0}/{1}/Index/{2}", commonUrl, IESWebConstants.CONTROLLER_VERSION, publishedRevision.Id);
            string[] subjectTokens = { publishedRevision.DisplayRevision, publishedRevision.DatePublished.Value.ToString("MM/dd/yyyy HH:mm:ss") };
            string[] bodyTokens = { publishedRevision.ReleaseNotes, homeUrl, exportUrl, summaryUrl };
            string distributionList = ConfigurationUtilities.GetAppSetting("EstimatingDataGroupDL");

            this.SendEmail(emailContent, distributionList, null, subjectTokens, bodyTokens, null, currentUser);
        }

        /// <summary>
        /// Sends the email when a revision has been successfully deployed to the classified environment.
        /// </summary>
        /// <param name="deployedRevision">The deployed revision</param>
        /// <param name="currentUser">The current user</param>
        public void SendClassifiedDeploymentSuccessEmail(RevisionModelView deployedRevision, UserData currentUser)
        {
            if (!ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false))
            {
                SendClassifiedDeploymentSuccessEmailDelegate emailDelegate = new SendClassifiedDeploymentSuccessEmailDelegate(this.PrivateSendClassifiedDeploymentSuccessEmailDelegate);
                this.DataFetchingScheduler.FetchEmails(emailDelegate, new object[] { deployedRevision, currentUser });
            }
            else
            {
                // if email is disabled by configuration setting
                this.log.Debug("Email is disabled by configuration setting in SendClassifiedDeploymentSuccessEmail");
            }
        }

        /// <summary>
        /// This should be a private function but due to threading emails and testing, it needs to be public.
        /// Sends the classified deployment success email.
        /// </summary>
        /// <param name="deployedRevision">The deployed revision.</param>
        /// <param name="currentUser">The current user</param>
        public void PrivateSendClassifiedDeploymentSuccessEmailDelegate(RevisionModelView deployedRevision, UserData currentUser)
        {
            if (deployedRevision == null)
            {
                throw new ArgumentNullException(nameof(deployedRevision));
            }

            EmailContent emailContent = Emails.CLASSIFIED_DEPLOYMENT_SUCCESS_EMAIL;
            string[] subjectTokens = { deployedRevision.DisplayRevision };
            string[] bodyTokens = { deployedRevision.DisplayRevision, deployedRevision.ReleaseNotes };
            string distributionList = ConfigurationUtilities.GetAppSetting("EmailDistributionList");

            this.SendEmail(emailContent, distributionList, null, subjectTokens, bodyTokens, null, currentUser);
        }

        /// <summary>
        /// Sends the email when a revision fails to deploy to the classified environment.
        /// </summary>
        /// <param name="failureMessage">The failure message</param>
        /// <param name="currentUser">The current user</param>
        public void SendClassifiedDeploymentFailedEmail(string failureMessage, UserData currentUser)
        {
            if (!ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false))
            {
                SendClassifiedDeploymentFailedEmailDelegate emailDelegate = new SendClassifiedDeploymentFailedEmailDelegate(this.PrivateSendClassifiedDeploymentFailedEmailDelegate);
                this.DataFetchingScheduler.FetchEmails(emailDelegate, new object[] { failureMessage, currentUser });
            }
            else
            {
                // if email is disabled by configuration setting
                this.log.Debug("Email is disabled by configuration setting in SendClassifiedDeploymentFailedEmail");
            }
        }

        /// <summary>
        /// This should be a private function but due to threading emails and testing, it needs to be public.
        /// Sends the classified deployment failed email.
        /// </summary>
        /// <param name="failureMessage">The failure message</param>
        /// <param name="currentUser">The current user</param>
        public void PrivateSendClassifiedDeploymentFailedEmailDelegate(string failureMessage, UserData currentUser)
        {
            EmailContent emailContent = Emails.CLASSIFIED_DEPLOYMENT_FAILED_EMAIL;
            string[] subjectTokens = { };
            string[] bodyTokens = { failureMessage };
            string distributionList = ConfigurationUtilities.GetAppSetting("EmailDistributionList");

            this.SendEmail(emailContent, distributionList, null, subjectTokens, bodyTokens, null, currentUser);
        }
    }
}
