<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.ManageEscalationRatesGridModelView>" %>

<script type="text/javascript">
    var ManageEscalationRatesGridWidget = new Widget('ManageEscalationRatesGrid', <%= ViewData["READONLY"] %>);

//    // Updates grid data
    ManageEscalationRatesGridWidget.UpdateGridData = function (input) {
        $(document).trigger('RATE_CHECKED');

        var rowElement = $(input).parents('tr');
        var ElementID = rowElement.attr('pkid');

        if (ManageEscalationRatesGridWidget.data.CheckedItems == undefined) {
            ManageEscalationRatesGridWidget.data.CheckedItems = new Array();
        }

        var updateIndex = -1;

        //First see if you have already updated this data.     
        for (var k in ManageEscalationRatesGridWidget.data.CheckedItems) {
            if (ManageEscalationRatesGridWidget.data.CheckedItems[k]['EscalationRateID'] == ElementID) {
                updateIndex = k;
                break;
            }
        }

        if (updateIndex == -1) {
            updateIndex = ManageEscalationRatesGridWidget.data.CheckedItems.length;
            ManageEscalationRatesGridWidget.data.CheckedItems[updateIndex] = {};
        }

        ManageEscalationRatesGridWidget.data.CheckedItems[updateIndex] = ManageEscalationRatesGridWidget.GetRowData(rowElement);
    };

    ManageEscalationRatesGridWidget.GetRowData = function(rowElement) {
        var data = {};
        data.EscalationRateID = rowElement.attr('pkid');
        data.DevEscalation = rowElement.find('a[name="DevEscalation"]').text();
        data.Year = rowElement.find('a[name="Year"]').text();
        data.LMSIEscalation = rowElement.find('a[name="LMSIEscalation"]').text();
        data.MiscRate = rowElement.find('a[name="MiscRate"]').text();
        if (data.MiscRate == undefined || data.MiscRate == ''){
            data.MiscRate = 0;
        }
        data.UpdateDateLong = rowElement.find('input[name="UpdateDateLong"]').val().toString();
        data.Deleted = rowElement.find('input[name="DeleteRate"]').prop('checked');
        data.inUse = rowElement.find('input[name="inUse"]').val();
        return data;
    }

    ManageEscalationRatesGridWidget.DeleteAllRecords = function ()
    {        
        // check all the boxes
        $('#ManageEscalationRatesGrid tbody input[name="DeleteRate"]').prop('checked', $(this).prop('checked'));

        // add all of the rows data to the widget.
        $('#ManageEscalationRatesGrid input[name="DeleteRate"]').each(function() {
            ManageEscalationRatesGridWidget.UpdateGridData($(this));
        });
    }

    $(function () {
        ManageEscalationRatesGridWidget.afterDOMLoad();
        $("#ManageEscalationRatesGrid tbody :input").change(function () { ManageEscalationRatesGridWidget.UpdateGridData($(this)) });

        ManageEscalationRatesGridWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageEscalationRatesGridWidget.cleanDirty(); });

        $('#DeleteAllRates').click(ManageEscalationRatesGridWidget.DeleteAllRecords);

        ManageEscalationRatesGridWidget.registerForEvent('GET_DELETED_RATES', function () {

                // find all values that are no longer checked
                var toRemove = new Array();
                for (var k in ManageEscalationRatesGridWidget.data.CheckedItems) {
                    if (ManageEscalationRatesGridWidget.data.CheckedItems[k].Deleted == false) {
                        toRemove.push(k);
                    }
                }
                toRemove.reverse();
                // remove them from the dataset
                for (var j in toRemove) {
                    ManageEscalationRatesGridWidget.data.CheckedItems.splice(toRemove[j],1);
                }
                $(document).trigger('DELETE_RATES', ManageEscalationRatesGridWidget.data); 
            
        });

        $('#ManageEscalationRatesGrid td.edit-rate-link').click(function() {
            var rowElement = $(this).parents('tr');
            $(document).trigger('EDIT_RATE', ManageEscalationRatesGridWidget.GetRowData(rowElement)); 
        });

    });

</script>

<div class="manage-default-resources-grid">
    <table id="ManageEscalationRatesGrid" class="grid readonly">
        <thead>
            <tr>
                <th class="delete-checkbox"><input type="checkbox" id="DeleteAllRates" /></th>                   
                <th class="year">Year</th>
                <th class="dev-escalation"><%: Model.DevEscalationTitle %> %</th>
                <th class="lmsi-escalation"><%: Model.PerDiemOrLmsiEscalationTitle %> %</th>
                <% if (Model.ShowMiscRate) { %><th class="lmsi-escalation">Misc / Car Escalation %</th><% } %>

            </tr>
        </thead>
        <tbody>
            <% foreach (GenBOE.ActionLogic.ModelView.Admin.EscalationRateModelView item in Model.EscalationRateModelViews) { %>
                <tr pkid="<%: item.EscalationRateID %>">
                    <% if (item.inUse == false)
                       { %>
                        <td class="delete-checkbox">
                            <input type="checkbox" name="DeleteRate" />
                            <input type="hidden" name="inUse" value="<%: item.inUse %>" />
                        </td>
                    <%}
                       else
                       { %>
                        <td>
                            <div class="in-use">In use</div>
                            <input type="hidden" name="inUse" value="<%: item.inUse %>" />
                        </td>

                    <%} %>
                    <td class="year edit-rate-link">
                        <a name="Year"><%: item.Year %></a>
                        <input type="hidden" name="Year" value="<%: item.Year %>" />
                        <input type="hidden" name="EscalationRateID" value="<%: item.EscalationRateID %>" />
                    </td>
                    <td class="dev-escalation edit-rate-link text-right">
                        <a name="DevEscalation"><%: item.DevEscalation%></a>
                    </td>
                    <td class="lmsi-escalation edit-rate-link text-right">
                        <a name="LMSIEscalation"><%: item.LMSIEscalation %></a>
                    </td>
                    <% if (Model.ShowMiscRate) { %>
                    <td class="misc-escalation edit-rate-link text-right">
                        <a name="MiscRate"><%: item.MiscRate %></a>
                    </td>                    
                    <% } %>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>