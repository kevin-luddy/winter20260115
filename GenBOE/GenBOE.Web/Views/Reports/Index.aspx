<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="IES.Common.classes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Reports - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Scripts.Render("~/bundles/discrepancy") %>
    <script type="text/javascript">
        var Reports = new Widget();
        var nonceUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_REPORTS %>',
                '<%: WebConstants.ACTION_GENERATE_REPORT_NONCE %>',
                '');

        var bulkDownloadUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_REPORTS %>',
                '<%: WebConstants.ACTION_BULK_DOWNLOAD_REPORT %>',
                '');

        $(function () {
            $('.main').addClass('reports');

            createModule($('.export-button-module.module'));
            refreshModule($('.export-button-module.module'));

            createModule($('.export-button-module2.module'));
            refreshModule($('.export-button-module2.module'));
        });

        function ExportSelected () {
            var checkedLinks = $('td.reportLink input:checked');
            if (checkedLinks.length > 0) {
                $('#PageLoading').removeClass('display-none');
                var dataToSend = {};
                dataToSend.downloadReports = [];
                checkedLinks.each(function() {
                    // first get the Nonce from the backend
                    var reportID = $(this).parents('tr').attr('pkid');
                    var ssrsUrl = $(this).parents('tr').attr('ssrsUrl');
                    var format = $(this).val();

                    var data = {"ReportType": reportID.toString(), "SSRSLink": ssrsUrl, "FormatType": format };
                    dataToSend.downloadReports.push(data);
                });

                // Use XMLHttpRequest instead of Jquery $ajax to download a blob
                var xhttp = new XMLHttpRequest();
                xhttp.onreadystatechange = function() {
                    var a;
                    if (xhttp.readyState === 4) {
                        $('#PageLoading').addClass('display-none');
                        if (xhttp.status === 200) {
                            // uncheck the checked reports
                            checkedLinks.attr('checked', false); 

                            if(window.navigator.msSaveOrOpenBlob) {
                                window.navigator.msSaveOrOpenBlob(xhttp.response, "BulkDownload_<%: SiteMasterUtilities.GetCurrentWorkspace() %>.zip");
                            } else {
                                // Trick for making downloadable link
                                a = document.createElement('a');
                                a.href = window.URL.createObjectURL(xhttp.response);
                                // Give filename you wish to download
                                a.download = "BulkDownload_<%: SiteMasterUtilities.GetCurrentWorkspace() %>.zip";
                                a.style.display = 'none';
                                document.body.appendChild(a);
                                a.click();
                            }
                        } else {
                            RaiseNotification('An error occured during generation of Report');
                        }
                    } 
                };

                // Post data to URL which handles post request
                xhttp.open("POST", bulkDownloadUrl);
                xhttp.setRequestHeader("Content-Type", "application/json");
                // You should set responseType as blob for binary responses
                xhttp.responseType = 'blob';
                xhttp.send(JSON.stringify(dataToSend));
            }
        }; 

        Reports.registerForEvent('<%:WebConstants.EVENT_REPORTS_VIEW_REPORT %>', function (e, params) {
            if (params.reportID == <%: (int)Reports.BOEStatus %>)
            {
                $('#report').children().remove();
                $('#report').html('<div class="loader"></div>');

                reportUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_REPORTS %>',
                        '<%: WebConstants.ACTION_DISPLAY_BOE_STATUS_REPORT %>',
                        '');

                $.ajax({
                    type: "POST",
                    url: reportUrl,
                    dataType: 'html',
                    success: function (response) {
                        $('#report').html(response);
                    }
                });
            }
            else if (params.reportID == <%: (int)Reports.BOEActivity %>)
            {
                $('#report').children().remove();
                $('#report').html('<div class="loader"></div>');

                $.ajax({
                    type: "POST",
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                                '<%: WebConstants.CONTROLLER_REPORTS %>',
                                                '<%: WebConstants.ACTION_DISPLAY_BOE_ACTIVITY_REPORT %>',
                                                ''),
                            contentType: 'application/json; charset=utf-8',
                            data: JSON.stringify(params),
                            dataType: 'html',
                            success: function (response) {
                                $('#report').html(response);
                            }
                });
            }
            else if (params.reportID == <%: (int)Reports.WorkspaceActivity %>)
            {
                $('#report').children().remove();
                $('#report').html('<div class="loader"></div>');

                reportUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                            '<%: WebConstants.CONTROLLER_REPORTS %>',
                            '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_ACTIVITY_REPORT %>',
                            '');

                $.ajax({
                    type: "POST",
                    url: reportUrl,
                    dataType: 'html',
                    success: function (response) {
                        $('#report').html(response);
                    }
                });
            }
            else if (params.reportID == <%: (int)Reports.BoeDiscrepancy %>)
            {
                $('#report').children().remove();
                $('#report').html('<div class="loader"></div>');

                reportUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_REPORTS %>',
                    '<%: WebConstants.ACTION_DISPLAY_BOE_DISCREPANCY %>',
                    '');

                $.ajax({
                    type: "POST",
                    url: reportUrl,
                    dataType: 'html',
                    success: function (response) {
                        $('#report').html(response);
                    }
                });
            }
            else if (params.reportID == <%: (int)Reports.ValidateAllBOE %>)
            {
                $('#report').children().remove();
                $('#report').html('<div class="loader"></div>');

                reportUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_REPORTS %>',
                    '<%: WebConstants.ACTION_DISPLAY_VALIDATE_ALL_BOE %>',
                    '');

                $.ajax({
                    type: "POST",
                    url: reportUrl,
                    dataType: 'html',
                    success: function (response) {
                        $('#report').html(response);
                    }
                });
			}
			else if (params.reportID == <%: (int)Reports.ConfidenceReport %>)
			{
				$('#report').children().remove();
				$('#report').html('<div class="loader"></div>');

				reportUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
					'<%: WebConstants.CONTROLLER_REPORTS %>',
					'<%: WebConstants.ACTION_DISPLAY_BOE_CONFIDENCE_REPORT_RESULTS %>',
					'');

				$.ajax({
					type: "POST",
					url: reportUrl,
					dataType: 'html',
					success: function (response) {
						$('#report').html(response);
					}
				});
			}
            else { // SSRS reports
                // first get the Nonce from the backend
                var nonceUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_REPORTS %>',
                '<%: WebConstants.ACTION_GENERATE_REPORT_NONCE %>',
                '');

                var dataToSend = JSON.stringify({ "reportType": params.reportID.toString() });
                $('#PageLoading').removeClass('display-none');
                $.ajax({
                    type: 'POST',
                    url: nonceUrl,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function (response) {
                        $('#PageLoading').addClass('display-none');
                        // kickoff download of ssrs report
                        var ssrsUrl = params.ssrsUrl + response.Nonce;
                        if (params.reportID == <%: (int)Reports.WorkbenchOffload %>){
                            // just download the word export directly
                            GenWidget.prototype.performExport(ssrsUrl);
                        } else {
                            // open the download in a new window
                            window.open(ssrsUrl, '_blank', 'resizable=yes, location=yes');
                        }
                    },
                    error: function (response) {
                        $('#PageLoading').addClass('display-none');
                        RaiseNotification('An error occured during generation of Report');
                    }
                });

            }
        }); 
	</script>
    <div id="report">
        <div class="module">
            <div class="module-header">
                <div class="module-header-left">
                </div>
                <div class="module-header-center">
                    <div class="module-header-data">
                        Reports
                    </div>
                </div>
                <div class="module-header-right">
                </div>
            </div>
        </div>
        <%  Html.RenderAction(WebConstants.ACTION_DISPLAY_EXPORTS);
            if ((bool)ViewData["DisplayProjectMapOnly"] == false)
            {
                Html.RenderAction(WebConstants.ACTION_DISPLAY_GENERAL_REPORTS);
            }

            if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
            {%>
                <div class="export-button-module module inner-collapsible">
                    <div class="module-header-data">
                        <button class="ies-action exportReportButton" id="Export-SelectedReports" onclick="ExportSelected()" type="button">Export Selected</button>
                    </div>
                </div>
              <%
                  // Html.RenderAction(WebConstants.ACTION_DISPLAY_SUMMARY_REPORTS);
                  Html.RenderAction(WebConstants.ACTION_DISPLAY_CUSTOMER_REPORTS);
                  Html.RenderAction(WebConstants.ACTION_DISPLAY_FINANCE_REPORTS);
                  Html.RenderAction(WebConstants.ACTION_DISPLAY_ADDITIONAL_REPORTS);
              %>
                <div class="export-button-module2 module inner-collapsible">
                    <div class="module-header-data">
                        <button class="ies-action exportReportButton" id="Export-SelectedReports2" onclick="ExportSelected()" type="button">Export Selected</button>
                    </div>
                </div>
            <% }%>
        <div class="report-footer">
            <div class="module-footer-left">
            </div>
            <div class="module-footer-center">
            </div>
            <div class="module-footer-right">
            </div>
        </div>
    </div>

    <% Html.RenderAction(WebConstants.ACTION_DISPLAY_WORKSPACE_UPDATE_RATES_DIALOG, WebConstants.CONTROLLER_WORKSPACE, new { useCookie = false }); %>
</asp:Content>
