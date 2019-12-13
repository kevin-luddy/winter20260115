<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.BOETravelGridModelView>>" %>

<script type="text/javascript">
    var readOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var boeId = '<%: ViewData["BOEID"] %>';
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>'; 
    var boeZoneTravelController = '<%:WebConstants.CONTROLLER_BOE_ZONE_TRAVEL %>';
    var loadDuplicateZoneTravelTaskDialogUrl = CreatePostURL(workspace,
                                    '<%:WebConstants.CONTROLLER_BOE %>',
                                    '<%:WebConstants.ACTION_LOAD_DUPLICATE_TASK_DIALOG %>',
                                    'boe/' + boeId + '/type/<%: TaskType.Travel %>')
    var saveReorderZoneTravelTaskElementsUrl = CreatePostURL(workspace, boeZoneTravelController,'<%:WebConstants.ACTION_SAVE_REORDER_ZONE_TRAVEL_TASK_ELEMENTS %>', 'boe/' + boeId);

    var BOEZoneTravelWidget = InitializeBOEZoneTravelWidget(readOnly);

    $(function () {
        var deleteAllBOEZoneTravelUrl = CreatePostURL(workspace,boeZoneTravelController,'<%:WebConstants.ACTION_DELETE_ALL_BOE_ZONE_TRAVEL%>', 'boe/' + boeId);
        var boeSummaryGridReloadEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_RELOAD %>';
        var deleteBOEZoneTravelUrl = CreatePostURL(workspace, boeZoneTravelController, '<%:WebConstants.ACTION_DELETE_BOE_ZONE_TRAVEL%>', 'boe/' + boeId);

        AfterDomLoadBOEZoneTravelWidget(BOEZoneTravelWidget, deleteAllBOEZoneTravelUrl, boeSummaryGridReloadEvent, deleteBOEZoneTravelUrl);
    });
</script>

<div class="boe-ZONETRAVEL module inner-collapsible" id="MSTZoneTravelGridContainer">
    <div class="module-header-data"></div>
    <div class="module-content-data">
        <div class="buttons">
            <button id="boe-AddZoneTravelElement" name="add-task-element" class="ies-action" type="button">+ Add task element</button>
            <button id="ReOrderZoneTravelTaskElement" onclick="BOEZoneTravelWidget.displayReOrderZoneTravelTaskElementDialog()" class="ies" type="button">Sort tasks</button>
            <button id="DuplicateZoneTravelTask" onclick="BOEZoneTravelWidget.displayDuplicateZoneTravelTaskElementDialog()" class="ies" type="button">Duplicate tasks</button>
            <div id="DuplicateZoneTravelTaskElementDialog-Loader" class="loader display-none"></div>
        </div>
        <div class="ZoneTravelGrid">
            <table id="BOEZONETRAVELGrid" class="sortable grid readonly">
                <thead id="BOEZONETRAVELGridHeader">
                    <tr>
                        <th class="sort taskID" sorttype="number">Task ID</th>
                        <th class="sort taskTitle">Task Title</th>
                        <th class="sort taskStart">Task Start</th>
                        <th class="sort taskEnd">Task End</th>
                        <th class="sort taskTotalCost" sorttype="cost">Total Cost</th>
                        <th class="delete last-child">
                            <div class="delete"></div>
                        </th>
                    </tr>
                </thead>
                <tbody id="BOEZONETRAVELGridBody">
                    <% foreach (var item in Model)
                       { %>
                    <tr>
                        <td onclick="BOEZoneTravelWidget.expandZoneTravelElement('<%= item.TravelID %>')">
                            <a><%: item.TaskID %></a>
                            <%: Html.HiddenFor(model => item.TravelID, new { @class="hidden-zone-travel-id" }) %>
                        </td>
                        <td onclick="BOEZoneTravelWidget.expandZoneTravelElement('<%= item.TravelID %>')">
                            <a><%: item.TaskTitle %></a>
                        </td>
                        <td>
                            <%: item.TaskStartDate %>
                        </td>
                        <td>
                            <%: item.TaskEndDate %>
                        </td>
                        <td class="travelTotalCost">
                            <%: item.TotalCost %>
                        </td>
                        <td class="deleteColumn">
                            <div class="delete"></div>
                        </td>
                    </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>



    <div id="ReOrderZoneTravelTaskElementDialog" class="reorder-task-element-dialog" style="display: none;">
        <div class="container">
            <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ReOrderZoneTravelTaskElementForm" }))
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
                        <button id="TasksMoveItemsUp" class="ies move-button" style="margin-left: 7px;" type="button">Move Up</button>
                        <br /><br/>
                        <button id="TasksMoveItemsDown" class="ies move-button" style="margin-left: 7px;" type="button">Move Down</button>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-element">Note: Must not contain any OCI, classified, export controlled or third party proprietary information.</div>

            </div>
            <div class="buttons">
                <button id="ReOrderZoneTravelTaskElementDialog-Save" class="ies-action disabled" onclick=" BOEZoneTravelWidget.SaveReOrderTaskElements()" name="save-button" type="button">Save</button>
                <div id="ReOrderZoneTravelTaskElementDialog-Loader" class="loader display-none"></div>
                <button id="ReOrderZoneTravelTaskElementDialog-Cancel" class="ies" onclick="BOEZoneTravelWidget.CancelReOrderTaskElement()" name="cancel-button" type="button">Cancel</button>
            </div>
            <% } %>
        </div>
    </div>
    <div id="DuplicateZoneTravelTaskElementDialog" class="duplicate-task-element-dialog">
    </div>
</div>