// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace APTSPropricerApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Contains the export results of a ProPricer task or resource export.
    /// </summary>
    public class ProPricerExportResult
    {
        /// <summary>
        /// Logger for logging error messages.
        /// </summary>
        private Logger logger = new Logger(typeof(ProPricerExportResult));

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProPricerExportResult()
        {
            this.TaskIdErrorMessages = new Dictionary<string, string>();
            this.TotalRecordsInException = 0;
            this.TotalRecordsInImportFile = 0;
            this.TotalRecordsSkipped = 0;
            this.TotalRecordsSuccessfullyExported = 0;
            this.ProcessExceptionFileError = string.Empty;
        }

        /// <summary>
        /// A dictionary of exported Task Id and associated error message if the entity failed to export.
        /// </summary>
        public Dictionary<string, string> TaskIdErrorMessages { get; set; }

        /// <summary>
        /// Total number of records we attempted to export.
        /// </summary>
        public int TotalRecordsInImportFile { get; set; }

        /// <summary>
        /// Total number of records that faild to export.
        /// </summary>
        public int TotalRecordsInException { get; set; }

        /// <summary>
        /// This may be meaningless in our export context. It could mean records skipped by the user during an
        /// interactive import session.
        /// </summary>
        public int TotalRecordsSkipped { get; set; }

        /// <summary>
        /// Total number of records that were successfully exported.
        /// </summary>
        public int TotalRecordsSuccessfullyExported { get; set; }

        /// <summary>
        /// If an exception occurs during the log file processing, report this to the user.
        /// </summary>
        public string ProcessExceptionFileError { get; set; }

        /// <summary>
        /// Processes errors for the exception and log files. Theses files have corresponding 1:1 record where the exception file
        /// contains records we attempted to export and the log file contains error messages resulting in the failed export of
        /// those records.
        /// </summary>
        /// <param name="exceptionFile">Fully qualified path to the exception file.</param>
        /// <param name="logFile">Fully qualified path to the log file.</param>
        public void ProcessExceptionFile(string exceptionFile, string logFile)
        {
            if (File.Exists(exceptionFile) && File.Exists(logFile))
            {
                StreamReader exceptionFileHandle = null;
                StreamReader logFileHandle = null;
                try
                {
                    exceptionFileHandle = new StreamReader(exceptionFile);
                    logFileHandle = new StreamReader(logFile);
                    string exceptionRecord;
                    while ((exceptionRecord = exceptionFileHandle.ReadLine()) != null)
                    {
                        // exceptionRecord is one of the export records from the csv file we tried to export. The
                        // first field is the TaskId. Strip it out of the record.
                        string taskId = exceptionRecord.Remove(exceptionRecord.IndexOf(","));
                        string errorMessage;
                        // Get the associated error message for the above task.
                        if ((errorMessage = logFileHandle.ReadLine()) != null)
                        {
                            if (!TaskIdErrorMessages.ContainsKey(taskId))
                            {
                                TaskIdErrorMessages.Add(taskId, errorMessage);
                            }
                        }
                        else
                        {
                            // There should be a 1:1 corresponding records in each file. If we get here, that was 
                            // not the case, so just break out.
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    StringBuilder error = new StringBuilder(ex.Message);
                    error.Append(" Please Contact your system administrator. ");
                    this.ProcessExceptionFileError = error.ToString();
                    error.Append(ex.StackTrace);
                    this.logger.Error(error.ToString());
                }
                finally
                {
                    if (exceptionFileHandle != null)
                    {
                        exceptionFileHandle.Close();
                    }
                    if (logFileHandle != null)
                    {
                        logFileHandle.Close();
                    }
                }
            }
        }
    }
}
