<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.DefaultPerfOrgGridModelView>" %>

<script type="text/javascript">

    ManageDefaultPerfOrgDataWidget = new Widget('ManageDefaultPerfOrgGrid', <%= ViewData["READONLY"] %>);

    // Updates grid data
    ManageDefaultPerfOrgDataWidget.UpdateGridData = function (input) {
        $(document).trigger('PERF_ORG_CHECKED');

        var rowElement = $(input).parents('tr');
        var ElementID = rowElement.attr('pkid');

        if (ManageDefaultPerfOrgDataWidget.data.CheckedItems == undefined) {
            ManageDefaultPerfOrgDataWidget.data.CheckedItems = new Array();
        }

        var updateIndex = -1;

        //First see if you have already updated this data.     
        for (var k in ManageDefaultPerfOrgDataWidget.data.CheckedItems) {
            if (ManageDefaultPerfOrgDataWidget.data.CheckedItems[k]['PerfOrgID'] == ElementID) {
                updateIndex = k;
                break;
            }
        }

        if (updateIndex == -1) {
            updateIndex = ManageDefaultPerfOrgDataWidget.data.CheckedItems.length;
            ManageDefaultPerfOrgDataWidget.data.CheckedItems[updateIndex] = {};
            ManageDefaultPerfOrgDataWidget.data.CheckedItems[updateIndex]['PerfOrgID'] = ElementID;
        }

        ManageDefaultPerfOrgDataWidget.data.CheckedItems[updateIndex]['PerfOrgName'] = rowElement.find('[name="PerfOrgName"]').text();
        ManageDefaultPerfOrgDataWidget.data.CheckedItems[updateIndex]['PerfOrgDesc'] = rowElement.find('[name="PerfOrgDesc"]').text();
        ManageDefaultPerfOrgDataWidget.data.CheckedItems[updateIndex]['UpdateDateLong'] = rowElement.find('input[name="UpdateDateLong"]').val().toString();
        ManageDefaultPerfOrgDataWidget.data.CheckedItems[updateIndex]['Deleted'] = rowElement.find('input[name="DeletePerfOrg"]').prop('checked');
    };

     ManageDefaultPerfOrgDataWidget.EnableSave = function() {
        $('#Save-ManageDefaultPerfOrg').removeClass('disabled');
     }

    ManageDefaultPerfOrgDataWidget.DeleteAllRecords = function ()
    {
        
        // check all the boxes
        $('#ManageDefaultPerfOrgGrid tbody input[name="DeletePerfOrg"]').prop('checked', $(this).prop('checked'));

        // add all of the rows data to the widget.
        $('#ManageDefaultPerfOrgGrid input[name="DeletePerfOrg"]').each(function() {
            ManageDefaultPerfOrgDataWidget.UpdateGridData($(this));
        });
    }

    $(function () {
        ManageDefaultPerfOrgDataWidget.afterDOMLoad();
        $("#ManageDefaultPerfOrgGrid tbody :input").change(function () { ManageDefaultPerfOrgDataWidget.UpdateGridData($(this)) });

        $('#DeleteAllPerfOrg').click(ManageDefaultPerfOrgDataWidget.DeleteAllRecords);

        ManageDefaultPerfOrgDataWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageDefaultPerfOrgDataWidget.cleanDirty(); });

        ManageDefaultPerfOrgWidget.TotalResults = <%: Model.TotalResults %>;

        ManageDefaultPerfOrgDataWidget.registerForEvent('GET_DELETED_PERFORMING_ORGS', function () {
            if ($('#ManageDefaultPerfOrgGrid tbody input[name="DeletePerfOrg"]:not(:checked)').length == 0 && 
                pagingData.data.NumPages == 1 &&
                ManageDefaultPerfOrgWidget.TotalResults <= ManageDefaultPerfOrgDataWidget.data.CheckedItems.length) {
                setTimeout(function() { Session.alertDialog("Cannot Delete All Options", "At least one option is required. Please uncheck at least one option before deleting."); }, 1);
            }
            else {
                // find all values that are no longer checked
                var toRemove = new Array();
                for (var k in ManageDefaultPerfOrgDataWidget.data.CheckedItems) {
                    if (ManageDefaultPerfOrgDataWidget.data.CheckedItems[k].Deleted == false) {
                        toRemove.push(k);
                    }
                }
                toRemove.reverse();
                // remove them from the dataset
                for (var j in toRemove) {
                    ManageDefaultPerfOrgDataWidget.data.CheckedItems.splice(toRemove[j],1);
                }
                $(document).trigger('DELETE_PERFORMING_ORGS', ManageDefaultPerfOrgDataWidget.data); 
            }
        });

        $('#ManageDefaultPerfOrgGrid tbody tr:not(.in-use) td:not(.delete-perforg)').click(function() {
             var rowElement = $(this).parents('tr');
            $(document).trigger('EDIT_PERF_ORG', ManageDefaultPerfOrgDataWidget.GetRowData(rowElement)); 
        });

         ManageDefaultPerfOrgDataWidget.GetRowData = function(rowElement) {
            var data = {};
            data.ID = rowElement.attr('pkid');
            data.Name = rowElement.find('a[name=PerfOrgName]').text();
            data.Description = rowElement.find('a[name=PerfOrgDesc]').text();
            data.UpdateDate = rowElement.find('input[name=UpdateDateLong]').val().toString();

            return data;
        }

        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('#PageControls');
        pagingData.action = function(data) { $(document).trigger('PAGE_PERFORMING_ORGS', data); }
        pagingData.type = "Arrow";

        ManageDefaultPerfOrgDataWidget.AddPaging(pagingData);
    });
</script>

<div class="manage-default-performing-organizations-grid">
    <table id="ManageDefaultPerfOrgGrid" class="grid readonly">
        <thead>
            <tr>
                <th class="delete-checkbox"><input type="checkbox" id="DeleteAllPerfOrg" /></th>                                   
                <th class="performing-organization-id">ID</th>
                <th class="performing-organization-description last-child">Description<div id="PageControls"></div></th>                                  
            </tr>
        </thead>
        <tbody>
            <%  foreach (var item in Model.PerfOrgResults) 
                { %>                          
                    <tr pkid="<%:item.PerfOrgID%>">
                        <td class="delete-perforg delete-checkbox" >
                            <input type="checkbox" name="DeletePerfOrg" />
                        </td>
                        <td>
                            <a name="PerfOrgName"><%: item.PerfOrgName %></a>
                            <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                        </td>
                        <td>
                            <a name="PerfOrgDesc"><%: item.PerfOrgDesc %></a>
                        </td>
                    </tr>
            <% } %>
        </tbody>
    </table>
</div>