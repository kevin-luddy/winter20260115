<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Dtos.BOEStatusReportModelView>>" %>

<% bool IsUCOTEnabledForWorkspace = (bool)ViewData["IsUCOTEnabledForWorkspace"]; %>

<table id="BOEStatusReportGrid-BOE"  class="sortable grid readonly">
    <thead>
        <tr>
            <th class="sort wbs">WBS</th>
            <th class="sort boetitle">BOE Title</th>
            <th class="sort clin">CLIN</th>
            <th class="sort start-date" sortType="date">Start Date</th>
            <th class="sort end-date" sortType="date">End Date</th>
            <th class="sort total-hours" sortType="number">Total <%: ViewData["HoursLabel"]%></th>
			<% if (IsUCOTEnabledForWorkspace) { %>
				<th class="sort total-hours" sortType="number">Total UCOT <%: ViewData["HoursLabel"]%></th>
				<th class="sort total-hours" sortType="number">Grand Total <%: ViewData["HoursLabel"]%></th>
			<% } %>
            <th class="sort total-cost" sortType="number">Total Cost</th>
            <th class="sort author">Authors</th>
            <th class="sort approvers">Approvers</th>
            <th class="sort status last-child ">Status</th>
        </tr>
    </thead>
    <tbody>
        <%  foreach (var item in Model) { %>
        <tr pkid="<%:item.BOEID %>">
            <td title="<%:IES.Common.Utilities.FormatNumberTitleString(item.WBSNumber, item.WBSTitle, " ") %>"><a><%:IES.Common.Utilities.FormatNumberTitleString(item.WBSNumber, item.WBSTitle, " ") %></a></td>
            <td title="<%:item.BOEID + " " + item.BOETitle %>"><a><%:item.BOETitle %></a></td>
            <td title="<%:IES.Common.Utilities.FormatNumberTitleString(item.CLINNumber, item.CLINTitle, " ") %>"><a><%:IES.Common.Utilities.FormatNumberTitleString(item.CLINNumber, item.CLINTitle, " ") %></a></td>
            <td><%:Html.DisplayFor(i => item.StartDate) %></td>
            <td><%:Html.DisplayFor(i => item.EndDate) %></td>
            <td title="<%:item.TotalHoursFormatted %>"><%:item.TotalHoursFormatted %></td>
            <% if (IsUCOTEnabledForWorkspace) { %>
				<td title="<%:item.TotalUCOTHoursFormatted %>"><%:item.TotalUCOTHoursFormatted %></td>
				<td title="<%:item.TotalHoursWithUCOTFormatted %>"><%:item.TotalHoursWithUCOTFormatted %></td>
			<% } %>
			<td title="<%:item.TotalCost%>"><%:item.TotalCost.ToString("#0.00")%></td>
            <%  string authorList = string.Empty;
                foreach (var author in item.Authors) {
                    authorList += author + "\n";
                }
            %>
            <td style="white-space: nowrap;" title="<%:authorList%>">
                <% foreach (var author in item.Authors)
                    { %>
                    <%: author%><br />
                <% } %>
            </td>
            <%  string approverList = string.Empty;
                foreach (var approver in item.Approvers) {
                    approverList += approver + "\n";
                }
            %>
            <td style="white-space: nowrap;" title="<%:approverList%>">
                <% foreach (var approver in item.Approvers)
                    { %>
                    <%: approver %><br />
                <% } %>
            </td>
            <td title="<%:item.Status %>"><%:item.Status %></td>
        </tr>
        <% } %>
    </tbody>
</table>
