<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<WhosOnlineGridModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%
        var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">
    
    $(function () { 
        WhosOnlineWidget.updatePagingData(<%= serializer.Serialize(Model) %>);
    });

</script>
<div id="WhosOnlineDivContainer">
    <div id="WhosOnlineDivContent"  class="whosonline-content">
         <table class="whosonline-table">
             <thead>
                 <tr class="whosonline-header-row">
                     <td>User Name</td>
                     <td>NT ID</td>
                     <td>Time Last Accessed</td>
                     <td>Time Since Last Accessed</td>
                 </tr>
             </thead>
             <% foreach (/*UserOnlineDetails*/var user in Model.UserResults)
                {
             %>        
             <tr class="whosonline-row">
                 <td><%: user.DisplayName%></td>
                 <td><%: user.Ntid %></td>
                 <td><%: user.TimeLastAccessed %></td>
                 <td><%: user.TimeSinceLastAccess %></td>
             </tr>       
             <%
                }       
             %>
         </table>
         <div class="whosonline-paging">
             <div class="paging-control"></div>
         </div>
    </div>
</div>
