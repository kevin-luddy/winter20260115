/*
This javascript file is called from BOE Zone Travel ascx files
*/

function InitializeBOEZoneTravelWidget(readOnly) {
    var BOEZoneTravelWidget = new GridWidget('BOEZONETRAVEL', '', readOnly);

    //Display Duplicate Task Element Dialog
    BOEZoneTravelWidget.displayDuplicateZoneTravelTaskElementDialog = function () {
        $('#DuplicateZoneTravelTask').addClass('display-none');
        $('#DuplicateZoneTravelTaskElementDialog-Loader').removeClass('display-none');

        $("#DuplicateZoneTravelTaskElementDialog").dialog({width: 700, modal: true, resizable: false, draggable: true, title : 'Duplicate Task Elements', autoOpen: false, close: BOEZoneTravelWidget.closeDuplicateZoneTravelTaskElementDialog });

        $.ajax({
            type: 'GET',
            url: loadDuplicateZoneTravelTaskDialogUrl,
            success: function (response) {
                $('#DuplicateZoneTravelTaskElementDialog').html(response).dialog('open');
                DuplicateTaskWidget.AfterSave = function () { $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID'); }
                $('#DuplicateZoneTravelTask').removeClass('display-none');
                $('#DuplicateZoneTravelTaskElementDialog-Loader').addClass('display-none');
            },
            error : function () {
                $('#DuplicateZoneTravelTask').removeClass('display-none');
                $('#DuplicateZoneTravelTaskElementDialog-Loader').addClass('display-none');
            }
        });
    };

    //Close the duplicate task dialog this way so there will never be multiple instances of it existing across task tabs
    BOEZoneTravelWidget.closeDuplicateZoneTravelTaskElementDialog = function () {
        $('#DuplicateZoneTravelTaskElementDialog').dialog('destroy').empty();
    }

    BOEZoneTravelWidget.ReOrderZoneTravelTaskElementDialog = {};
    BOEZoneTravelWidget.ReOrderZoneTravelTaskElementDialog.Element = $("#ReOrderZoneTravelTaskElementDialog");
    BOEZoneTravelWidget.ReOrderZoneTravelTaskElementDialog.Params = { width: 700, height: 365, modal: true, resizable: false, draggable: true, title: 'Sort Task Elements' };

    BOEZoneTravelWidget.displayReOrderZoneTravelTaskElementDialog = function () {
        BOEZoneTravelWidget.OpenDialogAfterInitialize(BOEZoneTravelWidget.ReOrderZoneTravelTaskElementDialog);
    }

    $('#ReOrderZoneTravelTaskElementDialog #TasksMoveItemsUp').click(function () {
        BOEZoneTravelWidget.ShiftSelectedItemsUp($('#ReOrderZoneTravelTaskElementDialog #TaskElementsList'));
        BOEZoneTravelWidget.EnableSave();

    });

    $('#ReOrderZoneTravelTaskElementDialog #TasksMoveItemsDown').click(function () {
        BOEZoneTravelWidget.ShiftSelectedItemsDown($('#ReOrderZoneTravelTaskElementDialog #TaskElementsList'));
        BOEZoneTravelWidget.EnableSave();

    });

    BOEZoneTravelWidget.CancelReOrderTaskElement = function () {
        BOEZoneTravelWidget.CloseDialog(BOEZoneTravelWidget.ReOrderZoneTravelTaskElementDialog);
        $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Loader').addClass('display-none');
        $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Save').addClass('disabled');
        $('#ReOrderZoneTravelTaskElementDialog').addClass('display-none');
        $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
    };


    BOEZoneTravelWidget.EnableSave = function () {
        $('#ReOrderZoneTravelTaskElementDialog-Save').removeClass('disabled');
    };

    BOEZoneTravelWidget.SaveReOrderTaskElements = function () {
        $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Save').addClass('display-none');
        $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Loader').removeClass('display-none');
        var dataToSend = {};

        var listOrder = 0;
        dataToSend.BOETaskElements = [];
        $('#ReOrderZoneTravelTaskElementDialog #TaskElementsList').children('option').each(function () {
            var taskToSave = {};
            var taskValue = $(this).val();

            taskToSave.TaskID = taskValue;
            taskToSave.ListOrder = listOrder++;
            dataToSend.BOETaskElements.push(taskToSave);
        });

        listOrder = 0;

        dataToSend = JSON.stringify(dataToSend);
        BOEZoneTravelWidget.ajaxRequest({
            type: 'POST',
            url: saveReorderZoneTravelTaskElementsUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {
                BOEZoneTravelWidget.CloseDialog(BOEZoneTravelWidget.ReOrderZoneTravelTaskElementDialog);
                $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Save').removeClass('display-none');
                $('#ReOrderZoneTravelTaskElementDialog').addClass('display-none');
                $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
            },
            error: function () {
                $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderZoneTravelTaskElementDialog #ReOrderZoneTravelTaskElementDialog-Save').removeClass('display-none');
            }
        });
    };

    BOEZoneTravelWidget.ShiftSelectedItemsDown = function (list) {
        $($(list).children('option:selected').get().reverse()).each(function () {
            if ($(this).next().length == 0) {
                return false;
            }

            $(this).next().after($(this));
        });
    };
    // Moves selected list items up in the list
    BOEZoneTravelWidget.ShiftSelectedItemsUp = function (list) {
        $(list).children('option:selected').each(function () {
            if ($(this).prev().length == 0) {
                return false;
            }

            $(this).prev().before($(this));
        });
    };

    BOEZoneTravelWidget.removeDeletedItems = function (TRAVELID) {
        $(this).find('tr').attr("pkid").fadeOut(400, function () {
            $(this).find('tr').attr("pkid").remove();
        });
    }

    BOEZoneTravelWidget.expandZoneTravelElement = function (ZONETRAVELID) {
        window.location.hash = 'Travel/travel/' + ZONETRAVELID;
    }

    BOEZoneTravelWidget.addNewZoneTravelElement = function () {
        window.location.hash = 'Travel/travel/';
    }

    return BOEZoneTravelWidget;
}

function AfterDomLoadBOEZoneTravelWidget(BOEZoneTravelWidget, deleteAllBOEZoneTravelUrl, boeSummaryGridReloadEvent, deleteBOEZoneTravelUrl) {
    BOEZoneTravelWidget.InitializeDialog(BOEZoneTravelWidget.ReOrderZoneTravelTaskElementDialog);

    BOEZoneTravelWidget.registerForLiveEvent('click', '#boe-AddZoneTravelElement:not(.disabled)', BOEZoneTravelWidget.addNewZoneTravelElement);

    $("#BOEZONETRAVELGridHeader .delete").click(function () {
        Session.confirmDialog("Delete All Travel Elements", "Are you sure you want to delete all Travel Elements?", function () {

            var dataToSend = JSON.stringify(BOEZoneTravelWidget.data);

            $.ajax({
                type: 'POST',
                url: deleteAllBOEZoneTravelUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function (response) {
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger(boeSummaryGridReloadEvent); // recalculate our summary table
                }
            });
        });
    });

    $("#BOEZONETRAVELGridBody .delete").click(function () {
        var input = this;
        Session.confirmDialog("Delete Travel Element", "Are you sure you want to delete the selected Travel Element?", function () {

            deletedTask = {};
            deletedTask.TravelID = $(input).parents('tr').find('.hidden-zone-travel-id').val();
            
            BOEZoneTravelWidget.data = deletedTask;

            var dataToSend = JSON.stringify(BOEZoneTravelWidget.data);

            $.ajax({
                type: 'POST',
                url: deleteBOEZoneTravelUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function (response) {
                    $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
                    $(document).trigger(boeSummaryGridReloadEvent); // recalculate our summary table
                }
            });
        });
    });

    $("#BOEZONETRAVELGrid .travelTotalCost").each(function () {
        $(this).text(Helper.addCommas($(this).text().trim(), "$"));
    });

    if (BOEZoneTravelWidget.isReadOnly()) {
        $('#MSTZoneTravelGridContainer .buttons').addClass('display-none');
        $('#BOEZONETRAVELGrid .delete').addClass('display-none');
        $('#BOEZONETRAVELGrid .deleteColumn').addClass('display-none');
    }

    createModule($('.boe-ZONETRAVEL.module'));
    SortableGrid('.ZoneTravelGrid');
    refreshModule($('.boe-ZONETRAVEL.module'));
};

function InitializeZoneTravelElementDetailsWidget(ZoneTravelElementDetails_ReadOnly, boeId, zoneTravelId, zoneTravelUpdateDateLong) {
    var ZoneTravelElementDetails = new Widget("ZoneTravelElementDetailsFormContainer", ZoneTravelElementDetails_ReadOnly);

    ZoneTravelElementDetails.preparedForSubmit = function () {
        ZoneTravelElementDetails.data.BOEID = boeId;
        ZoneTravelElementDetails.data.TravelID = zoneTravelId;
        ZoneTravelElementDetails.data.UpdateDateLong = zoneTravelUpdateDateLong;
        ZoneTravelElementDetails.data.TaskID = $(".zone-travel-element-details :input[name=TaskID]").val();
        ZoneTravelElementDetails.data.TaskTitle = $(".zone-travel-element-details :input[name=ZoneTravelTaskTitle]").val();
        ZoneTravelElementDetails.data.CustomFieldValues = [];

        
        $("#ZoneTravelElementDetailsForm :input.customField").each(function () {
            if ($(this).attr("openended") === "true") {
                ZoneTravelElementDetails.data.CustomFieldValues.push({
                    IsOpenEnded: true,
                    SelectionID: $(this).attr("selectionid"),
                    CustomFieldValueID: $(this).attr("customfieldvalueid"),
                    UpdateDateLong: $(this).attr("UpdateDateLong"),
                    OpenEndedValue: $(this).val(),
                    CustomFieldID: $(this).attr("customfieldid")
                });
            } else {
                if ($(this).val() == "" && $(this).attr("selectionid") != "-1") {
                    //if the field value was deleted
                    ZoneTravelElementDetails.data.CustomFieldValues.push({
                        SelectionID: $(this).attr("selectionid"),
                        UpdateDateLong: $(this).attr("UpdateDateLong")
                    });
                } else if ($(this).val() != "" && $(this).attr("selectionid") != "-1") {
                    //if the field value was updated
                    ZoneTravelElementDetails.data.CustomFieldValues.push({
                        SelectionID: $(this).attr("selectionid"),
                        CustomFieldValueID: $(this).val(),
                        UpdateDateLong: $(this).attr("UpdateDateLong")
                    });
                } else if ($(this).val() != "" && $(this).attr("selectionid") == "-1") {
                    //new field value
                    ZoneTravelElementDetails.data.CustomFieldValues.push({ CustomFieldValueID: $(this).val() });
                } //else is no change
            }
        });
        ZoneTravelElementDetails.data.TravelTaskDescription = $(".zone-travel-element-details :input[name=ZoneTravelTaskDescription]").val();
        
        // explicitly re-extract rich-text field values from tinyMCE editor
        if (tinyMCE.EditorManager.editors.TravelTaskDescription) {
            ZoneTravelElementDetails.data.TravelTaskDescription = tinyMCE.EditorManager.editors.ZoneTravelTaskDescription.getContent();
        }

        ZoneTravelElementDetails.data.StartDate = $(".zone-travel-element-details :input[name=ZoneTravelStartDate]").val();
        ZoneTravelElementDetails.data.EndDate = $(".zone-travel-element-details :input[name=ZoneTravelEndDate]").val();
        return true;
    };

    ZoneTravelElementDetails.registerForEvent("ZoneTravelDatesAdjusted", function (e, Dates) {
        $(".zone-travel-element-details").find("#TaskStartDate").html(Dates.StartDate);
        $(".zone-travel-element-details").find("#TaskEndDate").html(Dates.EndDate);
        $(".zone-travel-element-details").find("#TaskStartDateLabel").show();
        $(".zone-travel-element-details").find("#TaskEndDateLabel").show();
    });

    ZoneTravelElementDetails.refreshModule = function () {
        refreshModule($('.zone-travel-element-details.module'))
    };

    return ZoneTravelElementDetails;
}

function AfterDomLoadZoneTravelElementDetailsWidget(ZoneTravelElementDetails, boeStateIsDraft, WorkspaceState, rteFieldSize) {
    ZoneTravelElementDetails.afterDOMLoad();
    var module = $('.zone-travel-element-details.module');
    createModule(module);

    if (!ZoneTravelElementDetails_ReadOnly &&
        (WorkspaceState != "Locked" ||
        (WorkspaceState == "Locked" && boeStateIsDraft)))  // check BOE state (widget is read-only, but BOE is editable in Draft)
    {
        module.find(":input[name=ZoneTravelStartDate]").prop('disabled', false);
        module.find(":input[name=ZoneTravelEndDate]").prop('disabled', false);
        module.find('.ui-datepicker-trigger').removeClass('display-none');

        module.find(":input[name=ZoneTravelStartDate], :input[name=ZoneTravelEndDate]", $("#ZoneTravelElementDetailsForm")).keydown(function () {
            $("#Travel" + $(this).attr("name")).html($(this).val());
        });

        $("#ZoneTravelElementDetailsForm #ZoneTravelStartDate").datepicker();
        $("#ZoneTravelElementDetailsForm :input[name=ZoneTravelEndDate]").datepicker();

        module.find(":input[name=ZoneTravelStartDate], :input[name=ZoneTravelEndDate]", $("#ZoneTravelElementDetailsForm")).datepicker({
            onSelect: function (dateText, inst) {
                var element = $(this);
                var input = (element == undefined) ? $(inst.input) : element;
                input.keydown();
                input.blur();
                input.siblings('.default-text').addClass('display-none');
                input.addClass('active');
            }
        });

    }
    else {
        module.find(":input[name=ZoneTravelStartDate]").prop('disabled', true);
        module.find(":input[name=ZoneTravelEndDate]").prop('disabled', true);
        module.find('.ui-datepicker-trigger').addClass('display-none');
    }

    if (WorkspaceState === "Locked" && !boeStateIsDraft) {
        $('#TaskElementCustomFieldID select').prop('disabled', 'disabled');
        $('#TaskElementCustomFieldID input').prop('disabled', 'disabled');
    }

    ZoneTravelElementDetails.applyReadOnly();

    ZoneTravelElementDetails.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { ZoneTravelElementDetails.cleanDirty(); });

    refreshModule($('.zone-travel-element-details.module'));

    CollapsibleModule($('.zone-travel-element-details.module'));

    ZoneTravelElementDetails.registerForEvent("AllZoneTravelWidgetLoaded", function (e) {
        ZoneTravelElementDetails.refreshModule();
    });

    $(document).trigger('ZoneTravelWidgetLoaded', "ZoneTravelTrips");

    $(document).trigger("ZoneTravelDatesAdjusted", { StartDate: $("#ZoneTravelElementDetailsForm :input[name=ZoneTravelStartDate]").val(), EndDate: $("#ZoneTravelElementDetailsForm :input[name=ZoneTravelEndDate]").val() })

    if (!ZoneTravelElementDetails.isReadOnly()) {
        InitializeRTE('ZoneTravelTaskDescription', { maxlen: rteFieldSize, enableCharCounting: true }, ZoneTravelElementDetails);
    }
    else {
        HandleRTEDataForReadOnly("#ZoneTravelTaskDescription", ".replacedWidgetText");
    }
};

function InitializeZoneTravelElementsCompositeWidget(ZoneTravelElementsComposite_ReadOnly, containsOCI, boeSummaryGridReloadEvent,
        saveZoneTravelDetailsCompositeUrl) {
    var ZoneTravelElementsComposite = new Widget("ZoneTravelElementsComposite", ZoneTravelElementsComposite_ReadOnly);

    ZoneTravelElementsComposite.ContainsOCI = containsOCI;

    ZoneTravelElementsComposite.UpdateFields = function () {
        $("#Save-ZoneTravelUpdates").removeClass("display-none");
        $("#Loader-ZoneTravelUpdates").addClass("display-none");
        $(document).trigger(boeSummaryGridReloadEvent);
        window.location.hash = 'Travel';
        ZoneTravelElementsComposite.cleanAllDirty();
    };

    ZoneTravelElementsComposite.cleanAllDirty = function () {
        ZoneTravelTripsWidget.cleanDirty();
        ZoneTravelElementDetails.cleanDirty();
        ZoneTravelElementsComposite.cleanDirty();
    }

    ZoneTravelElementsComposite.SaveBOEUpdates = function () {
        if (ZoneTravelElementDetails.preparedForSubmit()
            && ZoneTravelTripsWidget.preparedForSubmit()) {

            $("#Save-ZoneTravelUpdates").addClass("display-none");
            $("#Loader-ZoneTravelUpdates").removeClass("display-none");

            ZoneTravelElementsComposite.data.inDetailsMV = ZoneTravelElementDetails.data;
            ZoneTravelElementsComposite.data.inTravelTripsCollection = ZoneTravelTripsWidget.data;
            
            var dataToSend = JSON.stringify(ZoneTravelElementsComposite.data);

            //this calls into the travel trips container because this is where they want the error message at.
            ZoneTravelElementsComposite.ajaxRequest({
                type: 'POST',
                url: saveZoneTravelDetailsCompositeUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: ZoneTravelElementsComposite.UpdateFields,
                error: function () {
                    refreshModule($('#ZoneTravelTripsContainerDiv'));
                    $("#Save-ZoneTravelUpdates").removeClass("display-none");
                    $("#Loader-ZoneTravelUpdates").addClass("display-none");
                }
            });
        }
    };

    ZoneTravelElementsComposite.isDirty = function () {
        return (ZoneTravelTripsWidget.isDirty() ||
                ZoneTravelElementDetails.isDirty());
    };

    ZoneTravelElementsComposite.CancelToMainGrid = function () {
        if (ZoneTravelElementsComposite.isDirty) {
            Session.confirmDialog(
                "Cancel",
                "Are you sure you want to cancel all changes?",
                function () {

                    ZoneTravelElementsComposite.cleanAllDirty();

                    $("#BOESummary").html('<div class="loader"></div>');
                    $(document).trigger(boeSummaryGridReloadEvent);
                    window.location.hash = 'Travel';
                },
                null);
        }
        else {
            $("#BOESummary").html('<div class="loader"></div>');
            $(document).trigger(boeSummaryGridReloadEvent);
            window.location.hash = 'Travel';
        }
    }

    return ZoneTravelElementsComposite;
}

function AfterDomLoadZoneTravelElementsCompositeWidget(ZoneTravelElementsComposite) {
    $("#Cancel-ZoneTravelUpdates").click(ZoneTravelElementsComposite.CancelToMainGrid);

    ZoneTravelElementsComposite.widgetsLoaded = 0;

    ZoneTravelElementsComposite.registerForEvent("ZoneTravelWidgetLoaded", function (e) {
        ZoneTravelElementsComposite.widgetsLoaded++;
        if (ZoneTravelElementsComposite.widgetsLoaded == 2) {
            Widget.applyValidation();
            $(document).trigger("AllZoneTravelWidgetLoaded");
        }
    });

    //Change the OCI note based off the workspace
	if (ZoneTravelElementsComposite.ContainsOCI == true) {
		$('#ZoneTravel-OCINote').html(ZoneTravelElementsComposite.BannerTextWithoutOCI);
    }
    else {
		$('#ZoneTravel-OCINote').html(ZoneTravelElementsComposite.BannerTextWithOCI);
    }

    if (ZoneTravelElementsComposite.isReadOnly()) {
        $("#Save-ZoneTravelUpdates").hide();
    }

    ZoneTravelElementsComposite.registerForLiveEvent('click', "#Save-ZoneTravelUpdates:not(.disabled)", ZoneTravelElementsComposite.SaveBOEUpdates);

    refreshModule($('.travel-details .module'));
}

function InitializeZoneTravelTripsWidget(ZoneTravelTripsWidget) {
    ZoneTravelTripsWidget.AddEditZoneTravelTripDialog = {};

    ZoneTravelTripsWidget.preparedForSubmit = function () {
        //TODO: add back in validation task
        //Widget.applyValidation();

        for(var x in ZoneTravelTripsWidget.data)
        {
            if(ZoneTravelTripsWidget.data[x].TravelTripID > 0)
            {
                ZoneTravelTripsWidget.data[x].UpdateDateLong = $("table#ZoneTravelTrips tr[pkid=" + ZoneTravelTripsWidget.data[x].TravelTripID + "]").attr("updatedatelong");
            }
        }

        return true;
    };

    ZoneTravelTripsWidget.refreshModule = function () {
        ZoneTravelTripsWidget.applyReadOnly();

        if (ZoneTravelTripsWidget.isReadOnly()) {
            $(".ZoneTravelTrips .travelbuttons").addClass('display-none');
            $(".ZoneTravelTrips .delete").addClass('display-none');
        }

        refreshModule($('#ZoneTravelTrips').parents('.module'));
    };

    ZoneTravelTripsWidget.perforgAutoCompleteSelect = function (event, ui) {
        ZoneTravelTripsWidget.PerfOrgSelect(ui.item.ID, ui.item.value);
    };

    ZoneTravelTripsWidget.PerfOrgSelect = function (id, perforg) {
        var perfOrgInput = $("#addEditTripDialog input[name=PerformingOrgName]");
        var perfOrgIdInput = $("#addEditTripDialog input[name=PerformingOrgID]");
        $(perfOrgIdInput).attr("perforg", perforg)
        $(perfOrgIdInput).val(id);
        $(perfOrgInput).val(perforg);
        $(perfOrgInput).change();
        $(perfOrgIdInput).change();
        var formBeingSaved = $(perfOrgInput).parents("form")[0];
        var valBox = $("ul.validation-box", formBeingSaved);
        if (id == 0) {
            $(valBox).find("li[name=PerformingOrg]").remove();
            valBox.append("<li name='PerformingOrg'>Performing Org is invalid</li>");
            $(valBox).show();
            $(valBox).fadeIn(500);
            ZoneTravelTripsWidget.focusValidation();
        }
        else {
            $(valBox).find("li[name=PerformingOrg]").remove();
            if ($(valBox).find("li").length == 0) {
                $(valBox).hide();
            }
        }
        ZoneTravelTripsWidget.refreshModule();
    };

    ZoneTravelTripsWidget.ClinSelect = function (id, clin) {
        var clinInput = $("#addEditTripDialog input[name=ClinName]");
            $(clinInput).val(clin);
            $(clinInput).change();

        var clinIdInput = $("#addEditTripDialog input[name=ClinId]");
            $(clinIdInput).attr("clin", clin)
            $(clinIdInput).val(id);
            $(clinIdInput).change();

        var formBeingSaved = $(clinInput).parents("form")[0];
        var valBox = $("ul.validation-box", formBeingSaved);
        if (id == 0) {
            $(valBox).find("li[name=Clin]").remove();
            valBox.append("<li name='Clin'>Clin is invalid</li>");
            $(valBox).show();
            $(valBox).fadeIn(500);
            ZoneTravelTripsWidget.focusValidation();
        }
        else {
            $(valBox).find("li[name=Clin]").remove();
            if ($(valBox).find("li").length == 0) {
                $(valBox).hide();
            }
        }
        ZoneTravelTripsWidget.refreshModule();
    };

    ZoneTravelTripsWidget.WbsSelect = function (id, wbs) {
        var wbsInput = $("#addEditTripDialog input[name=WbsName]");
            $(wbsInput).val(wbs);
            $(wbsInput).change();

        var wbsIdInput = $("#addEditTripDialog input[name=WbsId]");
            $(wbsIdInput).attr("wbs", wbs)
            $(wbsIdInput).val(id);
            $(wbsIdInput).change();

        var formBeingSaved = $(wbsInput).parents("form")[0];
        var valBox = $("ul.validation-box", formBeingSaved);
        if (id == 0) {
            $(valBox).find("li[name=Wbs]").remove();
            valBox.append("<li name='Wbs'>WBS is invalid</li>");
            $(valBox).show();
            $(valBox).fadeIn(500);
            ZoneTravelTripsWidget.focusValidation();
        }
        else {
            $(valBox).find("li[name=Wbs]").remove();
            if ($(valBox).find("li").length == 0) {
                $(valBox).hide();
            }
        }
        ZoneTravelTripsWidget.refreshModule();
    };

    ZoneTravelTripsWidget.autocompleteResources = function (request, response) {
        var toSearch = request.term.toLowerCase();
        var returned = {};

        returned = $.grep(BOEDetails.WSResources, function (r) {
            return r.ElementOfCost == '6' // travel 
            && r.ResourceDesc.toLowerCase().indexOf(toSearch) != -1;
        })

        response($.map(returned, function (item) {
            return {
                label: item.ResourceDesc,
                value: item.ResourceDesc,
                ID: item.Id
            }
        }));
    };

    ZoneTravelTripsWidget.resourceAutoCompleteSelect = function (event, ui) {
        if (ui.item != undefined) {
            ZoneTravelTripsWidget.NonZoneResourceSelect(ui.item.ID, ui.item.value);
        }
    };

    // added for nonzoneresoure based on perf org select list
    ZoneTravelTripsWidget.NonZoneResourceSelect = function (id, nonzoneresource) {
        var nonZoneResourceInput = $("#addEditTripDialog input[name=NonZoneResourceName]");
        var nonZoneResourceIdInput = $("#addEditTripDialog input[name=NonZoneResourceID]");
        $(nonZoneResourceIdInput).attr("nonzoneresource", nonzoneresource)
        $(nonZoneResourceIdInput).val(id);
        $(nonZoneResourceInput).val(nonzoneresource);
        $(nonZoneResourceInput).change();
        $(nonZoneResourceIdInput).change();
        var formBeingSaved = $(nonZoneResourceInput).parents("form")[0];
        var valBox = $("ul.validation-box", formBeingSaved);
        if (id == 0) {
            $(valBox).find("li[name=NonZoneResource]").remove();
            valBox.append("<li name='NonZoneResource'>Resource is invalid</li>");
            $(valBox).show();
            $(valBox).fadeIn(500);
            ZoneTravelTripsWidget.focusValidation();
        }
        else {
            $(valBox).find("li[name=NonZoneResource]").remove();
            if ($(valBox).find("li").length == 0) {
                $(valBox).hide();
            }
        }
        ZoneTravelTripsWidget.refreshModule();
    };
    // end added for nonzoneresource

    ZoneTravelTripsWidget.manageDefaultText = function (input) {
        if ($(input).hasClass("default-text")) {
            $(input).addClass('display-none');
            $(input).siblings('input').focus();
        } else {
            $(input).siblings('.default-text').addClass('display-none');
        }
    };

    ZoneTravelTripsWidget.setItemsForDelete = function () {
        $("table#ZoneTravelTrips tbody td.delete input:checked").each(function () {
            var travelID = $(this).parents("tr").attr("pkid");

            var found = false
            for (var x in ZoneTravelTripsWidget.data) {
                if (ZoneTravelTripsWidget.data[x].TravelTripID == travelID) {
                    if (ZoneTravelTripsWidget.data[x].TravelTripID > 0) {
                        ZoneTravelTripsWidget.data[x].Deleted = "true";
                    } else {
                        ZoneTravelTripsWidget.data.splice(x, 1);
                    }
                    found = true;
                }
            }

            if (!found && travelID > 0) {
                //cheesy
                ZoneTravelTripsWidget.populateEdit($(this));
                var datatopush = ZoneTravelTripsWidget.gatherDialogData();
                datatopush[0].Deleted = "true";
                if (datatopush[0].Mode == "") {
                    datatopush[0].Mode = 0;
                }
                if (datatopush[0].PerDiemID == "") {
                    datatopush[0].PerDiemID = 0;
                }
                ZoneTravelTripsWidget.data.push(datatopush[0]);
            }
            //ZoneTravelTripsWidget.RemoveCostFromTravel(travelID);
            $(this).parents("tr").remove();
        });

        ZoneTravelTripsWidget.refreshModule();
    }

    ZoneTravelTripsWidget.clearDialogData = function () {
        $("#addEditTripDialog :input").val("");
        $("#addEditTripDialog .default-text").removeClass('display-none');
        $("#addEditTripDialog .occurenceRows").hide();
        $("#addEditTripDialog :input[name=makeOccurrences]").prop("checked", false);
        $("#addEditTripDialog").find("button[name='save-button']").addClass("disabled");
        $("#addEditTripDialog").find("button[name='save-add-another-button']").addClass("disabled");

        // Clear attributes of Open Ended Custom Field Values
        var tripCustomFields = $("#addEditTripDialog").find("[id*=TaskElementCustomFieldID]");
        for (var x = 0, len = tripCustomFields.length; x < len; x++) {
            var customFieldInput = tripCustomFields[x].getElementsByTagName('input')[0];
            if (typeof customFieldInput !== "undefined") {
                customFieldInput.setAttribute("customfieldvalueid", "-1");
                customFieldInput.setAttribute("selectionid", "-1");
                customFieldInput.setAttribute("updatedatelong", "0");
            }
        }

        $('.zone-inputs').addClass('display-none');
        $('.nonzone-inputs').addClass('display-none');
        $('.shared-inputs').addClass('display-none');

        if (!ZoneTravelTripsWidget.isReadOnly()) {
            SetupMonthPicker("#DateOfEst");
            SetupMonthPicker("#TripDate");
        }
    };

    ZoneTravelTripsWidget.clearZoneData = function () {
        $("#addEditTripDialog .zone-inputs :input").val("");
    };

    ZoneTravelTripsWidget.clearNonzoneData = function () {
        $("#addEditTripDialog .nonzone-inputs :input").val("");
    };

    ZoneTravelTripsWidget.populateTripCustomFields = function (customFieldValue) {
        var found = false;
        var tripCustomFields = $("#addEditTripDialog").find("[id*=TaskElementCustomFieldID]");
        for (var x = 0, len = tripCustomFields.length; x < len; x++) {
            if (found == true) {
                break;
            }
            var customFieldDropdown = tripCustomFields[x].getElementsByTagName('select')[0];
            if (typeof customFieldDropdown !== "undefined") {
                var listOptions = customFieldDropdown.options;
                for (var i = 0, listlen = listOptions.length; i < listlen; i++) {
                    {
                        if (listOptions[i].value == customFieldValue.val()) {
                            customFieldDropdown.value = customFieldValue.val();
                            customFieldDropdown.setAttribute("selectionid", customFieldValue.attr("selectionid"));
                            customFieldDropdown.setAttribute("updatedatelong", customFieldValue.attr("updatedatelong"));
                            found = true;
                            break;
                        }
                    }
                }
            } else {
                var customFieldInput = tripCustomFields[x].getElementsByTagName('input')[0];
                if (typeof customFieldInput !== "undefined" && customFieldInput.getAttribute("customfieldid") === customFieldValue.attr("customfieldid")) {
                    customFieldInput.value = customFieldValue.val();
                    customFieldInput.setAttribute("customfieldvalueid", customFieldValue.attr("customfieldvalueid"));
                    customFieldInput.setAttribute("selectionid", customFieldValue.attr("selectionid"));
                    customFieldInput.setAttribute("updatedatelong", customFieldValue.attr("updatedatelong"));
                    found = true;
                    break;
                }
            }
        }
    };

    ZoneTravelTripsWidget.populateEdit = function (rowInput) {
        var row = $(rowInput).parents("tr");
        var gpid = (row.find("td.GroupID").attr("groupid") == 0) ? "" : row.find("td.GroupID").attr("groupid");
        $("#addEditTripDialog").find("input[name=TravelTripID]").val(row.attr("pkid"));
        $("#addEditTripDialog").find("input[name=GroupID]").val(gpid);
        row.find("td.GroupID").find("input[name=customField]").each(function () {
            ZoneTravelTripsWidget.populateTripCustomFields($(this));
        });
        var modeID = row.find("td.Mode").attr("modeid");
        $("#addEditTripDialog").find(":input[name=mode]").val(modeID);
        ZoneTravelTripsWidget.adjustInputFields(modeID);
        $("#addEditTripDialog").find("input[name=Purpose]").val(row.find("td.Purpose").attr("purpose"));
        $("#addEditTripDialog").find("input[name=PerformingOrgName]").val(row.find("td.PerfOrg").attr("perforgname"));
        $("#addEditTripDialog").find("input[name=PerformingOrgID]").val(row.find("td.PerfOrg").attr("perforgid"));
        $("#addEditTripDialog").find("input[name=PerformingOrgID]").attr("perforg", row.find("td.PerfOrg").attr("perforgname"));
        $("#addEditTripDialog").find("input[name=ClinName]").val(row.find("td.Clin").attr("clintext"));
        $("#addEditTripDialog").find("input[name=ClinId]").val(row.find("td.Clin").attr("clinid"));
        $("#addEditTripDialog").find("input[name=ClinId]").attr("clin", row.find("td.Clin").attr("clintext"));
        $("#addEditTripDialog").find("input[name=WbsName]").val(row.find("td.Wbs").attr("wbstext"));
        $("#addEditTripDialog").find("input[name=WbsId]").val(row.find("td.Wbs").attr("wbsid"));
        $("#addEditTripDialog").find("input[name=WbsId]").attr("wbs", row.find("td.Wbs").attr("wbstext"));
        $("#addEditTripDialog").find("input[name=DateOfEst]").val(row.find("td.DateOfEst").attr("dateofest"));
        $("#addEditTripDialog").find("input[name=TripDate]").val(row.find("td.TripDate").attr("tripdate"));
        $("#addEditTripDialog .default-text").addClass('display-none');
        $("#addEditTripDialog").find("input[name=NumOfPeople]").val(row.find("td.numOfPeople").attr("numofpeople"));
        $("#addEditTripDialog").find("input[name=NumOfDays]").val(row.find("td.numOfDays").attr("numofdays"));
        //Zone only fields
        if (modeID == ZoneTravelTripsWidget.ZoneNoAirfare || modeID == ZoneTravelTripsWidget.ZoneAirfare) { 
            $("#addEditTripDialog").find(":input[name=OriginID]").val(row.find("td.DepartureName").attr("originid"));
            $("#addEditTripDialog").find("input[name=DestinationCity]").val(row.find("td.DestinationName").attr("destinationcity"));
            $("#addEditTripDialog").find(":input[name=DestinationID]").val(row.find("td.DestinationName").attr("destinationstateid"));
        }
        //Nonzone only fields
        else if (modeID == ZoneTravelTripsWidget.NonzoneDomestic || modeID == ZoneTravelTripsWidget.NonzoneInternational) {
            $("#addEditTripDialog").find("input[name=NonZoneResourceName]").val(row.find("td.NonZoneResource").attr("nonzoneresourcename"));
            $("#addEditTripDialog").find("input[name=NonZoneResourceID]").val(row.find("td.NonZoneResource").attr("nonzoneresourceid"));
            $("#addEditTripDialog").find("input[name=NonZoneResourceID]").attr("nonzoneresource", row.find("td.NonZoneResource").attr("nonzoneresourcename"));
            

            $("#addEditTripDialog").find("input[name=FromLocation]").val(row.find("td.DepartureName").attr("fromlocation"));
            $("#addEditTripDialog").find("input[name=ToLocation]").val(row.find("td.DestinationName").attr("tolocation"));
            $("#addEditTripDialog").find("input[name=NumOfCars]").val(row.find("td.numOfCars").attr("numofcars"));
            $("#addEditTripDialog").find("input[name=AirfareEstimate]").val(row.find("td.costOfTrip").attr("airfareestimate"));
            $("#addEditTripDialog").find("input[name=PerDiemDaily]").val(row.find("td.costOfTrip").attr("perdiemdaily"));
            $("#addEditTripDialog").find("input[name=CarRentalTrans]").val(row.find("td.costOfTrip").attr("carrentaltrans"));
        }
    };

    ZoneTravelTripsWidget.adjustInputFields = function (mode) {
        //display zone and shared inputs for zone modes
        if (mode == ZoneTravelTripsWidget.ZoneNoAirfare || mode == ZoneTravelTripsWidget.ZoneAirfare) {
            $('#addEditTripDialog').find('.zone-inputs').removeClass('display-none');
            $('#addEditTripDialog').find('.nonzone-inputs').addClass('display-none');
            $('#addEditTripDialog').find('.shared-inputs').removeClass('display-none');
        }
        //display nonzone and shared inputs for nonzone modes
        else if (mode == ZoneTravelTripsWidget.NonzoneDomestic || mode == ZoneTravelTripsWidget.NonzoneInternational) {
            $('#addEditTripDialog').find('.zone-inputs').addClass('display-none');
            $('#addEditTripDialog').find('.nonzone-inputs').removeClass('display-none');
            $('#addEditTripDialog').find('.shared-inputs').removeClass('display-none');
            //show domestic travel agency fee and misc cost for nonzone domestic mode
            if (mode == ZoneTravelTripsWidget.NonzoneDomestic) {
                $('#addEditTripDialog').find('.nonzone-domestic-only').removeClass('display-none');
                $('#addEditTripDialog').find('.nonzone-international-only').addClass('display-none');
            }
            //show international travel agency fee for nonzone international mode
            else {
                $('#addEditTripDialog').find('.nonzone-international-only').removeClass('display-none');
                $('#addEditTripDialog').find('.nonzone-domestic-only').addClass('display-none');
            }
        }
        else { //hide all if no mode selected
            $('#addEditTripDialog').find('.zone-inputs').addClass('display-none');
            $('#addEditTripDialog').find('.nonzone-inputs').addClass('display-none');
            $('#addEditTripDialog').find('.shared-inputs').addClass('display-none');
        }
    };

    ZoneTravelTripsWidget.generateUpdateRow = function (dialogInputs) {
        for (var x in dialogInputs) {
            var dialogInput = dialogInputs[x];
            //delete from json if it is in there and then add it again.
            for (var w in ZoneTravelTripsWidget.data) {
                if (ZoneTravelTripsWidget.data[w].TravelTripID == dialogInput.TravelTripID) {
                    ZoneTravelTripsWidget.data.splice(w, 1);
                }
            }
            ZoneTravelTripsWidget.data.push(dialogInput);
            //if there is no row for it then create a row for it and then update.
            if ($("table#ZoneTravelTrips").find("tr[pkid=" + dialogInput.TravelTripID + "]").length == 0) {
                ZoneTravelTripsWidget.createRow(dialogInput.TravelTripID, dialogInput.CustomFieldValues);
            }

            ZoneTravelTripsWidget.updateRow(dialogInput);
        }
    };

    ZoneTravelTripsWidget.createRow = function (pkid, customFields) {
        var clinAndWbsIfNeeded = '';

        if (ZoneTravelTripsWidget.IsMulti == 'True') {
            clinAndWbsIfNeeded = '<td class="Clin"><a></a></td><td class="Wbs"><a></a></td>';
        }

        $("table#ZoneTravelTrips tbody").append(
            '<tr pkid="'+pkid+'">'+
            '<td class="delete"><input type=checkbox /></td>'+
            '<td class="GroupID"><a></a>'+
            ZoneTravelTripsWidget.generateCustomFieldInputs(customFields) +
            '</td>'+
            '<td class="PerfOrg"><a></a></td>' +
            clinAndWbsIfNeeded +
            '<td class="NonZoneResource"><a></a></td>' +
            '<td class="Mode"><a></a></td>' +
            '<td class="DepartureName"><a></a></td>' +
            '<td class="DestinationName"><a></a></td>' +
            '<td class="Zone"><a></a></td>' +
            '<td class="Purpose"><a></a></td>' +
            '<td class="DateOfEst"><a></a></td>' +
            '<td class="TripDate"></td>' +
            '<td class="numOfPeople"></td>' +
            '<td class="numOfDays"></td>' +
            '<td class="numOfCars"></td>' +
            '<td class="costOfTrip" cost="0"></td>' +
            '</tr>');
    };

    ZoneTravelTripsWidget.updateRow = function (rowInput) {
        var row = $("table#ZoneTravelTrips tbody").find("tr[pkid=" + rowInput.TravelTripID + "]");

        var isZone = (rowInput.ModeID == ZoneTravelTripsWidget.ZoneNoAirfare || rowInput.ModeID == ZoneTravelTripsWidget.ZoneAirfare);
        
        //TODO - Update BOE Summary with cost
        row.find("td.GroupID a").text(rowInput.GroupID);
        row.find("td.GroupID").attr("groupid", rowInput.GroupID);
        row.find("td.GroupID").find("input[name=customField]").remove();
        row.find("td.GroupID").append(ZoneTravelTripsWidget.generateCustomFieldInputs(rowInput.CustomFieldValues));

        row.find("td.PerfOrg a").text(rowInput.PerformingOrgName);
        row.find("td.PerfOrg").attr("perforgname", rowInput.PerformingOrgName);
        row.find("td.PerfOrg").attr("perforgid", rowInput.PerformingOrgID);

        row.find("td.Clin a").text(rowInput.ClinText);
        row.find("td.Clin").attr("clintext", rowInput.ClinText);
        row.find("td.Clin").attr("clinid", rowInput.ClinId);

        row.find("td.Wbs a").text(rowInput.WbsText);
        row.find("td.Wbs").attr("wbstext", rowInput.WbsText);
        row.find("td.Wbs").attr("wbsid", rowInput.WbsId);

        row.find("td.Mode a").text(rowInput.Mode);
        row.find("td.Mode").attr("modename", rowInput.Mode);
        row.find("td.Mode").attr("modeid", rowInput.ModeID);

        var departureName = rowInput.FromLocation;
        var destinationName = rowInput.ToLocation;
        if (isZone) {
            departureName = rowInput.OriginName;
            destinationName = rowInput.DestinationCity + ", " + rowInput.DestinationStateName;
        }

        row.find("td.DepartureName a").text(departureName);
        row.find("td.DepartureName").attr("departurename", departureName);
        row.find("td.DepartureName").attr("originname", rowInput.OriginName);
        row.find("td.DepartureName").attr("originid", rowInput.OriginID);
        row.find("td.DepartureName").attr("fromlocation", rowInput.FromLocation);
        
        row.find("td.DestinationName a").text(destinationName);
        row.find("td.DestinationName").attr("destinationname", destinationName);
        row.find("td.DestinationName").attr("destinationcity", rowInput.DestinationCity);
        row.find("td.DestinationName").attr("destinationstatename", rowInput.DestinationStateName);
        row.find("td.DestinationName").attr("destinationstateid", rowInput.DestinationStateID);
        row.find("td.DestinationName").attr("tolocation", rowInput.ToLocation);
    
        row.find("td.Purpose").text(rowInput.Purpose);
        row.find("td.Purpose").attr("purpose", rowInput.Purpose);
        
        row.find("td.DateOfEst").text(rowInput.DateOfEstimate);
        row.find("td.DateOfEst").attr("dateofest", rowInput.DateOfEstimate);

        row.find("td.TripDate").text(rowInput.EstTripDate);
        row.find("td.TripDate").attr("tripdate", rowInput.EstTripDate);

        row.find("td.numOfPeople").text(rowInput.numOfPeople);
        row.find("td.numOfPeople").attr("numofpeople", rowInput.numOfPeople);

        row.find("td.numOfDays").text(rowInput.numOfDays);
        row.find("td.numOfDays").attr("numofdays", rowInput.numOfDays);

        if (isZone) {
            row.find("td.Zone").text(rowInput.Zone);
            row.find("td.Zone").attr("zone", rowInput.Zone);

            row.find("td.numOfCars").text(ZoneTravelTripsWidget.NotApplicableString);
            row.find("td.costOfTrip").text(ZoneTravelTripsWidget.NotApplicableString);

            row.find("td.NonZoneResource").text(ZoneTravelTripsWidget.NotApplicableString);
        }
        else {
            row.find("td.Zone").text(ZoneTravelTripsWidget.NotApplicableString);

            row.find("td.NonZoneResource a").text(rowInput.NonZoneResourceName);
            row.find("td.NonZoneResource").attr("nonzoneresourcename", rowInput.NonZoneResourceName);
            row.find("td.NonZoneResource").attr("nonzoneresourceid", rowInput.NonZoneResourceID);

            row.find("td.numOfCars").text(rowInput.numOfCars);
            row.find("td.numOfCars").attr("numofcars", rowInput.numOfCars);

            row.find("td.costOfTrip").text(Helper.addCommas(parseFloat(rowInput.cost).toFixed(2), "$"));
            row.find("td.costOfTrip").attr("cost", parseFloat(rowInput.cost).toFixed(2));
            row.find("td.costOfTrip").attr("airfareestimate", parseFloat(rowInput.AirfareEst).toFixed(2));
            row.find("td.costOfTrip").attr("perdiemdaily", parseFloat(rowInput.PerDiemDaily).toFixed(2));
            row.find("td.costOfTrip").attr("carrentaltrans", parseFloat(rowInput.CarRentalTrans).toFixed(2));
            row.find("td.costOfTrip").attr("travelagencyfee", parseFloat(rowInput.TravelAgencyFee).toFixed(2));
            row.find("td.costOfTrip").attr("miscothercosts", parseFloat(rowInput.MiscOtherCosts).toFixed(2));
        }
    };

    ZoneTravelTripsWidget.generateCustomFieldInputs = function (customFields) {
        var toReturn = "";
        for (var x = 0; x < customFields.length; x++) {
            if (customFields[x].IsOpenEnded) {
                toReturn = toReturn.concat('<input type="hidden" name="customField" value="' + customFields[x].OpenEndedValue + '"customfieldid="' + customFields[x].CustomFieldID + '"customfieldvalueid="' + customFields[x].CustomFieldValueID + '"updatedatelong="' + customFields[x].UpdateDateLong + '" selectionid="' + customFields[x].SelectionID + '"openended="true">');
            } else {
                toReturn = toReturn.concat('<input type="hidden" name="customField" value="' + customFields[x].CustomFieldValueID + '"updatedatelong="' + customFields[x].UpdateDateLong + '" selectionid="' + customFields[x].SelectionID + '">');
            }
        }
        return toReturn;
    };
}

function AfterDomLoadZoneTravelTripsWidget(ZoneTravelTripsWidget, VerifyInputsUrl, taskStartDate, taskEndDate) {
    ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element = $('#addEditTripDialog');
    ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Params = { title: "Add Trip", modal: true, resizable: false, width: 640 }
    ZoneTravelTripsWidget.InitializeDialog(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog);
    
    $("#addEditTripDialog div.default-text").click(function () { ZoneTravelTripsWidget.manageDefaultText($(this)) });
    $("#addEditTripDialog input[name=DateOfEst]").focusin(function () {
        ZoneTravelTripsWidget.manageDefaultText($(this));
    });

    $("#addEditTripDialog input[name=TripDate]").focusin(function () {
        ZoneTravelTripsWidget.manageDefaultText($(this));
    });
    
    $(".ZoneTravelTrips .zonetravelbuttons button[name='add-button']:not(.disabled)").click(function () {
        ZoneTravelTripsWidget.clearDialogData();
        ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element.find("input[name=TravelTripID]").val(ZoneTravelTripsWidget.newItemCount);
        ZoneTravelTripsWidget.OpenDialogAfterInitialize(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog);
        ZoneTravelTripsWidget.clearValidationBox($("#addEditTripDialog ul.validation-box"));
    });

    $("#addEditTripDialog input[name=PerformingOrgName]").autocomplete({
        source: BOEDetails.autocompletePerformingOrgs,
        autoFocus: true,
        minLength: 2,
        focus: function (event, ui) {
        },
        select: ZoneTravelTripsWidget.perforgAutoCompleteSelect
    }).on('blur', function (e) {
        var that = $(this);
        var perfOrgIdInput = $("#addEditTripDialog input[name=PerformingOrgID]");
        if (that.val() != perfOrgIdInput.attr("perforg")) {
            if ($('.ui-autocomplete li:visible').length > 0) {
                var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                that.val(item.value);
                ZoneTravelTripsWidget.PerfOrgSelect(item.id, item.value);
            }
            else {
                ZoneTravelTripsWidget.PerfOrgSelect(0, that.val());
            }
        }
    });

    $("#addEditTripDialog input.select-nonzoneresource-autocomplete").autocomplete({
        source: ZoneTravelTripsWidget.autocompleteResources,
        autoFocus: true,
        minLength: 2,
        focus: function (event, ui) {
        },
        select: ZoneTravelTripsWidget.resourceAutoCompleteSelect
    }).on('blur', function (e) {
        var that = $(this);
        var nonZoneResourceIDInput = $("#addEditTripDialog input[name=NonZoneResourceID]");
        if (that.val() != nonZoneResourceIDInput.attr("nonzoneresource")) {
            if ($('.ui-autocomplete li:visible').length > 0) {
                var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                that.val(item.value);
                ZoneTravelTripsWidget.NonZoneResourceSelect(item.id, item.value);
            }
            else {
                ZoneTravelTripsWidget.NonZoneResourceSelect(0, that.val());
            }
        }
    });

    ZoneTravelTripsWidget.registerForLiveEvent('click', ".ZoneTravelTrips .zonetravelbuttons button[name='delete-button']", function () {
        ZoneTravelTripsWidget.setItemsForDelete();
        ZoneTravelTripsWidget.setDirty();
        if ($(".ZoneTravelTrips td.delete :input:checked").length == 0) {
            $(".ZoneTravelTrips .zonetravelbuttons button[name='delete-button']").addClass("disabled");
        } else {
            $(".ZoneTravelTrips .zonetravelbuttons button[name='delete-button']").removeClass("disabled");
        }
    });

    ZoneTravelTripsWidget.registerForLiveEvent('click', ".ZoneTravelTrips table .delete input", function () {
        if ($(".ZoneTravelTrips td.delete :input:checked").length == 0) {
            $(".ZoneTravelTrips .zonetravelbuttons button[name='delete-button']").addClass("disabled");
        } else {
            $(".ZoneTravelTrips .zonetravelbuttons button[name='delete-button']").removeClass("disabled");
        }
    });

    $(".ZoneTravelTrips th.delete input").click(function () {
        $(".ZoneTravelTrips td.delete input").prop("checked", $(".ZoneTravelTrips th.delete input").prop("checked"));
    });

    ZoneTravelTripsWidget.gatherDialogData = function () {
        var toReturn = [];
        var monthsToAdd = 0;
        var timesToAdd = 1;
        var dateOfEstString = $("#addEditTripDialog").find("input[name=DateOfEst]").val();
        var tripDateString = $("#addEditTripDialog").find("input[name=TripDate]").val();
        var modeID = $("#addEditTripDialog").find(":input[name=mode]").val();
        var travelAgencyFee = $("#addEditTripDialog #DomesticTravelAgencyFee").data("cost");
        var miscOtherCosts = $("#addEditTripDialog #MiscOtherCosts").data("cost");

        if (modeID == ZoneTravelTripsWidget.ZoneNoAirfare || modeID == ZoneTravelTripsWidget.ZoneAirfare) {
            ZoneTravelTripsWidget.clearNonzoneData();
        }
        else if (modeID == ZoneTravelTripsWidget.NonzoneDomestic || modeID == ZoneTravelTripsWidget.NonzoneInternational) {
            ZoneTravelTripsWidget.clearZoneData();
            if (modeID == ZoneTravelTripsWidget.NonzoneInternational) { 
                travelAgencyFee = $("#addEditTripDialog #InternationalTravelAgencyFee").data("cost");
                miscOtherCosts = 0;
            }
        }

        if ($("#addEditTripDialog").find("input[name=makeOccurrences]").prop("checked")) {
            monthsToAdd = parseInt($("#addEditTripDialog input[name=Interval]").val());
            timesToAdd = parseInt($("#addEditTripDialog input[name=numOfOccurrences]").val());

            var errors = [];

            if (isNaN(monthsToAdd) || monthsToAdd <= 0) {
                var error = {};
                error.ValidationIssue = 'Interval is invalid.';
                errors.push(error);
            }

            if (isNaN(timesToAdd) || timesToAdd <= 0) {
                var error = {};
                error.ValidationIssue = '# Occurrences is invalid.';
                errors.push(error);
            }

            if (errors.length > 0) {
                ZoneTravelTripsWidget.processValidationErrors(errors, $("#addEditTripValidation"));
                return -1;
            }
        }

        //if date of estimate is blank, replace it with today's date
        if (dateOfEstString == null || dateOfEstString == "") {
            var today = new Date();
            var month = today.getMonth() + 1; //January is 0
            var year = today.getFullYear();
            if (month < 10) {
                month = '0' + month;
            }
            dateOfEstString = month + '/' + year;
        }

        var traveltripID = $("#addEditTripDialog").find("input[name=TravelTripID]").val();
        var tripCustomfieldselectionid = 0;

        for (var i = 0; i < timesToAdd; i++) {
            var CustomFieldValuesCollection = [];
            $("#addEditTripDialog :input.customField").each(function () {
                if ($(this).attr("openended") === "true") {
                    CustomFieldValuesCollection.push({
                        IsOpenEnded: true,
                        SelectionID: $(this).attr("selectionid"),
                        CustomFieldValueID: $(this).attr("customfieldvalueid"),
                        UpdateDateLong: $(this).attr("UpdateDateLong"),
                        OpenEndedValue: $(this).val(),
                        CustomFieldID: $(this).attr("customfieldid")
                    });
                } else {
                    if ($(this).val() == "" && $(this).attr("selectionid") != "-1") {
                        //if the field value was deleted
                        CustomFieldValuesCollection.push({
                            IsOpenEnded: false,
                            SelectionID: $(this).attr("selectionid"),
                            UpdateDateLong: $(this).attr("UpdateDateLong")
                        });
                    } else if ($(this).val() != "" && timesToAdd > 1) {
                        CustomFieldValuesCollection.push({
                            IsOpenEnded: false,
                            SelectionID: --tripCustomfieldselectionid,
                            CustomFieldValueID: $(this).val()
                        });
                    } else if ($(this).val() != "" && parseInt($(this).attr("selectionid")) > 0) {
                        //if the field value was updated
                        CustomFieldValuesCollection.push({
                            IsOpenEnded: false,
                            SelectionID: $(this).attr("selectionid"),
                            CustomFieldValueID: $(this).val(),
                            UpdateDateLong: $(this).attr("UpdateDateLong")
                        });
                    } else if ($(this).val() != "" && parseInt($(this).attr("selectionid")) < 0) {
                        //new field value
                        CustomFieldValuesCollection.push({
                            IsOpenEnded: false,
                            SelectionID: $(this).attr("selectionid"),
                            CustomFieldValueID: $(this).val()
                        });
                    } //else is no change
                }
            });
            toReturn.push({
                TravelTripID: traveltripID,
                Mode: $("#addEditTripDialog").find(":input[name=mode] :selected").text(),
                ModeID: modeID,
                GroupID: $("#addEditTripDialog").find("input[name=GroupID]").val(),
                PerformingOrgName: $("#addEditTripDialog").find("input[name=PerformingOrgName]").val(),
                PerformingOrgID: $("#addEditTripDialog").find("input[name=PerformingOrgID]").val(),
                ClinText: $("#addEditTripDialog").find("input[name=ClinName]").val(),
                ClinId: $("#addEditTripDialog").find("input[name=ClinId]").val(),
                WbsText: $("#addEditTripDialog").find("input[name=WbsName]").val(),
                WbsId: $("#addEditTripDialog").find("input[name=WbsId]").val(),
                NonZoneResourceName: $("#addEditTripDialog").find("input[name=NonZoneResourceName]").val(),
                NonZoneResourceID: $("#addEditTripDialog").find("input[name=NonZoneResourceID]").val(),
                OriginName: $("#addEditTripDialog").find(":input[name=OriginID] :selected").text(),
                OriginID: $("#addEditTripDialog").find(":input[name=OriginID]").val(),
                DestinationCity: $("#addEditTripDialog").find("input[name=DestinationCity]").val(),
                DestinationStateName: $("#addEditTripDialog").find(":input[name=DestinationID] :selected").text(),
                DestinationStateID: $("#addEditTripDialog").find(":input[name=DestinationID]").val(), 
                Zone: $("#addEditTripDialog").find(":input[name=DestinationID] :selected").attr("zone"), 
                FromLocation: $("#addEditTripDialog").find("input[name=FromLocation]").val(),
                ToLocation: $("#addEditTripDialog").find("input[name=ToLocation]").val(),
                Purpose: $("#addEditTripDialog").find("input[name=Purpose]").val(),
                numOfPeople: $("#addEditTripDialog").find("input[name=NumOfPeople]").val(),
                numOfDays: $("#addEditTripDialog").find("input[name=NumOfDays]").val(),
                DateOfEstimate: dateOfEstString,
                EstTripDate: tripDateString,
                numOfCars: $("#addEditTripDialog").find("input[name=NumOfCars]").val(),
                AirfareEst: $("#addEditTripDialog").find("input[name=AirfareEstimate]").val(),
                PerDiemDaily: $("#addEditTripDialog").find("input[name=PerDiemDaily]").val(),
                CarRentalTrans: $("#addEditTripDialog").find("input[name=CarRentalTrans]").val(),
                TravelAgencyFee: travelAgencyFee,
                MiscOtherCosts: miscOtherCosts,
                CustomFieldValues: CustomFieldValuesCollection
            });

            traveltripID = --ZoneTravelTripsWidget.newItemCount;
            var TempTripDate = tripDateString.toDate();
            TempTripDate.addMonths(monthsToAdd);
            tripDateString = TempTripDate.toFormattedString(true);
        }

        return toReturn;
    };
    
    ZoneTravelTripsWidget.registerForLiveEvent('click', ".ZoneTravelTrips a", function () {
        ZoneTravelTripsWidget.clearDialogData();
        ZoneTravelTripsWidget.populateEdit(this);
        ZoneTravelTripsWidget.clearValidationBox($("#addEditTripDialog ul.validation-box"));
        ZoneTravelTripsWidget.OpenDialogAfterInitialize(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog);
    });

    ZoneTravelTripsWidget.registerForLiveEvent('click', "#Add-AddTrip:not(.disabled)", function () {
        $('#Loader-AddTrip').removeClass('display-none');
        $('#Add-AddTrip').addClass('display-none');

        ZoneTravelTripsWidget.clearValidationBox($("#addEditTripDialog ul.validation-box"));

        var data = ZoneTravelTripsWidget.gatherDialogData()

        if (data != -1) {
            ZoneTravelTripsWidget.verifyInput(data);
        } else {
            $('#Loader-AddTrip').addClass('display-none');
            $('#Add-AddTrip').removeClass('display-none');
        }
    });

    ZoneTravelTripsWidget.registerForLiveEvent('click', "#AddAnother-AddTrip:not(.disabled)", function () {
        ZoneTravelTripsWidget.clearValidationBox($("#addEditTripDialog ul.validation-box"));
        $('#Loader-AddTrip').removeClass('display-none');
        $('#AddAnother-AddTrip').addClass('display-none');

        var data = ZoneTravelTripsWidget.gatherDialogData()

        $('#Loader-AddTrip').addClass('display-none');
        $('#AddAnother-AddTrip').removeClass('display-none');

        ZoneTravelTripsWidget.verifyInput(data, true);
    });

    ZoneTravelTripsWidget.verifyInput = function (dialogInputs, addAnother) {        
        var dataToSend = JSON.stringify(dialogInputs);
        
        var VerifyInputsUrlWithDates = VerifyInputsUrl + '?taskStartDate=' + taskStartDate + '&taskEndDate=' + taskEndDate;
        ZoneTravelTripsWidget.ajaxRequest({
            type: 'POST',
            url: VerifyInputsUrlWithDates,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {
                $('#Loader-AddTrip').addClass('display-none');
                $('#Add-AddTrip').removeClass('display-none');

                // display errors if there are any
                if (response.errors.length > 0) {
                    ZoneTravelTripsWidget.processValidationErrors(response.errors, $("#addEditTripValidation"));
                }
                // otherwise add costs to trips and complete the add
                else {
                    dialogInputs.forEach(function (element) {
                        if ((element.ModeID == ZoneTravelTripsWidget.NonzoneDomestic || element.ModeID == ZoneTravelTripsWidget.NonzoneInternational) && element.TravelTripID in response.result) {
                            element.cost = response.result[element.TravelTripID];
                        }
                    });
                    if (addAnother) {
                        ZoneTravelTripsWidget.completeAddAnother(dialogInputs);
                    }
                    else {
                        ZoneTravelTripsWidget.completeAdd(dialogInputs);
                    }
                }
            }
        });
    };

    ZoneTravelTripsWidget.completeAdd = function (data) {
        ZoneTravelTripsWidget.generateUpdateRow(data);
        ZoneTravelTripsWidget.CloseDialog(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog);
        ZoneTravelTripsWidget.setDirty();
        ZoneTravelTripsWidget.newItemCount--;
        ZoneTravelTripsWidget.clearDialogData();
    };

    ZoneTravelTripsWidget.completeAddAnother = function (data) {
        ZoneTravelTripsWidget.generateUpdateRow(data);
        ZoneTravelTripsWidget.setDirty();
        ZoneTravelTripsWidget.newItemCount--;
        ZoneTravelTripsWidget.clearDialogData();
        $("#addEditTripDialog").find("input[name=TravelTripID]").val(ZoneTravelTripsWidget.newItemCount);
    };
    
    $(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element).find(":input").on('keyup change', function () {
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    $(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element).find("#Mode-select").change(function () {
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    $(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element).find("#Origin-select").change(function () {
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    $(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element).find("#Destination-select").change(function () {
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    ZoneTravelTripsWidget.EnableSaveButtons = function () {
        $(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element).find("button[name='save-button']").removeClass("disabled");
        $(ZoneTravelTripsWidget.AddEditZoneTravelTripDialog.Element).find("button[name='save-add-another-button']").removeClass("disabled");
    }

    $('#ZoneTravelStartDate').change(function () {
        taskStartDate = $('#ZoneTravelStartDate').val().replace("/", "-");
    });

    $('#ZoneTravelEndDate').change(function () {
        taskEndDate = $('#ZoneTravelEndDate').val().replace("/", "-");
    });

    $('#addEditTripDialog input[name=WbsName]').change(function () {
        $('#addEditTripDialog input[name=WbsId]').attr("wbs", '').val('');
    });
    $('#addEditTripDialog input[name=ClinName]').change(function () {
        $('#addEditTripDialog input[name=ClinId]').attr("clin", '').val('');
    });

    ZoneTravelTripsWidget.applyReadOnly();

    if (ZoneTravelTripsWidget.isReadOnly()) {
        $("#ZoneTravelTripsWidgetContainer .travelbuttons").addClass('display-none');
        $("#addEditTripDialog input, #addEditTripDialog select").prop('disabled', true);
        $("#clin-select, #wbs-select, #perform-select, #addEditTripDialog .buttons, #MakeOccurrencesRow").addClass('display-none');
    }

    $(".occurenceRows").hide();

    ZoneTravelTripsWidget.registerForEvent("AllZoneTravelWidgetLoaded", function (e) {
        ZoneTravelTripsWidget.refreshModule();
    });

    $("input[name=makeOccurrences]").change(function () {
        if ($(this).prop("checked")) {
            $(".occurenceRows").show();
        } else {
            $(".occurenceRows").hide();
        }
    });

    $('.select-perforg.popup-div', ZoneTravelTripsWidget.context).on('click', '.perforg-option', function () {
        var option = $(this);
        ZoneTravelTripsWidget.PerfOrgSelect(option.attr("perforgid"), option.attr('perforgname'));
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    var options = {
        propagateOnClick: true,
        button: 'div.popup-div-button.for-perf-orgs',
        onShow: function (button) { ZoneTravelTripsWidget.PerfOrgPopupButton = $(button); },
        leftOffSet: -105
    };
    $('div.popup-div.select-perforg').genPopUp(options);

    if (!ZoneTravelTripsWidget.isReadOnly()) {
        var html = "";

        $.each(BOEDetails.WSPerfOrgs, function (i, item) {
            html += '<div class="perforg-option" perforgid="' + item.PerformingOrgID + '" perforgname="' + item.PerformingOrgName + '" perforgdesc="' + item.PerformingOrgDesc + '">' + item.PerformingOrgName + '-' + item.PerformingOrgDesc + '</div>';
        });
        ZoneTravelTripsWidget.getElement("div.select-perforg.popup-div").html(html);
    };

    // CLIN
    $('.select-clin.popup-div', ZoneTravelTripsWidget.context).on('click', '.clin-option', function () {
        var option = $(this);
        ZoneTravelTripsWidget.ClinSelect(option.attr("clinId"), option.attr('clinname'));
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    var options = {
        propagateOnClick: true,
        button: 'div.popup-div-button.for-clins',
        onShow: function (button) { ZoneTravelTripsWidget.ClinPopupButton = $(button); },
        leftOffSet: -105
    };
    $('div.popup-div.select-clin').genPopUp(options);

    if (!ZoneTravelTripsWidget.isReadOnly()) {
        var html = "";

        $.each(BOEDetails.WSClins, function (i, item) {
            html += '<div class="clin-option" clinid="' + item.ClinId + '" clinname="' + item.ClinName + '">' + item.ClinName + '</div>';
        });
        ZoneTravelTripsWidget.getElement("div.select-clin.popup-div").html(html);
    };

    // WBS
    $('.select-wbs.popup-div', ZoneTravelTripsWidget.context).on('click', '.wbs-option', function () {
        var option = $(this);
        ZoneTravelTripsWidget.WbsSelect(option.attr("wbsId"), option.attr('wbsname'));
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    var options = {
        propagateOnClick: true,
        button: 'div.popup-div-button.for-wbss',
        onShow: function (button) { ZoneTravelTripsWidget.WbsPopupButton = $(button); },
        leftOffSet: -105
    };
    $('div.popup-div.select-wbs').genPopUp(options);

    if (!ZoneTravelTripsWidget.isReadOnly()) {
        var html = "";

        $.each(BOEDetails.WSWbss, function (i, item) {
            html += '<div class="wbs-option" wbsid="' + item.WbsId + '" wbsname="' + item.WbsName + '">' + item.WbsName + '</div>';

        });
        ZoneTravelTripsWidget.getElement("div.select-wbs.popup-div").html(html);
    };

    // added for nonzoneresource
    $('.select-nonzoneresource.popup-div', ZoneTravelTripsWidget.context).on('click', '.nonzoneresource-option', function () {
        var option = $(this);
        ZoneTravelTripsWidget.NonZoneResourceSelect(option.attr("nonzoneresourceid"), option.attr('nonzoneresourcename'));
        ZoneTravelTripsWidget.EnableSaveButtons();
    });

    var optionsResource = {
        propagateOnClick: true,
        button: 'div.popup-div-button.for-nonzone-resources',
        onShow: function (button) {  ZoneTravelTripsWidget.NonZoneResourcePopupButton = $(button); },
        leftOffSet: -105
    };
    $('div.popup-div.select-nonzoneresource').genPopUp(optionsResource);

    if (!ZoneTravelTripsWidget.isReadOnly()) {
        var html = "";

        $.each(BOEDetails.WSResources, function (i, item) {
            if(item.ElementOfCost == 6) // todo:  tglick can we avoid hardcoding to get just travel resources?
                html += '<div class="nonzoneresource-option" nonzoneresourceid="' + item.Id + '" nonzoneresourcename="' + item.ResourceName + '" nonzoneresourcedesc="' + item.ResourceDesc + '">' + item.ResourceName + '-' + item.ResourceDesc + '</div>';
        });

        ZoneTravelTripsWidget.getElement("div.select-nonzoneresource.popup-div").html(html);
    };
    // end added nonzoneresource
}