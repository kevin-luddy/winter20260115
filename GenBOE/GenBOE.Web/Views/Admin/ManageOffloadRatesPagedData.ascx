<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PagedSystemOffloadRateModelView>" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>
<%
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">
    var ManageOffloadRatesGridWidget = new Widget('ManageOffloadRatesGrid', <%= ViewData["READONLY"] %>);

    // Updates grid data
    ManageOffloadRatesGridWidget.UpdateGridData = function (input) {
        $(document).trigger('RATE_CHECKED');

        var rowElement = $(input).parents('tr');
        var ElementID = rowElement.attr('pkid');

        if (ManageOffloadRatesGridWidget.data.CheckedItems == undefined) {
            ManageOffloadRatesGridWidget.data.CheckedItems = new Array();
        }

        var updateIndex = -1;

        //First see if you have already updated this data.     
        for (var k in ManageOffloadRatesGridWidget.data.CheckedItems) {
            if (ManageOffloadRatesGridWidget.data.CheckedItems[k]['OffloadRateID'] == ElementID) {
                updateIndex = k;
                break;
            }
        }

        if (updateIndex == -1) {
            updateIndex = ManageOffloadRatesGridWidget.data.CheckedItems.length;
            ManageOffloadRatesGridWidget.data.CheckedItems[updateIndex] = {};
        }

        ManageOffloadRatesGridWidget.data.CheckedItems[updateIndex] = ManageOffloadRatesGridWidget.GetRowData(rowElement);
    };

    ManageOffloadRatesGridWidget.GetRowData = function(rowElement) {
        var data = {};
        data.OffloadRateID = rowElement.attr('pkid');
        data.Resource = rowElement.find('a[name="Resource"]').text();
        data.SubcontractorResource = rowElement.find('a[name="SubcontractorResource"]').text();
        data.Year = rowElement.find('a[name="Year"]').text();
        data.PerformingOrg = rowElement.find('a[name="PerformingOrg"]').text();
        data.PercentToOffload = rowElement.find('input[name="PercentToOffload"]').val();
        data.HourlyRate = rowElement.find('input[name="HourlyRate"]').val();

        data.UpdateDateLong = rowElement.find('input[name="UpdateDateLong"]').val().toString();
        data.Deleted = rowElement.find('input[name="DeleteRate"]').prop('checked');
        return data;
    }

    ManageOffloadRatesGridWidget.DeleteAllRecords = function ()
    {        
        // check all the boxes
        $('#ManageOffloadRatesGrid tbody input[name="DeleteRate"]').prop('checked', $(this).prop('checked'));

        // add all of the rows data to the widget.
        $('#ManageOffloadRatesGrid input[name="DeleteRate"]').each(function() {
            ManageOffloadRatesGridWidget.UpdateGridData($(this));
        });
    }

    $(function () {
        ManageOffloadRatesWidget.updatePagingData(<%= serializer.Serialize(Model) %>);

        ManageOffloadRatesGridWidget.afterDOMLoad();
        $("#ManageOffloadRatesGrid tbody :input").change(function () { ManageOffloadRatesGridWidget.UpdateGridData($(this)) });

        ManageOffloadRatesGridWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageOffloadRatesGridWidget.cleanDirty(); });

        $('#DeleteAllRates').click(ManageOffloadRatesGridWidget.DeleteAllRecords);

        ManageOffloadRatesGridWidget.registerForEvent('GET_DELETED_RATES', function () {

            // find all values that are no longer checked
            var toRemove = new Array();
            for (var k in ManageOffloadRatesGridWidget.data.CheckedItems) {
                if (ManageOffloadRatesGridWidget.data.CheckedItems[k].Deleted == false) {
                    toRemove.push(k);
                }
            }
            toRemove.reverse();
            // remove them from the dataset
            for (var j in toRemove) {
                ManageOffloadRatesGridWidget.data.CheckedItems.splice(toRemove[j],1);
            }
            $(document).trigger('DELETE_RATES', ManageOffloadRatesGridWidget.data); 
            
        });

        $('#ManageOffloadRatesGrid td.edit-rate-link').click(function() {
            var rowElement = $(this).parents('tr');
            $(document).trigger('EDIT_RATE', ManageOffloadRatesGridWidget.GetRowData(rowElement)); 
        });
    });
</script>

<div class="manage-default-resources-grid">
    <table id="ManageOffloadRatesGrid" class="grid readonly">
        <thead>
<tr>
    <th class="delete-checkbox"><input type="checkbox" id="DeleteAllRates" /></th>                   
    <th class="sort year">Year</th>
    <th class="sort resource">Resource</th>
    <th class="sort performing-org">Performing Org</th>
    <th class="sort sub-resource">Subcontractor Resource</th>
    <th class="offload-percent">Offload Percent</th>
    <th class="hourly-rate">Hourly Rate</th>
</tr>
</thead>
        <tbody>

<%if (Model.Results.Count>0)
  {%>

    <%foreach (GenBOE.ActionLogic.ModelView.Admin.OffloadRateModelView item in Model.Results)
      { %>
      
    <tr pkid="<%:item.OffloadRateID %>">
       <td class="delete-checkbox">
            <input type="checkbox" name="DeleteRate" />
        </td>
        <td class="year edit-rate-link">
            <a name="Year"><%: item.Year %></a>
            <input type="hidden" name="Year" value="<%: item.Year %>" />
            <input type="hidden" name="OffloadRateID" value="<%: item.OffloadRateID %>" />
        </td>
        <td class="resource edit-rate-link">
            <a name="Resource"><%: item.Resource%></a>
        </td>
        <td class="performing-org edit-rate-link">
            <a name="PerformingOrg"><%: item.PerformingOrg%></a>
        </td>
        <td class="sub-resource edit-rate-link">
            <a name="SubcontractorResource"><%: item.SubcontractorResource%></a>
        </td>
        <td class="offload-percent edit-rate-link">
            <a name="PercentToOffloadFormatted"><%: item.PercentToOffload.ToString(Constants.PERCENTAGE_FORMATTING)%></a>
            <input type="hidden" value="<%:item.PercentToOffload%>" name="PercentToOffload" />
        </td>
        <td class="hourly-rate edit-rate-link">
            <a name="HourlyRateFormatted"><%: string.Format(Constants.MONEY_FORMATTING, item.HourlyRate)%></a>
            <input type="hidden" value="<%:item.HourlyRate%>" name="HourlyRate" />
        </td>
    </tr>

    <%} %>

<% }else{%>
    <tr><td colspan="7" >There are no offload rates.</td></tr>
<%} %>

</tbody>
    </table>
</div>
