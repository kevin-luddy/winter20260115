<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.BOECustomFieldPerformingOrgGridModelView>" %>
<%
    bool isReadOnly = bool.Parse((string)ViewData["READONLY"]);
%>
<script type="text/javascript">
    var PerfOrg_ReadOnly = '<%= isReadOnly %>'.isTrue();
    BOECustomFieldsPerformingOrgDataWidget = new Widget('BOECustomFieldsPerformingOrgData', PerfOrg_ReadOnly);

    // Updates grid data
    BOECustomFieldsPerformingOrgDataWidget.UpdateGridData = function (input) {
        $(document).trigger('PERFORMING_ORG_CHECKED');

        var rowElement = $(input).parents('tr');
        var ElementID = rowElement.attr('pkid');

        if (BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems == undefined) {
            BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems = new Array();
        }

        var updateIndex = -1;

        //First see if you have already updated this data.     
        for (var k in BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems) {
            if (BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems[k]['CustomFieldOptionID'] == ElementID) {
                updateIndex = k;
                break;
            }
        }

        if (updateIndex == -1) {
            updateIndex = BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems.length;
            BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems[updateIndex] = {};
        }

        BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems[updateIndex] = BOECustomFieldsPerformingOrgDataWidget.GetRowData(rowElement);
    };

    BOECustomFieldsPerformingOrgDataWidget.GetRowData = function(rowElement) {
        var data = {};
        data.CustomFieldOptionID = rowElement.attr('pkid');
        data.ID = rowElement.find('a[name="ID"]').text();
        data.Description = rowElement.find('a[name="Description"]').text();
        data.UpdateDateLong = rowElement.find('input[name="UpdateDateLong"]').val().toString();
        data.Deleted = rowElement.find('input[name="DeletePerformingOrg"]').prop('checked');
        return data;
    }

    BOECustomFieldsPerformingOrgDataWidget.DeleteAllRecords = function ()
    {        
        // check all the boxes
        $('#BOECustomFieldsPerformingOrgData tbody input[name="DeletePerformingOrg"]').prop('checked', $(this).prop('checked'));

        // add all of the rows data to the widget.
        $('#BOECustomFieldsPerformingOrgData tbody input[name="DeletePerformingOrg"]').each(function() {
            BOECustomFieldsPerformingOrgDataWidget.UpdateGridData($(this));
        });
    }

    $(function() {
        $("#BOECustomFieldsPerformingOrgData tbody :input").change(function () { BOECustomFieldsPerformingOrgDataWidget.UpdateGridData($(this)) });

        BOECustomFieldsPerformingOrgDataWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { BOECustomFieldsPerformingOrgDataWidget.cleanDirty(); });

        $('#DeleteAllPerformingOrgs').click(BOECustomFieldsPerformingOrgDataWidget.DeleteAllRecords);

        BOECustomFieldsPerformingOrgDataWidget.TotalPerformingOrgs = <%: Model.TotalResults %>;

        BOECustomFieldsPerformingOrgDataWidget.registerForEvent('GET_DELETED_PERFORMING_ORGS', function () {          
            if ($('#BOECustomFieldsPerformingOrgData tbody input[name="DeletePerformingOrg"]:not(:checked)').length == 0 &&
                $('#BOECustomFieldsPerformingOrgData tr.in-use').length == 0 &&
                pagingData.data.NumPages == 1 &&
                BOECustomFieldsPerformingOrgDataWidget.TotalPerformingOrgs <= BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems.length) {
                setTimeout(function() { 
                    Session.alertDialog("Cannot Delete All Options", "At least one option is required. Please uncheck at least one option before deleting.");
                }, 1);
            }
            else {
                // find all values that are no longer checked
                var toRemove = new Array();
                for (var k in BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems) {
                    if (BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems[k].Deleted == false) {
                        toRemove.push(k);
                    }
                }
                toRemove.reverse();
                // remove them from the dataset
                for (var j in toRemove) {
                    BOECustomFieldsPerformingOrgDataWidget.data.CheckedItems.splice(toRemove[j],1);
                }
                $(document).trigger('DELETE_PERFORMING_ORGS', BOECustomFieldsPerformingOrgDataWidget.data); 
            }
        });

        BOECustomFieldsPerformingOrgDataWidget.registerForDelegateEvent('click', 'td.edit-performing-org-link', function() { 
            var rowElement = $(this).parents('tr');
            $(document).trigger('EDIT_PERFORMING_ORG', BOECustomFieldsPerformingOrgDataWidget.GetRowData(rowElement)); 
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
        pagingData.action = function(data) { $(document).trigger('PAGE_PERFORMING_ORGS', data); }
        pagingData.type = "PageNumber";

        BOECustomFieldsPerformingOrgDataWidget.AddPaging(pagingData);
    });
</script>

<div id="BOECustomFieldsPerformingOrgData" class="boe-custom-field-performing-org-grid">
    <table id="BOECustomFieldPerformingOrgTable" class="readonly grid">
        <thead>
            <tr>
                <th class="delete-checkbox"><% if (!isReadOnly) { %><input type="checkbox" id="DeleteAllPerformingOrgs" /><% } %></th>
                <th class="performing-organization-id">ID</th>
                <th class="performing-organization-description last-child">Description</th>
            </tr>
        </thead>
        <tbody>
            <%  foreach (var item in Model.PerformingOrgResults) { %>     
                <% if (item.InUse) { %>
                <tr pkid="<%:item.CustomFieldOptionID%>" class="in-use">
                    <td>
                        <div>In use</div>
                    </td>
                    <td>
                        <div><%: item.ID %></div>
                    </td>
                    <td>
                        <div><%: item.Description %></div>
                    </td>
                </tr>
               <% } else if (isReadOnly) { %>
               <tr pkid="<%:item.CustomFieldOptionID%>" class="in-use">
                    <td>
                        <div></div>
                    </td>
                    <td>
                        <div><%: item.ID %></div>
                    </td>
                    <td>
                        <div><%: item.Description %></div>
                    </td>
                </tr>
               <% } else { %>
                <tr pkid="<%:item.CustomFieldOptionID%>">
                    <td class="delete-checkbox">
                       <input type="checkbox" name="DeletePerformingOrg" />
                    </td>
                    <td class="edit-performing-org-link">
                        <a name="ID"><%: item.ID %></a>        
                    </td>
                    <td class="edit-performing-org-link">
                        <a name="Description"><%: item.Description %></a>
                        <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                    </td>
                </tr>
                <% } %>
            <% } %>
        </tbody>
    </table>
</div>
