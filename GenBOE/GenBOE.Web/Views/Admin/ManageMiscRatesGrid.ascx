<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.ManageMiscRatesGridModelView>" %>

<script type="text/javascript">
    var ManageMiscRatesGridWidget = new Widget('ManageMiscRatesGrid', <%= ViewData["READONLY"] %>);

    // Updates grid data
    ManageMiscRatesGridWidget.UpdateGridData = function (input) {
        $(document).trigger('RATE_CHECKED');
    };

    ManageMiscRatesGridWidget.GetRowData = function(rowElement) {
        var data = {};
        data.MiscTravelRateID = rowElement.attr('pkid');
        data.MiscTravelRateMode = rowElement.find('a[name="MiscTravelRateMode"]').text();
        data.MiscTravelRate = rowElement.find('a[name="MiscTravelRate"]').text();
        data.SortCode = rowElement.find('a[name="SortCode"]').text();
        data.UpdateDateLong = rowElement.find('input[name="UpdateDateLong"]').val().toString();
        data.Deleted = rowElement.find('input[name="DeleteRate"]').prop('checked');
        return data;
    }

    ManageMiscRatesGridWidget.DeleteAllRecords = function ()
    {        
        // check all the boxes
        $('#ManageMiscRatesGrid tbody input[name="DeleteRate"]').prop('checked', $(this).prop('checked'));
        $(document).trigger('RATE_CHECKED');
    }

    $(function () {
        ManageMiscRatesGridWidget.afterDOMLoad();
        $("#ManageMiscRatesGrid tbody :input").change(function () { ManageMiscRatesGridWidget.UpdateGridData($(this)) });

        ManageMiscRatesGridWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageMiscRatesGridWidget.cleanDirty(); });

        $('#DeleteAllRates').click(ManageMiscRatesGridWidget.DeleteAllRecords);

        ManageMiscRatesGridWidget.registerForEvent('GET_DELETED_RATES', function () {

            var updateIndex = 0;

            ManageMiscRatesGridWidget.data.CheckedItems = [];
            $('#ManageMiscRatesGrid').find('tr input[name=DeleteRate]:checked').each(function() {
                ManageMiscRatesGridWidget.data.CheckedItems[updateIndex++] = ManageMiscRatesGridWidget.GetRowData($(this).closest('tr'));
            });

            $(document).trigger('DELETE_RATES', ManageMiscRatesGridWidget.data); 
        });

        $('#ManageMiscRatesGrid td.edit-rate-link').click(function() {
            var rowElement = $(this).parents('tr');
            $(document).trigger('EDIT_RATE', ManageMiscRatesGridWidget.GetRowData(rowElement)); 
        });

        SortableGrid('.manage-misc-rates-grid');

        refreshModule($('.manage-misc-rates-grid'));

    });

</script>

<div class="manage-misc-rates-grid">
        <table id="ManageMiscRatesGrid" class="sortable grid readonly">
            <thead>
            <tr>
                <th class="delete-checkbox"><input type="checkbox" id="DeleteAllRates" /></th>                   
                <th class="sort transportation-mode" style="width:350px">Transportation Mode</th>
                <th class="sort miscRate" sortType="number" style="width:200px">Miscellaneous Rate</th>
                <th class="sort sort-code" sortType="number" style="width:100px">Sort Code</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (MiscRateModelView item in Model.MiscRateModelView) { %>
                <tr pkid="<%: item.MiscTravelRateID %>">
                    <% if (item.inUse == false)
                       { %>
                    <td class="delete-checkbox">
                        <input type="checkbox" name="DeleteRate" />
                    </td>
                    <%}
                       else
                       { %>
                    <td>
                        <div id="InUse">In use</div>
                    </td>

                    <%} %>
                    <td class="transportation-mode edit-rate-link">
                        <a name="MiscTravelRateMode" style="white-space:pre-wrap;"><%: item.MiscTravelRateMode %></a>
                        <input type="hidden" name="MiscTravelRateMode" value="<%: item.MiscTravelRateMode %>" />
                    </td>
                    <td class="miscRate edit-rate-link text-right" style="padding-right:100px;">
                        <a name="MiscTravelRate"><%: item.MiscTravelRateTwoDecimals%></a>
                    </td>
                    <td class="sort-code edit-rate-link">
                        <a name="SortCode"><%: item.SortCode %></a>
                        <input type="hidden" name="SortCode" value="<%: item.SortCode %>" />
                    </td>
                </tr>
            <% } %>
        </tbody>
        </table>
</div>