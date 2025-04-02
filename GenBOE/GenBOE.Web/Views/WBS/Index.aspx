<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Manage WBS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/manageWBS") %>
<script type="text/javascript">
     app.value('ManageWBSModel', {
        workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        controller: '<%:WebConstants.CONTROLLER_WBS %>',
        action: '<%:WebConstants.ACTION_GET_MANAGE_WBS_MODEL %>',
        boeController: '<%: WebConstants.CONTROLLER_BOE %>',
        saveAction: '<%:WebConstants.ACTION_SAVE_MANAGE_WBS_UPDATES %>',
        exportAction: '<%:WebConstants.ACTION_EXPORT_WBS %>',
        exportTemplateAction: '<%:WebConstants.ACTION_EXPORT_WBS_TEMPLATE %>',
        workspaceState: '<%: ((GenBOEMasterModelView)Model).WorkspaceState %>',
        importAction: '<%:WebConstants.ACTION_COMPLETE_IMPORT_WBS %>',
        createBOEsAction: '<%:WebConstants.ACTION_CREATE_BOES %>'
     });

	var isClassEnvironment = <%: SiteMasterUtilities.IsClassEnvironment.ToString().ToLower() %>;
	console.log(isClassEnvironment);
    var ManageWBSWidget;
    $(".main").addClass("wbs");

    $(function () {
        var ManageWBSWidgetConfig = { ContextID: "ManageWBSs", IsModule: true, isReadOnly: false }; // always mark this as false so we can essentially override the GenListWidget readonly rules
        ManageWBSWidget = new GenWidget(ManageWBSWidgetConfig);
    });
</script>

<div data-ng-controller="ManageWBSController" data-ng-cloak="">
    <div id="ManageWBS" class="manage-wbs module">
        <div class="module-header-data">Manage WBS</div>
        <div class="module-content-data">
            <div class="form-row">Add new or edit WBS elements.  Create BOEs from selected WBS elements.  CLINs must exist on the 
                <%:
                    Html.RouteLink("Manage CLINs",
                    WebConstants.ROUTE_WORKSPACE,
                    null,
                    null,
                    null,
                    new { controller = WebConstants.CONTROLLER_CLIN, 
                        action = WebConstants.ACTION_INDEX,
                        workspace = SiteMasterUtilities.GetCurrentWorkspace() },
                    null)
                %>
                page in order to associate a CLIN to a WBS.
            </div>
            <div class="form-row css3pie-position-fix">
                <ul class="validation-box" style="display: none;"></ul>
                <gen-validation data-errors="errors"></gen-validation>
                <div class="buttons inline css3pie-position-fix" style="line-height: 28px;width: 600px;">
                    <button class="ies-action" data-ng-disabled="createBOEsDisabled" data-ng-click="createBOEs()" data-ng-show="isWorkingState" id="CreateBOEs-ManageWBS" type="button">Create BOE for selected WBS</button>
                    <button class="ies-action" id="Add-ManageWBS" data-ng-show="isWorkingState" data-ng-click="AddWbs()" type="button">+ Add</button>
                    <button class="ies-action" id="Import-ManageWBS" data-ng-show="isWorkingState" data-ng-click="toggleImport()" type="button">Import</button>
                    <button class="ies-action" id="Export-ManageWBS" data-ng-click="export(false)" data-ng-disabled="isExporting" type="button">Export</button>
                </div>
                <div id="noteMessage" data-ng-show="isDataFiltered()">You are viewing filtered data. <a data-ng-click="clearAllFilters()">Click here</a> to reset all your filters.</div>
                <div class="search-box float-right">
                    <input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
                    <div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
                </div>
            </div>
            <div class="form-row">
               <div class="manage-wbs-grid">
                    <table id="ManageWBSGrid" class="grid readonly" width="962">
                        <colgroup>
                            <col width="20px"/>
                            <col width="20%" />
                            <col width="30%" />
                            <col/>
                            <col/>
                        </colgroup>
                        <thead>
                            <tr>
                                <th><input data-ng-show="isWorkingState" type="checkbox" id="checkAllWbs" data-ng-click="checkAllWBSClicked()" data-ng-model="checkAllClicked" /></th>
                                <th class="wbs-number bootstrap"><a data-ng-click="changeSorting(columns.wbsNumber)" data-ng-class="{ 'bold': boldSort(columns.wbsNumber) }">WBS #</a>
                                        <a data-ng-click="toggleFilter(columns.wbsNumber)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="wbs-title bootstrap"><a data-ng-click="changeSorting(columns.title)" data-ng-class="{ 'bold': boldSort(columns.title) }">WBS Title</a>
                                        <a data-ng-click="toggleFilter(columns.title)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th class="clin-for-boes bootstrap"><a data-ng-click="changeSorting(columns.clins)" data-ng-class="{ 'bold': boldSort(columns.clins) }">CLINs</a>
                                        <a data-ng-click="toggleFilter(columns.clins)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
                                </th>
                                <th  id="DeleteHeader" class="delete last-child"><div data-ng-show="isWorkingState" data-ng-click="deleteAll()" class="delete"></div></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr data-ng-show="isLoading"><td colspan="{{colSpan}}"><div class="loader"></div></td></tr>
                            <tr data-ng-show="!isLoading && (data.length === 0 || filteredResults.length === 0)"><td colspan="{{colSpan}}"><div class="empty-grid-text">There are no WBS elements.</div></td></tr>
                            <tr pkid="{{::wbs.WbsID}}" data-ng-repeat="wbs in (filteredResults = (data | filter:filterWBSs | orderBy:predicate:reverse)) | limitTo:pageSize:currentPage*pageSize">
                                <td colspan="2" data-ng-click="editWbs(wbs)" class="text wbs-number" title="{{::wbs.WbsNumber}}">
                                    <div>
                                        <span ng-repeat="n in [].constructor(wbs.Level) track by $index">&nbsp;&nbsp;</span>
                                        <input class="delete-chck" type="checkbox" data-ng-show="isWorkingState" data-ng-click="createBOEToggle($event, wbs)" data-ng-model="wbs.Selected" data-ng-disabled="wbs.Disabled" />
                                        <a name="WbsNumber">{{::wbs.WbsNumber}}</a>
                                    </div>
                                </td>
                                <td class="text wbs-title"  title="{{::wbs.WbsTitle}}">
                                    <a data-ng-click="editWbs(wbs)" name="WbsTitleGrid">{{::wbs.WbsTitle}}</a>
                                </td>
                                <td class="clin-select clin-for-boes"  title="{{::wbs.ClinNumbers}}">
                                    <div name="ClinNumbers">{{::wbs.ClinNumbers}}</div>
                                </td>
                                <td class="delete-cell">
                                    <div data-ng-show="wbs.InUse">In use</div>
                                    <div data-ng-hide="wbs.InUse || !isWorkingState" data-ng-click="delete(wbs)" class="delete"></div>
                                </td>
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
    
        <div gen-dialog id="FilterOptions" class="dialog form" data-width="400" data-height="300" data-title="Apply Filters" data-open="filter.open" data-on-close="onClose()">
            <div class="options wbs" data-ng-show="filter.filterColumn === columns.wbsNumber">
                <span>Filter down by WBS #:</span>
                <ul>
                    <li data-ng-repeat="item in filter.wbsNumber">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>
            <div class="options boe" data-ng-show="filter.filterColumn === columns.title">
                <span>Filter down by WBS Title:</span>
                <ul>
                    <li data-ng-repeat="item in filter.title">
                        <label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
                    </li>
                </ul>
            </div>
            <div class="options clin" data-ng-show="filter.filterColumn === columns.clins">
                <span>Filter down by CLIN:</span>
                <ul>
                    <li data-ng-repeat="item in filter.clins">
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

        <div gen-dialog id="WBSElementDialog" class="manage-wbs-dialog" data-width="400" data-title="Manage WBS" data-open="edit.open" data-on-close="onEditClose()">
            <gen-validation data-errors="modalErrors"></gen-validation>
            <div class="form-row">
                <div class="form-label">
                <span helptext="Used to define a WBS in the Workspace.<br /><br />Can have up to 10 levels and 5 characters for each level.  Use a period to separate each level (example: 1.1).">WBS # *</span>
                </div>
                <div class="form-element">
                    <span data-ng-hide="isWorkingState">{{::edit.wbs.WbsNumber}}</span>
                    <input data-ng-show="isWorkingState" type="text" data-ng-change="setEditDirty()" data-ng-model="edit.wbs.WbsNumber" maxlength="50" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">WBS Title*</div>
                <div class="form-element">
                    <span data-ng-hide="isWorkingState">{{::edit.wbs.WbsTitle}}</span>
                    <input data-ng-show="isWorkingState" class="wbs-title" data-ng-change="setEditDirty()" spellcheck="true" type="text" data-ng-model="edit.wbs.WbsTitle" maxlength="255" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label"><span helptext="Indicate the CLINs associated with this WBS.<br /><br />If CLINs are selected and Create BOE is selected, a BOE will be created for each CLIN.">CLINs</span></div>
                <div class="form-element">
                    <div class="clin-selection-box">
                        <div data-ng-repeat="clin in availableClins">
                            <input type="checkbox" data-ng-change="setEditDirty()" data-ng-disabled="!isWorkingState || clinInUse(clin.ClinID)" checklist-model="edit.wbs.ClinIDs" checklist-value="clin.ClinID" name="ClinBoxes" id="Clin_{{::clin.ClinID}}" />
                            <label for="Clin_{{::clin.ClinID}}">{{::clin.ClinString}}</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="form-label"></div>
            <div class="form-element">
                <div id="OCINote" class="oci-note">
                    <span data-ng-show="containsOCI"><b>Note:</b> <%: SiteMasterUtilities.GetBannerText(true) %> </span>
                    <span data-ng-hide="containsOCI"><b>Note:</b> <%: SiteMasterUtilities.GetBannerText() %></span>
                </div>
                <div class="button-container">
                    <div class="buttons">
                        <button data-ng-click="save(false)" data-ng-show="isWorkingState" data-ng-disabled="!edit.isDirty" name="save-button" type="button" class="ies-action stateful_button">Save</button>
                        <button data-ng-click="save(true)" data-ng-show="isWorkingState" data-ng-disabled="!edit.isDirty" name="save-add-another-button" type="button" class="ies stateful_button">Save &amp; add another</button></div>
                </div>
            </div>
        </div>

        <div gen-dialog id="ImportWBSDialog" class="import-wbs dialog form" data-width="680" data-height="620" data-title="{{ dialog.title }}" data-open="dialog.open">
            <div data-ng-hide="dialog.showImportResults">
                <div id="Instructions">
                    <span>To import WBS elements, follow the steps below.</span>        
                </div>
                <div class="step one">
                    <div class="title">Step 1: Select what to import</div>
                    <div>
                        <input type="radio" class="radio" id="ImportType-New" data-ng-model="dialog.importExisting" data-ng-value="false" data-ng-change="showImportSteps()" />
                    <label for="ImportType-New">Import <i>only</i> new WBS elements</label>
                    </div>
                    <div>
                        <input id="ImportType-Existing" type="radio" class="radio" data-ng-model="dialog.importExisting" data-ng-value="true" data-ng-change="showImportSteps()" />
                        <label for="ImportType-Existing">Import new WBS elements <i>and</i> import updates to existing WBS elements.</label>
                    </div>
                </div>
                <div data-ng-show="dialog.showSteps">
                    <% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new { 
                                            controller = WebConstants.CONTROLLER_WBS, 
                                            action = WebConstants.ACTION_IMPORT_WBS, 
                                            workspace = SiteMasterUtilities.GetCurrentWorkspace()}, 
                                            FormMethod.Post,
                                            new { enctype = "multipart/form-data", id = "ImportWBSDialog-Form", target = "ImportWBSDialog-UploadTarget" }); %>
                    <div class="step two" data-ng-hide="dialog.importExisting">
                        <div class="title">Step 2: Download the WBS template file</div>
                        <div>Start by downloading the WBS template file.  This file has the correct column headings genBOE needs to import the WBS elements.</div>
                        <div>If you have a WBS in a different file, in order to use it, it must have the same exact headings as the WBS template file.  The file type must be an Excel .xlsx file.</div>
                        <div><a data-ng-click="export(true)">Download WBS template file</a></div>
                    </div>
                    <div class="step two" data-ng-show="dialog.importExisting">
                        <div class="title">Step 2: Export the existing WBS</div>
                        <div>Start by exporting the existing WBS.  This file has the correct column headings and WBS IDs genBOE needs to import new WBS elements.</div>
                        <div><a data-ng-click="export(false)">Export existing WBS</a></div>
                    </div>
                    <div class="step three" data-ng-hide="dialog.importExisting">
                        <div class="title">Step 3: Enter WBS elements into the file</div>
                        <div>Enter WBS elements into the file.  If you have a WBS in a different file, you can copy and paste WBS elements from it into the genBOE WBS template.  Make sure the WBS data you copy matches the column headings provided in the template.</div>
                        <div>
                            A WBS # and WBS Title must exist for all WBS elements.<br />
                            All WBS #s must be unique.<br />
                            CLINs must exist on the Manage CLINs page in order to associate a CLIN to a WBS.<br />
                            In the 'CLINs' column, enter only the CLIN #.  Separate multiple CLINs with a comma.  Example: 0001, 0002, 0003.<br />
                        </div>
                        <div class="important">
                            IMPORTANT: Do not change the column headings in the file.  Do not enter a value for genBOE WBS ID in column A.  genBOE WBS IDs are unique identifiers for WBS elements.  One will br automatically generated for each new WBS element once the import is complete.  Column A has been hidden to prevent accidental edits.  These need to be unchanged for the import to work.
                        </div>
                    </div>
                    <div class="step three" data-ng-show="dialog.importExisting">
                        <div class="title">Step 3: Enter/Update WBS elements in the file</div>
                        <div>Enter new WBS elements into the file or update existing WBS elements.  If you have a WBS in a different file, you can copy and paste WBS elements from it into the genBOE WBS template.  Make sure the WBS data you copy matches the column headings provided in the genBOE WBS file.</div>
                        <div>
                            A WBS # and WBS Title must exist for all WBS elements.<br />
                            All WBS #s must be unique.<br />
                            CLINs must exist on the Manage CLINs page in order to associate a CLIN to a WBS.<br />
                            In the 'CLINs' column, enter only the CLIN #.  Separate multiple CLINs with a comma.  Example: 0001, 0002, 0003.<br />
                        </div>
                        <div class="important">
                            IMPORTANT: Do not change the column headings and genBOE WBS IDs in the file.  The genBOE WBS IDs in column A are unique identifiers for WBS elements.  One will be automatically generated for each new WBS element once the import is complete.  Column A has been hidden to prevent accidental edits.  These need to be unchanged for the import to work.
                        </div>
                        <div class="important">If you delete a CLIN for a WBS that a BOE exists for, the CLIN will not be deleted.</div>
                    </div>
                     <div class="step four">
                        <div class="title">Step 4: Import the updated WBS template file</div>
                        <div>Choose a file to import.  The file you import must be an Excel file that ends in .xlsx or .xlsm.</div>
                        <div>
                            <input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
                            <input type="file" size="60" id="ImportDialog-File" name="file" onchange="angular.element(this).scope().fileUploadChange(this)"/>
                        </div>
                        <div id="OCINoteImport" class="oci-note">
                            <span data-ng-show="containsOCI"><b>Note:</b> <%: SiteMasterUtilities.GetBannerText(true) %></span>
                            <span data-ng-hide="containsOCI"><b>Note:</b> <%: SiteMasterUtilities.GetBannerText() %></span>
                        </div>
                        <div class="buttons">
                            <button id="ImportWBSDialog-ImportButton" type="button" class="ies-action" data-ng-hide="dialog.importWorking" data-ng-disabled="dialog.disableImport" data-ng-click="importWBSs()" name="import-button">Import</button>
                            <div id="ImportWBSDialog-ImportLoader" class="loader" data-ng-show="dialog.importWorking"></div>
                            <button id="ImportWBSDialog-ImportCancelButton" type="button" class="ies" data-ng-click="toggleImport()">Cancel</button>
                        </div>
                    </div>
                    <% Html.EndForm(); %>
                </div>
            </div>

            <div id="ImportResults" data-ng-show="dialog.showImportResults" class="import-verification">
                <div class="content import-verification"></div>
                <div class="buttons">
                    <button id="Back-WorkofflineImportVerification" type="button" class="ies" data-ng-click="backFromImport()" name="back-button">Back</button>
                    <button id="CompleteImportButton-WorkofflineImportVerification" class="ies-action" data-ng-if="!dialog.invalidData" data-ng-hide="dialog.completeImportWorking" data-ng-click="completeImportWBSs()">Complete Import</button>
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
