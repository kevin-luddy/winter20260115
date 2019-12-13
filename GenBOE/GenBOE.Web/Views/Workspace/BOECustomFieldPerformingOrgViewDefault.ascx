<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.BOECustomFieldOptionModelView>>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>

<script type="text/javascript">
    $(function () {
        $('#Close-PerformingOrgDefaultList').click(function () { $(document).trigger('CLOSE_DEFAULT_PERFORMING_ORGS'); });
    });    
</script>

<div class="default-performing-organizations">
    <table id="BOECustomFieldsPerformingOrgViewDefaultTable" class="grid readonly">
        <thead>
            <tr>                        
                <th class="id">ID</th>
                <th class="description last-child">Description</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (BOECustomFieldOptionModelView item in Model) { %>
                <tr pkid="<%: item.CustomFieldOptionID %>">
                    <td class="id">
                        <%: item.ID %>
                    </td>
                    <td class="description">
                        <%: item.Description %>
                    </td>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>

<div class="buttons">
    <button id="Close-PerformingOrgDefaultList" class="ies" name="close-button" type="button">Close</button>
</div>