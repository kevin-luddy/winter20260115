// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using GenBOE.Dtos;

    /// <summary>
    /// SSRS Report Modelview
    /// Inherits Exports Modelview, with report URL
    /// </summary>
    public class SSRSReportsModelView : ExportsModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SSRSReportsModelView()
        {
            this.ReportUrl = new Uri(string.Empty);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="report">Report DTO</param>
        public SSRSReportsModelView(ReportDTO report) : base(report)
        {
            this.ReportUrl = new Uri(string.Empty);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="report">Report DTO</param>
        /// <param name="url">Report URL</param>
        public SSRSReportsModelView(ReportDTO report, Uri url) : base(report)
        {
            this.ReportUrl = url;
        }

        /// <summary>
        /// Gets/Sets the Report URL
        /// </summary>
        public Uri ReportUrl { get; set; }
    }
}
