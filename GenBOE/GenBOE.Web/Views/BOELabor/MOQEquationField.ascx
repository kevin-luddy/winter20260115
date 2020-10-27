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

        TaskElementDetailsWidget.ChildWidgets.push(MOQEquationFieldWidget);
        TaskElementDetailsWidget.registerForDelegateEvent('click', '.menu-icon', function (event) {
            if ($('#menu-options-box').hasClass("display-none")) {
                $('#menu-options-box').removeClass('display-none');
            }
            else {
                $('#menu-options-box').addClass('display-none');
            }
        });
        TaskElementDetailsWidget.registerForDelegateEvent('click', '#menu-options-box', function (event) {
            $('#menu-options-box').addClass('display-none');
            event.stopPropagation();
        });
        TaskElementDetailsWidget.CheckToShowMetrics();
        TaskElementDetailsWidget.MOQText = CreateRteTemplate('<%:showMoqQuestions%>'.isTrue(), <%: numberMoqQuestions %>);

        if (!MOQEquationFieldWidget.isReadOnly() || shouldMoqReadOnlyBeReversed || !TaskElementDetailsWidget.isReadOnly()) {
            InitializeRteTemplate(TaskElementDetailsWidget.MOQText, 'MOQText', rteFieldSize);
        }
        else {
            HandleRTETemplateDataForReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText');
        }

        $(document).trigger('MOQWidgetLoaded', "MOQEquationField");
        $(document).trigger('WidgetLoaded', "MOQEquationField");
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
                    <li><a id="CopyMoqFromBoeLink" data-ng-click="CopyMoqFromBoeClicked()">Copy MOQ from BOE</a></li>
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
    <div data-ng-if="model.UsingTemplateBOE" class="form-element moqRteFieldContainer" data-ng-repeat="moqType in model.SelectedMoqTypes">
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
            <div class="tableDataParent" data-ng-repeat="tableData in moqType.TableData">
                <div class="tableData">
                    <table>
                        <tr>
                            <td class="form-label" data-ng-click="toggle(tableData)">
                                <div class="moqTypeHeader" data-ng-class="{'collapsed': tableData.collapsed, 'expanded': !tableData.collapsed}"></div>
                                {{model.MoqTypeTableDataLabels.TableName}} *
                            </td>
                            <td><input type="text" required data-ng-model="tableData.TableName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="!model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.RepositoryName}} *</td>
                            <td><input type="text" required data-ng-model="tableData.RepositoryName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="!model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.QueryType}} *</td>
                            <td>
                                <select required data-ng-model="tableData.QueryType">
                                    <option value=""></option>
                                    <option value="Monthly">Monthly</option>
                                    <option value="Weekly">Weekly</option>
                                </select>
                            </td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.DateOfReport}} *</td>
                            <td><input type="date" required data-ng-model="tableData.DateOfReport" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.HistoricalProgramName}} *</td>
                            <td><input type="text" required data-ng-model="tableData.HistoricalProgramName" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed" data-ng-if="model.IsRMS">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.ContractNumber}} *</td>
                            <td><input type="text" required data-ng-model="tableData.ContractNumber" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.WbsElement}} *</td>
                            <td><textarea cols="20" required data-ng-model="tableData.WbsElement" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.PoPStart}} *</td>
                            <td><input type="date" required data-ng-model="tableData.PoPStart" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.PoPEnd}} *</td>
                            <td><input type="date" required data-ng-model="tableData.PoPEnd" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.TotalWbsHours}} *</td>
                            <td><input type="number" required min="0" data-ng-model="tableData.TotalWbsHours" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.AdditionalQueryFilters}} *</td>
                            <td><textarea cols="20" required data-ng-model="tableData.AdditionalQueryFilters" /></td>
                        </tr>
                        <tr data-ng-show="!tableData.collapsed">
                            <td class="form-label">{{model.MoqTypeTableDataLabels.TotalRelevantHours}} *</td>
                            <td><input type="number" required min="0" data-ng-model="tableData.TotalRelevantHours" /></td>
                        </tr>
                   </table>
                </div>
                <div class="tableDataButtons">
                    <button style="margin-bottom:9px;" data-ng-if="!model.IsReadOnly && $index == 0" data-ng-click="CreateNewTable(moqType.TableData)" type="button" class="ies-action moqTypesButton">Add Table Data</button>
                    <button style="display:block;" data-ng-if="!model.IsReadOnly && moqType.TableData.length > 1" data-ng-click="RemoveTable(tableData, moqType.TableData)" type="button" class="ies-danger moqTypesButton">Delete Table Data</button>
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
            </div>
            <div class="form-element">
                <input type="text" class="cerPmArTextBox" required data-ng-model="moqType.CerName" />
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%> || moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%> || moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>">
            <div class="form-label">
                <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.CostEstimatingRelationships%>">CER</span>
                <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.ParametricEstimates%>">Parametric model or tool</span>
                <span data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.AnalogousRelationships%>">Analogous relationship</span>
                 location in the proposal: *
            </div>
            <div class="form-element">
                <input type="text" class="cerPmArTextBox" required placeholder="{{MoqTypesPlaceholder('Location', moqType.SelectedMOQType)}}" data-ng-model="moqType.CerLocation" />
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%> || moqType.SelectedMOQType == <%:(int)MOQType.SOW%>">
            <div class="form-label" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.LOE%>">
                <span>Description of Hours required: *</span>
            </div>
            <div class="form-label" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SOW%>">
                <span>Description of Hours required & location in SOW: *</span>
            </div>
            <div class="form-element">
                <textarea cols="20" name="DescriptionHoursRequired_{{moqType.SelectedMOQType}}" data-ng-model="moqType.DescriptionHoursRequired"></textarea>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The SME selected Expert judgement for this basis of estimate for the following reasons: *</span>
            </div>
            <div class="form-element">
                <textarea cols="20" name="SmeReason_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeReason"></textarea>
            </div>           
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The logic and assumptions used to estimate hours is: *</span>
            </div>
            <div class="form-element">
                <textarea cols="20" name="SmeHoursLogic_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeHoursLogic"></textarea>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The logic and assumptions used to estimate duration is: *</span>
            </div>
            <div class="form-element">
                <textarea cols="20" name="SmeDurationLogic_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SmeDurationLogic"></textarea>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType == <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>The following tasks are estimates in this BOE: *</span>
            </div>
            <div class="form-element">
                <textarea cols="20" name="SmeTaskEstimates_{{moqType.SelectedMOQType}}" placeholder="{{MoqTypesPlaceholder('Rationale', moqType.SelectedMOQType)}}" data-ng-model="moqType.SmeTaskEstimates"></textarea>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType != <%:(int)MOQType.SME%>">
            <div class="form-label">
                <span>Rationale: *</span>
            </div>
            <div class="form-element">
                <textarea cols="20" name="Rationale_{{moqType.SelectedMOQType}}" placeholder="{{MoqTypesPlaceholder('Rationale', moqType.SelectedMOQType)}}" data-ng-model="moqType.Rationale"></textarea>
            </div>
        </div>
        <div class="form-row" data-ng-show="!moqType.collapsed" data-ng-if="moqType.SelectedMOQType != <%:(int)MOQType.SOW%> && moqType.SelectedMOQType != <%:(int)MOQType.NonLabor%>">
            <div class="form-label">
                <span>Skill Mix Rationale: *</span>
            </div>
            <div class="form-element">
                <textarea cols="20" name="SkillMixRationale_{{moqType.SelectedMOQType}}" data-ng-model="moqType.SkillMixRationale"></textarea>
            </div>
        </div>
        <hr data-ng-show="!moqType.collapsed" />
    </div>
    <div data-ng-if="model.UsingTemplateBOE && !model.IsReadOnly" class="form-row">
        <div class="form-label">
            <span>Add New MOQ Type</span>
        </div>
        <div class="form-element">
            <select data-ng-model="model.selectedMOQType" data-ng-options="moqType.SelectedMOQTypeText for moqType in model.MOQTypes | moqTypesFilter:model.SelectedMoqTypes" class="moqTypes">
            </select>
            <button data-ng-click="AddMoqType()" data-ng-disabled="!model.selectedMOQType" class="moqTypesButton ies-action" type="button">Add MOQ Type</button>
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
        <div data-ng-if="model.UsingTemplateBOE && '<%:Model.MoqTemplateAnswers.Any()%>'.isTrue()" class="form-label">Additional MOQ Types <br />Information</div>

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
</div>
<div id="VariableSumOfBOEsByWBSDialogContainer"></div>
<div id="VariableSumOfBOEsByCLINDialogContainer"></div>