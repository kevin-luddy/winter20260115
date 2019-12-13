<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOECustomFieldOptionModelView>" %>
<%
    bool isReadOnly = bool.Parse((string)ViewData["READONLY"]);
%>
<script type="text/javascript">
    var PerfOrg_ReadOnly = '<%= isReadOnly %>'.isTrue();
    BOECustomFieldsPerfOrgWidget = new Widget("OptionDialog", PerfOrg_ReadOnly);

    BOECustomFieldsPerfOrgWidget.ImportDialog = {};
    BOECustomFieldsPerfOrgWidget.ImportDialog.Element = $('#ImportDialog');
    BOECustomFieldsPerfOrgWidget.ImportDialog.Params = { width: 750, modal: true, resizable: false, draggable: true, title: "Import Performing Organizations" };
    BOECustomFieldsPerfOrgWidget.ImportInProgressDialog = {};
    BOECustomFieldsPerfOrgWidget.ImportInProgressDialog.Element = $("#ImportInProgressDialog");
    BOECustomFieldsPerfOrgWidget.ImportInProgressDialog.Params = { width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" };
    BOECustomFieldsPerfOrgWidget.ImportResultsDialog = {};
    BOECustomFieldsPerfOrgWidget.ImportResultsDialog.Element = $("#ImportResultsDialog");
    BOECustomFieldsPerfOrgWidget.ImportResultsDialog.Params = { width: 750, modal: true, resizable: false, draggable: true, title: "Import Successful", close:  BOECustomFieldsPerfOrgWidget.ReloadGridData };    
    BOECustomFieldsPerfOrgWidget.RestoreResultsDialog = {};
    BOECustomFieldsPerfOrgWidget.RestoreResultsDialog.Element = $('#RestoreResultsDialog');
    BOECustomFieldsPerfOrgWidget.RestoreResultsDialog.Params = { width: 750, minHeight: 493, modal: true, resizable: false, draggable: true, title: "Restore in Progress" };
    BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog = {};
    BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog.Element = $('#DefaultPerformingOrgList');
    BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog.Params = { width: 650, minHeight: 493, modal: true, resizable: false, draggable: true, title: '<%: ViewData["SYSTEM_LIST_NAME"]%> - Default Options' };
    BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog.Loaded = false;
    BOECustomFieldsPerfOrgWidget.OptionDialog = {};
    BOECustomFieldsPerfOrgWidget.OptionDialog.Element = $("#OptionDialog");
    BOECustomFieldsPerfOrgWidget.OptionDialog.Params = { width: 350, modal: true, resizable: false, draggable: true };
   
    BOECustomFieldsPerfOrgWidget.ReloadGridData = function() {
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                '<%:WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG_GRID %>', ''),
            dataType: 'html',
            success: function(response)
            {             
                $('#PerformingOrgGridContent').html(response);
                $('input[name=SearchText]').val('');
                $('#DeleteLoader-BOECustomFieldsPerformingOrg').addClass('display-none');
                $('#Delete-BOECustomFieldsPerformingOrg').removeClass('display-none');
                $('#Delete-BOECustomFieldsPerformingOrg').addClass('disabled');
                refreshModule($(BOECustomFieldsPerfOrgWidget.Module));
            }
        });
    }

    BOECustomFieldsPerfOrgWidget.ConfirmRestore = function() {
        var text = 'Are you sure you want to use the default list of options? Options in use by at least one BOE will remain in the list.';
        var title = 'Restore or Get Updated Default Options';
        
        Session.confirmDialog(title, text, BOECustomFieldsPerfOrgWidget.RestoreConfirmed, null);
    }

    BOECustomFieldsPerfOrgWidget.RestoreConfirmed = function() {
        BOECustomFieldsPerfOrgWidget.OpenDialogAfterInitialize(BOECustomFieldsPerfOrgWidget.RestoreResultsDialog);

        $('#RestoreResultsDialog-Loader').removeClass('display-none');
        $('#RestoreResultsDialog-Waiting').removeClass('display-none');
        $('#RestoreResultsData').addClass('display-none');

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.ACTION_RESTORE_CUSTOM_FIELD_PERFORMING_ORGANIZATIONS %>', ''),
            dataType: 'html',
            success: function(response) {
                $('#RestoreResultsDialog-Loader').addClass('display-none');
                $('#RestoreResultsDialog-Waiting').addClass('display-none');
                $('#RestoreResultsData').removeClass('display-none');
                $("#RestoreResultsDialog").dialog({title : "Restore Successful"});
                $('#RestoreResultsData').html(response);
                $('#BOECustomFieldsPerfOrg .restore-link').addClass('display-none');
                BOECustomFieldsPerfOrgWidget.ReloadGridData();
            },
            error: function() { 
                $('#RestoreResultsDialog-Loader').addClass('display-none');
                $('#RestoreResultsDialog-Waiting').addClass('display-none');
                BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.RestoreResultsDialog); 
            }
        });
    }

    BOECustomFieldsPerfOrgWidget.DeleteRecords = function(event, data) {
         Session.confirmDialog("Delete Performing Organizations Confirmation", "Are you sure you want to delete these Performing Organizations?",
            function () {
                $('#Delete-BOECustomFieldsPerformingOrg').addClass('display-none');
                $('#DeleteLoader-BOECustomFieldsPerformingOrg').removeClass('display-none');
                BOECustomFieldsPerfOrgWidget.SavePerformingOrgs(data.CheckedItems);
            }, function () {
                $('#Delete-BOECustomFieldsPerformingOrg').removeClass('display-none');
                $('#DeleteLoader-BOECustomFieldsPerformingOrg').addClass('display-none');
            });        
    }
    
    BOECustomFieldsPerfOrgWidget.AddRecord = function() {
        $('#OptionDialog input[name="ID"]').val('');
        $('#OptionDialog input[name="UpdateDateLong"]').val('0');
        $('#OptionDialog input[name="PerformingOrgID"]').val(-1);
        $('#OptionDialog input[name="Description"]').val('');
        BOECustomFieldsPerfOrgWidget.OpenDialogAfterInitialize(BOECustomFieldsPerfOrgWidget.OptionDialog);
        BOECustomFieldsPerfOrgWidget.OptionDialog.Element.dialog({title : "Add Performing Org"});
    }

    BOECustomFieldsPerfOrgWidget.EditRecord = function(event, data) {
        $('#OptionDialog input[name="ID"]').val(data.ID);
        $('#OptionDialog input[name="UpdateDateLong"]').val(data.UpdateDateLong);
        $('#OptionDialog input[name="PerformingOrgID"]').val(data.CustomFieldOptionID);
        $('#OptionDialog input[name="Description"]').val(data.Description);
        BOECustomFieldsPerfOrgWidget.OpenDialogAfterInitialize(BOECustomFieldsPerfOrgWidget.OptionDialog);
        BOECustomFieldsPerfOrgWidget.OptionDialog.Element.dialog({title : "Edit Performing Org"});
    }

    BOECustomFieldsPerfOrgWidget.SaveRecord = function() {

            var data = new Array();
            var option = {};
            option.ID = $('#OptionDialog input[name=ID]').val();
            option.UpdateDateLong = $('#OptionDialog input[name=UpdateDateLong]').val().toString();
            option.CustomFieldOptionID = $('#OptionDialog input[name=PerformingOrgID]').val();
            option.Description = $('#OptionDialog input[name=Description]').val();

            data.push(option);

            BOECustomFieldsPerfOrgWidget.SavePerformingOrgs(data);
       
    }

    BOECustomFieldsPerfOrgWidget.SavePerformingOrgs = function (data) {
        if (data.length > 0) {
            var dataToSend = {};
            dataToSend.performingOrgs = data;

            dataToSend = JSON.stringify(dataToSend);

            BOECustomFieldsPerfOrgWidget.ajaxRequest({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_SAVE_CUSTOM_FIELD_PERFORMING_ORGS %>', ''),
                contentType: 'application/json; charset=utf-8',
                data: dataToSend,
                success: function(response) {
                    BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.OptionDialog);
                    $('#BOECustomFieldsPerfOrg .restore-link').removeClass('display-none');

                    if (response.Status == true) {
                        BOECustomFieldsPerfOrgWidget.ReloadGridData();
                    }
                    else {
                        // this was an edit, update the row
                        var originalPkid = data[0].CustomFieldOptionID;
                        var originalRow = $('#BOECustomFieldsPerfOrg tr[pkid=' + originalPkid + ']');
                        var row = originalRow.clone();

                        row.attr('pkid', response.CustomFieldOptionID);
                        row.find('a[name=ID]').html(response.ID);
                        row.find('input[name=UpdateDateLong]').val(response.UpdateDateLong);
                        row.find('a[name=Description]').html(response.Description);


                        originalRow.replaceWith(row);
                    }
                },
                error: function(response) {
                    $('#Delete-BOECustomFieldsPerformingOrg').removeClass('display-none');
                    $('#DeleteLoader-BOECustomFieldsPerformingOrg').addClass('display-none');
                }
            }, $('#Save-OptionDialog'));
        }
        else {
            $('#Delete-BOECustomFieldsPerformingOrg').addClass('disabled');
            $('#Delete-BOECustomFieldsPerformingOrg').removeClass('display-none');
            $('#DeleteLoader-BOECustomFieldsPerformingOrg').addClass('display-none');
        }
    }

    BOECustomFieldsPerfOrgWidget.Search = function(element) {
        var data = {};
        data.searchText = element.val();
        var dataToSend = JSON.stringify(data);

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                '<%:WebConstants.ACTION_SEARCH_CUSTOM_FIELD_PERFORMING_ORGANIZATIONS %>', ''),
            contentType: 'application/json; charset=utf-8',
            data: dataToSend,
            dataType: 'html',
            success: function(response) {
                $('#PerformingOrgGridContent').html(response);
                $('#DeleteLoader-BOECustomFieldsPerformingOrg').addClass('display-none');
                $('#Delete-BOECustomFieldsPerformingOrg').removeClass('display-none');
                $('#Delete-BOECustomFieldsPerformingOrg').addClass('disabled');
                refreshModule($(BOECustomFieldsPerfOrgWidget.Module));
            }
        });
    }

    BOECustomFieldsPerfOrgWidget.PagePerformingOrgs = function(event, data) {
        var dataToSend = JSON.stringify(data);

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                '<%:WebConstants.ACTION_PAGE_CUSTOM_FIELD_PERFORMING_ORGS %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function(response)
            {             
                $('#PerformingOrgGridContent').html(response);
                $('input[name=SearchText]').val('');
                $('#DeleteLoader-BOECustomFieldsPerformingOrg').addClass('display-none');
                $('#Delete-BOECustomFieldsPerformingOrg').removeClass('display-none');
                $('#Delete-BOECustomFieldsPerformingOrg').addClass('disabled');
                refreshModule($(BOECustomFieldsPerfOrgWidget.Module));
            }
        });
    }

    /*-------------------------------  Import/Export Functions --------------------------------*/
    
    // Remove old dialogs left behind when jumping back to main jump page
    BOECustomFieldsPerfOrgWidget.RemoveStaleDialogs = function() {
        $('body .ui-dialog').children('#ImportDialog, #ImportInProgressDialog, #ImportResultsDialog, #RestoreResultsDialog, #DefaultResourceList').parent().remove();
        $('body').children('#ImportDialog, #ImportInProgressDialog, #ImportResultsDialog, #RestoreResultsDialog, #DefaultResourceList').remove();
    }

    BOECustomFieldsPerfOrgWidget.ExportButtonClick = function() {
        if (Session.isDirty())
        {
            Session.confirmDialog(
            "Save Changes",
            "Changes must be saved to include them in the export. Do you want to save changes made before exporting?",
            function()
            {
                //TODO: Geoff, add a call to save here 31683
                BOECustomFieldsPerfOrgWidget.Export();
            },
            BOECustomFieldsPerfOrgWidget.Export);
        }
        else {
            BOECustomFieldsPerfOrgWidget.Export();
        }
    }

    BOECustomFieldsPerfOrgWidget.Export = function() {       
        // Remove the old hidden iFrame, if it exists
        $('#DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DownloadTarget',
            'class': 'display-none',
            'src': CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_EXPORT_BOE_CUSTOM_FIELD_PERFORMING_ORG %>',
                    '')
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    }

    BOECustomFieldsPerfOrgWidget.SubmitUploadForm = function () {
        if (BOECustomFieldsPerfOrgWidget.ValidateFileInput())
        {
            BOECustomFieldsPerfOrgWidget.HideImportDialogError();
            BOECustomFieldsPerfOrgWidget.OpenDialogAfterInitialize(BOECustomFieldsPerfOrgWidget.ImportInProgressDialog);
            
            BOECustomFieldsPerfOrgWidget.AppendIFrameForUploadResponse();
            $("#ImportDialog-Form").submit();
        }
    }

    BOECustomFieldsPerfOrgWidget.AppendIFrameForUploadResponse = function() {
        // Remove the old hidden iFrame, if it exists
        $('#ImportDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportDialog-Form').append('<iframe id="ImportDialog-UploadTarget" name="ImportDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportDialog-UploadTarget').load(BOECustomFieldsPerfOrgWidget.StopUpload);

        BOECustomFieldsPerfOrgWidget.SetDocumentDomains();
    }

    BOECustomFieldsPerfOrgWidget.StopUpload = function() { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide dialogs
                BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.ImportDialog);
                BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.ImportInProgressDialog);

                // Render Import Results Dialog
                BOECustomFieldsPerfOrgWidget.RenderImportResults(results.Data);

                // Show Import Results
                BOECustomFieldsPerfOrgWidget.OpenDialogAfterInitialize(BOECustomFieldsPerfOrgWidget.ImportResultsDialog);
            }
            else {
                $("#ImportDialog-ErrorText").html(results.Message);
                BOECustomFieldsPerfOrgWidget.ShowImportDialogError();
                BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.ImportInProgressDialog);
            }
        }
        else {
            BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.ImportInProgressDialog);
        }
    }

    BOECustomFieldsPerfOrgWidget.RenderImportResults = function(resultsData) {
    
        // Render html for options that were added
        if (resultsData != null && resultsData.AddedOptions != undefined && resultsData.AddedOptions != null && resultsData.AddedOptions.length) {
            var options = resultsData.AddedOptions;

            var newHTML = "<b>" + options.length + " options added:</b><ul>";
                    
            for(var ndx = 0; ndx < options.length; ndx++)
            {
                newHTML += "<li>" +
                            options[ndx].PerformingOrgName + ", " +
                            options[ndx].PerformingOrgDesc +
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
                    
            for(var ndx = 0; ndx < options.length; ndx++)
            {
                newHTML += "<li>" +
                            options[ndx].Old.PerformingOrgName + ", " +
                            options[ndx].Old.PerformingOrgDesc +
                            " changed to " +
                            options[ndx].New.PerformingOrgName + ", " +
                            options[ndx].New.PerformingOrgDesc +
                            "</li>";
            }

            newHTML += "</ul>";
        }
        else {
            var newHTML = "<b>0 options changed.</b>";
        }

        $('#ImportResultsDialog-Changed').html(newHTML);
                    
        // Render html for options that were deleted
        if (resultsData != null && resultsData != null && resultsData.DeletedOptions != undefined && resultsData.DeletedOptions != null && resultsData.DeletedOptions.length)
        {
            var options = resultsData.DeletedOptions;

            var newHTML = "<b>" + options.length + " options deleted:</b><ul>";
                    
            for(var ndx = 0; ndx < options.length; ndx++)
            {
                newHTML += "<li>" +
                            options[ndx].PerformingOrgName + ", " +
                            options[ndx].PerformingOrgDesc +
                            "</li>";
            }

            newHTML += "</ul>";
        }
        else {
            var newHTML = "<b>0 options deleted.</b>";
        }

        $('#ImportResultsDialog-Deleted').html(newHTML);
                    
        // Render html for options that were in use and left unchanged
        if (resultsData != null && resultsData.InUseOptions != undefined && resultsData.InUseOptions != null && resultsData.InUseOptions.length)
        {
            var options = resultsData.InUseOptions;

            var newHTML = "<b>" + options.length + " options could not be changed or deleted because they are in use by at least one BOE:</b><ul>";
                    
            for(var ndx = 0; ndx < options.length; ndx++)
            {
                newHTML += "<li>" +
                            options[ndx].PerformingOrgName + ", " +
                            options[ndx].PerformingOrgDesc +
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
    BOECustomFieldsPerfOrgWidget.SetDocumentDomains = function () {
        $('#ImportDialog-DocumentDomain').val(document.domain);
    }

    BOECustomFieldsPerfOrgWidget.ValidateFileInput = function() {
        if ($("#ImportDialog-File").val().length == 0) {
            BOECustomFieldsPerfOrgWidget.ShowImportDialogFileValidation();
            BOECustomFieldsPerfOrgWidget.DisableImportDialogImportButton();
            return false;
        }
        else {
            BOECustomFieldsPerfOrgWidget.HideImportDialogFileValidation();
            BOECustomFieldsPerfOrgWidget.EnableImportDialogImportButton();
            return true;
        }
    }

    BOECustomFieldsPerfOrgWidget.EnableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").removeClass('disabled');
    }

    BOECustomFieldsPerfOrgWidget.DisableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").addClass('disabled');
    }

    BOECustomFieldsPerfOrgWidget.ShowImportDialogError = function () {
        $("#ImportDialog-Error").slideDown("slow");
    }

    BOECustomFieldsPerfOrgWidget.HideImportDialogError = function () {
        $("#ImportDialog-Error").slideUp("slow");
    }

    BOECustomFieldsPerfOrgWidget.ShowImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideDown("slow");
    }

    BOECustomFieldsPerfOrgWidget.HideImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideUp("slow");
    }
   
    /*-------------------------------  End Import/Export Functions --------------------------------*/

    $(function() {
        BOECustomFieldsPerfOrgWidget.RemoveStaleDialogs();

        BOECustomFieldsPerfOrgWidget.InitializeDialog(BOECustomFieldsPerfOrgWidget.RestoreResultsDialog);
        BOECustomFieldsPerfOrgWidget.InitializeDialog(BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog);
        BOECustomFieldsPerfOrgWidget.InitializeDialog(BOECustomFieldsPerfOrgWidget.OptionDialog);
        BOECustomFieldsPerfOrgWidget.InitializeDialog(BOECustomFieldsPerfOrgWidget.ImportDialog);
        BOECustomFieldsPerfOrgWidget.InitializeDialog(BOECustomFieldsPerfOrgWidget.ImportInProgressDialog);
        BOECustomFieldsPerfOrgWidget.InitializeDialog(BOECustomFieldsPerfOrgWidget.ImportResultsDialog);

        BOECustomFieldsPerfOrgWidget.Module = $('#BOECustomFieldsPerfOrg');

        $('#Cancel-BOECustomFieldsPerfOrg, #Back-BOECustomFieldsPerfOrg').click(function() { 
            window.location.hash = '#BOECustomFields';
        });

        BOECustomFieldsPerfOrgWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { BOECustomFieldsPerfOrgWidget.cleanDirty(); });

        BOECustomFieldsPerfOrgWidget.registerForLiveEvent('click', '#Delete-BOECustomFieldsPerformingOrg:not(.disabled)', function() { $(document).trigger('GET_DELETED_PERFORMING_ORGS'); });
        $('#Add-BOECustomFieldsPerformingOrg').click(BOECustomFieldsPerfOrgWidget.AddRecord);
        $('#Import-BOECustomFieldsPerformingOrg').click(function() { BOECustomFieldsPerfOrgWidget.OpenDialogAfterInitialize(BOECustomFieldsPerfOrgWidget.ImportDialog); });
        $('#Export-BOECustomFieldsPerformingOrg').click(BOECustomFieldsPerfOrgWidget.ExportButtonClick);

        $('#Search-BOECustomFieldsPerformingOrg, #Search-BOECustomFieldsPerformingOrg2').click(function() { BOECustomFieldsPerfOrgWidget.Search($(this).parent().find('input[name=SearchText]')); });
        
        BOECustomFieldsPerfOrgWidget.registerForDelegateEvent('keydown',
            'input[name=SearchText]', function (e) {
                /****enter key to search */
                var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
                if (keyCode == 13) {
                    BOECustomFieldsPerfOrgWidget.Search($(this));
                }
            },
            '#BOECustomFieldsPerfOrg'
        ); 

        $('#ImportDialog-ImportButton').click(BOECustomFieldsPerfOrgWidget.SubmitUploadForm);
        $('#ImportDialog-CancelButton').click(function() { BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.ImportDialog); });
        
        $('#ImportResultsDialog-CloseButton').click(function() { BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.ImportResultsDialog); });

        BOECustomFieldsPerfOrgWidget.registerForDelegateEvent('click',
            '#Save-OptionDialog:not(.disabled)', BOECustomFieldsPerfOrgWidget.SaveRecord);
        $('#BOECustomFieldsPerfOrg-Cancel-OptionDialog').click(function () { BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.OptionDialog); });
        
        BOECustomFieldsPerfOrgWidget.ValidateFileInput();

        $('#RestoreButton').click(function() {
            BOECustomFieldsPerfOrgWidget.ConfirmRestore();
        });

        $('#ViewDefaultList').click(function() {
            BOECustomFieldsPerfOrgWidget.OpenDialogAfterInitialize(BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog);

            if (!BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog.Loaded) {
                $('#DefaultPerformingOrgList-Loader').removeClass('display-none');
                $('#DefaultPerformingOrgList-Waiting').removeClass('display-none');
                $('#DefaultListData').addClass('display-none');
            
                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG_VIEW_DEFAULT %>', ''),
                    dataType: 'html',
                    success: function(response) {
                        $('#DefaultPerformingOrgList-Loader').addClass('display-none');
                        $('#DefaultPerformingOrgList-Waiting').addClass('display-none');
                        $('#DefaultListData').removeClass('display-none');
                        $('#DefaultListData').html(response);
                        BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog.Loaded = true;
                    },
                    error: function() { 
                        $('#DefaultPerformingOrgList-Loader').addClass('display-none');
                        $('#DefaultPerformingOrgList-Waiting').addClass('display-none');
                        BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog); 
                    }
                });
            }
        });        

        BOECustomFieldsPerfOrgWidget.registerForEvent("CLOSE_PERFORMING_ORG_RESTORE_RESULTS", function() { BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.RestoreResultsDialog); });
        BOECustomFieldsPerfOrgWidget.registerForEvent("CLOSE_DEFAULT_PERFORMING_ORGS", function() { BOECustomFieldsPerfOrgWidget.CloseDialog(BOECustomFieldsPerfOrgWidget.DefaultPerformingOrgsListDialog); });
        BOECustomFieldsPerfOrgWidget.registerForEvent('PERFORMING_ORG_CHECKED', function() { $('#Delete-BOECustomFieldsPerformingOrg').removeClass('disabled'); });
        BOECustomFieldsPerfOrgWidget.registerForEvent('DELETE_PERFORMING_ORGS', BOECustomFieldsPerfOrgWidget.DeleteRecords);
        BOECustomFieldsPerfOrgWidget.registerForEvent('EDIT_PERFORMING_ORG', BOECustomFieldsPerfOrgWidget.EditRecord);
        BOECustomFieldsPerfOrgWidget.registerForEvent('PAGE_PERFORMING_ORGS', BOECustomFieldsPerfOrgWidget.PagePerformingOrgs);

        createModule($(BOECustomFieldsPerfOrgWidget.Module));
        refreshModule($(BOECustomFieldsPerfOrgWidget.Module));

        BOECustomFieldsPerfOrgWidget.ReloadGridData();
    });

</script>

<div id="BOECustomFieldsPerfOrg" class="boe-custom-field-performing-org module">
    <div class="module-header-data">Performing Organization</div>
    <div class="module-content-data">
        <div class="form-row">
            Edit the field properties.
        </div>
        <div class="form-row">
            <div class="form-label">
                <span>Field Name *</span>
                <div id="FieldName-Help" class="help-icon" onclick="BOECustomFieldsPerfOrgWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="FieldName-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Field name displayed to the user on the BOE.</div>
                </div>
            </div>
            <div class="form-element">
                Performing Organization
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <span>Display Field for Level *</span>
                <div id="FieldLevel-Help" class="help-icon" onclick="BOECustomFieldsPerfOrgWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="FieldLevel-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Performing Organization can only be entered for each Resource Type.</div>
                </div>
            </div>
            <div class="form-element">
                <%: Html.RadioButton("BOE", false, false, new { disabled = "true" })%>&nbsp;<label for="BOE">BOE</label>
                <%: Html.RadioButton("Task", false, false, new { disabled = "true" })%>&nbsp;<label for="Task">Task</label>
                <%: Html.RadioButton("Resource Type", true, true, new { disabled = "true" })%>&nbsp;<label for="Resource Type">Resource Type</label>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <span>Required Field</span>
                <div id="RequiredField-Help" class="help-icon" onclick="BOECustomFieldsPerfOrgWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="RequiredField-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">This is a required field and cannot be changed.</div>
                </div>
            </div>
            <div class="form-element">
                <%: Html.CheckBox("Required", true, new { disabled="true"}) %>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <span>Options *</span>
                <div id="Options-Help" class="help-icon" onclick="BOECustomFieldsPerfOrgWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="Options-HelpDialog" class="help-dialog" style="width: 300px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Users will be able to select one of these options for the field.<br /><br />Options currently in use by a BOE cannot be edited or deleted. To edit or delete the option, Authors must select a different option in their BOEs.</div>
                </div>
                <div class="restore-link<% if (!(bool)ViewData["PERFORMING_ORGS_CHANGED"] || isReadOnly) { %> display-none<% } %>">
                    <span><a id="RestoreButton">Restore or get updated default options</a> (<a id="ViewDefaultList">View</a>)</span>
                    <div id="Restore-Help" class="help-icon" onclick="BOECustomFieldsPerfOrgWidget.ToggleHelp(this);"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="Restore-HelpDialog" class="help-dialog" style="width: 300px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">The Workspace’s options are different than the default <%: ViewData["SYSTEM_LIST_NAME"] %> options provided by the System Administrator because either changes were made to the Workspace options or the System Administrator has updated the default options. </div>
                    </div>
                </div>
            </div>
            <div class="form-element">
                <div class="buttons">
                    <% if (!isReadOnly) { %>
                    <button id="Delete-BOECustomFieldsPerformingOrg" class="ies-action disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-BOECustomFieldsPerformingOrg" class="loader display-none"></div>
                    <button id="Add-BOECustomFieldsPerformingOrg" class="ies-action" type="button">+ Add</button> 
                    <button id="Import-BOECustomFieldsPerformingOrg" class="ies-action" name="import-button" type="button">Import</button>
                    <% } %>
                    <button id="Export-BOECustomFieldsPerformingOrg" class="ies-action" type="button">Export</button>
                </div>
                <div class="search-box float-right">
                    <div class="paging-control"></div>
                    <%: Html.TextBox("SearchText") %>
                    <div id="Search-BOECustomFieldsPerformingOrg" class="button search-magnify-glass-button"></div>
                </div>
                <div id="PerformingOrgGridContent"></div>
                <div style="height: 28px; margin-top: 10px">
                    <div class="search-box inline float-right">
                        <div class="paging-control"></div>
                        <%: Html.TextBox("SearchText") %>
                        <div id="Search-BOECustomFieldsPerformingOrg2" class="button search-magnify-glass-button"></div>
                    </div> 
                </div> 
            </div>
        </div>
    </div>
</div>

<div id="ImportInProgressDialog" style="display: none; font-size: 18px; font-weight: bold; font-family: Arial, Helvetica; text-align: center;">
    <div class="loader"></div>
    <br />
    Importing Performing Organization Data
</div>
<div id="ImportDialog" style="display: none;">
    <div id="ImportDialog-Error" class="validation-box" style="display: none;">
        <div><b>Import Failed.</b>
            <div id="ImportDialog-ErrorText">
            </div>
        </div>
        <div class="clear"></div>
    </div>
    <% Html.BeginForm(WebConstants.ACTION_IMPORT_BOE_CUSTOM_FIELD_PERFORMING_ORG, WebConstants.CONTROLLER_WORKSPACE, new {workspace= SiteMasterUtilities.GetCurrentWorkspace()}, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportDialog-Form", target = "ImportDialog-UploadTarget" }); %>
    <div style="margin: 5px 0px 15px 0px;">
        Import a new list of options. Current list will be replaced with imported list.
    </div>
    <table style="border: none;">
        <tr style="background-color: White;">
            <td style="vertical-align: top; width: 140px; padding-top: 10px;">
                <b>Performing Organizations File Location:</b>
            </td>
            <td>
                <input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
                <input type="file" size="60" id="ImportDialog-File" name="file" onchange="BOECustomFieldsPerfOrgWidget.ValidateFileInput();" />
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
    The Import of Performing Organization options was successful.<br /><br />
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

<div id="OptionDialog" style="display: none;">

    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "PerformingOrgOptionForm" })) { %>
    <ul class="validation-box"></ul>
    <div class="form-row">
        <div class="form-label">ID*</div>
        <div class="form-element">
            <input type="text" id="ID" name="ID" maxlength="20" />
            <input type="hidden" id="UpdateDateLong" name="UpdateDateLong" />
            <input type="hidden" id="PerformingOrgID" name="PerformingOrgID" />
            <input type="hidden" id="PerformingOrgListID" name="PerformingOrgListID" value="<%: ViewData["SYSTEM_LIST_ID"] %>"/>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Description*</div>
        <div class="form-element">
            <input type="text" id="Description" name="Description" maxlength="50"/>
        </div>
    </div>
    <div class="form-label"></div>
    <div class="form-element">
        <div class="buttons">
            <button id="Save-OptionDialog" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="Loader-OptionDialog" class="loader display-none"></div>
            <button id="BOECustomFieldsPerfOrg-Cancel-OptionDialog" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
    </div>
    <% } %>
</div>

<div id="RestoreResultsDialog" class="restore-results-dialog" style="display: none;">
    <div id="RestoreResultsDialog-Loader" class="loader"></div>
    <div id="RestoreResultsDialog-Waiting">
        <br />
        Restoring Performing Organization Data
    </div>
    <div id="RestoreResultsData">
    </div>
</div>

<div id="DefaultPerformingOrgList" class="default-performing-org-dialog" style="display: none;">
    <div id="DefaultPerformingOrgList-Loader" class="loader"></div>
    <div id="DefaultPerformingOrgList-Waiting">
        <br />
        Retrieving <%: ViewData["SYSTEM_LIST_NAME"]%> Default Options
    </div>
    <div id="DefaultListData">
    </div>
</div>
