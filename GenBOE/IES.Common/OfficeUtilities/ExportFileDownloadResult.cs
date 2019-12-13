// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.OfficeUtilities
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Web.Mvc;
    using System.Threading;

    /// <summary>
    /// Action result for returning export files
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ExportFileDownloadResult : ExportFileDownloadBase
    {
        /// <summary>
        /// Logger
        /// </summary>
        private Logger logger = new Logger(typeof(ExportFileDownloadResult));

        /// <summary>
        /// Constructor
        /// </summary>
        public ExportFileDownloadResult()
            : base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="physicalPath">Path to file</param>
        /// <param name="fileDownloadName">File name</param>
        public ExportFileDownloadResult(string physicalPath, string fileDownloadName)
            : base(physicalPath, fileDownloadName)
        {
        }

        /// <summary>
        /// Execute method for the Action
        /// </summary>
        /// <param name="context">Controller context</param>
        protected override void DoExecuteResult(ControllerContext context)
        {
            if (context != null)
            {
                this.ApplyContentType(context.HttpContext.Response);
                this.ApplyFileName(context.HttpContext.Response);

                try
                {
                    context.HttpContext.Response.TransmitFile(this.PhysicalPath);
                    context.HttpContext.Response.Flush();
                }
                finally
                {
                    // sleep up to 6 seconds trying to delete the file
                    int slept = 0;
                    bool deleted = false;
                    Exception toLog = null;
                    while (!deleted && slept < 6000)
                    {
                        try
                        {
                            File.Delete(this.PhysicalPath);
                            deleted = true;
                        }
                        catch (ArgumentException e)
                        {
                            toLog = e;
                            Thread.Sleep(100);
                            slept += 100;
                            this.logger.Warn("Unable to delete file " + this.PhysicalPath + " due to an invalid argument.  Trying again.  Log Message : " + toLog.Message);
                        }
                        catch (DirectoryNotFoundException e)
                        {
                            toLog = e;
                            Thread.Sleep(100);
                            slept += 100;
                            this.logger.Warn("Unable to delete file " + this.PhysicalPath + " due to the directory not being able to be found.  Trying again.  Log Message : " + toLog.Message);
                        }
                        catch (IOException e)
                        {
                            toLog = e;
                            Thread.Sleep(100);
                            slept += 100;
                            this.logger.Warn("Unable to delete file " + this.PhysicalPath + " due to an IO error.  Trying again.  Log Message : " + toLog.Message);
                        }
                        catch (NotSupportedException e)
                        {
                            toLog = e;
                            Thread.Sleep(100);
                            slept += 100;
                            this.logger.Warn("Unable to delete file " + this.PhysicalPath + "due to an unsupported functionality.  Trying again.  Log Message : " + toLog.Message);
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            toLog = e;
                            Thread.Sleep(100);
                            slept += 100;
                            this.logger.Warn("Unable to delete file " + this.PhysicalPath + " due to an access denial.  Trying again.  Log Message : " + toLog.Message);
                        }

                    }

                    if (slept >= 6000)
                    {
                        this.logger.Error("Unable to delete file " + this.PhysicalPath + ".  GIVING UP!  Log : " + toLog.Message);
                    }
                }
            }
        }
    }
}