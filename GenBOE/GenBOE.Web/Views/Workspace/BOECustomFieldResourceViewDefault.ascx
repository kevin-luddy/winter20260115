<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.BOECustomFieldResourceModelView>>" %>

<script type="text/javascript">
    $(function () {
        $('#Close-DefaultList').click(function () { $(document).trigger('CLOSE_DEFAULT_RESOURCES'); });
    });    
</script>

<div class="default-resources">
    <table id="BOECustomFieldsResourceViewDefaultTable" class="grid readonly">
        <thead>
            <tr>                        
                <th class="id">ID</th>
                <th class="description">Description</th>
                <th class="segment-region">Segment/Region</th>
                <th class="labor-type">Labor Type</th>
                <th class="element-of-cost last-child">Element of Cost</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (BOECustomFieldResourceModelView item in Model) { %>
                <tr pkid="<%: item.CustomFieldOptionID %>">
                    <td class="id">
                        <%: item.ID %>
                    </td>
                    <td class="description">
                        <%: item.Description %>
                    </td>
                    <td class="segment-region"> 
                        <%: item.SegmentRegion %>
                    </td>
                    <td class="labor-type">
                        <%: item.LaborType %>
                    </td>
                    <td class="element-of-cost">
                        <%: item.ElementOfCostDisplay %>
                    </td>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>

<div class="buttons">
    <button id="Close-DefaultList" class="ies" name="close-button" type="button">Close</button>
</div>