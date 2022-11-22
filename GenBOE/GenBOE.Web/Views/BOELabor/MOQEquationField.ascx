<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.MOQEquationModelView>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView.BOE" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>

<% 
    IEnumerable<WorkspaceVariableModelView> workspaceVariables = (IEnumerable<WorkspaceVariableModelView>)ViewData["WorkspaceVariables"];
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    int taskElementId = Model.TaskElementId > 0 ? Model.TaskElementId : -1;
    bool showMoqQuestions = Model.MoqTemplateAnswers.Any();;
    int numberMoqQuestions = showMoqQuestions ? Model.MoqTemplateAnswers.Count : 1;

    // these 2 fields, as well as the processing below is necessary to deal with the date serialization weirdness
    string dateFixRegexSearch = @"\""\\/Date\((-?\d+)\)\\/\""";
    string dateFixRegexReplace = "new Date($1)";
%>
<script type="text/javascript">
    var workspaceVariables = <%= serializer.Serialize(workspaceVariables) %>;
    var MOQEquationFieldWidget_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var rteFieldSize = <%:ViewBag.RteFieldSize%>;

    var MOQEquationFieldModel = {
        BaseUrl: '<%=this.ResolveClientUrl("~/")%>',
        MoqEquationLabel: '<%=Model.MOQEquationLabel%>',
        MoqEquationType: '<%=Model.TypeOfMoqEquation%>',
        MOQType: '<%=Model.MOQType%>',
        TaskElementId: <%=Model.TaskElementId > 0 ? Model.TaskElementId : -1%>,
        RteFieldSize: rteFieldSize,
        ShowSearchMetricsLink: '<%=Model.ShowSearchMetricsLink%>'.isTrue(),
        UsingTemplateBOE: '<%=Model.UsingTemplateBOE%>'.isTrue(),
        IsReadOnly: MOQEquationFieldWidget_ReadOnly,
        WorkspaceVariables: workspaceVariables,
        MOQTypes: <%=serializer.Serialize(Model.MOQTypes.Select(x => new { SelectedMOQType = x.Value, SelectedMOQTypeText = x.Text }))%>,
        MoqTypeTableDataLabels:<%=serializer.Serialize(Model.MoqTypeTableDataLabels)%>,
        MoqTypeHelpUrls:<%=serializer.Serialize(Model.MoqTypeHelpUrls)%>,
        SelectedMoqTypes:<%=Regex.Replace(serializer.Serialize(Model.SelectedMoqTypes), dateFixRegexSearch, dateFixRegexReplace)%>,
        ShouldMoqReadOnlyBeReversed: '<%:ViewData["ShouldMoqReadOnlyBeReversed"]%>'.isTrue(),
        IsRMS: '<%:Model.Company == CompanyConfiguration.MST%>'.isTrue(),
        HistoricalMoqType: <%:(int)MOQType.Historical%>,
        ComparativeMoqType: <%:(int)MOQType.Comparative%>,
        SAPEnabled: '<%:Utilities.IsSAPEnabled%>'.isTrue(),
        SapWebiRepository: '<%=RepositoryName.SapWebi.GetDescription()%>'
    };

    var ordinaryVariables = <%= serializer.Serialize(Model.TaskOrdinaryVariables) %>;
    var newOrdinaryVariableID = <%:(Model.TypeOfMoqEquation == MOQEquationType.Cost ? -250 : -1) %>

    var sumOfBoes = '<%:(int)VarValueType.SumOfBOEs %>';
    var discrete = '<%:(int)VarValueType.Discrete %>';
    var shouldMoqReadOnlyBeReversed = '<%:ViewData["ShouldMoqReadOnlyBeReversed"]%>'.isTrue();
    var isNotSubContractor = '<%:(bool)ViewData["IsSubContractor"] == false%>'.isTrue();
    var sortBOEByWBS = '<%:(int)VarSortBOEBy.WBS%>';
    var sortBOEByClin = '<%:(int)VarSortBOEBy.CLIN%>';

    var validationUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_BOE_LABOR%>',
                        '<%: WebConstants.ACTION_MOQ_VALIDATE%>', 'boe/' + '<%: ViewData["BOEID"] %>');
    var calculateMOQResultUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_BOE_LABOR%>',
                        '<%: WebConstants.ACTION_MOQ_CALCULATE%>', 'boe/' + '<%: ViewData["BOEID"] %>');
    var openSumOfBoesByWbsUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
                        '<%:WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_WBS %>',
                        'boe/<%: (int)ViewData["BOEID"] %>');
    var openSumOfBoesByClinUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
                        '<%:WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_CLIN %>',
                        'boe/<%: (int)ViewData["BOEID"] %>');

    // Initializes the widget. Has to be called after Angular is done initializing / binding, instead of on page load. This is necessary here due to the DOM complexity
    initializeWidget = function () {
        MOQEquationFieldWidget = InitializeMOQEquationFieldWidget(MOQEquationFieldWidget_ReadOnly, workspaceVariables, ordinaryVariables, newOrdinaryVariableID, sumOfBoes, discrete,
            validationUrl, shouldMoqReadOnlyBeReversed, calculateMOQResultUrl, openSumOfBoesByWbsUrl, openSumOfBoesByClinUrl, isNotSubContractor, sortBOEByWBS, sortBOEByClin);

        MOQEquationFieldWidget.AfterDomLoad(TaskElementDetailsWidget, '<%:showMoqQuestions%>'.isTrue(), <%:numberMoqQuestions%>);
    }
</script>

<div id="MOQEquationField" class="bootstrap" data-ng-controller="MoqEquationController" data-ng-init="init()">
    <div class="form-row">
        <div class="form-label">{{model.MoqEquationLabel}} **</div>
        <div class="form-element">
            <div uib-dropdown class="btn-group">
                <div id="dropdown-magnifier" uib-dropdown-toggle class="magnifier-button"></div>
                <ul class="uib-dropdown-menu dropdown-menu" role="menu" aria-labelledby="dropdown-magnifier">
                    <li><a id="InsertWorkspaceVariableLink" data-ng-click="InsertWorkspaceVariableClicked()">Insert Workspace Variable</a></li>
                    <li data-ng-if="model.ShowSearchMetricsLink"><a id="SearchEstimatingCatalogLink" data-ng-click="SearchEstimatingCatalogClicked()">Search Estimating Catalog</a></li>
                    <!-- Hiding for BOEJ-4829. Will have to be unhidden, or removed in the future (2020.6.x) -->
                    <li data-ng-if="false"><a id="CopyMoqFromBoeLink" data-ng-click="CopyMoqFromBoeClicked()">Copy MOQ from BOE</a></li>
                </ul>
            </div>
            <input type="text" id="MOQEquation" name="MOQEquation" value="<%: Model.MOQEquation %>" class="moq-equation-input" maxlength="250" />
            <div>
                <input id="MOQDefaultSizes" value="" type="hidden" />
                <input id="HistoricalMetricEquation" value="" type="hidden" />
            </div>
        </div>
        <span class="moq-equation-equals">=</span>
        <span data-ng-if="model.IsCostEquation">$</span>
        <div id="equals" class="form-element moq-equation-result"></div>
        <div class="clear"></div>
        <div class="form-label"></div>
        <div class="form-element LockedWorkspaceState" style="display: inline-block; margin-top: 2px;"></div>
    </div>
    <div id="MOQEquation-Error" class="form-row" style="display: none;">
        <div class="form-label"></div>
        <div class="form-element">
            <div class="validation-box" style="width: 520px; display: block">
                <div>
                    <div id="MOQEquation-ErrorTitle"><b>Invalid MOQ Equation.</b></div>
                    <div id="MOQEquation-ErrorText"></div>
                </div>
                <div class="clear"></div>
            </div>
        </div>
    </div>
    <table id="MOQEquation-Variables" class="moqVariables"></table>
    <div class="moqVariableNote clear display-none"><b>Note:</b> The value of Variables that are the sum of select BOEs will change if the <i>Totals</i> of the select BOEs change.</div>

    <div data-ng-if="model.UsingTemplateBOE" class="form-row">
        <div class="form-label">
            <span>MOQ Type(s) *</span>
        </div>
        <div id="AddMOQWarning" class="form-element" data-ng-hide="model.SelectedMoqTypes.length > 0">
            Please add MOQ Type(s).
        </div>
    </div>
    <div id="moqTypes" data-ng-if="model.UsingTemplateBOE" class="form-element moqRteFieldContainer moqContainerClass" data-ng-repeat="moqType in model.SelectedMoqTypes | orderBy: 'Order'">
        <div class="form-row" data-ng-class="{'collapsedBorder': moqType.collapsed}">
            <div class="form-label"  data-ng-class="{'comparativeLabel':  moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>, 'historicalLabel': moqType.SelectedMOQType == <%:(int)MOQType.Historical%>}">
                <a data-nodrag="" data-ng-click="toggle(moqType)">
                    <div class="moqTypeHeader" data-ng-class="{'collapsed': moqType.collapsed, 'expanded': !moqType.collapsed}"></div>
                </a>
                {{moqType.SelectedMOQTypeText}}
            </div>
            <div class="btn-group">
                <button data-ng-if="!ActualReadOnly() && moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-disabled="disableHistoricalComparativeConvertButtons()" data-ng-click="convertMoqType(moqType, <%:(int)MOQType.Historical%>)" class="ies moqTypesButton" type="button">Convert to Historical</button>
                <button data-ng-if="!ActualReadOnly() && moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-disabled="disableHistoricalComparativeConvertButtons()" data-ng-click="convertMoqType(moqType, <%:(int)MOQType.Comparative%>)" class="ies moqTypesButton" type="button">Convert to Comparative</button>
                <button data-ng-if="!ActualReadOnly() && (moqType.SelectedMOQType == <%:(int)MOQType.Comparative%> || moqType.SelectedMOQType == <%:(int)MOQType.Historical%>)" data-ng-disabled="isDirty()" data-ng-class="{disabled: isDirty()}" data-ng-click="openImportMoqTables(moqType)" class="ies-action moqTypesButton" type="button">Import</button>
                <button data-ng-if="!ActualReadOnly() && (moqType.SelectedMOQType == <%:(int)MOQType.Comparative%> || moqType.SelectedMOQType == <%:(int)MOQType.Historical%>)" data-ng-disabled="isDirty() || isExporting" data-ng-class="{disabled: isDirty() || isExporting}" data-ng-click="exportMoqTables(moqType)" class="ies-action moqTypesButton" type="button">Export</button>
                <button data-ng-if="!ActualReadOnly()" data-ng-click="RemoveMoqType(moqType)" class="ies-danger moqTypesButton" type="button">Delete MOQ Type</button>
            </div>
        </div>
        <div class="moqTaskBasedOn" data-ng-show="!moqType.collapsed">
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>">{{PortionOfTask()}} historical data:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>">{{PortionOfTask()}} similar historical data:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%>">{{PortionOfTask()}} a Cost Estimating Relationship (CER). Please provide:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%>">{{PortionOfTask()}} Parametric Model/tool. Please provide:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>">{{PortionOfTask()}} Analogous Relationship. Please provide:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SOW%>">{{PortionOfTask()}} a Statement of Work (SOW) directive:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%>">{{PortionOfTask()}} a Level of Effort (LOE):</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">{{PortionOfTask()}} Subject Matter Expert (SME) Judgment.</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.NonLabor%>">This task is Non-Labor:</span>
        </div>
		<div data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%> || moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>">
			<div class="tableDataParent" data-ng-repeat="tableData in moqType.TableData | orderBy: 'Order'">
               <gen-validation data-ng-if="actualsValidation.errors.get(tableData.Id)" data-errors="actualsValidation.errors.get(tableData.Id)"></gen-validation>
				<div class="tableData">
                    <table pkid="{{tableData.Id}}">
                        <tr>
                            <td class="form-label">
                                <div class="moqTypeHeader" data-ng-class="{'collapsed': tableData.collapsed, 'expanded': !tableData.collapsed}" data-ng-click="toggle(tableData)"></div>
                                {{model.MoqTypeTableDataLabels.TableName}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TableNameHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TableNameComparativeSuffix);"></div>
                            </td>
                            <td><input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="text" required data-ng-model="tableData.TableName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="!model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.RepositoryName}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.RepositoryNameHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.RepositoryNameComparativeSuffix);"></div>
                            </td>
                            <td>
                                <select data-ng-readonly="ActualReadOnly()" class="skip-read-only" required data-ng-model="tableData.RepositoryNameSelection" data-ng-change="UpdateRepository(tableData)">
                                    <option value=""></option>
                                    <option value="<%: RepositoryName.SapWebi.GetDescription() %>"><%: RepositoryName.SapWebi.GetDescription() %></option>
                                    <option value="<%: RepositoryName.Other.GetDescription() %>"><%: RepositoryName.Other.GetDescription() %></option>
                                </select>
                                <input data-ng-if="tableData.RepositoryNameSelection == '<%: RepositoryName.Other.GetDescription() %>'" data-ng-readonly="ActualReadOnly()" class="skip-read-only repository-name-other" type="text" required data-ng-model="tableData.RepositoryName" />
                            </td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="!model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.QueryType}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.QueryTypeHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.QueryTypeComparativeSuffix);"></div>
                            <td>
                                <select data-ng-disabled="ActualReadOnly()" class="skip-read-only" required data-ng-model="tableData.QueryType" data-ng-change="clearPoPDates(tableData)">
                                    <option value=""></option>
                                    <option value="<%: MoqTableData.MONTHLY%>"><%: MoqTableData.MONTHLY%></option>
                                    <option value="<%: MoqTableData.WEEKLY%>"><%: MoqTableData.WEEKLY%></option>
                                </select>
                            </td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.HistoricalProgramName}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.HistoricalProgramNameHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.HistoricalProgramNameComparativeSuffix);"></div>
                            </td>
                            <td><input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="text" required data-ng-model="tableData.HistoricalProgramName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.WbsElement}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.WBSElementHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.WBSElementComparativeSuffix);"></div>
                            </td>
                            <td><textarea data-ng-readonly="ActualReadOnly()" class="skip-read-only" cols="20" required data-ng-model="tableData.WbsElement" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.PoPStart}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPStartHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPStartComparativeSuffix);"></div>
                            </td>
                            <td>
                                <input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="date"  data-ng-if="model.IsRMS" required data-ng-model="tableData.PoPStart" data-ng-class="{'ng-invalid': ValidatePopStart(tableData.PoPStart) }" onchange="MOQEquationFieldWidget.setDirty()" />
                                <input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="month" data-ng-if="!model.IsRMS && tableData.QueryType === '<%: MoqTableData.MONTHLY%>'" required data-ng-model="tableData.PoPStart" onchange="MOQEquationFieldWidget.setDirty()" />

                                <span data-ng-if="!model.IsRMS && tableData.QueryType === '<%: MoqTableData.WEEKLY%>'" >
                                    FW <input data-ng-readonly="ActualReadOnly()" type="number" class="weekYear skip-read-only" min="1" max="53" step="1" required data-ng-model="tableData.PoPStartWeek" onchange="MOQEquationFieldWidget.setDirty()" />
                                    Year <input data-ng-readonly="ActualReadOnly()" type="number" class="weekYear skip-read-only" min="1980" max="2050" step="1" required data-ng-model="tableData.PoPStartYear" onchange="MOQEquationFieldWidget.setDirty()" />
                                </span>
                            </td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.PoPEnd}} *                                
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPEndHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPEndComparativeSuffix);"></div>
                            </td>
                            <td>
                                <input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="date"  data-ng-if="model.IsRMS" required data-ng-model="tableData.PoPEnd" data-ng-class="{'ng-invalid': ValidatePopEnd(tableData.PoPEnd) }" onchange="MOQEquationFieldWidget.setDirty()" />
                                <input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="month" data-ng-if="!model.IsRMS && tableData.QueryType === '<%: MoqTableData.MONTHLY%>'" required data-ng-model="tableData.PoPEnd" onchange="MOQEquationFieldWidget.setDirty()" />
                                
                                <span data-ng-if="!model.IsRMS && tableData.QueryType === '<%: MoqTableData.WEEKLY%>'" >
                                    FW <input data-ng-readonly="ActualReadOnly()" type="number" class="weekYear skip-read-only" min="1" max="53" step="1" required data-ng-model="tableData.PoPEndWeek" onchange="MOQEquationFieldWidget.setDirty()" />
                                    Year <input data-ng-readonly="ActualReadOnly()" type="number" class="weekYear skip-read-only" min="1980" max="2050" step="1" required data-ng-model="tableData.PoPEndYear" onchange="MOQEquationFieldWidget.setDirty()" />
                                </span>
                            </td>
                        </tr>
                        <tr data-ng-repeat="customField in MoqTableCustomFields" data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{customField.CustomFieldMetaData.FieldName}}{{(customField.CustomFieldMetaData.isRequired) ? ' *' : ''}}</td>
                            <td class="custom-field">{{ selectedItem = findMoqTableCustomFieldValue(customField, tableData); "" }}
                                <input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="text" onchange="MOQEquationFieldWidget.setDirty(); validateMoqTableCustomField(this);" data-ng-required="customField.CustomFieldMetaData.isRequired" data-ng-class="{'ng-invalid': customField.CustomFieldMetaData.isRequired && selectedItem.openEndedValue.length==0}" data-ng-value="selectedItem.openEndedValue" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" customfieldvalueid="{{selectedItem.openEndedId}}" selectionid="{{selectedItem.selectedID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="true" />
                            </td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.DateOfReport}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.DateOfReportHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.DateOfReportComparativeSuffix);"></div>
                            </td>
                            <td><input data-ng-readonly="ActualReadOnly() || IsSapEnabledAndSetAsRepository(tableData.RepositoryName)" class="skip-read-only" type="date" required data-ng-model="tableData.DateOfReport" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.ContractNumber}} * <div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.ContractNumberSuffix);"></div></td>
                            <td><input data-ng-readonly="ActualReadOnly()" class="skip-read-only" type="text" required data-ng-model="tableData.ContractNumber" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.TotalWbsHours}} * <div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.TotalWBSHoursSuffix);"></div></td>
                            <td><input data-ng-readonly="ActualReadOnly() || IsSapEnabledAndSetAsRepository(tableData.RepositoryName)" class="skip-read-only" type="number" required data-ng-model="tableData.TotalWbsHours" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.AdditionalQueryFilters}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.AdditionalQueryFiltersHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.AdditionalQueryFiltersComparativeSuffix);"></div>
                                <button data-ng-if="!ActualReadOnly() && IsSapEnabledAndSetAsRepository(tableData.RepositoryName)" type="button" class="ies-action moqTypesButton sapButton" data-ng-click="ShowFilterDialog(tableData)">Update Filters</button>
                            </td>
                            <td><textarea data-ng-readonly="ActualReadOnly()" class="skip-read-only" cols="20" required data-ng-model="tableData.AdditionalQueryFilters" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.TotalRelevantHours}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TotalRelevantHoursHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TotalRelevantHoursComparativeSuffix);"></div></td>
                            <td><input data-ng-readonly="ActualReadOnly() || IsSapEnabledAndSetAsRepository(tableData.RepositoryName)" class="skip-read-only" type="number" required min="0" data-ng-model="tableData.TotalRelevantHours" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                   </table>
                </div>
                <div class="tableDataButtons">
                    <button data-ng-if="!ActualReadOnly() && $index == 0" data-ng-click="CreateNewTable(moqType.TableData)" type="button" class="ies-action moqTypesButton">Add Table Data</button>
                    <button data-ng-if="!ActualReadOnly() && $index == 0" data-ng-disabled="moqType.TableData.length <= 1" data-ng-click="displayReOrderMoqTablesDialog(moqType)" class="moqTypesButton ies-blue" type="button">Sort MOQ Tables</button>
                    <button data-ng-if="!ActualReadOnly() && moqType.TableData.length > 1" style="display:block;" data-ng-click="RemoveTable(tableData, moqType.TableData)" type="button" class="ies-danger moqTypesButton" data-ng-class="{'moqTypesDelete': $index == 0}">Delete Table Data</button>
                    <button data-ng-if="!ActualReadOnly() && IsSapEnabledAndSetAsRepository(tableData.RepositoryName)" type="button" class="ies-action moqTypesButton sapButton" data-ng-click="exportActuals(tableData)">Export Actuals</button>
                    <button data-ng-if="!ActualReadOnly() && IsSapEnabledAndSetAsRepository(tableData.RepositoryName)" type="button" class="ies-action moqTypesButton sapButton" data-ng-click="validateActuals(tableData)">Validate Actuals</button>
                    <button data-ng-if="!ActualReadOnly() && IsSapEnabledAndSetAsRepository(tableData.RepositoryName)" type="button" class="ies-action moqTypesButton sapButton" data-ng-click="calculateActuals(tableData)">Calculate Actuals</button>
                </div>
                <hr />
            </div>
            <div data-ng-if="!model.IsRMS" class="moqTypeNote">Note: Hours cited above are paid hours that exclude unpaid (zero cost) hours and service center hours.</div>
        </div>
		<div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%> || moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%> || moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>">
            <div class="form-label">
                <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%>">CER</span>
                <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%>">Parametric model or tool</span>
                <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>">Analogous relationship</span>
                 name: * 
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.CERNameSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PENameSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.ARNameSuffix);"></div>
            </div>
            <div class="form-element">
                <input data-ng-readonly="ActualReadOnly()" type="text" class="cerPmArTextBox skip-read-only" required data-ng-model="moqType.CerName" />
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%> || moqType.SelectedMOQType == <%:(int)MOQType.SOW%>">
            <div class="form-label" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%>">
                <span>Description of Hours required: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.LOEDescriptionSuffix);"></div>
            </div>
            <div class="form-label" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SOW%>">
                <span>Description of Hours required & location in SOW: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SOWDescriptionSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!ActualReadOnly()" cols="20" name="DescriptionHoursRequired_{{moqType.SelectedMOQType}}" data-ng-model="moqType.DescriptionHoursRequired"></textarea>
                <span data-ng-if="ActualReadOnly()" data-ng-bind-html="moqType.DescriptionHoursRequired"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The SME selected Expert judgment for this basis of estimate for the following reasons: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SMEReasonsSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!ActualReadOnly()" cols="20" name="SmeReason_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeReason"></textarea>
                <span data-ng-if="ActualReadOnly()" data-ng-bind-html="moqType.SmeReason"></span>
            </div>           
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <b>For the tasks below:</b>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The logic and assumptions used to estimate hours is: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SMEHoursLogicSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!ActualReadOnly()" cols="20" name="SmeHoursLogic_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeHoursLogic"></textarea>
                <span data-ng-if="ActualReadOnly()" data-ng-bind-html="moqType.SmeHoursLogic"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The logic and assumptions used to estimate duration is: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SMEDurationLogicSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!ActualReadOnly()" cols="20" name="SmeDurationLogic_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeDurationLogic"></textarea>
                <span data-ng-if="ActualReadOnly()" data-ng-bind-html="moqType.SmeDurationLogic"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The following tasks are estimates in this BOE: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SMETasksSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!ActualReadOnly()" cols="20" name="SmeTaskEstimates_{{moqType.SelectedMOQType}}" placeholder="{{MoqTypesPlaceholder('Rationale', moqType.SelectedMOQType)}}" data-ng-model="moqType.SmeTaskEstimates"></textarea>
                <span data-ng-if="ActualReadOnly()" data-ng-bind-html="moqType.SmeTaskEstimates"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType != <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>Rationale: *</span>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.HistoricalRationaleSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.ComparativeRationaleSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.CerRationaleSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PeRationaleSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.ArRationaleSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SOW%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.SowRationaleSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.LoeRationaleSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.NonLabor%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.NonLaborRationaleSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!ActualReadOnly()" cols="20" name="Rationale_{{moqType.SelectedMOQType}}" placeholder="{{MoqTypesPlaceholder('Rationale', moqType.SelectedMOQType)}}" data-ng-model="moqType.Rationale"></textarea>
                <span data-ng-if="ActualReadOnly()" data-ng-bind-html="moqType.Rationale"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType != <%:(int)MOQType.NonLabor%>">
            <div class="form-label">
                <span>Skill Mix Rationale: *</span>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.HistoricalSkillMixSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.ComparativeSkillMixSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.CerSkillMixSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PeSkillMixSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.ArSkillMixSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SOW%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.SowSkillMixSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.LoeSkillMixSuffix);"></div>
                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.SmeSkillMixSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!ActualReadOnly()" cols="20" name="SkillMixRationale_{{moqType.SelectedMOQType}}" placeholder="{{MoqTypesPlaceholder('Skill Mix Rationale', moqType.SelectedMOQType)}}" data-ng-model="moqType.SkillMixRationale"></textarea>
                <span data-ng-if="ActualReadOnly()" data-ng-bind-html="moqType.SkillMixRationale"></span>
            </div>
        </div>
        <hr data-ng-show="!moqType.collapsed" />
    </div>
    <div data-ng-if="model.UsingTemplateBOE" class="form-row moqContainerClass">
        <div class="form-label">
            <span>Add New MOQ Type</span>
        </div>
        <div class="form-element">
            <select data-ng-model="model.selectedMOQType" data-ng-options="moqType.SelectedMOQTypeText for moqType in model.MOQTypes | moqTypesFilter:model.SelectedMoqTypes" class="moqTypes"></select>
            <button data-ng-if="!ActualReadOnly()" data-ng-click="AddMoqType()" data-ng-disabled="!model.selectedMOQType" class="moqTypesButton ies-action" type="button">Add MOQ Type</button>
            <button data-ng-if="!ActualReadOnly()" data-ng-disabled="model.SelectedMoqTypes.length <= 1" data-ng-click="displayReOrderMoqTypesDialog()" class="moqTypesButton ies-blue" type="button">Sort MOQ Types</button>
			<button data-ng-if="!ActualReadOnly()" data-ng-disabled="model.SelectedMoqTypes.length <= 1" data-ng-click="calculateAllMoqActuals()" class="moqTypesButton ies-action" type="button">Calculate All Actuals</button>
        </div>
    </div>

    <div data-ng-if="!model.UsingTemplateBOE" class="form-row">
        <div class="form-label">
            <span>MOQ Type **</span>
            <div class="help-icon" onclick="MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text"><%: Html.Raw(Model.HelpText) %></div>
            </div>
        </div>
        <div class="form-element" id="MoqType">
            <select id="MOQType" name="MOQType" class="moqTypes">
                <%=ViewData["MOQTypes"]%>
            </select>
        </div>
    </div>

    <hr data-ng-show="model.UsingTemplateBOE && '<%:Model.MoqTemplateAnswers.Any()%>'.isTrue()" />

    <div class="form-row">
        <div data-ng-if="!model.UsingTemplateBOE" class="form-label"><%: Model.MOQTextLabel %> **</div>
        <div data-ng-if="model.UsingTemplateBOE && '<%:Model.MoqTemplateAnswers.Any()%>'.isTrue()" class="form-label">Additional MOQ Rationale</div>

        <div data-ng-if="!model.UsingTemplateBOE || '<%:Model.MoqTemplateAnswers.Any()%>'.isTrue()" class="form-element moq-text-area"><% Html.RenderPartial(WebConstants.VIEW_RTE_TEMPLATE, new GenBOE.Web.ModelView.RteTemplateModelView(Model.MoqTemplateAnswers, "MOQText", Model.MOQText));  %></div>
    </div>

    <div id="UsedHistoricalMetrics" class="form-row display-none">
        <div class="form-label">
            <%if (Model.Company == CompanyConfiguration.MST) 
              {%>
            Historical Measures<br />Used
            <div class="help-icon" style="margin-top:1px;" onclick="MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text">The historical measures used to estimate the labor for this task.  If the historical measure is no longer used, it should be deleted.</div>
            </div>
            <%}
              else
              { %>
            <span>Historical Metrics
                <br />
                Used</span>
            <div class="help-icon" style="margin-top:1px;" onclick="MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text">The historical metrics used to estimate the labor for this task.  If the historical metric is no longer used, it should be deleted.</div>
            </div>
            <%} %>
        </div>
        <div id="addMetricToList" class="form-element">
            <button id="HMUDeleteButton" class="ies" name="delete-button" type="button" onclick="TaskElementDetailsWidget.deleteAllSelected()">Delete</button>
            <table id="HistoricalMetricsUsedGrid" class="manage-historical-metrics-used-grid grid readonly">
                <thead>
                    <% if (Model.Company == CompanyConfiguration.MST)
                        { %>
                    <tr>
                        <th class="delete-checkbox"><input type="checkbox" id="Checkbox1" onclick="TaskElementDetailsWidget.deleteAllToggle(this)" /></th>    
                        <th class="metric-title"><b>Measure Name</b></th>
                        <th class="status"><b>Program Name</b></th>
                        <th class="moq"><b>Date Applied to BOE</b></th>
                    </tr>
                    <% } %>
                    <% else
                        { %>
                    <tr>
                        <th class="delete-checkbox"><input type="checkbox" id="Checkbox1" onclick="TaskElementDetailsWidget.deleteAllToggle(this)" /></th>    
                        <th class="metric-title"><b>Metric Title</b></th>
                        <th class="status"><b>Status</b></th>
                        <th class="moq"><b>MOQ Equation</b></th>
                        <th class="moq-type"><b>MOQ Type</b></th>
                    </tr>
                    <% } %>
                </thead>
                <tbody id="HistoricalMetricsBody">
                    <%if (Model.Company == CompanyConfiguration.MST)
                        { %>
                        <% foreach (MSTMetricDetailsDTO historicalMetric in Model.PMMetricsUsed)
                           { %>
                            <tr name="historicalMetric" id="HMURow" historicalMetricID="<%:historicalMetric.Id%>"><td class="delete-checkbox" id="DeleteCheckboxId"><input type="checkbox" name="DeleteResource" id="DeleteThisResource" onclick="TaskElementDetailsWidget.deleteToggled(this)"/>
                                <td class="metric-id display-none"> <a name="MetricID" class="edit-resource-link"></a> </td>               
                                <td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(null, {metricId :<%:historicalMetric.Id%>, getFromSource : false}, true)" style="white-space:normal; width:100px;" title="<%:historicalMetric.MeasureName%>"><%:historicalMetric.MeasureName%></a> </td>
                                <td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(null, {metricId :<%:historicalMetric.Id%>, getFromSource : false}, true)" style="white-space:normal; width:100px;" title="<%:historicalMetric.ProgramName %>"><%:historicalMetric.ProgramName %></a> </td>
                                <td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(null, {metricId :<%:historicalMetric.Id%>, getFromSource : false}, true)" style="white-space:normal; width:100px;" title="<%:historicalMetric.DateAddedToTaskElement != null ? historicalMetric.DateAddedToTaskElement.Value.ToString("MM/dd/yyyy") : "N/A"%>"><%:historicalMetric.DateAddedToTaskElement != null ? historicalMetric.DateAddedToTaskElement.Value.ToString("MM/dd/yyyy") : "N/A"%></a> </td>
                                <br id="HistoricalEndRow"/>
                            </tr>
                        <%}%>
                    <% }%>
                </tbody>
            </table>
        </div>
    </div>
	<div id="ReOrderMoqTypesDialog" class="reorder-moq-types-dialog" style="display: none;">
        <div class="container">
            <div class="form-row">
                <div class="form-element">Sort MOQ Types using the move buttons.  Close when finished.  The defined order will be maintained when exporting data to MS Word.</div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    <select size="7" data-ng-model="model.SortingMoqType" data-ng-options="moqType as moqType.SelectedMOQTypeText for moqType in model.SelectedMoqTypes | orderBy: 'Order'" class="moqSortingList"></select>
                    <div class="buttons inline-block centered">
                        <button data-ng-disabled="!model.SortingMoqType" data-ng-hide="sortingInProgress" id="MoqTypesMoveItemsUp" class="ies move-button moqTypes" data-ng-click="MoveUpMoqType()" type="button">Move Up</button>
                        <button data-ng-disabled="!model.SortingMoqType" data-ng-hide="sortingInProgress" id="MoqTypesMoveItemsDown" class="ies move-button moqTypes" data-ng-click="MoveDownMoqType()"type="button">Move Down</button>

                        <div class="move-button moqTypes loader" data-ng-show="sortingInProgress"></div>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-element">Note: Must not contain any OCI, classified, export controlled or third party proprietary information.</div>
            </div>
            <div class="buttons">
                <button id="ReOrderMoqTypesDialog-Close" class="ies" data-ng-click="closeReOrderMoqTypes()" name="cancel-button" type="button">Close</button>
            </div>
        </div>
    </div>
    <div id="ReOrderMoqTablesDialog" class="reorder-moq-tables-dialog" style="display: none;">
        <div class="container">
            <div class="form-row">
                <div class="form-element">Sort MOQ Tables using the move buttons.  Close when finished.  The defined order will be maintained when exporting data to MS Word.</div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    <select size="5" data-ng-model="model.SortingMoqTable" data-ng-options="moqTable as moqTable.TableName for moqTable in model.SortingMoqTablesMoqTypeParent.TableData | orderBy: 'Order'" class="moqSortingList"></select>
                    <div class="buttons inline-block centered">
                        <button data-ng-disabled="!model.SortingMoqTable" id="MoqTablesMoveItemsUp" style="margin-left: 7px;" class="ies move-button moqTypes" data-ng-click="MoveUpMoqTable()" type="button">Move Up</button>
                        <button data-ng-disabled="!model.SortingMoqTable" id="MoqTablesMoveItemsDown" style="margin-left: 7px;" class="ies move-button moqTypes" data-ng-click="MoveDownMoqTable()"type="button">Move Down</button>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-element">Note: Must not contain any OCI, classified, export controlled or third party proprietary information.</div>
            </div>
            <div class="buttons">
                <button id="ReOrderMoqTablesDialog-Close" class="ies" data-ng-click="closeReOrderMoqTables()" name="cancel-button" type="button">Close</button>
            </div>
        </div>
    </div>
    <div gen-dialog id="ImportMoqTableDialog" class="import-moq-table-dialog form dialog" data-width="650" data-height="430" data-title="Import MOQ Tables" data-open="dialog.open">
        <div class="container" data-ng-hide="dialog.showImportResults">
            <div class="form-row">
                Import MOQ Tables using an Excel file.
                <% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new { 
                                                controller = WebConstants.CONTROLLER_BOE_LABOR, 
                                                action = WebConstants.ACTION_IMPORT_MOQ_TABLES, 
                                                workspace = SiteMasterUtilities.GetCurrentWorkspace()}, 
                                                FormMethod.Post, 
                                                new { enctype = "multipart/form-data", id = "ImportMoqTableDialog-Form", target = "ImportMoqTableDialog-UploadTarget" }); %>
                <div class="step" id="ImportMoqStepOne">
                    <div class="title">Step 1: Export the existing MOQ Tables file</div>
                    <div>Start by exporting the existing MOQ Tables.</div>
                    <div>
                        <a data-ng-hide="isExporting" data-ng-click="exportMoqTablesFromImport()">Export existing MOQ Tables</a>
                        <div class="loader" data-ng-show="isExporting"></div>
                    </div>
                </div>
                <div class="step" id="ImportMoqStepTwo">
                    <div class="title">Step 2: Enter/Update MOQ Tables in the file</div>
                    <div>Enter new MOQ Tables into the file or update existing MOQ Tables. </div>
                    <div>All columns besides custom fields are required. Custom field columns may be required or optional. </div>
                    <div data-ng-if="model.IsRMS">Date of Report, PoP Start Date, and PoP End date must be in the format m/yyyy or mm/yyyy</div>
                    <div data-ng-if="!model.IsRMS">
                        <div>Date of Report must be in the format m/yyyy or mm/yyyy</div>
                        <div>For Weekly Query Type, PoP Start Date and PoP End date must be in the format fw/yyyy</div>
                        <div>For Monthly Query Type, PoP Start Date and PoP End date must be in the format m/yyyy or mm/yyyy</div>
                    </div>
                    <br />                    
                    <div class="important">IMPORTANT: This will replace all existing tables. If a table is removed or excluded from the Excel file, the table will be deleted.</div>
                </div>
                <div class="step" id="ImportMoqStepThree">
                    <div class="title">Step 3: Import the updated MOQ Table template file</div>
                    <div>Choose a file to import. The file you import ust be an Excel file that ends in .xlsx or .xlsm. </div>
                    <div>
                        <input type="hidden" id="ImportMoqTableDialog-DocumentDomain" name="documentDomain" />
                        <input type="file" size="60" id="ImportMoqTableDialog-File" name="file" onchange="angular.element('[data-ng-controller=MoqEquationController]').scope().fileUploadChange(this)" />
                    </div>
                </div>
                <% Html.EndForm(); %>
                <div class="buttons">
                    <button id="ImportMoqTableDialog-ImportButton" class="ies-action" name="import-button" data-ng-click="importMoqTables()" data-ng-hide="dialog.importWorking" data-ng-disabled="dialog.disableImport">Import</button>
                    <div id="ImportMoqTableDialog-ImportLoader" class="loader" data-ng-show="dialog.importWorking"></div>
                    <button id="ImportMoqTableDialog.CancelButton" class="ies" name="cancel-button" type="button" data-ng-click="closeImportMoqTables()">Cancel</button>
                </div>
            </div>
        </div>
        <div id="ImportResults" data-ng-show="dialog.showImportResults" class="import-verification">
            <div class="content import-verification"></div>
            <div class="important">IMPORTANT: This will replace all existing tables. If a table is removed or excluded from the Excel file, the table will be deleted.</div>
            <br />
            <div class="buttons">
                <button id="Back-ImportMoqTableVerification" type="button" class="ies" data-ng-click="backFromImport()" name="back-button">Back</button>
                <button id="CompleteImportButton-ImportMoqTableVerification" class="ies-action" data-ng-if="!dialog.invalidData" data-ng-hide="dialog.completeImportWorking" data-ng-click="completeImportMoqTables()">Complete Import</button>
                <div id="CompleteImportLoader-ImportMoqTableVerification" class="loader" data-ng-show="dialog.completeImportWorking" style="width: 129px"></div>
            </div>
        </div>
    </div>
	<div gen-dialog id="UpdateFiltersMoqDialog" class="update-filters-moq-dialog form dialog" data-width="875" data-height="430" data-title="Update Filters" data-open="filterDialog.open">
        <div class="container">
			<div class="form-element">
            <div data-ng-if="filterDialog.showError" class="warning-box" style="display: block">
                <div data-ng-if="filterDialog.error && filterDialog.error.length > 0" class="warning-message">
					<ng-container data-ng-repeat="err in filterDialog.error">
					<b>{{err}}</b><br />
					</ng-container>
                </div>
				<div class="small-close-button" data-ng-click="HideFilterError()"></div>
            </div>
        </div>
            <div class="form-row">
				<div>
					<button data-ng-disabled="filterDialog.isLoading" id="UpdateFiltersMoqDialog-AddButton" class="ies-action" name="filter-moq-add-button" data-ng-click="AddFilterRow()">+ Add</button>
					<button data-ng-disabled="filterDialog.isLoading || filterDialog.data === undefined || filterDialog.data.length < 2" id="UpdateFiltersMoqDialog-ParensButton" class="ies-action" name="filter-moq-parens-button" data-ng-click="UpdateParens()">Parens</button>
				</div>
			</div>
			<div class="form-row">
                <div class="update-filter-grid">
					<table id="UpdateFilterMoqGrid" class="grid">
						<thead>
							<tr>
								<th class="parens">(</th>
								<th class="field">Field</th>
								<th class="operator">Operator</th>
								<th class="value">Value</th>
								<th class="parens">)</th>
								<th class="join">Join</th>
								<th class="up-down"></th>
								<th class="deleteColumn">
                                   <div data-ng-click="DeleteAllFilters()" class="delete DeleteButton" />
                                </th>
							</tr>
						</thead>
						<tbody>
							<tr data-ng-show="filterDialog.isLoading"><td colspan="9"><div class="loader"></div></td></tr>
							<tr data-ng-show="!filterDialog.isLoading && (filterDialog.data === undefined || filterDialog.data.length === 0)"><td colspan="9"><div class="empty-grid-text">There are no Filters, please Add a new row.</div></td></tr>
                            <tr data-ng-repeat="queryFilter in filterDialog.data">
								<td>
									<span data-ng-if="queryFilter.StartParens">(</span>
									<div data-ng-if="!queryFilter.StartParens && !queryFilter.EndParens">
                                        <input class="parens-chck" type="checkbox" data-ng-model="queryFilter.ParensChecked" data-ng-click="$event.stopPropagation()" />
                                    </div>
								</td>
								<td>
									<select data-ng-model="queryFilter.Field" data-ng-change="ResetOperators(queryFilter)" data-ng-options="item.Value as item.Value for item in filterDialog.fieldsArr">
										<option value=""></option>
									</select>
								</td>
								<td>
									<select data-ng-if="queryFilter.Type" data-ng-model="queryFilter.Operator" data-ng-options="item.Value as item.DropdownText for item in filterDialog.operators[queryFilter.Type]">
										<option value=""></option>
									</select>
								</td>
								<td>
									<div data-ng-if="queryFilter.Type" >
										<div data-ng-repeat="val in queryFilter.Value track by $index">
											<input data-ng-if="queryFilter.Type !== 'System.DateTime'" type="text" data-ng-model="queryFilter.Value[$index]" />
											<input data-ng-if="queryFilter.Type === 'System.DateTime'" jqdatepicker type="text" data-ng-model="queryFilter.Value[$index]" style="width:75px;" />
											<button data-ng-if="$first" class="ies-action add-filter-button" data-ng-click="AddFilterValue(queryFilter.Value)"><span>ADD</span></button> 
										</div>
									</div>
								</td>
								<td><span data-ng-if="queryFilter.EndParens">)</span></td>
								<td>
									<select data-ng-if="!$last" data-ng-model="queryFilter.Join">
										<option></option>
										<option value="AND">AND</option>
										<option value="OR">OR</option>
									</select>
								</td>
								<td class="up-down">
									<button data-ng-disabled="$first" data-ng-click="MoveFilterUp($index)"><i class="fa fa-arrow-up"></i></button>
									<button data-ng-disabled="$last" data-ng-click="MoveFilterDown($index)"><i class="fa fa-arrow-down"></i></button>
								</td>
								<td class="deleteColumn">
                                   <div data-ng-click="DeleteFilter($index)" class="delete DeleteButton" />
                                </td>
							</tr>
						</tbody>
					</table>
				</div>
            </div>
			<div class="form-row">
				<div class="buttons">
					<button id="UpdateFiltersMoqDialog-Save" class="ies" data-ng-click="SaveMoqFilters()" name="action-button" type="button">Save</button>
					<button id="UpdateFiltersMoqDialog-Close" class="ies" data-ng-click="CloseMoqFilters()" name="cancel-button" type="button">Close</button>
				</div>
			</div>
        </div>
    </div>
</div>
<div id="VariableSumOfBOEsByWBSDialogContainer"></div>
<div id="VariableSumOfBOEsByCLINDialogContainer"></div>