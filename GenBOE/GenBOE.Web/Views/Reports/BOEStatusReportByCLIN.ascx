<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Dtos.BOEStatusReportModelView>>" %>

<% var AllCLIN = (IEnumerable<GenBOE.Dtos.ClinDTO>)ViewData["AllCLIN"]; %>

<table id="BOEStatusReportGrid-CLIN"  class="grid readonly">
    <thead>
        <tr>
            <th class="clin">CLIN</th>
            <th class="boetitle">BOE Title</th>
            <th class="wbs">WBS</th>
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
            var boesGroupedByClin = from c in AllCLIN
                                    orderby c.ClinPaddedNumber
                                    select new
                                    {
                                        CLINTitle = c.ClinString,
                                        BOEs = (from b in Model
                                               where b.CLINNumber == c.ClinNumber
                                               select b).ToList()
                                    };

            string decimalPrecisionStringFormat = Model.Any() ? Model.First().DecimalPrecisionStringFormat : "F0";                       
            foreach (var clin in boesGroupedByClin) 
            {
                if (clin.BOEs.Any())
                {
                    string totalHours = clin.BOEs.Sum(c => c.TotalHours).ToString(decimalPrecisionStringFormat);
                    %>

            <tr>
                <td colspan="5" ><b><%: clin.CLINTitle %></b></td>
                <td colspan="1" title="<%:totalHours%>"><b><%:totalHours%></b></td>
                <td colspan="4"><b><%: clin.BOEs.Sum(c => c.TotalCost).ToString("#0.00")%></b></td>
            </tr>

            <%
                }
                
                foreach (var boe in clin.BOEs)
               { %>
                    <tr pkid="<%: boe.BOEID %>">
                        <td title="<%: clin.CLINTitle %>" style="padding-left: 20px;"><a><%: clin.CLINTitle %></a></td>
                        <td title="<%:boe.BOEID + " " + boe.BOETitle %>"><a><%:boe.BOETitle %></a></td>
                        <td title="<%: IES.Common.Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " ") %>"><a><%: IES.Common.Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " ") %></a></td>
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
