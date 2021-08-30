// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.AutoBackup
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    /// <summary>
    /// Automatic WS Backup Generator app
    /// </summary>
    public class AutoBackupGenerator
    {
        #region Private Properties

        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = new Logger(typeof(AutoBackupGenerator));

        /// <summary>
        /// The email on failure only setting
        /// </summary>
        private static bool emailOnFailureOnly = bool.Parse(ConfigurationManager.AppSettings.Get("EmailOnFailureOnly"));

        /// <summary>
        /// The email to address
        /// </summary>
        private static string emailTo = ConfigurationManager.AppSettings.Get("SendEmailTo").ToString();

        /// <summary>
        /// The version loader
        /// </summary>
        private static WorkspaceVersionMetaDataDTODataLoader versionLoader = new WorkspaceVersionMetaDataDTODataLoader();

        /// <summary>
        /// The ws loader
        /// </summary>
        private static WorkspaceDTODataLoader wsLoader = new WorkspaceDTODataLoader();

        /// <summary>
        /// Running list of the workspaces which failed
        /// </summary>
        private static List<string> workspacesWhichFailed = new List<string>();

        #endregion

        /// <summary>
        /// Entry into the application
        /// </summary>
        public static void Main()
        {
            try
            {
                Console.WriteLine("## genBOE Autoupdater Starting ##"); logger.Debug("Starting a run");

                List<Tuple<int, WorkspaceState>> wsToBackup = wsLoader.GetWsForBackupGeneration();

                Console.WriteLine($"  ## WS Retrieved ({wsToBackup.Count}) ##"); logger.Debug($"# Of Workspaces to backup: {wsToBackup.Count}");

                Collection<WorkspaceVersionMetaDataDTO> versionData = versionLoader.GetForAutoBackupGeneration(wsToBackup.Select(x => x.Item1).ToList());

                Console.WriteLine($"  ## Versions Retrieved ({versionData.Count}) ##");

                wsToBackup.ForEach(x =>
                {
                    BackupWorkspace(x.Item1, x.Item2, versionData.Where(z => z.WorkspaceID == x.Item1).ToList());
                    Console.WriteLine($"    ## WS Backed up - {x.Item2} ##");
                });

                Console.WriteLine($"  ## Handling {workspacesWhichFailed.Count} Errors ##");

                ProcessFailedWorkspaces();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"## FAILED {ex.Message} ##"); logger.Error(ex, "FAILED");
            }

            Console.WriteLine("## genBOE Autoupdater Finished ##"); logger.Debug("Ending a run");
        }

        /// <summary>
        /// Backups the workspace.
        /// </summary>
        /// <param name="wsId">The ws identifier.</param>
        /// <param name="wsState">State of the ws.</param>
        /// <param name="wsVersionsOrdered">The ws versions.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void BackupWorkspace(int wsId, WorkspaceState wsState, ICollection<WorkspaceVersionMetaDataDTO> wsVersionsOrdered)
        {
            try
            {
                WorkspaceVersionMetaDataDTO backup = new WorkspaceVersionMetaDataDTO()
                {
                    VersionID = -1,
                    CreatedByID = CommonConstants.SYSTEM_USER_ID,
                    Updateable = UpdateType.Upsert,
                    VersionName = $"{CommonConstants.AUTO_SYSTEM_BACKUP_DAILY} {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}",
                    VersionState = wsState,
                    WorkspaceID = wsId
                };

                // backup
                Console.WriteLine($"      Backing up WsId: {wsId}.");
                versionLoader.Upsert(backup, backup.WorkspaceID);

                // delete unnecessary versions (we just created one, and we'll keep 1 more)
                wsVersionsOrdered.Skip(1).ToList().ForEach(version =>
                {
                    Console.WriteLine($"      Deleting WsId: {wsId}, VersionId: {version.VersionID}.");
                    versionLoader.Delete(version, wsId);
                });
            }
            catch (Exception ex)
            {
                workspacesWhichFailed.Add(wsId.ToString());
                Console.WriteLine("    !!! FAILED !!!");
                logger.Error(ex, $"BackupWorkspace - Failed for workspace id {wsId}");
            }
        }

        /// <summary>
        /// Processes failed workspaces.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void ProcessFailedWorkspaces()
        {
            string errorText = string.Empty;
            string emailSubject = "GenBOE Auto Emailer Executed";

            if (workspacesWhichFailed.Any())
            {
                errorText = $"The following workspaces failed: {string.Join(", ", workspacesWhichFailed)}";
                emailSubject = "FAILED --" + emailSubject;

                logger.Debug(errorText);
            }
            else if (!emailOnFailureOnly)
            {
                errorText = "There were no issues, all backups completed successfully.";
            }

            if (!string.IsNullOrEmpty(errorText))
            {
                try
                {
                    EmailContent email = new EmailContent() { Subject = emailSubject, Body = errorText };

                    Emailer em = new Emailer();
                    em.SendEmail(email, emailTo, null, new string[0], new string[0], null, new UserData() { Email = emailTo });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ProcessFailedWorkspaces - Failed to send email. Contents: {errorText}");
                }
            }
        }
    }
}