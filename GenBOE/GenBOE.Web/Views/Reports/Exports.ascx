<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.ExportsModelView>>" %>

<%@ Import Namespace="System.Collections.ObjectModel" %>

<%
    System.Collections.ObjectModel.Collection<GenBOE.Web.Controllers.SelectListItemWithTitle> outputFormatTemplates = ViewData["OutputFormatTemplates"] as System.Collections.ObjectModel.Collection<GenBOE.Web.Controllers.SelectListItemWithTitle>;

    Collection<SelectListItem> summarizeByCustomFieldOptions = (Collection<SelectListItem>)ViewData["SummarizeByCustomFieldOptions"];
    bool IsUsingSummarizeByCustomFieldTemplate = ViewData.ContainsKey("IsUsingSummarizeByCustomFieldTemplate") ? (bool)ViewData["IsUsingSummarizeByCustomFieldTemplate"] : false;
    bool supportCustomExport = ViewData.ContainsKey("SupportCustomExport") ? (bool)ViewData["SupportCustomExport"] : false;
    ICollection<string> workspaceAdmins = (ICollection<string>) this.ViewData["WorkspaceAdmins"];
    ICollection<string> outOfSyncMessages = (ICollection<string>) this.ViewData["OutOfSyncMessages"];
%>

<script type="text/javascript">

    var ExportsWidget = new GridWidget("Exports");
    var SummarizeByCustomFieldDialog;
    var IsRunningCustomExport = false;
    var IsPtmDataOutOfSyncDialog;

    // Create Discrepancy Validator.
    var validateDiscrepancyUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_REPORTS %>',
                '<%: WebConstants.ACTION_VALIDATE_BOES_FOR_DISCREPANCIES %>', '');
    var DiscrepancyValidator = InitializeDiscrepancyValidator(validateDiscrepancyUrl);

    ExportsWidget.doExport = function (reportName, reportID, ssrsUrl) {
        <% if (!Utilities.DisablePiwik()) { %>
        if (piwikTracker3) {
            piwikTracker3.trackEvent('Export', 'Report', reportName);
        }
        <% } %>
        if (reportID == '<%:(int)Reports.WorkbenchOffload%>') {
            var data = {};
            data.reportID = reportID;
            data.ssrsUrl = ssrsUrl;
            $(document).trigger('<%: WebConstants.EVENT_REPORTS_VIEW_REPORT %>', data);
        } else {
            if (reportID == '<%:(int)Reports.StandardReports%>') {
                // Standard Reports runs a number of SSRS reports, and the All BOEs export
                // Once the SSRS reports are opened, the rest of the function continues as usual for the All BOEs export

                document.getElementById('Report<%:(int)Reports.OffloadDetailedReport%>').click();

                $('tr[pkid=<%:(int)Reports.CostByClinResActYr%>]').children('td.reportLink').children('input').prop('checked', 'checked');
                $('tr[pkid=<%:(int)Reports.CostByClinActYr%>]').children('td.reportLink').children('input').prop('checked', 'checked');
                $('tr[pkid=<%:(int)Reports.CostByClinResActYrFlat%>]').children('td.reportLink').children('input.excelIcon').prop('checked', 'checked');

                ExportSelected();
            }

            var reportGenerationUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_REPORTS %>',
                '<%: WebConstants.ACTION_EXPORT %>',
                'report/' + reportID);

            if (reportID != <%:(int)Reports.AllBOEs%> && reportID != <%:(int)Reports.AllBOEsSegmented%> && reportID != <%:(int)Reports.StandardReports%>) {
                // Remove the old hidden iFrame, if it exists
                $('#Exports-DownloadTarget').remove();

                // Create a new hidden iFrame and set it's source to the chosen report's URL
                var targetIFrame = $('<iframe />', {
                        'id': 'Exports-DownloadTarget',
                        'class': 'display-none',
                        'src': reportGenerationUrl
                });

                // Append the iFrame to the body, causing the controller action to fire and
                // the download to occur inside the iFrame
                targetIFrame.appendTo('body');
            } else {
                // ALL BOEs report has been converted to memory stream the file and doesn't need iframe
                var summarizeByArgString = '';
                var summarizeByCustomFieldVal = $('#SummarizeByCustomField').val(); 
                if (SummarizeByCustomFieldDialog && summarizeByCustomFieldVal) {
                    // Include SummarizeByCustomField parameter
                    summarizeByArgString = '?SummarizeByCustomField=' + summarizeByCustomFieldVal;
                }
                window.location = reportGenerationUrl + summarizeByArgString;
            }
        }
    };

    ExportsWidget.LaunchBOECustomReportSelector = function () {
        // If the dialog is already created, then show it; otherwise, create a new one
        if ($('#CustomReportSelectorContainer').length > 0) {
            CustomReportSelectorWidget.OpenDialog();
        } else {
            var url = CreatePostURL('<%:SiteMasterUtilities.GetCurrentWorkspace()%>', '<%:WebConstants.CONTROLLER_REPORTS%>', '<%:WebConstants.ACTION_DISPLAY_BOE_CUSTOM_REPORT_SELECTOR%>');
            $('#BOECustomReportSelectorOuterContainer').load(url);
        }
    };

    ExportsWidget.ConfirmExport = function (reportName, reportID, ssrsUrl) {
        GenSession.confirmDialog("Export Report", "The export is a long running process. <br/>Please do not leave this page until the file is available to open/save. <br/>Continue with this export? <br/><br/>Please refrain from clicking the export link multiple times until the download is complete.",
            function () {
                if ((reportID === '3' || reportID === '<%:(int)Reports.AllBOEsSegmented%>' || reportID === '14' || reportID === '15' || reportID === '<%:(int)Reports.StandardReports%>' || reportID === '<%:(int)Reports.WbsBoeReport%>')) {
                    ExportsWidget.doExport(reportName, reportID);
                } else {
                    ExportsWidget.doExport(reportName, reportID, ssrsUrl);
                }
            }
            , null);
    };

    ExportsWidget.displaySummarizeByCustomFieldDialog = function (reportID) {
        $("#SummarizeByCustomField-Export").data("report-id", reportID);
        if (!SummarizeByCustomFieldDialog) {
            SummarizeByCustomFieldDialog = $("#SummarizeByCustomFieldDialog").dialog({ width: 530, modal: true, resizable: false, draggable: true, title: 'All BOEs Report: Summarize by Custom Field', autoOpen: false, close: ExportsWidget.closeSummarizeByCustomFieldDialog });
        }
        SummarizeByCustomFieldDialog.dialog('open');
    };

    ExportsWidget.closeSummarizeByCustomFieldDialog = function () {
        if (SummarizeByCustomFieldDialog) {
            SummarizeByCustomFieldDialog.dialog('close');
        }
    };

    ExportsWidget.closeIsPtmDataOutOfSyncDialog = function() {
        if (IsPtmDataOutOfSyncDialog) {
            IsPtmDataOutOfSyncDialog.dialog('close');
        }
    };

    ExportsWidget.processSummarizeByCustomFieldDialog = function (reportID) {
        // if running custom export, open Custom Report Selector dialog
        if (IsRunningCustomExport) {
            ExportsWidget.LaunchBOECustomReportSelector();
        } else {
            // run regular export
            ExportsWidget.doExport('Custom BOE Export', reportID);
        }
    };

    $(function () {
        createModule($('.exports.module'));

        // if we are exporting all BOEs (report id == 3,14,15) we need to run a validation check first and possibly check with the user since 
        // rates might not be 'valid' and they should be warned the calculations could be off
        $('#Exports a[name=Reports-ExportButton]').click(function () {
            IsRunningCustomExport = false;
            var reportID = $(this).parents('tr').attr('pkid');
            var reportName = $(this).parents('tr').attr('reportName');
                
            if (reportID == '<%:(int)Reports.INLFormsExport%>') {
                // If the dialog is already created, then show it; otherwise, create a new one
                if ($('#InlFormExportSelectorContainer').length > 0) {
                    INLReportSelectorWidget.OpenDialog();
                } else {
                    var url = CreatePostURL('<%:SiteMasterUtilities.GetCurrentWorkspace()%>', '<%:WebConstants.CONTROLLER_REPORTS%>', '<%:WebConstants.ACTION_DISPLAY_INL_FORM_EXPORT_GRID%>');
                    GenSession.ShowLoadingBox();
                    $('#InlFormExportSelectorOuterContainer').load(url, undefined, function () { GenSession.HideLoadingBox(); });
                }

                return;
            }

            // The All BOEs and Workspace Data reports must be checked for discrepancies prior to being run.
            if (reportID == '<%: (int)Reports.AllBOEs %>' || reportID == '<%: (int)Reports.AllBOEsSegmented %>' || reportID == '<%: (int)Reports.WorkspaceData %>')
            {
                // Establish the function to run if there are no discrepancies or if the user elects to proceed.
                var proceedWithExportFunction = function () {
                    ExportsWidget.ConfirmExport(reportName, reportID);
                };

                <%if (IsUsingSummarizeByCustomFieldTemplate) {%>
                    if (reportID === '<%: (int)Reports.AllBOEs %>') {
                        // override the proceedWithExportFunction to display the "Summarize by custom field" dialog
                        proceedWithExportFunction = function () {
                            ExportsWidget.displaySummarizeByCustomFieldDialog(reportID);
                        };
                    }
                <%}%>

                DiscrepancyValidator.ValidateDiscrepancies(proceedWithExportFunction);
            }
            else
            {
                // Discrepancies are ignored for all other exports.
                var ssrsUrl = $(this).parents('tr').attr('ssrsUrl')
                ExportsWidget.ConfirmExport(reportName, reportID, ssrsUrl);
            }
        });

        // exports the project map into an excel download
        $('#Exports a[name="ProjectMap-ViewButton"]').click(function (e) {
            e.stopPropagation();
            var offloadType = $(this).parents('tr').attr('offloadtype');
            var reportGenerationUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.ACTION_EXPORT_PROJECT_MAP %>',
                '?offload=' + offloadType);

            GenSession.confirmDialog("Export Report", "The export is a long running process. <br/>Please do not leave this page until the file is available to open/save. <br/>Continue with this export? <br/><br/>Please refrain from clicking the export link multiple times until the download is complete.",
                function () {
                    GenWidget.prototype.performExport(reportGenerationUrl);
                }
                , null);
        });

        $('#Exports a[name=Reports-CustomExportButton]').click(function () {
            IsRunningCustomExport = true;
            // The All BOEs Custom Export must be checked for discrepancies prior to being run.
            // Establish the function to run if there are no discrepancies or if the user elects to proceed.
            var proceedWithExportFunction = function () {
                <%if (IsUsingSummarizeByCustomFieldTemplate) {%>
                // open the SummarizeByCustomField dialog first, followed by the BOE CustomReportSelector dialog.
                IsRunningCustomExport = true;
                var reportID = $(this).parents('tr').attr('pkid');
                ExportsWidget.displaySummarizeByCustomFieldDialog(reportID);
                <%} else {%>
                ExportsWidget.LaunchBOECustomReportSelector();
                <%}%>
            };
            DiscrepancyValidator.ValidateDiscrepancies(proceedWithExportFunction);
        });

        // Decrease width of Action column for Project Map
        // No need for Custom Export, make more room for Standard Reports Description
        <% if (ViewBag.IsProjectMapWs)
           { %>
            $('#ActionHeader').css("width", "42px");
        <% }%>

        <% if (ViewBag.IsPtmDataOutOfSync)
           {%>
            if (!IsPtmDataOutOfSyncDialog) {
                IsPtmDataOutOfSyncDialog = $("#PtmDataOutOfSyncDialog").dialog({
                    width: 530,
                    modal: true,
                    resizable: false,
                    draggable: true,
                    title: 'PTM Data is out of date',
                    autoOpen: false,
                    close: ExportsWidget.closeIsPtmDataOutOfSyncDialog
                });
            }
            IsPtmDataOutOfSyncDialog.dialog('open');
         <%}%>

        refreshModule($('.exports.module'));
        CollapsibleModule($('.exports.module'));
    });

</script>

<div id="Exports" class="exports module expanded inner-collapsible">
    <div class="module-header-data">Exports</div>

    <div class="module-content-data expanded-content">
        <table class="grid readonly">
            <thead>
                <tr>
                    <th class="report">Report</th>
                    <th class="description">Description</th>
                    <th id="ActionHeader" class="last-child" style="width: 125px; text-align: right; padding-right: 15px;">Export</th>
                </tr>
            </thead>
            <tbody>
                <% foreach (var item in Model) { 
                    bool isAllBOEsReport = (item.ReportID == (int)Reports.AllBOEs);
                    
                    if (isAllBOEsReport) {%>
                        <tr pkid="<%: item.ReportID %>" reportName="<%: item.ReportName %>">
                            <td style="vertical-align: top;"><span><%: item.ReportName %></span></td>
                            <td>
                                <span><%: item.Description %></span>
                            </td>
                            <td style="text-align: right">
                                <span><a name="Reports-ExportButton">Export...</a>
                                <%if (supportCustomExport) {%>
                                    <% if (!ViewBag.IsProjectMapWs) { %>
                                      &nbsp;|&nbsp;<a name="Reports-CustomExportButton">Custom Export...</a>
                                    <%}%>
                                <%}%>
                                </span>
                            </td>
                        </tr>
                    <%} else {
                            string ssrs = string.Empty;
                            var ssrsItem = item as GenBOE.ActionLogic.ModelView.SSRSReportsModelView;
                            if (ssrsItem != null)
                            {
                                ssrs = "ssrsUrl=" + ssrsItem.ReportUrl;
                            }
                            %>
                    <tr pkid="<%: item.ReportID %>" <%:ssrs %> reportName="<%: item.ReportName %>">
                        <td><span><%: item.ReportName %></span></td>
                        <td><span><%: item.Description %></span></td>
                        <td style="text-align: right">
                            <%if (item.ReportID == (int)Reports.AllBOEs) { %>
                                <span><a name="Reports-ExportButton">Export...</a>
                                    <%if (supportCustomExport) {%>
                                        &nbsp;|&nbsp;<a name="Reports-CustomExportButton">Custom Export...</a>
                                    <%}%>
                                </span>
                            <% } else { %>
                                <span><a name="Reports-ExportButton">Export...</a></span>
                            <% } %>
                        </td>
                    </tr>
                <%}%>
            <% } // end foreach 
                if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST) {
                %>
                <tr offloadtype="false">
                    <td><span>Project Map Export</span></td>
                    <td><span>Exports all BOEs to Project Map format.</span></td>
                    <td style="text-align: right"><span><a name="ProjectMap-ViewButton">Export...</a></span>
                    </td>
                </tr>
                <tr offloadtype="true">
                    <td><span>Project Map Export with Offloading</span></td>
                    <td><span>Exports all BOEs after Offloading to Project Map format.</span></td>
                    <td style="text-align: right"><span><a name="ProjectMap-ViewButton">Export...</a></span>
                    </td>
                </tr>
                <% } %>
            </tbody>
        </table>
    </div>
</div>

<div id="SummarizeByCustomFieldDialog" style="display: none">
    <form id="SummarizeByCustomFieldForm">
        <div class="form-row center">The export is a long running process. <br/>Please do not leave this page until the file is available to open/save.</div>
        <% if (summarizeByCustomFieldOptions == null || !summarizeByCustomFieldOptions.Any()) { %>
            <div class="form-row center error">Warning: This report was configured to summarize by custom field set at the "Resource Types" level.<br/>No custom fields were detected, at the "Resource Types" level, in this workspace...</div>
        <% } else  { %>
            <div class="form-row center">This report was configured to summarize by custom field set at the "<b><i>Resource Types</i></b>" level.</div>
            <div class="form-row center" style="font-size: 12px;">
                <div class="form-label">Please select one custom field for summarization: </div>
                <div class="form-element">
                    <%: Html.DropDownList("SummarizeByCustomField", (IEnumerable<SelectListItem>)ViewData["SummarizeByCustomFieldOptions"])%>
                </div>
            </div>
        <% } %>
        <div class="form-row center">Continue with this export?<br/><br/>Please refrain from clicking the export link multiple times until the download is complete.</div>

        <div class="buttons center">
            <button id="SummarizeByCustomField-Export" class="ies" data-report-id="" onclick="ExportsWidget.closeSummarizeByCustomFieldDialog(); ExportsWidget.processSummarizeByCustomFieldDialog($(this).data('report-id'));" name="yes-button" type="button">Yes</button>
            <button id="SummarizeByCustomField-Cancel" class="ies" onclick="ExportsWidget.closeSummarizeByCustomFieldDialog();" name="no-button" type="button">No</button>
        </div>
    </form>
</div>

<div id="PtmDataOutOfSyncDialog" style="display: none">
    <form id="PtmDataOutOfSyncForm">
        <div class="form-row">The following PTM fields are out of date and should be updated:</div>
        <ul>
            <% foreach (string message in outOfSyncMessages)
                { %>
                <li><%:message%></li>
            <% }%>
        </ul>
        <br />
        <div class="form-row">Please contact this Workspace's Administrators to update these fields:</div>
        <ul>
            <% foreach (string admin in workspaceAdmins)
                { %>
                <li><%:admin%></li>
            <% }%>
        </ul>
        <br />
        <div class="buttons right">
            <button id="PtmDataOutOfSync-cancel" class="ies" onclick="ExportsWidget.closeIsPtmDataOutOfSyncDialog()" type="button">OK</button>
        </div>
    </form>
</div>

<span id="hiddenFilterByValue" class='display-none'></span>
<span id="hiddenSecondaryFilterByValue" class="display-none"></span>

<div id="BOECustomReportSelectorOuterContainer"></div>
<div id="InlFormExportSelectorOuterContainer"></div>
