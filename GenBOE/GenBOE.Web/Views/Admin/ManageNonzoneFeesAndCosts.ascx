<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.NonzoneFeesAndCostsModelView>" %>

<script type ="text/javascript">
    var ManageNonzoneFeesAndCostsWidget = new Widget('FeesAndCostsElementForm', '<%: ViewData["READONLY"] %>'.isTrue());

    ManageNonzoneFeesAndCostsWidget.FeesAndCostsElementDialog = {};

    ManageNonzoneFeesAndCostsWidget.Initialize = function () {
        ManageNonzoneFeesAndCostsWidget.FeesAndCostsElementDialog.Element = $("#FeesAndCostsElementDialog");
        ManageNonzoneFeesAndCostsWidget.FeesAndCostsElementDialog.Params = { width: 580, modal: true, resizable: false, draggable: true };
        ManageNonzoneFeesAndCostsWidget.InitializeDialog(ManageNonzoneFeesAndCostsWidget.FeesAndCostsElementDialog);
    };

    ManageNonzoneFeesAndCostsWidget.BindEvents = function () {
        ManageNonzoneFeesAndCostsWidget.registerForEvent("RELOAD_GRID", ManageNonzoneFeesAndCostsWidget.ReloadGridData);
        ManageNonzoneFeesAndCostsWidget.registerForEvent("EDIT_FEES_AND_COSTS", ManageNonzoneFeesAndCostsWidget.EditFeesAndCostsFormRow);
        ManageNonzoneFeesAndCostsWidget.registerForLiveEvent('click', '#FeesAndCostsElementDialog-Save:not(.disabled)', ManageNonzoneFeesAndCostsWidget.Save);
    }

    ManageNonzoneFeesAndCostsWidget.ReloadGridData = function () {
        $('#PageLoading').removeClass('display-none');

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_NONZONE_FEES_AND_COSTS_GRID %>'),
            contentType: 'applications/json; charset=utf-8',
            dataType: 'html',
            success: function (response) {
                $('#NonzoneFeesAndCostsGridContent').html(response);
                $('#PageLoading').addClass('display-none');
            },
            error: function (response) {
                $('#PageLoading').addClass('display-none');
            }
        });
    };

    ManageNonzoneFeesAndCostsWidget.EditFeesAndCostsFormRow = function (event, data) {
        if (data != undefined) {
            var dataRow = $(data);

            var feesAndCosts = {};

            feesAndCosts.ModeID = dataRow.data('pkid');
            feesAndCosts.Mode = dataRow.find('input.Mode').val();
            feesAndCosts.TravelAgencyFee = dataRow.find('input.TravelAgencyFee').val();
            feesAndCosts.MiscOther = dataRow.find('input.MiscOther').val();

            if (feesAndCosts.Mode == '<%:MSTTravelMode.NonZoneInternational.GetDescription() %>') {
                $('#NonZoneMiscOtherRow').addClass("display-none");
            } else {
                $('#NonZoneMiscOtherRow').removeClass("display-none");
            }

            ManageNonzoneFeesAndCostsWidget.EditFeesAndCosts(feesAndCosts);
        }
    };

    ManageNonzoneFeesAndCostsWidget.EditFeesAndCosts = function (feesAndCosts) {
        if (feesAndCosts != undefined)
        {
            $('#FeesAndCostsElementForm #ModeID').val(feesAndCosts.ModeID);
            $('#FeesAndCostsElementForm #Mode').val(feesAndCosts.Mode);
            $('#FeesAndCostsElementForm span.Mode').text(feesAndCosts.Mode);
            $('#FeesAndCostsElementForm #TravelAgencyFee').val(parseFloat(feesAndCosts.TravelAgencyFee).toFixed(2));
            $('#FeesAndCostsElementForm #MiscOther').val(parseFloat(feesAndCosts.MiscOther).toFixed(2));

            ManageNonzoneFeesAndCostsWidget.OpenDialogAfterInitialize(ManageNonzoneFeesAndCostsWidget.FeesAndCostsElementDialog);
            ManageNonzoneFeesAndCostsWidget.clearValidationBox($("#FeesAndCostsElementForm ul.validation-box"));
        }
    };

    ManageNonzoneFeesAndCostsWidget.Save = function () {
        var dataToSend = {};

        dataToSend.Mode = $('#FeesAndCostsElementForm #ModeID').val();
        dataToSend.TravelAgencyFee = $('#FeesAndCostsElementForm #TravelAgencyFee').val();
        dataToSend.MiscOther = $('#FeesAndCostsElementForm #MiscOther').val();
        
        dataToSend = JSON.stringify(dataToSend);

        $('#FeesAndCostsElementDialog-Save').addClass('display-none');
        $('#FeesAndCostsElementDialog-Loader').removeClass('display-none');

        ManageNonzoneFeesAndCostsWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                                          '<%:WebConstants.ACTION_SAVE_FEES_AND_COSTS%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function () {
                $('#FeesAndCostsElementDialog-Save').removeClass('display-none');
                $('#FeesAndCostsElementDialog-Loader').addClass('display-none');
                ManageNonzoneFeesAndCostsWidget.ReloadGridData();
                ManageNonzoneFeesAndCostsWidget.CloseDialog(ManageNonzoneFeesAndCostsWidget.FeesAndCostsElementDialog);
            },
            error: function () {
                $('#FeesAndCostsElementDialog-Save').removeClass('display-none');
                $('#FeesAndCostsElementDialog-Loader').addClass('display-none');
            }
        }, $('#FeesAndCostsElementDialog-Save'));
    };

    $(function () {
        ManageNonzoneFeesAndCostsWidget.Initialize();
        ManageNonzoneFeesAndCostsWidget.BindEvents();
        ManageNonzoneFeesAndCostsWidget.ReloadGridData();
    });
</script>

<div id="NonzoneFeesAndCosts" class="manage-nonzone-fees-and-costs section">
    <div class="title">Manage Fees and Costs for Nonzone Travel</div>
    <div class ="data">
        <div>
            Edit the Travel Agency Fee and Miscellaneous/Other Costs for Nonzone Domestic and International Travel.
        </div>
        <br />

        <div class="form-row">
            <div id="NonzoneFeesAndCostsGridContent" class="form-element"></div>
        </div>
    </div>
</div>

<div class="manage-nonzone-fees-and-costs-element" id="FeesAndCostsElementDialog" title="Edit Fees and Costs" style="display: none">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "FeesAndCostsElementForm", name = "FeesAndCostsElementForm" }))
        { %>
    <ul class="validation-box"></ul>

    <%: Html.HiddenFor(model => model.ModeID) %>

    <div class="form-row">
        <div class="form-label">Mode</div>
        <div class="form-element">
            <span class="Mode"></span>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Travel Agency Fee</div>
        <div class="form-element">
            <%: Html.TextBoxFor(model => model.TravelAgencyFee, new { @class = "long" }) %>
        </div>
    </div>
    <div class="form-row" id="NonZoneMiscOtherRow">
        <div class="form-label">Misc/Other</div>
        <div class="form-element">
            <%: Html.TextBoxFor(model => model.MiscOther, new { @class = "long" }) %>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"></div>
        <div class="form-element">
            <div>
                <button id="FeesAndCostsElementDialog-Save" class="ies-action" name="save-button" type="button">Save</button>
                <div id="FeesAndCostsElementDialog-Loader" class="loader display-none"></div>
            </div>
        </div>
    </div>
    <% } %>
</div>
