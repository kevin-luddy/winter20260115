<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.Backend.GeneralReportViewModel>>" %>

<%
	bool enableReports = (bool)ViewData["EnableReports"];
%>

<script type="text/javascript">

	var GeneralReportsWidget = new GridWidget("GeneralReports");

	$(function () {
		createModule($('.general-reports.module'));

		$('#GeneralReports a[name="GeneralReports-ViewButton"]').click(function () {
			var data = {};
			data.reportID = $(this).parents('tr').attr('pkid');
			data.reportName = 'General Reports';
			switch (data.reportID) {
				case '<%: (int)Reports.BOEStatus %>':
					data.reportName += ' - BOE Status';
					break;
				case '<%: (int)Reports.BOEActivity %>':
					data.reportName += ' - BOE Activity';
					break;
				case '<%: (int)Reports.WorkspaceActivity %>':
					data.reportName += ' - Workspace Activity';
					break;
				case '<%: (int)Reports.BoeDiscrepancy %>':
					data.reportName += ' - BOE Discrepancy';
					break;
				case '<%: (int)Reports.ValidateAllBOE %>':
					data.reportName += ' - Validate All BOEs';
					break;
				case '<%: (int)Reports.ConfidenceReport %>':
					data.reportName += ' - Confidence Report';
					break;
				default:
					data.reportName += ' - SSRS Reports';
					break;
			}

			$(document).trigger('<%: WebConstants.EVENT_REPORTS_VIEW_REPORT %>', data);
		});

		refreshModule($('.general-reports.module'));
		CollapsibleModule($('.general-reports.module'));
	});

</script>

<div id="GeneralReports" class="general-reports module expanded inner-collapsible">
	<div class="module-header-data">General Reports</div>

	<div class="module-content-data expanded-content">
		<table class="grid readonly">
			<thead>
				<tr>
					<th class="report">Report</th>
					<th class="description" style="width: 660px;">Description</th>
					<th class="last-child action" style="width: 42px;">View</th>
				</tr>
			</thead>
			<tbody>
				<% foreach (var item in Model) { %>
					<tr pkid="<%: item.ReportID %>" reportname="<%: item.ReportName %>">
						<td><span><%: item.ReportName %></span></td>
						<td><span><%: item.Description %></span></td>
						<% if (enableReports) { %>
							<td><span><a name="GeneralReports-ViewButton">View</a></span></td>
						<% } else { %>
						    <td title="There are Children objects (i.e. CLIN, BOE, Task, Resource) that are outside the PoP of their Parents">
								<span>Invalid</span>
							</td>
						<% } %>
				</tr>
				<% } %>
			</tbody>
		</table>
	</div>
</div>
