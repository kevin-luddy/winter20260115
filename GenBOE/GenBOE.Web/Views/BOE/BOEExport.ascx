<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="System.Collections.ObjectModel" %>

<%
	Collection<SelectListItem> summarizeByCustomFieldOptions = (Collection<SelectListItem>)ViewData["SummarizeByCustomFieldOptions"];
	bool IsUsingSummarizeByCustomFieldTemplate = ViewData.ContainsKey("IsUsingSummarizeByCustomFieldTemplate") ? (bool)ViewData["IsUsingSummarizeByCustomFieldTemplate"] : false;
%>
<script type="text/javascript">
	var BOEExport = new Widget("BOEExport");
	var SummarizeByCustomFieldDialog;

	$(function () {
		$('#BOEExport-ExportButton').click(function () {

			<% if (IsUsingSummarizeByCustomFieldTemplate) { %>
				BOEExport.displaySummarizeByCustomFieldDialog();
			<% } else { %>
				BOEExport.runActualExport();
			<% }%>
		});

		BOEExport.runActualExport = function () {
			var summarizeByCustomFieldVal = $('#SummarizeByCustomField').val(); 

			// Remove the old hidden iFrame, if it exists
			$('#BOEExport-DownloadTarget').remove();

			<% if (!Utilities.DisablePiwik()) { %>
			if (piwikTracker3) {
				piwikTracker3.trackEvent('Export', 'BOE', '<%:ViewData["BOETitle"]%> - <%: ViewData["BOEID"] %>');
			}
			<% } %>

			// Create a new hidden iFrame and set it's source to the chosen report's URL
			var targetIFrame = $('<iframe />', {
				'id': 'BOEExport-DownloadTarget',
				'class': 'display-none',
				'src': CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
					'<%: WebConstants.CONTROLLER_BOE %>',
					'<%: WebConstants.ACTION_EXPORT_BOE %>',
					'?boeID=' + <%: ViewData["BOEID"] %>,
					'?SummarizeByCustomField=' + summarizeByCustomFieldVal)
			});

			<% if (IsUsingSummarizeByCustomFieldTemplate) { %>
			BOEExport.closeSummarizeByCustomFieldDialog();
			<% } %>

			// Append the iFrame to the body, causing the controller action to fire and
			// the download to occur inside the iFrame
			targetIFrame.appendTo('body');
		}

		BOEExport.closeSummarizeByCustomFieldDialog = function () {
			SummarizeByCustomFieldDialog.dialog('close');
		};

		BOEExport.displaySummarizeByCustomFieldDialog = function () {
			$("#SummarizeByCustomField-Export").data("report-id");
			if (!SummarizeByCustomFieldDialog) {
				SummarizeByCustomFieldDialog = $("#SummarizeByCustomFieldDialog").dialog({ width: 530, modal: true, resizable: false, draggable: true, 
					title: 'Preview in MS Word: Summarize by Custom Field', autoOpen: false, close: BOEExport.closeSummarizeByCustomFieldDialog });
			}
			SummarizeByCustomFieldDialog.dialog('open');
		};


	});
</script>

<button id="BOEExport-ExportButton" class="ies" type="button">Preview in MS Word</button>

<div id="SummarizeByCustomFieldDialog" style="display: none">
	<form id="SummarizeByCustomFieldForm">
		<div class="form-row center">
			The export is a long running process.
			<br />
			Please do not leave this page until the file is available to open/save.
		</div>
		<% if (summarizeByCustomFieldOptions == null || !summarizeByCustomFieldOptions.Any()) { %>
		<div class="form-row center error">
			Warning: This report was configured to summarize by custom field set at the "Resource Types" level.<br />
			No custom fields were detected, at the "Resource Types" level, in this workspace...
		</div>
		<% } else  { %>
		<div class="form-row center">This report was configured to summarize by custom field set at the "<b><i>Resource Types</i></b>" level.</div>
		<div class="form-row center" style="font-size: 12px;">
			<div class="form-label">Please select one custom field for summarization: </div>
			<div class="form-element">
				<%: Html.DropDownList("SummarizeByCustomField", summarizeByCustomFieldOptions)%>
			</div>
		</div>
		<% } %>
		<div class="form-row center">
			Continue with this export?<br />
			<br />
			Please refrain from clicking the export link multiple times until the download is complete.
		</div>

		<div class="buttons center">
			<button id="SummarizeByCustomField-Export" class="ies" data-report-id="" onclick="BOEExport.runActualExport();" name="yes-button" type="button">Yes</button>
			<button id="SummarizeByCustomField-Cancel" class="ies" onclick="BOEExport.closeSummarizeByCustomFieldDialog();" name="no-button" type="button">No</button>
		</div>
	</form>
</div>
