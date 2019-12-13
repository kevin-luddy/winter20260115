using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    public class WorkspaceActivityReportModelView
    {
        public string DaysLeftUntilProposalSubmittalDate { get; set; }

        public string DaysInInitialization { get; set; }
        public string DaysInWorking { get; set; }
        public string DaysInLocked { get; set; }
        public string DaysFromInitializationToComplete { get; set; }
        public string DaysFromWorkingToComplete { get; set; }
        public string DaysToClosed { get; set; }

        public string NumberOfTimesInInitialization { get; set; }
        public string NumberOfTimesInWorking { get; set; }
        public string NumberOfTimesInLocked { get; set; }
        public string NumberOfTimesInComplete { get; set; }
        public string NumberOfTimesInClosed { get; set; }
        public string NumberOfTimesExportedToProPricer { get; set; }
               
        public string NumberOfBOEs { get; set; }
        public string NumberOfBOEsUnassigned { get; set; }
        public string NumberOfBOEsDraft { get; set; }
        public string NumberOfBOEsAwaitingApproval { get; set; }
        public string NumberOfBOEsApproved { get; set; }
               
        public string NumberOfAdministrators { get; set; }
        public string NumberOfAuthors { get; set; }
        public string NumberOfApprovers { get; set; }
        public string NumberOfReviewers { get; set; }
        
        public string AverageNumberOfBOEsPerAuthor { get; set; }
    }
}
