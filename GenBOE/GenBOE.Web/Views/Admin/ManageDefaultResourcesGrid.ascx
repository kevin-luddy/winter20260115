<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.DefaultResourcesGridModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView.Admin" %>

<script type="text/javascript">
    var ManageDefaultResourcesDataWidget = new Widget('ManageDefaultResourcesGrid', <%= ViewData["READONLY"] %>);

    // Updates grid data
    ManageDefaultResourcesDataWidget.UpdateGridData = function (input) {
        $(document).trigger('RESOURCE_CHECKED');

        var rowElement = $(input).parents('tr');
        var ElementID = rowElement.attr('pkid');

        if (ManageDefaultResourcesDataWidget.data.CheckedItems == undefined) {
            ManageDefaultResourcesDataWidget.data.CheckedItems = new Array();
        }

        var updateIndex = -1;

        //First see if you have already updated this data.     
        for (var k in ManageDefaultResourcesDataWidget.data.CheckedItems) {
            if (ManageDefaultResourcesDataWidget.data.CheckedItems[k]['ResourceID'] == ElementID) {
                updateIndex = k;
                break;
            }
        }

        if (updateIndex == -1) {
            updateIndex = ManageDefaultResourcesDataWidget.data.CheckedItems.length;
            ManageDefaultResourcesDataWidget.data.CheckedItems[updateIndex] = {};
        }

        ManageDefaultResourcesDataWidget.data.CheckedItems[updateIndex] = ManageDefaultResourcesDataWidget.GetRowData(rowElement);
    };

    
     DefaultResourcesWidget.EnableSave = function() {
        $('#Save-DefaultResources').removeClass('disabled');
     }

    ManageDefaultResourcesDataWidget.GetRowData = function(rowElement) {
        var data = {};
        data.ResourceID = rowElement.attr('pkid');
        data.ID = rowElement.find('a[name="ID"]').text();
        data.Description = rowElement.find('a[name="Description"]').text();
        data.SegmentRegion = rowElement.find('a[name="SegmentRegion"]').text();
        data.LaborType = rowElement.find('a[name="LaborType"]').text();
        data.RateTypeID = rowElement.find('input[name="RateTypeID"]').val();
        data.ElementOfCost = rowElement.find('a[name="ElementOfCostDisplay"]').text();
        data.ElementOfCostId = rowElement.find('input[name="ElementOfCostID"]').val();
        data.UpdateDateLong = rowElement.find('input[name="UpdateDateLong"]').val().toString();
        data.Deleted = rowElement.find('input[name="DeleteResource"]').prop('checked');
        data.InUse = rowElement.find('input[name="InUse"]').val();
        return data;
    }

    ManageDefaultResourcesDataWidget.DeleteAllRecords = function ()
    {        
        // check all the boxes
        $('#ManageDefaultResourcesGrid tbody input[name="DeleteResource"]').prop('checked', $(this).prop('checked'));

        // add all of the rows data to the widget.
        $('#ManageDefaultResourcesGrid input[name="DeleteResource"]').each(function() {
            ManageDefaultResourcesDataWidget.UpdateGridData($(this));
        });
    }

    $(function () {
        ManageDefaultResourcesDataWidget.afterDOMLoad();
        $("#ManageDefaultResourcesGrid tbody :input").change(function () { ManageDefaultResourcesDataWidget.UpdateGridData($(this)) });

        ManageDefaultResourcesDataWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageDefaultResourcesDataWidget.cleanDirty(); });

        $('#DeleteAllResources').click(ManageDefaultResourcesDataWidget.DeleteAllRecords);

        ManageDefaultResourcesDataWidget.TotalResults = <%: Model.TotalResults %>;

        ManageDefaultResourcesDataWidget.registerForEvent('GET_DELETED_RESOURCES', function () {

            if ($('#ManageDefaultResourcesGrid tbody input[name="DeleteResource"]:not(:checked)').length == 0 && pagingData.data.NumPages == 1 &&
                ManageDefaultResourcesDataWidget.TotalResults <= ManageDefaultResourcesDataWidget.data.CheckedItems.length) {
                setTimeout(function() { Session.alertDialog("Cannot Delete All Options", "At least one option is required. Please uncheck at least one option before deleting."); }, 1);
            }
            else {
                // find all values that are no longer checked
                var toRemove = new Array();
                for (var k in ManageDefaultResourcesDataWidget.data.CheckedItems) {
                    if (ManageDefaultResourcesDataWidget.data.CheckedItems[k].Deleted == false) {
                        toRemove.push(k);
                    }
                }
                toRemove.reverse();
                // remove them from the dataset
                for (var j in toRemove) {
                    ManageDefaultResourcesDataWidget.data.CheckedItems.splice(toRemove[j],1);
                }

                $(document).trigger('DELETE_RESOURCES', ManageDefaultResourcesDataWidget.data); 
            }
        });

        $('#ManageDefaultResourcesGrid td.edit-resource-link').click(function() { 
            var rowElement = $(this).parents('tr');
            $(document).trigger('EDIT_RESOURCE', ManageDefaultResourcesDataWidget.GetRowData(rowElement)); 
        });

        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('#PageControls');
        pagingData.action = function(data) { $(document).trigger('PAGE_RESOURCES', data); }
        pagingData.type = "Arrow";

        ManageDefaultResourcesDataWidget.AddPaging(pagingData);
    });

</script>

<div class="manage-default-resources-grid" style="width: 800px;">
    <table id="ManageDefaultResourcesGrid" class="grid readonly">
        <thead>
            <tr>
                <th class="delete-checkbox"><input type="checkbox" id="DeleteAllResources" /></th>                   
                <th class="resource-id">ID</th>
                <th class="description">Description</th>
                <th class="segment-region">Segment/Region</th>
                <th class="labor-type">Labor Type</th>
                <th class="rate-type">Rate Type</th>
                <th class="element-of-cost last-child">Element of Cost</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (DefaultResourceModelView item in Model.ResourceResults) { %>

                <tr pkid="<%: item.ResourceID %>">                
                    <td class="delete-checkbox">
                    <input type="checkbox" name="DeleteResource" />
                    <input type="hidden" name="InUse" value="<%: item.InUse %>" />
                    </td>

                    <td class="resource-id edit-resource-link" title="<%: item.ID %>">
                        <a name="ID"><%: item.ID %></a>
                        <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                    </td>
                    <td class="description edit-resource-link" title="<%: item.Description %>">
                        <a name="Description"><%: item.Description %></a>
                    </td>
                    <td class="segment-region edit-resource-link">
                        <a name="SegmentRegion"><%: item.SegmentRegion %></a>
                    </td>
                    <td class="labor-type edit-resource-link">
                        <a name="LaborType"><%: item.LaborType %></a>
                    </td>
                    <td class="rate-type edit-resource-link">
                        <a name="RateType"><%: item.RateTypeDisplay %></a><!---->
                        <input type="hidden" value="<%: (int)item.RateTypeID%>" name="RateTypeID" /><!---->
                    </td>
                    <td class="element-of-cost edit-resource-link">
                        <a name="ElementOfCost"><%: item.ElementOfCostDisplay %></a>
                        <input type="hidden" value="<%: (int)item.ElementOfCostId %>" name="ElementOfCostID" />
                    </td>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>