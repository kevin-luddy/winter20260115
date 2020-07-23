// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2015 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace APTSPropricerApi
{
    using System;
    using System.IO;
    using System.Text;
    using EBS.ProPricer.Model;
    using EBS.ProPricer.ImportExport;
    using EBS.ProPricer.ImportExport.Ascii;
    using APTSPropricerApi.Connection;
    using EBS.Core;

    /// <summary>
    /// ProPricerProposalExporter handles exporting task and resource data directly to a ProPricer proposal.
    /// </summary>
    public class ProPricerProposalExporter
    {
        #region private fields / constants

        /// <summary>
        /// Logger
        /// </summary>
        private Logger logger = new Logger(typeof(ProPricerProposalExporter));

        /// <summary>
        /// File extension name for the task export file.
        /// </summary>
        private const string TASK_EXPORT_FILE_EXTENSION = "tsk";

        /// <summary>
        /// File extension name for the resource export file.
        /// </summary>
        private const string RESOURCE_EXPORT_FILE_EXTENSION = "res";

        /// <summary>
        /// When errors occur with and export, ProPricer creates an exception file with a .exc extension. This file
        /// contains a list of export records that are in error. It needs to be mapped to a corresponding .log file.
        /// </summary>
        private const string PROPRICER_ERROR_FILE_EXTENSION = "exc";

        /// <summary>
        /// When errors occur with an export, ProPricer creates an exception file with a .exc extension. This file
        /// contains a list of export records that are in error. It needs to be mapped to a corresponding .log file. The
        /// log file contains the actual error message. So record 1 in exc file maps to record 1 in the log file.
        /// </summary>
        private const string PROPRICER_LOG_FILE_EXTENSION = "log";

        #endregion

        /// <summary>
        /// Exports genBOE task and resource data to a ProPricer proposal.
        /// </summary>
        /// <param name="inTempPathFile">Temporary file path to store task and resoure export files.</param>
        /// <param name="proposalId">Id of the Proposal to export to.</param>
        /// <param name="instanceId">PP Instance Id</param>
        /// <param name="taskData">task data to export.</param>
        /// <param name="resourceData">resource data to export.</param>
        /// <param name="taskExportOption">Task data export option.</param>
        /// <param name="resourceExportOption">Resource Hours/Cost export option.</param>
        /// <returns>The results of the Task and resource export.</returns>
        public ProPricerExportResults ExportToProPricerProposal(string inTempPathFile, string proposalId, int instanceId, 
            string taskData, string resourceData, ProPricerExportOption taskExportOption, 
            ProPricerExportOption resourceExportOption)
        {
            ProPricerExportResults exportResults = new ProPricerExportResults();

            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                Guid id = new Guid(proposalId);
                Proposal proposal = ppc.Workspace.Proposals.Find(id).Value();
                if (proposal != null)
                {
                    proposal.Open();
                    string taskFileName = CreateTemporaryExportFile(inTempPathFile, TASK_EXPORT_FILE_EXTENSION, taskData);
                    string exceptionFileName = GetExceptionOrLogFileName(taskFileName, PROPRICER_ERROR_FILE_EXTENSION);
                    exportResults.TaskExportResults = ExportTasksToProposal(proposal, taskFileName, exceptionFileName, taskExportOption);

                    // Export resources. This needs to be done after the Tasks due to dependency.
                    string resourceFileName = CreateTemporaryExportFile(inTempPathFile, RESOURCE_EXPORT_FILE_EXTENSION, resourceData);
                    exceptionFileName = GetExceptionOrLogFileName(resourceFileName, PROPRICER_ERROR_FILE_EXTENSION);
                    exportResults.ResourceExportResults = ExportResourcesToProposal(proposal, resourceFileName, exceptionFileName, resourceExportOption);
                    proposal.Close();
                }
                else
                {
                    throw new ArgumentException("Proposal was not found or could not be opened in the workspace.", "proposalId");
                }
            }

            return exportResults;
        }

        #region Private Helpers

        /// <summary>
        /// Exports genBOE task data to a ProPricer ProPricer proposal.
        /// </summary>
        /// <param name="proposal">ProPricer Proposal.</param>
        /// <param name="taskExportFileName">Path to task file to be exported.</param>
        /// <param name="exceptionFileName">Name of the file to contain exceptions for exports. This file along with a .log file with
        /// the same name and path can be use to report errors.</param>
        /// <param name="taskExportOption">Task export option.</param>
        /// <returns>The results of the export.</returns>
        private ProPricerExportResult ExportTasksToProposal(Proposal proposal, string taskExportFileName, string exceptionFileName, ProPricerExportOption taskExportOption)
        {
            ProPricerExportResult exportResults = new ProPricerExportResult();

            try
            {
                // Instantiate, Initialize and Execute export using ProPricer ASCII "Task Data" import API.
                TaskDataImport taskDataImport = new TaskDataImport(proposal.Tasks);

                if (taskExportOption == ProPricerExportOption.ReplaceAllExisting)
                {
                    taskDataImport.ReplaceAllExisting = true;
                }
                else
                {
                    taskDataImport.DuplicateOption = this.ConvertExportOption(taskExportOption);
                }

                taskDataImport.FileName = taskExportFileName;
                taskDataImport.ExceptionFilePath = exceptionFileName;
                taskDataImport.IgnoreFirstLine = false;
                taskDataImport.Delimiter = ',';
                taskDataImport.Process();
                exportResults = GetExportResults(taskDataImport.Statistics, exceptionFileName, this.GetExceptionOrLogFileName(taskExportFileName, PROPRICER_LOG_FILE_EXTENSION));
            }
            finally
            {
                // Clean up export files.
                DeleteFile(taskExportFileName);
                DeleteFile(exceptionFileName);
                DeleteFile(GetExceptionOrLogFileName(taskExportFileName, PROPRICER_LOG_FILE_EXTENSION));
            }

            return exportResults;
        }

        /// <summary>
        /// Exports genBOE task data to a ProPricer ProPricer proposal.
        /// </summary>
        /// <param name="proposal">ProPricer Proposal.</param>
        /// <param name="resourceExportFile">Path to resource file to be exported.</param>
        /// <param name="exceptionFileName">Name of the file to contain exceptions for exports. This file along with a .log file with
        /// the same name and path can be use to report errors.</param>
        /// <param name="resourceExportOption">Task export option.</param>
        /// <returns>The results of the export.</returns>
        private ProPricerExportResult ExportResourcesToProposal(Proposal proposal, string resourceExportFile, string exceptionFileName, ProPricerExportOption resourceExportOption)
        {
            ProPricerExportResult exportResults = new ProPricerExportResult();

            try
            {
                // Instantiate, Initialize and Execute export using ProPricer ASCII "Resource Hours/Cost" import API.
                ResourceHoursCostImport hoursCostImport = new ResourceHoursCostImport(proposal.Tasks);

                if (resourceExportOption == ProPricerExportOption.ReplaceAllExisting)
                {
                    hoursCostImport.ReplaceAllExisting = true;
                }
                else
                {
                    hoursCostImport.DuplicateOption = this.ConvertExportOption(resourceExportOption);
                }
                
                hoursCostImport.Delimiter = ',';
                hoursCostImport.FileName = resourceExportFile;
                hoursCostImport.ExceptionFilePath = exceptionFileName;
                hoursCostImport.IgnoreFirstLine = false;
                hoursCostImport.Process();
                exportResults = GetExportResults(hoursCostImport.Statistics, exceptionFileName, this.GetExceptionOrLogFileName(resourceExportFile, PROPRICER_LOG_FILE_EXTENSION));
            }
            finally
            {
                DeleteFile(resourceExportFile);
                DeleteFile(exceptionFileName);
                DeleteFile(GetExceptionOrLogFileName(resourceExportFile, PROPRICER_LOG_FILE_EXTENSION));
            }

            return exportResults;
        }

        /// <summary>
        /// Converts a ProPricerExportOption to an EBS.ProPricer.ImportExport.DuplicateOption that is required for the export.
        /// </summary>
        /// <param name="exportOption">ProPricerExportOption</param>
        /// <returns>Converted DuplicateOption.</returns>
        private DuplicateOption ConvertExportOption(ProPricerExportOption exportOption)
        {
            DuplicateOption convertedExportOption = DuplicateOption.Skip;

            switch (exportOption)
            {
                case (ProPricerExportOption.OverwriteDuplicates):
                {
                    // TODO: What is the difference between Overwrite, SafeOverwrite, and ReleaseOverwrite?
                    // Overwrites duplicate records.
                    convertedExportOption = DuplicateOption.Overwrite;
                    break;
                }
                case (ProPricerExportOption.DoNotOverwriteDuplicates):
                {
                    // TODO: Does DuplicateOption.Skip equate to Do not overwrite???
                    convertedExportOption = DuplicateOption.Skip;
                    break;
                }
                case (ProPricerExportOption.AddValueToDuplicate):
                {
                    // Accumulates resource hours if a resource already exists.
                    convertedExportOption = DuplicateOption.AddToExisting;
                    break;
                }
            }

            return convertedExportOption;
        }

        /// <summary>
        /// Creates a temporary physical file from a memory stream. The ProPricer API requires a path to a physical file for export. This file
        /// should be deleted after the export completes.
        /// </summary>
        /// <param name="folderPath">Path to the folder for storing the physical file.</param>
        /// <param name="fileExtension">File extension to use.</param>
        /// <param name="data">The data to write out.</param>
        /// <returns>The fully qualified path to the file.</returns>
        private string CreateTemporaryExportFile(string folderPath, string fileExtension, string data)
        {
            // Create a new unique file name for saving the data.
            string fullFileName = Path.GetDirectoryName(folderPath) + "\\" + Path.GetRandomFileName() + "." + fileExtension;
            File.WriteAllText(fullFileName, data);

            return fullFileName;
        }

        /// <summary>
        /// Gets a user readable string that is the result of an export.
        /// </summary>
        /// <param name="importStats">EBS import statistics object.</param>
        /// <param name="exceptionFileName">Exception File Name</param>
        /// <param name="logFileName">Log File Name</param>
        /// <returns>Export results string.</returns>
        private ProPricerExportResult GetExportResults(Statistics importStats, string exceptionFileName, string logFileName)
        {
            ProPricerExportResult exportResults = new ProPricerExportResult
            {
                TotalRecordsInException = importStats.Exceptioned,
                TotalRecordsInImportFile = importStats.All,
                TotalRecordsSkipped = importStats.Skipped,
                TotalRecordsSuccessfullyExported = importStats.Imported
            };

            if (exportResults.TotalRecordsInException > 0)
            {
                exportResults.ProcessExceptionFile(exceptionFileName, logFileName);
            }

            return exportResults;
        }

        /// <summary>
        /// Gets a user readable string that is the result of an export.
        /// </summary>
        /// <param name="importStats">EBS import statistics object.</param>
        /// <returns>Export results string.</returns>
        private string GetImportStatistics(Statistics importStats)
        {
            StringBuilder stats = new StringBuilder();
            stats.Append("IMPORTING COMPLETED" + Environment.NewLine + Environment.NewLine);
            stats.Append("Records in the Import File:       " + importStats.All + Environment.NewLine);
            stats.Append("Records in the Exception File:    " + importStats.Exceptioned + Environment.NewLine);
            stats.Append("Duplicates Skipped as Requested:  " + importStats.Skipped + Environment.NewLine + Environment.NewLine);
            stats.Append("Records Successfully Imported:    " + importStats.Imported + Environment.NewLine);
            return stats.ToString();
        }

        /// <summary>
        /// Gets the name of the .exc or .log file used for writing errors that occur during the export process. The .exc file
        /// contains a list export records that were in error and the .log file contains a list of corresponding error messages 
        /// for the records in error. The file names will be the same as the export file name with .exc or .log extensions.
        /// </summary>
        /// <param name="baseFileName">The export file name.</param>
        /// <param name="fileExtension">File extension string.</param>
        /// <returns>Name of .exc or log file.</returns>
        private string GetExceptionOrLogFileName(string baseFileName, string fileExtension)
        {
            string exceptionFileName = string.Empty;

            if (baseFileName == null)
            {
                throw new ArgumentNullException("baseFileName");
            }
            exceptionFileName = String.Format("{0}{1}{2}{3}", Path.GetDirectoryName(baseFileName), @"\", Path.GetFileNameWithoutExtension(baseFileName), "." + fileExtension);
            return exceptionFileName;
        }

        /// <summary>
        /// Deletes temporary files created by the export process.
        /// </summary>
        /// <param name="fileName">Fully qualified file name.</param>
        private void DeleteFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(String.Format("Failed to delete ProPricer export file: {0} Exception: {1}", fileName, ex.Message));
            }
        }

        #endregion
    }
}