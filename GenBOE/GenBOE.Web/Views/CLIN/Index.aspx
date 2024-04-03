<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Manage CLINs - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%: Styles.Render("~/Content/genCss") %>
<%: Scripts.Render("~/bundles/manageCLIN") %>
<script type="text/javascript">

	var dateShiftUrl = CreatePostURL(
		'<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
		'<%: WebConstants.CONTROLLER_DATESHIFT %>',
		'<%: WebConstants.ACTION_INDEX %>',
		'id/');

	app.value('ManageCLINModel', {
		workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
		controller: '<%:WebConstants.CONTROLLER_CLIN %>',
		action: '<%:WebConstants.ACTION_GET_MANAGE_CLIN_MODEL %>',
		saveAction: '<%:WebConstants.ACTION_SAVE_CLIN %>',
		deleteAction: '<%:WebConstants.ACTION_DELETE_CLINS %>',
		exportAction: '<%:WebConstants.ACTION_EXPORT_CLINS %>',
		exportTemplateAction: '<%:WebConstants.ACTION_EXPORT_MANAGE_BOE_TEMPLATE %>',
		workspaceState: '<%: ((GenBOEMasterModelView)Model).WorkspaceState %>',
		completeImportAction: '<%: WebConstants.ACTION_COMPLETE_IMPORT_CLINS %>',
		getBOECountForClinAction: '<%: WebConstants.ACTION_GET_BOE_COUNT_FOR_CLIN%>',
		dateShiftUrl: dateShiftUrl,
		clinLevel: '/level/<%: ((int)IES.Common.Level.CLIN).ToString() %>',
	});
	var ManageCLINWidget;
	
	$(function () {
		$(".main").addClass("clin");

		// Create widget
		var widgetConfig = {};
		widgetConfig.ContextID = "ManageCLINs";
		widgetConfig.isReadOnly = false; // used so that way we can manually override readonly behaviors
		widgetConfig.IsModule = true;

		ManageCLINWidget = new GenWidget(widgetConfig);
		ManageCLINWidget.ImportedData = undefined;
	});
</script>
<div data-ng-controller="ManageCLINController" data-ng-cloak="">
	<div id="ManageCLIN" class="manage-clin module">
		<div class="module-header-data">Manage CLINs</div>
		<div class="module-content-data">
			<div class="form-row">
				<span>Add new or edit CLIN elements.</span>
				<div class="noteMessage" data-ng-show="isDataFiltered()">You are viewing filtered data. <a data-ng-click="clearAllFilters()">Click here</a> to reset all your filters.</div>
			</div>
			<div class="form-row css3pie-position-fix">
				<gen-validation data-errors="errors"></gen-validation>
				<div class="buttons float-left" style="line-height: 28px;width: 600px;">
					<div class="loader display-none" id="ManageCLIN-DeleteLoader"></div>
					<button class="ies-action" data-ng-disabled="disableSelected()" data-ng-click="delete()" data-ng-show="isWorkingState" name="delete-button" type="button" id="ManageCLIN-Delete">Delete</button>
					<button class="ies-action" data-ng-show="isWorkingState" data-ng-disabled="isLoading" data-ng-click="AddCLIN()" id="ManageCLIN-Add" type="button">+ Add</button>
					<button class="ies-action" data-ng-show="isWorkingState" data-ng-disabled="isLoading" data-ng-click="toggleImport()" id="ManageCLIN-Import" name="import-button" type="button">Import</button>
					<button class="ies-action" data-ng-click="export(false)" data-ng-disabled="isLoading || isExporting" id="ManageCLIN-Export" type="button">Export</button>
				</div>
				<div class="search-box float-right">
					<input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
					<div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
				</div>
			</div>
			<div class="form-row">
				<div class="manage-clin-grid">
					<table id="ManageCLINGrid" class="readonly grid" width="962">
						<thead>
							<tr>
								<th class="select-checkbox"><input type="checkbox" class="checkbox" data-ng-show="isWorkingState" data-ng-disabled="isLoading" id="SelectAllCheckbox" data-ng-model="selectAll" data-ng-change="toggleSelectAll()" /></th>
								<th class="clin-number bootstrap"><a data-ng-click="changeSorting(columns.clinNumber)" data-ng-class="{ 'bold': boldSort(columns.clinNumber) }">CLIN #</a>
										<a data-ng-click="toggleFilter(columns.clinNumber)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
								</th>
								<th class="clin-title bootstrap"><a data-ng-click="changeSorting(columns.title)" data-ng-class="{ 'bold': boldSort(columns.title) }">CLIN Title</a>
									<a data-ng-click="toggleFilter(columns.title)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
								</th>
								<th class="clin-title bootstrap"><a data-ng-click="changeSorting(columns.inUse)" data-ng-class="{ 'bold': boldSort(columns.inUse) }">In Use</a>
									<a data-ng-click="toggleFilter(columns.inUse)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
								</th>
								<th class="clin-title bootstrap" data-ng-hide="hideContractType"><a data-ng-click="changeSorting(columns.contract)" data-ng-class="{ 'bold': boldSort(columns.contract) }">Contract Type</a>
									<a data-ng-click="toggleFilter(columns.contract)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
								</th>
								<th class="start-date bootstrap"><a data-ng-click="changeSorting(columns.start)" data-ng-class="{ 'bold': boldSort(columns.start) }">Start Date</a>
									<a data-ng-click="toggleFilter(columns.start)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
								</th>
								<th class="end-date last-child bootstrap"><a data-ng-click="changeSorting(columns.end)" data-ng-class="{ 'bold': boldSort(columns.end) }">End Date</a>
									<a data-ng-click="toggleFilter(columns.end)"><i class="glyphicon glyphicon-filter"></i> Filters</a>
								</th>
							</tr>
						</thead>
						<tbody>
							<tr data-ng-show="isLoading"><td colspan="{{colSpan}}"><div class="loader"></div></td></tr>
							<tr data-ng-show="!isLoading && (data.length === 0 || filteredResults.length === 0)"><td colspan="{{colSpan}}"><div class="empty-grid-text">There are no CLINs.</div></td></tr>
							<tr pkid="{{::clin.ClinID}}" data-ng-repeat="clin in (filteredResults = (data | filter:filterCLINs | orderBy:predicate:reverse)) | limitTo:pageSize:currentPage*pageSize">
								<td>
									<input class="checkbox delete-chck" type="checkbox" data-ng-if="isWorkingState" data-ng-model="clin.Selected" data-ng-click="$event.stopPropagation()" />
								</td>
								<td class="text clin-number" title="{{::clin.ClinNumber}}" name="EditBoeLink">
									<a data-ng-click="editCLIN(clin)" name="ClinNumber">{{::clin.ClinNumber}}</a>
								</td>
								<td class="text clin-title" title="{{::clin.ClinTitle}}" name="EditBoeLink">
									<a data-ng-click="editCLIN(clin)" name="ClinTitleGrid">{{::clin.ClinTitle}}</a>
								</td>
								<td>
									{{::clin.InUseText}}
								</td>
								<td class="text contract-type" data-ng-hide="hideContractType">
									{{::clin.ContractTypeText}}
								</td>
								<td class="text start-date">
									{{::clin.StartDate}}
								</td>
								<td class="text end-date">
									{{::clin.EndDate}}
								</td>
							</tr>
						</tbody>
					</table>
				</div>
			</div>
			<div class="form-row last-form-row" style="height:28px">
				<div class="search-box inline full-width float-right">
					<input type="text" class="filter" data-ng-model="searchText" data-ng-model-options="{ debounce: 200 }" data-ng-change="searchChanged()" placeholder="Search..." style="float: right" />
					<div class="paging-control" genpaging data-num-pages="{{ numberOfPages(filteredResults) }}" data-current-page="currentPage"></div>
				</div>
			</div>
		</div>
	
		<div gen-dialog id="FilterOptions" class="dialog form" data-width="400" data-height="300" data-title="Apply Filters" data-open="filter.open" data-on-close="onClose()">
			<div class="options clin" data-ng-show="filter.filterColumn === columns.clinNumber">
				<span>Filter down by Clin #:</span>
				<ul>
					<li data-ng-repeat="item in filter.clinNumber">
						<label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
					</li>
				</ul>
			</div>
			<div class="options clin" data-ng-show="filter.filterColumn === columns.title">
				<span>Filter down by CLIN Title:</span>
				<ul>
					<li data-ng-repeat="item in filter.title">
						<label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
					</li>
				</ul>
			</div>
			<div class="options clin" data-ng-show="filter.filterColumn === columns.contract">
				<span>Filter down by Contract Type:</span>
				<ul>
					<li data-ng-repeat="item in filter.contract">
						<label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
					</li>
				</ul>
			</div>
			<div class="options clin" data-ng-show="filter.filterColumn === columns.inUse">
				<span>Filter down by In Use:</span>
				<ul>
					<li data-ng-repeat="item in filter.inUse">
						<label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
					</li>
				</ul>
			</div>
			<div class="options clin" data-ng-show="filter.filterColumn === columns.start">
				<span>Filter down by Start Date:</span>
				<ul>
					<li data-ng-repeat="item in filter.start">
						<label><input type="checkbox" data-ng-model="item.checked" data-ng-click="checkboxDirty($index, item)" />{{::item.display}}</label>
					</li>
				</ul>
			</div>
			<div class="options clin" data-ng-show="filter.filterColumn === columns.end">
				<span>Filter down by End Date:</span>
				<ul>
					<li data-ng-repeat="item in filter.end">
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

		<div gen-dialog id="CLINElementDialog" class="manage-clin-dialog" data-width="400" data-title="Manage CLIN" data-open="edit.open" data-on-close="onEditClose()">
			<% using (Html.BeginForm("", "ManageCLINModelView", FormMethod.Post, new { id = "CLINElementForm" }))
			   { %>

			<gen-validation data-errors="modalErrors"></gen-validation>
			<ul class="validation-box"></ul>
			<div class="form-row">
				<div class="form-label">CLIN #*</div>
				<div class="form-element">
					<span data-ng-hide="isWorkingState">{{::edit.clin.ClinNumber}}</span>
					<input data-ng-show="isWorkingState" type="text" data-ng-change="setEditDirty()" data-ng-model="edit.clin.ClinNumber" maxlength="50" />
				</div>
			</div>
			<div class="form-row">
				<div class="form-label">CLIN Title*</div>
				<div class="form-element">
					<span data-ng-hide="isWorkingState">{{::edit.clin.ClinTitle}}</span>
					<input data-ng-show="isWorkingState" class="clin-title" data-ng-change="setEditDirty()" spellcheck="true" type="text" data-ng-model="edit.clin.ClinTitle" maxlength="100" />
				</div>
			</div>
			<div class="form-row">
				<div class="form-label">Start Date</div>
				<div class="form-element">
					<span data-ng-hide="isWorkingState">{{::edit.clin.StartDate}}</span>
					<input type="text" id="StartDate" maxlength="10" data-ng-show="isWorkingState" data-ng-disabled="edit.clin.InUse" placeholder="mm/yyyy" data-ng-change="setEditDirty()" data-ng-model="edit.clin.StartDate" />
					<a name="AdjustDatesLink" data-ng-click="adjustDateClin(edit.clin.ClinID)" data-ng-show="edit.clin.InUse && isWorkingState">Adjust Dates</a>
				</div>
			</div>
			<div class="form-row">
				<div class="form-label">End Date</div>
				<div class="form-element">
					<span data-ng-hide="isWorkingState">{{::edit.clin.EndDate}}</span>
					<input type="text" id="EndDate" maxlength="10" data-ng-show="isWorkingState" data-ng-disabled="edit.clin.InUse" placeholder="mm/yyyy" data-ng-change="setEditDirty()" data-ng-model="edit.clin.EndDate" />
				</div>
			</div>
			<div class="form-row show-hide" data-ng-if="!hideContractType">
				<div class="form-label bootstrap">Contract Type
					<div id="helpIcon" class="help-icon" tooltip-placement="right" tooltip-class="text-left" tooltip-popup-delay='500' uib-tooltip="Select the applicable contract type for this CLIN. Note: This list ONLY includes the contract types selected on the Workspace Identification screen. If additional contract types are required you must first select them on the Workspace Identification screen."></div>
				</div>
				<div class="form-element">
					<select data-ng-model="edit.clin.ContractType" data-ng-disabled="!isWorkingState" data-ng-change="setEditDirty()">
						<option data-ng-repeat="contractType in gridModel.ContractTypeList" value="{{::contractType.Id}}">{{::contractType.Text}}</option>
					</select>
				</div>
			</div>
			<div class="form-label"></div>
			<div class="form-element">
				<div id="OCINote" class="oci-note">
					<span data-ng-show="gridModel.ContainsOCI"><b>Note:</b> Must not contain any classified, export controlled or third party proprietary information.</span>
					<span data-ng-hide="gridModel.ContainsOCI"><b>Note:</b> Must not contain any OCI, classified, export controlled or third party proprietary information.</span>
				</div>
				<div class="button-container">
					<div class="buttons">
						<button data-ng-click="save(false, true)" data-ng-show="isWorkingState" data-ng-disabled="!edit.isDirty" name="save-button" type="button" class="ies-action stateful_button">Save</button>
						<button data-ng-click="save(true, true)" data-ng-show="isWorkingState" data-ng-disabled="!edit.isDirty" name="save-add-another-button" type="button" class="ies stateful_button">Save &amp; add another</button>
					</div>
				</div>
			</div>
			<% } %>
		</div>

		<div gen-dialog  id="ImportCLINDialog" class="import-clin dialog form" data-width="680" data-height="640" data-title="{{ dialog.title }}" data-open="dialog.open">
			<div id="ImportInstructions" data-ng-hide="dialog.showImportResults" >
				<span>To import CLINs, follow the steps below.</span>
				<% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new
						   {
							   controller = WebConstants.CONTROLLER_CLIN,
							   action = WebConstants.ACTION_IMPORT_CLINS,
							   workspace = SiteMasterUtilities.GetCurrentWorkspace()
						   },
											FormMethod.Post,
											new { enctype = "multipart/form-data", id = "ImportCLINDialog-Form", target = "ImportCLINDialog-UploadTarget" }); %>
				<div class="container">
					<div class="step one">
						<div class="title">Step 1: Select what to import</div>
						<div>
							<input type="radio" class="radio" data-ng-model="dialog.importExisting"  data-ng-value="false" data-ng-change="showImportSteps()" />
							<label for="ImportType-New">Import <i>only</i> new CLINs</label>
						</div>
						<div>
							<input type="radio" class="radio" data-ng-model="dialog.importExisting" data-ng-value="true" data-ng-change="showImportSteps()"  />
							<label for="ImportType-Existing">Import new CLINs <i>and</i> import updates to existing CLINs.</label>
						</div>
					</div>
					<div data-ng-show="dialog.showSteps">
						<div class="step two" data-ng-hide="dialog.importExisting">
							<div class="title">Step 2: Download the CLIN template file</div>
							<div>Start by downloading the CLIN template file. This file has the correct column headings genBOE needs to import the CLINs.</div>
							<div>If you have CLINs in a different file, in order to use it, it must have the same exact headings as the CLIN template file. The file type must be an Excel .xlsx file.</div>
							<div><a href="<%= this.ResolveClientUrl("~/Templates/Export/CLINs.xlsx") %>">Download CLIN template file</a></div>
						</div>
						<div class="step two" data-ng-show="dialog.importExisting">
							<div class="title">Step 2: Export the existing CLINs</div>
							<div>Start by exporting the existing CLINs. This file has the correct column headings and CLIN IDs genBOE needs to import new CLINs and updates.</div>
							<div><a id="ManageCLIN-ExportLink" data-ng-click="export(false)">Export existing CLINs</a></div>
						</div>
						<div class="step three" data-ng-hide="dialog.importExisting">
							<div class="title">Step 3: Enter CLINs into the file</div>
							<div>Enter CLINs into the file. If you have a CLIN in a different file, you can copy and paste CLINs from it into the genBOE CLIN template. Make sure the CLIN data you copy matches the column headings provided in the template.</div>
							<ul>
								<li>A CLIN # and CLIN Title must exist for all CLINs.</li>
								<li>All CLIN #’s must be unique.</li>
								<li>Start and End dates must be in the following format:  m/yyyy or mm/yyyy.</li>
								<li>Start Date must be on or after Contract Start Date.</li>
								<li>End Date must be on or before Contract End Date.</li>
							</ul>
							<div class="important">
								IMPORTANT: Do not change the column headings or options in the file. Do not enter a value for genBOE CLIN ID in column A. genBOE CLIN IDs are unique identifiers for CLINs. One will be automatically generated for each new CLIN once the import is complete. Column A  and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.
							</div>
						</div>
						<div class="step three" data-ng-show="dialog.importExisting">
							<div class="title">Step 3: Enter/Update CLINs in the file</div>
							<div>Enter new CLINs into the file or update existing CLINs. If you have a CLIN in a different file, you can copy and paste CLINs from it into the genBOE CLIN file. Make sure the CLIN data you copy matches the column headings provided in the genBOE CLIN file.</div>
							<ul>
								<li>A CLIN # and CLIN Title must exist for all CLINs.</li>
								<li>All CLIN #’s must be unique.</li>
								<li>Start and End dates must be in the following format:  m/yyyy or mm/yyyy.</li>
								<li>Start Date must be on or after Contract Start Date.</li>
								<li>End Date must be on or before Contract End Date.</li>
							</ul>
							<div class="important">
								IMPORTANT: Do not change the column headings or options in the file. Do not enter a value for genBOE CLIN ID in column A. genBOE CLIN IDs are unique identifiers for CLINs. One will be automatically generated for each new CLIN once the import is complete. Column A  and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.
							</div>
							<div class="important">
								If you delete a CLIN from the file, the CLIN will not be removed in genBOE. To delete a CLIN, it must be deleted directly in genBOE to properly remove all associations.
							</div>
						</div>
						<div class="step four">
							<div class="title">Step 4: Import the updated CLIN template file</div>
							<div>Choose a file to import. The file you import must be an Excel file that ends in .xlsx or .xlsm.</div>
							<div>
								<input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
								<input type="file" size="60" id="ImportDialog-File" name="file"  onchange="angular.element(this).scope().fileUploadChange(this)" />
							</div>
						</div>
					</div>
				</div>
				<div id="OCINote" data-ng-show="dialog.showSteps" class="oci-note">
					<span data-ng-show="gridModel.ContainsOCI"><b>Note:</b> Must not contain any classified, export controlled or third party proprietary information.</span>
					<span data-ng-hide="gridModel.ContainsOCI"><b>Note:</b> Must not contain any OCI, classified, export controlled or third party proprietary information.</span>
				</div>
				<div>
					<div class="buttons" data-ng-show="dialog.showSteps">
						<button type="button" class="ies-action" data-ng-hide="dialog.importWorking" data-ng-disabled="dialog.disableImport" data-ng-click="importCLINs()" name="import-button">Import</button>
						<div class="loader" data-ng-show="dialog.importWorking"></div>
						<button type="button" class="ies" data-ng-click="toggleImport()">Cancel</button>
					</div>    
				</div>
				<% Html.EndForm(); %>
			</div>
			<div id="ImportResults" data-ng-show="dialog.showImportResults" class="import-verification">
				<span>The import file will make the following updates. To continue with the import,
				click <i>Complete Import</i>. Otherwise, click <i>Back</i> to import a different
				file or close this dialog window to not import a file</span>
				<gen-validation data-errors="dialog.errors"></gen-validation>
				<div class="container">
					<div class="title"><span>#</span> CLINs will be added:</div>
				<ul result="<%: (int)ClinImportResult.CreateClin %>"></ul>

				<div class="title"><span>#</span> CLINs will be updated. If the CLIN #, CLIN Title, Start Date or End Date was updated, Authors of BOEs associated with those CLINs will be notified. BOEs associated with these CLINs that are Awaiting Approval/Approved will be moved back to Draft:</div>
				<ul result="<%: (int)ClinImportResult.UpdateClin %>"></ul>

				<div class="title">CLINs will not be added/updated because a CLIN # and/or CLIN Title is missing:</div>
				<ul result="<%: (int)ClinImportResult.MissingNumberOrTitle %>"></ul>

				<div class="title"><span>#</span> CLINs will not be added/updated because the CLIN # must be unique:</div>
				<ul result="<%: (int)ClinImportResult.NonUniqueClinNumber %>"></ul>

				<div class="title"><span>#</span> CLINs will have their titles truncated because they exceed 100 characters:</div>
				<ul result="<%: (int)ClinImportResult.TruncateTitle %>"></ul>

				<div class="title"><span>#</span> CLINs will not be added/updated because their CLIN # exceeds 20 characters:</div>
				<ul result="<%: (int)ClinImportResult.ClinNumberTooLong %>"></ul>

				<div class="title"><span>#</span> CLIN Start Dates will not be added or updated because they are not in the format m/yyyy or mm/yyyy:</div>
				<ul result="<%: (int)ClinImportResult.InvalidStartDateFormat %>"></ul>

				<div class="title"><span>#</span> CLIN End Dates will not be added or updated because they are not in the format m/yyyy or mm/yyyy:</div>
				<ul result="<%: (int)ClinImportResult.InvalidEndDateFormat %>"></ul>

				<div class="title"><span>#</span> CLIN Start Dates will not be added or updated because they are not on or after the Contract Start Date:</div>
				<ul result="<%: (int)ClinImportResult.StartDateTooEarly %>"></ul>

				<div class="title"><span>#</span> CLIN End Dates will not be added or updated because they are not on or before the Contract End Date:</div>
				<ul result="<%: (int)ClinImportResult.EndDateTooLate %>"></ul>

				<div class="title"><span>#</span> CLIN dates will not be added or updated because the End date entered was not after the Start Date:</div>
				<ul result="<%: (int)ClinImportResult.StartDateMustBeBeforeEndDate %>"></ul>

				<div class="title"><span>#</span> CLIN dates will not be added or updated because only one valid CLIN date was entered:</div>
				<ul result="<%: (int)ClinImportResult.NoDatesOrBothDatesRequired %>"></ul>

				<div class="title"><span>#</span> CLINs that will not be added or updated because they are missing or have an invalid Contract Type:</div>
				<ul result="<%: (int)ClinImportResult.MissingInvalidContractType %>"></ul>
				</div>
				<div class="buttons">
					<button type="button" class="ies" data-ng-click="backFromImport()" name="back-button">Back</button>
					<button class="ies-action" data-ng-if="!dialog.invalidData" data-ng-hide="dialog.completeImportWorking" data-ng-click="completeImportCLINs()">Complete Import</button>
					<div class="loader" data-ng-show="dialog.completeImportWorking" style="width: 129px"></div>
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
