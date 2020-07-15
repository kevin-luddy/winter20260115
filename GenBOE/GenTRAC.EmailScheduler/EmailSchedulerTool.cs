// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace EmailScheduler
{
    using System;
    using System.Security.Principal;
    using System.Threading;
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// This class sends emails to users for Proposals that are reaching their due dates or are late.
    /// </summary>
    public static class EmailSchedulerTool
    {
        private static readonly Logger logger = new Logger(typeof(EmailSchedulerTool));

        public static void Main(string[] args)
        {
            Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            if (args != null)
            {
                if (args.Length == 0)
                {
                    try
                    {
                        // Only send out emails within business hours
                        if (Utilities.IsWithinBusinessHours())
                        {
                            // find Proposals that require email
                            IEmailInformationLoader loader = new EmailInformationLoader();
                            SecurityInformation securityInformation = new SecurityInformation(new ActiveDirectoryUtilities(30), new MemoryCache());
                            DataFetchingScheduler dataFetchingScheduler = new DataFetchingScheduler();
                            IPtmEmailer emailer = new PtmEmailer(securityInformation, dataFetchingScheduler);
                            ApprovalEmailer approvalEmailer = new ApprovalEmailer(loader, emailer);
                            approvalEmailer.SendEmails(null);

                            if (Utilities.IsTimeForDocumentReminderEmails())
                            {
                                DocumentReminderEmailer documentReminderEmailer = new DocumentReminderEmailer(loader, emailer);
                                documentReminderEmailer.SendEmails();
                            }

                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                        System.Console.WriteLine(ex.StackTrace);

                        logger.Error(ex);
                        throw;
                    }
                }
                else
                {
                    // Assume arg[0] is an email address to which we will send a test message
                    SendTestEmail(args[0]);
                }
            }
        }

        /// <summary>
        /// Sends a test email to ensure that all of the app configurations are correct including SMTP server and From email address
        /// </summary>
        /// <param name="emailAddress">email address to send to</param>
        private static void SendTestEmail(string emailAddress)
        {
            PtmEmailer emailer = new PtmEmailer(null, null);

            string returnValue = emailer.SendTestEmail(emailAddress);

            if (string.IsNullOrEmpty(returnValue))
            {
                logger.Info("A test email has been sent to: " + emailAddress);
            }
            else
            {
                // Do not update the database, the email has not been sent for this proposal.
                logger.Error("There was an error sending an email to: " + emailAddress + "; " + returnValue);
            }
        }
    }
}
