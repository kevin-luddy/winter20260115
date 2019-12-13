<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.BOEHistoryModelView>>" %>

<script type="text/javascript">
    
    $(function () {
        createModule($('.boe-history.module'));
        refreshModule($('.boe-history.module'));
    });

</script>

<div class="boe-history module inner-collapsible">
    <div class="module-header-data"></div>
    <div class="module-content-data">
        <table id="BOEHistoryGrid" class="grid readonly">
            <thead>
                <tr>
                    <th class="field">Field</th>
                    <th class="description">Old Value</th>
                    <th class="description">New Value</th>
                    <th class="performed-by">Performed By</th>
                    <th class="date-time last-child">Date & Time</th>
                </tr>
            </thead>
            <tbody>
                <% foreach (var item in Model) { %>  
                <tr>
                    <td><span><%: item.Field %></span></td>
                    <td><span><%: item.OldValue %></span></td>
                    <td><span><%: item.NewValue %></span></td>
                    <td><span><%: item.PerformedBy %></span></td>
                    <td><span><%: Html.DisplayFor(i => item.Timestamp) %></span>
                    </td>
                </tr>
                <% } %>
            </tbody>
        </table>
    </div>
</div>