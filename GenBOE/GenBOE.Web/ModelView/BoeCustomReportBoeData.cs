namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;

    public class BoeCustomReportBoeData
    {
        public int BOEID { get; set; }
        public string BoeTitle { get; set; }

        public int? WBSID { get; set; }
        public string WbsDisplayValue { get; set; }
        public string WbsNumber { get; set; }
        public string WbsTitle { get; set; }

        public int? CLINID { get; set; }
        public string ClinDisplayValue { get; set; }
        public string ClinNumber { get; set; }
        public string ClinTitle { get; set; }

        public ICollection<BoeCustomReportAuthorData> Authors { get; set; }
        public ICollection<BoeCustomReportCustomFieldData> CustomFields { get; set; }
    }

    public class BoeCustomReportAuthorData
    {
        public int AuthorUserID { get; set; }
        public string AuthorDisplayValue { get; set; }
        public string AuthorFirstName { get; set; }
        public string AuthorMiddleName { get; set; }
        public string AuthorLastName { get; set; }
        public bool IsSubcontractor { get; set; }
    }

    public class BoeCustomReportCustomFieldData
    {
        public int CustomFieldValueID { get; set; }
        public string CustomFieldValueName { get; set; }
        public string CustomFieldValueDescription { get; set; }
    }
}