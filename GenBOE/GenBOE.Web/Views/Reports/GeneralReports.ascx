<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.ExportsModelView>>" %>

<script type="text/javascript">

    var GeneralReportsWidget = new GridWidget("GeneralReports");

    $(function () {
        createModule($('.general-reports.module'));

        $('#GeneralReports a[name="GeneralReports-ViewButton"]').click(function () {
            var data = {};
            data.reportID = $(this).parents('tr').attr('pkid');
            $(document).trigger('<%: WebConstants.EVENT_REPORTS_VIEW_REPORT %>', data);
        });

        refreshModule($('.general-reports.module'));
        CollapsibleModule($('.general-reports.module'));
    });

</script>

<div id="GeneralReports" class="general-reports module expanded inner-collapsible">
    <div class="module-header-data">General Reports</div>

    <div class="module-content-data expanded-content">
        <table class="grid readonly">
            <thead>
                <tr>
                    <th class="report" style="width:175px">Report</th>
                    <th class="description">Description</th>
                    <th class="last-child action" style="width: 42px;">View</th>
                </tr>
            </thead>
            <tbody>
                <% foreach (var item in Model) { %>  
                <tr pkid="<%: item.ReportID %>" reportName="<%: item.ReportName %>" >
                    <td><span><%: item.ReportName %></span></td>
                    <td><span><%: item.Description %></span></td>
                    <td><span><a name="GeneralReports-ViewButton">View</a></span>
                    </td>
                </tr>
                <% } %>
            </tbody>
        </table>
    </div>
</div>