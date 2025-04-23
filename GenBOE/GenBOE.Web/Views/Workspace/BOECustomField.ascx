<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOECustomFieldModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%  var readOnly = bool.Parse(ViewData["READONLY"] as string); %>
<script type="text/javascript">
    BOECustomFieldsWidget = new Widget("BOECustomFieldsForm", <%: ViewData["READONLY"] %>);
    
    BOECustomFieldsWidgetGrid = new GridWidget("BOECustomFieldsForm","CustomFieldOptionID");
    BOECustomFieldsWidgetGrid.newItemCount = 0;

    BOECustomFieldsWidget.ReloadThisPage = function () {
        BOECustomFieldsWidget.cleanDirty();
        BOECustomFieldsWidget.ReloadGridData();
    };

    BOECustomFieldsWidget.ReloadGridData = function() {
        var dataToSend = JSON.stringify(BOECustomFieldsGridWidget.data);
        $.ajax({
            type: "POST",
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                                '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD %>', ''),
            contentType: 'application/json; charset=utf-8',
            data: dataToSend,
            dataType: 'html',
            success: function (response) {
                BOECustomFieldsGridWidget.ContentDiv.html(response);
                $(document).trigger("CUSTOM_FIELDS_LOADED");
            }
        });
    }

    BOECustomFieldsWidget.preparedForSubmit = function() {

        BOECustomFieldsWidget.data={FieldName:$('#BOECustomFieldsForm input[name="CustomFieldMetaData.FieldName"]').val(),
                                        CustomFieldID:<%: Model.CustomFieldMetaData.CustomFieldID %>,
                                        CustomFieldDisplayID:$('#BOECustomFieldsForm input[name="CustomFieldMetaData.CustomFieldDisplayID"]:checked').val(),
                                        isRequired: $('#BOECustomFieldsForm input[name="CustomFieldMetaData.isRequired"]').is(':checked'),
                                        isOpenEnded: $('#BOECustomFieldsForm input[name="CustomFieldMetaData.isOpenEnded"]').is(':checked')};
                                  
           return true;
       
    };


    /*-------------------------------  Import/Export Functions --------------------------------*/

    // Remove old dialogs left behind when jumping back to main jump page
    BOECustomFieldsWidget.RemoveStaleDialogs = function () {
        $('body .ui-dialog').children('#ImportDialog, #ImportInProgressDialog, #ImportResultsDialog').parent().remove();
        $('body').children('#ImportDialog, #ImportInProgressDialog, #ImportResultsDialog').remove();
    };

    BOECustomFieldsWidget.ExportButtonClick = function () {
        if (BOECustomFieldsWidget.isDirty()) {
            Session.confirmDialog(
            "Save Changes",
            "Changes must be saved to include them in the export. Do you want to save changes made before exporting?",
            function () {
                //save, then export
                BOECustomFieldsWidget.SaveEdits(false);
                BOECustomFieldsWidget.Export();
            },
            BOECustomFieldsWidget.Export);
        }
        else {
            BOECustomFieldsWidget.Export();
        }
    };

    BOECustomFieldsWidget.Export = function () {
        // Remove the old hidden iFrame, if it exists
        $('#DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DownloadTarget',
            'class': 'display-none',
            'src': CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_EXPORT_BOE_CUSTOM_FIELD %>',
                    'customfield/' + '<%:Model.CustomFieldMetaData.CustomFieldID%>')
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    }

    BOECustomFieldsWidget.SubmitUploadForm = function () {
        if (BOECustomFieldsWidget.ValidateFileInput()) {
            BOECustomFieldsWidget.HideImportDialogError();
            BOECustomFieldsWidget.ShowImportInProgressDialog();

            BOECustomFieldsWidget.AppendIFrameForUploadResponse();
            $("#ImportDialog-Form").submit();
        }
    }

    BOECustomFieldsWidget.AppendIFrameForUploadResponse = function () {
        // Remove the old hidden iFrame, if it exists
        $('#ImportDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportDialog-Form').append('<iframe id="ImportDialog-UploadTarget" name="ImportDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportDialog-UploadTarget').load(BOECustomFieldsWidget.StopUpload);

        BOECustomFieldsWidget.SetDocumentDomains();
    }

    BOECustomFieldsWidget.StopUpload = function () { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportDialog-UploadTarget").contents().find("body #UploadResponse");
        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide dialogs
                BOECustomFieldsWidget.HideImportDialog();
                BOECustomFieldsWidget.HideImportInProgressDialog();

                // Render Import Results Dialog
                BOECustomFieldsWidget.RenderImportResults(results.Data);

                // Show Import Results
                BOECustomFieldsWidget.ShowImportResultsDialog();
            }
            else {
                $("#ImportDialog-ErrorText").text(results.Message);
                BOECustomFieldsWidget.ShowImportDialogError();
                BOECustomFieldsWidget.HideImportInProgressDialog();
            }
        }
        else {
            BOECustomFieldsWidget.HideImportInProgressDialog();
        }
    }

    BOECustomFieldsWidget.RenderImportResults = function (resultsData) {

        // Render html for options that were added
        if (resultsData != null && resultsData.AddedOptions != undefined && resultsData.AddedOptions != null && resultsData.AddedOptions.length) {
            var options = resultsData.AddedOptions;

            var newHTML = "<b>" + options.length + " options added:</b><ul>";

            for (var ndx = 0; ndx < options.length; ndx++) {
                newHTML += "<li>" +
                            options[ndx].CustomFieldValueName + ", " +
                            options[ndx].CustomFieldValueDescription +
                            "</li>";
            }

            newHTML += "</ul>";
        }
        else {
            var newHTML = "<b>0 options added.</b>";
        }

        $('#ImportResultsDialog-Added').html(newHTML);

        // Render html for options that were changed
        if (resultsData != null && resultsData.ChangedOptions != undefined && resultsData.ChangedOptions != null && resultsData.ChangedOptions.length) {
            var options = resultsData.ChangedOptions;

            var newHTML = "<b>" + options.length + " options changed:</b><ul>";

            for (var ndx = 0; ndx < options.length; ndx++) {
                newHTML += "<li>" +
                            options[ndx].Old.CustomFieldValueName + ", " +
                            options[ndx].Old.CustomFieldValueDescription +
                            " changed to " +
                            options[ndx].New.CustomFieldValueName + ", " +
                            options[ndx].New.CustomFieldValueDescription +
                            "</li>";
            }

            newHTML += "</ul>";
        }
        else {
            var newHTML = "<b>0 options changed.</b>";
        }

        $('#ImportResultsDialog-Changed').html(newHTML);

        // Render html for options that were deleted
        if (resultsData != null && resultsData != null && resultsData.DeletedOptions != undefined && resultsData.DeletedOptions != null && resultsData.DeletedOptions.length) {
            var options = resultsData.DeletedOptions;

            var newHTML = "<b>" + options.length + " options deleted:</b><ul>";

            for (var ndx = 0; ndx < options.length; ndx++) {
                newHTML += "<li>" +
                            options[ndx].CustomFieldValueName + ", " +
                            options[ndx].CustomFieldValueDescription +
                            "</li>";
            }

            newHTML += "</ul>";
        }
        else {
            var newHTML = "<b>0 options deleted.</b>";
        }

        $('#ImportResultsDialog-Deleted').html(newHTML);

        // Render html for options that were in use and left unchanged
        if (resultsData != null && resultsData.InUseOptions != undefined && resultsData.InUseOptions != null && resultsData.InUseOptions.length) {
            var options = resultsData.InUseOptions;

            var newHTML = "<b>" + options.length + " options could not be changed or deleted because they are in use by at least one BOE:</b><ul>";

            for (var ndx = 0; ndx < options.length; ndx++) {
                newHTML += "<li>" +
                            options[ndx].CustomFieldValueName + ", " +
                            options[ndx].CustomFieldValueDescription +
                            "</li>";
            }

            newHTML += "</ul>";
        }
        else {
            var newHTML = "<b>0 options in use.</b>";
        }

        $('#ImportResultsDialog-InUse').html(newHTML);
    }

    // This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
    BOECustomFieldsWidget.SetDocumentDomains = function () {
        $('#ImportDialog-DocumentDomain').val(document.domain);
    }

    BOECustomFieldsWidget.ValidateFileInput = function () {
        if ($("#ImportDialog-File").val().length == 0) {
            BOECustomFieldsWidget.ShowImportDialogFileValidation();
            BOECustomFieldsWidget.DisableImportDialogImportButton();
            return false;
        }
        else {
            BOECustomFieldsWidget.HideImportDialogFileValidation();
            BOECustomFieldsWidget.EnableImportDialogImportButton();
            return true;
        }
    }

    BOECustomFieldsWidget.EnableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").removeClass('disabled');
    }

    BOECustomFieldsWidget.DisableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").addClass('disabled');
    }

    BOECustomFieldsWidget.ShowImportDialog = function () {
        $("#ImportDialog").dialog({ width: 750, modal: true, resizable: false, draggable: true, title: "Import <%:Model.CustomFieldMetaData.FieldName%>" });
    }

    BOECustomFieldsWidget.HideImportDialog = function () {
        $('#ImportDialog').dialog('destroy');
    }

    BOECustomFieldsWidget.ShowImportDialogError = function () {
        $("#ImportDialog-Error").slideDown("slow");
    }

    BOECustomFieldsWidget.HideImportDialogError = function () {
        $("#ImportDialog-Error").slideUp("slow");
    }

    BOECustomFieldsWidget.ShowImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideDown("slow");
    }

    BOECustomFieldsWidget.HideImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideUp("slow");
    }

    BOECustomFieldsWidget.ShowImportInProgressDialog = function () {
        $('#ImportInProgressDialog').dialog({ width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" });
    }

    BOECustomFieldsWidget.HideImportInProgressDialog = function () {
        $('#ImportInProgressDialog').dialog('destroy');
    }

    BOECustomFieldsWidget.ShowImportResultsDialog = function () {
        $("#ImportResultsDialog").dialog({ width: 750, modal: true, resizable: false, draggable: true, title: "Import Successful", close: BOECustomFieldsWidget.ReloadThisPage });
    }

    BOECustomFieldsWidget.HideImportResultsDialog = function () {
        $('#ImportResultsDialog').dialog('close');
        $('#ImportResultsDialog').dialog('destroy');
    }
    /*-------------------------------  End Import/Export Functions --------------------------------*/

    BOECustomFieldsWidget.SetupValidation = function(){
        Widget.applyValidation();
    };

    BOECustomFieldsWidget.BackToCustomFields = function() {
        BOECustomFieldsWidget.cleanDirty();
        $(document).trigger("WS_LOAD_CUSTOM_FIELDS");
    };

    BOECustomFieldsWidget.SaveEdits = function(redirectOnSuccess) {
        // if the user clicked Save when no data has changed, just redirect them to the Home page
        if (BOECustomFieldsWidget.preparedForSubmit() && BOECustomFieldsWidgetGrid.preparedForSubmit()) 
        {
            var dataToSend = {
                UserHasConfirmed: false,
                CustomFieldMetaData: BOECustomFieldsWidget.data,
                CustomFieldOptions:BOECustomFieldsWidgetGrid.data
            };

            BOECustomFieldsWidget.SubmitSaveEdits(dataToSend, redirectOnSuccess, BOECustomFieldsWidget.ProcessSubmitSaveEditsResults);
        }
    };

    /*
    * Post custom-field edits to the server to be saved.
    *
    * @param dataToSend All edited values
    * @param redirectOnSuccess  Whether to redirect to the custom-field grid page on a successful save
    * @param callbackHandler Callback function of the form: function(success, response, originalArgs)
    */
    BOECustomFieldsWidget.SubmitSaveEdits = function(dataToSend, redirectOnSuccess, callbackHandler) {
        $("#Save-BOECustomFields").addClass("display-none");
        $("#Loader-BOECustomFields").removeClass("display-none");

        var originalArgs = { dataToSend : dataToSend, redirectOnSuccess: redirectOnSuccess };

        BOECustomFieldsWidget.ajaxRequest({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE%>',
                '<%:WebConstants.ACTION_SAVE_CUSTOM_FIELDS%>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(dataToSend),
            success: function(data, textStatus, jqXHR) {
                callbackHandler(true, $.parseJSON(jqXHR.responseText), originalArgs);
            },
            error: function(jqXHR, textStatus, errorThrown) {
                callbackHandler(false, $.parseJSON(jqXHR.responseText), originalArgs);
            }
        }, $("#Save-BOECustomFields"));
    };

    /*
    * Callback handler to process the custom-field save request.
    *
    * @param success Whether the save call was executed and the result returned without exceptions.  True or false.
    * @param response The JSON response from the save request
    * @param originalArgs The original arguments to the save call, needed if a second call is required (i.e. with a user-confirmation response)
    */
    BOECustomFieldsWidget.ProcessSubmitSaveEditsResults = function(success, response, originalArgs) {
        if (success) {
            var redirectOnSuccess = originalArgs.redirectOnSuccess;

            // If user-confirmation is required before proceeding ...
            if (response.NeedUserConfirmation == true) {
                $("#Save-BOECustomFields").removeClass("display-none");
                $("#Loader-BOECustomFields").addClass("display-none");

                // ... then generate the confirmation popup dialog
                Session.confirmDialog(
                    "Confirm to Continue",
                    response.UserConfirmationMessage,
                    function() {  // Yes: resubmit save with user confirmation indicated
                        var dataToSend = originalArgs.dataToSend;
                        dataToSend.UserHasConfirmed = true;

                        BOECustomFieldsWidget.SubmitSaveEdits(dataToSend, redirectOnSuccess, BOECustomFieldsWidget.ProcessSubmitSaveEditsResults);
                    },
                    function() {  // No: stay on this edit page
                    }
                );
            } else {  // save was successful
                BOECustomFieldsWidget.cleanDirty();
                $("#Save-BOECustomFields").removeClass("display-none");
                $("#Loader-BOECustomFields").addClass("display-none");

                if (redirectOnSuccess) { 
                    BOECustomFieldsWidget.BackToCustomFields();
                }
                $('.validation-box').removeClass('height-added');
            }
        } else {  // save failed
            $("#Save-BOECustomFields").removeClass("display-none");
            $("#Loader-BOECustomFields").addClass("display-none");
            //For Ie 8 needed to adjust size of module-content-center div so it would grow with 
            //table worked fine in firefox without this.
            var valbox = $('.validation-box');
                   
            if(!valbox.hasClass('height-added'))
            {
                var gridheight = valbox.outerHeight();
                var center = $('.module-content-center');
                var centerheight = center.height();
                center.height(centerheight + gridheight);
                refreshModule($('#CustomOptionsTable').parents('.module'));
                valbox.addClass('height-added');
            }
        }
    };

    $(function() {
        BOECustomFieldsWidget.afterDOMLoad();

        BOECustomFieldsWidget.RemoveStaleDialogs();

        BOECustomFieldsWidget.Module = $('#BOECustomFields');

        BOECustomFieldsWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { BOECustomFieldsWidget.cleanDirty(); });

        if($("#CustomFieldMetaData_FieldName").val()!=""){
            $('#BOECustomFields .module-header-data').html($("#CustomFieldMetaData_FieldName").val());
            $("#CustomOptionsImport").removeClass('display-none');
        }
        else {
            $("#CustomOptionsImport").addClass('display-none');
        }

        <% if (Model.CustomFieldMetaData.CustomFieldDisplayID != 0)
           { %>
        $("#BOECustomFieldsForm input[name='CustomFieldMetaData.CustomFieldDisplayID'][value='<%: (int)Model.CustomFieldMetaData.CustomFieldDisplayID  %>']").prop('checked', true);
        <% } %>
        
    /*
    *Wire up events
    */

    BOECustomFieldsWidget.registerForLiveEvent('click', 'td.ID, td.Description', function() {
            $(this).children('input').show().focus();
    });

    BOECustomFieldsWidget.registerForEvent("CUSTOM_FIELDS_LOADED", BOECustomFieldsWidget.SetupValidation);

        $('#Cancel-BOECustomFields, #Back-BOECustomFields').click(function() {
        if (BOECustomFieldsWidget.isDirty()) {
            Session.confirmDialog(
                "Cancel",
                "Are you sure you want to cancel all changes?",
                function() {
                BOECustomFieldsWidget.BackToCustomFields();
            },
                    null);
        }
        else {
            BOECustomFieldsWidget.BackToCustomFields();
        }
    });

    BOECustomFieldsWidgetGrid.registerForLiveEvent('click',  '#CustomOptionsTable tbody tr:not(.blank) div.delete', function(){
        BOECustomFieldsWidgetGrid.deleteRecord(this);

        //subtract the rowheight from the module-content-center to keep size proportional in IE.
        var rowheight =  $(this).closest('tr').height();
        var centerheight = $(this).closest('.module-content-center').height();
            $(this).closest('.module-content-center').height(centerheight - rowheight);

        refreshModule($('#CustomOptionsTable').parents('.module'));
        BOECustomFieldsWidget.setDirty();

    });

    BOECustomFieldsWidgetGrid.registerForLiveEvent('click',  '#CustomOptionsTableHeader div.delete', function() {
            $('#CustomOptionsTable tbody tr:not(.blank)').each(function() {
            BOECustomFieldsWidgetGrid.deleteRecord($(this).find('div[name=deletableRow]'));
        });

        // Only set dirty if there are populated (in use) rows left 
        // In-use rows have disabled class - use this since tr:not(.blank) still counts deleted rows here
        if ($('#CustomOptionsTable').find('tr.disabled').length) {
            BOECustomFieldsWidget.setDirty();
        }
    });


    BOECustomFieldsWidget.registerForLiveEvent('click', "#BOECustomFieldButtons.buttons button[name='save-button']:not(.disabled)",function() {
        BOECustomFieldsWidget.SaveEdits(true);
    });

    //whenever a new row is created, bind new functions
    BOECustomFieldsWidgetGrid.registerForEvent('options-grid_NEW_ROW_CREATED', function (e)
        {

        BOECustomFieldsWidgetGrid.newItemCount--;

        //For Ie 8 needed to adjust size of module-content-center div so it would grow with 
        //table worked fine in firefox without this.
        var gridheight = $(this).find('tr[class=blank]').height();
        var centerheight = $(this).find('.module-content-center').height();
            $(this).find('.module-content-center').height(centerheight + gridheight);
            
            $('#CustomOptionsTable .blank').attr("pkid", BOECustomFieldsWidgetGrid.newItemCount);
            $('#CustomOptionsTable div.delete').show();
            $('#CustomOptionsTable .blank div.delete').hide();
        refreshModule($('#CustomOptionsTable').parents('.module'));
    });

    BOECustomFieldsWidgetGrid.registerForLiveEvent('focusin', '#CustomOptionsTable td', function() {
        if (!$(this).children('div').hasClass('delete')) {
               $(this).addClass('selected');
        }
    });

    BOECustomFieldsWidgetGrid.registerForLiveEvent('focusout', '#CustomOptionsTable td', function() {
        if (!$(this).children('div').hasClass('delete')) {
               $(this).removeClass('selected');
        }
    });

    BOECustomFieldsWidgetGrid.registerForDelegateEvent('focusin', 'td.ID, td.Description', function() {
            $(this).children('.default-text').addClass('display-none');
    });

    $('input[type=radio][name="CustomFieldMetaData.CustomFieldDisplayID"]').change(function () {
        if (this.value == "<%:(int)CustomFieldType.MoqTypeTableDataDisplay%>") {
            // set to open ended if not already
            if (!$('#CustomFieldMetaData_isOpenEnded').prop('checked')) {
                $('#CustomFieldMetaData_isOpenEnded').click();
            }

            // disable the checkbox so it can't be changed from open ended
            $('#CustomFieldMetaData_isOpenEnded').addClass('disabled');
            $('#CustomFieldMetaData_isOpenEnded').prop('disabled', true);
        } else {
            $('#CustomFieldMetaData_isOpenEnded').removeClass('disabled');
            $('#CustomFieldMetaData_isOpenEnded').prop('disabled', false);
        }
    });

        $('#ImportButton').click(BOECustomFieldsWidget.ShowImportDialog);
        $('#ExportButton').click(BOECustomFieldsWidget.ExportButtonClick);
        
        $('#ImportDialog-ImportButton').click(BOECustomFieldsWidget.SubmitUploadForm);
        $('#ImportDialog-CancelButton').click(BOECustomFieldsWidget.HideImportDialog);
        
        $('#ImportResultsDialog-CloseButton').click(BOECustomFieldsWidget.HideImportResultsDialog);

    BOECustomFieldsWidget.ValidateFileInput();

    /*
    *finalize
    */

    BOECustomFieldsWidget.ContainsOCI = <%= ViewData["ContainsOCI"] %>;

		if (BOECustomFieldsWidget.ContainsOCI == true) {
			var text = '<%: SiteMasterUtilities.GetBannerText(true) %>';
            $('#OCINote').html('<b>Note:</b> ' + text);
        }
		else {
			var text = '<%: SiteMasterUtilities.GetBannerText() %>';
            $('#OCINote').html('<b>Note:</b> ' + text);
        }

        AddableGrid('options-grid');
        createModule($(BOECustomFieldsWidget.Module));
        refreshModule($(BOECustomFieldsWidget.Module));    

        <% if (Model.CustomFieldMetaData.isOpenEnded)
        { %>
            // hide values if open ended on load
            var valuesHeight = $('#values').height();
            var center = $('#values').closest('.module-content-center');
            center.height(center.height() - valuesHeight);
            $("#values").addClass("display-none");
            refreshModule($(BOECustomFieldsWidget.Module));
        <%}

        if (Model.CustomFieldMetaData.inUse)
        { %>
            // Disable the Open Ended checkbox if the field is in use
            $('#CustomFieldMetaData_isOpenEnded').prop('disabled', true);
        <% }%>

        $('#CustomFieldMetaData_isOpenEnded').change(function () {
        // Show or hide Values table
        // Adjust module height so the page scales appropriately in older versions of IE
        if (this.checked) {
            var valuesHeight = $("#values").height();
            var center = $(this).closest('.module-content-center');
            center.height(center.height() - valuesHeight);

            $("#values").addClass("display-none");
        } else {
            $("#values").removeClass("display-none");

            var valuesHeight = $("#values").height();
            var center = $(this).closest('.module-content-center');
            center.height(center.height() + valuesHeight);
            }
        refreshModule($(BOECustomFieldsWidget.Module));
        });

        // Disable open ended button if MOQ Table CF
        if ($('input[type=radio][name="CustomFieldMetaData.CustomFieldDisplayID"]:checked').val() == "<%:(int)CustomFieldType.MoqTypeTableDataDisplay%>") {
                    $('#CustomFieldMetaData_isOpenEnded').addClass('disabled');
                    $('#CustomFieldMetaData_isOpenEnded').prop('disabled', true);
        };

       function applyReadOnlyStyle() { 
            // call default applyReadOnly
            BOECustomFieldsWidget.applyReadOnly();

            // do specific read only changes
            $('#BOECustomFieldButtons').removeClass('display-none');
            $('#Save-BOECustomFields').addClass('display-none');
            $('#ImportButton').addClass('display-none');
        }

        if ( <%: ViewData["READONLY"] %>)
        {
           applyReadOnlyStyle();
       }
    });
</script>

<div id="BOECustomFields" class="boe-custom-field module">
    <div class="module-header-data">
        Custom Field Name</div>
    <div class="module-content-data">
    <div class="form-row">
            Edit the field properties.
        </div>
          
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "BOECustomFieldsForm" })) { %>
         
             <ul class="validation-box"> </ul>

        <div class="form-row">
            <div class="form-label">
                <span>Field Name *</span>
                <div id="FieldName-Help" class="help-icon" onclick="BOECustomFieldsWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="FieldName-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Field name displayed to the user on the BOE.</div>
                </div>
            </div>
            <%if (readOnly) { %>
                 <div class="form-element">
                 <%: Model.CustomFieldMetaData.FieldName %>
              
            </div>
            <% }
            else
            {%>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.CustomFieldMetaData.FieldName, new { onchange = "$('#BOECustomFields .module-header-data').html($(this).val());", @class = "customFieldName", @maxlength = "20" })%>
            </div>
            <% }%>
        </div>
        <div class="form-row">
            <div class="form-label">
                <span>Display Field for Level *</span>
                <div class="help-icon" onclick="BOECustomFieldsWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="FieldLevel-HelpDialog" class="help-dialog" style="width: 200px;">
				    <div class="help-dialog-close"></div>
                    <div id="template-boe-help" class="help-dialog-text">Specify if users should enter a value for this field for each BOE, Task<%: Model.UsingTemplateBoe ? ", Resource Type or MOQ Table." : " or Resource Type." %></div>
                </div>
            </div>
            <div class="form-element">
                <input type="radio" id="CustomFieldMetaData.CustomFieldDisplayID.BOE" name="CustomFieldMetaData.CustomFieldDisplayID" value="<%:(int)CustomFieldType.BoeDisplay%>" />
                <label for="CustomFieldMetaData.CustomFieldDisplayID.BOE">BOE</label>
                <input type="radio" id="CustomFieldMetaData.CustomFieldDisplayID.Task" name="CustomFieldMetaData.CustomFieldDisplayID" value="<%:(int)CustomFieldType.TaskDisplay%>" />
                <label for="CustomFieldMetaData.CustomFieldDisplayID.Task">Task</label>
                <input type="radio" id="CustomFieldMetaData.CustomFieldDisplayID.Labor" name="CustomFieldMetaData.CustomFieldDisplayID" value="<%:(int)CustomFieldType.LaborTypeDisplay%>" />
                <label for="CustomFieldMetaData.CustomFieldDisplayID.Labor">Resource Types</label>
                <% if (Model.UsingTemplateBoe) { %>
                <input type="radio" id="CustomFieldMetaData.CustomFieldDisplayID.MoqTable" name="CustomFieldMetaData.CustomFieldDisplayID" value="<%:(int)CustomFieldType.MoqTypeTableDataDisplay%>" />
                <label for="CustomFieldMetaData.CustomFieldDisplayID.MoqTable">MOQ Table</label>
                <% } %>
            </div>
        </div>
         <div class="form-row">
            <div class="form-label">
                <span>Required</span>
                <div class="help-icon" onclick="BOECustomFieldsWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="Required-HelpDialog" class="help-dialog" style="width: 200px;">
				    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Specify if the field is required to be entered by the user.</div>
                </div>
            </div>
            <div class="form-element">
                <%: Html.CheckBoxFor(model => model.CustomFieldMetaData.isRequired, new { name = "isRequired"})%>&nbsp;<label for="CustomFieldMetaData_isRequired">Required</label>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <span>Open Ended</span>
                <div class="help-icon" onclick="BOECustomFieldsWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="OpenEnded-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Specify if the field is open ended and contains no set values. Cannot be changed<%: Model.UsingTemplateBoe ? " for MOQ Table Custom Fields or" : "" %> if the field is in use.</div>
                </div>
            </div>
            <div class="form-element">
                <%: Html.CheckBoxFor(model => model.CustomFieldMetaData.isOpenEnded, new { name = "isOpenEnded" }) %>&nbsp;<label for="CustomFieldMetaData_isOpenEnded">Open Ended</label>
            </div>
        </div>
         <div class="form-row" id="values">
            <div class="form-label">
                <span>Values*</span>
                <div class="help-icon" onclick="BOECustomFieldsWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="Values-HelpDialog" class="help-dialog" style="width: 200px;">
				    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Users will be able to select one of these values for the field.<br />
                    <br />Values currently in use by a BOE cannot be edited or deleted. To edit or delete the value, Authors must select a different value in their BOEs.</div>
                </div>
            </div>
            <div class="form-element">
                <div class="buttons">
                    <span id="CustomOptionsImport">
                        <button id="ImportButton" class="ies-action" type="button">Import</button>
                    </span>
                    <button id="ExportButton" class="ies-action" type="button">Export</button>
                </div>
                <div class="options-grid">
                <table id="CustomOptionsTable" class="grid addable editable">
                <thead id="CustomOptionsTableHeader"><tr><th width="150px">ID</th><th width="350px">Description</th><th class="last-child" width="20px"><div class="delete"></div></th></tr></thead>
                <tbody>
                
                <%
               if (Model.CustomFieldOptions != null)
               {
                    foreach (BOECustomFieldOptionModelView fieldOption in Model.CustomFieldOptions)
                  {
                      if (fieldOption.InUse)
                      {
                      %>
                          <tr pkid="<%: fieldOption.CustomFieldOptionID %>" class="disabled">
                          <td><input type="hidden" value="<%: fieldOption.ID %>" name="ID"/><%: fieldOption.ID %></td>
                          <td><input type="text" value="<%: fieldOption.Description %>" name="Description" onchange="BOECustomFieldsWidgetGrid.updateData(this);"  /></td>
                          <td class="delete">In use</td>
                          </tr>
                      <%
                      }
                      else
                      {
                          %>
                          <tr pkid="<%: fieldOption.CustomFieldOptionID %>">
                          <td><input type="text" value="<%: fieldOption.ID %>" name="ID" onchange="BOECustomFieldsWidgetGrid.updateData(this);" /></td>
                          <td><input type="text" value="<%: fieldOption.Description %>" name="Description" onchange="BOECustomFieldsWidgetGrid.updateData(this);"  /></td>
                          <td><div class="delete" name="deletableRow"></div><%: Html.Hidden("Deleted", "False") %></td>
                          </tr>
                          
                      <%
                      }
                   }
                }
                 %>
                 <tr class="blank">
                      <td class="ID"><div class="default-text">Add ID</div><input type="text" name="ID"  onchange="BOECustomFieldsWidgetGrid.updateData(this);" /></td>
                      <td class="Description"><div class="default-text">Add description</div><input type="text" name="Description"  onchange="BOECustomFieldsWidgetGrid.updateData(this);" /></td>
                      <td><div class="delete"name="deletableRow"></div></td>
                </tr>
                </tbody>
                </table>
                </div>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label"><%--label not used but needed to have the buttons in the proper position--%></div>
            <div class="form-element">
                <div id="OCINote" class="oci-note"></div>
                <div id="BOECustomFieldButtons" class="buttons">
                    <button id="Save-BOECustomFields" class="ies-action disabled" name="save-button" type="button">Save</button>
                    <div id="Loader-BOECustomFields" class="loader display-none"></div>
                    <button id="Cancel-BOECustomFields" class="ies" name="cancel-button" type="button">Cancel</button>
                </div>
            </div>
        </div>

        <%} %>
        <button id="Back-BOECustomFields" class="ies back-to-workspace-settings-button display-none" type="button">Back to Workspace Settings</button>
    </div>
</div>
<div id="ImportInProgressDialog" style="display: none; font-size: 18px; font-weight: bold;
    font-family: Arial, Helvetica; text-align: center;">
    <div class="loader"></div>
    <br />
    Importing <%:Model.CustomFieldMetaData.FieldName%> Data
</div>
<div id="ImportDialog" style="display: none;">
    <div id="ImportDialog-Error" class="validation-box" style="display: none;">
        <div>
            <b>Import Failed.</b>
            <div id="ImportDialog-ErrorText">
            </div>
        </div>
        <div class="clear">
        </div>
    </div>
    <% Html.BeginForm(WebConstants.ACTION_IMPORT_BOE_CUSTOM_FIELD, WebConstants.CONTROLLER_WORKSPACE, new { workspace = SiteMasterUtilities.GetCurrentWorkspace(), customFieldID = Model.CustomFieldMetaData.CustomFieldID }, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportDialog-Form", target = "ImportDialog-UploadTarget" }); %>
    <div style="margin: 5px 0px 15px 0px;">
        Import a new list of options. Current list will be replaced with imported list.
    </div>
    <table style="border: none;">
        <tr style="background-color: White;">
            <td style="vertical-align: top; width: 140px; padding-top: 10px;">
                <b><%:Model.CustomFieldMetaData.FieldName%> File Location:</b>
            </td>
            <td>
                <input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
                <input type="file" size="60" id="ImportDialog-File" name="file" onchange="BOECustomFieldsWidget.ValidateFileInput();" />
                <div id="ImportDialog-FileValidation" style="color: #990000; display: none;">
                    Please select a file to import</div>
                <ul style="padding: 0px 0px 0px 15px; margin: 10px 0px 0px 0px;">
                    <li>File must contain the same headers as <a href="https://isgs-gen.external.lmco.com/sites/Estimating_Init/doclib14/Wireframes/Supporting%20Documentation/Performing%20Organizations%20Format.xlsx">
                        this format example</a> prior to importing and be in .xlsx format.</li>
                    <li>All IDs must be unique.</li>
                    <li>Options in use by a BOE will remain in the list.</li>
                </ul>
            </td>
        </tr>
    </table>
    <div style="text-align: left; margin-top: 20px;">
        <button id="ImportDialog-ImportButton" class="ies-action" name="import-button" type="button">Import</button>
        <button id="ImportDialog-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
    </div>
    <% Html.EndForm(); %>
</div>
<div id="ImportResultsDialog" class="import-results-dialog" style="display: none;">
    The Import of <%:Model.CustomFieldMetaData.FieldName%> options was successful.<br /><br />
    <div class="container">
        <div id="ImportResultsDialog-Added"></div>
        <div id="ImportResultsDialog-Changed"></div>
        <div id="ImportResultsDialog-Deleted"></div>
        <div id="ImportResultsDialog-InUse"></div>
    </div>
    <div class="buttons">
        <button id="ImportResultsDialog-CloseButton" class="ies" name="close-button" type="button">Close</button>
    </div>
</div>
