<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Dtos.BOEStatusReportModelView>>" %>

<% var AllWBS = (IEnumerable<GenBOE.Dtos.WbsDTO>)ViewData["AllWBS"]; %>

<table id="BOEStatusReportGrid-WBS" class="grid readonly">
    <thead>
        <tr>
            <th class="wbs">WBS</th>
            <th class="boetitle">BOE Title</th>
            <th class="clin">CLIN</th>
            <th class="start-date" sortType="date">Start Date</th>
            <th class="end-date" sortType="date">End Date</th>
            <th class="total-hours" sortType="number">Total <%: ViewData["HoursLabel"]%></th>
            <th class="total-cost" sortType="number">Total Cost</th>
            <th class="author">Authors</th>
            <th class="approvers">Approvers</th>
            <th class="status last-child ">Status</th>
        </tr>
    </thead>
    <tbody>
        <%
            var boesGroupedByWBS = from w in AllWBS
                                    orderby w.WbsPaddedNumber
                                    select new
                                    {
                                        WBSTitle = w.WbsString,
                                        WBSLevel = w.Level,
                                        BOEs = (from b in Model
                                               where b.WBSNumber == w.WbsNumber
                                               select b).ToList()
                                    };
                                    
            string decimalPrecisionStringFormat = Model.Any() ? Model.First().DecimalPrecisionStringFormat : "F0"; 
            foreach (var wbs in boesGroupedByWBS)
            {

                if (wbs.BOEs.Any())
                {
                    string totalHours = wbs.BOEs.Sum(c => c.TotalHours).ToString(decimalPrecisionStringFormat);
                    %>

                    <tr>
                        <td colspan="5" style="font-weight: bold; padding-left: <%: wbs.WBSLevel * 10 %>px;"><%: wbs.WBSTitle%></td>
                        <td colspan="1" title="<%:totalHours%>"><b><%:totalHours%></b></td>
                        <td colspan="4"><b><%: wbs.BOEs.Sum(c => c.TotalCost).ToString("#0.00")%></b></td>
                    </tr>

                    <% 
                }
                
                foreach (var boe in wbs.BOEs)
                { %>
                    <tr pkid="<%: boe.BOEID %>">
                        <td title="<%:IES.Common.Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " ") %>" style="padding-left: <%: wbs.WBSLevel * 10 %>px;"><a><%:IES.Common.Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " ")%></a></td>
                        <td title="<%:boe.BOEID + " " + boe.BOETitle %>"><a><%:boe.BOETitle %></a></td>
                        <td title="<%:IES.Common.Utilities.FormatNumberTitleString(boe.CLINNumber, boe.CLINTitle, " ") %>"><a><%:IES.Common.Utilities.FormatNumberTitleString(boe.CLINNumber, boe.CLINTitle, " ") %></a></td>
                        <td><%:Html.DisplayFor(i => boe.StartDate)%></td>
                        <td><%:Html.DisplayFor(i => boe.EndDate)%></td>
                        <td title="<%:boe.TotalHoursFormatted %>"><%:boe.TotalHoursFormatted%></td>
                        <td title="<%:boe.TotalCost %>"><%:boe.TotalCost.ToString("#0.00")%></td>
                        <%  string authorList = string.Empty;
                            foreach (var author in boe.Authors) {
                                authorList += author + "\n";
                            }
                        %>
                        <td style="white-space: nowrap;" title="<%:authorList%>">
                            <% foreach (var author in boe.Authors)
                               { %>
                                <%: author%><br />
                            <% } %>
                        </td>
                        <%  string approverList = string.Empty;
                            foreach (var approver in boe.Approvers) {
                                approverList += approver + "\n";
                            }
                        %>
                        <td style="white-space: nowrap;" title="<%:approverList%>">
                            <% foreach (var approver in boe.Approvers)
                               { %>
                                <%: approver%><br />
                            <% } %>
                        </td>
                        <td title="<%:boe.Status %>"><%:boe.Status%></td>
                    </tr>
            <% }
           } %>
    </tbody>
</table>
