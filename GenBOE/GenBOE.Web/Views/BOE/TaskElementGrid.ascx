<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.GenericTaskElementGridModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>

<% 
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    bool missingBrcCodes = Html.GetViewDataValue < bool > ("MissingBrcCodes", false);
%>
<script type="text/javascript">

    var readOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var loadedData = <%= serializer.Serialize(Model.TaskElements) %>;
    var boeId = '<%: ViewData["BOEID"] %>';
    var boeController = '<%:WebConstants.CONTROLLER_BOE %>';
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var duplicateTaskDialogUrl = CreatePostURL(workspace, boeController, '<%:WebConstants.ACTION_LOAD_DUPLICATE_TASK_DIALOG %>', 'boe/' + boeId + '/type/<%: TaskType.Labor %>');
    var saveReorderTaskElementUrl = CreatePostURL(workspace, boeController, '<%:WebConstants.ACTION_SAVE_REORDER_LABOR_TASK_ELEMENTS %>', 'boe/' + boeId);

    var TaskElementGrid = InitializeTaskElementGrid(readOnly, loadedData, duplicateTaskDialogUrl, saveReorderTaskElementUrl);

    $(function () {
        
        var deleteBoeTaskElementUrl = CreatePostURL(workspace, boeController,'<%: Model.DeleteAction %>', 'boe/' + boeId);
        var deleteAllBoeTaskElementsUrl = CreatePostURL(workspace, boeController,'<%: WebConstants.ACTION_DELETE_ALL_BOE_TASK_ELEMENTS %>', 'boe/' + boeId);
        var laborTaskElementType = <%: (int)TaskElementType.Labor %>;

        AfterDomLoadTaskElementGrid(TaskElementGrid, deleteBoeTaskElementUrl, deleteAllBoeTaskElementsUrl, laborTaskElementType);
    });
</script>

<div class="task-element-grid module inner-collapsible" id="TaskElementGridContainer">
    <div class="module-header-data"></div>
    <div class="module-content-data">
        <%if (Model.TaskElements.Any(x => x.FailedValidation)) { %>
            <div class="validation-box" style="display:block;">
                Tasks highlighted in red contain data or date errors.  Please review these tasks for accuracy.
            </div>
        <% } %>
        <div class="buttons">
            <%if (missingBrcCodes) { %>
                <button id="AddTaskElement" name="add-task-element" class="ies-action" disabled="disabled" type="button">+ Add task element</button>
            <% } %>
            <%else { %>
                <button id="AddTaskElement" name="add-task-element" class="ies-action" type="button">+ Add task element</button>
            <% } %>
            <button id="ReOrderTaskElement" onclick="TaskElementGrid.displayReOrderTaskElementDialog()" class="ies" type="button">Sort tasks</button>
            <%if (missingBrcCodes) { %>
                <button id="DuplicateTask" disabled="disabled" class="ies" type="button">Duplicate tasks</button>
            <% } %>
            <%else { %>
                <button id="DuplicateTask" onclick="TaskElementGrid.displayDuplicateTaskElementDialog()" class="ies" type="button">Duplicate tasks</button>
            <% } %>
            <div id="DuplicateTaskElementDialog-Loader" class="loader display-none"></div>
        </div>
        <table id="TaskElementGrid" class="sortable grid readonly">
            <thead id="TaskElementGridHeader">
                <tr>
                    <th class="sort task-id">Task ID</th>
                    <th class="sort task-title">Task Title</th>
                    <th class="sort task-start" sorttype="date">Task Start</th>
                    <th class="sort task-end" sorttype="date">Task End</th>
                    <th class="sort total-hours" sorttype="number">Total <%: ViewData["HoursLabel"] %></th>
				<%if (Model.IsUCOTEnabledForWorkspace) { %>
					<th class="sort total-hours" sorttype="number">Total UCOT Hours</th>
					<th class="sort total-hours" sorttype="number">Grand Total Hours</th>
				<% } %>
                    <th class="sort total-cost" sorttype="number">Total Cost</th>
                    <th class="delete last-child">
                        <div class="delete"></div>
                    </th>
                </tr>
            </thead>
            <tbody id="TaskElementGridBody">
                <%  foreach (var item in Model.TaskElements)
                    {
                        string styleDueToValidation = item.FailedValidation ? "background-color: #FFB2B2; font-weight: bold; font-style: italic;" : string.Empty;
                %>

                <tr pkid="<%= item.TaskElementDetailID %>" style="<%: styleDueToValidation %>">
                    <td class="element-link" title="<%: Html.DisplayFor(model => item.TaskID) %>" >
                        <a><%: Html.DisplayFor(model => item.TaskID) %></a>
                    </td>
                    <td class="element-link" title="<%: Html.DisplayFor(model => item.Title) %>">
                        <a><%: Html.DisplayFor(model => item.Title) %></a></td>
                    <td><%: Html.DisplayFor(model => item.StartDate) %></td>
                    <td><%: Html.DisplayFor(model => item.EndDate) %></td>
                    <td><%:item.TotalHoursFormatted %></td>
				<%if (Model.IsUCOTEnabledForWorkspace) { %>
					<td><%:item.TotalUCOTHoursFormatted %></td>
					<td><%:item.TotalHoursWithUCOTFormatted %></td>
				<% } %>
                    <td>$<%: Html.DisplayFor(model => item.TotalCost)%></td>
                    <td class="deleteColumn">
                        <div class="delete"></div>
                    </td>
                </tr>
                <% } %>
            </tbody>
        </table>
    </div>
    <div id="ReOrderTaskElementDialog" class="reorder-task-element-dialog" style="display: none;">
    <div class="container">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ReOrderTaskElementForm" }))
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
                        <button id="TasksMoveItemsUp" style="margin-left: 7px;" class="ies move-button" type="button">Move Up</button>
                        <br /><br/>
                        <button id="TasksMoveItemsDown" style="margin-left: 7px;" class="ies move-button" type="button">Move Down</button>
                    </div>
                </div>
                </div>
                  <div class="form-row">
                <div class="form-element">Note: <%: SiteMasterUtilities.GetBannerText(Model.ContainsOCI) %></div>
            </div>
            <div class="buttons">
                <button id="ReOrderTaskElementDialog-Save" class="ies-action disabled" onclick="TaskElementGrid.SaveReOrderTaskElements()" name="save-button" type="button">Save</button>
                <div id="ReOrderTaskElementDialog-Loader" class="loader display-none"></div>
                <button id="ReOrderTaskElementDialog-Cancel" class="ies" onclick="TaskElementGrid.CancelReOrderTaskElement()" name="cancel-button" type="button">Cancel</button>
            </div>
         <% } %>
        </div>
    </div>
    <div id="DuplicateLaborTaskElementDialog" class="duplicate-task-element-dialog">
    </div>
</div>

