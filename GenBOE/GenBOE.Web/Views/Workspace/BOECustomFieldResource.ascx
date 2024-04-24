<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.BOECustomFieldResourceModelView>" %>
<%
    bool isReadOnly = bool.Parse((string)ViewData["READONLY"]);
%>
<script type="text/javascript">
	BOECustomFieldsResourceWidget = new Widget("OptionDialog", <%: ViewData["READONLY"] %>);

	BOECustomFieldsResourceWidget.SearchText = '';

	BOECustomFieldsResourceWidget.ImportDialog = {};
	BOECustomFieldsResourceWidget.ImportDialog.Element = $('#ImportDialog');
	BOECustomFieldsResourceWidget.ImportDialog.Params = { width: 750, modal: true, resizable: false, draggable: true, title: "Import Resources" };
	BOECustomFieldsResourceWidget.ImportInProgressDialog = {};
	BOECustomFieldsResourceWidget.ImportInProgressDialog.Element = $("#ImportInProgressDialog");
	BOECustomFieldsResourceWidget.ImportInProgressDialog.Params = { width: 300, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" };
	BOECustomFieldsResourceWidget.ImportResultsDialog = {};
	BOECustomFieldsResourceWidget.ImportResultsDialog.Element = $("#ImportResultsDialog");
	BOECustomFieldsResourceWidget.ImportResultsDialog.Params = { width: 750, modal: true, resizable: false, draggable: true, title: "Import Successful", close: BOECustomFieldsResourceWidget.ReloadGridData };
	BOECustomFieldsResourceWidget.RestoreResultsDialog = {};
	BOECustomFieldsResourceWidget.RestoreResultsDialog.Element = $('#RestoreResultsDialog');
	BOECustomFieldsResourceWidget.RestoreResultsDialog.Params = { width: 750, minHeight: 493, modal: true, resizable: false, draggable: true, title: "Restore in Progress" };
	BOECustomFieldsResourceWidget.DefaultResourceListDialog = {};
	BOECustomFieldsResourceWidget.DefaultResourceListDialog.Element = $('#DefaultResourceList');
	BOECustomFieldsResourceWidget.DefaultResourceListDialog.Params = { width: 750, minHeight: 493, modal: true, resizable: false, draggable: true, title: '<%: ViewData["SYSTEM_LIST_NAME"]%> - Default Options' };
	BOECustomFieldsResourceWidget.DefaultResourceListDialog.Loaded = false;
	BOECustomFieldsResourceWidget.OptionDialog = {};
	BOECustomFieldsResourceWidget.OptionDialog.Element = $("#OptionDialog");
	BOECustomFieldsResourceWidget.OptionDialog.Params = { width: 350, modal: true, resizable: false, draggable: true };

	BOECustomFieldsResourceWidget.ReloadGridData = function () {
		$('#PageLoading').removeClass('display-none');
		$('#ImportDialog-Error').attr("style", "none");

		var data = {};
		data.searchText = BOECustomFieldsResourceWidget.SearchText;
		data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
		data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
		data.showSub = $('#ElementsOfCost-Sub').prop('checked');
		data.showODC = $('#ElementsOfCost-ODC').prop('checked');
		data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
		data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

		$.ajax({
			type: 'POST',
			url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                '<%:WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE_GRID %>', ''),
			contentType: 'application/json; charset=utf-8',
			data: JSON.stringify(data),
			dataType: 'html',
			success: function (response) {
				$('#ResourceGridContent').html(response);
				$('#DeleteLoader-BOECustomFieldsResource').addClass('display-none');
				$('#Delete-BOECustomFieldsResource').removeClass('display-none');
				$('#Delete-BOECustomFieldsResource').addClass('disabled');
				refreshModule($(BOECustomFieldsResourceWidget.Module));
				$('#PageLoading').addClass('display-none');
			},
			error: function (response) {
				$('#PageLoading').addClass('display-none');
			}

		});
	}

	BOECustomFieldsResourceWidget.DeleteRecords = function (event, data) {
		$('#Delete-BOECustomFieldsResourceWidget').addClass('display-none');
		$('#DeleteLoader-BOECustomFieldsResourceWidget').removeClass('display-none');

		var deleteConfirmTitle = "Delete Default Resource Confirmation";
		var deleteConfirmMessage = "Are you sure you want to delete these Resources?";

		Session.confirmDialog(deleteConfirmTitle, deleteConfirmMessage,
			function () {
				$('#Delete-BOECustomFieldsResource').addClass('display-none');
				$('#DeleteLoader-BOECustomFieldsResource').removeClass('display-none');
				BOECustomFieldsResourceWidget.SaveResources(data.CheckedItems);
			}, function () {
				$('#Delete-BOECustomFieldsResource').removeClass('display-none');
				$('#DeleteLoader-BOECustomFieldsResource').addClass('display-none');
			});
	}

	BOECustomFieldsResourceWidget.AddRecord = function () {
		$('#OptionDialog input[name="ID"]').val('');
		$('#OptionDialog input[name="ID"]').prop('disabled', false);
		$('#OptionDialog input[name="UpdateDateLong"]').val('0');
		$('#OptionDialog input[name="ResourceID"]').val(-1);
		$('#OptionDialog input[name="Description"]').val('');
		$('#OptionDialog input[name="SegmentRegion"]').val('');
		$('#OptionDialog input[name="LaborType"]').val('');
		$('#OptionDialog input[name="RateType"]').val('');
		$('#OptionDialog input[name="ElementOfCostId"]').val(0);
		$('#OptionDialog input[name="ElementOfCostId"]').prop('disabled', false);
		$('#OptionDialog select[name="RateTypeID"]').prop("disabled", false);
		$('#OptionDialog select[name="ElementOfCostId"]').prop("disabled", false);

		BOECustomFieldsResourceWidget.clearValidationBox($("#ResourceOptionForm ul.validation-box"));
		BOECustomFieldsResourceWidget.OpenDialogAfterInitialize(BOECustomFieldsResourceWidget.OptionDialog);
		BOECustomFieldsResourceWidget.OptionDialog.Element.dialog({ title: "Add Resource" });
		$('#RateTypeID').val(0);
		$('#ElementOfCostId').val(0)
		$("#InuseWarningFlag").hide();
	}

	BOECustomFieldsResourceWidget.EditRecord = function (event, data) {
		$('#OptionDialog input[name="ID"]').val(data.ID);
		$('#OptionDialog input[name="ID"]').prop('disabled', true);
		$('#OptionDialog input[name="UpdateDateLong"]').val(data.UpdateDateLong);
		$('#OptionDialog input[name="ResourceID"]').val(data.CustomFieldOptionID);
		$('#OptionDialog input[name="Description"]').val(data.Description);
		$('#OptionDialog input[name="SegmentRegion"]').val(data.SegmentRegion);
		$('#OptionDialog input[name="LaborType"]').val(data.LaborType);
		$('#OptionDialog select[name="RateTypeID"]').val(data.RateType);
		$('#OptionDialog select[name="ElementOfCostId"]').val(data.ElementOfCostId);

		BOECustomFieldsResourceWidget.clearValidationBox($("#ResourceOptionForm ul.validation-box"));
		BOECustomFieldsResourceWidget.OpenDialogAfterInitialize(BOECustomFieldsResourceWidget.OptionDialog);
		BOECustomFieldsResourceWidget.OptionDialog.Element.dialog({ title: "Edit Resource" });

		if (data.InUse == 'True') {
			$('#OptionDialog select[name="ElementOfCostId"]').prop('disabled', true);
			$('#OptionDialog select[name="RateTypeID"]').prop('disabled', true);

			$("#InuseWarningFlag").show();
		}
		else {
			$('#OptionDialog select[name="ElementOfCostId"]').prop('disabled', false);
			$('#OptionDialog select[name="RateTypeID"]').prop('disabled', false);
			$("#InuseWarningFlag").hide();
		}

	}


	BOECustomFieldsResourceWidget.SaveRecord = function () {
		//this is a hack for some reason the first validate won't caught ID being required so doing it twice is a low cost fix.

		var data = new Array();
		var option = {};
		option.ID = $('#OptionDialog input[name="ID"]').val();
		option.UpdateDateLong = $('#OptionDialog input[name="UpdateDateLong"]').val().toString();
		option.CustomFieldOptionID = $('#OptionDialog input[name="ResourceID"]').val();
		option.Description = $('#OptionDialog input[name="Description"]').val();
		option.SegmentRegion = $('#OptionDialog input[name="SegmentRegion"]').val();
		option.LaborType = $('#OptionDialog input[name="LaborType"]').val();
		option.RateTypeID = $('#OptionDialog select[name="RateTypeID"]').val();
		option.ElementOfCostId = $('#OptionDialog select[name="ElementOfCostId"]').val();
		data.push(option);

		BOECustomFieldsResourceWidget.SaveResources(data);

	}

	BOECustomFieldsResourceWidget.SaveResources = function (data) {
		if (data.length > 0) {
			$('#Loader-OptionDialog').removeClass('display-none');
			$('#Save-OptionDialog').addClass('display-none');

			var dataToSend = {};
			dataToSend.resources = data;

			dataToSend = JSON.stringify(dataToSend);

			BOECustomFieldsResourceWidget.ajaxRequest({
				type: 'POST',
				url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_SAVE_CUSTOM_FIELD_RESOURCES %>', ''),
				contentType: 'application/json; charset=utf-8',
				data: dataToSend,
				success: function (response) {
					$('#Loader-OptionDialog').addClass('display-none');
					$('#Save-OptionDialog').removeClass('display-none');
					BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.OptionDialog);
					$('#BOECustomFieldsResource .restore-link').removeClass('display-none');

					if (response.Status == true) {
						BOECustomFieldsResourceWidget.ReloadGridData();
					} else {
						// this was an edit, update the row
						var originalPkid = data[0].CustomFieldOptionID;
						var originalRow = $('#BOECustomFieldsResource tr[pkid=' + originalPkid + ']');
						var row = originalRow.clone();

						row.attr('pkid', response.CustomFieldOptionID);
						row.find('a[name=ID]').html(response.ID);
						row.find('input[name=UpdateDateLong]').val(response.UpdateDateLong);
						row.find('a[name=Description]').html(response.Description);
						row.find('a[name=SegmentRegion]').html(response.SegmentRegion);
						row.find('a[name=LaborType]').html(response.LaborType);
						row.find('a[name=RateType]').html(response.RateTypeDisplay);
						row.find('input[name=RateTypeID]').val(response.RateTypeID);
						row.find('a[name=ElementOfCost]').html(response.ElementOfCostDisplay);
						row.find('input[name=ElementOfCostID]').val(response.ElementOfCostId);

						originalRow.replaceWith(row);
					}
				},
				error: function (response) {
					$('#Loader-OptionDialog').addClass('display-none');
					$('#Save-OptionDialog').removeClass('display-none');
					$('#Delete-BOECustomFieldsResource').removeClass('display-none');
					$('#DeleteLoader-BOECustomFieldsResource').addClass('display-none');
				}
			}, $('#Save-OptionDialog'));
		}
		else {
			$('#Delete-BOECustomFieldsResource').addClass('disabled');
			$('#Delete-BOECustomFieldsResource').removeClass('display-none');
			$('#DeleteLoader-BOECustomFieldsResource').addClass('display-none');
		}
	}

	BOECustomFieldsResourceWidget.Search = function (element) {
		$('#PageLoading').removeClass('display-none');

		BOECustomFieldsResourceWidget.SearchText = element.val();

		var data = {};
		data.searchText = BOECustomFieldsResourceWidget.SearchText;
		data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
		data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
		data.showSub = $('#ElementsOfCost-Sub').prop('checked');
		data.showODC = $('#ElementsOfCost-ODC').prop('checked');
		data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
		data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

		var dataToSend = JSON.stringify(data);

		$.ajax({
			type: 'POST',
			url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                '<%:WebConstants.ACTION_SEARCH_CUSTOM_FIELD_RESOURCES %>', ''),
			contentType: 'application/json; charset=utf-8',
			data: dataToSend,
			dataType: 'html',
			success: function (response) {
				$('#ResourceGridContent').html(response);
				$('#DeleteLoader-BOECustomFieldsResource').addClass('display-none');
				$('#Delete-BOECustomFieldsResource').removeClass('display-none');
				$('#Delete-BOECustomFieldsResource').addClass('disabled');
				refreshModule($(BOECustomFieldsResourceWidget.Module));
				$('#PageLoading').addClass('display-none');
			},
			error: function (response) {
				$('#PageLoading').addClass('display-none');
			}
		});
	}

	BOECustomFieldsResourceWidget.PageResources = function (event, data) {
		var dataToSend = JSON.stringify(data);
		$('#PageLoading').removeClass('display-none');

		$.ajax({
			type: 'POST',
			url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                '<%:WebConstants.ACTION_PAGE_CUSTOM_FIELD_RESOURCES %>', ''),
			contentType: 'application/json; charset=utf-8',
			dataType: 'html',
			data: dataToSend,
			success: function (response) {
				$('#ResourceGridContent').html(response);
				$('#DeleteLoader-BOECustomFieldsResource').addClass('display-none');
				$('#Delete-BOECustomFieldsResource').removeClass('display-none');
				$('#Delete-BOECustomFieldsResource').addClass('disabled');
				refreshModule($(BOECustomFieldsResourceWidget.Module));
				$('#PageLoading').addClass('display-none');
			}
		});
	}

	BOECustomFieldsResourceWidget.InUseEditsConfirmed = function () {
		// need to close warning message
		$("#InuseWarningFlag").hide();
	};

	BOECustomFieldsResourceWidget.ConfirmRestore = function () {
		var text = 'Are you sure you want to use the default list of Resources? Resources in use by at least one BOE will remain in the list unchanged.';
		var title = 'Restore or Get Updated Default Resources';

		Session.confirmDialog(title, text, BOECustomFieldsResourceWidget.RestoreConfirmed, null);
	}

	BOECustomFieldsResourceWidget.RestoreConfirmed = function () {
		BOECustomFieldsResourceWidget.OpenDialogAfterInitialize(BOECustomFieldsResourceWidget.RestoreResultsDialog);

		$('#RestoreResultsDialog-Loader').removeClass('display-none');
		$('#RestoreResultsDialog-Waiting').removeClass('display-none');
		$('#RestoreResultsData').addClass('display-none');

		$.ajax({
			type: 'POST',
			url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            '<%: WebConstants.CONTROLLER_WORKSPACE %>',
			'<%: WebConstants.ACTION_RESTORE_CUSTOM_FIELD_RESOURCES %>', ''),
		dataType: 'html',
		success: function (response) {
			$('#RestoreResultsDialog-Loader').addClass('display-none');
			$('#RestoreResultsDialog-Waiting').addClass('display-none');
			$('#RestoreResultsData').removeClass('display-none');
			$("#RestoreResultsDialog").dialog({ title: "Restore Successful" });
			$('#RestoreResultsData').html(response);
			$('#BOECustomFieldsResource .restore-link').addClass('display-none');
			BOECustomFieldsResourceWidget.ReloadGridData();
		},
		error: function () {
			$('#RestoreResultsDialog-Loader').addClass('display-none');
			$('#RestoreResultsDialog-Waiting').addClass('display-none');
			BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.RestoreResultsDialog);
		}
	});
	}

	/*-------------------------------  Import/Export Functions --------------------------------*/

	// Remove old dialogs left behind when jumping back to main jump page
	BOECustomFieldsResourceWidget.RemoveStaleDialogs = function () {
		$('body .ui-dialog').children('#ImportDialog, #ImportInProgressDialog, #ImportResultsDialog, #RestoreResultsDialog, #DefaultResourceList').parent().remove();
		$('body').children('#ImportDialog, #ImportInProgressDialog, #ImportResultsDialog, #RestoreResultsDialog, #DefaultResourceList').remove();

	}

	BOECustomFieldsResourceWidget.ExportButtonClick = function () {
		if (Session.isDirty()) {
			Session.confirmDialog(
				"Save Changes",
				"Changes must be saved to include them in the export. Do you want to save changes made before exporting?",
				function () {
					BOECustomFieldsResourceWidget.registerForEvent('EXPORT_SAVE_SUCCESSFUL', BOECustomFieldsResourceWidget.Export);
					BOECustomFieldsResourceWidget.SaveWithoutRedirect('EXPORT');
				},
				BOECustomFieldsResourceWidget.Export);
		}
		else {
			BOECustomFieldsResourceWidget.Export();
		}
	}

	BOECustomFieldsResourceWidget.Export = function () {
		// Remove the old hidden iFrame, if it exists
		$('#DownloadTarget').remove();

		var data = {};
		data.searchText = BOECustomFieldsResourceWidget.SearchText;
		data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
		data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
		data.showSub = $('#ElementsOfCost-Sub').prop('checked');
		data.showODC = $('#ElementsOfCost-ODC').prop('checked');
		data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
		data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

		var dataToSend = JSON.stringify(data);

		// Create a new hidden iFrame and set it's source to the chosen report's URL
		var parms = data.showLabor + '/' +
			data.showIWTA + '/' +
			data.showSub + '/' +
			data.showODC + '/' +
			data.showTravel + '/' +
			data.showMaterials +
			((data.searchText != '') ? ('/' + data.searchText) : '');

		var targetIFrame = $('<iframe />', {
			'id': 'DownloadTarget',
			'class': 'display-none',
			'src': CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_EXPORT_BOE_CUSTOM_FIELD_RESOURCE %>',
				parms)
		});

		// Append the iFrame to the body, causing the controller action to fire and
		// the download to occur inside the iFrame
		targetIFrame.appendTo('body');
	}

	BOECustomFieldsResourceWidget.ExportTemplate = function () {
		// Remove the old hidden iFrame, if it exists
		$('#DownloadTarget').remove();

		var data = {};
		data.searchText = BOECustomFieldsResourceWidget.SearchText;
		data.showLabor = $('#ElementsOfCost-ISGSLabor').prop('checked');
		data.showIWTA = $('#ElementsOfCost-IWTA').prop('checked');
		data.showSub = $('#ElementsOfCost-Sub').prop('checked');
		data.showODC = $('#ElementsOfCost-ODC').prop('checked');
		data.showTravel = $('#ElementsOfCost-Travel').prop('checked');
		data.showMaterials = $('#ElementsOfCost-Materials').prop('checked');

		var dataToSend = JSON.stringify(data);

		// Create a new hidden iFrame and set it's source to the chosen report's URL
		var parms = data.showLabor + '/' +
			data.showIWTA + '/' +
			data.showSub + '/' +
			data.showODC + '/' +
			data.showTravel + '/' +
			data.showMaterials +
			((data.searchText != '') ? ('/' + data.searchText) : '');

		var targetIFrame = $('<iframe />', {
			'id': 'DownloadTarget',
			'class': 'display-none',
			'src': CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_EXPORT_BOE_CUSTOM_FIELD_RESOURCE_TEMPLATE %>',
				parms)
		});

		// Append the iFrame to the body, causing the controller action to fire and
		// the download to occur inside the iFrame
		targetIFrame.appendTo('body');
	}

	BOECustomFieldsResourceWidget.SubmitUploadForm = function () {
		if (BOECustomFieldsResourceWidget.ValidateFileInput()) {
			BOECustomFieldsResourceWidget.HideImportDialogError();
			BOECustomFieldsResourceWidget.OpenDialogAfterInitialize(BOECustomFieldsResourceWidget.ImportInProgressDialog);

			BOECustomFieldsResourceWidget.AppendIFrameForUploadResponse();
			$("#ImportDialog-Form").submit();
		}
	}

	BOECustomFieldsResourceWidget.AppendIFrameForUploadResponse = function () {
		// Remove the old hidden iFrame, if it exists
		$('#ImportDialog-UploadTarget').remove();

		// Append the iFrame to the body, causing the controller action to fire and
		// the download to occur inside the iFrame
		$('#ImportDialog-Form').append('<iframe id="ImportDialog-UploadTarget" name="ImportDialog-UploadTarget" class="display-none"></iframe>');
		$('#ImportDialog-UploadTarget').load(BOECustomFieldsResourceWidget.StopUpload);

		BOECustomFieldsResourceWidget.SetDocumentDomains();
	}

	BOECustomFieldsResourceWidget.StopUpload = function () { //Function will be called when iframe is loaded
		var uploadResponseElement = $("#ImportDialog-UploadTarget").contents().find("body #UploadResponse");

		if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
			var results = eval('(' + uploadResponseElement.html() + ')');

			if (results != undefined && results.Status) {
				// Hide dialogs
				BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.ImportDialog);
				BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.ImportInProgressDialog);

				// Render Import Results Dialog
				BOECustomFieldsResourceWidget.RenderImportResults(results.Data);

				// Show Import Results
				BOECustomFieldsResourceWidget.OpenDialogAfterInitialize(BOECustomFieldsResourceWidget.ImportResultsDialog);
			}
			else {
				$("#ImportDialog-ErrorText").text(results.Message);
				BOECustomFieldsResourceWidget.ShowImportDialogError();
				BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.ImportInProgressDialog)

			}
		}
		else {
			BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.ImportInProgressDialog);
			BOECustomFieldsResourceWidget.ReloadGridData();
		}
	}

	BOECustomFieldsResourceWidget.RenderImportResults = function (resultsData) {

		// Render html for options that were added

		if (resultsData != null && resultsData.AddedOptions != undefined && resultsData.AddedOptions != null && resultsData.AddedOptions.length) {
			var options = resultsData.AddedOptions;

			var newHTML = "<b>" + options.length + " options added:</b><ul>";
		}
		else {
			var newHTML = "<b>0 options added.</b>";
		}

		$('#ImportResultsDialog-Added').html(newHTML);

		// Render html for options that were changed
		if (resultsData != null && resultsData.ChangedOptions != undefined && resultsData.ChangedOptions != null && resultsData.ChangedOptions.length) {
			var options = resultsData.ChangedOptions;
			var newHTML = "<b>" + options.length + " options changed:</b><ul>";

			for (var ndx = 0; ndx < options.length; ndx++) {
				newHTML += "<li>" +
					options[ndx].Old.ResourceName + ", " +
					options[ndx].Old.ResourceDesc + ", " +
					options[ndx].Old.SegRegion + ", " +
					options[ndx].Old.LaborType + ", " +
					options[ndx].Old.RateTypeString +
					" changed to " +
					options[ndx].New.ResourceName + ", " +
					options[ndx].New.ResourceDesc + ", " +
					options[ndx].New.SegRegion + ", " +
					options[ndx].New.LaborType + ", " +
					options[ndx].New.RateTypeString
				"</li>";
			}

			newHTML += "</ul>";
		}
		else {
			var newHTML = "<b>0 options changed.</b>";
		}

		$('#ImportResultsDialog-Changed').html(newHTML);

		// Render html for options that were deleted
		if (resultsData != null && resultsData != null && resultsData.DeletedOptions != undefined && resultsData.DeletedOptions != null && resultsData.DeletedOptions.length) {
			var options = resultsData.DeletedOptions;

			var newHTML = "<b>" + options.length + " options deleted:</b><ul>";
		}
		else {
			var newHTML = "<b>0 options deleted.</b>";
		}

		$('#ImportResultsDialog-Deleted').html(newHTML);

		// Render html for options that were in use and left unchanged
		if (resultsData != null && resultsData.InUseOptions != undefined && resultsData.InUseOptions != null && resultsData.InUseOptions.length) {
			var options = resultsData.InUseOptions;

			var newHTML = "<b>" + options.length + " options could not be changed or deleted because 'ID', 'Element of Cost', and 'Rate Type' cannot be modified when they are in use by at least one BOE:</b><ul>";

			for (var ndx = 0; ndx < options.length; ndx++) {
				newHTML += "<li>" +
					options[ndx].ResourceName + ", " +
					options[ndx].ResourceDesc + ", " +
					options[ndx].SegRegion + ", " +
					options[ndx].LaborType + ", " +
					options[ndx].RateTypeString
				"</li>";
			}

			newHTML += "</ul>";
		}
		else {
			var newHTML = "<b>0 options in use.</b>";
		}

		$('#ImportResultsDialog-InUse').html(newHTML);
		BOECustomFieldsResourceWidget.ReloadGridData();
	}

	// This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
	BOECustomFieldsResourceWidget.SetDocumentDomains = function () {
		$('#ImportDialog-DocumentDomain').val(document.domain);
	}

	BOECustomFieldsResourceWidget.ValidateFileInput = function () {
		if ($("#ImportDialog-File").val().length == 0) {
			BOECustomFieldsResourceWidget.ShowImportDialogFileValidation();
			BOECustomFieldsResourceWidget.DisableImportDialogImportButton();
			return false;
		}
		else {
			BOECustomFieldsResourceWidget.HideImportDialogFileValidation();
			BOECustomFieldsResourceWidget.EnableImportDialogImportButton();
			return true;
		}
	}

	BOECustomFieldsResourceWidget.EnableImportDialogImportButton = function () {
		$("#ImportDialog-ImportButton").removeClass('disabled');
	}

	BOECustomFieldsResourceWidget.DisableImportDialogImportButton = function () {
		$("#ImportDialog-ImportButton").addClass('disabled');
	}

	BOECustomFieldsResourceWidget.ShowImportDialogError = function () {
		$("#ImportDialog-Error").slideDown("slow");
	}

	BOECustomFieldsResourceWidget.HideImportDialogError = function () {
		$("#ImportDialog-Error").slideUp("slow");
	}

	BOECustomFieldsResourceWidget.ShowImportDialogFileValidation = function () {
		$("#ImportDialog-FileValidation").slideDown("slow");
	}

	BOECustomFieldsResourceWidget.HideImportDialogFileValidation = function () {
		$("#ImportDialog-FileValidation").slideUp("slow");
	}

	/*-------------------------------  End Import/Export Functions --------------------------------*/

	$(function () {
		BOECustomFieldsResourceWidget.RemoveStaleDialogs();

		BOECustomFieldsResourceWidget.Module = $('#BOECustomFieldsResource');

		BOECustomFieldsResourceWidget.InitializeDialog(BOECustomFieldsResourceWidget.ImportDialog);
		BOECustomFieldsResourceWidget.InitializeDialog(BOECustomFieldsResourceWidget.DefaultResourceListDialog);
		BOECustomFieldsResourceWidget.InitializeDialog(BOECustomFieldsResourceWidget.RestoreResultsDialog);
		BOECustomFieldsResourceWidget.InitializeDialog(BOECustomFieldsResourceWidget.OptionDialog);
		BOECustomFieldsResourceWidget.InitializeDialog(BOECustomFieldsResourceWidget.ImportInProgressDialog);
		BOECustomFieldsResourceWidget.InitializeDialog(BOECustomFieldsResourceWidget.ImportResultsDialog);

		BOECustomFieldsResourceWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { BOECustomFieldsResourceWidget.cleanDirty(); });

		$('#Cancel-BOECustomFieldsResource, #Back-BOECustomFieldsResource').click(function () {
			window.location.hash = '#BOECustomFields';
		});

		BOECustomFieldsResourceWidget.registerForLiveEvent('click', '#Delete-BOECustomFieldsResource:not(.disabled)', function () { $(document).trigger('GET_DELETED_RESOURCES'); });
		$('#Add-BOECustomFieldsResource').click(BOECustomFieldsResourceWidget.AddRecord);
		$('#Import-BOECustomFieldsResource').click(function () {
			$('#ImportDialog-Error').attr("style", "none");
			$('#ElementsOfCostImport-ISGSLabor').prop('checked', $('#ElementsOfCost-ISGSLabor').prop('checked'));
			$('#ElementsOfCostImport-IWTA').prop('checked', $('#ElementsOfCost-IWTA').prop('checked'));
			$('#ElementsOfCostImport-Sub').prop('checked', $('#ElementsOfCost-Sub').prop('checked'));
			$('#ElementsOfCostImport-ODC').prop('checked', $('#ElementsOfCost-ODC').prop('checked'));
			$('#ElementsOfCostImport-Travel').prop('checked', $('#ElementsOfCost-Travel').prop('checked'));
			$('#ElementsOfCostImport-Materials').prop('checked', $('#ElementsOfCost-Materials').prop('checked'));

			BOECustomFieldsResourceWidget.OpenDialogAfterInitialize(BOECustomFieldsResourceWidget.ImportDialog);
		});
		$('#Export-BOECustomFieldsResource').click(BOECustomFieldsResourceWidget.ExportButtonClick);

		$('#Search-BOECustomFieldsResource, #Search-BOECustomFieldsResource2').click(function () { BOECustomFieldsResourceWidget.Search($(this).parent().find('input[name=SearchText]')); });

		BOECustomFieldsResourceWidget.registerForDelegateEvent('keydown',
			'input[name=SearchText]', function (e) {
				/****enter key to search */
				var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
				if (keyCode == 13) {
					BOECustomFieldsResourceWidget.Search($(this));
				}
			},
			'#BOECustomFieldsResource'
		);

		$('#ImportDialog-ImportButton').click(BOECustomFieldsResourceWidget.SubmitUploadForm);
		$('#ImportDialog-CancelButton').click(function () {
			BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.ImportDialog);
			$('#ImportDialog-Error').attr("style", "none");
		});

		$('#ImportResultsDialog-CloseButton').click(function () {
			BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.ImportResultsDialog);
			$('#ImportDialog-Error').attr("style", "none");
		});

		BOECustomFieldsResourceWidget.registerForDelegateEvent('click',
			'#Save-OptionDialog:not(.disabled)', BOECustomFieldsResourceWidget.SaveRecord);
		$('#BOECustomFieldsResource-Cancel-OptionDialog').click(function () { BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.OptionDialog); });

		$('input[name="ElementsOfCost"]').change(BOECustomFieldsResourceWidget.ReloadGridData);

		BOECustomFieldsResourceWidget.ValidateFileInput();

		$('#RestoreButton').click(function () {
			BOECustomFieldsResourceWidget.ConfirmRestore();
		});

		$('#ViewDefaultList').click(function () {
			BOECustomFieldsResourceWidget.OpenDialogAfterInitialize(BOECustomFieldsResourceWidget.DefaultResourceListDialog);

			if (!BOECustomFieldsResourceWidget.DefaultResourceListDialog.Loaded) {
				$('#DefaultResourceList-Loader').removeClass('display-none');
				$('#DefaultResourceList-Waiting').removeClass('display-none');
				$('#DefaultListData').addClass('display-none');

				$.ajax({
					type: 'POST',
					url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE_VIEW_DEFAULT %>', ''),
					dataType: 'html',
					success: function (response) {
						$('#DefaultResourceList-Loader').addClass('display-none');
						$('#DefaultResourceList-Waiting').addClass('display-none');
						$('#DefaultListData').removeClass('display-none');
						$('#DefaultListData').html(response);
						BOECustomFieldsResourceWidget.DefaultResourceListDialog.Loaded = true;
					},
					error: function () {
						$('#DefaultResourceList-Loader').addClass('display-none');
						$('#DefaultResourceList-Waiting').addClass('display-none');
						BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.DefaultResourceListDialog);
					}
				});
			}
		});


		$('a[name=manageWorkspaceRates]').click(function () {
			$(document).trigger("WS_LOAD_RESOURCE_RATES");
		});

		BOECustomFieldsResourceWidget.registerForEvent("CLOSE_RESTORE_RESULTS", function () { BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.RestoreResultsDialog); });
		BOECustomFieldsResourceWidget.registerForEvent("CLOSE_DEFAULT_RESOURCES", function () { BOECustomFieldsResourceWidget.CloseDialog(BOECustomFieldsResourceWidget.DefaultResourceListDialog); });
		BOECustomFieldsResourceWidget.registerForEvent('RESOURCE_CHECKED', function () { $('#Delete-BOECustomFieldsResource').removeClass('disabled'); });
		BOECustomFieldsResourceWidget.registerForEvent('DELETE_RESOURCES', BOECustomFieldsResourceWidget.DeleteRecords);
		BOECustomFieldsResourceWidget.registerForEvent('EDIT_RESOURCE', BOECustomFieldsResourceWidget.EditRecord);
		BOECustomFieldsResourceWidget.registerForEvent('PAGE_RESOURCES', BOECustomFieldsResourceWidget.PageResources);

		createModule(BOECustomFieldsResourceWidget.Module);
		refreshModule(BOECustomFieldsResourceWidget.Module);

		BOECustomFieldsResourceWidget.ReloadGridData();
	});

	$('#ExportTemplate-CustomResource').click(BOECustomFieldsResourceWidget.ExportTemplate);
</script>

<div id="BOECustomFieldsResource" class="boe-custom-field-resource module">
	<div class="module-header-data">Resource</div>
	<div class="module-content-data">
		<div class="form-row">
			Edit the field properties.
		</div>
		<div class="form-row">
			<div class="form-label">
				<span>Field Name *</span>
				<div id="FieldName-Help" class="help-icon" onclick="BOECustomFieldsResourceWidget.ToggleHelp(this);"></div>
				<!-- This comment is needed for the jquery animation to work in IE8... -->
				<div id="FieldName-HelpDialog" class="help-dialog" style="width: 200px;">
					<div class="help-dialog-close"></div>
					<div class="help-dialog-text">Field name displayed to the user on the BOE.</div>
				</div>
			</div>
			<div class="form-element">
				Resource
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span>Display Field for Level *</span>
				<div id="FieldLevel-Help" class="help-icon" onclick="BOECustomFieldsResourceWidget.ToggleHelp(this);"></div>
				<!-- This comment is needed for the jquery animation to work in IE8... -->
				<div id="FieldLevel-HelpDialog" class="help-dialog" style="width: 200px;">
					<div class="help-dialog-close"></div>
					<div class="help-dialog-text">Resource can only be entered for each Resource Type.</div>
				</div>
			</div>
			<div class="form-element">
				<%: Html.RadioButton("BOE", false, false, new { disabled="true"}) %>&nbsp;<label for="BOE">BOE</label>
				<%: Html.RadioButton("Task", false, false, new { disabled = "true" })%>&nbsp;<label for="Task">Task</label>
				<%: Html.RadioButton("Resource Type", true, true, new { disabled = "true" })%>&nbsp;<label for="Resource Type">Resource Type</label>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span>Required Field</span>
				<div id="RequiredField-Help" class="help-icon" onclick="BOECustomFieldsResourceWidget.ToggleHelp(this);"></div>
				<!-- This comment is needed for the jquery animation to work in IE8... -->
				<div id="RequiredField-HelpDialog" class="help-dialog" style="width: 200px;">
					<div class="help-dialog-close"></div>
					<div class="help-dialog-text">This is a required field and cannot be changed.</div>
				</div>
			</div>
			<div class="form-element">
				<%: Html.CheckBox("Required", true, new { disabled="true"}) %>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span>Elements of Cost</span>
				<div id="ElementsOfCost-Help" class="help-icon" onclick="BOECustomFieldsResourceWidget.ToggleHelp(this);"></div>
				<!-- This comment is needed for the jquery animation to work in IE8... -->
				<div id="ElementsOfCost-HelpDialog" class="help-dialog" style="width: 200px;">
					<div class="help-dialog-close"></div>
					<div class="help-dialog-text">Select which Elements of Cost you would like to be displayed. Export will only show those which are displayed.</div>
				</div>
			</div>
			<div class="form-element">
				<input type="checkbox" id="ElementsOfCost-ISGSLabor" name="ElementsOfCost" checked="true" />
				<label for="ElementsOfCost-ISGSLabor">LM Labor</label>
				<input type="checkbox" id="ElementsOfCost-IWTA" name="ElementsOfCost" />
				<label for="ElementsOfCost-IWTA">IWTA</label>
				<input type="checkbox" id="ElementsOfCost-Sub" name="ElementsOfCost" />
				<label for="ElementsOfCost-Sub">Sub</label>
				<input type="checkbox" id="ElementsOfCost-ODC" name="ElementsOfCost" />
				<label for="ElementsOfCost-ODC">ODC</label>
				<input type="checkbox" id="ElementsOfCost-Travel" name="ElementsOfCost" />
				<label for="ElementsOfCost-Travel">Travel</label>
				<input type="checkbox" id="ElementsOfCost-Materials" name="ElementsOfCost" />
				<label for="ElementsOfCost-Materials">Materials</label>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span>Options *</span>
				<div id="Options-Help" class="help-icon" onclick="BOECustomFieldsResourceWidget.ToggleHelp(this);"></div>
				<!-- This comment is needed for the jquery animation to work in IE8... -->
				<div id="Options-HelpDialog" class="help-dialog" style="width: 300px;">
					<div class="help-dialog-close"></div>
					<div class="help-dialog-text">Users will be able to select one of these options for the field.<br />
						<br />
						Options currently in use by a BOE cannot be deleted. To delete the option, Authors must select a different option in their BOEs.</div>
				</div>
				<div class="restore-link<% if (!(bool)ViewData["RESOURCES_CHANGED"] || isReadOnly) { %> display-none<% } %>">
					<span><a id="RestoreButton">Restore or get updated default Resources</a> (<a id="ViewDefaultList">View</a>)</span>
					<div id="Restore-Help" class="help-icon" onclick="BOECustomFieldsResourceWidget.ToggleHelp(this);"></div>
					<!-- This comment is needed for the jquery animation to work in IE8... -->
					<div id="Restore-HelpDialog" class="help-dialog" style="width: 300px;">
						<div class="help-dialog-close"></div>
						<div class="help-dialog-text">The Workspace’s Resources are different than the default <%: ViewData["SYSTEM_LIST_NAME"] %> options provided by the System Administrator because either changes were made to the Workspace Resources or the System Administrator has updated the default Resources. </div>
					</div>
				</div>
			</div>
			<div class="form-element">
				<div class="buttons">
					<button id="Delete-BOECustomFieldsResource" class="ies-action disabled hide-on-readonly" name="delete-button" type="button">Delete</button>
					<div id="DeleteLoader-BOECustomFieldsResource" class="loader display-none"></div>
					<button id="Add-BOECustomFieldsResource" class="ies-action hide-on-readonly" type="button">+ Add</button>
					<button id="Import-BOECustomFieldsResource" class="ies-action hide-on-readonly" name="import-button" type="button">Import</button>
					<button id="Export-BOECustomFieldsResource" class="ies-action" type="button">Export</button>
				</div>
				<div class="search-box inline float-right">
					<div id="PageControls" class="paging-control"></div>
					<%: Html.TextBox("SearchText") %>
					<div id="Search-BOECustomFieldsResource" class="button search-magnify-glass-button"></div>
				</div>
				<div id="ResourceGridContent">
				</div>
				<div style="height: 28px; margin-top: 10px">
					<div class="search-box inline float-right">
						<div id="PageControls2" class="paging-control"></div>
						<%: Html.TextBox("SearchText") %>
						<div id="Search-BOECustomFieldsResource2" class="button search-magnify-glass-button"></div>
					</div>
				</div>
			</div>
		</div>
	</div>
</div>

<div id="ImportInProgressDialog" style="display: none; font-size: 18px; font-weight: bold; font-family: Arial, Helvetica; text-align: center;">
	<div class="loader"></div>
	<br />
	Importing Resource Data
</div>
<div id="ImportDialog" class="import-elements-of-cost dialog form" style="display: none;">
	<div id="ImportDialog-Error" class="validation-box" style="display: none;">
		<div>
			<b>Import Failed.</b>
			<div id="ImportDialog-ErrorText">
			</div>
		</div>
		<div class="clear"></div>
	</div>
	<% Html.BeginForm(WebConstants.ACTION_IMPORT_BOE_CUSTOM_FIELD_RESOURCE, WebConstants.CONTROLLER_WORKSPACE, new { workspace = SiteMasterUtilities.GetCurrentWorkspace() }, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportDialog-Form", target = "ImportDialog-UploadTarget" }); %>
	<div style="margin: 5px 0px 15px 0px;">
		Import a new list of options. Current list will be replaced with imported list.
	</div>
	<div class="form-label">
		<span>Elements of Cost</span>
		<div id="ImportElementsOfCost-Help" class="help-icon" onclick="BOECustomFieldsResourceWidget.ToggleHelp(this, null, false);"></div>
		<!-- This comment is needed for the jquery animation to work in IE8... -->
		<div id="ImportElementsOfCost-HelpDialog" class="help-dialog" style="width: 200px;">
			<div class="help-dialog-close"></div>
			<div class="help-dialog-text">Select which Elements of Cost you would like to be imported. Only the selected Elements of Cost will be replaced.</div>
		</div>
	</div>
	<div class="form-element">
		<input type="checkbox" id="ElementsOfCostImport-ISGSLabor" name="ImportISGSLabor" />
		<label for="ElementsOfCostImport-ISGSLabor">LM Labor</label>
		<input type="checkbox" id="ElementsOfCostImport-IWTA" name="ImportIWTA" />
		<label for="ElementsOfCostImport-IWTA">IWTA</label>
		<input type="checkbox" id="ElementsOfCostImport-Sub" name="ImportSub" />
		<label for="ElementsOfCostImport-Sub">Sub</label>
		<input type="checkbox" id="ElementsOfCostImport-ODC" name="ImportODC" />
		<label for="ElementsOfCostImport-ODC">ODC</label>
		<input type="checkbox" id="ElementsOfCostImport-Travel" name="ImportTravel" />
		<label for="ElementsOfCostImport-Travel">Travel</label>
		<input type="checkbox" id="ElementsOfCostImport-Materials" name="ImportMaterials" />
		<label for="ElementsOfCostImport-Materials">Materials</label>
	</div>
	<div class="form-label">
		<span>Resources File Location:</span>
	</div>
	<div class="form-element">
		<input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
		<input type="file" size="60" id="ImportDialog-File" name="file" onchange="BOECustomFieldsResourceWidget.ValidateFileInput();" />
		<div id="ImportDialog-FileValidation" style="color: #990000; display: none;">
			Please select a file to import
		</div>
		<ul style="padding: 0px 0px 0px 15px; margin: 10px 0px 0px 0px;">
			<li>File must contain the same headers as <a id="ExportTemplate-CustomResource">this format example</a> prior to importing and be in .xlsx format.</li>
			<li>All IDs must be unique.</li>
			<li>Resource options in use by a BOE will remain in the list. Any changes to an in use resource option will apply to all BOEs using the resource. All BOE authors and approvers of modified BOEs in the Awaiting Approval & Approved state will be notified of changes via email.</li>
			<li>Any rates assigned to these resources, which are not being used within a BOE, will be deleted.  To save your rates for later import, navigate to the <a name="manageWorkspaceRates">Manage Subcontractor, IWTA, and LMSI Rates</a> page and export rates. </li>
		</ul>
	</div>
	<div style="text-align: left; margin-top: 20px;">
		<button id="ImportDialog-ImportButton" class="ies-action" name="import-button" type="button">Import</button>
		<button id="ImportDialog-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
	</div>
	<% Html.EndForm(); %>
</div>

<div id="ImportResultsDialog" class="import-results-dialog" style="display: none;">
	The Import of Resource options was successful.<br />
	<br />
	<div class="container">
		<div id="ImportResultsDialog-Added"></div>
		<div id="ImportResultsDialog-Deleted"></div>
		<div id="ImportResultsDialog-Changed"></div>
		<div id="ImportResultsDialog-InUse"></div>
	</div>
	<div class="buttons">
		<button id="ImportResultsDialog-CloseButton" class="ies" name="close-button" type="button">Close</button>
	</div>
</div>

<div class="manage-boe-resources-edit" id="OptionDialog" style="display: none;">

	<% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ResourceOptionForm" }))
		{ %>
	<ul class="validation-box"></ul>
	<div id="InuseWarningFlag" class="warning-box">
		<div class="warning-message" style="padding-right: 5px; padding-left: 55px">
			Resource option is currently in use by at least
			<br />
			one BOE. Any changes to an in use resource
			<br />
			option will apply to all BOEs using the resource.
			<br />
			All BOE authors and approvers of modified BOEs
			<br />
			in the Awaiting Approval & Approved state will be
			<br />
			notified of changes via email.
		</div>

		<div class="small-close-button" onclick="BOECustomFieldsResourceWidget.InUseEditsConfirmed()"></div>
	</div>
	<div class="form-row">
		<div class="form-label">ID*</div>
		<div class="form-element">
			<%: Html.TextBoxFor(model => model.ID, new { @maxlength = "20" })%>
			<input type="hidden" id="UpdateDateLong" name="UpdateDateLong" />
			<input type="hidden" id="ResourceID" name="ResourceID" />
			<input type="hidden" id="ResourceListID" name="ResourceListID" value="<%: ViewData["SYSTEM_LIST_ID"] %>" />

		</div>
	</div>
	<div class="form-row">
		<div class="form-label">Description*</div>
		<div class="form-element">
			<%: Html.TextBoxFor(model => model.Description, new { @maxlength = "100" })%>
		</div>
	</div>
	<div class="form-row">
		<div class="form-label">Segment Region*</div>
		<div class="form-element">
			<%: Html.TextBoxFor(model => model.SegmentRegion, new {@maxlength = "50" })%>
		</div>
	</div>
	<div class="form-row">
		<div class="form-label">Labor Type*</div>
		<div class="form-element">
			<%: Html.TextBoxFor(model => model.LaborType, new { @maxlength = "50" })%>
		</div>
	</div>
	<div class="form-row">
		<div class="form-label">Rate Type*</div>
		<div class="form-element">
			<%: Html.DropDownListFor(m => Model.RateTypeID, (IEnumerable<SelectListItem>)ViewData["RateTypes"])%><!---->
		</div>
	</div>
	<div class="form-row">
		<div class="form-label">Element of Cost*</div>
		<div class="form-element">
			<select class="element-of-cost" id="ElementOfCostId" name="ElementOfCostId">
				<%foreach (var listItem in (IEnumerable<SelectListItem>)ViewData["ElementOfCostTypes"])
					{ %>
				<option value="<%:listItem.Value%>"
					<% if (listItem.Selected) Response.Write("selected=true"); %>>
					<%:listItem.Text %>
				</option>
				<%}%>
			</select>
		</div>
	</div>
	<div class="form-row">
		<div class="form-label"></div>
		<div class="form-element">
			<div class="buttons" style="width: 200px;">
				<button id="Save-OptionDialog" class="ies-action" name="save-button" type="button">Save</button>
				<div id="Loader-OptionDialog" class="loader display-none"></div>
				<button id="BOECustomFieldsResource-Cancel-OptionDialog" class="ies" name="cancel-button" type="button">Cancel</button>
			</div>
		</div>
	</div>

	<% } %>
</div>

<div id="RestoreResultsDialog" class="restore-results-dialog" style="display: none;">
	<div id="RestoreResultsDialog-Loader" class="loader"></div>
	<div id="RestoreResultsDialog-Waiting">
		<br />
		Restoring Resource Data
	</div>
	<div id="RestoreResultsData">
	</div>
</div>

<div id="DefaultResourceList" class="default-resouce-dialog" style="display: none;">
	<div id="DefaultResourceList-Loader" class="loader"></div>
	<div id="DefaultResourceList-Waiting">
		<br />
		Retrieving <%: ViewData["SYSTEM_LIST_NAME"]%> Default Options   
	</div>
	<div id="DefaultListData">
	</div>
</div>




