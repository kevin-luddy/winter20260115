// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;

    /// <summary>
    /// Action result for streaming export files directly to the browser
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ExportFileDownloadStream : ExportFileDownloadBase
    {
        /// <summary>
        /// Contents of the file
        /// </summary>
        private byte[] theFileContent;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExportFileDownloadStream()
            : base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileDownloadName">File name</param>
        /// <param name="fileContent">Contents of the file</param>
        public ExportFileDownloadStream(string fileDownloadName, byte[] fileContent)
            : base(fileDownloadName)
        {
            this.theFileContent = fileContent;
        }

        /// <summary>
        /// Execute method for the Action
        /// </summary>
        /// <param name="context">Controller context</param>
        protected override void DoExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this.ResetHeaders(context.HttpContext.Response);
            this.ApplyContentType(context.HttpContext.Response);
            this.ApplyFileName(context.HttpContext.Response);

            context.HttpContext.Response.OutputStream.Write(this.theFileContent, 0, this.theFileContent.Length);
            context.HttpContext.Response.Flush();
        }
    }
}