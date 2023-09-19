angular.module('genboe').controller('WorkspaceHomeController', ['$scope', '$http', '$timeout', '$filter','$cookies', 'WorkspaceHomeModel', function ($scope, $http, $timeout, $filter, $cookies, WorkspaceHomeModel) {
    $scope.columns = {
        wbs: 'WBSText',
        boe: 'BOETitle',
        clin: 'CLINText',
        material: 'isMaterial',
        authors: 'AuthorOrderName',
        approvers: 'ApproverOrderName',
        status: 'Status',
        updateDate: 'UpdateDate'
    };

    $scope.dialog = {
        title: 'Import/Export BOEs',
        open: false,
        validBOEs: [],
        file: null,
        importWorking: false,
        completeImportWorking: false,
        showImportResults: false,
        disableImport: true,
        invalidData: false,
        importResults: [],
        importExport: '',
        exportValue: 'Export',
        importValue: 'Import',
        exportSelect: '',
        exportAllValue: 'ExportAllBoes',
        exportSelectValue: 'SelectBoesToExport'
    };

    $scope.filter = {
        wbs: {}, boe: {}, clin: {}, author: {}, approver: {}, status: {},
        filterColumn: '',
        open: false
    };

    $scope.data = [];
    $scope.isLoading = true;
    $scope.isReadOnly = false;
    $scope.isWorkingState = WorkspaceHomeModel.workspaceState === 'Working';
    $scope.isWorkspaceLocked = WorkspaceHomeModel.workspaceState === 'Locked';

    // custom sorting for wbs
    $scope.predicate = [$scope.columns.wbs, $scope.columns.clin, $scope.columns.authors];
    $scope.reverse = false;
    $scope.pageSize = 100;
    $scope.currentPage = 0;
    $scope.searchText = '';
    $scope.colSpan = 6;

    // locked workspace adds an extra column so we need to increase colSpan
    if ($scope.isWorkspaceLocked) {
        $scope.colSpan++;
    }

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
        return selectedFilters.wbs.length > 0 || selectedFilters.boe.length > 0
            || selectedFilters.clin.length > 0 || selectedFilters.author.length > 0
            || selectedFilters.approver.length > 0 || selectedFilters.status.length > 0 || $scope.searchText.length > 0;
    };

    // reset all filters to be empty and unselected
    $scope.clearAllFilters = function (noClose) {
        selectedFilters.wbs = [];
        selectedFilters.boe = [];
        selectedFilters.clin = [];
        selectedFilters.author = [];
        selectedFilters.approver = [];
        selectedFilters.status = [];
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

    $scope.showWaiting = function () {
        // written as lowercase because that's how the filter function expects it
        var statusType = "awaiting approval";

        $scope.clearAllFilters(true);
        selectedFilters.status.push(statusType);

        // search for the status typer and set it to checked
        $scope.filter.status.some(function (item) {
            if (item.value === statusType) {
                item.checked = true;
                return true;
            }
        });

        $scope.SaveFilterToCookies();
    };

    $scope.sortRecent = function () {
        $scope.predicate = [$scope.columns.updateDate, $scope.columns.wbs];
        $scope.reverse = true;

        $scope.SaveFilterToCookies();
    };

    $scope.filterBOEs = function (data) {
        var compareValue = $scope.searchText.toLowerCase();
        var foundName = false;

        // if top filter contains data, it MUST match
        if (selectedFilters.wbs.length > 0 && selectedFilters.wbs.indexOf(data.WBSText.toLowerCase()) === -1) {
            return false;
        }

        if (selectedFilters.boe.length > 0 && selectedFilters.boe.indexOf(data.BOETitle.toLowerCase()) === -1) {
            return false;
        }

        if (selectedFilters.clin.length > 0 && selectedFilters.clin.indexOf(data.CLINText.toLowerCase()) === -1) {
            return false;
        }

        if (selectedFilters.status.length > 0 && selectedFilters.status.indexOf(data.Status.toLowerCase()) === -1) {
            return false;
        }

        if (selectedFilters.author.length > 0) {
            foundName = data.AuthorsName.some(function (item) {
                if (selectedFilters.author.indexOf(item.toLowerCase()) !== -1) {
                    return true;
                }
            });
            if (!foundName) {
                return false;
            }
        }

        if (selectedFilters.approver.length > 0) {
            foundName = data.Approvers.some(function (item) {
                if (selectedFilters.approver.indexOf(item.DisplayName.toLowerCase()) !== -1) {
                    return true;
                }
            });
            if (!foundName) {
                return false;
            }
        }
        
        // filter based on search term
        if (data.WBSText.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.BOETitle.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.CLINText.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        if (data.isMaterial.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }

        var foundName = data.AuthorsName.some(function (item) {
            if (item.toLowerCase().indexOf(compareValue) !== -1) {
                return true;
            }
        });
        if (foundName) {
            return true;
        }

        foundName = data.Approvers.some(function (item) {
            if (item.DisplayName.toLowerCase().indexOf(compareValue) !== -1) {
                return true;
            }
        });
        if (foundName) {
            return true;
        }

        if (data.Status.toLowerCase().indexOf(compareValue) !== -1) {
            return true;
        }


        // nothing matches so return false
        return false;
    };

    // Save the filter settings
    $scope.SaveFilterToCookies = function() {
        var date = new Date();
        var minutes = 240;
        date.setTime(date.getTime() + (minutes * 60 * 1000));

        $cookies.put('wsForCookies', angular.toJson(WorkspaceHomeModel.workspace), { expires: date });

        $cookies.put('filtersWbs', angular.toJson($scope.filter.wbs), { expires: date });
        $cookies.put('filtersBoe', angular.toJson($scope.filter.boe), { expires: date });
        $cookies.put('filtersClin', angular.toJson($scope.filter.clin), { expires: date });
        $cookies.put('filtersAuthor', angular.toJson($scope.filter.author), { expires: date });
        $cookies.put('filtersApprover', angular.toJson($scope.filter.approver), { expires: date });
        $cookies.put('filtersStatus', angular.toJson($scope.filter.status), { expires: date });

        $cookies.put('searchTextBoes', angular.toJson($scope.searchText), { expires: date });
        $cookies.put('predicateBoes', angular.toJson($scope.predicate), { expires: date });
        $cookies.put('reverseBoes', angular.toJson($scope.reverse), { expires: date });
    }

    // Load the filter settings
    $scope.LoadFilterFromCookies = function () {
        try {
            var temp;
            var cookies = $cookies.getAll();
            var wsForCookies = '';

            if ('wsForCookies' in cookies) {
                temp = JSON.parse(cookies['wsForCookies']);
                if (temp != undefined) {
                    wsForCookies = temp;
                }
            }

            if(WorkspaceHomeModel.workspace === wsForCookies) {
                if ('filtersWbs' in cookies) {
                    temp = JSON.parse(cookies['filtersWbs']);
                    if (temp != undefined) {
                        $scope.filter.wbs = temp;
                        $scope.filter.filterColumn = $scope.columns.wbs;
                        $scope.applyFilters(false);
                    }
                }
                if ('filtersBoe' in cookies) {
                    temp = JSON.parse(cookies['filtersBoe']);
                    if (temp != undefined) {
                        $scope.filter.boe = temp;
                        $scope.filter.filterColumn = $scope.columns.boe;
                        $scope.applyFilters(false);
                    }
                }
                if ('filtersClin' in cookies) {
                    temp = JSON.parse(cookies['filtersClin']);
                    if (temp != undefined) {
                        $scope.filter.clin = temp;
                        $scope.filter.filterColumn = $scope.columns.clin;
                        $scope.applyFilters(false);
                    }
                }
                if ('filtersAuthor' in cookies) {
                    temp = JSON.parse(cookies['filtersAuthor']);
                    if (temp != undefined) {
                        $scope.filter.author = temp;
                        $scope.filter.filterColumn = $scope.columns.authors;
                        $scope.applyFilters(false);
                    }
                }
                if ('filtersApprover' in cookies) {
                    temp = JSON.parse(cookies['filtersApprover']);
                    if (temp != undefined) {
                        $scope.filter.approver = temp;
                        $scope.filter.filterColumn = $scope.columns.approvers;
                        $scope.applyFilters(false);
                    }
                }
                if ('filtersStatus' in cookies) {
                    temp = JSON.parse(cookies['filtersStatus']);
                    if (temp != undefined) {
                        $scope.filter.status = temp;
                        $scope.filter.filterColumn = $scope.columns.status;
                        $scope.applyFilters(false);
                    }
                }

                if ('searchTextBoes' in cookies) {
                    temp = JSON.parse(cookies['searchTextBoes']);
                    if (temp != undefined) {
                        $scope.searchText = temp;
                    }
                }
                if ('predicateBoes' in cookies) {
                    temp = JSON.parse(cookies['predicateBoes']);
                    if (temp != undefined) {
                        $scope.predicate = temp;
                    }
                }
                if ('reverseBoes' in cookies) {
                    temp = JSON.parse(cookies['reverseBoes']);
                    if (temp != undefined) {
                        $scope.reverse = temp;
                    }
                }
            }
        } catch(ex) {
            console.log('Bad cookie load');
            console.log(ex);

            var date = new Date();
            var minutes = -1;
            date.setTime(date.getTime() + (minutes * 60 * 1000));

            $cookies.put('wsForCookies', null, { expires: date });
            $cookies.put('filtersWbs', null, { expires: date });
            $cookies.put('filtersBoe', null, { expires: date });
            $cookies.put('filtersClin', null, { expires: date });
            $cookies.put('filtersAuthor', null, { expires: date });
            $cookies.put('filtersApprover', null, { expires: date });
            $cookies.put('filtersStatus', null, { expires: date });
            $cookies.put('searchTextBoes', null, { expires: date });
            $cookies.put('predicateBoes', null, { expires: date });
            $cookies.put('reverseBoes', null, { expires: date });
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

    $scope.toggleImportExport = function () {
        if ($scope.isWorkingState) {
            $scope.dialog.importExport = '';
            $scope.dialog.exportSelect = '';
            $scope.dialog.disableImport = true;
            $scope.dialog.importWorking = false;
            $scope.dialog.showImportResults = false;
            $scope.dialog.completeImportWorking = false;
            $scope.dialog.open = !$scope.dialog.open;

            // clear file input field
            resetUploadForm();
        }
    };

    $scope.getApproverStatus = function (boeState, approver) {
        if (boeState == WorkspaceHomeModel.awaitingState) {
            if (angular.isDefined(approver.ApproverResponded) && approver.ApproverResponded !== null) {
                return approver.ApproverResponse == WorkspaceHomeModel.approvedApprover ? "approve-accept" : "approver-reject";
            } else {
                return "approve-reject";
            }
        } else {
            return "";
        }
    };

    /*
     * **************************** NOTE *****************************
     * Functions below relate to the logic of the dialog import/export
     * ***************************************************************
     */
    $scope.disableExportButton = function () {
        // if we are exporting all then don't disable the button
        if ($scope.dialog.exportSelect === $scope.dialog.exportAllValue) {
            return false;
        }

        // if we find at least 1 item checked, don't disable the button
        var foundSelectedBOEs = $scope.dialog.validBOEs.some(function (item) {
            if (item.checked) {
                return true;
            }
        });

        return !foundSelectedBOEs;
    };

    $scope.disableValidBOE = function (item) {
        return angular.isDefined(item) && (item.isMaterial || item.state != WorkspaceHomeModel.draftState);
    };

    $scope.fileUploadChange = function (element) {
        $scope.$apply(function($scope) {
            $scope.dialog.disableImport = element.value.endsWith('.xlsm') ? false : true;
            $scope.dialog.file = $scope.dialog.disableImport ? null : element.files[0];
        });
    };

    $scope.exportBOEs = function () {
        // remove old iframe
        $('#DownloadTarget-ExportBOE').remove();

        // export all boes
        if ($scope.dialog.exportSelect === $scope.dialog.exportAllValue) {
            // Create a new hidden iFrame and set it's source to the chosen report's URL
            var targetIFrame = $('<iframe />', {
                'id': 'DownloadTarget-ExportBOE',
                'class': 'display-none',
                'src': CreatePostURL(WorkspaceHomeModel.workspace, WorkspaceHomeModel.controller, WorkspaceHomeModel.exportAllAction, '')
            });
            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            targetIFrame.appendTo('body');
        } else if ($scope.dialog.exportSelect === $scope.dialog.exportSelectValue) {
            // gather up selected BOEs
            var selectedBoeIDs = [];
            $scope.dialog.validBOEs.forEach(function (item) {
                if (item.checked) {
                    selectedBoeIDs.push(item.BoeID);
                }
            });

            // Create a new hidden iFrame and set it's source to the chosen report's URL
            var targetIFrame = $('<iframe />', {
                'id': 'DownloadTarget-ExportBOE',
                'class': 'display-none',
                'src': CreatePostURL(WorkspaceHomeModel.workspace, WorkspaceHomeModel.controller, WorkspaceHomeModel.exportSelectAction, selectedBoeIDs)
            });
            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            targetIFrame.appendTo('body');
        }

        $scope.toggleImportExport();
    };

    $scope.importBOEs = function () {
        if (!$scope.dialog.disableImport) {
            // create form data
            var fd = new FormData();
            fd.append("file", $scope.dialog.file);

            // get url from form
            var url = $('#ImportExportBoesDialog-Form').attr('action');
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
                    $scope.dialog.invalidData = window.WorkspaceHomeWorkofflineImportVerificationWidget.invalidData;
                    $scope.dialog.importResults = window.WorkspaceHomeWorkofflineImportVerificationWidget.importResults;
                }, 0);
            });
        }
    };

    $scope.completeImportBOEs = function () {
        $scope.dialog.completeImportWorking = true;

        $http({
            method: 'POST',
            url: CreatePostURL(WorkspaceHomeModel.workspace, WorkspaceHomeModel.controller, WorkspaceHomeModel.importAction, ''),
            data: JSON.stringify($scope.dialog.importResults)
        }).then(function () {
            $scope.toggleImportExport();
            loadBOEs();
            RaiseNotification('Import Successful');
        }).catch(function () {
            $scope.toggleImportExport();
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
    var selectedFilters = { wbs: [], boe: [], clin: [], author: [], approver: [], status: [] };

    var resetUploadForm = function () {
        $("#ImportExportBoesDialog-Form")[0].reset();
        $scope.dialog.disableImport = true;
        $scope.dialog.file = null;
    };

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
            }
        }
    };

    var loadBOEs = function () {
        $scope.isLoading = true;
        $scope.data = [];

        return $http({
            method: 'POST',
            url: CreatePostURL(WorkspaceHomeModel.workspace, WorkspaceHomeModel.controller, WorkspaceHomeModel.action, '')
        }).then(function (response) {
            currentUserName = response.data.CurrentUserDisplayName;

            console.log(response.data);
            $scope.isReadOnly = response.data.isReadOnly;
            response.data.items.forEach(function (item) {
                item.url = CreatePostURL(WorkspaceHomeModel.workspace, WorkspaceHomeModel.boeController, WorkspaceHomeModel.editAction, 'boe/') + item.BOEID;

                // build filter sets based on the data returned
                $scope.filter.wbs[item.WBSText] = { display: item.WBSText, value: item.WBSText, checked: false };
                $scope.filter.boe[item.BOETitle] = { display: item.BOETitle, value: item.BOETitle, checked: false };
                $scope.filter.clin[item.CLINText] = { display: item.CLINText, value: item.CLINText, checked: false };
                $scope.filter.status[item.Status] = { display: item.Status, value: item.Status, checked: false };
                item.AuthorsName.forEach(function (name) {
                    $scope.filter.author[name] = { display: name, value: name, checked: false };
                });
                item.Approvers.forEach(function (approver) {
                    $scope.filter.approver[approver.DisplayName] = { display: approver.DisplayName, value: approver.DisplayName, checked: false };
                });
            });

            // need to convert to arrays and sort the data
            $scope.filter.wbs = convertToArray($scope.filter.wbs);
            $scope.filter.boe = convertToArray($scope.filter.boe);
            $scope.filter.clin = convertToArray($scope.filter.clin);
            $scope.filter.author = convertToArray($scope.filter.author);
            $scope.filter.approver = convertToArray($scope.filter.approver);
            $scope.filter.status = convertToArray($scope.filter.status);

            $scope.data = response.data.items;
            $scope.LoadFilterFromCookies();

            $scope.isLoading = false;            
            firstLoad = false;
        });
    }    

    var loadValidBOEs = function () {
        $scope.dialog.validBOEs = [];

        return $http({
            method: 'POST',
            url: CreatePostURL(WorkspaceHomeModel.workspace, WorkspaceHomeModel.controller, WorkspaceHomeModel.loadValidBOEsAction, '')
        }).then(function (response) {
            $scope.dialog.validBOEs = response.data;
        });
    };

    // load the main data then load the valid BOEs for the import/export dialog
    loadBOEs().then(loadValidBOEs);
}]);