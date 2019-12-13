<%@ Import Namespace="GenBOE.ActionLogic.ModelView.Workspace" %>
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<WorkspaceResourceRateGridTMModelView>" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>
<% var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }; %>

<script type="text/javascript">

    $(function () {
    
        ResourceRatesTMWidget.updatePagingData(<%= serializer.Serialize(Model) %>);

        // Edit Button Click.
        ResourceRatesTMWidget.getElement("a[name=ResourceName]").click(function () {
            var row = $(this).parents('tr');
            var id = $(row).attr('pkid');
            
            window.location.hash = '#AddResourceRateTM/rate/' + id;
        });
    });
</script>

<table id="WorkspaceResourceRateGrid" class="grid readonly" style="width: 100%">
    <thead>
        <tr>
            <th sortID="<%: WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_NAME %>">Resource ID</th>
            <th sortID="<%: WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_DESCRIPTION %>">Resource Description</th>
            <th sortID="<%: WorkspaceResourceRateTMModelView.SORT_ID_START_DATE %>">Start Date</th>
            <th sortID="<%: WorkspaceResourceRateTMModelView.SORT_ID_END_DATE %>">End Date</th>
            <th sortID="<%: WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_RATE %>" class="text-right last-child">Rate $</th>
        </tr>
    </thead>
    <tbody>

    <%if (Model.WorkspaceResourceRateTMResults.Count > 0)
      {%>

        <%foreach (WorkspaceResourceRateTMModelView item in Model.WorkspaceResourceRateTMResults)
          { %>
        <tr pkid="<%:item.ResourceRateID%>" <% if (item.InUse) { %>class="inUse"<% } %>> 
            <td>
                <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                <input type="hidden" name="ResourceID" value="<%: item.ResourceID %>" />
                <a name="ResourceName"><%:item.ResourceName %></a>
            </td>
            <td><%: item.ResourceDescription%></td>
            <td name="StartDate"><%: item.StartDate %></td>
            <td name="EndDate"><%: item.EndDate %></td>
            <td name="ResourceRate" class="text-right"><%:String.Format("{0:N}", item.ResourceRate) %></td>
        </tr>

        <%} %>

    <% }else{%>
        <tr><td colspan="7" >There are no Rates.</td></tr>
    <%} %>

    </tbody>
</table>