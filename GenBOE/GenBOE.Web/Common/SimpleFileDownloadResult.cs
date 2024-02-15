using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using IES.Common;
using IES.Common.OfficeUtilities;

namespace GenBOE.Web.Common
{
    [ExcludeFromCodeCoverage]
    public class SimpleExportFileDownloadResult : ActionResult
    {
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

                    context.HttpContext.Response.ContentType = ExportFileDownloadBase.GetContentType(FileDownloadName);
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