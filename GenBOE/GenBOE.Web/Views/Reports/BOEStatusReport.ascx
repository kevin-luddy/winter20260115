<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Dtos.BOEStatusReportModelView>>" %>

<script type="text/javascript">

    var BOEStatusReportGrid = new GridWidget("BOEStatusReportGrid");
    BOEStatusReportGrid.Module = {};

    $(function () {
        BOEStatusReportGrid.Module = $('#BOEStatusReportGrid');

        $('.boe-status-report-grid tbody tr[pkid]').click(function () {
            var boeID = $(this).attr('pkid');
            BOEStatusReportGrid.GoToSelectedBOE(boeID);
        });

        $('#BOEStatusReportGrid-Export').click(function () {
            // Remove the old hidden iFrame, if it exists
            $('#BOEStatusReportGrid-DownloadTarget').remove();

            // Create a new hidden iFrame and set it's source to the chosen report's URL
            var targetIFrame = $('<iframe />', {
                'id': 'BOEStatusReportGrid-DownloadTarget',
                'class': 'display-none',
                'src': CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_REPORTS %>',
                        '<%: WebConstants.ACTION_EXPORT %>',
                        'report/' + $('#BOEStatusReportGrid-View').val())
            });

            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            targetIFrame.appendTo('body');
        });

        $('#BOEStatusReportGrid-View').change(function () {
            $('#BOEStatusReportGrid-BOE, #BOEStatusReportGrid-WBS, #BOEStatusReportGrid-CLIN').hide();

            switch ($(this).val()) {
                case '<%: (int)Reports.BOEStatusByWBS %>':
                    $('#BOEStatusReportGrid-WBS').show();
                    break;

                case '<%: (int)Reports.BOEStatusByCLIN %>':
                    $('#BOEStatusReportGrid-CLIN').show();
                    break;

                default:
                    $('#BOEStatusReportGrid-BOE').show();
                    break;
            }

            refreshModule(BOEStatusReportGrid.Module);
        });

        SortableGrid('boe-status-report-grid');

        createModule(BOEStatusReportGrid.Module);
        refreshModule(BOEStatusReportGrid.Module);

        $('#BOEStatusReportGrid-View').change();
    });

    BOEStatusReportGrid.GoToSelectedBOE = function (boeID) {
        // Get the selected BOE ID 
        window.location = CreatePostURL(
        '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
        '<%: WebConstants.CONTROLLER_BOE %>',
        '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>',
        'boe/' + boeID);
    }


</script>
<div id="BOEStatusReportGrid" class="module">
    <div class="module-header-data">BOE Status</div>
    <div class="module-content-data">
    
        <div class="float-left">
            View
            <select id="BOEStatusReportGrid-View">
                <option value="<%: (int)Reports.BOEStatusByBOE %>" selected="selected">BOEs Only</option>
                <option value="<%: (int)Reports.BOEStatusByWBS %>">BOEs by WBS</option>
                <option value="<%: (int)Reports.BOEStatusByCLIN %>">BOEs by CLIN</option>
            </select>
        </div>

        <div class="buttons float-right">
            <button id="BOEStatusReportGrid-Export" class="ies" type="button">Export</button>
        </div>

        <div class="all-hours float-right">
            Total <%: ViewData["HoursLabel"]%> for all BOEs:
            <%: Model.Sum(m => m.TotalHours).ToString(Model.Any() ? Model.First().DecimalPrecisionStringFormat : "F0") %>
        </div>

        <div class="clear"></div>

        <div class="boe-status-report-grid">
            <%: Html.Partial("BOEStatusReportBOEOnly", Model) %>
            <%: Html.Partial("BOEStatusReportByCLIN", Model) %>
            <%: Html.Partial("BOEStatusReportByWBS", Model) %>
        </div>

    </div>
</div>
