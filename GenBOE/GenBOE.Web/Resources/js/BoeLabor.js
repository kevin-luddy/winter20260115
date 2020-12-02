/*
This javascript file is called from BOELabor ascx files
*/

function InitializeImportLaborTypeWidget(boeId, workspace, completeImportUrl, spreadTypeHours, spreadTypeCost) {
    var ImportLaborType = new Widget("import_LaborType", false);
    ImportLaborType.ImportErrorDialog = {};
    ImportLaborType.ImportLaborTypesDialog = {};
    ImportLaborType.ImportLaborTypesResultsDialog = {};

    ImportLaborType.CurrentWorkspace = workspace;
    ImportLaborType.BoeID = boeId;

    ImportLaborType.Import = function () {
        $("#ManageLaborType-ImportButton").addClass('display-none');
        $("#ManageLaborType-ImportLoader").removeClass('display-none');

        // This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
        $('#ImportLaborTypeDialog-Project-DocumentDomain').val(document.domain);        
        // Remove the old hidden iFrame, if it exists
        $('#ImportLaborTypeDialog-UploadTarget').remove();
        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportLaborTypeDialog-Project-Form').append('<iframe id="ImportLaborTypeDialog-UploadTarget" name="ImportLaborTypeDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportLaborTypeDialog-UploadTarget').load(ImportLaborType.UploadComplete);

        $("#ImportLaborTypeDialog-Project-Form").submit();
    };

    ImportLaborType.createPreviewOutput = function (lt, usingTemplateBoe = 'False') {

        var toReturn =
            '<li>' +
                'Resources:' + lt.Resource +
                ' Performing Org:' + lt.PerformingOrg +
                ' Date Range:' + lt.StartDateFormatted +
                '-' + lt.EndDateFormatted +
            ' Curve:' + lt.Curve;

        if (usingTemplateBoe === 'True') {
            toReturn += ' MOQ Type:' + lt.MoqTypeText;
        }

        if (lt.SpreadType === spreadTypeHours) {
            if (lt.ValueSpread !== undefined) {
                toReturn += ' Hours Spread:' + lt.ValueSpread;
            }

            if (lt.PercentSpread !== undefined) {
                toReturn += ' Percent Spread:' + lt.PercentSpread;
            }
        }
        else if (lt.SpreadType === spreadTypeCost) {
            if (lt.ValueSpread !== undefined) {
                toReturn += ' Cost:$' + lt.ValueSpread;
            }
        }
                
        toReturn += '</li>';

        return toReturn;
    };

    
    ImportLaborType.showImportDialog = function () {

        $('#importLaborTypesDialog input[name=ImportType]').each(function () { $(this).prop("checked", false).prop("disabled", false); });
        $('#importLaborTypesDialog .step[path]').addClass('display-none');
        $("#ImportLaborTypeDialog-Project-Form")[0].reset();
        ImportLaborType.OpenDialogAfterInitialize(ImportLaborType.ImportLaborTypesDialog);
    };

    ImportLaborType.CompleteImport = function () {

        $("#ManageLaborType-CompleteImportButton").addClass('display-none');
        $("#ManageLaborType-CompleteImportLoader").removeClass('display-none');

        var dataToPrep = {};
        dataToPrep.importResults = ImportLaborType.ImportedData;
        
        for (x in dataToPrep.importResults) {
            var result = dataToPrep.importResults[x];
            result.StartDate = result.StartDateFormatted;
            result.EndDate = result.EndDateFormatted;
            if (result.ValueSpread != null) {
                result.ValueSpread = result.ValueSpread + "";
            }
            if (result.PercentSpread != null) {
                result.PercentSpread = result.PercentSpread + "";
            }
            delete result.LaborSpreads;
            delete result.UpdateDate;
            delete result.Updateable;
            delete result.SpreadType;

            // fix up the imported labor spreads
            for (y in result.ImportedLaborSpreads) {
                result.ImportedLaborSpreads[y].LaborSpreadDate = result.ImportedLaborSpreads[y].LaborSpreadDateFormatted;
            
                delete result.ImportedLaborSpreads[y].LaborSpreads;
                delete result.ImportedLaborSpreads[y].UpdateDate;
                delete result.ImportedLaborSpreads[y].Updateable;
                delete result.ImportedLaborSpreads[y].SpreadType;
            }
        }

        

        dataToPrep.moqEquation = $.trim($("#MOQEquation").val());

        var dataToSend = JSON.stringify(dataToPrep);

        $.ajax({
            type: 'POST',
            data: dataToSend, 
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            url: completeImportUrl,
            success: function (response) {
                ImportLaborType.CloseDialog(ImportLaborType.ImportLaborTypesResultsDialog);
                $("#ManageLaborType-CompleteImportLoader").addClass('display-none');
                $("#ManageLaborType-CompleteImportButton").removeClass('display-none');
                RaiseNotification("Import Successful");

                // refresh data in angular grid
                var scope = angular.element(document.querySelector("#TaskElementsComposite")).scope();
                scope.refresh();
                scope.$apply();
            },
            error: function () {
                $("#ManageLaborType-CompleteImportLoader").addClass('display-none');
                $("#ManageLaborType-CompleteImportButton").removeClass('display-none');
            }
        });
    };

    return ImportLaborType;
}

function AfterDomLoadImportLaborTypeWidget(ImportLaborType) {
    ImportLaborType.afterDOMLoad();
    createModule($('#import_LaborType'));
    refreshModule($('#import_LaborType'));

    $("#LaborTypeImportErrors button[name='ok-button']").click(function () {
        ImportLaborType.CloseDialog(ImportLaborType.ImportErrorDialog);
    });

    $("#importLaborTypesDialog button[name='cancel-button']").click(function () {
        ImportLaborType.CloseDialog(ImportLaborType.ImportLaborTypesDialog);
    });

    $("#importLaborTypesDialog button[name='import-button']").click(function () {
        ImportLaborType.Import();
    });

    $('#importLaborTypesDialog input[name=ImportType]').change(function () {
        var selected = $(this).val();
        $('input[name=laborTypeImportType]').val(selected.toString().toLowerCase());
        $('#importLaborTypesDialog div.step').each(function () {
            if ($(this).attr('path') == undefined || $(this).attr('path').indexOf(selected) >= 0)
                $(this).removeClass('display-none');
            else
                $(this).addClass('display-none');
        });
    });

    $("#ImportLaborTypeResults #ManageLaborType-CompleteImportButton").click(function () {
        ImportLaborType.CompleteImport();
    });

    $("#ImportLaborTypeResults #ManageLaborType-Back").click(function () {
        ImportLaborType.CloseDialog(ImportLaborType.ImportLaborTypesResultsDialog);
        ImportLaborType.OpenDialogAfterInitialize(ImportLaborType.ImportLaborTypesDialog);
    });

    ImportLaborType.ImportErrorDialog.Element = $("#LaborTypeImportErrors");
    ImportLaborType.ImportErrorDialog.Params = { width: 500, height: 'auto', modal: true, resizable: false, draggable: true, closeOnEscape: false };

    ImportLaborType.ImportLaborTypesDialog.Element = $("#importLaborTypesDialog");
    ImportLaborType.ImportLaborTypesDialog.Params = { title: "Import Resource Types", modal: true, resizable: false, width: 680, height: 633 };

    ImportLaborType.ImportLaborTypesResultsDialog.Element = $("#ImportLaborTypeResults");
    ImportLaborType.ImportLaborTypesResultsDialog.Params = { title: "Import Resource Types", modal: true, resizable: false, width: 680 }

    ImportLaborType.InitializeDialog(ImportLaborType.ImportErrorDialog);
    ImportLaborType.InitializeDialog(ImportLaborType.ImportLaborTypesDialog);
    ImportLaborType.InitializeDialog(ImportLaborType.ImportLaborTypesResultsDialog);
}

function InitializeTaskElementDetailsWidget(metricsSearchDialogTitle, searchMetricsDialogIdSuffix, readOnly, workspaceState, boeId, taskElementId, laborTypeWarning,
    loadMOQEquationUrl, confirmWarningUrl, searchHistoricalMetricsMSTUrl, historicalMetricsDetailsUrl, pagingMetricsUrl,
    boeStateNotDraft, isMetricStoreConnected, searchTypeAheadUrl, allowDateShift, recalculateAndRefreshPageUrl, dateShiftUrl, saveReorderLaborTypesUrl, showDescQuestions, numberDescQuestions, rteFieldSize) {
    var TaskElementDetailsWidget;
    var formConfigs = [];
    formConfigs.push({
        ElementID: 'TaskElementDetailsForm',
        Buttons: [],
        HideOCI: true,
    });

    var dialogConfigs = [];
    //Metrics Search dialog for ISGS and SSC
    dialogConfigs.push({
        ElementID: 'MOQEquation-SearchEstimatingCatalogDialogCommon',
        Params: { 
            width: 556, 
            height: 200,
            modal: true, 
            resizable: false, 
            draggable: true, 
            title: metricsSearchDialogTitle, 
            close: function() { $(document).trigger('InsertMOQElementDialogClosing'); }
        }
    });
    //Metrics Search dialog for MST
    dialogConfigs.push({
        ElementID: 'MOQEquation-SearchEstimatingCatalogDialogMST',
        Params: { 
            width: 575, 
            modal: true, 
            resizable: false, 
            draggable: true, 
            title: metricsSearchDialogTitle, 
            close: function() { $(document).trigger('InsertMOQElementDialogClosing');}
        }
    });

    dialogConfigs.push({
        ElementID: 'MetricSearchResultsContainter',
        Params: {
            width: "auto", 
            modal: true, 
            resizable: false,
            draggable: true, 
            title: metricsSearchDialogTitle + ' Results',
            close: function() { $('#MOQEquation-SearchEstimatingCatalogDialog' + searchMetricsDialogIdSuffix).dialog("option","title",metricsSearchDialogTitle);}               
        }
    });

    var widgetConfig = {};
    widgetConfig.FormConfigs = formConfigs;
    widgetConfig.DialogConfigs = dialogConfigs;
    widgetConfig.ContextID = "TaskElementDetailsModule";
    widgetConfig.isReadOnly = readOnly;
    widgetConfig.IsModule = true;

    TaskElementDetailsWidget = new GenWidget(widgetConfig);
    
    TaskElementDetailsWidget.WorkspaceState = workspaceState;
    TaskElementDetailsWidget.BoeId = boeId;
    TaskElementDetailsWidget.TaskElementId = taskElementId;
    TaskElementDetailsWidget.LTWarningLoadedAtStartup = laborTypeWarning;
    TaskElementDetailsWidget.waitingBeforeSubmit = false;
    TaskElementDetailsWidget.ValidationCreated = false;
    TaskElementDetailsWidget.isInitialization = false;

    TaskElementDetailsWidget.Search = {};
    TaskElementDetailsWidget.ChildWidgets = [];

    TaskElementDetailsWidget.ToggleHelp = function (helpButton, side, modifyTop) {
        var helpDialog = $(helpButton).next();

        //modifyTop defaults to true - raises the position of the top of the dialog
        if (modifyTop == undefined || modifyTop == true) {
            helpDialog.css("top", $(helpButton).position().top - helpDialog.height());
        }

        if (side != undefined && side.toLowerCase() == 'left') {
            helpDialog.css("left", $(helpButton).position().left - helpDialog.width() - 20);
        }
        else {
            helpDialog.css("left", $(helpButton).position().left + $(helpButton).width() + 5);
        }

        helpDialog.toggle("drop");
    };

    TaskElementDetailsWidget.messageConfirmed = function () {
        $(document).trigger("LABORTYPE_MESSAGE_USER_CONFIRMED");
    };

    TaskElementDetailsWidget.registerForEvent('hideMOQChangedNotification', function () {
        $('#LaborTypeWarningFlag').hide();
        $("#labor-typesmoduleexpandedModuleContent0").hide();
        TaskElementDetailsWidget.refreshModule();
    });

    TaskElementDetailsWidget.openWindow = function (currentWorkspace, boeLaborController, action) {
        var url = CreatePostURL(currentWorkspace, boeLaborController, action);
        var name = 'Curves';
        var width = 840;
        var height = 560;
        if (action.contains('Resource')) {
            name = 'Resources';
            width = 770;
             height = 580;
        } else if (action.contains('Perf')) {
            name = 'Performing Orgs';
            width = 700;
        }
        var newwindow = window.open(url, name, 'scrollbars=1,toolbar=no,resizeable=no,status=no,width=' + width + ',height=' + height);
    };

    TaskElementDetailsWidget.CancelToMainGrid = function () {
        if (TaskElementDetailsWidget.isAnyDirty()) {
            Session.confirmDialog(
                "Cancel",
                "Are you sure you want to cancel all changes?",
                function () {
                    TaskElementDetailsWidget.cleanAllDirty();

                    for (widgetIndex in TaskElementDetailsWidget.ChildWidgets) {
                        TaskElementDetailsWidget.ChildWidgets[widgetIndex].cleanDirty();
                    }

                    $("#BOESummary").html('<div class="loader"></div>');
                    $(document).trigger('BOESUMMARYGRID_RELOAD');
                    window.location.hash = 'LMLabor';
                },
                null);
        }
        else {
            $("#BOESummary").html('<div class="loader"></div>');
            $(document).trigger('BOESUMMARYGRID_RELOAD');
            window.location.hash = 'LMLabor';
        }
    };

    TaskElementDetailsWidget.LoadMOQEquationField = function (content, id) {
        $.ajax({
            type: 'POST',
            url: loadMOQEquationUrl + id,
            dataType: 'html',
            success: function (response) {
                  
                content.html(response);
                content.each(function () {
                    var contentSelector = $(this);
                    angular.element(document).injector().invoke(
                        [
                            "$compile", function ($compile) {
                                var scope = angular.element(contentSelector).scope();
                                $compile(contentSelector)(scope);
                                scope.$apply();
                            }
                        ]);
                });
            },
            error: function () {
            }
        });
    };

    TaskElementDetailsWidget.markWarningMessageAsConfirmed = function () {
        var dataToSend = { "TaskID" : TaskElementDetailsWidget.TaskElementId }
        dataToSend = JSON.stringify(dataToSend);

        $.ajax({
            type: 'POST',
            url: confirmWarningUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            complete: function() { $(document).trigger('hideMOQChangedNotification'); }
        });    
    };

    /*
    * Metric search for MST
    */
    TaskElementDetailsWidget.SearchHistoricMetricsMST = function () {
        $('#SearchEstimatingCatalog-SearchButton' + searchMetricsDialogIdSuffix).addClass('display-none');
        $('#SearchEstimatingCatalog-Loader' + searchMetricsDialogIdSuffix).removeClass('display-none');

        TaskElementDetailsWidget.clearValidationBox($("#SearchEstimatingCatalogDialogForm" + searchMetricsDialogIdSuffix + " ul.validation-box"));
        TaskElementDetailsWidget.Search.SearchFor = $('#SearchForTextMST').val();
        TaskElementDetailsWidget.Search.SelectedDataSourceId = $('#FilterSource').val();
        TaskElementDetailsWidget.Search.SelectedMeasureFunctionId = $('#FilterFunction').val();
        TaskElementDetailsWidget.Search.SelectedMeasureNameId = $('#FilterName').val();
        TaskElementDetailsWidget.Search.SelectedProgramId = $('#FilterProgram').val();
        TaskElementDetailsWidget.Search.SelectedMeasureQualifierId = $('#FilterQualifier').val();

        var dataToSend = JSON.stringify(TaskElementDetailsWidget.Search);

        TaskElementDetailsWidget.ajaxRequest({
            url: searchHistoricalMetricsMSTUrl,
            dataType: 'html',
            data: dataToSend,
            success: function (response) {
                $('#MetricSearchResults').html(response);
                $('#MetricSearchResults').removeClass('display-none');
                $('#MetricDetails').addClass('display-none');
                $('#SearchEstimatingCatalog-Loader' + searchMetricsDialogIdSuffix).addClass('display-none');
                $('#SearchEstimatingCatalog-SearchButton' + searchMetricsDialogIdSuffix).removeClass('display-none');
                // close the search dialog so the user doesn't see the title changing
                TaskElementDetailsWidget.getDialog("MOQEquation-SearchEstimatingCatalogDialog" + searchMetricsDialogIdSuffix).closeDialog();
                TaskElementDetailsWidget.getDialog('MetricSearchResultsContainter').openDialog();
                $('#MetricSearchResultsContainter').dialog("option","title",metricsSearchDialogTitle + " Results");
            },
            error: function () {
                $('#SearchEstimatingCatalog-Loader' + searchMetricsDialogIdSuffix).addClass('display-none');
                $('#SearchEstimatingCatalog-SearchButton' + searchMetricsDialogIdSuffix).removeClass('display-none');
            }
        }, $('#SearchEstimatingCatalog-SearchButton' + searchMetricsDialogIdSuffix));
    };

    TaskElementDetailsWidget.DisplayMetricSelected = function (event, inID, openDialogFlag) {
        // Added openDialogFlag for when displaying metric details from the Historical Metrics Used table on the Task Element Details page.       
        var dataToSend = JSON.stringify(inID);
        $.ajax({
            type: 'POST',
            url: historicalMetricsDetailsUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function (response) {
                $('#MetricDetails').html(response);
                $('#MetricDetails').removeClass('display-none');
                $('#MetricSearchResults').addClass('display-none');
                // If check added for displaying metric details from the Historical Metrics Used table.          
                if ((openDialogFlag != undefined) && (openDialogFlag == true)) {
                    // close the search dialog so the user doesn't see the title changing
                TaskElementDetailsWidget.getDialog("MOQEquation-SearchEstimatingCatalogDialog" + searchMetricsDialogIdSuffix).closeDialog();
                    TaskElementDetailsWidget.getDialog('MetricSearchResultsContainter').openDialog();
                    $('#MetricDetailButtons').addClass('display-none');
                    $('#AddDialogDescription').addClass('display-none');
                }

            },
            error: function () {
            }
        });
    };

    TaskElementDetailsWidget.PageHistoricalMetricResults = function (event, metricResultsData) {            
        var metricsDataToSend = JSON.stringify(metricResultsData);
        $.ajax({
            type: 'POST',
            url: pagingMetricsUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: metricsDataToSend,
            success: function (response) {
                var SearchResults = $('#MetricSearchResults');
                SearchResults.html(response);
            }
        });
    };

    TaskElementDetailsWidget.CancelSearchHistoricMetricsMST = function () {
        TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialog' + searchMetricsDialogIdSuffix).closeDialog();
    };
    
    /*
    *Adds the selected metric to the task element for SSC
    */
    TaskElementDetailsWidget.AddMetricToBOE = function (event, inData) {
        var headerDetails = {};
        headerDetails.DataSourceLocation = inData.DataSourceLocation;
        headerDetails.SourceProgramLongName = inData.SourceProgramLongName;
        headerDetails.SourceProgramShortName = inData.SourceProgramShortName;
        headerDetails.ID = $("#TaskID").val();

        var moqEquationBeforeAppend = $.trim($("#MOQEquation").val());
        if (moqEquationBeforeAppend.length > 0) {
            $("#MOQEquation").val(moqEquationBeforeAppend + " " + inData.MOQEquation);
        } else {
            $("#MOQEquation").val(inData.MOQEquation);
        }

        var moqTextBeforeAppend = $.trim(tinyMCE.EditorManager.editors.MOQText_0.getContent());
        tinyMCE.EditorManager.editors.MOQText_0.setContent(moqTextBeforeAppend + " " + inData.MOQText);

        $("#MOQType").val(inData.MOQTypeValue);
        $(document).trigger('ADD_METRIC_TO_BOE_HEADER', headerDetails);

        if(!($.inArray(inData.ID, CurrentMetricsUsed) > -1))
        {
            $('#HMUDeleteButton').addClass('disabled');
            $('#HistoricalMetricsBody').append('<tr name="historicalMetric" id="HMURow" historicalMetricID="' + inData.ID + '"><td class="delete-checkbox" id="DeleteCheckboxId"><input type="checkbox" name="DeleteResource" id="DeleteThisResource" onclick="TaskElementDetailsWidget.deleteToggled(this)"/>' +
            '<td class="metric-id display-none"> <a name="MetricID" class="edit-resource-link"' + '</a> </td>' +               
            '<td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(' + null + ', {metricId : ' + inData.ID + ', getFromSource : true}, true)" style="white-space:normal; width:100px;" title="' + inData.MetricTitle + '">' + inData.MetricTitle + '</a> </td>' +
            '<td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(' + null + ', {metricId : ' + inData.ID + ', getFromSource : true}, true)" style="white-space:normal; width:100px;" title="' + inData.MetricStatus + '">' + inData.MetricStatus + '</a> </td>' +
            '<td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(' + null + ', {metricId : ' + inData.ID + ', getFromSource : true}, true)" style="white-space:normal; width:100px;" title="' + inData.MOQEquation + '">' + inData.MOQEquation + '</a> </td>' +
            '<td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(' + null + ', {metricId : ' + inData.ID + ', getFromSource : true}, true)" style="white-space:normal; width:100px;" title="' + inData.MOQTypeDescription + '">' + inData.MOQTypeDescription + '</a> </td>' +
            '<br id="HistoricalEndRow"/>' +
            '</tr>');

            $('#UsedHistoricalMetrics').removeClass('display-none');
        }

        var CurrentMetricsUsed =[];

        $('tr[name=historicalMetric]').each(function(){ 
            CurrentMetricsUsed.push($(this).attr('historicalMetricID'));
        });

        TaskElementDetailsWidget.getDialog('MetricSearchResultsContainter').closeDialog();
        $(document).trigger('ValidateMOQEquation');

        TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialog' + searchMetricsDialogIdSuffix).closeDialog();
        TaskElementDetailsWidget.refreshModule();
    };

    /*
    *Adds the selected metric to the task element for MST
    */
    TaskElementDetailsWidget.AddMetricToBOEMst = function (event, inData) {
        var headerDetails = {};
        headerDetails.DataSourceLocation = "";
        headerDetails.SourceProgramLongName = "";
        headerDetails.SourceProgramShortName = "";

        $('#MOQType option:contains("Historical Performance")').attr('selected', 'selected');

        var moqTextBeforeAppend = $.trim(tinyMCE.EditorManager.editors.MOQText_0.getContent());
            
        //Include blank lines before and after table because MCE doesn't let you click out of table 
        //if that's all that exists in the rich text field
        var tablePrefix = '<br/><table style="border-collapse: collapse;"><tbody>';
        var cellPrefix = '<td style="border: 0.5pt solid black; background-color: transparent;">';
        var cellSuffix = '</td>'
        var tableSuffix = '</tbody></table><br/>';
        var tableRows = '';

        for (var key in inData.metricData) {
            tableRows = tableRows + '<tr>' + cellPrefix + '<b>' + key + '</b>' + cellSuffix + cellPrefix + inData.metricData[key] + cellSuffix + '</tr>'

        }           

        var populatedTable = tablePrefix + tableRows + tableSuffix;
        tinyMCE.EditorManager.editors.MOQText_0.setContent(moqTextBeforeAppend + " " + populatedTable);

        $(document).trigger('ADD_METRIC_TO_BOE_HEADER', headerDetails);

        if(!($.inArray(inData.ID, CurrentMetricsUsed) > -1))
        {
            $('#HMUDeleteButton').addClass('disabled');
            $('#HistoricalMetricsBody').append('<tr name="historicalMetric" id="HMURow" historicalMetricID="' + inData.ID + '"><td class="delete-checkbox" id="DeleteCheckboxId"><input type="checkbox" name="DeleteResource" id="DeleteThisResource" onclick="TaskElementDetailsWidget.deleteToggled(this)"/>' +
            '<td class="metric-id display-none"> <a name="MetricID" class="edit-resource-link"' + '</a> </td>' +               
            '<td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(' + null + ', {metricId : ' + inData.ID + ', getFromSource : true}, true)" style="white-space:normal; width:100px;" title="' + inData.MeasureName + '">' + inData.MeasureName + '</a> </td>' +
            '<td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(' + null + ', {metricId : ' + inData.ID + ', getFromSource : true}, true)" style="white-space:normal; width:100px;" title="' + inData.ProgramName + '">' + inData.ProgramName + '</a> </td>' +
            '<td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(' + null + ', {metricId : ' + inData.ID + ', getFromSource : true}, true)" style="white-space:normal; width:100px;" title="' + inData.DateApplied + '">' + inData.DateApplied + '</a> </td>' +
            '<br id="HistoricalEndRow"/>' +
            '</tr>');

            $('#UsedHistoricalMetrics').removeClass('display-none');
        }

        var CurrentMetricsUsed =[];

        $('tr[name=historicalMetric]').each(function(){ 
            CurrentMetricsUsed.push($(this).attr('historicalMetricID'));
        });

        TaskElementDetailsWidget.getDialog('MetricSearchResultsContainter').closeDialog();

        TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialog' + searchMetricsDialogIdSuffix).closeDialog();
        TaskElementDetailsWidget.ResetDialogTitle('Historical Measures: Measure Detail', metricsSearchDialogTitle);
        TaskElementDetailsWidget.refreshModule();
    };

    TaskElementDetailsWidget.ReturnToResults = function () {
        $('#MetricSearchResults').removeClass('display-none');
        $('#MetricDetails').addClass('display-none');
    };

    TaskElementDetailsWidget.ReturnToResultsMST = function () {
        $('#MetricSearchResults').removeClass('display-none');
        $('#MetricDetails').addClass('display-none');
        TaskElementDetailsWidget.ResetDialogTitle('Historical Measures: Measure Detail', metricsSearchDialogTitle + 'Results');
    };


    TaskElementDetailsWidget.ResetDialogTitle = function(from, to) {
        var closeButton = $("button.ui-dialog-titlebar-close"); 
        if (closeButton.length > 0) { 
            var titles = closeButton.siblings("span.ui-dialog-title"); 
            if (titles.length > 0) { 
                for (var i = titles.length - 1; i > 0; i--)
                {
                    titles.each(function(i,j){
                        if ($(j).text() == from)
                        {
                            $(j).text(to);
                            return false;
                        }
                    });
                }
            } 
        } 
    };

    TaskElementDetailsWidget.SearchMetricsAgain = function () {
        $('#MetricDetails').addClass('display-none');
        $('#MetricSearchResults').addClass('display-none');
        TaskElementDetailsWidget.getDialog('MetricSearchResultsContainter').closeDialog();
        TaskElementDetailsWidget.ResetDialogTitle(metricsSearchDialogTitle + 'Results', metricsSearchDialogTitle);
        TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialog' + searchMetricsDialogIdSuffix).openDialog();
    };

    TaskElementDetailsWidget.SearchMetricsAgainMST = function () {
        $('#MetricDetails').addClass('display-none');
        $('#MetricSearchResults').addClass('display-none');
        TaskElementDetailsWidget.getDialog('MetricSearchResultsContainter').closeDialog();
        TaskElementDetailsWidget.ResetDialogTitle(metricsSearchDialogTitle + 'Results', metricsSearchDialogTitle);
        TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialog' + searchMetricsDialogIdSuffix).openDialog();
    };

    TaskElementDetailsWidget.CancelMetricsSearchResults = function () {
        $('#MetricDetails').addClass('display-none');
        $('#MetricSearchResults').addClass('display-none');
        TaskElementDetailsWidget.getDialog('MetricSearchResultsContainter').closeDialog();
        TaskElementDetailsWidget.ResetDialogTitle(metricsSearchDialogTitle + 'Results', metricsSearchDialogTitle);
    };

    //Show dialog when external metric source cannot be accessed
    TaskElementDetailsWidget.ShowNoMetricConnection = function () {
        var text = 'genBOE is unable to connect to the Estimating Catalog.';
        var title = 'Connection Failed';
        Session.alertDialog(title, text);
    };

    TaskElementDetailsWidget.DeleteMetric = function(){
        $(this).parent().remove();
        TaskElementDetailsWidget.CheckToShowMetrics();
        TaskElementDetailsWidget.refreshModule();
    };

    TaskElementDetailsWidget.CheckToShowMetrics = function(){
            
        if ($('tr[name=historicalMetric]').length > 0)
        {
            $('#UsedHistoricalMetrics').removeClass('display-none');    
        }
        else
            $('#UsedHistoricalMetrics').addClass('display-none');
    };

    //End metrics Search

    TaskElementDetailsWidget.EnableLockedStateFields = function(){
        if (TaskElementDetailsWidget.WorkspaceState == "Locked")
        {
            $("#TaskElementDetails-MOQTypesDropDownList").attr("readonly", true);
            $(".LockedWorkspaceState").addClass('display-none');

            // check BOE state (widget is not read-only, but custom field values are treated separately)
            if (boeStateNotDraft) {
                $('#TaskElementCustomFieldID select').prop('disabled', 'disabled');
                $('#TaskElementCustomFieldID input').prop('disabled', 'disabled');
                $('#AdjustTaskDatesLink').addClass('display-none');
            }
        }   
    };

    TaskElementDetailsWidget.LoadMOQEquationField($('#MOQEquationFieldContent'), taskElementId);

    $("#SearchEstimatingCatalog-SearchButtonCommon").click(TaskElementDetailsWidget.SearchHistoricMetrics);
    $("#SearchEstimatingCatalog-SearchButtonMST").click(TaskElementDetailsWidget.SearchHistoricMetricsMST);
    $("#SearchEstimatingCatalog-CancelButtonMST").click(TaskElementDetailsWidget.CancelSearchHistoricMetricsMST);

    TaskElementDetailsWidget.on('METRIC_SELECTED', TaskElementDetailsWidget.DisplayMetricSelected);
    TaskElementDetailsWidget.on('BACK_TO_RESULTS', TaskElementDetailsWidget.ReturnToResults);
    TaskElementDetailsWidget.on('BACK_TO_RESULTS_MST', TaskElementDetailsWidget.ReturnToResultsMST);
    TaskElementDetailsWidget.on('ADD_METRIC_TO_BOE', TaskElementDetailsWidget.AddMetricToBOE);
    TaskElementDetailsWidget.on('ADD_METRIC_TO_BOE_MST', TaskElementDetailsWidget.AddMetricToBOEMst);

    TaskElementDetailsWidget.on('SEARCH_METRICS', function() {
        //Display Metric Search dialog if the metrics store is accessible
        if (isMetricStoreConnected == 'true')
        {
            TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialog' + searchMetricsDialogIdSuffix).openDialog(); 
        }
        else
        {
            TaskElementDetailsWidget.ShowNoMetricConnection();                
        }
    });
    TaskElementDetailsWidget.on('SEARCH_METRICS_AGAIN', TaskElementDetailsWidget.SearchMetricsAgain);
    TaskElementDetailsWidget.on('SEARCH_METRICS_AGAIN_MST', TaskElementDetailsWidget.SearchMetricsAgainMST);
    TaskElementDetailsWidget.on('CANCEL_SEARCH_METRICS', TaskElementDetailsWidget.CancelMetricsSearchResults);
    TaskElementDetailsWidget.on('CLEAN_BOE_DETAILS_DIRTY', function() { TaskElementDetailsWidget.cleanDirty(); } );
    TaskElementDetailsWidget.on('PageHistoricalMetricSearch', TaskElementDetailsWidget.PageHistoricalMetricResults);
        
        
    TaskElementDetailsWidget.EnableLockedStateFields();
        
    TaskElementDetailsWidget.CheckToShowMetrics();

    $("#MetricsSeachText").autocomplete({

        source: function(request, add){

            $.ajax({
                type: 'POST',
                url: searchTypeAheadUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify({searchTerm:request.term}),
                success: function (returned) {
                    add(returned);
                }
            });

        }
    });

        
    TaskElementDetailsWidget.on('click', '#AdjustTaskDatesLink', function() { 
        window.location = dateShiftUrl;
    });

    $('#ChangeTaskDatesForm').on('change', "input[name=NewStartDate], input[name=NewEndDate]", TaskElementDetailsWidget.CheckDateRange);
    $('#ChangeTaskDatesForm').on('change', 'input', function() {
        var next = $(this).closest('form').find("button[name='next-button']");
        var screen = next.attr('screen');
        $(this).closest('form').find("button[name='finish-button']").removeClass('disabled');

        TaskElementDetailsWidget.AdjustDates[screen] = "changed";
    });
    $('#ChangeTaskDatesForm').on('change', 'input[name=FlowdownUpdateSelection]', function() {
        var form = $(this).closest('form');

        if ($(this).val() == "Automatic" && TaskElementDetailsWidget.AdjustDates.HasDiscreteTypes) {
            form.find("button[name='next-button']").removeClass('display-none disabled');
            form.find("button[name='finish-button']").addClass('display-none');
        } else {
            form.find("button[name='next-button']").addClass('display-none');
            form.find("button[name='finish-button']").removeClass('display-none');
        }
    });

    if(!TaskElementDetailsWidget.isReadOnly()) {
        if (!(TaskElementDetailsWidget.TaskElementId > 0)) {
            $('#AdjustTaskDatesLink').addClass('display-none');

            $('#TaskElementDetailsForm input[name=StartDate]').prop('disabled', false).datepicker({
                onSelect: function () {
                    $(this).keydown();
                    $(this).blur();
                    $(this).addClass('active');
                }
            });
            $('#TaskElementDetailsForm input[name=EndDate]').prop('disabled', false).datepicker({
                onSelect: function () {
                    $(this).keydown();
                    $(this).blur();
                    $(this).addClass('active');
                }
            });
        }
    }
    else
    {
        $("#InsertWorkspaceVariableLink").parent().prev().remove();
        $("#InsertWorkspaceVariableLink").parent().remove();
        $("#SearchEstimatingCatalogLink").parent().prev().remove();
        $("#SearchEstimatingCatalogLink").parent().remove();

        if (allowDateShift == 'false') {
            $('#AdjustTaskDatesLink').addClass('display-none');
        }

            TaskElementDetailsWidget.refreshModule();
    }

    // Reorder dialog functions for Labor Types

    TaskElementDetailsWidget.ReOrderLaborTypesDialog = {};
    TaskElementDetailsWidget.ReOrderLaborTypesDialog.Element = $("#ReOrderLaborTypesDialog");
    TaskElementDetailsWidget.ReOrderLaborTypesDialog.Params = { width: 700, height: 365, modal: true, resizable: false, draggable: true, title: 'Sort Resource Types' };

    TaskElementDetailsWidget.DisplayReOrderLaborTypesDialog = function () {
        TaskElementDetailsWidget.OpenDialogAfterInitialize(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
    };

    $('#ReOrderLaborTypesDialog #LaborsMoveItemsUp').click(function () {
        TaskElementDetailsWidget.ShiftSelectedItemsUp($('#ReOrderLaborTypesDialog #LaborTypesList'));
        TaskElementDetailsWidget.EnableSave();
    });

    $('#ReOrderLaborTypesDialog #LaborsMoveItemsDown').click(function () {
        TaskElementDetailsWidget.ShiftSelectedItemsDown($('#ReOrderLaborTypesDialog #LaborTypesList'));
        TaskElementDetailsWidget.EnableSave();
    });

    TaskElementDetailsWidget.EnableSave = function () {
        $('#ReOrderLaborTypesDialog-Save').removeClass('disabled');
    };

    TaskElementDetailsWidget.CancelReOrderLaborTypes = function () {
        TaskElementDetailsWidget.CloseDialog(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
        $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').addClass('display-none');
        $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').addClass('disabled');
        $(document).trigger('LOAD_TASK_ELEMENT_GRID');
    };

    TaskElementDetailsWidget.SaveReOrderLaborTypes = function () {

        $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').addClass('display-none');
        $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').removeClass('display-none');

        var dataToSend = {};

        var listOrder = 0;
        dataToSend.LaborTypes = [];
        $('#ReOrderLaborTypesDialog #LaborTypesList').children('option').each(function () {
            var laborTypeToSave = {};
            var laborTypeValue = $(this)[0].value;

            laborTypeToSave.LaborTypeID = laborTypeValue;
            laborTypeToSave.ListOrder = listOrder++;
            dataToSend.LaborTypes.push(laborTypeToSave);
        });

        listOrder = 0;

        dataToSend = JSON.stringify(dataToSend);
        TaskElementDetailsWidget.ajaxRequest({
            type: 'POST',
            url: saveReorderLaborTypesUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {
                TaskElementDetailsWidget.CloseDialog(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
                $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').addClass('display-none');
                $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').removeClass('display-none');
                // refresh the page so we can get the new data
                GenSession.ShowLoadingBox();
                location.reload();
            },
            error: function () {
                $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').addClass('display-none');
                $('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').removeClass('display-none');
            }
        });


    };

    TaskElementDetailsWidget.ShiftSelectedItemsDown = function (list) {
        $($(list).children('option:selected').get().reverse()).each(function () {
            if ($(this).next().length === 0) {
                return false;
            }

            $(this).next().after($(this));
        });
    };
    // Moves selected list items up in the list
    TaskElementDetailsWidget.ShiftSelectedItemsUp = function (list) {
        $(list).children('option:selected').each(function () {
            if ($(this).prev().length === 0) {
                return false;
            }

            $(this).prev().before($(this));
        });
    };

    // End Reorder dialog functions for Labor Types

    // Duplicate dialog functions for Labor Types

    TaskElementDetailsWidget.DuplicateLaborTypesDialog = {};
    TaskElementDetailsWidget.DuplicateLaborTypesDialog.Element = $("#DuplicateLaborTypesDialog");
    TaskElementDetailsWidget.DuplicateLaborTypesDialog.Params = { width: 700, modal: true, resizable: false, draggable: true, title: 'Duplicate Resource Types' };

    TaskElementDetailsWidget.DisplayDuplicateLaborTypesDialog = function () {
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').removeClass('display-none');
        TaskElementDetailsWidget.OpenDialogAfterInitialize(TaskElementDetailsWidget.DuplicateLaborTypesDialog);
    };

    TaskElementDetailsWidget.EnableDuplicateSave = function () {
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').removeClass('disabled');
    };

    TaskElementDetailsWidget.ShowDuplicateLoader = function () {
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').addClass('disabled');
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').addClass('display-none');
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Loader').removeClass('display-none');
    };

    TaskElementDetailsWidget.CancelDuplicateLaborTypes = function () {
        TaskElementDetailsWidget.CloseDialog(TaskElementDetailsWidget.DuplicateLaborTypesDialog);
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Loader').addClass('display-none');
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').addClass('disabled');
        $('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').removeClass('display-none');
    };

    TaskElementDetailsWidget.UpdateOpenEndedCustomFields = function (inputId, value) {
        var input = document.getElementById(inputId);

        if (input) {
            input.value = value;
        }
    };

    // End Duplicate dialog functions for Labor Types

    TaskElementDetailsWidget.on("AllWidgetLoaded", function() {
        if (TaskElementDetailsWidget.LTWarningLoadedAtStartup) {
            $(document).trigger('showMOQChangedNotification');
        }
    });

    TaskElementDetailsWidget.on("LABORTYPE_MESSAGE_USER_CONFIRMED", function() {
        if (TaskElementDetailsWidget.LTWarningLoadedAtStartup) {
            TaskElementDetailsWidget.markWarningMessageAsConfirmed();
        } else {
            $(document).trigger('hideMOQChangedNotification');
        }
    });
   
    TaskElementDetailsWidget.deleteAllSelected = function () {
        var table = document.getElementById("HistoricalMetricsUsedGrid");
        var rowCount = table.rows.length;
        //Skip the header row by not including the 0th element
        for (i = 1; i < rowCount; i++) {
            var row = table.rows[i];
            var chkbox = row.cells[0].childNodes[0]; //get check box object                 

            if(null != chkbox && true == chkbox.checked) {
                table.deleteRow(i);
                rowCount--;
                i--;
            }
        }
    };

    TaskElementDetailsWidget.deleteToggled = function () {
        if ($("input[name=DeleteResource]:checked").length > 0) {
            $("button[name='delete-button']").removeClass("disabled");
        } else {
            $("button[name='delete-button']").addClass("disabled");
        }
    };

    TaskElementDetailsWidget.deleteAllToggle = function (element) {
        $("input[name=DeleteResource]").prop("checked", $(element).prop("checked"));
        TaskElementDetailsWidget.deleteToggled();
    };

        
    /*
     * This does NOT recalculate the page. It recalculates data in the DB, saves it and then reloads the page. Only to be used if the data is bad and recalculation cannot be forced by 
     * changing values in the page. This is the nuclear option.
     */
    TaskElementDetailsWidget.RecalculateAndRefreshPage = function () {
        GenSession.confirmDialog(
            "Recalculate?",
            "Recalculation of this task element will correct data and date entries to fall within the BOE period of performance.  Discrete Spread data which falls outside of the period of performance will be deleted.  Resources utilizing a spread curve will be recalculated.  Updates will be automatically saved.  Delta Hours/EPs and discrete costs may need to be addressed post this action.<BR />Do you wish to continue with the recalculation of this task?",
            function () {
                GenSession.ShowLoadingBox();
                $.ajax({
                    type: 'POST',
                    url: recalculateAndRefreshPageUrl,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'html',
                    success: function () {
                        // refresh the page.. so we can get the new data
                        location.reload();
                    },
                    error: function () {
                    }
                });
            },
            null);
    };

    TaskElementDetailsWidget.cleanAllDirty = function () {
        if (!TaskLaborTypesWidget.isReadOnly()) {
            TaskLaborTypesWidget.cleanDirty();
        }
        if (!TaskLaborSpreadsWidget.isReadOnly()) {
            TaskLaborSpreadsWidget.cleanDirty();
        }
        TaskElementDetailsWidget.cleanDirty();

        for (widgetIndex in TaskElementDetailsWidget.ChildWidgets) {
            TaskElementDetailsWidget.ChildWidgets[widgetIndex].cleanDirty();
        }
    };

    TaskElementDetailsWidget.isAnyDirty = function () {
        for (widgetIndex in TaskElementDetailsWidget.ChildWidgets) {
            if (TaskElementDetailsWidget.ChildWidgets[widgetIndex].isDirty()) {
                return true;
            }
        }

        return TaskElementDetailsWidget.isDirty() ||
            (!TaskLaborTypesWidget.isReadOnly() && TaskLaborTypesWidget.isDirty()) ||
            (!TaskLaborSpreadsWidget.isReadOnly() && TaskLaborSpreadsWidget.isDirty());
    };

    TaskElementDetailsWidget.InitializeDialog = function (dialog) {

        var that = this;
        // Remove old dialogs left behind when jumping between jump pages
        var dialogID = dialog.Element.attr('id');
        if (dialogID !== undefined) {
            $('body .ui-dialog').children('#' + dialogID).parent().remove();
            $('body').children('#' + dialogID).remove();
        }

        // Remove display-none class
        if (dialog.Element.hasClass('display-none')) {
            dialog.Element.removeClass('display-none');
        }

        // Set the dialog disabled so that it doesn't open when initialized
        if (dialog.Params.autoOpen === undefined) {
            dialog.Params.autoOpen = false;
        }
        if (dialog.Params.close === undefined) {
            dialog.Params.close = function () {
                var form = $(dialog.Element).find('form');
                that.cleanDirty(form.attr("id"));
            };
        }

        // Initialize the dialog
        $(dialog.Element).dialog(dialog.Params);
    };
       
    TaskElementDetailsWidget.OpenDialogAfterInitialize = function (dialog) {
        var form = $(dialog.Element).find('form');
        $(dialog.Element).dialog('open');
    };

    TaskElementDetailsWidget.CloseDialog = function (dialog) {
        $(dialog.Element).dialog('close');
    };

    TaskElementDetailsWidget.TaskDescription = CreateRteTemplate(showDescQuestions, numberDescQuestions);

    if (!readOnly) {
        $('#description-element .replacedWidgetText').remove();
        $('#description-element *').removeClass('display-none');

        InitializeRteTemplate(TaskElementDetailsWidget.TaskDescription, 'TaskDescription', rteFieldSize);
    } else {
        HandleRTETemplateDataForReadOnly(TaskElementDetailsWidget.TaskDescription, 'TaskDescription');
    }

    return TaskElementDetailsWidget;
}

function AfterDomLoadTaskElementDetailsWidget(TaskElementDetailsWidget, taskElementId) {
    TaskElementDetailsWidget.cleanAllDirty();

    $("#labor-typesmoduleexpandedModuleContent0").hide();

    TaskElementDetailsWidget.EnableLockedStateFields();

    TaskElementDetailsWidget.isInitialization = false;

    TaskElementDetailsWidget.InitializeDialog(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
    TaskElementDetailsWidget.InitializeDialog(TaskElementDetailsWidget.DuplicateLaborTypesDialog);

    if (taskElementId === '') {
        $('#LTExportButton').addClass('disabled');
    }
}
