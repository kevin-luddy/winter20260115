<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.WorkspaceVersionModelView>" %>

<script type="text/javascript">
    ManageBackupVersionsWidget = new Widget("ManageBackupVersionsContainer", '<%= ViewData["READONLY"] %>'.isTrue());
 
    ManageBackupVersionsWidget.ReloadGridData = function() {
       $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                '<%:WebConstants.ACTION_DISPLAY_BACKUP_VERSIONS_GRID %>', ''),
            success: function(response)
            {            
                $('#BackupVersionsGridContent').html(response);
                refreshModule($(ManageBackupVersionsWidget.Module));              
            }
        });
    }

    ManageBackupVersionsWidget.SaveNewVersion = function () {
        $('#versionErrorMessage').text('').hide();
        var versionName = $("div#NewVersionDialog input[name=VersionName]").val();

        if (versionName != null && versionName != 'undefined' && versionName.replace(/^\s+|\s+$/g, "") != "") {
            $('#SaveVersionSpinner').show();
            $('#SaveVersionButton').hide();
            var data = JSON.stringify({ "versionName": versionName});
            ManageBackupVersionsWidget.ajaxRequest({
                type: 'POST',
                data: data,
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.ACTION_SAVE_WORKSPACE_VERSION %>', ''),
                success: function (response) {
                    ManageBackupVersionsWidget.CloseDialog(ManageBackupVersionsWidget.NewVersionDialog);
                    $('#SaveVersionSpinner').hide();
                    $('#SaveVersionButton').show();
                    RaiseNotification("Version successfully created");
                    location.reload();
                },
                error: function (errorDetails) {
                    $('#versionErrorMessage').text($.parseJSON(errorDetails.responseText).Message).show();
                    ManageBackupVersionsWidget.processValidationErrors([]);  // clear existing validation errors

                    $('#NewVersionDialog-Save').removeClass('display-none');
                    $('#SaveVersionSpinner').hide();
                    $('#SaveVersionButton').show();
                }
            });

        }

    };

    ManageBackupVersionsWidget.ConfirmRestore = function (versionID) {
        var versionRow = $("table#ManageBackupVersionsGridTable tr[pkid=" + versionID + "]");
        var text = "Restoring version: " + versionRow.attr("name") + "<br/>" +
            "The status of all BOEs will be set to Draft <br/>" +
            "The status of the Workspace will be set to " + versionRow.data('restore-version') + "<br/>" +
            "You cannot undo the restore operation. <br/>" +
            "Are you sure you want to restore this version?";
        var title = 'Restoring a version of the Workspace';
        Session.confirmDialog(title, text, function () { ManageBackupVersionsWidget.RestoreConfirmed(versionID); }, null);
    };

    ManageBackupVersionsWidget.RestoreConfirmed = function (versionID) {
        $('#PageLoading').removeClass('display-none');
        $.ajax({
            type: 'POST',
            data: { "VersionID": versionID },
            dataType: 'json',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.ACTION_RESTORE_WORKSPACE_VERSION %>', ''),
            success: function (response) {
                // check both empty and null 
                ManageBackupVersionsWidget.restoreResponse = response;
                if (response == null || response == "" || response.Status == true) {
                    RaiseNotification("Version successfully restored");
                    ManageBackupVersionsWidget.RefreshUrl();
                }
                else {
                    // If WARNING is returned, the restore was still successful but we want to display the warnings 
                    if (response.Errors !== undefined && response.Errors.substring(7, 0) == "WARNING") {
                        var text = "Version successfully restored." + "<br/><br/>" + "The following warnings occurred because the data no longer exists. Valid values will need to be entered. <br/>";
                        Session.alertDialog("Version successfully restored with Warnings", text + response.Errors, ManageBackupVersionsWidget.RefreshUrl);
                    }
                    else {
                        Session.alertDialog("Problems performing your restore", response.Errors, ManageBackupVersionsWidget.RefreshUrl);
                    }
                }
                $('#PageLoading').addClass('display-none');


            },
            error: function () {
                $('#PageLoading').addClass('display-none');
                RaiseNotification("Restore Fail");
            }
        });
    };

    ManageBackupVersionsWidget.RefreshUrl = function () {
        if (ManageBackupVersionsWidget.restoreResponse != undefined && ManageBackupVersionsWidget.restoreResponse.Shortname != null && ManageBackupVersionsWidget.restoreResponse.Shortname !== undefined) {
            var refreshUrl = CreatePostURL(ManageBackupVersionsWidget.restoreResponse.Shortname, '<%: WebConstants.CONTROLLER_WORKSPACE %>', '<%: WebConstants.VIEW_WORKSPACE_SETTINGS%>' + '#BackupVersions' );
                window.location = refreshUrl;
        }
    };

    ManageBackupVersionsWidget.enableDelete = function(){
        if($("div#ManageBackupVersionsContainer input[name=DeleteVersion]:checked").length>0){
            $("div#ManageBackupVersionsContainer button[name='delete-button']").removeClass("disabled");
        }else{
            $("div#ManageBackupVersionsContainer button[name='delete-button']").addClass("disabled");
        }
    };

    ManageBackupVersionsWidget.DeleteRecords = function(event, data) {
        $('#Delete-ManageBackupVersionsWidget').addClass('display-none');
        $('#DeleteLoader-ManageBackupVersionsWidget').removeClass('display-none');
        Session.confirmDialog("Delete Version of Workspace?",
            "Are you sure you want to delete the selected version(s)?", function(){ManageBackupVersionsWidget.DeleteConfirmed();});
    }

    ManageBackupVersionsWidget.DeleteConfirmed = function() {
        var dataToSend = [];

        $("div#ManageBackupVersionsContainer input[name=DeleteVersion]:checked").each(function(){
            var versionToDelete = {};
            var parentRow = $(this).parents('tr');
            versionToDelete.VersionID = parentRow.attr('pkid');
            //versionToDelete.VersionName = parentRow.find("td[name=versionName]").html();
            dataToSend.push(versionToDelete);
        });
        
        $.ajax({
            type: 'POST',
            dataType: 'json',
            data:JSON.stringify(dataToSend),
            contentType: 'application/json; charset=utf-8',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.ACTION_DELETE_WORKSPACE_VERSIONS %>', ''),
            success: function(response) {
                $('#ManageBackupVersions-Delete').addClass('disabled');
                RaiseNotification("Version(s) successfully deleted");
                location.reload();
            },
            error: function() { 
                ManageBackupVersionsWidget.CloseDialog(ManageBackupVersionsWidget.RestoreResultsDialog); 
            }
        });
    };
        
    ManageBackupVersionsWidget.AddRecord = function() {
        $("div#NewVersionDialog input[name=VersionName]").val("");
        $('#versionErrorMessage').hide();
        ManageBackupVersionsWidget.OpenDialogAfterInitialize(ManageBackupVersionsWidget.NewVersionDialog);
        $("#NewVersionDialog button[name='save-button']").addClass("disabled");
    };

    $(function () {
        ManageBackupVersionsWidget.NewVersionDialog = {};
        ManageBackupVersionsWidget.NewVersionDialog.Element = $('#NewVersionDialog');
        ManageBackupVersionsWidget.NewVersionDialog.Params = { width: 450, modal: true, resizable: false, draggable: true, 
            title: "Create New Version of Workspace" };
        ManageBackupVersionsWidget.Module = $('div#ManageBackupVersionsContainer');

        ManageBackupVersionsWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { ManageBackupVersionsWidget.cleanDirty(); });
        
        ManageBackupVersionsWidget.InitializeDialog(ManageBackupVersionsWidget.NewVersionDialog);

        $("#ManageBackupVersionsContainer button[name='add-button']").click(ManageBackupVersionsWidget.AddRecord);

        ManageBackupVersionsWidget.registerForLiveEvent('click',
            "#ManageBackupVersionsContainer button[name='delete-button']:not(.disabled)", ManageBackupVersionsWidget.DeleteRecords);

        // activate
        $('#NewVersionDialog input[name=VersionName]').keyup(function () {
            var versionName = $("div#NewVersionDialog input[name=VersionName]").val();

            if (versionName != null && versionName != 'undefined'
                    && versionName.replace(/^\s+|\s+$/g, "") != "") {
                $("#NewVersionDialog button[name='save-button']").removeClass("disabled");
            } else {
                $("#NewVersionDialog button[name='save-button']").addClass("disabled");
            }

        });


        $("#NewVersionDialog button[name='save-button']").click(function () {
            ManageBackupVersionsWidget.SaveNewVersion();
        });

        $("#NewVersionDialog button[name='cancel-button']").click(function () {
            ManageBackupVersionsWidget.CloseDialog(ManageBackupVersionsWidget.NewVersionDialog);
        });

        ManageBackupVersionsWidget.registerForEvent("CUSTOM_FIELD_RESOURCE_LOADED", function () {
            //Widget.applyValidation();
            return true;
        });

        ManageBackupVersionsWidget.ContainsOCI = <%= ViewData["ContainsOCI"] %>;

        if (ManageBackupVersionsWidget.ContainsOCI == true){
            $('#OCINote').html('<b>Note:</b> Must not contain any classified, export controlled or third party proprietary information.');
        }
        else {
            $('#OCINote').html('<b>Note:</b> Must not contain any OCI, classified, export controlled or third party proprietary information.');
        }

        createModule(ManageBackupVersionsWidget.Module);
        refreshModule(ManageBackupVersionsWidget.Module);

        ManageBackupVersionsWidget.ReloadGridData();
    });
</script>

<div id="ManageBackupVersionsContainer" class="module">
    <div class="module-header-data">Manage Backup Versions</div>

    <div class="module-content-data">
        <ul class="validation-box"> </ul>
            <div class="form-row">
                Create a new version from the current Workspace, restore the Workspace from a previous version, or delete a version that is no longer needed.
            </div>
            <div style="width: 300px;" class="buttons">
                <button id="ManageBackupVersions-Delete" class="ies-action disabled" name="delete-button" type="button">Delete</button>
                <button id="ManageBackupVersions-Add" name="add-button" class="ies-action" type="button">+ Add</button>
            </div>
            <div id="BackupVersionsGridContent"></div>
    </div>
</div>

<div id="NewVersionDialog" class="" style="display: none;">
    <div class="container">
        <div class="form-row">
            <div id="versionErrorMessage" class="validation-box"></div>
            <div class="form-label">Version Name*</div>
             <div class="form-element">
                 <input type="text" name="VersionName" size="50" maxlength="50" />
            </div>
        </div>
        <div id="OCINote" class="oci-note"></div>
        <div style="width:200px">
            <button id="SaveVersionButton" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="SaveVersionSpinner" class="loader" style="display:none"></div>
            <button id="CancelVersionButton" class="ies" name="cancel-button" type="button">Cancel</button>
         </div>
    </div>
</div>