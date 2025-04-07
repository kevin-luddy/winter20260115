<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<WorkspaceVersionModelView>>" %>


<script type="text/javascript">
    ManageBackupVersionsGridWidget = new Widget('ManageBackupVersionsGrid', <%= ViewData["READONLY"] %>);

    ManageBackupVersionsGridWidget.OpenExport = function (versionID) {
        var versionRow = $("table#ManageBackupVersionsGridTable tr[pkid=" + versionID + "]");
        var text = "Exporting version: " + versionRow.attr("name");
        $("#versionText").text(text);
        $("#versionText").attr("pkid", versionID);

        $("table#BoeToExportGrid tbody tr").each(function () { $(this).addClass("display-none") });
        $("table#BoeToExportGrid tr[version=" + versionID + "]").each(function () { $(this).removeClass("display-none") });

        $("#ExportDialog input:radio").each(function () { $(this).prop('checked', false); });
        $("#ExportDialog input:checkbox").each(function () { $(this).prop('checked', false); });
        $("#ExportDialog button[name='export-button']").addClass("disabled");

        $("#exportErrorMessage").text("").hide();
        $("#boes-grid").addClass("display-none");

        ManageBackupVersionsGridWidget.OpenDialogAfterInitialize(ManageBackupVersionsGridWidget.ExportDialog);
    };

    ManageBackupVersionsGridWidget.DoExport = function () {
        $("#ExportDialog button[name='export-button']").addClass("display-none");
        $("#ExportDialog #ExportSpinner").removeClass("display-none");
        $("#exportErrorMessage").text("").hide();

        var errorText = "";

        // validate Scope was selected
        if ($("div.selections:not(:has(:radio:checked))").length) {
            errorText += "A Scope must be selected.";
        };

        // if Select BOEs - verify at least 1 selected
        if ($("#SelectBoes").is(":checked") && $("#ExportDialog input:checkbox:checked").length === 0) {
            if (errorText.length > 0) {
                errorText += "\n\n";
            }

            errorText += "At least one BOE must be selected for a Scope of Select BOEs.";
        }

        if (errorText.length > 0) {
            $("#exportErrorMessage").text(errorText).show();
        } else {
            var versionId = $("#versionText").attr("pkid");
            var fullWsExport = $("#FullWs").is(":checked");
            var selectedBoes = $("#ExportDialog tr[version='" + versionId + "'] input:checkbox:checked");

            var reportGenerationUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_EXPORT_WORKSPACE_VERSION %>', '?versionId=' + versionId +
                        '&exportAllBoes=' + fullWsExport);

            selectedBoes.each(function () {
                reportGenerationUrl += "&boesToExport=" + $(this).attr("pkid");
            });

            // Create a new hidden iFrame and set it's source to the chosen report's URL
            var targetIFrame = $('<iframe />', {
                'id': 'Exports-DownloadTarget',
                'class': 'display-none',
                'src': reportGenerationUrl
            });

            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            targetIFrame.appendTo('body');
        }
        $("#ExportDialog button[name='export-button']").removeClass("display-none");
        $("#ExportDialog #ExportSpinner").addClass("display-none");

        if (errorText.length === 0) {
            ManageBackupVersionsGridWidget.CloseDialog(ManageBackupVersionsGridWidget.ExportDialog);
        }
    };

    $(function () {
        ManageBackupVersionsGridWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { ManageBackupVersionsGridWidget.cleanDirty(); });

        $('#ManageBackupVersionsGridTable a[name=Restore]').click(function() {
            ManageBackupVersionsWidget.ConfirmRestore($(this).parents('tr').attr('pkid'));
        });

        $('#ManageBackupVersionsGridTable a[name=Export]').click(function () {
            ManageBackupVersionsGridWidget.OpenExport($(this).parents('tr').attr('pkid'));
        });

        $('#ManageBackupVersionsGridTable input[name=DeleteVersion]').click(function() {
            ManageBackupVersionsWidget.enableDelete();
        });

        $('#ManageBackupVersionsGridTable input#DeleteAllVersion').click(function() {
            if($('#ManageBackupVersionsGridTable input#DeleteAllVersion').is(':checked'))
            {
                $("div#ManageBackupVersionsContainer input[name=DeleteVersion]").prop("checked",true);
            }else{
                $("div#ManageBackupVersionsContainer input[name=DeleteVersion]").prop("checked", false);
            }
            ManageBackupVersionsWidget.enableDelete();
        });

        ManageBackupVersionsGridWidget.ExportDialog = {};
        ManageBackupVersionsGridWidget.ExportDialog.Element = $('#ExportDialog');
        ManageBackupVersionsGridWidget.ExportDialog.Params = {
            width: 600, modal: true, resizable: false, draggable: true,
            title: "Export Workspace Version", position: {at: "center center-15%"}
        };

        ManageBackupVersionsGridWidget.InitializeDialog(ManageBackupVersionsGridWidget.ExportDialog);

        $("#BoeToExport-SelectAllCheckbox").change(function () {
            if (this.checked) {
                // $("#ExportDialog tr[version=''] input:checkbox").each(function () { $(this).prop('checked', true); });
                $("#ExportDialog input:checkbox").each(function () { $(this).prop('checked', true); });
            } else {
                $("#ExportDialog input:checkbox").each(function () { $(this).prop('checked', false); });
            }
        });

        $("#ExportDialog tbody input:checkbox").change(function () {
            // uncheck the select all checkbox if another checkbox is unchecked
            if (!this.checked) {
                $("#BoeToExport-SelectAllCheckbox").prop('checked', false);
            }
        })

        $("#ExportDialog button[name='export-button']").click(function () {
            if (!$(this).hasClass("disabled")) {
                GenSession.confirmDialog("Export Report", "The export is a long running process. <br/>Please do not leave this page until the file is available to open/save. <br/>Continue with this export? <br/><br/>Please refrain from clicking the export link multiple times until the download is complete.",
                    function () { ManageBackupVersionsGridWidget.DoExport() }, null );                
            }
        });

        $("#ExportDialog button[name='cancel-button']").click(function () {
            ManageBackupVersionsGridWidget.CloseDialog(ManageBackupVersionsGridWidget.ExportDialog);
        });

        $("#ExportDialog #FullWs").click(function () {
            $("#boes-grid").addClass("display-none");
        });

        $("#ExportDialog #SelectBoes").click(function () {
            $("#boes-grid").removeClass("display-none");
        });

        $("#ExportDialog input:radio").click(function () {
            var scopeSelected = $("#ExportDialog input[name='Scope']:checked").length;

            // if both have selections made, enable export button
            if (scopeSelected === 1) {
                $("#ExportDialog button[name='export-button']").removeClass("disabled");
            }
        });

		if (<%= ViewData["ContainsOCI"] %>) {
			var text = '<%: SiteMasterUtilities.GetBannerText(true) %>';
            $('.oci-note').html('<b>Note:</b> ' + text);
        }
		else {
			var text = '<%: SiteMasterUtilities.GetBannerText() %>';
            $('.oci-note').html('<b>Note:</b> ' + text);
        }
    });
</script>

<div id="ManageBackupVersionsGrid">
    <table id="ManageBackupVersionsGridTable" class="grid readonly">
        <thead>
            <tr>
                <th class="delete-checkbox"><input type="checkbox" id="DeleteAllVersion" /></th>                   
                <th class="versionID">Version Name</th>
                <th class="dateCreated">Date Created</th>
                <th class="createdBy">Created By</th>
                <th class="restore">Restore</th>
                <th class="export last-child">Export<div id="PageControls"></div></th>
            </tr>
        </thead>
        <tbody>
            <% foreach (WorkspaceVersionModelView item in Model) {%>
                <tr pkid="<%: item.VersionID %>" name="<%: item.VersionName%>" data-restore-version="<%: item.RestoreToWorkspaceState %>">
                    <td class="delete-checkbox">
                        <%if (ViewBag.IsSystemAdmin || !item.IsSystemBackup) { %>
                        <input type="checkbox" name="DeleteVersion" />
                        <% } %>
                    </td>
                    <td name="versionName">
                        <%:item.VersionName %>
                    </td>
                    <td class="" nowrap="nowrap">
                        <%: item.DateCreated %>
                    </td>
                    <td class="">
                        <%: item.CreatedByDisplayName %>
                    </td>
                    <td class="">
                        <a name="Restore">Restore</a>
                    </td>
                    <td class="">
                        <a name="Export">Export</a>
                    </td>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>

<div id="ExportDialog" style="display: none">
    <div class="container">
        <div class="form-row">
            <div>Export All BOEs (Word) and Workspace Data (Excel) Reports for a previous version.</div>
            <div id="versionText" pkid=""></div>
        </div>
        <div id="exportErrorMessage" class="validation-box"></div>
        <div class="form-row">
            <div class="form-label">Select Scope</div>
            <div class="form-element selections">
                <div>
                    <input id="FullWs" type="radio" name="Scope" />
                    <label>Entire Workspace</label>
                </div>
                <div>
                    <input id="SelectBoes" type="radio" name="Scope" />
                    <label>Select BOEs</label>
                </div>
            </div>
        </div> 
        <div id="boes-grid" class="display-none">
            <table id="BoeToExportGrid" class="boes-to-export-grid readonly grid" style="width: 100%;">
            <colgroup>
                <col width="5%"/>
                <col width="15%"/>
                <col width="20%"/>
                <col width="25%"/>
                <col width="15%"/>
                <col/>
            </colgroup>
            <thead>
                <tr>
                    <th class="select-checkbox">
                        <input type="checkbox" id="BoeToExport-SelectAllCheckbox" />
                    </th>
                    <th class="wbsnum">
                        WBS #
                    </th>
                    <th class="wbs">
                        WBS
                    </th>
                    <th class="boetitle">
                        BOE Title
                    </th>
                    <th class="clinnum">
                        CLIN #
                    </th>
                    <th class="clin">
                        CLIN
                    </th>
                </tr>
            </thead>
            <tbody>
                <% if (Model != null)
                    {
                        foreach (WorkspaceVersionModelView model in Model)
                        {
                            foreach (GenBOE.Dtos.BoeVersionDTO item in model.Boes)
                            { %>
                                <tr pkid="<%: item.BoeId %>" version="<%: model.VersionID %>" class="display-none">
                                    <td>
                                        <input type="checkbox" class="checkbox" pkid="<%: item.BoeId %>" />
                                    </td>
                                    <td title="<%: item.WbsNumber %>">
                                        <%: item.WbsNumber %>
                                    </td>
                                    <td title="<%: item.Wbs %>">
                                        <%: item.Wbs %>
                                    </td>
                                    <td title="<%: item.BoeTitle %>">
                                        <%: item.BoeTitle %>
                                    </td>
                                    <td title="<%: item.ClinNumber %>">
                                        <%: item.ClinNumber %>
                                    </td>
                                    <td title="<%: item.Clin %>">
                                        <%: item.Clin %>
                                    </td>
                                </tr>
                            <% }
                         }
                    } %>
            </tbody>
        </table>
        </div>
        <br />
        <div class="oci-note"></div>
        <div>
            <button id="ExportButton" class="ies-action disabled" name="export-button" type="button">Export</button>
            <div id="ExportSpinner" class="loader display-none"></div>
            <button id="CancelExportButton" class="ies" name="cancel-button" type="button">Cancel</button>
         </div>
    </div>
</div>