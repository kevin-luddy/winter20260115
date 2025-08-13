namespace GenBOE.ActionLogic._ModelView.Backend
{
	using GenBOE.ActionLogic.ModelView.Backend;
	using System.Collections.Generic;
	using System.Web.Mvc;

	public class ExportReportViewModel
	{
		public ICollection<GeneralReportViewModel> Reports { get; set; }
		public bool IsUsingSummarizeByCustomFieldTemplate { get; set; }
		public bool SupportCustomExport { get; set; }
		public bool IsProjectMapWs { get; set; }
		public bool IsPtmDataOutOfSync { get; set; }
		public ICollection<SelectListItem> SummarizeByCustomFieldOptions { get; set; }
		public ICollection<string> OutOfSyncMessages { get; set; }
		public ICollection<string> WorkspaceAdmins { get; set; }

	}
}
