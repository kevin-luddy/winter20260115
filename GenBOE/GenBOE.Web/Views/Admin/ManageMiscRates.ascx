<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.MiscRateModelView>" %>

<script type="text/javascript">
    var ManageMiscRatesWidget = new Widget('ManageMiscRatesForm', <%: ViewData["READONLY"] %>);

    ManageMiscRatesWidget.OptionDialog = {};
    ManageMiscRatesWidget.OptionDialog.Element = $("#OptionDialog");
    ManageMiscRatesWidget.OptionDialog.Params = { width: 350, modal: true, resizable: false, draggable: true };

    ManageMiscRatesWidget.BindEvents = function() {
        // Bind events for single UI elements

        ManageMiscRatesWidget.registerForLiveEvent('click',
            '#Save-OptionDialog:not(.disabled)', ManageMiscRatesWidget.preparedForSubmit);
        ManageMiscRatesWidget.registerForLiveEvent('click',
            '#SaveAddAnother-OptionDialog:not(.disabled)', ManageMiscRatesWidget.preparedForSubmit);

        $('#AddButton-MiscRates').click(function() { ManageMiscRatesWidget.AddRecord(); });

        ManageMiscRatesWidget.registerForLiveEvent('click', '#DeleteButton-MiscRates:not(.disabled)', function() { $(document).trigger('GET_DELETED_RATES'); });

        ManageMiscRatesWidget.registerForEvent('RATE_CHECKED', function() { 
            if($("#ManageMiscRatesGrid input:checkbox:checked").length === 0)
            {
                $('#DeleteButton-MiscRates').addClass('disabled');
            }
            else
            {
                $('#DeleteButton-MiscRates').removeClass('disabled'); 
            }
        });
        ManageMiscRatesWidget.registerForEvent('DELETE_RATES', function(event, data) {
            Session.confirmDialog("Delete Miscellaneous Rates for Travel Confirmation", "Are you sure you want to delete these Miscellaneous Rates for Travel?", function(){ManageMiscRatesWidget.DeleteRecords(event, data);});
            } );
        ManageMiscRatesWidget.registerForEvent('EDIT_RATE', ManageMiscRatesWidget.EditRecord);
        ManageMiscRatesWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageMiscRatesWidget.cleanDirty(); });
          
    }

    ManageMiscRatesWidget.preparedForSubmit = function(event) {
         ManageMiscRatesWidget.SaveRecord(event);
    }

    ManageMiscRatesWidget.BackToJumpPage = function() {
        window.location.hash = '#';
    }

    ManageMiscRatesWidget.FilterSortCodeIntegers =  function() {
        $('#SortCode').val($('#SortCode').val().replace(/[^\,\.0-9]/g, ''));
    };

    ManageMiscRatesWidget.ReloadGridData = function() {
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_MISC_RATES_GRID %>', ''),
            dataType: 'html',
            success: function(response)
            {             
                $('#MiscRatesGridContent').html(response);
                $('#SearchText').val('');
                $('#DeleteLoader-MiscRates').addClass('display-none');
                $('#DeleteButton-MiscRates').removeClass('display-none');
                $('#DeleteButton-MiscRates').addClass('disabled');
            }
        });
    }

    ManageMiscRatesWidget.DeleteRecords = function (event, data) {
        $('#DeleteButton-MiscRates').addClass('display-none');
        $('#DeleteLoader-MiscRates').removeClass('display-none');

        ManageMiscRatesWidget.SaveRates(event, data.CheckedItems);
    }

    ManageMiscRatesWidget.AddRecord = function() {
        $('#OptionDialog input[name="MiscTravelRateMode"]').val('');
        $('#OptionDialog input[name="MiscTravelRate"]').val(0);
        $('#OptionDialog input[name="SortCode"]').val(0);

        ManageMiscRatesWidget.OpenDialogAfterInitialize(ManageMiscRatesWidget.OptionDialog);
        ManageMiscRatesWidget.clearValidationBox($("#MiscRateOptionForm ul.validation-box"));
        ManageMiscRatesWidget.OptionDialog.Element.dialog({title : "Add Miscellaneous Rate"});
    }

    ManageMiscRatesWidget.EditRecord = function(event, data) {
        $('#OptionDialog input[name="MiscTravelRateID"]').val(data.MiscTravelRateID);
        $('#OptionDialog input[name="MiscTravelRateMode"]').val(data.MiscTravelRateMode);
        $('#OptionDialog input[name="MiscTravelRate"]').val(data.MiscTravelRate);
        $('#OptionDialog input[name="SortCode"]').val(data.SortCode);

        ManageMiscRatesWidget.OpenDialogAfterInitialize(ManageMiscRatesWidget.OptionDialog);
        ManageMiscRatesWidget.clearValidationBox($("#MiscRateOptionForm ul.validation-box"));
        ManageMiscRatesWidget.OptionDialog.Element.dialog({title : "Edit Miscellaneous Rate"});
    }

    ManageMiscRatesWidget.SaveRecord = function(event) {
            var data = new Array();
            var option = {};
            var addAnother = false;
            option.MiscTravelRateID = $('#OptionDialog input[name="MiscTravelRateID"]').val();
            option.MiscTravelRateMode = $('#OptionDialog input[name="MiscTravelRateMode"]').val();
            option.MiscTravelRate = $('#OptionDialog input[name="MiscTravelRate"]').val();
            option.SortCode = $('#OptionDialog input[name="SortCode"]').val(); 

            data.push(option);

            ManageMiscRatesWidget.SaveRates(event, data);
    }

    ManageMiscRatesWidget.SaveRates = function(event, data) {
        if (data.length > 0) {
            var dataToSend = {};
            dataToSend.miscRates = data;
            ManageMiscRatesWidget.clearValidationBox($("#MiscRateOptionForm ul.validation-box"));
            dataToSend = JSON.stringify(dataToSend);

            ManageMiscRatesWidget.ajaxRequest({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_SAVE_MISC_RATES %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                data: dataToSend,
                success: function(response) {
                if($(event.target).attr('id') != 'SaveAddAnother-OptionDialog')
                {
                    ManageMiscRatesWidget.CloseDialog(ManageMiscRatesWidget.OptionDialog);
                }
                    $('#OptionDialog input[name="MiscTravelRateID"]').val('-1');
                    $('#OptionDialog input[name="MiscTravelRateMode"]').val('');
                    $('#OptionDialog input[name="MiscTravelRate"]').val('');
                    $('#OptionDialog input[name="SortCode"]').val(''); 
                    ManageMiscRatesWidget.ReloadGridData();
                    RaiseNotification("Changes saved");
                },
                error: function(response) {
                    $('#DeleteButton-MiscRates').removeClass('display-none');
                    $('#DeleteLoader-MiscRates').addClass('display-none');
                }
            }, $('#Save-OptionDialog'));
        }
        else {
            $('#DeleteButton-MiscRates').addClass('disabled');
            $('#DeleteButton-MiscRates').removeClass('display-none');
            $('#DeleteLoader-MiscRates').addClass('display-none');
        }
    }


    $(function() {
        ManageMiscRatesWidget.afterDOMLoad();
        ManageMiscRatesWidget.InitializeDialog(ManageMiscRatesWidget.OptionDialog);
        ManageMiscRatesWidget.BindEvents();
        ManageMiscRatesWidget.registerForEvent('MISC_RATES_LOADED', function() {
            return true;
        });
    });
</script>

<div id="ManageMiscRatesForm">
<div id="MiscRates" class="manage-default-resources section">
    <div class="title">Manage Miscellaneous Rates for Travel Mode</div>
    <div class="data">
        <div>
            Add new or edit miscellaneous rates for travel.
        </div>
        <br />
        <div class="form-row">
            <div class="manage-default-resources-buttons form-element">
                <div class="buttons inline">
                    <button id="DeleteButton-MiscRates" class="ies disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-MiscRates" class="loader display-none"></div>
                    <button id="AddButton-MiscRates" class="ies" type="button">+ Add</button>
                </div>
            </div>
        </div>

        <div class="form-row">
            <div id="MiscRatesGridContent" class="form-element">
                <% Html.RenderAction(WebConstants.ACTION_DISPLAY_MANAGE_MISC_RATES_GRID, WebConstants.CONTROLLER_ADMIN); %>
            </div>
        </div>
    </div>
</div>

<div class="manage-default-resources-edit" id="OptionDialog" style="display: none;">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "MiscRateOptionForm" })) { %>
    <ul class="validation-box" style="white-space:pre-wrap;"> </ul>
    <div class="form-row">
        <div class="form-label" style="width:120px">Transportation Mode*</div>
        <div class="form-element">
            <input type="hidden" id="MiscTravelRateID" name="MiscTravelRateID" value="-1" />
            <input type="text" id="MiscTravelRateMode" name="MiscTravelRateMode" maxlength="25"/>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label" style="width:120px">Miscellaneous Rate*</div>
        <div class="form-element">
            <input type="text" id="MiscTravelRate" name="MiscTravelRate"/>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"style="width:120px">Sort Code*</div>
        <div class="form-element">
            <input type="text" id="SortCode" name="SortCode" maxlength="4" onkeyup="ManageMiscRatesWidget.FilterSortCodeIntegers(this)"/>
        </div>
    </div>
    <div class="form-label"></div>
    <div class="form-element">
        <div>
            <button id="Save-OptionDialog" class="ies-action" name="save-button" type="button">Save</button>
            <div id="Loader-OptionDialog" class="loader display-none"></div>
            <button id="SaveAddAnother-OptionDialog" class="ies" name="save-add-another-button" type="button">Save & add another</button>
        </div>
    </div>
    <% } %>
</div>

</div>