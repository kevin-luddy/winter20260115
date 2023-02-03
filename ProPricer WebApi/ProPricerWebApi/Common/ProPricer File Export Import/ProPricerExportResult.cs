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
    }
}
