<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.ManageOutputFormatTemplatesModelView>" %>

<script  type="text/javascript">
    
    WorkspaceOutputFormatGridWidget = new Widget("WorkspaceOutputFormatGrid", <%: ViewData["READONLY"] %>);

    WorkspaceOutputFormatGridWidget.preparedForSubmit = function () {
        return true;
    }

    WorkspaceOutputFormatGridWidget.EditTemplate = function(templateId) {
        WorkspaceOutputFormatWidget.DisplayEditTemplate(templateId);
    };

    WorkspaceOutputFormatGridWidget.ViewTemplate = function(templateId) {
        // Remove the old hidden iFrame, if it exists
        $('#DownloadTarget-OutputFormatTemplate').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DownloadTarget-OutputFormatTemplate',
            'class': 'display-none',
            'src': CreateSystemAdminWithParmsPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_GET_OUTPUT_FORMAT_TEMPLATE %>',
                    templateId)
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    }

    WorkspaceOutputFormatGridWidget.ArchiveTemplate = function(templateId) {
        var row = $('#ManageOutputFormatTemplatesGrid tr[pkid="' + templateId + '"]');
        var updateDate = row.find('input[name=UpdateDateLong]');
        var archiveConfirmMessage = '<div style="text-align: left;">Are you sure you want to archive ' + row.find('.manage-output-format-name').text().trim() + '?</div>';

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>', 
                                                          '<%:WebConstants.ACTION_GET_WORKSPACES_USING_TEMPLATE%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({templateId:templateId}),
            success: function (response) {
                if (response.length == 0) {
                    archiveConfirmMessage += '</br><div style="text-align:left;">No workspaces are using the template.</div>';
                } else {
                    archiveConfirmMessage += '</br><div style="text-align:left;">Workspaces that are using the template will continue using it until another template is selected in that workspace\'s settings.</br></br>The following workspaces are using the template:</div><ul style="text-align: left;">';
                    for (var i = 0; i < response.length; i++) {
                        archiveConfirmMessage += "<li>" + response[i].Name + "</li>";
                    }
                    archiveConfirmMessage += "</ul>";
                }

                Session.confirmDialog("Archive Template", archiveConfirmMessage, function(){

                    $.ajax({
                        type: 'POST',
                        url: CreateSystemAdminPostURL('<%: WebConstants.CONTROLLER_ADMIN %>', '<%: WebConstants.ACTION_ARCHIVE_OUTPUT_FORMAT_TEMPLATE %>'),
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json',
                        data: JSON.stringify({ templateId:templateId, updateDate: updateDate.val().toString() }),
                        success: function () {
                            WorkspaceOutputFormatWidget.ReloadGridData();

                            // Show success notification
                            RaiseNotification('Archive successful');
                        }
                    });
                })
            }
        });
    };

    WorkspaceOutputFormatGridWidget.RestoreArchivedTemplate = function(templateId) {
        var row = $('#ManageOutputFormatTemplatesGrid tr[pkid="' + templateId + '"]');
        var updateDate = row.find('input[name=UpdateDateLong]');
        var restoreConfirmMessage = '<div style="text-align: left;">Are you sure you want to restore ' +
            row.find('.manage-output-format-name').text().trim() +
            '?</div>';

        Session.confirmDialog("Restore Template",
            restoreConfirmMessage,
            function() {
                $.ajax({
                    type: 'POST',
                    url: CreateSystemAdminPostURL('<%: WebConstants.CONTROLLER_ADMIN %>',
                        '<%: WebConstants.ACTION_RESTORE_OUTPUT_FORMAT_TEMPLATE %>'),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: JSON.stringify({ templateId: templateId, updateDate: updateDate.val().toString() }),
                    success: function() {
                        WorkspaceOutputFormatWidget.ReloadGridData(false);

                        // Show success notification
                        RaiseNotification('Restore successful');
                    }
                });
            });
    };

    WorkspaceOutputFormatGridWidget.displaySelectWorkspacesForTemplate = function (templateId, isAvailableToAll) {
        $("#SelectWorkspacesForTemplate").data('templateId', templateId).dialog({ 
            width: 625, modal: true, resizable: false, draggable: true, title: 'Select Workspaces for template',
            open: function() {
                $('#WorkspaceSelectionContainer').addClass('display-none');
                $('#WorkspaceSelectionLoader').removeClass('display-none');

                if (isAvailableToAll.toLowerCase() === 'true') {
                    $('#AvailableToAll').prop("checked", true);
                    $("#SelectWorkspaces").addClass("display-none disabled");
                } else {
                    $('#AvailableToAll').prop("checked", false);
                    $("#SelectWorkspaces").removeClass("display-none disabled");
                }

                $('#AvailableWorkspacesSelect').addClass('display-none');
                $('#AvailableWorkspacesSelectLoader').removeClass('display-none');
                $('#AssignedWorkspacesSelect').addClass('display-none');
                $('#AssignedWorkspacesSelectLoader').removeClass('display-none');

                var availableDone = false;
                var selectedDone = false;

                $.ajax({
                    type: 'POST',
                    url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>', 
                                                    '<%:WebConstants.ACTION_AVAILABLE_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE%>'),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: JSON.stringify({inExportTemplateId:templateId}),
                    success: function (response) {
                        $('#AvailableWorkspacesSelectLoader').addClass('display-none');
                        $('#AvailableWorkspacesSelect').empty(); // blank out previous options
                        $('#AvailableWorkspacesSelect').removeClass('display-none');
                        $.each(response, function(key, value) {
                            $('#AvailableWorkspacesSelect').append($('<option></option>').prop('value', value.AttrVal).text(value.ElVal));
                        });

                        WorkspaceOutputFormatGridWidget.ResizeMultiSelects();

                        availableDone = true;
                        if (selectedDone) {
                            $('#WorkspaceSelectionContainer').removeClass('display-none');
                            $('#WorkspaceSelectionLoader').addClass('display-none');
                        }
                    }
                });

                $.ajax({
                    type: 'POST',
                    url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>', 
                                                            '<%:WebConstants.ACTION_ASSIGNED_WORKSPACES_FOR_OUTPUT_FORMAT_TEMPLATE%>'),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: JSON.stringify({inExportTemplateId:templateId}),
                    success: function (response) {
                        $('#AssignedWorkspacesSelectLoader').addClass('display-none');
                        $('#AssignedWorkspacesSelect').empty(); // blank out previous options
                        $('#AssignedWorkspacesSelect').removeClass('display-none');
                        $.each(response, function(key, value) {
                            $('#AssignedWorkspacesSelect').append($('<option></option>').prop('value', value.AttrVal).text(value.ElVal));
                        });

                        WorkspaceOutputFormatGridWidget.ResizeMultiSelects();

                        selectedDone = true;
                        if (availableDone) {
                            $('#WorkspaceSelectionContainer').removeClass('display-none');
                            $('#WorkspaceSelectionLoader').addClass('display-none');
                        }
                    }
                });
            }
         });
    }

    WorkspaceOutputFormatGridWidget.ResizeMultiSelects = function() {
        var min = 15;

        var available = $('#AvailableWorkspacesSelect option').length;
        var assigned = $('#AssignedWorkspacesSelect option').length;

        if(available < min) { available = min; }
        if(assigned < min) { assigned = min; }

        $('#AvailableWorkspacesSelect').attr('size', available);
        $('#AssignedWorkspacesSelect').attr('size', assigned);

    }

    WorkspaceOutputFormatGridWidget.MoveAvailableWorkspaces = function (sourceSelect, targetSelect) {
        // take the option(s) selected and move them according to the source/target selects
        $(sourceSelect).prependTo($(targetSelect)).prop('selected', true);

        WorkspaceOutputFormatGridWidget.ResizeMultiSelects();
    }

    WorkspaceOutputFormatGridWidget.PerformSave = function () {
        $('#save-button-loader').removeClass('display-none');
        $('#save-button').addClass('display-none');

        var templateId = $("#SelectWorkspacesForTemplate").data('templateId');
        var availableToAll = $('#AvailableToAll').prop("checked");

        // take the ids from the options from the assigned workspace and pass them back to jquery
        var wsIdArray = [];
        $('#AssignedWorkspacesSelect [value]').each(function () {
            wsIdArray.push($(this).val());
        });

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>', 
                                            '<%:WebConstants.ACTION_SAVE_WORKSPACES_FOR_OUTPUT_TEMPLATES%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(
                            {
                                inTemplateId:templateId,
                                isAvailableToAllWorkspaces: availableToAll,
                                inUserSelectedAssignedWsIds:wsIdArray                                
                            }),
            success: function (response) {
                $('#save-button-loader').addClass('display-none');
                $('#save-button').removeClass('display-none');
                $("#SelectWorkspacesForTemplate").dialog('close');

                // Update onclick action in case Availble To All selected
                document.getElementById("SelectWorkspaces-" + templateId).setAttribute("onclick",
                    "WorkspaceOutputFormatGridWidget.displaySelectWorkspacesForTemplate('" +
                    templateId +
                    "', '" +
                    availableToAll +
                    "')");
            },
            error: function(response) {
                $('#save-button-loader').addClass('display-none');
                $('#save-button').removeClass('display-none');
            }
        });
    }

    $(function () {
        WorkspaceOutputFormatGridWidget.afterDOMLoad();

        WorkspaceOutputFormatGridWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { WorkspaceOutputFormatGridWidget.cleanDirty(); });

        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('#PageControls');
        pagingData.action = function(data) { $(document).trigger('PAGE_OUTPUT_FORMAT', data); }
        pagingData.type = "Arrow";

        WorkspaceOutputFormatGridWidget.AddPaging(pagingData);

        $('#AvailableToAll').change(function() {
            if (this.checked) {
                $("#SelectWorkspaces").addClass("display-none disabled");
            } else {
                $("#SelectWorkspaces").removeClass("display-none disabled");
            }
        });
    });

</script>

<div class="manage-output-format-templates-grid">
    <table id="ManageOutputFormatTemplatesGrid" class="grid readonly full">
        <thead>
            <tr>
                <th class="manage-output-format-name">Template</th>
                <th class="manage-output-format-description">Template Description</th>                                  
                <th class="manage-output-format-links last-child">&nbsp;<div id="PageControls"></div></th>                                  
            </tr>
        </thead>
        <tbody>
            <%  foreach (var item in Model.GridRows) { %>                                  
            <tr pkid="<%:item.TemplateId%>" replaceable="<%:item.IsReplaceable%>">
                <td class="manage-output-format-name" title="<%:item.TemplateName%>">
                    <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                    <%: item.TemplateName%>
                </td>
                <td class="manage-output-format-description" title="<%:item.TemplateDescription%>">
                    <%: item.TemplateDescription%>
                </td>
                <td class="manage-output-format-links">
                    <span>
                        <% if (item.IsActive)
                           { %>
                            <a onclick="WorkspaceOutputFormatGridWidget.EditTemplate('<%: item.TemplateId %>')">Edit</a> |
                            <a id="SelectWorkspaces-<%: item.TemplateId %>" onclick="WorkspaceOutputFormatGridWidget.displaySelectWorkspacesForTemplate('<%: item.TemplateId %>', '<%: item.IsAvailableToAllWorkspaces %>')" name="WorkspacesForTemplate">Select Workspaces</a> |
                            <a onclick="WorkspaceOutputFormatGridWidget.ViewTemplate('<%: item.TemplateId %>')" name="WorkspacesForTemplate">View template</a>
                            <% if (item.IsReplaceable)
                               { %>
                                | <a onclick="WorkspaceOutputFormatGridWidget.ArchiveTemplate('<%: item.TemplateId %>')">Archive</a>
                            <% }
                               else
                               { %>
                                <span style="color: white">| Archive</span>
                            <% }
                            %>
                        <% }
                        else
                        { %>
                            <a onclick="WorkspaceOutputFormatGridWidget.ViewTemplate('<%: item.TemplateId %>')" name="WorkspacesForTemplate">View template</a>
                            | <a onclick="WorkspaceOutputFormatGridWidget.RestoreArchivedTemplate('<%: item.TemplateId %>')">Restore</a>
                        <% } %>
                    </span>
                </td>
            </tr>
            <% } %>
        </tbody>
    </table>
</div>

<div style="display:none; width: auto; min-height: 400px; height: auto;" id="SelectWorkspacesForTemplate" class="ui-dialog-content ui-widget-content">
    <div class="manage-output-format-templates-grid-description">
        <div class="title">Edit the list of Workspaces that can access an output template by adding or removing the Workspaces from the list box.  Template will immediately be available to its assigned Workspaces.</div>
    </div>
    <div id="WorkspaceSelectionContainer">
        <div class="manage-output-format-templates-available-to-all">
            <label><input type="checkbox" id="AvailableToAll"/> Make available to all Workspaces</label>
        </div>
        <div id="SelectWorkspaces" class="manage-output-format-templates-selector-container">
            <div id="AvailableWorkspaces" class="manage-output-format-templates-selector-left-column float-left">
                <div>
                    <b>Workspaces available for template assignment</b><br />
                    (Closed and Complete Workspaces not shown)<br />
                </div>
                <div id="AvailableWorkspacesDiv">
                    <div id="AvailableWorkspacesSelectLoader" class="loader"></div>
                    <select id="AvailableWorkspacesSelect" multiple="multiple" size="20" class="manage-output-format-templates-selector-left-column display-none">
                    </select>
                </div>
            </div>
    
            <div id="SelectAndDeselectBoxes" class="manage-output-format-templates-selector-arrows float-left">
                <input type="button" value="&gt;" class="manage-output-format-templates-selector-arrows-moveright" onclick="WorkspaceOutputFormatGridWidget.MoveAvailableWorkspaces($('#AvailableWorkspacesSelect option:selected'), $('#AssignedWorkspacesSelect'))" /><br />
                <input type="button" value="&lt;" class="manage-output-format-templates-selector-arrows-moveleft" onclick="WorkspaceOutputFormatGridWidget.MoveAvailableWorkspaces($('#AssignedWorkspacesSelect option:selected'), $('#AvailableWorkspacesSelect'))" /><br />        
            </div>

            <div id="AssignedWorkspaces" class="manage-output-format-templates-selector-right-column float-right">
                <div>
                    <b>Workspaces assigned to this template</b><br />
                    (Closed and Complete Workspaces not shown)<br />
                </div>
                <div id="AssignedWorkspacesDiv">
                    <div id="AssignedWorkspacesSelectLoader" class="loader"></div>
                    <select id="AssignedWorkspacesSelect" multiple="multiple" size="20" class="manage-output-format-templates-selector-right-column display-none">
                    </select>
                </div>
            </div>
        </div>
        <br/>
        <div class="manage-output-format-templates-buttons">
            <div id="save-button-loader" class="loader display-none"></div>
            <button id="save-button" class="ies-action" onclick="WorkspaceOutputFormatGridWidget.PerformSave()" name="save-button" type="button">Save</button>
        </div>
    </div>
    <div id="WorkspaceSelectionLoader" class="loader"></div>
</div>

