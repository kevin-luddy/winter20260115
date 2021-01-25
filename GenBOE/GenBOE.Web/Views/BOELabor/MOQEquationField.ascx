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
        IsRMS:'<%:Model.Company == CompanyConfiguration.MST%>'.isTrue()
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
    <div data-ng-if="model.UsingTemplateBOE" class="form-element moqRteFieldContainer" data-ng-repeat="moqType in model.SelectedMoqTypes | orderBy: 'Order'">
        <div class="form-row" data-ng-class="{'collapsedBorder': moqType.collapsed}">
            <div class="form-label">
                <a data-nodrag="" data-ng-click="toggle(moqType)">
                    <div class="moqTypeHeader" data-ng-class="{'collapsed': moqType.collapsed, 'expanded': !moqType.collapsed}"></div>
                </a>
                {{moqType.SelectedMOQTypeText}}
            </div>
            <button data-ng-if="!model.IsReadOnly" data-ng-click="RemoveMoqType(moqType)" class="ies-danger moqTypesButton" type="button">Delete MOQ Type</button>
        </div>
        <div class="moqTaskBasedOn" data-ng-show="!moqType.collapsed">
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>">{{PortionOfTask()}} historical data:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>">{{PortionOfTask()}} similar historical data:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%>">{{PortionOfTask()}} a Cost Estimating Relationship (CER). Please provide:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%>">{{PortionOfTask()}} Parametric Model/tool. Please provide:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>">{{PortionOfTask()}} Analogous Relationship. Please provide:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SOW%>">{{PortionOfTask()}} a Statement of Work (SOW) directive:</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%>">{{PortionOfTask()}} a Level of Effort (LOE):</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">{{PortionOfTask()}} Subject Matter Expert (SME) Judgement.</span>
            <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.NonLabor%>">This task is Non-Labor:</span>
        </div>
        <div data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%> || moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>">
            <div class="tableDataParent" data-ng-repeat="tableData in moqType.TableData | orderBy: 'Order'">
                <div class="tableData">
                    <table pkid="{{tableData.Id}}">
                        <tr>
                            <td class="form-label">
                                <div class="moqTypeHeader" data-ng-class="{'collapsed': tableData.collapsed, 'expanded': !tableData.collapsed}" data-ng-click="toggle(tableData)"></div>
                                {{model.MoqTypeTableDataLabels.TableName}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TableNameHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TableNameComparativeSuffix);"></div>
                            </td>
                            <td><input type="text" required data-ng-model="tableData.TableName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="!model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.RepositoryName}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.RepositoryNameHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.RepositoryNameComparativeSuffix);"></div>
                            </td>
                            <td><input type="text" required data-ng-model="tableData.RepositoryName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="!model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.QueryType}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.QueryTypeHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.QueryTypeComparativeSuffix);"></div>
                            <td>
                                <select required data-ng-model="tableData.QueryType">
                                    <option value=""></option>
                                    <option value="Monthly">Monthly</option>
                                    <option value="Weekly">Weekly</option>
                                </select>
                            </td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.DateOfReport}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.DateOfReportHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.DateOfReportComparativeSuffix);"></div>
                            </td>
                            <td><input type="date" required data-ng-model="tableData.DateOfReport" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.HistoricalProgramName}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.HistoricalProgramNameHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.HistoricalProgramNameComparativeSuffix);"></div>
                            </td>
                            <td><input type="text" required data-ng-model="tableData.HistoricalProgramName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.ContractNumber}} * <div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.ContractNumberSuffix);"></div></td>
                            <td><input type="text" required data-ng-model="tableData.ContractNumber" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.WbsElement}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.WBSElementHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.WBSElementComparativeSuffix);"></div>
                            </td>
                            <td><textarea cols="20" required data-ng-model="tableData.WbsElement" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.PoPStart}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPStartHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPStartComparativeSuffix);"></div>
                            </td>
                            <td><input type="date" required data-ng-model="tableData.PoPStart" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.PoPEnd}} *                                
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPEndHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.PoPEndComparativeSuffix);"></div>
                            </td>
                            <td><input type="date" required data-ng-model="tableData.PoPEnd" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.TotalWbsHours}} * <div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.TotalWBSHoursSuffix);"></div></td>
                            <td><input type="number" required min="0" data-ng-model="tableData.TotalWbsHours" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.AdditionalQueryFilters}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.AdditionalQueryFiltersHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.AdditionalQueryFiltersComparativeSuffix);"></div>
                            </td>
                            <td><textarea cols="20" required data-ng-model="tableData.AdditionalQueryFilters" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.TotalRelevantHours}} * 
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Historical%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TotalRelevantHoursHistoricalSuffix);"></div>
                                <div class="help-icon" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.Comparative%>" data-ng-click="openHelp(model.MoqTypeHelpUrls.TotalRelevantHoursComparativeSuffix);"></div></td>
                            <td><input type="number" required min="0" data-ng-model="tableData.TotalRelevantHours" onchange="MOQEquationFieldWidget.setDirty()" /></td>
                        </tr>
                        <tr data-ng-repeat="customField in MoqTableCustomFields" data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{customField.CustomFieldMetaData.FieldName}}{{(customField.CustomFieldMetaData.isRequired) ? ' *' : ''}}</td>
                            <td class="custom-field">{{ selectedItem = findMoqTableCustomFieldValue(customField, tableData); "" }}
                                <input type="text" onchange="MOQEquationFieldWidget.setDirty(); validateMoqTableCustomField(this);" data-ng-required="customField.CustomFieldMetaData.isRequired" data-ng-class="{'ng-invalid': customField.CustomFieldMetaData.isRequired && selectedItem.openEndedValue.length==0}" data-ng-value="selectedItem.openEndedValue" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" customfieldvalueid="{{selectedItem.openEndedId}}" selectionid="{{selectedItem.selectedID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="true" />
                            </td>
                        </tr>
                   </table>
                </div>
                <div class="tableDataButtons" data-ng-if="!model.IsReadOnly">
                    <button style="margin-bottom:9px;" data-ng-if="$index == 0" data-ng-click="CreateNewTable(moqType.TableData)" type="button" class="ies-action moqTypesButton">Add Table Data</button>
                    <button data-ng-disabled="moqType.TableData.length <= 1" data-ng-if="$index == 0" data-ng-click="displayReOrderMoqTablesDialog(moqType)" class="moqTypesButton ies-blue" type="button">Sort MOQ Tables</button>
                    <button style="display:block;" data-ng-if="moqType.TableData.length > 1" data-ng-click="RemoveTable(tableData, moqType.TableData)" type="button" class="ies-danger moqTypesButton">Delete Table Data</button>
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
                <input type="text" class="cerPmArTextBox" required data-ng-model="moqType.CerName" />
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
                <textarea data-ng-if="!model.IsReadOnly" cols="20" name="DescriptionHoursRequired_{{moqType.SelectedMOQType}}" data-ng-model="moqType.DescriptionHoursRequired"></textarea>
                <span data-ng-if="model.IsReadOnly" data-ng-bind-html="moqType.DescriptionHoursRequired"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The SME selected Expert judgement for this basis of estimate for the following reasons: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SMEReasonsSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!model.IsReadOnly" cols="20" name="SmeReason_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeReason"></textarea>
                <span data-ng-if="model.IsReadOnly" data-ng-bind-html="moqType.SmeReason"></span>
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
                <textarea data-ng-if="!model.IsReadOnly" cols="20" name="SmeHoursLogic_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeHoursLogic"></textarea>
                <span data-ng-if="model.IsReadOnly" data-ng-bind-html="moqType.SmeHoursLogic"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The logic and assumptions used to estimate duration is: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SMEDurationLogicSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!model.IsReadOnly" cols="20" name="SmeDurationLogic_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeDurationLogic"></textarea>
                <span data-ng-if="model.IsReadOnly" data-ng-bind-html="moqType.SmeDurationLogic"></span>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The following tasks are estimates in this BOE: *</span><div class="help-icon" data-ng-click="openHelp(model.MoqTypeHelpUrls.SMETasksSuffix);"></div>
            </div>
            <div class="form-element">
                <textarea data-ng-if="!model.IsReadOnly" cols="20" name="SmeTaskEstimates_{{moqType.SelectedMOQType}}" placeholder="{{MoqTypesPlaceholder('Rationale', moqType.SelectedMOQType)}}" data-ng-model="moqType.SmeTaskEstimates"></textarea>
                <span data-ng-if="model.IsReadOnly" data-ng-bind-html="moqType.SmeTaskEstimates"></span>
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
                <textarea data-ng-if="!model.IsReadOnly" cols="20" name="Rationale_{{moqType.SelectedMOQType}}" placeholder="{{MoqTypesPlaceholder('Rationale', moqType.SelectedMOQType)}}" data-ng-model="moqType.Rationale"></textarea>
                <span data-ng-if="model.IsReadOnly" data-ng-bind-html="moqType.Rationale"></span>
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
                <textarea data-ng-if="!model.IsReadOnly" cols="20" name="SkillMixRationale_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SkillMixRationale"></textarea>
                <span data-ng-if="model.IsReadOnly" data-ng-bind-html="moqType.SkillMixRationale"></span>
            </div>
        </div>
        <hr data-ng-show="!moqType.collapsed" />
    </div>
    <div data-ng-if="model.UsingTemplateBOE && !model.IsReadOnly" class="form-row">
        <div class="form-label">
            <span>Add New MOQ Type</span>
        </div>
        <div class="form-element">
            <select data-ng-model="model.selectedMOQType" data-ng-options="moqType.SelectedMOQTypeText for moqType in model.MOQTypes | moqTypesFilter:model.SelectedMoqTypes" class="moqTypes"></select>
            <button data-ng-if="!model.IsReadOnly" data-ng-click="AddMoqType()" data-ng-disabled="!model.selectedMOQType" class="moqTypesButton ies-action" type="button">Add MOQ Type</button>
            <button data-ng-if="!model.IsReadOnly" data-ng-disabled="model.SelectedMoqTypes.length <= 1" data-ng-click="displayReOrderMoqTypesDialog()" class="moqTypesButton ies-blue" type="button">Sort MOQ Types</button>
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
                        <button data-ng-disabled="!model.SortingMoqType" id="MoqTypesMoveItemsUp" style="margin-left: 7px;" class="ies move-button moqTypes" data-ng-click="MoveUpMoqType()" type="button">Move Up</button>
                        <button data-ng-disabled="!model.SortingMoqType" id="MoqTypesMoveItemsDown" style="margin-left: 7px;" class="ies move-button moqTypes" data-ng-click="MoveDownMoqType()"type="button">Move Down</button>
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
</div>
<div id="VariableSumOfBOEsByWBSDialogContainer"></div>
<div id="VariableSumOfBOEsByCLINDialogContainer"></div>