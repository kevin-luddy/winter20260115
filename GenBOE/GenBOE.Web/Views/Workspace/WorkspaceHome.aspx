<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%--HomeWorkspaceGridModelView--%>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	genBOE - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/workspace") %>
    <script type="text/javascript">
        $(function () {
            // pulls out the error message from the query..
            var errorMessage = sessionStorage.errorMessage;
            if (errorMessage) {
                $('#errorMessage').addClass('validation-box').css('display', 'block').html(errorMessage);
            }
            sessionStorage.errorMessage = '';

            $(".main").addClass("workspace");
        });

    </script>

    <script type="text/javascript">
        app.value('WorkspaceHomeModel', {
            workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            controller: '<%:WebConstants.CONTROLLER_WORKSPACE %>',
            action: '<%:WebConstants.ACTION_GET_WORKSPACE_HOME_MODEL %>',
            boeController: '<%: WebConstants.CONTROLLER_BOE %>',
            editAction: '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>',
            workspaceState: '<%: ((GenBOEMasterModelView)Model).WorkspaceState %>',
            draftState: '<%: (int)BOEState.Draft %>',
            awaitingState: '<%: (int)BOEState.AwaitingApproval %>',
            approvedApprover: '<%: (int)ApproverReponseType.Approved %>'
        });
    </script>

    <div id="workspace-home" data-ng-controller="WorkspaceHomeController" data-ng-cloak="">
        <div class="help-module module" id="helpmodule">
           <div class="module-header-data">Getting Started with the Workspace
                <div class="small-close-button" onclick="HideHelp()" title="You can use the Help menu to show Getting Started again"> Hide</div>
           </div>
            <div class="module-content-data">
                <% try
                {
                    Html.RenderAction(WebConstants.ACTION_DISPLAY_WORKSPACE_HOME_HELP,
                        WebConstants.CONTROLLER_WORKSPACE,
                        new { workspace = SiteMasterUtilities.GetCurrentWorkspace() });
                }
                catch 
                { 
                    // no permissions ... just means this person is not an admin
                    // hide the div but don't send a message to the controller and have it commit something
                    // to the database
                %>
                    <script type="text/javascript">
                        $("#helpmodule").hide();
                    </script>
                <% } %>
            </div>
        </div>

        <div class="module workspace-home" id="boesmodule">
            <div class="module-header-data">BOEs</div>
            <div class="module-content-data">
                <div class="form-row css3pie-position-fix">

                    <div id="errorMessage"></div>

                    <div id="noteMessage" data-ng-show="isDataFiltered()">You are viewing filtered data. <a data-ng-click="clearAllFilters()">Click here</a> to reset all your filters.</div>

                    <div id="WorkspaceHome-SearchRow" class="search-box float-right">
                        <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right">
                        <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                    </div>
                </div>
                <div id="QuickFilters" class="form-row css3pie-position-fix" data-ng-hide="isLoading">
                    Quick Action Filters:
                    <span><a data-ng-click="assignedAsAuthor()">Assigned as Author</a></span>
                    <span><a data-ng-click="assignedAsApprover()">Assigned as Approver</a></span>
                    <span><a data-ng-click="showWaiting()">Show Only Awaiting Approval</a></span>
                    <span><a data-ng-click="sortRecent()">Sort Most Recently Updated</a></span>
                </div>
                <div class="form-row">
                    <div class="workspace-home-grid">
                        <table id="WorkspaceHomeGrid" class="grid readonly">
                            <thead>
                                <tr>
                                    <th class="wbs-title bootstrap"><a data-ng-click="changeSorting(columns.wbs)" data-ng-class="{ 'bold': boldSort(columns.wbs) }">Work Breakdown Structure (WBS)</a>
                                        <a data-ng-click="toggleFilter(columns.wbs)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                    </th>
                                    <th class="boe-title bootstrap"><a data-ng-click="changeSorting(columns.boe)" data-ng-class="{ 'bold': boldSort(columns.boe) }">BOE Title</a>
                                        <a data-ng-click="toggleFilter(columns.boe)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                    </th>
                                    <th class="clin-title bootstrap"><a data-ng-click="changeSorting(columns.clin)" data-ng-class="{ 'bold': boldSort(columns.clin) }">Contract Line Item Number (CLIN)</a>
                                        <a data-ng-click="toggleFilter(columns.clin)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                    </th>
                                    <th class="author-name bootstrap"><a data-ng-click="changeSorting(columns.authors)" data-ng-class="{ 'bold': boldSort(columns.authors) }">Authors</a>
                                        <a data-ng-click="toggleFilter(columns.authors)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                    </th>
                                    <th class="approver-name bootstrap"><a data-ng-click="changeSorting(columns.approvers)" data-ng-class="{ 'bold': boldSort(columns.approvers) }">Approvers</a>
                                        <a data-ng-click="toggleFilter(columns.approvers)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                    </th>
                                    <th class="status-value bootstrap"><a data-ng-click="changeSorting(columns.status)" data-ng-class="{ 'bold': boldSort(columns.status) }">Status</a>
                                        <a data-ng-click="toggleFilter(columns.status)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                    </th>
                                    <th data-ng-if="isWorkspaceLocked" class="bootstrap" style="width: 25px;">State</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr data-ng-show="isLoading"><td colspan="{{colSpan}}"><div class="loader"></div></td></tr>
                                <tr data-ng-show="!isLoading && (data.length === 0 || filteredResults.length === 0)"><td colspan="{{colSpan}}"><div class="empty-grid-text">There are no BOEs for the Workspace.</div></td></tr>
                                <tr data-ng-repeat="boe in (filteredResults = (data | filter:filterBOEs)) | orderBy:predicate:reverse | limitTo:pageSize:currentPage*pageSize">
                                    <td><a title="{{::boe.WBSText}}" href="{{::boe.url}}">{{::boe.WBSText}}</a></td>
                                    <td><a title="{{::boe.BOETitle}}" href="{{::boe.url}}">{{::boe.BOETitle}}</a></td>
                                    <td><a title="{{::boe.CLINText}}" href="{{::boe.url}}">{{::boe.CLINText}}</a></td>
                                    <td>
                                        <div data-ng-repeat="authorName in boe.AuthorsName">{{::authorName}}</div>
                                    </td>
                                    <td>
                                        <div data-ng-repeat="approver in boe.Approvers" class="{{ ::getApproverStatus(boe.State, approver) }}">
                                            {{::approver.DisplayName}}
                                        </div>
                                    </td>
                                    <td>{{::boe.Status}}</td>
                                    <td data-ng-if="isWorkspaceLocked" data-ng-switch="boe.State">
                                        <img data-ng-switch-when="2" src="/Resources/css/images/unlock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" />
                                        <img data-ng-switch-when="6" src="/Resources/css/images/lock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" />
                                        <img data-ng-switch-when="3" src="/Resources/css/images/lock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" />
                                        <img data-ng-switch-when="4" src="/Resources/css/images/lock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="form-row last-form-row">
                    <div id="WorkspaceHome-SearchRow2" class="search-box full-width">
                        <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" placeholder="Search..." style="float: right">
                        <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                    </div>
                </div>
            </div>
        </div>

        <div gen-dialog id="FilterOptions" class="dialog form" data-width="400" data-height="300" data-title="Apply Filters" data-open="filter.open" data-on-close="onClose()">
            <div class="options wbs" data-ng-show="filter.filterColumn === columns.wbs">
                <span>Filter down by WBS:</span>
                <ul>
                    <li data-ng-repeat="item in filter.wbs">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>
            <div class="options boe" data-ng-show="filter.filterColumn === columns.boe">
                <span>Filter down by BOE Title:</span>
                <ul>
                    <li data-ng-repeat="item in filter.boe">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>
            <div class="options clin" data-ng-show="filter.filterColumn === columns.clin">
                <span>Filter down by CLIN:</span>
                <ul>
                    <li data-ng-repeat="item in filter.clin">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>
            <div class="options authors" data-ng-show="filter.filterColumn === columns.authors">
                <span>Filter down by Authors:</span>
                <ul>
                    <li data-ng-repeat="item in filter.author">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>
            <div class="options approvers" data-ng-show="filter.filterColumn === columns.approvers">
                <span>Filter down by Approvers:</span>
                <ul>
                    <li data-ng-repeat="item in filter.approver">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>
            <div class="options status" data-ng-show="filter.filterColumn === columns.status">
                <span>Filter down by Status Types:</span>
                <ul>
                    <li data-ng-repeat="item in filter.status">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>

            <div>
                <button class="ies float-right" type="button" name="cancel-button" data-ng-click="toggleFilter()">Cancel</button>
                <button class="ies-action float-right" type="button" data-ng-click="applyFilters(true)">Apply</button>
                <button class="ies" type="button" data-ng-click="clearFilter()">Clear Data</button>
            </div>
        </div>
    </div>

    <script>
        angular.element(document).ready(function () {
            angular.bootstrap(document, ['genboe']);
        });
    </script>
</asp:Content>