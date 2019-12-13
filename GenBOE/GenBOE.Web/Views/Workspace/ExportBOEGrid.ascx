<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.Workspace.ExportBOEModelView>>" %>
<div class="export-boe-grid">
    <table id="ExportBOEGrid" class="grid readonly" width="620">
        <colgroup>
            <col width="4%" />
            <col width="30%" />
            <col width="21%" />
            <col width="25%" />
            <col width="20%" />
        </colgroup>
        <thead>
            <tr>
                <th class="boe-select"></th>
                <th class="wbs-title"><a>WBS</a></th>
                <th class="boe-title"><a>BOE Title</a></th>
                <th class="clin-title"><a>CLIN</a></th>
                <th class="status-value last-child"><a>Status</a></th>
            </tr>
       </thead>
       <tbody>
            <%  ulong count = 0;
                if (Model.Count() == 0)
                { %>
                <tr name="NoResultsRow">
                    <td colspan="5"><div class="empty-grid-text">There are no BOEs available for export.</div></td>
                </tr>
            <% }
            foreach (var item in Model) {
                bool isDisabled = item.State != BOEState.Draft || item.isMaterial;
                %>    
                <tr name="BOE" pkid="<%:item.BoeID%>" <% if (count++ % 2 == 0) { Response.Write(" class=\"trAlternate\""); } %>>
                    <td>
                        <input type="checkbox" <% if (isDisabled) { %>disabled="true" <%} %>/>
                    </td>                       
                    <td class="text wbs-title">
                        <a name="Wbs" link="BOE" title="<%= item.WBSText %>"><%: item.WBSText%></a>
                        <%: Html.Hidden("BOEID", item.BoeID)%>
                    </td>
                    <td class="text boe-title">
                        <a name="BOETitle" link="BOE" title="<%= item.BOETitle %>"><%: item.BOETitle %></a>
                    </td>
                    <td class="text clin-title">
                        <a name="Clin" link="BOE" title="<%= item.CLINText %>"><%: item.CLINText %></a>     
                    </td>                       
                    <td class="status-static">
                        <div name="Status"><%: item.Status %></div>
                    </td>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>