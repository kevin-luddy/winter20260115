<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Calculate Actuals
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/calculateActuals") %>
	<%
		MoqTypeTableDataLabels labels = new MoqTypeTableDataLabels();
	%>
<script type="text/javascript">
	app.value('WorkspaceCalculateActualsModel', {
        workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        controller: '<%:WebConstants.CONTROLLER_WORKSPACE_RECALCULATE_ACTUALS %>',
        action: '<%:WebConstants.ACTION_GET_WORKSPACE_RECALCULATE_ACTUALS_MODEL %>',
		workspaceState: '<%: ((GenBOEMasterModelView)Model).WorkspaceState %>',
		colSpan: <%: (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) ? 8 : 6 %>
     });

	var WorkspaceCalculateActualsWidget;
    $(".main").addClass("wbs");

    $(function () {
		var WorkspaceCalculateActualsWidgetConfig = { ContextID: "WorkspaceCalculateActualss", IsModule: true, isReadOnly: false }; // always mark this as false so we can essentially override the GenListWidget readonly rules
		WorkspaceCalculateActualsWidget = new GenWidget(WorkspaceCalculateActualsWidgetConfig);
    });
</script>

<div data-ng-controller="WorkspaceCalculateActualsController" data-ng-cloak="">
    <div id="WorkspaceCalculateActuals" class="workspace-calculate-actuals module">
        <div class="module-header-data">Calculate Actuals</div>
        <div class="module-content-data">
            <div class="form-row css3pie-position-fix">
                <ul class="validation-box" style="display: none;"></ul>
                <gen-validation data-errors="errors"></gen-validation>
				<div class="float-left">
					<button data-ng-disabled="isLoading" data-ng-click="recalculate()" class="ies-action" type="button">Recalculate</button>
				</div>
                <div class="search-box float-right">
                    <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
                    <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                </div>
            </div>
            <div class="form-row">
               <div class="calculate-actuals-grid">
                    <table id="CalculateActualsGrid" class="grid readonly" width="962">
                        <colgroup>
                            <col/>
                            <col/>
                            <col/>
                            <col width="100px"/>
                            <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) 
								{  %>
							<col />
							<col />
							<%  } %>
							<col/>
							<col/>
                        </colgroup>
                        <thead>
                            <tr>
								<th class="bootstrap"><a data-ng-click="changeSorting(columns.boe)" data-ng-class="{ 'bold': boldSort(columns.boe) }">Boe Title</a></th>
								<th class="bootstrap"><a data-ng-click="changeSorting(columns.task)" data-ng-class="{ 'bold': boldSort(columns.task) }">Task Title</a></th>
								<th class="bootstrap"><a data-ng-click="changeSorting(columns.table)" data-ng-class="{ 'bold': boldSort(columns.table) }">Table Name</a></th>
                                <th class="bootstrap"><a data-ng-click="changeSorting(columns.boeState)" data-ng-class="{ 'bold': boldSort(columns.boePrevState) }">Boe Previous State</a></th>
								 <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) 
								   {  %>
								<th class="bootstrap"><a data-ng-click="changeSorting(columns.wbsHoursPrevious)" data-ng-class="{ 'bold': boldSort(columns.wbsHoursPrevious) }">Previous <%:labels.TotalWbsHours %></a></th>
								<th class="bootstrap"><a data-ng-click="changeSorting(columns.wbsHours)" data-ng-class="{ 'bold': boldSort(columns.wbsHours) }">New <%:labels.TotalWbsHours %></a></th>
								<% } %>
								<th class="bootstrap"><a data-ng-click="changeSorting(columns.hoursPrevious)" data-ng-class="{ 'bold': boldSort(columns.hoursPrevious) }">Previous <%:labels.TotalRelevantHours %></a></th>
								<th class="bootstrap"><a data-ng-click="changeSorting(columns.hours)" data-ng-class="{ 'bold': boldSort(columns.hours) }">New <%:labels.TotalRelevantHours %></a></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr data-ng-show="isLoading"><td colspan="{{colSpan}}"><div class="loader"></div></td></tr>
                            <tr data-ng-show="hasLoaded && !isLoading && (data.length === 0 || filteredResults.length === 0)"><td colspan="{{colSpan}}"><div class="empty-grid-text">There are no updates to Actuals inside MOQ Tables.</div></td></tr>
                            <tr data-ng-repeat="actual in (filteredResults = (data | filter:filterActuals | orderBy:predicate:reverse)) | limitTo:pageSize:currentPage*pageSize">
                                <td class="text" title="{{::actual.BoeTitle}}">{{::actual.BoeTitle}}</td>
                                <td class="text"  title="{{::actual.Task}}">{{::actual.Task}}</td>
                                <td class="text"  title="{{::actual.TableName}}">{{::actual.TableName}}</td>
								<td colspan="{{colSpan - 3}}" data-ng-if="!actual.IsSuccessful">
									<div ng-repeat="message in actual.Messages"><strong>{{::message}}</strong></div>
								</td>
								<td class="text" data-ng-if="actual.IsSuccessful" title="{{::actual.BoePreviousState}}">{{::actual.BoeStatePrevious}}</td>
								<% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) 
								   {  %>
								<td class="text" data-ng-if="actual.IsSuccessful" title="{{::actual.WbsHoursPrevious}}">{{::actual.WbsHoursPrevious}}</td>
								<td class="text" data-ng-if="actual.IsSuccessful" title="{{::actual.WbsHours}}">{{::actual.WbsHours}}</td>
								<% } %>
								<td class="text" data-ng-if="actual.IsSuccessful" title="{{::actual.TotalRelevantHoursPrevious}}">{{::actual.TotalRelevantHoursPrevious}}</td>
								<td class="text" data-ng-if="actual.IsSuccessful" title="{{::actual.TotalRelevantHours}}">{{::actual.TotalRelevantHours}}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="form-row last-form-row">
                <div class="search-box full-width">
                    <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
                    <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                </div>
            </div>
        </div>
    </div>
</div>    

<script type="text/javascript">
    angular.element(document).ready(function () {
        angular.bootstrap(document, ['genboe']);
    });
</script>
</asp:Content>
