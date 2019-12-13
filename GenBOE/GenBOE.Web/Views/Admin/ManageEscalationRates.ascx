<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.EscalationRateModelView>" %>

<script type="text/javascript">
    var ManageEscalationRatesWidget = new Widget('ManageEscalationRatesForm', <%: ViewData["READONLY"] %>);

    ManageEscalationRatesWidget.OptionDialog = {};
    ManageEscalationRatesWidget.OptionDialog.Element = $("#ManageEscalationRateDialog");
    ManageEscalationRatesWidget.OptionDialog.Params = { width: 350, modal: true, resizable: false, draggable: true };
    ManageEscalationRatesWidget.ImportDialog = {};
    ManageEscalationRatesWidget.ImportInProgressDialog = {};

    ManageEscalationRatesWidget.BindEvents = function() {
        // Bind events for single UI elements

        ManageEscalationRatesWidget.registerForLiveEvent('click',
            '#ManageEscalationRateDialog .add-one:not(.disabled)', ManageEscalationRatesWidget.preparedForSubmit);
        ManageEscalationRatesWidget.registerForLiveEvent('click',
            "#ManageEscalationRateDialog button[name='save-add-another-button']:not(.disabled)", ManageEscalationRatesWidget.preparedForSubmit);

        $("#EscalationRates button[name='add-button']").click(function() { ManageEscalationRatesWidget.AddRecord();
         ManageEscalationRatesWidget.clearValidationBox($("#EscalationRateOptionForm ul.validation-box"));
         });

        ManageEscalationRatesWidget.registerForLiveEvent('click', '#DeleteButton-EscalationRates:not(.disabled)', function() { $(document).trigger('GET_DELETED_RATES'); });

        ManageEscalationRatesWidget.registerForEvent('RATE_CHECKED', function() {
            if($("#ManageEscalationRatesGrid input:checkbox:checked").length === 0)
            {
                $('#DeleteButton-EscalationRates').addClass('disabled');
            }
            else{
                $('#DeleteButton-EscalationRates').removeClass('disabled');
            }
        });
        ManageEscalationRatesWidget.registerForEvent('DELETE_RATES', function(event, data) {
            Session.confirmDialog("Delete Escalation Rates for Travel Confirmation", "Are you sure you want to delete these Escalation Rates for Travel?", function(){ManageEscalationRatesWidget.DeleteRecords(event, data);});
            } );
        ManageEscalationRatesWidget.registerForEvent('EDIT_RATE', ManageEscalationRatesWidget.EditRecord);
        ManageEscalationRatesWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageEscalationRatesWidget.cleanDirty(); });

        $('#Import-EscalationRates').click(ManageEscalationRatesWidget.Import);
        $('#Export-EscalationRates, #ImportEscalationRates-ExportExisting').click(ManageEscalationRatesWidget.Export);
        $('#ImportEscalationRatesDialog-ImportButton').click(ManageEscalationRatesWidget.SubmitUploadForm);
        $('#ImportEscalationRatesDialog-CancelButton').click(function () { ManageEscalationRatesWidget.CloseDialog(ManageEscalationRatesWidget.ImportDialog) });
    }
    
    
 ManageEscalationRatesWidget.preparedForSubmit = function(event) {

     return ManageEscalationRatesWidget.SaveRecord(event);

    }

    ManageEscalationRatesWidget.BackToJumpPage = function() {
        window.location.hash = '#';
    }

    // Remove old dialogs left behind when jumping back to main Admin jump page
    ManageEscalationRatesWidget.RemoveStaleDialogs = function () {
        $('body .ui-dialog').children('#ImportEscalationRatesDialog, #ImportEscalationRatesInProgressDialog').parent().remove();
        $('body').children('#ImportEscalationRatesDialog, #ImportEscalationRatesInProgressDialog').remove();
    }

    ManageEscalationRatesWidget.ReloadGridData = function() {
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_ESCALATION_RATES_GRID %>', ''),
            dataType: 'html',
            success: function(response)
            {             
                $('#EscalationRatesGridContent').html(response);
                $('#SearchText').val('');
                $('#DeleteLoader-EscalationRates').addClass('display-none');
                $('#DeleteButton-EscalationRates').removeClass('display-none');
                $('#DeleteButton-EscalationRates').addClass('disabled');
            }
        });
    }
    
    ManageEscalationRatesWidget.DeleteRecords = function (event, data) {
        $('#DeleteButton-EscalationRates').addClass('display-none');
        $('#DeleteLoader-EscalationRates').removeClass('display-none');
        ManageEscalationRatesWidget.SaveRates(event, data.CheckedItems);
    }

    ManageEscalationRatesWidget.AddRecord = function() {
        $('#ManageEscalationRateDialog input[name="Year"]').val('');
        $('#ManageEscalationRateDialog input[name="DevEscalation"]').val('');
        $('#ManageEscalationRateDialog input[name="LMSIEscalation"]').val('');
        $('#ManageEscalationRateDialog input[name="MiscRate"]').val('');
        $('#ManageEscalationRateDialog input[name="Year"]').prop('disabled', false);
        $('#ManageEscalationRateDialog input[name="DevEscalation"]').prop('disabled', false);
        $('#ManageEscalationRateDialog input[name="LMSIEscalation"]').prop('disabled', false);
        $('#ManageEscalationRateDialog input[name="MiscRate"]').prop('disabled', false);
        ManageEscalationRatesWidget.OpenDialogAfterInitialize(ManageEscalationRatesWidget.OptionDialog);
        ManageEscalationRatesWidget.OptionDialog.Element.dialog({title : "Add Escalation Rate"});

    }

    ManageEscalationRatesWidget.EditRecord = function(event, data) {
        $('#ManageEscalationRateDialog input[name="EscalationRateID"]').val(data.EscalationRateID);
        $('#ManageEscalationRateDialog input[name="Year"]').val(data.Year);
        $('#ManageEscalationRateDialog input[name="DevEscalation"]').val(data.DevEscalation);
        $('#ManageEscalationRateDialog input[name="LMSIEscalation"]').val(data.LMSIEscalation);
        $('#ManageEscalationRateDialog input[name="MiscRate"]').val(data.MiscRate);

         if (data.inUse == 'True' )
        {
             $('#ManageEscalationRateDialog input[name="Year"]').prop('disabled', true);
             $('#ManageEscalationRateDialog input[name="DevEscalation"]').prop('disabled', true);
             $('#ManageEscalationRateDialog input[name="LMSIEscalation"]').prop('disabled', true);
             $('#ManageEscalationRateDialog input[name="MiscRate"]').prop('disabled', true);
         }
        else
        {
             $('#ManageEscalationRateDialog input[name="Year"]').prop('disabled', false);
             $('#ManageEscalationRateDialog input[name="DevEscalation"]').prop('disabled', false);
             $('#ManageEscalationRateDialog input[name="LMSIEscalation"]').prop('disabled', false);
             $('#ManageEscalationRateDialog input[name="MiscRate"]').prop('disabled', false);
        }
        ManageEscalationRatesWidget.OpenDialogAfterInitialize(ManageEscalationRatesWidget.OptionDialog);
        ManageEscalationRatesWidget.OptionDialog.Element.dialog({title : "Edit Escalation Rate"});
        ManageEscalationRatesWidget.clearValidationBox($("#EscalationRateOptionForm ul.validation-box"));
    }

    ManageEscalationRatesWidget.SaveRecord = function(event) {
            var data = new Array();
            var option = {};
            option.EscalationRateID = $('#ManageEscalationRateDialog input[name="EscalationRateID"]').val();
            option.Year = $('#ManageEscalationRateDialog input[name="Year"]').val();
            option.DevEscalation = $('#ManageEscalationRateDialog input[name="DevEscalation"]').val();
            option.LMSIEscalation = $('#ManageEscalationRateDialog input[name="LMSIEscalation"]').val(); 
            option.MiscRate = $('#ManageEscalationRateDialog input[name="MiscRate"]').val(); 

            if (option.MiscRate == undefined || option.MiscRate == ''){
                option.MiscRate = 0;
            }
        
            data.push(option);
            ManageEscalationRatesWidget.SaveRates(event, data);
    }

    ManageEscalationRatesWidget.SaveRates = function(event, data) {
        if (data.length > 0) {
            var dataToSend = {};
            dataToSend.escRates = data;
            ManageEscalationRatesWidget.clearValidationBox($("#EscalationRateOptionForm ul.validation-box"));
            dataToSend = JSON.stringify(dataToSend);

            ManageEscalationRatesWidget.ajaxRequest({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_SAVE_ESCALATION_RATES %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                data: dataToSend,
                success: function(response) {
                    if($(event.target).attr("name") != 'save-add-another-button')
                    {
                        ManageEscalationRatesWidget.CloseDialog(ManageEscalationRatesWidget.OptionDialog);
                    }
                    $('#ManageEscalationRateDialog input[name="EscalationRateID"]').val('-1');
                    $('#ManageEscalationRateDialog input[name="Year"]').val('');
                    $('#ManageEscalationRateDialog input[name="DevEscalation"]').val('');
                    $('#ManageEscalationRateDialog input[name="LMSIEscalation"]').val(''); 
                    $('#ManageEscalationRateDialog input[name="MiscRate"]').val(''); 
                    ManageEscalationRatesWidget.ReloadGridData();
                    RaiseNotification("Changes saved");
                },
                error: function(response) {
                    $('#DeleteButton-EscalationRates').removeClass('display-none');
                    $('#DeleteLoader-EscalationRates').addClass('display-none');
                }
            }, $("button[name='save-button'].add-one"));
        }
        else {
            $('#DeleteButton-EscalationRates').addClass('disabled');
            $('#DeleteButton-EscalationRates').removeClass('display-none');
            $('#DeleteLoader-EscalationRates').addClass('display-none');
        }
    }

    ManageEscalationRatesWidget.Import = function () {
        ManageEscalationRatesWidget.OpenDialogAfterInitialize(ManageEscalationRatesWidget.ImportDialog);
        $('#ImportInstructions').removeClass('display-none');
        $('#ImportEscalationRates-ImportLoader').addClass('display-none');
    };
    
    ManageEscalationRatesWidget.SubmitUploadForm = function () {
        if (ManageEscalationRatesWidget.ValidateFileInput()) {
            ManageEscalationRatesWidget.HideImportEscalationRatesDialogError();
            ManageEscalationRatesWidget.OpenDialogAfterInitialize(ManageEscalationRatesWidget.ImportInProgressDialog);

            ManageEscalationRatesWidget.AppendIFrameForUploadResponse();
            $("#ImportEscalationRatesDialog-Form").submit();
        }
    }

    ManageEscalationRatesWidget.AppendIFrameForUploadResponse = function () {
        // Remove the old hidden iFrame, if it exists
        $('#ImportEscalationRatesDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportEscalationRatesDialog-Form').append('<iframe id="ImportEscalationRatesDialog-UploadTarget" name="ImportEscalationRatesDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportEscalationRatesDialog-UploadTarget').load(ManageEscalationRatesWidget.StopUpload);
    }

    ManageEscalationRatesWidget.StopUpload = function () { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportEscalationRatesDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide Dialogs
                ManageEscalationRatesWidget.CloseDialog(ManageEscalationRatesWidget.ImportDialog);
                ManageEscalationRatesWidget.CloseDialog(ManageEscalationRatesWidget.ImportInProgressDialog);

                // Show success notification
                RaiseNotification('Import successful and changes saved');

                // Redirect back to jump page
                ManageEscalationRatesWidget.ReloadGridData();
            }
            else {
                $("#ImportEscalationRatesDialog-ErrorText").html(results.Message);
                ManageEscalationRatesWidget.ShowImportEscalationRatesDialogError();
                ManageEscalationRatesWidget.CloseDialog(ManageEscalationRatesWidget.ImportInProgressDialog);
            }
        }
        else {
            ManageEscalationRatesWidget.CloseDialog(ManageEscalationRatesWidget.ImportInProgressDialog);
        }
    }



    ManageEscalationRatesWidget.ValidateFileInput = function () {
        if ($("#ImportEscalationRatesDialog-File").val().length == 0) {
            ManageEscalationRatesWidget.ShowImportEscalationRatesDialogFileValidation();
            ManageEscalationRatesWidget.DisableImportEscalationRatesDialogImportButton();
            return false;
        }
        else {
            ManageEscalationRatesWidget.HideImportEscalationRatesDialogFileValidation();
            ManageEscalationRatesWidget.EnableImportEscalationRatesDialogImportButton();
            return true;
        }
    }

    ManageEscalationRatesWidget.EnableImportEscalationRatesDialogImportButton = function () {
        $("#ImportEscalationRatesDialog-ImportButton").removeClass('display-none');
    }

    ManageEscalationRatesWidget.DisableImportEscalationRatesDialogImportButton = function () {
        $("#ImportEscalationRatesDialog-ImportButton").addClass('display-none');
    }

    ManageEscalationRatesWidget.ShowImportEscalationRatesDialogError = function () {
        $("#ImportEscalationRatesDialog-Error").slideDown("slow");
    }

    ManageEscalationRatesWidget.HideImportEscalationRatesDialogError = function () {
        $("#ImportEscalationRatesDialog-Error").slideUp("slow");
    }

    ManageEscalationRatesWidget.ShowImportEscalationRatesDialogFileValidation = function () {
        $("#ImportEscalationRatesDialog-FileValidation").slideDown("slow");
    }

    ManageEscalationRatesWidget.HideImportEscalationRatesDialogFileValidation = function () {
        $("#ImportEscalationRatesDialog-FileValidation").slideUp("slow");
    }

    ManageEscalationRatesWidget.Export = function () {
        // Remove the old hidden iFrame, if it exists
        $('#ManageZoneTravelEscalationRates-DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'ManageZoneTravelEscalationRates-DownloadTarget',
            'class': 'display-none',
            'src': CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_EXPORT_ESCALATION_RATES%>')
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    };

    $(function() {
        ManageEscalationRatesWidget.afterDOMLoad();
        ManageEscalationRatesWidget.RemoveStaleDialogs();
        ManageEscalationRatesWidget.ValidateFileInput();
        ManageEscalationRatesWidget.InitializeDialog(ManageEscalationRatesWidget.OptionDialog);
        ManageEscalationRatesWidget.ImportDialog.Element = $("#ImportEscalationRatesDialog");
        ManageEscalationRatesWidget.ImportDialog.Params = { title: "Import Escalation Rates", width: 675, modal: true, resizable: false, draggable: true, height: 490 };
        ManageEscalationRatesWidget.InitializeDialog(ManageEscalationRatesWidget.ImportDialog);

        ManageEscalationRatesWidget.ImportInProgressDialog.Element = $("#ImportEscalationRatesInProgressDialog");
        ManageEscalationRatesWidget.ImportInProgressDialog.Params = { width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" };
        ManageEscalationRatesWidget.InitializeDialog(ManageEscalationRatesWidget.ImportInProgressDialog);

        ManageEscalationRatesWidget.BindEvents();
    });
</script>

<div id="ManageEscalationRatesForm">
<div id="EscalationRates" class="manage-default-resources section">
    <div class="title">Manage Escalation Rates for Travel</div>
    <div class="data">
        <div>
            Add new or edit escalation rates for travel.
        </div>
        <br />
        <div class="form-row">
            <div class="manage-default-resources-buttons form-element">
                <div class="inline">
                    <button id="DeleteButton-EscalationRates" class="ies disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-EscalationRates" class="loader display-none"></div>
                    <button id="AddButton-EscalationRates" name="add-button" class="ies" type="button">+ Add</button>
                    <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST && false) // for now we are disabling the export functionality
                        {  %>
                    <button id="Import-EscalationRates" class="ies" name="import-button" type="button">Import</button>
                    <button id="Export-EscalationRates" class="ies" type="button">Export</button>
                    <% } %>
                </div>
            </div>
        </div>

        <div class="form-row">
            <div id="EscalationRatesGridContent" class="form-element">
                <% Html.RenderAction(WebConstants.ACTION_DISPLAY_ESCALATION_RATES_GRID, WebConstants.CONTROLLER_ADMIN); %>
            </div>
        </div>
    </div>
</div>

<div class="manage-default-resources-edit" id="ManageEscalationRateDialog">

    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "EscalationRateOptionForm" })) { %>
    <ul class="validation-box"></ul>
    <div class="form-row">
        <div class="form-label" style="width:130px">Year</div>
        <div class="form-element">
            <input type="hidden" id="EscalationRateID" name="EscalationRateID" value="-1" />
            <input type="text" id="Year" name="Year" maxlength="25"/>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label" style="width:130px"><%: Model.DevEscalationTitle %> %*</div>
        <div class="form-element">
            <input type="text" id="DevEscalation" name="DevEscalation" maxlength="13"/>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"style="width:130px"><%: Model.PerDiemOrLmsiEscalationTitle %> %*</div>
        <div class="form-element">
            <input type="text" id="LMSIEscalation" name="LMSIEscalation" maxlength="13"/>
        </div>
    </div>
    <%if (Model.ShowMiscRate) { %>
    <div class="form-row">
        <div class="form-label"style="width:130px">Misc/Car Escalation %*</div>
        <div class="form-element">
            <input type="text" id="MiscRate" name="MiscRate" maxlength="13"/>
        </div>
    </div>
    <% } %>
    <div class="form-label"></div>
    <div class="form-element">
        <div>
            <button class="ies-action add-one" name="save-button" type="button">Save</button>
            <div class="loader display-none"></div>
            <button class="ies" name="save-add-another-button" type="button">Save & add another</button>
        </div>
    </div>
    <% } %>
</div>
</div>

<div id="ImportEscalationRatesDialog" class="import-EscalationRates dialog form" style="display: none;">
    <div id="ImportEscalationRatesDialog-Error" class="validation-box" style="display: none;">
        <div><b>Import Failed.</b>
            <div id="ImportEscalationRatesDialog-ErrorText">
            </div>
        </div>
        <div class="clear"></div>
    </div>
    <div id="ImportInstructions">
        <span>Importing Escalation Rates does a complete replacement of the existing Escalation Rates.  Start by exporting the existing Escalation Rates defined in genBOE to get the column definitions and the existing data. Then you modify the data and finally import the updated information.</span>
        <div class="container">
            <div class="step one">
                <div class="title">Step 1: Export the existing Escalation Rates Data</div>
                <span>Start by exporting the existing Escalation Rates Data. This file has the correct column headings and all of the current Escalation Rates data. Then you modify the data and finally import the updated information.</span>
                <div><a id="ImportEscalationRates-ExportExisting">Export existing Escalation Rates</a></div>
            </div>
            <div class="step two">
                <div class="title">Step 2: Modify the existing Escalation Rates Data</div>
                <div>
                    <ul>
                        <li>If you are only going to add new Escalation Rates, then leave all of the existing data in the spreadsheet.</li>
                        <li>Add data for new Escalation Rates.</li>
                        <li>You should not modify column A (normally hidden). It should be left blank for new Escalation Rates.</li>
                        <li>The header row should not be modified.</li>
                        <li>Columns B and C (Year and Escalation %) are required.</li>
                    </ul>
                </div>
            </div>
            <div class="step three">
                <% Html.BeginForm(WebConstants.ACTION_IMPORT_ESCALATION_RATES, WebConstants.CONTROLLER_ADMIN, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportEscalationRatesDialog-Form", target = "ImportEscalationRatesDialog-UploadTarget" }); %>
                <div class="title">Step 3: Import the Escalation Rates data </div>
                <div id="ImportEscalationRatesDialog-FileValidation" style="color: #990000; display: none;">
                <div>Select the file to import and then click the <i>Import</i> button.</div>
                </div>
                <div>
                    <input type="hidden" id="ImportEscalationRates-DocumentDomain" name="documentDomain" />
                    <input type="file" style="width:410px;" id="ImportEscalationRatesDialog-File" name="file" onchange="ManageEscalationRatesWidget.ValidateFileInput();" />
                </div>            
                <div class="buttons">
                    <button id="ImportEscalationRatesDialog-ImportButton" class="ies-action display-none" name="import-button" type="button">Import</button>
                    <button id="ImportEscalationRatesDialog-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
                    <div id="ImportEscalationRates-ImportLoader" class="loader display-none"></div>
                </div>
                <% Html.EndForm(); %>
            </div>
        </div>
    </div>
</div>