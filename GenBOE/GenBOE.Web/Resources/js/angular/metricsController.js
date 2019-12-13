angular.module('genboe').controller('metricsCtrl', ['$scope', '$http', '$timeout', '$filter', '$cookies', 'HomePageModel', function ($scope, $http, $timeout, $filter, $cookies, HomePageModel) {

    $scope.canCreateWS = false;
    $scope.isSysAdminBool = false;
    $scope.showFavoritesOnly = false;
    $scope.hideTracking = HomePageModel.hideTracking;
    $scope.isPtmIntegrated = HomePageModel.isPtmIntegrated;

    $scope.columns = {
        name: 'WorkspaceName',
        trackingNumber: 'TrackingNumber',
        submittalDate: 'ProposalSubmittalDate',
        state: 'WorkspaceState',
        favorite: 'IsFavorite',
        lead: 'CostVolumeLeadPricer',
        lob: 'LineOfBusiness',
        accessDate: 'LastAccessedDate'
    };

    $scope.filter = {
        lob: {}, state: {}, lead: {},
        filterColumn: '',
        open: false
    };

    $scope.restoreDialog = {
        open: false,
        errors: [],
        leadNtId: '',
        leadName: '',
        trackingNumber: '',
        potentialTrackingNumbers: [],
        loadingTrackingNumbers: false,
        ws: null
    };

    $scope.data = [];
    $scope.isLoading = true;

    var defaultPredicate = ['-IsFavorite', '!la', '-la', 'WorkspaceName']; // !la and -la deals with last accessed date, the !la is needed to put the null values on the bottom

    $scope.predicate = defaultPredicate;
    $scope.defaultReverse = false;
    $scope.reverse = false;
    $scope.pageSize = 10;
    $scope.currentPage = 0;
    $scope.searchText = '';
    $scope.colSpan = 10;
    $scope.viewAllLabel = 'View All';

    /*
     * ******************* NOTE ********************
     * Functions below relate to the Restore Workspace logic
     * *********************************************
     */

    $scope.restoreDialogClick = function () {
        var toBeRestored = angular.copy($scope.restoreDialog.ws);
        toBeRestored.CostVolumeLeadPricer = $scope.restoreDialog.leadName;
        toBeRestored.CostVolumeLeadPricerNtId = $scope.restoreDialog.leadNtId;
        toBeRestored.TrackingNumber = $scope.restoreDialog.trackingNumber;
        $scope.restoreWorkspaceCall(toBeRestored);
    };

    $scope.disableRestoreButton = function () {
        return $scope.restoreDialog.loadingTrackingNumbers || $scope.restoreDialog.trackingNumber === '';
    };

    $scope.closeRestoreDialog = function () {
        // clear validation errors
        $scope.errors = [];
        $scope.restoreDialog.open = false;
    };

    $scope.findPtmTrackingNumbers = function () {
        if ($scope.restoreDialog.leadNtId && $scope.restoreDialog.leadNtId !== '') {
            $scope.restoreDialog.loadingTrackingNumbers = true;
            $scope.restoreDialog.potentialTrackingNumbers = [];
            $scope.restoreDialog.errors = [];

            var data = { leadEstimatorNtId: $scope.restoreDialog.leadNtId, workspaceId: $scope.restoreDialog.ws.WorkspaceId };
            return $http({
                method: 'POST',
                data: data,
                url: CreatePostURL(HomePageModel.workspace, HomePageModel.controller, HomePageModel.getTrackingNumbersAction, '')
            }).then(function (response) {
                $scope.restoreDialog.potentialTrackingNumbers = response.data.trackingNumbers;

                $scope.restoreDialog.loadingTrackingNumbers = false;
            }, function (response) {
                // error handler
                var errors = [];
                var error = { ValidationIssue: 'Error retrieving Tracking Numbers for Lead Estimator.' };
                errors.push(error);
                $scope.restoreDialog.loadingTrackingNumbers = false;
                $scope.restoreDialog.errors = errors;
            });
        }
    };

    $scope.resetRestoreDialog = function () {
        $scope.restoreDialog.leadName = '';
        $scope.restoreDialog.leadNtId = '';
        $scope.restoreDialog.trackingNumber = '';
        $scope.restoreDialog.potentialTrackingNumbers = [];
        $scope.restoreDialog.ws = null;
    };

    $scope.showRestoreDialog = function (el) {
        if ($scope.isPtmIntegrated) {
            $scope.resetRestoreDialog();
            $scope.restoreDialog.ws = el.ws;
            $scope.restoreDialog.leadName = el.ws.CostVolumeLeadPricer;
            $scope.restoreDialog.leadNtId = el.ws.CostVolumeLeadPricerNtId;
            $scope.findPtmTrackingNumbers();

            // Setup the Lead Pricer lookup
            $('#restoreCostVolumeLeadPricerDisplayName').lookupUser({
                accountNameElementId: 'RestoreCostVolumeLeadPricerNTID',
                accountNameInitial: $scope.restoreDialog.leadNtId,
                accountDisplayNameInitial: $scope.restoreDialog.leadName,
                enabled: true,
                fieldDisplayName: 'Cost Volume Lead Pricer',
                allowGroups: false,
                checkNameCallbackHandler: function (valid) { },
                onChange: function () {
                    $scope.$apply(function () {
                        // pull ids from page
                        $scope.restoreDialog.leadNtId = $("#RestoreCostVolumeLeadPricerNTID").val();
                        $scope.restoreDialog.leadName = $("#restoreCostVolumeLeadPricerDisplayName").val();
                        $scope.findPtmTrackingNumbers();
                    });
                }
            });

            $scope.restoreDialog.open = true;
        } else {
            this.restoreWorkspace(el);
        }
    };

    /*
     * ******************* NOTE ********************
     * Functions below relate to the filtering logic
     * *********************************************
     */

    // reset current page when a user searches
    $scope.searchChanged = function () {
        $scope.currentPage = 0;
        $scope.SaveFilterToCookies();
    };

    $scope.showFavorites = function () {
        $scope.showFavoritesOnly = true;
        $scope.updateWorkspaces();
        $scope.SaveFilterToCookies();
    };

    $scope.sortMostRecent = function () {
        $scope.predicate = [$scope.columns.accessDate];
        $scope.reverse = true;
        $scope.SaveFilterToCookies();
    };

    $scope.toggleFilter = function (column) {
        if ($scope.isLoading) {
            return;
        }

        if (angular.isDefined(column)) {
            $scope.filter.filterColumn = column;
        }

        $scope.filter.open = !$scope.filter.open;
    };

    $scope.togglePaging = function() {
        $scope.isLoading = true;
        
        $timeout( function() {
            if ($scope.pageSize > 500) {
                $scope.pageSize = 20;
                $scope.viewAllLabel = 'View All';
            } else {
                $scope.pageSize = 10000;
                $scope.viewAllLabel = 'Enable Pagination';
            }

            $scope.currentPage = 0;
            $scope.isLoading = false;
        }, 0);
    }

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
        // hack to make sure the workspace name's width is set correctly afterward.
        var width = $('#HomeGrid th.name').width();
        $('#HomeGrid th.name').width(width);

        var selected = [];
        var workingSet = getWorkingFilterSet();
        if (workingSet) {
            workingSet.forEach(function(item) {
                if (item.checked) {
                    selected.push(item.value);
                }
            });
        }

        setWorkingFilterSet(selected);
        originalFilterSet = {};
        $scope.toggleFilter();

        $scope.updateWorkspaces();

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
        return $scope.showFavoritesOnly || selectedFilters.lob.length > 0 || selectedFilters.state.length > 0
            || selectedFilters.lead.length > 0 || $scope.searchText.length > 0;
    };

    // reset all filters to be empty and unselected
    $scope.clearAllFilters = function (noClose) {
        selectedFilters.lob = [];
        selectedFilters.state = [];
        selectedFilters.lead = [];
        $scope.searchText = '';

        $scope.filter.lob.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.state.forEach(function (item) {
            item.checked = false;
        });
        $scope.filter.lead.forEach(function (item) {
            item.checked = false;
        });
        $scope.predicate = defaultPredicate;
        $scope.reverse = $scope.defaultReverse;

        $scope.showFavoritesOnly = false;

        $scope.currentPage = 0;
        if (!noClose) {
            $scope.filter.open = false;
        }
        $scope.SaveFilterToCookies();
    };

    $scope.filterWorkspaces = function (data) {
        var compareValue = $scope.searchText.toLowerCase();
        var foundName = false;

        if (!$scope.isSysAdminBool && data.HasBeenDeleted) {
            return false;
        }

        if ($scope.showFavoritesOnly && data.IsFavorite === false) {
            return false;
        }

        if (selectedFilters.lob.length > 0 && ( // an option in the filter was selected
                (data.LineOfBusiness == null && selectedFilters.lob.indexOf(data.LineOfBusiness) === -1) || // checks nulls
                (data.LineOfBusiness != null && selectedFilters.lob.indexOf(data.LineOfBusiness.toLowerCase()) === -1) // checks values
            )) {
            return false;
        }

        if (selectedFilters.state.length > 0 && ( // an option in the filter was selected
                (data.WorkspaceState == null && selectedFilters.state.indexOf(data.WorkspaceState) === -1) || // checks nulls
                (data.WorkspaceState != null && selectedFilters.state.indexOf(data.WorkspaceState.toLowerCase()) === -1) // checks values
        )) {
            return false;
        }

        if (selectedFilters.lead.length > 0 && ( // an option in the filter was selected
                (data.CostVolumeLeadPricer == null && selectedFilters.lead.indexOf(data.CostVolumeLeadPricer) === -1) || // checks nulls
                (data.CostVolumeLeadPricer != null && selectedFilters.lead.indexOf(data.CostVolumeLeadPricer.toLowerCase()) === -1) // checks values
        )) {
            return false;
        }

        // filter based on search term
        if (compareValue.length > 0) {
            if (data.WorkspaceName.toLowerCase().indexOf(compareValue) !== -1) {
                return true;
            }

            if (data.TrackingNumber !== undefined && data.TrackingNumber != null && data.TrackingNumber.toLowerCase().indexOf(compareValue) !== -1) {
                return true;
            }

            if (data.WorkspaceState !== undefined && data.WorkspaceState != null && data.WorkspaceState.toLowerCase().indexOf(compareValue) !== -1) {
                return true;
            }

            if (data.CostVolumeLeadPricer !== undefined && data.CostVolumeLeadPricer != null && data.CostVolumeLeadPricer.toLowerCase().indexOf(compareValue) !== -1) {
                return true;
            }

            if (data.LineOfBusiness !== undefined && data.LineOfBusiness != null && data.LineOfBusiness.toLowerCase().indexOf(compareValue) !== -1) {
                return true;
            }
        } else {
            // haven't been thrown out yet, so it must be a match with no search term
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

        var tempArray = [];
        $.each($scope.filter.lead, function(i, item) {
            if (item.checked === true) {
                tempArray.push(item);
            }
        });

        $cookies.put('filtersLead', angular.toJson(tempArray), { expires: date });
        $cookies.put('filtersLob', angular.toJson($scope.filter.lob), { expires: date });
        $cookies.put('filtersState', angular.toJson($scope.filter.state), { expires: date });

        $cookies.put('searchText', angular.toJson($scope.searchText), { expires: date });
        $cookies.put('predicate', angular.toJson($scope.predicate), { expires: date });
        $cookies.put('reverse', angular.toJson($scope.reverse), { expires: date });
        $cookies.put('showFavoritesOnly', angular.toJson($scope.showFavoritesOnly), { expires: date });
    }

    // Load the filter settings
    $scope.LoadFilterFromCookies = function () {
        try {
            var temp;
            var cookies = $cookies.getAll();
            if ('filtersLob' in cookies) {
                temp = JSON.parse(cookies['filtersLob']);
                if (temp != undefined) {
                    $scope.filter.lob = temp;
                    $scope.filter.filterColumn = $scope.columns.lob;
                    $scope.applyFilters(false);
                }
            }
            if ('filtersState' in cookies) {
                temp = JSON.parse(cookies['filtersState']);
                if (temp != undefined) {
                    $scope.filter.state = temp;
                    $scope.filter.filterColumn = $scope.columns.state;
                    $scope.applyFilters(false);
                }
            }
            if ('filtersLead' in cookies) {
                temp = JSON.parse(cookies['filtersLead']);
                if (temp != undefined) {
                    $.each($scope.filter.lead, function(i, filterItem) {
                            $.each(temp, function(j, checkedItem) {
                                    if (filterItem.value === checkedItem.value) {
                                        filterItem.checked = true;
                                    }
                                });
                        });

                    $scope.filter.filterColumn = $scope.columns.lead;
                    $scope.applyFilters(false);
                }
            }

            if ('searchText' in cookies) {
                temp = JSON.parse(cookies['searchText']);
                if (temp != undefined) {
                    $scope.searchText = temp;
                }
            }
            if ('showFavoritesOnly' in cookies) {
                temp = JSON.parse(cookies['showFavoritesOnly']);
                if (temp != undefined) {
                    $scope.showFavoritesOnly = temp;
                }
            }
            if ('predicate' in cookies) {
                temp = JSON.parse(cookies['predicate']);
                if (temp != undefined) {
                    $scope.predicate = temp;
                }
            }
            if ('reverse' in cookies) {
                temp = JSON.parse(cookies['reverse']);
                if (temp != undefined) {
                    $scope.reverse = temp;
                }
            }
        } catch(ex) {
            console.log('Bad cookie load');
            console.log(ex);

            var date = new Date();
            var minutes = -1;
            date.setTime(date.getTime() + (minutes * 60 * 1000));

            $cookies.put('filtersLead', null, { expires: date });
            $cookies.put('filtersLob', null, { expires: date });
            $cookies.put('filtersState', null, { expires: date });
            $cookies.put('searchText', null, { expires: date });
            $cookies.put('predicate', null, { expires: date });
            $cookies.put('reverse', null, { expires: date });
            $cookies.put('showFavoritesOnly', null, { expires: date });
        }

        $scope.updateWorkspaces();
    }

    /*
     * ***************** NOTE ******************
     * Functions below relate to the table logic
     * *****************************************
     */
    $scope.changeSorting = function (sortValue, defaultReverse) {
        // clicking the same column reverses the sort
        if ($scope.predicate[0] === sortValue) {
            $scope.reverse = !$scope.reverse;
        } else {
            if (defaultReverse === undefined) {
                $scope.reverse = false;
            } else {
                $scope.reverse = defaultReverse;
            }

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

    /*
     * *********** NOTE ************
     * Private variables & functions 
     * *****************************
     */
    var blank = "(blanks)";
    var firstLoad = true;
    var originalFilterSet = {};
    var selectedFilters = { lob: [], state: [], lead: [] };

    var convertToArray = function (data) {
        var newData = [];

        for (var key in data) {
            var item = data[key];

            // set the display value to '(blanks)' if the display name is empty
            if (item.display == null || item.display == '') {
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
        if ($scope.filter.filterColumn === $scope.columns.lob) {
            return $scope.filter.lob;
        } else if ($scope.filter.filterColumn === $scope.columns.state) {
            return $scope.filter.state;
        } else if ($scope.filter.filterColumn === $scope.columns.lead) {
            return $scope.filter.lead;
        }
    };

    var setWorkingFilterSet = function (currentSet) {
        if (angular.isDefined(currentSet)) {
            if ($scope.filter.filterColumn === $scope.columns.lob) {
                selectedFilters.lob = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.state) {
                selectedFilters.state = currentSet;
            } else if ($scope.filter.filterColumn === $scope.columns.lead) {
                selectedFilters.lead = currentSet;
            }
        }
    };

    var loadWorkspaces = function () {
        $scope.isLoading = true;
        $scope.data = [];

        return $http({
            method: 'POST',
            url: CreatePostURL(HomePageModel.workspace, HomePageModel.controller, HomePageModel.action, '')
        }).then(function (response) {
            $scope.isSysAdminBool = response.data.isSysAdmin;

            if ($scope.hideTracking) {
                $scope.colSpan--;
            }

            $scope.canCreateWS = response.data.canCreateWS;
            $scope.updateWorkspaces();

            response.data.workspaceGridRows.forEach(function (item) {
                if (item.LastAccessed !== undefined && item.LastAccessed !== null && item.LastAccessed !== '') {
                    item.LastAccessedDate = new Date(item.LastAccessed);
                } else {
                    item.LastAccessedDate = new Date(2000, 1); // use an old date so that it is at the bottom of the list when sorting
                }

                if (item.ProposalSubmittal !== undefined && item.ProposalSubmittal !== null && item.ProposalSubmittal !== '') {
                    item.ProposalSubmittalDate = new Date(item.ProposalSubmittal);
                } else {
                    item.ProposalSubmittalDate = new Date(2000, 1); // use an old date so that it is at the bottom of the list when sorting
                }

                // build filter sets based on the data returned
                $scope.filter.lob[item.LineOfBusiness] = { display: item.LineOfBusiness, value: item.LineOfBusiness, checked: false };
                $scope.filter.state[item.WorkspaceState] = { display: item.WorkspaceState, value: item.WorkspaceState, checked: false };
                $scope.filter.lead[item.CostVolumeLeadPricer] = { display: item.CostVolumeLeadPricer, value: item.CostVolumeLeadPricer, checked: false };
            });

            // need to convert to arrays and sort the data
            $scope.filter.lob = convertToArray($scope.filter.lob);
            $scope.filter.state = convertToArray($scope.filter.state);
            $scope.filter.lead = convertToArray($scope.filter.lead);

            $scope.data = response.data.workspaceGridRows;
            $scope.LoadFilterFromCookies();

            $scope.isLoading = false;
            firstLoad = false;
        });
    };

    $scope.isSysAdmin = function () {
        return ($scope.isSysAdminBool != null && $scope.isSysAdminBool != undefined && $scope.isSysAdminBool == true);
    };

    $scope.createWorkspace = function () {
        window.location = CreateSystemAdminPostURL(HomePageModel.workspaceController, HomePageModel.createAction);
    };

    $scope.filterToBeDeleted = function () {
        var retArray = [];
        for (var i = 0; i < $scope.data.length; i++) {
            if ($scope.data[i].toBeDeleted != null && $scope.data[i].toBeDeleted != undefined && $scope.data[i].toBeDeleted == true) {
                retArray.push($scope.data[i]);
            }
        }
        return retArray;
    }

    $scope.disableDeleteCheckbox = function (el) {
        // The "delete" checkbox gets disabled when:
        // all BOEs are in Approved state
        //        OR
        // the user doesn't have admin permissions to the WS
        if (
                ((el.ws.NumBOEsInDraft != null && el.ws.NumBOEsInDraft != undefined && el.ws.NumBOEsInDraft == 0) //in draft == 0
                  && (el.ws.NumBOEsUnassigned != null && el.ws.NumBOEsUnassigned != undefined && el.ws.NumBOEsUnassigned == 0) // unassigned == 0
                  && (el.ws.NumBOEsInAwaitingApproval != null && el.ws.NumBOEsInAwaitingApproval != undefined && el.ws.NumBOEsInAwaitingApproval == 0) //awaiting approval ==0
                  && (el.ws.NumBOEsApproved != null && el.ws.NumBOEsApproved != undefined && el.ws.NumBOEsApproved > 0)) //approved > 0
                || !el.ws.CurrentUserHasAdmin
            ) {
            return true;
        }
        else {
            return false;
        }
    };

    $scope.openReports = function (el) {
        var wsShortName = el.ws.WorkspaceShortName;
        var wsid = el.ws.WorkspaceId;

        var page = "/" + wsShortName + "/Reports/Index/report";
        var popup = window.open(page, wsid + "Popup", "width=1024, height=768")

        return false;
    };

    $scope.updateFavorite = function (el) {
        var url = CreateSystemAdminPostURL(HomePageModel.controller, HomePageModel.changeFavoriteAction);
        $scope.errors = [];
        
        var dataToSend = {};
        dataToSend.workspaceId = el.ws.WorkspaceId;
        dataToSend.isFavorite = !el.ws.IsFavorite;

        $http.post(url, dataToSend)
            .then(function () {
                el.ws.IsFavorite = !el.ws.IsFavorite;
            })
            .catch(function (response) {
                $scope.errors = response.data.MessageList;
            });
    };

    $scope.dateFormatClass = function (inProposalDateString, inProposalDate, inNotApprovedTotal, inApprovedTotal) {
        var propdate = inProposalDate;
        var today = new Date();
        today.setHours(0,0,0,0);
        var diffInDays = (propdate - today) / (1000 * 60 * 60 * 24);

        var returnClassName;

        if (inNotApprovedTotal == 0 && inApprovedTotal > 0) {
            returnClassName = "grayTD";
        }

        else {
            if (inProposalDateString == "") {
                returnClassName = "";
            }
            else {
                if (diffInDays < 0) {
                    returnClassName = "redTD";
                }
                else if (diffInDays <= 14) {
                    returnClassName = "yellowTD";
                }
                else {
                    returnClassName = "greenTD";
                }
            }
        }
        return returnClassName;
    };

    $scope.deleteBOEsDisabled = function () {
        if ($scope.data != null && $scope.data != undefined && $scope.filterToBeDeleted().length > 0) {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.workspaceHasBeenDeleted = function (el) {
        if (el.ws.HasBeenDeleted != null && el.ws.HasBeenDeleted != undefined && el.ws.HasBeenDeleted == true) {
            return true;
        }
        else {
            return false;
        }
    };

    $scope.showRestore = function (el) {
        if (el.ws.HasBeenDeleted != null && el.ws.HasBeenDeleted != undefined
            && el.ws.HasBeenDeleted == true && $scope.isSysAdmin() == true) {

            return true;
        }
        else {
            return false;
        }
    };

    $scope.restoreWorkspace = function (el) {
        var dialog = {};
        dialog.title = 'Restore Workspaces and Backups';
        dialog.text = 'Are you sure you want to restore the selected workspace and all backups?';

        var toBeRestored = el.ws;

        GenSession.confirmDialog(dialog.title, dialog.text, function () {
            $scope.$apply(function () {
                $scope.restoreWorkspaceCall(toBeRestored);
            });
        }, function () {
        });
    };

    $scope.restoreWorkspaceCall = function (toBeRestored) {
        $scope.errors = [];
        var action = $scope.isPtmIntegrated ? HomePageModel.restorePtmAction : HomePageModel.restoreAction;
        var url = CreateSystemAdminPostURL(HomePageModel.controller, action);
        $http.post(url, toBeRestored)
            .then(function () {
                toBeRestored.HasBeenDeleted = false;
                if ($scope.isPtmIntegrated) {
                    $scope.restoreDialog.open = false;
                    $scope.resetRestoreDialog();
                    // reload the grid to update estimating lead/deleted status
                    loadWorkspaces();
                }
            })
            .catch(function (response) {
                $scope.errors = response.data.MessageList;
            });
    };

    $scope.SubmitDelete = function () {
        var toBeDeleted = $scope.filterToBeDeleted();
        var dialog = {};
        dialog.title = 'Delete Workspaces and Backups';
        dialog.text = 'Are you sure you want to delete the selected workspaces and all backups? <br/> Once deleted, this can not be undone.';

        GenSession.confirmDialog(dialog.title, dialog.text, function () {
            $scope.$apply(function () {
                $scope.checkForHiddenItems(toBeDeleted);
            });
        }, function () {
        });
    };

    $scope.continueDelete = function (toBeDeleted) {
        $scope.errors = [];
        var url = CreateSystemAdminPostURL(HomePageModel.controller, HomePageModel.deleteAction);
        $http.post(url, toBeDeleted)
            .then(function (response) {
                var popupDialog = $('<div></div>');
                popupDialog.html(String(response.data));

                // if any users are currently using a selected workspace, show message
                if (popupDialog.find(".whosonline-row").length > 0) {
                    popupDialog.find("#WhosOnlineDivContent b").html("<div class=\"validation-summary-errors\">No workspaces will be deleted.  At least one user is currently using a workspace.  The following users are using the selected workspaces.</div>");
                    GenSession.commonDialog("Delete Workspaces and Backups", popupDialog, [{
                        buttonClass: 'ies',
                        ButtonText: 'OK',
                        ButtonName: "ok-button"
                    }], '590px');
                }
                else {
                    for (var i = 0; i < toBeDeleted.length; i++) {
                        toBeDeleted[i].toBeDeleted = false;
                        toBeDeleted[i].HasBeenDeleted = true;
                        if ($scope.isPtmIntegrated) {
                            toBeDeleted[i].TrackingNumber = '';
                        }
                    }
                }
            })
            .catch(function (response) {
                $scope.errors = response.data.MessageList;
            });
    };

    $scope.checkForHiddenItems = function (data) {
        // start and end indexes for items on page
        var start = $scope.currentPage * $scope.pageSize;
        var end = start + $scope.pageSize

        // get items currently displayed on page
        var currentPageItems = $scope.filteredResults.slice(start, end);

        // check if any items are selected, but not currently displayed
        var itemsOnOtherPages = data.some(function (s) {
            return currentPageItems.indexOf(s) < 0;
        });

        if (itemsOnOtherPages) {
            Session.confirmDialog('Hidden Items',
                'You have selected Workspaces that are currently hidden from view. Would you like to continue? Select No to review these items.',
                function () {
                    $scope.continueDelete(data);
                });
        } else {
            $scope.continueDelete(data);
        }
    }

    $scope.updateWorkspaces = function () {
        //reset paging if filter changed
        $scope.currentPage = 0;
    };

    // load the main data
    loadWorkspaces();
}]);