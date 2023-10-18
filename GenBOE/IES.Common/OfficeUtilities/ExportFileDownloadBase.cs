// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Action result for returning export files
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ExportFileDownloadBase : ActionResult
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
        /// File name
        /// </summary>
        private string theFileDownloadName;

        /// <summary>
        /// Create static Regex object for FileDownload.
        /// </summary>
        private static Regex regexFileDownload = new Regex("[\\\\/:\\*\\?\"<>\\|, ]", RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Constructor
        /// </summary>
        protected ExportFileDownloadBase()
            : base()
        {
            this.PhysicalPath = string.Empty;
            this.FileDownloadName = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileDownloadName">File name</param>
        protected ExportFileDownloadBase(string fileDownloadName)
            : this()
        {
            this.FileDownloadName = fileDownloadName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="physicalPath">Path to file</param>
        /// <param name="fileDownloadName">File name</param>
        protected ExportFileDownloadBase(string physicalPath, string fileDownloadName)
            : this()
        {
            this.PhysicalPath = physicalPath;
            this.FileDownloadName = fileDownloadName;
        }

        /// <summary>
        /// Path to file
        /// </summary>
        protected string PhysicalPath { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        protected string FileDownloadName
        {
            get
            {
                return this.theFileDownloadName;
            }

            set
            {
                this.theFileDownloadName = regexFileDownload.Replace(value, "_");
            }
        }

        /// <summary>
        /// Derive content type from the file name extension and apply it to the response
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>MIME content type that was applied</returns>
        protected string ApplyContentType(HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            string contentType = GetContentType(this.FileDownloadName);

            response.ContentType = contentType;

            return contentType;
        }

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

        /// <summary>
        /// Set response header for the file name
        /// </summary>
        /// <param name="response">Response</param>
        protected void ApplyFileName(HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            response.AddHeader("Content-Disposition", "attachment; filename=\"" + this.FileDownloadName + "\"");
        }

        /// <summary>
        /// Clear header and header content
        /// </summary>
        /// <param name="response">Response</param>
        protected void ResetHeaders(HttpResponseBase response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            // Clear the headers
            response.ClearHeaders();
            response.ClearContent();

            response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);

            // Unset character set
            response.Charset = string.Empty;
        }

        /// <summary>
        /// Execute method for the Action
        /// </summary>
        /// <param name="context">Controller context</param>
        protected abstract void DoExecuteResult(ControllerContext context);

        /// <summary>
        /// Execute method for the Action
        /// </summary>
        /// <param name="context">Controller context</param>
        public override void ExecuteResult(ControllerContext context)
        {
            this.DoExecuteResult(context);
        }
    }
}