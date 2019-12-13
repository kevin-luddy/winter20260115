<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.BOECustomFieldResourceGridModelView>" %>

<%
    int page = Model.CurrentPage;
      
      
       %>

<script type="text/javascript">
    BOECustomFieldsResourceDataWidget = new Widget('BOECustomFieldsResourceData', <%= ViewData["READONLY"] %>);

    var BOECustomFieldsResourceDataWidget_ReadOnly = <%= ViewData["READONLY"] %>;

    // Updates grid data
    BOECustomFieldsResourceDataWidget.UpdateGridData = function (input) {
        $(document).trigger('RESOURCE_CHECKED');

        var rowElement = $(input).parents('tr');
        var ElementID = rowElement.attr('pkid');

        if (BOECustomFieldsResourceDataWidget.data.CheckedItems == undefined) {
            BOECustomFieldsResourceDataWidget.data.CheckedItems = new Array();
        }

        var updateIndex = -1;

        //First see if you have already updated this data.     
        for (var k in BOECustomFieldsResourceDataWidget.data.CheckedItems) {
            if (BOECustomFieldsResourceDataWidget.data.CheckedItems[k]['CustomFieldOptionID'] == ElementID) {
                updateIndex = k;
                break;
            }
        }

        if (updateIndex == -1) {
            updateIndex = BOECustomFieldsResourceDataWidget.data.CheckedItems.length;
            BOECustomFieldsResourceDataWidget.data.CheckedItems[updateIndex] = {};
        }

        BOECustomFieldsResourceDataWidget.data.CheckedItems[updateIndex] = BOECustomFieldsResourceDataWidget.GetRowData(rowElement);
    };

    BOECustomFieldsResourceDataWidget.GetRowData = function(rowElement) {
        var data = {};
        data.CustomFieldOptionID = rowElement.attr('pkid');
        data.ID = rowElement.find('a[name="ID"]').text();
        data.Description = rowElement.find('a[name="Description"]').text();
        data.SegmentRegion = rowElement.find('a[name="SegmentRegion"]').text();
        data.LaborType = rowElement.find('a[name="LaborType"]').text();
        data.ElementOfCostId = rowElement.find('input[name="ElementOfCostID"]').val();
        data.UpdateDateLong = rowElement.find('input[name="UpdateDateLong"]').val().toString();
        data.Deleted = rowElement.find('input[name="DeleteResource"]').prop('checked');
        data.InUse = rowElement.find('input[name="InUse"]').val();
        data.RateTypeID = rowElement.find('input[name="RateTypeID"]').val();
        data.RateType = rowElement.find('input[name="RateTypeID"]').val();
        return data;
    }

    BOECustomFieldsResourceDataWidget.DeleteAllRecords = function ()
    {
        // check all the boxes
        $('#BOECustomFieldsResourceData tbody input[name="DeleteResource"]').prop('checked', $(this).prop('checked'));

        // add all of the rows data to the widget.
        $('#BOECustomFieldsResourceData tbody input[name="DeleteResource"]').each(function() {
            BOECustomFieldsResourceDataWidget.UpdateGridData($(this));
        });
    }

    //override the applyReadOnly because having it call the real function causes an IE script error (bug was created to investigate issue)
    BOECustomFieldsResourceDataWidget.applyReadOnly = function()
    {
        if (BOECustomFieldsResourceDataWidget_ReadOnly)
        {
            
            $("table#BOECustomFieldsResourceTable :input").hide();
            $("table#BOECustomFieldsResourceTable td.edit-resource-link").unbind('click');
            $("table#BOECustomFieldsResourceTable").addClass("readonly-apply");
            $(".hide-on-readonly").hide();
           
         }
    };

    $(function() {
        BOECustomFieldsResourceDataWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { BOECustomFieldsResourceDataWidget.cleanDirty(); });

        $("#BOECustomFieldsResourceData tbody :input").on('change', function () { BOECustomFieldsResourceDataWidget.UpdateGridData($(this)) });

        $('#DeleteAllResources').click(BOECustomFieldsResourceDataWidget.DeleteAllRecords);

        BOECustomFieldsResourceDataWidget.TotalResources = <%: Model.TotalResults %>;

        BOECustomFieldsResourceDataWidget.registerForEvent('GET_DELETED_RESOURCES', function () {
            if ($('#BOECustomFieldsResourceData tbody input[name="DeleteResource"]:not(:checked)').length == 0 &&
                $('#BOECustomFieldsResourceData tr.in-use').length == 0 &&
                pagingData.data.NumPages == 1 &&
                BOECustomFieldsResourceDataWidget.TotalResources <= BOECustomFieldsResourceDataWidget.data.CheckedItems.length) {
                setTimeout(function() { Session.alertDialog("Cannot Delete All Options", "At least one option is required. Please uncheck at least one option before deleting."); }, 1);
            } else {
                // find all values that are no longer checked
                var toRemove = new Array();
                for (var k in BOECustomFieldsResourceDataWidget.data.CheckedItems) {
                    if (BOECustomFieldsResourceDataWidget.data.CheckedItems[k].Deleted == false) {
                        toRemove.push(k);
                    }
                }
                toRemove.reverse();
                // remove them from the dataset
                for (var j in toRemove) {
                    BOECustomFieldsResourceDataWidget.data.CheckedItems.splice(toRemove[j],1);
                }
                $(document).trigger('DELETE_RESOURCES', BOECustomFieldsResourceDataWidget.data); 
            }
        });

        BOECustomFieldsResourceDataWidget.registerForDelegateEvent('click', 'td.edit-resource-link', function() { 
            var rowElement = $(this).parents('tr');
            $(document).trigger('EDIT_RESOURCE', BOECustomFieldsResourceDataWidget.GetRowData(rowElement)); 
        });

        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('.paging-control');
        pagingData.action = function(data) { $(document).trigger('PAGE_RESOURCES', data); }
        pagingData.type = "PageNumber";

        BOECustomFieldsResourceDataWidget.AddPaging(pagingData);

        BOECustomFieldsResourceDataWidget.applyReadOnly();
  
       
    });
</script>

<div id="BOECustomFieldsResourceData" class="boe-custom-field-resource-grid">
    <table id="BOECustomFieldsResourceTable" class="grid readonly">
        <thead>
            <tr>
                <th class="delete-checkbox"><input type="checkbox" id="DeleteAllResources" /></th>                   
                <th class="resource-id">ID</th>
                <th class="description">Description</th>
                <th class="segment-region">Segment/Region</th>
                <th class="labor-type">Labor Type</th>
                <th class="segment">Rate Type</th>
                <th class="element-of-cost last-child">Element of Cost</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (BOECustomFieldResourceModelView item in Model.ResourceResults) {
                   
                   %>
                    <tr pkid="<%: item.CustomFieldOptionID %>">
                    
                        <% if (item.InUse)
                            { %>
                                <td>
                                <div class="in-use">In use</div>
                                <input type="hidden" name="InUse" value="<%: item.InUse %>" />
                                </td>
                        <% }
                            else
                            { %>
                                <td class="delete-checkbox">
                                <input type="checkbox" name="DeleteResource" />
                                <input type="hidden" name="InUse" value="<%: item.InUse %>" />
                                </td>
                        <% } %>
                        <td class="resource-id edit-resource-link">
                            <a name="ID"><%: item.ID %></a>
                            <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                        </td>
                        <td class="description edit-resource-link">
                            <a name="Description"><%: item.Description %></a>
                        </td>
                        <td class="segment-region edit-resource-link">
                            <a name="SegmentRegion"><%: item.SegmentRegion %></a>
                        </td>
                        <td class="labor-type edit-resource-link">
                            <a name="LaborType"><%: item.LaborType %></a>
                        </td>
                        <td class="rate-type edit-resource-link">
                            <a name="RateType"><%: item.RateTypeDisplay %></a>
                            <input type="hidden" value="<%: (int)item.RateTypeID %>" name="RateTypeID" />
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
