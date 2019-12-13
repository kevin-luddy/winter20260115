<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.MSTZoneTravelDestinationModelView>" %>

<script type="text/javascript">
    var ManageZoneTravelDestinationsWidget = new Widget('DestinationElementForm', '<%: ViewData["READONLY"] %>'.isTrue());

    ManageZoneTravelDestinationsWidget.DestinationElementDialog = {};

    ManageZoneTravelDestinationsWidget.Initialize = function () {
        ManageZoneTravelDestinationsWidget.DestinationElementDialog.Element = $("#DestinationElementDialog");
        ManageZoneTravelDestinationsWidget.DestinationElementDialog.Params = { width: 580, modal: true, resizable: false, draggable: true };
        ManageZoneTravelDestinationsWidget.InitializeDialog(ManageZoneTravelDestinationsWidget.DestinationElementDialog);
    };

    ManageZoneTravelDestinationsWidget.BindEvents = function () {
        ManageZoneTravelDestinationsWidget.registerForLiveEvent('click',
            '#DestinationElementDialog-Save:not(.disabled)', ManageZoneTravelDestinationsWidget.Save);
        ManageZoneTravelDestinationsWidget.registerForEvent('EDIT_DESTINATION', ManageZoneTravelDestinationsWidget.EditDestinationFormRow);
        ManageZoneTravelDestinationsWidget.registerForEvent('RELOAD_GRID', ManageZoneTravelDestinationsWidget.ReloadGridData);
    };

    ManageZoneTravelDestinationsWidget.ReloadGridData = function () {
        $('#PageLoading').removeClass('display-none');
        
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_ZONE_TRAVEL_DESTINATIONS_GRID %>'),
            contentType: 'applications/json; charset=utf-8',
            dataType: 'html',
            success: function (response) {
                $('#ZoneTravelDestinationsGridContent').html(response);
                $('#PageLoading').addClass('display-none');
            },
            error: function (response) {
                $('#PageLoading').addClass('display-none');
            }
        });
    };

    ManageZoneTravelDestinationsWidget.EditDestinationFormRow = function (event, data) {
        if (data != undefined) {
            var dataRow = $(data);

            var destination = {};

            destination.DestinationID = dataRow.data('pkid');
            destination.Destination = dataRow.find('input.Destination').val();
            destination.Abbreviation = dataRow.find('input.Abbreviation').val();
            destination.Zone = dataRow.find('input.Zone').val();

            ManageZoneTravelDestinationsWidget.EditDestination(destination);
        }
    };

    ManageZoneTravelDestinationsWidget.EditDestination = function (destination) {
        if (destination != undefined)
        {
            $('#DestinationElementForm #DestinationID').val(destination.DestinationID);
            $('#DestinationElementForm #Destination').val(destination.Destination);
            $('#DestinationElementForm span.Destination').text(destination.Destination);
            $('#DestinationElementForm #Abbreviation').val(destination.Abbreviation);
            $('#DestinationElementForm span.Abbreviation').text(destination.Abbreviation);
            $('#DestinationElementForm #Zone').val(destination.Zone);

            ManageZoneTravelDestinationsWidget.ChangeDialogTitle(ManageZoneTravelDestinationsWidget.DestinationElementDialog, "Edit Destination");
            ManageZoneTravelDestinationsWidget.OpenDialogAfterInitialize(ManageZoneTravelDestinationsWidget.DestinationElementDialog);
            ManageZoneTravelDestinationsWidget.clearValidationBox($("#DestinationElementForm ul.validation-box"));
        }
    };
    
    ManageZoneTravelDestinationsWidget.Save = function () {
        var dataToSend = {};

        dataToSend.DestinationID = $('#DestinationElementForm #DestinationID').val();
        dataToSend.Destination = $('#DestinationElementForm #Destination').val();
        dataToSend.Abbreviation = $('#DestinationElementForm #Abbreviation').val();
        dataToSend.Zone = $('#DestinationElementForm #Zone').val();

        dataToSend = JSON.stringify(dataToSend);

        $('#DestinationElementDialog-Save').addClass('display-none');
        $('#DestinationElementDialog-Loader').removeClass('display-none');

        ManageZoneTravelDestinationsWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                                          '<%:WebConstants.ACTION_SAVE_DESTINATION%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function () {
                $('#DestinationElementDialog-Save').removeClass('display-none');
                $('#DestinationElementDialog-Loader').addClass('display-none');

                ManageZoneTravelDestinationsWidget.ReloadGridData();

                ManageZoneTravelDestinationsWidget.CloseDialog(ManageZoneTravelDestinationsWidget.DestinationElementDialog);
            },
            error: function () {
                $('#DestinationElementDialog-Save').removeClass('display-none');
                $('#DestinationElementDialog-Loader').addClass('display-none');
            }
        }, $('#DestinationElementDialog-Save'));
    };


    $(function () {
        ManageZoneTravelDestinationsWidget.afterDOMLoad();
        ManageZoneTravelDestinationsWidget.Initialize();
        ManageZoneTravelDestinationsWidget.BindEvents();

        ManageZoneTravelDestinationsWidget.ReloadGridData();
    });
</script>

<div id="ZoneTravelDestinations" class="manage-zone-travel-destinations section">
    <div class="title">Manage Zone Travel Destinations</div>
    <div class="data">
        <div>
            Edit zones for zone travel destinations.
        </div>
        <br />

        <div class="form-row">
            <div id="ZoneTravelDestinationsGridContent" class="form-element">
            </div>
        </div>
    </div>
</div>

<div class="manage-zone-travel-destination-element" id="DestinationElementDialog" style="display: none">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "DestinationElementForm", name = "DestinationElementForm" }))
        { %>
    <ul class="validation-box"></ul>

    <%: Html.HiddenFor(model => model.DestinationID) %>
    <%: Html.HiddenFor(model => model.Destination) %>
    <%: Html.HiddenFor(model => model.Abbreviation) %>
        
    <div class="form-row">
        <div class="form-label">Destination</div>
        <div class="form-element">
            <span class="Destination"></span><span> (</span><span class="Abbreviation"></span><span>)</span>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Zone</div>
        <div class="form-element">
            <%: Html.DropDownList("Zone", (IEnumerable<SelectListItem>)ViewData["Zones"], "Select a Zone") %>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"></div>
        <div class="form-element">
            <div>
                <button id="DestinationElementDialog-Save" class="ies-action" name="save-button" type="button">Save</button>
                <div id="DestinationElementDialog-Loader" class="loader display-none"></div>
            </div>
        </div>
    </div>
    <% } %>
</div>