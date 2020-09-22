angular.module('genboe').controller('ManageWBSController', ['$scope', '$http', '$timeout', '$filter', '$cookies', 'ManageWBSModel', function ($scope, $http, $timeout, $filter, $cookies, ManageWBSModel) {
    $scope.columns = {
        wbsNumber: 'WbsNumber',
        title: 'WbsTitle',
        clins: 'ClinNumbers'
    };
    $scope.errors = [];
    $scope.modalErrors = [];
    $scope.checkAllClicked = false;
    $scope.createBOEsDisabled = true;
    $scope.containsOCI = true;
    $scope.availableClins = [];
    $scope.isExporting = false;
    // Manage WBS can be done when workspace state is Initialization or Working
    $scope.isWorkingState = ManageWBSModel.workspaceState === 'Working' || ManageWBSModel.workspaceState === 'Initialization';
    $scope.isWorkspaceLocked = ManageWBSModel.workspaceState === 'Locked';

    $scope.dialog = {
        title: 'Import WBSs',
        open: false,
        file: null,
        importWorking: false,
        completeImportWorking: false,
        showImportResults: false,
        disableImport: true,
        invalidData: false,
        importResults: [],
        importExisting: null,  // true if importing existing WBSs, otherwise false
        showSteps: false // steps 2 - 4 are hidden until step 1 is complete
    };

    $scope.filter = {
        wbsNumber: {}, title: {}, clins: {},
        filterColumn: '',
        open: false
    };

    $scope.edit = {
        open: false,
        wbs: {},
        isDirty: false
    };

    $scope.data = [];
    $scope.isLoading = true;

    $scope.predicate = [$scope.columns.wbsNumber];
    $scope.reverse = false;
    $scope.pageSize = 100;
    $scope.currentPage = 0;
    $scope.searchText = '';
    $scope.colSpan = 5;

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
        return selectedFilters.wbsNumber.length > 0 || selectedFilters.title.length > 0
            || selectedFilters.clins.length > 0 || $scope.searchText.length > 0;
    };

    // reset all filters to be empty and unselected
    $scope.clearAllFilters = function (noClose) {
        selectedFilters.wbsNumber = [];
        selectedFilters.title = [];
        selectedFilters.clins = [];
        $scope.searchText = '';

        $scope.filter.wbsNumber.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.title.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.clins.forEach(function (item) {
            item.checked = false;
        });

        $scope.currentPage = 0;
        if (!noClose) {
            $scope.filter.open = false;
        }

        $scope.SaveFilterToCookies();
    };

    $scope.filterWBSs = function (data) {
        var compareValue = $scope.searchText.toLowerCase();
        var foundName = false;

        // if top filter contains data, it MUST match
        if (selectedFilters.wbsNumber.length > 0 && selectedFilters.wbsNumber.indexOf(data.WbsNumber.toLowerCase()) === -1) {
            return false;
        }

        if (selectedFilters.title.length > 0 && selectedFilters.title.indexOf(data.WbsTitle.toLowerCase()) === -1) {
            return false;
        }

        if (selectedFilters.clins.length > 0) {
            foundName = data.ClinIDs.some(function (item) {
                if (selectedFilters.clins.indexOf(item.toString()) !== -1) {
                    return true;
                }
            });
            if (!foundName) {
                return false;
            }
        }

        // filter based on search term
        if (data.WbsNumber && data.WbsNumber.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.WbsTitle && data.WbsTitle.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.ClinNumbers && data.ClinNumbers.toLowerCase().indexOf(compareValue) !== -1) {
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

        $cookies.put('filtersWbsNumber' + ManageWBSModel.workspace, angular.toJson($scope.filter.wbsNumber), { expires: date });
        $cookies.put('filtersWbsTitle' + ManageWBSModel.workspace, angular.toJson($scope.filter.title), { expires: date });
        $cookies.put('filtersClins' + ManageWBSModel.workspace, angular.toJson($scope.filter.clins), { expires: date });

        $cookies.put('searchTextWbs' + ManageWBSModel.workspace, angular.toJson($scope.searchText), { expires: date });
        $cookies.put('predicateWbs' + ManageWBSModel.workspace, angular.toJson($scope.predicate), { expires: date });
        $cookies.put('reverseWbs' + ManageWBSModel.workspace, angular.toJson($scope.reverse), { expires: date });
    };

    // Load the filter settings
    $scope.LoadFilterFromCookies = function () {
        try {
            var temp;
            var cookies = $cookies.getAll();

            var cookieName = 'filtersWbsNumber' + ManageWBSModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.wbsNumber = temp;
                    $scope.filter.filterColumn = $scope.columns.wbsNumber;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'filtersWbsTitle' + ManageWBSModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.title = temp;
                    $scope.filter.filterColumn = $scope.columns.title;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'filtersClins' + ManageWBSModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.filter.clins = temp;
                    $scope.filter.filterColumn = $scope.columns.clins;
                    $scope.applyFilters(false);
                }
            }

            cookieName = 'searchTextWbs' + ManageWBSModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.searchText = temp;
                }
            }

            cookieName = 'predicateWbs' + ManageWBSModel.workspace;
            if (cookieName in cookies) {
                temp = JSON.parse(cookies[cookieName]);
                if (temp != undefined) {
                    $scope.predicate = temp;
                }
            }

            cookieName = 'reverseWbs' + ManageWBSModel.workspace;
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

            $cookies.put('filtersWbsNumber' + ManageWBSModel.workspace, null, { expires: date });
            $cookies.put('filtersWbsTitle' + ManageWBSModel.workspace, null, { expires: date });
            $cookies.put('filtersClins' + ManageWBSModel.workspace, null, { expires: date });
            $cookies.put('searchTextWbs' + ManageWBSModel.workspace, null, { expires: date });
            $cookies.put('predicateWbs' + ManageWBSModel.workspace, null, { expires: date });
            $cookies.put('reverseWbs' + ManageWBSModel.workspace, null, { expires: date });
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
            if (sortValue === $scope.columns.wbsNumber) {
                $scope.predicate = [sortValue, $scope.columns.clins];
            } else if (sortValue === $scope.columns.clins) {
                $scope.predicate = [sortValue, $scope.columns.wbsNumber];
            } else {
                $scope.predicate = [sortValue];
            }
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
     * **************************** NOTE *****************************
     * Functions below relate to the logic of the dialog import
     * ***************************************************************
     */

    $scope.fileUploadChange = function (element) {
        $scope.$apply(function ($scope) {
            $scope.dialog.disableImport = element.value.endsWith('.xlsx') || element.value.endsWith('.xlsm') ? false : true;
            $scope.dialog.file = $scope.dialog.disableImport ? null : element.files[0];
        });
    };

    $scope.showImportSteps = function () {
        $scope.dialog.showSteps = true;
    };

    var resetUploadForm = function () {
        $("#ImportWBSDialog-Form")[0].reset();
        $scope.dialog.disableImport = true;
        $scope.dialog.file = null;
    };

    $scope.importWBSs = function () {
        if (!$scope.dialog.disableImport) {
            // create form data
            var fd = new FormData();
            fd.append("file", $scope.dialog.file);

            // get url from form
            var url = $('#ImportWBSDialog-Form').attr('action');
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
                    $scope.dialog.invalidData = window.WbsImportVerificationWidget.invalidData;
                    $scope.dialog.importResults = window.WbsImportVerificationWidget.data.importResults;
                }, 0);
            });
        }
    };

    $scope.completeImportWBSs = function () {
        $scope.dialog.completeImportWorking = true;

        var data = {};
        data.importResults = $scope.dialog.importResults;

        $http({
            method: 'POST',
            url: CreatePostURL(ManageWBSModel.workspace, ManageWBSModel.controller, ManageWBSModel.importAction, ''),
            data: data
        }).then(function () {
            $scope.toggleImport();
            loadWBSs();
            RaiseNotification('Import Successful');
        }).catch(function () {
            $scope.toggleImport();
            loadWBSs();
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
    var selectedFilters = { wbsNumber: [], title: [], clins: [] };

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
        if ($scope.filter.filterColumn === $scope.columns.wbsNumber) {
            return $scope.filter.wbsNumber;
        } else if ($scope.filter.filterColumn === $scope.columns.title) {
            return $scope.filter.title;
        } else if ($scope.filter.filterColumn === $scope.columns.clins) {
            return $scope.filter.clins;
        }
    };

    var setWorkingFilterSet = function (currentSet) {
        if (angular.isDefined(currentSet)) {
            if ($scope.filter.filterColumn === $scope.columns.wbsNumber) {
                selectedFilters.wbsNumber = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.title) {
                selectedFilters.title = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.clins) {
                selectedFilters.clins = currentSet;
            }
        }
    };

    $scope.delete = function (wbs) {
        Session.confirmDialog('Delete WBS', 'Are you sure you want to delete WBS ' + wbs.WbsNumber + ' ' + wbs.WbsTitle + '?', function () {
            var data = {};
            data.wbsCollection = [];
            wbs.Deleted = true;
            data.wbsCollection.push(wbs);
            $scope.errors = [];

            $('#PageLoading').removeClass('display-none');
            $http({
                method: 'POST',
                url: CreatePostURL(ManageWBSModel.workspace, ManageWBSModel.controller, ManageWBSModel.saveAction, ''),
                data: data
            }).then(function successCallback(response) {
                loadWBSs();
                $('#PageLoading').addClass('display-none');
            }, function errorCallback(response) {
                $scope.errors = response.data.MessageList;
                $('#PageLoading').addClass('display-none');
            });
        });
    };

    $scope.deleteAll = function () {
        var data = {};
        data.wbsCollection = [];

        // get all visible Ids
        var ids = $('input.delete-chck:not([disabled])').parents('tr').map(function () { return $(this).attr("pkid"); }).get();

        // mark items appropriately
        $scope.filteredResults.forEach(function (item) {
            var deleted = !item.InUse && $.inArray(item.WbsID + '', ids) !== -1;
            if (deleted) {
                item.Deleted = true;
                data.wbsCollection.push(item);
            }
        });

        if (data.wbsCollection.length > 0) {
            GenSession.confirmDialog('Delete All visible WBS Elements Not In Use', 'Are you sure you want to delete all visible WBS elements that are not in use?', function () {
                $timeout(function () {
                    $scope.checkForHiddenItems(data, $scope.continueDelete);
                }, 50);

            });
        } else {
            GenSession.alertDialog('No WBS Elements to Delete', 'All WBS elements are in use and cannot be deleted.');
        }
    };

    $scope.continueDelete = function (data) {
        $scope.errors = []
        $('#PageLoading').removeClass('display-none');
        $http({
            method: 'POST',
            url: CreatePostURL(ManageWBSModel.workspace, ManageWBSModel.controller, ManageWBSModel.saveAction, ''),
            data: data
        }).then(function successCallback(response) {
            loadWBSs();
            $('#PageLoading').addClass('display-none');
        }, function errorCallback(response) {
            $scope.errors = response.data.MessageList;
            $('#PageLoading').addClass('display-none');
        });
    }

    $scope.checkForHiddenItems = function (data, callbackFunction) {
        // start and end indexes for items on page
        var start = $scope.currentPage * $scope.pageSize;
        var end = start + $scope.pageSize

        // get items currently displayed on page
        var currentPageItems = $scope.filteredResults.slice(start, end);

        // check if any items are selected, but not currently displayed
        var itemsOnOtherPages = false;

        if (typeof data.wbsCollection != 'undefined') {
            itemsOnOtherPages = data.wbsCollection.some(function (s) {
                return currentPageItems.indexOf(s) < 0;
            });
        } else if (typeof data.selectedWbsIDs != 'undefined') {
            itemsOnOtherPages = data.selectedWbsIDs.some(function (s) {
                return currentPageItems.map(function (x) {
                    return x.WbsID.toString();
                }).indexOf(s) < 0;
            });
        }

        if (itemsOnOtherPages) {
            Session.confirmDialog('Hidden Items',
                'There are WBS elements that are currently hidden from view that will be affected by this action. Would you like to continue? Select No to review these items.',
                function () {
                    callbackFunction(data);
                });
        } else {
            callbackFunction(data);
        }
    }

    $scope.save = function (saveAndAdd) {
        if ($scope.edit.isDirty) {
            var savingBOEs = false;
            var totalWbsWithAssociatedBoes = 0;
            if ($scope.edit.wbs.ClinsInUse != undefined)
            {
                totalWbsWithAssociatedBoes = $scope.edit.wbs.ClinsInUse.length;
            }

            if ($scope.edit.wbs.HasBOE && totalWbsWithAssociatedBoes == 0) {   // BOEs exists, but none of them are mapped to a CLIN
                GenSession.alertDialog('BOE Previously Created With No CLIN', 'A BOE was previously created with no CLIN. A new BOE will be created for each selected CLIN. The BOE previously created with no CLIN will not be affected. The BOEs can be edited on the Manage BOEs page.');
            } else if (totalWbsWithAssociatedBoes > 0 && $scope.edit.wbs.ClinIDs.length > totalWbsWithAssociatedBoes) {  // there are existing BOEs AND also new CLIN selections
                // business logic dictates that in this scenario, a new BOE will be created for each new CLIN selected
                savingBOEs = true;
            }
            $('#PageLoading').removeClass('display-none');
            var data = {};
            
            data.wbsCollection = [$scope.edit.wbs];
            $scope.modalErrors = [];

            $http({
                method: 'POST',
                url: CreatePostURL(ManageWBSModel.workspace, ManageWBSModel.controller, ManageWBSModel.saveAction, ''),
                data: data
            }).then(function successCallback(response) {
                if (savingBOEs) {
                    RaiseNotification('BOE created for selected WBS');
                }

                if (saveAndAdd) {
                    $scope.AddWbs();
                } else {
                    $scope.edit.open = false;
                }

                loadWBSs();
                $('#PageLoading').addClass('display-none');
            }, function errorCallback(response) {
                $scope.modalErrors = response.data.MessageList;
                $('#PageLoading').addClass('display-none');
            });
        }
    };

    $scope.AddWbs = function () {
        $scope.edit.wbs = { Id: -1 };
        $scope.edit.open = true;
    };

    $scope.onEditClose = function () {
        $scope.edit.open = false;
        $scope.edit.isDirty = false;
    };

    $scope.export = function (isTemplate) {
        var timeoutTime = 2000;
        $scope.isExporting = true;

        var action = isTemplate ? ManageWBSModel.exportTemplateAction : ManageWBSModel.exportAction;

        var exportUrl = CreatePostURL(ManageWBSModel.workspace, ManageWBSModel.controller, action, '');
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

    $scope.editWbs = function (wbs) {
        $scope.edit.wbs = angular.copy(wbs);
        $scope.edit.open = true;
    };

    $scope.clinInUse = function (clinID) {
        var inUse = false;
        if ($scope.edit.wbs.ClinsInUse != undefined) {
            inUse = $scope.edit.wbs.ClinsInUse.includes(clinID);
        }

        return inUse;
    };

    $scope.wbsCheckboxModified = function() {
        var numChecked = 0;
        $scope.data.forEach(function (d) {
            // only count those selected by the user, not those checked and disabled for having a BOE
            if (d.Selected && !d.Disabled && !d.InUse) {
                numChecked++;
            }
        });

        if (numChecked > 0) {
            $scope.createBOEsDisabled = false;
        }
        else {
            $scope.createBOEsDisabled = true;
        }
    };

    $scope.DisableParents = function (selectedWbs) {
        // find the wbs # for all checked rows
        var checkedWbsNumbers = [];
        $scope.data.forEach(function (d) {
            if (d.Selected && d.HasBOE) {
                checkedWbsNumbers.push(d.WbsNumber);
            }

            // enable checkbox if it or parent doesn't have BOE
            if ((!d.HasBOE && !d.ParentOrChildHasBoe) && !$scope.checkAllClicked) {
                d.Disabled = false;
            }
        });

        if (selectedWbs != null && selectedWbs.Selected) {
            checkedWbsNumbers.push(selectedWbs.WbsNumber);
        }
        
        // foreach wbs #
        for (index in checkedWbsNumbers) {
            // find its parents            
            var parents = [];
            var parentString = checkedWbsNumbers[index] + "";
            while (parentString.indexOf('.') > 0) {
                parentString = parentString.substring(0, parentString.lastIndexOf('.'));
                parents.push(parentString);
            }

            $scope.data.forEach(function (d) {
                // disable parents
                if ($.inArray(d.WbsNumber, parents) >= 0) {
                    d.Disabled = true;
                }

                // disable children
                if (d.WbsNumber.indexOf(checkedWbsNumbers[index] + '.') == 0 && d.WbsNumber != checkedWbsNumbers[index]) {
                    d.Disabled = true;
                }
            });
        }
    }

    $scope.checkAllWBSClicked = function () {
        if ($scope.checkAllClicked === false) {
            // uncheck all
            $scope.data.forEach(function (item) {
                if (!item.Disabled) {
                    item.Selected = $scope.checkAllClicked;

                    // this stops children from being checked if the parent is checked #badname
                    $scope.DisableParents(item);
                }
            });
        } else {
            // get all visible Ids
            var ids = $('input.delete-chck:not([disabled])').parents('tr').map(function () { return $(this).attr("pkid"); }).get();

            // mark items appropriately
            $scope.filteredResults.forEach(function (item) {
                var selected = !item.Disabled && $.inArray(item.WbsID + '', ids) !== -1;
                if (selected) {
                    item.Selected = true;

                    // this stops children from being checked if the parent is checked #badname
                    $scope.DisableParents(item);
                }
            });
        }

        // Checkbox was clicked, fire event.
        $scope.wbsCheckboxModified();
    };
    
    $scope.createBOEToggle = function (e, wbs) {
        e.stopPropagation();
        $scope.DisableParents(wbs);
        // Checkbox was clicked, fire event.
        $scope.wbsCheckboxModified();
    };

    $scope.createBOEs = function () {
        if (!$scope.createBOEsDisabled) {
            var data = {};
            data.selectedWbsIDs = [];
            $scope.data.forEach(function (d) {
                if (d.Selected != undefined && d.Selected && !d.Disabled && !d.HasBOE && !d.ParentOrChildHasBoe) {
                    data.selectedWbsIDs.push(d.WbsID.toString());
                }
            });

            $scope.errors = [];            

            $timeout(function () {
                $scope.checkForHiddenItems(data, $scope.continueCreateBoes);  
            }, 50);
        }
    };

    $scope.continueCreateBoes = function (data) {
        var dataToSend = JSON.stringify(data);
        $('#PageLoading').removeClass('display-none');
        $http({
            method: 'POST',
            url: CreatePostURL(ManageWBSModel.workspace,
                ManageWBSModel.controller,
                ManageWBSModel.createBOEsAction, ''),
            data: dataToSend
        }).then(function (response) {
            GenSession.RaiseNotification('BOE created for selected WBS');
            $scope.createBOEsDisabled = true;
            loadWBSs();
            $('#PageLoading').addClass('display-none');
        }, function errorCallback(response) {
            $scope.errors = response.data.MessageList;
            $('#PageLoading').addClass('display-none');
        });
    }

    var loadWBSs = function () {
        $scope.isLoading = true;
        $scope.data = [];
        $scope.availableClins = [];
        $scope.errors = [];

        return $http({
            method: 'POST',
            url: CreatePostURL(ManageWBSModel.workspace, ManageWBSModel.controller, ManageWBSModel.action, '')
        }).then(function (response) {

            $scope.checkAllClicked = false;
            var wbsFilter = {};
            var wbsTitleFilter = {};
            var clinFilter = {};
            response.data.WbsResults.forEach(function (item) {
                // build filter sets based on the data returned
                wbsFilter[item.WbsNumber] = { display: item.WbsNumber, value: item.WbsNumber, checked: false };
                wbsTitleFilter[item.WbsTitle] = { display: item.WbsTitle, value: item.WbsTitle, checked: false };
                delete item.UpdateDate;
            });

            response.data.AvailableClins.forEach(function (item){
                // build filter sets based on the data returned
                clinFilter[item.ClinID] = { display: item.ClinString, value: item.ClinID.toString(), checked: false };
            });

            // need to convert to arrays and sort the data
            $scope.filter.wbsNumber = convertToArray(wbsFilter);
            $scope.filter.title = convertToArray(wbsTitleFilter);
            $scope.filter.clins = convertToArray(clinFilter);

            $scope.data = response.data.WbsResults;
            // select and disable WBS that have BOEs, make sure the rest are enabled and deselected
            $scope.data.forEach(function (d) {
                if (d.HasBOE || d.InUse || d.ParentOrChildHasBoe) {
                    d.Selected = false;
                    d.Disabled = true;
                } else {
                    d.Selected = false;
                    d.Disabled = false;
                }
            });

            $scope.availableClins = response.data.AvailableClins;
            $scope.containsOCI = response.data.ContainsOCI;
            $scope.createBOEsDisabled = true;
            
            $scope.LoadFilterFromCookies();

            $scope.isLoading = false;
            firstLoad = false;

            $timeout(function () {
                $scope.DisableParents();
            }, 500);
            
        }, function errorCallback(response) {
            $scope.errors = response.data.MessageList;
            $scope.isLoading = false;
        });
    }    

    // load the main data then load the valid BOEs for the import/export dialog
    loadWBSs();
}]);