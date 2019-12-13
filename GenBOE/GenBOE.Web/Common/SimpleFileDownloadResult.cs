using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using IES.Common;

namespace GenBOE.Web.Common
{
    [ExcludeFromCodeCoverage]
    public class SimpleExportFileDownloadResult : ActionResult
    {
        public const string ContentType_DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string ContentType_XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string ContentType_XLSM = "application/vnd.ms-excel.sheet.macroEnabled.12";
        public const string ContentType_CSV = "text/csv";
        public const string ContentType_ZIP = "application/zip";

        private string _fileDownloadName;

        /// <summary>
        /// Create static Regex object for FileDownload.
        /// </summary>
        private static Regex regexFileDownload = new Regex("[\\\\/:\\*\\?\"<>\\|, ]", RegexOptions.None, Constants.REGEX_TIMEOUT);

        public SimpleExportFileDownloadResult()
            : base()
        {
            this.PhysicalPath = string.Empty;
            this.FileDownloadName = string.Empty;
        }

        public SimpleExportFileDownloadResult(string physicalPath, string fileDownloadName)
            : this()
        {
            this.PhysicalPath = physicalPath;
            this.FileDownloadName = fileDownloadName;
        }

        public string PhysicalPath { get; set; }

        public string FileDownloadName
        {
            get
            {
                return _fileDownloadName;
            }

            set
            {
                _fileDownloadName = regexFileDownload.Replace(value, "_");
            }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context != null)
            {
                if (!String.IsNullOrEmpty(FileDownloadName))
                {
                    context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + this.FileDownloadName);

                    if (FileDownloadName.EndsWith(".docx", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.HttpContext.Response.ContentType = ContentType_DOCX;
                    }
                    else if (FileDownloadName.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.HttpContext.Response.ContentType = ContentType_XLSX;
                    }
                    else if (FileDownloadName.EndsWith(".xlsm", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.HttpContext.Response.ContentType = ContentType_XLSM;
                    }
                    else if (FileDownloadName.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.HttpContext.Response.ContentType = ContentType_CSV;
                    }
                    else if (FileDownloadName.EndsWith(".zip", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.HttpContext.Response.ContentType = ContentType_ZIP;
                    }

                }

                try
                {
                    context.HttpContext.Response.TransmitFile(this.PhysicalPath);
                    context.HttpContext.Response.Flush();
                }
                finally
                {
                    //nothing
                }
            }
        }
    }
}