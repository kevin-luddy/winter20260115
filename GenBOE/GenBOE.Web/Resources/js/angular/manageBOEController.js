angular.module('genboe').controller('ManageBOEController', ['$scope', '$http', '$timeout', '$filter', '$cookies', 'ManageBOEModel', function ($scope, $http, $timeout, $filter, $cookies, ManageBOEModel) {
	$scope.columns = {
		wbs: 'WbsDisplayName',
		boe: 'BOETitle',
		clin: 'ClinDisplayName',
		authors: 'AuthorOrderName',
		approvers: 'ApproverOrderName',
		status: 'Status',
		updateDate: 'UpdateDate',
		hours: 'TotalHours',
		cost: 'TotalCost',
		subauthors: 'SubcontractorOrderName',
		boeId: 'BoeID'
	};

	$scope.errors = [];
	$scope.modalErrors = [];
	$scope.gridModel = { containsOCI: true };
	$scope.deleteAll = {};
	$scope.deleteAll.deleteAll = false;

	$scope.isExporting = false;
	// Manage BOE can be done when workspace state is Initialization or Working
	$scope.isWorkingState = ManageBOEModel.workspaceState === 'Working' || ManageBOEModel.workspaceState === 'Initialization';
	$scope.isWorkspaceLocked = ManageBOEModel.workspaceState === 'Locked';

	$scope.dialog = {
		title: 'Import BOEs',
		open: false,
		file: null,
		importWorking: false,
		completeImportWorking: false,
		showImportResults: false,
		disableImport: true,
		invalidData: false,
		importResults: [],
		importExisting: null,  // true if importing existing BOEs, otherwise false
		showSteps: false // steps 2 - 4 are hidden until step 1 is complete
	};

	$scope.filter = {
		wbs: {}, boe: {}, clin: {}, author: {}, approver: {}, status: {}, boeId: {},
		filterColumn: '',
		open: false
	};

	$scope.edit = {
		open: false,
		boe: {},
		isAdd: false,
		isDirty: false,
		showMaterial: false,
		showSelectMultiClinWarning: false,
		showDeselectMultiClinWarning: false,
		showStatus: 'Draft'
	};

	$scope.data = [];
	$scope.bulkAssignData = [];
	$scope.bulkAssignBoes = [];
	$scope.bulkAssignAuthors = [];
	$scope.bulkAssignApprovers = [];
	$scope.bulkAssignSubAuthors = [];
	$scope.isLoading = true;
	$scope.isBulkAssign = false;
	$scope.isDirty = false;

	$scope.selectedBoes = [];
	$scope.selectedRole;
	$scope.selectedUsers = [];

	$scope.boeDropdownSettings = {
		scrollableHeight: '200px',
		scrollable: true,
		checkBoxes: true,
		enableSearch: true,
		searchField: 'search',
		enableSearch: true
	};

	$scope.userDropdownSettings = {
		scrollableHeight: '200px',
		scrollable: true,
		checkBoxes: true,
		enableSearch: true
	};

	$scope.roles = {
		author: 'Author',
		approver: 'Approver',
		subAuthor: 'Subcontract Author'
	}

	$scope.predicate = [$scope.columns.wbs, $scope.columns.clin, $scope.columns.authors];
	$scope.reverse = false;
	$scope.pageSize = 100;
	$scope.currentPage = 0;
	$scope.searchText = '';
	$scope.colSpan = 9;

	$scope.enableTaskAuthor = false;

	// reset current page when a user searches
	$scope.searchChanged = function () {
		$scope.currentPage = 0;
		$scope.SaveFilterToCookies();
	}

	/*
	 * ******************* NOTE ********************
	 * Functions below relate to the filtering logic
	 * *********************************************
	 */
	$scope.toggleFilter = function (column) {
		if ($scope.isLoading) {
			return;
		}

		if (angular.isDefined(column)) {
			$scope.filter.filterColumn = column;
		}

		$scope.filter.open = !$scope.filter.open;
	};

	// called when a filter checkbox is clicked
	$scope.checkboxDirty = function (index, item) {
		// if the item has not been added, then save it's original value
		if (!angular.isDefined(originalFilterSet[index])) {
			// save the opposite of the value because by the time it gets here, the value
			// has already been updated and we want to save its original value, not the
			// current value
			originalFilterSet[index] = !item.checked;
		}
	};

	// clear all checked rows
	$scope.clearFilter = function () {
		var workingSet = getWorkingFilterSet();
		workingSet.forEach(function (item, index) {
			if (item.checked) {
				item.checked = false;

				// need to save the state of each cleared checkbox
				$scope.checkboxDirty(index, item);
			}
		});

		$scope.SaveFilterToCookies();
	};

	$scope.applyFilters = function (saveFilters, toggleFilters = true) {
		var selected = [];
		var workingSet = getWorkingFilterSet();
		workingSet.forEach(function (item) {
			if (item.checked) {
				selected.push(item.value);
			}
		});

		setWorkingFilterSet(selected);
		$scope.currentPage = 0;
		originalFilterSet = {};

		if (toggleFilters) {
			$scope.toggleFilter();
		}

		if (saveFilters) {
			$scope.SaveFilterToCookies();
		}
	};

	// function called when the dialog closes. if the original filter set is populated, we will
	// revert that data for the current working filter
	$scope.onClose = function () {
		var workingSet = getWorkingFilterSet();

		// revert changes to their original value
		for (var key in originalFilterSet) {
			workingSet[key].checked = originalFilterSet[key];
		}

		originalFilterSet = {};
	};

	// determines when to show filtering message
	$scope.isDataFiltered = function () {
		return selectedFilters.wbs.length > 0 || selectedFilters.boe.length > 0
			|| selectedFilters.clin.length > 0 || selectedFilters.author.length > 0
			|| selectedFilters.approver.length > 0 || selectedFilters.status.length > 0
			|| $scope.searchText.length > 0 || selectedFilters.boeId.length > 0;
	};

	// reset all filters to be empty and unselected
	$scope.clearAllFilters = function (noClose) {
		selectedFilters.wbs = [];
		selectedFilters.boe = [];
		selectedFilters.clin = [];
		selectedFilters.author = [];
		selectedFilters.approver = [];
		selectedFilters.status = [];
		selectedFilters.boeId = [];
		$scope.searchText = '';

		$scope.filter.wbs.forEach(function (item) {
			item.checked = false;
		});
		$scope.filter.boe.forEach(function (item) {
			item.checked = false;
		});
		$scope.filter.clin.forEach(function (item) {
			item.checked = false;
		});
		$scope.filter.approver.forEach(function (item) {
			item.checked = false;
		});
		$scope.filter.author.forEach(function (item) {
			item.checked = false;
		});
		$scope.filter.status.forEach(function (item) {
			item.checked = false;
		});
		$scope.filter.boeId.forEach(function (item) {
			item.checked = false;
		});

		$scope.currentPage = 0;
		if (!noClose) {
			$scope.filter.open = false;
		}

		$scope.SaveFilterToCookies();
	};

	$scope.assignedAsAuthor = function () {
		var currentName = currentUserName.toLowerCase();

		$scope.clearAllFilters(true);
		selectedFilters.author.push(currentName);

		// search for the name and set it to checked
		$scope.filter.author.some(function (item) {
			if (item.value === currentName) {
				item.checked = true;
				return true;
			}
		});

		$scope.SaveFilterToCookies();
	};

	$scope.assignedAsApprover = function () {
		var currentName = currentUserName.toLowerCase();

		$scope.clearAllFilters(true);
		selectedFilters.approver.push(currentName);

		// search for the name and set it to checked
		$scope.filter.approver.some(function (item) {
			if (item.value === currentName) {
				item.checked = true;
				return true;
			}
		});

		$scope.SaveFilterToCookies();
	};

	$scope.filterBOEs = function (data) {
		var compareValue = $scope.searchText.toLowerCase();
		var foundName = false;

		// if top filter contains data, it MUST match
		if (selectedFilters.wbs.length > 0 && selectedFilters.wbs.indexOf(data.WbsDisplayName.toLowerCase()) === -1) {
			return false;
		}

		if (selectedFilters.boe.length > 0 && selectedFilters.boe.indexOf(data.BOETitle.toLowerCase()) === -1) {
			return false;
		}

		if (selectedFilters.clin.length > 0 && selectedFilters.clin.indexOf(data.ClinDisplayName.toLowerCase()) === -1) {
			return false;
		}

		if (selectedFilters.status.length > 0 && selectedFilters.status.indexOf(data.Status.toLowerCase()) === -1) {
			return false;
		}

		if (selectedFilters.boeId.length > 0 && selectedFilters.boeId.indexOf(data.BoeID.toString()) === -1) {
			return false;
		}

		if (data.AuthorsDisplayNames && selectedFilters.author.length > 0) {
			foundName = data.AuthorsDisplayNames.some(function (item) {
				if (selectedFilters.author.indexOf(item.toLowerCase()) !== -1) {
					return true;
				}
			});
			if (!foundName) {
				return false;
			}
		}
		if (data.ApproversDisplayNames && selectedFilters.approver.length > 0) {
			foundName = data.ApproversDisplayNames.some(function (item) {
				if (selectedFilters.approver.indexOf(item.toLowerCase()) !== -1) {
					return true;
				}
			});
			if (!foundName) {
				return false;
			}
		}

		// filter based on search term
		if (data.WbsDisplayName && data.WbsDisplayName.toLowerCase().indexOf(compareValue) !== -1) {
			return true;
		}

		if (data.BOETitle && data.BOETitle.toLowerCase().indexOf(compareValue) !== -1) {
			return true;
		}

		if (data.ClinDisplayName && data.ClinDisplayName.toLowerCase().indexOf(compareValue) !== -1) {
			return true;
		}

		var foundName = false;
		if (data.AuthorsDisplayNames) {
			var foundName = data.AuthorsDisplayNames.some(function (item) {
				if (item.toLowerCase().indexOf(compareValue) !== -1) {
					return true;
				}
			});
			if (foundName) {
				return true;
			}
		}

		if (data.ApproversDisplayNames) {
			foundName = data.ApproversDisplayNames.some(function (item) {
				if (item.toLowerCase().indexOf(compareValue) !== -1) {
					return true;
				}
			});
			if (foundName) {
				return true;
			}
		}

		if (data.Status && data.Status.toLowerCase().indexOf(compareValue) !== -1) {
			return true;
		}

		// nothing matches so return false
		return false;
	};

	// Save the filter settings
	$scope.SaveFilterToCookies = function () {
		var date = new Date();
		var minutes = 240;
		date.setTime(date.getTime() + (minutes * 60 * 1000));
		console.log(ManageBOEModel.workspace);

		$cookies.put('filtersWbs' + ManageBOEModel.workspace, angular.toJson($scope.filter.wbs), { expires: date });
		// $cookies.put('filtersBoe' + ManageBOEModel.workspace, angular.toJson($scope.filter.boe), { expires: date });
		$cookies.put('filtersClin' + ManageBOEModel.workspace, angular.toJson($scope.filter.clin), { expires: date });
		$cookies.put('filtersAuthor' + ManageBOEModel.workspace, angular.toJson($scope.filter.author), { expires: date });
		$cookies.put('filtersApprover' + ManageBOEModel.workspace, angular.toJson($scope.filter.approver), { expires: date });
		$cookies.put('filtersStatus' + ManageBOEModel.workspace, angular.toJson($scope.filter.status), { expires: date });

		$cookies.put('searchTextBoe' + ManageBOEModel.workspace, angular.toJson($scope.searchText), { expires: date });
		$cookies.put('predicateBoe' + ManageBOEModel.workspace, angular.toJson($scope.predicate), { expires: date });
		$cookies.put('reverseBoe' + ManageBOEModel.workspace, angular.toJson($scope.reverse), { expires: date });
	}

	// Load the filter settings
	$scope.LoadFilterFromCookies = function () {
		try {
			var temp;
			var cookieName;
			var cookies = $cookies.getAll();

			cookieName = 'filtersWbs' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.filter.wbs = temp;
					$scope.filter.filterColumn = $scope.columns.wbs;
					$scope.applyFilters(false);
				}
			}

			cookieName = 'filtersBoe' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.filter.boe = temp;
					$scope.filter.filterColumn = $scope.columns.boe;
					$scope.applyFilters(false);
				}
			}

			cookieName = 'filtersClin' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.filter.clin = temp;
					$scope.filter.filterColumn = $scope.columns.clin;
					$scope.applyFilters(false);
				}
			}

			cookieName = 'filtersAuthor' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.filter.author = temp;
					$scope.filter.filterColumn = $scope.columns.authors;
					$scope.applyFilters(false);
				}
			}

			cookieName = 'filtersApprover' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.filter.approver = temp;
					$scope.filter.filterColumn = $scope.columns.approvers;
					$scope.applyFilters(false);
				}
			}

			cookieName = 'filtersStatus' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.filter.status = temp;
					$scope.filter.filterColumn = $scope.columns.status;
					$scope.applyFilters(false);
				}
			}

			cookieName = 'searchTextBoe' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.searchText = temp;
				}
			}

			cookieName = 'predicateBoe' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.predicate = temp;
				}
			}

			cookieName = 'reverseBoe' + ManageBOEModel.workspace;
			if (cookieName in cookies) {
				temp = JSON.parse(cookies[cookieName]);
				if (temp != undefined) {
					$scope.reverse = temp;
				}
			}
		} catch (ex) {
			console.log('Bad cookie load');
			console.log(ex);

			var date = new Date();
			var minutes = -1;
			date.setTime(date.getTime() + (minutes * 60 * 1000));

			$cookies.put('filtersWbs' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('filtersBoe' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('filtersClin' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('filtersAuthor' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('filtersApprover' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('filtersStatus' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('searchTextBoe' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('predicateBoe' + ManageBOEModel.workspace, null, { expires: date });
			$cookies.put('reverseBoe' + ManageBOEModel.workspace, null, { expires: date });
		}
	}

	/*
	 * ***************** NOTE ******************
	 * Functions below relate to the table logic
	 * *****************************************
	 */
	$scope.changeSorting = function (sortValue) {
		// clicking the same column reverses the sort
		if ($scope.predicate[0] === sortValue) {
			$scope.reverse = !$scope.reverse;
		} else {
			$scope.reverse = false;

			// custom sorting for wbs and clin
			if (sortValue === $scope.columns.wbs) {
				$scope.predicate = [sortValue, $scope.columns.clin, $scope.columns.authors];
			} else if (sortValue === $scope.columns.clin) {
				$scope.predicate = [sortValue, $scope.columns.wbs, $scope.columns.authors];
			} else {
				$scope.predicate = [sortValue];
			}
		}

		$scope.sortBulkAssignData();

		$scope.SaveFilterToCookies();
	};

	$scope.sortBulkAssignData = function () {
		$scope.bulkAssignBoes = [];
		var sortedBoes = $filter('orderBy')($scope.isBulkAssign ? $scope.bulkAssignData : $scope.data, $scope.predicate, $scope.reverse);

		sortedBoes.forEach(function (boe) {
			var boeString = boe.WbsDisplayName + (boe.BOETitle.length > 0 ? " | " + boe.BOETitle : "") + " | " + boe.ClinDisplayName;

			$scope.bulkAssignBoes.push({ id: boe.BoeID, label: boeString, search: boeString });
		});
	}

	// returns true when the column being sorted matches the first index
	$scope.boldSort = function (sortColumn) {
		return $scope.predicate[0] === sortColumn;
	};

	$scope.numberOfPages = function (numFilteredRows) {
		var totalRows = angular.isDefined(numFilteredRows) ? numFilteredRows.length : 0;
		return Math.ceil(totalRows / $scope.pageSize);
	}

	$scope.toggleImport = function () {
		if ($scope.isWorkingState) {
			$scope.dialog.disableImport = true;
			$scope.dialog.importWorking = false;
			$scope.dialog.showImportResults = false;
			$scope.dialog.completeImportWorking = false;
			$scope.dialog.open = !$scope.dialog.open;
			$scope.dialog.showSteps = false;
			$scope.dialog.importExisting = null;
			// clear file input field
			resetUploadForm();
		}
	};

	/*
	 * **************************** NOTE *****************************
	 * Functions below relate to the logic of the dialog import
	 * ***************************************************************
	 */

	$scope.fileUploadChange = function (element) {
		$scope.$apply(function($scope) {
			$scope.dialog.disableImport = element.value.endsWith('.xlsx') || element.value.endsWith('.xlsm') ? false : true;
			$scope.dialog.file = $scope.dialog.disableImport ? null : element.files[0];
		});
	};

	$scope.showImportSteps = function () {
		$scope.dialog.showSteps = true;
	};

	var resetUploadForm = function () {
		$("#ImportBOEDialog-Form")[0].reset();
		$scope.dialog.disableImport = true;
		$scope.dialog.file = null;
	};

	$scope.importBOEs = function () {
		if (!$scope.dialog.disableImport) {
			// create form data
			var fd = new FormData();
			fd.append("file", $scope.dialog.file);

			// get url from form
			var url = $('#ImportBOEDialog-Form').attr('action');
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
					$scope.dialog.invalidData = window.ManageBoeImportVerificationWidget.invalidData;
					$scope.dialog.importResults = window.ManageBoeImportVerificationWidget.data.importResults;
				}, 0);
			});
		}
	};

	$scope.completeImportBOEs = function () {
		$scope.dialog.completeImportWorking = true;
		var data = {};
		data.importResults = $scope.dialog.importResults;
		$http({
			method: 'POST',
			url: CreatePostURL(ManageBOEModel.workspace, ManageBOEModel.controller, ManageBOEModel.completeImportAction, ''),
			data: data
		}).then(function () {
			$scope.toggleImport();
			loadBOEs();
			RaiseNotification('Import Successful');
		}).catch(function () {
			$scope.toggleImport();
			loadBOEs();
			RaiseNotification('Import Failed');
		}).finally(function () {
			$scope.backFromImport();
		});
	};

	// clicking back from import results
	$scope.backFromImport = function () {
		resetUploadForm();
		$scope.dialog.showImportResults = false;
	};

	/*
	 * *********** NOTE ************
	 * Private variables & functions 
	 * *****************************
	 */
	var blank = "(blanks)";
	var firstLoad = true;
	var originalFilterSet = {};
	var currentUserName = '';
	var selectedFilters = { wbs: [], boe: [], clin: [], author: [], approver: [], status: [], boeId: [] };

	var convertToArray = function (data) {
		var newData = [];

		for (var key in data) {
			var item = data[key];

			// set the display value to '(blanks)' if the display name is empty
			if (item.display == '') {
				item.display = blank;
			}

			// set the value to lowercase
			if (angular.isDefined(item.value) && item.value !== null) {
				item.value = item.value.toLowerCase();
			}

			newData.push(item);
		}

		return $filter('orderBy')(newData, 'value');
	};

	var getWorkingFilterSet = function () {
		if ($scope.filter.filterColumn === $scope.columns.wbs) {
			return $scope.filter.wbs;
		} else if ($scope.filter.filterColumn === $scope.columns.boe) {
			return $scope.filter.boe;
		} else if ($scope.filter.filterColumn === $scope.columns.clin) {
			return $scope.filter.clin;
		} else if ($scope.filter.filterColumn === $scope.columns.approvers) {
			return $scope.filter.approver;
		} else if ($scope.filter.filterColumn === $scope.columns.authors) {
			return $scope.filter.author;
		} else if ($scope.filter.filterColumn === $scope.columns.status) {
			return $scope.filter.status;
		} else if ($scope.filter.filterColumn === $scope.columns.boeId) {
			return $scope.filter.boeId;
		}
	};

	var setWorkingFilterSet = function (currentSet) {
		if (angular.isDefined(currentSet)) {
			if ($scope.filter.filterColumn === $scope.columns.wbs) {
				selectedFilters.wbs = currentSet;
			} else if ($scope.filter.filterColumn === $scope.columns.boe) {
				selectedFilters.boe = currentSet;
			} else if ($scope.filter.filterColumn === $scope.columns.clin) {
				selectedFilters.clin = currentSet;
			} else if ($scope.filter.filterColumn === $scope.columns.approvers) {
				selectedFilters.approver = currentSet;
			} else if ($scope.filter.filterColumn === $scope.columns.authors) {
				selectedFilters.author = currentSet;
			} else if ($scope.filter.filterColumn === $scope.columns.status) {
				selectedFilters.status = currentSet;
			} else if ($scope.filter.filterColumn === $scope.columns.boeId) {
				selectedFilters.boeId = currentSet;
			}
		}
	};

	$scope.toggleDeleteAll = function () {
		if ($scope.deleteAll.deleteAll === false) {
			// unmark ALL
			$scope.data.forEach(function (item) {
				item.Deleted = $scope.deleteAll.deleteAll;
			});
		} else {
			// get all visible BoeIDs
			var boeIds = $('input.delete-chck:not([disabled])').parents('tr').map(function () { return $(this).attr("pkid"); }).get();

			// mark items appropriately
			$scope.filteredResults.forEach(function (item) {
				item.Deleted = $.inArray(item.BoeID + '', boeIds) !== -1;
			});
		}
	};

	$scope.disableDelete = function () {
		var disabled = true;
		$scope.data.forEach(function (item) {
			if (item.Deleted) {
				disabled = false;
			}
		});

		return disabled;
	};

	$scope.delete = function () {
		Session.confirmDialog('Delete BOEs', 'Are you sure you want to delete these BOEs? This cannot be undone.', function () {
			$timeout(function () {
				var data = {};
				data.boes = [];

				$scope.data.forEach(function (item) {
					if (item.Deleted) {
						data.boes.push(item);
					}
				});

				if (data.boes.length > 4) {
					RaiseNotification('The system automatically backed up this workspace prior to deletion');
				}

				$scope.checkForHiddenItems(data, true);
			}, 50);
		});
	};

	$scope.resetDraft = function () {
		Session.confirmDialog('Reset BOEs to Draft', 'Are you sure you want to reset these BOEs to Draft? This cannot be undone.', function () {
			$timeout(function () {
				var data = {};
				data.boes = [];

				$scope.data.forEach(function (item) {
					if (item.Deleted) { // reusing the checkbox/property for Deleted
						data.boes.push(item);
					}
				});

				$scope.checkForHiddenItems(data, false);
			}, 50);
		});
	};

	$scope.checkForHiddenItems = function (data, isDelete) {
		// start and end indexes for items on page
		var start = $scope.currentPage * $scope.pageSize;
		var end = start + $scope.pageSize

		// get items currently displayed on page
		var currentPageItems = $scope.filteredResults.slice(start, end);

		// check if any items are selected, but not currently displayed
		var itemsOnOtherPages = data.boes.some(function (s) {
			return currentPageItems.indexOf(s) < 0;
		});

		if (itemsOnOtherPages) {
			Session.confirmDialog('Hidden Items',
				'You have selected BOEs that are currently hidden from view. Would you like to continue? Select No to review these items.',
				function () {
					if (isDelete) {
						$scope.continueDelete(data);
					} else {
						$scope.continueResetDraft(data);
					}
				});
		} else {
			if (isDelete) {
				$scope.continueDelete(data);
			} else {
				$scope.continueResetDraft(data);
			}
		}
	}

	$scope.continueResetDraft = function (data) {
		$scope.errors = [];

		$('#PageLoading').removeClass('display-none');
		$http({
			method: 'POST',
			url: CreatePostURL(ManageBOEModel.workspace, ManageBOEModel.controller, ManageBOEModel.resetDraftAction, ''),
			data: data
		}).then(function successCallback(response) {

			loadBOEs();
			$('#PageLoading').addClass('display-none');
		}, function errorCallback(response) {
			$scope.errors = response.data.MessageList;
			$('#PageLoading').addClass('display-none');
		});
	};

	$scope.continueDelete = function (data) {
		$scope.errors = [];

		$('#PageLoading').removeClass('display-none');
		$http({
			method: 'POST',
			url: CreatePostURL(ManageBOEModel.workspace, ManageBOEModel.controller, ManageBOEModel.saveAction, ''),
			data: data
		}).then(function successCallback(response) {

			// remove the items from the data []
			data.boes.forEach(function (item) {
				$scope.removeBOE(item);
			});

			loadBOEs();
			$('#PageLoading').addClass('display-none');
		}, function errorCallback(response) {
			$scope.errors = response.data.MessageList;
			$('#PageLoading').addClass('display-none');
		});
	};

	$scope.removeBOE = function (boe) {
		var index = -1;

		for (var i = 0; i < $scope.data.length; i++) {
			if ($scope.data[i].BoeID == boe.BoeID) {
				index = i;
			}
		}

		if (index >= 0) {
			$scope.data.splice(index, 1);
		}
	};

	$scope.hideSelectMultiClinWarning = function () {
		$scope.edit.showSelectMultiClinWarning = false;
	};

	$scope.hideDeselectMultiClinWarning = function () {
		$scope.edit.showDeselectMultiClinWarning = false;
	};

	$scope.updateMultiClin = function () {
		$scope.setEditDirty();

		if ($scope.edit.boe.IsMultiClinWbs) {
			// changed to Multi Clin
			$scope.edit.showMaterial = false;
			$scope.edit.boe.isMaterial = false;
			$scope.edit.showSelectMultiClinWarning = !$scope.edit.isAdd;
			$scope.edit.showDeselectMultiClinWarning = false;
			$scope.edit.boe.WbsID = $scope.gridModel.MultiWBSId;
			$scope.edit.boe.ClinID = $scope.gridModel.MultiClin.ClinID;
		} else {
			// changed to not be Multi Clin
			$scope.edit.showSelectMultiClinWarning = false;
			$scope.edit.showDeselectMultiClinWarning = !$scope.edit.isAdd;
			$scope.edit.boe.WbsID = '-1';
			$scope.edit.boe.ClinID = '';
		}
	};

	$scope.save = function (saveAndAdd, showModalErrors) {
		if ($scope.edit.isDirty) {
			$('#PageLoading').removeClass('display-none');
			var data = {};

			$scope.edit.boe.Deleted = false;

			data.boes = [$scope.edit.boe];
			if (showModalErrors) {
				$scope.modalErrors = [];
			} else {
				$scope.errors = [];
			}

			$http({
				method: 'POST',
				url: CreatePostURL(ManageBOEModel.workspace, ManageBOEModel.controller, ManageBOEModel.saveAction, ''),
				data: data
			}).then(function successCallback(response) {
				if (saveAndAdd) {
					$scope.AddBOE();
				} else {
					$scope.edit.open = false;
				}

				loadBOEs();
				$('#PageLoading').addClass('display-none');
			}, function errorCallback(response) {
				if (showModalErrors) {
					$scope.modalErrors = response.data.MessageList;
				} else {
					$scope.errors = response.data.MessageList;
				}

				$('#PageLoading').addClass('display-none');
			});
		}
	};

	$scope.AddBOE = function () {
		$scope.edit.boe = { Id: -1, WbsID: '-1' };
		$scope.edit.showSelectMultiClinWarning = false;
		$scope.edit.showDeselectMultiClinWarning = false;
		$scope.edit.showMaterial = false;
		$scope.edit.isAdd = true;
		$scope.edit.showStatus = 'Draft';
		$scope.edit.open = true;
	};

	$scope.onEditClose = function () {
		$scope.edit.open = false;
		$scope.edit.isDirty = false;
		$scope.edit.showSelectMultiClinWarning = false;
		$scope.edit.showDeselectMultiClinWarning = false;
	};

	$scope.export = function (isTemplate) {
		var timeoutTime = 2000;
		$scope.isExporting = true;

		var action = isTemplate ? ManageBOEModel.exportTemplateAction : ManageBOEModel.exportAction;

		var exportUrl = CreatePostURL(ManageBOEModel.workspace, ManageBOEModel.controller, action, '');
		GenWidget.prototype.performExport(exportUrl);

		// export is done via attaching an iframe, wait an arbitrary # of seconds (2-3) until showing the export button again to stop double-click
		$timeout(function () {
			$scope.isExporting = false;
		}, timeoutTime);

		// this is needed so IE does not stop animating gif(s) on the page
		return false;
	};

	$scope.setEditDirty = function () {
		$scope.edit.isDirty = true;
	};

	$scope.lockBOE = function (boe) {
		$scope.edit.boe = angular.copy(boe);
		$scope.edit.boe.State = ManageBOEModel.draftLockedState;
		$scope.edit.isDirty = true;
		$scope.save(false, false);
	};

	$scope.unlockBOE = function (boe) {
		Session.confirmDialog(
			"Confirm to Continue",
			"Unlocking this BOE will set the BOE back to Draft.  Do you wish to continue?",
			function () {  // Yes: OK to transition
				$scope.edit.boe = angular.copy(boe);
				$scope.edit.boe.State = ManageBOEModel.draftState;
				$scope.edit.isDirty = true;
				$scope.save(false, false);
			}
		);
	};

	$scope.lockEditBOE = function () {
		$scope.edit.boe.State = ManageBOEModel.draftLockedState;
		$scope.setEditDirty();
	};

	$scope.disableAuthorApprover = function () {
		var enable = $scope.isWorkingState && $scope.edit.showStatus !== 'Approved' && $scope.edit.showStatus !== 'AwaitingApproval' && $scope.edit.showStatus !== 'DraftLocked';
		return !enable;
	}

	$scope.unlockEditBOE = function () {
		$scope.edit.boe.State = ManageBOEModel.draftState;
		$scope.setEditDirty();
	};

	$scope.showEditStatus = function () {
		return $scope.isWorkingState && ($scope.edit.showStatus == 'Approved' || $scope.edit.showStatus == 'AwaitingApproval');
	};

	$scope.editBOE = function (boe) {
		$scope.edit.isAdd = false;

		if (boe.State <= 2) {
			$scope.edit.showStatus = 'Draft';
		} else if (boe.State == 3) {
			$scope.edit.showStatus = 'AwaitingApproval';
		} else if (boe.State == 4) {
			$scope.edit.showStatus = 'Approved';
		}

		$scope.edit.boe = angular.copy(boe);

		// State, WbsID, ClinID have to be strings for the angular select to work
		$scope.edit.boe.State = $scope.edit.boe.State.toString();
		if (!$scope.edit.boe.WbsID) {
			$scope.edit.boe.WbsID = '-1';
		} else {
			$scope.edit.boe.WbsID = $scope.edit.boe.WbsID.toString();
		}

		if ($scope.edit.boe.ClinID) {
			$scope.edit.boe.ClinID = $scope.edit.boe.ClinID.toString();
		}

		$scope.edit.showSelectMultiClinWarning = false;
		$scope.edit.showDeselectMultiClinWarning = false;
		// only show Material checkbox if the BOE is already material
		$scope.edit.showMaterial = $scope.edit.boe.isMaterial;
		$scope.edit.open = true;
	};

	var loadBOEs = function () {
		$scope.isLoading = true;
		$scope.data = [];
		$scope.bulkAssignData = [];
		$scope.errors = [];

		$scope.bulkAssignBoes = [];
		$scope.bulkAssignAuthors = [];
		$scope.bulkAssignApprovers = [];
		$scope.bulkAssignSubAuthors = [];

		return $http({
			method: 'POST',
			url: CreatePostURL(ManageBOEModel.workspace, ManageBOEModel.controller, ManageBOEModel.action, '')
		}).then(function (response) {

			$scope.gridModel = response.data;

			if ($scope.isWorkspaceLocked && $scope.gridModel.AllowBOEStateChanges) {
				$scope.colSpan = 10;
			} else {
				$scope.colSpan = 9;
			}

			// reset the filters
			$scope.filter = {
				wbs: {}, boe: {}, clin: {}, author: {}, approver: {}, status: {}, boeId: {},
				filterColumn: '',
				open: false
			};

			response.data.BoeResults.forEach(function (item) {
				if (!item.WbsDisplayName) {
					item.WbsDisplayName = 'NO WBS';
				}

				if (!item.ClinDisplayName) {
					item.ClinDisplayName = 'NO CLIN';
				}

				// build filter sets based on the data returned
				$scope.filter.boe[item.BOETitle] = { display: item.BOETitle, value: item.BOETitle, checked: false };
				$scope.filter.status[item.Status] = { display: item.Status, value: item.Status, checked: false };
				$scope.filter.boeId[item.BoeID] = { display: item.BoeID, value: item.BoeID.toString(), checked: false };

				// Remove c# date
				delete item.UpdateDate;
			});

			response.data.PotentialAuthors.forEach(function (author) {
				$scope.filter.author[author.Text] = { display: author.Text, value: author.Text, checked: false };
			});

			response.data.PotentialSubcontractorAuthors.forEach(function (sub) {
				$scope.filter.author[sub.Text] = { display: sub.Text, value: sub.Text, checked: false };
			});

			response.data.PotentialApprovers.forEach(function (approver) {
				$scope.filter.approver[approver.Text] = { display: approver.Text, value: approver.Text, checked: false };
			});

			response.data.PotentialWbs.forEach(function (wbs) {
				$scope.filter.wbs[wbs.Text] = { display: wbs.Text, value: wbs.Text, checked: false };
			});

			$scope.filter.wbs['MULTI'] = { display: 'MULTI', value: 'MULTI', checked: false };

			// For some odd reason the Potential Clins does not push in a blank, so push it in manually
			var blankClin = {
				ClinString: '',
				ClinID: '',
				StartDate: response.data.DefaultStartDate,
				EndDate: response.data.DefaultEndDate,
			};
			response.data.PotentialClins.unshift(blankClin);

			response.data.PotentialClins.forEach(function (clin) {
				$scope.filter.clin[clin.ClinString] = { display: clin.ClinString, value: clin.ClinString, checked: false };
			});

			$scope.filter.clin['MULTI'] = { display: 'MULTI', value: 'MULTI', checked: false };

			// need to convert to arrays and sort the data
			$scope.filter.wbs = convertToArray($scope.filter.wbs);
			$scope.filter.boe = convertToArray($scope.filter.boe);
			$scope.filter.clin = convertToArray($scope.filter.clin);
			$scope.filter.author = convertToArray($scope.filter.author);
			$scope.filter.approver = convertToArray($scope.filter.approver);
			$scope.filter.status = convertToArray($scope.filter.status);
			$scope.filter.boeId = convertToArray($scope.filter.boeId);

			$scope.data = response.data.BoeResults; 

			$scope.gridModel.PotentialAuthors.forEach(function (user) {
				$scope.bulkAssignAuthors.push({ id: user.Value, label: user.Text });
			});

			$scope.gridModel.PotentialApprovers.forEach(function (user) {
				$scope.bulkAssignApprovers.push({ id: user.Value, label: user.Text });
			});

			$scope.gridModel.PotentialSubcontractorAuthors.forEach(function (user) {
				$scope.bulkAssignSubAuthors.push({ id: user.Value, label: user.Text });
			});

			$scope.LoadFilterFromCookies();

			$scope.sortBulkAssignData();

			$scope.enableTaskAuthor = response.data.EnableTaskAuthor;

			$scope.isLoading = false;
			firstLoad = false;

		}, function errorCallback(response) {
			if (response.data && response.data.MessageList) {
				$scope.errors = response.data.MessageList;
			}
			$scope.isLoading = false;
		});
	}

	// load the main data
	loadBOEs();

	/*
	 * ******* NOTE *********
	 * Bulk Assign Functions
	 * **********************
	 */

	$scope.openBulkAssign = function () {
		// Only get BOEs in Draft or Unassigned - roles are locked in any other status
		$scope.bulkAssignData = angular.copy($scope.data.filter(x => x.Status == "Unassigned" || x.Status == "Draft"));
		$scope.isBulkAssign = true;
		$scope.sortBulkAssignData();
	};

	$scope.cancelBulkAssign = function () {
		if ($scope.isDirty) {
			Session.confirmDialog("Cancel", "Are you sure you want to cancel all changes?", function () { $scope.$apply(function () { $scope.continueCancelBulkAssign(); }) }, null);
		} else {
			$scope.continueCancelBulkAssign();
		}
	};

	$scope.continueCancelBulkAssign = function () {
		$scope.bulkAssignData = [];
		$scope.errors = [];
		$scope.isBulkAssign = false;
		$scope.cleanDirty();
	}

	$scope.bulkAssignRoles = function () {
		$scope.setDirty();

		$scope.selectedBoes.forEach(function (selectedBoe) {
			var boeToUpdate = $scope.bulkAssignData.find(boe => boe.BoeID == selectedBoe.id);

			if (boeToUpdate != undefined) {
				$scope.selectedUsers.forEach(function (selectedUser) {
					var userId = parseInt(selectedUser.id);

					if ($scope.selectedRole == $scope.roles.author) {
						if (!boeToUpdate.Authors.includes(userId)) {
							boeToUpdate.isDirty = true;
							boeToUpdate.Authors.push(userId);
							boeToUpdate.AuthorsDisplayNames.push(selectedUser.label);
						}
					} else if ($scope.selectedRole == $scope.roles.approver) {
						if (!boeToUpdate.Approvers.includes(userId)) {
							boeToUpdate.isDirty = true;
							boeToUpdate.Approvers.push(userId);
							boeToUpdate.ApproversDisplayNames.push(selectedUser.label);
						}
					} else if ($scope.selectedRole == $scope.roles.subAuthor) {
						if (!boeToUpdate.SubcontractorAuthors.includes(userId)) {
							boeToUpdate.isDirty = true;
							boeToUpdate.SubcontractorAuthors.push(userId);
							boeToUpdate.AuthorsDisplayNames.push(selectedUser.label);
						}
					}
				})
			}
		});

		$scope.validateBulkAssign(false, false);
		$scope.clearAllSelections();
	};

	$scope.bulkRemoveRoles = function () {
		$scope.setDirty();

		$scope.selectedBoes.forEach(function (selectedBoe) {
			var boeToUpdate = $scope.bulkAssignData.find(boe => boe.BoeID == selectedBoe.id);

			if (boeToUpdate != undefined) {
				$scope.selectedUsers.forEach(function (selectedUser) {
					var userId = parseInt(selectedUser.id);

					if ($scope.selectedRole == $scope.roles.author) {
						if (boeToUpdate.Authors.includes(userId)) {
							boeToUpdate.isDirty = true;
							var idIndex = boeToUpdate.Authors.indexOf(userId);
							var nameIndex = boeToUpdate.AuthorsDisplayNames.indexOf(selectedUser.label);
							boeToUpdate.Authors.splice(idIndex, 1);
							boeToUpdate.AuthorsDisplayNames.splice(nameIndex, 1);
						}
					} else if ($scope.selectedRole == $scope.roles.approver) {
						if (boeToUpdate.Approvers.includes(userId)) {
							boeToUpdate.isDirty = true;
							var idIndex = boeToUpdate.Approvers.indexOf(userId);
							var nameIndex = boeToUpdate.ApproversDisplayNames.indexOf(selectedUser.label);
							boeToUpdate.Approvers.splice(idIndex, 1);
							boeToUpdate.ApproversDisplayNames.splice(nameIndex, 1);
						}
					} else if ($scope.selectedRole == $scope.roles.subAuthor) {
						if (boeToUpdate.SubcontractorAuthors.includes(userId)) {
							boeToUpdate.isDirty = true;
							var idIndex = boeToUpdate.SubcontractorAuthors.indexOf(userId);
							var nameIndex = boeToUpdate.AuthorsDisplayNames.indexOf(selectedUser.label);
							boeToUpdate.SubcontractorAuthors.splice(idIndex, 1);
							boeToUpdate.AuthorsDisplayNames.splice(nameIndex, 1);
						}
					}
				});
			}
		});

		$scope.validateBulkAssign(false, false);
		$scope.clearAllSelections();
	};

	$scope.clearAllSelections = function () {
		$scope.selectedBoes = [];
		$scope.selectedRole = '';
		$scope.selectedUsers = [];
	}

	$scope.clearUsers = function () {
		$scope.selectedUsers = [];
	}

	$scope.disableAssignRemove = function () {
		return $scope.selectedBoes == undefined || $scope.selectedBoes == ''
			|| $scope.selectedRole == undefined || $scope.selectedRole == ''
			|| $scope.selectedUsers == undefined || $scope.selectedUsers == '';
	}

	$scope.filterForIncomplete = function () {
		$scope.clearAllFilters(true);

		$scope.bulkAssignData.forEach(function (boe) {
			if ((boe.Authors.length === 0 && boe.SubcontractorAuthors.length === 0) || boe.Approvers.length === 0) {
				$scope.filter.boeId.some(function (item) {
					if (item.value === boe.BoeID.toString()) {
						item.checked = true;
						return true;
					}
				});
			}
		});

		$scope.filter.filterColumn = $scope.columns.boeId;

		$scope.applyFilters(false, false);
	}

	$scope.filterForErrors = function () {
		$scope.clearAllFilters(true);
		$scope.bulkAssignData.forEach(function (boe) {
			if (boe.hasError) {
				$scope.filter.boeId.some(function (item) {
					if (item.value === boe.BoeID.toString()) {
						item.checked = true;
						return true;
					}
				});
			}
		});

		$scope.filter.filterColumn = $scope.columns.boeId;

		$scope.applyFilters(false, false);
	}

	$scope.anyBoesWithErrors = function () {
		return $scope.bulkAssignData.some(function (boe) { return boe.hasError; });
	}

	$scope.validateBulkAssign = function (displaySuccessMessage, displayErrorMessage) {
		$scope.errors = [];

		$scope.bulkAssignData.forEach(function (boe) {
			var hasAuthor = boe.Authors.length > 0 || boe.SubcontractorAuthors.length > 0;
			var hasApprover = boe.Approvers.length > 0;
			boe.hasError = false;

			if (boe.Status == "Unassigned") {
				if (!hasAuthor && hasApprover) {
					if (displayErrorMessage && boe.isDirty) {
						$scope.errors.push({ ValidationIssue: "Currently Unassigned BOE " + boe.WbsDisplayName + " | " + boe.ClinDisplayName + " is missing an Author. Remove the Approver(s) to keep it unassigned or add at least one Author." });
					}
					boe.hasError = true;
				} else if (hasAuthor && !hasApprover) {
					if (displayErrorMessage && boe.isDirty) {
						$scope.errors.push({ ValidationIssue: "Currently Unassigned BOE " + boe.WbsDisplayName + " | " + boe.ClinDisplayName + " is missing an Approver.  Remove the Author(s) to keep it unassigned or add at least one Approver." });
					}
					boe.hasError = true;
				}
			} else if (!hasAuthor && !hasApprover) {
				if (displayErrorMessage && boe.isDirty) {
					$scope.errors.push({ ValidationIssue: boe.WbsDisplayName + " | " + boe.ClinDisplayName + " is missing an Author and Approver. At least one Author and at least one Approver must be assigned." });
				}
				boe.hasError = true;
			} else if (!hasAuthor) {
				if (displayErrorMessage && boe.isDirty) {
					$scope.errors.push({ ValidationIssue: boe.WbsDisplayName + " | " + boe.ClinDisplayName + " is missing an Author. At least one Author must be assigned." });
				}
				boe.hasError = true;
			} else if (!hasApprover) {
				if (displayErrorMessage && boe.isDirty) {
					$scope.errors.push({ ValidationIssue: boe.WbsDisplayName + " | " + boe.ClinDisplayName + " is missing an Approver. At least one Approver must be assigned." });
				}
				boe.hasError = true;
			}

			if (hasAuthor && hasApprover && boe.Approvers.some(function (approver) { return boe.Authors.indexOf(approver) >= 0; })) {
				if (boe.isDirty) {
					$scope.errors.push({ ValidationIssue: boe.WbsDisplayName + " | " + boe.ClinDisplayName + " cannot have the same person as both an Author and Approver." });
				}
				boe.hasError = true;
			}

			// add "(error)" to the dropdown search string if has error, otherwise make sure it's cleared
			var dropdownBoe = $scope.bulkAssignBoes.find(b => b.id == boe.BoeID);
			if (dropdownBoe != undefined) {
				if (boe.hasError) {
					dropdownBoe.search = dropdownBoe.search.concat(" (error)");
				} else {
					dropdownBoe.search = dropdownBoe.search.replace(" (error)", "");
				}
			}
		});

		if (displaySuccessMessage && $scope.errors.length == 0) {
			RaiseNotification("Validation successful. There were no errors for updated rows.");
		}
	}

	$scope.setDirty = function () {
		$scope.isDirty = true;
		ManageBOEWidget.setDirty();
	}

	$scope.cleanDirty = function () {
		$scope.isDirty = false;
		ManageBOEWidget.cleanDirty();
	}

	$scope.saveBulkAssign = function () {
		$scope.validateBulkAssign(false, true);

		if (!$scope.bulkAssignData.some(x => x.hasError && x.isDirty)) {
			$('#PageLoading').removeClass('display-none');
			var boesToSave = [];
			$scope.bulkAssignData.forEach(function (boe) {
				if (boe.isDirty) {
					boesToSave.push({
						BoeID: boe.BoeID,
						State: boe.State,
						Authors: boe.Authors,
						Approvers: boe.Approvers,
						SubcontractorAuthors: boe.SubcontractorAuthors
					});
				}
			});

			if (boesToSave && boesToSave.length > 0) {
				var data = {};
				data.boeRolesToSave = boesToSave;

				$http({
					method: 'POST',
					url: CreatePostURL(ManageBOEModel.workspace, ManageBOEModel.controller, ManageBOEModel.bulkAssignRolesAction, ''),
					data: data
				}).then(function successCallback(response) {
					if (response.data.ErrorMessages.length > 0) {
						$scope.displayBulkAssignSaveErrors(response.data.ErrorMessages);
						$('#PageLoading').addClass('display-none');
					} else {
						// On success, return to Manage BOEs
						RaiseNotification("Save of Bulk Assign Roles was successful.");
						loadBOEs();
						$scope.continueCancelBulkAssign();
						$('#PageLoading').addClass('display-none');
					}
				}, function errorCallback(response) {
					$scope.displayBulkAssignSaveErrors(response.data.MessageList);
					$('#PageLoading').addClass('display-none');
				});
			} else {
				$('#PageLoading').addClass('display-none');
				RaiseNotification("No changes found to save.");
			}
		} 

		$scope.displayBulkAssignSaveErrors = function (errors) {
			if (errors != undefined) {
				$scope.errors = [];
				errors.forEach(function (error) {
					$scope.errors.push({ ValidationIssue: error });
				});
			}
		}
	}
}]);