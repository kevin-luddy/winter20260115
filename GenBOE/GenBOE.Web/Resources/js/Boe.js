/*
This javascript file is called from BOE ascx files
*/

function InitializeTaskElementGrid(readOnly, loadedData, duplicateTaskDialogUrl, saveReorderTaskElementUrl) {

    var TaskElementGrid = new GridWidget("BOELabor", '', readOnly);

    // serialize the task elements to be stored as a javascript object
    TaskElementGrid.loadedData = loadedData;

    //Display Duplicate Task Element Dialog
    TaskElementGrid.displayDuplicateTaskElementDialog = function () {
        $("#DuplicateLaborTaskElementDialog").dialog({ width: 700, modal: true, resizable: false, draggable: true, title: 'Duplicate Task Elements', autoOpen: false, close: TaskElementGrid.closeDuplicateTaskElementDialog });

        $('#DuplicateTask').addClass('display-none');
        $('#DuplicateTaskElementDialog-Loader').removeClass('display-none');

        $.ajax({
            type: 'GET',
            url: duplicateTaskDialogUrl,
            success: function (response) {
                $('#DuplicateLaborTaskElementDialog').html(response).dialog('open');
                DuplicateTaskWidget.AfterSave = function () { $(document).trigger('LOAD_TASK_ELEMENT_GRID'); }
                $('#DuplicateTask').removeClass('display-none');
                $('#DuplicateTaskElementDialog-Loader').addClass('display-none');
            },
            error: function () {
                $('#DuplicateTask').removeClass('display-none');
                $('#DuplicateTaskElementDialog-Loader').addClass('display-none');
            }
        });
    };

    //Close the duplicate task dialog this way so there will never be multiple instances of it existing across task tabs
    TaskElementGrid.closeDuplicateTaskElementDialog = function () {
        $('#DuplicateLaborTaskElementDialog').dialog('destroy').empty();
    }

    TaskElementGrid.ReOrderTaskElementDialog = {};
    TaskElementGrid.ReOrderTaskElementDialog.Element = $("#ReOrderTaskElementDialog");
    TaskElementGrid.ReOrderTaskElementDialog.Params = { width: 700, height: 365, modal: true, resizable: false, draggable: true, title: 'Sort Task Elements' };

    TaskElementGrid.displayReOrderTaskElementDialog = function () {
        TaskElementGrid.OpenDialogAfterInitialize(TaskElementGrid.ReOrderTaskElementDialog);
    };

    $('#ReOrderTaskElementDialog #TasksMoveItemsUp').click(function () {
        TaskElementGrid.ShiftSelectedItemsUp($('#ReOrderTaskElementDialog #TaskElementsList'));
        TaskElementGrid.EnableSave();
    });

    $('#ReOrderTaskElementDialog #TasksMoveItemsDown').click(function () {
        TaskElementGrid.ShiftSelectedItemsDown($('#ReOrderTaskElementDialog #TaskElementsList'));
        TaskElementGrid.EnableSave();
    });

    TaskElementGrid.EnableSave = function () {
        $('#ReOrderTaskElementDialog-Save').removeClass('disabled');
    };

    TaskElementGrid.CancelReOrderTaskElement = function () {
        TaskElementGrid.CloseDialog(TaskElementGrid.ReOrderTaskElementDialog);
        $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Loader').addClass('display-none');
        $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Save').addClass('disabled');
        $(document).trigger('LOAD_TASK_ELEMENT_GRID');
    };

    TaskElementGrid.SaveReOrderTaskElements = function () {

        $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Save').addClass('display-none');
        $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Loader').removeClass('display-none');

        var dataToSend = {};

        var listOrder = 0;
        dataToSend.BOETaskElements = [];
        $('#ReOrderTaskElementDialog #TaskElementsList').children('option').each(function () {
            var taskToSave = {};
            var taskValue = $(this).val();

            taskToSave.TaskID = taskValue;
            taskToSave.ListOrder = listOrder++;
            dataToSend.BOETaskElements.push(taskToSave);
        });

        listOrder = 0;

        dataToSend = JSON.stringify(dataToSend);
        TaskElementGrid.ajaxRequest({
            type: 'POST',
            url: saveReorderTaskElementUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {
                TaskElementGrid.CloseDialog(TaskElementGrid.ReOrderTaskElementDialog);
                $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Save').removeClass('display-none');
                $(document).trigger('LOAD_TASK_ELEMENT_GRID');


            },
            error: function () {
                $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderTaskElementDialog #ReOrderTaskElementDialog-Save').removeClass('display-none');
            }
        });


    };

    TaskElementGrid.ShiftSelectedItemsDown = function (list) {
        $($(list).children('option:selected').get().reverse()).each(function () {
            if ($(this).next().length == 0) {
                return false;
            }

            $(this).next().after($(this));
        });
    };
    // Moves selected list items up in the list
    TaskElementGrid.ShiftSelectedItemsUp = function (list) {
        $(list).children('option:selected').each(function () {
            if ($(this).prev().length == 0) {
                return false;
            }

            $(this).prev().before($(this));
        });
    };

    TaskElementGrid.registerForDelegateEvent('click', '#AddTaskElement:not(.disabled)', function () { TaskElementGrid.expandTaskElement(''); });

    TaskElementGrid.expandTaskElement = function (id) {
        window.location.hash = 'LMLabor/task/' + id;
    }

    TaskElementGrid.addNewTaskElement = function () {
        window.location.hash = 'LMLabor/task/';
    }

    TaskElementGrid.getElementData = function(key) {
        return TaskElementGrid.allTaskElements[key];
    };

    return TaskElementGrid;
}

function AfterDomLoadTaskElementGrid(TaskElementGrid, deleteBoeTaskElementUrl, deleteAllBoeTaskElementsUrl, laborTaskElementType) {
    TaskElementGrid.afterDOMLoad();
    TaskElementGrid.InitializeDialog(TaskElementGrid.ReOrderTaskElementDialog);

    TaskElementGrid.registerForLiveEvent('click', '#AddTaskElement:not(.disabled)', TaskElementGrid.addNewTaskElement);

    createModule($('.task-element-grid.module'));

    TaskElementGrid.applyReadOnly();
    if (TaskElementGrid.isReadOnly()) {
        $("#AddTaskElement").parent().hide();
    }

    // convert loaded data from an array to a dictionary for quick lookup
    TaskElementGrid.allTaskElements = {};
    TaskElementGrid.loadedData.forEach(function(element) {
        TaskElementGrid.allTaskElements[element.TaskElementDetailID] = element;
    });

    // clean out the old data for garbage collection
    delete TaskElementGrid.loadedData;

    $("#TaskElementGridBody .delete").click(function () {
        var input = this;
        Session.confirmDialog('Delete Task Element', 'Are you sure you want to delete this task? This can not be undone.', function () {
            var key = $(input).closest('tr').attr('pkid');
            var element = TaskElementGrid.getElementData(key);

            var deletedTask = {};
            deletedTask.TaskElementDetailID = element.TaskElementDetailID;
            deletedTask.UpdateDateLong = element.UpdateDateLong;

            TaskElementGrid.data = deletedTask;

            var dataToSend = JSON.stringify(TaskElementGrid.data);
            $('#PageLoading').removeClass('display-none');
            $.ajax({
                type: 'POST',
                url: deleteBoeTaskElementUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function (response) {
                    $(document).trigger("LOAD_TASK_ELEMENT_GRID");
                    BOESummary.Reload();
                    $('#PageLoading').addClass('display-none');
                },
                error: function (response) {
                    $('#PageLoading').addClass('display-none');
                }
            });
        });
    });

    $("#TaskElementGridHeader .delete").click(function () {
        var input = this;
        Session.confirmDialog('Delete All Task Element', 'Are you sure you want to delete all task elements? This can not be undone.', function () {

            var dataToSend = JSON.stringify(TaskElementGrid.data);
            $('#PageLoading').removeClass('display-none');
            $.ajax({
                type: 'POST',
                url: deleteAllBoeTaskElementsUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function (response) {
                    $(document).trigger("LOAD_TASK_ELEMENT_GRID");
                    BOESummary.Reload();
                    $('#PageLoading').addClass('display-none');
                },
                error: function (response) {
                    $('#PageLoading').addClass('display-none');
                }
            });
        });
    });

    $('#TaskElementGrid td.element-link').click(function () {
        var row = $(this).closest('tr');
        var key = row.attr('pkid');
        var element = TaskElementGrid.getElementData(key);

        switch (element.TaskType) {
            case laborTaskElementType:
                TaskElementGrid.expandTaskElement(key);
                break;
            default:
                throw "Invalid Task Element Type '" + element.TaskType + "'";
        }
    });

    if (TaskElementGrid.isReadOnly()) {
        $("button[name='add-task-element']").addClass('display-none');
        $('#TaskElementGridContainer .buttons').addClass('display-none');
        $('#TaskElementGrid .delete').addClass('display-none');
        $('#TaskElementGrid .deleteColumn').addClass('display-none');
    }

    SortableGrid('.task-element-grid');

    refreshModule($('.task-element-grid.module'));

}

function SetupMonthPicker(inputBoxName) {
    var inputBox = $(inputBoxName);

    SetupMonthPickerJquery(inputBox);
}

function SetupMonthPickerJquery(inputBox) {
    inputBox.datepicker("enable");
    inputBox.datepicker("option", "showButtonPanel", true);


    var curYear = new Date().getFullYear();
    var yearRange = (curYear - 10) + ':' + (curYear + 50);  // BOEJ-1664 - set range of years for datepicker year menu. 
    inputBox.datepicker({
        dateFormat: 'mm/yy',
        changeMonth: true,
        changeYear: true,
        yearRange: yearRange,
        showButtonPanel: true,
        beforeShow: function () {
            var selDate;
            if ((selDate = $(this).val()).length > 0) {
                var iYear = selDate.substring(selDate.length - 4, selDate.length);
                var iMonth = parseInt(selDate.substring(0, selDate.length - 5));
                // change iMonth from 1-based to 0-based
                var iMonth = iMonth - 1;

                var date = new Date(iYear, iMonth, 1);
                $(this).datepicker('option', 'monthNames');
                $(this).datepicker('option', 'defaultDate', date);
                $(this).datepicker('setDate', date);
            }
        },

        onClose: function (dateText, inst) {

            function isDonePressed() {
                return ($('#ui-datepicker-div').html().indexOf('ui-datepicker-close ui-state-default ui-priority-primary ui-corner-all ui-state-hover') > -1);
            }

            if (isDonePressed()) {
                var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
                var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
                $(this).datepicker('setDate', new Date(year, month, 1)).trigger('change');
            }
        },
        onSelect: function () {
            $(this).keydown();
            $(this).blur();
            $(this).addClass('active');
            $(this).change();
        },
        showOn: "button"
    });

    inputBox.focus(function () {
        $(".ui-datepicker-calendar").hide();
        $("#ui-datepicker-div").position({
            my: "center top",
            at: "center bottom",
            of: $(this)
        });
    });

}

function AfterDomLoadBoeHeaderWidget(containsOCI, readOnly, workspaceState, saveEditBoeHeaderUrl,  
    boeStateNotDraft, allowDateShift, rteFieldSize, dateShiftUrl, findAdjacentBoesUrl, boeId, newBoeUrl, useQuestions, numberQuestions) {
    var formConfigs = [];
    formConfigs.push({
        ElementID: 'BoeHeaderForm',
        Buttons: [
            {
                ButtonClass: 'ies-action',
                ButtonText: 'Save',
                ButtonName: 'save-button',
                ButtonAction: function (buttonPressed) {
                    BoeHeaderWidget.Save($(buttonPressed));
                },
                Stateful: true
            }, 
            {
                ButtonClass: 'ies',
                ButtonText: 'Cancel',
                ButtonName: 'cancel-button',
                ButtonAction: function (buttonPressed) {
                    if (BoeHeaderWidget.isDirty()) {
                        Session.confirmDialog("Cancel", "Are you sure you want to cancel all changes?", BoeHeaderWidget.Cancel, null);
                    }
                    else {
                        BoeHeaderWidget.Cancel();
                    }
                },
                Stateful: false
            },
            {
                ButtonClass: 'ies disabled',
                ButtonText: 'Next BOE',
                ButtonName: 'next-boe-button',
                ButtonAction: function (buttonPressed) {
                    if (BoeHeaderWidget.isDirty() || (typeof TaskElementDetailsWidget !== 'undefined' && TaskElementDetailsWidget.isAnyDirty())) {
                        Session.confirmDialog("Navigate to Next BOE", "Are you sure you want to cancel all changes?", BoeHeaderWidget.NavigateToNextBOE, null);
                    }
                    else {
                        BoeHeaderWidget.NavigateToNextBOE();
                    }
                },
                Stateful: false
            },
            {
                ButtonClass: 'ies disabled',
                ButtonText: 'Previous BOE',
                ButtonName: 'previous-boe-button',
                ButtonAction: function (buttonPressed) {
                    if (BoeHeaderWidget.isDirty() || (typeof TaskElementDetailsWidget !== 'undefined' && TaskElementDetailsWidget.isAnyDirty())) {
                        Session.confirmDialog("Navigate to Previous BOE", "Are you sure you want to cancel all changes?", BoeHeaderWidget.NavigateToPreviousBOE, null);
                    }
                    else {
                        BoeHeaderWidget.NavigateToPreviousBOE();
                    }
                },
                Stateful: false
            }
        ],
        ContainsOCI: containsOCI,
        OnDataRetrieved: function(data) {
            data.CustomFieldValues=[];

            $("#BoeHeader :input.customField").each(function () {
                if ($(this).attr("openended") === "true") {
                    data.CustomFieldValues.push({
                        IsOpenEnded: true,
                        SelectionID: $(this).attr("selectionid"),
                        CustomFieldValueID: $(this).attr("customfieldvalueid"),
                        UpdateDateLong: $(this).attr("UpdateDateLong"),
                        OpenEndedValue: $(this).val(),
                        CustomFieldID: $(this).attr("customfieldid")
                    });
                } else {
                    if($(this).val()==="" && $(this).attr("selectionid")!== "-1"){
                    //if the field value was deleted
                        data.CustomFieldValues.push({
                            SelectionID: $(this).attr("selectionid"),
                            UpdateDateLong: $(this).attr("UpdateDateLong")
                        });
                    $(this).attr("selectionid", "-1");
                    }else if($(this).val()!=="" && $(this).attr("selectionid")!== "-1"){
                        //if the field value was updated
                        data.CustomFieldValues.push({
                            SelectionID: $(this).attr("selectionid"),
                            CustomFieldValueID: $(this).val(),
                            UpdateDateLong: $(this).attr("UpdateDateLong")
                        });
                    }else if($(this).val()!=="" && $(this).attr("selectionid")=== "-1"){
                        //new field value
                        data.CustomFieldValues.push({ CustomFieldValueID: $(this).val() });
                    }
                }
                
            });
        }
    });

    var dialogConfigs = [];

    var widgetConfig = {};
    widgetConfig.ContextID = "BoeHeader";
    widgetConfig.isReadOnly = readOnly;
    widgetConfig.IsModule = true;
    widgetConfig.FormConfigs = formConfigs;
    widgetConfig.DialogConfigs = dialogConfigs;

    BoeHeaderWidget = new GenWidget(widgetConfig);
    BoeHeaderWidget.WorkspaceState = workspaceState;
    BoeHeaderWidget.PreviousBoeId = undefined;
    BoeHeaderWidget.NextBoeId = undefined;
    $('#BoeHeaderForm button[name=previous-boe-button]').prop('disabled', true);
    $('#BoeHeaderForm button[name=next-boe-button]').prop('disabled', true);

    BoeHeaderWidget.DataSource = CreateRteTemplate(useQuestions, numberQuestions);

    BoeHeaderWidget.GetCookieValue = function (cookieArray, name) {
        var nameEQ = name + "=";
        var value = '';
        for (var i = 0; i < cookieArray.length; i++) {
            var c = cookieArray[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) {
                value = c.substring(nameEQ.length, c.length);
                break;
            }
        }

        if (value != '') {
            return JSON.parse(unescape(value));
        } else {
            return undefined;
        }

    };

    BoeHeaderWidget.FindAdjacentBOEs = function () {
        var data = {};
        data.boeId = boeId;

        var ca = document.cookie.split(';');
        var predicate = BoeHeaderWidget.GetCookieValue(ca, 'predicateBoes');
        var reverse = BoeHeaderWidget.GetCookieValue(ca, 'reverseBoes');
        if (reverse === undefined || reverse === false) {
            data.sortOrder = "asc";
        } else {
            data.sortOrder = "desc";
        }

        if (predicate === undefined) {
            data.predicate = [];
            data.predicate.push('WBSText');
        } else {
            data.predicate = predicate;
        }

        $.ajax({
            type: "POST",
            url: findAdjacentBoesUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response) {
                    if (response.PreviousId !== undefined && response.PreviousId !== null) {
                        BoeHeaderWidget.PreviousBoeId = response.PreviousId;
                        $('#BoeHeaderForm button[name=previous-boe-button]').removeClass("disabled");
                        $('#BoeHeaderForm button[name=previous-boe-button]').removeProp('disabled');
                    }

                    if (response.NextId !== undefined && response.NextId !== null) {
                        BoeHeaderWidget.NextBoeId = response.NextId;
                        $("button[name=next-boe-button]").removeClass('display-none');
                        $('#BoeHeaderForm button[name=next-boe-button]').removeClass("disabled");
                        $('#BoeHeaderForm button[name=next-boe-button]').removeProp('disabled');
                    }
                }
            }
        });
    };

    BoeHeaderWidget.NavigateToPreviousBOE = function () {
        if (BoeHeaderWidget.PreviousBoeId !== undefined) {
            // reload new url
            var url = newBoeUrl + BoeHeaderWidget.PreviousBoeId.toString();
            BoeHeaderWidget.cleanDirty();
            if (typeof TaskElementDetailsWidget !== 'undefined'){
               TaskElementDetailsWidget.cleanAllDirty();
            }
            $(document).trigger("SHOW_LOADING_BOX");
            window.location = url;
        }
    };

    BoeHeaderWidget.NavigateToNextBOE = function () {
        if (BoeHeaderWidget.NextBoeId !== undefined) {
            // reload new url
            var url = newBoeUrl + BoeHeaderWidget.NextBoeId.toString();
            BoeHeaderWidget.cleanDirty();
            if (typeof TaskElementDetailsWidget !== 'undefined'){
                TaskElementDetailsWidget.cleanAllDirty();
            }
            $(document).trigger("SHOW_LOADING_BOX");
            window.location = url;
        }
    };

    BoeHeaderWidget.FindAdjacentBOEs();

    BoeHeaderWidget.Save = function(button) {
        var data = BoeHeaderWidget.getForm('BoeHeaderForm').getData();          
            
        // there seems to be an issue with the jquery serializer when dealing with rich text pasted from excel so we need to get these field contents again
        data.RteTemplateAnswers = [];
        GetRteTemplateJson(BoeHeaderWidget.Description, 'Description', data);   

        if(!BoeHeaderWidget.isReadOnly()) {
            GetRteTemplateJson(BoeHeaderWidget.DataSource, 'DataSource', data);    
        }

        var dataToSend = JSON.stringify(data);
        BoeHeaderWidget.saveRequest({
            url: saveEditBoeHeaderUrl,
            data: dataToSend,
            success: BoeHeaderWidget.UpdateFields,
        }, button);
    };

    /** 
    * Capture the original values in case the user presses cancel
    */
    BoeHeaderWidget.captureOriginalValues = function () {

        $("#Title").val($.trim($("#Title").val())); // Remove leading and trailing whitespace from Boe Title 
        BoeHeaderWidget.OriginalTitle = $("#Title").val();
        if ($('#MetricUsed:checked').length > 0) {
            BoeHeaderWidget.OriginalShowDisclosure = true;
        } else {
            BoeHeaderWidget.OriginalShowDisclosure = false;
        }

        // capture custom field values
        BoeHeaderWidget.OriginalCustomFieldValues = [];
        $('#BoeHeader select[name="CustomFieldValues"], #BoeHeader input.customField').each(function (index) {
            var $this = $(this);
            BoeHeaderWidget.OriginalCustomFieldValues.push({ customfieldid: $this.attr('customfieldid'), customfieldvalueid: $this.attr('customfieldvalueid'), value: $this.val() });
        });
    }

    BoeHeaderWidget.Cancel = function () {
        ResetRteTemplate(BoeHeaderWidget.Description, 'Description');

        if (!BoeHeaderWidget.isReadOnly()) {
            ResetRteTemplate(BoeHeaderWidget.DataSource, 'DataSource');
        }

        $("#Title").val(BoeHeaderWidget.OriginalTitle);
        $('#MetricUsed').prop('checked', BoeHeaderWidget.OriginalShowDisclosure);

        // restore custom fields to their original values
        var customField;
        $('#BoeHeader select[name="CustomFieldValues"], #BoeHeader input.customField').each(function (index) {
            var $this = $(this);
            var customfieldid = $this.attr('customfieldid');
            for (var i = 0; i < BoeHeaderWidget.OriginalCustomFieldValues.length; i++) {
                var customField = BoeHeaderWidget.OriginalCustomFieldValues[i];
                if (customField.customfieldid === customfieldid) {
                    $this.val(customField.value);
                    break;
                }
            }
        });

        BoeHeaderWidget.getForm('BoeHeaderForm').clearValidationBox();

        // refresh the sizing of the module for the outside borders (because of hiding/showing custom forms)
        BoeHeaderWidget.refreshModule();

        setTimeout(function () {
            BoeHeaderWidget.cleanDirty(); 
        }, 50);
    };

    BoeHeaderWidget.UpdateFields = function (returnModel) {
        BoeHeaderWidget.captureOriginalValues();

        $("#UpdateDateLong").val(returnModel.UpdateDateLong);

        // Set these attributes to ensure they're set for blank fields
        // They will be overridden as needed below
        $("#BoeHeader :input.customField").attr("customfieldvalueid", "-1").attr("UpdateDateLong", "0").attr("selectionid", "-1");

        if (returnModel.CustomFieldValues.length > 0) {
            for (x in returnModel.CustomFieldValues) {
                var toSet = $("#BoeHeader :input.customField[selectionid=" + returnModel.CustomFieldValues[x].SelectionID + "]");
                var customFieldValue = returnModel.CustomFieldValues[x].CustomFieldValueID;
                if (toSet.length == 0) {
                    toSet = $("#BoeHeader :input.customField[customfieldid=" + returnModel.CustomFieldValues[x].CustomFieldID + "]");
                    $(toSet).attr("selectionid", returnModel.CustomFieldValues[x].SelectionID);
                }
                $(toSet).attr("UpdateDateLong", returnModel.CustomFieldValues[x].UpdateDateLong);
                $(toSet).attr("customfieldvalueid", customFieldValue);
            }
        }

        // Update the UpdateDate and Id for template answers
        if (returnModel.HeaderRteTemplateAnswers.length > 0) {
            for (x in returnModel.HeaderRteTemplateAnswers) {
                var answer = returnModel.HeaderRteTemplateAnswers[x];
                var source = "";

                switch (answer.SourceId) {
                    case 1:
                        source = "Description";
                        break;
                    case 2:
                        source = "DataSource";
                        break;
                }
                
                var answerElement = $("#" + source + "_" + answer.SortOrder);

                if (answerElement.length > 0) {
                    answerElement[0].setAttribute('updateDateLong', returnModel.HeaderRteTemplateAnswers[x].UpdateDateLong);
                    answerElement[0].setAttribute('pkid', returnModel.HeaderRteTemplateAnswers[x].Id);
                }
            }
        }
    };

    BoeHeaderWidget.AdjustFieldState = function(){
        if (BoeHeaderWidget.WorkspaceState == "Locked") {
            $(".LockedWorkspaceState").addClass('display-none');

            // check BOE state (widget is not read-only, but custom field values are treated separately)
            if (boeStateNotDraft) {
                $('#AdjustDatesLink').addClass('display-none');
                $('#customFieldID select').prop('disabled', 'disabled');
                $('#customFieldID input').prop('disabled', 'disabled');
            }
        }
    };

    BoeHeaderWidget.AddMetricToBOEHeader = function(event, inHeaderInfo){

        if (inHeaderInfo.SourceProgramShortName.length || inHeaderInfo.DataSourceLocation.length) {
            // Update sources of data by append to existing and set dirty bit.
            var newDataSource = $.trim(tinyMCE.EditorManager.editors.DataSource_0.getContent()) + "<BR />" + inHeaderInfo.SourceProgramShortName;
            if (inHeaderInfo.DataSourceLocation.length) {
                newDataSource = newDataSource + " - " + inHeaderInfo.DataSourceLocation;
            }
            tinyMCE.EditorManager.editors.DataSource_0.setContent(newDataSource);
            BoeHeaderWidget.setDirty();
        }

        $("#BoeHeaderForm :input").prop("disabled", false);
        $(".specialBOEActions").show();
        $("#BoeHeaderSave").remove("disabled");
        $("#MetricUsed").prop('checked', false);

        $("#DisclosureCheck").closest('.form-row').removeClass("display-none");

        BoeHeaderWidget.refreshModule();
    };
   
    BoeHeaderWidget.AdjustFieldState();

    BoeHeaderWidget.on('ADD_METRIC_TO_BOE_HEADER', BoeHeaderWidget.AddMetricToBOEHeader);
        
    // Adjust Dates Button
    BoeHeaderWidget.on('click', '#AdjustDatesLink', function() { 
        window.location = dateShiftUrl;
    });

    $('#ChangeBoeDatesForm').on('change', "input[name=NewStartDate], input[name=NewEndDate]", BoeHeaderWidget.CheckDateRange);
    $('#ChangeBoeDatesForm').on('change', 'input', function() {
        var next = $(this).closest('form').find("button[name='next-button']");
        var screen = next.attr('screen');
        $(this).closest('form').find("button[name='finish-button']").removeClass('disabled');

        BoeHeaderWidget.AdjustDates[screen] = "changed";
    });

    $('#ChangeBoeDatesForm').on('change', 'input[name=FlowdownUpdateSelection]', function() {
        var form = $(this).closest('form');

        if ($(this).val() == "Automatic" && BoeHeaderWidget.AdjustDates.HasDiscreteTypes) {
            form.find("button[name='next-button']").removeClass('display-none disabled');
            form.find("button[name='finish-button']").addClass('display-none');
        } else {
            form.find("button[name='next-button']").addClass('display-none');
            form.find("button[name='finish-button']").removeClass('display-none');
        }
    });

    BoeHeaderWidget.on('TOGGLE_SPECIAL_BUTTONS', function(event, isActive) {
        if (isActive || !BoeHeaderWidget.isReadOnly()) {
            $(".specialBOEActions").show();
        } else {
            $(".specialBOEActions").hide();
        }
        BoeHeaderWidget.refreshModule();
    });

    BoeHeaderWidget.captureOriginalValues();

    if(!BoeHeaderWidget.isReadOnly())
    {
        InitializeRteTemplate(BoeHeaderWidget.DataSource, 'DataSource', rteFieldSize);
    }
    else  // read-only
    { 
        HandleRTETemplateDataForReadOnly(BoeHeaderWidget.DataSource, 'DataSource');
    }

    if (!allowDateShift) {
        $('#AdjustDatesLink').addClass('display-none');
    }

    var refresh = function() {BoeHeaderWidget.refreshModule()};
    if (window.attachEvent) // Microsoft
    {            
        window.attachEvent('onload', function() {setTimeout(refresh, 1);}); //needs slight delay to work properly in IE
    }
    else if (window.addEventListener) // W3C standard
    {            
        window.addEventListener('load', function() {refresh();}); // NB **not** 'onload'
    } 
    
    return BoeHeaderWidget;
}

function AfterDomLoadBoeHeaderDescription(BOEHeaderDescription_ReadOnly, rteFieldSize, useQuestions, numberQuestions) {
    var BOEHeaderDescription = new Widget("BoeHeaderDescription", BOEHeaderDescription_ReadOnly);
    BoeHeaderWidget.Description = CreateRteTemplate(useQuestions, numberQuestions);

    //bind any events that the objects need to observe to and member functions.
    BOEHeaderDescription.InitializeDescription = function () {
        BOEHeaderDescription.applyReadOnly();

        // because the parent widget may set things to readonly if the parent is readonly
        // we also need to manually set things back to updateable if our widget is not readonly
        if (!BOEHeaderDescription_ReadOnly) {
            $('#description-element .replacedWidgetText').remove();
            $('#description-element *').removeClass('display-none');
            $('#BoeHeaderForm .oci-note').css('display', 'block');

            InitializeRteTemplate(BoeHeaderWidget.Description, 'Description', rteFieldSize);
        } else {
            HandleRTETemplateDataForReadOnly(BoeHeaderWidget.Description, 'Description');
            $('#BoeHeaderForm .buttons button[name=save-button]').addClass('display-none');
            $('#BoeHeaderForm .buttons button[name=cancel-button]').addClass('display-none');
        }

        $('#BoeHeaderForm .buttons').removeClass('display-none');
    };

    BOEHeaderDescription.InitializeDescription();
    BoeHeaderWidget.refreshModule();

    return BOEHeaderDescription;
}

function InitializeBOESummaryWidget(displayBoeSummaryUrl) {

    //create base js object;
    var BOESummary = new Widget("boesummary");

    // Create array to hold labor type names and ids
    BOESummary.laborTypeNames = new Array();

    BOESummary.RemoveTravelCost = function (e, updateInfo) {
        if (updateInfo.LaborTypeID != "") {
            var laborTypeTotalToUpdate = $('#LaborTypeSummaryContentTable').find('tr[nameofelement="' + Helper.htmlEncodeForjQuery(updateInfo.LaborTypeID) + '"]');
            var costToUpdate = laborTypeTotalToUpdate.find("td.labor-cost");
            var hoursToUpdate = laborTypeTotalToUpdate.find("td.labor-hours");

            if (updateInfo.DeltaCost != undefined) {

                if (laborTypeTotalToUpdate.length) {
                    var toUpdate = toFixed(parseFloat(costToUpdate.attr("value")) - parseFloat(updateInfo.DeltaCost), 2);
                    costToUpdate.attr("value", toUpdate);
                    costToUpdate.attr("title", Helper.addCommas(toUpdate, "$"));
                    if (toUpdate == 0) {
                        costToUpdate.find("div.summaryText").text("");
                    } else {
                        costToUpdate.find("div.summaryText").text(Helper.addCommas(toUpdate, "$"));
                    }
                }
                else {
                    BOESummary.InsertNewLaborType(updateInfo);
                }
                var CellTotalToUpdate = $('#BOESummary-TotalCost-Total');
                var totalToUpdate = toFixed(parseFloat(CellTotalToUpdate.attr("value")) - parseFloat(updateInfo.DeltaCost), 2);
                CellTotalToUpdate.attr("value", totalToUpdate);
                CellTotalToUpdate.attr("title", Helper.addCommas(totalToUpdate, "$"));
                CellTotalToUpdate.find("div.summaryText").text(Helper.addCommas(totalToUpdate, "$"));

            }

            BOESummary.RemoveEmptyLaborTypes();
            // Update the grid total        
        }
    };

    // Function callback to update the grid when a labor type update is triggered
    BOESummary.UpdateSummaryInfo = function (e, updateInfo) {
        if (updateInfo.LaborTypeID != "") {
            var laborTypeTotalToUpdate = $('#LaborTypeSummaryContentTable').find('tr[nameofelement="' + Helper.htmlEncodeForjQuery(updateInfo.LaborTypeID) + '"]');
            var costToUpdate = laborTypeTotalToUpdate.find("td.labor-cost");
            var hoursToUpdate = laborTypeTotalToUpdate.find("td.labor-hours");

            if (updateInfo.DeltaCost != undefined) {

                if (laborTypeTotalToUpdate.length) {
                    var toUpdate = toFixed(parseFloat(costToUpdate.attr("value")) + parseFloat(updateInfo.DeltaCost), 2);
                    costToUpdate.attr("value", toUpdate);
                    costToUpdate.attr("title", Helper.addCommas(toUpdate, "$"));
                    if (toUpdate == 0) {
                        costToUpdate.find("div.summaryText").text("");
                    } else {
                        costToUpdate.find("div.summaryText").text(Helper.addCommas(toUpdate, "$"));
                    }
                }
                else {
                    BOESummary.InsertNewLaborType(updateInfo);
                }
                var CellTotalToUpdate = $('#BOESummary-TotalCost-Total');
                var totalToUpdate = toFixed(parseFloat(CellTotalToUpdate.attr("value")) + parseFloat(updateInfo.DeltaCost), 2);
                CellTotalToUpdate.attr("value", totalToUpdate);
                CellTotalToUpdate.attr("title", Helper.addCommas(totalToUpdate, "$"));
                CellTotalToUpdate.find("div.summaryText").text(Helper.addCommas(totalToUpdate, "$"));

            }

            if (updateInfo.DeltaHours != undefined) {
                if (laborTypeTotalToUpdate.length) {
                    var toUpdate = parseInt(hoursToUpdate.attr("value")) + parseInt(updateInfo.DeltaHours);
                    hoursToUpdate.attr("value", toUpdate);
                    hoursToUpdate.attr("title", Helper.addCommas(toUpdate));
                    if (toUpdate == 0) {
                        hoursToUpdate.find("div.summaryText").text("");
                    } else {
                        hoursToUpdate.find("div.summaryText").text(Helper.addCommas(toUpdate));
                    }
                }
                else {
                    BOESummary.InsertNewLaborType(updateInfo);
                }
                var CellTotalToUpdate = $('#BOESummary-TotalHours-Total');
                var totalToUpdate = parseInt(CellTotalToUpdate.attr("value")) + parseInt(updateInfo.DeltaHours);
                CellTotalToUpdate.attr("value", totalToUpdate);
                CellTotalToUpdate.attr("title", Helper.addCommas(totalToUpdate));
                CellTotalToUpdate.find("div.summaryText").text(Helper.addCommas(totalToUpdate));

            }
            BOESummary.RemoveEmptyLaborTypes();
            // Update the grid total        
        }
    };

    BOESummary.Reload = function () {
        $("#BOESummary").load(displayBoeSummaryUrl);
    };


    // Loops through labor types and removes any existing rows that have 0 hour totals
    BOESummary.RemoveEmptyLaborTypes = function () {
        $("#LaborTypeSummaryContentTable tr").each(function () {
            if ($(this).find('.labor-hours').attr("value") == 0 && $(this).find('.labor-cost').attr("value") == 0) {
                $(this).remove();
            }
        });

        refreshModule($('.boesummary.module'));
    }

    // Inserts a labor type into the grid when that labor type didn't previously have a
    // total displayed
    BOESummary.InsertNewLaborType = function (updateInfo) {
        var selectedItem = null;

        $('#LaborTypeSummaryContentTable tr[category=' + updateInfo.category + ']').each(function () {
            // If the current Labor Type ID is in the grid and is greater than the ID being inserted
            // then save the index to insert before, and leave the for loop
            if ($(this).attr("nameofelement") > updateInfo.LaborTypeID) {
                selectedItem = $(this);
                return false;
            }
        });

        var DeltaHours = 0;
        var DeltaHoursPrint = "";
        if (updateInfo.DeltaHours != null) {
            DeltaHours = parseFloat(updateInfo.DeltaHours);
            DeltaHoursPrint = updateInfo.DeltaHours;
        }

        var DeltaCost = 0;
        var DeltaCostPrint = "";
        if (updateInfo.DeltaCost != null) {
            DeltaCost = parseFloat(updateInfo.DeltaCost);
            DeltaCostPrint = updateInfo.DeltaCost;
        }

        var divToInsert =
            '<tr category="' + updateInfo.category + '" class="BOESummary-LaborType" nameofelement="' +
            Helper.htmlEncodeForjQuery(updateInfo.LaborTypeID) +
            '" resourcetype="' + updateInfo.LaborTypeID + '" rollupcount="1"><td class="labor-type"><div  class="summaryText">' +
            Helper.htmlEncode(updateInfo.LaborTypeID) +
            '</div></td><td class="labor-hours" value="' + DeltaHours + '" title="' + Helper.addCommas(DeltaHoursPrint) + '"><div  class="summaryText">' +
            Helper.addCommas(DeltaHoursPrint) +
            '</div></td><td class="labor-cost" value="' + DeltaCost + '" title="' + Helper.addCommas(DeltaCostPrint) + '"><div  class="summaryText">' +
            Helper.addCommas(DeltaCostPrint, "$") +
            '</div></td></tr>';

        // If an index was found in the above for loop, insert before that index
        if (selectedItem != null) {
            selectedItem.before(divToInsert);
        }
            // Otherwise, find the closet category tr and insert after that.
        else {
            var sorted = false;
            for (x = updateInfo.category; x > 0; x--) {
                if ($('table#LaborTypeSummaryContentTable tr[category=' + x + ']:last').length != 0) {
                    $('table#LaborTypeSummaryContentTable tr[category=' + x + ']:last').after(divToInsert);
                    sorted = true;
                    x = 0;
                }
            }
            if (!sorted) {
                //if there is stuff in the summary then we know we are before them
                if ($('table#LaborTypeSummaryContentTable tr').length != 0) {
                    $('table#LaborTypeSummaryContentTable tr:first').before(divToInsert);
                } else {
                    //else there is nothing in the table.
                    $('table#LaborTypeSummaryContentTable').append(divToInsert);
                }
            }

        }
    }

    BOESummary.prepNamesForjQuerySelectors = function () {
        $("table#LaborTypeSummaryContentTable tr").each(function () {
            $(this).attr("nameofelement", Helper.htmlEncodeForjQuery($(this).attr("nameofelement")));
        });
    };

    return BOESummary;
}

function AfterDomLoadBOESummaryWidget(BOESummary, hoursUpdatedEvent, travelHoursUpdatedEvent, reloadSummaryEvent) {
    createModule($('.boesummary.module'));

    // Bind to the event for updating the totals in the Summary Grid
    BOESummary.registerForEvent(hoursUpdatedEvent, BOESummary.UpdateSummaryInfo);
    BOESummary.registerForEvent(travelHoursUpdatedEvent, BOESummary.RemoveTravelCost);

    BOESummary.registerForEvent(reloadSummaryEvent, BOESummary.Reload);

    $("td.labor-hours div.summaryText").each(function () {
        $(this).text(Helper.addCommas($(this).text()));
    });

    $("td.labor-cost div.summaryText").each(function () {
        $(this).text(Helper.addCommas($(this).text(), "$"));
    });

    // Remove labor types with zero hours from the grid
    BOESummary.RemoveEmptyLaborTypes();

    BOESummary.prepNamesForjQuerySelectors();

    refreshModule($('.boesummary.module'));
    CollapsibleModule($('.boesummary.module'));
}

function InitializeBOEDetailsWidget(isMaterial, boeDetailsReloadHistoryEvent, boeDetailsReloadCommentEvent, displayTaskElementDetailsEvent, displayOdcDetailsEvent, displayTravelDetailsEvent,
     displayMaterialDetailsEvent) {
    var BOEDetails = new Widget("BOEDetails");
    //which page are you on on the ISGS Labor Tab.
    BOEDetails.LoadingHtml = '<div class="module-content"><div class="module-content-left" style="height: 44px"></div>' +
                            '<div class="module-content-center" style="width: 1392px; padding-bottom: 3px;">' +
                                '<div class="module-content-data">' + 
                                    '<div class="loader" />' +
                                 '</div>' +
                            '</div>' +
                            '<div class="module-content-right" style="height: 44px"></div></div>';
    
    BOEDetails.FinishTabClicked = function () {

        // clean dirty.
        $(document).trigger('CLEAN_BOE_DETAILS_DIRTY');

        // this is first because it's the only line in this method that relies on the current state.
        var urlHash = BOEDetails.currentLocation.hash.substring(1).split('/');

        // change tabs
        window.location = window.location.protocol + "//" + window.location.host + window.location.pathname + BOEDetails.href;

        // 30 ms delay to ensure that loading the grid happens in the background, completely hidden from the user.
        setTimeout(function () {
            // then reload the old tab if on a details page.
            var currentTab = urlHash[0];
            
            if (currentTab == '') {
                if (isMaterial) {
                    currentTab = 'Material';
                } else  {
                    currentTab = 'LMLabor';
                } 
            }

            // Load the grid page if needed.
            if (currentTab == 'LMLabor') {
                if (urlHash[1] == 'task' || urlHash[1] == 'missiontask') {
                    $("div[tab*=LMLabor] .task-grid").html(BOEDetails.LoadingHtml);
                    $("div[tab*=LMLabor] .task-grid").removeClass("display-none");
                    $("div[tab*=LMLabor] .task-details").addClass("display-none");
                    $(document).trigger('LOAD_TASK_ELEMENT_GRID');
                }
            }
            else if (currentTab == 'ODC') {
                if (urlHash[1] == 'odc') {
                    $("div[tab*=ODC] .odc-grid").html(BOEDetails.LoadingHtml);
                    $("div[tab*=ODC] .odc-grid").removeClass("display-none");
                    $("div[tab*=ODC] .odc-details").addClass("display-none");
                    $(document).trigger('LOAD_ODC_ELEMENT_GRID');
                }
            }
            else if (currentTab == 'Travel') {
                if (urlHash[1] == 'travel') {
                    $("div[tab*=Travel] .travel-grid").html(BOEDetails.LoadingHtml);
                    $("div[tab*=Travel] .travel-grid").removeClass("display-none");
                    $("div[tab*=Travel] .travel-details").addClass("display-none");
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                }
            }
            else if (currentTab == 'Material') {
                if (urlHash[1] == 'material') {
                    $("div[tab*=Material] .material-grid").html(BOEDetails.LoadingHtml);
                    $("div[tab*=Material] .material-grid").removeClass("display-none");
                    $("div[tab*=Material] .material-details").addClass("display-none");
                    $(document).trigger('LOAD_MATERIAL_ELEMENT_GRID');
                }
            }
        }, 30);

        $(document).trigger('TOGGLE_SPECIAL_BUTTONS', true);
    };

    BOEDetails.LoadBOEDetails = function () {
        BOEDetails.InitialLoad = true;

        //        var currentTab = window.location.hash;

        var urlHash = window.location.hash.substring(1).split('/');
        var currentTab = urlHash[0];

        if (isMaterial) {
            switch (currentTab) {

                case 'History':
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    $(document).trigger('LOAD_MATERIAL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    break;

                case 'Comments':
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    $(document).trigger('LOAD_MATERIAL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    break;

                default:
                    $(document).trigger('LOAD_MATERIAL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    break;
            }
        }
        else {
            switch (currentTab) 
            {
                case 'LMLabor':
                    $(document).trigger('LOAD_TASK_ELEMENT_GRID');
                    $(document).trigger('LOAD_ODC_ELEMENT_GRID');
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    break;

                case 'ODC':
                    $(document).trigger('LOAD_ODC_ELEMENT_GRID');
                    $(document).trigger('LOAD_TASK_ELEMENT_GRID');
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    break;

                case 'Travel':
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger('LOAD_TASK_ELEMENT_GRID');
                    $(document).trigger('LOAD_ODC_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    break;

                case 'History':
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    $(document).trigger('LOAD_TASK_ELEMENT_GRID');
                    $(document).trigger('LOAD_ODC_ELEMENT_GRID');
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    break;

                case 'Comments':
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    $(document).trigger('LOAD_TASK_ELEMENT_GRID');
                    $(document).trigger('LOAD_ODC_ELEMENT_GRID');
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    break;

                default:
                    $(document).trigger('LOAD_TASK_ELEMENT_GRID');
                    $(document).trigger('LOAD_ODC_ELEMENT_GRID');
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger(boeDetailsReloadCommentEvent);
                    $(document).trigger(boeDetailsReloadHistoryEvent);
                    break;
            }
        }
    }

    BOEDetails.LoadTab = function () {

        if (Session.isDirty() && window.location.hash != BOEDetails.currentHash && (Session.getDirtyLength() > 1 || !Session.isDirty('BoeHeaderForm'))) {

            Session.confirmDialog(
                    "Unsaved Changes",
                    "You have selected to leave this tab without saving your current changes.  If you wish to leave the unsaved changes select Yes.  To stay on the page select No.",
                    BOEDetails.FinishLoadTab,
                    BOEDetails.RevertLoadTab);
        } else {
            BOEDetails.FinishLoadTab();
        }
    }

    BOEDetails.RevertLoadTab = function () {
        window.location.hash = BOEDetails.currentHash;
    }

    BOEDetails.FinishLoadTab = function () {

        if (window.location.hash != BOEDetails.currentHash) {

            $(document).trigger('CLEAN_BOE_DETAILS_DIRTY');

            $('div[tab*=LMLabor]').hide();
            $('div[tab*=Travel]').hide();
            $('div[tab*=Material]').hide();
            $('div[tab*=ODC]').hide();
            $('div[tab*=Comments]').hide();
            $('div[tab*=History]').hide();

            var urlHash = window.location.hash.substring(1).split('/');
            var currentTab = urlHash[0];

            if (currentTab == '') {
                if (isMaterial) {
                    currentTab = 'Material';
                }
                else {
                    currentTab = 'LMLabor';
                } 
            }

            // Load the detail pages if needed.
            if (currentTab == 'LMLabor') {

                if (urlHash[1] == 'task') {
                    // load task element
                    var data = {};
                    data.taskElementID = urlHash[2];
                    $(document).trigger(displayTaskElementDetailsEvent, data);
                } else if (!BOEDetails.InitialLoad) {
                    $(document).trigger('DISPLAY_TASK_ELEMENT_GRID');
                } else if (BOEDetails.InitialLoad) {
                    // do nothing
                } else {
                    throw "Invalid URL Hash";
                }
            }
            else if (currentTab == 'ODC') {

                if (urlHash[1] == 'odc') {
                    // load odc element
                    $(document).trigger(displayOdcDetailsEvent, urlHash[2]);
                } else {
                    $(document).trigger('DISPLAY_ODC_ELEMENT_GRID');
                }
            }
            else if (currentTab == 'Travel') {
                
                if (urlHash[1] == 'travel') {
                    $(document).trigger(displayTravelDetailsEvent, urlHash[2]);
                } else {
                    $(document).trigger('DISPLAY_TRAVEL_ELEMENT_GRID');
                }
            }
            else if (currentTab == 'Material') {

                if (urlHash[1] == 'material') {
                    // load material element
                    $(document).trigger(displayMaterialDetailsEvent, urlHash[2]);
                } else if (!BOEDetails.InitialLoad) {
                    $(document).trigger('DISPLAY_MATERIAL_ELEMENT_GRID');
                }
            }

            var currentTabDiv = $('div[tab*=' + currentTab + ']');
            
            $(currentTabDiv).parent().find('li.active').removeClass('active');
            $(currentTabDiv).parent().find('.tabs a[tab*=' + currentTab + ']').parent().addClass('active');
            $(currentTabDiv).show();

            BOEDetails.currentHash = window.location.hash;

            $('div[tab*=' + currentTab + '] .module').each(function () {
                if ($(this).find('.module-header').length == 0) {
                    createModule($(this));
                }
                refreshModule(this);
            });

            // if BOE Material is selected, remove the other tabs minus boe state tabs (history, comments&approvals)
            if (isMaterial) {
                $('[tab*=LMLabor]').parents("li").addClass('display-none');
                $('[tab*=Travel]').parents("li").addClass('display-none');
                $('[tab*=ODC]').parents("li").addClass('display-none');
            } else {
                $('[tab*=Material]').parents("li").addClass('display-none');
            }
            BOEDetails.InitialLoad = false;
        }
    }

    return BOEDetails;
}

function AfterDomLoadBOEDetailsWidget(BOEDetails, displayTaskElementDetailsEvent, displayTaskElementCompositeUrl, displayOdcDetailsEvent,
    displayOdcCompositeUrl, displayTravelDetailsEvent, displayTravelCompositeUrl, displayZoneTravelCompositeUrl, displayTaskElementGridUrl,
    displayBoeSummaryUrl, boeDetailsReloadCommentEvent, displayBoeCommentUrl, boeDetailsReloadHistoryEvent, displayBoeHistoryUrl, displayOdcGridUrl, displayTravelGridUrl,
    displayZoneTravelGridUrl, displayMaterialGridUrl, displayMaterialDetailsEvent, displayMaterialCompositeUrl, isZoneTravel) {
    BOEDetails.afterDOMLoad();

    window.onhashchange = BOEDetails.LoadTab;

    $('#BOEDetailTabs .tabs a').click(function (event) {
        event.preventDefault();
        BOEDetails.href = $(this).attr('href');
        BOEDetails.currentLocation = window.location;

        if (Session.isDirty()) {

            Session.confirmDialog(
             "Unsaved Changes",
             "You have selected to leave this tab without saving your current changes.  If you wish to leave the unsaved changes select Yes.  To stay on the page select No.",
             BOEDetails.FinishTabClicked,
             null);
        } else {
            BOEDetails.FinishTabClicked();
        }
    });

    BOEDetails.registerForEvent(displayTaskElementDetailsEvent, function (e, params) {
        var taskElementID = params.taskElementID;

        $(document).trigger('TOGGLE_SPECIAL_BUTTONS', false);

        if (document.querySelector("#TaskElementsComposite")) {

            // if ui-tinymce was created for TaskDescription, then remove it since it sometimes causes problems during partial page refresh
            var taskDescription = $('textarea[name=TaskDescription]');
            if (taskDescription.length > 0) {
                var newId = taskDescription.attr('id');
                tinymce.execCommand('mceRemoveControl', true, newId);
            }
            
            angular.element(document.querySelector("#TaskElementsComposite")).scope().$destroy();
        }
        $("div[tab*=LMLabor] .task-details").children().remove();
        $("div[tab*=LMLabor] .task-details").html(BOEDetails.LoadingHtml);
        $("div[tab*=LMLabor] .task-details").removeClass("display-none");
        $("div[tab*=LMLabor] .task-grid").addClass("display-none");
        var taskUrl = "";
        if (taskElementID > 0) {
            taskUrl = displayTaskElementCompositeUrl + '/taskelement/' + taskElementID;
        }
        else {
            taskUrl = displayTaskElementCompositeUrl;
        }

        var data = {};

        $.ajax({
            type: "POST",
            url: taskUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: JSON.stringify(data),
            success: function (response) {
                $('div[tab*=LMLabor] .task-details').html(response);

                $('div[tab*=LMLabor] .task-details').each(function () {
                    var content = $(this);
                    angular.element(document).injector().invoke(
                    [
                        "$compile", function ($compile) {
                            var scope = angular.element(content).scope();
                            $compile(content)(scope);
                            scope.$apply();
                        }
                    ]);
                });
            }
        });
    });

    BOEDetails.registerForEvent("DISPLAY_TASK_ELEMENT_GRID", function (e) {
        if ($("div[tab*=LMLabor] .task-grid").hasClass('display-none')) {
            $(document).trigger('TOGGLE_SPECIAL_BUTTONS', true);
            $("div[tab*=LMLabor] .task-grid").removeClass("display-none");
            $("div[tab*=LMLabor] .task-details").addClass("display-none");
            $(document).trigger('LOAD_TASK_ELEMENT_GRID');
        }
    });

    BOEDetails.registerForEvent(displayOdcDetailsEvent, function (e, odcElementID) {
        $(document).trigger('TOGGLE_SPECIAL_BUTTONS', false);
        $("div[tab*=ODC] .odc-details").children().remove();
        $("div[tab*=ODC] .odc-details").html(BOEDetails.LoadingHtml);
        $("div[tab*=ODC] .odc-details").removeClass("display-none");
        $("div[tab*=ODC] .odc-grid").addClass("display-none");

        var odcUrl = "";
        if (odcElementID.length > 0) {
            odcUrl =displayOdcCompositeUrl + '/odcelement/' + odcElementID;
        }
        else {
            odcUrl = displayOdcCompositeUrl;
        }

        $.ajax({
            type: "POST",
            url: odcUrl,
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                $('div[tab*=ODC] .odc-details').html(response);

            }
        });
    });

    BOEDetails.registerForEvent("DISPLAY_ODC_ELEMENT_GRID", function (e) {
        if ($("div[tab*=ODC] .odc-grid").hasClass('display-none')) {
            $(document).trigger('TOGGLE_SPECIAL_BUTTONS', true);
            $("div[tab*=ODC] .odc-grid").removeClass("display-none");
            $("div[tab*=ODC] .odc-details").addClass("display-none");
            $(document).trigger('LOAD_ODC_ELEMENT_GRID');
        }
    });

    BOEDetails.registerForEvent(displayTravelDetailsEvent, function (e, travelElementID) {
        $(document).trigger('TOGGLE_SPECIAL_BUTTONS', false);
        $("div[tab*=Travel] .travel-details").children().remove();
        $("div[tab*=Travel] .travel-details").html(BOEDetails.LoadingHtml);
        $("div[tab*=Travel] .travel-details").removeClass("display-none");
        $("div[tab*=Travel] .travel-grid").addClass("display-none");

        var travelUrl = "";
        
        if (isZoneTravel && isZoneTravel == true) {
            if (travelElementID.length > 0) {
                travelUrl = displayZoneTravelCompositeUrl + '/travelelement/' + travelElementID;
            }
            else {
                travelUrl = displayZoneTravelCompositeUrl;
            }
        }
        else {
            if (travelElementID.length > 0) {
                travelUrl = displayTravelCompositeUrl + '/travelelement/' + travelElementID;
            }
            else {
                travelUrl = displayTravelCompositeUrl;
            }
        }

        $.ajax({
            type: "POST",
            url: travelUrl,
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                $('div[tab*=Travel] .travel-details').html(response);

            }
        });
    });

    BOEDetails.registerForEvent("DISPLAY_TRAVEL_ELEMENT_GRID", function (e) {
        if ($("div[tab*=Travel] .travel-grid").hasClass('display-none')) {
            $(document).trigger('TOGGLE_SPECIAL_BUTTONS', true);
            $("div[tab*=Travel] .travel-grid").removeClass("display-none");
            $("div[tab*=Travel] .travel-details").addClass("display-none");
            $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
        }
    });

    BOEDetails.registerForEvent('LOAD_TASK_ELEMENT_GRID', function (e) {
        $("div[tab*=LMLabor] .task-grid").html(BOEDetails.LoadingHtml);
        $.ajax({
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            url: displayTaskElementGridUrl,
            success: function (response) {
                $("div[tab*=LMLabor] .task-grid").html(response);

            }
        });
    });

    BOEDetails.registerForEvent('LOAD_REFRESH_SUMMARY_BOE_SUMMARY', function (e) {
        $('#BOESummary').html('<div class="loader" />');
        $.ajax({
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            url: displayBoeSummaryUrl,
            success: function (response) {
                $('#BOESummary').html(response);
            }
        });
    });

    BOEDetails.registerForEvent(boeDetailsReloadCommentEvent, function (e) {
        $("div[tab*=Comments]").html(BOEDetails.LoadingHtml);
        $.ajax({
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            url: displayBoeCommentUrl,
            success: function (response) {
                $("div[tab*=Comments]").html(response);
            }
        });
    });

    BOEDetails.registerForEvent(boeDetailsReloadHistoryEvent, function (e) {
        $("div[tab*=History]").html(BOEDetails.LoadingHtml);
        $.ajax({
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            url: displayBoeHistoryUrl,
            success: function (response) {
                $("div[tab*=History]").html(response);
            }
        });
    });

    BOEDetails.registerForEvent('LOAD_ODC_ELEMENT_GRID', function () {
        $("div[tab*=ODC] .odc-grid").html(BOEDetails.LoadingHtml);
        $.ajax({
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            url: displayOdcGridUrl,
            success: function (response) {
                $("div[tab*=ODC] .odc-grid").html(response);
            }
        });
    });

    BOEDetails.registerForEvent('LOAD_TRAVEL_ELEMENT_GRID', function () {
        $("div[tab*=Travel] .travel-grid").html(BOEDetails.LoadingHtml);

        var travelGridUrl = "";
        if (isZoneTravel && isZoneTravel == true) {
            travelGridUrl = displayZoneTravelGridUrl;
        }
        else {
            travelGridUrl = displayTravelGridUrl;
        }

        $.ajax({
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            url: travelGridUrl,
            success: function (response) {
                $("div[tab*=Travel] .travel-grid").html(response);
            }
        });
    });

    BOEDetails.registerForEvent("DISPLAY_MATERIAL_ELEMENT_GRID", function (e) {
        if ($("div[tab*=Material] .material-grid").hasClass('display-none')) {
            $(document).trigger('TOGGLE_SPECIAL_BUTTONS', true);
            $("div[tab*=Material] .material-grid").removeClass("display-none");
            $("div[tab*=Material] .material-details").addClass("display-none");
            $(document).trigger('LOAD_MATERIAL_ELEMENT_GRID');
        }
    });

    BOEDetails.registerForEvent('LOAD_MATERIAL_ELEMENT_GRID', function (e) {
        $("div[tab*=Material] .material-grid").html(BOEDetails.LoadingHtml);
        $.ajax({
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            url: displayMaterialGridUrl,
            success: function (response) {
                $("div[tab*=Material] .material-grid").html(response);

            }
        });
    });

    BOEDetails.registerForEvent(displayMaterialDetailsEvent, function (e, materialID) {
        $(document).trigger('TOGGLE_SPECIAL_BUTTONS', true);

        $("div[tab*=Material] .material-details").children().remove();
        $("div[tab*=Material] .material-details").html(BOEDetails.LoadingHtml);
        $("div[tab*=Material] .material-details").removeClass("display-none");
        $("div[tab*=Material] .material-grid").addClass("display-none");

        var materialUrl = "";
        if (materialID.length > 0) {
            materialUrl = displayMaterialCompositeUrl + '/materialelement/' + materialID;
        }
        else {
            materialUrl = displayMaterialCompositeUrl;
        }

        $.ajax({
            type: "POST",
            url: materialUrl,
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                $('div[tab*=Material] .material-details').html(response);

            }
        });
    });

    BOEDetails.LoadBOEDetails();
    BOEDetails.LoadTab();
}

function InitializeSubmitForReviewWidget(boeId, boeSubmitForReviewUrl) {
    var BOESubmitForReview = new Widget("BOESubmitForReview");

    BOESubmitForReview.ValidateClicked = function () {
        $('#BOESubmitForReview-Button').addClass('display-none');
        $('#BOESubmitForReview-Loader').removeClass('display-none');
        var dataToSend = { "id": boeId };
        dataToSend = JSON.stringify(dataToSend);

        $.ajax({
            type: 'POST',
            url: boeSubmitForReviewUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: BOESubmitForReview.SubmitForReviewSuccess,
            error: BOESubmitForReview.SubmitForReviewFailure
        });
    };

    BOESubmitForReview.SubmitForReviewSuccess = function(results) {
        $('#BOESubmitForReview-Button').removeClass('display-none');
        $('#BOESubmitForReview-Loader').addClass('display-none');
    };

    BOESubmitForReview.SubmitForReviewFailure = function(results) {
        $('#BOESubmitForReview-Button').removeClass('display-none');
        $('#BOESubmitForReview-Loader').addClass('display-none');
    };
    return BOESubmitForReview
}

function InitializeBOESubmitForApprovalWidget(boeContainsSumBoesUrl, boeSubmitForApprovalUrl, displayBoeValidateResultsUrl, workspace, boeId) {
    var BOESubmitForApproval = new Widget("BOESubmitForApproval");

    BOESubmitForApproval.SubmitForApprovalClicked = function () {
        $('#BOESubmitForApproval-Button').addClass('display-none');
        $('#BOESubmitForApproval-Loader').removeClass('display-none');

        $.ajax({
            type: 'POST',
            url: boeContainsSumBoesUrl,
            dataType: 'json',
            success: function (response) {
                $('#BOESubmitForApproval-Button').removeClass('display-none');
                $('#BOESubmitForApproval-Loader').addClass('display-none');

                if (response.Type === "Variable" && response.Result) {
                    Session.confirmDialog(
                    'Updates to Variable Values May Change Total',
                    'The BOE contains Task Variables and/or Workspace Variables that are based on the sum of select BOEs. If the Total of the select BOEs change, you will be required to review and resubmit the BOE for approval. Are you sure you want to submit the BOE for approval at this time?',
                    function () { BOESubmitForApproval.ValidateClicked(); },
                    null);
                } else {
                    BOESubmitForApproval.ValidateClicked();
                }
            },
            error: function () {
                BOESubmitForApproval.ValidateClicked();
            }
        });
    };

    BOESubmitForApproval.ValidateClicked = function () {
        $('#BOESubmitForApproval-Button').addClass('display-none');
        $('#BOESubmitForApproval-Loader').removeClass('display-none');

        var dataToSend = { "id": boeId };
        dataToSend = JSON.stringify(dataToSend);

        $.ajax({
            type: 'POST',
            url: boeSubmitForApprovalUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: BOESubmitForApproval.SubmitForReviewSuccess,
            error: BOESubmitForApproval.SubmitForReviewFailure
        });

    };

    BOESubmitForApproval.SubmitForReviewSuccess = function (results) {

        if (!results.isValid) {
            window.open(displayBoeValidateResultsUrl);

            $('#BOESubmitForApproval-Button').removeClass('display-none');
            $('#BOESubmitForApproval-Loader').addClass('display-none');

            if (results.NewState !== undefined) {
                // if the BOE state has changed, then refresh the page
                $(window.location).attr('href', window.location.href);
            }
        }
        else {
            $(window.location).attr('href', window.location.protocol + '//' + window.location.host + "/" + workspace);
        }
    };

    BOESubmitForApproval.SubmitForReviewFailure = function (results) {
        $('#BOESubmitForApproval-Button').removeClass('display-none');
        $('#BOESubmitForApproval-Loader').addClass('display-none');
    };
    return BOESubmitForApproval;
}

function InitializeBOESearchWidget(readOnly, boeId) {
    var BOESearchWidget = new Widget("BOESearch", readOnly);

    BOESearchWidget.BoeID = boeId;

    BOESearchWidget.DisplayBOESearchDialog = function () {
        $('#BOESearch').dialog({ width: 700, modal: true, resizable: false, draggable: true, title: 'Search for BOE', close: BOESearchWidget.ClearDirtyBits });
    }

    BOESearchWidget.CloseBOESearchDialog = function() {
        $('#BOESearch').dialog('close');
    }

    BOESearchWidget.DisplaySearchResultsDialog = function() {
        $('#SearchResults').dialog({ width: 700, modal: true, resizable: false, draggable: true, title: 'BOE Search Results' });
    }

    BOESearchWidget.CloseSearchResultsDialog = function() {
        $('#SearchResults').dialog('close');
    }

    BOESearchWidget.ClearForm = function () {
        $(document).trigger("CLEAR_FORM");
    }

    // using its own method to do this since the iboe.js one only looks for 1 form, and this dialog has 2.
    BOESearchWidget.ClearDirtyBits = function() {
        BOESearchWidget.cleanDirty('QuickForm');
        BOESearchWidget.cleanDirty('AdvancedForm');
    }

    return BOESearchWidget;
}

function AfterDomLoadBOESearchWidget(BOESearchWidget, pageSearchResultsUrl) {
    BOESearchWidget.afterDOMLoad();
    BOESearchWidget.registerForEvent("SEARCH_AGAIN", function () {
        $('#Search-QuickSearch').removeClass('display-none');
        $('#Loader-QuickSearch').addClass('display-none');
        $('#Search-AdvancedSearch').removeClass('display-none');
        $('#Loader-AdvancedSearch').addClass('display-none');
        BOESearchWidget.CloseSearchResultsDialog();
        BOESearchWidget.DisplayBOESearchDialog();
    });

    BOESearchWidget.registerForEvent("CLOSE_SEARCH_RESULTS", BOESearchWidget.CloseSearchResultsDialog);



    BOESearchWidget.registerForEvent("PAGE_RESULTS", function (event, data) {
        var dataToSend = JSON.stringify(data);
        $.ajax({
            type: 'POST',
            url: pageSearchResultsUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function (response) {
                var SearchResults = $('#SearchResults');
                SearchResults.html(response);
            }
        });
    });

    $('#CopyFromExistingBOE-Button').click(function () {
        $('#IsCopyFromBoeContext').val(true);
        BOESearchWidget.ClearForm();
        BOESearchWidget.DisplayBOESearchDialog();
    });


    // Triggers when user selects "Copy MOQ from BOE" on the task element "MOQ Equation" field dropdown.
    BOESearchWidget.registerForEvent("COPY_MOQ_SELECT", function (event, data) {
        $('#IsCopyFromBoeContext').val(false);
        BOESearchWidget.ClearForm();
        BOESearchWidget.DisplayBOESearchDialog();
    });

    BOESearchWidget.registerForEvent("DISPLAY_RESULTS", function (event, data) {
        var SearchResults = $('#SearchResults');
        SearchResults.html(data);
        BOESearchWidget.CloseBOESearchDialog();
        BOESearchWidget.DisplaySearchResultsDialog();
    });

    BOESearchWidget.registerForEvent("CHANGE_SEARCH_HEADER", function () {
        $('#SearchResults').dialog({ title: "Copy BOE Conflicts" });

    });

    BOESearchWidget.registerForEvent("CANCEL_SEARCH", BOESearchWidget.CloseBOESearchDialog);
}

function InitializeBOECommentsGridWidget(BOECommentsGrid, originalApproverInfo, workspaceLocked, reloadCommentGridEvent, saveBOECommentsUrl,
    reloadHistoryGridEvent, workspace) {
    BOECommentsGrid.EditingComment = "";
    BOECommentsGrid.SelectedResponse = null;
    BOECommentsGrid.FocusedTextArea = null;

    BOECommentsGrid.ShowAddNewComment = function () {
        $(this).children('td').removeClass('selected');
        // Clone new comment row
        var newCommentRow = $('#BOECommentsGrid-AddedRowToClone').clone(true);

        // Remove the clone row id from the new row
        newCommentRow.attr('id', '');
        newCommentRow.attr('new', 'true');

        // Insert the new row into the table
        newCommentRow.insertBefore('#BOECommentsGrid-NewCommentRow');

        // Show the new row
        newCommentRow.show();

        // Call the click event on the new comment to set focus on the text field
        newCommentRow.find('.editable-comment').click();

        $(newCommentRow).children('td:first-child').addClass('selected');
    };

    BOECommentsGrid.HideNewCommentRow = function () {
        $('#BOECommentsGrid-AddedRowToClone').hide();

        refreshModule($('.boe-comments.module'));
    };

    BOECommentsGrid.HideAllExistingCommentInputs = function () {
        $('.boe-comments tbody textarea').parent().hide();

        refreshModule($('.boe-comments.module'));
    };

    BOECommentsGrid.ShowEditComment = function () {
        if (BOECommentsGrid.FocusedTextArea == null)
        {
            var cellTextArea = $(this).find('textarea');
            BOECommentsGrid.EditingComment = cellTextArea.val();
            $(this).find('.editable-comment').hide();
            $(this).find('.editable-response').hide();
            cellTextArea.parent().show();
            BOECommentsGrid.FocusedTextArea = cellTextArea;
            cellTextArea.focus();
            cellTextArea.val(cellTextArea.val());

            refreshModule($('.boe-comments.module'));
        }
    };

    BOECommentsGrid.ExistingCommentFocusOut = function () {
        // If the trimmed entered comment length is zero
        if ($.trim($(this).val()).length == 0) {
            // If the trimmed entered comment is blank and this is a saved comment
            if ($(this).closest('td').find('div.editable-comment').attr('existing') != undefined) {
                // Show the static comment text and hide the text box
                $(this).closest('td').find('div.editable-comment').show();
                $(this).parent().hide();
                // Reset the comment to its previous text
                $(this).val(BOECommentsGrid.EditingComment);
                // Alert the user that saved comments cannot be deleted
                Session.alertDialog('Invalid Comment', 'Saved comments cannot be deleted and must not be left blank.');
            }
                // If the trimmed comment is blank and this is a new comment
            else {
                // Remove the new blank comment
                $(this).closest('tr').remove();
            }
        }
            // If the trimmed comment is not empty
        else {
            // IF the comment has indeed changed, then we'll mark it updated
            if ($.trim($(this).val()) != $.trim(BOECommentsGrid.EditingComment)) {
                // Set the comment to updated for save purposes
                $(this).closest('td').find('div.editable-comment').attr('updated', true);
                BOECommentsGrid.setDirty();
            }
            // Set the static comment text to the user entered text from the input and show it
            $(this).closest('td').find('div.editable-comment').show().html($(this).val().replace(/\n/g, '<br/>'));
            // Hide the input element
            $(this).parent().hide();
        }

        BOECommentsGrid.FocusedTextArea = null;
        refreshModule($('.boe-comments.module'));
    };

    BOECommentsGrid.ExistingResponseFocusOut = function () {
        // IF the comment has indeed changed, then we'll mark it updated
        if ($.trim($(this).val()) != $.trim(BOECommentsGrid.EditingComment)) {
            // Set the comment to updated for save purposes
            $(this).closest('td').find('div.editable-response').attr('updated', true);
            BOECommentsGrid.setDirty();
        }
        // Set the static comment text to the user entered text from the input and show it
        $(this).closest('td').find('div.editable-response').show().html($(this).val().replace(/\n/g, '<br/>'));
        // Hide the input element
        $(this).parent().hide();

        BOECommentsGrid.FocusedTextArea = null;
        refreshModule($('.boe-comments.module'));
    };

    BOECommentsGrid.UnfocusFocusedTextAreas = function () {
        // If a text area is currently being edited, we'll unfocus it and save its value
        if (BOECommentsGrid.FocusedTextArea != null) {
            BOECommentsGrid.FocusedTextArea.focusout();
        }
    }

    BOECommentsGrid.ApprovalRadioClick = function () {
        // Force the selection; Common Dialog workaround        
        $(this).prop('checked', true);

        if($(this).val() == 0) {  // if Reject
            $('#BOECommentsGrid-NewCommentRow').removeClass('display-none');
            BOECommentsGrid.SelectedResponse = $(this).val();

            // check: if workspace is Locked, then give Approver option to unlock the BOE for edit
            if (workspaceLocked) {
                $('.approve-reject > .boe-full-edit').show();
            }
        }
        else {  // if Approve
            if (BOECommentsGrid.SelectedResponse == 0 && $('tr[new=true]').length) {
                BOECommentsGrid.SelectResponse(BOECommentsGrid.SelectedResponse);
                var body="Comments can only be entered when rejecting a BOE. Changing your response to Approve will remove all new comments.<br /><br />Are you sure you want to change the response to Approve?"
                Session.confirmDialog("Change response from Reject to Approve", body, BOECommentsGrid.RejectToAcceptDialog_Yes, null)
            }
            else {
                BOECommentsGrid.ChooseAccept();
                BOECommentsGrid.SelectedResponse = $(this).val();
            }
        }
        BOECommentsGrid.setDirty();
        refreshModule($('.boe-comments.module'));
    };

    BOECommentsGrid.CancelButtonClick = function () {
        if (BOECommentsGrid.isDirty()) {
            Session.confirmDialog(
                "Cancel",
                "Are you sure you want to cancel all changes?",
                function() {
                    BOECommentsGrid.cleanDirty();
                    $(document).trigger(reloadCommentGridEvent);
                },
                null);
        }
        else {
            $(document).trigger(reloadCommentGridEvent);
        }
    };

    BOECommentsGrid.SaveButtonClick = function () {
        BOECommentsGrid.UnfocusFocusedTextAreas();

        $('#BOECommentsSave').addClass('display-none');
        $('#BOECommentsLoader').removeClass('display-none');

        // Call Save to gather updated textareas and save their comments
        BOECommentsGrid.SaveComments();
    };

    BOECommentsGrid.SaveComments = function () {

        BOECommentsGrid.data = {};
        BOECommentsGrid.data.Approver = null;
        BOECommentsGrid.data.Comments = [];

        $.each($('div[updated*=true]'), function () {
            var newComment = {};

            if ($(this).attr('fkid') != undefined) {

                newComment.ReviewerCommentID = $(this).attr('fkid');
                newComment.AuthorResponse = $(this).closest('td').find('textarea').val();

                if ($(this).attr('pkid') != undefined) {
                    newComment.AuthorResponseID = $(this).attr('pkid');
                    newComment.AuthorResponseUpdateDTLong = $(this).attr('updateDT');
                }
                else {
                    newComment.AuthorResponseID = -1;
                }
            }
            else if ($(this).attr('pkid') != undefined) {
                newComment.ReviewerCommentID = $(this).attr('pkid');
                newComment.ReviewerComment = $(this).closest('td').find('textarea').val();
                newComment.ReviewerCommentUpdateDTLong = $(this).attr('updateDT');
            }
            else {
                newComment.ReviewerCommentID = -1;
                newComment.ReviewerComment = $(this).closest('td').find('textarea').val(); ;
            }

            BOECommentsGrid.data.Comments.push(newComment);
        });
        
        if (!BOECommentsGrid.ApprovalsReadOnly && BOECommentsGrid.CurrentUserApproverInfo != null)
        {
            if (BOECommentsGrid.SelectedResponse != null && BOECommentsGrid.SelectedResponse != BOECommentsGrid.CurrentUserApproverInfo.ApproverResponse) {
                BOECommentsGrid.CurrentUserApproverInfo.ApproverResponse = BOECommentsGrid.SelectedResponse;
                BOECommentsGrid.CurrentUserApproverInfo.ApproverResponded = true;
                BOECommentsGrid.data.Approver = BOECommentsGrid.CurrentUserApproverInfo;
            }
        }

        if (BOECommentsGrid.data.Comments.length || BOECommentsGrid.data.Approver != null) {

            var dataToSend = JSON.stringify(BOECommentsGrid.data);
            BOECommentsGrid.ajaxRequest({
                type: 'POST',
                url: saveBOECommentsUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function () {
                    BOECommentsGrid.cleanDirty();

                    if (BOECommentsGrid.data.Approver != null) {
                        $(window.location).attr('href', window.location.protocol + '//' + window.location.host + "/" + workspace);
                    }
                    else {
                        $(document).trigger(reloadCommentGridEvent);
                        $(document).trigger(reloadHistoryGridEvent);
                    }
                },
                error: function() {
                    BOECommentsGrid.CurrentUserApproverInfo = JSON.cloneData(originalApproverInfo);
                    $('#BOECommentsLoader').addClass('display-none');
                    $('#BOECommentsSave').removeClass('display-none');
                }
            });
        }
        else {
            $('#BOECommentsLoader').addClass('display-none');
            $('#BOECommentsSave').removeClass('display-none');
        }
    };

    BOECommentsGrid.TextAreaMaxLength = function (object, maxLength) {
        return (object.value.length <= maxLength);
    };

    BOECommentsGrid.RejectToAcceptDialog_Yes = function() {
        BOECommentsGrid.SelectedResponse = 1;
        BOECommentsGrid.SelectResponse(BOECommentsGrid.SelectedResponse);
    };

    BOECommentsGrid.ChooseAccept = function() {
        $('#BOECommentsGrid-NewCommentRow').addClass('display-none');
        $('tr[new=true]').remove();
    };

    BOECommentsGrid.SelectResponse = function(response) {
        $('.boe-comments .approve-reject').children('input[value="' + response + '"]').click();
    };

    BOECommentsGrid.EnableComments = function() {
        $('#BOECommentsGrid-NewCommentRow').removeClass('display-none');
        $('#BOECommentsGrid-NewCommentRow').click(BOECommentsGrid.ShowAddNewComment);
        $('.editable-comment').closest('td').click(BOECommentsGrid.ShowEditComment);
        $('.editable-comment').each(function () { $(this).next().children(':first').focusout(BOECommentsGrid.ExistingCommentFocusOut); });
    };

    BOECommentsGrid.EnableResponses = function() {
        $('.editable-response').closest('td').click(BOECommentsGrid.ShowEditComment);
        $('.editable-response').each(function () { $(this).next().children(':first').focusout(BOECommentsGrid.ExistingResponseFocusOut); });
            
        $('.response-header-new').removeClass('display-none');
    };

    BOECommentsGrid.EnableApprovals = function() {
        $('#BOECommentsGrid-NewCommentRow').addClass('display-none');

        $('input[name=approval]').click(BOECommentsGrid.ApprovalRadioClick);
            
        if (BOECommentsGrid.CurrentUserApproverInfo != null) {
            BOECommentsGrid.SelectResponse(BOECommentsGrid.CurrentUserApproverInfo.ApproverResponse);
        }
    };

    return BOECommentsGrid;
}

function AfterDomLoadBOECommentsGridWidget(BOECommentsGrid) {
    createModule($('.boe-comments.module'));

    BOECommentsGrid.afterDOMLoad();
    BOECommentsGrid.registerForLiveEvent('focusin', '.boe-comments td', function () {
        $(this).addClass('selected');
    });

    BOECommentsGrid.registerForLiveEvent('focusout', '.boe-comments td', function () {
        $(this).removeClass('selected');
    });

    BOECommentsGrid.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { BOECommentsGrid.cleanDirty(); });

    BOECommentsGrid.HideAllExistingCommentInputs();
    BOECommentsGrid.HideNewCommentRow();

    if (!BOECommentsGrid.CommentsReadOnly) {
        BOECommentsGrid.EnableComments();
    }

    if (!BOECommentsGrid.ResponsesReadOnly) {
        BOECommentsGrid.EnableResponses();
    }

    if (!BOECommentsGrid.CommentsReadOnly || !BOECommentsGrid.ResponsesReadOnly || !BOECommentsGrid.ApprovalsReadOnly) {
        BOECommentsGrid.registerForLiveEvent('mousedown', '#BOECommentsSave:not(.disabled)', BOECommentsGrid.SaveButtonClick);
        $('#BOECommentsCancel').mousedown(BOECommentsGrid.CancelButtonClick);
    }
    else {
        //we need to kill the mouse down just incase the live event was ever previously loaded.  
        //However if the parent is hidden i don't see why we would need to wire the button up in this conditional.
        $('#BOECommentsSave:not(.disabled)').off('mousedown');
        $('#BOECommentsSave').closest('.buttons').hide();
    }

    if (!BOECommentsGrid.ApprovalsReadOnly) {
        BOECommentsGrid.EnableComments();
        BOECommentsGrid.EnableApprovals();
    }

    if (BOECommentsGrid.ContainsOCI == true) {
        $('.oci-note').html('<b>Note:</b> Must not contain any classified, export controlled or third party proprietary information.');
    }
    else {
        $('.oci-note').html('<b>Note:</b> Must not contain any OCI, classified, export controlled or third party proprietary information.');
    }

    refreshModule($('.boe-comments.module'));
}

function CreateRteTemplate(useQuestions, numberQuestions) {
    return {
        UseQuestions: useQuestions,
        NumberQuestions: numberQuestions,
        OriginalAnswers: []
    };
}

function InitializeRteTemplate(template, templateName, rteFieldSize) {
    // capture original values as well as initialize
    for (var i = 0; i < template.NumberQuestions; i++) {
        template.OriginalAnswers = [];
        template.OriginalAnswers.push($('#' + templateName + '_' + i.toString()).val());
        InitializeRTE(templateName + '_' + i.toString(), { maxlen: rteFieldSize, enableCharCounting: true }, BoeHeaderWidget);
    }
}

function ResetRteTemplate(template, templateName) {
    if (tinyMCE.get(templateName + '_0')) {
        for (var i = 0; i < template.NumberQuestions; i++) {
            var editor = tinyMCE.get(templateName + '_' + i.toString());
            editor.setContent(template.OriginalAnswers[i]);

            $('#' + templateName + '_' + i.toString()).val(template.OriginalAnswers[i]);
        }
    }
}

function GetRteTemplateJson(template, templateName, data) {
    var editor = tinyMCE.get(templateName + '_0')
    if (editor) {
        if (template.UseQuestions) {
            data[templateName] = '_'; // set it since it is required
            // set the RTE template answers
            for (var i = 0; i < template.NumberQuestions; i++) {
                editor = tinyMCE.get(templateName + '_' + i.toString());
                var element = $('#' + templateName + '_' + i.toString());
                data.RteTemplateAnswers.push({
                    Id: element.attr('pkid'),
                    QuestionId: element.attr('questionId'),
                    BoeId: element.attr('boeId'),
                    SourceId: element.attr('sourceId'),
                    AnswerText: editor.getContent(),
                    UpdateDateLong: element.attr('updateDateLong'),
                    TaskId: element.attr('taskId'),
                    Required: element.attr('isrequired')
                });
            }
        } else {
            data[templateName] = editor.getContent();
        }
    }
}

function HandleRTETemplateDataForReadOnly(template, templateName) {
    for (var i = 0; i < template.NumberQuestions; i++) {
        HandleRTEDataForReadOnly('#' + templateName + '_' + i.toString(), '.replacedWidgetText');
    }
}

/**
 *  Mainly used by MOQ RTE field where MOQ View is read-only but the RTE field should not be so it has to be reset to editable
 * @param {any} template
 * @param {any} templateName
 * @param {any} section
 */
function RemoveRTETemplateReadOnly(template, templateName, section) {
    if (template !== undefined) {
        for (var i = 0; i < template.NumberQuestions; i++) {
            var name = '#' + templateName + '_' + i.toString();

            var textElement = $(name, section);
            $('.replacedWidgetText', textElement.parent()).addClass('display-none');
            textElement.removeClass('display-none');
        }
    }
}
