<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.LaborTaskModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<% 
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    string asterisk = "*";
    int rteFieldSize = ViewBag.RteFieldSize;
    bool showDescQuestions = Model.DescriptionTemplateAnswers.Any();
    int numberDescQuestions = showDescQuestions ? Model.DescriptionTemplateAnswers.Count : 1;
%>
<script type="text/javascript">
    var debugOutputEnabled = false;
    var isSaveButtonHidden = '<%: ViewData["READONLY"] %>'.isTrue();

    // Get the read-only attribute passed in from the controller
    var TaskElementsComposite_ReadOnly = <%= ViewData["READONLY"] %>;
    var ManageWBS_ContainsOCI = <%= Model.ContainsOci.ToString().ToLower() %>;
    var currentWorkspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var boeLaborController = '<%: WebConstants.CONTROLLER_BOE_LABOR %>';
    var boeId = '<%: ViewData["BOEID"] %>';
    var taskId = '<%: ViewData["TASKID"] %>';
    var taskElementId = '-1';
    if (taskId != '') {
        taskElementId = taskId;
    }

    var TaskElementDetailsWidget;
    var TableData = '<%: ViewData["TableData"] %>';
    var SelectedLaborTypes = '<%: ViewData["SelectedLaborTypes"] %>';

    var TaskLaborTypesWidget;
    var TaskLaborSpreadsWidget;
    var TaskSkillMixWidget;

    validateCustomFields = function () {
        // Does nothing, but called from manageTaskController() even when using static page
        // so blank method is here to prevent console error for an undefined method
    }

    $(function () {
        $(document).trigger("SHOW_LOADING_BOX");

        // Labor Types

        var widgetConfig = {};
        widgetConfig.ContextID = "LaborTypesWidgetContainer";
        widgetConfig.isReadOnly = true;
        widgetConfig.IsModule = true;
        TaskLaborTypesWidget = new GenWidget(widgetConfig);

        // Labor Spreads

        var spreadWidgetConfig = {};
        spreadWidgetConfig.ContextID = "ManageLaborSpread";
        spreadWidgetConfig.isReadOnly = true;
        spreadWidgetConfig.IsModule = true;
        TaskLaborSpreadsWidget = new GenWidget(spreadWidgetConfig);

        // Skill Mix Rationale
        var skillMixConfig = {};
        skillMixConfig.ContextID = "SkillMixRationaleContainer";
        skillMixConfig.isReadOnly = true;
        skillMixConfig.IsModule = true;
        TaskSkillMixWidget = new GenWidget(skillMixConfig);


        // Task Element details      

        var currentWorkspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
        var boeLaborController = '<%: WebConstants.CONTROLLER_BOE_LABOR %>';
        var taskElementId = '<%: Model.TaskElementId.HasValue ? Model.TaskElementId.Value : 0 %>';
        var boeId = '<%: Model.BoeId %>';
        var loadMOQEquationUrl = GenSession.CreateUrl({
            workspace: currentWorkspace,
            controller: '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
            action: '<%:WebConstants.ACTION_DISPLAY_MOQ_HOURS_EQUATION_FIELD %>',
            boe: boeId,
            taskelement: ''
        });
        var confirmWarningUrl = GenSession.CreateUrl({
            workspace: currentWorkspace,
            controller: '<%: WebConstants.CONTROLLER_BOE_LABOR%>',
            action: '<%: WebConstants.ACTION_MARK_WARNING_AS_CONFIRMED%>',
            boe: '<%: Model.BoeId %>'
        });
        var searchHistoricalMetricsMSTUrl = GenSession.CreateUrl({
            workspace: currentWorkspace,
            controller: boeLaborController,
            action: '<%: WebConstants.ACTION_SEARCH_HISTORICAL_METRICS_IN_MST %>'
        });
        var historicalMetricsDetailsUrl = GenSession.CreateUrl({
            workspace: currentWorkspace,
            controller: boeLaborController,
            action: '<%: WebConstants.ACTION_GET_HISTORICAL_METRIC_DETAILS %>'
        });
        var pagingMetricsUrl = GenSession.CreateUrl({
            workspace: currentWorkspace,
            controller: boeLaborController,
            action: '<%: Model.MetricsPagingActionName %>'
        });
        var searchTypeAheadUrl = CreatePostURL(currentWorkspace, boeLaborController, '<%: WebConstants.ACTION_GET_SEARCH_TYPE_AHEAD %>', '');
        var recalculateAndRefreshPageUrl = CreatePostURL(currentWorkspace,
            boeLaborController,
            'DoFullRecalculationWithPageRefresh',
            'boe/' + boeId + '/taskelement/' + taskElementId);

        TaskElementDetailsWidget = InitializeTaskElementDetailsWidget("<%:Model.MetricsSearchDialogParameters.DialogTitle%>",
            '<%:Model.MetricsSearchDialogParameters.SearchMetricsDialogIdSuffix%>',
            <%: ViewData["READONLY"] %>,
            '<%: ViewData["WorkspaceState"] %>',
            boeId,
            taskElementId,
            <%: Model.LaborTypeWarning.ToString().ToLower() %>,
            loadMOQEquationUrl,
            confirmWarningUrl,
            searchHistoricalMetricsMSTUrl,
            historicalMetricsDetailsUrl,
            pagingMetricsUrl,
            '<%:Model.BOEState%>' != '<%:(int)BOEState.Draft%>',
            '<%:Model.BOEState%>' == '<%:(int)BOEState.Draft%>' || '<%:Model.BOEState%>' == '<%:(int)BOEState.DraftLocked%>',
            '<%:Model.MetricsSearchDialogParameters.MetricStoreConnected%>'.toLowerCase(),
            searchTypeAheadUrl,
            '<%:Model.AllowDateShift%>'.toLowerCase(),
            recalculateAndRefreshPageUrl,
            '#', // dateshift url not needed for static page
            '#', // saveReorderLaborTypes url not needed for static page
            '<%: showDescQuestions %>'.isTrue(),
            <%: numberDescQuestions %>,
            <%: rteFieldSize %>,
			'<%= SiteMasterUtilities.IsClassEnvironment %>'
        );

        // End Task Element details       

        $("#Cancel-BOEUpdates").click(TaskElementDetailsWidget.CancelToMainGrid);

        //Change the OCI note based off the workspace
        if (ManageWBS_ContainsOCI == true) {
            $('#Task-OCINote').html('Must not contain any classified, export controlled or third party proprietary information.');
        }
        else {
            $('#Task-OCINote').html('Must not contain any OCI, classified, export controlled or third party proprietary information.');
        }

        $(document).one("MOQWidgetLoaded", function () {
            $(document).trigger("AllWidgetLoaded");
            $(document).trigger("ValidateMOQEquation", { initialLoad: true });
        });

        <% if (Model.ContainsDiscrete)
    { %>
        // Change the Labor Spread Section to be expanded if collapsed
        $('#ManageLaborSpread.collapsed .collapsible-header').click();
        <% } %>

        if (<%: ViewData["READONLY"] %>) {
            $('#Save-BOEUpdatesAndClose').addClass('display-none');
        }

        // Validates individual custom field
        validateCustomField = function (item) {
            if ($(item).attr('isRequired') === 'true' && $(item).attr('isNew') !== 'true') {
                if (item.value.length === 0) {
                    $(item).parent('td').addClass('inputError');
                } else {
                    $(item).parent('td').removeClass('inputError');
                }
            }
        }

        refreshModule($('.boe-details .module'));
    });
</script>

<div id="TaskElementsComposite" class="task-elements-composite composite" data-ng-app="genboe" data-ng-controller="ManageTaskController" data-ng-init="init('<%= ViewData["TASKID"] != null ? ViewData["TASKID"] : "-1" %>')" data-ng-cloak="">
    <%
        int? taskElementID = null;

        if (ViewData["TASKID"] != null)
        {
            taskElementID = (int)ViewData["TASKID"];
        }

    %>

    <div id="TaskDetailsPlaceholder">
        <div class="task-element-details module expanded" id="TaskElementDetailsModule">
            <div class="module-header-data">
                <div class="float-left">
                    Task Element Details
                </div>
                <div class="float-right right-header">
                    <div id="TaskStartDateLabel">
                        Task Start Date:
                        <div id="TaskStartDate">{{model.TaskElementData.StartDate}}</div>
                    </div>
                    |
                    <div id="TaskEndDateLabel">
                        Task End Date:
                        <div id="TaskEndDate">{{model.TaskElementData.EndDate}}</div>
                    </div>
                </div>
            </div>
            <div class="module-content-data expanded-content">

                <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "TaskElementDetailsForm" }))
                    { %>

                <gen-validation data-errors="errors"></gen-validation>
                <div style="margin-bottom: 10px; display: none;" id="recalculateTaskElementButton">
                    <button type="button" onclick="TaskElementDetailsWidget.RecalculateAndRefreshPage()" class="ies-danger">Recalculate Task Element</button>
                </div>
                <div class="form-row">
                    <div class="form-label">
                        Task ID 
                    </div>
                    <div class="form-element">
                        <div class="replacedWidgetText">{{model.TaskElementData.TaskID}}</div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-label">
                        Task Title *
                    </div>
                    <div class="form-element">
                        <div class="replacedWidgetText">{{model.TaskElementData.Title}}</div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-label">
                        Task Description *
                    </div>
                    <div class="form-element">
                        <% Html.RenderPartial(WebConstants.VIEW_RTE_TEMPLATE, new GenBOE.Web.ModelView.RteTemplateModelView(Model.DescriptionTemplateAnswers, "TaskDescription", Model.TaskDescription));  %>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-label task-start-date-label">
                        Task Start Date *
                    </div>
                    <div class="form-element task-start-date">
                        <div class="replacedWidgetText">{{model.TaskElementData.StartDate}}</div>
                    </div>
                    <div class="form-label task-end-date-label">
                        Task End Date *
                    </div>
                    <div class="form-element task-end-date">
                        <div class="replacedWidgetText">{{model.TaskElementData.EndDate}}</div>
                    </div>
                </div>
                <div id="MOQEquationFieldContent">
                    <div class="form-row">
                        <div class="form-label">
                            <%:Model.MOQEquationLabel %>  **
                        </div>
                        <div class="form-element">
                            <div class="loader"></div>
                        </div>
                        <div id="MOQEquationNote" class="moq-equation-note display-none">
                            <b>Note:</b> The value of Variables that are the sum of select BOEs will change if the Totals of the select BOEs change.<br />
                        </div>
                    </div>
                </div>
                <br />
                <br />
                <br />
                <div class="form-row custom-field-editable" data-ng-repeat="customField in TaskCustomFields" data-ng-if="IsDraftOrDraftLocked">
                    <div class="form-label">
                        {{ selectedItem = findTaskCustomField(customField); ""}} 
                        {{customField.CustomFieldMetaData.FieldName}} {{customField.CustomFieldMetaData.isRequired ? '*' : ''}}
                    </div>
                    <div class="form-element" id="TaskElementCustomFieldID">
                        <input data-ng-if="customField.CustomFieldMetaData.isOpenEnded" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="TaskElement-CF{{customField.CustomFieldMetaData.CustomFieldID}}" customfieldvalueid="{{selectedItem.openEndedId}}" selectionid="{{selectedItem.selectedID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="true" class="customField TaskElementCustomField skip-read-only" maxlength="250" onchange="TaskElementDetailsWidget.setDirty()" data-ng-value="selectedItem.openEndedValue" />
                        <select data-ng-if="!customField.CustomFieldMetaData.isOpenEnded" onchange="TaskElementDetailsWidget.setDirty()" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="TaskElement-CF{{customField.CustomFieldMetaData.CustomFieldID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="false"
                            class="customField TaskElementCustomField skip-read-only" selectionid="{{selectedItem.selectedID}}">
                            <option value="-1"></option>
                            <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                            <option data-ng-repeat="option in customField.CustomFieldOptions" data-ng-value="option.CustomFieldOptionID" data-ng-selected="selectedItem.selectedOptionID == option.CustomFieldOptionID">{{option.ID}}-{{option.Description}}</option>
                        </select>
                    </div>
                </div>
                <div class="form-row custom-field-locked" data-ng-repeat="customField in TaskCustomFields" data-ng-init="selectedItem = findTaskCustomField(customField)" data-ng-if="!IsDraftOrDraftLocked">
                    <div class="form-label">
                        {{customField.CustomFieldMetaData.FieldName}} {{customField.CustomFieldMetaData.isRequired ? '*' : ''}}
                    </div>
                    <div class="form-element" id="TaskElementCustomFieldID">
                        <span class="replacedWidgetText">{{selectedItem.selectedOptionText}}</span>
                    </div>
                </div>
                <%  }//end form %>
            </div>
        </div>
    </div>
    <div id="LaborTypesPlaceholder">
        <div class="labor-types module expanded" id="LaborTypesWidgetContainer">
            <div class="module-header-data">
                <div class="float-left">Resource Types</div>
                <div class="float-right right-header">
                    <div style="float: left; margin-top: -8px;" id="LaborTypeDeltaLabel" class="labortype-label">Delta <%: Model.HoursLabel%></div>
                    <div style="float: left; margin-top: -8px;" id="LaborTypeDelta" class="labortype">{{deltaHours}}</div>
                    <div style="float: left; width: 1px; height: 32px; margin-top: 2px; background-color: #999A95;">&nbsp;</div>
                    <div class="float-right labor-calc-container">
                        <div class="module-header-data-first-row" style="line-height: 20px;">
                            <div id="LaborTypeTotalLabel" class="labortype-label">Total <%: Model.HoursLabel%> </div>
                            <div id="LaborTypeTotal" class="labortype float-right">{{totalHours}}</div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="module-content-data">
                <div id="LaborTypeWarningFlag" class="warning-box">
                    <div class="small-close-button" onclick="TaskElementDetailsWidget.messageConfirmed()" style="float: right"></div>
                    <div class="warning-message">
                        The Total <%: Model.HoursLabel %> computed by the MOQ Equation has changed.
                        <ul>
                            <li>For each Resource using a Spread Curve, if the % Spread is locked, it was maintained and the <%: Model.HoursLabel%> Spread adjusted accordingly.  If the <%: Model.HoursLabel%> Spread is locked, it was maintained and the % Spread adjusted accordingly.</li>
                            <li>No changes were made to the Discrete resource spreads.</li>
                        </ul>
                        The Resource Spreads should be verified before submitting for approval. 
                    </div>
                </div>
            </div>
            <div class="module-content-data expanded-content">
                <gen-validation data-errors="laborTypeErrors"></gen-validation>
                <div id="LTWidget" class="addedableLaborTypes">

                    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "LaborTypesForm" }))
                        { %>
                    <ul class="validation-box"></ul>
                    <div class="import-buttons">
                        <button data-ng-disabled="isLoading || isExporting" data-ng-click="tryExport()" id="LTExportButton" class="ies-action" type="button">Export</button>
                    </div>
                    <form id="LaborTypesForm">
                        <div id="LaborTypesGridBlock">
                            <div id="LaborFixedDiv">
                                <table class="labor-types-grid addable grid editable" id="LaborTypesFixed" name="laborTypesFixed">
                                    <thead>
                                        <tr>
                                            <th class="element-of-cost">Element of Cost</th>
                                            <%-- This code: &#10013; is to display the Cross on the page --%>
                                            <th class="resource">Resource (<a href="#" onclick="TaskElementDetailsWidget.openWindow(currentWorkspace, boeLaborController,'<%:WebConstants.ACTION_DISPLAY_LABOR_RESOURCES%>'); return false;">View</a>){{IsBRCEnabled ? '&#10013;' : '*'}}</th>
                                            <th data-ng-show="IsBRCEnabled" class="resource">Business Resource Code (<a href="#" onclick="TaskElementDetailsWidget.openWindow(currentWorkspace, boeLaborController, '<%:WebConstants.ACTION_DISPLAY_BUSINESS_RESOURCE_CODES%>'); return false;">View</a>)&#10013;</th>
                                            <th class="performing-org">Performing Org (<a href="#" onclick="TaskElementDetailsWidget.openWindow(currentWorkspace, boeLaborController,'<%:WebConstants.ACTION_DISPLAY_LABOR_PERF_ORGS%>'); return false;">View</a>)*</th>
                                            <% if (Model.BOEIsMulti)
                                                {%>
                                            <th class="resource-wbs">WBS</th>
                                            <th class="resource-clin">CLIN</th>
                                            <% } %>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr data-ng-repeat="item in tableData | filter: { Deleted: false }" data-ng-hide="item.Deleted">
                                            <td>{{ getElementOfCostText(item.ElementOfCost)}}
                                            </td>
                                            <td>
                                                <div title="{{item.ResourceDescription}}">{{item.ResourceDescription}}</div>
                                            </td>
                                            <td data-ng-show="IsBRCEnabled">
                                                <div title="{{item.BusinessResourceCodeDescription}}">{{item.BusinessResourceCodeDescription}}</div>
                                            </td>
                                            <td>
                                                <div title="{{item.PerformingOrgName}}">{{item.PerformingOrgName}}</div>
                                            </td>
                                            <% if (Model.BOEIsMulti)
                                                {%>
                                            <td class="resource-wbs">{{ getWbsText(item.WBSID) }}</td>
                                            <td class="resource-clin">{{ getClinText(item.CLINID) }}</td>
                                            <%  }%>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div id="LaborFloatingDiv" class="<%if (Model.BOEIsMulti)
                                {%>resource-wbs-clin<%} %>">
                                <table class="labor-types-grid addable grid editable" id="LaborTypes" name="laborTypes">
                                    <thead>
                                        <tr>
                                            <th data-ng-repeat="customField in LaborCustomFields" class="custom-field">
                                                <div>{{customField.CustomFieldMetaData.FieldName}}{{(customField.CustomFieldMetaData.isRequired) ? '*' : ''}}</div>
                                            </th>
                                            <th class="date">Start Date<%:asterisk%></th>
                                            <th class="date">End Date<%:asterisk%></th>
                                            <th class="spread-curve">Spread Curve (<a href="#" onclick="TaskElementDetailsWidget.openWindow(currentWorkspace, boeLaborController,'<%:WebConstants.ACTION_DISPLAY_LABOR_CURVES%>'); return false;">View</a>)*</th>
                                            <th class="spread-percent">
                                                <div class="inline-block">% Spread*</div>
                                            </th>
                                            <th class="spread-hours">
                                                <div class="inline-block"><%: Model.HoursLabel%> Spread*</div>
                                            </th>
                                            <th class="resourcetype-cost">
                                                <div class="inline-block">Cost*</div>
                                            </th>
                                            <% if (Model.IsOffloadWorkspace)
                                                { %>
                                            <th class="resourcetype-offload">
                                                <div class="inline-block">Offload</div>
                                            </th>
                                            <% } // end is Offload Workspace%>
                                        </tr>
                                    </thead>
                                    <tbody>


                                        <tr data-ng-repeat="item in tableData | filter: { Deleted: false }" pkid="{{item.BOELaborTypeID}}">
                                            <td data-ng-repeat="customField in LaborCustomFields" class="custom-field custom-field-editable" data-ng-init="innerIndex = $index" data-ng-if="IsDraftOrDraftLocked">{{ selectedItem = findLaborCustomField(customField, item); "" }}
                                        <input id="LT{{item.BOELaborTypeID}}CF{{customField.CustomFieldMetaData.CustomFieldID}}" tabindex="{{225 + ((12 + LaborCustomFields.length) * outerIndex) + innerIndex}}" data-ng-if="customField.CustomFieldMetaData.isOpenEnded && item.BOELaborTypeID > 0" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="LaborType-CF{{customField.CustomFieldMetaData.CustomFieldID}}" customfieldvalueid="{{selectedItem.openEndedId}}" selectionid="{{selectedItem.selectedID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="true" class="customField LaborTypeCustomField skip-read-only" maxlength="250" onchange="TaskElementDetailsWidget.setDirty(); validateCustomField(this);" isrequired="{{customField.CustomFieldMetaData.isRequired}}" isnew="{{item.NewLaborType}}" data-ng-value="selectedItem.openEndedValue" />
                                                <select id="LT{{item.BOELaborTypeID}}CF{{customField.CustomFieldMetaData.CustomFieldID}}" tabindex="{{225 + ((12 + LaborCustomFields.length) * outerIndex) + innerIndex}}" data-ng-if="!customField.CustomFieldMetaData.isOpenEnded && item.BOELaborTypeID > 0" onchange="TaskElementDetailsWidget.setDirty(); validateCustomField(this);" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="TaskElement-CF{{customField.CustomFieldMetaData.CustomFieldID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="false" isrequired="{{customField.CustomFieldMetaData.isRequired}}"
                                                    class="customField LaborTypeCustomField skip-read-only" selectionid="{{selectedItem.selectedID}}" isnew="{{item.NewLaborType}}">
                                                    <option value=""></option>
                                                    <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                                                    <option data-ng-repeat="option in customField.CustomFieldOptions" data-ng-value="option.CustomFieldOptionID" data-ng-selected="selectedItem.selectedOptionID == option.CustomFieldOptionID">{{option.ID}}-{{option.Description}}</option>
                                                </select>
                                            </td>
                                            <td data-ng-repeat="customField in LaborCustomFields" class="custom-field custom-field-locked" data-ng-if="!IsDraftOrDraftLocked">{{ getLaborCustomFieldText(item, customField)}}</td>
                                            <td class="text start-date">{{item.StartDate }}</td>
                                            <td class="text end-date">{{item.EndDate }}</td>
                                            <td>{{getSpreadCurveText(item)}}</td>
                                            <td class="text ltpercentspread">{{getPercentSpreadText(item.PercentSpread)}}</td>
                                            <td class="text lthourspread">{{getHoursSpreadText(item.HourSpread)}}</td>
                                            <td class="text ltcost">${{getCostSpreadText(item.CostSpread)}}</td>
                                            <% if (Model.IsOffloadWorkspace)
                                                { %>
                                            <td class="resourcetype-offload">
                                                <div class="inline-block">{{item.CanOffload}}</div>
                                            </td>
                                            <% } // end is Offload Workspace%>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </form>
                    <% } %>
                </div>
            </div>
        </div>
    </div>
    <div id="LaborSpreadPlaceholder">
        <div class="labor-spread module <% if (!Model.ContainsDiscrete)
            { %>collapsed<% }
            else
            { %>expanded<% } %>"
            id="ManageLaborSpread">
            <div class="module-header-data">
                Resource Spread
            </div>
            <div class="module-content-data expanded-content">
                <div id="LaborSpreadGridContent" class="clear labor-spread-grid-content">
                    <gen-validation data-errors="laborSpreadErrors"></gen-validation>
                    <div id="LaborSpreadGridBlock" class="ReadOnly">
                        <div data-ng-class="{'labor-spread-labels-brc': IsBRCEnabled }" class="labor-spread-labels">
                            <table class="header-rows">
                                <thead>
                                    <tr>
                                        <th>Resource</th>
                                        <th data-ng-if="IsBRCEnabled">Business Resource Code</th>
                                        <th>Performing Org</th>
                                        <th class="display-none">&nbsp;</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <%
                                        /*
										 * Note: This table uses fixed-size column widths to ensure the contents will not overflow into the adjacent (scrolling) div/table.
										 * The spans (foreach below) that hold the data must be sized to fit the widest content for the entire column (here specifically
										 * the "Total" labels).  Then this table container-div (above) as well as the scrolling div (below) must be sized accordingly (i.e.
										 * to fit within the outer div width).  Really just a matter of playing with the widths until everything lays out correctly.
										 */
                                    %>
                                    <tr data-ng-repeat="item in tableData | filter: { Deleted: false }">
                                        <td title="{{item.ResourceName}}"><span>{{ item.ResourceName ? item.ResourceName : '_'}}</span></td>
                                        <td title="{{item.BusinessResourceCodeName}}" data-ng-if="IsBRCEnabled"><span>{{ item.BusinessResourceCodeName ? item.BusinessResourceCodeName : '_'}}</span></td>
                                        <td class="PerformingOrgName" title="{{item.PerformingOrgName}}"><span>{{item.PerformingOrgName ? item.PerformingOrgName : "_"}}</span></td>
                                    </tr>
                                    <tr data-ng-if="showUCOT" class="subheader">
                                        <td colspan="3" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">UCOT Business Resource Code</td>
                                    </tr>
                                    <tr data-ng-if="showUCOT" data-ng-repeat="item in tableData | filter: { Deleted: false, NewLaborType: false } track by item.BOELaborTypeID">
                                        <td data-ng-if="item.RateType === ManageTaskModel.RateTypeHours && item.UcotHours > 0" colspan="3" title="{{item.BusinessResourceCodeName}}"><span>{{ item.BusinessResourceCodeName}}</span></td>
                                    </tr>
                                    <tr id="LaborSpreadHeaderDividerRow" class="subheader">
                                        <td colspan="{{IsBRCEnabled ? 3 : 2}}" style="background-color: #EBEBEB; line-height: 2px; padding: 0px;">&nbsp;</td>
                                    </tr>
                                    <tr>
                                        <td class="subheader" colspan="{{IsBRCEnabled ? 3 : 2}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total <%: Model.HoursLabel %> by Months{{showUCOT ? ' excluding UCOT' : ''}}</td>
                                    </tr>
                                    <tr data-ng-if="showUCOT">
                                        <td class="subheader" colspan="3" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total UCOT <%: Model.HoursLabel %> by Months</td>
                                    </tr>
									<tr>
                                        <td class="subheader" colspan="{{IsBRCEnabled ? 3 : 2}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total Discrete Cost by Months</td>
                                    </tr>
                                    <tr>
                                        <td class="subheader" colspan="{{IsBRCEnabled ? 2 : 1}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total <%: Model.HoursLabel %>{{showUCOT ? ' excluding UCOT' : ''}}</td>
                                        <td class="hours-total">{{totalSpreadHours}}</td>
                                    </tr>
                                    <tr data-ng-if="showUCOT">
                                        <td class="subheader" colspan="2" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total UCOT <%: Model.HoursLabel %></td>
                                        <td class="hours-total">{{totalUcotSpreadHours}}</td>
                                    </tr>
                                    <tr data-ng-if="showUCOT">
                                        <td class="subheader" colspan="2" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Grand Total <%: Model.HoursLabel %></td>
                                        <td class="hours-total">{{grandTotalHours}}</td>
                                    </tr>
                                    <tr>
                                        <td class="subheader" colspan="{{IsBRCEnabled ? 2 : 1}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total Discrete Cost</td>
                                        <td class="cost-total"><span class="labor-spread-currency">$</span>{{ totalSpreadCost}}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                        <div data-ng-if="(tableData | filter: { Deleted: false }).length > 0" data-ng-class="{'labor-spread-scroll-brc': IsBRCEnabled}" class="labor-spread-scroll">
                            <table class="data" name="LaborSpreadData">
                                <thead>
                                    <tr>
                                        <% // display the header (all dates across the spread)
// Note:  putting divs in the th cells and setting the width on them was the only method I tried
// that actually worked to set the columns to a specific width.  Setting the width on a col element,
// or the th itself, or a span in the th resulted in either it being ignored or being treated as a
// relative size.
                                        %>
                                        <th data-ng-repeat="dt in model.SpreadDatesFull">
                                            <div>{{dt}}</div>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr data-ng-repeat="item in tableData | filter: { Deleted: false }" data-ng-init="dollarSign = getDollarSign(item);">
                                        <td data-ng-repeat="dt in model.SpreadDatesFull" date="{{dt}}">{{dollarSign}}{{getSpreadMonthText(item, dt)}}</td>
                                    </tr>
                                    <tr data-ng-if="showUCOT" class="subheader" colspan="2" style="background-color: #EBEBEB;">
                                        <td colspan="{{model.SpreadDatesFull.length}}" style="border: 0;padding: 2px">&nbsp;</td>
                                    </tr>
                                    <tr data-ng-if="showUCOT && item.UcotHours > 0" data-ng-repeat="item in tableData | filter: { Deleted: false, NewLaborType: false, RateType: ManageTaskModel.RateTypeHours } track by item.BOELaborTypeID">
                                        <td data-ng-repeat="dt in model.SpreadDatesFull" date="{{dt}}">{{getUcotSpreadMonthText(item, dt)}}</td>
                                    </tr>
                                    <tr id="LaborSpreadDataDividerRow">
                                        <td colspan="{{model.SpreadDatesFull.length}}" class="subheader" style="background-color: #EBEBEB; line-height: 2px; padding: 0px;">&nbsp;</td>
                                    </tr>
                                    <tr class="hours-total">
                                        <td data-ng-repeat="dt in model.SpreadDatesFull" date="{{dt}}">{{getHoursTotals(dt)}}</td>
                                    </tr>
                                    <tr data-ng-if="showUCOT" class="hours-total">
                                        <td data-ng-repeat="dt in model.SpreadDatesFull" date="{{dt}}">{{getUcotHoursTotals(dt)}}</td>
                                    </tr>
                                    <tr class="cost-total">
                                        <td data-ng-repeat="dt in model.SpreadDatesFull" date="{{dt}}"><span class="labor-spread-currency">$</span>{{getCostTotals(dt)}}</td>
                                    </tr>
                                    <tr>
                                        <td colspan="{{model.SpreadDatesFull.length}}" style="border: 0;">&nbsp;</td>
                                    </tr>
                                    <tr>
                                        <td colspan="{{model.SpreadDatesFull.length}}" style="border: 0; padding-bottom: 3px;">&nbsp;</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
    <% if (Model.EnableSkillMix)
        { %>
    <div id="SkillMixPlaceholder" data-ng-show="showSkillMix()">
        <div class="skill-mix-tables module collapsed" id="SkillMixRationaleContainer">
            <div class="module-header-data">
                Skill Mix Rationale
            </div>
            <div class="module-content-data expanded-content">
                <div class="form-element" data-ng-if="isSkillMixDisabled()">
                    <div class="form-label"><p>The SAP MOQ Actuals have not been calculated.  Please ensure all MOQ Tables are updated to enable Skill Mix.</p><br />&nbsp;</div>
                </div>
                <div class="form-element" data-ng-if="!isSkillMixDisabled()">
                    <!-- Skill Mix Table -->
                    <div class="form-label">
                        <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                            {  %>
                                    Legacy Skill Mix Table
                        <% } %>
                        <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                            {  %>
                                    Current Skill Mix Table
                        <%  } %>
                    </div>
                    <div class="SkillMixTable skillMixTable">
                        <table name="currentSkillMix" class="grid editable">
                            <thead>
                                <tr>
                                    <th style="width: 95px;">Resource</th>
                                    <th data-ng-if="!ManageTaskModel.IsSpace" style="width: 130px;">Current Resource</th>
                                    <th style="width: 100px;">Historical Hours</th>
                                    <th style="width: 90px;">Labor Skill Mix</th>
                                    <th style="width: 55px">Included</th>
                                    <th style="width: 90px;">BOE Skill Mix</th>
                                    <th style="width: 95px;">Proposed Hours</th>
                                    <th>Rationale**</th>
                                </tr>
                            </thead>
                            <tbody>
                                <!-- Display the data for each of the Skill Mix Table rows. -->
                                <tr ng-repeat="row in skillMixRationale.data.SkillMixRows">
                                    <td>{{row.ResourceOld}}</td>
                                    <td data-ng-if="!ManageTaskModel.IsSpace">{{row.ResourceNew}}</td>
                                    <td style="text-align: right">{{row.HistoricalHours | number:2}}</td>
                                    <td style="text-align: right">{{row.LaborSkillMix | number:1}}%</td>
                                    <td>{{row.Included | yesNo}}</td>
                                    <td style="text-align: right">{{row.BOESkillMix | number:1}}%</td>
                                    <td style="text-align: right">{{row.ProposedHours}}</td>
                                    <td>{{row.Rationale}}</td>
                                </tr>
                                <!-- Display the Skill Mix Totals row. -->
                                <tr>
                                    <td>Totals</td>
                                    <td data-ng-if="!ManageTaskModel.IsSpace"></td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.HistoricalHours | number:2}}</td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.LaborSkillMix | number:1}}%</td>
                                    <td></td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.BoeSkillMix | number:1}}%</td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.ProposedHours}}</td>
                                    <td></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <!-- Common Disclosure Skill Mix Table -->
                    <div class="form-label" data-ng-show="IsBRCEnabled">
                        LM Enterprise Skill Mix Table
                    </div>
                    <div class="SkillMixTable skillMixTable" data-ng-show="IsBRCEnabled">
                        <table name="currentSkillMix" class="grid editable">
                            <thead>
                                <tr>
                                    <th style="width: 95px;">Resource</th>
                                    <th style="width: 130px;">Business Resource Code</th>
                                    <th style="width: 100px;">Historical Hours</th>
                                    <th style="width: 90px;">Labor Skill Mix</th>
                                    <th style="width: 55px">Included</th>
                                    <th style="width: 90px;">BOE Skill Mix</th>
                                    <th style="width: 95px;">Proposed Hours</th>
                                    <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                                        {  %>
                                        <th style="width: 90px;">UCOT Hours</th>
                                    <% } %>
                                    <th>Rationale**</th>
                                </tr>
                            </thead>
                            <tbody>
                                <!-- Display the data for each of the Common Disclosure Skill Mix Table rows. -->
                                <tr ng-repeat="row in skillMixRationale.data.CommonDisclosureRows">
                                    <td>{{row.ResourceID}}</td>
                                    <td>{{row.BusinessResourceID}}</td>
                                    <td style="text-align: right">{{row.HistoricalHours | number:2}}</td>
                                    <td style="text-align: right">{{row.LaborSkillMix | number:1}}%</td>
                                    <td>{{row.Included | yesNo}}</td>
                                    <td style="text-align: right">{{row.BOESkillMix | number:1}}%</td>
                                    <td style="text-align: right">{{row.ProposedHours}}</td>
                                    <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                                    {  %>
                                        <td style="text-align: right">{{row.UCOTHours | number:2}}</td>
                                    <% } %>
                                    <td>{{row.Rationale}}</td>
                                </tr>
                                <!-- Display the Common Disclosure Totals row. -->
                                <tr>
                                    <td>Totals</td>
                                    <td></td>
                                    <td style="text-align: right">{{skillMixRationale.data.CommonDisclosureTotals.HistoricalHours | number:2}}</td>
                                    <td style="text-align: right">{{skillMixRationale.data.CommonDisclosureTotals.LaborSkillMix | number:1}}%</td>
                                    <td></td>
                                    <td style="text-align: right">{{skillMixRationale.data.CommonDisclosureTotals.BoeSkillMix | number:1}}%</td>
                                    <td style="text-align: right">{{skillMixRationale.data.CommonDisclosureTotals.ProposedHours}}</td>
                                    <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                                        {  %>
                                        <td style="text-align: right">{{skillMixRationale.data.CommonDisclosureTotals.UCOTHours | number:2}}</td>
                                    <% } %>
                                    <td></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        <% } %>
        <div class="buttons-left"></div>

        <div class="buttons">
            <div class="required-note">
                <div>* required for saving as draft.</div>
                <div>** required for validating and submitting for approval</div>
            </div>
            <div class="oci-note"><b>Note: </b><span id="Task-OCINote"></span></div>
            <button id="Save-BOEUpdatesAndClose" data-ng-hide="isSaving" data-ng-click="saveAndClose(true)" class="ies-action stateful_button" name="save-button" type="button">Save & Close</button>
            <div id="Loader-BOEUpdates" class="loader" data-ng-show="isSaving"></div>
            <button id="Cancel-BOEUpdates" class="ies" name="cancel-button" type="button">Cancel</button>
            <button id="Next-Task" data-ng-disabled="isNextTaskDisabled()" data-ng-click="navigateToNext()" class="ies" name="next-task-button" type="button">Next Task</button>
            <button id="Previous-Task" data-ng-disabled="isPreviousTaskDisabled()" data-ng-click="navigateToPrevious()" class="ies" name="previous-task-button" type="button">Previous Task</button>
        </div>
        <div class="buttons-right"></div>
    </div>
