<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.TaskElementDuplicateFormCollection>" %>

<script type="text/javascript">

    var DuplicateTaskWidget = new GridWidget('DupTask', '', '<%= ViewData["READONLY"] %>'.isTrue());

    //Save Duplicate Task Form
    DuplicateTaskWidget.SaveDuplicateTaskElements = function () {
        $('#DuplicateTaskElementDialog #DuplicateTaskElementDialogSave').addClass('display-none');
        $('#DuplicateTaskElementDialog #DuplicateTaskElementDialog-Loader').removeClass('display-none');
        DuplicateTaskWidget.clearValidationBox($('#DuplicateTaskValidationBox'));

        var dataToSend = {};
        dataToSend.DuplicateTaskRequests = [];

        $('#DuplicateTaskElementDialog #DuplicateTaskElementGridBody').children('tr').each(function () {
            var taskToSave = {};
            var taskCount = $(this).find('.duplicate-count').val();

            taskToSave.TaskID = $(this).find('.hidden-task-id').val();
            taskToSave.DuplicateCount = taskCount != "" ? taskCount : 0;
            dataToSend.DuplicateTaskRequests.push(taskToSave);
        });

        dataToSend = JSON.stringify(dataToSend);
        DuplicateTaskWidget.ajaxRequest({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                    '<%:WebConstants.CONTROLLER_BOE %>',
                                    '<%:WebConstants.ACTION_SAVE_DUPLICATE_TASK_ELEMENTS %>',
                                    'boe/<%: ViewData["BOEID"] %>/type/<%: Model.TaskType %>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {
                if (response.Status == false) {
                    DuplicateTaskWidget.processValidationErrors(response.Results.ValidationMessages, $('#DuplicateTaskElementDialogSave'));
                    $('#DuplicateTaskElementDialog #DuplicateTaskElementDialog-Loader').addClass('display-none');
                    $('#DuplicateTaskElementDialog #DuplicateTaskElementDialogSave').removeClass('display-none');
                }
                else {
                    $('#DuplicateTaskElementDialog').parent('div').dialog('destroy').empty();

                    if (typeof DuplicateTaskWidget.AfterSave == 'function') { DuplicateTaskWidget.AfterSave(); } //refresh appropriate task grid
                    $(document).trigger('<%: WebConstants.EVENT_BOESUMMARYGRID_RELOAD %>'); // recalculate our summary table
                }
            },
            error: function () {
                $('#DuplicateTaskElementDialog #DuplicateTaskElementDialog-Loader').addClass('display-none');
                $('#DuplicateTaskElementDialog #DuplicateTaskElementDialogSave').removeClass('display-none');
            }
        });
    };

    //Cancel Duplicate Task Dialog
    DuplicateTaskWidget.CancelDuplicateTaskElement = function () {
        //Remove the html from the parent div so there aren't multiple instances of dialog existing across tabs
        $('#DuplicateTaskElementDialog').parent('div').dialog('destroy').empty();
    };

    //Enable Save button
    DuplicateTaskWidget.EnableSaveDuplicateTaskElement = function () {
        $('#DuplicateTaskElementDialog #DuplicateTaskElementDialogSave').removeClass('disabled');
    };

    $(function () {
        SortableGrid('.duplicate-task-element-dialog');
        DuplicateTaskWidget.registerForLiveEvent('click',
            '#DuplicateTaskElementDialogSave:not(.disabled)', DuplicateTaskWidget.SaveDuplicateTaskElements);
    });

</script>

<div id="DuplicateTaskElementDialog" class="duplicate-task-element-dialog">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "DuplicateTaskElementForm" }))
       { %>
    <ul id="DuplicateTaskValidationBox" class="validation-box"></ul>
    <div class="container">
        <div class="form-row">
            <div class="form-label"></div>
            <div class="form-element">Enter a number between 1 and 9 in # Dups to duplicate a Task Element. The specified number of duplicate Task Elements will be created within the current BOE. Task Elements will be duplicated with blank Task IDs, and "COPY" prepended to the original Task Title.</div>
        </div>

        <div class="form-row duplicate-task-list">
            <table id="DuplicateTaskElementGrid" class="sortable grid readonly" style="width: 100%;">
                <colgroup>
                    <col width="8%" />
                    <col width="8%" />
                    <col width="84%" />
                </colgroup>
                <thead id="DuplicateTaskElementGridHeader">
                    <tr>
                        <th># Dups</th>
                        <th class="sort taskID">Task ID</th>
                        <th class="sort taskTitle">Task Title</th>
                    </tr>
                </thead>
                <tbody id="DuplicateTaskElementGridBody">
                    <%if (Model.DuplicateTaskRequests.Any())
                      {%>
                    <% foreach (var item in Model.DuplicateTaskRequests)
                       { %>
                    <tr>
                        <td>
                            <input class="duplicate-count" id="DuplicateCount" type="text" maxlength="1" value="0" onkeydown="DuplicateTaskWidget.EnableSaveDuplicateTaskElement()" /></td>
                        <td>
                            <div><%: item.BOETaskID %></div>
                            <%: Html.HiddenFor(model => item.TaskID, new { @class="hidden-task-id" }) %>
                        </td>
                        <td title="<%: Html.DisplayFor(model => item.TaskTitle) %>">
                            <%: item.TaskTitle %>
                        </td>
                    </tr>
                    <% } %>
                    <% }
                      else
                      {%>
                    <tr>
                        <td colspan="3">There are no Task Elements to duplicate.</td>
                    </tr>
                    <%} %>
                </tbody>
            </table>
        </div>
        <div class="form-row">
            <div class="form-element">Note: Must not contain any OCI, classified, export controlled or third party proprietary information.</div>
        </div>
        <div class="buttons">
            <button id="DuplicateTaskElementDialogSave" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="DuplicateTaskElementDialog-Loader" class="loader display-none"></div>
            <button id="DuplicateTaskElementDialog-Cancel" class="ies" onclick="DuplicateTaskWidget.CancelDuplicateTaskElement()" name="cancel-button" type="button">Cancel</button>
        </div>

    </div>
    <% } %>
</div>
