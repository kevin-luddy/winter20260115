angular.module('genboe').controller('ManageCLINController', ['$scope', '$http', '$timeout', '$filter', '$cookies', 'ManageCLINModel', function ($scope, $http, $timeout, $filter, $cookies, ManageCLINModel) {
    $scope.columns = {
        clinNumber: 'ClinNumber',
        title: 'ClinTitle',
        contract: 'ContractTypeText',
        inUse: 'InUseText',
        start: 'StartDate',
        end: 'EndDate'
    };

    $scope.hideContractType = true;
    $scope.errors = [];
    $scope.modalErrors = [];
    $scope.gridModel = { containsOCI: true};
    $scope.deleteAll = false;
    $scope.selectAll = false;
    $scope.containsOCI = true;

    $scope.isExporting = false;
    // Manage CLIN can be done when workspace state is Initialization or Working
    $scope.isWorkingState = ManageCLINModel.workspaceState === 'Working' || ManageCLINModel.workspaceState === 'Initialization';
    $scope.isWorkspaceLocked = ManageCLINModel.workspaceState === 'Locked';

    $scope.ClinImportResult = {};
    
    $scope.dialog = {
        title: 'Import CLINs',
        open: false,
        file: null,
        importWorking: false,
        completeImportWorking: false,
        showImportResults: false,
        disableImport: true,
        invalidData: false,
        importResults: [],
        importExisting: null,  // true if importing existing CLINs, otherwise false
        showSteps: false, // steps 2 - 4 are hidden until step 1 is complete
        errors: []
    };

    $scope.adjustDatesDialog = {
        title: 'Update Dates',
        open: false,
        clins: [],
        errors: [],
        loading: false,
        hasBoes: false,
        hasDiscrete: false,
        step: 1,
        hideNext: false,
        hideFinish: true,
        isDirty: false,
        resourceFlowdown: 'NotSet',
        discreteFlowdown: 'NotSet',
        sendEmail: false
    };

    $scope.filter = {
        clinNumber: {}, title: {}, contract: {}, inUse: {}, start: {}, end: {},
        filterColumn: '',
        open: false
    };

    $scope.edit = {
        open: false,
        clin: {},
        isDirty: false,
        datePickersInitialized: false
    };

    $scope.data = [];
    $scope.isLoading = true;
    
    $scope.predicate = [$scope.columns.clinNumber];
    $scope.reverse = false;
    $scope.pageSize = 100;
    $scope.currentPage = 0;
    $scope.searchText = '';
    $scope.colSpan = 6;

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

    $scope.applyFilters = function (saveFilters) {
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
        $scope.toggleFilter(); 

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
        return selectedFilters.clinNumber.length > 0 || selectedFilters.title.length > 0
        || selectedFilters.inUse.length > 0 || selectedFilters.start.length > 0 || selectedFilters.end.length > 0
            || selectedFilters.contract.length > 0 || $scope.searchText.length > 0;
    };

    // reset all filters to be empty and unselected
    $scope.clearAllFilters = function (noClose) {
        selectedFilters.clinNumber = [];
        selectedFilters.contract = [];
        selectedFilters.title = [];
        selectedFilters.inUse = [];
        selectedFilters.start = [];
        selectedFilters.end = [];
        $scope.searchText = '';

        $scope.filter.clinNumber.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.title.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.contract.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.inUse.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.start.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.end.forEach(function (item) {
            item.checked = false;
        });
        
        $scope.currentPage = 0;
        if (!noClose) {
            $scope.filter.open = false;
        }

        $scope.SaveFilterToCookies();
    };

    $scope.filterCLINs = function (data) {
        var compareValue = $scope.searchText.toLowerCase();
        var foundName = false;

        // if top filter contains data, it MUST match
        if (data.ClinNumber) {
            if (selectedFilters.clinNumber.length > 0 && selectedFilters.clinNumber.indexOf(data.ClinNumber.toLowerCase()) === -1) {
                return false;
            }
        }

        if (data.ClinTitle) {
            if (selectedFilters.title.length > 0 && selectedFilters.title.indexOf(data.ClinTitle.toLowerCase()) === -1) {
                return false;
            }
        }

        if (data.ContractTypeText) {
            if (selectedFilters.contract.length > 0 && selectedFilters.contract.indexOf(data.ContractTypeText.toLowerCase()) === -1) {
                return false;
            }
        }

        if (data.InUseText != undefined) {
            if (selectedFilters.inUse.length > 0 && selectedFilters.inUse.indexOf(data.InUseText.toLowerCase()) === -1) {
                return false;
            }
        }

        if (selectedFilters.start.length > 0 && selectedFilters.start.indexOf(data.StartDate ? data.StartDate : '') === -1) {
            return false;
        }
        
        if (selectedFilters.end.length > 0 && selectedFilters.end.indexOf(data.EndDate ? data.EndDate : '') === -1) {
            return false;
        }

        // filter based on search term
        if (data.ClinNumber && data.ClinNumber.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.ClinTitle && data.ClinTitle.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.ContractTypeText && data.ContractTypeText.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.StartDate && data.StartDate.indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.EndDate && data.EndDate.indexOf(compareValue) !== -1) {
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

        $cookies.put('filtersClinNumber' + ManageCLINModel.workspace, angular.toJson($scope.filter.clinNumber), { expires: date });
        $cookies.put('filtersClinTitle' + ManageCLINModel.workspace, angular.toJson($scope.filter.title), { expires: date });
        $cookies.put('filtersInUse' + ManageCLINModel.workspace, angular.toJson($scope.filter.inUse), { expires: date });
        $cookies.put('filtersContract' + ManageCLINModel.workspace, angular.toJson($scope.filter.contract), { expires: date });
        $cookies.put('filtersStart' + ManageCLINModel.workspace, angular.toJson($scope.filter.start), { expires: date });
        $cookies.put('filtersEnd' + ManageCLINModel.workspace, angular.toJson($scope.filter.end), { expires: date });

        $cookies.put('searchTextClin' + ManageCLINModel.workspace, angular.toJson($scope.searchText), { expires: date });
        $cookies.put('predicateClin' + ManageCLINModel.workspace, angular.toJson($scope.predicate), { expires: date });
        $cookies.put('reverseClin' + ManageCLINModel.workspace, angular.toJson($scope.reverse), { expires: date });
    }

    // Load the filter settings
    $scope.LoadFilterFromCookies = function () {
        try {
            var temp;
            var cookies = $cookies.getAll();

            var cookieName = 'filtersClinNumber' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.clinNumber = temp;
                    $scope.filter.filterColumn = $scope.columns.clinNumber;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'filtersClinTitle' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.title = temp;
                    $scope.filter.filterColumn = $scope.columns.title;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'filtersInUse' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.inUse = temp;
                    $scope.filter.filterColumn = $scope.columns.inUse;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'filtersContract' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.contract = temp;
                    $scope.filter.filterColumn = $scope.columns.contract;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'filtersStart' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.start = temp;
                    $scope.filter.filterColumn = $scope.columns.start;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'filtersEnd' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.end = temp;
                    $scope.filter.filterColumn = $scope.columns.end;
                    $scope.applyFilters(false);
                }
            }
            
            cookieName = 'searchTextClin' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.searchText = temp;
                }
            }

            cookieName = 'predicateClin' + ManageCLINModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.predicate = temp;
                }
            }

            cookieName = 'reverseClin' + ManageCLINModel.workspace;
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

            $cookies.put('filtersClinNumber' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('filtersClinTitle' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('filtersInUse' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('filtersContract' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('filtersStart' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('filtersEnd' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('searchTextClin' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('predicateClin' + ManageCLINModel.workspace, null, { expires: date });
            $cookies.put('reverseClin' + ManageCLINModel.workspace, null, { expires: date });
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

            $scope.predicate = [sortValue];
        }

        $scope.SaveFilterToCookies();
    };

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
     * ***************** NOTE ******************
     * Functions below relate to the Date Adjust dialog
     * *****************************************
     */
    $scope.finishDateAdjust = function () {
        $scope.adjustDatesDialog.loading = true;

        var data = {};
        data.WorkspaceClinCollection = $scope.adjustDatesDialog.clins;
        data.SendEmailForBoeUpdates = $scope.adjustDatesDialog.sendEmail;
        data.DiscreteResourceSelection = $scope.adjustDatesDialog.discreteFlowdown;
        data.FlowdownUpdateSelection = $scope.adjustDatesDialog.resourceFlowdown

        data.WorkspaceClinCollection.forEach(function (clin) {
            clin.ClinStartDate = clin.StartDate;
            clin.ClinEndDate = clin.EndDate;
        });

        // finish adjust date metadata
        $http({
            method: 'POST',
            url: CreatePostURL(ManageCLINModel.workspace, ManageCLINModel.controller, ManageCLINModel.adjustDateAction, ''),
            data: data
        }).then(function successCallback(response) {
            $scope.adjustDatesDialog.loading = false;
            $scope.toggleAdjustDates();
            loadCLINs();
        }, function errorCallback(response) {
            $scope.adjustDatesDialog.loading = false;
            if (response && response.data && response.data.MessageList) {
                $scope.adjustDatesDialog.errors = response.data.MessageList;
            }
        });

    };

    $scope.adjustDateClin = function (clinId) {
        window.location = ManageCLINModel.dateShiftUrl + clinId + ManageCLINModel.clinLevel;
    }   

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
        $("#ImportCLINDialog-Form")[0].reset();
        $scope.dialog.disableImport = true;
        $scope.dialog.file = null;
    };

    $scope.importCLINs = function () {
        if (!$scope.dialog.disableImport) {
            // create form data
            var fd = new FormData();
            fd.append("file", $scope.dialog.file);

            // get url from form
            var url = $('#ImportCLINDialog-Form').attr('action');
            $scope.dialog.importWorking = true;
            $scope.dialog.errors = [];

            $http.post(url, fd, {
                headers: {
                    'Content-Type': undefined
                }
            }).then(function (response) {
                if (response != undefined && response.data != undefined) {
                    var results = response.data;
                    if (results.Status) {
                        $scope.dialog.importResults = results.Data;

                        $('#ImportResults ul[result]').html('').prev().removeClass('display-none');

                        for (var resultNdx = 0; resultNdx < results.Data.length; resultNdx++) {
                            var ImportTypes = [];
                            for (var resultTypeNdx = 0; resultTypeNdx < results.Data[resultNdx].ImportTypes.length; resultTypeNdx++) {
                                var ImportType = results.Data[resultNdx].ImportTypes[resultTypeNdx].toString();
                                switch (ImportType) {

                                    case '2': // ClinImportResult.MissingNumberOrTitle
                                        $('#ImportResults ul[result=' + ImportType + ']').append('<li>See import file for missing CLIN #\'s and/or CLIN Titles.</li>');
                                        break;

                                    default:
                                        $('#ImportResults ul[result=' + ImportType + ']').append('<li>' + results.Data[resultNdx].ClinNumber + ' ' + results.Data[resultNdx].ClinTitle + '</li>');
                                        break;
                                }

                                results.Data[resultNdx].Updateable = results.Data[resultNdx].Updateable.toString();
                                delete results.Data[resultNdx].StartDate;
                                delete results.Data[resultNdx].EndDate;
                                ImportTypes.push(ImportType.toString());
                            }

                            results.Data[resultNdx].ImportTypes = ImportTypes;
                        }

                        $('#ImportResults ul[result]').each(function () {
                            var numResults = $(this).children().length;
                            if (numResults == 0) {
                                $(this).prev().addClass('display-none');
                            } else {
                                $(this).prev().children('span').html(numResults + ' ');
                            }
                        });

                        $scope.dialog.showImportResults = true;
                    } else {
                        $scope.showImportError(results.Message);
                    }
                } else {
                    $scope.showImportError('A general import error occurred. Please check your import file for validity and try the import again.');
                }

                $scope.dialog.importWorking = false;
            }).catch(function () {
                $scope.showImportError('A general import error occurred. Please check your import file for validity and try the import again.');
                $scope.dialog.importWorking = false;
            });
        }
    };

    $scope.showImportError = function(message) {
        var error = {};
        error.Title = "Import CLIN Error";
        error.Message = message;
        error.Details = message;

        DisplayExceptionDialog(error);
    };

    $scope.completeImportCLINs = function () {
        $scope.dialog.completeImportWorking = true;
        var data = {};
        data.importResults = $scope.dialog.importResults;
        $http({
            method: 'POST',
            url: CreatePostURL(ManageCLINModel.workspace, ManageCLINModel.controller, ManageCLINModel.completeImportAction, ''),
            data: data
        }).then(function () {
            $scope.toggleImport();
            loadCLINs();
            RaiseNotification('Import Successful');
        }).catch(function () {
            $scope.toggleImport();
            loadCLINs();
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
    var selectedFilters = { clinNumber: [], title: [], contract: [], inUse: [], start: [], end: [] };

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
        if ($scope.filter.filterColumn === $scope.columns.clinNumber) {
            return $scope.filter.clinNumber;
        } else if ($scope.filter.filterColumn === $scope.columns.title) {
            return $scope.filter.title;
        } else if ($scope.filter.filterColumn === $scope.columns.contract) {
            return $scope.filter.contract;
        } else if ($scope.filter.filterColumn === $scope.columns.inUse) {
            return $scope.filter.inUse;
        } else if ($scope.filter.filterColumn === $scope.columns.start) {
            return $scope.filter.start;
        } else if ($scope.filter.filterColumn === $scope.columns.end) {
            return $scope.filter.end;
        }
    };

    var setWorkingFilterSet = function (currentSet) {
        if (angular.isDefined(currentSet)) {
            if ($scope.filter.filterColumn === $scope.columns.clinNumber) {
                selectedFilters.clinNumber = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.title) {
                selectedFilters.title = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.contract) {
                selectedFilters.contract = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.inUse) {
                selectedFilters.inUse = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.start) {
                selectedFilters.start = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.end) {
                selectedFilters.end = currentSet;
            }
        }
    };

    $scope.toggleSelectAll = function () {
        if ($scope.selectAll === false) {
            // unmark ALL
            $scope.data.forEach(function (item) {
                item.Selected = $scope.selectAll;
            });
        } else {
            // get all visible ClinIds
            var ids = $('input.delete-chck:not([disabled])').parents('tr').map(function () { return $(this).attr("pkid"); }).get();

            // mark items appropriately
            $scope.filteredResults.forEach(function (item) {
                item.Selected = $.inArray(item.ClinID + '', ids) !== -1;
            });
        }
    };

    $scope.disableSelected = function () {
        var disabled = true;
        $scope.data.forEach(function (item) {
            if (item.Selected) {
                disabled = false;
            }
        });

        return disabled;
    };

    $scope.delete = function () {
        var data = {};
        data.inDeletedClins = [];
        var inUseClins = [];

        $scope.data.forEach(function (item) {
            if (item.Selected) {
                if (item.InUse) {
                    inUseClins.push(item.ClinNumber);
                } else {
                    var clin = angular.copy(item);
                    clin.Deleted = true;
                    data.inDeletedClins.push(clin);
                }
            }
        });

        var confirmString = "";
        if (inUseClins.length > 0) {
            confirmString += '<div class="warning-box" style="display: block; text-align: left; min-height: 50px">';

            for (var i = 0; i < inUseClins.length; i++) {
                confirmString += '<div class="warning-message" style="padding-right: 10px;">Clin ';
                confirmString += inUseClins[i];
                confirmString += ' is in use and will not be deleted.</div>';
            }

            confirmString += '</div>';
        }

        confirmString += 'Are you sure you want to delete the selected CLIN(s)? ';

        Session.confirmDialog('Delete CLINs', confirmString, function () {
            $timeout(function () {
                $scope.checkForHiddenItems(data);
            });            
        });
    };

    $scope.continueDelete = function (data) {
        $scope.errors = [];

        $('#PageLoading').removeClass('display-none');
        $http({
            method: 'POST',
            url: CreatePostURL(ManageCLINModel.workspace, ManageCLINModel.controller, ManageCLINModel.deleteAction, ''),
            data: data
        }).then(function successCallback(response) {
            $scope.selectAll = false;
            loadCLINs();
            $('#PageLoading').addClass('display-none');
        }, function errorCallback(response) {
            $scope.errors = response.data.MessageList;
            $('#PageLoading').addClass('display-none');
        });
    };

    $scope.checkForHiddenItems = function (data) {
        // start and end indexes for items on page
        var start = $scope.currentPage * $scope.pageSize;
        var end = start + $scope.pageSize

        // get items currently displayed on page
        var currentPageItems = $scope.filteredResults.slice(start, end);

        // check if any items are selected, but not currently displayed
        var itemsOnOtherPages = data.inDeletedClins.some(function (s) {
            return currentPageItems.map(function (x) {
                return x.ClinID;
            }).indexOf(s.ClinID) < 0;
        });

        if (itemsOnOtherPages) {
            Session.confirmDialog('Hidden Items',
                'You have selected CLINs that are currently hidden from view. Would you like to continue? Select No to review these items.',
                function () {
                    $scope.continueDelete(data);
                });
        } else {
            $scope.continueDelete(data);
        }
    };

    $scope.save = function (saveAndAdd, showModalErrors) {
        if ($scope.edit.isDirty) {
            $('#PageLoading').removeClass('display-none');
            var data = {};

            $scope.edit.clin.Deleted = false;
            data.inUpdatedClin = $scope.edit.clin;
            if (showModalErrors) {
                $scope.modalErrors = [];
            } else {
                $scope.errors = [];
            }

            $http({
                method: 'POST',
                url: CreatePostURL(ManageCLINModel.workspace, ManageCLINModel.controller, ManageCLINModel.saveAction, ''),
                data: data
            }).then(function successCallback(response) {
                if (saveAndAdd) {
                    $scope.AddCLIN();
                } else {
                    $scope.edit.open = false;
                }

                loadCLINs();
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

    $scope.AddCLIN = function () {
        $scope.edit.clin = { ClinID: -1, ContractType: "-1" };
        $scope.edit.showStatus = 'Draft';
        if (!$scope.edit.datePickersInitialized) {
            // Setup the Datepickers
            SetupMonthPicker('#CLINElementDialog #StartDate');
            SetupMonthPicker('#CLINElementDialog #EndDate');
            $scope.edit.datePickersInitialized = true;
        }

        $scope.edit.open = true;
    };

    $scope.onEditClose = function () {
        $scope.edit.open = false;
        $scope.edit.isDirty = false;
    };

    $scope.export = function (isTemplate) {
        var timeoutTime = 2000;
        $scope.isExporting = true;

        var action = isTemplate ? ManageCLINModel.exportTemplateAction : ManageCLINModel.exportAction;

        var exportUrl = CreatePostURL(ManageCLINModel.workspace, ManageCLINModel.controller, action, '');
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

    $scope.editCLIN = function (clin) {
        $scope.edit.clin = angular.copy(clin);
        if ($scope.isWorkingState) {
            if ($scope.edit.clin.InUse) {
                // need to destroy the datepickers
                $("#CLINElementDialog #StartDate").datepicker("destroy");
                $("#CLINElementDialog #EndDate").datepicker("destroy");
                $scope.edit.datePickersInitialized = false;
            } else if (!$scope.edit.datePickersInitialized) {
                // Setup the Datepickers
                SetupMonthPicker('#CLINElementDialog #StartDate');
                SetupMonthPicker('#CLINElementDialog #EndDate');
                $scope.edit.datePickersInitialized = true;
            }
        }

        $scope.edit.open = true;
    };
    
    var loadCLINs = function () {
        $scope.isLoading = true;
        $scope.data = [];
        $scope.errors = [];

        return $http({
            method: 'POST',
            url: CreatePostURL(ManageCLINModel.workspace, ManageCLINModel.controller, ManageCLINModel.action, '')
        }).then(function (response) {

            $scope.gridModel = response.data;
            $scope.hideContractType = $scope.gridModel.HideContractType;

            if (!$scope.gridModel.HideContractType) {
                $scope.colSpan = 7;
            }

            var clinFilter = {};
            var clinTitleFilter = {};
            var contractFilter = {};
            var inUseFilter = {};
            var startFilter = {};
            var endFilter = {};

            response.data.ClinResults.forEach(function (item) {
                // build filter sets based on the data returned
                item.InUseText = item.InUse ? "Yes" : "No";
                clinFilter[item.ClinNumber] = { display: item.ClinNumber, value: item.ClinNumber, checked: false };
                clinTitleFilter[item.ClinTitle] = { display: item.ClinTitle, value: item.ClinTitle, checked: false };
                contractFilter[item.ContractTypeText] = { display: item.ContractTypeText, value: item.ContractTypeText, checked: false };
                inUseFilter[item.InUseText] = { display: item.InUseText, value: item.InUseText, checked: false };
                startFilter[item.StartDate] = { display: item.StartDate, value: item.StartDate, checked: false };
                endFilter[item.EndDate] = { display: item.EndDate, value: item.EndDate, checked: false };
                if (item.ContractType) {
                    item.ContractType = item.ContractType.toString();
                }

                delete item.UpdateDate;
            });

            // need to convert to arrays and sort the data
            $scope.filter.clinNumber = convertToArray(clinFilter);
            $scope.filter.title = convertToArray(clinTitleFilter);
            $scope.filter.contract = convertToArray(contractFilter);
            $scope.filter.inUse = convertToArray(inUseFilter);
            $scope.filter.start = convertToArray(startFilter);
            $scope.filter.end = convertToArray(endFilter);

            $scope.data = response.data.ClinResults;

            $scope.LoadFilterFromCookies();

            $scope.isLoading = false;            
            firstLoad = false;
            
        }, function errorCallback(response) {
            $scope.errors = response.data.MessageList;
            $scope.isLoading = false;
        });
    }    

    // load the main data
    loadCLINs();
}]);