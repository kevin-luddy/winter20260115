// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using GenBOE.Dtos;
    public class ExportsModelView
    {
        public ExportsModelView()
        {
            this.ReportName = string.Empty;
            this.Description = string.Empty;
            this.ReportID = 0;
        }

        public ExportsModelView(ReportDTO report) : this()
        {
            if (report != null)
            {
                this.ReportName = report.ReportName;
                this.ReportID = report.ReportID;
                this.Description = report.Description;
            }
        }

        public string ReportName { get; set; }
        public string Description { get; set; }
        public int ReportID { get; set; }
    }
}