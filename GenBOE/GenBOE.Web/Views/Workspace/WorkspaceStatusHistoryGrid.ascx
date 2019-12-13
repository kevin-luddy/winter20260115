<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Dtos.WorkspaceStatusHistoryModelView>>" %>

<script type="text/javascript">

    var WorkspaceStatusHistoryGrid = new GridWidget("WorkspaceStatusHistoryGrid");

    $(function () {

        $('#WorkspaceStatusHistoryGrid-Back').click(function () {
            window.location.hash = '#';
        });

        createModule($('#WorkspaceStatusHistoryGrid'));
        refreshModule($('#WorkspaceStatusHistoryGrid'));
    });

</script>

<div id="WorkspaceStatusHistoryGrid" class="module">
    <div class="module-header-data">Workspace Status History</div>
    <div class="module-content-data">
        <div class="workspace-status-history-grid">
                <button id="WorkspaceStatusHistoryGrid-Back" class="ies back-to-workspace-settings-button" type="button">Back to Workspace Settings</button>
            <table class="grid readonly">
                <thead>
                    <tr>
                        <th class="description">Old Value</th>
                        <th class="description">New Value</th>
                        <th class="performed-by">Performed By</th>
                        <th class="date-time last-child">Date & Time</th>
                    </tr>
                </thead>
                <tbody>
                    <%  foreach (var item in Model) { %>
                    <tr>
                        <td><%:item.OldValue %></td>
                        <td><%:item.NewValue %></td>
                        <td><%:item.PerformedBy %></td>
                        <td><%:Html.DisplayFor(i => item.Date) %></td>
                    </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>
</div>
