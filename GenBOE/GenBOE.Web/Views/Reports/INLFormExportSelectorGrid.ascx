<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ICollection<GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView>>" %>

<script type="text/javascript">
    var INLReportSelectorWidget;

    InitializeInlReportSelector = function () {
        var DialogConfigs = [];

        DialogConfigs.push({
            ElementID: "InlFormExportSelectorContainer",
            Params: {
                width: 850,
                title: "INL Forms: Export IBOE / PBOE Forms",
                disabled: false,
                modal: true,
                resizable: false,
                draggable: true,
                closeOnEscape: false
            }
        });
        var FormConfigs = [];
        FormConfigs.push({
            ElementID: "InlFormExportForm",
            Buttons: [{
                ButtonClass: 'ies-action',
                ButtonText: 'Export',
                ButtonName: "export-button",
                Stateful: false,
                ButtonAction: function (buttonPressed) {
                    var exportValidationUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                            '<%: WebConstants.CONTROLLER_BOE_FORMS %>',
                            '<%: WebConstants.ACTION_VALIDATE_INL_FORM_TM_RESOURCES %>',
                            '?' + $("#InlFormExportForm tr.checked input:hidden").serialize());
                    $.ajax({
                        type: 'POST',
                        url: exportValidationUrl,
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json',
                        data: '',
                        success: function (valid) {
                            if (valid.status) {
                                INLReportSelectorWidget.doExport();
                            } else {
                                Session.alertDialog("Export Validation", INLReportSelectorWidget.GenerateValidationMessageMarkup(valid.validationErrors));
                            }
                        }
                    });
                }
			}],
			HideOCI: false,
			// We Load both OCI and NON OCI texts so that the Generation.JS will use the ContainsOCI to display the correct text
			BannerTextWithOCI: '<%: SiteMasterUtilities.GetBannerText() %>',
			BannerTextWithoutOCI: '<%: SiteMasterUtilities.GetBannerText(true) %>'
		});

        var widgetConfig = {};
        widgetConfig.ContextID = "InlFormExportSelectorDialog";
        widgetConfig.IsModule = false;
        widgetConfig.DialogConfigs = DialogConfigs;
        widgetConfig.FormConfigs = FormConfigs;

        INLReportSelectorWidget = new GenWidget(widgetConfig);

        INLReportSelectorWidget.OpenDialog = function () {
            INLReportSelectorWidget.ClearState();
            INLReportSelectorWidget.getDialog("InlFormExportSelectorContainer").openDialog();
        };

        INLReportSelectorWidget.doExport = function () {
            var reportGenerationUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                            '<%: WebConstants.CONTROLLER_BOE_FORMS %>',
                            '<%: WebConstants.ACTION_EXPORT_INL_FORMS %>',
                            '?' + $("#InlFormExportForm tr.checked input:hidden").serialize());

            <% if (!Utilities.DisablePiwik()) { %>
            if (piwikTracker3) {
                piwikTracker3.trackEvent('Export', 'INL Forms', reportGenerationUrl);
            }
            <% } %>
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

            // Now close the dialog
            INLReportSelectorWidget.getDialog("InlFormExportSelectorContainer").closeDialog();

        };

        INLReportSelectorWidget.ExportCheckboxToggled = function () {
            if ($("#BOEFormsGrid input:checkbox:checked").length === 0) {
                $("#InlFormExportForm button[name='export-button']").addClass('disabled');
            }
            else {
                $("#InlFormExportForm button[name='export-button']").removeClass('disabled');
            };
        }

        // generate markup showing validation errors as an unordered list
        INLReportSelectorWidget.GenerateValidationMessageMarkup = function (validationErrors) {
            if (validationErrors && validationErrors.length) {
                var markup = $("<div style='text-align: left'>Unable to export INL forms due to the following issues:</div>");
                $('<br/>').appendTo(markup);
                var ul = $('<ul style="text-align: left"/>');
                $.each(validationErrors, function (i) {
                    $('<li/>').text(validationErrors[i].ValidationIssue).appendTo(ul);
                });
                ul.appendTo(markup);
                return markup;
            } else {
                return 'Unable to export INL forms due to errors.'; // should never get here, but add generic message just in case.
            }
        }

        // set state of window to its Initial(brand new) state
        INLReportSelectorWidget.ClearState = function () {
            $("input[type='checkbox']:checked").removeAttr('checked');
            $("#InlFormExportForm button[name='export-button']").addClass('disabled');
        }

        $('#CheckAllBoeForms').change(function () {
            $('#BOEFormsGrid tbody input[name="ExportBoeForm"]').not(':disabled').prop('checked', $('#CheckAllBoeForms').prop('checked'));
            if ($('#CheckAllBoeForms').prop('checked')) {
                $('#BOEFormsGrid tbody tr').not(".trdisabled").addClass("checked");
            } else {
                $('#BOEFormsGrid tbody tr').removeClass("checked");
            }
            INLReportSelectorWidget.ExportCheckboxToggled();
        });

        $("#BOEFormsGrid tbody :input").change(function () {
            INLReportSelectorWidget.ExportCheckboxToggled();
            if (this.checked) {
                $(this).parents('tr').addClass("checked");
            } else {
                $(this).parents('tr').removeClass("checked");
            }
        });

        // Set initial state of export button to disabled
        $("#InlFormExportForm button[name='export-button']").addClass('disabled');
        INLReportSelectorWidget.OpenDialog();

        // click event for editing an incomplete item
        $('#InlFormExportForm a.edit-boe-forms-link').click(function () {
            var $row = $(this).parents('tr');
            var id = $row.attr('data-pkid');
            var type = $row.attr('data-formType');
            var url = "/<%: SiteMasterUtilities.GetCurrentWorkspace() %>/BOEForm/UpdateForm?boeFormId=" + id + "&boeFormType=" + type;
            window.onbeforeunload = null;
            window.location.href = url; 
        });
    }

    $(function () {
        InitializeInlReportSelector();
        SortableGrid('.inl-form-selector-grid');
    });
</script>

<div id="InlFormExportSelectorContainer" class="display-none">
    <div id="InlFormExportSelectorDialog" class="custom-report-selector-dialog">
        <%using (Html.BeginForm("", "", FormMethod.Post, new { id = "InlFormExportForm" })) {%>
        <%: Html.ValidationSummary() %>
        <div class="container">
            <div class="inl-form-selector-grid">
                <table id="BOEFormsGrid" class="sortable grid readonly">
                    <thead>
                        <tr>
                            <th class="checkbox"><input type="checkbox" id="CheckAllBoeForms" /></th>
                            <th class="sort name">Name</th>
                            <th class="sort type">Type</th>
                            <th>Validation Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        <%  foreach (GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView item in Model) {
                                string disabled = item.IsIncomplete ? "disabled=\"disabled\" " : string.Empty;
                                string trDisabled = item.IsIncomplete ? "trdisabled" : string.Empty;
                                string indexer = (item.BOEFormType == BOEFormType.IBOE) ? "i" : "p";
                                %>
                            <tr data-pkid="<%: item.BOEFormId %>" data-formType="<%: item.BOEFormType.ToString() %>" data-UpdateDateLong="<%: item.UpdateDateLong %>" class="<%: trDisabled%>">
                                <%: Html.Hidden(indexer, item.BOEFormId) %>
                                <td class="checkbox"><input <%:disabled %> type="checkbox" name="ExportBoeForm" id="<%:indexer %>chk" /></td>
                                <td class="name">
                                    <span><%: item.BOEFormName %></span>
                                </td>
                                <td class="type">
                                    <span><%: item.BOEFormType.GetDescription() %></span>
                                </td>
                                <td>
                                    <% if (item.IsIncomplete)
                                    {
                                            %>
                                    <a href="#" class="edit-boe-forms-link" title="<%:string.Join("<br />", item.IncompleteMessages) %>">Incomplete data found</a>
                                    <% } %>
                                </td>
                            </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
        <%}%>
    </div>
</div>