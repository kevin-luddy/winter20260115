using System;
using IES.Common;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ReportDTO
    {
        public ReportDTO()
        {
            ReportID = -1;
            ReportType = ReportType.Export;
            ReportName = string.Empty;
            Description = string.Empty;
        }

        public int ReportID { get; set; }
        public ReportType ReportType { get; set; }
        public string ReportName { get; set; }
        public string Description { get; set; }
    }
}
