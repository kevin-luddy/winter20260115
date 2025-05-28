/*
This javascript file is called from BOE Travel ascx files
*/

function InitializeBOETravelWidget(readOnly, loadDuplicateTravelTaskDialogUrl, saveReorderTravelTaskElementsUrl) {
    var BOETravelWidget = new GridWidget('BOETRAVEL', '', readOnly);

    //Display Duplicate Task Element Dialog
    BOETravelWidget.displayDuplicateTravelTaskElementDialog = function () {
        $('#DuplicateTravelTask').addClass('display-none');
        $('#DuplicateTravelTaskElementDialog-Loader').removeClass('display-none');

        $("#DuplicateTravelTaskElementDialog").dialog({ width: 700, modal: true, resizable: false, draggable: true, title: 'Duplicate Task Elements', autoOpen: false, close: BOETravelWidget.closeDuplicateTravelTaskElementDialog });

        $.ajax({
            type: 'GET',
            url: loadDuplicateTravelTaskDialogUrl,
            success: function (response) {
                $('#DuplicateTravelTaskElementDialog').html(response).dialog('open');
                DuplicateTaskWidget.AfterSave = function () { $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID'); }
                $('#DuplicateTravelTask').removeClass('display-none');
                $('#DuplicateTravelTaskElementDialog-Loader').addClass('display-none');
            },
            error: function () {
                $('#DuplicateTravelTask').removeClass('display-none');
                $('#DuplicateTravelTaskElementDialog-Loader').addClass('display-none');
            }
        });

    };

    //Close the duplicate task dialog this way so there will never be multiple instances of it existing across task tabs
    BOETravelWidget.closeDuplicateTravelTaskElementDialog = function () {
        $('#DuplicateTravelTaskElementDialog').dialog('destroy').empty();
    }

    BOETravelWidget.ReOrderTravelTaskElementDialog = {};
    BOETravelWidget.ReOrderTravelTaskElementDialog.Element = $("#ReOrderTravelTaskElementDialog");
    BOETravelWidget.ReOrderTravelTaskElementDialog.Params = { width: 700, height: 365, modal: true, resizable: false, draggable: true, title: 'Sort Task Elements' };

    BOETravelWidget.displayReOrderTravelTaskElementDialog = function () {

        BOETravelWidget.OpenDialogAfterInitialize(BOETravelWidget.ReOrderTravelTaskElementDialog);

    }
    $('#ReOrderTravelTaskElementDialog #TasksMoveItemsUp').click(function () {
        BOETravelWidget.ShiftSelectedItemsUp($('#ReOrderTravelTaskElementDialog #TaskElementsList'));
        BOETravelWidget.EnableSave();

    });

    $('#ReOrderTravelTaskElementDialog #TasksMoveItemsDown').click(function () {
        BOETravelWidget.ShiftSelectedItemsDown($('#ReOrderTravelTaskElementDialog #TaskElementsList'));
        BOETravelWidget.EnableSave();

    });

    BOETravelWidget.CancelReOrderTaskElement = function () {
        BOETravelWidget.CloseDialog(BOETravelWidget.ReOrderTravelTaskElementDialog);
        $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Loader').addClass('display-none');
        $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Save').addClass('disabled');
        $('#ReOrderTravelTaskElementDialog').addClass('display-none');
        $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
    };


    BOETravelWidget.EnableSave = function () {
        $('#ReOrderTravelTaskElementDialog-Save').removeClass('disabled');
    };

    BOETravelWidget.SaveReOrderTaskElements = function () {
        $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Save').addClass('display-none');
        $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Loader').removeClass('display-none');
        var dataToSend = {};

        var listOrder = 0;
        dataToSend.BOETaskElements = [];
        $('#ReOrderTravelTaskElementDialog #TaskElementsList').children('option').each(function () {
            var taskToSave = {};
            var taskValue = $(this).val();

            taskToSave.TaskID = taskValue;
            taskToSave.ListOrder = listOrder++;
            dataToSend.BOETaskElements.push(taskToSave);
        });

        listOrder = 0;

        dataToSend = JSON.stringify(dataToSend);
        BOETravelWidget.ajaxRequest({
            type: 'POST',
            url: saveReorderTravelTaskElementsUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {
                BOETravelWidget.CloseDialog(BOETravelWidget.ReOrderTravelTaskElementDialog);
                $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Save').removeClass('display-none');
                $('#ReOrderTravelTaskElementDialog').addClass('display-none');
                $(document).trigger('LOAD_TRAVEL_ELEMENT_GRID');
            },
            error: function () {
                $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Loader').addClass('display-none');
                $('#ReOrderTravelTaskElementDialog #ReOrderTravelTaskElementDialog-Save').removeClass('display-none');
            }
        });


    };

    BOETravelWidget.ShiftSelectedItemsDown = function (list) {
        $($(list).children('option:selected').get().reverse()).each(function () {
            if ($(this).next().length == 0) {
                return false;
            }

            $(this).next().after($(this));
        });
    };
    // Moves selected list items up in the list
    BOETravelWidget.ShiftSelectedItemsUp = function (list) {
        $(list).children('option:selected').each(function () {
            if ($(this).prev().length == 0) {
                return false;
            }

            $(this).prev().before($(this));
        });
    };



    BOETravelWidget.removeDeletedItems = function (TRAVELID) {

        $(this).find('tr').attr("pkid").fadeOut(400, function () {
            $(this).find('tr').attr("pkid").remove();
        });
    }

    BOETravelWidget.expandTravelElement = function (TRAVELID) {
        window.location.hash = 'Travel/travel/' + TRAVELID;
    }

    BOETravelWidget.addNewTravelElement = function () {
        window.location.hash = 'Travel/travel/';
    }

    return BOETravelWidget;
}

function AfterDomLoadBOETravelWidget(BOETravelWidget, deleteAllBOETravelUrl, boeSummaryGridReloadEvent, deleteBOETravelUrl) {
    BOETravelWidget.InitializeDialog(BOETravelWidget.ReOrderTravelTaskElementDialog);


    BOETravelWidget.registerForLiveEvent('click', '#boe-AddTravelElement:not(.disabled)', BOETravelWidget.addNewTravelElement);


    $("#BOETRAVELGridHeader .delete").click(function () {
        Session.confirmDialog("Delete All Travel Elements", "Are you sure you want to delete all Travel Elements?", function () {

            var dataToSend = JSON.stringify(BOETravelWidget.data);

            $.ajax({
                type: 'POST',
                url: deleteAllBOETravelUrl,
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

    $("#BOETRAVELGridBody .delete").click(function () {
        var input = this;
        Session.confirmDialog("Delete Travel Element", "Are you sure you want to delete the selected Travel Element?", function () {
            deletedTask = {};
            deletedTask.TravelID = $(input).parents('tr').find('.hidden-travel-id').val();

            BOETravelWidget.data = deletedTask;

            var dataToSend = JSON.stringify(BOETravelWidget.data);

            $.ajax({
                type: 'POST',
                url: deleteBOETravelUrl,
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

    $("#BOETRAVELGrid .travelTotalCost").each(function () {
        $(this).text(Helper.addCommas($(this).text().trim(), "$"));
    });


    if (BOETravelWidget.isReadOnly()) {
        $('#boe-AddTravelElement').addClass('display-none');
        $('#BOETRAVELGridContainer .buttons').addClass('display-none');
        $('#BOETRAVELGrid .delete').addClass('display-none');
        $('#BOETRAVELGrid .deleteColumn').addClass('display-none');
    }

    createModule($('.boe-TRAVEL.module'));
    SortableGrid('.TravelGrid');
    refreshModule($('.boe-TRAVEL.module'));
}

function InitializeTravelElementDetailsWidget(TravelElementDetails_ReadOnly, boeId, travelId, travelUpdateDateLong) {
    var TravelElementDetails = new Widget("TravelElementDetailsFormContainer", TravelElementDetails_ReadOnly);
    
    TravelElementDetails.preparedForSubmit = function () {
        TravelElementDetails.data.BOEID = boeId;
        TravelElementDetails.data.TravelID = travelId;
        TravelElementDetails.data.UpdateDateLong = travelUpdateDateLong;
        TravelElementDetails.data.TaskID= $(".travel-element-details :input[name=TaskID]").val();
        TravelElementDetails.data.TaskTitle = $(".travel-element-details :input[name=TravelTaskTitle]").val();
        TravelElementDetails.data.CustomFieldValues=[];


        $("#TravelElementDetailsForm :input.customField").each(function(){
            if($(this).val()=="" && $(this).attr("selectionid")!= "-1"){
                //if the field value was deleted
                TravelElementDetails.data.CustomFieldValues.push({SelectionID:$(this).attr("selectionid"),UpdateDateLong:$(this).attr("UpdateDateLong")});
            }else if($(this).val()!="" && $(this).attr("selectionid")!= "-1"){
                //if the field value was updated
                TravelElementDetails.data.CustomFieldValues.push({SelectionID:$(this).attr("selectionid"),CustomFieldValueID:$(this).val(),UpdateDateLong:$(this).attr("UpdateDateLong")});
            }else if($(this).val()!="" && $(this).attr("selectionid")== "-1"){
                //new field value
                TravelElementDetails.data.CustomFieldValues.push({CustomFieldValueID:$(this).val()});
            }//else is no change
        });
        TravelElementDetails.data.TravelTaskDescription = $(".travel-element-details :input[name=TravelTaskDescription]").val();
        // explicitly re-extract rich-text field values from tinyMCE editor
        if(tinyMCE.EditorManager.editors.TravelTaskDescription){
            TravelElementDetails.data.TravelTaskDescription = tinyMCE.EditorManager.editors.TravelTaskDescription.getContent();
        }

        TravelElementDetails.data.StartDate = $(".travel-element-details :input[name=TravelStartDate]").val();
        TravelElementDetails.data.EndDate = $(".travel-element-details :input[name=TravelEndDate]").val();
        return true;
    };

    TravelElementDetails.registerForEvent("TravelDatesAdjusted", function(e, Dates){
        $(".travel-element-details").find("#TaskStartDate").html(Dates.StartDate);
        $(".travel-element-details").find("#TaskEndDate").html(Dates.EndDate);
        $(".travel-element-details").find("#TaskStartDateLabel").show();
        $(".travel-element-details").find("#TaskEndDateLabel").show();
    });

    TravelElementDetails.refreshModule = function () {
        refreshModule($('.travel-element-details.module'))
    };

    return TravelElementDetails;
}

function AfterDomLoadTravelElementDetailsWidget(TravelElementDetails, boeStateIsDraft, WorkspaceState) {
    TravelElementDetails.afterDOMLoad();
    var module = $('.travel-element-details.module');
    createModule(module);

    if (!TravelElementDetails_ReadOnly &&
        (WorkspaceState != "Locked" ||
        (WorkspaceState == "Locked" && boeStateIsDraft)))  // check BOE state (widget is read-only, but BOE is editable in Draft)
    {
        module.find(":input[name=TravelStartDate]").prop('disabled', false);
        module.find(":input[name=TravelEndDate]").prop('disabled', false);
        module.find('.ui-datepicker-trigger').removeClass('display-none');

        module.find(":input[name=TravelStartDate], :input[name=TravelEndDate]", $("#TravelElementDetailsForm")).keydown(function () {
            $("#Travel" + $(this).attr("name")).html($(this).val());
        });

        $("#TravelElementDetailsForm #TravelStartDate").datepicker();
        $("#TravelElementDetailsForm :input[name=TravelEndDate]").datepicker();

        module.find(":input[name=TravelStartDate], :input[name=TravelEndDate]", $("#TravelElementDetailsForm")).datepicker({
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
        module.find(":input[name=TravelStartDate]").prop('disabled', true);
        module.find(":input[name=TravelEndDate]").prop('disabled', true);
        module.find('.ui-datepicker-trigger').addClass('display-none');
    }

    TravelElementDetails.applyReadOnly();

    TravelElementDetails.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { TravelElementDetails.cleanDirty(); });

    refreshModule($('.travel-element-details.module'));

    CollapsibleModule($('.travel-element-details.module'));

    TravelElementDetails.registerForEvent("AllTravelWidgetLoaded", function (e) {
        TravelElementDetails.refreshModule();
    });

    $(document).trigger('TravelWidgetLoaded', "TravelTrips");

    $(document).trigger("TravelDatesAdjusted", { StartDate: $("#TravelElementDetailsForm :input[name=TravelStartDate]").val(), EndDate: $("#TravelElementDetailsForm :input[name=TravelEndDate]").val() })

    if (!TravelElementDetails.isReadOnly()) {
        InitializeRTE('TravelTaskDescription', { maxlen: 500000 }, TravelElementDetails);
    }
    else {
        HandleRTEDataForReadOnly("#TravelTaskDescription", ".replacedWidgetText");
    }
}

function InitializeTravelElementsCompositeWidget(TravelElementsComposite_ReadOnly, containsOCI, boeSummaryGridReloadEvent,
    saveTravelDetailsCompositeUrl) {
    var TravelElementsComposite = new Widget("TravelElementsComposite", TravelElementsComposite_ReadOnly);
    
    TravelElementsComposite.ContainsOCI = containsOCI;

    TravelElementsComposite.UpdateFields = function () {
        $("#Save-TravelUpdates").removeClass("display-none");
        $("#Loader-TravelUpdates").addClass("display-none");
        $(document).trigger(boeSummaryGridReloadEvent);
        window.location.hash = 'Travel';
        TravelElementsComposite.cleanAllDirty();
    };

    TravelElementsComposite.cleanAllDirty =function(){
        TravelTripsWidget.cleanDirty();
        TravelElementDetails.cleanDirty();
        TravelElementsComposite.cleanDirty();
    }
    
    TravelElementsComposite.SaveBOEUpdates = function () {
        if (TravelElementDetails.preparedForSubmit() 
            && TravelTripsWidget.preparedForSubmit()){

            $("#Save-TravelUpdates").addClass("display-none");
            $("#Loader-TravelUpdates").removeClass("display-none");
                
            TravelElementsComposite.data.inDetailsWV = TravelElementDetails.data;
            TravelElementsComposite.data.inTravelTripsCollection = TravelTripsWidget.data;


            var dataToSend = JSON.stringify(TravelElementsComposite.data);
            
            //this calls into the travel trips container because this is where they want the error message at.
            TravelElementsComposite.ajaxRequest({
                type: 'POST',
                url: saveTravelDetailsCompositeUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: TravelElementsComposite.UpdateFields,
                error: function() {
                    refreshModule($('#TravelTripsContainerDiv'));
                    $("#Save-TravelUpdates").removeClass("display-none");
                    $("#Loader-TravelUpdates").addClass("display-none");
                }
            });
        }
    };

    TravelElementsComposite.isDirty = function()
    {
        return (TravelTripsWidget.isDirty() ||
                TravelElementDetails.isDirty());
    };


    TravelElementsComposite.CancelToMainGrid = function(){

        if ( TravelElementsComposite.isDirty) {
            TravelElementsComposite.CancelToMainGrid = function(){

                if ( TravelElementsComposite.isDirty) {
                    Session.confirmDialog(
                        "Cancel",
                        "Are you sure you want to cancel all changes?",
                        function() {
                    
                            TravelElementsComposite.cleanAllDirty();

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
            Session.confirmDialog(
                        "Cancel",
                        "Are you sure you want to cancel all changes?",
                        function() {
                            TravelElementsComposite.cleanAllDirty();

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

    return TravelElementsComposite;
}

function AfterDomLoadTravelElementsCompositeWidget(TravelElementsComposite) {
    $("#Cancel-TravelUpdates").click(TravelElementsComposite.CancelToMainGrid);

    TravelElementsComposite.widgetsLoaded = 0;

    TravelElementsComposite.registerForEvent("TravelWidgetLoaded", function (e) {
        TravelElementsComposite.widgetsLoaded++;
        if (TravelElementsComposite.widgetsLoaded == 2) {
            Widget.applyValidation();
            $(document).trigger("AllTravelWidgetLoaded");
        }
    });


    //Change the OCI note based off the workspace
	if (TravelElementsComposite.ContainsOCI == true) {
        $('#Travel-OCINote').html(TravelElementsComposite.BannerTextWithoutOCI);
    }
    else {
        $('#Travel-OCINote').html(TravelElementsComposite.BannerTextWithOCI);
    }

    if (TravelElementsComposite.isReadOnly()) {
        $("#Save-TravelUpdates").hide();
    }

    TravelElementsComposite.registerForLiveEvent('click', "#Save-TravelUpdates:not(.disabled)", TravelElementsComposite.SaveBOEUpdates);

    refreshModule($('.travel-details .module'));
}

function InitializeTravelTripsWidget(TravelTripsWidget, summaryGridTravelHoursUpdatedEvent, travelElementCostType, calculateTravelTripsUrl, summaryGridHoursUpdatedEvent,
    getFilteredTripDataUrl) {
    $('#addEditTripDialog div.help-dialog-close').on('click', function () {
        $(this).parent().prev().click();
    });

    TravelTripsWidget.AddEditTravelTripDialog = {};

    TravelTripsWidget.preparedForSubmit = function () {
        Widget.applyValidation();


        var toReturn=true;
        for(var x in TravelTripsWidget.data)
        {
            if(TravelTripsWidget.data[x].TravelTripID > 0)
            {
                TravelTripsWidget.data[x].UpdateDateLong = $("table#TravelTrips tr[pkid="+TravelTripsWidget.data[x].TravelTripID+"]").attr("updatedatelong");
            }
            
        }

        if(TravelTripsWidget.waitingBeforeSubmit)
        {
            return false;
        }

        return toReturn;
    };

    TravelTripsWidget.InitializeValidation = function() {
        // we have to add the number of days in the trip to the trip date to ensure we have the month of the last day of the trip
        // to bounds check (i.e. BOE start = 1/2000, end = 12/2000, TripDate=12/2000 and days in trip are 33 .. the month to check is 1/2001
        // if the user selected multiple occurrences, we will take the day of the last occurence and add the trip length to that

        // if the user selected multiple occurrences we have to check each occurrence and ensure it is within the BOE dates
        var tripEndDate = $("#addEditTripDialog :input[name=TripDate]").val().toDate();
        if ($("#addEditTripDialog :input[name=makeOccurences]").prop('checked')) {
            // add X months to first trip date where X = ((occurrences-1) * interval)
            var numOfOccurrences = parseInt($("#addEditTripDialog :input[name=numOfOccurrences]").val());
            var interval = parseInt($("#addEditTripDialog :input[name=Interval]").val());

            tripEndDate.setMonth(tripEndDate.getMonth() + parseInt(((numOfOccurrences-1) * interval)));
        } 

        // add the length of the trip to the last trip
        tripEndDate.setDate(tripEndDate.getDate() + parseInt($("#addEditTripDialog :input[name=numOfDays]").val()));


    }

    TravelTripsWidget.refreshModule = function () {
        TravelTripsWidget.applyReadOnly();

        if (TravelTripsWidget.isReadOnly()) {
            $(".TravelTrips .travelbuttons").addClass('display-none');
            $(".TravelTrips .delete").addClass('display-none');
        }

        refreshModule($('#TravelTrips').parents('.module'));
    };
    
    TravelTripsWidget.manageDefaultText = function (input) {
        if($(input).hasClass("default-text"))
        {
            $(input).addClass('display-none');
            $(input).siblings('input').focus();
        }else{
            $(input).siblings('.default-text').addClass('display-none');
        }
    };

    TravelTripsWidget.setItemsForDelete = function(){
        $("table#TravelTrips tbody td.delete input:checked").each(function(){
            var travelID = $(this).parents("tr").attr("pkid");

            var found = false
            for(var x in TravelTripsWidget.data)
            {
                if(TravelTripsWidget.data[x].TravelTripID == travelID)
                {
                    if(TravelTripsWidget.data[x].TravelTripID > 0)
                    {
                        TravelTripsWidget.data[x].Deleted = "true";
                    }else{
                        TravelTripsWidget.data.splice(x, 1);
                    }
                    found=true;
                }
            }

            if(!found && travelID > 0)
            {
                //cheesy
                TravelTripsWidget.populateEdit($(this));
                var datatopush = TravelTripsWidget.gatherDialogData();
                datatopush[0].Deleted = "true";
                if (datatopush[0].Mode == "") {
                    datatopush[0].Mode = 0;
                }
                if (datatopush[0].PerDiemID == "") {
                    datatopush[0].PerDiemID = 0;
                }
                TravelTripsWidget.data.push(datatopush[0]);
            }
            TravelTripsWidget.RemoveCostFromTravel(travelID);
            $(this).parents("tr").remove();
        });

        TravelTripsWidget.refreshModule();  
    }
    
    TravelTripsWidget.gatherDialogData = function(){
        var toReturn = [];
        var monthsToAdd= 0;
        var timesToAdd= 1;
        var tripDateString= $("#addEditTripDialog").find("input[name=TripDate]").val();
     
        if($("#addEditTripDialog").find("input[name=makeOccurences]").prop("checked")){
            monthsToAdd= parseInt($("#addEditTripDialog input[name=Interval]").val());
            timesToAdd= parseInt($("#addEditTripDialog input[name=numOfOccurrences]").val());
        }

        var traveltripID = $("#addEditTripDialog").find("input[name=TravelTripID]").val();
        var tripCustomfieldselectionid = 0;
        for(var y=0; y<timesToAdd; y++)
        {
            var CustomFieldValuesCollection =[];
            $("#addEditTripDialog :input.customField").each(function(){
                if($(this).val()=="" && $(this).attr("selectionid")!= "-1"){
                    //if the field value was deleted
                    CustomFieldValuesCollection.push({SelectionID:$(this).attr("selectionid"),UpdateDateLong:$(this).attr("UpdateDateLong")});
                }else if($(this).val()!="" && timesToAdd > 1){
                    CustomFieldValuesCollection.push({SelectionID:--tripCustomfieldselectionid,CustomFieldValueID:$(this).val()});
                }
                else if($(this).val()!="" && $(this).attr("selectionid")!= "-1"){
                    //if the field value was updated
                    CustomFieldValuesCollection.push({SelectionID:$(this).attr("selectionid"),CustomFieldValueID:$(this).val(),UpdateDateLong:$(this).attr("UpdateDateLong")});
                }else if($(this).val()!="" && $(this).attr("selectionid")== "-1"){
                    //new field value
                    CustomFieldValuesCollection.push({ SelectionID:$(this).attr("selectionid"),CustomFieldValueID:$(this).val()});
                }//else is no change
            });
            toReturn.push({
                TravelTripID:traveltripID,
                GroupID:$("#addEditTripDialog").find("input[name=GroupID]").val(),
                Segment:$("#addEditTripDialog").find(":input[name=Segment]").val(), 
                SegmentName:$("#addEditTripDialog").find(":input[name=Segment] :selected").text(),         
                PerformingOrgName:$("#addEditTripDialog").find("input[name=PerformingOrgName]").val(),
                PerformingOrgID:$("#addEditTripDialog").find("input[name=PerformingOrgID]").val(),
                ModeName:$("#addEditTripDialog").find(":input[name=Mode] :selected").text(),
                Mode:$("#addEditTripDialog").find(":input[name=Mode]").val(),
                DepartureName:$("#addEditTripDialog").find(":input[name=DepartureID] :selected").text(),
                Departure:$("#addEditTripDialog").find(":input[name=DepartureID]").val(),
                DestinationName:$("#addEditTripDialog").find(":input[name=DestinationLoc] :selected").text(),
                Destination:$("#addEditTripDialog").find("input[name=DestinationID]").val(),
                PerDiemID:$("#addEditTripDialog").find("input[name=PerDiemID]").val(),
                SystemTripID:$("#addEditTripDialog").find("input[name=SystemTripID]").val(),
                TripDate:tripDateString,
                Purpose:$("#addEditTripDialog").find("input[name=Purpose]").val(),
                numOfPeople:$("#addEditTripDialog").find("input[name=numOfPeople]").val(),
                numOfDays:$("#addEditTripDialog").find("input[name=numOfDays]").val(),
                numOfTrips:$("#addEditTripDialog").find("input[name=numOfTrips]").val(),
                CustomFieldValues:CustomFieldValuesCollection
            });

            traveltripID = --TravelTripsWidget.newItemCount;
            var TempTripDate = tripDateString.toDate();
            TempTripDate.addMonths(monthsToAdd)
            tripDateString = TempTripDate.toFormattedString();
        }
        return toReturn;
    };

    TravelTripsWidget.clearDialogData = function(){
        $("#addEditTripDialog :input").val("");
        $("#addEditTripDialog .default-text").removeClass('display-none');
        $("#addEditTripDialog .default-text").removeClass('display-none');
        $("#addEditTripDialog .occurenceRows").hide();
        $("#addEditTripDialog :input[name=makeOccurences]").prop("checked", false);
        $("#addEditTripDialog").find("button[name='save-button']").addClass("disabled");
       
    };
    TravelTripsWidget.RemoveCostFromTravel = function(tripID){
        var cost = 0;
        $('#TravelTrips tr').each(function() {
            if(this.getAttribute("pkid") === tripID)
            {
                cost = this.lastElementChild.getAttribute("cost");
                return false;
            }
        });
        $(document).trigger(summaryGridTravelHoursUpdatedEvent,
                 { LaborTypeID: "Travel",
                     DeltaCost: (parseFloat(cost/100)),
                     category: travelElementCostType
                     });
    };

    TravelTripsWidget.updateRow = function(rowInput){
      
        TravelTripsWidget.RemoveCostFromTravel(rowInput.TravelTripID)
       
        $.ajax({
            type: 'POST',
            url: calculateTravelTripsUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({TravelTrip:rowInput}),
            success: function( cost ) {
                var row = $("table#TravelTrips tbody").find("tr[pkid="+ rowInput.TravelTripID +"]");
                      
                $(document).trigger(summaryGridHoursUpdatedEvent,
                    { LaborTypeID: "Travel",
                        DeltaCost: (parseFloat(cost.Status/100)),
                        category: travelElementCostType
                        });
               
        row.find("td.GroupID a").text(rowInput.GroupID);
        row.find("td.GroupID").attr("groupid", rowInput.GroupID);
        row.find("td.GroupID").find("input[name=customField]").remove();
        row.find("td.GroupID").append(TravelTripsWidget.generateCustomFieldInputs(rowInput.CustomFieldValues));
        row.find("td.Segment a").text(rowInput.SegmentName);
        row.find("td.Segment").attr("segment", rowInput.Segment);
        row.find("td.PerfOrg a").text(rowInput.PerformingOrgName);
        row.find("td.PerfOrg").attr("perforgname", rowInput.PerformingOrgName);
        row.find("td.PerfOrg").attr("perforgid", rowInput.PerformingOrgID);
        row.find("td.Mode a").text(rowInput.ModeName);
        row.find("td.Mode").attr("modename", rowInput.ModeName);
        row.find("td.Mode").attr("modeid", rowInput.Mode);
        row.find("td.DepartureName a").text(rowInput.DepartureName);
        row.find("td.DepartureName").attr("departurename", rowInput.DepartureName);
        row.find("td.DepartureName").attr("departureid", rowInput.Departure);
        row.find("td.DestinationName a").text(rowInput.DestinationName);
        row.find("td.DestinationName").attr("destinationname", rowInput.DestinationName);
        row.find("td.DestinationName").attr("destinationid", rowInput.DestinationID);
        row.find("td.DestinationName").attr("perdiemid", rowInput.PerDiemID);
        row.find("td.DestinationName").attr("systemtripid", rowInput.SystemTripID);
        row.find("td.Purpose").text(rowInput.Purpose);
        row.find("td.Purpose").attr("purpose",rowInput.Purpose);
        row.find("td.TripDate").text(rowInput.TripDate);
        row.find("td.TripDate").attr("tripdate",rowInput.TripDate);
        row.find("td.numOfTrips").text(rowInput.numOfTrips);
        row.find("td.numOfTrips").attr("numoftrips",rowInput.numOfTrips);
        row.find("td.numOfPeople").text(rowInput.numOfPeople);
        row.find("td.numOfPeople").attr("numofpeople",rowInput.numOfPeople);
        row.find("td.numOfDays").text(rowInput.numOfDays);
        row.find("td.numOfDays").attr("numofdays",rowInput.numOfDays);
        row.find("td.costOfTrip").text(Helper.addCommas((parseFloat(cost.Status)/100).toFixed(2),"$"));
        row.find("td.costOfTrip").attr("cost", cost.Status);
    }
    });               
    };
        
    TravelTripsWidget.populateTripCustomFields = function(customFieldValue){
        var found= false;
        var tripCustomFields = $("#addEditTripDialog").find("[id*=TaskElementCustomFieldID]");
        for(var x = 0, len = tripCustomFields.length; x< len; x++)
        {
            if(found==true)
            {
                break;
            }
            var customFieldDropdown =tripCustomFields[x].getElementsByTagName('select')[0];
            var listOptions = customFieldDropdown.options;
            for (var i = 0, listlen = listOptions.length; i < listlen; i++){
                {
                    if(listOptions[i].value==customFieldValue.val())
                    {
                        customFieldDropdown.value=customFieldValue.val();
                        customFieldDropdown.setAttribute("selectionid", customFieldValue.attr("selectionid"));
                        customFieldDropdown.setAttribute("updatedatelong", customFieldValue.attr("updatedatelong"));
                        found = true;
                        break;
                    }
                }
            }
        }};
        
    TravelTripsWidget.populateEdit = function(rowInput){
        var row = $(rowInput).parents("tr");
        var gpid = (row.find("td.GroupID").attr("groupid") == 0)?"":row.find("td.GroupID").attr("groupid"); 
        $("#addEditTripDialog").find("input[name=TravelTripID]").val(row.attr("pkid"));
        $("#addEditTripDialog").find("input[name=GroupID]").val(gpid);
        row.find("td.GroupID").find("input[name=customField]").each(function(){
            TravelTripsWidget.populateTripCustomFields($(this));
        });
        $("#addEditTripDialog").find(":input[name=Segment]").val(row.find("td.Segment").attr("Segment"));
        $("#addEditTripDialog").find("input[name=PerformingOrgName]").val(row.find("td.PerfOrg").attr("perforgname"));
        $("#addEditTripDialog").find("input[name=PerformingOrgID]").val(row.find("td.PerfOrg").attr("perforgid"));
        $("#addEditTripDialog").find("input[name=PerformingOrgID]").attr("perforg", row.find("td.PerfOrg").attr("perforgname"));
        $("#addEditTripDialog").find("input[name=SystemTripID]").val(row.find("td.DestinationName").attr("systemtripid"));
        $("#addEditTripDialog").find("input[name=Purpose]").val(row.find("td.Purpose").attr("purpose"));
        $("#addEditTripDialog").find("input[name=TripDate]").val(row.find("td.TripDate").attr("tripdate"));
        $("#addEditTripDialog").find("input[name=numOfPeople]").val(row.find("td.numOfPeople").attr("numofpeople"));
        $("#addEditTripDialog").find("input[name=numOfDays]").val(row.find("td.numOfDays").attr("numofdays"));
        $("#addEditTripDialog").find("input[name=numOfTrips]").val(row.find("td.numOfTrips").attr("numoftrips"));
        $("#addEditTripDialog .default-text").addClass('display-none');
        var departureValue = row.find("td.DepartureName").attr("departurename");        
        var destinationValue = row.find("td.DestinationName").attr("destinationname");
        var modeValue = row.find("td.Mode").attr("modename");

        var departureID = $("#addEditTripDialog").find('select[name=DepartureID] option[text="' + departureValue + '"]').prop('value');
        $("#addEditTripDialog").find("select[name=DepartureID]").val(departureID);

        TravelTripsWidget.filterTrips($("#addEditTripDialog").find("select[name=DepartureID]"), function() {
                
            var destinationID = $("#addEditTripDialog").find('select[name=DestinationLoc] option[text="' + destinationValue + '"]').prop('value');
                 
            $("#addEditTripDialog").find("select[name=DestinationLoc]").val(destinationID);
                    
            TravelTripsWidget.filterTrips($("#addEditTripDialog").find("select[name=DestinationLoc]"), function() { 
                 
                var modeID = $("#addEditTripDialog").find("select[name=Mode] option[text='" + modeValue + "']").prop('value');
                $("#addEditTripDialog").find(":input[name=Mode]").val(modeID);
                TravelTripsWidget.filterTrips($("#addEditTripDialog").find("select[name=Mode]"));
            });
        });
    };

    TravelTripsWidget.generateCustomFieldInputs = function(customFields){
        var toReturn ="";
        for(var x = 0; x<customFields.length;x++){
            toReturn = toReturn.concat('<input type="hidden" name="customField" value="'+customFields[x].CustomFieldValueID+'"updatedatelong="'+customFields[x].UpdateDateLong+'" selectionid="'+customFields[x].SelectionID+'">');
        }
        return toReturn;
    };

    TravelTripsWidget.createRow = function(pkid, customFields){
        $("table#TravelTrips tbody").append(
                '<tr pkid="'+pkid+'">'+
                '<td class="delete"><input type=checkbox /></td>'+
                '<td class="GroupID"><a></a>'+
                TravelTripsWidget.generateCustomFieldInputs(customFields) +
                '</td>'+
                '<td class="Segment"><a></a></td>'+
                '<td class="PerfOrg"><a></a></td>'+
                '<td class="Mode"><a></a></td>'+
                '<td class="DepartureName"><a></a></td>'+
                '<td class="DestinationName"><a></a></td>'+
                '<td class="Purpose"><a></a></td>'+
                '<td class="TripDate"></td>'+
                '<td class="numOfTrips"></td>'+
                '<td class="numOfPeople"></td>'+
                '<td class="numOfDays"></td>'+
                '<td class="costOfTrip" cost="0"></td>'+
                '</tr>');
        TravelTripsWidget.refreshModule();
    }

    TravelTripsWidget.generateUpdateRow = function(dialogInputs){
        
        for(var x in dialogInputs){
            var dialogInput = dialogInputs[x];
            //delete from json if it is in there and then add it again.
            for(var w in TravelTripsWidget.data){
                if(TravelTripsWidget.data[w].TravelTripID==dialogInput.TravelTripID){
                    TravelTripsWidget.data.splice(w, 1);
                }
            }
            TravelTripsWidget.data.push(dialogInput);
            //if there is no row for it then create a row for it and then update.
            if($("table#TravelTrips").find("tr[pkid="+dialogInput.TravelTripID+"]").length == 0)
            {
                TravelTripsWidget.createRow(dialogInput.TravelTripID,dialogInput.CustomFieldValues);
            }
        
            TravelTripsWidget.updateRow(dialogInput);
        }
    }

    TravelTripsWidget.filterTrips = function(selectChanged, runOnComplete) {
           
        var firstSelect = $('select[order=first]');
        var secondSelect = $('select[order=second]');
        var thirdSelect = $('select[order=third]');
        
        if (typeof(selectChanged) != 'undefined') {
            
            if (selectChanged.val() == "") {
                if (selectChanged.attr('order') == 'first') {
                    secondSelect.val("");
                    secondSelect.attr('order', '');
                    thirdSelect.val("");
                    thirdSelect.attr('order', '');
                } else if (selectChanged.attr('order') == 'second') {
                    thirdSelect.val("");
                    thirdSelect.attr('order', '');
                }
                selectChanged.attr('order', '');

            } else if (selectChanged.attr('order') == "" || selectChanged.attr('order') == undefined) {
                if (secondSelect.length > 0) {
                    selectChanged.attr('order', 'third')
                } else if (firstSelect.length > 0) {
                    selectChanged.attr('order', 'second')
                } else {
                    selectChanged.attr('order', 'first')
                }
            } else if (selectChanged.attr('order') == 'first') {
                secondSelect.val("");
                secondSelect.attr('order', '');
                thirdSelect.val("");
                thirdSelect.attr('order', '');
            } else if (selectChanged.attr('order') == 'second') {
                thirdSelect.val("");
                thirdSelect.attr('order', '');
            }
        } else {
            firstSelect.attr('order', '');
            secondSelect.attr('order', '');
            thirdSelect.attr('order', '');
        }


        var departureSelect = $('select[name=DepartureID]');
        var destinationSelect = $('select[name=DestinationLoc]');
        var modeSelect = $('select[name=Mode]');

        var destinationValue = destinationSelect.find(':selected').attr('text');

        var qualificationText = "";
        var destinationText = "";
        if (destinationValue != null){
            var firstHyphen = destinationValue.indexOf(" - ");
            //Check if there exists a hyphen meaning there is a qualification.
            if(firstHyphen != -1)
            {
                var destinationArray = [destinationValue.substring(0, firstHyphen), destinationValue.substring(firstHyphen+3, destinationValue.length)];
                destinationText = destinationArray[0];
                qualificationText = destinationArray[1];
            }else{
                destinationText = destinationValue;
            }
        }

        // get trip data
        data = {};
        data.departure = departureSelect.find(':selected').text();
        data.destination = destinationText;
        data.mode = modeSelect.find(':selected').text();
        data.qualification = qualificationText;

        if (data.departure == "" || data.destination == "" || data.mode == "") {
            departureSelect.prop('disabled', true);
            destinationSelect.prop('disabled', true);
            modeSelect.prop('disabled', true);
            $('#PageLoading').removeClass('display-none');

            $.ajax({
                type: 'POST',
                url: getFilteredTripDataUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify(data),
                success: function( returned ) {
                    if (data.departure == "") {
                        departureSelect.html(returned.Departures);
                    }
                    if (data.destination == "") {
                        destinationSelect.html(returned.Destinations);
                    }
                    if (data.mode == "") {
                        modeSelect.html(returned.Modes);
                    }

                    departureSelect.prop('disabled', false);
                    destinationSelect.prop('disabled', false);
                    modeSelect.prop('disabled', false);
                
                    $('#PageLoading').addClass('display-none');

                    if (typeof runOnComplete == 'function') {
                        runOnComplete();
                    }
                }
            });
        }
    };

    TravelTripsWidget.perforgAutoCompleteSelect = function(event, ui){
        TravelTripsWidget.PerfOrgSelect(ui.item.ID, ui.item.value);
    };

    TravelTripsWidget.PerfOrgSelect = function(id, perforg){
        var perfOrgInput = $("#addEditTripDialog input[name=PerformingOrgName]");
        var perfOrgIdInput = $("#addEditTripDialog input[name=PerformingOrgID]");
        $(perfOrgIdInput).attr("perforg", perforg)
        $(perfOrgIdInput).val(id);
        $(perfOrgInput).val(perforg);
        $(perfOrgInput).change();
        $(perfOrgIdInput).change();
        var formBeingSaved = $(perfOrgInput).parents("form")[0];
        var valBox = $("ul.validation-box", formBeingSaved);
        if (id == 0){
            $(valBox).find("li[name=PerformingOrg]").remove();
            valBox.append("<li name='PerformingOrg'>Performing Org is invalid</li>");
            $(valBox).show();
            $(valBox).fadeIn(500);
            TravelTripsWidget.focusValidation();
        }
        else{
            $(valBox).find("li[name=PerformingOrg]").remove();
            if ($(valBox).find("li").length == 0) {
                $(valBox).hide();
            }
        }
        TravelTripsWidget.refreshModule();
    };

}

function AfterDomLoadTravelTripsWidget(TravelTripsWidget, workspace, getTripIdUrl) {
    TravelTripsWidget.afterDOMLoad();
    TravelTripsWidget.AddEditTravelTripDialog.Element = $("#addEditTripDialog");
    TravelTripsWidget.AddEditTravelTripDialog.Params = { title: "Add Trip", modal: true, resizable: false, width: 640 }
    TravelTripsWidget.InitializeDialog(TravelTripsWidget.AddEditTravelTripDialog);
    
    $("#addEditTripDialog div.default-text").click(function () { TravelTripsWidget.manageDefaultText($(this)) });
    $("#addEditTripDialog input[name=TripDate]").focusin(function () {
        TravelTripsWidget.manageDefaultText($(this));
    });

    $("#addEditTripDialog input[name=PerformingOrgName]").autocomplete({
        source: BOEDetails.autocompletePerformingOrgs,
        autoFocus: true,
        minLength: 2,
        focus: function (event, ui) {
        },
        select: TravelTripsWidget.perforgAutoCompleteSelect
    }).on('blur', function (e) {
        var that = $(this);
        var perfOrgIdInput = $("#addEditTripDialog input[name=PerformingOrgID]");
        if (that.val() != perfOrgIdInput.attr("perforg")) {
            if ($('.ui-autocomplete li:visible').length > 0) {
                var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                that.val(item.value);
                TravelTripsWidget.PerfOrgSelect(item.id, item.value);
            }
            else {
                TravelTripsWidget.PerfOrgSelect(0, that.val());
            }
        }
    });

    TravelTripsWidget.registerForEvent('TravelTotalUpdated', function (e, LTData) {
        var HS = $("table#TravelTrips tr[pkid='" + LTData.ltid + "']").find("input[name=Cost]");
        $(HS).val(LTData.total);
        $(HS).trigger("change");
    });

    $(".TravelTrips .travelbuttons button[name='add-button']:not(.disabled)").click(function () {
        TravelTripsWidget.clearDialogData();
        TravelTripsWidget.AddEditTravelTripDialog.Element.find("input[name=TravelTripID]").val(TravelTripsWidget.newItemCount);
        TravelTripsWidget.filterTrips();
        TravelTripsWidget.OpenDialogAfterInitialize(TravelTripsWidget.AddEditTravelTripDialog);
        TravelTripsWidget.clearValidationBox($("#addEditTripDialog ul.validation-box"));
    });

    TravelTripsWidget.registerForLiveEvent('click', ".TravelTrips .travelbuttons button[name='delete-button']", function () {
        TravelTripsWidget.setItemsForDelete();
        TravelTripsWidget.setDirty();
        if ($(".TravelTrips td.delete :input:checked").length == 0) {
            $(".TravelTrips .travelbuttons button[name='delete-button']").addClass("disabled");
        } else {
            $(".TravelTrips .travelbuttons button[name='delete-button']").removeClass("disabled");
        }
    });

    TravelTripsWidget.registerForLiveEvent('click', ".TravelTrips table .delete input", function () {
        if ($(".TravelTrips td.delete :input:checked").length == 0) {
            $(".TravelTrips .travelbuttons button[name='delete-button']").addClass("disabled");
        } else {
            $(".TravelTrips .travelbuttons button[name='delete-button']").removeClass("disabled");
        }
    });

    $(".TravelTrips .costOfTrip").each(function () {
        $(this).text(Helper.addCommas($(this).text().trim(), "$"));
    });

    TravelTripsWidget.getElementsForTripSave = function () {
        TravelTripsWidget.clearValidationBox($("#addEditTripDialog ul.validation-box"));
        ShowLoadingBox();
        var formContext = $('#addEditTripDialogForm');
        var destinationValue = $('select[name=DestinationLoc]', formContext).val();
        destinationValue = destinationValue.replace("(", "");
        destinationValue = destinationValue.replace(")", "");
        var destinationIDs = destinationValue.split(", ");
        $('input[name=DestinationID]', formContext).val(destinationIDs[0]);
        $('input[name=PerDiemID]', formContext).val(destinationIDs[1]);

        TravelTripsWidget.InitializeValidation();

        var departureSelect = $('select[name=DepartureID]', formContext);
        var destinationSelect = $('select[name=DestinationLoc]', formContext);
        var modeSelect = $('select[name=Mode]', formContext);
        var segmentSelect = $('select[name=Segment]', formContext);

        var destinationLabel = destinationSelect.find(':selected').attr('text');
        var qualificationText = "";
        var destinationText = "";
        if (destinationLabel != null) {
            var firstHyphen = destinationLabel.indexOf(" - ");
            //Check if there exists a hyphen meaning there is a qualification.
            if (firstHyphen != -1) {
                var destinationArray = [destinationLabel.substring(0, firstHyphen), destinationLabel.substring(firstHyphen + 3, destinationLabel.length)];
                destinationText = destinationArray[0];
                qualificationText = destinationArray[1];
            } else {
                destinationText = destinationLabel;
            }
        }

        var e = document.getElementById("Segment-select");
        var createmultiple = false;
        
        if ($("#addEditTripDialog").find("input[name=makeOccurences]").prop("checked")) {
            createmultiple = true;
        }
        // get trip data
        data = {};
        data.GroupID = $('input[name=GroupID]', formContext).val();
        data.workspace = workspace;
        data.Segment = e.options[e.selectedIndex].text;
        data.departureName = departureSelect.find(':selected').text();
        data.destinationName = destinationSelect.find(':selected').text();
        data.destination = destinationText;
        data.Mode = modeSelect.find(':selected').text();
        data.qualification = qualificationText;
        data.Purpose = $('input[name=Purpose]', formContext).val();
        data.PerformingOrgName = $('input[name=PerformingOrgName]', formContext).val();
        data.PerformingOrgID = $('input[name=PerformingOrgID]', formContext).val();
        data.TripDate = $('input[name=TripDate]', formContext).val();
        data.numOfPeople = $('input[name=numOfPeople]', formContext).val();
        data.numOfDays = $('input[name=numOfDays]', formContext).val();
        data.numOfTrips = $('input[name=numOfTrips]', formContext).val();
        data.startDate = $('#StartDate', formContext).val();
        data.endDate = $('#EndDate', formContext).val();
        data.numOfOccurrences = $('input[name=numOfOccurrences]', formContext).val();
        data.Interval = $('input[name=Interval]', formContext).val();
        data.CreateMultiple = createmultiple;
        data.CustomFieldValues = [];
        $("#addEditTripDialogForm :input.customField").each(function () {
            if ($(this).val() == "" && $(this).attr("selectionid") != "-1") {
                //if the field value was deleted
                data.CustomFieldValues.push({ SelectionID: $(this).attr("selectionid"), UpdateDateLong: $(this).attr("UpdateDateLong") });
            } else if ($(this).val() != "" && $(this).attr("selectionid") != "-1") {
                //if the field value was updated
                data.CustomFieldValues.push({ SelectionID: $(this).attr("selectionid"), CustomFieldValueID: $(this).val(), UpdateDateLong: $(this).attr("UpdateDateLong") });
            } else if ($(this).val() != "" && $(this).attr("selectionid") == "-1") {
                //new field value
                data.CustomFieldValues.push({ SelectionID: $(this).attr("selectionid"), CustomFieldValueID: $(this).val() });
            }//else is no change
        });
        return data;
    };

    TravelTripsWidget.LoadExistingTrips = function (trip) {
        var toReturn = []
        var CustomFieldValuesCollection = [];
        for (var x = 0; x < trip.CustomFieldValues.length; x++) {
            CustomFieldValuesCollection.push({ SelectionID: trip.CustomFieldValues[x].SelectionID, CustomFieldValueID: trip.CustomFieldValues[x].CustomFieldValueID, UpdateDateLong: trip.CustomFieldValues[x].UpdateDateLong })
        }
        TravelTripsWidget.data.push({
            TravelTripID: trip.TravelTripID,
            GroupID: trip.GroupID,
            Segment: trip.Segment,
            SegmentName: trip.SegmentName,
            PerformingOrgName: trip.PerformingOrgName,
            PerformingOrgID: trip.PerformingOrgID,
            ModeName: trip.ModeName,
            Mode: trip.ModeID,
            DepartureName: trip.DepartureName,
            Departure: trip.DepartureID,
            DestinationName: trip.DestinationName,
            Destination: trip.DestinationID,
            PerDiemID: trip.PerDiemID,
            SystemTripID: trip.SystemTripID,
            TripDate: trip.TripDateToString,
            Purpose: trip.Purpose,
            numOfPeople: trip.numOfPeople,
            numOfDays: trip.numOfDays,
            numOfTrips: trip.numOfTrips,
            CustomFieldValues: CustomFieldValuesCollection
        })
    };

    TravelTripsWidget.registerForLiveEvent('click', ".TravelTrips a", function () {
        TravelTripsWidget.clearDialogData();
        TravelTripsWidget.populateEdit(this);
        TravelTripsWidget.clearValidationBox($("#addEditTripDialog ul.validation-box"));
        TravelTripsWidget.OpenDialogAfterInitialize(TravelTripsWidget.AddEditTravelTripDialog);
    });

    $(".TravelTrips th.delete input").click(function () {
        $(".TravelTrips td.delete input").prop("checked", $(".TravelTrips th.delete input").prop("checked"));
    });

    TravelTripsWidget.registerForLiveEvent('click', "#Add-AddTrip:not(.disabled)", function () {
        $('#Loader-AddTrip').removeClass('display-none');
        $('#Add-AddTrip').addClass('display-none');

        var data = TravelTripsWidget.getElementsForTripSave();

        TravelTripsWidget.ajaxRequest({
            type: 'POST',
            url: getTripIdUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(data),
            success: function (returned) {
                $('#Loader-AddTrip').addClass('display-none');
                $('#Add-AddTrip').removeClass('display-none');

                HideLoadingBox();
                $('input[name=SystemTripID]').val(returned.Status);

                TravelTripsWidget.generateUpdateRow(TravelTripsWidget.gatherDialogData());
                TravelTripsWidget.CloseDialog(TravelTripsWidget.AddEditTravelTripDialog);
                TravelTripsWidget.setDirty();
                TravelTripsWidget.newItemCount--;
                TravelTripsWidget.clearDialogData();
            },
            error: function () {
                HideLoadingBox();
                //do something
                $('#Loader-AddTrip').addClass('display-none');
                $('#Add-AddTrip').removeClass('display-none');
            }
        }, $('#Add-AddTrip'));

        HideLoadingBox();
    });

    $('select[name=DepartureID]').change(function () { TravelTripsWidget.filterTrips($(this)); });
    $('select[name=DestinationLoc]').change(function () { TravelTripsWidget.filterTrips($(this)); });
    $('select[name=Mode]').change(function () { TravelTripsWidget.filterTrips($(this)); });

    TravelTripsWidget.registerForLiveEvent('click', "#AddAnother-AddTrip:not(.disabled)", function () {
        $('#Loader-AddTrip').removeClass('display-none');
        $('#AddAnother-AddTrip').addClass('display-none');

        var data = TravelTripsWidget.getElementsForTripSave();

        TravelTripsWidget.ajaxRequest({
            type: 'POST',
            url: getTripIdUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(data),
            success: function (returned) {
                $('#Loader-AddTrip').addClass('display-none');
                $('#AddAnother-AddTrip').removeClass('display-none');

                HideLoadingBox();
                $('input[name=SystemTripID]').val(returned.Status);

                TravelTripsWidget.generateUpdateRow(TravelTripsWidget.gatherDialogData());
                TravelTripsWidget.setDirty();
                TravelTripsWidget.newItemCount--;
                TravelTripsWidget.clearDialogData();
                $("#addEditTripDialog").find("input[name=TravelTripID]").val(TravelTripsWidget.newItemCount);
            },
            error: function () {
                $('#Loader-AddTrip').addClass('display-none');
                $('#AddAnother-AddTrip').removeClass('display-none');
                HideLoadingBox();
            }
        }, $('#AddAnother-AddTrip'));
        HideLoadingBox();
    });


    $(TravelTripsWidget.AddEditTravelTripDialog.Element).find(":input").keyup(function () {
        $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("button[name='save-button']").removeClass("disabled");

    })

    $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("#Segment-select").change(function () {
        $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("button[name='save-button']").removeClass("disabled");
    })

    $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("#DepartureID-select").change(function () {
        $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("button[name='save-button']").removeClass("disabled");
    })

    $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("#DestinationLoc-select").change(function () {
        $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("button[name='save-button']").removeClass("disabled");
    })

    $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("#Mode-select").change(function () {
        $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("button[name='save-button']").removeClass("disabled");
    })

    TravelTripsWidget.applyReadOnly();

    if (TravelTripsWidget.isReadOnly()) {
        $("#TravelTripsWidgetContainer .travelbuttons").addClass('display-none');
        $('#TravelTrips td a').each(function () {
            $(this).addClass('display-none');
            var text = $(this).text();
            $(this).after('<span>' + text + '</span>');
        });
    }

    $(".occurenceRows").hide();

    $("input[name=makeOccurences]").change(function () {
        if ($(this).prop("checked")) {
            $(".occurenceRows").show();
        } else {
            $(".occurenceRows").hide();
        }
    });

    SortableGrid('.TravelTrips');

    TravelTripsWidget.registerForEvent("AllTravelWidgetLoaded", function (e) {
        TravelTripsWidget.refreshModule();
    });

    TravelTripsWidget.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { TravelTripsWidget.cleanDirty(); });

    $('.select-perforg.popup-div', TravelTripsWidget.context).on('click', '.perforg-option', function () {
        var option = $(this);
        TravelTripsWidget.PerfOrgSelect(option.attr("perforgid"), option.attr('perforgname'));
        $(TravelTripsWidget.AddEditTravelTripDialog.Element).find("button[name='save-button']").removeClass("disabled");
    });

    $(document).trigger('TravelWidgetLoaded', "TravelTrips");

    var options = {
        propagateOnClick: true,
        button: 'div.popup-div-button.for-perf-orgs',
        onShow: function (button) { TravelTripsWidget.PerfOrgPopupButton = $(button); },
        leftOffSet: -105
    };
    $('div.popup-div.select-perforg').genPopUp(options);

    if (!TravelTripsWidget.isReadOnly()) {
        var html = "";

        $.each(BOEDetails.WSPerfOrgs, function (i, item) {
            html += '<div class="perforg-option" perforgid="' + item.PerformingOrgID + '" perforgname="' + item.PerformingOrgName + '" perforgdesc="' + item.PerformingOrgDesc + '">' + item.PerformingOrgName + '-' + item.PerformingOrgDesc + '</div>';
        });
        TravelTripsWidget.getElement("div.select-perforg.popup-div").html(html);
    }
}