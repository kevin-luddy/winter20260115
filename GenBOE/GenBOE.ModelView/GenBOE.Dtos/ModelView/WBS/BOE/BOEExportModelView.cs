// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.ObjectModel;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    public class BOEExportModelView
    {
        public BOEExportModelView()
            : base()
        {
            ProgramName = String.Empty;
            SolicitationNumber = String.Empty;
            WBSTitle = String.Empty;
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            WBSNumber = String.Empty;
            CLINNumber = String.Empty;
            CLINTitle = String.Empty;
            CLINStartDate = DateTime.MinValue;
            CLINEndDate = DateTime.MinValue;
            ContractStartDate = DateTime.MinValue;
            ContractEndDate = DateTime.MinValue;
            ProposalSubmittalDate = null;
            PreparedBy = string.Empty;
            SubmittedDate = string.Empty;
            BOETitle = string.Empty;
            BOEDescription = string.Empty;
            IsMaterial = false;
            IsMultiClinWbs = false;
            ExportFormat = new ExcelReportTemplate();
            TaskElements = new Collection<BOEExportTaskElement>();
            Approvers = new Collection<BoeExportApproverInfo>();
            Authors = new Collection<string>();
            ContainsOCI = false;
            DataSource = string.Empty;
            PaddedWbsName = string.Empty;
            PaddedClinName = string.Empty;
            ExportFields = new Dictionary<string, string>();
            TrackingNumber = string.Empty;
            Category = string.Empty;
            SOWNumber = string.Empty;
            SOWTitle = string.Empty;
            IsProjectMap = false;
            ClassOfCost = string.Empty;
            this.WorkspaceDescription = string.Empty;
        }

        public int BoeID { get; set; }
        public string ProgramName { get; set; }
        public string SolicitationNumber { get; set; }
        public string WBSTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string WBSNumber { get; set; }
        public string CLINNumber { get; set; }
        public string CLINTitle { get; set; }
        public DateTime? CLINStartDate { get; set; }
        public DateTime? CLINEndDate { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? ProposalSubmittalDate { get; set; }
        public string PreparedBy { get; set; }
        public string SubmittedDate { get; set; }
        public string BOETitle { get; set; }
        public string BOEDescription { get; set; }
        public Boolean IsMaterial { get; set; }
        public Boolean IsMultiClinWbs { get; set; }
        public ExcelReportTemplate ExportFormat { get; set; }
        public Collection<BOEExportTaskElement> TaskElements { get; set; }

        // collection of approver info
        public Collection<BoeExportApproverInfo> Approvers { get; set; }

        // collection of author info (NEW - 9/10/13)
        public Collection<string> Authors { get; set; }

        public bool ContainsOCI { get; set; }
        public string DataSource { get; set; }
        public string PaddedWbsName { get; set; }
        public Dictionary<string, string> ExportFields { get; set; }
        public string PaddedClinName { get; set; }
        public string TrackingNumber { get; set; }
        public string Category { get; set; }
        public string SOWNumber { get; set; }
        public string SOWTitle { get; set; }

        public Boolean IsProjectMap { get; set; }
        public string ClassOfCost { get; set; }
        public string WorkspaceDescription { get; set; }
        public string AddDelete { get; set; }
        public string ActivityId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BoeExportApproverInfo
    {
        public BoeExportApproverInfo()
        {
            ApprovedDate = string.Empty;
            ApprovedBy = string.Empty;

        }

        public string ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
    }
}
