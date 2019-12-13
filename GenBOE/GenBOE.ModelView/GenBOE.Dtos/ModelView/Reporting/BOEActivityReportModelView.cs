using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    public class BOEActivityReportModelView
    {
        public string DaysLeftUntilProposalSubmittalDate { get; set; }

        public Collection<BOEActivityReportRow> Rows { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BOEActivityReportRow
    {
        public string WBSNum { get; set; }
        public string WBSNumPadded { get; set; }
        public string WBSTitle { get; set; }
        public string BOETitle { get; set; }
        public string CLINNum { get; set; }
        public string CLINTitle { get; set; }
        public string Authors { get; set; }
        public string Status { get; set; }

        public string DaysInUnassigned { get; set; }
        public string DaysInDraft { get; set; }
        public string DaysInAwaitingApproval { get; set; }
        public string DaysFromCreatedToApproved { get; set; }
        public string DaysFromDraftToApproved { get; set; }

        public string NumTimesInDraft { get; set; }
        public string NumTimesInAwaitingApproval { get; set; }
        public string NumTimesInApproved { get; set; }
        public string NumTimesAuthorReassigned { get; set; }
    }
}
