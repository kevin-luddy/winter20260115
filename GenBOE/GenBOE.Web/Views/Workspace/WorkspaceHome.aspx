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
            exportAllAction: '<%: WebConstants.ACTION_EXPORT_ALL_BOES_WORKOFFLINE %>',
            exportSelectAction: '<%: WebConstants.ACTION_EXPORT_BOES_WORKOFFLINE %>',
            loadValidBOEsAction: '<%: WebConstants.ACTION_GET_EXPORT_BOE_MODEL %>',
            importAction: '<%:WebConstants.ACTION_COMPLETE_WORKOFFLINE_IMPORT_WORKSPACE_HOME %>',
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

                    <div class="buttons inline css3pie-position-fix" style="line-height: 28px;">
                        <button id="ImportExportButton-WorkspaceHome" data-ng-if="isWorkingState" data-ng-click="toggleImportExport()" data-ng-disabled="isLoading" data-ng-show="!isReadOnly" type="button" class="ies-action css3pie-position-fix">Import/Export BOEs</button>
                        &nbsp; <!-- need line-height and nbsp to ensure div still occupies some real estate when button is hidden -->
                    </div>

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

        <div gen-dialog id="ImportExportBoesDialog" class="dialog form" data-width="680" data-height="633" data-title="{{ dialog.title }}" data-open="dialog.open">
            <div data-ng-hide="dialog.showImportResults">
                <div id="Instructions">
                    <span>Select an option below to import or export BOEs.</span>        
                </div>
                <div class="step one">
                    <div class="title">Step 1: Select an option.</div>
                    <div>
                        <input id="Radio-Button-Export" type="radio" class="radio" data-ng-model="dialog.importExport" data-ng-value="dialog.exportValue" data-ng-disaled="dialog.validBOEs.length === 0"/>
                        <label for="Radio-Button-Export">Export BOEs</label>
                    </div>
                    <div>
                        <input id="Radio-Button-Import" type="radio" class="radio" data-ng-model="dialog.importExport" data-ng-value="dialog.importValue" />
                        <label for="Radio-Button-Import">Import BOEs</label>
                    </div>
                </div>
                <div class="step two" data-ng-show="dialog.importExport === dialog.exportValue">
                    <div class="title">Step 2: Export BOEs</div>
                    To Export BOEs, select an option below.  Only BOEs with a status of Draft are available to export.  
                    Material BOEs are not available for export.  Material, ODC, and Travel tasks are not available for export.  
                    BOEs can only be exported/imported by the current BOE Author(s).
                    <br />
                    <div class="title">Select an option:</div>
                    <div>
                        <input type="radio" class="radio" data-ng-model="dialog.exportSelect" data-ng-value="dialog.exportAllValue" data-ng-disabled="dialog.validBOEs.length === 0" id="Radio-Button-ExportAllBoes"/>
                        <label for="Radio-Button-ExportAllBoes">Export all BOEs</label>
                    </div>
                    <div>
                        <input type="radio" class="radio" data-ng-model="dialog.exportSelect" data-ng-value="dialog.exportSelectValue" id="Radio-Button-SelectBoesToExport"/>
                        <label for="Radio-Button-SelectBoesToExport">Select BOEs to export</label>
                    </div>
                    <div class="module workspace-home" id="exportboesmodule" data-ng-show="dialog.exportSelect === dialog.exportSelectValue">
                        <hr />
                        <div class="module-content-data">
                            <div class="form-row">
                                <div id="ExportBOEGridData">
                                    <table class="grid readonly">
                                        <colgroup>
                                            <col width="4%" />
                                            <col width="30%" />
                                            <col width="21%" />
                                            <col width="25%" />
                                            <col width="20%" />
                                        </colgroup>
                                        <thead>
                                            <tr>
                                                <th class="boe-select"></th>
                                                <th class="wbs-title"><a>WBS</a></th>
                                                <th class="boe-title"><a>BOE Title</a></th>
                                                <th class="clin-title"><a>CLIN</a></th>
                                                <th class="status-value"><a>Status</a></th>
                                            </tr>
                                       </thead>
                                        <tbody>
                                            <tr data-ng-show="dialog.validBOEs.length === 0"><td colspan="5"><div class="empty-grid-text">There are no BOEs available for export.</div></td></tr>
                                            <tr data-ng-repeat="validBOE in dialog.validBOEs">
                                                <td><input type="checkbox" data-ng-model="validBOE.checked" data-ng-disabled="disableValidBOE()"/></td>
                                                <td><a title="{{validBOE.WBSText}}">{{::validBOE.WBSText}}</a></td>
                                                <td><a title="{{validBOE.BOETitle}}">{{::validBOE.BOETitle}}</a></td>
                                                <td><a title="{{validBOE.CLINText}}">{{::validBOE.CLINText}}</a></td>
                                                <td>{{::validBOE.Status}}</td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>
                    <hr />
                    <div class="important">IMPORTANT: Do not change the column headings or options in the file. Do not enter a value for ID in column A. IDs are unique identifiers for BOEs, Tasks and Resources. One will be automatically generated for each new Task once the import is complete. Column A and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.</div>
                    <div class="important">If you delete an existing Task or Resource Type from the file, the Task or Resource Type will not be removed in genBOE. To delete a Resource Type, it must be deleted directly in genBOE to properly remove all associations.</div>
                    <hr />
                    <div class="buttons css3pie-position-fix">
                        <button id="ImportExportBoesDialog-ExportButton" data-ng-click="exportBOEs()" type="button" class="ies" data-ng-hide="dialog.importWorking" data-ng-disabled="disableExportButton()">Export</button>
                        <button id="ImportExportBoesDialog-ExportCancelButton" data-ng-click="toggleImportExport()" type="button" class="ies" name="cancel-button">Cancel</button>
                    </div>

                </div>
                <div class="step two" data-ng-show="dialog.importExport === dialog.importValue">
                    <% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new { 
                                            controller = WebConstants.CONTROLLER_WORKSPACE, 
                                            action = WebConstants.ACTION_IMPORT_WORKSPACEHOME_BOES, 
                                            workspace = SiteMasterUtilities.GetCurrentWorkspace()}, 
                                            FormMethod.Post,
                                            new { enctype = "multipart/form-data", id = "ImportExportBoesDialog-Form", target = "ImportExportBoesDialog-UploadTarget" }); %>
                    <div class="title">Step 2: Import BOEs</div>
                    <div>
                        Choose a file to import. The file you import must be an Excel file that ends in .xlsm.<br />
                        Current BOE data will be replaced with imported data. 
                        <br /><u>Required Fields:</u>
                        <ul>
                            <li>Task Title</li>
                            <li>Task Start Date</li>
                            <li>Task End Date</li>
                            <li>Resource Types / Resource Spreads</li>
                                <ul>
                                    <li>Element of Cost</li>
                                    <li>Resource</li>
                                    <li>Performing Org</li>
                                    <li>Resource Start Date</li>
                                    <li>Resource End Date</li>
                                    <li>Spread Curve</li>
                                </ul>
                        </ul>
                        <br />
                        Start and End dates must be in the following format: m/yyyy or mm/yyyy.<br /> 
                        Task Start Date must be on or after BOE Start Date. <br />
                        Task End Date must be on or before BOE End Date. <br />
                        Resource Start Date must be on or after Task Start Date. <br />
                        Resource End Date must be on or before Task End Date. <br /><br />
                            <span class="title">File location: </span>
                            <span>
                                <input type="hidden" id="importExportBoesDialog-Project-DocumentDomain" name="documentDomain" />
                                <input type="hidden" name="importExportBoesDialog" />
                                <input type="file" id="ImportExportBoesDialog-File" name="ImportExportBoesDialog-File" size="60" onchange="angular.element(this).scope().fileUploadChange(this)"/>
                            </span><br />   
                            File must contain the same headers and columns as exported prior to importing and be in .xlsm format.<br /><hr /><br />
                            <div class="important">IMPORTANT: Do not change the column headings or options in the file. Do not enter a value for ID in column A. IDs are unique identifiers for BOEs, Tasks and Resources. One will be automatically generated for each new Task once the import is complete. Column A and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.</div>
                            <div class="important">If you delete an existing Task or Resource Type from the file, the Task or Resource Type will not be removed in genBOE. To delete a Resource Type, it must be deleted directly in genBOE to properly remove all associations.</div>
                            <br /><hr /><br />
                            <div class="buttons">
                                <button id="ImportExportBoesDialog-ImportButton" type="button" class="ies-action" data-ng-hide="dialog.importWorking" data-ng-disabled="dialog.disableImport" data-ng-click="importBOEs()" name="import-button">Import</button>
                                <div id="ImportExportBoesDialog-ImportLoader" class="loader" data-ng-show="dialog.importWorking"></div>
                                <button id="ImportExportBoesDialog-ImportCancelButton" type="button" class="ies" data-ng-click="toggleImportExport()">Cancel</button>
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

    <script>
        angular.element(document).ready(function () {
            angular.bootstrap(document, ['genboe']);
        });
    </script>
</asp:Content>