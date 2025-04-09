<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.BOEOtherDirectCostGridModelView>>" %>

<script type="text/javascript">
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var BOEOtherDirectCostWidget_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var boeIDUrlpart = 'boe/<%: ViewData["BOEID"] %>';
    var loadDuplicateODCTaskDialogUrl = CreatePostURL(workspace,
                                    '<%:WebConstants.CONTROLLER_BOE %>',
                                    '<%:WebConstants.ACTION_LOAD_DUPLICATE_TASK_DIALOG %>',
                                    boeIDUrlpart + '/type/<%: TaskType.ODC %>');
    var saveReOrderODCTaskElementsUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                    '<%:WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST %>',
                                    '<%:WebConstants.ACTION_SAVE_REORDER_BOE_OTHER_DIRECT_TASK_ELEMENTS %>',
                                    boeIDUrlpart);

    var BOEOtherDirectCostWidget = InitializeBOEOtherDirectCostWidget(BOEOtherDirectCostWidget_ReadOnly, loadDuplicateODCTaskDialogUrl, saveReOrderODCTaskElementsUrl);

    $(function () {
        var boeODCController = '<%:WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST%>';
        var summaryGridReloadEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_RELOAD %>';

        AfterDomLoadBOEOtherDirectCostWidget(BOEOtherDirectCostWidget, summaryGridReloadEvent);
    });

</script>

<div class="boe-ODC module inner-collapsible" id ="BOEOtherDirectCostGridContainer">
    <div class="module-header-data"></div>
    <div class="module-content-data">
        <table id="BOEOtherDirectCostGrid" class="sortable grid readonly">
            <thead id="BOEOtherDirectCostGridHeader">
                <tr>
                    <th class="sort taskID" sorttype="number" style="width: 175px">Task ID</th>
                    <th class="sort taskTitle" style="width: 400px">Task Title</th>
                    <th class="sort task-start" sorttype="date" style="width: 160px">Task Start</th>
                    <th class="sort task-end" sorttype="date" style="width: 160px">Task End</th>
                    <th class="delete last-child">
                        <div class="delete"></div>
                    </th>
                </tr>
            </thead>
            <tbody id="BOEOtherDirectCostGridBody">
                <% foreach (var item in Model)
                   { %>
                <tr>
                    <td onclick="BOEOtherDirectCostWidget.expandODCElement('<%= item.ODCID %>')">
                        <a style="width: 175px"><%: item.TaskID %></a>
                        <%: Html.HiddenFor(model => item.ODCID, new { @class="hidden-odc-id" }) %>
                    </td>
                    <td onclick="BOEOtherDirectCostWidget.expandODCElement('<%= item.ODCID %>')">
                        <a style="width: 400px"><%: item.TaskTitle %></a>
                    </td>
                    <td><a style="width: 160px"><%: Html.DisplayFor(model => item.StartDate) %></a></td>
                    <td><a style="width: 160px"><%: Html.DisplayFor(model => item.EndDate) %></a></td>
                    <td class="deleteColumn">
                        <div class="delete"></div>
                    </td>
                </tr>
                <% } %>
            </tbody>
        </table>
        <div class="buttons">
            <button id="boeODC-AddODCElement" class="ies-action" name="add-odc-element-button" type="button">+ Add ODC element</button>
            <button id="ReOrderODCTaskElement" onclick="BOEOtherDirectCostWidget.displayReOrderODCTaskElementDialog()" class="ies" type="button">Sort tasks</button>
            <button id="DuplicateOdcTask" onclick="BOEOtherDirectCostWidget.displayDuplicateOdcTaskElementDialog()" class="ies" type="button">Duplicate tasks</button>
            <div id="DuplicateOdcTaskElementDialog-Loader" class="loader display-none"></div>
        </div>
    </div>
    <div id="ReOrderODCTaskElementDialog" class="reorder-task-element-dialog" style="display: none;">
        <div class="container">
            <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ReOrderODCTaskElementForm" }))
               { %>
            <ul class="validation-box"></ul>
            <div class="form-row">
                <div class="form-label"></div>
                <div class="form-element">Sort task elements using the move buttons, select save when finished.  Multiple item selection is supported via Shift key for grouped items and Ctrl key for separated items.  The defined task order will be maintained when exporting the BOEs to MS Word.</div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    <%: Html.ListBox("TaskElementsList", (IEnumerable<SelectListItem>)ViewData["Order_Of_TaskElements"], new { @class = "select-list" })%>
                    <div class="buttons inline-block centered">
                        <button id="TasksMoveItemsUp" class="ies move-button" type="button">Move Up</button>
                        <br /><br />
                        <button id="TasksMoveItemsDown" class="ies move-button" type="button">Move Down</button>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-element">Note: <%: SiteMasterUtilities.GetBannerText() %></div>

            </div>
            <div class="buttons">
                <button id="ReOrderODCTaskElementDialog-Save" class="ies-action disabled" onclick=" BOEOtherDirectCostWidget.SaveReOrderTaskElements()" name="save-button" type="button">Save</button>
                <div id="ReOrderODCTaskElementDialog-Loader" class="loader display-none"></div>
                <button id="ReOrderODCTaskElementDialog-Cancel" class="ies" onclick="BOEOtherDirectCostWidget.CancelReOrderTaskElement()" name="cancel-button" type="button">Cancel</button>
            </div>
            <% } %>
        </div>
    </div>
    <div id="DuplicateOdcTaskElementDialog" class="duplicate-task-element-dialog">
    </div>
</div>


