<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="IES.Common.classes" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems){%>genBOE<%} else {%>generation - genBOE<%} %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Scripts.Render("~/bundles/angularapp") %>
    <%: Scripts.Render("~/bundles/genboe") %>
    <script type="text/javascript">
        angular.module('genboe').value('HomePageModel', {
            workspace: 'default',
            controller: '<%:WebConstants.CONTROLLER_HOME %>',
            workspaceController: '<%:WebConstants.CONTROLLER_WORKSPACE %>',
            action: '<%:WebConstants.ACTION_HOME_GET_USER_METRICS %>',
            createAction: '<%: WebConstants.ACTION_CREATE_WORKSPACE %>',
            changeFavoriteAction: '<%: WebConstants.ACTION_HOME_CHANGE_FAVORITE %>',
            restoreAction: '<%: WebConstants.ACTION_HOME_RESTORE_WORKSPACE %>',
            restorePtmAction: '<%: WebConstants.ACTION_HOME_RESTORE_PTM_WORKSPACE %>',
            deleteAction: '<%: WebConstants.ACTION_HOME_DELETE_WORKSPACES %>',
            hideTracking: '<%: (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST).ToString().ToLower()%>' === 'true',
            isPtmIntegrated: '<%: Utilities.IsPTMIntegrated.ToString().ToLower() %>' === 'true',
            getTrackingNumbersAction: '<%: WebConstants.ACTION_HOME_GET_TRACKING_NUMBERS %>'
        });

        var UsageMetrics = new Widget("UsageMetrics");

        $(function () {
            // pulls out the error message from the query..
            var errorMessage = sessionStorage.errorMessage;
            if (errorMessage) {
                $('#errorMessage').addClass('validation-box').css('display', 'block').html(errorMessage);
            }
            sessionStorage.errorMessage = '';

            $('.header .title').addClass('genBOE');

            var chooseWorkspaceURL = CreateSystemAdminPostURL('<%: WebConstants.CONTROLLER_WORKSPACE %>', '<%: WebConstants.ACTION_DISPLAY_CHOOSE_WORKSPACE %>');
        });
    </script>
    
    <div class="module workspace-home" data-ng-controller="metricsCtrl" data-ng-cloak="">
        <div class="module-header-data">Workspaces</div>
        <div class="module-content-data">
            <div id="GenBOEMetrics">
                <div>
                    <div id="genBoeMetricsDiv"></div>
                    <!-- Grid -->
                    <div>
                        <div class="form-row css3pie-position-fix">

                            <div id="errorMessage"></div>
                            <div id="noteMessage" data-ng-show="isDataFiltered()">You are viewing filtered data. <a data-ng-click="clearAllFilters()">Click here</a> to reset all your filters.</div>

                            <button id="createWorkspaceBtn" data-ng-show="canCreateWS" data-ng-disabled="isLoading" data-ng-click="createWorkspace()" class="ies ies-action css3pie-position-fix" type="button">Create Workspace</button>
                            <div data-ng-hide="isLoading" class="buttons inline css3pie-position-fix" style="line-height: 28px;">
                                &nbsp; <!-- need line-height and nbsp to ensure div still occupies some real estate for the noteMessage below -->
                            </div>
                            <div id="Home-SearchRow" class="search-box float-right">
                                <div data-ng-show="!isLoading" class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                            </div>
                            <span class="viewAllHomePage" title="Depending on the number of workspaces you have available, this action may take a bit of time to complete."
                                    data-ng-click="togglePaging()" data-ng-hide="isLoading"> {{ viewAllLabel }}
                            </span>
                        </div>
                        <div id="QuickFilters" class="form-row css3pie-position-fix" data-ng-hide="isLoading">
                            <div class="search-box float-left" style="width:133px;margin-right:20px;">
                                <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." />
                            </div>
                            Quick Action Filters:
                            <span><a data-ng-click="showFavorites()">Show Only Favorites</a></span>
                            <span><a data-ng-click="sortMostRecent()">Sort Most Recently Viewed</a></span>
                        </div>
                        <div class="form-row">
                            <div class="workspace-home-grid">
                                <table id="HomeGrid" class="grid readonly">
                                    <thead>
                                        <tr>
                                            <th class="name bootstrap"><a data-ng-click="changeSorting(columns.name)" data-ng-class="{ 'bold': boldSort(columns.name) }">Workspace</a>
                                            </th>
                                            <th class="bootstrap favorite"><a data-ng-click="changeSorting(columns.favorite, true)" data-ng-class="{ 'bold': boldSort(columns.favorite) }">Favorite</a></th>
                                            <th class="lob bootstrap"><a data-ng-click="changeSorting(columns.lob)" data-ng-class="{ 'bold': boldSort(columns.lob) }">LOB</a>
                                                <br />
                                                <a data-ng-click="toggleFilter(columns.lob)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                            </th>
                                            <th data-ng-hide="hideTracking" class="track bootstrap"><a data-ng-click="changeSorting(columns.trackingNumber)" data-ng-class="{ 'bold': boldSort(columns.trackingNumber) }">Tracking<br />Number</a>
                                            </th>
                                            <th class="access bootstrap"><a data-ng-click="changeSorting(columns.accessDate, true);" data-ng-class="{ 'bold': boldSort(columns.accessDate) }">Last Accessed<br />Date</a></th>
                                            <th class="submittal bootstrap"><a data-ng-click="changeSorting(columns.submittalDate, true)" data-ng-class="{ 'bold': boldSort(columns.submittalDate) }">Estimated<br />Submittal Date</a>
                                                <div id="propDate" class="help-icon" onclick="UsageMetrics.ToggleHelp(this);"></div>
                                                <div id="propDateHelp" class="help-dialog" style="width: 320px;">
                                                    <div class="help-dialog-text">
                                                        The following colors signify the Workspace’s current Submittal Date status:<br />
                                                        Gray = All BOEs have been submitted <br />
                                                        Red = Proposal Submittal Date has passed <br />
                                                        Yellow = Proposal Submittal Date is in 14 or less days<br />
                                                        Green = Proposal Submittal Date is in more than 14 days
                                                    </div>
                                                </div>
                                            </th>
                                            <th class="state bootstrap"><a data-ng-click="changeSorting(columns.state)" data-ng-class="{ 'bold': boldSort(columns.state) }">Workspace<br />Status</a>
                                                <a data-ng-click="toggleFilter(columns.state)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                            </th>
                                            <th class="lead bootstrap"><a data-ng-click="changeSorting(columns.lead)" data-ng-class="{ 'bold': boldSort(columns.lead) }">Estimating<br />Lead/Pricer</a>
                                                <a data-ng-click="toggleFilter(columns.lead)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                            </th>
                                            <th class="reports bootstrap">Reports</th>
                                            <th class="delete" style="white-space:normal">
                                                <div class="buttons inline css3pie-position-fix">
                                                    <button class="ies-action css3pie-position-fix" type="button" name="Delete" value="Delete" data-ng-disabled="deleteBOEsDisabled()" data-ng-click="SubmitDelete()" data-ng-show="!isReadOnly">Delete</button>
                                                </div>
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr data-ng-show="isLoading"><td colspan="{{colSpan}}"><div class="loader"></div></td></tr>
                                        <tr data-ng-show="!isLoading && (data.length === 0 || filteredResults.length === 0)"><td colspan="{{colSpan}}"><div class="empty-grid-text">There are no matching Workspaces.</div></td></tr>
                                        <tr data-ng-show="!isLoading"
                                            data-ng-repeat="ws in (filteredResults = (data | filter:filterWorkspaces | orderBy:predicate:reverse)) | limitTo:pageSize:currentPage*pageSize">
                                            <td class="name"><a title="{{::ws.WorkspaceName}}" href="/{{::ws.WorkspaceShortName}}">{{::ws.WorkspaceName}}</a></td>
                                            <td class="bootstrap greenStar"><i class="glyphicon pointer" data-ng-click="updateFavorite(this)" data-ng-class="{'glyphicon-star' : ws.IsFavorite, 'glyphicon-star-empty' : !ws.IsFavorite}"></i></td>
                                            <td title="{{::ws.LineOfBusiness}}">{{::ws.LineOfBusiness}}</td>
                                            <td data-ng-hide="hideTracking" title="{{ws.TrackingNumber}}">{{ws.TrackingNumber}}</td>
                                            <td>{{::ws.LastAccessed}}</td>
                                            <td class="{{dateFormatClass(ws.ProposalSubmittal, ws.ProposalSubmittalDate, ws.NumBOEsInDraft + ws.NumBOEsInAwaitingApproval + ws.NumBOEsUnassigned, ws.NumBOEsApproved)}}">
                                                {{::ws.ProposalSubmittal}}
                                            </td>
                                            <td>
                                                {{::ws.WorkspaceState}}
                                            </td>
                                            <td title="{{::ws.CostVolumeLeadPricer}}">
                                                {{::ws.CostVolumeLeadPricer}}
                                            </td>
                                            <td>
                                                <div class="link" id="reportLink{{::ws.WorkspaceId}}" data-ng-click="openReports(this)">Reports</div>
                                            </td>
                                            <td>
                                                <input type="checkbox" name="workspaceRows[]" value="{{::ws.WorkspaceId}}" data-ng-checked="ws.toBeDeleted" data-ng-model="ws.toBeDeleted" data-ng-disabled="disableDeleteCheckbox(this)" data-ng-show="!workspaceHasBeenDeleted(this) && !isReadOnly"/>
                                                <div class="link" data-ng-click="showRestoreDialog(this)" data-ng-show="showRestore(this)">Restore</div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                        <div class="form-row last-form-row">
                            <div id="Home-SearchRow2" class="search-box full-width" data-ng-hide="isLoading">
                                <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" placeholder="Search..." style="float: left">
                                <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                            </div>
                        </div>
                    </div>

                    <!-- Filter Dialogs-->
                    <div gen-dialog id="FilterOptions" class="dialog form" data-width="400" data-height="300" data-title="Apply Filters" data-open="filter.open" data-on-close="onClose()">
                        <div class="options wbs" data-ng-show="filter.filterColumn === columns.lob">
                            <span>Filter down by LOB:</span>
                            <ul>
                                <li data-ng-repeat="item in filter.lob">
                                    <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                                </li>
                            </ul>
                        </div>
                        <div class="options boe" data-ng-show="filter.filterColumn === columns.state">
                            <span>Filter down by Workspace Status:</span>
                            <ul>
                                <li data-ng-repeat="item in filter.state">
                                    <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                                </li>
                            </ul>
                        </div>
                        <div class="options clin" data-ng-show="filter.filterColumn === columns.lead">
                            <span>Filter down by Estimating Lead/Pricer:</span>
                            <ul>
                                <li data-ng-repeat="item in filter.lead">
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
                    <script type="text/javascript">
                        angular.element(document).ready(function () {
                            angular.bootstrap(document, ['genboe']);
                        });

                    </script>
                </div>
            </div>
        </div>
        <div gen-dialog id="RestoreWorkspaceDialog" class="dialog form" data-width="500" data-height="300" data-title="Restore Workspace" data-open="restoreDialog.open">
            <gen-validation data-errors="errors"></gen-validation>
            <ul class="validation-box"></ul>
            <div class="form-row">
                <div class="form-element">
                    <br />
                    <p>
                    In order to restore a Workspace a PTM Tracking number is required to be associated with it.<br />
                    Note:  All automatic fields will be updated based on this PTM Tracking number.
                    </p>
                    <br />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Estimating Lead *</div>
                <div class="form-element">
                    <input type="hidden" id="RestoreCostVolumeLeadPricerNTID" name="RestoreCostVolumeLeadPricerNTID" data-ng-model="restoreDialog.leadNtId" data-ng-change="findPtmTrackingNumbers()" />
                    <input id="restoreCostVolumeLeadPricerDisplayName" name="restoreCostVolumeLeadPricerDisplayName" type="text" class="half" maxlength="50" data-ng-model="restoreDialog.leadName" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">PTM Tracking Number *</div>
                <div class="form-element">
                    <div class="loader" data-ng-show="restoreDialog.loadingTrackingNumbers"></div>
                    <input id="proposalSelect" list="proposalList" type="text" autocomplete="on" data-ng-model="restoreDialog.trackingNumber" data-ng-trim="false" data-ng-change="setTrackingNumber()" data-ng-hide="restoreDialog.loadingTrackingNumbers">
                    <datalist id="proposalList" data-ng-hide="restoreDialog.loadingTrackingNumbers">
                        <option ng-repeat="potentialTrackingNumber in restoreDialog.potentialTrackingNumbers" value="{{potentialTrackingNumber.Value}}">{{potentialTrackingNumber.Text}}</option>
                    </datalist>
                </div>
            </div>
            <div class="modal-footer dialog-footer">
                <button class="ies float-right" type="button" name="cancel-button" data-ng-click="closeRestoreDialog()">Cancel</button>
                <button class="ies-action float-right" type="button" data-ng-disabled="disableRestoreButton()" data-ng-click="restoreDialogClick()">Restore</button>
            </div>
        </div>
    </div>
</asp:Content>