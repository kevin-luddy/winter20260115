<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<object>" %>
<%
    ICollection<string> customFieldNames = (ICollection<string>)ViewData["CustomFieldNames"];
    %>
<script type="text/javascript">
    var separatorField = '<%:ExportToProPricerModelView.CUSTOM_FIELD_ID_DESCRIPTION_SEPARATOR%>';

    var ManageSystemProPricerExportsWidget = new Widget('ManageSystemProPricerExports', false);

    ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog = {};
    ManageSystemProPricerExportsWidget.AddCustomFieldDialog = {};

    ManageSystemProPricerExportsWidget.Initialize = function () {
        
        ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog.Element = $('#ExportToProPricerElementDialog');
        ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog.Params = { width: 680, modal: true, resizable: false, height: 640 };

        ManageSystemProPricerExportsWidget.InitializeDialog(ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog);

        ManageSystemProPricerExportsWidget.AddCustomFieldDialog.Element = $('#AddCustomFieldDialog');
        ManageSystemProPricerExportsWidget.AddCustomFieldDialog.Params = { width: 350, modal: true, resizable: true, height: 200, title: 'Add System Custom Field' };

        ManageSystemProPricerExportsWidget.InitializeDialog(ManageSystemProPricerExportsWidget.AddCustomFieldDialog);
    }

    ManageSystemProPricerExportsWidget.BoldRequiredFields = function () {
        $('#ExportToProPricerElementDialog #TasksSelected option, #ExportToProPricerElementDialog #TasksUnselected option').each(function () {
            var value = $(this).val();

            // Set required values
            if (value == '<%: (int)ProPricerField_Task.BOEStartDate %>' ||
                value == '<%: (int)ProPricerField_Task.BOEEndDate %>') {
                $(this).addClass('required').attr('required', 'true').text($(this).text() + '*');
            }
        });
    };

    ManageSystemProPricerExportsWidget.BindEvents = function () {
        // Object Events

        $('#SystemCustomFieldTable td.inUse').click(function () {
            var customFieldName = $(this).text();
            $("#OriginalCustomFieldName").val(customFieldName);
            $("#NewCustomFieldName").val(customFieldName);
            $("#EditWarning").show();
            $("#AddWarning").hide();
            ManageSystemProPricerExportsWidget.ChangeDialogTitle(ManageSystemProPricerExportsWidget.AddCustomFieldDialog, 'Edit System Custom Field');
            $('#AddCustomFieldDialog-Save').removeClass('disabled');
            ManageSystemProPricerExportsWidget.OpenDialogAfterInitialize(ManageSystemProPricerExportsWidget.AddCustomFieldDialog);
        });

        $('#ExportToProPricer-Add').click(ManageSystemProPricerExportsWidget.AddNewExportFormat);

        $('#ExportToProPricerElementDialog-Cancel').click(function () {
            ManageSystemProPricerExportsWidget.CloseDialog(ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog);
        });

        $('#CustomFieldSystemProPricer-Add').click(function () {
            $("#OriginalCustomFieldName").val('');
            $("#NewCustomFieldName").val('');
            $("#EditWarning").hide();
            $("#AddWarning").show();
            ManageSystemProPricerExportsWidget.ChangeDialogTitle(ManageSystemProPricerExportsWidget.AddCustomFieldDialog, 'Add System Custom Field');
            $('#AddCustomFieldDialog-Save').removeClass('disabled');
            ManageSystemProPricerExportsWidget.OpenDialogAfterInitialize(ManageSystemProPricerExportsWidget.AddCustomFieldDialog);
        });


        $('#AddCustomFieldDialog-Cancel').click(function () {
            ManageSystemProPricerExportsWidget.CloseDialog(ManageSystemProPricerExportsWidget.AddCustomFieldDialog);
        });

        $('#AddCustomFieldDialog-Save').click(ManageSystemProPricerExportsWidget.SaveCustomField);

        $(' #ExportToProPricer-Delete:not(.disabled)').click(ManageSystemProPricerExportsWidget.DeleteExportFormats);

        $('#ExportToProPricerElementDialog #TasksAddBlank:not(.disabled)').click(function () {
            ManageSystemProPricerExportsWidget.AddBlank($('#ExportToProPricerElementDialog #TasksSelected'), '<%: (int)ProPricerField_Task.BLANK %>');
        });

        $('#ExportToProPricerElementDialog #TasksMoveToSelected').click(function () {
            ManageSystemProPricerExportsWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #TasksUnselected'), $('#ExportToProPricerElementDialog #TasksSelected'));
        });

        $('#ExportToProPricerElementDialog #TasksMoveToUnselected').click(function () {
            ManageSystemProPricerExportsWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #TasksSelected'), $('#ExportToProPricerElementDialog #TasksUnselected'));
            ManageSystemProPricerExportsWidget.SortSelectList($('#ExportToProPricerElementDialog #TasksUnselected'));
        });

        $('#ExportToProPricerElementDialog #TasksMoveItemsUp').click(function () {
            ManageSystemProPricerExportsWidget.ShiftSelectedItemsUp($('#ExportToProPricerElementDialog #TasksSelected'));
        });

        $('#ExportToProPricerElementDialog #TasksMoveItemsDown').click(function () {
            ManageSystemProPricerExportsWidget.ShiftSelectedItemsDown($('#ExportToProPricerElementDialog #TasksSelected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesAddBlank:not(.disabled)').click(function () {
            ManageSystemProPricerExportsWidget.AddBlank($('#ExportToProPricerElementDialog #ResourcesSelected'), '<%: (int)ProPricerField_Resources.BLANK %>');
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveToSelected').click(function () {
            ManageSystemProPricerExportsWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #ResourcesUnselected'), $('#ExportToProPricerElementDialog #ResourcesSelected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveToUnselected').click(function () {
            ManageSystemProPricerExportsWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #ResourcesSelected'), $('#ExportToProPricerElementDialog #ResourcesUnselected'));
            ManageSystemProPricerExportsWidget.SortSelectList($('#ExportToProPricerElementDialog #ResourcesUnselected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveItemsUp').click(function () {
            ManageSystemProPricerExportsWidget.ShiftSelectedItemsUp($('#ExportToProPricerElementDialog #ResourcesSelected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveItemsDown').click(function () {
            ManageSystemProPricerExportsWidget.ShiftSelectedItemsDown($('#ExportToProPricerElementDialog #ResourcesSelected'));
        });

        ManageSystemProPricerExportsWidget.registerForLiveEvent('click', '#ExportToProPricerElementDialog-Save:not(.disabled)', ManageSystemProPricerExportsWidget.SaveExportFormat);
        ManageSystemProPricerExportsWidget.registerForLiveEvent('click', '#ExportToProPricer-Delete:not(.disabled)', ManageSystemProPricerExportsWidget.DeleteExportFormats);

        $('#ExportToProPricerElementDialog select[name=Copy]').change(function () {
            var rowToCopy = $('#ExportToProPricer #ProPricerGridContent table tbody tr[pkid=' + $(this).val() + ']');

            if (rowToCopy != undefined && rowToCopy.length) {
                ManageSystemProPricerExportsWidget.CopyExportFormat(rowToCopy);
            }
            else {
                ManageSystemProPricerExportsWidget.AddNewExportFormat();
            }
        });

        $('#ExportToProPricerElementDialog input[name=Name]').keyup(function () {

            var name = $(this).val();
            if (name.length > 0) {
                $('#ExportToProPricerElementDialog-Save').removeClass('disabled');
            }
            else {
                $('#ExportToProPricerElementDialog-Save').addClass('disabled');
            }
        });

        $('#ExportToProPricerElementDialog').on('dialogclose', function (event) {
            ManageSystemProPricerExportsWidget.clearValidationBox($('#ExportToProPricerElementForm ul.validation-box'));
        });

        // Triggered Events
        ManageSystemProPricerExportsWidget.registerForEvent('ENABLE_PROPRICER_DELETE_BUTTON', function (event, enable) {
            if (enable != undefined && enable == false) {
                $('#ExportToProPricer-Delete').addClass('disabled');
            }
            else {
                $('#ExportToProPricer-Delete').removeClass('disabled');
            }
        });

        ManageSystemProPricerExportsWidget.registerForEvent('EDIT_PROPRICER_EXPORT_FORMAT', function (event, rowToEdit) {
            if (rowToEdit != undefined) {
                ManageSystemProPricerExportsWidget.EditExportFormat(rowToEdit);
            }
        });

        // Set the formats available for copy on the Add/Edit ProPricer Export Format Dialog
        ManageSystemProPricerExportsWidget.registerForEvent('SET_PROPRICER_COPY_FROM_FORMATS', function (event, availableFormatsJSON) {
            if (availableFormatsJSON != undefined && availableFormatsJSON.length > 0) {
                var availableFormats = JSON.parse(availableFormatsJSON);

                if (availableFormats != undefined) {
                    var copyFromList = $('#ExportToProPricerElementDialog select[name=Copy]');

                    // Clear the list of options
                    copyFromList.children('option').remove();

                    // Add the empty option
                    copyFromList.append('<option value="-1"></option>');

                    // Add each format option
                    for (var ndx = 0; ndx < availableFormats.length; ndx++) {
                        copyFromList.append('<option value="' + availableFormats[ndx].ID + '">' + availableFormats[ndx].Name + '</option>');
                    }
                }
            }
        });
    };

    ManageSystemProPricerExportsWidget.SaveCustomField = function () {
        // Validate name is set and not a duplicate
        var name = $("#NewCustomFieldName").val();
        var re = new RegExp("[a-zA-Z0-9/ \_-]{0,}");
        var original = $("#OriginalCustomFieldName").val();

        if (name != name.match(re)) {
            // alert for bad input
            GenSession.alertDialog('Save Changes', 'The custom field name must be alphanumeric with blanks, dashes and underscores allowed.');
        } else {
            if (name && name.length > 0) {
                // check if duplicate
                var duplicate = false;
                var found = $("#SystemCustomFieldTable tbody tr td[custom='" + name + "']");
                if (found && found.length > 0) {
                    duplicate = true;
                }

                if (duplicate) {
                    // alert for duplicate
                    GenSession.alertDialog('Save Changes', 'The custom field name being added is a duplicate.');
                } else {
                    if (original === '') {
                        // this is an Add

                        // Add to Lists
                        var taskList = $('#ExportToProPricerElementDialog #TasksUnselected');
                        var resourceList = $('#ExportToProPricerElementDialog #ResourcesUnselected')
                        taskList.append('<option value="' + name + separatorField + '1">' + name + ' ID</option>');
                        taskList.append('<option value="' + name + separatorField + '2">' + name + ' Description</option>');
                        resourceList.append('<option value="' + name + separatorField + '1">' + name + ' ID</option>');
                        resourceList.append('<option value="' + name + separatorField + '2">' + name + ' Description</option>');
                        $('#SystemCustomFieldTable tbody').append('<tr><td custom=' + name + '>' + name + '</td></tr>');
                        // Resort the lists
                        ManageSystemProPricerExportsWidget.SortSelectList(taskList);
                        ManageSystemProPricerExportsWidget.SortSelectList(resourceList);

                        // Close the Dialog
                        ManageSystemProPricerExportsWidget.CloseDialog(ManageSystemProPricerExportsWidget.AddCustomFieldDialog);
                    } else {
                        // this is an update
                        $('#AddCustomFieldDialog #AddCustomFieldDialog-Loader').removeClass('display-none');
                        $('#AddCustomFieldDialog #AddCustomFieldDialog-Save').addClass('display-none');

                        var dataToSend = {};
                        dataToSend.originalName = original;
                        dataToSend.updatedName = name;
                        dataToSend = JSON.stringify(dataToSend);

                        ManageSystemProPricerExportsWidget.ajaxRequest({
                            type: 'POST',
                            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN %>',
                                '<%:WebConstants.ACTION_SAVE_PROPRICER_EXPORT_CUSTOM_FIELD %>', ''),
                            contentType: 'application/json; charset=utf-8',
                            dataType: 'json',
                            data: dataToSend,
                            success: function (response) {
                                ManageSystemProPricerExportsWidget.ReloadExportGrid();
                                // Update to Lists
                                var taskList = $('#ExportToProPricerElementDialog #TasksUnselected');
                                var resourceList = $('#ExportToProPricerElementDialog #ResourcesUnselected');
                                var taskListOptions = $('#ExportToProPricerElementDialog #TasksUnselected option');
                                var resourceListOptions = $('#ExportToProPricerElementDialog #ResourcesUnselected option');
                                taskListOptions.filter("[value='" + original + separatorField + "1']").remove();
                                taskListOptions.filter("[value='" + original + separatorField + "2']").remove();
                                resourceListOptions.filter("[value='" + original + separatorField + "1']").remove();
                                resourceListOptions.filter("[value='" + original + separatorField + "2']").remove();
                                taskList.append('<option value="' + name + separatorField + '1">' + name + ' ID</option>');
                                taskList.append('<option value="' + name + separatorField + '2">' + name + ' Description</option>');
                                resourceList.append('<option value="' + name + separatorField + '1">' + name + ' ID</option>');
                                resourceList.append('<option value="' + name + separatorField + '2">' + name + ' Description</option>');
                                var customTd = $('#SystemCustomFieldTable tbody tr td[custom="' + original + '"');
                                customTd.text(name);
                                customTd.attr('custom', name);

                                // Resort the lists
                                ManageSystemProPricerExportsWidget.SortSelectList(taskList);
                                ManageSystemProPricerExportsWidget.SortSelectList(resourceList);
                                
                                ManageSystemProPricerExportsWidget.CloseDialog(ManageSystemProPricerExportsWidget.AddCustomFieldDialog);
                                $('#AddCustomFieldDialog #AddCustomFieldDialog-Loader').addClass('display-none');
                                $('#AddCustomFieldDialog #AddCustomFieldDialog-Save').removeClass('display-none');
                            },
                            error: function () {
                                $('#AddCustomFieldDialog #AddCustomFieldDialog-Loader').addClass('display-none');
                                $('#AddCustomFieldDialog #AddCustomFieldDialog-Save').removeClass('display-none');
                            }
                        }, $('#AddCustomFieldDialog-Save'));
                    }
                }
            } else {
                // alert for no name input
                GenSession.alertDialog('Save Changes', 'The custom field name is required.');
            }
        }
    };

    // Opens the Format Element dialog with fields cleared and ready to create a new format
    ManageSystemProPricerExportsWidget.AddNewExportFormat = function () {

        $('#ExportToProPricerElementDialog input[name=PKID]').val(-1);
        $('#ExportToProPricerElementDialog input[name=UpdateDateLong]').val('0');

        $('#ExportToProPricerElementDialog input[name=Name]').val('').keyup();

        $('#ExportToProPricerElementDialog select[name=Copy]').val(-1);


        ManageSystemProPricerExportsWidget.ClearSelectedTasks();
        ManageSystemProPricerExportsWidget.ClearSelectedResources();

        ManageSystemProPricerExportsWidget.SetElementFormReadOnly(false);

        ManageSystemProPricerExportsWidget.ChangeDialogTitle(ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog, 'Add System ProPricer Export Format');
        ManageSystemProPricerExportsWidget.OpenDialogAfterInitialize(ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog);
        $('#ExportToProPricerElementDialog #CopyFromRow').removeClass('display-none');
    };

    // Opens the Format Element dialog with fields set to the values for an existing format
    ManageSystemProPricerExportsWidget.EditExportFormat = function (rowToEdit) {
        rowToEdit = $(rowToEdit);

        $('#ExportToProPricerElementDialog input[name=PKID]').val(rowToEdit.attr('pkid'));
        $('#ExportToProPricerElementDialog input[name=UpdateDateLong]').val(rowToEdit.find('input[name=UpdateDateLong]').val().toString());

        $('#ExportToProPricerElementDialog input[name=Name]').val(rowToEdit.find('input[name=Name]').val()).removeClass('display-none').keyup();

        $('#ExportToProPricerElementDialog select[name=Copy]').val(-1);

        // Clear Task and Resource selections
        ManageSystemProPricerExportsWidget.ClearSelectedTasks();
        ManageSystemProPricerExportsWidget.ClearSelectedResources();

        ManageSystemProPricerExportsWidget.MoveSelectedRowItemsToSelectedBox(rowToEdit);

        ManageSystemProPricerExportsWidget.SetElementFormReadOnly(false);
        ManageSystemProPricerExportsWidget.ChangeDialogTitle(ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog, 'Edit System ProPricer Export Format');
        ManageSystemProPricerExportsWidget.OpenDialogAfterInitialize(ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog);
		$('#ExportToProPricerElementDialog #CopyFromRow').addClass('display-none');
    };

    // Opens the Format Element dialog with fields set to the values for an existing format
    ManageSystemProPricerExportsWidget.CopyExportFormat = function (rowToCopy) {
        rowToCopy = $(rowToCopy);

        var formatName = 'Copy of ' + rowToCopy.find('input[name=Name]').val();

        if (formatName.length > 100) {
            formatName = formatName.substring(0, 100);
        }

        $('#ExportToProPricerElementDialog input[name=Name]').val(formatName).removeClass('display-none').focusout().keyup();

        // Clear Task and Resource selections
        ManageSystemProPricerExportsWidget.ClearSelectedTasks();
        ManageSystemProPricerExportsWidget.ClearSelectedResources();

        ManageSystemProPricerExportsWidget.MoveSelectedRowItemsToSelectedBox(rowToCopy);
    };

    ManageSystemProPricerExportsWidget.MoveSelectedRowItemsToSelectedBox = function (tableRow) {
        // Move selected Tasks to Selected List
        var tasks = JSON.parse(tableRow.find('input[name=OrderedTasks]').val());

        for (var ndx in tasks) {
            if (tasks[ndx] == '<%: (int)ProPricerField_Task.BLANK %>') {
                ManageSystemProPricerExportsWidget.AddBlank($('#ExportToProPricerElementDialog #TasksSelected'), '<%: (int)ProPricerField_Task.BLANK %>');
            }
            else {
                ManageSystemProPricerExportsWidget.MoveSelectListItem($('#ExportToProPricerElementDialog #TasksUnselected option[value="' + tasks[ndx] + '"]'), $('#ExportToProPricerElementDialog #TasksSelected'));
            }
        }

        // Move selected Resources to Selected List
        var resources = JSON.parse(tableRow.find('input[name=OrderedResources]').val());

        for (var ndx in resources) {
            if (resources[ndx] == '<%: (int)ProPricerField_Resources.BLANK %>') {
                ManageSystemProPricerExportsWidget.AddBlank($('#ExportToProPricerElementDialog #ResourcesSelected'), '<%: (int)ProPricerField_Resources.BLANK %>');
            }
            else {
                ManageSystemProPricerExportsWidget.MoveSelectListItem($('#ExportToProPricerElementDialog #ResourcesUnselected option[value="' + resources[ndx] + '"]'), $('#ExportToProPricerElementDialog #ResourcesSelected'));
            }
        }
    };


    ManageSystemProPricerExportsWidget.SetElementFormReadOnly = function (readOnly) {
        if (readOnly != undefined && !readOnly) {
            $('#ExportToProPricerElementDialog #CopyFromRow').removeClass('display-none');
            $('#ExportToProPricerElementDialog span[name=Name]').addClass('display-none');
            $('#ExportToProPricerElementDialog input[name=Name]').removeClass('display-none');
            $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').show();
            $('#ExportToProPricerElementDialog #TasksSelected, #ExportToProPricerElementDialog #TasksUnselected, #ExportToProPricerElementDialog button').prop('disabled', false);
            $('#ExportToProPricerElementDialog #ResourcesSelected, #ExportToProPricerElementDialog #ResourcesUnselected').prop('disabled', false);
        }
        else {
            $('#ExportToProPricerElementDialog #CopyFromRow').addClass('display-none');
            $('#ExportToProPricerElementDialog input[name=Name]').addClass('display-none');
            $('#ExportToProPricerElementDialog span[name=Name]').html($('#ExportToProPricerElementDialog input[name=Name]').val()).removeClass('display-none');
            $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').hide();
            $('#ExportToProPricerElementDialog #TasksSelected, #ExportToProPricerElementDialog #TasksUnselected, #ExportToProPricerElementDialog button').prop('disabled', true);
            $('#ExportToProPricerElementDialog #ResourcesSelected, #ExportToProPricerElementDialog #ResourcesUnselected').prop('disabled', true);
        }
    }

    ManageSystemProPricerExportsWidget.ClearSelectedTasks = function () {
        ManageSystemProPricerExportsWidget.SelectAllItems($('#ExportToProPricerElementDialog #TasksSelected'));
        ManageSystemProPricerExportsWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #TasksSelected'), $('#ExportToProPricerElementDialog #TasksUnselected'));
        ManageSystemProPricerExportsWidget.SortSelectList($('#ExportToProPricerElementDialog #TasksUnselected'));
    };

    ManageSystemProPricerExportsWidget.ClearSelectedResources = function () {
        ManageSystemProPricerExportsWidget.SelectAllItems($('#ExportToProPricerElementDialog #ResourcesSelected'));
        ManageSystemProPricerExportsWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #ResourcesSelected'), $('#ExportToProPricerElementDialog #ResourcesUnselected'));
        ManageSystemProPricerExportsWidget.SortSelectList($('#ExportToProPricerElementDialog #ResourcesUnselected'));
    };

    ManageSystemProPricerExportsWidget.SaveExportFormat = function () {
        $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').addClass('display-none');
        $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Loader').removeClass('display-none');
        ManageSystemProPricerExportsWidget.clearValidationBox($('#ExportToProPricerElementForm ul.validation-box'));

        var dataToSend = {};
        dataToSend.ID = $('#ExportToProPricerElementDialog input[name=PKID]').val();
        dataToSend.Name = $('#ExportToProPricerElementDialog input[name=Name]').val();
        dataToSend.Scope = '<%: (int)ProPricerScope.System %>';
        dataToSend.UpdateDateLong = $('#ExportToProPricerElementDialog input[name=UpdateDateLong]').val().toString();
        dataToSend.Deleted = false;

        var listOrder = 0;

        dataToSend.Tasks = [];
        $('#ExportToProPricerElementDialog #TasksSelected').children('option').each(function () {
            var taskToSave = {};
            var taskValue = $(this).val();

            if (taskValue.indexOf(separatorField) != -1) {
                taskToSave.CustomFieldName = taskValue.split(separatorField)[0];
                taskToSave.Selection = taskValue.split(separatorField)[1];
            }
            else {
                taskToSave.Task = taskValue;
            }

            taskToSave.ListOrder = listOrder++;
            dataToSend.Tasks.push(taskToSave);
        });

        listOrder = 0;

        dataToSend.Resources = [];
        $('#ExportToProPricerElementDialog #ResourcesSelected').children('option').each(function () {
            var resourceToSave = {};
            var resourceValue = $(this).val();

            if (resourceValue.indexOf(separatorField) != -1) {
                resourceToSave.CustomFieldName = resourceValue.split(separatorField)[0];
                resourceToSave.Selection = resourceValue.split(separatorField)[1];
            }
            else {
                resourceToSave.Resource = $(this).val();
            }

            resourceToSave.ListOrder = listOrder++;
            dataToSend.Resources.push(resourceToSave);
        });

        dataToSend = JSON.stringify(dataToSend);

        ManageSystemProPricerExportsWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN %>',
                    '<%:WebConstants.ACTION_SAVE_PROPRICER_EXPORT_FORMAT %>', ''),
               contentType: 'application/json; charset=utf-8',
               dataType: 'json',
               data: dataToSend,
               success: function (response) {
                   ManageSystemProPricerExportsWidget.CloseDialog(ManageSystemProPricerExportsWidget.ExportToProPricerElementDialog);
                   ManageSystemProPricerExportsWidget.ReloadExportGrid();

                   $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Loader').addClass('display-none');
                   $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').removeClass('display-none');
               },
               error: function () {
                   $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Loader').addClass('display-none');
                   $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').removeClass('display-none');
               }
           }, $('#ExportToProPricerElementDialog-Save'));

    };

    ManageSystemProPricerExportsWidget.DeleteExportFormats = function () {
        Session.confirmDialog(
            'Delete Formats',
            'Are you sure you want to delete the selected formats?',
            function () { ManageSystemProPricerExportsWidget.DeleteSelectedExportFormats(); },
            null);
    };

    ManageSystemProPricerExportsWidget.DeleteSelectedExportFormats = function () {
        var dataToSend = [];

        $('#ExportToProPricer #ProPricerGridContent table tbody input[type=checkbox]:checked').each(function () {
            var parentRow = $(this).parents('tr');

            var exportFormatToSend = {};
            exportFormatToSend.ID = parentRow.attr('pkid');
            exportFormatToSend.Deleted = true;
            exportFormatToSend.UpdateDateLong = parentRow.find('input[name=UpdateDateLong]').val().toString();

            dataToSend.push(exportFormatToSend);
        });

        if (dataToSend.length > 0) {
            dataToSend = JSON.stringify(dataToSend);

            ManageSystemProPricerExportsWidget.ajaxRequest({
                type: 'POST',
                url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN %>',
                    '<%:WebConstants.ACTION_DELETE_PROPRICER_EXPORT_FORMATS %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function (response) {
                    ManageSystemProPricerExportsWidget.ReloadExportGrid();
                }
            });
        }
    };

    ManageSystemProPricerExportsWidget.ReloadExportGrid = function () {
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID %>', ''),
            dataType: 'html',
            success: function (response) {
                $('#ProPricerGridContent').html(response);
                $('#ExportToProPricer-Delete').addClass('disabled');
            }
        });
    };

    // Selects all options in a select list
    ManageSystemProPricerExportsWidget.SelectAllItems = function (list) {
        $(list).children('option').each(function () {
            $(this).prop('selected', true);
        });
    };

    // Sorts the items in a select list
    ManageSystemProPricerExportsWidget.SortSelectList = function sortSelect(list) {
        list = $(list);

        var optionArray = list.children('option');

        optionArray.sort(ManageSystemProPricerExportsWidget.CompareSelectListOptions);

        list.children().remove();

        for (var ndx = 0; ndx < optionArray.length; ndx++) {
            list.append(optionArray[ndx]);
        }

        return;
    };

    // Function to compare two Select List Option Items for equality
    ManageSystemProPricerExportsWidget.CompareSelectListOptions = function (a, b) {
        var aText = $(a).text().toLowerCase();
        var bText = $(b).text().toLowerCase();

        if (aText < bText)
            return -1;
        else if (aText > bText)
            return 1;
        else
            return 0;
    };

    // Adds a blank to a list
    ManageSystemProPricerExportsWidget.AddBlank = function (list, blankValue) {
        $(list).append('<option value="' + blankValue + '">Blank</option>');
    };

    // Moves selected list items from one list to another
    ManageSystemProPricerExportsWidget.MoveSelectedItems = function (fromList, toList) {
        $(fromList).children('option:selected').each(function () {
            ManageSystemProPricerExportsWidget.MoveSelectListItem(this, toList);
        });
    };

    ManageSystemProPricerExportsWidget.MoveSelectListItem = function (item, toList) {
        item = $(item);

        // Do not move required fields
        if (!item.attr('required')) {
            var value = item.val();

            // Do not MOVE blanks, just remove them from the fromList
            if (value == '<%: (int)ProPricerField_Task.BLANK %>' ||
                value == '<%: (int)ProPricerField_Resources.BLANK %>') {
                item.remove();
            }
            // Move all other fields
            else {
                $(toList).append(item);
            }
        }

        // Unselect each item
        item.prop('selected', false);
    };

    // Moves selected list items up in the list
    ManageSystemProPricerExportsWidget.ShiftSelectedItemsUp = function (list) {
        $(list).children('option:selected').each(function () {
            if ($(this).prev().length == 0) {
                return false;
            }

            $(this).prev().before($(this));
        });
    };

    //Moves selected list items down in the list
    ManageSystemProPricerExportsWidget.ShiftSelectedItemsDown = function (list) {
        $($(list).children('option:selected').get().reverse()).each(function () {
            if ($(this).next().length == 0) {
                return false;
            }

            $(this).next().after($(this));
        });
    };

    $(function () {

        ManageSystemProPricerExportsWidget.registerForEvent('MANAGE_SYSTEM_PROPRICER_EXPORTS_LOADED', function () {
             return true;
        });

        ManageSystemProPricerExportsWidget.Initialize();
        ManageSystemProPricerExportsWidget.BindEvents();


    });
    
</script>

<div id="ExportToProPricer" class="section">
    <div class="title">Manage System ProPricer Export Templates</div>
    <div class="data">
        <ul class="validation-box"> </ul>
        <br />
        <div class="buttons">
            <button class="ies-action disabled" name="delete-button" type="button" id="ExportToProPricer-Delete">Delete</button>
            <button class="ies-action" id="ExportToProPricer-Add" type="button">+ Add</button>
        </div>
        <div id="ProPricerGridContent" class="clear" >
            <% Html.RenderAction(
                    WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID,
                    WebConstants.CONTROLLER_ADMIN); %>
        </div>
    </div>
</div>
<div class="section">
    <div class="title">Manage System Custom Fields</div>
    <div class="data">
        <br />
        <div class="buttons">
            <button class="ies-action" id="CustomFieldSystemProPricer-Add" type="button">+ Add</button>
        </div>
        <div class="clear">
            <table id="SystemCustomFieldTable" class="readonly grid full-width">
                <thead>
                    <tr>
                        <th>Custom Field Name</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach(var customField in customFieldNames) { %>
                        <tr>
                            <td custom="<%: customField %>" class="inUse"><%: customField %></td>
                        </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>
</div>
<div id="AddCustomFieldDialog" class="display-none">
    <div class="container">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "AddCustomFieldForm" })){ %>
        <ul class="validation-box"> </ul>
        <ul id="EditWarning" class="warning-validation-box">
            <li>Note:  Editing a System Custom Field will change the Custom Field Name for all System ProPricer Exports.</li>
        </ul>
        <ul id="AddWarning" class="warning-validation-box">
            <li>Note:  New System Custom Fields are not permanently saved until they are selected in at least one System ProPricer Export.</li>
        </ul>
        <div class="form-row">
            <div class="form-label">Name*</div>
            <div class="form-element">
                <input type="text" id="NewCustomFieldName" name="CustomFieldName" required="required" class="full" />
                <input type="hidden" id="OriginalCustomFieldName" name="OriginalCustomFieldName" />
            </div>
        </div>
        <br />
        <div class="buttons right">
            <button id="AddCustomFieldDialog-Save" class="ies-action" name="save-button" type="button">Save</button>
            <div id="AddCustomFieldDialog-Loader" class="loader display-none"></div>
            <button id="AddCustomFieldDialog-Cancel" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
        <%} %>
    </div>
</div>
<div id="ExportToProPricerElementDialog" class="export-to-propricer-element-dialog display-none">
    <div class="container">
 
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ExportToProPricerElementForm" }))
           { %>
            <ul class="validation-box"> </ul>
            <div id="CopyFromRow" class="form-row display-none">
                <div class="form-label">
                    Copy from
                    <div id="ExportToProPricerElementDialog-CopyFromHelp" class="help-icon" style="margin-left: 0px;"
                        onclick="ManageSystemProPricerExportsWidget.ToggleHelp(this);">
                    </div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="ExportToProPricerElementDialog-CopyFromHelpDialog" class="help-dialog" style="width: 350px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Make a selection here to copy the export definition format from an already existing format.  The current settings will be replaced with those from the selected format.  The current settings will not be saved.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <select name="Copy" class="full" >
                    </select>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Name*</div>
                <div class="form-element">
                    <input type="text" name="Name" class="full" />
                    <span name="Name" class="display-none"></span>
                    <input type="hidden" name="PKID" value="-1" />
                    <input type="hidden" name="UpdateDateLong" value="12345" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Tasks</div>
                <div class="form-element"></div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    <%: Html.ListBox("TasksUnselected", (IEnumerable<SelectListItem>)ViewData["SelectList_Task_Unselected"], new { @class = "select-list" })%>
                    <div class="buttons inline-block centered">
                        <button id="TasksMoveToSelected" class="ies" type="button">&gt;</button><br /><br/>
                        <button id="TasksMoveToUnselected" class="ies" type="button">&lt;</button><br /><br />
                        <button id="TasksAddBlank" class="ies" type="button">Blank &gt;</button>
                    </div>
                    <%: Html.ListBox("TasksSelected", (IEnumerable<SelectListItem>)ViewData["SelectList_Task_Selected"], new { @class = "select-list" })%>
                    <div class="buttons inline-block centered">
                        <button id="TasksMoveItemsUp" class="ies move-button" type="button">Move Up</button><br /><br />
                        <button id="TasksMoveItemsDown" class="ies move-button" type="button">Move Down</button>
                    </div>
                </div>
                <div class="form-element"></div>
            </div>
            <div class="form-row">
                <div class="form-label">Resources</div>
                <div class="form-element"></div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    <%: Html.ListBox("ResourcesUnselected", (IEnumerable<SelectListItem>)ViewData["SelectList_Resources_Unselected"], new { @class = "select-list" })%>
                    <div class="buttons inline-block centered">
                        <button id="ResourcesMoveToSelected" class="ies" type="button">&gt;</button><br /><br/>
                        <button id="ResourcesMoveToUnselected" class="ies" type="button">&lt;</button><br /><br/>
                        <button id="ResourcesAddBlank" class="ies" type="button">Blank &gt;</button>
                    </div>
                    <%: Html.ListBox("ResourcesSelected", (IEnumerable<SelectListItem>)ViewData["SelectList_Resources_Selected"], new { @class = "select-list" })%>
                    <div class="buttons inline-block centered">
                        <button id="ResourcesMoveItemsUp" class="ies move-button" type="button">Move Up</button><br /><br />
                        <button id="ResourcesMoveItemsDown" class="ies move-button" type="button">Move Down</button>
                    </div>
                </div>
                <div class="form-element"></div>
            </div>
            <br />
            <div class="buttons">
                <button id="ExportToProPricerElementDialog-Save" class="ies-action disabled" name="save-button" type="button">Save</button>
                <div id="ExportToProPricerElementDialog-Loader" class="loader display-none"></div>
                <button id="ExportToProPricerElementDialog-Cancel" class="ies" name="cancel-button" type="button">Cancel</button>
            </div>
        <% } %>
    </div>
</div>
