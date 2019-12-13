<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.DefaultResourcesModelView>" %>
<script type="text/javascript">
    var DefaultResourcesWidget = new Widget('OptionDialog', <%: ViewData["READONLY"] %>);

    DefaultResourcesWidget.ImportDialog = {};
    DefaultResourcesWidget.ImportDialog.Element = $('#ImportDialog');
    DefaultResourcesWidget.ImportDialog.Params = { width: 750, modal: true, resizable: false, draggable: true, title: "Import Resources" };
    DefaultResourcesWidget.ImportInProgressDialog = {};
    DefaultResourcesWidget.ImportInProgressDialog.Element = $("#ImportInProgressDialog");
    DefaultResourcesWidget.ImportInProgressDialog.Params = { width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" };
    DefaultResourcesWidget.OptionDialog = {};
    DefaultResourcesWidget.OptionDialog.Element = $("#OptionDialog");
    DefaultResourcesWidget.OptionDialog.Params = { width: 350, modal: true, resizable: false, draggable: true, close: function(){} };

    DefaultResourcesWidget.BindEvents = function() {
        // Bind events for single UI elements
        $('#ImportButton-DefaultResources').click(function() {
            $('#ImportDialog-Error').attr("style","none");

            $('#ElementsOfCostImport-ISGSLabor').prop('checked', $('#ElementsOfCost-ISGSLabor').prop('checked'));
            $('#ElementsOfCostImport-IWTA').prop('checked', $('#ElementsOfCost-IWTA').prop('checked'));
            $('#ElementsOfCostImport-Sub').prop('checked', $('#ElementsOfCost-Sub').prop('checked'));
            $('#ElementsOfCostImport-ODC').prop('checked', $('#ElementsOfCost-ODC').prop('checked'));
            $('#ElementsOfCostImport-Travel').prop('checked', $('#ElementsOfCost-Travel').prop('checked'));
            $('#ElementsOfCostImport-Materials').prop('checked', $('#ElementsOfCost-Materials').prop('checked'));

            DefaultResourcesWidget.OpenDialogAfterInitialize(DefaultResourcesWidget.ImportDialog); 
        });
        $('#ExportButton-DefaultResources').click(DefaultResourcesWidget.Export);

        $('#ImportDialog-ImportButton').click(DefaultResourcesWidget.SubmitUploadForm);

        $('#ImportDialog-CancelButton').click(function() { 
            DefaultResourcesWidget.CloseDialog(DefaultResourcesWidget.ImportDialog); 
            $('#ImportDialog-Error').attr("style","none");
            DefaultResourcesWidget.cleanDirty();
        });

        $('#SearchMagnifyButton-DefaultResources').click(DefaultResourcesWidget.Search);

        $('#AddButton-DefaultResources').click(DefaultResourcesWidget.AddRecord);
        
        DefaultResourcesWidget.registerForDelegateEvent('click','#Save-OptionDialog:not(.disabled)', DefaultResourcesWidget.SaveRecord);

        $('#DefaultResources-Cancel-OptionDialog').click(function() { 
            DefaultResourcesWidget.CloseDialog(DefaultResourcesWidget.OptionDialog); 
            DefaultResourcesWidget.cleanDirty();
        });

        DefaultResourcesWidget.registerForLiveEvent('click', "#Save-DefaultResources:not(.disabled)", DefaultResourcesWidget.SaveResourceListName);
        DefaultResourcesWidget.registerForLiveEvent('click', '#DeleteButton-DefaultResources:not(.disabled)', function() { $(document).trigger('GET_DELETED_RESOURCES'); });

        DefaultResourcesWidget.registerForEvent('PAGE_RESOURCES', DefaultResourcesWidget.PageResources);
        DefaultResourcesWidget.registerForEvent('RESOURCE_CHECKED', function() { 
            if($("#ManageDefaultResourcesGrid input:checkbox:checked").length === 0)
            {
                $('#DeleteButton-DefaultResources').addClass('disabled');
            }
            else
            {$('#DeleteButton-DefaultResources').removeClass('disabled'); }

        });
        DefaultResourcesWidget.registerForEvent('DELETE_RESOURCES', function(event, data) {
            
            var deleteConfirmTitle = "Delete Default Resource Confirmation";
            var deleteConfirmMessage = "Are you sure you want to delete these Resources?";
            
            Session.confirmDialog(deleteConfirmTitle, deleteConfirmMessage, function(){DefaultResourcesWidget.DeleteRecords(event, data);});
        });
        DefaultResourcesWidget.registerForEvent('EDIT_RESOURCE', DefaultResourcesWidget.EditRecord);
        DefaultResourcesWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { DefaultResourcesWidget.cleanDirty(); });

        $('#ExportTemplate-DefaultResource').click(DefaultResourcesWidget.ExportTemplate);
    };

    DefaultResourcesWidget.BackToJumpPage = function() {
        DefaultResourcesWidget.cleanDirty();

        window.location.hash = '#';
    };

    DefaultResourcesWidget.ReloadGridData = function() {
        $('#PageLoading').removeClass('display-none');
        var data = {};
        data.searchText = $("#SearchText").val();
        data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
        data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
        data.showSub = $('#ElementsOfCost-Sub').prop('checked');
        data.showODC = $('#ElementsOfCost-ODC').prop('checked');
        data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
        data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_RESOURCES_GRID %>', ''),
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            dataType: 'html',
            success: function(response)
            {             
                $('#DefaultResourcesGridContent').html(response);
                $('#DeleteLoader-DefaultResources').addClass('display-none');
                $('#DeleteButton-DefaultResources').removeClass('display-none');
                $('#DeleteButton-DefaultResources').addClass('disabled');
                $('#PageLoading').addClass('display-none');
            },
            error: function(response)
            {
                $('#PageLoading').addClass('display-none');
            }
        });
    }

    DefaultResourcesWidget.DeleteRecords = function (event, data) {
        $('#DeleteButton-DefaultResources').addClass('display-none');
        $('#DeleteLoader-DefaultResources').removeClass('display-none');

        DefaultResourcesWidget.SaveResources(data.CheckedItems);
    };

    DefaultResourcesWidget.AddRecord = function() {
        $('#OptionDialog input[name="ID"]').val('').prop('disabled', false);
        $('#OptionDialog input[name="UpdateDateLong"]').val('0');
        $('#OptionDialog input[name="ResourceID"]').val(-1);
        $('#OptionDialog input[name="Description"]').val('');
        $('#OptionDialog input[name="SegmentRegion"]').val('');
        $('#OptionDialog input[name="LaborType"]').val('');
        $('#OptionDialog input[name="RateType"]').val('');/**/
        $('#OptionDialog input[name="ElementOfCostId"]').val(0);
        $('#OptionDialog select').val('');
        $('#Save-OptionDialog').addClass('disabled');
        $('#OptionDialog select[name="RateTypeID"]').prop("disabled", false);/**/
        $('#OptionDialog select[name="ElementOfCostId"]').prop("disabled", false);
        DefaultResourcesWidget.OpenDialogAfterInitialize(DefaultResourcesWidget.OptionDialog);
        DefaultResourcesWidget.OptionDialog.Element.dialog({title : "Add Resource"});
        DefaultResourcesWidget.clearValidationBox($("#ResourceOptionForm ul.validation-box"));
    }

    DefaultResourcesWidget.EditRecord = function(event, data) {
        $('#Save-OptionDialog').addClass('disabled');
        $('#OptionDialog input[name="ID"]').val(data.ID);
        $('#OptionDialog input[name="UpdateDateLong"]').val(data.UpdateDateLong.toString());
        $('#OptionDialog input[name="ResourceID"]').val(data.ResourceID);
        $('#OptionDialog input[name="Description"]').val(data.Description);
        $('#OptionDialog input[name="SegmentRegion"]').val(data.SegmentRegion);
        $('#OptionDialog input[name="LaborType"]').val(data.LaborType);
        $('#OptionDialog select[name="RateTypeID"]').val(data.RateTypeID);/**/
        $('#OptionDialog select[name="ElementOfCostId"]').val(data.ElementOfCostId);


        DefaultResourcesWidget.OpenDialogAfterInitialize(DefaultResourcesWidget.OptionDialog);
        DefaultResourcesWidget.OptionDialog.Element.dialog({title : "Edit Resource"});
        DefaultResourcesWidget.clearValidationBox($("#ResourceOptionForm ul.validation-box"));

        if (data.InUse == 'True')
        {
            $('#OptionDialog select[name="ElementOfCostId"]').prop('disabled', true);
            $('#OptionDialog select[name="RateTypeID"]').prop('disabled', true);/**/
            $('#OptionDialog input[name="ID"]').prop('disabled', true);
        }
        else
        {
            $('#OptionDialog select[name="ElementOfCostId"]').prop('disabled', false);
            $('#OptionDialog select[name="RateTypeID"]').prop('disabled', false);/**/
            $('#OptionDialog input[name="ID"]').prop('disabled', false);
        }
    }

    DefaultResourcesWidget.SaveRecord = function() {
        var data = new Array();
        var option = {};
        option.ID = $('#OptionDialog input[name="ID"]').val();
        option.UpdateDateLong = $('#OptionDialog input[name="UpdateDateLong"]').val().toString();
        option.ResourceID = $('#OptionDialog input[name="ResourceID"]').val();
        option.Description = $('#OptionDialog input[name="Description"]').val();
        option.SegmentRegion = $('#OptionDialog input[name="SegmentRegion"]').val();
        option.LaborType = $('#OptionDialog input[name="LaborType"]').val();
        option.RateTypeID = $('#OptionDialog select[name="RateTypeID"]').val();/**/
        option.ElementOfCostId = $('#OptionDialog select[name="ElementOfCostId"]').val();

        data.push(option);

        DefaultResourcesWidget.SaveResources(data);

    }

    DefaultResourcesWidget.SaveResources = function(data) {
        if (data.length > 0) {            
            DefaultResourcesWidget._SaveResources(data);
        }
        else {
            $('#DeleteButton-DefaultResources').addClass('disabled');
            $('#DeleteButton-DefaultResources').removeClass('display-none');
            $('#DeleteLoader-DefaultResources').addClass('display-none');
        }      
    }
    
    DefaultResourcesWidget._SaveResources = function(data) {
        if (data.length > 0) {
            $('#Loader-OptionDialog').removeClass('display-none');
            $('#Save-OptionDialog').addClass('display-none');
            var dataToSend = {};
            dataToSend.resources = data;

            dataToSend = JSON.stringify(dataToSend);

            DefaultResourcesWidget.ajaxRequest({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_SAVE_DEFAULT_RESOURCES %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                data: dataToSend,
                success: function(response) {
                    $('#Loader-OptionDialog').addClass('display-none');
                    $('#Save-OptionDialog').removeClass('display-none');
                    $('#Save-OptionDialog').addClass('disabled');
                    DefaultResourcesWidget.cleanDirty();
                    DefaultResourcesWidget.CloseDialog(DefaultResourcesWidget.OptionDialog);
                    DefaultResourcesWidget.ReloadGridData();
                    RaiseNotification("Changes saved");
                },
                error: function(response) {
                    $('#Loader-OptionDialog').addClass('display-none');
                    $('#Save-OptionDialog').removeClass('display-none');
                    $('#DeleteButton-DefaultResources').removeClass('display-none');
                    $('#DeleteLoader-DefaultResources').addClass('display-none');
                }
            }, $('#Save-OptionDialog'));
        }
        else {
            $('#DeleteButton-DefaultResources').addClass('disabled');
            $('#DeleteButton-DefaultResources').removeClass('display-none');
            $('#DeleteLoader-DefaultResources').addClass('display-none');
        }
    }

    DefaultResourcesWidget.SaveResourceListName = function() {
        $('#Save-DefaultResources').addClass('display-none');
        $('#Loader-DefaultResources').removeClass('display-none');

        DefaultResourcesWidget.data.ListName = $('#ListName').val();
        DefaultResourcesWidget.data.ListID = <%: Model.ListID %>;
        DefaultResourcesWidget.data.UpdateDateLong = '<%: Model.UpdateDateLong %>';

        delete DefaultResourcesWidget.data.DefaultResources;

        dataToSend = JSON.stringify(DefaultResourcesWidget.data);

        DefaultResourcesWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                    '<%:WebConstants.ACTION_SAVE_RESOURCE_LIST %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: DefaultResourcesWidget.BackToJumpPage,
            error: function() {
                $('#Loader-DefaultResources').addClass('display-none');
                $('#Save-DefaultResources').removeClass('display-none');
            }
        }, $('#Save-DefaultResources'));
    }

    DefaultResourcesWidget.Search = function() {
        $('#PageLoading').removeClass('display-none');

        var data = {};
        data.searchText = $("#SearchText").val();
        data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
        data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
        data.showSub = $('#ElementsOfCost-Sub').prop('checked');
        data.showODC = $('#ElementsOfCost-ODC').prop('checked');
        data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
        data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

        var dataToSend = JSON.stringify(data);
           
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_SEARCH_RESOURCES %>', ''),
                contentType: 'application/json; charset=utf-8',
                data: dataToSend,
                dataType: 'html',
                success: function(response) {
                    $('#DefaultResourcesGridContent').html(response);
                    $('#DeleteLoader-DefaultResources').addClass('display-none');
                    $('#DeleteButton-DefaultResources').removeClass('display-none');
                    $('#DeleteButton-DefaultResources').addClass('disabled');
                    $('#PageLoading').addClass('display-none');
                },
                error: function (response) {
                    $('#PageLoading').addClass('display-none');
                }
            });
        }

        DefaultResourcesWidget.PageResources = function(event, data) {
            var dataToSend = JSON.stringify(data);

            $.ajax({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_PAGE_RESOURCES %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function(response)
            {       
                $('#DefaultResourcesGridContent').html(response);
                $('#DeleteLoader-DefaultResources').addClass('display-none');
                $('#DeleteButton-DefaultResources').removeClass('display-none');
                $('#DeleteButton-DefaultResources').addClass('disabled');
            }
        });
    }

    /*-------------------------------  Import/Export Functions --------------------------------*/

    // Remove old dialogs left behind when jumping back to main Admin jump page
    DefaultResourcesWidget.RemoveStaleDialogs = function() {
        $('body .ui-dialog').children('#ImportDialog, #ImportInProgressDialog, #OptionDialog').parent().remove();
        $('body').children('#ImportDialog, #ImportInProgressDialog, #OptionDialog').remove();
    }

    // This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
    DefaultResourcesWidget.SetDocumentDomains = function () {
        $('#ImportDialog-DocumentDomain').val(document.domain);
    }

    DefaultResourcesWidget.SubmitUploadForm = function () {
        if (DefaultResourcesWidget.ValidateFileInput())
        {
            DefaultResourcesWidget.HideImportDialogError();
            DefaultResourcesWidget.OpenDialogAfterInitialize(DefaultResourcesWidget.ImportInProgressDialog);
            
            DefaultResourcesWidget.AppendIFrameForUploadResponse();
            $("#ImportDialog-Form").submit();
        }
    }

    DefaultResourcesWidget.AppendIFrameForUploadResponse = function() {
        // Remove the old hidden iFrame, if it exists
        $('#ImportDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportDialog-Form').append('<iframe id="ImportDialog-UploadTarget" name="ImportDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportDialog-UploadTarget').load(DefaultResourcesWidget.StopUpload);

        DefaultResourcesWidget.SetDocumentDomains();
    }

    DefaultResourcesWidget.StopUpload = function() { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide dialogs
                DefaultResourcesWidget.CloseDialog(DefaultResourcesWidget.ImportDialog);
                DefaultResourcesWidget.CloseDialog(DefaultResourcesWidget.ImportInProgressDialog);

                // Show success notification
                RaiseNotification('Import successful and changes saved');
                
                // Redirect back to jump page
                DefaultResourcesWidget.ReloadGridData();
            }
            else { 
                $("#ImportDialog-ErrorText").html(results.Message.replace(/(\r\n|\n|\r)/gm, '<br/>'));
                DefaultResourcesWidget.ShowImportDialogError();
                DefaultResourcesWidget.CloseDialog(DefaultResourcesWidget.ImportInProgressDialog);
            }
        }
        else {
            DefaultResourcesWidget.CloseDialog(DefaultResourcesWidget.ImportInProgressDialog);
            $("#ImportDialog-Error").attr("style","none");
            DefaultResourcesWidget.ReloadGridData();
        }
    }

    DefaultResourcesWidget.ValidateFileInput = function () {
  
        if ($("#ImportDialog-File").val().length == 0) {
            DefaultResourcesWidget.ShowImportDialogFileValidation();
            DefaultResourcesWidget.DisableImportDialogImportButton();
            return false;
        }
        else {
            DefaultResourcesWidget.HideImportDialogFileValidation();
            DefaultResourcesWidget.EnableImportDialogImportButton();
            return true;
        }
    }

    DefaultResourcesWidget.EnableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").removeClass('display-none');
    }

    DefaultResourcesWidget.DisableImportDialogImportButton = function () {
        $("#ImportDialog-ImportButton").addClass('display-none');
    }

    DefaultResourcesWidget.ShowImportDialogError = function () {
        $("#ImportDialog-Error").slideDown("slow");
        $('#ImportDialog-Error').show();
        
    }

    DefaultResourcesWidget.HideImportDialogError = function () {
        $("#ImportDialog-Error").slideUp("slow");
    }

    DefaultResourcesWidget.ShowImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideDown("slow");
    }

    DefaultResourcesWidget.HideImportDialogFileValidation = function () {
        $("#ImportDialog-FileValidation").slideUp("slow");
    }

    DefaultResourcesWidget.Export = function() {
        // Remove the old hidden iFrame, if it exists
        $('#DefaultResources-DownloadTarget').remove();

        var data = {};
        data.searchText = $("#SearchText").val();
        data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
        data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
        data.showSub = $('#ElementsOfCost-Sub').prop('checked');
        data.showODC = $('#ElementsOfCost-ODC').prop('checked');
        data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
        data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

        var dataToSend = JSON.stringify(data);

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var parms = data.showLabor + '/' +
                    data.showIWTA + '/' +
                    data.showSub + '/' +
                    data.showODC + '/' +
                    data.showTravel + '/' +
                    data.showMaterials +
                    ((data.searchText != '') ? ('/' + data.searchText) : '');

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DefaultResources-DownloadTarget',
            'class': 'display-none',
            'src': CreateSystemAdminWithParmsPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_EXPORT_DEFAULT_RESOURCES %>',
                    parms)
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    }

    DefaultResourcesWidget.ExportTemplate = function() {
         
        // Remove the old hidden iFrame, if it exists
        $('#DefaultResources-DownloadTarget').remove();

        var data = {};
        data.searchText = $("#SearchText").val();
        data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
        data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
        data.showSub = $('#ElementsOfCost-Sub').prop('checked');
        data.showODC = $('#ElementsOfCost-ODC').prop('checked');
        data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
        data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

        var dataToSend = JSON.stringify(data);

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var parms =  data.showLabor + '/' +
                    data.showIWTA + '/' +
                    data.showSub + '/' +
                    data.showODC + '/' +
                    data.showTravel + '/' +
                    data.showMaterials +
                    ((data.searchText != '') ? ('/' + data.searchText) : '');

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DefaultResources-DownloadTarget',
            'class': 'display-none',
            'src': CreateSystemAdminWithParmsPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_EXPORT_DEFAULT_RESOURCES_TEMPLATE %>',
                    parms)
        });

  
        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');

    }
    /*-------------------------------  End Import/Export Functions --------------------------------*/

    $(function() {
        DefaultResourcesWidget.afterDOMLoad();
        $('input[name="ElementsOfCost"]').change(DefaultResourcesWidget.ReloadGridData);

        DefaultResourcesWidget.RemoveStaleDialogs();
        DefaultResourcesWidget.ValidateFileInput();
        DefaultResourcesWidget.InitializeDialog(DefaultResourcesWidget.ImportDialog);
        DefaultResourcesWidget.InitializeDialog(DefaultResourcesWidget.OptionDialog);
        DefaultResourcesWidget.InitializeDialog(DefaultResourcesWidget.ImportInProgressDialog);
        DefaultResourcesWidget.BindEvents();
    });
</script>

<div id="DefaultResources" class="manage-default-resources section">
    <div class="title">Manage Default Resources</div>
    <div class="data">
        <div>
            Add or edit the default list of Resources that can be used by Workspaces. Updates will immediately be used
            for new Workspaces. Other incomplete Workspaces will be notified of the updates, and will be given the option
            to retrieve the updates.
        </div>
        <br />
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "DefaultResourcesForm", name = "DefaultResourcesForm" })) { %>
            <ul class="validation-box"></ul>
            <div class="form-row">
                <div class="form-label">
                    <span>List Name</span>
                    <div id="Help-DefaultResourcesListName" class="help-icon" onclick="DefaultResourcesWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="DefaultResourcesListName-HelpDialog" class="help-dialog" style="width: 200px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Used to identify the default list of Resources seen by all Workspaces.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <%: Html.TextBoxFor(model => model.ListName, new { onkeyup = "DefaultResourcesWidget.EnableSave()", @class = "half", @maxlength = "50" })%>
                    <div class="buttons inline">
                        <button id="Save-DefaultResources" class="ies-action disabled" name="save-button" type="button">Save</button>
                        <div id="Loader-DefaultResources" class="loader display-none"></div>
                    </div>
                </div>
            </div>
        <% } %>
        <div class="form-row">
            <div class="form-label">
                <span>Elements of Cost</span>
                <div id="ElementsOfCost-Help" class="help-icon" onclick="DefaultResourcesWidget.ToggleHelp(this);"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="ElementsOfCost-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">Select which Elements of Cost you would like to be displayed. Export will only show those which are displayed.</div>
                </div>
            </div>
            <div class="form-element">
                <input type="checkbox" id="ElementsOfCost-ISGSLabor" name="ElementsOfCost" checked="true" />
                <label for="ElementsOfCost-ISGSLabor">LM Labor</label>
                <input type="checkbox" id="ElementsOfCost-IWTA" name="ElementsOfCost" />
                <label for="ElementsOfCost-IWTA">IWTA</label>
                <input type="checkbox" id="ElementsOfCost-Sub" name="ElementsOfCost" />
                <label for="ElementsOfCost-Sub">Sub</label>
                <input type="checkbox" id="ElementsOfCost-ODC" name="ElementsOfCost" />
                <label for="ElementsOfCost-ODC">ODC</label>
                <input type="checkbox" id="ElementsOfCost-Travel" name="ElementsOfCost" />
                <label for="ElementsOfCost-Travel">Travel</label>
                <input type="checkbox" id="ElementsOfCost-Materials" name="ElementsOfCost" />
                <label for="ElementsOfCost-Materials">Materials</label>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <span>Options</span>
                <div id="Help-DefaultResourceOptions" class="help-icon" onclick="DefaultResourcesWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="DefaultResourceOptions-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        Users will be able to select one of these options for the field. <br /><br />
                        Options currently in use by a BOE cannot be deleted. To delete the option, Authors must select a different option in their BOEs.
                    </div>
                </div>
            </div>
            <div class="manage-default-resources-buttons form-element">
                <div class="buttons inline">
                    <button id="DeleteButton-DefaultResources" class="ies-action disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-DefaultResources" class="loader display-none"></div>
                    <button id="AddButton-DefaultResources" class="ies-action" type="button">+ Add</button>
                    <button id="ImportButton-DefaultResources" class="ies-action" name="import-button" type="button">Import</button>
                    <button id="ExportButton-DefaultResources" class="ies-action" type="button">Export</button>
                </div>
                <div class="search-box float-right">
                    <div id="PageControls" class="paging-control"></div>   
                    <%: Html.TextBox("SearchText") %>
                    <div id="SearchMagnifyButton-DefaultResources" class="button search-magnify-glass-button"></div>
                </div>
            </div>
        </div>

        <div class="form-row">
            <div class="form-label">
            </div>
            <div id="DefaultResourcesGridContent" class="form-element">
                <% Html.RenderAction(WebConstants.ACTION_DISPLAY_MANAGE_DEFAULT_RESOURCES_GRID, WebConstants.CONTROLLER_ADMIN, new { searchText="", showLabor = true, showIWTA = false, showSub = false, showODC = false, showTravel = false, showMaterials = false }); %>
            </div>
        </div>
    </div>
</div>

<div id="ImportInProgressDialog" style="display: none; font-size: 18px; font-weight: bold; font-family: Arial, Helvetica; text-align: center;">
    <div class="loader"></div>
    <br />
    Importing Resource Data
</div>

<div id="ImportDialog" class="import-elements-of-cost dialog form" style="display: none;">
    <div id="ImportDialog-Error" class="validation-box" style="display: none;">
        <div><b>Import Failed.</b>
            <div id="ImportDialog-ErrorText">
            </div>
        </div>
        <div class="clear"></div>
    </div>
    <% Html.BeginForm(WebConstants.ACTION_IMPORT_DEFAULT_RESOURCES, WebConstants.CONTROLLER_ADMIN, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportDialog-Form", target = "ImportDialog-UploadTarget" }); %>
    <div style="margin: 5px 0px 15px 0px;">
        Import a new list of options. Current list will be replaced with imported list.
    </div>
    <div class="form-label">
        <span>Elements of Cost</span>
        <div id="ImportElementsOfCost-Help" class="help-icon" onclick="DefaultResourcesWidget.ToggleHelp(this, null, false);"></div>
        <!-- This comment is needed for the jquery animation to work in IE8... -->
        <div id="ImportElementsOfCost-HelpDialog" class="help-dialog" style="width: 200px;">
            <div class="help-dialog-close"></div>
            <div class="help-dialog-text">Select which Elements of Cost you would like to be imported. Only the selected Elements of Cost will be replaced.</div>
        </div>
    </div>
    <div class="form-element">
        <input type="checkbox" id="ElementsOfCostImport-ISGSLabor" name="ImportISGSLabor" />
        <label for="ElementsOfCostImport-ISGSLabor">LM Labor</label>
        <input type="checkbox" id="ElementsOfCostImport-IWTA" name="ImportIWTA" />
        <label for="ElementsOfCostImport-IWTA">IWTA</label>
        <input type="checkbox" id="ElementsOfCostImport-Sub" name="ImportSub" />
        <label for="ElementsOfCostImport-Sub">Sub</label>
        <input type="checkbox" id="ElementsOfCostImport-ODC" name="ImportODC" />
        <label for="ElementsOfCostImport-ODC">ODC</label>
        <input type="checkbox" id="ElementsOfCostImport-Travel" name="ImportTravel" />
        <label for="ElementsOfCostImport-Travel">Travel</label>
        <input type="checkbox" id="ElementsOfCostImport-Materials" name="ImportMaterials" />
        <label for="ElementsOfCostImport-Materials">Materials</label>
    </div>
    <div class="form-label">
        <span>Resources File Location:</span>
    </div>
    <div class="form-element">
        <input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
        <input type="hidden" id="ImportDialog-ResourceListID" name="listID" value="<%:Model.ListID %>" />
        <input type="hidden" id="ImportDialog-ResourceListUpdateDateLong" name="listUpdateDateLong" value="<%:Model.UpdateDateLong %>" />
        <input type="file" size="60" id="ImportDialog-File" name="file" onchange="DefaultResourcesWidget.ValidateFileInput();" />
        <div id="ImportDialog-FileValidation" style="color: #990000; display: none;">
            Please select a file to import</div>
        <ul style="padding: 0px 0px 0px 15px; margin: 10px 0px 0px 0px;">
            <li>File must contain the same headers as <a id="ExportTemplate-DefaultResource"> 
                this format example</a> prior to importing and be in .xlsx format.</li>
            <li>All IDs must be unique.</li>
        </ul>
    </div>
    <div style="text-align: left; margin-top: 20px;">
        <button id="ImportDialog-ImportButton" class="ies-action" name="import-button" type="button">Import</button>
        <button id="ImportDialog-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
    </div>
    <% Html.EndForm(); %>
</div>

<div class="manage-default-resources-edit" id="OptionDialog" style="display: none;">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ResourceOptionForm" })) { %>
    <ul class="validation-box"> </ul>
    <div class="form-row">
        <div class="form-label">ID*</div>
        <div class="form-element">
            <%: Html.TextBoxFor(model => model.DefaultResources.FirstOrDefault().ID, new { @maxlength = "20" })%>
            <input type="hidden" id="UpdateDateLong" name="UpdateDateLong" />
            <input type="hidden" id="ResourceID" name="ResourceID" />
            <input type="hidden" id="ResourceListID" name="ResourceListID" value="<%: Model.ListID %>" />
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Description*</div>
        <div class="form-element">
            <%: Html.TextBoxFor(model => model.DefaultResources.FirstOrDefault().Description, new { @maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Segment Region*</div>
        <div class="form-element">
            <%: Html.TextBoxFor(model => model.DefaultResources.FirstOrDefault().SegmentRegion, new { @maxlength = "50" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Labor Type*</div>
        <div class="form-element">
            <%: Html.TextBoxFor(model => model.DefaultResources.FirstOrDefault().LaborType, new { @maxlength = "50" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Rate Type*</div>
        <div class="form-element">
            <%: Html.DropDownListFor(m => Model.DefaultResources.FirstOrDefault().RateTypeID, (IEnumerable<SelectListItem>)ViewData["RateTypes"])%><!---->
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Element of Cost*</div>
        <div class="form-element">
            <select class="element-of-cost" id="ElementOfCostId" name="ElementOfCostId">
                <%foreach (var listItem in (IEnumerable<SelectListItem>)ViewData["ElementOfCostTypes"])
                    { %>
                    <option value="<%:listItem.Value%>" 
                        <% if (listItem.Selected) Response.Write("selected=true"); %>>
                                <%:listItem.Text %>
                    </option>
                <%}%>
            </select>
        </div>
    </div>
    <div class="form-label"></div>
    <div class="form-element">
        <div class="buttons dialogbuttons" style="width:200px">
            <button id="Save-OptionDialog" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="Loader-OptionDialog" class="loader display-none"></div>
            <button id="DefaultResources-Cancel-OptionDialog" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
    </div>
    <% } %>
</div>