<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ExportToProPricerModelView>" %>
<%
    var enableSendDirectly = (bool)ViewData["EnableSendDirectly"];
    var enableProPricer = (bool)ViewData["EnableProPricer"];
%>
<script type="text/javascript">
    var ExportToProPricerWidget = new Widget('ExportToProPricer');
    var separatorField = '<%:ExportToProPricerModelView.CUSTOM_FIELD_ID_DESCRIPTION_SEPARATOR%>';

    ExportToProPricerWidget.Module = {};
    ExportToProPricerWidget.ExportToProPricerElementDialog = {};
    ExportToProPricerWidget.EnableSendDirectly = '<%=enableSendDirectly %>'.isTrue();

    ExportToProPricerWidget.Initialize = function () {
        ExportToProPricerWidget.Module = $('#ExportToProPricer');

        ExportToProPricerWidget.ExportToProPricerElementDialog.Element = $('#ExportToProPricerElementDialog');
        ExportToProPricerWidget.ExportToProPricerElementDialog.Params = { width: 680, modal: true, resizable: false, height: 640 };

        ExportToProPricerWidget.InitializeDialog(ExportToProPricerWidget.ExportToProPricerElementDialog);
        
        if (!ExportToProPricerWidget.EnableSendDirectly) {
            $("#SendDirectly").addClass("display-none");
        }
    }

    ExportToProPricerWidget.BoldRequiredFields = function () {
        $('#ExportToProPricerElementDialog #TasksSelected option, #ExportToProPricerElementDialog #TasksUnselected option').each(function () {
            var value = $(this).val();

            // Set required values
            if (value == '<%: (int)ProPricerField_Task.BOEStartDate %>' ||
                value == '<%: (int)ProPricerField_Task.BOEEndDate %>') {
                $(this).addClass('required').attr('required', 'true').text($(this).text() + '*');
            }
        });

        $('#ExportToProPricerElementDialog #ResourcesSelected option, #ExportToProPricerElementDialog #ResourcesUnselected option').each(function () {
            var value = $(this).val();

            // Set required values
            // Check against required enum values
            if (false) {
                $(this).addClass('required').attr('required', 'true').text($(this).text() + '*');
            }
        });
    };

    ExportToProPricerWidget.BindEvents = function () {
        // Object Events

        <% if (enableProPricer) { %>
			$('#ExportToProPricer-Add').click(ExportToProPricerWidget.AddNewExportFormat);
        <% } %>
        $('#ExportToProPricerElementDialog-Cancel').click(function () {
            ExportToProPricerWidget.CloseDialog(ExportToProPricerWidget.ExportToProPricerElementDialog);
        });

        $(' #ExportToProPricer-Delete:not(.disabled)').click(ExportToProPricerWidget.DeleteExportFormats);

        $('#ExportToProPricerElementDialog #TasksAddBlank:not(.disabled)').click(function () {
            ExportToProPricerWidget.AddBlank($('#ExportToProPricerElementDialog #TasksSelected'), '<%: (int)ProPricerField_Task.BLANK %>');
        });

        $('#ExportToProPricerElementDialog #TasksMoveToSelected').click(function () {
            ExportToProPricerWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #TasksUnselected'), $('#ExportToProPricerElementDialog #TasksSelected'));
        });

        $('#ExportToProPricerElementDialog #TasksMoveToUnselected').click(function () {
            ExportToProPricerWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #TasksSelected'), $('#ExportToProPricerElementDialog #TasksUnselected'));
            ExportToProPricerWidget.SortSelectList($('#ExportToProPricerElementDialog #TasksUnselected'));
        });

        $('#ExportToProPricerElementDialog #TasksMoveItemsUp').click(function () {
            ExportToProPricerWidget.ShiftSelectedItemsUp($('#ExportToProPricerElementDialog #TasksSelected'));
        });

        $('#ExportToProPricerElementDialog #TasksMoveItemsDown').click(function () {
            ExportToProPricerWidget.ShiftSelectedItemsDown($('#ExportToProPricerElementDialog #TasksSelected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesAddBlank:not(.disabled)').click(function () {
            ExportToProPricerWidget.AddBlank($('#ExportToProPricerElementDialog #ResourcesSelected'), '<%: (int)ProPricerField_Resources.BLANK %>');
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveToSelected').click(function () {
            ExportToProPricerWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #ResourcesUnselected'), $('#ExportToProPricerElementDialog #ResourcesSelected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveToUnselected').click(function () {
            ExportToProPricerWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #ResourcesSelected'), $('#ExportToProPricerElementDialog #ResourcesUnselected'));
            ExportToProPricerWidget.SortSelectList($('#ExportToProPricerElementDialog #ResourcesUnselected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveItemsUp').click(function () {
            ExportToProPricerWidget.ShiftSelectedItemsUp($('#ExportToProPricerElementDialog #ResourcesSelected'));
        });

        $('#ExportToProPricerElementDialog #ResourcesMoveItemsDown').click(function () {
            ExportToProPricerWidget.ShiftSelectedItemsDown($('#ExportToProPricerElementDialog #ResourcesSelected'));
        });

        ExportToProPricerWidget.registerForLiveEvent('click', '#ExportToProPricerElementDialog-Save:not(.disabled)', ExportToProPricerWidget.SaveExportFormat);
        ExportToProPricerWidget.registerForLiveEvent('click', '#ExportToProPricer-Delete:not(.disabled)', ExportToProPricerWidget.DeleteExportFormats);

        $('#ExportToProPricerElementDialog select[name=Copy]').change(function () {
            var rowToCopy = $('#ExportToProPricer #ProPricerGridContent table tbody tr[pkid=' + $(this).val() + ']');

            if (rowToCopy != undefined && rowToCopy.length) {
                ExportToProPricerWidget.CopyExportFormat(rowToCopy);
            }
            else {
                ExportToProPricerWidget.AddNewExportFormat();
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
            ExportToProPricerWidget.clearValidationBox($('#ExportToProPricerElementForm ul.validation-box'));
        });

        // Triggered Events
        ExportToProPricerWidget.registerForEvent('ENABLE_PROPRICER_DELETE_BUTTON', function (event, enable) {
            if (enable != undefined && enable == false) {
                $('#ExportToProPricer-Delete').addClass('disabled');
            }
            else {
                $('#ExportToProPricer-Delete').removeClass('disabled');
            }
        });

        ExportToProPricerWidget.registerForEvent('EDIT_PROPRICER_EXPORT_FORMAT', function (event, rowToEdit) {
            if (rowToEdit != undefined) {
                ExportToProPricerWidget.EditExportFormat(rowToEdit);
            }
        });

        // Set the formats available for copy on the Add/Edit ProPricer Export Format Dialog
        ExportToProPricerWidget.registerForEvent('SET_PROPRICER_COPY_FROM_FORMATS', function (event, availableFormatsJSON) {
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

        ExportToProPricerWidget.registerForEvent('EXPORT_PROPRICER_EXPORT_FORMAT', function (event, format) {
            if (format != undefined && format.formatID != undefined) {
                ExportToProPricerWidget.Export(format.formatID, format.formatName, format.scope);
            }
        });
    };

    // Opens the Format Element dialog with fields cleared and ready to create a new format
    ExportToProPricerWidget.AddNewExportFormat = function () {

        $('#ExportToProPricerElementDialog input[name=PKID]').val(-1);
        $('#ExportToProPricerElementDialog input[name=UpdateDateLong]').val('0');

        $('#ExportToProPricerElementDialog input[name=Name]').val('').keyup();

        $('#ScopeLabel').text('Workspace');

        $('#ExportToProPricerElementDialog select[name=Copy]').val(-1);


        ExportToProPricerWidget.ClearSelectedTasks();
        ExportToProPricerWidget.ClearSelectedResources();

        ExportToProPricerWidget.SetElementFormReadOnly(false);

        ExportToProPricerWidget.ChangeDialogTitle(ExportToProPricerWidget.ExportToProPricerElementDialog, 'Add ProPricer Export Format');
        ExportToProPricerWidget.OpenDialogAfterInitialize(ExportToProPricerWidget.ExportToProPricerElementDialog);
        $('#ExportToProPricerElementDialog #CopyFromRow').removeClass('display-none');
    };

    // Opens the Format Element dialog with fields set to the values for an existing format
    ExportToProPricerWidget.EditExportFormat = function (rowToEdit) {
        rowToEdit = $(rowToEdit);

        $('#ExportToProPricerElementDialog input[name=PKID]').val(rowToEdit.attr('pkid'));
        $('#ExportToProPricerElementDialog input[name=UpdateDateLong]').val(rowToEdit.find('input[name=UpdateDateLong]').val().toString());

        $('#ExportToProPricerElementDialog input[name=Name]').val(rowToEdit.find('input[name=Name]').val()).removeClass('display-none').keyup();

        if (rowToEdit.find('input[name=Scope]').val().toString() == '<%: (int)ProPricerScope.Workspace%>') {
            $('#ScopeLabel').text('Workspace');
        } else {
            $('#ScopeLabel').text('System');
        }

        $('#ExportToProPricerElementDialog select[name=Copy]').val(-1);
        
        // Clear Task and Resource selections
        ExportToProPricerWidget.ClearSelectedTasks();
        ExportToProPricerWidget.ClearSelectedResources();

        ExportToProPricerWidget.MoveSelectedRowItemsToSelectedBox(rowToEdit);

        if (rowToEdit.find('input[name=Scope]').val() == '<%= (int)ProPricerScope.System %>') {
            ExportToProPricerWidget.SetElementFormReadOnly();
        }
        else {
            ExportToProPricerWidget.SetElementFormReadOnly(false);
        }

        ExportToProPricerWidget.ChangeDialogTitle(ExportToProPricerWidget.ExportToProPricerElementDialog, 'Edit ProPricer Export Format');
        ExportToProPricerWidget.OpenDialogAfterInitialize(ExportToProPricerWidget.ExportToProPricerElementDialog);
        $('#ExportToProPricerElementDialog #CopyFromRow').addClass('display-none');
    };

    // Opens the Format Element dialog with fields set to the values for an existing format
    ExportToProPricerWidget.CopyExportFormat = function (rowToCopy) {
        rowToCopy = $(rowToCopy);

        var formatName = 'Copy of ' + rowToCopy.find('input[name=Name]').val();

        if (formatName.length > 100) {
            formatName = formatName.substring(0, 100);
        }

        $('#ExportToProPricerElementDialog input[name=Name]').val(formatName).removeClass('display-none').focusout().keyup();

        $('#ScopeLabel').text('Workspace');

        // Clear Task and Resource selections
        ExportToProPricerWidget.ClearSelectedTasks();
        ExportToProPricerWidget.ClearSelectedResources();

        ExportToProPricerWidget.MoveSelectedRowItemsToSelectedBox(rowToCopy);
    };

    ExportToProPricerWidget.MoveSelectedRowItemsToSelectedBox = function (tableRow) {
        // Move selected Tasks to Selected List
        var tasks = JSON.parse(tableRow.find('input[name=OrderedTasks]').val());

        for (var ndx in tasks) {
            if (tasks[ndx] == '<%: (int)ProPricerField_Task.BLANK %>') {
                ExportToProPricerWidget.AddBlank($('#ExportToProPricerElementDialog #TasksSelected'), '<%: (int)ProPricerField_Task.BLANK %>');
            }
            else {
                ExportToProPricerWidget.MoveSelectListItem($('#ExportToProPricerElementDialog #TasksUnselected option[value="' + tasks[ndx] + '"]'), $('#ExportToProPricerElementDialog #TasksSelected'));
            }
        }

        // Move selected Resources to Selected List
        var resources = JSON.parse(tableRow.find('input[name=OrderedResources]').val());

        for (var ndx in resources) {
            if (resources[ndx] == '<%: (int)ProPricerField_Resources.BLANK %>') {
                ExportToProPricerWidget.AddBlank($('#ExportToProPricerElementDialog #ResourcesSelected'), '<%: (int)ProPricerField_Resources.BLANK %>');
            }
            else {
                ExportToProPricerWidget.MoveSelectListItem($('#ExportToProPricerElementDialog #ResourcesUnselected option[value="' + resources[ndx] + '"]'), $('#ExportToProPricerElementDialog #ResourcesSelected'));
            }
        }
    };


    ExportToProPricerWidget.SetElementFormReadOnly = function (readOnly) {
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

    ExportToProPricerWidget.ClearSelectedTasks = function () {
        ExportToProPricerWidget.SelectAllItems($('#ExportToProPricerElementDialog #TasksSelected'));
        ExportToProPricerWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #TasksSelected'), $('#ExportToProPricerElementDialog #TasksUnselected'));
        ExportToProPricerWidget.SortSelectList($('#ExportToProPricerElementDialog #TasksUnselected'));
    };

    ExportToProPricerWidget.ClearSelectedResources = function () {
        ExportToProPricerWidget.SelectAllItems($('#ExportToProPricerElementDialog #ResourcesSelected'));
        ExportToProPricerWidget.MoveSelectedItems($('#ExportToProPricerElementDialog #ResourcesSelected'), $('#ExportToProPricerElementDialog #ResourcesUnselected'));
        ExportToProPricerWidget.SortSelectList($('#ExportToProPricerElementDialog #ResourcesUnselected'));
    };

    ExportToProPricerWidget.SaveExportFormat = function () {
            $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').addClass('display-none');
            $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Loader').removeClass('display-none');
            ExportToProPricerWidget.clearValidationBox($('#ExportToProPricerElementForm ul.validation-box'));

            var dataToSend = {};
            dataToSend.ID = $('#ExportToProPricerElementDialog input[name=PKID]').val();
            dataToSend.Name = $('#ExportToProPricerElementDialog input[name=Name]').val();
            dataToSend.Scope = $('#ExportToProPricerElementDialog input[name=Scope]').val();
            dataToSend.UpdateDateLong = $('#ExportToProPricerElementDialog input[name=UpdateDateLong]').val().toString();
            dataToSend.Deleted = false;

            var listOrder = 0;

            dataToSend.Tasks = [];
            $('#ExportToProPricerElementDialog #TasksSelected').children('option').each(function () {
                var taskToSave = {};
                var taskValue = $(this).val();

                if (taskValue.indexOf(separatorField) != -1) {
                    taskToSave.CustomFieldID = taskValue.split(separatorField)[0];
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
                    resourceToSave.CustomFieldID = resourceValue.split(separatorField)[0];
                    resourceToSave.Selection = resourceValue.split(separatorField)[1];
                }
                else {
                    resourceToSave.Resource = $(this).val();
                }

                resourceToSave.ListOrder = listOrder++;
                dataToSend.Resources.push(resourceToSave);
            });

            dataToSend = JSON.stringify(dataToSend);

           ExportToProPricerWidget.ajaxRequest({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                    '<%:WebConstants.CONTROLLER_REPORTS %>',
                                    '<%:WebConstants.ACTION_SAVE_PROPRICER_EXPORT_FORMAT %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function (response) {
                    ExportToProPricerWidget.CloseDialog(ExportToProPricerWidget.ExportToProPricerElementDialog);
                    ExportToProPricerWidget.ReloadExportGrid();

                    $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Loader').addClass('display-none');
                    $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').removeClass('display-none');
                },
                error: function () {
                    $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Loader').addClass('display-none');
                    $('#ExportToProPricerElementDialog #ExportToProPricerElementDialog-Save').removeClass('display-none');
                }
            }, $('#ExportToProPricerElementDialog-Save'));
  
    };

    ExportToProPricerWidget.DeleteExportFormats = function () {
        Session.confirmDialog(
            'Delete Formats',
            'Are you sure you want to delete the selected formats?',
            function () { ExportToProPricerWidget.DeleteSelectedExportFormats(); },
            null);
    };

    ExportToProPricerWidget.DeleteSelectedExportFormats = function () {
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

            ExportToProPricerWidget.ajaxRequest({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                    '<%:WebConstants.CONTROLLER_REPORTS %>',
                                    '<%:WebConstants.ACTION_DELETE_PROPRICER_EXPORT_FORMATS %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function (response) {
                    ExportToProPricerWidget.ReloadExportGrid();
                }
            });
        }
    };

    ExportToProPricerWidget.Export = function (formatID, formatName, scope) {
        <% if (!Utilities.DisablePiwik()) { %>
        if (piwikTracker3) {
            piwikTracker3.trackEvent('Export', 'ProPricer', formatName);
        }
        <% } %>
        GenSession.confirmDialog("Export Report", "The export is a long running process. <br/>Please do not leave this page until the file is available to open/save. <br/>Continue with this export? <br/><br/>Please refrain from clicking the export link multiple times until the download is complete.",
            function () {
                // Remove the old hidden iFrame, if it exists
                $('#ExportToProPricer-DownloadTarget').remove();

                // Create a new hidden iFrame and set it's source to the chosen report's URL
                var targetIFrame = $('<iframe />', {
                    'id': 'ExportToProPricer-DownloadTarget',
                    'class': 'display-none',
                    'src': CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                            '<%: WebConstants.CONTROLLER_REPORTS %>',
                            '<%: WebConstants.ACTION_EXPORT_PROPRICER_EXPORT_FORMAT %>',
                        'report/' + formatID + '/scope/' + scope)
                });

                // Append the iFrame to the body, causing the controller action to fire and
                // the download to occur inside the iFrame
                targetIFrame.appendTo('body');
            }, null);
    }

    ExportToProPricerWidget.ReloadExportGrid = function () {
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            '<%:WebConstants.CONTROLLER_REPORTS %>',
            '<%:WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID %>', ''),
            dataType: 'html',
            success: function (response) {
                $('#ProPricerGridContent').html(response);
                $('#ExportToProPricer-Delete').addClass('disabled');
                refreshModule(ExportToProPricerWidget.Module);

                $('#ProPricerGridContent').each(function () {
                    var content = $(this);
                    angular.element(document).injector().invoke(
                    [
                        "$compile", function ($compile) {
                            var scope = angular.element(content).scope();
                            $compile(content)(scope);
                        }
                    ]);
                });
            }
        });
    };

    // Selects all options in a select list
    ExportToProPricerWidget.SelectAllItems = function (list) {
        $(list).children('option').each(function () {
            $(this).prop('selected', true);
        });
    };

    // Sorts the items in a select list
    ExportToProPricerWidget.SortSelectList = function sortSelect(list) {
        list = $(list);

        var optionArray = list.children('option');

        optionArray.sort(ExportToProPricerWidget.CompareSelectListOptions);

        list.children().remove();

        for (var ndx = 0; ndx < optionArray.length; ndx++) {
            list.append(optionArray[ndx]);
        }

        return;
    };

    // Function to compare two Select List Option Items for equality
    ExportToProPricerWidget.CompareSelectListOptions = function (a, b) {
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
    ExportToProPricerWidget.AddBlank = function (list, blankValue) {
        $(list).append('<option value="' + blankValue + '">Blank</option>');
    };

    // Moves selected list items from one list to another
    ExportToProPricerWidget.MoveSelectedItems = function (fromList, toList) {
        $(fromList).children('option:selected').each(function () {
            ExportToProPricerWidget.MoveSelectListItem(this, toList);
        });
    };

    ExportToProPricerWidget.MoveSelectListItem = function (item, toList) {
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
    ExportToProPricerWidget.ShiftSelectedItemsUp = function (list) {
        $(list).children('option:selected').each(function () {
            if ($(this).prev().length == 0) {
                return false;
            }

            $(this).prev().before($(this));
        });
    };

    //Moves selected list items down in the list
    ExportToProPricerWidget.ShiftSelectedItemsDown = function (list) {
        $($(list).children('option:selected').get().reverse()).each(function () {
            if ($(this).next().length == 0) {
                return false;
            }

            $(this).next().after($(this));
        });
    };
    
    $(function () {
        ExportToProPricerWidget.Initialize();
        ExportToProPricerWidget.BindEvents();

        createModule(ExportToProPricerWidget.Module);
        refreshModule(ExportToProPricerWidget.Module);

        angular.bootstrap(document, ['genboe']);
    });
</script>

<div id="ExportToProPricer" class="export-to-propricer module">
    <div class="module-header-data">
        Export to ProPricer</div>
    <div class="module-content-data">
        <% if ((bool)ViewData["DisplayTravelLockedMessage"]) { %>
            <div class="bold"><%: ViewData["TravelMessage"]%></div><br />
        <% } else if ((bool)ViewData["DisplayTravelWarningMessage"]) { %>
            <div class="warning-box full export-to-pripricer-warning"><%: ViewData["TravelMessage"]%></div><br />
        <% } %>

        <ul class="validation-box"> </ul>
        <div>Export to ProPricer or manage the export format definitions.</div>
        <ul>
            <li>Click "Export Files" to generate export files for ProPricer.</li>
            <li id="SendDirectly">Click "Send Directly" to send the data to ProPricer directly.</li>
            <li>Click the format name to view or edit the format definition.</li>
            <li>Click +Add to add a new format definition.</li>
            <li>Select one or more formats and then click Delete to remove unneeded format definitions.</li>
        </ul>
        <br />
        <div class="buttons">
            <button class="ies-action disabled" name="delete-button" type="button" id="ExportToProPricer-Delete">Delete</button>
            <button class="ies-action" data-ng-class="{disabled: !enableProPricer }" id="ExportToProPricer-Add" type="button">+ Add</button>
        </div>
        <% if ((bool)ViewData["EnableProPricer"]) { %>
            <div id="ProPricerGridContent" class="clear" >
                <% Html.RenderAction(
                        WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER_GRID,
                        WebConstants.CONTROLLER_REPORTS,
                        new { workspace = SiteMasterUtilities.GetCurrentWorkspace() }); %>
            </div>
        <% } else { %>
            <ul class="validation-directive validation-box">
                <li class="ng-scope">
                    <div class="ng-binding">
                        There are Children objects (i.e. CLIN, BOE, Task, Resource) that are outside the PoP of their Parents, please open the Validate All BOEs report and fix before exporting
                    </div>
                </li>
            </ul>
        <% } %>
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
                        onclick="ExportToProPricerWidget.ToggleHelp(this);">
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
                <div class="form-label">Scope*</div>
                <div class="form-element">
                <input name="Scope" type="hidden" value="<%: (int)ProPricerScope.Workspace %>" />
                    <span id="ScopeLabel">Workspace</span>
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

<% Html.RenderAction(WebConstants.ACTION_DISPLAY_WORKSPACE_UPDATE_RATES_DIALOG, WebConstants.CONTROLLER_WORKSPACE, new { useCookie = false}); %>
