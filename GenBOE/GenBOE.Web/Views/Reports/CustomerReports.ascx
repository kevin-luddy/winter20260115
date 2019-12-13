<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ICollection<GenBOE.ActionLogic.ModelView.SSRSReportsModelView>>" %>

<script type="text/javascript">

    var CustomerReportsWidget = new GridWidget("CustomerReports");

    $(function () {
        createModule($('.customer-reports.module'));

        $('#CustomerReports a[name="CustomerReports-ViewButton"]').click(function () {
            var data = {};
            data.reportID = $(this).parents('tr').attr('pkid');
            data.ssrsUrl = $(this).parents('tr').attr('ssrsUrl');
            $(document).trigger('<%: WebConstants.EVENT_REPORTS_VIEW_REPORT %>', data);
        });

        $('#CustomerReports img.wordIcon').click(function () {
            $('#CustomerReports input.wordIcon').attr('checked', true);
        });

        $('#CustomerReports img.excelIcon').click(function () {
            $('#CustomerReports input.excelIcon').attr('checked', true);
        });

        $('#CustomerReports i.pdfIcon').click(function () {
            $('#CustomerReports input.pdfIcon').attr('checked', true);
        });

        refreshModule($('.customer-reports.module'));
        CollapsibleModule($('.customer-reports.module'));
    });

</script>

<div id="CustomerReports" class="customer-reports module expanded inner-collapsible">
    <div class="module-header-data">Customer Reports</div>

    <div class="module-content-data expanded-content">
        <table class="grid readonly">
            <thead>
                <tr>
                    <th class="reportLink"><img src="/Resources/css/images/word.png" class="reportIcon wordIcon" /></th>
                    <th class="reportLink"><img src="/Resources/css/images/excel.png" class="reportIcon excelIcon" /></th>
                    <th class="reportLink"><i class="fa fa-file-pdf-o pdfIcon" style="font-size:16px;color:red"></i></th>
                    <th class="report" style="width:250px">Report</th>
                    <th class="description">Description</th>
                    <th class="last-child action" style="width: 42px;">View</th>
                </tr>
            </thead>
            <tbody>
                <% foreach (var item in this.Model) { %>  
                <tr pkid="<%: item.ReportID %>" reportName="<%: item.ReportName %>" ssrsUrl="<%: item.ReportUrl %>">
                    <td class="reportLink"><input class="wordIcon" type="checkbox" value="word" /></td>
                    <td class="reportLink"><input class="excelIcon" type="checkbox" value="excel" /></td>
                    <td class="reportLink"><input class="pdfIcon" type="checkbox" value="pdf" /></td>
                    <td><span title="<%: item.ReportName %>"><%: item.ReportName %></span></td>
                    <td title="<%: item.Description %>"><span><%: item.Description %></span></td>
                    <td><span><a name="CustomerReports-ViewButton" id="Report<%: item.ReportID %>">View</a></span></td>
                </tr>
                <% } %>
            </tbody>
        </table>
    </div>
</div>