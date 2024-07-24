<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Dtos.BOEActivityReportModelView>" %>

<script type="text/javascript">

    var BOEActivityReport = new GridWidget("BOEActivityReport");

    BOEActivityReport.Export = function() {
        $('#BOEActivityReport-ExportLoader').removeClass('display-none');
        $('#BOEActivityReport-Export').addClass('display-none');        
    }

    $(function () {
        if (<%: Model.Rows.Count() %> <= 0) {
            $('#BOEActivityReportGrid tbody').html('<tr><td colspan="15"><div class="empty-grid-text">There are no BOEs.</div></td></tr>');
        }


        BOEActivityReport.sortField = '<%: ViewData["SortField"] %>';
        BOEActivityReport.sortDirection = '<%: ViewData["SortDirection"] %>';


        $('#BOEActivityReportGrid th a').click(function () {
            var sortField = $(this).parent().attr('sortby');

            var data = {};
            data.reportID = <%: (int)Reports.BOEActivity %>;
            data.sortField = sortField;
            data.sortDirection = 'Asc';
            data.reportName = 'BOE Activity'

            if (BOEActivityReport.sortField == sortField || BOEActivityReport.sortField == '') {
                if (BOEActivityReport.sortDirection == 'Asc') {
                    data.sortDirection = 'Desc';
                }
            }
            BOEActivityReport.sortField = data.sortField;
            BOEActivityReport.sortDirection = data.sortDirection;

            $(document).trigger('<%: WebConstants.EVENT_REPORTS_VIEW_REPORT %>',
                                data);
        });

        $('#BOEActivityReport-Export').click(function () {
            $('BOEActivityReport-ExportLoader').removeClass('display-none');
            $('BOEActivityReport-Export').addClass('display-none');

            // Remove the old hidden iFrame, if it exists
            $('#BOEActivityReport-DownloadTarget').remove();

            // Create a new hidden iFrame and set it's source to the chosen report's URL
            var targetIFrame = $('<iframe />', {
                'id': 'BOEActivityReport-DownloadTarget',
                'class': 'display-none',
                'src': CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_REPORTS %>',
                        '<%: WebConstants.ACTION_EXPORT %>',
                        'report/<%:(int)Reports.BOEActivity %>')
            });

            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            targetIFrame.appendTo('body');

            $('BOEActivityReport-ExportLoader').addClass('display-none');
            $('BOEActivityReport-Export').removeClass('display-none');

        });

        createModule($('#BOEActivityReport'));
        refreshModule($('#BOEActivityReport'));
    });

</script>


<div id="BOEActivityReport" class="module">
    <div class="module-header-data">BOE Activity</div>
    <div class="module-content-data">
        <div>
            <%:Model.DaysLeftUntilProposalSubmittalDate %><br /><br />
        </div>

        <div>
            <div class="boe-activityreport-left float-left">
                <table id="BOEActivityReportGrid" class="boe-activityreport-table grid readonly full">
                    <thead>
                        <tr>
                            <th class="boe-activityreport-wbsnum" sortby="wbsnum"><a>WBS #</a></th>
                            <th class="boe-activityreport-wbstitle" sortby="wbstitle"><a>WBS Title</a></th>
                            <th class="boe-activityreport-boetitle" sortby="boetitle"><a>BOE Title</a></th>
                            <th class="boe-activityreport-clinnum" sortby="clinnum"><a>CLIN #</a></th>
                            <th class="boe-activityreport-clintitle" sortby="clintitle"><a>CLIN Title</a></th>
                            <th class="boe-activityreport-author"><a>Authors</a></th>
                            <th class="boe-activityreport-status" sortby="status"><a>Status</a></th>
                            <th class="boe-activityreport-days-unassigned" sortby="days_unassigned"><a>Days in Unassigned</a></th>
                            <th class="boe-activityreport-days-draft" sortby="days_draft"><a>Days in Draft</a></th>
                            <th class="boe-activityreport-days-awaiting-approval" sortby="days_waiting"><a>Days in Awaiting Approval</a></th>
                            <th class="boe-activityreport-days-created-to-approved" sortby="days_create_approve"><a>Days from Created to Approved</a></th>
                            <th class="boe-activityreport-draft-to-approved" sortby="days_draft_approve"><a>Days from Draft to Approved</a></th>
                            <th class="boe-activityreport-times-in-draft" sortby="times_draft"><a># Times in Draft</a></th>
                            <th class="boe-activityreport-times-awaiting-approval" sortby="times_waiting"><a># Times in Awaiting Approval</a></th>
                            <th class="boe-activityreport-times-approved" sortby="times_approved"><a># Times in Approved</a></th>
                            <th class="boe-activityreport-times-authors-reassigned last-child" sortby="times_author_reassigned"><a># Times Authors Reassigned</a></th>                                  
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (GenBOE.Dtos.BOEActivityReportRow row in Model.Rows)
                           { %>
                        <tr>
                            <td class="boe-activityreport-wbsnum" title="<%:row.WBSNum%>">
                                <%: row.WBSNum%>
                            </td>
                            <td class="boe-activityreport-wbstitle" title="<%:row.WBSTitle%>">
                                <%: row.WBSTitle%>
                            </td>
                            <td style="white-space: nowrap;" class="boe-activityreport-boetitle" title="<%:row.BOETitle%>">
                                <%: row.BOETitle%>
                            </td>
                            <td class="boe-activityreport-clinnum" title="<%:row.CLINNum%>">
                                <%: row.CLINNum%>
                            </td>
                            <td class="boe-activityreport-clintitle" title="<%:row.CLINTitle%>">
                                <%: row.CLINTitle%>
                            </td>
                            <td style="white-space: nowrap;" class="boe-activityreport-author" title="<%:row.Authors%>">
                                <%: row.Authors%>
                            </td>
                            <td class="boe-activityreport-status" title="<%:row.Status%>">
                                <%: row.Status%>
                            </td>
                            <td class="boe-activityreport-days-unassigned" title="<%:row.DaysInUnassigned%>">
                                <%: row.DaysInUnassigned%>
                            </td>
                            <td class="boe-activityreport-days-draft" title="<%:row.DaysInDraft%>">
                                <%: row.DaysInDraft%>
                            </td>
                            <td class="boe-activityreport-days-awaiting-approval" title="<%:row.DaysInAwaitingApproval%>">
                                <%: row.DaysInAwaitingApproval%>
                            </td>
                            <td class="boe-activityreport-days-created-to-approved" title="<%:row.DaysFromCreatedToApproved%>">
                                <%: row.DaysFromCreatedToApproved%>
                            </td>
                            <td class="boe-activityreport-draft-to-approved" title="<%:row.DaysFromDraftToApproved%>">
                                <%: row.DaysFromDraftToApproved%>
                            </td>
                            <td class="boe-activityreport-times-in-draft" title="<%:row.NumTimesInDraft%>">
                                <%: row.NumTimesInDraft%>
                            </td>
                            <td class="boe-activityreport-times-awaiting-approval" title="<%:row.NumTimesInAwaitingApproval%>">
                                <%: row.NumTimesInAwaitingApproval%>
                            </td>
                            <td class="boe-activityreport-times-approved" title="<%:row.NumTimesInApproved%>">
                                <%: row.NumTimesInApproved%>
                            </td>
                            <td class="boe-activityreport-times-authors-reassigned" title="<%:row.NumTimesAuthorReassigned%>">
                                <%: row.NumTimesAuthorReassigned%>
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>

        <button id="BOEActivityReport-Export" class="ies" type="button">Export</button>
        <div id="BOEActivityReport-ExportLoader" class="loader display-none"></div>
    </div>
</div>