// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using IES.Common;

    /// <summary>
    /// Houses information for Downloading Reports
    /// </summary>
    public class DownloadReportModelView
    {
        /// <summary>
        /// Gets or sets the report identifier.
        /// </summary>
        public Reports ReportType { get; set; }

        /// <summary>
        /// Gets or sets the format type of the report (word, pdf, excel)
        /// </summary>
        public string FormatType { get; set; }

        /// <summary>
        /// Gets or sets the SSRS Link.
        /// </summary>
        public string SSRSLink { get; set; }
    }
}
