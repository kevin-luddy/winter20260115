<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ICollection<GenBOE.Dtos.SikorskyLegacyResourceDTO>>" %>

<div id="LegacyResources" class="manage-default-resources section">
    <div class="title">View Legacy Resources</div>
    <div class="data">
        <table id="ManageLegacyResources" class="sortable grid readonly">
            <thead>
                <tr>
                    <th style="width:200px">Legacy Resource</th>
                    <th style="width:500px">Legacy Resource Name</th>
                </tr>
            </thead>
            <tbody>
                <% foreach (GenBOE.Dtos.SikorskyLegacyResourceDTO item in Model) { %>
                    <tr>
                        <td><%: item.LegacyResourceID %></td>
                        <td><%: item.LegacyResourceName %></td>
                    </tr>
                <% } %>
            </tbody>
        </table>
    </div>
</div>