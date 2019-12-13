<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.NonzoneFeesAndCostsGridModelView>" %>

<script type="text/javascript">
    var ManageNonzoneFeesAndCostsGridWidget = new Widget('ManageNonzoneFeesAndCostsGrid', '<%= ViewData["READONLY"] %>'.isTrue());

    ManageNonzoneFeesAndCostsGridWidget.BindEvents = function () {
        $('#ManageNonzoneFeesAndCostsGrid tbody tr a').click(function () {
            $(document).trigger('EDIT_FEES_AND_COSTS', $(this).parents('tr'));
        });
    };

    $(function () {
        ManageNonzoneFeesAndCostsGridWidget.BindEvents();
    });
</script>

<div class="manage-nonzone-fees-and-costs-grid" style="overflow: auto; width:950px;">
    <table id="ManageNonzoneFeesAndCostsGrid" class="grid readonly" style="width: 950px">
        <thead>
            <tr>
                <th style="width: 1px"></th>
                <th class="mode" style="width: 200px">
                    <div>Mode</div>
                </th>
                <th class="travel-agency-fee" style="width: 250px">
                    <div>Travel Agency Fee</div>
                </th>
                <th class="misc-other" style="width:250px">
                    <div>Miscellaneous/Other</div>
                </th>
            </tr>
        </thead>
        <tbody>
            <% foreach (GenBOE.ActionLogic.ModelView.Admin.NonzoneFeesAndCostsModelView item in Model.NonzoneFeesAndCostsCollection)
            { %>
                <tr data-pkid="<%: ((int)item.Mode).ToString() %>">
                    <td style="text-overflow: clip">
                        <input type="hidden" value="<%: item.Mode.ToDescription() %>" class="Mode" />
                        <input type="hidden" value="<%: item.TravelAgencyFee %>" class="TravelAgencyFee" />
                        <input type="hidden" value="<%: item.MiscOther %>" class="MiscOther" />
                    </td>
                    <td title="<%: item.Mode.ToDescription() %>"><a><%: item.Mode.ToDescription() %></a></td>
                    <td title="<%: item.TravelAgencyFee %>"><a><%: item.TravelAgencyFee.ToString("N2") %></a></td>
                    <td title="<%: item.MiscOther %>"><% if (item.Mode != MSTTravelMode.NonZoneInternational) { %><a><%: item.MiscOther.ToString("N2") %></a><% } %></td>
                </tr>
            <%} %>
            <tr class="<%: Model.NonzoneFeesAndCostsCollection.Any() ? "display-none" : string.Empty %>">
                <td></td>
                <td colspan="3">
                    There are no modes.
                </td>
            </tr>
        </tbody>
    </table>
</div>
