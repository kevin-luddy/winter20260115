/*
This javascript file is called from BOE ODC ascx files
*/

function InitializeODCTypesWidget(ODCTypesWidget, spreadCurvesLoad, summaryGridHoursUpdatedEvent, elementOfCostODC, spreadCurvesDiscreteCost, getPerformingOrgByNameUrl) {
    ODCTypesWidget.getLTTypeCost = function (inLaborTypeID)
    {
        var cost = $("table#ODCTypes tr[pkid='" + inLaborTypeID + "']").find("input[name=Cost]").val();
        return cost;
    }

    /*
    <summary>Helper method to create a LTDefine need for request to the updateLT event</summary>
    <param>input that changed</param>
    */
    ODCTypesWidget.createODCDefine = function (input) {
        var elementID = $(input).parents('tr').attr('pkid');
        var name = $(input).attr("name").replace(blankElementReservedWord, "");
        var value;

        if (name == "StartDate" || name == "EndDate") {
            value = new Date($(input).val().replace("/", "/15/"));
            if (!Helper.isValidDate(value)) {
                value = null;
            }
        } else if (name == "ResourceDescription") {
            value = $(input).find(":selected").attr("resourcename");
            name = "ResourceID";
        } else if (name == "Cost") {
            value = $(input).val();
        } else if (name == "ODCSpreadCurveID") {
            value = $(input).find(":selected").val();
        } else if (name == "PerformingOrg") {
            name = "PerformingOrgID";
            value = $(input).val();
        }  else {
            value = $(input).find(":selected").text().trim();
        }

        return {
            ODCTypeID: elementID,
            Name: name,
            Value: value
        }
    };

    ODCTypesWidget.preparedForSubmit = function () {

        return true;
    };


    ODCTypesWidget.SetPerformingOrgIdByName =function (input, jsonIndex){
        ODCTypesWidget.waitingBeforeSubmit=true;
        var name = $(input).val();
        $(input).attr("PerformingOrgID", null)
        var formBeingSaved = $(input).parents("form")[0];
        var valBox = $("ul.validation-box", formBeingSaved);
        $.ajax({
            type: 'POST',
            url: getPerformingOrgByNameUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({Name:name}),
            success: function( returned ) {
                var perfOrgId = returned.Status;
                $(input).siblings("input[name=PerformingOrgID]").val(perfOrgId);
                ODCTypesWidget.data[jsonIndex].PerformingOrgID = perfOrgId;
             
                ODCTypesWidget.waitingBeforeSubmit=false;
                if (perfOrgId == 0 || perfOrgId == null){
                    $(valBox).find("li[name=PerformingOrg]").remove();
                    valBox.append("<li name='PerformingOrg'>Performing Org is invalid</li>");
                    $(valBox).show();
                    $(valBox).fadeIn(500);
                    ODCTypesWidget.focusValidation();
                }
                else{
                    $(valBox).find("li[name=PerformingOrg]").remove();
                    if ($(valBox).find("li").length == 0) {
                        $(valBox).hide();
                    }
                }
                ODCTypesWidget.refreshModule();
            }
        });
    }

    /*
     <summary>Disable colums if necessary</summary>
     <param>input that changed</param>
    */
    ODCTypesWidget.disableHoursInput = function (input) {
        if ($(input).val() == spreadCurvesDiscreteCost) {
            $(input).parents('tr').find("input.Cost").prop("disabled", true);
            //somethign about reseting the default text
        } else {
            $(input).parents('tr').find("input.Cost").prop("disabled", false);
        }
    };

    ODCTypesWidget.refreshModule = function () {
        ODCTypesWidget.applyReadOnly();
        refreshModule($('#ODCTypes').parents('.module'));
    };
    
    ODCTypesWidget.PopulateDropdowns = function(element)
    {
        var row = $(element).parents('tr');

        var descriptionElement = row.find('select[name="ResourceDescription"]');

        var descriptionValue = descriptionElement.val();

        var descriptionOptions = '<option></option>';
        for (var index in ODCTypesWidget.resources) {
            descriptionOptions += '<option resourcename="' +ODCTypesWidget.resources[index].ResourceCode + '" value="' + ODCTypesWidget.resources[index].ResourceDescription +
                                           '" ltid="'+ ODCTypesWidget.resources[index].BOESummaryText + '">' + ODCTypesWidget.resources[index].ResourceDescription + '</option>';
        }
        descriptionElement.html(descriptionOptions);
        descriptionElement.val(descriptionValue);
    }
     
    ODCTypesWidget.calculateTimeFrame = function(input) {
        if($(input).parents("tr").find("input[name=StartDate]").val()!="" || $(input).parents("tr").find("input[name=StartDate]").val()!= null){
            
            var StartDate = $(input).parents("tr").find("input[name=StartDate]").val().toDate();
            var NumberOfMonths=  $(input).parents("tr").find("input[name=NumberOfMonths]").val();
            var EndDateElement = $(input).parents("tr").find("input[name=EndDate]");
            var EndDate = EndDateElement.val().toDate();

            if(StartDate.toFormattedString() == '12/1969') {
                $('#ODCTypesForm .validation-box').html('<li>ODC Type Start date is invalid.</li>').show();
            }
            else if(EndDate.toFormattedString() == '12/1969') {
                $('#ODCTypesForm .validation-box').html('<li>ODC Type End date is invalid.</li>').show();
            }
            else {
                $('#ODCTypesForm .validation-box').html('').hide();
            }

            if($(input).attr("name")=="NumberOfMonths"){
                var thisEndDate=StartDate;
                thisEndDate.addMonths(parseInt(NumberOfMonths)-1);             
                EndDateElement.val(thisEndDate.toFormattedString());
                EndDateElement.siblings('.default-text').addClass('display-none');
                EndDateElement.trigger("change");
            }else if($(input).attr("name")=="EndDate"){
                $(input).parents("tr").find("input[name=NumberOfMonths]").val(StartDate.getMonthsBetween(EndDate)+1);
            }else if($(input).attr("name")=="StartDate"){
                var thisEndDate=StartDate;
                if(NumberOfMonths != "" || NumberOfMonths != null){
                    thisEndDate.addMonths(parseInt(NumberOfMonths)-1);
                    EndDateElement.val(thisEndDate.toFormattedString());
                    EndDateElement.siblings('.default-text').addClass('display-none');
                    EndDateElement.trigger("change");
                }                
            }
        }
    }

    ODCTypesWidget.deleteRecordConfirm = function(obj) {
        Session.confirmDialog("Delete ODC Type", "Are you sure you want to delete the selected ODC Type?", function () {ODCTypesWidget.deleteRecord(obj);}, null);
    };

    ODCTypesWidget.PerfOrgSelect = function(selectedorg, id, perforg){
        var perfOrgIdInput = $(selectedorg).siblings("input[name=PerformingOrgID]");
        $(selectedorg).removeClass('placeholder');
        $(perfOrgIdInput).attr("perforg", perforg)
        $(perfOrgIdInput).val(id);
        $(selectedorg).change();

    };
}

function AfterDomLoadODCTypesWidget(ODCTypesWidget, exportODCTypeUrl, ocdElementId) {

    ODCTypesWidget.afterDOMLoad();
    createModule($('.odc-types.module'));

    if (ODCTypesWidget.isReadOnly()) {
        $('div.popup-div-button.for-perf-orgs').removeClass('attached-down-arrow-button');
    }

    $('#ODCTypes tbody tr').each(function () {
        ODCTypesWidget.PopulateDropdowns($(this).children());
        // select resource code
        if (!$(this).hasClass('blank')) {
            var selectElement2 = $(this).find('select[name="ResourceDescription"]');
            selectElement2.val(selectElement2.attr('orgval'));
            ODCTypesWidget.calculateTimeFrame($(this).find("input[name=EndDate]"));
            $(this).find("input[name=NumberOfMonths]").attr("numOfMonths", $(this).find("input[name=NumberOfMonths]").val());
        }
    });

    $("#ManageODCType-Export").click(function () {
        // Remove the old hidden iFrame, if it exists
        $('#ManageODCType-Export-DownloadTarget').remove();

        if (ocdElementId == '') {
            ocdElementId = -1;
        }
        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'ManageODCType-Export-DownloadTarget',
            'class': 'display-none',
            'src': exportODCTypeUrl + odcElementId
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    });

    ODCTypesWidget.registerForLiveEvent('change', '#ODCTypes tr select[name="ResourceDescription"]', function () {
        $(this).parents('tr').find('select[name="resource-description"]').attr("orgval", "");
        ODCTypesWidget.PopulateDropdowns($(this));
    });

    ODCTypesWidget.registerForLiveEvent('focusout', '#ODCTypes tr input[name="StartDate"], #ODCTypes tr input[name="EndDate"], #ODCTypes tr input[name="NumberOfMonths"]', function () {
        ODCTypesWidget.calculateTimeFrame($(this));
    });


    ODCTypesWidget.registerForLiveEvent('focusin', '.odc-types-grid td', function () {
        if (!$(this).children('div').hasClass('delete')) {
            $(this).addClass('selected');
        }
    });

    ODCTypesWidget.registerForLiveEvent('focusout', '.odc-types-grid td', function () {
        $(this).removeClass('selected');
        var perfOrgValue = $(this).find('input.select-perforg-autocomplete').val();
        //get the performing org id on a valid type ahead (i.e, do not process an empty perf org entry)
        if ($(this).hasClass('performing-org') && perfOrgValue != '') {
            ODCTypesWidget.updateData($(this).find('input'));
        }
    });

    //Wiring Events
    ODCTypesWidget.registerForLiveEvent('click', 'td.resources, td.spread-curve', function () {
        $(this).children('select').focus();
    });

    ODCTypesWidget.registerForLiveEvent('click', 'td.start-date, td.end-date, td.performing-org, td.text', function () {
        $(this).children('input').focus();
    });

    ODCTypesWidget.registerForLiveEvent('focusin', 'td.start-date, td.end-date, td.performing-org, td.text, td.resources, td.spread-curve', function () {
        $(this).children('.default-text').addClass('display-none');
    });

    ODCTypesWidget.registerForLiveEvent('focusout', 'td.spread-curve', function () {
        $(this).parents().children('td.cost').children('.default-text').addClass('display-none');
    });


    $("input.select-perforg-autocomplete").autocomplete({
        source: BOEDetails.autocompletePerformingOrgs,
        autoFocus: true,
        minLength: 2,
        delay: 400
    }).on('blur', function (e) {
        var that = $(this);
        var perfOrgIdInput = that.siblings("input[name=PerformingOrgID]");
        if (that.val() != perfOrgIdInput.attr("perforg")) {
            if ($('.ui-autocomplete li:visible').length > 0) {
                var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                that.val(item.value);
                ODCTypesWidget.PerfOrgSelect(this, item.ID, item.value);
            }
            else {
                ODCTypesWidget.PerfOrgSelect(this, 0, "");
            }
        }
    });

    ODCTypesWidget.registerForEvent('ODCTotalUpdated', function (e, LTData) {
        var HS = $("table#ODCTypes tr[pkid='" + LTData.ltid + "']").find("input[name=Cost]");
        $(HS).val(LTData.total);
        $(HS).trigger("change");
    });

    //new row is added
    ODCTypesWidget.registerForEvent('addedableODCTypes_NEW_ROW_CREATED', function () {
        ODCTypesWidget.newItemCount--;

        $("input.select-perforg-autocomplete").autocomplete({
            source: BOEDetails.autocompletePerformingOrgs,
            autoFocus: true,
            minLength: 2,
            delay: 400
        }).on('blur', function (e) {
            var that = $(this);
            var perfOrgIdInput = that.siblings("input[name=PerformingOrgID]");
            if (that.val() != perfOrgIdInput.attr("perforg")) {
                if ($('.ui-autocomplete li:visible').length > 0) {
                    var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                    that.val(item.value);
                    ODCTypesWidget.PerfOrgSelect(this, item.ID, item.value);
                }
                else {
                    ODCTypesWidget.PerfOrgSelect(this, 0, "");
                }
            }
        });

        $('.addedableODCTypes .addable .blank').attr("pkid", ODCTypesWidget.newItemCount);

        ODCTypesWidget.PopulateDropdowns($('#ODCTypes tbody tr.blank').children());
        //ODCTypesWidget.setupPerformingOrgComboBox($("#ODCTypes tr:last"))

        ODCTypesWidget.refreshModule();
    });


    //add negative pkid to new row.
    $('.addedableODCTypes .addable .blank').attr("pkid", ODCTypesWidget.newItemCount);

    //disable hours for distinct rows
    $("table#ODCTypes tr").each(function (index, value) {
        ODCTypesWidget.disableHoursInput($(value).find(".ODCSpreadCurveID"));
    });

    ODCTypesWidget.applyReadOnly();
    refreshModule($('.odc-types.module'));
    CollapsibleModule($('.odc-types.module'));

    ODCTypesWidget.registerForEvent("AllODCWidgetLoaded", function (e) {
        AddableGrid("addedableODCTypes");
        ODCTypesWidget.refreshModule();
    });

    ODCTypesWidget.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { ODCTypesWidget.cleanDirty(); });

    if (!ODCTypesWidget.isReadOnly()) {
        $('.select-perforg.popup-div', ODCTypesWidget.context).on('click', '.perforg-option', function () {
            var cell = ODCTypesWidget.PerfOrgPopupButton.closest('td');
            var option = $(this);

            var select = cell.find('input.select-perforg-autocomplete');
            select.val(option.attr('perforgname'));
            var perfOrgIdInput;
            if (cell.closest('tr').hasClass('blank')) {
                perfOrgIdInput = select.siblings("input[name=PerformingOrgID-blank]");
            } else {
                perfOrgIdInput = select.siblings("input[name=PerformingOrgID]");
            }
            select.removeClass('placeholder');
            var selectedPerfOrg = option.attr("perforgid");
            $(perfOrgIdInput).val(selectedPerfOrg);
            ODCTypesWidget.updateData(select);
            ODCTypesWidget.clearValidationBox($("#ODCTypesForm ul.validation-box"));
        });

        var options = {
            propagateOnClick: true,
            button: 'div.popup-div-button.for-perf-orgs',
            onShow: function (button) { ODCTypesWidget.PerfOrgPopupButton = $(button); },
            leftOffSet: -105
        };
        $('div.popup-div.select-perforg').genPopUp(options);
    }

    $(document).trigger('ODCWidgetLoaded', "ODCTypes");

    SortableGrid('.ODCTypeGrid');

    if (!ODCTypesWidget.isReadOnly()) {
        var html = "";

        $.each(BOEDetails.WSPerfOrgs, function (i, item) {
            html += '<div class="perforg-option" perforgid="' + item.PerformingOrgID + '" perforgname="' + item.PerformingOrgName + '" perforgdesc="' + item.PerformingOrgDesc + '">' + item.PerformingOrgName + '-' + item.PerformingOrgDesc + '</div>';
        });
        ODCTypesWidget.getElement("div.select-perforg.popup-div").html(html);
    }
}

function InitializeODCElementsCompositeWidget(summaryGridReloadEvent) {
    var ODCElementsComposite = new Widget("ODCElementsComposite", true);

    ODCElementsComposite.isDirty = function () {
        return (ODCTypesWidget.isDirty() ||
               ODCSpreadWidget.isDirty() ||
               ODCElementDetails.isDirty());
    };

    ODCElementsComposite.cleanAllDirty = function () {
        ODCTypesWidget.cleanDirty();
        ODCSpreadWidget.cleanDirty();
        ODCElementDetails.cleanDirty();
        this.cleanDirty();
    }

    ODCElementsComposite.CancelToMainGrid = function () {

        if (ODCElementsComposite.isDirty) {
            Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function () {
                        ODCElementsComposite.cleanAllDirty();
                        $("#BOESummary").html('<div class="loader"></div>');
                        $(document).trigger(summaryGridReloadEvent);
                        window.location.hash = 'ODC';
                    },
                    null);
        }
        else {
            $("#BOESummary").html('<div class="loader"></div>');
            $(document).trigger(summaryGridReloadEvent);
            window.location.hash = 'ODC';
        }
    }

    return ODCElementsComposite;
}

function AfterDomLoadODCElementsCompositeWidget(ODCElementsComposite, ODC_ContainsOCI) {
    $("#Cancel-ODCUpdates").click(ODCElementsComposite.CancelToMainGrid);

    ODCElementsComposite.widgetsLoaded=0;

    ODCElementsComposite.registerForEvent("ODCWidgetLoaded", function(e, meta){
        ODCElementsComposite.widgetsLoaded++;
        if(ODCElementsComposite.widgetsLoaded == 3)
        {
            Widget.applyValidation();
            $(document).trigger("AllODCWidgetLoaded");
        }
    });
      
    //Change the OCI note based off the workspace
    if (ODC_ContainsOCI == true){
        $('#ODC-OCINote').html('Must not contain any classified, export controlled or third party proprietary information.');
    }
    else {
        $('#ODC-OCINote').html('Must not contain any OCI, classified, export controlled or third party proprietary information.');
    }

    refreshModule($('.odc-details .module'));
}

function InitializeBOEOtherDirectCostWidget(BOEOtherDirectCostWidget_ReadOnly, loadDuplicateODCTaskDialogUrl, saveReOrderODCTaskElementsUrl) {
    var BOEOtherDirectCostWidget = new GridWidget('BOEOdc', '', BOEOtherDirectCostWidget_ReadOnly);

    //Display Duplicate Task Element Dialog
    BOEOtherDirectCostWidget.displayDuplicateOdcTaskElementDialog = function () {
        $("#DuplicateOdcTaskElementDialog").dialog({ width: 700, modal: true, resizable: false, draggable: true, title: 'Duplicate Task Elements', autoOpen: false, close: BOEOtherDirectCostWidget.closeDuplicateOdcTaskElementDialog });

        $('#DuplicateOdcTask').addClass('display-none');
        $('#DuplicateOdcTaskElementDialog-Loader').removeClass('display-none');

        $.ajax({
            type: 'GET',
            url: loadDuplicateODCTaskDialogUrl,
            success: function (response) {
                $('#DuplicateOdcTaskElementDialog').html(response).dialog('open');
                DuplicateTaskWidget.AfterSave = function () { $(document).trigger('LOAD_ODC_ELEMENT_GRID'); }
                $('#DuplicateOdcTask').removeClass('display-none');
                $('#DuplicateOdcTaskElementDialog-Loader').addClass('display-none');
            },
            error: function () {
                $('#DuplicateOdcTask').removeClass('display-none');
                $('#DuplicateOdcTaskElementDialog-Loader').addClass('display-none');
            }
        });

    };

    //Close the duplicate task dialog this way so there will never be multiple instances of it existing across task tabs
    BOEOtherDirectCostWidget.closeDuplicateOdcTaskElementDialog = function () {
        $('#DuplicateOdcTaskElementDialog').dialog('destroy').empty();
    }

    BOEOtherDirectCostWidget.ReOrderODCTaskElementDialog = {};
    BOEOtherDirectCostWidget.ReOrderODCTaskElementDialog.Element = $("#ReOrderODCTaskElementDialog");
    BOEOtherDirectCostWidget.ReOrderODCTaskElementDialog.Params = { width: 700, height: 365, modal: true, resizable: false, draggable: true, title: 'Sort Task Elements' };

    BOEOtherDirectCostWidget.displayReOrderODCTaskElementDialog = function () {
        BOEOtherDirectCostWidget.OpenDialogAfterInitialize(BOEOtherDirectCostWidget.ReOrderODCTaskElementDialog);

    }
    $('#ReOrderODCTaskElementDialog #TasksMoveItemsUp').click(function () {
        BOEOtherDirectCostWidget.ShiftSelectedItemsUp($('#ReOrderODCTaskElementDialog #TaskElementsList'));
        BOEOtherDirectCostWidget.EnableSave();

    });

    $('#ReOrderODCTaskElementDialog #TasksMoveItemsDown').click(function () {
        BOEOtherDirectCostWidget.ShiftSelectedItemsDown($('#ReOrderODCTaskElementDialog #TaskElementsList'));
        BOEOtherDirectCostWidget.EnableSave();

    });

    BOEOtherDirectCostWidget.CancelReOrderTaskElement = function () {
        BOEOtherDirectCostWidget.CloseDialog(BOEOtherDirectCostWidget.ReOrderODCTaskElementDialog);
        $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Loader').addClass('display-none');
        $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Save').addClass('disabled');
        $('#ReOrderODCTaskElementDialog').addClass('display-none');
        $(document).trigger('LOAD_ODC_ELEMENT_GRID');
    };

    BOEOtherDirectCostWidget.EnableSave = function () {
        $('#ReOrderODCTaskElementDialog-Save').removeClass('disabled');
    };

    BOEOtherDirectCostWidget.SaveReOrderTaskElements = function () {
        $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Save').addClass('display-none');
        $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Loader').removeClass('display-none');
        var dataToSend = {};

        var listOrder = 0;
        dataToSend.BOETaskElements = [];
        $('#ReOrderODCTaskElementDialog #TaskElementsList').children('option').each(function () {
            var taskToSave = {};
            var taskValue = $(this).val();

            taskToSave.TaskID = taskValue;
            taskToSave.ListOrder = listOrder++;
            dataToSend.BOETaskElements.push(taskToSave);
        });

        listOrder = 0;

        dataToSend = JSON.stringify(dataToSend);
        BOEOtherDirectCostWidget.ajaxRequest({
            type: 'POST',
            url: saveReOrderODCTaskElementsUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {
                BOEOtherDirectCostWidget.CloseDialog(BOEOtherDirectCostWidget.ReOrderODCTaskElementDialog);
                $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Save').removeClass('display-none');
                $('#ReOrderODCTaskElementDialog').addClass('display-none');
                $(document).trigger('LOAD_ODC_ELEMENT_GRID');
            },
            error: function () {
                $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderODCTaskElementDialog #ReOrderODCTaskElementDialog-Save').removeClass('display-none');
            }
        });


    };

    BOEOtherDirectCostWidget.ShiftSelectedItemsDown = function (list) {
        $($(list).children('option:selected').get().reverse()).each(function () {
            if ($(this).next().length == 0) {
                return false;
            }

            $(this).next().after($(this));
        });
    };
    // Moves selected list items up in the list
    BOEOtherDirectCostWidget.ShiftSelectedItemsUp = function (list) {
        $(list).children('option:selected').each(function () {
            if ($(this).prev().length == 0) {
                return false;
            }

            $(this).prev().before($(this));
        });
    };

    BOEOtherDirectCostWidget.expandODCElement = function (ODCID) {
        window.location.hash = 'ODC/odc/' + ODCID;
    }

    BOEOtherDirectCostWidget.addNewODCElement = function () {
        window.location.hash = 'ODC/odc/';
    }

    BOEOtherDirectCostWidget.removeDeletedItems = function (input) {

        $(input).parents('tr').fadeOut(400, function () {
            $(input).parents('tr').remove();
        });
    }

    return BOEOtherDirectCostWidget;
}

function AfterDomLoadBOEOtherDirectCostWidget(BOEOtherDirectCostWidget, summaryGridReloadEvent) {
    BOEOtherDirectCostWidget.InitializeDialog(BOEOtherDirectCostWidget.ReOrderODCTaskElementDialog);

    BOEOtherDirectCostWidget.registerForLiveEvent('click', '#boeODC-AddODCElement:not(.disabled)', BOEOtherDirectCostWidget.addNewODCElement);

    $("button[name='add-odc-element-button']").addClass('display-none');
    $('#BOEOtherDirectCostGridContainer .buttons').addClass('display-none');
    $('#BOEOtherDirectCostGrid .delete').addClass('display-none');
    $('#BOEOtherDirectCostGrid .deleteColumn').addClass('display-none');
    
    SortableGrid('.boe-ODC');

    createModule($('.boe-ODC.module'));
    refreshModule($('.boe-ODC.module'));
}

function InitializeODCElementDetailsWidget(boeId, updateDateLong) {
    var ODCElementDetails = new Widget("ODCElementDetails", true);

    ODCElementDetails.preparedForSubmit = function () {

        console.log('entering ODCElementDetails.preparedForSubmit');
   
        ODCElementDetails.data.BOEID = boeId;
        ODCElementDetails.data.ODCID = $("#ODCID").val();
        ODCElementDetails.data.UpdateDateLong = updateDateLong;
        ODCElementDetails.data.TaskID = $(".odc-element-details :input[name=TaskID]").val();
        ODCElementDetails.data.TaskTitle = $(".odc-element-details :input[name=ODCTaskTitle]").val();

        ODCElementDetails.data.ODCTaskDescription = $(".odc-element-details :input[name=ODCTaskDescription]").val();
        ODCElementDetails.data.ODCMOQText = $(".odc-element-details :input[name=ODCMOQText]").val();
        // explicitly re-extract rich-text field values from tinyMCE editor
        if(tinyMCE.EditorManager.editors.ODCTaskDescription){
            ODCElementDetails.data.ODCTaskDescription = tinyMCE.EditorManager.editors.ODCTaskDescription.getContent();
        }
        if(tinyMCE.EditorManager.editors.ODCMOQText){
            ODCElementDetails.data.ODCMOQText = tinyMCE.EditorManager.editors.ODCMOQText.getContent();
        }

        ODCElementDetails.data.StartDate = $(".odc-element-details :input[name=ODCStartDate]").val();
        ODCElementDetails.data.EndDate = $(".odc-element-details :input[name=ODCEndDate]").val();

        console.log('exiting ODCElementDetails.preparedForSubmit');

        return true;
    };

    ODCElementDetails.registerForEvent("ODCDatesAdjusted", function(e, Dates){
        $(".odc-element-details").find("#TaskStartDate").html(Dates.StartDate);
        $(".odc-element-details").find("#TaskEndDate").html(Dates.EndDate);
        $(".odc-element-details").find("#ODCStartDateLabel").show();
        $(".odc-element-details").find("#ODCEndDateLabel").show();
    });

    return ODCElementDetails;
}

function AfterDomLoadODCElementDetailsWidget(ODCElementDetails, boeStateIsDraft) {
    ODCElementDetails.afterDOMLoad();

    createModule($('.odc-element-details.module'));

    ODCElementDetails.applyReadOnly();

    ODCElementDetails.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { ODCElementDetails.cleanDirty(); });

    refreshModule($('.odc-element-details.module'));

    CollapsibleModule($('.odc-element-details.module'));

    
    $(":input[name=ODCStartDate]").prop('disabled', true);
    $(":input[name=ODCEndDate]").prop('disabled', true);
    $('.ui-datepicker-trigger').addClass('display-none');

    ODCElementDetails.registerForEvent("AllODCWidgetLoaded", function (e) {
        ODCElementDetails.refreshModule();
    });
    $(document).trigger('ODCWidgetLoaded', "ODCDetails");

    $(document).trigger("ODCDatesAdjusted", { StartDate: $(".odc-element-details :input[name=ODCStartDate]").val(), EndDate: $(".odc-element-details :input[name=ODCEndDate]").val() });

    
    HandleRTEDataForReadOnly("#ODCTaskDescription", ".replacedWidgetText");
    HandleRTEDataForReadOnly("#ODCMOQText", ".replacedWidgetText");
}

function InitializeManageODCSpreadWidget(odcElementId, displayOdcSpreadGridUrl, exportODCSpreadUrl) {
    var ManageODCSpread = new Widget('ManageODCSpread', true);

    ManageODCSpread.Module = {};
    
    ManageODCSpread.Initialize = function () {
        ManageODCSpread.Module = $('#ManageODCSpread');
        
        ManageODCSpread.ODCElementID = odcElementId;
    };

    ManageODCSpread.BindEvents = function () {
        $('#ManageODCSpread-Export, #ManageODCSpread-ExportLink').click(ManageODCSpread.ConfirmExport);

        // Triggered Events
        ManageODCSpread.unregisterForEvent('ReloadODCSpreadGrid');
        ManageODCSpread.registerForEvent('ReloadODCSpreadGrid', ManageODCSpread.ReloadGrid);
    };    

    ManageODCSpread.ReloadGrid = function () {
        $.ajax({
            type: 'POST',
            url: displayOdcSpreadGridUrl,
            dataType: 'html',
            success: function (response) {
                $('#ODCSpreadGridContent').html(response);
                refreshModule(ManageODCSpread.Module);
            }
        });
    };

    ManageODCSpread.ConfirmExport = function () {
        if (ManageODCSpread.isDirty()) {
            Session.confirmDialog(
            'Export Resource Spreads',
            'You have unsaved work that will not be included in this export. Would you like to export anyway?',
            ManageODCSpread.Export,
            null);
        }
        else {
            ManageODCSpread.Export();
        }
    };

    ManageODCSpread.Export = function () {
        if (ManageODCSpread.ODCElementID == '') {
            ManageODCSpread.ODCElementID = '-1';
        }
        // Remove the old hidden iFrame, if it exists
        $('#ManageODCSpread-DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'ManageODCSpread-DownloadTarget',
            'class': 'display-none',
            'src': exportODCSpreadUrl
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    };
    return ManageODCSpread;
}

function AfterDomLoadManageODCSpreadWidget(ManageODCSpread){
    ManageODCSpread.Initialize();
    ManageODCSpread.BindEvents();

    createModule(ManageODCSpread.Module);
    refreshModule(ManageODCSpread.Module);
    CollapsibleModule(ManageODCSpread.Module);
    ManageODCSpread.ReloadGrid();
}

function InitializeODCSpreadWidget(ODC_SpreadCurves, odcJson, spreadCurvesLoad) {
    var ODCSpreadWidget = new GridWidget("ODCSpreadWidgetContainer", null, true);

    //this is a hack because the use of enums is not consistent.  The Controller automatically binds as the number while our code was using the enum String value.  To fix this quickly we check for both.
    ODCSpreadWidget.SpreadCurves = ODC_SpreadCurves;


    //starting row of grid
    ODCSpreadWidget.earlistStartDate = null;
    //end row of grid
    ODCSpreadWidget.latestEndDate = null;
    //object for data that is incomplete.
    ODCSpreadWidget.partialData = new Array();
    //probably don't need can you tbody instead

    ODCSpreadWidget.data = odcJson;

    ODCSpreadWidget.MOQTotal = null;

    //when data is received from the LSJSON view data it needs to be converted to the string representation of the enum.


    ODCSpreadWidget.prepCompleteSpreadJsonData = function () {
        //prepdata dates and convert ODCTypeID to String
        for (var x in ODCSpreadWidget.data) {
            ODCSpreadWidget.data[x].Cost = ODCSpreadWidget.data[x].Cost.toString();

            ODCSpreadWidget.data[x].StartDate = Helper.ConvertJSONDateToJSDate(ODCSpreadWidget.data[x].StartDate).MonthPrecision();
            ODCSpreadWidget.data[x].EndDate = Helper.ConvertJSONDateToJSDate(ODCSpreadWidget.data[x].EndDate).MonthPrecision();

            ODCSpreadWidget.data[x].ODCSpreadCurveID += "";

            for (y in ODCSpreadWidget.SpreadCurves) {
                if (ODCSpreadWidget.SpreadCurves[y].id == "" + ODCSpreadWidget.data[x].ODCSpreadCurveID) {
                    ODCSpreadWidget.data[x].ODCSpreadCurveID = ODCSpreadWidget.SpreadCurves[y].name;
                }
            }
            ODCSpreadWidget.prepSpreadArrayJsonData(ODCSpreadWidget.data[x].ODCSpreads);
        }
    };

    ODCSpreadWidget.prepSpreadArrayJsonData = function (SpreadArray) {
        for (var y in SpreadArray) {
            if (SpreadArray[y].ODCSpreadDate != null) {
                SpreadArray[y].ODCSpreadDate = Helper.ConvertJSONDateToJSDate(SpreadArray[y].ODCSpreadDate).MonthPrecision();
                SpreadArray[y].CostSpreadValue = SpreadArray[y].CostSpreadValue + "";
            } else {
                delete SpreadArray[y];
            }
        }
    };


    /*
       Summary: Sets up a Col in the table with the correct input boxes and default values.
       Param LTDetails: LS to add :ODCSpreadDetailModelView
       Param colToInsert: The spot to insert the Col, if none is given it will insert it in the last row.
    */
    ODCSpreadWidget.addNewLaborType = function (LTDetails, colToInsertInto) {
        //remove total cols
        $("table#ODCSpread tr .totalcol").remove();
        //create rows.
        ODCSpreadWidget.createRows(LTDetails);
        //Create columns.
        ODCSpreadWidget.createCols(LTDetails, colToInsertInto);
        //fill cells.
        ODCSpreadWidget.fillCells(LTDetails, colToInsertInto);

        //calculate totals
        ODCSpreadWidget.calculateTotals();

        if (LTDetails.ODCSpreadCurveID != ODCSpreadWidget.SpreadCurves[0].id && LTDetails.ODCSpreadCurveID != ODCSpreadWidget.SpreadCurves[0].name) {

            ODCSpreadWidget.lockDownCol(LTDetails.ODCTypeID, true);
        } else if (ODCSpreadWidget.isInitialized) {

            var ltid = LTDetails.ODCTypeID;
            //calculate total for each LT
            var ltTotal = $("table#ODCSpread thead tr.totals th.totals[ltid='" + ltid + "']").html();
            //tell LT that the total changed.

            $(document).trigger('ODCTotalUpdated',
            {
                ltid: ltid,
                total: ltTotal
            });

        }

        ODCSpreadWidget.hideUnusedRows();
    };

    ODCSpreadWidget.preparedForSubmit = function () {
        return true;
    };

    /*
       Summary: Adds rows if nessacary as defined by a new LT.
       Param: LTDetails LS to add :ODCSpreadDetailModelView
    */
    ODCSpreadWidget.createRows = function (LTDetails) {
        var amountOfColumns = ($("table#ODCSpread tr:first th").length - 1);
        if (ODCSpreadWidget.earlistStartDate == null && ODCSpreadWidget.latestEndDate == null) {
            var x;
            for (x = new Date(LTDetails.StartDate) ; x <= LTDetails.EndDate; x.addMonths(1)) {
                $("table#ODCSpread tbody:last").append("<tr date='" + x.toFormattedString() + "'> <td>" + x.toFormattedString() + "</td></tr>");

                for (var y = 0; y < amountOfColumns; y++) {
                    $("table#ODCSpread tbody tr:last").append("<td></td>");
                }
            }
            ODCSpreadWidget.earlistStartDate = LTDetails.StartDate;
            ODCSpreadWidget.latestEndDate = LTDetails.EndDate;
        }
        else {
            LTDetails.StartDate = LTDetails.StartDate.MonthPrecision();
            if (LTDetails.StartDate < ODCSpreadWidget.earlistStartDate) {
                //copy date into the loop iterator.
                var x = new Date(ODCSpreadWidget.earlistStartDate.MonthPrecision());

                //if table is empty, add first row
                if ($("table#ODCSpread tbody tr:eq(0)").length == 0) {
                    $("table#ODCSpread tbody:last").append("<tr date='" + ODCSpreadWidget.earlistStartDate.MonthPrecision().toFormattedString() + "'> <td>" + ODCSpreadWidget.earlistStartDate.MonthPrecision().toFormattedString() + "</td></tr>");

                    for (var y = 0; y < amountOfColumns; y++) {
                        $("table#ODCSpread tbody tr:last").append("<td></td>");
                    }
                }

                //remove one month from it.
                x.addMonths(-1);
                for (x; x >= LTDetails.StartDate; x.addMonths(-1)) {
                    $("table#ODCSpread tbody tr:eq(0)").before("<tr date='" + x.toFormattedString() + "'> <td>" + x.toFormattedString() + "</td></tr>");
                    for (var y = 0; y < amountOfColumns; y++) {
                        $("table#ODCSpread tbody tr:eq(0)").append("<td></td>");
                    }
                }
                ODCSpreadWidget.earlistStartDate = LTDetails.StartDate;
            }

            var x = new Date(ODCSpreadWidget.latestEndDate);
            //remove one month from it.
            x.addMonths(1);

            if (LTDetails.EndDate > ODCSpreadWidget.latestEndDate) {
                for (x ; x <= LTDetails.EndDate; x.addMonths(1)) {
                    $("table#ODCSpread tbody:first").append("<tr date='" + x.toFormattedString() + "'> <td>" + x.toFormattedString() + "</td></tr>");
                    for (var y = 0; y < amountOfColumns; y++) {
                        $("table#ODCSpread tbody tr:last").append("<td></td>");
                    }
                }
                ODCSpreadWidget.latestEndDate = LTDetails.EndDate;
            }
        }

        //$(document).trigger("ODCDatesAdjusted" ,{StartDate:ODCSpreadWidget.convertDateToString(ODCSpreadWidget.earlistStartDate),EndDate:ODCSpreadWidget.convertDateToString(ODCSpreadWidget.latestEndDate)})
        refreshModule($('#ODCSpread').parents('.module'));
    };

    ODCSpreadWidget.convertDateToString = function (Date) {
        return (Date.getMonth() + 1) + "/" + Date.getFullYear();
    };

    //used to clean up any spread date
    ODCSpreadWidget.cleanUpSpreadDatesOutsideLTDateRange = function (DetailedLSObject) {
        for (var t = 0; DetailedLSObject.ODCSpreads.length > t; t++) {
            if (DetailedLSObject.StartDate > DetailedLSObject.ODCSpreads[t].ODCSpreadDate || DetailedLSObject.EndDate < DetailedLSObject.ODCSpreads[t].ODCSpreadDate) {
                DetailedLSObject.ODCSpreads.splice(t, 1);
                t--;
            }
        }
    };


    ODCSpreadWidget.hideUnusedRows = function () {
        $("#ODCSpread").find("tbody tr").each(function () {
            if ($(this).find(".LSDateinput").length < 1) {
                $(this).hide();
            } else {
                $(this).show();
            }
        })

        refreshModule($('#ODCSpread').parents('.module'));
    };

    /*
       Summary: Creates a Col in the LS table
       Param: LTDetails LS to add :ODCSpreadDetailModelView
       Param: colToInsert The spot to insert the Col, if none is given it will insert it in the last row.
    */
    ODCSpreadWidget.createCols = function (LTDetails, colToInsertInto) {
        if (colToInsertInto == null) {
            //if no colToInsert is given add it to the end of the table.
            colToInsertInto = $("table#ODCSpread thead tr:eq(1) th").length - 1;
        } else {
            colToInsertInto--;
        }
        $("table#ODCSpread thead tr:eq(0) th:eq(" + colToInsertInto + ")").after("<th class='ODCID ResourceID' ltid='" + LTDetails.ODCTypeID + "'>" + LTDetails.ResourceID + "</th>");
        $("table#ODCSpread thead tr:eq(1) th:eq(" + colToInsertInto + ")").after("<th class='PerformingOrgID' ltid='" + LTDetails.ODCTypeID + "'>" + LTDetails.PerformingOrgID + "</th>");
        $("table#ODCSpread thead tr:eq(2) th:eq(" + colToInsertInto + ")").after("<th class='totals' ltid='" + LTDetails.ODCTypeID + "'></th>");
        $("table#ODCSpread tbody tr").find("td:eq(" + colToInsertInto + ")").after("<td></td>");
    };

    /*
       Summary: Sets up a Col in the table with the correct input boxes and default values.
       Param LTDetails: LS to add :ODCSpreadDetailModelView
       Param colToInsert: The spot to insert the Col, if none is given it will insert it in the last row.
    */
    ODCSpreadWidget.fillCells = function (LTDetails, colToInsertInto) {
        if (colToInsertInto == null) {
            //if no colToInsert is given add it to the end of the table, however calculate this based on the Thead since if this is the first entry nothing will be in the tbody.
            colToInsertInto = $("table#ODCSpread thead tr:eq(1) th").length - 1;
        }

        for (x = 0; x < $("table#ODCSpread tbody tr").length; x++) {
            var rowDate = $("table#ODCSpread tbody tr:eq(" + x + ")").attr("date").toDate();
            //if inbetween the start and end date add inputs if we have a value add it too.
            if (LTDetails.StartDate <= rowDate && LTDetails.EndDate >= rowDate) {

                var hours = "0";

                if (LTDetails.ODCSpreadCurveID == spreadCurvesLoad) {
                    if (LTDetails.Cost != undefined) {
                        hours = LTDetails.Cost;
                    }
                    else {
                        hours = 0;
                    }
                } else {
                    for (y = 0; y < LTDetails.ODCSpreads.length; y++) {
                        if (rowDate.toString() == LTDetails.ODCSpreads[y].ODCSpreadDate.toFormattedString().toDate().toString() && LTDetails.ODCSpreads[y].Deleted != true) {
                            hours = LTDetails.ODCSpreads[y].CostSpreadValue;
                            break;
                        }
                    }
                }
                $("table#ODCSpread tbody tr:eq(" + x + ") td:eq(" + colToInsertInto + ")").append("<input name='spreadValueInput' id='spreadValueInput" + x + "' class='LSDateinput' type='text' ltid='" + LTDetails.ODCTypeID + "' value='" + hours + "' onchange='ODCSpreadWidget.updateData(this)'></input>");
            }
        }
    };

    /*
        Summary: removes date rows if unused.
    */
    ODCSpreadWidget.readjustDates = function () {
        var newEarlistStartDate = null;
        var newLatestEndDate = null;

        if (ODCSpreadWidget.data.length > 0) {
            //default to first set of data
            newEarlistStartDate = ODCSpreadWidget.data[0].StartDate;
            newLatestEndDate = ODCSpreadWidget.data[0].EndDate;
            for (var x = 1; x < ODCSpreadWidget.data.length; x++) {
                if (ODCSpreadWidget.data[x].StartDate < newEarlistStartDate) {
                    newEarlistStartDate = ODCSpreadWidget.data[x].StartDate;
                }
                if (ODCSpreadWidget.data[x].EndDate > newLatestEndDate) {
                    newLatestEndDate = ODCSpreadWidget.data[x].EndDate;
                }
            }


            //only set this if this function removed dates.
            if (ODCSpreadWidget.earlistStartDate < newEarlistStartDate) {
                ODCSpreadWidget.earlistStartDate = newEarlistStartDate;
            }
            if (ODCSpreadWidget.latestEndDate > newLatestEndDate) {
                ODCSpreadWidget.latestEndDate = newLatestEndDate;
            }

        } else {
            $("table#ODCSpread tbody tr").remove();
            ODCSpreadWidget.earlistStartDate = null;
            ODCSpreadWidget.latestEndDate = null;
        }


        //remove from table
        $("table#ODCSpread tbody tr").each(function (index, value) {
            var rowDate = $(value).attr("date").toDate();
            //if inbetween the start and end date add inputs if we have a value add it too.
            if (rowDate < ODCSpreadWidget.earlistStartDate || rowDate > ODCSpreadWidget.latestEndDate) {
                $(value).remove();
            }
        });

        refreshModule($('#ODCSpread').parents('.module'));
    };

    /*
        Summary: locks down a col
        param: ltid
        param: to lockdown or not too.
    */
    ODCSpreadWidget.lockDownCol = function (ltid, lockDown) {
        var col = $("#ODCSpread th.ODCID[ltid=" + ltid + "]").index();
        if (lockDown) {
            $("table#ODCSpread tbody tr").find("td:eq(" + col + ") :input").prop("disabled", true);
        } else {
            $("table#ODCSpread tbody tr").find("td:eq(" + col + ") :input").prop("disabled", false);
        }
    };

    /*
        Summary:Calculates Totals for the entire table.
    */
    ODCSpreadWidget.calculateTotals = function () {
        //remove total cols
        $("table#ODCSpread tr .totalcol").remove();

        //calculate total for each LT
        $("table#ODCSpread thead tr.totals th.totals").each(function (index, value) {
            total = 0;
            $("table#ODCSpread tbody").find('input[ltid=' + $(value).attr('ltid') + ']').each(function (rowindex, inputValue) {
                var inputValueFloat = parseFloat($(inputValue).val(), 10);
                if (!isNaN(inputValueFloat)) {
                    //avoid rounding errors and maintain 2 decimal digits of precision;
                    total = Math.round((total + inputValueFloat) * 100) / 100;
                }
            });

            $(value).text(Helper.roundAwayFromZero(total * 100) / 100);
        });

        //created totalcol again.
        $("table#ODCSpread tbody tr").append("<td class='totalcol'></td>");
        $("table#ODCSpread thead tr:eq(0)").append("<th class='totalcol last-child' rowspan='2'>Total by Months</th>");
        $("table#ODCSpread thead tr:eq(3)").append("<th class='totalcol totalcoltotal' ></th>");

        //calculate total cols.
        $("table#ODCSpread tbody tr").each(function (index, value) {
            var total = 0;
            $("table#ODCSpread tbody tr:eq(" + index + ") td :input").each(function (index2, cellValue) {
                cellValue = parseFloat($(cellValue).val(), 10);
                if (!isNaN(cellValue)) {
                    //avoid rounding errors and maintain 2 decimal digits of precision;
                    total = Math.round((total + cellValue) * 100) / 100;
                }
            });
            $("table#ODCSpread tbody tr td.totalcol:eq(" + index + ")").html(Helper.roundAwayFromZero(total * 100) / 100);
        });

        //calculate totals for totals col.
        $("th#ODCTotalTotalCol").remove();
        var total = 0;
        $("table#ODCSpread tbody td.totalcol").each(function (rowindex, rowTotal) {
            var rowTotalFloat = parseFloat($(rowTotal).html(), 10);
            if (!isNaN(rowTotalFloat)) {
                //avoid rounding errors and maintain 2 decimal digits of precision;
                total = Math.round((total + rowTotalFloat) * 100) / 100;
            }
        });
        $("table#ODCSpread thead tr.totals").append("<th class='last-child' id='ODCTotalTotalCol'></th>");
        $("th#ODCTotalTotalCol").html(Helper.roundAwayFromZero(total * 100) / 100);
    };

    // The ODC Types Discrete cost should always be the same as the ODC spreads totals
    ODCSpreadWidget.getDiscreteTotalCostPerODCType = function (odcTypeID) {
        var ltTotal = $("table#ODCSpread thead tr.totals th.totals[ltid='" + odcTypeID + "']").html();
        return ltTotal;
    };

    return ODCSpreadWidget;
}

function AfterDomLoadODCSpreadWidget(ODCSpreadWidget) {
    ODCSpreadWidget.afterDOMLoad();
    ODCSpreadWidget.prepCompleteSpreadJsonData();
    ODCSpreadWidget.isInitialized = false;
    ODCSpreadWidget.registerForLiveEvent('focusin', '#ODCSpread td', function () {
        if (!$(this).children('div').hasClass('delete')) {
            $(this).addClass('selected');
        }
    });

    ODCSpreadWidget.registerForLiveEvent('focusout', '#ODCSpread td', function () {
        $(this).removeClass('selected');
    });

    //Populate initial data.
    for (var x in ODCSpreadWidget.data) {
        ODCSpreadWidget.addNewLaborType(ODCSpreadWidget.data[x]);
    }

    ODCSpreadWidget.isInitialized = true;

    ODCSpreadWidget.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { ODCSpreadWidget.cleanDirty(); });

    ODCSpreadWidget.registerForDelegateEvent('keyup', 'td', function (event) {
        var parent = $(this).parent();
        var columnNum = parent.children().index($(this));
        var nextElement;
        if (event.keyCode == '13' || event.keyCode == '40') {
            $(this).focusout();
            nextElement = $(parent.next().find('td')[columnNum]).find('input');
            nextElement.focus();
        } else if (event.keyCode == '38') {
            $(this).focusout();
            nextElement = $(parent.prev().find('td')[columnNum]).find('input');
            nextElement.focus();
        }

    });

    ODCSpreadWidget.applyReadOnly();
    $(document).trigger('ODCWidgetLoaded', "ODCSpreads");
}