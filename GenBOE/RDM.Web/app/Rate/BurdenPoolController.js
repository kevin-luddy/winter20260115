angular.module('RDM').controller('BurdenPoolController', ['$scope', '$http', '$filter', '$window', '$rootScope', '$timeout', 'lockService', 'utilityService', 'uiGridConstants', 'BurdenPoolModel', function ($scope, $http, $filter, $window, $rootScope, $timeout, lockService, utilityService, uiGridConstants, BurdenPoolModel) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.isDataError = false;
    $scope.selectedVersion;
    $scope.versions = [];
    $scope.burdenPools = [];
    $scope.burdenElements = [];
    $scope.rateCodes = [];
    $scope.lockInfo;
    $scope.showEditLockTimeoutExpiration = false;
    $rootScope.errors = [];
    $scope.PageIsDirty = false;
    $scope.addedBurdenPoolId = -1;
    $scope.burdenElementColumnCount = 0;
    $scope.canEdit = function () { return $scope.lockInfo !== undefined && !$scope.lockInfo.IsReadOnly; };
    lockService.id = BurdenPoolModel.Area;

    $scope.gridOptions = {};

    $scope.cellClass = function (grid, row, col, rowRenderIndex, colRenderIndex) {
        var cellClass = col.cellTemplate.indexOf("type='checkbox'") !== -1 ? 'center ' : '';
        if (grid.appScope.isReadOnly) {
            return cellClass + 'disabled-text';
        } else if ($scope.dirtyArray !== undefined) {
            var dirtyEditedRow = $scope.dirtyArray.filter(function (daRow) {
                return daRow.Id === row.entity.Id;
            });
            if (dirtyEditedRow[0] && dirtyEditedRow[0][col.field] === true) {
                return cellClass + 'dirty-text';
            }
        }

        return cellClass;
    };

    $scope.initializeGridOptions = function () {
        var editHeaderCellTemplate = '<div role="columnheader" data-ng-class="{ \'sortable\': sortable }" ui-grid-one-bind-aria-labelledby-grid="col.uid + \'-header-text\'  + col.uid + ' +
            '\'-sortdir-text\'" aria-sort="none" aria-labelledby="1498216693300-uiGrid-0004-header-text 1498216693300-uiGrid-0004-sortdir-text"><div role="button" tabindex="0" ' +
            'class="ui-grid-cell-contents ui-grid-header-cell-primary-focus glyphicon glyphicon-pencil text-success" col-index="renderIndex" style="padding-left:0px; padding-right:0px">' +
            '<span class="ui-grid-header-cell-label" ui-grid-one-bind-id-grid="col.uid + \'-header-text\'" id="1498216693300-uiGrid-0004-header-text">{{col.displayName}}</span>' +
            '<span ui-grid-one-bind-id-grid="col.uid + \'-sortdir-text\'" ' +
            'ui-grid-visible="col.sort.direction" aria-label="Sort None" class="ui-grid-invisible" id="1498216693300-uiGrid-0004-sortdir-text" >' +
            '<i data-ng-class="{ \'ui-grid-icon-up-dir\': col.sort.direction == asc, \'ui-grid-icon-down-dir\': col.sort.direction == desc, \'ui-grid-icon-blank\': !col.sort.direction }"' +
            'title="" aria-hidden="true" class="ui-grid-icon-blank" style="position: relative; z-index:1000;"></i > <sub ui-grid-visible="isSortPriorityVisible()" class="ui-grid-sort-priority-number ui-grid-invisible">1</sub>' +
            '</span ></div ><!----><div role="button" tabindex="0" ui-grid-one-bind-id-grid="col.uid + \'-menu-button\'" class="ui-grid-column-menu-button" ' +
            'data-ng-if="grid.options.enableColumnMenus &amp;&amp; !col.isRowHeader  &amp;&amp; col.colDef.enableColumnMenu !== false" data-ng-click="toggleMenu($event)" ' +
            'data-ng-class="{\'ui-grid-column-menu-button-last-col\': isLastCol}" ui-grid-one-bind-aria-label="i18n.headerCell.aria.columnMenuButtonLabel" ' +
            'aria-haspopup="true" id="1498216693300-uiGrid-0004-menu-button" aria-label="Column Menu" > <i class="ui-grid-icon-angle-down body-content" aria-hidden="true">&nbsp;</i></div >' +
            '<!----><div ui-grid-filter=""></div></div >';

        $scope.gridOptions = {
            enableCellEditOnFocus: true,
            enableSorting: true,
            enableFiltering: true,
            enableGridMenu: true,
            enableHorizontalScrollbar: uiGridConstants.scrollbars.ALWAYS,
            enableColumnResizing: true,
            onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
            },
            columnDefs: [
                {
                    name: "Manage", visible: false, headerCellTemplate: "<div/>", cellTemplate: '<span title="Delete Burden Pool" role="button" data-ng-hide="grid.appScope.isReadOnly" data-ng-click="grid.appScope.showConfirmDelete(row.entity)" class="glyphicon glyphicon-remove text-danger text-center"></span>',
                    enableSorting: false, enableFiltering: false, pinnedLeft: true, cellClass: 'manage-rates center-icons center', enableCellEdit: false
                },
                {
                    name: '', headerCellTemplate: editHeaderCellTemplate, headerCellClass: 'glyphicon glyphicon-pencil text-success text-center', field: 'D', visible: false, cellTemplate: '<span title="Row Contains Edits" class="glyphicon glyphicon-pencil text-success text-center" data-ng-show="row.entity.D"></span>',
                    enableSorting: true, enableFiltering: false, pinnedLeft: true, width: 55, cellClass: 'dirty-text center-icons center', enableCellEdit: false, exporterSuppressExport: true
                },
                { name: "BurdenPool", field: "BurdenPool", pinnedLeft: true, width: 120, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass },
                { field: "Description", pinnedLeft: true, width: 200, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass },
                {
                    name: "IsCommercial", displayName: "Is Commercial", field: "IsCommercial", type: "boolean", width: 100, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass,
                    cellTemplate: "<input type='checkbox' disabled='disabled' data-ng-model='row.entity.IsCommercial'>"
                },
                {
                    name: "IsGaT2ApplicableForMissionSolutions", displayName: "Include G&A T2 in Services Burd Table", field: "IsGaT2ApplicableForMissionSolutions", type: "boolean", width: 114, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass,
                    cellTemplate: "<input type='checkbox' disabled='disabled' data-ng-model='row.entity.IsGaT2ApplicableForMissionSolutions'>"
                },
                {
                    name: "IncludeGaT2InBurdAndCommBurdTables", displayName: "Include G&A T2 in Burd & Comm Burd Tables", field: "IncludeGaT2InBurdAndCommBurdTables", type: "boolean", width: 114, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass,
                    cellTemplate: "<input type='checkbox' disabled='disabled' data-ng-model='row.entity.IncludeGaT2InBurdAndCommBurdTables'>"
                },
                {
                    name: "ExcludeFCCOMFromCommercial", displayName: "Exclude FCCOM from Commercial (or Comm) Burd Table", field: "ExcludeFCCOMFromCommercial", type: "boolean", width: 150, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass,
                    cellTemplate: "<input type='checkbox' disabled='disabled' data-ng-model='row.entity.ExcludeFCCOMFromCommercial'>"
                }
            ],
            data: $scope.filteredData
        };

        $scope.gridOptions.onRegisterApi = function(gridApi) {
            $scope.gridApi = gridApi;

            $scope.gridApi.edit.on.afterCellEdit($scope, function (rowEntity, colDef, newValue, oldValue) {
                if (!(newValue === oldValue)) {
                    $scope.PageIsDirty = true;
                    rowEntity.D = true;

                    // Set entry in dirty array to true so it will be marked as dirty-text
                    var dirtyEditedRow = $scope.dirtyArray.filter(function (row) {
                        return row.Id === rowEntity.Id;
                    });
                    dirtyEditedRow[0][colDef.field] = true;
                }
            });
        }
    };

    $scope.updateGridOptions = function() {
        // setup column definitions for the Burden Elements
        for (var index = 0; index < $scope.burdenElements.length; index++) {
            var burdenElement = $scope.burdenElements[index];
            $scope.gridOptions.columnDefs.push({
                name: burdenElement.Description, displayName: burdenElement.Description, field: "BurdenElementRateCodeArray[" + index + "]",
                editableCellTemplate: 'ui-grid/dropdownEditor', editDropdownIdLabel: 'Label', editDropdownValueLabel: 'Label', editDropdownOptionsArray: $scope.rateCodes,
                width: 110, enableSorting: true, enableFiltering: true, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass
            });
        }
        $scope.gridOptions.columnVirtualizationThreshold = $scope.gridOptions.columnDefs.length;
    };

    $scope.initialize = function () {
        $scope.isInitializing = false;
        $scope.initializeGridOptions();
        $scope.loadData();
    };

    $scope.loadData = function() {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = true;
            $scope.clearLockTimers();
            $scope.PageIsDirty = false;

            // Load Burden Pool data.  If SelectedRevisionId is null, then get WIP data.
            $scope.loadBurdenPools(BurdenPoolModel.SelectedRevisionId).then(function successCallback(response) {
                $scope.data = response.data.BurdenPools;
                $scope.versions = response.data.Versions;
                $scope.selectedVersion = $scope.versions[0];    // default to WIP revision
                if (BurdenPoolModel.SelectedRevisionId) {
                    for (var i = 0; i < $scope.versions.length; i++) {
                        var version = $scope.versions[i];
                        if (version.Id == BurdenPoolModel.SelectedRevisionId) {
                            $scope.selectedVersion = version;
                        }
                    }
                }
                $scope.burdenPools = response.data.BurdenPools;
                $scope.burdenElements = response.data.BurdenElements;
                $scope.rateCodes = response.data.RateCodes;
                $scope.lockInfo = response.data.LockInfo;
                $scope.updateGridOptions();
                $scope.filterData();
                $scope.initializeDirtyGrid();
                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            });
        }
    };

    $scope.initializeDirtyGrid = function () {
        var rowCount = $scope.filteredData.length;
        $scope.burdenElementColumnCount = $scope.filteredData[0].BurdenElementRateCodeArray.length;

        $scope.dirtyArray = new Array(rowCount);
        for (var i = 0; i < rowCount; i++) {
            // Add fixed columns
            $scope.dirtyArray[i] = { 'Id': $scope.filteredData[i].Id, 'BurdenPool': false, 'Description': false, 'IsCommercial': false, 'IsGaT2ApplicableForMissionSolutions': false, 'ExcludeFCCOMFromCommercial': false };

            // Add columns for the Burden Element Rate Code Mappings
            for (var j = 0; j < $scope.burdenElementColumnCount; j++) {
                $scope.dirtyArray[i]['BurdenElementRateCodeArray[' + j + ']'] = false;
            }
        }
    };

    $scope.loadBurdenPools = function (versionId) {
        return $http({
            method: 'POST',
            data: { versionId: versionId },
            url: createPostURL(BurdenPoolModel.controller, BurdenPoolModel.getBurdenPoolsAction)
        });
    };

    $scope.deleteBurdenPool = function (burdenPool) {
        // clear error message
        $scope.isDataValid = true;
        burdenPool.IsDeleted = true;
        $scope.PageIsDirty = true;
        burdenPool.D = true;

        // Remove deleted dirtyArray obect for deleted burden pool.
        for (var i = 0; i < $scope.dirtyArray.length; i++) 
        {
            if ($scope.dirtyArray[i].Id === burdenPool.Id)
            {
                $scope.dirtyArray.splice(i, 1);
                break;
            }
        } 

        $scope.refreshLock();
        $scope.filterData();
     };

    /* Adds a new (blank) Burden Pool row to the grid. */
    $scope.addBurdenPool = function() {
        // If a Burden Pool is added, mark the page as dirty
        $scope.PageIsDirty = true;
        // Insert a new Burden Pool row (at top of list).
        $scope.data.unshift({ Id: $scope.addedBurdenPoolId, IsGaT2ApplicableForMissionSolutions: false, BurdenElementRateCodeArray: [], 'D': true });

        // Insert a new row into the top of the dirtyArray
        var newDirtyRow = { 'Id': $scope.addedBurdenPoolId--, 'BurdenPool': false, 'Description': false, 'IsCommercial': false, 'IsGaT2ApplicableForMissionSolutions': false, 'ExcludeFCCOMFromCommercial': false, 'D': true };
        for (var i = 0; i < $scope.burdenElementColumnCount; i++) {
            newDirtyRow['BurdenElementRateCodeArray[' + i + ']'] = false;
        }
        $scope.dirtyArray.splice(0, 0, newDirtyRow);
        $scope.filterData();

        // Auto-scroll to the new item and set focus to the Burden Pool cell.  The timeout delay is required to allow for the push to complete.
        $timeout(function () {
            $scope.gridApi.cellNav.scrollToFocus($scope.gridOptions.data[0], $scope.gridOptions.columnDefs[1]);
        }, 100);
    };

    $scope.filterData = function() {
        $scope.filteredData = $scope.data.filter(function(row) {
            return $scope.filterRow(row);
        });
        $scope.gridOptions.data = $scope.filteredData;
    };

    $scope.filterRow = function(row) {
        if (row.IsDeleted) {
            return false;
        }
        // other filters (probably not needed if using ui-grid)
        return true;
    };

    $scope.saveBurdenPoolGrid = function (releaseLock) {
        // clear error message
        $rootScope.errors = [];
        $scope.isDataValid = true;
        $(document).trigger("SHOW_LOADING_BOX");

        // Clear lock timers so there's no pop-up in the middle of a save
        $scope.clearLockTimers();

        var saveUrl = createPostURL(BurdenPoolModel.controller, BurdenPoolModel.saveAction);

        var dirtyPools = $scope.data.filter(function (pool) { return pool.D === true; });

        return $http({
            method: 'POST',
            url: saveUrl,
            data: JSON.stringify(dirtyPools)
        }).then(function successCallback(response) {
            $scope.reloadGrid(releaseLock);
        }, function errorCallback(response) {
            $scope.refreshLock(); // Refresh lock to give user time to correct errors
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.showConfirmDelete = function (burdenPool) {
        ConfirmDialog("Delete Burden Pool", "Are you sure you want to delete this Burden Pool?", function () {
            $scope.deleteBurdenPool(burdenPool);
        });
    };

    $scope.reloadGrid = function (releaseLock) {
        // If Save & Continue make a copy of the lockInfo.
        var tempLockInfo = null;
        if (!releaseLock && $scope.lockInfo !== undefined && $scope.lockInfo.InUse !== null) {
            tempLockInfo = $scope.lockInfo;
        }
        // AJAX call to backend to get JSON data
        $(document).trigger("SHOW_LOADING_BOX");
        // clear error message
        $rootScope.errors = [];
        $scope.isDataLoading = true;
        $scope.PageIsDirty = false;

        // use $scope.selectedVersion.Id for the version id
        $scope.loadBurdenPools($scope.selectedVersion.Id).then(function successCallback(response) {
            $scope.data = response.data.BurdenPools;
            $scope.burdenPools = response.data.BurdenPools;
            $scope.burdenElements = response.data.BurdenElements;
            $scope.rateCodes = response.data.RateCodes;
            if (releaseLock !== undefined && !releaseLock && tempLockInfo !== null) {
                $scope.lockInfo = tempLockInfo;
                $scope.refreshLock();
            } else {
                $scope.lockInfo = response.data.LockInfo;
                $scope.releaseLock();
            }

            // reset grid options in case burden elements changed
            $scope.initializeGridOptions();
            $scope.showHideManageColumn();

            $scope.updateGridOptions();
            $scope.filterData();
            $scope.initializeDirtyGrid();
            $scope.isDataLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.isDataLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.lock = function () {
        lockService.lock().then(
            function (response) {
                $scope.lockInfo = response.data.LockInfo;
                $scope.showHideManageColumn();
                lockService.processResponse(response,
                    $scope.lockInfo.EditLockTimeoutWarningMinutes,
                    $scope.refreshLock,
                    $scope.lockInfo.EditLockTimeoutExpirationMinutes,
                    $scope.editLockTimeoutExpirationCallback,
                    $scope.clearLockTimers,
                    $scope.reloadGrid(false),
                    false);
            },
            function () {
                lockService.requestRejected();
            });
    };

    $scope.showHideManageColumn = function () {
        $scope.gridOptions.columnDefs[0].visible = $scope.canEdit(); // Manage column
        $scope.gridOptions.columnDefs[1].visible = $scope.canEdit(); // Edits column
        $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
    }

    $scope.clearLockTimers = function () {
        $scope.showEditLockTimeoutExpiration = false;
        lockService.clearLockTimers();
    };

    $scope.editLockTimeoutExpirationCallback = function () {
        $scope.showEditLockTimeoutExpiration = true;
    };

    $scope.refreshLock = function () {
        lockService.refreshLock().then(
            function (response) {
                $scope.lockInfo = response.data.LockInfo;
                $scope.showHideManageColumn();
                lockService.processResponse(response,
                    $scope.lockInfo.EditLockTimeoutWarningMinutes,
                    $scope.refreshLock,
                    $scope.lockInfo.EditLockTimeoutExpirationMinutes,
                    $scope.editLockTimeoutExpirationCallback,
                    $scope.clearLockTimers,
                    null,
                    true);
            },
            function () {
                lockService.requestRejected();
            });
    };

    $scope.releaseLock = function () {
        // See if we have a lock.
        if (lockService !== undefined && lockService.id !== undefined && $scope.canEdit()) {
            lockService.unlock().then(
                function (response) {
                    $scope.lockInfo = response.data.LockInfo;
                    $scope.showHideManageColumn();
                    lockService.processResponseRelease(response, $scope.clearLockTimers);
                },
                function () {
                    lockService.requestRejected();
                });
        }
    };

    $scope.confirmCancel = function () {
        if ($scope.PageIsDirty) {
            ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                // Yes
                $scope.reloadGrid(true);
            },
            function () {
                // No
                return false;
            });
        } else {
            // If no changes, continue with reloadGrid
            $scope.reloadGrid(true);
        }
    };

    window.onbeforeunload = function () {
        // Confirm leaving/refreshing page if the page is dirty
        if ($scope.PageIsDirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    window.onunload = function () {
        // If there is a lock and user owns it, remove the lock
        // Checks for this are done on the back end in the Home Controller Unlock() method
        if (lockService !== undefined && lockService.id !== undefined && $scope.canEdit()) {
            lockService.synchronousUnlock();
        }
    };

    // Initialization functions
    $scope.initialize();
}]);