<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.Reporting.BoeDiscrepancyReportModelView>>" %>
<%@ Import Namespace="GenBOE.ActionLogic.Reporting" %>

<script type="text/javascript">
    // Base URL used for all the links we'll need to create on this page
    var baseUrl = CreatePostURL('<%:SiteMasterUtilities.GetCurrentWorkspace()%>', '<%: WebConstants.CONTROLLER_BOE %>', '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>', 'boe/');

    function setUrlForBoe(boeId)
    {
        $('.boe' + boeId).attr('href', baseUrl + boeId);
    }

    function setUrlForTaskElement(boeId, taskId)
    {
        $('.task' + taskId).attr('href', baseUrl + boeId + '#LMLabor/task/' + taskId);
    }

    function setUrlForODCElement(boeId, taskId)
    {
        $('.odc' + taskId).attr('href', baseUrl + boeId + '#ODC/odc/' + taskId);
    }

    function exportClicked() {
        var reportGenerationUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_REPORTS %>', '<%: WebConstants.ACTION_EXPORT_BOE_DISCREPANCY %>', '');

        // Remove the old hidden iFrame, if it exists
        $('#Exports-DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', { 'id': 'Exports-DownloadTarget', 'class': 'display-none', 'src': reportGenerationUrl });

        // Append the iFrame to the body, causing the controller action to fire and the download to occur inside the iFrame
        targetIFrame.appendTo('body');
    }

    $(function () {
        if (<%: Model.Count() %> <= 0) {
            $('#BoeDiscrepancyReportTable tbody').html('<tr><td colspan="5"><div class="empty-grid-text">There are no BOEs.</div></td></tr>');
        }

        createModule($('#BoeDiscrepancyReport'));
        refreshModule($('#BoeDiscrepancyReport'));

        // Setup URLs for all the items in the table
        <% foreach(BoeDiscrepancyReportModelView item in Model) { %>
            setUrlForBoe('<%: item.BoeId %>');

            <% foreach (BoeTaskDetailsMV taskItem in item.ElementsWithIssues.Where(x => x.DiscrepancyEnum == BoeInconsistencyEnum.Hours 
                   || x.DiscrepancyEnum == BoeInconsistencyEnum.Cost || x.DiscrepancyEnum == BoeInconsistencyEnum.HoursAndCost))
               { %>
                    setUrlForTaskElement('<%: item.BoeId %>', '<%: taskItem.TaskId %>');
            <% } %>

            <% foreach (BoeTaskDetailsMV taskItem in item.ElementsWithIssues.Where(x => x.DiscrepancyEnum == BoeInconsistencyEnum.ODC))
               { %>
                    setUrlForODCElement('<%: item.BoeId %>', '<%: taskItem.TaskId %>');
            <% } %>
        <% } %>

        // Handle the collapse/expand
        $('.collapsibleCell').click(function() {
            if($(this).hasClass('collapsed')) {
                $(this).removeClass('collapsed').addClass('expanded');
                $(this).parent('td').parent('tr').nextAll('tr').each( function() { 
                    if ($(this).is('.collapsibleHeaderRow')) { 
                        return false; 
                    } 
                
                    $(this).show();
                }); 
            } else {
                $(this).removeClass('expanded').addClass('collapsed');
                $(this).parent('td').parent('tr').nextAll('tr').each( function() { 
                    if ($(this).is('.collapsibleHeaderRow')) { 
                        return false; 
                    } 
                
                    $(this).hide();
                });
            }
        });
    });

</script>

<%if(ViewData.ContainsKey("ExportBoeDiscrepancyReport")) { %>
    <!-- These styles are for Excel export only -->
    <style type="text/css">
        .module-header-data {
            font-weight:bolder;
            font-size:150%;
        }

        .subheaderRowForReport {
            color:brown; 
            font-weight:bold; 
            background-color:lightblue;
        }

        #BoeDiscrepancyReportTable tr td,
        #BoeDiscrepancyReportTable tr th  {
            border:0.5px solid black;
        }

        #BoeDiscrepancyReportTable th {
            background-color:dimgray;
            color:white;
        }

        .collapsibleHeaderRow {
            font-weight:bold;
        }

    </style>
<% } %>

<div id="BoeDiscrepancyReport" class="module">
    <div class="module-header-data">
        BOE Discrepancy Report
        <%if(ViewData.ContainsKey("ExportBoeDiscrepancyReport")) { %>
         - <%:SiteMasterUtilities.GetCurrentWorkspace() %>
        <% } %>

        <%if(ViewData["ExportBoeDiscrepancyReport"] == null) { %>
        <!-- Only display the Export button if we are not exporting.. :) -->
            <button style="float:right;" class="ies" onclick="exportClicked()" name="export-button" type="button">Export</button>
            <div style="height:0; width:0;">
            <%:Html.ActionLink("Export Link", WebConstants.ACTION_EXPORT_BOE_DISCREPANCY, new { workspace = SiteMasterUtilities.GetCurrentWorkspace() }, new { id = "exportLink" })%>
            </div>
        <% } %>
    </div>
    <div class="module-content-data">
        <%if(ViewData["ExportBoeDiscrepancyReport"] == null) { %>
        <!-- Only display the information if we are not exporting -->
            <div id="BoeDiscrepancyInstructions">
                The following BOEs and Tasks contain a data discrepancy in <%: ViewData["HoursLabel"]%> or cost values.  This report verifies the MOQ Equation Total equals 
                the sum of all resource <%: ViewData["HoursLabel"]%> spreads within a task.  Individual Resource Types totals are also checked to verify they equal the 
                sum of the monthly resource spreads.  
                <br /><br />
                To correct the data discrepancies:<br />
                <ul id="BoeDiscrepancyLevelOneUl">
                    <li>Verify the MOQ Equation Total is fully spread across the resources used in the task</li>
                    <li>If the MOQ Equation Total is fully spread and deltas exist, or data disconnects exist between the resource 
                    types totals and the resource spread totals, please do the following to trigger a recalculation on the page prior to resaving the task:
                        <ul id="BoeDiscrepancyLevelTwoUl">
                            <li>Modify the MOQ Equation and click outside of the text box to trigger a recalculation</li>
                            <li>Modify the <%: ViewData["HoursLabel"]%> spread, percent spread, or spread curve selection for the resource(s) with a disconnect in totals</li>
                            <li>Modify the monthly discrete spread value for the resource(s) with a disconnect in totals</li>
                        </ul>
                    </li>
                </ul>
            </div>
        <% } %>

        <table id="BoeDiscrepancyReportTable" class="grid readonly full">
            <thead>
                <tr>
                    <th width="20px"></th>
                    <th width="220px";>WBS</th>
                    <th width="240px">BOE Title</th>
                    <th width="220px">CLIN</th>
                    <th width="200px">BOE Author(s)</th>
                 </tr>
            </thead>
            <tbody>
                <% foreach(BoeDiscrepancyReportModelView item in Model)
                   { %>
                        <tr class="collapsibleHeaderRow">
                            <td><div class="TaskElementsToCopy expanded collapsibleCell"></div></td>
                            <td><a href class="boe<%:item.BoeId %>" target="_blank"><%=item.Wbs %></a></td>
                            <td><a href class="boe<%:item.BoeId %>" target="_blank"><%:item.BoeTitle%></a></td>
                            <td><a href class="boe<%:item.BoeId %>" target="_blank"><%=item.Clin %></a></td>
                            <td><a href class="boe<%:item.BoeId %>" target="_blank"><%=item.BoeAuthors %></a></td>
                        </tr>

                        <tr class="subheaderRowForReport">
                            <td></td>
                            <td></td>
                            <td>Task Id</td>
                            <td>Task Title</td>
                            <td>Discrepancy</td>
                        </tr>

                        <% foreach (BoeTaskDetailsMV taskItem in item.ElementsWithIssues) 
                            {
                                string type = string.Empty;
                                if (taskItem.DiscrepancyEnum == BoeInconsistencyEnum.ODC) type = "odc";
                                else if (taskItem.DiscrepancyEnum == BoeInconsistencyEnum.Cost
                                    || taskItem.DiscrepancyEnum == BoeInconsistencyEnum.Hours
                                    || taskItem.DiscrepancyEnum == BoeInconsistencyEnum.HoursAndCost) type = "task"; %>
                                <tr>
                                    <td></td>
                                    <td></td>
                                    <td><a href class="<%:type + taskItem.TaskId.ToString() %>" target="_blank"><%:taskItem.DisplayedTaskId %></a></td>
                                    <td><a href class="<%:type + taskItem.TaskId.ToString() %>" target="_blank"><%:taskItem.TaskTitle %></a></td>
                                    <td><a href class="<%:type + taskItem.TaskId.ToString() %>" target="_blank"><%:taskItem.InconsistencyText %></a></td>
                                </tr>
                        <% } %>

                        <tr><td colspan="5">&nbsp;</td></tr>
                <% } %>
            </tbody>
        </table>
    </div>
</div>