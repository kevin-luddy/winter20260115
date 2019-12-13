<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.TripForTravelModelView>" %>

<script type="text/javascript">

    var ManageTripsForTravelWidget = new Widget('TripElementForm', '<%: ViewData["READONLY"] %>'.isTrue());

    ManageTripsForTravelWidget.TripElementDialog = {};
    ManageTripsForTravelWidget.TripNotUniqueDialog = {};
    ManageTripsForTravelWidget.ImportTripsDialog = {};

    ManageTripsForTravelWidget.Initialize = function () {
        ManageTripsForTravelWidget.TripElementDialog.Element = $("#TripElementDialog");
        ManageTripsForTravelWidget.TripElementDialog.Params = { width: 580, modal: true, resizable: false, draggable: true };
        ManageTripsForTravelWidget.InitializeDialog(ManageTripsForTravelWidget.TripElementDialog);

        ManageTripsForTravelWidget.TripNotUniqueDialog.Element = $("#TripNotUniqueDialog");
        ManageTripsForTravelWidget.TripNotUniqueDialog.Params = { title: "Add Trip Warning", width: 300, modal: true, resizable: false, draggable: true };
        ManageTripsForTravelWidget.InitializeDialog(ManageTripsForTravelWidget.TripNotUniqueDialog);

        ManageTripsForTravelWidget.ImportTripsDialog.Element = $("#ImportTripsDialog");
        ManageTripsForTravelWidget.ImportTripsDialog.Params = { title: "Import Trips", width: 675, modal: true, resizable: false, draggable: true, height: 490 };
        ManageTripsForTravelWidget.InitializeDialog(ManageTripsForTravelWidget.ImportTripsDialog);
    };

    ManageTripsForTravelWidget.BindEvents = function () {

        ManageTripsForTravelWidget.registerForLiveEvent('click',
            '#TripElementDialog-Save:not(.disabled)', ManageTripsForTravelWidget.Save);

        ManageTripsForTravelWidget.registerForLiveEvent('click',
            '#TripElementDialog-SaveAddAnother:not(.disabled)', ManageTripsForTravelWidget.SaveAddAnother);

        $('#AddButton-Trips').click(ManageTripsForTravelWidget.AddTrip);

        ManageTripsForTravelWidget.registerForLiveEvent('click', '#DeleteButton-Trips:not(.disabled)', function () { $(document).trigger('GET_DELETED_TRIPS'); });

        ManageTripsForTravelWidget.registerForEvent('DELETE_TRIPS', ManageTripsForTravelWidget.DeleteSelectedTrips);

        ManageTripsForTravelWidget.registerForEvent('EDIT_TRIP', ManageTripsForTravelWidget.EditTripFromRow);
        ManageTripsForTravelWidget.registerForEvent('RELOAD_GRID', ManageTripsForTravelWidget.ReloadGridData);

        $('#Import-Trips').click(ManageTripsForTravelWidget.Import);
        $('#Export-Trips, #ImportTrips-ExportExisting').click(ManageTripsForTravelWidget.ExportData);
        $('#ImportTrips-File').change(ManageTripsForTravelWidget.ValidateFileInput);
        $('#ImportTrips-ImportButton').click(ManageTripsForTravelWidget.SubmitUploadForm);

        ManageTripsForTravelWidget.registerForEvent('ENABLE_TRIPS_DELETE_BUTTON', function (event, data) {
            if (data != undefined && data == false) {
                $('#DeleteButton-Trips').addClass('disabled');
            }
            else {
                $('#DeleteButton-Trips').removeClass('disabled');
            }
        });

        ManageTripsForTravelWidget.registerForEvent("PAGE_TRIP_RESULTS", ManageTripsForTravelWidget.PageTripResults);
        ManageTripsForTravelWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageTripsForTravelWidget.cleanDirty(); });

        $('#TripElementDialog #DepartureLocationName').autocomplete({
            source: ManageTripsForTravelWidget.AutocompleteDepartureLocation,
            minLength: 2,
            delay: 400,
            select: function (event, ui) {
                ManageTripsForTravelWidget.GetDepartureLocationDetails(ui.item.value, $('#TripElementDialog #DepartureLocationCode').val());
            }
        }).on('blur', function (event) {
            ManageTripsForTravelWidget.GetDepartureLocationDetails(this.value, $('#TripElementDialog #DepartureLocationCode').val());
        });

        $('#TripElementDialog #DepartureLocationCode').autocomplete({
            source: ManageTripsForTravelWidget.AutocompleteDepartureCode,
            minLength: 0,
            delay: 400,
            select: function (event, ui) {
                $('#TripElementDialog #DepartureLocationCode').attr("autofilled", "true");
                ManageTripsForTravelWidget.GetDepartureLocationDetails($('#TripElementDialog #DepartureLocationName').val(), ui.item.value);
            }
        }).focusin(function () {
            if ($(this).attr("autofilled") == "true") {
                $(this).attr("autofilled", "false");
            } else {
                $(this).autocomplete('search');
            }
        }).on('blur', function (event) {
            ManageTripsForTravelWidget.GetDepartureLocationDetails($('#TripElementDialog #DepartureLocationName').val(), this.value);
        });

        $('#TripElementDialog #DestinationLocationName').autocomplete({
            source: ManageTripsForTravelWidget.AutocompleteDestinationLocation,
            minLength: 2,
            delay: 400,
            select: function (event, ui) {
                ManageTripsForTravelWidget.GetDestinationLocationDetails(ui.item.value, $('#TripElementDialog #DestinationLocationCode').val());
                var perDiemDestValue = $('#TripElementDialog #PerDiemDestination').val();
                if (perDiemDestValue === "")
                {
                    perDiemDestValue = ui.item.value;
                }
                ManageTripsForTravelWidget.GetPerDiemLocationDetails(perDiemDestValue, $('#TripElementDialog #Qualification').val());
            }
        }).on('blur', function (event) {
            ManageTripsForTravelWidget.GetDestinationLocationDetails(this.value, $('#TripElementDialog #DestinationLocationCode').val());
            var perDiemDestValue = $('#TripElementDialog #PerDiemDestination').val();
            if (perDiemDestValue === "") {
                perDiemDestValue = this.value;
            }
            ManageTripsForTravelWidget.GetPerDiemLocationDetails(perDiemDestValue, $('#TripElementDialog #Qualification').val());
        });

        $('#TripElementDialog #DestinationLocationCode').autocomplete({
            source: ManageTripsForTravelWidget.AutocompleteDestinationCode,
            minLength: 0,
            delay: 400,
            select: function (event, ui) {
                $('#TripElementDialog #DestinationLocationCode').attr("autofilled", "true");
                ManageTripsForTravelWidget.GetDestinationLocationDetails($('#TripElementDialog #DestinationLocationName').val(), ui.item.value);
                ManageTripsForTravelWidget.GetPerDiemLocationDetails(ui.item.value, $('#TripElementDialog #Qualification').val());
            }
        }).focusin(function () {
            if ($(this).attr("autofilled") == "true") {
                $(this).attr("autofilled", "false");
            } else {
                $(this).autocomplete('search');
            }
        }).on('blur', function (event) {
            ManageTripsForTravelWidget.GetDestinationLocationDetails($('#TripElementDialog #DestinationLocationName').val(), this.value);
            ManageTripsForTravelWidget.GetPerDiemLocationDetails(this.value, $('#TripElementDialog #Qualification').val());
        });

        $('#TripElementDialog #PerDiemDestination').autocomplete({
            source: ManageTripsForTravelWidget.AutocompletePerDiemLocation,
            minLength: 2,
            delay: 400,
            select: function (event, ui) {
                ManageTripsForTravelWidget.GetPerDiemLocationDetails(ui.item.value, $('#TripElementDialog #Qualification').val());
            }
        }).on('blur', function (event) {
            ManageTripsForTravelWidget.GetPerDiemLocationDetails(this.value, $('#TripElementDialog #Qualification').val());
        });

        $('#TripElementDialog #Qualification').autocomplete({
            source: ManageTripsForTravelWidget.AutocompleteQualification,
            minLength: 2,
            delay: 400,
            select: function (event, ui) {
                ManageTripsForTravelWidget.GetPerDiemLocationDetails($('#TripElementDialog #PerDiemDestination').val(), ui.item.value);
            }
        }).on('blur', function (event) {
            ManageTripsForTravelWidget.GetPerDiemLocationDetails($('#TripElementDialog #PerDiemDestination').val(), this.value);
        });

        $('#TripElementDialog #PerDiemNotes, #TripElementDialog #HotelRate, #TripElementDialog #MIERate').change(ManageTripsForTravelWidget.PerDiemFieldChanged);

        $('#TripNotUniqueDialog-OK').click(function () { ManageTripsForTravelWidget.CloseDialog(ManageTripsForTravelWidget.TripNotUniqueDialog); });

        $('#TripNotUniqueDialog-EditExisting').click(function () {
            ManageTripsForTravelWidget.EditTrip(JSON.parse($('#TripNotUniqueDialog-Trip').val()));
            ManageTripsForTravelWidget.CloseDialog(ManageTripsForTravelWidget.TripNotUniqueDialog);
        });

        ManageTripsForTravelWidget.registerForLiveEvent('click',
                '#ManageTripsForTravelGrid .search-magnify-glass-button', function (e) {
                    ManageTripsForTravelWidget.ReloadGridData();
                });
        ManageTripsForTravelWidget.registerForLiveEvent('keydown',
                '#ManageTripsForTravelGrid .search-box input', function (e) {
                    /****enter key to search */
                    var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
                    if (keyCode == 13) {
                        ManageTripsForTravelWidget.ReloadGridData();
                    }
                });

    };

    ManageTripsForTravelWidget.GenericAutocomplete = function (action, request, response, dataToSend) {

        if (dataToSend == undefined) {
            dataToSend = {};
        }

        dataToSend.searchTerm = request.term;

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%: WebConstants.CONTROLLER_ADMIN%>',
                action),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(dataToSend),
            success: function (returned) {
                response($.map(returned, function (item) {
                    return {
                        label: item,
                        value: item
                    }
                }));
            }
        });
    };

    ManageTripsForTravelWidget.AutocompleteDepartureLocation = function (request, response) {
        ManageTripsForTravelWidget.GenericAutocomplete('<%: WebConstants.ACTION_AUTOCOMPLETE_LOCATION_NAME%>', request, response);
    };

    ManageTripsForTravelWidget.AutocompleteDepartureCode = function (request, response) {
        ManageTripsForTravelWidget.GenericAutocomplete('<%: WebConstants.ACTION_AUTOCOMPLETE_DEP_LOCATION_CODE%>', request, response, { locationName: $('#TripElementDialog #DepartureLocationName').val() });
    };

    ManageTripsForTravelWidget.GetDepartureLocationDetails = function (locationName, locationCode) {
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%: WebConstants.CONTROLLER_ADMIN%>',
                '<%: WebConstants.ACTION_GET_LOCATION_DETAILS%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ name: locationName, code: locationCode }),
            success: function (location) {
                if (location == undefined) {
                    $('#TripElementDialog input.DepartureLocationID').val('-1');
                }
                else {
                    $('#TripElementDialog input.DepartureLocationID').val(location.Id);
                }
            }
        });
    };

    ManageTripsForTravelWidget.AutocompleteDestinationLocation = function (request, response) {
        ManageTripsForTravelWidget.GenericAutocomplete('<%: WebConstants.ACTION_AUTOCOMPLETE_LOCATION_NAME%>', request, response);
    };

    ManageTripsForTravelWidget.AutocompleteDestinationCode = function (request, response) {
        ManageTripsForTravelWidget.GenericAutocomplete('<%: WebConstants.ACTION_AUTOCOMPLETE_DES_LOCATION_CODE%>', request, response, { locationName: $('#TripElementDialog #DestinationLocationName').val() });
    };

    ManageTripsForTravelWidget.GetDestinationLocationDetails = function (locationName, locationCode)  {
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%: WebConstants.CONTROLLER_ADMIN%>',
                '<%: WebConstants.ACTION_GET_LOCATION_DETAILS%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ name: locationName, code: locationCode }),
            success: function (location) {
                if (location == undefined) {
                    $('#TripElementDialog #DestinationLocationID').val('-1');
                }
                else {
                    $('#TripElementDialog #DestinationLocationID').val(location.Id);

                    var perDiemLocation = $('#TripElementDialog #PerDiemDestination');
                    if (!$.trim(perDiemLocation.val()).length) {
                        perDiemLocation.val($('#TripElementDialog #DestinationLocationName').val());
                    }
                }
            }
        });
    };

    ManageTripsForTravelWidget.AutocompletePerDiemLocation = function (request, response) {
        ManageTripsForTravelWidget.GenericAutocomplete('<%: WebConstants.ACTION_AUTOCOMPLETE_PER_DIEM%>', request, response, { departureLocation: $('#TripElementDialog #DepartureLocationName').val(), destinationLocation: $('#TripElementDialog #DestinationLocationName').val() });
    };

    ManageTripsForTravelWidget.AutocompleteQualification = function (request, response) {
        ManageTripsForTravelWidget.GenericAutocomplete('<%: WebConstants.ACTION_AUTOCOMPLETE_QUALIFICATION%>', request, response);
    };


    ManageTripsForTravelWidget.GetPerDiemLocationDetails = function (destination, qualification) {
        ManageTripsForTravelWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%: WebConstants.CONTROLLER_ADMIN%>',
                '<%: WebConstants.ACTION_GET_PER_DIEM_LOCATION_DETAILS%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ name: destination, qualification: qualification }),
            success: function (location) {
                $('#TripElementDialog-PerDiemWarning').slideUp();

                if (location == undefined) {
                    $('#TripElementDialog #PerDiemID').val('-1');
                }
                else {
                    $('#TripElementDialog #PerDiemID').val(location.PerDiemID);
                    $('#TripElementDialog span.PerDiemLastUpdated').text(location.PerDiemLastUpdated);
                    $('#TripElementDialog #PerDiemNotes').val(location.PerDiemNotes);
                    $('#TripElementDialog #HotelRate').val(location.HotelRate);
                    $('#TripElementDialog #MIERate').val(location.MIERate);
                    $('#TripElementDialog #PerDiemUpdateDateLong').val(location.PerDiemUpdateDateLong);
                }
            }
        }, $('#TripElementDialog - Save'));
    }

    ManageTripsForTravelWidget.PerDiemFieldChanged = function () {
        var perDiemID = $('#TripElementDialog #PerDiemID').val();
        if (perDiemID != undefined && perDiemID.length && perDiemID > 0) {
            $('#TripElementDialog-PerDiemWarning').slideDown();
        }
    }

    ManageTripsForTravelWidget.ReloadGridData = function (e, sortParms) {
        $('#PageLoading').removeClass('display-none');

        var data = {};
        data.sortField = sortParms && sortParms.inSortField ? sortParms.inSortField : '';
        data.sortDirection = sortParms && sortParms.inSortDirection ? sortParms.inSortDirection : '';
        data.tripIDSearchText = $("#SearchText-TripID").val();
        data.departureSearchText = $("#SearchText-Departure").val();
        data.destinationSearchText = $("#SearchText-Destination").val();
        data.perDiemDestSearchText = $("#SearchText-PerDiemDest").val();
        data.modeSearchText = $("#SearchText-Mode").val();
        data.qualificationSearchText = $("#SearchText-Qualification").val();
        data.departureCodeSearchText = $("#SearchText-DepartureCode").val();
        data.destinationCodeSearchText = $("#SearchText-DestinationCode").val();
        var dataToSend = JSON.stringify(data);

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_TRIPS_GRID %>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function (response) {
                $('#TripsGridContent').html(response);
                $("#SearchText-TripID").val(data.tripIDSearchText);
                $("#SearchText-Departure").val(data.departureSearchText);
                $("#SearchText-Destination").val(data.destinationSearchText);
                $("#SearchText-PerDiemDest").val(data.perDiemDestSearchText);
                $("#SearchText-Mode").val(data.modeSearchText);
                $("#SearchText-Qualification").val(data.qualificationSearchText);
                $("#SearchText-DepartureCode").val(data.departureCodeSearchText);
                $("#SearchText-DestinationCode").val(data.destinationCodeSearchText);
                $('#PageLoading').addClass('display-none');
                ManageTripsForTravelWidget.CloseDialog(ManageTripsForTravelWidget.ImportTripsDialog);
            },
            error: function (response) {
                $('#PageLoading').addClass('display-none');
            }
        });
    }

    ManageTripsForTravelWidget.PageTripResults = function (event, data) {
        $('#PageLoading').removeClass('display-none');
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_PAGE_TRIP_RESULTS %>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: JSON.stringify(data),
            success: function (response) {
                $('#PageLoading').addClass('display-none');
                $('#TripsGridContent').html(response);
            },
            error: function (response) {
                $('#PageLoading').addClass('display-none');
            }
        });
    };

    ManageTripsForTravelWidget.DeleteSelectedTrips = function () {

        var dataToSend = [];

        // only send the trips to be deleted if the user confirmed the delete
        Session.confirmDialog('Delete Trips for Travel', 'Are you sure you want to delete these Trips? ', function () {
            $('#DeleteLoader-Trips').removeClass('display-none');
            $('#DeleteButton-Trips').addClass('display-none');

            $('#ManageTripsForTravelGrid tbody input[type=checkbox]:checked').each(function () {
                var tripsToDelete = {};

                var parentRow = $(this).parents('tr');
                tripsToDelete.TripID = parentRow.data('pkid');
                tripsToDelete.UpdateDateLong = parentRow.find('input.UpdateDateLong').val().toString();
                tripsToDelete.Deleted = true;
                tripsToDelete.inUse = parentRow.find('input.inUse').val();

                dataToSend.push(tripsToDelete);
            });

            dataToSend = JSON.stringify(dataToSend);

            $.ajax({
                type: 'POST',
                url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                                              '<%:WebConstants.ACTION_DELETE_TRIPS%>'),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function () {
                    $('#DeleteLoader-Trips').addClass('display-none');
                    $('#DeleteButton-Trips').removeClass('display-none');
                    ManageTripsForTravelWidget.ReloadGridData();
                },
                error: function () {
                    $('#DeleteLoader-Trips').addClass('display-none');
                    $('#DeleteButton-Trips').removeClass('display-none');
                }
            });
        });
    };

    ManageTripsForTravelWidget.ClearTripElementDialog = function () {
        $('#TripElementForm :input').val('');
        $('#TripElementForm div.form-element span[class]').text('');
        $('#TripElementDialog-PerDiemWarning').hide();

        $('#TripElementDialog #TripID').val('-1');
        $('#TripElementDialog #DepartureLocationID').val('-1');
        $('#TripElementDialog #DestinationLocationID').val('-1');
        $('#TripElementDialog #PerDiemID').val('-1');
        $('#TripElementDialog #UpdateDateLong').val('0');
        $('#TripElementDialog #PerDiemUpdateDateLong').val('0');
        $('#TripElementDialog #RTMIles').val('0');
        $('#TripElementDialog #DepartureLocationCode').attr('new', 'true');
        $('#TripElementDialog #DestinationLocationCode').attr('new', 'true');
    };

    ManageTripsForTravelWidget.AddTrip = function () {
        ManageTripsForTravelWidget.ClearTripElementDialog();

        $('#TripElementForm .add').removeClass('display-none');
        $('#TripElementForm .edit').addClass('display-none');

        ManageTripsForTravelWidget.ChangeDialogTitle(ManageTripsForTravelWidget.TripElementDialog, "Add Trip");
        ManageTripsForTravelWidget.OpenDialogAfterInitialize(ManageTripsForTravelWidget.TripElementDialog);
        ManageTripsForTravelWidget.clearValidationBox($("#TripElementForm ul.validation-box"));
    };

    ManageTripsForTravelWidget.EditTripFromRow = function (event, data) {
        if (data != undefined) {

            var dataRow = $(data);

            var trip = {};

            trip.TripID = dataRow.data('pkid');
            trip.MiscTravelRateID = dataRow.find('input.MiscTravelRateID').val();
            trip.DepartureLocationID = dataRow.find('input.DepartureLocationID').val();
            trip.DepartureLocationName = dataRow.find('input.DepartureLocationName').val();
            trip.DepartureLocationCode = dataRow.find('input.DepartureLocationCode').val();
            trip.DestinationLocationID = dataRow.find('input.DestinationLocationID').val();
            trip.DestinationLocationName = dataRow.find('input.DestinationLocationName').val();
            trip.DestinationLocationCode = dataRow.find('input.DestinationLocationCode').val();
            trip.PerDiemID = dataRow.find('input.PerDiemID').val();
            trip.PerDiemDestination = dataRow.find('input.PerDiemDestination').val();
            trip.Qualification = dataRow.find('input.Qualification').val();
            trip.Fare = dataRow.find('input.Fare').val();
            trip.HotelRate = dataRow.find('input.HotelRate').val();
            trip.MIERate = dataRow.find('input.MIERate').val();
            trip.PerDiemNotes = dataRow.find('input.PerDiemNotes').val();
            trip.RentalCar = dataRow.find('input.RentalCar').val();
            trip.RTMIles = dataRow.find('input.RTMIles').val();
            trip.FareLastUpdated = dataRow.find('input.FareLastUpdated').val();
            trip.PerDiemLastUpdated = dataRow.find('input.PerDiemLastUpdated').val();
            trip.TripLastUsed = dataRow.find('input.TripLastUsed').val();
            trip.TripCount = dataRow.find('input.TripCount').val();
            trip.UpdateDateLong = dataRow.find('input.UpdateDateLong').val().toString();
            trip.PerDiemUpdateDateLong = dataRow.find('input.PerDiemUpdateDateLong').val().toString();

            ManageTripsForTravelWidget.EditTrip(trip);
        }
    };

    ManageTripsForTravelWidget.EditTrip = function (trip) {
        if (trip != undefined) {
            ManageTripsForTravelWidget.ClearTripElementDialog();
            
            $('#TripElementForm .add').addClass('display-none');
            $('#TripElementForm .edit').removeClass('display-none');
            $('#TripElementForm #TripID').val(trip.TripID);
            $('#TripElementForm #MiscTravelRateID').val(trip.MiscTravelRateID);
            $('#TripElementForm span.MiscTravelRateID').text($('#MiscTravelRateID option:selected').text());
            $('#TripElementForm #DepartureLocationID').val(trip.DepartureLocationID);
            $('#TripElementForm #DepartureLocationName').val(trip.DepartureLocationName);
            $('#TripElementForm span.DepartureLocationName').text(trip.DepartureLocationName);
            $('#TripElementForm #DepartureLocationCode').val(trip.DepartureLocationCode);
            $('#TripElementForm span.DepartureLocationCode').text(trip.DepartureLocationCode);
            $('#TripElementForm #DestinationLocationID').val(trip.DestinationLocationID);
            $('#TripElementForm #DestinationLocationName').val(trip.DestinationLocationName);
            $('#TripElementForm span.DestinationLocationName').text(trip.DestinationLocationName);
            $('#TripElementForm #DestinationLocationCode').val(trip.DestinationLocationCode);
            $('#TripElementForm span.DestinationLocationCode').text(trip.DestinationLocationCode);
            $('#TripElementForm #PerDiemID').val(trip.PerDiemID);
            $('#TripElementForm #PerDiemDestination').val(trip.PerDiemDestination);
            $('#TripElementForm span.PerDiemDestination').text(trip.PerDiemDestination);
            $('#TripElementForm #Qualification').val(trip.Qualification);
            $('#TripElementForm span.Qualification').text(trip.Qualification);
            $('#TripElementForm #Fare').val(trip.Fare);
            $('#TripElementForm #HotelRate').val(trip.HotelRate);
            $('#TripElementForm #MIERate').val(trip.MIERate);
            $('#TripElementForm #PerDiemNotes').val(trip.PerDiemNotes);
            $('#TripElementForm #RentalCar').val(trip.RentalCar);
            $('#TripElementForm #RTMIles').val(trip.RTMIles);
            $('#TripElementForm span.FareLastUpdated').text(trip.FareLastUpdated);
            $('#TripElementForm span.PerDiemLastUpdated').text(trip.PerDiemLastUpdated);
            $('#TripElementForm span.TripLastUsed').text(trip.TripLastUsed);
            $('#TripElementForm span.TripCount').text(trip.TripCount);
            $('#TripElementForm #UpdateDateLong').val(trip.UpdateDateLong);
            $('#TripElementForm #PerDiemUpdateDateLong').val(trip.PerDiemUpdateDateLong);

            ManageTripsForTravelWidget.ChangeDialogTitle(ManageTripsForTravelWidget.TripElementDialog, "Edit Trip");
            ManageTripsForTravelWidget.OpenDialogAfterInitialize(ManageTripsForTravelWidget.TripElementDialog);
            ManageTripsForTravelWidget.clearValidationBox($("#TripElementForm ul.validation-box"));
        }
    };

    ManageTripsForTravelWidget.Save = function () {
        ManageTripsForTravelWidget.SaveTrip();
    };

    ManageTripsForTravelWidget.SaveAddAnother = function () {
        ManageTripsForTravelWidget.SaveTrip('AddAnother');
    };

    ManageTripsForTravelWidget.SaveTrip = function (option) {
        var dataToSend = {};

        dataToSend.TripID = $('#TripElementForm #TripID').val();
        dataToSend.MiscTravelRateID = $('#TripElementForm #MiscTravelRateID').val();
        dataToSend.DepartureLocationID = $('#TripElementForm #DepartureLocationID').val();
        dataToSend.DepartureLocationName = $('#TripElementForm #DepartureLocationName').val();
        dataToSend.DepartureLocationCode = $('#TripElementForm #DepartureLocationCode').val();
        dataToSend.DestinationLocationID = $('#TripElementForm #DestinationLocationID').val();
        dataToSend.DestinationLocationName = $('#TripElementForm #DestinationLocationName').val();
        dataToSend.DestinationLocationCode = $('#TripElementForm #DestinationLocationCode').val();
        dataToSend.PerDiemID = $('#TripElementForm #PerDiemID').val();
        dataToSend.PerDiemDestination = $('#TripElementForm #PerDiemDestination').val();
        dataToSend.Qualification = $('#TripElementForm #Qualification').val();
        dataToSend.Fare = $('#TripElementForm #Fare').val();
        dataToSend.HotelRate = $('#TripElementForm #HotelRate').val();
        dataToSend.MIERate = $('#TripElementForm #MIERate').val();
        dataToSend.PerDiemNotes = $('#TripElementForm #PerDiemNotes').val();
        dataToSend.RentalCar = $('#TripElementForm #RentalCar').val();
        dataToSend.RTMIles = $('#TripElementForm #RTMIles').val();
        dataToSend.UpdateDateLong = $('#TripElementForm #UpdateDateLong').val().toString();
        dataToSend.PerDiemUpdateDateLong = $('#TripElementForm #PerDiemUpdateDateLong').val().toString();

        dataToSend = JSON.stringify(dataToSend);
        if (option != undefined && option == 'AddAnother') {
            $('#TripElementDialog-SaveAddAnother').addClass('display-none');
        } else {
            $('#TripElementDialog-Save').addClass('display-none');
        }
        $('#TripElementDialog-Loader').removeClass('display-none');
        ManageTripsForTravelWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                                            '<%:WebConstants.ACTION_SAVE_TRIP%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function () {
                $('#TripElementDialog-Loader').addClass('display-none');
                $('#TripElementDialog-Save').removeClass('display-none');
                $('#TripElementDialog-SaveAddAnother').removeClass('display-none');

                ManageTripsForTravelWidget.ReloadGridData();

                if (option != undefined && option == 'AddAnother') {
                    ManageTripsForTravelWidget.ClearTripElementDialog();
                    ManageTripsForTravelWidget.AddTrip();
                }
                else {
                    ManageTripsForTravelWidget.CloseDialog(ManageTripsForTravelWidget.TripElementDialog);
                }
            },
            error: function () {
                $('#TripElementDialog-Loader').addClass('display-none');
                $('#TripElementDialog-Save').removeClass('display-none');
                $('#TripElementDialog-SaveAddAnother').removeClass('display-none');
            }
        }, $('#TripElementDialog-Save'));
    };

    ManageTripsForTravelWidget.Export = function (action) {

        // Remove the old hidden iFrame, if it exists
        $('#DownloadTarget-Trips').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DownloadTarget-Trips',
            'class': 'display-none',
            'src': CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_EXPORT_TRIPS %>')
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');

    };

    ManageTripsForTravelWidget.ExportData = function () {
        ManageTripsForTravelWidget.Export('<%: WebConstants.ACTION_EXPORT_TRIPS %>');
    };

    ManageTripsForTravelWidget.ExportTemplate = function () {
        ManageTripsForTravelWidget.Export('<%: WebConstants.ACTION_EXPORT_TRIPS_TEMPLATE %>');
    };

    ManageTripsForTravelWidget.Import = function () {
        ManageTripsForTravelWidget.OpenDialogAfterInitialize(ManageTripsForTravelWidget.ImportTripsDialog);
        $('#ImportInstructions').removeClass('display-none');
        $('#ImportTrips-ImportButton').removeClass('display-none');
        $('#ImportTrips-ImportLoader').addClass('display-none');
        $('#ImportResults').addClass('display-none');
    };

    ManageTripsForTravelWidget.ValidateFileInput = function () {
        if ($("#ImportTrips-File").val().endsWith('.xlsx') ||
            $("#ImportTrips-File").val().endsWith('.xlsm')) {
            $('#ImportTrips-ImportButton').removeClass('disabled');
            return true;
        }
        else {
            $('#ImportTrips-ImportButton').addClass('disabled');
            return false;
        }
    };

    ManageTripsForTravelWidget.SubmitUploadForm = function () {
        if (ManageTripsForTravelWidget.ValidateFileInput()) {
            $('#ImportTrips-ImportButton').addClass('display-none');
            $('#ImportTrips-ImportLoader').removeClass('display-none');
            ManageTripsForTravelWidget.AppendIFrameForUploadResponse();
            $("#ImportTrips-Form").submit();
        }
    };

    ManageTripsForTravelWidget.AppendIFrameForUploadResponse = function () {
        // Remove the old hidden iFrame, if it exists
        $('#ImportTrips-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('body').append('<iframe id="ImportTrips-UploadTarget" name="ImportTrips-UploadTarget" class="display-none"></iframe>');

        ManageTripsForTravelWidget.SetDocumentDomains();
    };

    // This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
    ManageTripsForTravelWidget.SetDocumentDomains = function () {
        $('#ImportTrips-DocumentDomain').val(document.domain);
    };

    // This method is called from ManageTripsForTravelImportVerification.ascx.  Do not edit it without consulting that page first.
    ManageTripsForTravelWidget.ShowExpectedImportResults = function (data) {
        $('#ImportInstructions').addClass('display-none');
        $('#ImportTrips-ImportButton').removeClass('display-none');
        $('#ImportTrips-ImportLoader').addClass('display-none');
        $('#ImportResults').removeClass('display-none');
        $('#ImportResults').html(data);
    };

    ManageTripsForTravelWidget.DisplayImportError = function () {
        $('#ImportTrips-ImportButton').removeClass('display-none');
        $('#ImportTrips-ImportLoader').addClass('display-none');

        var error = {};
        error.Title = "Import Trips Error";
        error.Message = "An error occurred during the import.  Please verify your column names / types and try again.";
        error.Details = "Check the logs for the exception thrown from AdminController.ImportTrips";
        DisplayExceptionDialog(error);
    };

    ManageTripsForTravelWidget.RedisplayImport = function () {
        $('#ImportResults').addClass('display-none');
        $('#ImportInstructions').removeClass('display-none');
    };

    $(function () {
        ManageTripsForTravelWidget.afterDOMLoad();
        ManageTripsForTravelWidget.Initialize();
        ManageTripsForTravelWidget.BindEvents();

        ManageTripsForTravelWidget.registerForEvent('SA_LOAD_TRIPS', function () {
            //Apply Widget Validation Now
        });

        ManageTripsForTravelWidget.ReloadGridData();
    });

</script>

<div id="Trips" class="manage-trips-for-travel section">
    <div class="title">Manage Trips for Travel</div>
    <div class="data">
        <div>
            Add new or edit trips for travel.
        </div>
        <br />
        <div class="form-row">
            <div>
                <div class="inline">
                    <button id="DeleteButton-Trips" class="ies disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-Trips" class="loader display-none"></div>
                    <button id="AddButton-Trips" class="ies" type="button">+ Add</button>
                    <button id="Import-Trips" class="ies" name="import-button" type="button">Import</button>
                    <button id="Export-Trips" class="ies" type="button">Export</button>
                    <div name="PageControls" class="float-right" style="margin-top: 20px;"></div>
                </div>
            </div>
        </div>

       <div class="form-row">
            <div id="TripsGridContent" class="form-element">                
            </div>
        </div>
    </div>
</div>

<div class="manage-trips-for-travel-element" id="TripElementDialog" style="display: none;">

    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "TripElementForm", name = "TripElementForm" }))
       { %>
        <ul class="validation-box"></ul>

        <%: Html.HiddenFor(model => model.TripID) %>
        <%: Html.HiddenFor(model => model.DepartureLocationID, new { @class="DepartureLocationID" }) %>
        <%: Html.HiddenFor(model => model.DestinationLocationID) %>
        <%: Html.HiddenFor(model => model.PerDiemID) %>
        <%: Html.HiddenFor(model => model.UpdateDateLong) %>
        <%: Html.HiddenFor(model => model.PerDiemUpdateDateLong)%>

        <div class="form-row">
            <div class="form-label">Mode*</div>
            <div class="form-element">
                <%: Html.DropDownList("MiscTravelRateID", (IEnumerable<SelectListItem>)ViewData["Modes"], "Select a Mode", new { @class = "add" })%>
                <span class="MiscTravelRateID edit"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Departure Loc *</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.DepartureLocationName, new { @class = "long add", @maxlength = "40" })%>
                <span class="DepartureLocationName edit"></span>
                &nbsp;&nbsp;
                <span><b>Code</b></span>
                <%: Html.TextBoxFor(model => model.DepartureLocationCode, new { @class = "add", @maxlength = "10" })%>
                <span class="DepartureLocationCode edit"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Destination*</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.DestinationLocationName, new { @class = "long add", @maxlength = "40" })%>
                <span class="DestinationLocationName edit"></span>
                &nbsp;&nbsp;
                <span><b>Code</b></span>
                <%: Html.TextBoxFor(model => model.DestinationLocationCode, new { @class = "add", @maxlength = "10" })%>
                <span class="DestinationLocationCode edit"></span>
          </div>
        </div>
        <div class="form-row">
            <div class="form-label">Per Diem Dest*</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.PerDiemDestination, new { @class = "long add", @maxlength = "40" })%>
                <span class="PerDiemDestination edit"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Qualification</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.Qualification, new { @class = "long add", @maxlength = "40" })%>
                <span class="Qualification edit"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Fare *</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.Fare)%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Hotel*</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.HotelRate)%>
            </div>
        </div>
         <div class="form-row">
            <div class="form-label">MIE Rate *</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.MIERate)%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Per Diem Notes</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.PerDiemNotes, new { @class = "full", @maxlength = "100" })%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Rental Car *</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.RentalCar)%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">R/T Miles *</div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.RTMIles)%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label long">Fare Last Updated By:</div>
            <div class="form-element">
                <span class="FareLastUpdated"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label long">Per Diem Last Updated By:</div>
            <div class="form-element">
                <span class="PerDiemLastUpdated"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Last Used</div>
            <div class="form-element">
               <span class="TripLastUsed"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Trip Count</div>
            <div class="form-element">
                <span class="TripCount"></span>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label"></div>
            <div class="form-element">
                <div id="TripElementDialog-PerDiemWarning" class="warning-box">
                    <div class="warning-message" style="padding-right: 20px;">
                        Changing the <i>Hotel, MIE Rate, or Per Diem Notes</i> <br />
                        rate will apply to all trips that have the same <i>Per Diem <br />
                        Destintaion</i> and <i>Qualification</i>.
                    </div>
                </div>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label"></div>
            <div class="form-element">
                <div>
                    <button id="TripElementDialog-Save" class="ies-action" name="save-button" type="button">Save</button>
                    <div id="TripElementDialog-Loader" class="loader display-none"></div>
                    <button id="TripElementDialog-SaveAddAnother" class="ies" name="save-add-another-button" type="button">Save & add another</button>
                </div>
            </div>
        </div>
    <% } %>
</div>

<div id="TripNotUniqueDialog" class="dialog display-none">
    <input id="TripNotUniqueDialog-Trip" type="hidden" />
    <div>
        The data you have entered matches an existing trip definition. To create a new trip
        you must change one of the following inputs: Mode, Departure Location, Destination,
        or Qualification.
    </div>
    <br />
    <br />
    <div class="buttons" style="text-align: center; padding-bottom: 0px;">
        <button id="TripNotUniqueDialog-OK" class="ies" name="ok-button" type="button">OK</button>
        <button id="TripNotUniqueDialog-EditExisting" class="ies" type="button">Edit existing trip</button>
    </div>
</div>

<div id="ImportTripsDialog" class="import-trips dialog form" style="display: none;">
     <div id="ImportInstructions">
        <span>You can import new trip definitions, update existing trips or do both in the same import.  You can start by exporting the existing trips defined in genBOE to get the column definitions and the existing data.  Then you modify the data and finally import the updated information. </span>
        <div class="container">
            <div class="step one">
                <div class="title">Step 1: Export the existing Trip Rates Data </div>
                <div>Start by exporting the existing Trips Rates Data. This file has the correct column headings, all of the current Trip Rates data, plus some additional data to allow analysis of the usage of the Trip Rates.</div>
                <div><a id="ImportTrips-ExportExisting">Export existing trips</a></div>
            </div>
            <div class="step two">
                <div class="title">Step 2: Modify the existing Trip Rates Data</div>
                <div>
                    <ul>
                        <li>If you are only going to add new trips, then delete all rows in the spreadsheet except for the header row.</li>
                        <li>If you are also going to update existing trips, leave in all of the existing trips (or you can delete trips that you are not updating)</li>
                        <li>Add data for new trips</li>
                        <li>You should not modify column A (normally hidden).  It should be left blank for new trips.</li>
                        <li>The header row should not be modified</li>
                        <li>Columns O through T are ignored during import  so no data needs to be put into these columns</li>
                    </ul>
                </div>
            </div>
            <div class="step three">
                <% Html.BeginForm(WebConstants.ACTION_IMPORT_TRIPS, WebConstants.CONTROLLER_ADMIN, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportTrips-Form", target = "ImportTrips-UploadTarget" }); %>
                <div class="title">Step 3: Import the Trip data </div>
                <div>Select the file to import and then click the <i>Import</i> button.</div>
                <div>
                    <input type="hidden" id="ImportTrips-DocumentDomain" name="documentDomain" />
                    <input type="file" style="width:410px;" id="ImportTrips-File" name="file" />
                </div>            
                <div class="buttons">
                    <button id="ImportTrips-ImportButton" class="ies-action disabled" name="import-button" type="button">Import</button>
                    <div id="ImportTrips-ImportLoader" class="loader display-none"></div>
                </div>
                <% Html.EndForm(); %>
            </div>            
        </div>
    </div>
    <div id="ImportResults" class="display-none"></div>
</div>
