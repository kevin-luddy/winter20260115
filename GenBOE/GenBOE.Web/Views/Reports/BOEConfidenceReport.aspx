<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Confidence Report - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/confidenceReport") %>	
	<% var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };%>
	<script type="text/javascript">
		app.value('ConfidenceReportModel', {
			workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
			model: <%=serializer.Serialize(Model)%>,
			boeController: '<%: WebConstants.CONTROLLER_BOE %>',
			editBoeAction: '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>',
			reportsController: '<%: WebConstants.CONTROLLER_REPORTS %>',
			exportConfidenceReportAction: '<%: WebConstants.ACTION_EXPORT_CONFIDENCE_REPORT %>',
			boeId: '<%=ViewData["BOEID"]%>'
		});

		var ConfidenceReport;

		$(function () {
			var ConfidenceReportConfig = { ContextID: "ConfidenceReport", IsModule: true, isReadOnly: false };
			ConfidenceReport = new GenWidget(ConfidenceReportConfig);
		});
	</script>
	<div data-ng-app="genboe" data-ng-controller="BOEConfidenceReportController">
		<div id="BOEConfidenceReport" class="module">
			<div class="module-header-data">Confidence Report</div>
			<div class="module-content-data">
				<div class="form-row confidence-score">
					<h2>
						Confidence Score: {{confidenceScore}}
					</h2>
					<div>
						<button class="ies-action" id="Export-ManageBOE" data-ng-disabled="isExporting" data-ng-click="exportConfidenceReport()" type="button">Excel Export</button>
					</div>
					<div class="search-box float-right" data-ng-if="displayErrors">
						<input type="search" class="filter" data-ng-model="search.text" data-ng-model-options="{ debounce: 200 }" placeholder="Search..." />
					</div>
					<br />
				</div>
				<div data-ng-if="displayErrors == false">
					There are no Tasks with errors.
				</div>
				<div data-ng-if="displayErrors">
					<div class="form-row">
						<table id="BOEConfidenceReportGrid" class="confidence-report-table grid readonly full">
							<thead>
								<tr>
									<th class="confidence-report-wbs-num">
										<a data-ng-click="sort(columns.wbsNumber)" data-ng-class="{ 'bold': boldSort(columns.wbsNumber) }">WBS #</a>
									</th>
									<th class="confidence-report-wbs-title">
										<a data-ng-click="sort(columns.wbsTitle)" data-ng-class="{ 'bold': boldSort(columns.wbsTitle) }">WBS Title</a>
									</th>
									<th class="confidence-report-boe">
										<a data-ng-click="sort(columns.boeTitle)" data-ng-class="{ 'bold': boldSort(columns.boeTitle) }">BOE Title</a>
									</th>
									<th class="confidence-report-task">
										<a data-ng-click="sort(columns.taskTitle)" data-ng-class="{ 'bold': boldSort(columns.taskTitle) }">Task Title</a>
									</th>
									<th class="confidence-report-moqtypes">
										<a data-ng-click="sort(columns.moqTypes)" data-ng-class="{ 'bold': boldSort(columns.moqTypes) }">MOQ Types</a>
									</th>
									<th class="confidence-report-rtefields">
										<a data-ng-click="sort(columns.rteFields)" data-ng-class="{ 'bold': boldSort(columns.rteFields) }">RTE Fields</a>
									</th>
									<th class="confidence-report-errors">
										<a data-ng-click="sort(columns.errors)" data-ng-class="{ 'bold': boldSort(columns.errors) }">Error Messages</a>
									</th>
								</tr>
							</thead>
							<tbody>
								<tr data-ng-show="(data.length === 0 || filteredResults.length === 0)"><td colspan="5">There are no Tasks matching this search.</td></tr>
								<tr data-ng-repeat="item in (filteredResults = (data | filter:filterItems | orderBy:predicate:reverse))">
									<td class="confidence-report-wbs-num">{{item.WbsNumber}}</td>
									<td class="confidence-report-wbs-title">{{item.WbsTitle}}</td>
									<td class="confidence-report-boe">
										<a data-ng-click="openBoe(item.BoeId)">
											{{item.BoeTitle == "" ? noTitle : item.BoeTitle}}
										</a>
									</td>
									<td class="confidence-report-task">
										<a data-ng-click="openTask(item.BoeId, item.TaskId)">
											{{item.TaskTitle}}
										</a>
									</td>
									<td class="confidence-report-moqtypes">{{item.MoqTypes}}</td>
									<td class="confidence-report-rtefields">{{item.RteFields}}</td>
									<td class="confidence-report-errors">{{item.ErrorText}}</td>
								</tr>
							</tbody>
						</table>
					</div>
				</div>
			</div>
		</div>
	</div>
</asp:Content>