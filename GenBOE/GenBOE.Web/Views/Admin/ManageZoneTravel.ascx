<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Dtos.MSTZoneTravelOriginModelView>" %>

<script type="text/javascript">
    var ManageZoneTravelWidget = new Widget('OriginElementForm', '<%: ViewData["READONLY"] %>'.isTrue());

    ManageZoneTravelWidget.OriginElementDialog = {};
    ManageZoneTravelWidget.ImportOriginsDialog = {};
    ManageZoneTravelWidget.ImportOriginsInProgressDialog = {};

    ManageZoneTravelWidget.Initialize = function () {
        ManageZoneTravelWidget.OriginElementDialog.Element = $("#OriginElementDialog");
        ManageZoneTravelWidget.OriginElementDialog.Params = { width: 580, modal: true, resizable: false, draggable: true };
        ManageZoneTravelWidget.InitializeDialog(ManageZoneTravelWidget.OriginElementDialog);

        ManageZoneTravelWidget.ImportOriginsDialog.Element = $("#ImportOriginsDialog");
        ManageZoneTravelWidget.ImportOriginsDialog.Params = { title: "Import Origins", width: 675, modal: true, resizable: false, draggable: true, height: 490 };
        ManageZoneTravelWidget.InitializeDialog(ManageZoneTravelWidget.ImportOriginsDialog);

        ManageZoneTravelWidget.ImportOriginsInProgressDialog.Element = $("#ImportOriginsInProgressDialog");
        ManageZoneTravelWidget.ImportOriginsInProgressDialog.Params = { width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" };
        ManageZoneTravelWidget.InitializeDialog(ManageZoneTravelWidget.ImportOriginsInProgressDialog);

    };

    ManageZoneTravelWidget.BindEvents = function () {
        $('#AddButton-Origins').click(ManageZoneTravelWidget.AddOrigin);

        ManageZoneTravelWidget.registerForLiveEvent('click',
            '#OriginElementDialog-Save:not(.disabled)', ManageZoneTravelWidget.Save);

        ManageZoneTravelWidget.registerForLiveEvent('click',
            '#OriginElementDialog-SaveAddAnother:not(.disabled)', ManageZoneTravelWidget.SaveAddAnother);

        ManageZoneTravelWidget.registerForLiveEvent('click', '#DeleteButton-Origins:not(.disabled)', function () { $(document).trigger('GET_DELETED_ORIGINS'); });

        ManageZoneTravelWidget.registerForEvent('DELETE_ORIGINS', ManageZoneTravelWidget.DeleteSelectedOrigins);

        $('#Import-Origins').click(ManageZoneTravelWidget.Import);
        $('#Export-Origins, #ImportOrigins-ExportExisting').click(ManageZoneTravelWidget.Export);
        $('#ImportOriginsDialog-ImportButton').click(ManageZoneTravelWidget.SubmitUploadForm);
        $('#ImportOriginsDialog-CancelButton').click(function () { ManageZoneTravelWidget.CloseDialog(ManageZoneTravelWidget.ImportOriginsDialog) });
        
        ManageZoneTravelWidget.registerForEvent('ENABLE_ORIGINS_DELETE_BUTTON', function (event, data) {
            if (data != undefined && data == false) {
                $('#DeleteButton-Origins').addClass('disabled');
            }
            else {
                $('#DeleteButton-Origins').removeClass('disabled');
            }
        });

        ManageZoneTravelWidget.registerForEvent('EDIT_ORIGIN', ManageZoneTravelWidget.EditOriginFormRow);
        ManageZoneTravelWidget.registerForEvent('RELOAD_GRID', ManageZoneTravelWidget.ReloadGridData);
    };

    // Remove old dialogs left behind when jumping back to main Admin jump page
    ManageZoneTravelWidget.RemoveStaleDialogs = function () {
        $('body .ui-dialog').children('#ImportOriginsDialog, #ImportOriginsInProgressDialog, #OriginElementDialog').parent().remove();
        $('body').children('#ImportOriginsDialog, #ImportOriginsInProgressDialog, #OriginElementDialog').remove();
    }

    ManageZoneTravelWidget.ReloadGridData = function () {
        $('#PageLoading').removeClass('display-none');

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_ZONE_TRAVEL_GRID %>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (response) {
                $('#ZoneTravelGridContent').html(response);
                $('#PageLoading').addClass('display-none');
            },
            error: function (response) {
                $('#PageLoading').addClass('display-none');
            }
        });
    };

    ManageZoneTravelWidget.AddOrigin = function () {
        ManageZoneTravelWidget.ClearOriginElementDialog();
        ManageZoneTravelWidget.ChangeDialogTitle(ManageZoneTravelWidget.OriginElementDialog, "Add Origin");
        ManageZoneTravelWidget.OpenDialogAfterInitialize(ManageZoneTravelWidget.OriginElementDialog);
        ManageZoneTravelWidget.clearValidationBox($("#OriginElementForm ul.validation-box"));
    };

    ManageZoneTravelWidget.EditOriginFormRow = function (event, data) {
        if (data != undefined) {
            var dataRow = $(data);

            var origin = {};

            origin.OriginID = dataRow.data('pkid');
            origin.Origin = dataRow.find('input.Origin').val();
            origin.Site = dataRow.find('input.Site').val();

            origin.ResIDPR1 = dataRow.find('input.ResourceIDPRZ1').val();
            origin.ResIDPR2 = dataRow.find('input.ResourceIDPRZ2').val();
            origin.ResIDPR3 = dataRow.find('input.ResourceIDPRZ3').val();
            origin.ResIDPR4 = dataRow.find('input.ResourceIDPRZ4').val();
            origin.ResIDPR5 = dataRow.find('input.ResourceIDPRZ5').val();
            origin.ResIDPR6 = dataRow.find('input.ResourceIDPRZ6').val();
            origin.ResIDTR1 = dataRow.find('input.ResourceIDTRZ1').val();
            origin.ResIDTR2 = dataRow.find('input.ResourceIDTRZ2').val();
            origin.ResIDTR3 = dataRow.find('input.ResourceIDTRZ3').val();
            origin.ResIDTR4 = dataRow.find('input.ResourceIDTRZ4').val();
            origin.ResIDTR5 = dataRow.find('input.ResourceIDTRZ5').val();
            origin.ResIDTR6 = dataRow.find('input.ResourceIDTRZ6').val();

            origin.ResPR1 = dataRow.find('input.ResourcePRZ1').val();
            origin.ResPR2 = dataRow.find('input.ResourcePRZ2').val();
            origin.ResPR3 = dataRow.find('input.ResourcePRZ3').val();
            origin.ResPR4 = dataRow.find('input.ResourcePRZ4').val();
            origin.ResPR5 = dataRow.find('input.ResourcePRZ5').val();
            origin.ResPR6 = dataRow.find('input.ResourcePRZ6').val();
            origin.ResTR1 = dataRow.find('input.ResourceTRZ1').val();
            origin.ResTR2 = dataRow.find('input.ResourceTRZ2').val();
            origin.ResTR3 = dataRow.find('input.ResourceTRZ3').val();
            origin.ResTR4 = dataRow.find('input.ResourceTRZ4').val();
            origin.ResTR5 = dataRow.find('input.ResourceTRZ5').val();
            origin.ResTR6 = dataRow.find('input.ResourceTRZ6').val();

            ManageZoneTravelWidget.EditOrigin(origin);
        }
    };

    ManageZoneTravelWidget.EditOrigin = function (origin) {
        if (origin != undefined) {
            ManageZoneTravelWidget.ClearOriginElementDialog();
            
            $('#OriginElementForm #OriginID').val(origin.OriginID);
            $('#OriginElementForm #Origin').val(origin.Origin);
            $('#OriginElementForm #Site').val(origin.Site);
            
            $('#OriginElementForm #ResourceIDPRZ1').val(origin.ResIDPR1);
            $('#OriginElementForm #ResourceIDPRZ2').val(origin.ResIDPR2);
            $('#OriginElementForm #ResourceIDPRZ3').val(origin.ResIDPR3);
            $('#OriginElementForm #ResourceIDPRZ4').val(origin.ResIDPR4);
            $('#OriginElementForm #ResourceIDPRZ5').val(origin.ResIDPR5);
            $('#OriginElementForm #ResourceIDPRZ6').val(origin.ResIDPR6);

            $('#OriginElementForm #ResourceIDTRZ1').val(origin.ResIDTR1);
            $('#OriginElementForm #ResourceIDTRZ2').val(origin.ResIDTR2);
            $('#OriginElementForm #ResourceIDTRZ3').val(origin.ResIDTR3);
            $('#OriginElementForm #ResourceIDTRZ4').val(origin.ResIDTR4);
            $('#OriginElementForm #ResourceIDTRZ5').val(origin.ResIDTR5);
            $('#OriginElementForm #ResourceIDTRZ6').val(origin.ResIDTR6);

            $('#OriginElementForm #ResourcePRZ1').val(origin.ResPR1);
            $('#OriginElementForm #ResourcePRZ2').val(origin.ResPR2);
            $('#OriginElementForm #ResourcePRZ3').val(origin.ResPR3);
            $('#OriginElementForm #ResourcePRZ4').val(origin.ResPR4);
            $('#OriginElementForm #ResourcePRZ5').val(origin.ResPR5);
            $('#OriginElementForm #ResourcePRZ6').val(origin.ResPR6);

            $('#OriginElementForm #ResourceTRZ1').val(origin.ResTR1);
            $('#OriginElementForm #ResourceTRZ2').val(origin.ResTR2);
            $('#OriginElementForm #ResourceTRZ3').val(origin.ResTR3);
            $('#OriginElementForm #ResourceTRZ4').val(origin.ResTR4);
            $('#OriginElementForm #ResourceTRZ5').val(origin.ResTR5);
            $('#OriginElementForm #ResourceTRZ6').val(origin.ResTR6);

            ManageZoneTravelWidget.ChangeDialogTitle(ManageZoneTravelWidget.OriginElementDialog, "Edit Origin");
            ManageZoneTravelWidget.OpenDialogAfterInitialize(ManageZoneTravelWidget.OriginElementDialog);
            ManageZoneTravelWidget.clearValidationBox($("#OriginElementForm ul.validation-box"));
        }
    };

    ManageZoneTravelWidget.DeleteSelectedOrigins = function () {
        var dataToSend = [];

        //confirm delete
        Session.confirmDialog('Delete Origins', 'Are you sure you want to delete these Origins?', function () {
            $('#DeleteLoader-Origins').removeClass('display-none');
            $('#DeleteButton-Origins').addClass('display-none');

            $('#ManageZoneTravelGrid tbody input[type=checkbox]:checked').each(function () {
                var originsToDelete = {};

                var parentRow = $(this).parents('tr');
                originsToDelete.OriginID = parentRow.data('pkid');
                originsToDelete.Deleted = true;

                dataToSend.push(originsToDelete);
            });

            dataToSend = JSON.stringify(dataToSend);

            $.ajax({
                type: 'POST',
                url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                                              '<%:WebConstants.ACTION_DELETE_ORIGINS%>'),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function () {
                    $('#DeleteLoader-Origins').addClass('display-none');
                    $('#DeleteButton-Origins').removeClass('display-none');
                    $(document).trigger('ENABLE_ORIGINS_DELETE_BUTTON', false);
                    ManageZoneTravelWidget.ReloadGridData();
                },
                error: function () {
                    $('#DeleteLoader-Origins').addClass('display-none');
                    $('#DeleteButton-Origins').removeClass('display-none');
                }
            });
        });
    };

    ManageZoneTravelWidget.ClearOriginElementDialog = function () {
        $('#OriginElementForm :input').val('');
        $('#OriginElementForm div.form-element span[class]').text('');

        $('#OriginElementForm #OriginID').val('-1');

        $('#OriginElementForm #ResourceIDPRZ1').val('-1');
        $('#OriginElementForm #ResourceIDPRZ2').val('-1');
        $('#OriginElementForm #ResourceIDPRZ3').val('-1');
        $('#OriginElementForm #ResourceIDPRZ4').val('-1');
        $('#OriginElementForm #ResourceIDPRZ5').val('-1');
        $('#OriginElementForm #ResourceIDPRZ6').val('-1');

        $('#OriginElementForm #ResourceIDTRZ1').val('-1');
        $('#OriginElementForm #ResourceIDTRZ2').val('-1');
        $('#OriginElementForm #ResourceIDTRZ3').val('-1');
        $('#OriginElementForm #ResourceIDTRZ4').val('-1');
        $('#OriginElementForm #ResourceIDTRZ5').val('-1');
        $('#OriginElementForm #ResourceIDTRZ6').val('-1');
    };

    ManageZoneTravelWidget.Save = function () {
        ManageZoneTravelWidget.SaveOrigin();
    };

    ManageZoneTravelWidget.SaveAddAnother = function () {
        ManageZoneTravelWidget.SaveOrigin('AddAnother');
    };

    ManageZoneTravelWidget.SaveOrigin = function (option) {
        var dataToSend = {};

        dataToSend.OriginID = $('#OriginElementForm #OriginID').val();
        dataToSend.Origin = $('#OriginElementForm #Origin').val();
        dataToSend.Site = $('#OriginElementForm #Site').val();

        dataToSend.ResourceIDPRZ1 = $('#OriginElementForm #ResourceIDPRZ1').val();
        dataToSend.ResourceIDPRZ2 = $('#OriginElementForm #ResourceIDPRZ2').val();
        dataToSend.ResourceIDPRZ3 = $('#OriginElementForm #ResourceIDPRZ3').val();
        dataToSend.ResourceIDPRZ4 = $('#OriginElementForm #ResourceIDPRZ4').val();
        dataToSend.ResourceIDPRZ5 = $('#OriginElementForm #ResourceIDPRZ5').val();
        dataToSend.ResourceIDPRZ6 = $('#OriginElementForm #ResourceIDPRZ6').val();

        dataToSend.ResourceIDTRZ1 = $('#OriginElementForm #ResourceIDTRZ1').val();
        dataToSend.ResourceIDTRZ2 = $('#OriginElementForm #ResourceIDTRZ2').val();
        dataToSend.ResourceIDTRZ3 = $('#OriginElementForm #ResourceIDTRZ3').val();
        dataToSend.ResourceIDTRZ4 = $('#OriginElementForm #ResourceIDTRZ4').val();
        dataToSend.ResourceIDTRZ5 = $('#OriginElementForm #ResourceIDTRZ5').val();
        dataToSend.ResourceIDTRZ6 = $('#OriginElementForm #ResourceIDTRZ6').val();

        dataToSend.ResourcePRZ1 = $('#OriginElementForm #ResourcePRZ1').val();
        dataToSend.ResourcePRZ2 = $('#OriginElementForm #ResourcePRZ2').val();
        dataToSend.ResourcePRZ3 = $('#OriginElementForm #ResourcePRZ3').val();
        dataToSend.ResourcePRZ4 = $('#OriginElementForm #ResourcePRZ4').val();
        dataToSend.ResourcePRZ5 = $('#OriginElementForm #ResourcePRZ5').val();
        dataToSend.ResourcePRZ6 = $('#OriginElementForm #ResourcePRZ6').val();

        dataToSend.ResourceTRZ1 = $('#OriginElementForm #ResourceTRZ1').val();
        dataToSend.ResourceTRZ2 = $('#OriginElementForm #ResourceTRZ2').val();
        dataToSend.ResourceTRZ3 = $('#OriginElementForm #ResourceTRZ3').val();
        dataToSend.ResourceTRZ4 = $('#OriginElementForm #ResourceTRZ4').val();
        dataToSend.ResourceTRZ5 = $('#OriginElementForm #ResourceTRZ5').val();
        dataToSend.ResourceTRZ6 = $('#OriginElementForm #ResourceTRZ6').val();

        dataToSend = JSON.stringify(dataToSend);

        if (option != undefined && option == 'AddAnother') {
            $('#OriginElementDialog-SaveAddAnother').addClass('display-none');
        } else {
            $('#OriginElementDialog-Save').addClass('display-none');
        };
        $('#OriginElementDialog-Loader').removeClass('display-none');

        ManageZoneTravelWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                                            '<%:WebConstants.ACTION_SAVE_ORIGIN%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function () {
                $('#OriginElementDialog-Loader').addClass('display-none');
                $('#OriginElementDialog-Save').removeClass('display-none');
                $('#OriginElementDialog-SaveAddAnother').removeClass('display-none');

                ManageZoneTravelWidget.ReloadGridData();

                if (option != undefined && option == 'AddAnother') {
                    ManageZoneTravelWidget.ClearOriginElementDialog();
                    ManageZoneTravelWidget.AddOrigin();
                }
                else {
                    ManageZoneTravelWidget.CloseDialog(ManageZoneTravelWidget.OriginElementDialog);
                }
            },
            error: function () {
                $('#OriginElementDialog-Loader').addClass('display-none');
                $('#OriginElementDialog-Save').removeClass('display-none');
                $('#OriginElementDialog-SaveAddAnother').removeClass('display-none');
            }
        }, $('#OriginElementDialog-Save'));
    };

    ManageZoneTravelWidget.Import = function () {
        ManageZoneTravelWidget.OpenDialogAfterInitialize(ManageZoneTravelWidget.ImportOriginsDialog);
        $('#ImportInstructions').removeClass('display-none');
        $('#ImportOrigins-ImportLoader').addClass('display-none');
    };

    ManageZoneTravelWidget.SubmitUploadForm = function () {
        if (ManageZoneTravelWidget.ValidateFileInput()) {
            ManageZoneTravelWidget.HideImportOriginsDialogError();
            ManageZoneTravelWidget.OpenDialogAfterInitialize(ManageZoneTravelWidget.ImportOriginsInProgressDialog);

            ManageZoneTravelWidget.AppendIFrameForUploadResponse();
            $("#ImportOriginsDialog-Form").submit();
        }
    }

    ManageZoneTravelWidget.AppendIFrameForUploadResponse = function () {
        // Remove the old hidden iFrame, if it exists
        $('#ImportOriginsDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportOriginsDialog-Form').append('<iframe id="ImportOriginsDialog-UploadTarget" name="ImportOriginsDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportOriginsDialog-UploadTarget').load(ManageZoneTravelWidget.StopUpload);
    }

    ManageZoneTravelWidget.StopUpload = function () { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportOriginsDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide Dialogs
                ManageZoneTravelWidget.CloseDialog(ManageZoneTravelWidget.ImportOriginsDialog);
                ManageZoneTravelWidget.CloseDialog(ManageZoneTravelWidget.ImportOriginsInProgressDialog);

                // Show success notification
                RaiseNotification('Import successful and changes saved');

                // Redirect back to jump page
                ManageZoneTravelWidget.ReloadGridData();
            }
            else {
                $("#ImportOriginsDialog-ErrorText").html(results.Message);
                ManageZoneTravelWidget.ShowImportOriginsDialogError();
                ManageZoneTravelWidget.CloseDialog(ManageZoneTravelWidget.ImportOriginsInProgressDialog);
            }
        }
        else {
            ManageZoneTravelWidget.CloseDialog(ManageZoneTravelWidget.ImportOriginsInProgressDialog);
        }
    }



    ManageZoneTravelWidget.ValidateFileInput = function () {
        if ($("#ImportOriginsDialog-File").val().length == 0) {
            ManageZoneTravelWidget.ShowImportOriginsDialogFileValidation();
            ManageZoneTravelWidget.DisableImportOriginsDialogImportButton();
            return false;
        }
        else {
            ManageZoneTravelWidget.HideImportOriginsDialogFileValidation();
            ManageZoneTravelWidget.EnableImportOriginsDialogImportButton();
            return true;
        }
    }

    ManageZoneTravelWidget.EnableImportOriginsDialogImportButton = function () {
        $("#ImportOriginsDialog-ImportButton").removeClass('display-none');
    }

    ManageZoneTravelWidget.DisableImportOriginsDialogImportButton = function () {
        $("#ImportOriginsDialog-ImportButton").addClass('display-none');
    }

    ManageZoneTravelWidget.ShowImportOriginsDialogError = function () {
        $("#ImportOriginsDialog-Error").slideDown("slow");
    }

    ManageZoneTravelWidget.HideImportOriginsDialogError = function () {
        $("#ImportOriginsDialog-Error").slideUp("slow");
    }

    ManageZoneTravelWidget.ShowImportOriginsDialogFileValidation = function () {
        $("#ImportOriginsDialog-FileValidation").slideDown("slow");
    }

    ManageZoneTravelWidget.HideImportOriginsDialogFileValidation = function () {
        $("#ImportOriginsDialog-FileValidation").slideUp("slow");
    }

    ManageZoneTravelWidget.Export = function () {
        // Remove the old hidden iFrame, if it exists
        $('#ManageZoneTravelOrigins-DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'ManageZoneTravelOrigins-DownloadTarget',
            'class': 'display-none',
            'src': CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_EXPORT_ZONE_TRAVEL_ORIGINS %>')
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    };

    $(function () {
        ManageZoneTravelWidget.afterDOMLoad();
        ManageZoneTravelWidget.RemoveStaleDialogs();
        ManageZoneTravelWidget.ValidateFileInput();
        ManageZoneTravelWidget.Initialize();
        ManageZoneTravelWidget.BindEvents();

        ManageZoneTravelWidget.ReloadGridData();
    });
</script>

<div id="ZoneTravel" class="manage-zone-travel section">
    <div class="title">Manage Zone Travel Origins</div>
    <div class="data">
        <div>
            Add new or edit origins for zone travel
        </div>
        <br />
        <div class ="form-row">
            <div>
                <div class="inline">
                    <button id="DeleteButton-Origins" class="ies disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-Origins" class="loader display-none"></div>
                    <button id="AddButton-Origins" class="ies" type="button">+ Add</button>
                    <button id="Import-Origins" class="ies" name="import-button" type="button">Import</button>
                    <button id="Export-Origins" class="ies" type="button">Export</button>
                    <div name="PageControls" class="float-right" style="margin-top: 20px;"></div>
                </div>
            </div>
        </div>

        <div class="form-row">
            <div id="ZoneTravelGridContent" class="form-element">
            </div>
        </div>
    </div>
</div>

<div class="manage-zone-travel-element" id="OriginElementDialog" style="display: none">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "OriginElementForm", name = "OriginElementForm" }))
        { %>
        <ul class="validation-box"></ul>
        
        <%: Html.HiddenFor(Model => Model.OriginID) %>
        
        <br />
        <div class="form-row">
            <div class="form-label">Origin*</div>
            <div class="form-element">
                <%: Html.TextBoxFor(Model => Model.Origin, new { @class = "long", @maxlength = "100" }) %>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Site*</div>
            <div class="form-element">
                <%: Html.TextBoxFor(Model => Model.Site, new { @class = "long", @maxlength = "100" }) %>
            </div>
        </div>
        <br />
        <div id="Resources" style="text-align: center; margin-right: 100px;">
            <div class="title">
                Resource IDs
            </div>
            <div>
                Blank IDs will be set to NO-RATE.
            </div>
            <br />
            <div id="Types">
                <div class="title" style="display: inline; margin-right:150px">
                    Per Diem (PR)
                </div>
                <div class="title" style="display: inline">
                    Airfare (TR)
                </div>
            </div>
            <div id="zone1" class="form-row">
                <div class="form-label" style="display: inline">Zone 1</div>
                <div class="form-element" style="display: inline; margin-left:10px; margin-right:50px">
                    <%: Html.HiddenFor(Model => Model.ResourceIDPRZ1) %>
                    <%: Html.TextBoxFor(Model => Model.ResourcePRZ1, new { @class = "long", @maxlength = "100" }) %>
                </div>
                <div class="form-element" style="display: inline">
                    <%: Html.HiddenFor(Model => Model.ResourceIDTRZ1) %>
                    <%: Html.TextBoxFor(Model => Model.ResourceTRZ1, new { @class = "long", @maxlength = "100" }) %>
                </div>
            </div>
            <div id="zone2" class="form-row">
                <div class="form-label" style="display: inline">Zone 2</div>
                <div class="form-element" style="display: inline; margin-left:10px; margin-right:50px">
                    <%: Html.HiddenFor(Model => Model.ResourceIDPRZ2) %>
                    <%: Html.TextBoxFor(Model => Model.ResourcePRZ2, new { @class = "long", @maxlength = "100" }) %>
                </div>
                <div class="form-element" style="display: inline">
                    <%: Html.HiddenFor(Model => Model.ResourceIDTRZ2) %>
                    <%: Html.TextBoxFor(Model => Model.ResourceTRZ2, new { @class = "long", @maxlength = "100" }) %>
                </div>
            </div>
            <div id="zone3" class="form-row">
                <div class="form-label" style="display: inline">Zone 3</div>
                <div class="form-element" style="display: inline; margin-left:10px; margin-right:50px">
                    <%: Html.HiddenFor(Model => Model.ResourceIDPRZ3) %>
                    <%: Html.TextBoxFor(Model => Model.ResourcePRZ3, new { @class = "long", @maxlength = "100" }) %>
                </div>
                <div class="form-element" style="display: inline">
                    <%: Html.HiddenFor(Model => Model.ResourceIDTRZ3) %>
                    <%: Html.TextBoxFor(Model => Model.ResourceTRZ3, new { @class = "long", @maxlength = "100" }) %>
                </div>
            </div>
            <div id="zone4" class="form-row">
                <div class="form-label" style="display: inline">Zone 4</div>
                <div class="form-element" style="display: inline; margin-left:10px; margin-right:50px">
                    <%: Html.HiddenFor(Model => Model.ResourceIDPRZ4) %>
                    <%: Html.TextBoxFor(Model => Model.ResourcePRZ4, new { @class = "long", @maxlength = "100" }) %>
                </div>
                <div class="form-element" style="display: inline">
                    <%: Html.HiddenFor(Model => Model.ResourceIDTRZ4) %>
                    <%: Html.TextBoxFor(Model => Model.ResourceTRZ4, new { @class = "long", @maxlength = "100" }) %>
                </div>
            </div>
            <div id="zone5" class="form-row">
                <div class="form-label" style="display: inline">Zone 5</div>
                <div class="form-element" style="display: inline; margin-left:10px; margin-right:50px">
                    <%: Html.HiddenFor(Model => Model.ResourceIDPRZ5) %>
                    <%: Html.TextBoxFor(Model => Model.ResourcePRZ5, new { @class = "long", @maxlength = "100" }) %>
                </div>
                <div class="form-element" style="display: inline">
                    <%: Html.HiddenFor(Model => Model.ResourceIDTRZ5) %>
                    <%: Html.TextBoxFor(Model => Model.ResourceTRZ5, new { @class = "long", @maxlength = "100" }) %>
                </div>
            </div>
            <div id="zone6" class="form-row">
                <div class="form-label" style="display: inline">Zone 6</div>
                <div class="form-element" style="display: inline; margin-left:10px; margin-right:50px">
                    <%: Html.HiddenFor(Model => Model.ResourceIDPRZ6) %>
                    <%: Html.TextBoxFor(Model => Model.ResourcePRZ6, new { @class = "long", @maxlength = "100" }) %>
                </div>
                <div class="form-element" style="display: inline">
                    <%: Html.HiddenFor(Model => Model.ResourceIDTRZ6) %>
                    <%: Html.TextBoxFor(Model => Model.ResourceTRZ6, new { @class = "long", @maxlength = "100" }) %>
                </div>
            </div>
        </div>
        <br />
        <div class="form-row">
            <div class="form-label"></div>
            <div class="form-element">
                <div>
                    <button id="OriginElementDialog-Save" class="ies-action" name="save-button" type="button">Save</button>
                    <div id="OriginElementDialog-Loader" class="loader display-none"></div>
                    <button id="OriginElementDialog-SaveAddAnother" class="ies" name="save-add-another-button" type="button">Save & add another</button>
                </div>
            </div>
        </div>
    <% } %>
</div>

<div id="ImportOriginsDialog" class="import-origins dialog form" style="display: none;">
    <div id="ImportOriginsDialog-Error" class="validation-box" style="display: none;">
        <div><b>Import Failed.</b>
            <div id="ImportOriginsDialog-ErrorText">
            </div>
        </div>
        <div class="clear"></div>
    </div>
    <div id="ImportInstructions">
        <span>You can import new origin definitions, update existing origins or do both in the same import. You can start by exporting the existing origins defined in genBOE to get the column definitions and the existing data. Then you modify the data and finally import the updated information.</span>
        <div class="container">
            <div class="step one">
                <div class="title">Step 1: Export the existing Origins Data</div>
                <span>Start by exporting the existing Origins Data. This file has the correct column headings and all of the current Origins data. Then you modify the data and finally import the updated information.</span>
                <div><a id="ImportOrigins-ExportExisting">Export existing origins</a></div>
            </div>
            <div class="step two">
                <div class="title">Step 2: Modify the existing Origins Data</div>
                <div>
                    <ul>
                        <li>If you are only going to add new origins, then delete all rows in the spreadsheet except for the header row.</li>
                        <li>If you are also going to update existing origins, leave in all of the existing origins (or you can delete origins that you are not updating).</li>
                        <li>Add data for new origins.</li>
                        <li>You should not modify column A (normally hidden). It should be left blank for new origins.</li>
                        <li>The header row should not be modified.</li>
                        <li>Columns B and C (Origin and Site) are required.</li>
                        <li>Columns D through O (Origin IDs) are not required, but will be set to NO-RATE if left blank.</li>
                    </ul>
                </div>
            </div>
            <div class="step three">
                <% Html.BeginForm(WebConstants.ACTION_IMPORT_ORIGINS, WebConstants.CONTROLLER_ADMIN, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportOriginsDialog-Form", target = "ImportOriginsDialog-UploadTarget" }); %>
                <div class="title">Step 3: Import the Origins data </div>
                <div id="ImportOriginsDialog-FileValidation" style="color: #990000; display: none;">
                <div>Select the file to import and then click the <i>Import</i> button.</div>
                </div>
                <div>
                    <input type="hidden" id="ImportOrigins-DocumentDomain" name="documentDomain" />
                    <input type="file" style="width:410px;" id="ImportOriginsDialog-File" name="file" onchange="ManageZoneTravelWidget.ValidateFileInput();" />
                </div>            
                <div class="buttons">
                    <button id="ImportOriginsDialog-ImportButton" class="ies-action display-none" name="import-button" type="button">Import</button>
                    <button id="ImportOriginsDialog-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
                    <div id="ImportOrigins-ImportLoader" class="loader display-none"></div>
                </div>
                <% Html.EndForm(); %>
            </div>
        </div>
    </div>
</div>