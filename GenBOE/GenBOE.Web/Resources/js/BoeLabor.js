/*
This javascript file is called from BOELabor ascx files
*/

function InitializeImportLaborTypeWidget(boeId, workspace, completeImportUrl, spreadTypeHours, spreadTypeCost) {
	var ImportLaborType = new Widget("import_LaborType", false);
	ImportLaborType.ImportErrorDialog = {};
	ImportLaborType.ImportLaborTypesDialog = {};
	ImportLaborType.ImportLaborTypesResultsDialog = {};

	ImportLaborType.CurrentWorkspace = workspace;
	ImportLaborType.BoeID = boeId;

	ImportLaborType.Import = function () {
		$("#ManageLaborType-ImportButton").addClass('display-none');
		$("#ManageLaborType-ImportLoader").removeClass('display-none');

		// This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
		$('#ImportLaborTypeDialog-Project-DocumentDomain').val(document.domain);
		// Remove the old hidden iFrame, if it exists
		$('#ImportLaborTypeDialog-UploadTarget').remove();
		// Append the iFrame to the body, causing the controller action to fire and
		// the download to occur inside the iFrame
		$('#ImportLaborTypeDialog-Project-Form').append('<iframe id="ImportLaborTypeDialog-UploadTarget" name="ImportLaborTypeDialog-UploadTarget" class="display-none"></iframe>');
		$('#ImportLaborTypeDialog-UploadTarget').load(ImportLaborType.UploadComplete);

		$("#ImportLaborTypeDialog-Project-Form").submit();
	};

	ImportLaborType.createPreviewOutput = function (lt) {

		// Helper function that checks if the original field is null, if so, return the invalid value
		function returnInvalidValueIfNull(valid, invalid) {
			if (valid && valid.toString().trim() !== '') {
				return valid;
			}
			return invalid || 'invalid';
		}

		var resource = returnInvalidValueIfNull(lt.Resource, lt.InvalidResource);
		var brc = returnInvalidValueIfNull(lt.BusinessResourceCode, lt.InvalidBusinessResourceCode);
		var performingOrg = returnInvalidValueIfNull(lt.PerformingOrg, lt.InvalidPerformingOrg);

		var toReturn =
			'<li>' +
			'Resources:' + resource + 
			' Business Resource Code:' + brc +
			' Performing Org:' + performingOrg +
			' Date Range:' + lt.StartDateFormatted +
			'-' + lt.EndDateFormatted +
			' Curve:' + lt.Curve;

		if (lt.SpreadType == spreadTypeHours) {
			if (lt.ValueSpread != undefined) {
				toReturn += ' Hours Spread:' + lt.ValueSpread;
			}

			if (lt.PercentSpread != undefined) {
				toReturn += ' Percent Spread:' + lt.PercentSpread;
			}
		}
		else if (lt.SpreadType == spreadTypeCost) {
			if (lt.ValueSpread != undefined) {
				toReturn += ' Cost:$' + lt.ValueSpread;
			}
		}

		toReturn += '</li>';

		return toReturn;
	};


	ImportLaborType.showImportDialog = function () {

		$('#importLaborTypesDialog input[name=ImportType]').each(function () { $(this).prop("checked", false).prop("disabled", false); });
		$('#importLaborTypesDialog .step[path]').addClass('display-none');
		$("#ImportLaborTypeDialog-Project-Form")[0].reset();
		ImportLaborType.OpenDialogAfterInitialize(ImportLaborType.ImportLaborTypesDialog);
	};

	ImportLaborType.CompleteImport = function () {

		$("#ManageLaborType-CompleteImportButton").addClass('display-none');
		$("#ManageLaborType-CompleteImportLoader").removeClass('display-none');

		var dataToPrep = {};
		dataToPrep.importResults = ImportLaborType.ImportedData;

		for (x in dataToPrep.importResults) {
			var result = dataToPrep.importResults[x];
			result.StartDate = result.StartDateFormatted;
			result.EndDate = result.EndDateFormatted;
			if (result.ValueSpread != null) {
				result.ValueSpread = result.ValueSpread + "";
			}
			if (result.PercentSpread != null) {
				result.PercentSpread = result.PercentSpread + "";
			}
			delete result.LaborSpreads;
			delete result.UpdateDate;
			delete result.Updateable;
			delete result.SpreadType;

			// fix up the imported labor spreads
			for (y in result.ImportedLaborSpreads) {
				result.ImportedLaborSpreads[y].LaborSpreadDate = result.ImportedLaborSpreads[y].LaborSpreadDateFormatted;

				delete result.ImportedLaborSpreads[y].LaborSpreads;
				delete result.ImportedLaborSpreads[y].UpdateDate;
				delete result.ImportedLaborSpreads[y].Updateable;
				delete result.ImportedLaborSpreads[y].SpreadType;
			}
		}



		dataToPrep.moqEquation = $.trim($("#MOQEquation").val());

		var dataToSend = JSON.stringify(dataToPrep);

		$.ajax({
			type: 'POST',
			data: dataToSend,
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			url: completeImportUrl,
			success: function (response) {
				ImportLaborType.CloseDialog(ImportLaborType.ImportLaborTypesResultsDialog);
				$("#ManageLaborType-CompleteImportLoader").addClass('display-none');
				$("#ManageLaborType-CompleteImportButton").removeClass('display-none');
				RaiseNotification("Import Successful");

				// refresh data in angular grid
				var scope = angular.element(document.querySelector("#TaskElementsComposite")).scope();
				scope.refresh();
				scope.$apply();
			},
			error: function () {
				$("#ManageLaborType-CompleteImportLoader").addClass('display-none');
				$("#ManageLaborType-CompleteImportButton").removeClass('display-none');
			}
		});
	};

	return ImportLaborType;
}

function AfterDomLoadImportLaborTypeWidget(ImportLaborType) {
	ImportLaborType.afterDOMLoad();
	createModule($('#import_LaborType'));
	refreshModule($('#import_LaborType'));

	$("#LaborTypeImportErrors button[name='ok-button']").click(function () {
		ImportLaborType.CloseDialog(ImportLaborType.ImportErrorDialog);
	});

	$("#importLaborTypesDialog button[name='cancel-button']").click(function () {
		ImportLaborType.CloseDialog(ImportLaborType.ImportLaborTypesDialog);
	});

	$("#importLaborTypesDialog button[name='import-button']").click(function () {
		ImportLaborType.Import();
	});

	$('#importLaborTypesDialog input[name=ImportType]').change(function () {
		var selected = $(this).val();
		$('input[name=laborTypeImportType]').val(selected.toString().toLowerCase());
		$('#importLaborTypesDialog div.step').each(function () {
			if ($(this).attr('path') == undefined || $(this).attr('path').indexOf(selected) >= 0)
				$(this).removeClass('display-none');
			else
				$(this).addClass('display-none');
		});
	});

	$("#ImportLaborTypeResults #ManageLaborType-CompleteImportButton").click(function () {
		ImportLaborType.CompleteImport();
	});

	$("#ImportLaborTypeResults #ManageLaborType-Back").click(function () {
		ImportLaborType.CloseDialog(ImportLaborType.ImportLaborTypesResultsDialog);
		ImportLaborType.OpenDialogAfterInitialize(ImportLaborType.ImportLaborTypesDialog);
	});

	ImportLaborType.ImportErrorDialog.Element = $("#LaborTypeImportErrors");
	ImportLaborType.ImportErrorDialog.Params = { width: 500, height: 'auto', modal: true, resizable: false, draggable: true, closeOnEscape: false };

	ImportLaborType.ImportLaborTypesDialog.Element = $("#importLaborTypesDialog");
	ImportLaborType.ImportLaborTypesDialog.Params = { title: "Import Resource Types", modal: true, resizable: false, width: 680, height: 633 };

	ImportLaborType.ImportLaborTypesResultsDialog.Element = $("#ImportLaborTypeResults");
	ImportLaborType.ImportLaborTypesResultsDialog.Params = { title: "Import Resource Types", modal: true, resizable: false, width: 680 }

	ImportLaborType.InitializeDialog(ImportLaborType.ImportErrorDialog);
	ImportLaborType.InitializeDialog(ImportLaborType.ImportLaborTypesDialog);
	ImportLaborType.InitializeDialog(ImportLaborType.ImportLaborTypesResultsDialog);
}

function InitializeTaskElementDetailsWidget(readOnly, workspaceState, boeId, taskElementId, laborTypeWarning, loadMOQEquationUrl, confirmWarningUrl,
	boeStateNotDraft, boeStateDraftOrDraftLocked, allowDateShift, recalculateAndRefreshPageUrl, dateShiftUrl, saveReorderLaborTypesUrl, showDescQuestions,
	numberDescQuestions, rteFieldSize) {
	var TaskElementDetailsWidget;
	var formConfigs = [];
	formConfigs.push({
		ElementID: 'TaskElementDetailsForm',
		Buttons: [],
		HideOCI: true,
	});

	var widgetConfig = {};
	widgetConfig.FormConfigs = formConfigs;
	widgetConfig.ContextID = "TaskElementDetailsModule";
	widgetConfig.isReadOnly = readOnly;
	widgetConfig.IsModule = true;

	TaskElementDetailsWidget = new GenWidget(widgetConfig);

	TaskElementDetailsWidget.WorkspaceState = workspaceState;
	TaskElementDetailsWidget.BoeId = boeId;
	TaskElementDetailsWidget.TaskElementId = taskElementId;
	TaskElementDetailsWidget.LTWarningLoadedAtStartup = laborTypeWarning;
	TaskElementDetailsWidget.waitingBeforeSubmit = false;
	TaskElementDetailsWidget.ValidationCreated = false;
	TaskElementDetailsWidget.isInitialization = false;

	TaskElementDetailsWidget.Search = {};
	TaskElementDetailsWidget.ChildWidgets = [];

	TaskElementDetailsWidget.BoeStateDraftOrDraftLocked = boeStateDraftOrDraftLocked;

	TaskElementDetailsWidget.ToggleHelp = function (helpButton, side, modifyTop) {
		var helpDialog = $(helpButton).next();

		//modifyTop defaults to true - raises the position of the top of the dialog
		if (modifyTop == undefined || modifyTop == true) {
			helpDialog.css("top", $(helpButton).position().top - helpDialog.height());
		}

		if (side != undefined && side.toLowerCase() == 'left') {
			helpDialog.css("left", $(helpButton).position().left - helpDialog.width() - 20);
		}
		else {
			helpDialog.css("left", $(helpButton).position().left + $(helpButton).width() + 5);
		}

		helpDialog.toggle("drop");
	};

	TaskElementDetailsWidget.messageConfirmed = function () {
		$(document).trigger("LABORTYPE_MESSAGE_USER_CONFIRMED");
	};

	TaskElementDetailsWidget.registerForEvent('hideMOQChangedNotification', function () {
		$('#LaborTypeWarningFlag').hide();
		$("#labor-typesmoduleexpandedModuleContent0").hide();
		TaskElementDetailsWidget.refreshModule();
	});

	TaskElementDetailsWidget.openWindow = function (currentWorkspace, boeLaborController, action) {
		var url = CreatePostURL(currentWorkspace, boeLaborController, action);
		var name = 'Curves';
		var width = 840;
		var height = 560;

		if (action.contains('BusinessResourceCodes')) {
			name = 'BusinessResourceCodes';
			width = 770;
			height = 580;
		} else if (action.contains('DisplayResource')) {
			name = 'Resources';
			width = 770;
			height = 580;
		} else if (action.contains('Perf')) {
			name = 'Performing Orgs';
			width = 700;
		}
		var newwindow = window.open(url, name, 'scrollbars=1,toolbar=no,resizeable=no,status=no,width=' + width + ',height=' + height);
	};

	TaskElementDetailsWidget.CancelToMainGrid = function () {
		if (TaskElementDetailsWidget.isAnyDirty()) {
			Session.confirmDialog(
				"Cancel",
				"Are you sure you want to cancel all changes?",
				function () {
					TaskElementDetailsWidget.cleanAllDirty();

					for (widgetIndex in TaskElementDetailsWidget.ChildWidgets) {
						TaskElementDetailsWidget.ChildWidgets[widgetIndex].cleanDirty();
					}

					$("#BOESummary").html('<div class="loader"></div>');
					$(document).trigger('BOESUMMARYGRID_RELOAD');
					window.location.hash = 'LMLabor';
				},
				null);
		}
		else {
			$("#BOESummary").html('<div class="loader"></div>');
			$(document).trigger('BOESUMMARYGRID_RELOAD');
			window.location.hash = 'LMLabor';
		}
	};

	TaskElementDetailsWidget.LoadMOQEquationField = function (content, id) {
		$.ajax({
			type: 'POST',
			url: loadMOQEquationUrl + id,
			dataType: 'html',
			success: function (response) {

				content.html(response);
				content.each(function () {
					var contentSelector = $(this);
					angular.element(document).injector().invoke(
						[
							"$compile", function ($compile) {
								var scope = angular.element(contentSelector).scope();
								$compile(contentSelector)(scope);
								scope.$apply();
							}
						]);
				});
			},
			error: function () {
			}
		});
	};

	TaskElementDetailsWidget.markWarningMessageAsConfirmed = function () {
		var dataToSend = { "TaskID": TaskElementDetailsWidget.TaskElementId }
		dataToSend = JSON.stringify(dataToSend);

		$.ajax({
			type: 'POST',
			url: confirmWarningUrl,
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			data: dataToSend,
			complete: function () { $(document).trigger('hideMOQChangedNotification'); }
		});
	};

	TaskElementDetailsWidget.ResetDialogTitle = function (from, to) {
		var closeButton = $("button.ui-dialog-titlebar-close");
		if (closeButton.length > 0) {
			var titles = closeButton.siblings("span.ui-dialog-title");
			if (titles.length > 0) {
				for (var i = titles.length - 1; i > 0; i--) {
					titles.each(function (i, j) {
						if ($(j).text() == from) {
							$(j).text(to);
							return false;
						}
					});
				}
			}
		}
	};

	TaskElementDetailsWidget.EnableLockedStateFields = function () {
		if (TaskElementDetailsWidget.WorkspaceState == "Locked") {
			$("#TaskElementDetails-MOQTypesDropDownList").attr("readonly", true);
			$(".LockedWorkspaceState").addClass('display-none');

			// check BOE state (widget is not read-only, but adjust dates link is treated separately)
			if (boeStateNotDraft) {
				$('#AdjustTaskDatesLink').addClass('display-none');
			}
		}
	};

	// Under certain conditions, a second element with the same ID is present. Attach to the second if it exists.
	// TODO: root cause this issue IES-977.
	const fieldContainer = $('[id=MOQEquationFieldContent]:eq(1)');
	if (fieldContainer.length) {
		TaskElementDetailsWidget.LoadMOQEquationField(fieldContainer, taskElementId);
	} else {
		TaskElementDetailsWidget.LoadMOQEquationField($('#MOQEquationFieldContent'), taskElementId);
	}

	TaskElementDetailsWidget.on('CLEAN_BOE_DETAILS_DIRTY', function () { TaskElementDetailsWidget.cleanDirty(); });

	TaskElementDetailsWidget.EnableLockedStateFields();

	TaskElementDetailsWidget.on('click', '#AdjustTaskDatesLink', function () {
		window.location = dateShiftUrl;
	});

	$('#ChangeTaskDatesForm').on('change', "input[name=NewStartDate], input[name=NewEndDate]", TaskElementDetailsWidget.CheckDateRange);
	$('#ChangeTaskDatesForm').on('change', 'input', function () {
		var next = $(this).closest('form').find("button[name='next-button']");
		var screen = next.attr('screen');
		$(this).closest('form').find("button[name='finish-button']").removeClass('disabled');

		TaskElementDetailsWidget.AdjustDates[screen] = "changed";
	});
	$('#ChangeTaskDatesForm').on('change', 'input[name=FlowdownUpdateSelection]', function () {
		var form = $(this).closest('form');

		if ($(this).val() == "Automatic" && TaskElementDetailsWidget.AdjustDates.HasDiscreteTypes) {
			form.find("button[name='next-button']").removeClass('display-none disabled');
			form.find("button[name='finish-button']").addClass('display-none');
		} else {
			form.find("button[name='next-button']").addClass('display-none');
			form.find("button[name='finish-button']").removeClass('display-none');
		}
	});

	if (!TaskElementDetailsWidget.isReadOnly()) {
		if (!(TaskElementDetailsWidget.TaskElementId > 0)) {
			$('#AdjustTaskDatesLink').addClass('display-none');

			$('#TaskElementDetailsForm input[name=StartDate]').prop('disabled', false).datepicker({
				onSelect: function () {
					$(this).keydown();
					$(this).blur();
					$(this).addClass('active');
					$(this).change();
				}
			});
			$('#TaskElementDetailsForm input[name=EndDate]').prop('disabled', false).datepicker({
				onSelect: function () {
					$(this).keydown();
					$(this).blur();
					$(this).addClass('active');
					$(this).change();
				}
			});
		}
	}
	else {
		$("#InsertWorkspaceVariableLink").parent().prev().remove();
		$("#InsertWorkspaceVariableLink").parent().remove();
		$("#SearchEstimatingCatalogLink").parent().prev().remove();
		$("#SearchEstimatingCatalogLink").parent().remove();

		if (allowDateShift == 'false') {
			$('#AdjustTaskDatesLink').addClass('display-none');
		}

		TaskElementDetailsWidget.refreshModule();
	}

	// Reorder dialog functions for Labor Types

	TaskElementDetailsWidget.ReOrderLaborTypesDialog = {};
	TaskElementDetailsWidget.ReOrderLaborTypesDialog.Element = $("#ReOrderLaborTypesDialog");
	TaskElementDetailsWidget.ReOrderLaborTypesDialog.Params = { width: 700, height: 365, modal: true, resizable: false, draggable: true, title: 'Sort Resource Types' };

	TaskElementDetailsWidget.DisplayReOrderLaborTypesDialog = function () {
		TaskElementDetailsWidget.OpenDialogAfterInitialize(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
	};

	$('#ReOrderLaborTypesDialog #LaborsMoveItemsUp').click(function () {
		TaskElementDetailsWidget.ShiftSelectedItemsUp($('#ReOrderLaborTypesDialog #LaborTypesList'));
		TaskElementDetailsWidget.EnableSave();
	});

	$('#ReOrderLaborTypesDialog #LaborsMoveItemsDown').click(function () {
		TaskElementDetailsWidget.ShiftSelectedItemsDown($('#ReOrderLaborTypesDialog #LaborTypesList'));
		TaskElementDetailsWidget.EnableSave();
	});

	TaskElementDetailsWidget.EnableSave = function () {
		$('#ReOrderLaborTypesDialog-Save').removeClass('disabled');
	};

	TaskElementDetailsWidget.CancelReOrderLaborTypes = function () {
		TaskElementDetailsWidget.CloseDialog(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
		$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').addClass('display-none');
		$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').addClass('disabled');
		$(document).trigger('LOAD_TASK_ELEMENT_GRID');
	};

	TaskElementDetailsWidget.SaveReOrderLaborTypes = function () {

		$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').addClass('display-none');
		$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').removeClass('display-none');

		var dataToSend = {};

		var listOrder = 0;
		dataToSend.LaborTypes = [];
		$('#ReOrderLaborTypesDialog #LaborTypesList').children('option').each(function () {
			var laborTypeToSave = {};
			var laborTypeValue = $(this)[0].value;

			laborTypeToSave.LaborTypeID = laborTypeValue;
			laborTypeToSave.ListOrder = listOrder++;
			dataToSend.LaborTypes.push(laborTypeToSave);
		});

		listOrder = 0;

		dataToSend = JSON.stringify(dataToSend);
		TaskElementDetailsWidget.ajaxRequest({
			type: 'POST',
			url: saveReorderLaborTypesUrl,
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			data: dataToSend,
			success: function (response) {
				TaskElementDetailsWidget.CloseDialog(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
				$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').addClass('display-none');
				$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').removeClass('display-none');
				// refresh the page so we can get the new data
				GenSession.ShowLoadingBox();
				location.reload();
			},
			error: function () {
				$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Loader').addClass('display-none');
				$('#ReOrderLaborTypesDialog #ReOrderLaborTypesDialog-Save').removeClass('display-none');
			}
		});


	};

	TaskElementDetailsWidget.ShiftSelectedItemsDown = function (list) {
		$($(list).children('option:selected').get().reverse()).each(function () {
			if ($(this).next().length === 0) {
				return false;
			}

			$(this).next().after($(this));
		});
	};
	// Moves selected list items up in the list
	TaskElementDetailsWidget.ShiftSelectedItemsUp = function (list) {
		$(list).children('option:selected').each(function () {
			if ($(this).prev().length === 0) {
				return false;
			}

			$(this).prev().before($(this));
		});
	};

	// End Reorder dialog functions for Labor Types

	// Duplicate dialog functions for Labor Types

	TaskElementDetailsWidget.DuplicateLaborTypesDialog = {};
	TaskElementDetailsWidget.DuplicateLaborTypesDialog.Element = $("#DuplicateLaborTypesDialog");
	TaskElementDetailsWidget.DuplicateLaborTypesDialog.Params = { width: 700, modal: true, resizable: false, draggable: true, title: 'Duplicate Resource Types' };

	TaskElementDetailsWidget.DisplayDuplicateLaborTypesDialog = function () {
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').removeClass('display-none');
		TaskElementDetailsWidget.OpenDialogAfterInitialize(TaskElementDetailsWidget.DuplicateLaborTypesDialog);
	};

	TaskElementDetailsWidget.EnableDuplicateSave = function () {
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').removeClass('disabled');
	};

	TaskElementDetailsWidget.ShowDuplicateLoader = function () {
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').addClass('disabled');
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').addClass('display-none');
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Loader').removeClass('display-none');
	};

	TaskElementDetailsWidget.CancelDuplicateLaborTypes = function () {
		TaskElementDetailsWidget.CloseDialog(TaskElementDetailsWidget.DuplicateLaborTypesDialog);
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Loader').addClass('display-none');
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').addClass('disabled');
		$('#DuplicateLaborTypesDialog #DuplicateLaborTypesDialog-Save').removeClass('display-none');
	};

	TaskElementDetailsWidget.UpdateOpenEndedCustomFields = function (inputId, value) {
		var input = document.getElementById(inputId);

		if (input) {
			input.value = value;
		}
	};

	// End Duplicate dialog functions for Labor Types

	TaskElementDetailsWidget.on("AllWidgetLoaded", function () {
		if (TaskElementDetailsWidget.LTWarningLoadedAtStartup) {
			$(document).trigger('showMOQChangedNotification');
		}
	});

	TaskElementDetailsWidget.on("LABORTYPE_MESSAGE_USER_CONFIRMED", function () {
		if (TaskElementDetailsWidget.LTWarningLoadedAtStartup) {
			TaskElementDetailsWidget.markWarningMessageAsConfirmed();
		} else {
			$(document).trigger('hideMOQChangedNotification');
		}
	});

	TaskElementDetailsWidget.deleteToggled = function () {
		if ($("input[name=DeleteResource]:checked").length > 0) {
			$("button[name='delete-button']").removeClass("disabled");
		} else {
			$("button[name='delete-button']").addClass("disabled");
		}
	};

	TaskElementDetailsWidget.deleteAllToggle = function (element) {
		$("input[name=DeleteResource]").prop("checked", $(element).prop("checked"));
		TaskElementDetailsWidget.deleteToggled();
	};


	/*
	 * This does NOT recalculate the page. It recalculates data in the DB, saves it and then reloads the page. Only to be used if the data is bad and recalculation cannot be forced by 
	 * changing values in the page. This is the nuclear option.
	 */
	TaskElementDetailsWidget.RecalculateAndRefreshPage = function () {
		GenSession.confirmDialog(
			"Recalculate?",
			"Recalculation of this task element will correct data and date entries to fall within the BOE period of performance.  Discrete Spread data which falls outside of the period of performance will be deleted.  Resources utilizing a spread curve will be recalculated.  Updates will be automatically saved.  Delta Hours/EPs and discrete costs may need to be addressed post this action.<BR />Do you wish to continue with the recalculation of this task?",
			function () {
				GenSession.ShowLoadingBox();
				$.ajax({
					type: 'POST',
					url: recalculateAndRefreshPageUrl,
					contentType: 'application/json; charset=utf-8',
					dataType: 'html',
					success: function () {
						// refresh the page.. so we can get the new data
						location.reload();
					},
					error: function () {
					}
				});
			},
			null);
	};

	TaskElementDetailsWidget.cleanAllDirty = function () {
		if (!TaskLaborTypesWidget.isReadOnly()) {
			TaskLaborTypesWidget.cleanDirty();
		}
		if (!TaskLaborSpreadsWidget.isReadOnly()) {
			TaskLaborSpreadsWidget.cleanDirty();
		}
		if (!TaskSkillMixWidget.isReadOnly()) {
			TaskSkillMixWidget.cleanDirty();
		}

		TaskElementDetailsWidget.cleanDirty();

		for (widgetIndex in TaskElementDetailsWidget.ChildWidgets) {
			TaskElementDetailsWidget.ChildWidgets[widgetIndex].cleanDirty();
		}
	};

	TaskElementDetailsWidget.isAnyDirty = function () {
		for (widgetIndex in TaskElementDetailsWidget.ChildWidgets) {
			if (TaskElementDetailsWidget.ChildWidgets[widgetIndex].isDirty()) {
				return true;
			}
		}

		return TaskElementDetailsWidget.isDirty() ||
			(!TaskLaborTypesWidget.isReadOnly() && TaskLaborTypesWidget.isDirty()) ||
			(!TaskLaborSpreadsWidget.isReadOnly() && TaskLaborSpreadsWidget.isDirty()) ||
			(!TaskSkillMixWidget.isReadOnly() && TaskSkillMixWidget.isDirty());
	};

	TaskElementDetailsWidget.InitializeDialog = function (dialog) {

		var that = this;
		// Remove old dialogs left behind when jumping between jump pages
		var dialogID = dialog.Element.attr('id');
		if (dialogID !== undefined) {
			$('body .ui-dialog').children('#' + dialogID).parent().remove();
			$('body').children('#' + dialogID).remove();
		}

		// Remove display-none class
		if (dialog.Element.hasClass('display-none')) {
			dialog.Element.removeClass('display-none');
		}

		// Set the dialog disabled so that it doesn't open when initialized
		if (dialog.Params.autoOpen === undefined) {
			dialog.Params.autoOpen = false;
		}
		if (dialog.Params.close === undefined) {
			dialog.Params.close = function () {
				var form = $(dialog.Element).find('form');
				that.cleanDirty(form.attr("id"));
			};
		}

		// Initialize the dialog
		$(dialog.Element).dialog(dialog.Params);
	};

	TaskElementDetailsWidget.OpenDialogAfterInitialize = function (dialog) {
		var form = $(dialog.Element).find('form');
		$(dialog.Element).dialog('open');
	};

	TaskElementDetailsWidget.CloseDialog = function (dialog) {
		$(dialog.Element).dialog('close');
	};

	TaskElementDetailsWidget.TaskDescription = CreateRteTemplate(showDescQuestions, numberDescQuestions);

	if (!readOnly) {
		$('#description-element .replacedWidgetText').remove();
		$('#description-element *').removeClass('display-none');

		InitializeRteTemplate(TaskElementDetailsWidget.TaskDescription, 'TaskDescription', rteFieldSize);
	} else {
		HandleRTETemplateDataForReadOnly(TaskElementDetailsWidget.TaskDescription, 'TaskDescription');
	}

	return TaskElementDetailsWidget;
}

function AfterDomLoadTaskElementDetailsWidget(TaskElementDetailsWidget, taskElementId) {
	TaskElementDetailsWidget.cleanAllDirty();

	$("#labor-typesmoduleexpandedModuleContent0").hide();

	TaskElementDetailsWidget.EnableLockedStateFields();

	TaskElementDetailsWidget.isInitialization = false;

	TaskElementDetailsWidget.InitializeDialog(TaskElementDetailsWidget.ReOrderLaborTypesDialog);
	TaskElementDetailsWidget.InitializeDialog(TaskElementDetailsWidget.DuplicateLaborTypesDialog);

	if (taskElementId === '') {
		$('#LTExportButton').addClass('disabled');
	}
}
