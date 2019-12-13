<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.OffloadRatesModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<% 
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">
    var ManageOffloadRatesWidget = null;

    $(function() {
        
        var dataURL = GenSession.CreateSystemAdminPostURL(
                    '<%:WebConstants.CONTROLLER_ADMIN %>',
                    '<%:WebConstants.ACTION_PAGE_SYSTEM_OFFLOAD_RATES %>');

        //Paging Data Config.
        var pagingData = {};
        pagingData.ContentDiv = $('#ManageOffloadRatesForm .paging-area');
        pagingData.pagingUrl = dataURL,
        pagingData.searchFilter = false,
        pagingData.type = "Arrow";

        var DialogConfigs = [];
        DialogConfigs.push({
            ElementID: "ImportOffloadRatesDialog",
            Params: {
                width: 675,
                title: "Import Offload Rates",
                modal: true, 
                resizable: false, 
                draggable: true, 
                height: 490
            }
        });
        DialogConfigs.push({
            ElementID: "ImportOffloadRatesInProgressDialog",
            Params: {
                width: 400,
                title: "Import Offload Rates",
                height: 80, 
                modal: true, 
                resizable: false, 
                draggable: true, 
                closeOnEscape: false, 
                dialogClass: "ImportInProgress-Dialog"
            }
        });
        DialogConfigs.push({
            ElementID: "ManageOffloadRateDialog",
            Params: {
                width: 450,
                modal: true, 
                resizable: true, 
                draggable: true
            }
        });
    

        var widgetConfig = {};
        widgetConfig.ContextID = "ManageOffloadRatesForm";
        widgetConfig.PagingData = pagingData;
        widgetConfig.DialogConfigs = DialogConfigs;

        ManageOffloadRatesWidget = new GenListWidget(widgetConfig);

    
        ManageOffloadRatesWidget.WSResources = <%= serializer.Serialize(Model.Resources)%>;
        ManageOffloadRatesWidget.WSPerfOrgs = <%= serializer.Serialize(Model.PerformingOrgs)%>;
        ManageOffloadRatesWidget.WSSubResources = <%= serializer.Serialize(Model.SubcontractorResources)%>;

        ManageOffloadRatesWidget.BindEvents = function() {
            // Bind events for single UI elements
            $(document).off('click', '#ManageOffloadRateDialog .add-one:not(.disabled)');
            $(document).on('click', '#ManageOffloadRateDialog .add-one:not(.disabled)', null, ManageOffloadRatesWidget.preparedForSubmit);
            $(document).off('click', "#ManageOffloadRateDialog button[name='save-add-another-button']:not(.disabled)");
            $(document).on('click', "#ManageOffloadRateDialog button[name='save-add-another-button']:not(.disabled)", null, ManageOffloadRatesWidget.preparedForSubmit);

            $("#OffloadRates button[name='add-button']").click(function() { ManageOffloadRatesWidget.AddRecord();
             ManageOffloadRatesWidget.clearValidationBox($("#OffloadRateOptionForm ul.validation-box"));
             });

            $(document).off('click', '#DeleteButton-OffloadRates:not(.disabled)');
            $(document).on('click', '#DeleteButton-OffloadRates:not(.disabled)', null, function() { $(document).trigger('GET_DELETED_RATES'); });

            ManageOffloadRatesWidget.registerForEvent('RATE_CHECKED', function() {
                if($("#ManageOffloadRatesGrid input:checkbox:checked").length === 0)
                {
                    $('#DeleteButton-OffloadRates').addClass('disabled');
                }
                else{
                    $('#DeleteButton-OffloadRates').removeClass('disabled');
                }
            });
            ManageOffloadRatesWidget.registerForEvent('DELETE_RATES', function(event, data) {
                Session.confirmDialog("Delete Offload Rates Confirmation", "Are you sure you want to delete these Offload Rates?", function(){ManageOffloadRatesWidget.DeleteRecords(event, data);});
                } );
            ManageOffloadRatesWidget.registerForEvent('EDIT_RATE', ManageOffloadRatesWidget.EditRecord);
            ManageOffloadRatesWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageOffloadRatesWidget.cleanDirty(); });

            $('#Import-OffloadRates').click(ManageOffloadRatesWidget.Import);
            $('#Export-OffloadRates, #ImportOffloadRates-ExportExisting').click(ManageOffloadRatesWidget.Export);
            $('#ImportOffloadRatesDialog-ImportButton').click(ManageOffloadRatesWidget.SubmitUploadForm);
            $('#ImportOffloadRatesDialog-CancelButton').click(function () { ManageOffloadRatesWidget.getDialog("ImportOffloadRatesDialog").closeDialog(); });
        }
    
        ManageOffloadRatesWidget.preparedForSubmit = function(event) {
         return ManageOffloadRatesWidget.SaveRecord(event);
        }

        ManageOffloadRatesWidget.BackToJumpPage = function() {
            window.location.hash = '#';
        }

        ManageOffloadRatesWidget.DeleteRecords = function (event, data) {
            $('#DeleteButton-OffloadRates').addClass('display-none');
            $('#DeleteLoader-OffloadRates').removeClass('display-none');
            ManageOffloadRatesWidget.SaveRates(event, data.CheckedItems);
        }

        ManageOffloadRatesWidget.AddRecord = function() {
            $('#ManageOffloadRateDialog input[name="Year"]').val('');
            $('#ManageOffloadRateDialog input[name="Resource"]').val('').attr('resource', '');
            $('#ManageOffloadRateDialog input[name="SubcontractorResource"]').val('').attr('subresource', '');
            $('#ManageOffloadRateDialog input[name="PerformingOrg"]').val('').attr('perforg', '');
            $('#ManageOffloadRateDialog input[name="PercentToOffload"]').val('');
            $('#ManageOffloadRateDialog input[name="HourlyRate"]').val('');
            ManageOffloadRatesWidget.getDialog('ManageOffloadRateDialog').openDialog();
            $('#ManageOffloadRateDialog').dialog("option","title","Add Offload Rate");

        }

        ManageOffloadRatesWidget.EditRecord = function(event, data) {
            $('#ManageOffloadRateDialog input[name="OffloadRateID"]').val(data.OffloadRateID);
            $('#ManageOffloadRateDialog input[name="Year"]').val(data.Year);
            $('#ManageOffloadRateDialog input[name="Resource"]').val(data.Resource).attr('resource', data.Resource);
            $('#ManageOffloadRateDialog input[name="SubcontractorResource"]').val(data.SubcontractorResource).attr('subresource', data.SubcontractorResource);
            $('#ManageOffloadRateDialog input[name="PerformingOrg"]').val(data.PerformingOrg).attr('perforg', data.PerformingOrg);
            $('#ManageOffloadRateDialog input[name="PercentToOffload"]').val(data.PercentToOffload);
            $('#ManageOffloadRateDialog input[name="HourlyRate"]').val(data.HourlyRate);
            ManageOffloadRatesWidget.getDialog('ManageOffloadRateDialog').openDialog();
            $('#ManageOffloadRateDialog').dialog("option","title","Edit Offload Rate");
            ManageOffloadRatesWidget.clearValidationBox($("#OffloadRateOptionForm ul.validation-box"));
        }

        ManageOffloadRatesWidget.perforgAutoCompleteSelect = function(event, ui){
            ManageOffloadRatesWidget.PerfOrgSelect(ui.item.id, ui.item.value);
        };

        ManageOffloadRatesWidget.PerfOrgSelect = function(id, name){
            var perfOrgInput = $("#OffloadRateOptionForm input[name=PerformingOrg]");
            $(perfOrgInput).attr("perforg", name)
            $(perfOrgInput).val(name);
            $(perfOrgInput).change();
            var formBeingSaved = $(perfOrgInput).parents("form")[0];
            var valBox = $("ul.validation-box", formBeingSaved);
            if (id == 0){
                $(valBox).find("li[name=PerformingOrg]").remove();
                valBox.append("<li name='PerformingOrg'>Performing Org is invalid</li>");
                $(valBox).show();
                $(valBox).fadeIn(500);
                ManageOffloadRatesWidget.focusValidation();
            }
            else{
                $(valBox).find("li[name=PerformingOrg]").remove();
                if ($(valBox).find("li").length == 0) {
                    $(valBox).hide();
                }
            }
            ManageOffloadRatesWidget.refreshModule();
        };

        ManageOffloadRatesWidget.autocompletePerformingOrgs = function( request, response ) {
            var toSearch = request.term.toLowerCase();
            returned = $.grep(ManageOffloadRatesWidget.WSPerfOrgs, function (p) {
                return p.PerformingOrgName.toLowerCase().indexOf(toSearch) != -1 ||
                   p.PerformingOrgDesc.toLowerCase().indexOf(toSearch) != -1 ;})

            response($.map(returned, function( item ) {
                return {
                    label: item.PerformingOrgName+"-"+item.PerformingOrgDesc,
                    value: item.PerformingOrgName,
                    ID: item.Id
                }
            }));
        };

        $("#OffloadRateOptionForm input[name=PerformingOrg]").autocomplete({
            source: ManageOffloadRatesWidget.autocompletePerformingOrgs,
            autoFocus: true,
            minLength: 2,
            focus: function (event, ui) {
            },
            select: ManageOffloadRatesWidget.perforgAutoCompleteSelect
        }).on('blur', function (e) {
            var that = $(this);
            var perfOrgInput = $("#OffloadRateOptionForm input[name=PerformingOrg]");
            if (that.val() != perfOrgInput.attr("perforg")) {
                if ($('.ui-autocomplete li:visible').length > 0) {
                    var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                    that.val(item.value);
                    ManageOffloadRatesWidget.PerfOrgSelect(item.id, item.value);
                }
                else {
                    ManageOffloadRatesWidget.PerfOrgSelect(0, that.val());
                }
            }
        });

        $('.select-perforg.popup-div').on('click', '.perforg-option', function () {
            var option = $(this);
            ManageOffloadRatesWidget.PerfOrgSelect(option.attr("perforgid"), option.attr('perforgname'));
        });

        var options = {
            propagateOnClick: true,
            button: 'div.popup-div-button.for-perf-orgs',
            onShow: function (button) { ManageOffloadRatesWidget.PerfOrgPopupButton = $(button); },
            leftOffSet: -233
        };
        $('div.popup-div.select-perforg').genPopUp(options);

        var html = "";

        $.each(ManageOffloadRatesWidget.WSPerfOrgs, function (i, item) {
            html += '<div class="perforg-option" perforgid="' + item.Id + '" perforgname="' + item.PerformingOrgName + '" perforgdesc="' + item.PerformingOrgDesc + '">' + item.PerformingOrgName + '-' + item.PerformingOrgDesc + '</div>';
        });
        $('div.popup-div.select-perforg').html(html);
    
        // resource

        ManageOffloadRatesWidget.resourceAutoCompleteSelect = function(event, ui){
            ManageOffloadRatesWidget.ResourceSelect(ui.item.id, ui.item.value);
        };

        ManageOffloadRatesWidget.ResourceSelect = function(id, name){
            var resourceInput = $("#OffloadRateOptionForm input[name=Resource]");
            $(resourceInput).attr("resource", name)
            $(resourceInput).val(name);
            $(resourceInput).change();
            var formBeingSaved = $(resourceInput).parents("form")[0];
            var valBox = $("ul.validation-box", formBeingSaved);
            if (id == 0){
                $(valBox).find("li[name=Resource]").remove();
                valBox.append("<li name='Resource'>Resource is invalid</li>");
                $(valBox).show();
                $(valBox).fadeIn(500);
                ManageOffloadRatesWidget.focusValidation();
            }
            else{
                $(valBox).find("li[name=Resource]").remove();
                if ($(valBox).find("li").length == 0) {
                    $(valBox).hide();
                }
            }
            ManageOffloadRatesWidget.refreshModule();
        };

        ManageOffloadRatesWidget.autocompleteResources = function( request, response ) {
            var toSearch = request.term.toLowerCase();
            returned = $.grep(ManageOffloadRatesWidget.WSResources, function (p) {
                return p.ResourceName.toLowerCase().indexOf(toSearch) != -1 ||
                   p.ResourceDesc.toLowerCase().indexOf(toSearch) != -1 ;})

            response($.map(returned, function( item ) {
                return {
                    label: item.ResourceName+"-"+item.ResourceDesc,
                    value: item.ResourceName,
                    ID: item.Id
                }
            }));
        };

        $("#OffloadRateOptionForm input[name=Resource]").autocomplete({
            source: ManageOffloadRatesWidget.autocompleteResources,
            autoFocus: true,
            minLength: 2,
            focus: function (event, ui) {
            },
            select: ManageOffloadRatesWidget.resourceAutoCompleteSelect
        }).on('blur', function (e) {
            var that = $(this);
            var resourceInput = $("#OffloadRateOptionForm input[name=Resource]");
            if (that.val() != resourceInput.attr("resource")) {
                if ($('.ui-autocomplete li:visible').length > 0) {
                    var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                    that.val(item.value);
                    ManageOffloadRatesWidget.ResourceSelect(item.id, item.value);
                }
                else {
                    ManageOffloadRatesWidget.ResourceSelect(0, that.val());
                }
            }
        });

        $('.select-resource.popup-div').on('click', '.resource-option', function () {
            var option = $(this);
            ManageOffloadRatesWidget.ResourceSelect(option.attr("resourceid"), option.attr('resourcename'));
        });

        var options = {
            propagateOnClick: true,
            button: 'div.popup-div-button.for-resources', 
            onShow: function (button) { ManageOffloadRatesWidget.ResourcePopupButton = $(button); },
            leftOffSet: -233
        };
        $('div.popup-div.select-resource').genPopUp(options);

        html = "";

        $.each(ManageOffloadRatesWidget.WSResources, function (i, item) {
            html += '<div class="resource-option" resourceid="' + item.Id + '" resourcename="' + item.ResourceName + '" resourcedesc="' + item.ResourceDesc + '">' + item.ResourceName + '-' + item.ResourceDesc + '</div>';
        });
        $('div.popup-div.select-resource').html(html);
    
        // sub resource
        ManageOffloadRatesWidget.subresourceAutoCompleteSelect = function(event, ui){
            ManageOffloadRatesWidget.SubResourceSelect(ui.item.id, ui.item.value);
        };

        ManageOffloadRatesWidget.SubResourceSelect = function(id, name){
            var subresourceInput = $("#OffloadRateOptionForm input[name=SubcontractorResource]");
            $(subresourceInput).attr("subresource", name)
            $(subresourceInput).val(name);
            $(subresourceInput).change();
            var formBeingSaved = $(subresourceInput).parents("form")[0];
            var valBox = $("ul.validation-box", formBeingSaved);
            if (id == 0){
                $(valBox).find("li[name=SubResource]").remove();
                valBox.append("<li name='SubResource'>Subcontractor Resource is invalid</li>");
                $(valBox).show();
                $(valBox).fadeIn(500);
                ManageOffloadRatesWidget.focusValidation();
            }
            else{
                $(valBox).find("li[name=SubResource]").remove();
                if ($(valBox).find("li").length == 0) {
                    $(valBox).hide();
                }
            }
            ManageOffloadRatesWidget.refreshModule();
        };

        ManageOffloadRatesWidget.autocompleteSubResources = function( request, response ) {
            var toSearch = request.term.toLowerCase();
            returned = $.grep(ManageOffloadRatesWidget.WSSubResources, function (p) {
                return p.ResourceName.toLowerCase().indexOf(toSearch) != -1 ||
                   p.ResourceDesc.toLowerCase().indexOf(toSearch) != -1 ;})

            response($.map(returned, function( item ) {
                return {
                    label: item.ResourceName+"-"+item.ResourceDesc,
                    value: item.ResourceName,
                    ID: item.Id
                }
            }));
        };

        $("#OffloadRateOptionForm input[name=SubcontractorResource]").autocomplete({
            source: ManageOffloadRatesWidget.autocompleteSubResources,
            autoFocus: true,
            minLength: 2,
            focus: function (event, ui) {
            },
            select: ManageOffloadRatesWidget.subresourceAutoCompleteSelect
        }).on('blur', function (e) {
            var that = $(this);
            var subresourceInput = $("#OffloadRateOptionForm input[name=SubcontractorResource]");
            if (that.val() != subresourceInput.attr("subresource")) {
                if ($('.ui-autocomplete li:visible').length > 0) {
                    var item = $($(".ui-autocomplete li:visible:first").data()).attr('item.autocomplete');
                    that.val(item.value);
                    ManageOffloadRatesWidget.SubResourceSelect(item.id, item.value);
                }
                else {
                    ManageOffloadRatesWidget.SubResourceSelect(0, that.val());
                }
            }
        });

        $('.select-sub-resource.popup-div').on('click', '.resource-option', function () {
            var option = $(this);
            ManageOffloadRatesWidget.SubResourceSelect(option.attr("resourceid"), option.attr('resourcename'));
        });

        var options = {
            propagateOnClick: true,
            button: 'div.popup-div-button.for-sub-resources', 
            onShow: function (button) { ManageOffloadRatesWidget.SubResourcePopupButton = $(button); },
            leftOffSet: -233
        };
        $('div.popup-div.select-sub-resource').genPopUp(options);

        html = "";

        $.each(ManageOffloadRatesWidget.WSSubResources, function (i, item) {
            html += '<div class="resource-option" resourceid="' + item.Id + '" resourcename="' + item.ResourceName + '" resourcedesc="' + item.ResourceDesc + '">' + item.ResourceName + '-' + item.ResourceDesc + '</div>';
        });
        $('div.popup-div.select-sub-resource').html(html);
    
        ManageOffloadRatesWidget.SaveRecord = function(event) {
                var data = new Array();
                var option = {};
                option.OffloadRateID = $('#ManageOffloadRateDialog input[name="OffloadRateID"]').val();
                option.Year = $('#ManageOffloadRateDialog input[name="Year"]').val();
                option.Resource = $('#ManageOffloadRateDialog input[name="Resource"]').val();
                option.SubcontractorResource = $('#ManageOffloadRateDialog input[name="SubcontractorResource"]').val();
                option.PerformingOrg = $('#ManageOffloadRateDialog input[name="PerformingOrg"]').val();
                option.PercentToOffload = $('#ManageOffloadRateDialog input[name="PercentToOffload"]').val();
                option.HourlyRate = $('#ManageOffloadRateDialog input[name="HourlyRate"]').val();
            
                data.push(option);
                ManageOffloadRatesWidget.SaveRates(event, data);
        }

        ManageOffloadRatesWidget.SaveRates = function(event, data) {
            if (data.length > 0) {
                var dataToSend = {};
                dataToSend.offloadRates = data;
                ManageOffloadRatesWidget.clearValidationBox($("#OffloadRateOptionForm ul.validation-box"));
                dataToSend = JSON.stringify(dataToSend);

                ManageOffloadRatesWidget.saveRequest({
                    type: 'POST',
                    url: CreateSystemAdminPostURL(
                        '<%: WebConstants.CONTROLLER_ADMIN %>',
                        '<%: WebConstants.ACTION_SAVE_OFFLOAD_RATES %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'html',
                    data: dataToSend,
                    performDirtyProcessing: false,
                    success: function(response) {
                        if($(event.target).attr("name") != 'save-add-another-button')
                        {
                            ManageOffloadRatesWidget.getDialog("ManageOffloadRateDialog").closeDialog();
                        }
                        $('#ManageOffloadRateDialog input[name="OffloadRateID"]').val('-1');
                        $('#ManageOffloadRateDialog input[name="Year"]').val('');
                        $('#ManageOffloadRateDialog input[name="Resource"]').val('');
                        $('#ManageOffloadRateDialog input[name="SubcontractorResource"]').val(''); 
                        $('#ManageOffloadRateDialog input[name="PerformingOrg"]').val('');
                        $('#ManageOffloadRateDialog input[name="PercentToOffload"]').val('');
                        $('#ManageOffloadRateDialog input[name="HourlyRate"]').val(''); 
                        delete ManageOffloadRatesWidget.pagingData.data;
                        $('#DeleteButton-OffloadRates').removeClass('display-none');
                        $('#DeleteLoader-OffloadRates').addClass('display-none');
                        ManageOffloadRatesWidget.pageResults("down");
                        RaiseNotification("Changes saved");
                    },
                    error: function(response) {
                        $('#DeleteButton-OffloadRates').removeClass('display-none');
                        $('#DeleteLoader-OffloadRates').addClass('display-none');
                    }
                }, $("button[name='save-button'].add-one"));
            }
            else {
                $('#DeleteButton-OffloadRates').addClass('disabled');
                $('#DeleteButton-OffloadRates').removeClass('display-none');
                $('#DeleteLoader-OffloadRates').addClass('display-none');
            }
        }

        ManageOffloadRatesWidget.Import = function () {
            ManageOffloadRatesWidget.getDialog("ImportOffloadRatesDialog").openDialog();
            $('#ImportInstructions').removeClass('display-none');
            $('#ImportOffloadRates-ImportLoader').addClass('display-none');
        };
    
        ManageOffloadRatesWidget.SubmitUploadForm = function () {
            if (ManageOffloadRatesWidget.ValidateFileInput()) {
                ManageOffloadRatesWidget.HideImportOffloadRatesDialogError();
                ManageOffloadRatesWidget.getDialog("ImportOffloadRatesInProgressDialog").openDialog();
                
                ManageOffloadRatesWidget.AppendIFrameForUploadResponse();
                $("#ImportOffloadRatesDialog-Form").submit();
            }
        }

        ManageOffloadRatesWidget.AppendIFrameForUploadResponse = function () {
            // Remove the old hidden iFrame, if it exists
            $('#ImportOffloadRatesDialog-UploadTarget').remove();

            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            $('#ImportOffloadRatesDialog-Form').append('<iframe id="ImportOffloadRatesDialog-UploadTarget" name="ImportOffloadRatesDialog-UploadTarget" class="display-none"></iframe>');
            $('#ImportOffloadRatesDialog-UploadTarget').load(ManageOffloadRatesWidget.StopUpload);
        }

        ManageOffloadRatesWidget.StopUpload = function () { //Function will be called when iframe is loaded
            var uploadResponseElement = $("#ImportOffloadRatesDialog-UploadTarget").contents().find("body #UploadResponse");

            if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
                var results = eval('(' + uploadResponseElement.html() + ')');

                if (results != undefined && results.Status) {
                    // Hide Dialogs
                    ManageOffloadRatesWidget.getDialog("ImportOffloadRatesDialog").closeDialog();
                    ManageOffloadRatesWidget.getDialog("ImportOffloadRatesInProgressDialog").closeDialog();

                    // Show success notification
                    RaiseNotification('Import successful and changes saved');

                    // Redirect back to jump page
                    delete ManageOffloadRatesWidget.pagingData.data;
                    ManageOffloadRatesWidget.pageResults("down");
                }
                else {
                    $("#ImportOffloadRatesDialog-ErrorText").html(results.Message);
                    $("#ImportOffloadRatesDialog-File").val('');
                    ManageOffloadRatesWidget.DisableImportOffloadRatesDialogImportButton();
                    ManageOffloadRatesWidget.ShowImportOffloadRatesDialogError();
                    ManageOffloadRatesWidget.getDialog("ImportOffloadRatesInProgressDialog").closeDialog();
                }
            }
            else {
                ManageOffloadRatesWidget.getDialog("ImportOffloadRatesInProgressDialog").closeDialog();
            }
        }
    
        ManageOffloadRatesWidget.ValidateFileInput = function () {
            if ($("#ImportOffloadRatesDialog-File").val().length == 0) {
                ManageOffloadRatesWidget.ShowImportOffloadRatesDialogFileValidation();
                ManageOffloadRatesWidget.DisableImportOffloadRatesDialogImportButton();
                return false;
            }
            else {
                ManageOffloadRatesWidget.HideImportOffloadRatesDialogFileValidation();
                ManageOffloadRatesWidget.EnableImportOffloadRatesDialogImportButton();
                return true;
            }
        }

        ManageOffloadRatesWidget.EnableImportOffloadRatesDialogImportButton = function () {
            $("#ImportOffloadRatesDialog-ImportButton").removeClass('display-none');
        }

        ManageOffloadRatesWidget.DisableImportOffloadRatesDialogImportButton = function () {
            $("#ImportOffloadRatesDialog-ImportButton").addClass('display-none');
        }

        ManageOffloadRatesWidget.ShowImportOffloadRatesDialogError = function () {
            $("#ImportOffloadRatesDialog-Error").slideDown("slow");
        }

        ManageOffloadRatesWidget.HideImportOffloadRatesDialogError = function () {
            $("#ImportOffloadRatesDialog-Error").slideUp("slow");
        }

        ManageOffloadRatesWidget.ShowImportOffloadRatesDialogFileValidation = function () {
            $("#ImportOffloadRatesDialog-FileValidation").slideDown("slow");
        }

        ManageOffloadRatesWidget.HideImportOffloadRatesDialogFileValidation = function () {
            $("#ImportOffloadRatesDialog-FileValidation").slideUp("slow");
        }

        ManageOffloadRatesWidget.Export = function () {
            // Remove the old hidden iFrame, if it exists
            $('#ManageOffloadRates-DownloadTarget').remove();

            // Create a new hidden iFrame and set it's source to the chosen report's URL
            var targetIFrame = $('<iframe />', {
                'id': 'ManageOffloadRates-DownloadTarget',
                'class': 'display-none',
                'src': CreateSystemAdminPostURL(
                        '<%: WebConstants.CONTROLLER_ADMIN %>',
                        '<%: WebConstants.ACTION_EXPORT_OFFLOAD_RATES%>')
            });

            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            targetIFrame.appendTo('body');
        };

        ManageOffloadRatesWidget.ValidateFileInput();
        ManageOffloadRatesWidget.BindEvents();
    });
</script>

<div id="ManageOffloadRatesForm">
    <div id="OffloadRates" class="manage-default-resources section">
    <div class="title">Manage Offload Rates</div>
    <div class="data">
        <div>
            Add new or edit Offloading Rates.
        </div>
        <br />
        <div class="form-row">
            <div class="manage-default-resources-buttons form-element">
                <div class="inline">
                    <button id="DeleteButton-OffloadRates" class="ies disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-OffloadRates" class="loader display-none"></div>
                    <button id="AddButton-OffloadRates" name="add-button" class="ies" type="button">+ Add</button>
                    <button id="Import-OffloadRates" class="ies" name="import-button" type="button">Import</button>
                    <button id="Export-OffloadRates" class="ies" type="button">Export</button>
                </div>
            </div>
        </div>

        <div class="form-row">
            <div id="OffloadRatesGridContent" class="form-element">
                <div class="paging-area">
                    <% Html.RenderAction(WebConstants.ACTION_PAGE_SYSTEM_OFFLOAD_RATES, WebConstants.CONTROLLER_ADMIN); %>
                </div>
            </div>
        </div>
    </div>
</div>
</div>
<div class="manage-default-resources-edit" id="ManageOffloadRateDialog">

    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "OffloadRateOptionForm" })) { %>
    <ul class="validation-box"></ul>
    <div id="OffloadRatesNote">Please note that the "Percent To Offload" below is a decimal number (the % in the grid is 100 * the decimal).</div>

    <div class="form-row">
        <div class="form-label" style="width:130px">Resource*</div>
        <div class="form-element resource-selection">
            <input type="text" class="select-resource-autocomplete" placeholder="Select Resource" name="Resource" value="" resource="" />
            <div class="inline-block attached-down-arrow-button button popup-div-button for-resources"></div>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"style="width:130px">Performing Org*</div>
        <div class="form-element perforg-selection">
            <input type="text" class="select-perforg-autocomplete" placeholder="Select Performing Org" name="PerformingOrg" value="" perforg="" />
            <div class="inline-block attached-down-arrow-button button popup-div-button for-perf-orgs"></div>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"style="width:130px">Subcontractor Resource*</div>
        <div class="form-element sub-resource-selection">
            <input type="text" class="select-sub-resource-autocomplete" placeholder="Select Subcontractor Resource" name="SubcontractorResource" value="" subresource="" />
            <div class="inline-block attached-down-arrow-button button popup-div-button for-sub-resources"></div>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label" style="width:130px">Year*</div>
        <div class="form-element">
            <input type="hidden" id="OffloadRateID" name="OffloadRateID" value="-1" />
            <input type="text" id="Year" name="Year" maxlength="25"/>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"style="width:130px">Percent To Offload*</div>
        <div class="form-element">
            <input type="text" id="PercentToOffload" name="PercentToOffload" maxlength="5"/>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"style="width:130px">Hourly Rate*</div>
        <div class="form-element">
            <input type="text" id="HourlyRate" name="HourlyRate" maxlength="6"/>
        </div>
    </div>
    <div class="form-label"></div>
    <div class="form-element">
        <div>
            <button class="ies-action add-one" name="save-button" type="button">Save</button>
            <div class="loader display-none"></div>
            <button class="ies" name="save-add-another-button" type="button">Save & add another</button>
        </div>
    </div>

    <div class="popup-div select-perforg" style="display: none;">
    </div>
    <div class="popup-div select-resource" style="display: none;">
    </div>
    <div class="popup-div select-sub-resource" style="display: none;">
    </div>
    <% } %>
</div>
<div id="ImportOffloadRatesDialog" class="import-OffloadRates dialog form" style="display: none;">
    <div id="ImportOffloadRatesDialog-Error" class="validation-box" style="display: none;">
        <div><b>Import Failed.</b>
            <div id="ImportOffloadRatesDialog-ErrorText">
            </div>
        </div>
        <div class="clear"></div>
    </div>
    <div id="ImportInstructions">
        <span>Importing Offload Rates does a complete replacement of the existing Offload Rates.  Start by exporting the existing Offload Rates defined in genBOE to get the column definitions and the existing data. Then you modify the data and finally import the updated information.</span>
        <div class="container">
            <div class="step one">
                <div class="title">Step 1: Export the existing Offload Rates Data</div>
                <span>Start by exporting the existing Offload Rates Data. This file has the correct column headings and all of the current Offload Rates data. Then you modify the data and finally import the updated information.</span>
                <div><a id="ImportOffloadRates-ExportExisting">Export existing Offload Rates</a></div>
            </div>
            <div class="step two">
                <div class="title">Step 2: Modify the existing Offload Rates Data</div>
                <div>
                    <ul>
                        <li>If you are only going to add new Offload Rates, then leave all of the existing data in the spreadsheet.</li>
                        <li>Add data for new Offload Rates.</li>
                        <li>You should not modify column A (normally hidden). It should be left blank for new Offload Rates.</li>
                        <li>The header row should not be modified.</li>
                        <li>All Columns are required.</li>
                    </ul>
                </div>
            </div>
            <div class="step three">
                <% Html.BeginForm(WebConstants.ACTION_IMPORT_OFFLOAD_RATES, WebConstants.CONTROLLER_ADMIN, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportOffloadRatesDialog-Form", target = "ImportOffloadRatesDialog-UploadTarget" }); %>
                <div class="title">Step 3: Import the Offload Rates data </div>
                <div id="ImportOffloadRatesDialog-FileValidation" style="color: #990000; display: none;">
                <div>Select the file to import and then click the <i>Import</i> button.</div>
                </div>
                <div>
                    <input type="hidden" id="ImportOffloadRates-DocumentDomain" name="documentDomain" />
                    <input type="file" style="width:410px;" id="ImportOffloadRatesDialog-File" name="file" onchange="ManageOffloadRatesWidget.ValidateFileInput();" />
                </div>            
                <div class="buttons">
                    <button id="ImportOffloadRatesDialog-ImportButton" class="ies-action display-none" name="import-button" type="button">Import</button>
                    <button id="ImportOffloadRatesDialog-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
                    <div id="ImportOffloadRates-ImportLoader" class="loader display-none"></div>
                </div>
                <% Html.EndForm(); %>
            </div>
        </div>
    </div>
</div>