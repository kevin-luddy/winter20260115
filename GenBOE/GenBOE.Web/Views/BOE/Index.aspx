<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Manage BOE - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/manageBOE") %>
<script type="text/javascript">
    app.value('ManageBOEModel', {
        workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        controller: '<%:WebConstants.CONTROLLER_BOE %>',
        action: '<%:WebConstants.ACTION_GET_MANAGE_BOE_MODEL %>',
        saveAction: '<%:WebConstants.ACTION_SAVE_MANAGE_BOE %>',
        saveState: '<%:WebConstants.ACTION_SAVE_BOE_STATES %>',
        exportAction: '<%:WebConstants.ACTION_EXPORT_MANAGE_BOE %>',
        exportTemplateAction: '<%:WebConstants.ACTION_EXPORT_MANAGE_BOE_TEMPLATE %>',
        workspaceState: '<%: ((GenBOEMasterModelView)Model).WorkspaceState %>',
        completeImportAction: '<%: WebConstants.ACTION_COMPLETE_IMPORT_MANAGE_BOE %>',
        bulkAssignRolesAction: '<%: WebConstants.ACTION_SAVE_BULK_ROLE_ASSIGN %>',
        draftState: <%: (int)BOEState.Draft%>,
        draftLockedState: <%: (int)BOEState.DraftLocked%>
    });
    var ManageBOEWidget;
    var BulkAssignWidget;
    $(function () {
        $(".main").addClass("boe");

        var ManageBOEWidgetConfig = { ContextID: "ManageBOEs", IsModule: true, isReadOnly: false };
        ManageBOEWidget = new GenWidget(ManageBOEWidgetConfig);

        $('#WorkspaceStatusLink').attr('href', CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
            '<%: WebConstants.ACTION_WORKSPACE_SETTINGS %>#WorkspaceStatus', ''));

        var BulkAssignWidgetConfig = { ContextID: "BulkAssign", IsModule: false, isReadOnly: false };
        BulkAssignWidget = new GenWidget(BulkAssignWidgetConfig);
    });
</script>

<div data-ng-controller="ManageBOEController" data-ng-cloak="">
    <div id="ManageBOE" class="manage-boe module">
        <div class="module-header-data"><div data-ng-show="!isBulkAssign">Manage BOEs</div><div data-ng-show="isBulkAssign">Bulk BOE Role Assignment</div></div>
        <div class="module-content-data">
            <div class="form-row" data-ng-show="!isBulkAssign">Add new or edit BOEs.<span id="InitializationText"> To allow Authors to begin work on BOEs, set <i>Workspace Status</i> to <i>Working</i> on the <a id="WorkspaceStatusLink" href="">Workspace Status</a> page.</span></div>
            <div class="form-row color-red" data-ng-show="!isBulkAssign">{{gridModel.manageBoeHeaderInfo}}</div>
            <div class="form-row css3pie-position-fix" data-ng-show="!isBulkAssign">
                <ul class="validation-box" style="display: none;"></ul>
                <gen-validation data-errors="errors"></gen-validation>
                <div class="buttons inline css3pie-position-fix" style="line-height: 28px;width: 600px;">
                    <button class="ies-action" data-ng-disabled="disableDelete()" data-ng-click="delete()" data-ng-show="isWorkingState" type="button">Delete</button>
                    <button class="ies-action" id="Add-ManageBOE" data-ng-disabled="isLoading" data-ng-show="isWorkingState" data-ng-click="AddBOE()" type="button">+ Add</button>
                    <button class="ies-action" id="Import-ManageBOE" data-ng-disabled="isLoading" data-ng-show="isWorkingState" data-ng-click="toggleImport()" type="button">Import</button>
                    <button class="ies-action" id="Export-ManageBOE" data-ng-disabled="isLoading || isExporting" data-ng-click="export(false)" type="button">Export</button>
                    <button class="ies-action" id="BulkAssign-ManageBOE" data-ng-disabled="isLoading || data.length == 0" data-ng-show="isWorkingState" data-ng-click="openBulkAssign()" type="button">Bulk Assign Roles</button>
                </div>
                <div id="noteMessage" data-ng-show="isDataFiltered()">You are viewing filtered data. <a data-ng-click="clearAllFilters()">Click here</a> to reset all your filters.</div>
                <div class="search-box float-right">
                    <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
                    <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                </div>
            </div>
            <div class="form-row" data-ng-show="!isBulkAssign">
               <div class="manage-boe-grid">
                    <table id="ManageBOEGrid" class="grid readonly" width="962">
                        <thead>
                            <tr>
                                <th class="delete-checkbox"><input data-ng-if="isWorkingState" data-ng-disabled="isLoading" data-ng-model="deleteAll.deleteAll" type="checkbox" data-ng-change="toggleDeleteAll()" /></th>
                                
                                <th class="wbs-title bootstrap"><a data-ng-click="changeSorting(columns.wbs)" data-ng-class="{ 'bold': boldSort(columns.wbs) }">WBS</a>
                                        <a data-ng-click="toggleFilter(columns.wbs)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="boe-title bootstrap"><a data-ng-click="changeSorting(columns.boe)" data-ng-class="{ 'bold': boldSort(columns.boe) }">BOE Title</a>
                                    <a data-ng-click="toggleFilter(columns.boe)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="clin-title bootstrap"><a data-ng-click="changeSorting(columns.clin)" data-ng-class="{ 'bold': boldSort(columns.clin) }">CLIN</a>
                                    <a data-ng-click="toggleFilter(columns.clin)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="author-select bootstrap"><a data-ng-click="changeSorting(columns.authors)" data-ng-class="{ 'bold': boldSort(columns.authors) }">Authors</a>
                                    <a data-ng-click="toggleFilter(columns.authors)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="approver-select bootstrap"><a data-ng-click="changeSorting(columns.approvers)" data-ng-class="{ 'bold': boldSort(columns.approvers) }">Approvers</a>
                                    <a data-ng-click="toggleFilter(columns.approvers)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="hours bootstrap"><a data-ng-click="changeSorting(columns.hours)" data-ng-class="{ 'bold': boldSort(columns.hours) }">Hours</a></th>
                                <th class="cost bootstrap"><a data-ng-click="changeSorting(columns.cost)" data-ng-class="{ 'bold': boldSort(columns.cost) }">Cost</a></th>
                                <th class="status-select bootstrap" data-ng-class="{' last-child' : !isWorkspaceLocked || !gridModel.AllowBOEStateChanges}"><a data-ng-click="changeSorting(columns.status)" data-ng-class="{ 'bold': boldSort(columns.status) }">Status</a>
                                    <a data-ng-click="toggleFilter(columns.status)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th data-ng-if="isWorkspaceLocked && gridModel.AllowBOEStateChanges" class="bootstrap last-child" style="width: 25px;">State</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr data-ng-show="isLoading"><td colspan="{{colSpan}}"><div class="loader"></div></td></tr>
                            <tr data-ng-show="!isLoading && (data.length === 0 || filteredResults.length === 0)"><td colspan="{{colSpan}}"><div class="empty-grid-text">There are no BOEs for the Workspace.</div></td></tr>
                            <tr pkid="{{::boe.BoeID}}" data-ng-repeat="boe in (filteredResults = (data | filter:filterBOEs | orderBy:predicate:reverse)) | limitTo:pageSize:currentPage*pageSize">
                                <td data-ng-click="editBOE(boe)"class="text delete-checkbox">
                                    <div>
                                        <input class="delete-chck" type="checkbox" data-ng-if="isWorkingState" data-ng-model="boe.Deleted" data-ng-click="$event.stopPropagation()" />
                                    </div>
                                </td>
                                <td><a title="{{::boe.WbsDisplayName}}" data-ng-click="editBOE(boe)">{{::boe.WbsDisplayName}}</a></td>
                                <td><a title="{{::boe.BOETitle}}" data-ng-click="editBOE(boe)">{{::boe.BOETitle}}</a></td>
                                <td><a title="{{::boe.ClinDisplayName}}" data-ng-click="editBOE(boe)">{{::boe.ClinDisplayName}}</a></td>
                                <td>
                                    <div data-ng-repeat="authorName in boe.AuthorsDisplayNames">{{::authorName}}</div>
                                </td>
                                <td>
                                    <div data-ng-repeat="approver in boe.ApproversDisplayNames">
                                        {{::approver}}
                                    </div>
                                </td>
                                <td>{{::boe.TotalHours}}</td>
                                <td>{{::boe.TotalCost}}</td>
                                <td>{{::boe.Status}}</td>
                                <td data-ng-if="isWorkspaceLocked && gridModel.AllowBOEStateChanges" data-ng-switch="boe.State">
                                    <img data-ng-switch-when="2" title="Click to lock BOE" class="pointer" data-ng-click="lockBOE(boe)" src="/Resources/css/images/unlock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" />
                                    <img data-ng-switch-when="6" title="Click to unlock BOE" class="pointer" data-ng-click="unlockBOE(boe)" src="/Resources/css/images/lock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="form-row" data-ng-show="isBulkAssign">
                Assign or Remove users in bulk. Select one or more BOEs, a Role, and one or more Users to assign or remove. <br />
                Select "Assign" to assign the Users to the Role in the BOEs if they are not already assigned. <br />
                Select "Remove" to remove the selected Users from the selected Role in the selected BOEs if they are assigned. <br /><br />
                Note: Only BOEs that are Unassigned or in Draft are available for bulk assignment. Roles are locked for BOEs outside of these statuses.
            </div>
            <div id="BulkAssign" class="form-row" data-ng-show="isBulkAssign">
                <ul class="validation-box" style="display: none;"></ul>
                <gen-validation data-errors="errors"></gen-validation>
                <div class="bulk-assign-add-remove bootstrap">
                    <div class="bulk-select">
                        <div class="bulk-assign-label">
                            <span helptext="BOEs within the dropdown can be searched by WBS or CLIN. Search 'error' to get all BOEs currently with validation errors.">BOEs</span>
                        </div>
                        <div ng-dropdown-multiselect="" options="bulkAssignBoes" selected-model="selectedBoes" checkBoxes="true" extra-settings="boeDropdownSettings"></div>
                    </div>
                    <div class="bulk-select role-select">
                        <div class="bulk-assign-label">Role</div>
                        <select data-ng-model="selectedRole" data-ng-change="clearUsers()">
                            <option></option>
                            <option>{{roles.author}}</option>
                            <option>{{roles.approver}}</option>
                            <option>{{roles.subAuthor}}</option>
                        </select>
                    </div>
                    <div class="bulk-select">
                        <div class="bulk-assign-label">Users</div>
                        <div data-ng-if="selectedRole == undefined || selectedRole == ''" data-ng-disabled="true" ng-dropdown-multiselect="" disabled="true"></div>
                        <div data-ng-if="selectedRole == roles.author" ng-dropdown-multiselect="" options="bulkAssignAuthors" selected-model="selectedUsers" checkBoxes="true" extra-settings="userDropdownSettings"></div>
                        <div data-ng-if="selectedRole == roles.approver" ng-dropdown-multiselect="" options="bulkAssignApprovers" selected-model="selectedUsers" checkBoxes="true" extra-settings="userDropdownSettings"></div>
                        <div data-ng-if="selectedRole == roles.subAuthor" ng-dropdown-multiselect="" options="bulkAssignSubAuthors" selected-model="selectedUsers" checkBoxes="true" extra-settings="userDropdownSettings"></div>
                    </div>
                    <button id="bulk-assign-add" class="ies-action" data-ng-click="bulkAssignRoles()" data-ng-disabled="disableAssignRemove()">Assign</button>
                    <button id="bulk-assign-remove" class="ies-danger" data-ng-click="bulkRemoveRoles()" data-ng-disabled="disableAssignRemove()">Remove</button>
                    <div class="search-box float-right">
                        <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
                        <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                    </div>
                </div>
                <div id="noteMessage" class="noteMessage" data-ng-show="isDataFiltered()">You are viewing filtered data. <a data-ng-click="clearAllFilters()">Click here</a> to reset all your filters.</div>
                <div id="QuickFilters" data-ng-class="{ 'filter-margin': !isDataFiltered() }">
                    Quick Action Filters: 
                    <span>
                        <a data-ng-click="filterForIncomplete()">Incomplete BOEs</a>
                    </span>
                    <span>
                        <a data-ng-click="filterForErrors()" data-ng-disabled="!anyBoesWithErrors()" data-ng-class="{ 'disabled': !anyBoesWithErrors() }">BOEs with Errors</a>
                    </span>
                </div>
                <div class="bulk-assign-grid">
                    <table id="BulkAssignGrid" class="grid readonly" width="962">
                        <thead>
                            <tr>
                                <th class="wbs-title bootstrap">
                                    <a data-ng-click="changeSorting(columns.wbs)" data-ng-class="{ 'bold': boldSort(columns.wbs) }">WBS</a>
                                    <a data-ng-click="toggleFilter(columns.wbs)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="boe-title bootstrap">
                                    <a data-ng-click="changeSorting(columns.boe)" data-ng-class="{ 'bold': boldSort(columns.boe) }">BOE Title</a>
                                    <a data-ng-click="toggleFilter(columns.boe)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="clin-title bootstrap">
                                    <a data-ng-click="changeSorting(columns.clin)" data-ng-class="{ 'bold': boldSort(columns.clin) }">CLIN</a>
                                    <a data-ng-click="toggleFilter(columns.clin)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="author-select bootstrap">
                                    <a data-ng-click="changeSorting(columns.authors)" data-ng-class="{ 'bold': boldSort(columns.authors) }">Authors</a>
                                    <a data-ng-click="toggleFilter(columns.authors)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="approver-select bootstrap">
                                    <a data-ng-click="changeSorting(columns.approvers)" data-ng-class="{ 'bold': boldSort(columns.approvers) }">Approvers</a>
                                    <a data-ng-click="toggleFilter(columns.approvers)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr data-ng-show="isLoading"><td colspan="5"><div class="loader"></div></td></tr>
                            <tr data-ng-show="!isLoading && (data.length === 0 || filteredBulkResults.length === 0)"><td colspan="5"><div class="empty-grid-text">There are no BOEs for the Workspace.</div></td></tr>
                            <tr pkid="{{::boe.BoeID}}" data-ng-repeat="boe in (filteredBulkResults = (bulkAssignData | filter:filterBOEs | orderBy:predicate:reverse)) | limitTo:pageSize:currentPage*pageSize" data-ng-class="{ 'bulkAssignError': boe.hasError }">
                                <td>{{::boe.WbsDisplayName}}</td>
                                <td>{{::boe.BOETitle}}</td>
                                <td>{{::boe.ClinDisplayName}}</td>
                                <td><div data-ng-repeat="authorName in boe.AuthorsDisplayNames">{{authorName}}</div></td>
                                <td><div data-ng-repeat="approver in boe.ApproversDisplayNames">{{approver}}</div></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="buttons">
                    <div class="search-box float-right">
                        <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
                        <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredBulkResults) }}" data-current-page="currentPage"></div>
                    </div>
                    <button id="BulkAssign-ValidateButton" type="button" class="ies-action" data-ng-disabled="!isDirty" data-ng-click="validateBulkAssign(true, true)">Validate</button>
                    <button id="BulkAssign-SaveButton" type="button" class="ies-action" data-ng-disabled="!isDirty" data-ng-click="saveBulkAssign()">Save</button>
                    <button id="BulkAssign-CancelButton" type="button" class="ies" data-ng-click="cancelBulkAssign()">Cancel</button>
                </div>
            </div>
            <div class="form-row last-form-row" data-ng-show="!isBulkAssign">
                <div class="search-box full-width">
                    <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
                    <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
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

        <div gen-dialog  id="BoeElementDialog" class="manage-boe-dialog" data-width="400" data-title="Manage BOE" data-open="edit.open" data-on-close="onEditClose()">
            <gen-validation data-errors="modalErrors"></gen-validation>
            <div id="ExistingMultiWBSClinWarning" class="warning-box" data-ng-show="edit.showSelectMultiClinWarning">
                <div class="warning-message">
                   Selecting Multi will deselect current WBS and CLIN values for this BOE. Any existing Travel and ODC Task Elements will be deleted.
                </div>
                <div class="small-close-button" data-ng-click="hideSelectMultiClinWarning()">
                </div>
            </div>
            <div id="RemoveMultiWBSClinWarning" class="warning-box" data-ng-show="edit.showDeselectMultiClinWarning">
                <div class="warning-message">
                     Deselecting Multi will deselect current WBS and CLIN values for all resources in this BOE.
                </div>
                <div class="small-close-button" data-ng-click="hideDeselectMultiClinWarning()">
                </div>
            </div>
            <div class="form-row MultiWBSClin-row">
                <div class="form-label">Multi WBS/Clin</div>
                <div class="form-element">
                   <input name="MultiClinWbsCheckBox" data-ng-disabled="!isWorkingState" data-ng-model="edit.boe.IsMultiClinWbs" data-ng-click="updateMultiClin()" type="checkbox" id="MultiWBSClin" class = "MultiWBSClin" style="width:13px" />
                </div>
            </div> 
            <div class="form-row">
                <div class="form-label">WBS</div>
                <div class="form-element">
                    <div data-ng-show="edit.boe.IsMultiClinWbs">MULTI</div>
                    <select data-ng-hide="edit.boe.IsMultiClinWbs || !isWorkingState" class="atleastone" data-ng-change="setEditDirty()" data-ng-model="edit.boe.WbsID" id="WbsID" name="WbsID">
                        <option data-ng-repeat="wbs in gridModel.PotentialWbs" value="{{::wbs.Value}}" >{{::wbs.Text}}</option>
                    </select>
                    <div data-ng-show ="!edit.boe.IsMultiClinWbs && !isWorkingState">{{edit.boe.WbsDisplayName}}</div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">CLIN</div>
                <div class="form-element">
                    <div data-ng-show="edit.boe.IsMultiClinWbs">MULTI</div>
                    <select data-ng-hide="edit.boe.IsMultiClinWbs || !isWorkingState" class="atleastone" data-ng-change="setEditDirty()" data-ng-model="edit.boe.ClinID" id="ClinID" name="ClinID">
                        <option data-ng-repeat="clin in gridModel.PotentialClins" value="{{::clin.ClinID}}" startDate ="{{::clin.StartDate}}" endDate="{{::clin.EndDate}}">{{::clin.ClinString}}</option>
                    </select>
                    <div data-ng-show ="!edit.boe.IsMultiClinWbs && !isWorkingState">{{edit.boe.ClinDisplayName}}</div>
                </div>
            </div>
            <div class="form-row" data-ng-show="edit.showMaterial">
                <div class="form-label">Material</div>
                <div class="form-element">
                   <input name="Material" data-ng-click="setEditDirty()" data-ng-disabled="!isWorkingState" data-ng-model="edit.boe.isMaterial" type="checkbox" id="Material" class = "material" disabled="disabled" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Authors</div>
                <div class="form-element">
                    <div class="approver-selection-box" id="author-selection-box"> 
                        <%--no values, but we need something to assist validation--%>
                        <div data-ng-if="gridModel.PotentialAuthors.length === 0">
                            <input type="checkbox" value="empty" name="Authors" id="-1" class="display-none" />
                        </div>
                        <div data-ng-repeat="author in gridModel.PotentialAuthors">
                            <input type="checkbox" data-ng-change="setEditDirty()" data-ng-disabled="disableAuthorApprover()" checklist-model="edit.boe.Authors" checklist-value="author.Value" name="Authors" id="Author_{{::author.Value}}" />
                            <label for="Author_{{::author.Value}}">{{::author.Text}}</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Subcontractor<br/>Authors</div>
                <div class="form-element">
                    <div class="approver-selection-box" id="subcontractorauthor-selection-box">
                        <%--no values, but we need something to assist validation--%>
                        <div data-ng-if="gridModel.PotentialSubcontractorAuthors.length === 0">
                            <input type="checkbox" value="empty" name="SubcontractorAuthors" id="-2" class="display-none" />
                        </div>
                        <div data-ng-repeat="subcontractorAuthor in gridModel.PotentialSubcontractorAuthors">
                            <input type="checkbox" data-ng-change="setEditDirty()" data-ng-disabled="disableAuthorApprover()" checklist-model="edit.boe.SubcontractorAuthors" checklist-value="subcontractorAuthor.Value" name="SubcontractorAuthors" id="SubcontractorAuthor_{{::subcontractorAuthor.Value}}" />
                            <label for="SubcontractorAuthor_{{::subcontractorAuthor.Value}}">{{::subcontractorAuthor.Text}}</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Approvers</div>
                <div class="form-element">
                    <div class="approver-selection-box" id="approver-selection-box">
                        <%--no values, but we need something to assist validation--%>
                        <div data-ng-if="gridModel.PotentialApprovers.length === 0">
                            <input type="checkbox" value="empty" name="Approvers" id="-1" class="display-none" />
                        </div>
                        <div data-ng-repeat="approver in gridModel.PotentialApprovers">
                            <input type="checkbox" data-ng-change="setEditDirty()" data-ng-disabled="disableAuthorApprover()" checklist-model="edit.boe.Approvers" checklist-value="approver.Value" name="Approvers" id="Approver_{{::approver.Value}}" />
                            <label for="Approver_{{::approver.Value}}">{{::approver.Text}}</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="form-row" data-ng-if="!edit.isAdd">
                <div class="form-label">Status *</div>
                <div class="form-element">
                    <div data-ng-if="edit.showStatus == 'Draft' && isWorkingState">{{edit.boe.Status}}</div>
                    <select data-ng-if="showEditStatus()" data-ng-change="setEditDirty()" data-ng-model="edit.boe.State">
                        <option class="boeState" value="<%: (int)BOEState.Draft %>">Draft</option>
                        <option data-ng-if="edit.showStatus == 'AwaitingApproval'" class="boeState" value="<%: (int)BOEState.AwaitingApproval %>">Awaiting Approval</option>
                        <option data-ng-if="edit.showStatus == 'Approved'" class="boeState" value="<%: (int)BOEState.Approved %>">Approved</option>
                    </select>
                    <div data-ng-if="!isWorkingState">
                        <div style="float: left;">{{edit.boe.Status}}</div>
                        <img class="pointer" data-ng-if="gridModel.AllowBOEStateChanges && edit.boe.State == <%: (int)BOEState.DraftLocked %>" data-ng-click="unlockEditBOE()" src="/Resources/css/images/lock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" title="Click to unlock BOE">
                        <img class="pointer" data-ng-if="gridModel.AllowBOEStateChanges && edit.boe.State == <%: (int)BOEState.Draft %>" data-ng-click="lockEditBOE()" src="/Resources/css/images/unlock_resource_toggle.png" style="width: 14px !important; margin-left: 4px;" title="Click to lock BOE">
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label"></div>
                <div class="form-element">
                    <div id="OCINote" class="oci-note">
                        <span data-ng-show="gridModel.ContainsOCI"><b>Note:</b> Must not contain any classified, export controlled or third party proprietary information.</span>
                        <span data-ng-hide="gridModel.ContainsOCI"><b>Note:</b> Must not contain any OCI, classified, export controlled or third party proprietary information.</span>
                    </div>
                    <div class="button-container">
                        <div class="buttons">
                            <button data-ng-click="save(false, true)" data-ng-disabled="!edit.isDirty" name="save-button" type="button" class="ies-action stateful_button">Save</button>
                            <button data-ng-click="save(true, true)" data-ng-show="edit.isAdd" data-ng-disabled="!edit.isDirty" name="save-add-another-button" type="button" class="ies stateful_button">Save &amp; add another</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div gen-dialog id="ImportBOEDialog" class="import-boe dialog form" data-width="680" data-height="580" data-title="{{ dialog.title }}" data-open="dialog.open">
            <div id="ImportInstructions" data-ng-hide="dialog.showImportResults">
                <span>To import BOEs, follow the steps below.</span>
                <div class="step one">
                    <div class="title">Step 1: Select what to import</div>
                    <div>
                        <input type="radio" class="radio" data-ng-model="dialog.importExisting" data-ng-value="false" data-ng-change="showImportSteps()" />
                        <label for="ImportType-New">Import <i>only</i> new BOEs</label>
                    </div>
                    <div>
                        <input type="radio" class="radio" data-ng-model="dialog.importExisting" data-ng-value="true" data-ng-change="showImportSteps()" />
                        <label for="ImportType-Existing">Import new BOEs <i>and</i> import updates to existing BOEs.</label>
                    </div>
                </div>
                <div data-ng-show="dialog.showSteps">
                    <% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new { 
                                                controller = WebConstants.CONTROLLER_BOE, 
                                                action = WebConstants.ACTION_IMPORT_MANAGE_BOE, 
                                                workspace = SiteMasterUtilities.GetCurrentWorkspace()}, 
                                                FormMethod.Post, 
                                                new { enctype = "multipart/form-data", id = "ImportBOEDialog-Form", target = "ImportBOEDialog-UploadTarget" }); %>
                    <div class="step two" data-ng-hide="dialog.importExisting">
                        <div class="title">Step 2: Download the BOEs template file</div>
                        <div>Start by downloading the BOEs template file. This file must be used because it has the correct column headings and options genBOE needs to import the BOEs.</div>
                        <div><a data-ng-click="export(true)">Download BOEs template file</a></div>
                    </div>
                    <div class="step two" data-ng-show="dialog.importExisting">
                        <div class="title">Step 2: Export the existing BOEs</div>
                        <div>Start by exporting the existing BOEs. This file must be used because it has the correct column headings and options genBOE needs to import new BOEs and updates.</div>
                        <div><a data-ng-click="export(false)">Export existing BOEs</a></div>
                    </div>
                    <div class="step three" data-ng-hide="dialog.importExisting">
                        <div class="title">Step 3: Enter BOEs into the file</div>
                        <div>Enter BOEs into the file.</div>
                        <div>The following fields are required for a BOE:
                            <ul>
                                <li>WBS or CLIN</li>
                                <li>Material</li>
                                <li>Author or Subcontractor Author</li>
                                <li>At least one Approver</li>
                            </ul>
                        </div>
                        <div class="important">
                            IMPORTANT: Do not change the column headings or options in the file. Do not enter a value for genBOE BOE ID in column A. genBOE BOE IDs are unique identifiers for BOEs. One will be automatically generated for each new BOE once the import is complete. Column A and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.
                        </div>
                    </div>
                    <div class="step three " data-ng-show="dialog.importExisting">
                        <div class="title">Step 3: Enter BOEs into the file</div>
                        <div>Enter BOEs into the file.</div>
                        <div>The following fields are required for a BOE:
                            <ul>
                                <li>WBS or CLIN</li>
                                <li>Material</li>
                                <li>Author or Subcontractor Author</li>
                                <li>At least one Approver</li>
                            </ul>
                        </div>
                        <div class="important">
                            IMPORTANT: Do not change the column headings or options in the file. Do not enter a value for genBOE BOE ID in column A. genBOE BOE IDs are unique identifiers for BOEs. One will be automatically generated for each new BOE once the import is complete. Column A and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.
                        </div>
                        <div class="important">To delete an existing BOE from the file, enter “Delete” for the BOE’s status.</div>
                    </div>
                    <div class="step four">
                        <div class="title">Step 4: Import the updated BOEs template file</div>
                        <div>Choose the file to import. The file must be the downloaded Excel file that ends in .xlsx or .xlsm.</div>
                        <div>
                            <input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
                            <input type="file" size="60" id="ImportDialog-File" name="file" onchange="angular.element(this).scope().fileUploadChange(this)" />
                        </div>
                        <div id="OCINoteImport" class="oci-note">
                            <span data-ng-show="gridModel.ContainsOCI"><b>Note:</b> Must not contain any classified, export controlled or third party proprietary information.</span>
                            <span data-ng-hide="gridModel.ContainsOCI"><b>Note:</b> Must not contain any OCI, classified, export controlled or third party proprietary information.</span>
                        </div>
                        <div class="buttons">
                            <button id="ImportBOEDialog-ImportButton" type="button" class="ies-action" data-ng-hide="dialog.importWorking" data-ng-disabled="dialog.disableImport" data-ng-click="importBOEs()" name="import-button">Import</button>
                            <div id="ImportBOEDialog-ImportLoader" class="loader" data-ng-show="dialog.importWorking"></div>
                            <button id="ImportBOEDialog-ImportCancelButton" type="button" class="ies" data-ng-click="toggleImport()">Cancel</button>
                        </div>
                    </div>
                    <% Html.EndForm(); %>
                </div>
            </div>
            <div id="ImportResults" data-ng-show="dialog.showImportResults" class="import-verification">
                <div class="content import-verification"></div>
                <div class="buttons">
                    <button id="Back-WorkofflineImportVerification" type="button" class="ies" data-ng-click="backFromImport()" name="back-button">Back</button>
                    <button id="CompleteImportButton-WorkofflineImportVerification" class="ies-action" data-ng-if="!dialog.invalidData" data-ng-hide="dialog.completeImportWorking" data-ng-click="completeImportBOEs()">Complete Import</button>
                    <div id="CompleteImportLoader-WorkofflineImportVerification" class="loader" data-ng-show="dialog.completeImportWorking" style="width: 129px"></div>
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
