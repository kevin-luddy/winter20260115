<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.DefaultPerfOrgModelView>" %>

<script type="text/javascript">

    // Get the read-only attribute passed in from the controller
    var ManageDefaultPerfOrgGrid_ReadOnly = <%= ViewData["READONLY"] %>;

    var ManageDefaultPerfOrgWidget = new Widget('OptionDialog', ManageDefaultPerfOrgGrid_ReadOnly);
    
    ManageDefaultPerfOrgWidget.ImportDialog = {};
    ManageDefaultPerfOrgWidget.ImportDialog.Element = $('#ImportDialog');
    ManageDefaultPerfOrgWidget.ImportDialog.Params = { width: 750, modal: true, resizable: false, draggable: true, title: "Import Performing Organizations" };
    ManageDefaultPerfOrgWidget.ImportInProgressDialog = {};
    ManageDefaultPerfOrgWidget.ImportInProgressDialog.Element = $("#ImportInProgressDialog");
    ManageDefaultPerfOrgWidget.ImportInProgressDialog.Params = { width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" };
    ManageDefaultPerfOrgWidget.OptionDialog = {};
    ManageDefaultPerfOrgWidget.OptionDialog.Element = $("#OptionDialog");
    ManageDefaultPerfOrgWidget.OptionDialog.Params = { width: 350, modal: true, resizable: false, draggable: true,close: function(){}};

    ManageDefaultPerfOrgWidget.BindEvents = function() {
        
        // Bind events for single UI elements
        $('#ManageDefaultPerfOrgGrid-ImportButton').click(function () { ManageDefaultPerfOrgWidget.OpenDialogAfterInitialize(ManageDefaultPerfOrgWidget.ImportDialog) });
        $('#ManageDefaultPerfOrgGrid-ExportButton').click(ManageDefaultPerfOrgWidget.Export);
        $('#ImportDialog-ImportButton').click(ManageDefaultPerfOrgWidget.SubmitUploadForm);
        $('#ImportDialog-CancelButton').click(function() { ManageDefaultPerfOrgWidget.CloseDialog(ManageDefaultPerfOrgWidget.ImportDialog) });
        $("#ImportDialog-UploadTarget").load(ManageDefaultPerfOrgWidget.StopUpload);
        $('#ManageDefaultPerfOrgGrid-SearchMagnifyButton').click(ManageDefaultPerfOrgWidget.Search);
        $('#ManageDefaultPerfOrgGrid-AddButton').click(ManageDefaultPerfOrgWidget.AddRecord);
        $('#ManageDefaultPerfOrg-Cancel-OptionDialog').click(function() {
            ManageDefaultPerfOrgWidget.CloseDialog(ManageDefaultPerfOrgWidget.OptionDialog); 
            ManageDefaultPerfOrgWidget.cleanDirty();
         });

        ManageDefaultPerfOrgWidget.registerForDelegateEvent('click','#Save-OptionDialog:not(.disabled)',ManageDefaultPerfOrgWidget.SaveRecord);


        // Bind live events
        ManageDefaultPerfOrgWidget.registerForLiveEvent('click', '#ManageDefaultPerfOrgGrid-DeleteButton:not(.disabled)', function() { 
            Session.confirmDialog("Delete Performing Organizations Confirmation", "Are you sure you want to delete these Performing Organizations?", function() { $(document).trigger('GET_DELETED_PERFORMING_ORGS'); }); 
        });
        ManageDefaultPerfOrgWidget.registerForLiveEvent('click', "#Save-ManageDefaultPerfOrg:not(.disabled)", ManageDefaultPerfOrgWidget.SaveButtonClick);
        ManageDefaultPerfOrgWidget.registerForEvent('PERF_ORG_CHECKED', function() {
            if($("#PerfOrgGridContent input:checkbox:checked").length === 0)
            {
                $('#ManageDefaultPerfOrgGrid-DeleteButton').addClass('disabled');
            }
            else{
                $('#ManageDefaultPerfOrgGrid-DeleteButton').removeClass('disabled');
            };
        });
        ManageDefaultPerfOrgWidget.registerForEvent('DELETE_PERFORMING_ORGS', ManageDefaultPerfOrgWidget.DeleteRecords);
        ManageDefaultPerfOrgWidget.registerForEvent('PAGE_PERFORMING_ORGS', ManageDefaultPerfOrgWidget.PagePerformingOrgs);
        ManageDefaultPerfOrgWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageDefaultPerfOrgWidget.cleanDirty(); });
         ManageDefaultPerfOrgWidget.registerForEvent('EDIT_PERF_ORG', ManageDefaultPerfOrgWidget.EditRecord);

    }

    ManageDefaultPerfOrgWidget.BackToJumpPage = function() {
        ManageDefaultPerfOrgWidget.cleanDirty();
               
        window.location.hash = '#';
    }

    ManageDefaultPerfOrgWidget.PagePerformingOrgs = function(event, data) {
        var dataToSend = JSON.stringify(data);

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_PAGE_PERF_ORGS %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function(response)
            {             
                $('#PerfOrgGridContent').html(response);
                $('#SearchText').val('');
                $('#ManageDefaultPerfOrgGrid-DeleteLoader').addClass('display-none');
                $('#ManageDefaultPerfOrgGrid-DeleteButton').removeClass('display-none');
                $('#ManageDefaultPerfOrgGrid-DeleteButton').addClass('disabled');
            }
        });
    }

    ManageDefaultPerfOrgWidget.ReloadGridData = function() {
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_PERF_ORGS_GRID %>', ''),
            dataType: 'html',
            success: function(response)
            {             
                $('#PerfOrgGridContent').html(response);
                $('#SearchText').val('');
                $('#ManageDefaultPerfOrgGrid-DeleteLoader').addClass('display-none');
                $('#ManageDefaultPerfOrgGrid-DeleteButton').removeClass('display-none');
                $('#ManageDefaultPerfOrgGrid-DeleteButton').addClass('disabled');
            }
        });
    }

    ManageDefaultPerfOrgWidget.DeleteRecords = function (event, data) {
        data = data.CheckedItems;        
      
        $('#ManageDefaultPerfOrgGrid-DeleteLoader').removeClass('display-none');
        $('#ManageDefaultPerfOrgGrid-DeleteButton').addClass('display-none');

        // Getting values from the model since Deleting should never svae changes to the list name.
        ManageDefaultPerfOrgWidget.data.PerfOrgListName = "<%= Model.PerfOrgListName %>";
        ManageDefaultPerfOrgWidget.data.PerfOrgListID = <%: Model.PerfOrgListID %>;
        ManageDefaultPerfOrgWidget.data.UpdateDateLong = '<%: Model.UpdateDateLong %>';
            
        ManageDefaultPerfOrgWidget.data.DefaultPerfOrgs = data;

        var dataToSend = JSON.stringify(ManageDefaultPerfOrgWidget.data);

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_SAVE_DEFAULT_PERF_ORGS %>', ''),
            contentType: 'application/json; charset=utf-8',
            data: dataToSend,
            success: function()
            {           
                ManageDefaultPerfOrgWidget.ReloadGridData();
                RaiseNotification('Options Deleted');
            },
            error: function() {
                $('#ManageDefaultPerfOrgGrid-DeleteLoader').addClass('display-none');
                $('#ManageDefaultPerfOrgGrid-DeleteButton').removeClass('display-none');
            }
        });
     }


    ManageDefaultPerfOrgWidget.PreparedForSubmit = function() {
        return true;
    }

    ManageDefaultPerfOrgWidget.Search = function() {
        var text = {};
        text.inSearchText = $("#SearchText").val();
        var dataToSend = JSON.stringify(text);
           
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_SEARCH_PERF_ORGS %>', ''),
            contentType: 'application/json; charset=utf-8',
            data: dataToSend.toString(),
            dataType: 'html',
            success: function(SearchResponse) {
                $('#PerfOrgGridContent').html(SearchResponse);
                $('#ManageDefaultPerfOrgGrid-DeleteLoader').addClass('display-none');
                $('#ManageDefaultPerfOrgGrid-DeleteButton').removeClass('display-none');
                $('#ManageDefaultPerfOrgGrid-DeleteButton').addClass('disabled');
            }
        });
    }

    ManageDefaultPerfOrgWidget.AddRecord = function ()
    {
       $('#PerfOrgID').val('');
       $('#PerfOrgDesc').val('');
       $('#PerfOrgDBID').val(-1);
       $('#UpdateDateLong').val('0');
       $('#Save-OptionDialog').addClass('disabled');    
       ManageDefaultPerfOrgWidget.OpenDialogAfterInitialize(ManageDefaultPerfOrgWidget.OptionDialog); 
       ManageDefaultPerfOrgWidget.clearValidationBox($("#PerfOrgOptionForm ul.validation-box"));
       ManageDefaultPerfOrgWidget.OptionDialog.Element.dialog({title : "Add Option"});
    }

    ManageDefaultPerfOrgWidget.EditRecord = function(event, data)
    {
        $('#PerfOrgID').val(data.Name);
        $('#PerfOrgDesc').val(data.Description);
        $('#PerfOrgDBID').val(data.ID);
        $('#UpdateDateLong').val(data.UpdateDate);
        $('#Save-OptionDialog').addClass('disabled');
        ManageDefaultPerfOrgWidget.OpenDialogAfterInitialize(ManageDefaultPerfOrgWidget.OptionDialog);
        ManageDefaultPerfOrgWidget.clearValidationBox($("#PerfOrgOptionForm ul.validation-box"));
        ManageDefaultPerfOrgWidget.OptionDialog.Element.dialog({title : "Edit Option"});
    }

    ManageDefaultPerfOrgWidget.SaveRecord = function ()
    {
 
            $('#Save-OptionDialog').addClass('display-none');
            $('#Loader-OptionDialog').removeClass('display-none');

            // Getting values from the model since Adding a new performing org should never save changes to the list name.
            ManageDefaultPerfOrgWidget.data.PerfOrgListName = "<%= Model.PerfOrgListName %>";
            ManageDefaultPerfOrgWidget.data.PerfOrgListID = <%: Model.PerfOrgListID %>;
            ManageDefaultPerfOrgWidget.data.UpdateDateLong = '<%: Model.UpdateDateLong %>';

            ManageDefaultPerfOrgWidget.data.PerfOrg = {};
            ManageDefaultPerfOrgWidget.data.PerfOrg.PerfOrgID = $('#PerfOrgDBID').val();
            ManageDefaultPerfOrgWidget.data.PerfOrg.PerfOrgName = $("#PerfOrgID").val();
            ManageDefaultPerfOrgWidget.data.PerfOrg.PerfOrgDesc = $("#PerfOrgDesc").val();
            ManageDefaultPerfOrgWidget.data.PerfOrg.Deleted = false;
            ManageDefaultPerfOrgWidget.data.PerfOrg.UpdateDateLong = $("#UpdateDateLong").val().toString();

            ManageDefaultPerfOrgWidget.data.DefaultPerfOrgs = new Array();
            ManageDefaultPerfOrgWidget.data.DefaultPerfOrgs.push(ManageDefaultPerfOrgWidget.data.PerfOrg);
           
            var dataToSend = JSON.stringify(ManageDefaultPerfOrgWidget.data);

            ManageDefaultPerfOrgWidget.ajaxRequest({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%:WebConstants.CONTROLLER_ADMIN %>',
                    '<%:WebConstants.ACTION_SAVE_DEFAULT_PERF_ORGS %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                data: dataToSend,
                success: function()
                {
                    ManageDefaultPerfOrgWidget.CloseDialog(ManageDefaultPerfOrgWidget.OptionDialog);
                    ManageDefaultPerfOrgWidget.ReloadGridData();
                    RaiseNotification('Changes Saved');
                    $('#Loader-OptionDialog').addClass('display-none');
                    $('#Save-OptionDialog').removeClass('display-none');
                    $('#Save-OptionDialog').addClass('disabled');
                    $('#AddPerfOrgID').val('');
                    $('#AddPerfOrgDesc').val('');
                    ManageDefaultPerfOrgWidget.cleanDirty(); 
                 },
                error: function() {
                    $('#Loader-OptionDialog').addClass('display-none');
                    $('#Save-OptionDialog').removeClass('display-none');
                }
            });

    }
    ManageDefaultPerfOrgWidget.SaveButtonClick = function () {

            $('#Save-ManageDefaultPerfOrg').addClass('display-none');
            $('#Loader-ManageDefaultPerfOrg').removeClass('display-none');

            ManageDefaultPerfOrgWidget.data.PerfOrgListName = $('#PerfOrgListName').val();
            ManageDefaultPerfOrgWidget.data.PerfOrgListID = <%: Model.PerfOrgListID %>;
            ManageDefaultPerfOrgWidget.data.UpdateDateLong = '<%: Model.UpdateDateLong %>';

            delete ManageDefaultPerfOrgWidget.data.DefaultPerfOrgs;

            dataToSend = JSON.stringify(ManageDefaultPerfOrgWidget.data);

            ManageDefaultPerfOrgWidget.ajaxRequest({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%:WebConstants.CONTROLLER_ADMIN %>',
                    '<%:WebConstants.ACTION_SAVE_DEFAULT_PERF_ORGS %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                data: dataToSend,
                success: ManageDefaultPerfOrgWidget.BackToJumpPage,
                error: function() {
                        $('#Loader-ManageDefaultPerfOrg').addClass('display-none');
                        $('#Save-ManageDefaultPerfOrg').removeClass('display-none');
                }
        } , $('#Save-ManageDefaultPerfOrg'));
    }

    /*-------------------------------  Import/Export Functions --------------------------------*/

    // Remove old dialogs left behind when jumping back to main Admin jump page
    ManageDefaultPerfOrgWidget.RemoveStaleDialogs = function() {
        $('body .ui-dialog').children('#ImportDialog, #ImportInProgressDialog, #OptionDialog').parent().remove();
        $('body').children('#ImportDialog, #ImportInProgressDialog, #OptionDialog').remove();
    }

    // This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
    ManageDefaultPerfOrgWidget.SetDocumentDomains = function () {
        $('#ImportDialog-DocumentDomain').val(document.domain);
    }

    ManageDefaultPerfOrgWidget.SubmitUploadForm = function () {
        if (ManageDefaultPerfOrgWidget.ValidateFileInput())
        {
            ManageDefaultPerfOrgWidget.HideImportDialogError();
            ManageDefaultPerfOrgWidget.OpenDialogAfterInitialize(ManageDefaultPerfOrgWidget.ImportInProgressDialog);
            
            ManageDefaultPerfOrgWidget.AppendIFrameForUploadResponse();
            $("#ImportDialog-Form").submit();
        }
    }

    ManageDefaultPerfOrgWidget.AppendIFrameForUploadResponse = function() {
        // Remove the old hidden iFrame, if it exists
        $('#ImportDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportDialog-Form').append('<iframe id="ImportDialog-UploadTarget" name="ImportDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportDialog-UploadTarget').load(ManageDefaultPerfOrgWidget.StopUpload);

        ManageDefaultPerfOrgWidget.SetDocumentDomains();
    }

    ManageDefaultPerfOrgWidget.StopUpload = function() { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide Dialogs
                ManageDefaultPerfOrgWidget.CloseDialog(ManageDefaultPerfOrgWidget.ImportDialog);
                ManageDefaultPerfOrgWidget.CloseDialog(ManageDefaultPerfOrgWidget.ImportInProgressDialog);

                // Show success notification
                RaiseNotification('Import successful and changes saved');
                
                // Redirect back to jump page
                ManageDefaultPerfOrgWidget.ReloadGridData();
            }
            else {
                $("#ImportDialog-ErrorText").html(results.Message);
                ManageDefaultPerfOrgWidget.ShowImportDialogError();
                ManageDefaultPerfOrgWidget.CloseDialog(ManageDefaultPerfOrgWidget.ImportInProgressDialog);
            }
        }
        else {
            ManageDefaultPerfOrgWidget.CloseDialog(ManageDefaultPerfOrgWidget.ImportInProgressDialog);
        }
    }

    ManageDefaultPerfOrgWidget.ValidateFileInput = function () {
        if ($("#ImportDialog-File").val().length == 0) {
            ManageDefaultPerfOrgWidget.ShowImportDialogFileValidation();
            ManageDefaultPerfOrgWidget.DisableImportDialogImportButton();
            return false;
        }
        else {
            ManageDefaultPerfOrgWidget.HideImportDialogFileValidation();
            ManageDefaultPerfOrgWidget.EnableImportDialogImportButton();
            return true;
        }
    }

    ManageDefaultPerfOrgWidget.EnableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").removeClass('display-none');
    }

    ManageDefaultPerfOrgWidget.DisableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").addClass('display-none');
    }

    ManageDefaultPerfOrgWidget.ShowImportDialogError = function () {
        $("#ImportDialog-Error").slideDown("slow");
    }

    ManageDefaultPerfOrgWidget.HideImportDialogError = function () {
        $("#ImportDialog-Error").slideUp("slow");
    }

    ManageDefaultPerfOrgWidget.ShowImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideDown("slow");
    }

    ManageDefaultPerfOrgWidget.HideImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideUp("slow");
    }
            
     ManageDefaultPerfOrgWidget.Export = function() {
        // Remove the old hidden iFrame, if it exists
        $('#ManageDefaultPerfOrgGrid-DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'ManageDefaultPerfOrgGrid-DownloadTarget',
            'class': 'display-none',
            'src': CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_EXPORT_DEFAULT_PERFORMING_ORGS %>')
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    }


    /*-------------------------------  End Import/Export Functions --------------------------------*/
           
    $(function () {
        ManageDefaultPerfOrgWidget.afterDOMLoad();
        ManageDefaultPerfOrgWidget.RemoveStaleDialogs();    
        ManageDefaultPerfOrgWidget.ValidateFileInput();
        ManageDefaultPerfOrgWidget.InitializeDialog(ManageDefaultPerfOrgWidget.ImportDialog);
        ManageDefaultPerfOrgWidget.InitializeDialog(ManageDefaultPerfOrgWidget.OptionDialog);
        ManageDefaultPerfOrgWidget.InitializeDialog(ManageDefaultPerfOrgWidget.ImportInProgressDialog);
        ManageDefaultPerfOrgWidget.BindEvents();

        ManageDefaultPerfOrgWidget.registerForEvent('PERF_ORGS_LOADED', function() { 
             return true;
        });

    });
    
</script>

<div id="ManageDefaultPerformingOrganizations" class="manage-default-performing-organizations section">
    <div class="title">Manage Default Performing Organizations</div>
    <div class="data">
        <div>
            Add or edit the default list of Performing Organizations that can be used by Workspaces. Updates will immediately be used
            for new Workspaces. Other incomplete Workspaces will be notified of the updates, and will be given the option
            to retrieve the updates.
        </div>
        <br />
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ManageDefaultPerfOrgForm", name="ManageDefaultPerfOrgForm" })) { %>
            <ul class="validation-box"></ul>
            <div class="form-row">
                <div class="form-label">
                    <span>List Name</span>
                    <div id="PerfOrgListName-Help" class="help-icon" onclick="ManageDefaultPerfOrgWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="PerfOrgListName-HelpDialog" class="help-dialog" style="width: 200px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Used to identify the default list of Performing Organizations seen by all Workspaces.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <%: Html.TextBoxFor(model => model.PerfOrgListName, new { onkeyup = "ManageDefaultPerfOrgDataWidget.EnableSave()", @class = "half", @maxlength = "50" })%>
                    <div class ="buttons inline">
                        <button id="Save-ManageDefaultPerfOrg" class="ies-action disabled" name="save-button" type="button">Save</button>
                        <div id="Loader-ManageDefaultPerfOrg" class="loader display-none"></div>
                    </div>
                </div>
            </div>
        <% } %>
        <div class="form-row">
            <div class="form-label">
                <span>Options</span>
                <div id="ManageDefaultPerfOrgGrid-Options-Help" class="help-icon" onclick="ManageDefaultPerfOrgWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="ManageDefaultPerfOrgGrid-Options-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        Options: Options that users will be able to select if list is used by Workspace. 
                        &nbsp; &nbsp;
                        A new list of options can be imported by clicking on the <i>Import</i>. File must contain the same headers as this <a href="<%= this.ResolveClientUrl("~/Templates/Export/PerformingOrgs.xlsx") %>"
                            target="_blank">format example</a> prior to importing.
                            &nbsp; &nbsp;
                            Options can be exported to an Excel file. File can be used to make updates and imported or share with another Workspace.
                    </div>
                </div>
            </div>
            <div class="form-element">
                <div class="buttons inline">
                    <button id="ManageDefaultPerfOrgGrid-DeleteButton" class="ies-action disabled" name="delete-button" type="button">Delete</button>
                    <div id="ManageDefaultPerfOrgGrid-DeleteLoader" class="loader display-none"></div>
                    <button id="ManageDefaultPerfOrgGrid-AddButton" class="ies-action" type="button">+ Add</button> 
                    <button id="ManageDefaultPerfOrgGrid-ImportButton" class="ies-action" name="import-button" type="button">Import</button>
                    <button id="ManageDefaultPerfOrgGrid-ExportButton" class="ies-action" type="button">Export</button>
                </div>
                <div class="search-box float-right">
                    <%: Html.TextBox("SearchText") %>
                    <div id="ManageDefaultPerfOrgGrid-SearchMagnifyButton" class="button search-magnify-glass-button"></div>
                </div>
            </div>
        </div>
            
        <div class="form-row">
            <div class="form-label">
            </div>
            <div id="PerfOrgGridContent" class="form-element">
                <% Html.RenderAction(WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_PERF_ORGS_GRID, WebConstants.CONTROLLER_ADMIN); %>
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
    <% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new { 
                                controller = WebConstants.CONTROLLER_ADMIN, 
                                action = WebConstants.ACTION_IMPORT_DEFAULT_PERFORMING_ORGS, 
                                workspace = SiteMasterUtilities.GetCurrentWorkspace()}, 
                                FormMethod.Post, 
                                new { enctype = "multipart/form-data", id = "ImportDialog-Form", target = "ImportDialog-UploadTarget" }); %>
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
                <input type="hidden" id="ImportDialog-PerformingOrgListID" name="PerformingOrgListID" value="<%:Model.PerfOrgListID %>" />
                <input type="hidden" id="ImportDialog-PerformingOrgListUpdateDateLong" name="PerformingOrgListUpdateDateLong" value="<%:Model.UpdateDateLong %>" />
                <input type="file" size="60" id="ImportDialog-File" name="file" onchange="ManageDefaultPerfOrgWidget.ValidateFileInput();" />
                <div id="ImportDialog-FileValidation" style="color: #990000; display: none;">
                    Please select a file to import</div>
                <ul style="padding: 0px 0px 0px 15px; margin: 10px 0px 0px 0px;">
                    <li>File must contain the same headers as <a href="<%= this.ResolveClientUrl("~/Templates/Export/PerformingOrgs.xlsx") %>">
                        this format example</a> prior to importing and be in .xlsx format.</li>
                    <li>All IDs must be unique</li>
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

<div id="OptionDialog" style="display: none;">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "PerfOrgOptionForm" })) { %>
    <ul class="validation-box"></ul>
    <div class="form-row">
        <div class="form-label">ID*</div>
        <div class="form-element">
            <input type="text" id="PerfOrgID" name="PerfOrgName" maxlength="20"/>
            <input type="hidden" id="PerformingOrgListID" name="PerformingOrgListID" value="<%: Model.PerfOrgListID %>" />
            <input type="hidden" id="UpdateDateLong" name="UpdateDateLong" />
            <input type="hidden" id="PerfOrgDBID" name="PerformingOrgID" />
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Description*</div>
        <div class="form-element">
            <input type="text" id="PerfOrgDesc" name="PerfOrgDesc" maxlength="50"/>
        </div>
    </div>
    <div class="form-label"></div>
    <div class="form-element">
        <div class="buttons dialogbuttons"  style="width: 200px;">
            <button id="Save-OptionDialog" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="Loader-OptionDialog" class="loader display-none"></div>
            <button id="ManageDefaultPerfOrg-Cancel-OptionDialog" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
    </div>
    <% } %>
</div>
