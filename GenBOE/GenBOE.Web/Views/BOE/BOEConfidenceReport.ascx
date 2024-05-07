<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script type="text/javascript">
	var ConfidenceReport = new Widget("ConfidenceReport");

	$(function () {
		$('#BOEConfidenceReportButton').click(ConfidenceReport.OpenReport);

        if(<%=ViewData["HideConfidenceReport"]%>)
        {
			$('#BOEConfidenceReportButton').hide();
        }
	});

	ConfidenceReport.OpenReport = function () {
		var url = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
			'<%: WebConstants.CONTROLLER_REPORTS %>',
			'<%: WebConstants.ACTION_DISPLAY_BOE_CONFIDENCE_REPORT_RESULTS %>',
			'boe/<%: ViewData["BOEID"] %>');

		window.open(url);
	};
</script>
<button id="BOEConfidenceReportButton" class="ies boe-confidence-report-button" type="button">Confidence Report</button>