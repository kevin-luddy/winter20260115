angular.module('RDM').controller('CobraDetailController', ['$scope', '$http', '$rootScope', 'lockService', 'utilityService', 'uiGridConstants', 'uiGridExporterConstants', 'CobraDetailModel', function ($scope, $http, $rootScope, lockService, utilityService, uiGridConstants, uiGridExporterConstants, CobraDetailModel) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.isDataError = false;
    $scope.data = [];
    $scope.filteredData = [];
    $scope.selectedVersion = { Id: CobraDetailModel.SelectedRevisionId };
    $scope.versions = [];
    $scope.cobraCodes = [];
    $scope.lockInfo;
    $scope.showEditLockTimeoutExpiration = false;
    $rootScope.errors = [];
    $scope.PageIsDirty = false;
    $scope.canEdit = function () { return $scope.lockInfo !== undefined && !$scope.lockInfo.IsReadOnly; };
    $scope.gridOptions = {};
    lockService.id = CobraDetailModel.Area;

    // Initializations ------------------------------------------
    $scope.initialize = function () {
        $scope.isInitializing = false;
        $scope.setGridOptions();
        $scope.loadData();
    };

    $scope.cellClass = function (grid, row, col, rowRenderIndex, colRenderIndex) {
        if (grid.appScope.lockInfo.IsReadOnly) {
            return 'disabled-text';
        } else {            
            // This is a row-entity column, since we only have one Dirty flag for the row,
            // I broke out each column so interface is consistant.
            if (row.entity.D === true && $scope.dirtyArray !== undefined) {
                var dirtyEditedRow = $scope.dirtyArray.filter(function (daRow) {
                    return daRow.Id === row.entity.Id;
                });
                if (dirtyEditedRow[0] && dirtyEditedRow[0][col.field] === true) {
                    return 'dirty-text';
                }
            }
        }
        return 'no-spinner';    // Chrome is making number input elements show spinner.
    };

    $scope.setGridOptions = function () {
        $scope.gridOptions = {
            enablePagination: true,
            enablePaginationControls: true,
            paginationPageSize: 20,
            paginationPageSizes: [15, 20, 25, 30, 100, 500, 5000],
            enableCellEditOnFocus: true,
            enableSorting: true,
            enableFiltering: true,
            enableGridMenu: true,
            exporterMenuExcel: false,   // requires the "excel-builder" npm module which is no longer maintained - https://github.com/stephenliberty/excel-builder.js
            exporterMenuPdf: false,
            rowHeight: 18,          // This causes a performance hit in IE (like everything) but makes the grid paint & scroll correctly.
            data: $scope.filteredData
        };

        /* Get a reference to the gridApi so that we can auto-scroll later. */
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;

            $scope.gridApi.edit.on.afterCellEdit($scope,
                function (rowEntity, colDef, newValue, oldValue) {
                    if (!(newValue === oldValue)) {
                        $scope.PageIsDirty = true;  // Mark page as dirty to turn on Save/Cancel buttons.
                        rowEntity.D = true;         // Mark the row entity as Dirty.

                        // This is a row entity column and uses dirtyArray.
                        var dirtyEditedRow = $scope.dirtyArray.filter(function (row) {
                            return row.Id === rowEntity.Id;
                        });
                        dirtyEditedRow[0][colDef.field] = true;

                        if (colDef.field === 'C1D') {
                            if (rowEntity.C1D.length > 0) {
                                var code1 = $scope.cobraCodes.filter(function (code1) { return code1.Label === rowEntity.C1D; });
                                if (code1 !== undefined) {
                                    rowEntity.C1 = code1[0].Id;
                                }
                            } else {
                                rowEntity.C1 = 0;
                            }
                         }

                        $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.EDIT);
                    }
                });
        };
    };

    $scope.resetColumnDefinitions = function () {
        var editHeaderCellTemplate = '<div role="columnheader" data-ng-class="{ \'sortable\': sortable }" ui-grid-one-bind-aria-labelledby-grid="col.uid + \'-header-text\'  + col.uid + ' +
            '\'-sortdir-text\'" aria-sort="none" aria-labelledby="1498216693300-uiGrid-0004-header-text 1498216693300-uiGrid-0004-sortdir-text"><div role="button" tabindex="0" ' +
            'class="ui-grid-cell-contents ui-grid-header-cell-primary-focus glyphicon glyphicon-pencil text-success" col-index="renderIndex" style="padding-left:0px; padding-right:0px">' +
            '<span class="ui-grid-header-cell-label" ui-grid-one-bind-id-grid="col.uid + \'-header-text\'" id="1498216693300-uiGrid-0004-header-text"></span>' +
            '<span ui-grid-one-bind-id-grid="col.uid + \'-sortdir-text\'" ' +
            'ui-grid-visible="col.sort.direction" aria-label="Sort None" class="ui-grid-invisible" id="1498216693300-uiGrid-0004-sortdir-text" >' +
            '<i data-ng-class="{ \'ui-grid-icon-up-dir\': col.sort.direction == asc, \'ui-grid-icon-down-dir\': col.sort.direction == desc, \'ui-grid-icon-blank\': !col.sort.direction }"' +
            'title="" aria-hidden="true" class="ui-grid-icon-blank" style="position: relative; z-index:1000;"></i > <sub ui-grid-visible="isSortPriorityVisible()" class="ui-grid-sort-priority-number ui-grid-invisible">1</sub>' +
            '</span ></div ><!----><div role="button" tabindex="0" ui-grid-one-bind-id-grid="col.uid + \'-menu-button\'" class="ui-grid-column-menu-button" ' +
            'data-ng-if="grid.options.enableColumnMenus &amp;&amp; !col.isRowHeader  &amp;&amp; col.colDef.enableColumnMenu !== false" data-ng-click="toggleMenu($event)" ' +
            'data-ng-class="{\'ui-grid-column-menu-button-last-col\': isLastCol}" ui-grid-one-bind-aria-label="i18n.headerCell.aria.columnMenuButtonLabel" ' +
            'aria-haspopup="true" id="1498216693300-uiGrid-0004-menu-button" aria-label="Column Menu" > <i class="ui-grid-icon-angle-down body-content" aria-hidden="true">&nbsp;</i></div >' +
            '<!----><div ui-grid-filter=""></div></div >';

        var tempColDef = new Array(
                {
                    name: 'Edited', headerCellTemplate: editHeaderCellTemplate, headerCellClass: 'glyphicon glyphicon-pencil text-success text-center', field: 'D', visible: false, cellTemplate: '<span title="Row Contains Edits" class="glyphicon glyphicon-pencil text-success text-center" data-ng-show="row.entity.D"></span>',
                    enableSorting: true, enableFiltering: false, width: 55, cellClass: 'dirty-text center-icons center', enableCellEdit: false, exporterSuppressExport: true
                },
                { name: 'Category', field: 'RCD', width: 150, cellEditableCondition: false, cellClass: $scope.cellClass, cellTooltip: true },
                { name: 'Rate Code', field: 'Co', width: 145, cellEditableCondition: false, cellClass: $scope.cellClass },
                { name: 'Description', field: 'De', width: 250, cellEditableCondition: false, cellClass: $scope.cellClass, cellTooltip: true },
                {
                    name: 'Code1', field: 'C1D', cellEditableCondition: $scope.canEdit, width: 150, editableCellTemplate: 'ui-grid/dropdownEditor',
                    editDropdownIdLabel: 'Label', editDropdownValueLabel: 'Label', editDropdownOptionsArray: $scope.cobraCodes, cellClass: $scope.cellClass, cellTooltip: true
                },
                { name: 'Rate Set', field: 'RS', width: 250, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass, cellTooltip: true }
        );

        tempColDef[0].visible = $scope.canEdit(); // Edits column
        $scope.gridOptions.columnDefs = angular.copy(tempColDef);
    };

    $scope.filterData = function () {
        $scope.filteredData = $scope.data.filter(function (row) {
            return $scope.filterRow(row);
        });
        $scope.gridOptions.data = $scope.filteredData;
    };

    $scope.filterRow = function (row) {
        if (row.Del) {
            return false;
        }

        // other filters (probably not needed if using ui-grid)

        return true;
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
                    $scope.reloadGrid(true),
                    false);
            },
            function () {
                lockService.requestRejected();
            });
    };

    $scope.reloadGrid = function (isLock) {
        $(document).trigger("SHOW_LOADING_BOX");
        $scope.isDataLoading = true;

        // use $scope.selectedVersion.Id for the version id
        var getCobraDetailsUrl = createPostURL(CobraDetailModel.controller, CobraDetailModel.getCobraDetailsByVersionAction, $scope.selectedVersion.Id);
        $http({
            method: 'POST',
            url: getCobraDetailsUrl,
            data: { }
        }).then(function successCallback(response) {
            if (!$scope.versions.length) {
                $scope.versions = response.data.Versions;
                $scope.selectedVersion = $scope.findVersionById($scope.selectedVersion.Id, response.data.Versions);
            }
            $scope.cobraCodes = response.data.CobraCodes;
            $scope.lockInfo = response.data.LockInfo;
            $scope.data = [];
            $scope.gridOptions.data = [];
            $scope.gridOptions.columnDefs = new Array();
            $scope.resetColumnDefinitions();
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
            $scope.updateCobraDetails(angular.copy(response.data.CobraDetails));
            $scope.initializeDirtyGrid();
            if (!isLock) {
                $scope.releaseLock();
            }
            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.isDataLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    /* Helper method to find a version by Id */
    $scope.findVersionById = function (id, versions) {
        if (id) {
            for (var i = 0; i < versions.length; i++) {
                if (versions[i].Id == id) {
                    return versions[i];
                }
            }
        }
        return versions[0]; // default to first version -- typically WIP (unless user is not Admin)
    }

    $scope.saveCobraDetail = function (releaseLock) {
        // clear error messages.
        $rootScope.errors = [];
        $scope.isDataValid = true;
        $(document).trigger("SHOW_LOADING_BOX");

        // Clear lock timers so there's no pop-up in the middle of a save
        $scope.clearLockTimers();

        var saveUrl = createPostURL(CobraDetailModel.controller, CobraDetailModel.saveAction);

        var dirtyCobraDetails = $scope.data.filter(function (rate) { return rate.D === true && (rate.Del === false || rate.Id > 0); });

        return $http({
            method: 'POST',
            url: saveUrl,
            data: JSON.stringify(dirtyCobraDetails)
        }).then(function successCallback(response) {
            $scope.initializeDirtyGrid();
            $scope.updateCobraDetails(response.data);
            if (releaseLock) {
                $scope.releaseLock();
            } else {
                // start a new timer on Save & Continue.
                $scope.refreshLock();
            }

            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.refreshLock(); // Refresh lock to give user time to correct errors
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.updateCobraDetails = function (newCobraDetails) {
        $scope.data = newCobraDetails;
        $scope.PageIsDirty = false;
        $scope.filterData();
        $scope.isDataLoading = false;
        // Clear validation errors.
        $rootScope.errors = [];
        $scope.isDataValid = true;
    };

    $scope.showHideManageColumn = function () {
        $scope.gridOptions.columnDefs[0].visible = $scope.canEdit(); // Edits column
        $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
    };

    $scope.clearLockTimers = function () {
        $scope.showEditLockTimeoutExpiration = false;
        lockService.clearLockTimers();
    };

    $scope.editLockTimeoutExpirationCallback = function () {
        $scope.showEditLockTimeoutExpiration = true;
    };

    $scope.refreshLock = function () {
        if (lockService.id !== undefined) {
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
        }
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

    /* Loads the Data */
    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $scope.isDataLoading = true;
            $scope.isDataError = false;
            $scope.reloadGrid(true);
        }
    };

    $scope.initializeDirtyGrid = function () {
        var rowCount = $scope.data.length;
        $scope.dirtyArray = new Array(rowCount);
        for (var i = 0; i < rowCount; i++) {
            $scope.dirtyArray[i] = { 'Id': $scope.data[i].Id, 'C1D': false, 'RS': false };
        }
    };

    $scope.confirmCancel = function () {
        if ($scope.PageIsDirty) {
            ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                $scope.reloadGrid(false);
            });
        } else {
            // If no changes, continue with reloadGrid
            $scope.reloadGrid(false);
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

    // Start Initialization -----------------------------------------
    $scope.initialize();
}])
