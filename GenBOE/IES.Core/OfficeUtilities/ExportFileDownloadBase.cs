// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core.OfficeUtilities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
	using IES.Core;

	/// <summary>
	/// Action result for returning export files
	/// </summary>
	[ExcludeFromCodeCoverage]
    public abstract class ExportFileDownloadBase
    {
        /// <summary>
        /// MIME content type for Word documents
        /// </summary>
        public const string ContentType_DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        /// <summary>
        /// MIME content type for standard Excel (XLSX) documents
        /// </summary>
        public const string ContentType_XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// MIME content type for macro-enabled Excel (XLSM) documents
        /// </summary>
        protected const string ContentType_XLSM = "application/vnd.ms-excel.sheet.macroEnabled.12";

        /// <summary>
        /// MIME content type for CSV files
        /// </summary>
        protected const string ContentType_CSV = "text/csv";

        /// <summary>
        /// MIME content type for ZIP files
        /// </summary>
        protected const string ContentType_ZIP = "application/zip";

        /// <summary>
        /// MIME content type for JSON files
        /// </summary>
        protected const string ContentType_JSON = "application/json";

        /// <summary>
        /// Create static Regex object for FileDownload.
        /// </summary>
        private static Regex regexFileDownload = new Regex("[\\\\/:\\*\\?\"<>\\|, ]", RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Gets the content type based on the filename
        /// </summary>
        /// <param name="fileName">File to get content type against</param>
        /// <returns>Content Type of the file</returns>
        public static string GetContentType(string fileName)
        {
            string contentType = "text/plain";

            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.EndsWith(".docx", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentType = ContentType_DOCX;
                }
                else if (fileName.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentType = ContentType_XLSX;
                }
                else if (fileName.EndsWith(".xlsm", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentType = ContentType_XLSM;
                }
                else if (fileName.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentType = ContentType_CSV;
                }
                else if (fileName.EndsWith(".zip", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentType = ContentType_ZIP;
                }
                else if (fileName.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase))
                {
                    contentType = ContentType_JSON;
                }
            }

            return contentType;
        }
    }
}