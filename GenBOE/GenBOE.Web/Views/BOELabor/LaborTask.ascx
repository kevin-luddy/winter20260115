<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.LaborTaskModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>
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
    // Checks to see if the container is visible. This is needed for the custom field validation on page load
    function checkContainer() {
        if ($('#LaborTypes').is(':visible')) {
            validateCustomFields();
        } else {
            setTimeout(checkContainer, 50);
        }
    }

    // Validates all custom fields on the page
    validateCustomFields = function () {
        $('.LaborTypeCustomField').each(function (i, item) {
            validateCustomField(item);
        });
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

    validateMoqTableCustomField = function (item) {
        if ($(item).attr('required') === 'required') {
            if (item.value.length === 0) {
                $(item).addClass('ng-invalid');
            } else {
                $(item).removeClass('ng-invalid');
            }
        }
    }

    var debugOutputEnabled = false;
    var isSaveButtonHidden = false;

    // Get the read-only attribute passed in from the controller
    var ManageWBS_ContainsOCI = <%= Model.ContainsOci.ToString().ToLower() %>;
    var currentWorkspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var boeLaborController = '<%: WebConstants.CONTROLLER_BOE_LABOR %>';
    var isBrcEnabled = '<%= Utilities.IsBRCEnabledForWorkspace(SiteMasterUtilities.GetCurrentWorkspace()) %>'.isTrue();
    var completeImportUrl = CreatePostURL(currentWorkspace, boeLaborController,
                        '<%:WebConstants.ACTION_IMPORT_LABOR_TYPE_AND_SPREAD %>',
        'boe/' + '<%= ViewData["BOEID"] %>' + '/taskelement/' + '<%: ViewData["TASKID"] %>');

    var boeId = '<%: ViewData["BOEID"] %>';
    var taskId = '<%: ViewData["TASKID"] %>';
    var taskElementId = '-1';
    if (taskId != '') {
        taskElementId = taskId;
    }

    var TaskElementDetailsWidget;

    var TaskLaborTypesWidget;
    var TaskLaborSpreadsWidget;
    var TaskSkillMixWidget;
    var ImportLaborType = InitializeImportLaborTypeWidget('<%: ViewData["BOEID"] %>',
        currentWorkspace,
        completeImportUrl,
        <%: (int)SpreadType.Hours %>,
        <%: (int)SpreadType.Cost %>
    );

    ImportLaborType.UploadComplete = function () { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportLaborTypeDialog-UploadTarget").contents().find("body #UploadResponse");

        //hide and unload sections
        $("#ImportLaborTypeResults div.import-result-type").addClass('display-none');
        $('#ImportLaborTypeResults div.import-result-type ul.resultsList').empty();
        $('#ImportResults ul').empty();

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {

            var results = eval('(' + uploadResponseElement.html() + ')');
            if (results.Status) {
                ImportLaborType.CloseDialog(ImportLaborType.ImportLaborTypesDialog);
                ImportLaborType.ImportedData = results.Data;
                ImportLaborType.ImportedSpreadData = [];

                $('#ManageLaborType-CompleteImportButton').addClass('display-none');

                if (ImportLaborType.ImportedData.length == 0) {
                    $('#ImportLaborTypeResults #ImportResult-NoChanges').removeClass('display-none');
                }
                else {
                    var missingDataRows = 0;

                    for (var ltresultNdx = 0; ltresultNdx < ImportLaborType.ImportedData.length; ltresultNdx++) {
                        var importedSpreads = ImportLaborType.ImportedData[ltresultNdx].ImportedLaborSpreads;
                        if (importedSpreads != undefined) {
                            for (var spreadNdx = 0; spreadNdx < importedSpreads.length; spreadNdx++) {
                                var importedSpread = importedSpreads[spreadNdx];

                                //fix resource and performing org for new spreads
                                importedSpread.Resource = ImportLaborType.ImportedData[ltresultNdx].Resource;
                                importedSpread.PerformingOrg = ImportLaborType.ImportedData[ltresultNdx].PerformingOrg;

                                ImportLaborType.ImportedSpreadData.push(importedSpread);
                            }
                        }

                        for (var ltresultTypeNdx = 0; ltresultTypeNdx < ImportLaborType.ImportedData[ltresultNdx].ImportTypes.length; ltresultTypeNdx++) {
                            var ImportType = ImportLaborType.ImportedData[ltresultNdx].ImportTypes[ltresultTypeNdx].toString();

                            switch (ImportType) {
                                case '<%: (int)LaborTypeImportResult.MissingData %>':
                                case '<%: (int)LaborTypeImportResult.ResourceMultiValuesInvalid %>':
                                case '<%: (int)LaborTypeImportResult.InvalidData %>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    if (!isBrcEnabled) {
                                        $('#ImportLaborTypeResults #ImportResult-MissingData').removeClass('display-none');
                                        $('#ImportLaborTypeResults #ImportResult-MissingData ul.resultsList').append(listItemToAppend);
                                    }
                                    else {
                                        $('#ImportLaborTypeResults #ImportResult-MissingData-BRCEnabled').removeClass('display-none');
                                        $('#ImportLaborTypeResults #ImportResult-MissingData-BRCEnabled ul.resultsList').append(listItemToAppend);
                                    }
                                    missingDataRows++;
                                    break;
                                case '<%: (int)LaborTypeImportResult.AddLaborType%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-Added').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-Added ul.resultsList').append(listItemToAppend);
                                    $('#ManageLaborType-CompleteImportButton').removeClass('display-none');
                                    break;
                                case '<%: (int)LaborTypeImportResult.UpdateLaborType%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-Updated').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-Updated ul.resultsList').append(listItemToAppend);
                                    $('#ManageLaborType-CompleteImportButton').removeClass('display-none');
                                    break;
                                case '<%: (int)LaborTypeImportResult.LaborTypeDateOutsideOfPOPDateRange%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-Dates').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-Dates ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.RateTypeSpreadTypeAgreement%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-RateTypeSpreadTypeAgreement').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-RateTypeSpreadTypeAgreement ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.ResourceTypeIDMissingOrInvalid%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-ResourceTypeIDMissingOrInvalid').removeClass('display-none');
                                    break;
                                case '<%: (int)LaborTypeImportResult.HoursSpreadInvalid%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-HoursSpreadInvalid').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-HoursSpreadInvalid ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.CostDecimalPrecisionViolation%>':
                                case '<%: (int)LaborTypeImportResult.CostSpreadRangeInvalid%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-CostInvalid').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-CostInvalid ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.SpreadMonthColumnInvalid%>':
                                    $('#ImportLaborTypeResults #ImportResult-SpreadMonthColumnInvalid').removeClass('display-none');
                                    break;
                                case '<%: (int)LaborTypeImportResult.SpreadMonthValueOutsideDateRange%>':
                                    $('#ImportLaborTypeResults #ImportResult-SpreadMonthValueOutsideDateRange').removeClass('display-none');
                                    break;
                                case '<%: (int)LaborTypeImportResult.RateTypesDoNotMatch%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-RateTypesDoNotMatch').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-RateTypesDoNotMatch ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.MissingResource%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-MissingResource').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-MissingResource ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.MissingBusinessResourceCode%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-MissingBusinessResourceCode').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-MissingBusinessResourceCode ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.MissingStartEndDate%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-MissingStartEndDate').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-MissingStartEndDate ul.resultsList').append(listItemToAppend);
                                    break;
                                case '<%: (int)LaborTypeImportResult.MissingResourceOrBRC%>':
                                    var listItemToAppend = ImportLaborType.createPreviewOutput(ImportLaborType.ImportedData[ltresultNdx]);
                                    $('#ImportLaborTypeResults #ImportResult-MissingResourceOrBRC').removeClass('display-none');
                                    $('#ImportLaborTypeResults #ImportResult-MissingResourceOrBRC ul.resultsList').append(listItemToAppend);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                for (var resultNdx = 0; resultNdx < ImportLaborType.ImportedSpreadData.length; resultNdx++) {
                    var ImportTypes = [];

                    for (var resultTypeNdx = 0; resultTypeNdx < ImportLaborType.ImportedSpreadData[resultNdx].ImportTypes.length; resultTypeNdx++) {
                        var ImportType = ImportLaborType.ImportedSpreadData[resultNdx].ImportTypes[resultTypeNdx].toString();

                        var resource = ImportLaborType.ImportedSpreadData[resultNdx].Resource;
                        var performingOrg = ImportLaborType.ImportedSpreadData[resultNdx].PerformingOrg;
                        var laborSpreadDateFormatted = ImportLaborType.ImportedSpreadData[resultNdx].LaborSpreadDateFormatted;

                        if (resource == undefined) {
                            resource = '';
                        }
                        if (performingOrg == undefined) {
                            performingOrg = '';
                        }
                        if (laborSpreadDateFormatted == undefined) {
                            laborSpreadDateFormatted = '';
                        }

                        switch (ImportType) {
                            case '<%: (int)LaborSpreadImportResult.NoSpreadDate %>':
                                var listItemToAppend =
                                    '<li>Please check the Start Date and End Date columns of the import file for empty fields.</li>';
                                $('#ImportLaborTypeResults #ImportResults ul[result=' + ImportType + ']').append(listItemToAppend);
                                break;

                            case '<%: (int)LaborSpreadImportResult.InvalidSpreadDateFormat %>':
                                var listItemToAppend =
                                    '<li>Please check the Start Date and End Date columns of the import file for invalid dates.</li>';
                                $('#ImportLaborTypeResults #ImportResults ul[result=' + ImportType + ']').append(listItemToAppend);
                                break;

                            case '<%: (int)LaborSpreadImportResult.MissingResources %>':
                                var listItemToAppend = '<li>Please check the import file for missing resources.</li>';
                                $('#ImportLaborTypeResults #ImportResults ul[result=' + ImportType + ']').append(listItemToAppend);
                                break;
                            case '<%: (int)LaborSpreadImportResult.MissingStartDate %>':
                                var listItemToAppend = '<li>Please check the Start Date column of the import file for empty values.</li>';
                                $('#ImportLaborTypeResults #ImportResults ul[result=' + ImportType + ']').append(listItemToAppend);
                                break;
                            case '<%: (int)LaborSpreadImportResult.MissingEndDate %>':
                                var listItemToAppend = '<li>Please check the End Date column of the import file for empty values.</li>';
                                $('#ImportLaborTypeResults #ImportResults ul[result=' + ImportType + ']').append(listItemToAppend);
                                break;

                            case '<%: (int)LaborSpreadImportResult.UpdateSpread %>':
                                $('#ManageLaborType-CompleteImportButton').removeClass('display-none');
                                var listItemToAppend =
                                    '<li>Resource: <b>' + resource + '</b> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Performing Organization: <b>' + performingOrg + '</b> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Date: <b>' + laborSpreadDateFormatted + '</b></li>';
                                $('#ImportLaborTypeResults #ImportResults ul[result=' + ImportType + ']').append(listItemToAppend);
                                break;
                            case '<%: (int)LaborSpreadImportResult.InvalidSpreadValue %>':
                            case '<%: (int)LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange %>':
                            case '<%: (int)LaborSpreadImportResult.StartDateChanged %>':
                            case '<%: (int)LaborSpreadImportResult.EndDateChanged %>':
                            case '<%: (int)LaborSpreadImportResult.ResourceHourValueTooLarge %>':
                            case '<%: (int)LaborSpreadImportResult.ResourceCostValueTooLarge %>':
                            case '<%: (int)LaborSpreadImportResult.CostDecimalPrecisionViolation %>':
                            default:
                                var listItemToAppend =
                                    '<li>Resource: <b>' + resource + '</b> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Performing Organization: <b>' + performingOrg + '</b> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Date: <b>' + laborSpreadDateFormatted + '</b></li>';
                                $('#ImportLaborTypeResults #ImportResults ul[result=' + ImportType + ']').append(listItemToAppend);
                                break;
                        }

                        ImportTypes.push(ImportType.toString());
                    }

                    ImportLaborType.ImportedSpreadData[resultNdx].ImportTypes = ImportTypes;
                }

                $('#ImportLaborTypeResults #ImportResults ul[result]').each(function () {
                    if ($(this).children().length == 0) {
                        $(this).prev().addClass('display-none');
                    }
                });

                $('#ImportLaborTypeResults #ImportResult-MissingData .resultCount').text(missingDataRows);
                $('#ImportLaborTypeResults #ImportResult-Added .resultCount').text($('#ImportLaborTypeResults #ImportResult-Added .resultsList li').length);
                $('#ImportLaborTypeResults #ImportResult-Updated .resultCount').text($('#ImportLaborTypeResults #ImportResult-Updated .resultsList li').length);
                $('#ImportLaborTypeResults #ImportResult-Dates .resultCount').text($('#ImportLaborTypeResults #ImportResult-Dates .resultsList li').length);
                $('#ImportLaborTypeResults #ImportResult-NotAdjusted .resultCount').text($('#ImportLaborTypeResults #ImportResult-NotAdjusted .resultsList li').length);

                ImportLaborType.OpenDialogAfterInitialize(ImportLaborType.ImportLaborTypesResultsDialog);
            }
            else {
                var errorMessages = "";
                if (results.Message == 'Errors') {
                    for (message in results.Data) {
                        errorMessages += "<b>" + results.Data[message].Title + "</b><br/>";
                        errorMessages += results.Data[message].Message + "<br/><br/>";
                    }
                } else {
                    errorMessages = results.Message;
                }

                $("#LaborTypeImportErrors .errorSpace").html(errorMessages);
                ImportLaborType.OpenDialogAfterInitialize(ImportLaborType.ImportErrorDialog);
            }
        }

        $("#ManageLaborType-ImportLoader").addClass('display-none');
        $("#ManageLaborType-ImportButton").removeClass('display-none');
    }

    $(function () {
        $(document).trigger("SHOW_LOADING_BOX");

        // Labor Types

        var widgetConfig = {};
        widgetConfig.ContextID = "LaborTypesWidgetContainer";
        widgetConfig.isReadOnly = false;
        widgetConfig.IsModule = true;
        TaskLaborTypesWidget = new GenWidget(widgetConfig);

        // Labor Spreads

        var spreadWidgetConfig = {};
        spreadWidgetConfig.ContextID = "ManageLaborSpread";
        spreadWidgetConfig.isReadOnly = false;
        spreadWidgetConfig.IsModule = true;
        TaskLaborSpreadsWidget = new GenWidget(spreadWidgetConfig);

        // Skill Mix Rationale
        var skillMixConfig = {};
        skillMixConfig.ContextID = "SkillMixRationaleContainer";
        skillMixConfig.isReadOnly = false;
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
        var dateShiftUrl = CreatePostURL(
        '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        '<%: WebConstants.CONTROLLER_DATESHIFT %>',
        '<%: WebConstants.ACTION_INDEX %>',
            'id/' + taskElementId + '/level/<%: ((int)IES.Common.Level.Task).ToString() %>');

        var saveReorderLaborTypesUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        '<%: WebConstants.CONTROLLER_BOE_LABOR %>',
            '<%: WebConstants.ACTION_SAVE_REORDER_LABOR_TYPES %>',
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
            dateShiftUrl,
            saveReorderLaborTypesUrl,
            '<%: showDescQuestions %>'.isTrue(),
            <%: numberDescQuestions %>,
            <%: rteFieldSize %>
        );

        TaskElementDetailsWidget.waitingBeforeSubmit = false;

        // End Task Element details       

        $("#Cancel-BOEUpdates").click(TaskElementDetailsWidget.CancelToMainGrid);

        //Change the OCI note based off the workspace
        if (ManageWBS_ContainsOCI == true) {
            $('#Task-OCINote').html('Must not contain any classified, export controlled or third party proprietary information.');
        }
        else {
            $('#Task-OCINote').html('Must not contain any OCI, classified, export controlled or third party proprietary information.');
        }

        AfterDomLoadImportLaborTypeWidget(ImportLaborType);

        $(document).one("MOQWidgetLoaded", function () {
            $(document).trigger("AllWidgetLoaded");
            $(document).trigger("ValidateMOQEquation", { initialLoad: true });
        });

        <% if (Model.ContainsDiscrete)
    { %>
        // Change the Labor Spread Section to be expanded if collapsed
        $('#ManageLaborSpread.collapsed .collapsible-header').click();
        <% } %>

        refreshModule($('.boe-details .module'));

        checkContainer();
    });
</script>

<div id="TaskElementsComposite" class="task-elements-composite composite container-fluid" data-ng-app="genboe" data-ng-controller="ManageTaskController" data-ng-init="init('<%= ViewData["TASKID"] != null ? ViewData["TASKID"] : "-1" %>')" data-ng-cloak="">
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
                <div class="loader" data-ng-show="isLoading"></div>
                <div data-ng-hide="isLoading" class="ng-cloak">
                    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "TaskElementDetailsForm" }))
                        { %>

                    <gen-validation id="mainErrorBox" data-errors="errors"></gen-validation>
                    <div style="margin-bottom: 10px; display: none;" id="recalculateTaskElementButton">
                        <button type="button" onclick="TaskElementDetailsWidget.RecalculateAndRefreshPage()" class="ies-danger">Recalculate Task Element</button>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Task ID 
                        </div>
                        <div class="form-element">
                            <input type="text" data-ng-model="model.TaskElementData.TaskID" class="half" maxlength="3" data-ng-blur="setDirty()" />
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Task Title *
                        </div>
                        <div class="form-element">
                            <input type="text" data-ng-model="model.TaskElementData.Title" class="half" maxlength="100" data-ng-blur="setDirty()" />
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
                            <input type="text" class="normal-date" disabled="disabled" data-ng-model="model.TaskElementData.StartDate" name="StartDate" id="CurrentTaskStartDate" />
                        </div>
                        <div class="form-label task-end-date-label">
                            Task End Date *
                        </div>
                        <div class="form-element task-end-date">
                            <input type="text" class="normal-date" disabled="disabled" data-ng-model="model.TaskElementData.EndDate" name="EndDate" id="CurrentTaskEndDate" />
                            <a id="AdjustTaskDatesLink" style="float: right;">Adjust Dates</a>
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
                    <div class="form-row" data-ng-repeat="customField in TaskCustomFields">
                        <div class="form-label">
                            {{ selectedItem = findTaskCustomField(customField); ""}} 
                            {{customField.CustomFieldMetaData.FieldName}} {{customField.CustomFieldMetaData.isRequired ? '*' : ''}}
                        </div>
                        <div class="form-element" id="TaskElementCustomFieldID">
                            <input data-ng-if="customField.CustomFieldMetaData.isOpenEnded" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="TaskElement-CF{{customField.CustomFieldMetaData.CustomFieldID}}" customfieldvalueid="{{selectedItem.openEndedId}}" selectionid="{{selectedItem.selectedID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="true" class="customField TaskElementCustomField" maxlength="250" onchange="TaskElementDetailsWidget.setDirty()" data-ng-value="selectedItem.openEndedValue" />
                            <select data-ng-if="!customField.CustomFieldMetaData.isOpenEnded" onchange="TaskElementDetailsWidget.setDirty()" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="TaskElement-CF{{customField.CustomFieldMetaData.CustomFieldID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="false"
                                class="customField TaskElementCustomField" selectionid="{{selectedItem.selectedID}}">
                                <option value="-1"></option>
                                <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                                <option data-ng-repeat="option in customField.CustomFieldOptions" data-ng-value="option.CustomFieldOptionID" data-ng-selected="selectedItem.selectedOptionID == option.CustomFieldOptionID">{{option.ID}}-{{option.Description}}</option>
                            </select>
                        </div>
                    </div>
                    <%  }//end form %>
                </div>
            </div>
        </div>
    </div>
    <script type="text/ng-template" id="customPerfOrgTemplate.html">
      <a><span ng-bind-html="match.model.Label | uibTypeaheadHighlight:query"></span></a>
    </script>
    <div id="LaborTypesPlaceholder">
        <div class="labor-types module expanded" id="LaborTypesWidgetContainer">
            <div class="module-header-data">
                <div class="float-left">Resource Types</div>
                <div class="float-right right-header">
                    <div style="float: left; margin-top: -8px;" id="LaborTypeDeltaLabel" class="labortype-label">Delta <%: Model.HoursLabel%></div>
                    <div style="float: left; margin-top: -8px;" id="LaborTypeDelta" class="labortype"><span data-ng-if="!invalidSpreads">{{deltaHours}}</span><span data-ng-if="invalidSpreads">#ERR</span></div>
                    <div style="float: left; width: 1px; height: 32px; margin-top: 2px; background-color: #999A95;">&nbsp;</div>
                    <div class="float-right labor-calc-container">
                        <div class="module-header-data-first-row" style="line-height: 20px;">
                            <div id="LaborTypeTotalLabel" class="labortype-label">Total <%: Model.HoursLabel%> </div>
                            <div id="LaborTypeTotal" class="labortype float-right"><span data-ng-if="invalidSpreads">#ERR</span><span data-ng-if="!invalidSpreads">{{totalHours}}</span></div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="module-content-data expanded-content">
                <div class="loader" data-ng-show="isLoading"></div>
                <div data-ng-hide="isLoading">
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
                    <gen-validation data-errors="laborTypeErrors"></gen-validation>
                    <div id="LTWidget" class="addedableLaborTypes" data-ng-cloak>

                        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "LaborTypesForm" }))
                            { %>
                        <ul class="validation-box"></ul>
                        <div class="import-buttons">
                            <button data-ng-hide="taskElementId == -1" data-ng-disabled="isDirty() || isLoading || isExporting" data-ng-click="Import()" id="LTImportButton" class="ies-action" type="button">Import</button>
                            <button data-ng-hide="taskElementId == -1" data-ng-disabled="isDirty() || isLoading || isExporting" data-ng-click="tryExport()" id="LTExportButton" class="ies-action" type="button">Export</button>
                            <button data-ng-hide="taskElementId == -1" data-ng-disabled="isDirty() || isLoading || isExporting" data-ng-click="reorder()" id="LTReorderButton" class="ies-action" type="button">Sort Resource Types</button>
                            <button data-ng-disabled="isLoading || isExporting" data-ng-click="duplicate()" id="LTDucplicateButton" class="ies-action" type="button">Duplicate Resource Types</button>

                            <div id="ImportNote" data-ng-show="isDirty()" class="import-note">
                                <b>Note:</b> Importing, exporting, and sorting are disabled until the task is saved.
								<br />
                                To import, export, or sort, please save the task and return to this screen.
                            </div>
                            <br />
                            <br />
                            <div>
                                <span helptext="By default, the Resource and Performing Org rows use type-ahead. Turning on dropdowns can degrade performance. If there are performance issues, and you are using Internet Explorer, try using Google Chrome.">Use Dropdowns: </span>
                                <select data-ng-change="updateDropdowns()" data-ng-disabled="isLoading || isExporting" data-ng-model="showDropdowns">
                                    <option data-ng-value="false">No</option>
                                    <option data-ng-value="true">Yes</option>
                                </select>

                            </div>
                        </div>
                        <form id="LaborTypesForm">
                            <div id="LaborTypesGridBlock">
                                <div id="LaborFixedDiv">
                                    <table class="labor-types-grid addable grid editable container-fluid" id="LaborTypesFixed" name="laborTypesFixed">
                                        <thead>
                                            <tr>
                                                <th class="element-of-cost">Element of Cost</th>
                                                <th class="resource">Resource (<a href="#" onclick="TaskElementDetailsWidget.openWindow(currentWorkspace, boeLaborController,'<%:WebConstants.ACTION_DISPLAY_LABOR_RESOURCES%>'); return false;">View</a>){{IsBRCEnabled ? '&#10013;' : '*'}}</th>
                                                <th data-ng-show="IsBRCEnabled" class="resource">Business Resource Code (<a href="#" onclick="TaskElementDetailsWidget.openWindow(currentWorkspace, boeLaborController, '<%:WebConstants.ACTION_DISPLAY_BUSINESS_RESOURCE_CODES%>'); return false;">View</a>)&#10013;</th>
                                                <th class="performing-org">Performing Org (<a href="#" onclick="TaskElementDetailsWidget.openWindow(currentWorkspace, boeLaborController,'<%:WebConstants.ACTION_DISPLAY_LABOR_PERF_ORGS%>'); return false;">View</a>)*</th>
                                                <% if (Model.BOEIsMulti)
                                                    { %>
                                                <th class="resource-wbs">WBS</th>
                                                <th class="resource-clin">CLIN</th>
                                                <% } %>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr data-ng-repeat="item in tableData | filter: { Deleted: false } track by item.BOELaborTypeID" data-ng-hide="item.Deleted" pkid="{{item.BOELaborTypeID}}">
                                                <td>
                                                    <select tabindex="{{tabindex = 220 + $index * (12 + LaborCustomFields.length)}}" data-ng-model="item.ElementOfCost" data-ng-change="elementOfCostUpdated(item)" name="ElementOfCost" class="element-of-cost">
                                                        <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                                                        <option data-ng-repeat="option in ManageTaskModel.ElementsOfCost" data-ng-value="option.ElementOfCostId">{{option.ElementOfCostName}}</option>
                                                    </select>
                                                </td>
                                                <td class="resources" data-ng-class="{ inputError: getAndSetIsResourceValid(item, ResourceModels, true) === false }">
                                                    <div class="resource-selection bootstrap">
                                                        <select data-ng-if="showDropdowns" tabindex="{{tabindex + 1}}" data-ng-model="item.ResourceInput" data-ng-change="resourceSelected(item.ResourceInput, item)"
                                                            data-ng-options="resource as resource.ResourceDesc for resource in ResourceModels | filter:{ElementOfCost:item.ElementOfCost} | orderBy:'ResourceDesc'">
                                                        </select>
                                                        <input data-ng-if="!showDropdowns" tabindex="{{tabindex + 1}}" type="text" data-ng-model="item.ResourceInput" placeholder="Select a resource" uib-typeahead="resource as resource.ResourceDesc for resource in ResourceModels | filter:{ElementOfCost:item.ElementOfCost} | filter:{ResourceDesc:$viewValue}" class="form-control resize" typeahead-select-on-exact="true" typeahead-show-hint="false" typeahead-min-length="2" data-ng-change="resourceUpdated(item)" typeahead-on-select="resourceSelected($item, item)">
                                                    </div>
                                                </td>
                                                <td data-ng-show="IsBRCEnabled" class="business-resource-codes" data-ng-class="{ inputError: getAndSetIsBusinessResourceCodeValid(item, BusinessResourceCodeModels, true) === false }">
                                                    <div class="resource-selection bootstrap">
                                                        <select data-ng-if="showDropdowns" tabindex="{{tabindex + 2}}" data-ng-model="item.BusinessResourceCodeInput" data-ng-change="businessResourceCodeSelected(item.BusinessResourceCodeInput, item)"
                                                            data-ng-options="businessResourceCode as businessResourceCode.ResourceDesc for businessResourceCode in BusinessResourceCodeModels | filter:{ElementOfCost:item.ElementOfCost} | orderBy:'ResourceDesc'">
                                                        </select>
                                                        <input data-ng-if="!showDropdowns" tabindex="{{tabindex + 2}}" type="text" data-ng-model="item.BusinessResourceCodeInput" placeholder="Select a Business Resource Code" uib-typeahead="businessResourceCode as businessResourceCode.ResourceDesc for businessResourceCode in BusinessResourceCodeModels | filter:{ElementOfCost:item.ElementOfCost} | filter:{ResourceDesc:$viewValue}" class="form-control resize" typeahead-select-on-exact="true" typeahead-show-hint="false" type-ahead-min-length="2" data-ng-change="businessResourceCodeUpdated(item)" typeahead-on-select="businessResourceCodeSelected($item, item)">
                                                    </div>
                                                </td>
                                                <td class="performing-org" data-ng-class="{inputError: isPerfOrgValid(item.PerfOrgInput, PerfOrgModels) === false && item.NewLaborType === false }">
                                                    <div class="perforg-selection bootstrap">
                                                        <select data-ng-if="showDropdowns" tabindex="{{tabindex + 3}}" data-ng-model="item.PerfOrgInput" data-ng-change="perfOrgSelected(item.PerfOrgInput, item)"
                                                            data-ng-options="perfOrg as perfOrg.PerformingOrgName for perfOrg in PerfOrgModels | orderBy:'PerformingOrgName'">
                                                        </select>
                                                        <input data-ng-if="!showDropdowns" tabindex="{{tabindex + 3}}" type="text" data-ng-model="item.PerfOrgInput" placeholder="Add Performing Org" uib-typeahead="perfOrg as perfOrg.PerformingOrgName for perfOrg in PerfOrgModels | filter:{Label:$viewValue}" typeahead-template-url="customPerfOrgTemplate.html" class="form-control resize" typeahead-select-on-exact="true" typeahead-show-hint="false" typeahead-min-length="perfOrgTypeaheadLength" data-ng-change="perfOrgUpdated(item)" typeahead-on-select="perfOrgSelected($item, item)">
                                                    </div>
                                                </td>
                                                <% if (Model.BOEIsMulti)
                                                    { %>
                                                <td class="resource-wbs">
                                                    <select tabindex="{{tabindex + 4}}" class="wbs" data-ng-model="item.WBSID" data-ng-change="setDirty()" name="WBSID">
                                                        <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                                                        <option data-ng-repeat="option in ManageTaskModel.WBSElements" data-ng-value="option.Value">{{option.Text}}</option>
                                                    </select>
                                                </td>
                                                <td class="resource-clin">
                                                    <select tabindex="{{tabindex + 5}}" class="wbs" data-ng-model="item.CLINID" data-ng-change="setDirty()" name="CLINID">
                                                        <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                                                        <option data-ng-repeat="option in ManageTaskModel.CLINElements" data-ng-value="option.Value">{{option.Text}}</option>
                                                    </select>
                                                </td>
                                                <% } %>
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
                                                    <div class="inline-block">Offload (<span class="link" data-ng-click="exportOffload()">View</span>)*</div>
                                                </th>
                                                <% } // end is Offload Workspace%>
                                                <th class="delete last-child">
                                                    <div data-ng-click="deleteAllRecords()" class="delete"></div>
                                                </th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr data-ng-repeat="item in tableData | filter: { Deleted: false } track by item.BOELaborTypeID" pkid="{{item.BOELaborTypeID}}" data-ng-init="outerIndex = $index">
                                                <td data-ng-repeat="customField in LaborCustomFields" class="custom-field" data-ng-init="innerIndex = $index">{{ selectedItem = findLaborCustomField(customField, item); "" }}
                                                <input id="LT{{item.BOELaborTypeID}}CF{{customField.CustomFieldMetaData.CustomFieldID}}" tabindex="{{225 + ((12 + LaborCustomFields.length) * outerIndex) + innerIndex}}" data-ng-if="customField.CustomFieldMetaData.isOpenEnded" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="LaborType-CF{{customField.CustomFieldMetaData.CustomFieldID}}" customfieldvalueid="{{selectedItem.openEndedId}}" selectionid="{{selectedItem.selectedID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="true" class="customField LaborTypeCustomField" maxlength="250" onchange="TaskElementDetailsWidget.setDirty(); validateCustomField(this);" data-ng-keydown="checkIfNewRowNeeded(item)" isrequired="{{customField.CustomFieldMetaData.isRequired}}" isnew="{{item.NewLaborType}}" data-ng-value="selectedItem.openEndedValue" />
                                                    <select id="LT{{item.BOELaborTypeID}}CF{{customField.CustomFieldMetaData.CustomFieldID}}" tabindex="{{225 + ((12 + LaborCustomFields.length) * outerIndex) + innerIndex}}" data-ng-if="!customField.CustomFieldMetaData.isOpenEnded" onchange="TaskElementDetailsWidget.setDirty(); validateCustomField(this);" customfieldid="{{customField.CustomFieldMetaData.CustomFieldID}}" name="TaskElement-CF{{customField.CustomFieldMetaData.CustomFieldID}}" updatedatelong="{{selectedItem.updateDateLong}}" openended="false" isrequired="{{customField.CustomFieldMetaData.isRequired}}"
                                                        class="customField LaborTypeCustomField" selectionid="{{selectedItem.selectedID}}" data-ng-click="checkIfNewRowNeeded(item)" data-ng-keydown="checkIfNewRowNeeded(item)" isnew="{{item.NewLaborType}}">
                                                        <option value=""></option>
                                                        <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                                                        <option data-ng-repeat="option in customField.CustomFieldOptions" data-ng-value="option.CustomFieldOptionID" data-ng-selected="selectedItem.selectedOptionID == option.CustomFieldOptionID">{{option.ID}}-{{option.Description}}</option>
                                                    </select>
                                                </td>
                                                <td class="text start-date" data-ng-class="{inputError: validateStartDate(item, model.TaskElementData.StartDate) === false }">
                                                    <input tabindex="{{225 + LaborCustomFields.length + ((12 + LaborCustomFields.length) * outerIndex) + 1}}" type="text" data-ng-model="item.StartDate" data-ng-blur="startDateUpdated(item)" data-ng-change="checkIfNewRowNeeded(item)" />
                                                </td>
                                                <td class="text end-date" data-ng-class="{inputError: validateEndDate(item, model.TaskElementData.EndDate) === false }">
                                                    <input tabindex="{{225 + LaborCustomFields.length + ((12 + LaborCustomFields.length) * outerIndex) + 2}}" type="text" data-ng-model="item.EndDate" data-ng-blur="endDateUpdated(item)" data-ng-change="checkIfNewRowNeeded(item)" />
                                                </td>
                                                <td data-ng-class="{inputError: (!item.SpreadCurveID || item.SpreadCurveID === '' || item.SpreadCurveID === '-1') && item.NewLaborType === false}">
                                                    <select tabindex="{{225 + LaborCustomFields.length + ((12 + LaborCustomFields.length) * outerIndex) + 3}}" data-ng-model="item.SpreadCurveID" data-ng-change="spreadCurveUpdated(item)" name="SpreadCurveID">
                                                        <!-- keep option tag on one line to avoid insertion of line breaks (br) -->
                                                        <option data-ng-if="item.RateType === ManageTaskModel.RateTypeCost" data-ng-repeat="option in ManageTaskModel.SpreadCurvesCost" data-ng-value="option.Value">{{option.Text}}</option>
                                                        <option data-ng-if="item.RateType !== ManageTaskModel.RateTypeCost" data-ng-repeat="option in ManageTaskModel.SpreadCurvesHours" data-ng-value="option.Value">{{option.Text}}</option>
                                                    </select>
                                                </td>
                                                <td class="text ltpercentspread">
                                                    <div class="ScrollingLock" style="position: static; height: 16px;">
                                                        <div style="position: absolute;">
                                                            <div style="float: left;">
                                                                <input tabindex="{{225 + LaborCustomFields.length + ((12 + LaborCustomFields.length) * outerIndex) + 4}}" type="text" data-ng-click="updatePercentLock(item)" data-ng-class="{'disabled': !item.PercentSpreadLocked}" data-ng-disabled="item.RateType === ManageTaskModel.RateTypeCost || item.SpreadCurveID === ManageTaskModel.SpreadCurvesDiscreteHours" data-ng-model="item.PercentSpread" data-ng-blur="percentSpreadUpdated(item)" data-ng-change="updatePercentLock(item); checkIfNewRowNeeded(item);" name="PercentSpread" class="PercentSpread" />
                                                            </div>
                                                            <div data-ng-if="item.RateType !== ManageTaskModel.RateTypeCost && item.SpreadCurveID !== ManageTaskModel.SpreadCurvesDiscreteHours">
                                                                <img data-ng-if="!item.PercentSpreadLocked" data-ng-click="updatePercentLock(item)" class="LockImage UnLockImage" src="../../../../Resources/css/images/unlock_resource_toggle.png" title="Select lock for either % Spread or Hour Spread for Resources using a curve to maintain that value when the MOQ Equation is adjusted." />
                                                                <img data-ng-if="item.PercentSpreadLocked" class="LockImage" src="../../../../Resources/css/images/lock_resource_toggle.png" title="Select lock for either % Spread or Hour Spread for Resources using a curve to maintain that value when the MOQ Equation is adjusted." />
                                                            </div>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td class="text lthourspread">
                                                    <div class="ScrollingLock" style="position: static; height: 16px;">
                                                        <div style="position: absolute;">
                                                            <div style="float: left;">
                                                                <input tabindex="{{225 + LaborCustomFields.length + ((12 + LaborCustomFields.length) * outerIndex) + 5}}" type="text" data-ng-click="updateHoursLock(item)" data-ng-class="{'disabled': !item.HourSpreadLocked}" data-ng-disabled="item.RateType === ManageTaskModel.RateTypeCost || item.SpreadCurveID === ManageTaskModel.SpreadCurvesDiscreteHours" data-ng-model="item.HourSpread" data-ng-blur="hourSpreadUpdated(item)" data-ng-change="updateHoursLock(item); checkIfNewRowNeeded(item);" class="HourSpread" name="HourSpread">
                                                            </div>
                                                            <div data-ng-if="item.RateType !== ManageTaskModel.RateTypeCost && item.SpreadCurveID !== ManageTaskModel.SpreadCurvesDiscreteHours">
                                                                <img data-ng-if="!item.HourSpreadLocked" data-ng-click="updateHoursLock(item)" class="LockImage UnLockImage" src="../../../../Resources/css/images/unlock_resource_toggle.png" title="Select lock for either % Spread or Hour Spread for Resources using a curve to maintain that value when the MOQ Equation is adjusted." />
                                                                <img data-ng-if="item.HourSpreadLocked" class="LockImage" src="../../../../Resources/css/images/lock_resource_toggle.png" title="Select lock for either % Spread or Hour Spread for Resources using a curve to maintain that value when the MOQ Equation is adjusted." />
                                                            </div>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td class="text ltcost">
                                                    <span class="resourceCurrency">$</span>
                                                    <input tabindex="{{225 + LaborCustomFields.length + ((12 + LaborCustomFields.length) * outerIndex) + 6}}" data-ng-disabled="item.RateType !== ManageTaskModel.RateTypeCost || item.SpreadCurveID === ManageTaskModel.SpreadCurvesDiscreteCost" data-ng-blur="costSpreadUpdated(item)" data-ng-change="checkIfNewRowNeeded(item)" type="text" data-ng-model="item.CostSpread" class="CostSpread" data-ng-disabled="item.RateType !== ManageTaskModel.RateTypeCost" name="CostSpread" />
                                                    <% if (Model.IsOffloadWorkspace)
                                                        { %>
                                                <td id="labor-offload" class="text">
                                                    <select tabindex="{{225 + LaborCustomFields.length + ((12 + LaborCustomFields.length) * outerIndex) + 7}}" data-ng-model="item.CanOffload" data-ng-change="setDirty(); checkIfNewRowNeeded(item);" name="CanOffload">
                                                        <option data-ng-value="{{true}}">True</option>
                                                        <option data-ng-value="{{false}}">False</option>
                                                    </select>
                                                </td>
                                                <% } // end is Offload Workspace%>
                                                <td>
                                                    <input type="hidden" name="Deleted" value="false" />
                                                    <div data-ng-click="deleteLaborType(item)" class="delete DeleteButton" data-ng-disabled="item.NewLaborType" />
                                                </td>
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
                <div class="loader" data-ng-show="isLoading"></div>
                <div data-ng-hide="isLoading">
                    <div id="LaborSpreadGridContent" class="clear labor-spread-grid-content">
                        <gen-validation data-errors="laborSpreadErrors"></gen-validation>
                        <gen-validation data-classtype="'warning-'" data-errors="laborSpreadPasteErrors"></gen-validation>
                        <div id="LaborSpreadGridBlock" data-ng-cloak>
                            <div data-ng-class="{'labor-spread-labels-brc': IsBRCEnabled }" class="labor-spread-labels">
                                <table class="header-rows">
                                    <thead>
                                        <tr>
                                            <th>Resource</th>
                                            <th data-ng-if="IsBRCEnabled">Business Resource Code</th>
                                            <th>Performing Org</th>
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
                                        <tr data-ng-repeat="item in tableData | filter: { Deleted: false, NewLaborType: false } track by item.BOELaborTypeID">
                                            <td title="{{item.ResourceName}}"><span>{{ item.ResourceName ? item.ResourceName : '_'}}</span></td>
                                            <td title="{{item.BusinessResourceCodeName}}" data-ng-if="IsBRCEnabled"><span>{{ item.BusinessResourceCodeName ? item.BusinessResourceCodeName : '_'}}</span></td>
                                            <td class="PerformingOrgName" title="{{item.PerformingOrgName}}"><span>{{item.PerformingOrgName ? item.PerformingOrgName : "_"}}</span></td>
                                        </tr>
                                        <tr id="LaborSpreadHeaderDividerRow" class="subheader">
                                            <td colspan="{{IsBRCEnabled ? 3 : 2}}" style="background-color: #EBEBEB; line-height: 2px; padding: 0px;">&nbsp;</td>
                                        </tr>
                                        <tr>
                                            <td class="subheader" colspan="{{IsBRCEnabled ? 3 : 2}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total <%: Model.HoursLabel %> by Months</td>
                                        </tr>
                                        <tr>
                                            <td class="subheader" colspan="{{IsBRCEnabled ? 3 : 2}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total Discrete Cost by Months</td>
                                        </tr>
                                        <tr>
                                            <td class="subheader" colspan="{{IsBRCEnabled ? 2 : 1}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total <%: Model.HoursLabel %></td>
                                            <td class="hours-total">{{totalSpreadHours}}</td>
                                        </tr>
                                        <tr>
                                            <td class="subheader" colspan="{{IsBRCEnabled ? 2 : 1}}" style="background-color: #EBEBEB; padding: 2px; white-space: nowrap;">Total Discrete Cost</td>
                                            <td class="cost-total"><span class="labor-spread-currency">$</span>{{totalSpreadCost}}</td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div data-ng-if="(tableData | filter: { Deleted: false, NewLaborType: false }).length > 0" data-ng-class="{'labor-spread-scroll-brc': IsBRCEnabled}" class="labor-spread-scroll">
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
                                        <tr data-ng-repeat="item in tableData | filter: { Deleted: false, NewLaborType: false } track by item.BOELaborTypeID">
                                            <td data-ng-repeat="dt in model.SpreadDatesFull" date="{{dt}}" data-ng-class="{inputError: item.SpreadDataInvalid[$index]}">{{ isInRange = isMonthInRange(dt.toDate(), item.StartDate.toDate(), item.EndDate.toDate()); "" }}
                                            <span class="currency" data-ng-show="isInRange">{{getDollarSign(item)}}</span>
                                                <input class="LSDateinput" data-ng-show="isInRange" type="text" data-ng-model="item.SpreadData[$index]"
                                                    data-ng-blur="spreadInputChanged(dt, $index, item)" data-ng-disabled="getSpreadDisabled(item)" onclick="this.select()"
                                                    data-ng-paste="spreadsPaste($event);" rowid="{{item.BOELaborTypeID}}" cellid="{{$index}}" />
                                                <input class="LSDateinput" data-ng-hide="isInRange" type="text" disabled="disabled" />
                                            </td>
                                        </tr>
                                        <tr id="LaborSpreadDataDividerRow">
                                            <td colspan="{{model.SpreadDatesFull.length}}" class="subheader" style="background-color: #EBEBEB; line-height: 2px; padding: 0px;">&nbsp;</td>
                                        </tr>
                                        <tr class="hours-total">
                                            <td data-ng-repeat="dt in model.SpreadDatesFull" date="{{dt}}">{{getHoursTotals(dt)}}</td>
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

    </div>

    <% if (Model.EnableSkillMix)
        { %>
    <div id="SkillMixPlaceholder">
        <div class="skill-mix-tables module collapsed" id="SkillMixRationaleContainer">
            <div class="module-header-data">
                Skill Mix Rationale
            </div>
            <div class="module-content-data expanded-content">
                <div class="form-element">
                    <!-- Skill Mix Table -->
                    <div class="form-label">
                        <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                            {  %>
                                    Legacy Skill Mix table
                        <% } %>
                        <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                            {  %>
                                    Current Skill Mix Table
                        <%  } %>
                    </div>
                    <div class="SkillMixTable skillMixTable">
                        <table name="currentSkillMix" class="grid editable" style="width: 100%;">
                            <thead>
                                <tr>
                                    <th style="width: 95px;">Resource ID</th>
                                    <th style="width: 130px;">Current Resource ID</th>
                                    <th style="width: 100px;">Historical Hours</th>
                                    <th style="width: 90px;">Labor Skill Mix</th>
                                    <th style="width: 55px">Included</th>
                                    <th style="width: 90px;">BOE Skill Mix</th>
                                    <th style="width: 95px;">Proposed Hours</th>
                                    <th>Rationale**</th>
                                    <th style="width: 38px;"></th>
                                </tr>
                            </thead>
                            <tbody>
                                <!-- Display the data for each of the Skill Mix Table rows. -->
                                <tr ng-repeat="row in skillMixRationale.data.SkillMixRows">
                                    <td>{{row.ResourceOld}}</td>
                                    <td>
                                        <!-- Select for Current Resource ID will change the Included column and IsUserInput backend value. -->
                                        <select
                                            data-ng-options="option for option in skillMixRationaleLaborTypeSelections track by option"
                                            ng-model="row.ResourceNew"
                                            ng-change="refreshSkillMixTables()">
                                        </select>
                                    </td>
                                    <td style="text-align: right">{{row.HistoricalHours | number:2}}</td>
                                    <td style="text-align: right">{{row.LaborSkillMix | number:1}}%</td>
                                    <td>
                                        <!-- Read only for Included when loaded in as True -->
                                        <span ng-if="!row.metadata.ManuallySetIncluded && row.Included">
                                            <span ng-switch="row.Included">
                                                <span ng-switch-when="true">Yes</span>
                                                <span ng-switch-when="false">No</span>
                                            </span>
                                        </span>
                                        <!-- Dropdown Selection for Included when loaded in as False. -->
                                        <span ng-if="row.metadata.ManuallySetIncluded || !row.Included">
                                            <select ng-model="row.Included"
                                                ng-change="setSkillMixIsUserInput($index, row.Included)"
                                                ng-options="option === true ? 'Yes' : 'No' for option in [true, false]">
                                            </select>
                                        </span>
                                    </td>
                                    <td style="text-align: right">{{row.BOESkillMix | number:1}}%</td>
                                    <td style="text-align: right">{{row.ProposedHours}}</td>
                                    <td>
                                        <div id="skill-mix-table-rationale">
                                            <textarea id="skill-mix-rationale" data-ng-model="row.Rationale" style="width: 100%; height: 14px; max-height: 42px; overflow-y: auto; resize: vertical;" maxlength="255"></textarea>
                                        </div>
                                    </td>
                                    <!-- Action Buttons for adding (+) and deleting (-) rows. -->
                                    <td>
                                        <button ng-click="addSkillMixRow($index)" style="width: 18px; height: 18px; font-size: 12px; padding: 0; margin: 0; display: inline-block; vertical-align: top;">+</button>
                                        <button ng-click="deleteSkillMixRow($index)" ng-show="checkMultipleSkillMixRows(row.ResourceOld)" style="width: 18px; height: 18px; font-size: 12px; padding: 0; margin: 0; display: inline-block; vertical-align: top;">-</button>
                                    </td>
                                </tr>
                                <!-- Display the Skill Mix Totals row. -->
                                <tr>
                                    <td>Totals</td>
                                    <td></td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.HistoricalHours | number:2}}</td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.LaborSkillMix | number:1}}%</td>
                                    <td></td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.BoeSkillMix | number:1}}%</td>
                                    <td style="text-align: right">{{skillMixRationale.data.SkillMixTotals.ProposedHours}}</td>
                                    <td></td>
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
                        <table name="currentSkillMix" class="grid editable" style="width: 100%;">
                            <thead>
                                <tr>
                                    <th style="width: 95px;">Resource ID</th>
                                    <th style="width: 130px;">Business Resource Code</th>
                                    <th style="width: 100px;">Historical Hours</th>
                                    <th style="width: 90px;">Labor Skill Mix</th>
                                    <th style="width: 55px">Included</th>
                                    <th style="width: 90px;">BOE Skill Mix</th>
                                    <th style="width: 95px;">Proposed Hours</th>
                                    <th>Rationale**</th>
                                    <th style="width: 38px;"></th>
                                </tr>
                            </thead>
                            <tbody>
                                <!-- Display the data for each of the Common Disclosure Skill Mix Table rows. -->
                                <tr ng-repeat="row in skillMixRationale.data.CommonDisclosureRows">
                                    <td>{{row.ResourceID}}</td>
                                    <td>
                                        <!-- Select for Business Resouce ID if the Resource ID has a value. -->
                                        <select
                                            ng-if="!row.ResourceID.length"
                                            data-ng-options="option for option in commonDisclosureLaborTypeSelections track by option"
                                            ng-model="row.BusinessResourceID"
                                            ng-change="refreshSkillMixTables()">
                                        </select>
                                        <span ng-if="row.ResourceID.length">{{row.BusinessResourceID}}</span>
                                    </td>
                                    <td style="text-align: right">{{row.HistoricalHours | number:2}}</td>
                                    <td style="text-align: right">{{row.LaborSkillMix | number:1}}%</td>
                                    <td>{{row.Included | yesNo}}</td>
                                    <td style="text-align: right">{{row.BOESkillMix | number:1}}%</td>
                                    <td style="text-align: right">{{row.ProposedHours}}</td>
                                    <td>
                                        <div id="cd-table-rationale">
                                            <textarea id="cd-rationale" data-ng-model="row.Rationale" style="width: 100%; height: 14px; max-height: 45px; overflow-y: auto; resize: vertical;" maxlength="255"></textarea>
                                        </div>
                                    </td>
                                    <!-- Action Buttons for adding (+) and deleting (-) rows. -->
                                    <td>
                                        <button ng-click="addCommonDisclosureRow($index)" style="width: 18px; height: 18px; font-size: 12px; padding: 0; margin: 0; display: inline-block; vertical-align: top;">+</button>
                                        <button ng-click="deleteCommonDisclosureRow($index)" ng-show="checkMultipleCommonDisclosureRows(row.BusinessResourceID)" style="width: 18px; height: 18px; font-size: 12px; padding: 0; margin: 0; display: inline-block; vertical-align: top;">-</button>
                                    </td>
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
                                    <td></td>
                                    <td></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <% } %>

    <div class="buttons-left" data-ng-hide="isLoading"></div>
    <div class="buttons" data-ng-hide="isLoading">
        <div class="required-note">
            <div>* required for saving as draft.</div>
            <div>** required for validating and submitting for approval</div>
        </div>
        <div class="oci-note"><b>Note: </b><span id="Task-OCINote"></span></div>
        <div class="disable-save-text" data-ng-show="ManageTaskModel.DisableSave">{{ManageTaskModel.DisableSaveText}}</div>

        <button data-ng-if="taskElementId" data-ng-hide="isSaving || ManageTaskModel.DisableSave" data-ng-click="saveAndContinue()" id="Save-BOEUpdatesAndContinue" class="ies-action stateful_button" name="save-button" type="button">Save & Continue</button>
        <button id="Save-BOEUpdatesAndClose" data-ng-hide="isSaving || ManageTaskModel.DisableSave" data-ng-click="saveAndClose()" class="ies-action stateful_button" name="save-button" type="button">Save & Close</button>
        <button id="Save-BOEUpdatesAndAddAnother" data-ng-hide="isSaving || ManageTaskModel.DisableSave" data-ng-click="saveAndAddAnother()" class="ies-action stateful_button" name="save-button" type="button">Save & Add Another</button>
        <div id="Loader-BOEUpdates" class="loader" data-ng-show="isSaving"></div>
        <button disabled="disabled" id="Save-Disabled" class="disabled ies-action" type="button" data-ng-show="ManageTaskModel.DisableSave">Save</button>
        <button id="Cancel-BOEUpdates" class="ies" name="cancel-button" type="button">Cancel</button>
        <button id="Next-Task" data-ng-disabled="isNextTaskDisabled()" data-ng-click="navigateToNext()" class="ies" name="next-task-button" type="button">Next Task</button>
        <button id="Previous-Task" data-ng-disabled="isPreviousTaskDisabled()" data-ng-click="navigateToPrevious()" class="ies" name="previous-task-button" type="button">Previous Task</button>
    </div>
    <div class="buttons-right" data-ng-hide="isLoading"></div>

    <div id="MOQEquation-SearchEstimatingCatalogDialogCommon" style="display: none; height: 400px; overflow: visible;">
        <div>
            <br />
            <br />
            <form id="SearchEstimatingCatalogDialogFormCommon">
                <div style="float: left; display: inline-block; margin-top: 3px;">Search For:</div>
                <div id="MOQEquationSearch-Help" class="help-icon" style="margin-left: 0px;"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="MOQEquationSearch-HelpDialog" class="help-dialog" style="width: 275px;">
                    <div class="help-dialog-text">
                        Search for validated, not validated or all historical metrics. Validated metrics
                    have backup data to support the metric and it is auditable.<br />
                        <br />
                        The following fields for a historical metric will be searched for the word or phrase
                    entered:<br />
                        <ul>
                            <li>ID</li>
                            <li>Metric Title</li>
                            <li>Discipline</li>
                            <li>Source Program Long Name</li>
                            <li>Source Program Short Name</li>
                            <li>Major Component Characteristics</li>
                            <li>Program Milestone</li>
                            <li>Usage</li>
                            <li>Labor Source</li>
                            <li>MOQ Equation</li>
                            <li>MOQ Type</li>
                            <li>MOQ Text</li>
                            <li>Owner</li>
                            <li>Mined By</li>
                            <li>Data Source Location</li>
                        </ul>
                    </div>
                </div>
                <div class="form-element">
                    <select id="SearchEstimatingCatalog-Status" name="SearchEstimatingCatalog-Status">
                        <option value="3">All Historical Metrics</option>
                        <option value="2">Validated Metrics</option>
                        <option value="1">Not Validated Metrics</option>
                    </select>
                </div>
                <div class="form-element">
                    <input id="MetricsSeachText" type="text" maxlength="100" style="width: 300px;" value="" />
                </div>
                <div class="form-row">
                    <div class="form-element">
                        <div class="buttons" style="padding-left: 0px; border-left-width: 0px; margin-left: 87px; margin-top: 8px;">
                            <button id="SearchEstimatingCatalog-SearchButtonCommon" class="ies-action" type="button">Search</button>
                            <div id="SearchEstimatingCatalog-LoaderCommon" class="loader display-none"></div>
                        </div>
                    </div>
                </div>
            </form>

        </div>
    </div>
    <div id="MOQEquation-SearchEstimatingCatalogDialogMST">
        <div>
            <form id="SearchEstimatingCatalogDialogFormMST">
                <div class="form-row">
                    <div class="form-element"><a id="MeasureSearch-HelpLink" class="info-link" title="Learn more about searching Historical Measures" href="<%:Model.MetricsSearchDialogParameters.MSTSearchHelpLink%>" target="_blank"></a>Search for Measures to include in the current Task Element using one or more search fields below.</div>
                </div>

                <div class="form-row">
                    <div class="form-label">
                        Filter By:
                    </div>
                </div>
                <div class="metric-filter">
                    <div class="form-row">
                        <div class="form-label">
                            Program/Project Name:
                        </div>
                        <div class="form-element">
                            <%: Html.DropDownListFor(m => Model.MetricsSearchDialogParameters.ProgramNameFilter.FirstOrDefault().Text , (IEnumerable<SelectListItem>)Model.MetricsSearchDialogParameters.ProgramNameFilter, new{@id="FilterProgram"}) %>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Measure Name:
                        </div>
                        <div class="form-element">
                            <%: Html.DropDownListFor(m => Model.MetricsSearchDialogParameters.MeasureNameFilter.FirstOrDefault().Text , (IEnumerable<SelectListItem>)Model.MetricsSearchDialogParameters.MeasureNameFilter, new{@id="FilterName"})%>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Measure Function:
                        </div>
                        <div class="form-element">
                            <%: Html.DropDownListFor(m => Model.MetricsSearchDialogParameters.MeasureFunctionFilter.FirstOrDefault().Text , (IEnumerable<SelectListItem>)Model.MetricsSearchDialogParameters.MeasureFunctionFilter, new{@id="FilterFunction"})%>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Measure Qualifier:
                        </div>
                        <div class="form-element">
                            <%: Html.DropDownListFor(m => Model.MetricsSearchDialogParameters.MeasureQualifierFilter.FirstOrDefault().Text , (IEnumerable<SelectListItem>)Model.MetricsSearchDialogParameters.MeasureQualifierFilter, new{@id="FilterQualifier"})%>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Data Source:
                        </div>
                        <div class="form-element">
                            <%: Html.DropDownListFor(m => Model.MetricsSearchDialogParameters.DataSourceFilter.FirstOrDefault().Text , (IEnumerable<SelectListItem>)Model.MetricsSearchDialogParameters.DataSourceFilter, new{@id="FilterSource"})%>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Text:    
                    <div id="MeasureSearch-Help" class="help-icon" style="margin-left: 0px;"></div>
                            <!-- This comment is needed for the jquery animation to work in IE8... -->
                            <div id="MeasureSearch-HelpDialog" class="help-dialog" style="width: 250px;">
                                <div class="help-dialog-text">
                                    Search for a Measurement using a word or phrase. The following fields will be searched:<br />
                                    <br />
                                    <ul>
                                        <li>Comment</li>
                                        <li>Measure Description</li>
                                        <li>Measure Qualifier</li>
                                        <li>Program Description</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <div class="form-element">
                            <input id="SearchForTextMST" type="text" maxlength="200" value="" />

                        </div>
                    </div>
                </div>
                <div class="form-row" style="text-align: center; margin-top: 8px; display: block;">
                    <div class="form-element">
                        <div class="buttons" style="width: 500px;">
                            <button id="SearchEstimatingCatalog-SearchButtonMST" class="ies-action" type="button">Search</button>
                            <div id="SearchEstimatingCatalog-LoaderMST" class="loader display-none"></div>
                            <button id="SearchEstimatingCatalog-CancelButtonMST" class="ies" name="cancel-button" type="button">Cancel</button>
                        </div>
                    </div>
                </div>
            </form>

        </div>
    </div>

    <div id="MetricSearchResultsContainter" style="display: none; width: auto;">
        <div id="MetricSearchResults" class="display-none;min-width:845px;max-width:1078px;width:auto;"></div>
        <div id="MetricDetails" class="display-none;width:1078px;"></div>
    </div>

    <div id="importLaborTypesDialog" class="import-labor-types dialog form" style="display: none;">
        <div id="ImportInstructions">
            <span>To import Resource Types, follow the steps below.</span>
            <div class="step one">
                <div class="title">Step 1: Select what to import</div>
                <div>
                    <input type="radio" class="radio" name="ImportType" value="New" id="ImportType-New" />
                    <label for="ImportType-New">Import only <i>new</i> Resource Types.</label>
                </div>
                <div>
                    <input type="radio" class="radio" name="ImportType" value="Existing" id="ImportType-Existing" />
                    <label for="ImportType-Existing">Import new Resource Types <i>and</i> import updates to existing Resource Types.</label>
                </div>
            </div>
            <div class="step two display-none" path="New">
                <div class="title">Step 2: Download the Resource Type template file</div>
                <div>
                    Start by downloading the Resource Type template file. This file has the correct column headings genBOE needs to import the Resource Types.<br />
                    <br />
                    If you have Resource Types in a different file, in order to use it, it must have the same exact headings as the Resource Type template file.  Start Date and End Date must be set to Custom m/yyyy. The file type must be an Excel .xlsx file.
                </div>
                <div><a id="LTTemplateExport" onclick="angular.element(document.getElementById('TaskElementsComposite')).scope().export(true)">Download Resource Type template file</a></div>
            </div>
            <div class="step three display-none" path="New">
                <div class="title">Step 3: Enter Resource Types into the file</div>
                <div>Enter Resource Types into the file. If you have a Resource Type in a different file, you can copy and paste Resource Types from it into the genBOE Resource Type template. Make sure the Resource Type data you copy matches the column headings provided in the template.</div>
                <div>
                    Resource, Performing Orgs, Start Date, End Date, and Spread Curve are required fields.
                    <br />
                    Start and End dates must be in the following format:  m/yyyy or mm/yyyy.
                    <br />
                    Start Date must be on or after Task Start Date.
                    <br />
                    End Date must be on or before Task End Date.
                </div>
                <div class="important">
                    IMPORTANT: Do not change the column headings or options. Do not enter a value for genBOE Resource Type ID in column A. genBOE Resource Type IDs are unique identifiers for Resource Types. One will be automatically generated for each new Resource Type once the import is complete. Column A  and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work. 
                </div>
            </div>
            <div class="step two display-none" path="Existing">
                <div class="title">Step 2: Export the existing Resource Type file</div>
                <div>Start by exporting the existing Resource Types. This file has the correct column headings and Resource Type IDs genBOE needs to import new Resource Types and updates.</div>
                <div class="important">IMPORTANT: Unsaved Resource Types will not be included in this export.</div>
                <div><a onclick="angular.element(document.getElementById('TaskElementsComposite')).scope().tryExport()">Export existing Resource Types</a></div>
            </div>
            <div class="step three display-none" path="Existing">
                <div class="title">Step 3: Enter/Update Resource Types in the file</div>
                <div>Enter new Resource Types into the file or update existing Resource Types. If you have a Resource Type in a different file, you can copy and paste Resource Types from it into the genBOE Resource Type file. Make sure the Resource Type data you copy matches the column headings provided in the genBOE Resource Type file.</div>
                <div>
                    Resource, Performing Orgs, Start Date, End Date, and Spread Curve are required fields.
                    <br />
                    Start and End dates must be in the following format:  m/yyyy or mm/yyyy.
                    <br />
                    Start Date must be on or after Task Start Date.
                    <br />
                    End Date must be on or before Task End Date.
                </div>
                <div class="important">IMPORTANT: Do not change the column headings or options. Do not enter a value for genBOE Resource Type ID in column A. genBOE Resource Type IDs are unique identifiers for Resource Types. One will be automatically generated for each new Resource Type once the import is complete. Column A and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.</div>
                <div class="important">If you delete a Resource Type from the file, the Resource Type will not be removed in genBOE. To delete a Resource Type, it must be deleted directly in genBOE to properly remove all associations.</div>
            </div>
            <div class="step four display-none" path="New Existing">
                <% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new { controller = WebConstants.CONTROLLER_BOE_LABOR, action = WebConstants.ACTION_PREVIEW_IMPORT_LABOR_TYPE_AND_SPREAD, workspace = SiteMasterUtilities.GetCurrentWorkspace(), boeID = ViewData["BOEID"], taskElementID = ViewData["TASKID"] }, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportLaborTypeDialog-Project-Form", target = "ImportLaborTypeDialog-UploadTarget" }); %>
                <div class="title">Step 4: Import the updated Resource Type template file</div>
                <div>Choose a file to import. The file you import must be an Excel file that ends in .xlsx or .xlsm.</div>
                <div>
                    <input type="hidden" id="ImportLaborTypeDialog-Project-DocumentDomain" name="documentDomain" />
                    <input type="hidden" name="laborTypeImportType" value="new" />
                    <input type="file" name="importFile" size="60" />
                </div>
                <div class="buttons">
                    <button id="ManageLaborType-ImportButton" class="ies-action" name="import-button" type="button">Import</button>
                    <div id="ManageLaborType-ImportLoader" class="loader display-none"></div>
                    <button class="ies" name="cancel-button" type="button">Cancel</button>
                </div>
                <% Html.EndForm(); %>
            </div>
        </div>
    </div>
    <div id="LaborTypeImportErrors" style="display: none;" title="Errors During Import of Resource Types">
        Errors occurred during import of the Resource Types. No changes were made to the Workspace.
        <br />
        <br />
        <div class="errorSpace">
        </div>
        <br />
        <button class="ies" name="ok-button" type="button">OK</button>
    </div>
    <div id="ImportLaborTypeResults" class="import-verification dialog form" style="display: none;">
        <span>The import file will make the following updates. To continue with the import,
            click <i>Complete Import</i>, otherwise click <i>Back</i> to import a different
            file or close this dialog window to not import a file.</span>
        <br />
        <br />
        <div class="import-results">

            <div class="import-result-type display-none" id="ImportResult-Added">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will be added:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-Updated">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will be updated:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-NotAdjusted">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not have their % Spread or <%: ViewData["HoursLabel"]%>/Cost Spreads added/updated unless Spread Curve is changed from Discrete to a Curve:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-RateTypeSpreadTypeAgreement">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not be added/updated because the Spread Curve does not match the rate type for the resource:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-ResourceTypeIDMissingOrInvalid">
                <div class="title">
                    Resource ID(s) are invalid for this task.  Please verify the file selected is correct and has been exported from this task.
                </div>
            </div>
            <div class="import-result-type display-none" id="ImportResult-Dates">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not have their Start Date and End Date updated because these dates are out of task date range:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-MissingData">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not be added/uploaded because a Resource, Performing Org, Start Date, End Date, Spread Curve, WBS/CLIN, Offload, or required Custom Field is missing or invalid:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-MissingData-BRCEnabled">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not be added/uploaded because a Resource, Business Resource Code, Performing Org, Start Date, End Date, Spread Curve, WBS/CLIN, Offload, or required Custom Field is missing or invalid:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-HoursSpreadInvalid">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not be added/uploaded because the Hour Spread value is invalid or exceeds the configured workspace resource decimal precision:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-CostInvalid">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not be added/uploaded because the Cost value is invalid or exceeds the configured workspace cost decimal precision:
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-SpreadMonthColumnInvalid">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not be added/updated because one or more spread month columns are invalid.<br />
                    Please verify the import file spread month column headers are within the task start/end date range.              
                </div>
            </div>
            <div class="import-result-type display-none" id="ImportResult-SpreadMonthValueOutsideDateRange">
                <div class="title">
                    <span class="resultCount"></span>
                    Resource Types will not be added/updated because one or more spread month values are outside the spread date range.<br />
                    Please check the import file and clear any spread month values that are outside the resource spread date range.              
                </div>
            </div>
            <div class="import-result-type display-none" id="ImportResult-RateTypesDoNotMatch">
                <div class="title">
                    <span class="resultCount"></span>
                    Rate Types between Selected Resource and Business Resource Code do not match.
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-ResourceMissing">
                <div class="title">
                    <span class="resultCount"></span>
                    Element row(s) needs to have Resource Selected because End Date is before 1LMX Cutoff Date.
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-BusinessResourceCodeMissing">
                <div class="title">
                    <span class="resultCount"></span>
                    Element row(s) needs Business Resource Code Selected because start date is greater than or equal to 1LMX Cutoff Date.
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-MissingStartEndDate">
                <div class="title">
                    <span class="resultCount"></span>
                    Element row(s) missing Start and/or End Date.
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-MissingResourceOrBRC">
                <div class="title">
                    <span class="resultCount"></span>
                    Element row(s) missing Resource and/or Business Resource Code because Start Date is before 1LMX Cutoff Date and End Date is after 1LMX Cutoff Date.
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="import-result-type display-none" id="ImportResult-MissingBusinessResourceCode">
                <div class="title">
                    <span class="resultCount"></span>
                    Element row(s) missing Business Resource Code because Start Date is before 1LMX Cutoff Date and End Date is after 1LMX Cutoff Date.
                </div>
                <ul class="resultsList"></ul>
            </div>
            <div class="title import-result-type display-none" id="ImportResult-NoChanges">No Changes Detected</div>

            <div id="ImportResults" class="container">
                <div class="title">Resource Spreads will be added/updated:</div>
                <ul result="<%: (int)LaborSpreadImportResult.UpdateSpread %>"></ul>

                <div class="title">Resource Spreads will be updated:</div>
                <ul result="<%: (int)LaborSpreadImportResult.StartDateChanged %>"></ul>

                <div class="title">Resource Spreads will be updated:</div>
                <ul result="<%: (int)LaborSpreadImportResult.EndDateChanged %>"></ul>

                <div class="title">Resource Spreads will not be added/updated.  Monthly Hour Spread and Resource Spread Total must be between -9,999,999,999 and 9,999,999,999:</div>
                <ul result="<%: (int)LaborSpreadImportResult.ResourceHourValueTooLarge %>"></ul>

                <div class="title">Resource Spreads will not be added/updated.  Monthly Cost Spread and Resource Spread Total must be between -9,999,999,999.99 and 9,999,999,999.99 and contain only 2 decimal places:</div>
                <ul result="<%: (int)LaborSpreadImportResult.ResourceCostValueTooLarge %>"></ul>

                <div class="title">Resource Spreads will not be added/updated.  Monthly Cost Spread decimal precision must be within the Cost Decimal precision defined for the workspace.</div>
                <ul result="<%: (int)LaborSpreadImportResult.CostDecimalPrecisionViolation %>"></ul>

                <div class="title">Resource Spreads will not be updated because their values are invalid:</div>
                <ul result="<%: (int)LaborSpreadImportResult.InvalidSpreadValue %>"></ul>

                <div class="title">Resource Spreads will not be added because they are outside of the Resource Type's Date Range:</div>
                <ul result="<%: (int)LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange %>"></ul>

                <div class="title">Some rows have no spread date defined:</div>
                <ul result="<%: (int)LaborSpreadImportResult.NoSpreadDate %>"></ul>

                <div class="title">Some rows have spread dates defined in invalid formats:</div>
                <ul result="<%: (int)LaborSpreadImportResult.InvalidSpreadDateFormat %>"></ul>

                <div class="title">Resource Spreads will not be updated because there are missing resources:</div>
                <ul result="<%: (int)LaborSpreadImportResult.MissingResources %>"></ul>

                <div class="title">Resource Spreads will not be updated because their start dates are missing:</div>
                <ul result="<%: (int)LaborSpreadImportResult.MissingStartDate %>"></ul>

                <div class="title">Resource Spreads will not be updated because their end dates are missing:</div>
                <ul result="<%: (int)LaborSpreadImportResult.MissingEndDate %>"></ul>
            </div>
        </div>
        <br />
        <div class="buttons">
            <button id="ManageLaborType-Back" class="ies" name="back-button" type="button">Back</button>
            <button id="ManageLaborType-CompleteImportButton" class="ies-action" name="complete-import-button" type="button">Complete import</button>
            <div id="ManageLaborType-CompleteImportLoader" class="loader display-none"></div>
        </div>
    </div>
    <div id="ReOrderLaborTypesDialog" class="reorder-labor-types-dialog" style="display: none;">
        <div class="container">
            <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ReOrderLaborTypesForm" }))
                { %>
            <ul class="validation-box"></ul>
            <div class="form-row">
                <div class="form-label"></div>
                <div class="form-element">Sort Resource Types using the move buttons.  Select Save when finished.  Multiple item selection is supported via Shift key for grouped items and Ctrl key for separated items.  The defined Resource Type order will be maintained when exporting the BOEs to MS Word.</div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    <%: Html.ListBox("LaborTypesList", (IEnumerable<SelectListItem>)ViewData["Order_Of_ResourceTypes"], new { @class = "select-list" })%>
                    <div class="buttons inline-block centered">
                        <button id="LaborsMoveItemsUp" style="margin-left: 7px;" class="ies move-button" type="button">Move Up</button>
                        <br />
                        <br />
                        <button id="LaborsMoveItemsDown" style="margin-left: 7px;" class="ies move-button" type="button">Move Down</button>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-element">Note: Must not contain any OCI, classified, export controlled or third party proprietary information.</div>
            </div>
            <div class="buttons">
                <button id="ReOrderLaborTypesDialog-Save" class="ies-action disabled" onclick="TaskElementDetailsWidget.SaveReOrderLaborTypes()" name="save-button" type="button">Save</button>
                <div id="ReOrderLaborTypesDialog-Loader" class="loader display-none"></div>
                <button id="ReOrderLaborTypesDialog-Cancel" class="ies" onclick="TaskElementDetailsWidget.CancelReOrderLaborTypes()" name="cancel-button" type="button">Cancel</button>
            </div>
            <% } %>
        </div>
    </div>
    <div id="DuplicateLaborTypesDialog" class="duplicate-labor-types-dialog" style="display: none;">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "DuplicateLaborTypesForm" }))
            { %>
        <ul id="DuplicateResourceTypeValidationBox" class="validation-box"></ul>
        <div class="container">
            <div class="form-row">
                <div class="form-label"></div>
                <div class="form-element">Enter a number between 1 and 9 in # Dups to duplicate a Resource Type.  The specified number of duplicate Resource Types will be created within the current Task Element.</div>
            </div>

            <div class="form-row duplicate-task-list">
                <table id="DuplicateLaborTypesGrid" class="sortable grid readonly" style="width: 100%;">
                    <colgroup>
                        <col width="6%" />
                        <col width="20%" />
                        <col data-ng-show="IsBRCEnabled" width="20%" />
                        <col width="14%" />
                        <col width="8%" />
                        <col width="8%" />
                        <col width="12%" />
                        <col width="12%" />
                    </colgroup>
                    <thead id="DuplicateLaborTypesGridHeader">
                        <tr>
                            <th># Dups</th>
                            <th class="sort resource">Resource</th>
                            <th data-ng-show="IsBRCEnabled" class="sort businessResourceCode">Business Resource Code</th>
                            <th class="sort perfOrg">Performing Org</th>
                            <th class="sort startDate">Start Date</th>
                            <th class="sort endDate">End Date</th>
                            <th class="sort spreadCurve">Spread Curve</th>
                            <th class="sort spreadValue">SpreadValue</th>
                        </tr>
                    </thead>
                    <tbody id="DuplicateLaborTypesGridBody">
                        <tr data-ng-repeat="item in tableData | filter: { Deleted: false, NewLaborType: false } track by item.BOELaborTypeID">
                            <td>
                                <input class="duplicate-count" id="DuplicateCount" type="text" maxlength="1" data-ng-model="item.NumberOfDuplicates" data-ng-keypress="updateDuplicates(event)" />
                            </td>
                            <td>
                                <span title="{{item.ResourceDescription}}">{{item.ResourceDescription}}</span>
                            </td>
                            <td data-ng-show="IsBRCEnabled">
                                <span title="{{item.BusinessResourceCodeDescription}}">{{item.BusinessResourceCodeDescription}}</span>
                            </td>
                            <td>
                                <span>{{item.PerformingOrgName}}</span>
                            </td>
                            <td>
                                <span>{{item.StartDate}}</span>
                            </td>
                            <td>
                                <span>{{item.EndDate}}</span>
                            </td>
                            <td data-ng-if="item.RateType === ManageTaskModel.RateTypeCost" data-ng-repeat="option in ManageTaskModel.SpreadCurvesCost | filter: {Value:item.SpreadCurveID} : true">
                                <span>{{option.Text}}</span>
                            </td>
                            <td data-ng-if="item.RateType === ManageTaskModel.RateTypeCost">
                                <span>${{item.CostSpread}}</span>
                            </td>
                            <td data-ng-if="item.RateType === ManageTaskModel.RateTypeHours" data-ng-repeat="option in ManageTaskModel.SpreadCurvesHours | filter: {Value:item.SpreadCurveID} : true">
                                <span>{{option.Text}}</span>
                            </td>
                            <td data-ng-if="item.RateType === ManageTaskModel.RateTypeHours">
                                <span>{{item.HourSpread}}</span>
                            </td>
                        </tr>
                        <tr data-ng-show="tableData.length <= 1">
                            <td colspan="7">There are no Resource Types to duplicate.</td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="form-row">
                <div class="form-element">Note: Must not contain any OCI, classified, export controlled or third party proprietary information.</div>
            </div>
            <div class="buttons">
                <button id="DuplicateLaborTypesDialog-Save" class="ies-action disabled" data-ng-click="saveDuplicateResourceTypes()" name="save-button" type="button">Save</button>
                <div id="DuplicateLaborTypesDialog-Loader" class="loader display-none"></div>
                <button id="DuplicateLaborTypesDialog-Cancel" class="ies" data-ng-click="clearDuplicates()" onclick="TaskElementDetailsWidget.CancelDuplicateLaborTypes()" name="cancel-button" type="button">Cancel</button>
            </div>
        </div>
        <% } %>
    </div>
</div>
