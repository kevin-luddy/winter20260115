var MOQEquationFieldWidget = null;

/// <reference path="directives.js" />
// The controller for the MOQ equation section.
moqEquationApp.controller('MoqEquationController', ['$scope', '$uibModal', '$window', 'ManageTaskModel', '$timeout', '$http', function ($scope, $uibModal, $window, ManageTaskModel, $timeout, $http) {
	$scope.init = function () {

		$scope.model = $window.MOQEquationFieldModel;
		$scope.model.IsCostEquation = ($scope.model.MoqEquationType == 'Cost');
		$scope.model.insertWorkspaceModalOpen = false;

		$scope.newTableId = -1;

		// This is needed to allow for some other processing to finish, otherwise we get errors from angular.js
		setTimeout(function () {
			initializeWidget();

			angular.forEach($scope.model.SelectedMoqTypes.map(e => e.SelectedMOQType.toString()), function (id) {
				$scope.InitializeRteFields(id);
			});

			$scope.refreshDisableSave();
		}, 10);
	};

	$scope.dialog = {
		title: 'Import MOQ Tables',
		open: false,
		file: null,
		importWorking: false,
		completeImportWorking: false,
		showImportResults: false,
		disableImport: true,
		invalidData: false,
		importResults: []
	};

	$scope.filterDialog = {
		open: false,
		data: [],
		title: 'Update Filters',
		isLoading: true,
		fieldsArr: ManageTaskModel.SapFields,
		fields: ManageTaskModel.SapFields.reduce(function (map, obj) {
			map[obj.Value] = obj;
			return map;
		}, {}),
		operators: ManageTaskModel.SapOperators.reduce(function (r, a) {
			r[a.Type] = r[a.Type] || [];
			r[a.Type].push(a);
			return r;
		}, Object.create(null)),
		originalData: undefined,
		showError: false,
		error: []
	};

	$scope.actualsValidation = {
		errors: new Map(),
		isDirty: new Map()
	};

	$scope.isExporting = false;

	$scope.refreshDisableSave = function () {
		// only check for disabling save if SAP is enabled
		if ($scope.model.SAPEnabled && $scope.model.SapConnectionEnabled) {
			// only look at historical and comparative moq
			let moqTypes = $scope.model.SelectedMoqTypes.filter(x => x.SelectedMOQType == $scope.model.ComparativeMoqType || x.SelectedMOQType == $scope.model.HistoricalMoqType);

			// Only continue if in RMS or there is a repository set to SAP/WEBI for SSC
			if ($scope.IsSapSetAsAnyRepository(moqTypes)) {
				// check if any DateOfReport is older than 60 days
				let olderThan60 = false;
				let newTableNeedsCalculated = false;
				let tablesWithUserSource = [];
				const sixtyDays = new Date();
				sixtyDays.setMonth(sixtyDays.getMonth() - 2);

				moqTypes.forEach(moq => {
					if (moq.TableData) {
						moq.TableData.forEach(tableData => {
							if ($scope.IsSAPSetAsRepository(tableData.RepositoryName)) {
								if (tableData.DateOfReport < sixtyDays) {
									olderThan60 = true;
								}

								if (tableData.TotalRelevantHours === undefined) {
									newTableNeedsCalculated = true;
								}

								if ($scope.model.IsRMS && tableData.RepositoryName === ManageTaskModel.RmsSapDisabledSource) {
									tablesWithUserSource.push(tableData.TableName);
								}
							}
						});
					}
				});

				if (olderThan60) {
					ManageTaskModel.DisableSave = true;
					ManageTaskModel.DisableSaveText = 'All MOQ Tables older than two months need to have Actuals recalculated before Saving'
				} else if (newTableNeedsCalculated) {
					ManageTaskModel.DisableSave = true;
					ManageTaskModel.DisableSaveText = 'All new MOQ Tables need to have Actuals calculated before Saving';
				} else if ($scope.actualsValidation.isDirty.size > 0) {
					ManageTaskModel.DisableSave = true;
					ManageTaskModel.DisableSaveText = 'All MOQ Tables that have had filters updated need to have Actuals recalculated before Saving';
				} else if (tablesWithUserSource.length > 0) {
					ManageTaskModel.DisableSave = true;
					ManageTaskModel.DisableSaveText = 'Before saving, the following MOQ Tables must be recalculated after the SAP connection was enabled for the Workspace: ' + tablesWithUserSource.join(", ");
				} else {
					ManageTaskModel.DisableSave = false;
				}
			} else {
				ManageTaskModel.DisableSave = false;
			}
		}
	};

	// Called when the Insert Workspace Variable dropdown item is clicked.
	$scope.InsertWorkspaceVariableClicked = function () {
		// show the modal
		var modalInstance = $uibModal.open({
			templateUrl: $scope.model.BaseUrl + 'Resources/InsertWorkspaceVariable.html',
			controller: 'InsertWorkspaceVariableController',
			windowTopClass: 'bootstrap insert-ws-var-modal',
			windowClass: 'bootstrap',
			backdropClass: 'bootstrap',
			resolve: { model: [function () { return $scope.model.WorkspaceVariables; }] }
		});

		$scope.model.insertWorkspaceModalOpen = true;

		// handle the result
		modalInstance.result.then(function (selectedVariable) {
			$scope.model.insertWorkspaceModalOpen = false;

			if (angular.isDefined(selectedVariable)) {
				// add the variable to the MOQ equation field
				var moqEquationField = $('#MOQEquationField #MOQEquation');
				var previousValue = moqEquationField.val();
				moqEquationField.val(previousValue + selectedVariable.WorkspaceVariableName);

				// re-validate the equation
				$(document).trigger('ValidateMOQEquation');
			}
		});
	};

	// Called when the Search Estimating Catalog dropdown item is clicked.
	$scope.SearchEstimatingCatalogClicked = function () {
		$(document).trigger('SEARCH_METRICS');
	}

	// Called when the Copy MOQ from BOE dropdown item is clicked.
	$scope.CopyMoqFromBoeClicked = function () {
		$('#CopyMoqFromBoeLink').data('moq-task-id', $scope.model.TaskElementId);
		$(document).trigger("COPY_MOQ_SELECT");
	}

	// Adds MOQ Type to Selected MOQ Types (and removes it from the dropdown of available types)
	$scope.AddMoqType = function () {
		var selectedItem = $scope.model.selectedMOQType;
		selectedItem.Order = 2000;

		$scope.model.SelectedMoqTypes.push(selectedItem);
		$scope.InitializeRteFields(selectedItem.SelectedMOQType, true);

		// If both Comparative and Historical exist and one is being added here, 
		// Initialize RTE fields for the other to prevent an issue that occurs after
		// converting one and then adding the other
		if (selectedItem.SelectedMOQType == $scope.model.HistoricalMoqType && $scope.model.SelectedMoqTypes.find(x => x.SelectedMOQType == $scope.model.ComparativeMoqType)) {
			$scope.InitializeRteFields($scope.model.ComparativeMoqType, true);
		} else if (selectedItem.SelectedMOQType == $scope.model.ComparativeMoqType && $scope.model.SelectedMoqTypes.find(x => x.SelectedMOQType == $scope.model.HistoricalMoqType)) {
			$scope.InitializeRteFields($scope.model.HistoricalMoqType, true);
		}

		selectedItem.TableData = [];
		$scope.CreateNewTable(selectedItem.TableData);
		$scope.$emit('MOQ_TYPE_SELECTION_CHANGED', $scope.model.SelectedMoqTypes);
		MOQEquationFieldWidget.setDirty();
	}

	// Removes MOQ Type from Selected MOQ Types (and adds it into the dropdown)
	$scope.RemoveMoqType = function (item) {
		GenSession.confirmDialog('Delete MOQ Type?', 'Are you sure you want to delete the selected MOQ Type and all associated data?<br/> Once deleted, this can not be undone.', function () {
			$scope.$apply(function () {
				var index = $scope.model.SelectedMoqTypes.indexOf(item);
				$scope.model.SelectedMoqTypes.splice(index, 1);
				$scope.$emit('MOQ_TYPE_SELECTION_CHANGED', $scope.model.SelectedMoqTypes);
				MOQEquationFieldWidget.setDirty();
				$scope.refreshDisableSave();
			});
		});
	}

	// Used by RTE methods to update the scope when is dirty changes
	// what makes this work is the MoqEquationApp configuration of having a window listener throw the event into scope
	$scope.$on('RteDirtyChanged', function () {
		$scope.$apply();
	});

	// Initializes RTE fields for the selected MOQ Type option
	$scope.InitializeRteFields = function (id, skipInitialClean) {
		// setTimeout is needed to allow for the objects to be added into the DOM, before we can transform them into RTE
		setTimeout(function () {
			if (!$scope.model.IsReadOnly || $scope.model.ShouldMoqReadOnlyBeReversed) {
				InitializeRTE('DescriptionHoursRequired_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
				InitializeRTE('SmeReason_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
				InitializeRTE('SmeHoursLogic_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
				InitializeRTE('SmeDurationLogic_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
				InitializeRTE('SmeTaskEstimates_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
				InitializeRTE('Rationale_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
				InitializeRTE('SkillMixRationale_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
			}

			if ($scope.model.IsReadOnly && $scope.model.ShouldMoqReadOnlyBeReversed) {
				$('.moqContainerClass .replacedWidgetText').remove();
			}
		}, 1);
	}

	// Actual Read Only, including reversal
	$scope.ActualReadOnly = function () {
		return $scope.model.IsReadOnly && !$scope.model.ShouldMoqReadOnlyBeReversed;
	};

	// Create New Table Data for the MOQ Type
	$scope.CreateNewTable = function (tableDataArray) {
		var newTable = {};
		newTable.Id = $scope.newTableId--;
		newTable.Order = 2000;

		tableDataArray.push(newTable);
		MOQEquationFieldWidget.setDirty();
		$scope.refreshDisableSave();
	};

	// Remove existing Table Data
	$scope.RemoveTable = function (item, tableDataArray) {
		GenSession.confirmDialog('Delete Data Table?', 'Are you sure you want to delete the selected Data Table?<br/> Once deleted, this can not be undone.', function () {
			$scope.$apply(function () {
				var index = tableDataArray.indexOf(item);
				tableDataArray.splice(index, 1);
				MOQEquationFieldWidget.setDirty();
				$scope.actualsValidation.isDirty.delete(item.Id);
				$scope.refreshDisableSave();
			});
		});
	}

	// Generates string for the description of what MOQ types are based on
	$scope.PortionOfTask = function () {
		if ($scope.model.SelectedMoqTypes.length > 1) {
			return 'This portion of the task is based on ';
		} else {
			return 'This task is based on ';
		}
	}

	// Generates placeholder text for MOQ Types
	$scope.MoqTypesPlaceholder = function (field, selectedMOQType) {
		/*
			Enum values:
			Historical = 5001,
			Comparative = 5002,
			CostEstimatingRelationships = 5003,
			ParametricEstimates = 5004,
			AnalogousRelationships = 5005,
			SOW = 5006,
			LOE = 5007,
			SME = 5008,
			NonLabor = 5009
		*/
		switch (field) {
			case 'Rationale':
				switch (parseInt(selectedMOQType)) {
					case 5001:
						return 'Need to provide how the actual hours are relevant to the proposed effort.  Explain complexity factors.';
					case 5002:
						return 'Need to provide how the similar historical actuals are relevant to the proposed effort.  Explain all complexity factors and skill mix.';
					case 5003:
						return 'If CERs is not submitted in advance to USG, provide a complete explanation of how the model works, the historical or other data sources used, and how any model output was used to calculate proposed hours.  Proposal teams cannot claim the model is Lockheed Martin Proprietary Information and not provide.';
					case 5004:
						return 'If parametric estimating model is not submitted in advance to USG, provide a complete explanation of how the model works, the historical or other data sources used, and how any model output was used to calculate proposed hours.  Proposal teams cannot claim the model is Lockheed Martin Proprietary Information and not provide.';
					case 5005:
						return 'If AR is not submitted in advance to USG, provide a complete explanation of how the model works, the historical or other data sources used, and how any model output was used to calculate proposed hours.  Proposal teams cannot claim the model is Lockheed Martin Proprietary Information and not provide.';
					case 5006:
						return 'Document any business area hours per month used.';
					case 5007:
						return 'Document any business area hours per month used & why LOE selected.';
					case 5009:
						return 'Document any Non-Labor values used & why.';
					case 5008:
						return 'For each task please provide task name, a description of the task and the number of hours.  In the SME judgment estimate please break estimated tasks into small enough chunks that customer can fully understand what is being estimated.';
				}
				break;
			case 'Location':
				switch (parseInt(selectedMOQType)) {
					case 5003:
					case 5004:
					case 5005:
						return 'Reminder:  need to provide a copy of any model used/created that was used for estimating.';
				}
				break;
			case 'Skill Mix Rationale':
				return 'Need to provide a narrative which documents the source and rationale as to why the proposed skill mix was selected. Remember: If based on actuals and not the exact same labor mix, a summary table of the historical labor mix is required. Click the grey question mark for additional information and suggested format.';

		}
	}

	// Toggles an item between collapsed and expended
	$scope.toggle = function (item) {
		item.collapsed = !item.collapsed;
	}

	$scope.openHelp = function (suffix) {
		var url = $scope.model.MoqTypeHelpUrls.BaseUrl + suffix;
		$window.open(url, '_blank');
	};

	//#region MOQ Type Ordering

	// Returns an Ordered Selected Moq Types array
	$scope.OrderedSelectedMoqTypes = function () {
		return $scope.model.SelectedMoqTypes.sort((a, b) => (a.Order < b.Order) ? -1 : 1);
	}

	// Used to lock the page up while sorting. This is needed for TinyMCE to initialize correctly.
	$scope.sortingInProgress = false;

	// Move MOQ Type up
	$scope.MoveUpMoqType = function () {
		$scope.sortingInProgress = true;
		var newOrderArray = [];
		var pos = 0;
		$scope.OrderedSelectedMoqTypes().forEach(function (item) {
			if (item.SelectedMOQType === $scope.model.SortingMoqType.SelectedMOQType) {
				var previous = newOrderArray.pop();
				item.Order = pos - 1;
				newOrderArray.push(item);

				if (previous) {
					previous.Order = pos++;
					newOrderArray.push(previous);
				}
			}
			else {
				item.Order = pos++;
				newOrderArray.push(item);
			}
		});

		$scope.UpdateOrderingMoqTypes(newOrderArray);
	}

	// Move MOQ Type down
	$scope.MoveDownMoqType = function () {
		$scope.sortingInProgress = true;
		var newOrderArray = [];
		var pos = 0;
		var nextOffset = 0;
		$scope.OrderedSelectedMoqTypes().forEach(function (item) {
			if (item.SelectedMOQType === $scope.model.SortingMoqType.SelectedMOQType) {
				item.Order = 1 + pos++;

				nextOffset = -1;
			}
			else {
				item.Order = nextOffset + pos++;

				nextOffset = 0;
			}

			newOrderArray.push(item);
		});

		$scope.UpdateOrderingMoqTypes(newOrderArray);
	}

	// Applies the new order of MOQ Types to the underlying model.
	$scope.UpdateOrderingMoqTypes = function (newOrderArray) {
		$timeout(function () {
			$scope.model.SelectedMoqTypes.forEach(function (item) {
				var matchingItem = newOrderArray.find(({ SelectedMOQType }) => SelectedMOQType === item.SelectedMOQType);
				item.Order = matchingItem.Order;

				InitializeRTE('DescriptionHoursRequired_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
				InitializeRTE('SmeReason_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
				InitializeRTE('SmeHoursLogic_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
				InitializeRTE('SmeDurationLogic_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
				InitializeRTE('SmeTaskEstimates_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
				InitializeRTE('Rationale_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
				InitializeRTE('SkillMixRationale_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
			});

			$timeout(function () { $scope.sortingInProgress = false; }, 0);
		}, 0);
	}

	// Returns an Ordered Selected Moq Types array
	$scope.OrderedSelectedMoqTables = function () {
		return $scope.model.SortingMoqTablesMoqTypeParent.TableData.sort((a, b) => (a.Order < b.Order) ? -1 : 1);
	}

	// Move MOQ Table up
	$scope.MoveUpMoqTable = function () {
		var newOrderArray = [];
		var pos = 0;
		$scope.OrderedSelectedMoqTables().forEach(function (item) {
			if (item.TableName === $scope.model.SortingMoqTable.TableName) {
				var previous = newOrderArray.pop();
				item.Order = pos - 1;
				newOrderArray.push(item);

				if (previous) {
					previous.Order = pos++;
					newOrderArray.push(previous);
				}
			}
			else {
				item.Order = pos++;
				newOrderArray.push(item);
			}
		});

		$scope.UpdateOrderingMoqTables(newOrderArray);
	}

	// Move MOQ Table down
	$scope.MoveDownMoqTable = function () {
		var newOrderArray = [];
		var pos = 0;
		var nextOffset = 0;
		$scope.OrderedSelectedMoqTables().forEach(function (item) {
			if (item.TableName === $scope.model.SortingMoqTable.TableName) {
				item.Order = 1 + pos++;
				nextOffset = -1;
			}
			else {
				item.Order = nextOffset + pos++;
				nextOffset = 0;
			}

			newOrderArray.push(item);
		});

		$scope.UpdateOrderingMoqTables(newOrderArray);
	}

	// Applies the new order of MOQ Tables to the underlying model.
	$scope.UpdateOrderingMoqTables = function (newOrderArray) {
		$timeout(function () {
			$scope.$apply(function () {
				$scope.model.SortingMoqTablesMoqTypeParent.TableData.forEach(function (item) {
					var matchingItem = newOrderArray.find(({ TableName }) => TableName === item.TableName);
					item.Order = matchingItem.Order;
				});
			});
		}, 0);
	}

	// Display Reorder MOQ Types Dialog
	$scope.displayReOrderMoqTypesDialog = function () {
		$scope.model.SortingMoqType = $scope.model.SelectedMoqTypes[0];
		MOQEquationFieldWidget.OpenDialogAfterInitialize(MOQEquationFieldWidget.ReOrderMoqTypesDialog);
	};

	// Close Reorder MOQ Types Dialog
	$scope.closeReOrderMoqTypes = function () {
		MOQEquationFieldWidget.CloseDialog(MOQEquationFieldWidget.ReOrderMoqTypesDialog);
		MOQEquationFieldWidget.setDirty();
	}

	// Display Reorder MOQ Tables Dialog
	$scope.displayReOrderMoqTablesDialog = function (moqType) {
		$scope.model.SortingMoqTablesMoqTypeParent = moqType;
		$scope.model.SortingMoqTable = moqType.TableData[0];

		MOQEquationFieldWidget.OpenDialogAfterInitialize(MOQEquationFieldWidget.ReOrderMoqTablesDialog);
	};

	// Close Reorder MOQ Tables Dialog
	$scope.closeReOrderMoqTables = function () {
		MOQEquationFieldWidget.CloseDialog(MOQEquationFieldWidget.ReOrderMoqTablesDialog);
		$scope.model.SortingMoqTables = [];
		MOQEquationFieldWidget.setDirty();
	}

	$scope.clearPoPDates = function (tableData) {
		tableData.PoPStart = undefined;
		tableData.PoPStartWeek = undefined;
		tableData.PoPStartYear = undefined;
		tableData.PoPEnd = undefined;
		tableData.PoPEndWeek = undefined;
		tableData.PoPEndYear = undefined;
	}

	$scope.disableHistoricalComparativeConvertButtons = function () {
		// Disable the buttons if task already has both Historical and Comparative MOQ Types
		return $scope.model.SelectedMoqTypes.some(function (moqType) {
			return moqType.SelectedMOQType == $scope.model.HistoricalMoqType;
		}) && $scope.model.SelectedMoqTypes.some(function (moqType) {
			return moqType.SelectedMOQType == $scope.model.ComparativeMoqType;
		});
	}

	$scope.convertMoqType = function (moqType, convertToType) {
		var convertToMoq = $scope.model.MOQTypes.find(x => x.SelectedMOQType == convertToType);

		moqType.SelectedMOQType = convertToMoq.SelectedMOQType;
		moqType.SelectedMOQTypeText = convertToMoq.SelectedMOQTypeText;

		MOQEquationFieldWidget.setDirty();
	}

	/*
	* **************************** NOTE **********************************
	* Functions below relate to the logic of the MOQ Table import/export
	* ********************************************************************
	*/

	$scope.isDirty = function () {
		return (MOQEquationFieldWidget && MOQEquationFieldWidget.isDirty())
			|| (TaskElementDetailsWidget && TaskElementDetailsWidget.isDirty());
	};

	$scope.openImportMoqTables = function (moqType) {
		$scope.model.ImportingMoqType = moqType;
		resetUploadForm();
		$scope.dialog.open = true;
	};

	$scope.closeImportMoqTables = function () {
		resetUploadForm();
		$scope.dialog.open = false;
	};

	$scope.fileUploadChange = function (element) {
		$scope.$apply(function ($scope) {
			$scope.dialog.disableImport = element.value.endsWith('.xlsx') || element.value.endsWith('.xlsm') ? false : true;
			$scope.dialog.file = $scope.dialog.disableImport ? null : element.files[0];
		});
	};

	var resetUploadForm = function () {
		$("#ImportMoqTableDialog-Form")[0].reset();
		$scope.dialog.disableImport = true;
		$scope.dialog.file = null;
	};

	$scope.importMoqTables = function () {
		// create form data
		var fd = new FormData();
		fd.append("file", $scope.dialog.file);

		// get url from form
		var url = $('#ImportMoqTableDialog-Form').attr('action') + '&taskElementID=' + $scope.model.TaskElementId;
		$scope.dialog.importWorking = true;

		$http.post(url, fd, {
			headers: {
				'Content-Type': undefined
			}
		}).then(function (response) {
			$scope.dialog.importWorking = false;
			$scope.dialog.showImportResults = true;

			// place returned html into the content div
			$('#ImportResults .content').html(response.data);

			// grab the two values returned as JS inside the new html
			$timeout(function () {
				$scope.dialog.invalidData = window.MoqTableImportVerificationWidget.invalidData;
				$scope.dialog.importResults = window.MoqTableImportVerificationWidget.data.importResults;
			}, 0);
		});
	};

	$scope.completeImportMoqTables = function () {
		$scope.dialog.completeImportWorking = true;

		GenSession.confirmDialog('Complete import?', 'This will replace all existing tables. If a table is removed or excluded from the Excel file, the table will be deleted.<br/>Do you want to continue?',
			function () {
				$scope.completeImportMoqTablesFromConfirmDialog();
			}, function () {
				$scope.$apply(function () {
					$scope.dialog.completeImportWorking = false;
				});
			}
		);
	};

	$scope.completeImportMoqTablesFromConfirmDialog = function () {
		// fix imported dates
		$scope.dialog.importResults.forEach(function (r) {
			r.DateOfReport = $scope.convertJsonDate(r.DateOfReport);
			r.PoPStart = $scope.convertJsonDate(r.PoPStart);
			r.PoPEnd = $scope.convertJsonDate(r.PoPEnd);
		});

		var data = {};
		data.importResults = $scope.dialog.importResults;
		data.taskElementID = $scope.model.TaskElementId;
		data.moqTypeId = $scope.model.ImportingMoqType.Id;

		$http({
			method: 'POST',
			url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.CompleteImportMoqTablesAction, ''),
			data: data
		}).then(function () {
			$scope.refreshPage();
		}).catch(function () {
			$scope.dialog.completeImportWorking = false;
			$scope.backFromImport();
			RaiseNotification('Import failed');
		});
	};

	// clicking back from import results
	$scope.backFromImport = function () {
		resetUploadForm();
		$scope.dialog.showImportResults = false;
	};

	$scope.exportMoqTablesFromImport = function () {
		$scope.exportMoqTables($scope.model.ImportingMoqType);
	};

	$scope.exportMoqTables = function (moqType) {
		$scope.isExporting = true;
		var urlPart = '?taskElementID=' + $scope.model.TaskElementId + '&moqTypeId=' + moqType.Id;
		var exportUrl = CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.ExportMoqTablesAction, urlPart);
		GenWidget.prototype.performExport(exportUrl);

		// export is done via attaching an iframe, so just wait to prevent double-clicking
		$timeout(function () {
			$scope.isExporting = false;
		}, 2000);
	};

	//#endregion

	//#region SAP Filters

	$scope.CloseMoqFilters = function () {
		$scope.filterDialog.open = false;
	};

	$scope.HideFilterError = function () {
		$scope.filterDialog.showError = false;
	}

	$scope.ShowFilterError = function (err) {
		$scope.filterDialog.showError = true;
		$scope.filterDialog.error = err;
	};

	$scope.SaveMoqFilters = function () {
		$scope.filterDialog.isLoading = true;

		var data = {};
		data.filters = angular.copy($scope.filterDialog.data);
		data.boeId = ManageTaskModel.boeId;

		// Convert view models into text
		$http({
			method: 'POST',
			url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.ConvertSapFilterAction, ''),
			data: data
		}).then(function (response) {
			// place returned html into the content div
			if (response.data.IsSuccessful === true) {
				if (response.data.Data) {
					$scope.filterDialog.originalData.AdditionalQueryFilters = response.data.Data;
				} else {
					$scope.filterDialog.originalData.AdditionalQueryFilters = [''];
				}
				$scope.actualsValidation.isDirty.set($scope.filterDialog.originalData.Id, true);
				$scope.refreshDisableSave();
				$scope.filterDialog.open = false;
			} else {
				$scope.ShowFilterError(response.data.Messages);
			}

			$scope.filterDialog.isLoading = false;
		}).catch(function () {
			$scope.filterDialog.isLoading = false;
			$scope.ShowFilterError(['Error Converting Filters to Text']);
		});
	};

	$scope.AddFilterRow = function () {
		var newRow = { ParensChecked: false, Value: [''] };
		$scope.filterDialog.data.push(newRow);
	};

	$scope.DeleteValue = function (valArray, index) {
		// remove one value from the value array
		valArray.splice(index, 1);
	};

	$scope.DeleteFilter = function (index) {
		var row = $scope.filterDialog.data[index];
		if (row.StartParens) {
			// need to delete matching EndParens
			var numStart = 0; // running number of inner Start parentheses found
			for (var i = index + 1; i < $scope.filterDialog.data.length; i++) {
				const nextRow = $scope.filterDialog.data[i];
				if (nextRow.EndParens) {
					if (numStart == 0) {
						// found it
						nextRow.EndParens = false;
						break;
					} else {
						numStart--;
					}
				} else if (nextRow.StartParens) {
					numStart++;
				}
			}

		} else if (row.EndParens) {
			// need to delete matching StartParens
			var numEnd = 0; // running number of inner End parentheses found
			for (var j = index - 1; j >= 0; j--) {
				const prevRow = $scope.filterDialog.data[j];
				if (prevRow.StartParens) {
					if (numEnd == 0) {
						// found it
						prevRow.StartParens = false;
						break;
					} else {
						numEnd--;
					}
				} else if (prevRow.EndParens) {
					numEnd++;
				}
			}
		}

		if ($scope.filterDialog.data && $scope.filterDialog.data.length > index) {
			// check to see if last row.  If so, remove join from previous row
			if ($scope.filterDialog.data.length > 1 && $scope.filterDialog.data.length == index + 1) {
				$scope.filterDialog.data[index - 1].Join = undefined;
			}

			$scope.filterDialog.data.splice(index, 1);
		}
	};

	$scope.AddFilterValue = function (valueArray) {
		valueArray.push('');
	};

	$scope.ResetOperators = function (filterRow) {
		if (filterRow.Field && filterRow.Field !== '') {
			filterRow.Type = $scope.filterDialog.fields[filterRow.Field].Type;
		} else {
			filterRow.Type = undefined;
		}
	};

	$scope.DeleteAllFilters = function () {
		$scope.filterDialog.data = [];
	};

	$scope.UpdateParens = function () {
		// validate only two parens checkboxes are selected
		var firstRowIndex = -1;
		var secondRowIndex = -1;
		var tooManyCheckboxes = false;
		for (var i = 0; i < $scope.filterDialog.data.length; i++) {
			var row = $scope.filterDialog.data[i];
			if (row.ParensChecked) {
				if (firstRowIndex === -1) {
					firstRowIndex = i;
				} else if (secondRowIndex === -1) {
					secondRowIndex = i;
				} else {
					tooManyCheckboxes = true;
				}
			}
		}

		if (tooManyCheckboxes || secondRowIndex === -1) {
			$scope.ShowFilterError(['Invalid row selection: Two checkboxes must be selected to modify Parens']);
			return;
		}

		// validate selected checkboxes are allowed (not already having start/end parens for 2 selected rows)
		const firstRow = $scope.filterDialog.data[firstRowIndex];
		const secondRow = $scope.filterDialog.data[secondRowIndex];

		if (firstRow.StartParens || firstRow.EndParens || secondRow.StartParens || secondRow.EndParens) {
			if (!(firstRow.StartParens && secondRow.EndParens)) {
				$scope.ShowFilterError(['Invalid row selection: Both rows must not have Parens or first row must have Start Parens and second row must have End Parens']);
				return;
			}
		}

		// validate equal number of start parens to end parens between the 2 rows
		let numStartParens = 0;
		let numEndParens = 0;
		for (let j = firstRowIndex + 1; j < secondRowIndex; j++) {
			const parensRow = $scope.filterDialog.data[j];
			if (parensRow.StartParens) {
				numStartParens++;
			} else if (parensRow.EndParens) {
				numEndParens++;
			}
		}

		if (numStartParens !== numEndParens) {
			$scope.ShowFilterError(['Invalid row selection: the number of left and right Parens inside the checked rows do not match.']);
			return;
		}

		if (firstRow.StartParens && secondRow.EndParens) {
			firstRow.StartParens = false;
			secondRow.EndParens = false;
		} else {
			// add parens to the 2 rows (start and end)
			firstRow.StartParens = true;
			secondRow.EndParens = true;
		}

		firstRow.ParensChecked = false;
		secondRow.ParensChecked = false;
	};

	$scope.ShowFilterDialog = function (tableData) {
		$scope.filterDialog.data = [];
		$scope.filterDialog.isLoading = true;
		$scope.filterDialog.originalData = tableData;
		$scope.filterDialog.error = '';
		$scope.filterDialog.showError = false;
		$scope.filterDialog.open = true;

		const data = {};
		if (Array.isArray(tableData.AdditionalQueryFilters)) {
			data.text = tableData.AdditionalQueryFilters.join("\n");
		} else {
			data.text = tableData.AdditionalQueryFilters;
		}

		data.boeId = ManageTaskModel.boeId;

		// Convert text into view models
		$http({
			method: 'POST',
			url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.ParseSapFilterAction, ''),
			data: data
		}).then(function (response) {
			if (response.data.IsSuccessful === true) {

				if (!response.data.Data) {
					response.data.Data = [];
				}

				if (response.data.Data.length === 0) {
					// need to show at least one row
					response.data.Data.push({});
				}

				response.data.Data.forEach(item => {
					item.ParensChecked = false;
					if (item.Field) {
						item.Type = $scope.filterDialog.fields[item.Field].Type;
					}

					if (!item.Value) {
						item.Value = [];
					}

					if (item.Value.length === 0) {
						item.Value.push('');
					}
				});

				$scope.filterDialog.data = response.data.Data;
			} else {
				$scope.ShowFilterError(response.data.Messages);
			}
			$scope.filterDialog.isLoading = false;
		}).catch(function () {
			$scope.filterDialog.isLoading = false;
			$scope.ShowFilterError(['Parsing Additional Filter Text failed']);
		});
	}

	// Move MOQ Filter up
	$scope.MoveFilterUp = function (index) {
		// first make a copy of the array
		const arr = $scope.filterDialog.data.slice();
		const orig = arr[index];
		const prev = arr[index - 1];
		// swap the joins and the parens
		const origJoin = orig.Join;
		const origStartParens = orig.StartParens;
		const origEndParens = orig.EndParens;

		orig.Join = prev.Join;
		orig.StartParens = prev.StartParens;
		orig.EndParens = prev.EndParens;
		prev.Join = origJoin;
		prev.StartParens = origStartParens;
		prev.EndParens = origEndParens;

		arr[index - 1] = arr[index];
		arr[index] = prev;

		$scope.filterDialog.data = arr;
	};

	// Move MOQ Filter down
	$scope.MoveFilterDown = function (index) {
		// first make a copy of the array
		const arr = $scope.filterDialog.data.slice();
		const orig = arr[index];
		const next = arr[index + 1];
		// swap the joins and the parens
		const origJoin = orig.Join;
		const origStartParens = orig.StartParens;
		const origEndParens = orig.EndParens;

		orig.Join = next.Join;
		orig.StartParens = next.StartParens;
		orig.EndParens = next.EndParens;
		next.Join = origJoin;
		next.StartParens = origStartParens;
		next.EndParens = origEndParens;

		arr[index + 1] = orig;
		arr[index] = next;

		$scope.filterDialog.data = arr;
	};

	$scope.setPoP = function (table, tableData) {
		// If Space then we need to look at query type. If Weekly then need to convert
		if (ManageTaskModel.IsSpace && tableData.QueryType === 'Weekly') {
			if (tableData.PoPStartWeek && tableData.PoPStartYear) {
				table.PopStartFW = tableData.PoPStartYear.toString() + tableData.PoPStartWeek.toString().padStart(2, '0');
			}

			if (tableData.PoPEndWeek && tableData.PoPEndYear) {
				table.PopEndFW = tableData.PoPEndYear.toString() + tableData.PoPEndWeek.toString().padStart(2, '0');
			}
		}
	}

	$scope.calculateActuals = function (tableData) {
		$scope.actualsValidation.errors = new Map();

		const data = {
			tableData: [],
			boeId: ManageTaskModel.boeId
		};

		const table = {
			WbsElement: tableData.WbsElement,
			PoPStart: tableData.PoPStart,
			PoPEnd: tableData.PoPEnd,
			Filters: tableData.AdditionalQueryFilters,
			TableId: tableData.Id
		};

		$scope.setPoP(table, tableData);

		if (Array.isArray(tableData.AdditionalQueryFilters)) {
			table.Filters = tableData.AdditionalQueryFilters.join("\n");
		}

		data.tableData.push(table);

		if (data.tableData.length > 0) {
			// send to backend
			// display response to user
			$(document).trigger("SHOW_LOADING_BOX");

			$http({
				method: 'POST',
				url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.CalculateAllActualsSapAction, ''),
				data: data
			}).then(function (response) {
				// place returned html into the content div
				if (response.data.IsSuccessful === true) {
					// the response is wrapped inside response.data.data array
					if (response.data.data && Array.isArray(response.data.data)) {

						// update the moq data table with calcualted values
						response.data.data.forEach(result => {
							const res = result.Data[0];
							if (result.Messages && result.Messages.length > 0) {
								$scope.setActualsErrors(res.TableId, result.Messages);
							} else {
								tableData.DateOfReport = new Date();
								tableData.TotalRelevantHours = res.TotalHours;
								if (res.WbsHours) {
									tableData.TotalWbsHours = res.WbsHours;
								} else {
									tableData.TotalWbsHours = 0;
								}

								// this is RMS only
								if (!ManageTaskModel.IsSpace) {
									if (!tableData.ContractNumber && res.ContractNumber) {
										// only set if currently unset and response is set
										tableData.ContractNumber = res.ContractNumber;
									}

									// Set Source to SAP if SAP connection is enabled
									if ($scope.IsSapEnabledAndSetAsRepository()) {
										tableData.RepositoryName = $scope.model.RmsSapEnabledSource;
									}
								}

								MOQEquationFieldWidget.setDirty();
								$scope.actualsValidation.isDirty.delete(res.TableId);
								$scope.refreshDisableSave();
							}
						});
					}
				} else {
					RaiseNotification('Error talking to backend to Calculate Actuals');
				}

				$(document).trigger("HIDE_LOADING_BOX");
			}).catch(function () {
				RaiseNotification('Error talking to backend to Calculate Actuals');
				$(document).trigger("HIDE_LOADING_BOX");
			});
		}
	};

	$scope.calculateAllMoqActuals = function () {
		$scope.actualsValidation.errors = new Map();
		$scope.actualsValidation.isDirty = new Map();
		// Get all the data tables
		const data = {
			tableData: []
		};

		const moqTypes = $scope.model.SelectedMoqTypes.filter(x => x.SelectedMOQType == $scope.model.ComparativeMoqType || x.SelectedMOQType == $scope.model.HistoricalMoqType);
		if (moqTypes) {
			moqTypes.forEach(moq => {
				if (moq.TableData) {
					moq.TableData.forEach(tableData => {
						if ($scope.IsSapEnabledAndSetAsRepository(tableData.RepositoryName)) {
							const table = {
								WbsElement: tableData.WbsElement,
								PoPStart: tableData.PoPStart,
								PoPEnd: tableData.PoPEnd,
								Filters: tableData.AdditionalQueryFilters,
								TableId: tableData.Id
							};

							$scope.setPoP(table, tableData);

							if (Array.isArray(tableData.AdditionalQueryFilters)) {
								table.Filters = tableData.AdditionalQueryFilters.join("\n");
							}

							data.tableData.push(table);
						}
					});
				}
			});
		}

		data.boeId = ManageTaskModel.boeId;

		if (data.tableData.length > 0) {
			// send to backend
			// display response to user
			$(document).trigger("SHOW_LOADING_BOX");

			$http({
				method: 'POST',
				url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.CalculateAllActualsSapAction, ''),
				data: data
			}).then(function (response) {
				// place returned html into the content div
				if (response.data.IsSuccessful === true) {
					// the response is wrapped inside response.data.data array
					if (response.data.data && Array.isArray(response.data.data)) {

						// find the moq table data and update the data with calcualted values
						response.data.data.forEach(result => {
							var res = result.Data[0];
							if (result.Messages && result.Messages.length > 0) {
								$scope.setActualsErrors(res.TableId, result.Messages);
							} else {
								moqTypes.forEach(moq => {
									var tableData = moq.TableData.find(t => t.Id == res.TableId);
									if (tableData) {
										tableData.DateOfReport = new Date();
										tableData.TotalRelevantHours = res.TotalHours;
										if (res.WbsHours) {
											tableData.TotalWbsHours = res.WbsHours;
										} else {
											tableData.TotalWbsHours = 0;
										}

										// this is RMS only
										if (!ManageTaskModel.IsSpace && !tableData.ContractNumber && res.ContractNumber) {
											// only set if currently unset and response is set
											tableData.ContractNumber = res.ContractNumber;
										}

										MOQEquationFieldWidget.setDirty();
									}
								});
							}
						});
					}

					$scope.refreshDisableSave();
				} else {
					RaiseNotification('Error talking to backend to Calculate All Actuals');
				}

				$(document).trigger("HIDE_LOADING_BOX");
			}).catch(function () {
				RaiseNotification('Error talking to backend to Calculate All Actuals');
				$(document).trigger("HIDE_LOADING_BOX");
			});
		}

		$scope.refreshDisableSave();
	};

	$scope.setActualsErrors = function (id, errors) {
		if (Array.isArray(errors)) {
			if (errors.length > 0) {
				// need to see if we need to convert to ValidationMessage
				if (errors[0].ValidationIssue === undefined) {
					var err = [];
					errors.forEach(e => {
						var valErr = {
							ValidationIssue: e
						};
						err.push(valErr);
					});

					// reset array to new array
					errors = err;
				}

				$scope.actualsValidation.errors.set(id, errors);
			}
		}
	};

	$scope.exportActuals = function (tableData) {
		$scope.actualsValidation.errors = new Map();
		// pull the data from the form

		const data = {};
		data.tableData = {
			WbsElement: tableData.WbsElement,
			PoPStart: tableData.PoPStart,
			PoPEnd: tableData.PoPEnd,
			Filters: tableData.AdditionalQueryFilters,
			TableId: tableData.Id
		};

		$scope.setPoP(data.tableData, tableData);

		if (Array.isArray(tableData.AdditionalQueryFilters)) {
			data.tableData.Filters = tableData.AdditionalQueryFilters.join("\n");
		}

		data.boeId = ManageTaskModel.boeId;

		// send to backend
		// display response to user
		$(document).trigger("SHOW_LOADING_BOX");

		$http({
			method: 'POST',
			url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.ExportActualsSapAction, ''),
			data: data
		}).then(function (response) {
			// place returned html into the content div
			if (response.data.IsSuccessful !== true) {
				if (response.data.Messages && response.data.Messages.length > 0) {
					$scope.setActualsErrors(tableData.Id, response.data.Messages);
				} else {
					$scope.setActualsErrors(tableData.Id, [{ ValidationIssue: 'Error talking to backend to Export Actuals' }]);
				}
			}
			else {
				const blob = $scope.b64toBlob(response.data.Data, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
				const filename = 'ActualsExport.xlsx';

				if (navigator.msSaveBlob)
					navigator.msSaveBlob(blob, filename);
				else {
					// trick to download store a file having its URL
					const fileURL = URL.createObjectURL(blob);
					const a = document.createElement('a');
					a.href = fileURL;
					a.target = '_blank';
					a.download = filename;
					document.body.appendChild(a); //create the link "a"
					a.click(); //click the link "a"
					document.body.removeChild(a); //remove the link "a"
				}
			}

			$(document).trigger("HIDE_LOADING_BOX");
		}).catch(function () {
			$scope.setActualsErrors(tableData.Id, [{ ValidationIssue: 'Error talking to backend to Export Actuals' }]);
			$(document).trigger("HIDE_LOADING_BOX");
		});
	};

	$scope.b64toBlob = function (b64Data, contentType = '', sliceSize = 512) {
		// this converts base 64 encoded string into a Blob by slicing the bytes off
		// and then converting them into char codes
		// pulled from https://stackoverflow.com/a/16245768 
		const byteCharacters = atob(b64Data);
		const byteArrays = [];

		for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
			const slice = byteCharacters.slice(offset, offset + sliceSize);

			const byteNumbers = new Array(slice.length);
			for (let i = 0; i < slice.length; i++) {
				byteNumbers[i] = slice.charCodeAt(i);
			}

			const byteArray = new Uint8Array(byteNumbers);
			byteArrays.push(byteArray);
		}

		const blob = new Blob(byteArrays, { type: contentType });
		return blob;
	};

	//#endregion SAP Filters

	$scope.refreshPage = function () {
		$window.location.reload();
	};

	$scope.convertJsonDate = function (date) {
		return new Date(JSON.parse(date.match(/\d+/)));
	};

	$scope.ValidatePopStart = function (date) {
		// validate PoP Start is on a Monday (1)
		if (!$scope.model.SAPEnabled) {
			return false; // true means invalid
		} else {
			return typeof date !== "undefined" && date.getDay() != 1;
		}
	}

	$scope.ValidatePopEnd = function (date) {
		// validate PoP End is on a Sunday (0)
		// validate PoP Start is on a Monday (1)
		if (!$scope.model.SAPEnabled) {
			return false; // true means invalid
		} else {
			return typeof date !== "undefined" && date.getDay() != 0;
		}
	}

	$scope.RefreshPoPMonths = function (popStart, popEnd) {
		// Get the difference between the two dates in months (30 days), rounded to 2 decimals
		return +((popEnd - popStart) / (1000 * 60 * 60 * 24) / 30).toFixed(2);
	}

	$scope.IsSapEnabledAndSetAsRepository = function (repositoryName) {
		if ($scope.model.IsRMS) {
			// RMS does not use Repository Name, so just return SAP Enabled
			return $scope.model.SAPEnabled && $scope.model.SapConnectionEnabled;
		} else {
			// SSC requires Repository Name to be set to SAP / WEBI
			return $scope.model.SAPEnabled && $scope.model.SapConnectionEnabled && repositoryName == $scope.model.SapWebiRepository;
		}
	}

	$scope.IsSAPSetAsRepository = function (repositoryName) {
		if ($scope.model.IsRMS) {
			// RMS does not use Repository Name, so just return true
			return true;
		} else {
			// Return true if Repository Name set to SAP / WEBI for SSC
			return repositoryName == $scope.model.SapWebiRepository;
		}
	}

	$scope.IsSapSetAsAnyRepository = function (moqTypes) {
		if ($scope.model.IsRMS) {
			// RMS does not use Repository Name, so just true
			return true;
		} else {
			// Return true if any Repository Name set to SAP / WEBI for SSC
			if (moqTypes.some(moq => moq.TableData)) {
				return moqTypes.map(moq => moq.TableData)[0].some(table => table.RepositoryName == $scope.model.SapWebiRepository);
			} else {
				return false;
			}
		}
	}

	$scope.SetTableDirty = function (tableData) {
		if ($scope.IsSapEnabledAndSetAsRepository(tableData.RepositoryName)) {
			$scope.actualsValidation.isDirty.set(tableData.Id, true);
			$scope.refreshDisableSave();
		}
		MOQEquationFieldWidget.setDirty();
	};

	$scope.UpdateRepository = function (tableData) {
		if (tableData.RepositoryNameSelection == $scope.model.SapWebiRepository) {
			$scope.actualsValidation.isDirty.set(tableData.Id, true);
			tableData.RepositoryName = $scope.model.SapWebiRepository;
		} else {
			$scope.actualsValidation.isDirty.delete(tableData.Id);
			tableData.RepositoryName = "";
			$scope.actualsValidation.errors.set(tableData.Id, []); // clear SAP validation messages
		}

		$scope.refreshDisableSave();
	}
}]);

// initialize MOQ Equation Widget.. moved here so that way this much script is not in the ascx page
InitializeMOQEquationFieldWidget = function (MOQEquationFieldWidget_ReadOnly, workspaceVariables, ordinaryVariables, newOrdinaryVariableID, sumOfBOEs, discrete, validationUrl, shouldMoqReadOnlyBeReversed,
	calculateMOQResultUrl, openSumOfBoesByWbsUrl, openSumOfBoesByClinUrl, isNotSubContractor, sortBOEByWBS, sortBOEByClin) {
	// create base js object;
	MOQEquationFieldWidget = new Widget("MOQEquationField", MOQEquationFieldWidget_ReadOnly);

	MOQEquationFieldWidget.initialLoad = false;
	MOQEquationFieldWidget.MarkedEquation = "";
	MOQEquationFieldWidget.WorkspaceVariables = workspaceVariables;
	MOQEquationFieldWidget.OrdinaryVariables = ordinaryVariables;
	MOQEquationFieldWidget.MOQCalculating = false;
	MOQEquationFieldWidget.MOQValidating = false;
	MOQEquationFieldWidget.MOQValid = false;
	MOQEquationFieldWidget.SavePending = false;
	MOQEquationFieldWidget.NewOrdinaryVariableID = newOrdinaryVariableID;

	MOQEquationFieldWidget.HarvestWorkspaceVariablesForSave = function () {
		var toReturn = [];
		var workspaceVariableFields = MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

		for (var ndx = 0; ndx < MOQEquationFieldWidget.WorkspaceVariables.length; ndx++) {
			var variableID = MOQEquationFieldWidget.WorkspaceVariables[ndx].WorkspaceVariableID;
			var variableField = workspaceVariableFields.filter('[pkid=' + variableID + ']');

			if (variableField.length) {
				toReturn.push(variableID);
			}
		}

		return toReturn;
	};
	MOQEquationFieldWidget.HarvestOrdinaryVariablesForSave = function () {
		var toReturn = [];
		var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

		for (var ndx = 0; ndx < MOQEquationFieldWidget.OrdinaryVariables.length; ndx++) {
			var currentTaskOrdinaryVariable = MOQEquationFieldWidget.OrdinaryVariables[ndx];
			var variableID = currentTaskOrdinaryVariable.OrdinaryVariableID;
			var variableField = currentVariableInputs.filter('[pkid=' + variableID + ']');
			var variableValue;

			if (variableField.length && (variableValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField)).length > 0) {
				currentTaskOrdinaryVariable.Deleted = false;
				currentTaskOrdinaryVariable.OrdinaryVariableValue = variableValue;
				currentTaskOrdinaryVariable.IsPercentage = MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(variableField);

				if (variableField.attr('BOEToSum') != undefined) {
					currentTaskOrdinaryVariable.BOEToSum = JSON.parse(variableField.attr('BOEToSum'));
				}

				if (variableField.attr('WBSToSum') != undefined) {
					currentTaskOrdinaryVariable.WBSToSum = JSON.parse(variableField.attr('WBSToSum'));
				}

				if (variableField.attr('CLINToSum') != undefined) {
					currentTaskOrdinaryVariable.CLINToSum = JSON.parse(variableField.attr('CLINToSum'));
				}

				if (variableField.attr('ResourceTypes') != undefined) {
					currentTaskOrdinaryVariable.ResourceTypes = JSON.parse(variableField.attr('ResourceTypes'));
				}

				if (variableField.attr('SortBOEBy') != undefined) {
					currentTaskOrdinaryVariable.SortBOEBy = variableField.attr('SortBOEBy');
				}

				if (currentTaskOrdinaryVariable.BOEToSum.length || currentTaskOrdinaryVariable.WBSToSum.length || currentTaskOrdinaryVariable.CLINToSum.length) {
					currentTaskOrdinaryVariable.OrdinaryVariableValueType = sumOfBOEs;
				}
				else {
					currentTaskOrdinaryVariable.OrdinaryVariableValueType = discrete;
				}
			}
			else {
				currentTaskOrdinaryVariable.Deleted = true;
				currentTaskOrdinaryVariable.OrdinaryVariableValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField);
				currentTaskOrdinaryVariable.OrdinaryVariableValueType = currentTaskOrdinaryVariable.OrdinaryVariableValueType.toString();
			}

			delete currentTaskOrdinaryVariable.UpdateDate;
			currentTaskOrdinaryVariable.UpdateDateLong = currentTaskOrdinaryVariable.UpdateDateLong.toString();
			toReturn.push(currentTaskOrdinaryVariable);
		}

		if (currentVariableInputs.length > 0) {
			for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++) {
				var variableAlreadyInReturnSet = false;
				var currentVariableInput = $(currentVariableInputs[inputNdx]);
				var currentVariableInputID = currentVariableInput.attr('pkid');
				var currentVariableInputValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInput);

				if (currentVariableInputValue.length > 0) {
					for (var returnNdx = 0; returnNdx < toReturn.length; returnNdx++) {
						if (currentVariableInputID == toReturn[returnNdx].OrdinaryVariableID) {
							variableAlreadyInReturnSet = true;
							break;
						}
					}

					if (!variableAlreadyInReturnSet) {
						var newOrdinaryVariable = {};
						newOrdinaryVariable.OrdinaryVariableID = currentVariableInputID;
						newOrdinaryVariable.OrdinaryVariableName = currentVariableInput.attr('name');;
						newOrdinaryVariable.OrdinaryVariableValue = currentVariableInputValue;
						newOrdinaryVariable.IsPercentage = MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(currentVariableInput);
						newOrdinaryVariable.DefaultSize = currentVariableInput.closest('tr').data('variable-default-size');

						newOrdinaryVariable.BOEToSum = JSON.parse(currentVariableInput.attr('BOEToSum'));
						newOrdinaryVariable.WBSToSum = JSON.parse(currentVariableInput.attr('WBSToSum'));
						newOrdinaryVariable.CLINToSum = JSON.parse(currentVariableInput.attr('CLINToSum'));
						newOrdinaryVariable.ResourceTypes = JSON.parse(currentVariableInput.attr('ResourceTypes'));
						newOrdinaryVariable.SortBOEBy = currentVariableInput.attr('SortBOEBy');

						if (newOrdinaryVariable.BOEToSum.length || newOrdinaryVariable.WBSToSum.length || newOrdinaryVariable.CLINToSum.length) {
							newOrdinaryVariable.OrdinaryVariableValueType = sumOfBOEs;
						}
						else {
							newOrdinaryVariable.OrdinaryVariableValueType = discrete;
						}

						toReturn.push(newOrdinaryVariable);
					}
				}
			}
		}

		return toReturn;
	};
	MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen = function () {
		var scope = angular.element(document.getElementById('MOQEquationField')).scope();
		return scope.model.insertWorkspaceModalOpen;
	};
	MOQEquationFieldWidget.InsertMOQElementDialogClosing = function () {
		if (!MOQEquationFieldWidget.MOQValid && !MOQEquationFieldWidget.MOQValidating) {
			MOQEquationFieldWidget.ShowMOQValidationError();
		}
	};
	MOQEquationFieldWidget.GetAllVariableFields = function () {
		return $('#MOQEquationField #MOQEquation-Variables [wsVar], #MOQEquationField #MOQEquation-Variables [ordVar]');
	};
	MOQEquationFieldWidget.RefreshStyles = function () {
		MOQEquationFieldWidget.applyReadOnly();

		if (MOQEquationFieldWidget.isReadOnly()) {
			$("#MOQEquationField #InsertWorkspaceVariableLink").parent().prev().remove();
			$("#MOQEquationField #InsertWorkspaceVariableLink").parent().remove();
			$("#MOQEquationField #SearchEstimatingCatalogLink").parent().prev().remove();
			$("#MOQEquationField #SearchEstimatingCatalogLink").parent().remove();

			/*
				MOQ-Type (drop-down) and MOQ-Text EFFECTIVELY inherit THEIR editability from the Task Details widget.
 
				This partial view contains the MOQ Equation, MOQ Type and MOQ Text fields.  MOQ Equation is NOT editable in
				a Locked state, but the other two are.  The Widget logic automatically locks access to all three, so this
				logic is added to override that for the Type and Text fields.
 
				The REAL issue here (and in a broader sense, throughout the UI) is that the Widgets apply their
				read-only flag to EVERY child element as a whole -- But that CANNOT work for cases (like this one) where
				different elements can have different accessibilities.  The ideal approach is to have a SET of access flags
				for different (individual) fields and/or groups of fields.  This is what the security matrix is SUPPOSED to
				be used for, but our code does not always consult it (or even consult it correctly or completely in every case).
				In addition, we often have access-determination logic in the views themselves that should really be executed
				in the view models (or at least in the controllers) and then forwarded to the views as booleans.
			*/
			if (!TaskElementDetailsWidget.isReadOnly()) {
				var moqSection = $('#MOQEquationField');

				var moqTypeElement = $("#MOQType", moqSection);
				$('.replacedWidgetText', moqTypeElement.parent()).addClass('display-none');
				moqTypeElement.removeClass('display-none');

				RemoveRTETemplateReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText', moqSection);
			}
		}

		refreshModule($('.task-element-details.module'));
	};
	MOQEquationFieldWidget.ValidateMOQEquation = function () {
		TaskElementDetailsWidget.waitingBeforeSubmit = true;

		GenSession.ShowLoadingBox();
		MOQEquationFieldWidget.MOQValidating = true;

		MOQEquationFieldWidget.HideError();

		var moqEquation = $.trim($("#MOQEquationField #MOQEquation").val());

		var dataToSend = { "moqEquation": moqEquation }
		dataToSend = JSON.stringify(dataToSend);

		$.ajax({
			type: 'POST',
			url: validationUrl,
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			data: dataToSend,
			success: function (response, textStatus) {
				MOQEquationFieldWidget.MOQValidated(response);

				if (MOQEquationFieldWidget.isReadOnly() && shouldMoqReadOnlyBeReversed) {
					$('.magnifier-button').addClass('display-none');
					$('#MoqType').children('.replacedWidgetText').remove();
					$('.moqTypes').removeClass('display-none');

					var moqSection = $('#MOQEquationField');
					RemoveRTETemplateReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText', moqSection);
				}
			},
			error: function (response, textStatus) {
				MOQEquationFieldWidget.MOQValidationError(response);
			}
		});
	};
	MOQEquationFieldWidget.MOQValidationError = function (exceptionstring) {
		MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", "A general validation error occurred. Please check the MOQ equation for validity.");
		MOQEquationFieldWidget.ShowMOQValidationError();
		MOQEquationFieldWidget.MOQValidating = false;
		TaskElementDetailsWidget.waitingBeforeSubmit = false;
		GenSession.HideLoadingBox();
	};
	MOQEquationFieldWidget.SetMOQError = function (errorSubject, errorMessage) {
		if (errorSubject != undefined && errorMessage != undefined) {
			$("#MOQEquationField #MOQEquation-ErrorTitle").html(errorSubject);
			$("#MOQEquationField #MOQEquation-ErrorText").html(errorMessage);
		}
	};
	MOQEquationFieldWidget.ShowMOQValidationError = function () {
		$("#MOQEquationField #MOQEquation-Variables").hide();
		MOQEquationFieldWidget.ShowMOQError();
	};
	MOQEquationFieldWidget.ShowMOQError = function () {
		$("#MOQEquationField #MOQEquation-Error").show();
		MOQEquationFieldWidget.SetMOQResult("");
		MOQEquationFieldWidget.RefreshStyles();
	};
	MOQEquationFieldWidget.HideError = function () {
		$("#MOQEquationField #MOQEquation-Error").hide();
		MOQEquationFieldWidget.RefreshStyles();
	};
	MOQEquationFieldWidget.FocusError = function () {
		$("#MOQEquationField #MOQEquation-ErrorTitle").focus();
	}
	MOQEquationFieldWidget.MOQEquationPreparedForSubmit = function () {

		if (MOQEquationFieldWidget.MOQValidating || MOQEquationFieldWidget.MOQCalculating) {
			MOQEquationFieldWidget.SavePending = true;
		}

		return (!$("#MOQEquationField #MOQEquation-Error").is(":visible") &&
			!MOQEquationFieldWidget.MOQValidating &&
			!MOQEquationFieldWidget.MOQCalculating &&
			MOQEquationFieldWidget.ReadyForCalculate());
	};
	MOQEquationFieldWidget.MOQValidated = function (results) {
		TaskElementDetailsWidget.waitingBeforeSubmit = true;
		GenSession.ShowLoadingBox();
		// Set Widget valid flag
		MOQEquationFieldWidget.MOQValid = results.Status;

		if (results.Status) {
			var variables = results.Variables;

			MOQEquationFieldWidget.RemoveInvalidVariables(variables);

			for (var ndx = 0; ndx < variables.length; ndx++) {
				if (ndx == 0) {
					MOQEquationFieldWidget.MarkedEquation = variables[ndx];
				}
				else {
					var defaultSize = MOQEquationFieldWidget.GetVariableDefaultSize(variables[ndx]);
					MOQEquationFieldWidget.AddVariableField(variables[ndx], defaultSize);
				}
			}
			// Reinitialize imported historical metric data.
			$('#MOQDefaultSizes').val("");
			$('#HistoricalMetricEquation').val("");

			var currentVariables = MOQEquationFieldWidget.GetAllVariableFields();

			if (currentVariables.length > 0) {
				$("#MOQEquationField #MOQEquation-Variables").show();
				$("#MOQEquationField .moqVariableNote").removeClass('display-none');
			}
			else {
				$("#MOQEquationField #MOQEquation-Variables").hide();
			}

			MOQEquationFieldWidget.RefreshStyles();

			MOQEquationFieldWidget.CalculateMOQResult();
		}
		else {
			// Set the validation error text
			MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", results.Message);

			// If the user isn't inserting a variable, we'll show the error
			if (!MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen() &&
				(!TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogMST').isOpen() ||
					!TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogCommon').isOpen())) {
				MOQEquationFieldWidget.ShowMOQValidationError();
			}

			MOQEquationFieldWidget.SavePending = false;
		}

		MOQEquationFieldWidget.MOQValidating = false;
		GenSession.HideLoadingBox();
		TaskElementDetailsWidget.waitingBeforeSubmit = false;
	};
	/**
	* Gets a default size value for a variable that was created as a result of importing a historical
	* metric.  Returns "" for all other variable types.
	*/
	MOQEquationFieldWidget.GetVariableDefaultSize = function (currentVariable) {
		var metricEquation = $('#HistoricalMetricEquation').val().toUpperCase();
		var defaultSize = "";
		if (metricEquation.length) {
			var newMetricDefaultSizes = $('#MOQDefaultSizes').val();
			if (newMetricDefaultSizes != "" && metricEquation.indexOf(currentVariable.toUpperCase()) != -1) {
				// Parse out the 1st default sizes for assignment to the current variable.
				var moqVariableDefaultSizes = [];
				moqVariableDefaultSizes = JSON.parse(newMetricDefaultSizes);
				if (moqVariableDefaultSizes.length) {
					defaultSize = moqVariableDefaultSizes[0];
					moqVariableDefaultSizes.splice(0, 1);
					$('#MOQDefaultSizes').val(JSON.stringify(moqVariableDefaultSizes));
				}
			}
		}
		return defaultSize;
	};
	MOQEquationFieldWidget.RemoveInvalidVariables = function (currentVariables) {
		var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields();

		if (currentVariableInputs.length > 0) {
			for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++) {
				var currentVariableInput = $(currentVariableInputs[inputNdx]);
				var variableStillValid = false;

				for (var variableNdx = 0; variableNdx < currentVariables.length; variableNdx++) {
					if (currentVariableInput.attr('name') == currentVariables[variableNdx]) {
						variableStillValid = true;
						break;
					}
				}

				if (!variableStillValid) {
					currentVariableInput.parents('tr').remove();
				}
			}
		}
	};
	MOQEquationFieldWidget.AddVariableField = function (variableName, defaultSize) {
		var workspaceVariable = undefined;
		var ordinaryVariable = undefined;
		var existingVariableField = MOQEquationFieldWidget.GetAllVariableFields().filter('[name="' + variableName + '"]');
		if (!existingVariableField.length) {
			var moqEquationVariablesElement = $('#MOQEquationField #MOQEquation-Variables');

			// Workspace Variable Field
			if ((workspaceVariable = MOQEquationFieldWidget.GetWorkspaceVariable(variableName)) != undefined) {
				// Summed BOE Workspace Variable
				if (workspaceVariable.WorkspaceVariableValueType == 2) {
					moqEquationVariablesElement.append('<tr var="' + workspaceVariable.WorkspaceVariableID + '" data-variable-default-size="" ><td class="variableLabel">' + workspaceVariable.WorkspaceVariableName + ':</td><td><a wsVar="true" readOnlyVar="true" pkid="' + workspaceVariable.WorkspaceVariableID + '" name="' + workspaceVariable.WorkspaceVariableName.toUpperCase() + '" SortBOEBy="' + workspaceVariable.SortBOEBy + '">' + Helper.addCommas(workspaceVariable.WorkspaceVariableValue) + '</a></td></tr>');
				}
				// Discrete Workspace Variable
				else {
					moqEquationVariablesElement.append('<tr var="' + workspaceVariable.WorkspaceVariableID + '" data-variable-default-size="" ><td class="variableLabel">' + workspaceVariable.WorkspaceVariableName + ':</td><td><div wsVar="true" pkid="' + workspaceVariable.WorkspaceVariableID + '" name="' + workspaceVariable.WorkspaceVariableName.toUpperCase() + '" SortBOEBy="' + workspaceVariable.SortBOEBy + '">' + Helper.addCommas(workspaceVariable.WorkspaceVariableValue) + '</div></td></tr>');
				}
			}
			// Existing (Saved) Task Ordinary Variable Field
			else if ((ordinaryVariable = MOQEquationFieldWidget.GetOrdinaryVariable(variableName)) != undefined) {
				var defaultSizeLabel = '';
				if (ordinaryVariable.DefaultSize != undefined && ordinaryVariable.DefaultSize != '') {
					defaultSizeLabel = '(' + ordinaryVariable.DefaultSize + ')';
				}
				if (ordinaryVariable.OrdinaryVariableID < 0) {
					// Actually a new variable that came from a refreshed patial view.
					MOQEquationFieldWidget.NewOrdinaryVariableID--;
				}
				if (MOQEquationFieldWidget.isReadOnly()) {
					// READ ONLY Summed BOE Existing Task Ordinary Variable code
					if (ordinaryVariable.OrdinaryVariableValueType == sumOfBOEs) {
						moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.Size + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName
							+ ':</td><td><a ordVar="true" readOnlyVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '">'
							+ Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '</a></td></tr>');
					}
					// READ ONLY Discrete Existing Task Ordinary Variable code
					else {
						moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.Size + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName
							+ ':</td><td><input ordVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue)
							+ '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/></td></tr>');
					}
				}
				else {
					var notSubText = '';
					if (isNotSubContractor) {
						notSubText = '<a class="sumVariable" var="' + ordinaryVariable.OrdinaryVariableID + '">Select BOEs to sum</a>';
					}

					// Summed BOE Existing Task Ordinary Variable code
					if (ordinaryVariable.OrdinaryVariableValueType == sumOfBOEs) {
						moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName + ':</td><td><input disabled="disabled" ordVar="true" pkid="'
							+ ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue)
							+ '" BOEToSum="' + JSON.stringify(ordinaryVariable.BOEToSum) + '" WBSToSum="' + JSON.stringify(ordinaryVariable.WBSToSum) + '" CLINToSum="' + JSON.stringify(ordinaryVariable.CLINToSum) + '" ResourceTypes="'
							+ JSON.stringify(ordinaryVariable.ResourceTypes) + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/>'
							+ notSubText + '<span class="clearVariable" var="' + ordinaryVariable.OrdinaryVariableID + '"> | <a>Clear</a></span></td></tr>');
					}
					// Discrete Existing Task Ordinary Variable code
					else {
						moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.DefaultSize + '"><td class="variableLabel">' + defaultSizeLabel
							+ ordinaryVariable.OrdinaryVariableName + ':</td><td><input ordVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName
							+ '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '" BOEToSum="' + JSON.stringify(ordinaryVariable.BOEToSum) + '" WBSToSum="'
							+ JSON.stringify(ordinaryVariable.WBSToSum) + '" CLINToSum="' + JSON.stringify(ordinaryVariable.CLINToSum) + '" ResourceTypes="' + JSON.stringify(ordinaryVariable.ResourceTypes)
							+ '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/>' + notSubText + '</td></tr>');
					}
				}
			}
			// New Task Ordinary Variable Field
			else {
				var defaultSizeLabel = '';
				if (defaultSize != undefined && defaultSize != '') {
					defaultSizeLabel = '(' + defaultSize + ')';
				}
				MOQEquationFieldWidget.NewOrdinaryVariableID--;
				var notSubText = '';
				if (isNotSubContractor) {
					notSubText = '<a class="sumVariable" var="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '">Select BOEs to sum</a>';
				}

				moqEquationVariablesElement.append('<tr var="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '" data-variable-default-size="' + defaultSize + '"><td class="variableLabel">' + defaultSizeLabel
					+ variableName + ':</td><td><input ordVar="true" pkid="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '" name="' + variableName + '" class="variableInput" value="' + defaultSize
					+ '" BOEToSum="[]" WBSToSum="[]" CLINToSum="[]" ResourceTypes="[]" SortBOEBy="' + sortBOEByWBS + '"/> '
					+ notSubText + '</td></tr>');
			}
		}
	};
	MOQEquationFieldWidget.GetWorkspaceVariable = function (variableName) {
		for (var variableNdx = 0; variableNdx < MOQEquationFieldWidget.WorkspaceVariables.length; variableNdx++) {
			if ($.trim(variableName.toUpperCase()) == $.trim(MOQEquationFieldWidget.WorkspaceVariables[variableNdx].WorkspaceVariableName.toUpperCase())) {
				return MOQEquationFieldWidget.WorkspaceVariables[variableNdx];
			}
		}

		return undefined;
	};
	MOQEquationFieldWidget.GetOrdinaryVariable = function (variableName) {
		for (var variableNdx = 0; variableNdx < MOQEquationFieldWidget.OrdinaryVariables.length; variableNdx++) {
			if ($.trim(variableName.toUpperCase()) == $.trim(MOQEquationFieldWidget.OrdinaryVariables[variableNdx].OrdinaryVariableName.toUpperCase())) {
				return MOQEquationFieldWidget.OrdinaryVariables[variableNdx];
			}
		}

		return undefined;
	};
	MOQEquationFieldWidget.CalculateMOQResult = function () {
		if (MOQEquationFieldWidget.ReadyForCalculate()) {
			GenSession.ShowLoadingBox();
			var moqEquation = MOQEquationFieldWidget.GetInputEquation();

			MOQEquationFieldWidget.MOQCalculating = true;

			var dataToSend = { "moqEquation": moqEquation }
			dataToSend = JSON.stringify(dataToSend);

			$.ajax({
				type: 'POST',
				url: calculateMOQResultUrl,
				contentType: 'application/json; charset=utf-8',
				dataType: 'json',
				data: dataToSend,
				success: MOQEquationFieldWidget.MOQCalculationSuccess,
				error: MOQEquationFieldWidget.MOQCalculationError
			});
		}
		else {
			MOQEquationFieldWidget.SetMOQResult("");

			MOQEquationFieldWidget.SavePending = false;
		}
	};
	MOQEquationFieldWidget.ReadyForCalculate = function () {
		MOQEquationFieldWidget.HideError();

		var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

		if (currentVariableInputs.length > 0) {
			for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++) {
				var currentValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInputs[inputNdx]);

				if (currentValue.length > 0) {
					if (!currentValue.match(/^[\+\-]*[\d,]*\.?[\d,]+%?$/)) {
						MOQEquationFieldWidget.SetMOQError("Invalid MOQ Variables", "All MOQ equation variables must be valid numerical values. Please check each variable field below.");
						MOQEquationFieldWidget.ShowMOQError();
						return false;
					}
					else {
						$(currentVariableInputs[inputNdx]).val(Helper.addCommas(Helper.removeCommas(currentValue)));
					}
				}
				else {
					MOQEquationFieldWidget.SetMOQError("Incomplete MOQ Variables", "All MOQ equation variable fields must contain values. Please fill in all of the variable fields below.");
					MOQEquationFieldWidget.ShowMOQError();
					return false;
				}
			}
		}

		return true;
	};
	MOQEquationFieldWidget.GetInputEquation = function () {
		var markedEquation = $.trim(MOQEquationFieldWidget.MarkedEquation);
		var ordinaryVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

		if (ordinaryVariableInputs.length > 0) {
			for (var inputNdx = 0; inputNdx < ordinaryVariableInputs.length; inputNdx++) {
				var currentVariableInput = $(ordinaryVariableInputs[inputNdx]);
				var currentVariableInputName = currentVariableInput.attr('name');
				var currentVariableInputValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(ordinaryVariableInputs[inputNdx]);
				var result = currentVariableInputName.replace(/\(\d+.*\) /g, "");
				var currentVariableRegEx = new RegExp("<" + result + ">", "gi");
				markedEquation = markedEquation.replace(currentVariableRegEx, currentVariableInputValue);
			}
		}

		workspaceVariableDivs = MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

		if (workspaceVariableDivs.length > 0) {
			for (var inputNdx = 0; inputNdx < workspaceVariableDivs.length; inputNdx++) {
				var currentVariableInput = $(workspaceVariableDivs[inputNdx]);
				var currentVariableInputName = currentVariableInput.attr('name');
				var currentVariableInputValue = currentVariableInput.text();

				var currentVariableRegEx = new RegExp("<" + currentVariableInputName + ">", 'gi');
				markedEquation = markedEquation.replace(currentVariableRegEx, currentVariableInputValue);
			}
		}

		return markedEquation;
	};
	MOQEquationFieldWidget.GetOrdinaryVariableFieldValue = function (ordinaryVariableField) {
		ordinaryVariableField = $(ordinaryVariableField);

		var fieldValue = '';

		if (ordinaryVariableField.is('[readOnlyVar]')) {
			fieldValue = ordinaryVariableField.text();
		}
		else {
			fieldValue = ordinaryVariableField.val();
		}

		return fieldValue;
	};
	MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage = function (ordinaryVariableField) {
		ordinaryVariableField = $(ordinaryVariableField);

		var fieldValue = '';

		if (ordinaryVariableField.is('[readOnlyVar]')) {
			fieldValue = ordinaryVariableField.text();
		}
		else {
			fieldValue = ordinaryVariableField.val();
		}

		return fieldValue.match(/%/) != null;
	};
	MOQEquationFieldWidget.MOQCalculationSuccess = function (results) {
		GenSession.ShowLoadingBox();

		if (results.Status) {
			MOQEquationFieldWidget.SetMOQResult(results.Result);

			if (MOQEquationFieldWidget.SavePending) {
				MOQEquationFieldWidget.SavePending = false;
				MOQEquationFieldWidget.MOQCalculating = false;
				MOQEquationFieldWidget.CallSaveManually();
			}

			// MOQ equation has been changed, the change has been validated, and the new hours total (<result>) has been determined - OK to apply recalculations
			var scope = angular.element(document.querySelector("#TaskElementsComposite")).scope();
			scope.moqUpdated(MOQEquationFieldWidget.initialLoad);
			scope.$apply();
		}
		else {
			MOQEquationFieldWidget.MOQCalculationError(results.Message);

			MOQEquationFieldWidget.SavePending = false;
		}

		// if the moqEquation is empty, resource types/spreads will be calculated as if the equation  = 0, but we don't want to show the result as 0 to the user 
		var moqEquation = MOQEquationFieldWidget.GetInputEquation();
		if (moqEquation.length == 0) {
			MOQEquationFieldWidget.SetMOQResult("");
		}
		MOQEquationFieldWidget.MOQCalculating = false;
		MOQEquationFieldWidget.initialLoad = false;

		GenSession.HideLoadingBox();
	};
	MOQEquationFieldWidget.MOQCalculationError = function (exceptionstring) {
		MOQEquationFieldWidget.SetMOQError("MOQ Calculation Error", exceptionstring);
		MOQEquationFieldWidget.ShowMOQError();

		MOQEquationFieldWidget.SavePending = false;
		MOQEquationFieldWidget.MOQCalculating = false;
		MOQEquationFieldWidget.initialLoad = false;

		GenSession.HideLoadingBox();
	};
	MOQEquationFieldWidget.SetMOQResult = function (result) {
		$("#MOQEquationField #equals").text(Helper.addCommas(result));
		$(document).trigger('MOQEquationReady');
	};
	MOQEquationFieldWidget.CallSaveManually = function () {
		$(document).trigger('SaveBOEUpdatesAndClose');
	};
	MOQEquationFieldWidget.FilterFieldToIntegers = function (field) {
		field.value = field.value.replace(/[^+\-\,\.0-9]/g, '');
	};
	MOQEquationFieldWidget.OpenSumOfBOEsByWBS = function (event, eventData) {
		ShowLoadingBox();
		$.ajax({
			type: 'POST',
			url: openSumOfBoesByWbsUrl,
			success: function (response) {
				HideLoadingBox();
				$('#VariableSumOfBOEsByWBSDialogContainer').html(response);
				$(document).trigger(event, eventData);

			},
			error: function () {
				HideLoadingBox();
			}
		});
	};
	MOQEquationFieldWidget.OpenSumOfBOEsByCLIN = function (event, eventData) {
		ShowLoadingBox();
		$.ajax({
			type: 'POST',
			url: openSumOfBoesByClinUrl,
			success: function (response) {
				HideLoadingBox();
				$('#VariableSumOfBOEsByCLINDialogContainer').html(response);
				$(document).trigger(event, eventData);
			},
			error: function () {
				HideLoadingBox();
			}
		});
	};
	MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent = function (event, eventData) {
		var VariableBOESumByWBSDialog = $('#VariableBOESumByWBS');

		if (VariableBOESumByWBSDialog == undefined || VariableBOESumByWBSDialog.length == 0) {
			// if the WBS dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByWBS function)
			MOQEquationFieldWidget.OpenSumOfBOEsByWBS(event, eventData);
		}
	};
	MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent = function (event, eventData) {
		var VariableBOESumByCLINDialog = $('#VariableBOESumByCLIN');

		if (VariableBOESumByCLINDialog == undefined || VariableBOESumByCLINDialog.length == 0) {
			// if the CLIN dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByCLIN function)
			MOQEquationFieldWidget.OpenSumOfBOEsByCLIN(event, eventData);
		}
	};


	MOQEquationFieldWidget.AfterDomLoad = function (taskElementDetailsWidget, showMoqQuestions, numberOfMoqQuestions) {
		taskElementDetailsWidget.ChildWidgets.push(MOQEquationFieldWidget);
		taskElementDetailsWidget.registerForDelegateEvent('click', '.menu-icon', function (event) {
			if ($('#menu-options-box').hasClass("display-none")) {
				$('#menu-options-box').removeClass('display-none');
			}
			else {
				$('#menu-options-box').addClass('display-none');
			}
		});
		taskElementDetailsWidget.registerForDelegateEvent('click', '#menu-options-box', function (event) {
			$('#menu-options-box').addClass('display-none');
			event.stopPropagation();
		});

		taskElementDetailsWidget.registerForDelegateEvent('change', '.moqRteFieldContainer input, .tableData textarea', function (event) {
			MOQEquationFieldWidget.setDirty();
		});

		taskElementDetailsWidget.CheckToShowMetrics();
		taskElementDetailsWidget.MOQText = CreateRteTemplate(showMoqQuestions, numberOfMoqQuestions);

		if (!MOQEquationFieldWidget.isReadOnly() || shouldMoqReadOnlyBeReversed || !taskElementDetailsWidget.isReadOnly()) {
			InitializeRteTemplate(taskElementDetailsWidget.MOQText, 'MOQText', rteFieldSize);
		}
		else {
			setTimeout(function () {
				HandleRTETemplateDataForReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText');
			}, 1);
		}

		MOQEquationFieldWidget.InitializeDialog(MOQEquationFieldWidget.ReOrderMoqTypesDialog);
		MOQEquationFieldWidget.InitializeDialog(MOQEquationFieldWidget.ReOrderMoqTablesDialog);

		$(document).trigger('MOQWidgetLoaded', "MOQEquationField");
		$(document).trigger('WidgetLoaded', "MOQEquationField");
	}

	/// Reordering MOQ Types
	MOQEquationFieldWidget.ReOrderMoqTypesDialog = {};
	MOQEquationFieldWidget.ReOrderMoqTypesDialog.Element = $("#ReOrderMoqTypesDialog");
	MOQEquationFieldWidget.ReOrderMoqTypesDialog.Params = { width: 600, height: 250, modal: true, resizable: false, draggable: true, title: 'Sort Moq Types' };
	/// END Reordering MOQ Types

	/// Reordering MOQ Tables
	MOQEquationFieldWidget.ReOrderMoqTablesDialog = {};
	MOQEquationFieldWidget.ReOrderMoqTablesDialog.Element = $("#ReOrderMoqTablesDialog");
	MOQEquationFieldWidget.ReOrderMoqTablesDialog.Params = { width: 600, height: 230, modal: true, resizable: false, draggable: true, title: 'Sort Moq Tables' };
	/// END Reordering MOQ Tables

	$("#MOQEquationField #MOQEquation").change(function () {
		$(document).trigger('ValidateMOQEquation');
	});
	$("#MOQEquationField #MOQEquation").change(MOQEquationFieldWidget.setDirty);

	// use change instead of focusout to catch all changes having to deal with the moq equation or variables. focusout was not catching changes on boe to sum variable in IE
	MOQEquationFieldWidget.registerForLiveEvent('change', "#MOQEquationField #MOQEquation-Variables input.variableInput", function () {
		MOQEquationFieldWidget.CalculateMOQResult();
	});
	MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables a[readOnlyVar]', function () {
		var eventData = {};
		var variable = undefined;

		if ($(this).is('[wsVar]')) {
			variable = MOQEquationFieldWidget.GetWorkspaceVariable($(this).attr('name'));

			if (variable == undefined) { return; }

			eventData.dialogTitle = 'Summed BOEs for ' + variable.WorkspaceVariableName;
		}
		else if ($(this).is('[ordVar]')) {
			variable = MOQEquationFieldWidget.GetOrdinaryVariable($(this).attr('name'));

			if (variable == undefined) { return; }

			eventData.dialogTitle = 'Summed BOEs for ' + variable.OrdinaryVariableName;
		}

		eventData.BOEToSum = variable.BOEToSum;
		eventData.WBSToSum = variable.WBSToSum;
		eventData.CLINToSum = variable.CLINToSum;
		eventData.ResourceTypes = variable.ResourceTypes;
		eventData.SortBOEBy = variable.SortBOEBy;
		eventData.ReadOnly = true;

		if (eventData.SortBOEBy == sortBOEByClin) {
			$(document).trigger('VariableBOESumByCLIN_OpenReadOnly', eventData);
		}
		else {
			$(document).trigger('VariableBOESumByWBS_OpenReadOnly', eventData);
		}
	});
	MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables a.sumVariable', function () {
		var pkid = $(this).attr('var');
		var selectedInput = $(this).siblings('input[ordVar][pkid=' + pkid + ']');

		var eventData = {};
		eventData.pkid = pkid;
		eventData.BOEToSum = JSON.parse(selectedInput.attr('BOEToSum'));
		eventData.WBSToSum = JSON.parse(selectedInput.attr('WBSToSum'));
		eventData.CLINToSum = JSON.parse(selectedInput.attr('CLINToSum'));
		eventData.ResourceTypes = JSON.parse(selectedInput.attr('ResourceTypes'));
		eventData.SortBOEBy = selectedInput.attr('SortBOEBy');
		eventData.ReadOnly = false;

		if (eventData.BOEToSum.length || eventData.WBSToSum.length || eventData.CLINToSum.length) {
			if (eventData.SortBOEBy == sortBOEByClin) {
				$(document).trigger('VariableBOESumByCLIN_Open', eventData);
			} else {
				$(document).trigger('VariableBOESumByWBS_Open', eventData);
			}
		}
		else {
			if (eventData.SortBOEBy == sortBOEByClin) {
				$(document).trigger('VariableBOESumByCLIN_OpenNew', eventData);
			} else {
				$(document).trigger('VariableBOESumByWBS_OpenNew', eventData);
			}
		}
	});
	MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables span.clearVariable a', function () {
		var pkid = $(this).parent().attr('var');

		$(this).parents('#MOQEquationField #MOQEquation-Variables')
			.find('input[pkid=' + pkid + ']')
			.attr('BOEToSum', '[]')
			.attr('WBSToSum', '[]')
			.attr('CLINToSum', '[]')
			.attr('ResourceTypes', '[]')
			.attr('SortBOEBy', sortBOEByWBS)
			.prop('disabled', false)
			.val('')
			.change()
			.siblings('span.clearVariable[var=' + pkid + ']')
			.remove();
	});

	MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Create', function (event, eventData) {
		MOQEquationFieldWidget.setDirty();

		var variableInput = $('#MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');

		variableInput
			.val(Helper.addCommas(eventData.Sum))
			.attr('BOEToSum', JSON.stringify(eventData.BOEToSum))
			.attr('WBSToSum', '[]')
			.attr('CLINToSum', JSON.stringify(eventData.CLINToSum))
			.attr('ResourceTypes', JSON.stringify(eventData.ResourceTypes))
			.attr('SortBOEBy', eventData.SortBOEBy)
			.prop('disabled', true)
			.change();

		if (!variableInput.siblings('span.clearVariable[var=' + eventData.pkid + ']').length) {
			variableInput
				.parent()
				.append(' <span class="clearVariable" var="' + eventData.pkid + '">|<a>Clear</a></span>');
		}

		$(document).trigger('ValidateMOQEquation');
	});
	MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Create', function (event, eventData) {
		MOQEquationFieldWidget.setDirty();

		var variableInput = $('#MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');

		variableInput
			.val(Helper.addCommas(eventData.Sum))
			.attr('BOEToSum', JSON.stringify(eventData.BOEToSum))
			.attr('WBSToSum', JSON.stringify(eventData.WBSToSum))
			.attr('CLINToSum', '[]')
			.attr('ResourceTypes', JSON.stringify(eventData.ResourceTypes))
			.attr('SortBOEBy', eventData.SortBOEBy)
			.prop('disabled', true)
			.change();

		if (!variableInput.siblings('span.clearVariable[var=' + eventData.pkid + ']').length) {
			variableInput
				.parent()
				.append(' <span class="clearVariable" var="' + eventData.pkid + '">| <a>Clear</a></span>');
		}

		$(document).trigger('ValidateMOQEquation');
	});
	MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Open', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
	MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenNew', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
	MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenReadOnly', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
	MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Open', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
	MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenNew', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
	MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenReadOnly', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
	MOQEquationFieldWidget.registerForEvent('ValidateMOQEquation', function (event, options) {
		if (options != undefined && options.initialLoad != undefined && options.initialLoad != null) {
			MOQEquationFieldWidget.initialLoad = options.initialLoad;
		} else {
			MOQEquationFieldWidget.initialLoad = false;
		}

		MOQEquationFieldWidget.ValidateMOQEquation();
	});
	MOQEquationFieldWidget.registerForEvent('InsertMOQElementDialogClosing', MOQEquationFieldWidget.InsertMOQElementDialogClosing);

	if (MOQEquationFieldWidget_ReadOnly) {
		$('.magnifier-button').addClass('display-none');
	}

	return MOQEquationFieldWidget;
}
