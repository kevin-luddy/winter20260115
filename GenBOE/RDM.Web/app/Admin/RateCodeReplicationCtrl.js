angular.module('RDM').controller('RateCodeReplicationCtrl', ['$scope', '$http', '$rootScope', 'RateCodeReplicationModel', 'lockService', 'uiGridConstants', 'uiGridExporterConstants', '$window', function ($scope, $http, $rootScope, RateCodeReplicationModel, lockService, uiGridConstants, uiGridExporterConstants, $window) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $rootScope.errors = [];
    $scope.data = [];
    $scope.filteredData = [];
    $scope.lockInfo;
    $scope.rateCodes = [];
    $scope.showEditLockTimeoutExpiration = false;
    $scope.PageIsDirty = false;
    $scope.canEdit = function () { return $scope.lockInfo !== undefined && !$scope.lockInfo.IsReadOnly; };
    $scope.gridOptions = {};
    $scope.addedReplicationId = -1;
    lockService.id = RateCodeReplicationModel.Area;
    
    initialize = function () {
        $scope.isInitializing = false;
        $scope.setGridOptions();
        $scope.loadData();
    };

    $scope.addReplication = function () {
        // Mark page as dirty and invalid on add.
        $scope.PageIsDirty = true;

         // Create the row object.
        var newRate = {
            D: true, // Dirty
            Id: $scope.addedReplicationId,
            Del: false, // IsDeleted
            From: '',
            To: '',
            Updateable: 1
        };

        // Insert a new row into the top of the dirtyArray & decrement the new row id.
        var newDirtyRow = { 'Id': $scope.addedReplicationId--, 'D': true, 'IsDataValid': false };
        $scope.dirtyArray.splice(0, 0, newDirtyRow);
        $scope.data.splice(0, 0, newRate);
        $scope.filterData();
    };

    $scope.deleteReplication = function (replication) {
        // clear error message
        replication.Del = true;
        $scope.PageIsDirty = true;
        replication.D = true;
        $scope.refreshLock();
        $scope.filterData();
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
            enablePagination: false,
            enableCellEditOnFocus: true,
            enableSorting: true,
            enableFiltering: false,
            enableGridMenu: true,
            exporterMenuExcel: false,   // requires the "excel-builder" npm module which is no longer maintained - https://github.com/stephenliberty/excel-builder.js
            exporterMenuPdf: false,
            rowHeight: 24,          // This causes a performance hit in IE (like everything) but makes the grid paint & scroll correctly.
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

                        // If all required fields are filled in mark row as valid.
                        if (rowEntity.From.length > 0 && rowEntity.To.length > 0) {
                            dirtyEditedRow[0].IsDataValid = true;
                        }

                        $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.EDIT);
                    }
                });
        };
    };

    $scope.typeaheadSelected = function (entity, selectedItem, isFrom) {
        $scope.PageIsDirty = true;  // Mark page as dirty to turn on Save/Cancel buttons.
        entity.D = true;         // Mark the row entity as Dirty.

        // This is a row entity column and uses dirtyArray.
        var dirtyEditedRow = $scope.dirtyArray.filter(function (row) {
            return row.Id === entity.Id;
        });

        if (isFrom) {
            entity.From = selectedItem.Rate;
            entity.FromD = selectedItem.Desc;
            dirtyEditedRow[0]['From'] = true;
        } else {
            entity.To = selectedItem.Rate;
            entity.ToD = selectedItem.Desc;
            dirtyEditedRow[0]['To'] = true;
        }

        // If all required fields are filled in mark row as valid.
        if (entity.From.length > 0 && entity.To.length > 0) {
            dirtyEditedRow[0].IsDataValid = true;
        }
    };

    $scope.resetColumnDefinitions = function () {
        var editHeaderCellTemplate = '<div role="columnheader" data-ng-class="{ \'sortable\': sortable }" ui-grid-one-bind-aria-labelledby-grid="col.uid + \'-header-text\'  + col.uid + ' +
            '\'-sortdir-text\'" aria-sort="none" aria-labelledby="1498216693300-uiGrid-0004-header-text 1498216693300-uiGrid-0004-sortdir-text"><div role="button" tabindex="0" ' +
            'class="ui-grid-cell-contents ui-grid-header-cell-primary-focus glyphicon glyphicon-pencil text-success" col-index="renderIndex" style="padding-left:10px; padding-right:0px">' +
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

        var dropdownTemplateFrom = '<div class="typeaheadcontainer"><input type="text" ' +
            'class="typeaheadcontrol" ' +
            'ng-model="MODEL_COL_FIELD" ' +
            'typeahead-append-to-body="true"' +
            'uib-typeahead="rateCode as rateCode.Show for rateCode in grid.appScope.rateCodes | filter:{Show:$viewValue}"' +
            'ng-required="true" ' +
            'typeahead-editable="false"' +
            'typeahead-min-length="0"'+
            'typeahead-on-select="grid.appScope.typeaheadSelected(row.entity, $item, true)" ' +
            '/></div>';

        var dropdownTemplateTo = '<div class="typeaheadcontainer"><input type="text" ' +
            'class="typeaheadcontrol" ' +
            'ng-model="MODEL_COL_FIELD" ' +
            'typeahead-append-to-body="true"' +
            'uib-typeahead="rateCode as rateCode.Show for rateCode in grid.appScope.rateCodes | filter:{Show:$viewValue}"' +
            'ng-required="true" ' +
            'typeahead-editable="false"' +
            'typeahead-min-length="0"' +
            'typeahead-on-select="grid.appScope.typeaheadSelected(row.entity, $item, false)" ' +
            '/></div>';

        var tempColDef = new Array(
                {
                    name: 'Manage', enableColumnMenu: false, cellTemplate: '<span title="Delete Replication" role="button" data-ng-hide="!grid.appScope.canEdit()" data-ng-click="grid.appScope.showConfirmDelete(row.entity)" class="glyphicon glyphicon-remove text-danger text-center padRight"></span>',
                    enableSorting: false, pinnedLeft: true, width: 70, cellClass: 'manage-rates center-icons center', enableCellEdit: false, exporterSuppressExport: true
                },
                {
                    name: 'Edited', enableColumnMenu: false, headerCellTemplate: editHeaderCellTemplate, field: 'D', cellTemplate: '<span title="Row Contains Edits" class="glyphicon glyphicon-pencil text-success text-center" data-ng-show="row.entity.D"></span>',
                    enableSorting: false, width: 30, cellClass: 'dirty-text center-icons center', enableCellEdit: false, exporterSuppressExport: true
                },
                {
                    name: 'From', field: 'From', width: 145, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass, editableCellTemplate: dropdownTemplateFrom
                },
                { name: 'From Description', field: 'FromD', width: 300, enableCellEdit: false, cellTooltip: true },
                { name: 'To', field: 'To', width: 145, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass, editableCellTemplate: dropdownTemplateTo } ,
                { name: 'To Description', field: 'ToD', width: 300, enableCellEdit: false, cellTooltip: true }
        );

        tempColDef[0].visible = $scope.canEdit(); // Edits column
        tempColDef[1].visible = $scope.canEdit(); // Edits column
        $scope.gridOptions.columnDefs = angular.copy(tempColDef);
    };

    $scope.showConfirmDelete = function (replication) {
        var confirmText = 'Do you want to delete this Rate Code Replication?';

        ConfirmDialog("Delete Rate Code Replication", confirmText, function () {
            $scope.deleteReplication(replication);
        });
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
                    $scope.reloadGrid(),
                    false);
            },
            function () {
                lockService.requestRejected();
            });
    };

    $scope.reloadGrid = function () {
        $(document).trigger("SHOW_LOADING_BOX");
        
        $scope.isDataLoading = true;

        $http({
            method: 'POST',
            url: createPostURL(RateCodeReplicationModel.controller, RateCodeReplicationModel.getAction),
            data: {}
        }).then(function successCallback(response) {

            if (response.data.ValidationMessages !== undefined && response.data.ValidationMessages !== null) {
                $rootScope.errors = response.data.ValidationMessages;
            } else {
                $rootScope.errors = [];
            }

            $scope.lockInfo = response.data.LockInfo;
            response.data.RateCodes.forEach(function (item) {
                item.Show = item.Rate + ' - ' + item.Desc;
            });
            $scope.rateCodes = response.data.RateCodes;
            $scope.data = [];
            $scope.gridOptions.data = [];
            $scope.gridOptions.columnDefs = new Array();
            $scope.resetColumnDefinitions();
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
            $scope.updateDetails(angular.copy(response.data.Replications));
            $scope.initializeDirtyGrid();
            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.isDataLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.confirmSave = function () {
        var dirtyItems = $scope.data.filter(function (item) { return item.D === true && (item.Del === false || item.Id > 0); });

        var changes = "<div class='left'>Deletions: <br/>";
        dirtyItems.forEach(function (item) {
            if (item.Del) {
                changes += "&nbsp;&nbsp;&nbsp;From: " + item.From + " To: " + item.To + "<br/>";
            }
        });

        changes += "Updates: <br/>";
        dirtyItems.forEach(function (item) {
            if (!item.Del) {
                var from = item.From || 'missing';
                var to = item.To || 'missing';
                changes += "&nbsp;&nbsp;&nbsp;(From) RateCode " + from + " will replicate over (To) RateCode " + to + "<br/>";
            }
        });

        changes += "</div>"

        ConfirmDialog("Confirm Save", "Are you sure you want to save all the following changes?<br/><br/>" + changes, function () {
            $scope.saveRateCodeReplication(dirtyItems);
        },
            function () {
                // No
                return false;
        });
    };

    /* Saves the Resource Code Replication. */
    $scope.saveRateCodeReplication = function (dirtyItems) {
        // clear error messages.
        $rootScope.errors = [];
        $(document).trigger("SHOW_LOADING_BOX");

        // Clear lock timers so there's no pop-up in the middle of a save
        $scope.clearLockTimers();

        var saveUrl = createPostURL(RateCodeReplicationModel.controller, RateCodeReplicationModel.saveAction);

        return $http({
            method: 'POST',
            url: saveUrl,
            data: JSON.stringify(dirtyItems)
        }).then(function successCallback(response) {
            $scope.releaseLock(true);

            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.refreshLock(); // Refresh lock to give user time to correct errors
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.updateDetails = function (newDetails) {
        if (newDetails) {
            newDetails.forEach(function (item) {
                // find RateCode for From
                var rate = $.grep($scope.rateCodes, function (e) { return e.Rate == item.From; });
                if (rate !== undefined && rate.length > 0) {
                    item.FromD = rate[0].Desc;
                }

                // find RateCode for To
                rate = $.grep($scope.rateCodes, function (e) { return e.Rate == item.To; });
                if (rate !== undefined && rate.length > 0) {
                    item.ToD = rate[0].Desc;
                }
            });
            $scope.data = newDetails;


        }
        $scope.PageIsDirty = false;
        $scope.filterData();
        $scope.isDataLoading = false;
    };

    $scope.showHideManageColumn = function () {
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

    $scope.reloadPage = function (reload) {
        if (reload === true) {
            $scope.PageIsDirty = false;
            $window.location.reload(true);
        }
    };

    $scope.releaseLock = function (reloadPage) {
        // See if we have a lock.
        if (lockService !== undefined && lockService.id !== undefined && $scope.canEdit()) {
            lockService.unlock().then(
                function (response) {
                    $scope.lockInfo = response.data.LockInfo;
                    $scope.showHideManageColumn();
                    lockService.processResponseRelease(response, $scope.clearLockTimers);
                    $scope.reloadPage(reloadPage);
                },
                function () {
                    lockService.requestRejected();
                    $scope.reloadPage(reloadPage);
                });
        } else {
            $scope.reloadPage(reloadPage);
        }
    };

    /* Loads the data. */
    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $scope.isDataLoading = true;
            $scope.isDataError = false;
            $scope.reloadGrid();
        }
    };

    $scope.initializeDirtyGrid = function () {
        var rowCount = $scope.data.length;
        $scope.dirtyArray = new Array(rowCount);
        for (var i = 0; i < rowCount; i++) {
            $scope.dirtyArray[i] = { 'Id': $scope.data[i].Id, 'From': false, 'To': false };
        }
    };

    $scope.confirmCancel = function () {
        if ($scope.PageIsDirty) {
            ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                $scope.replicationForm.$setPristine();
                $scope.releaseLock(true);
            },
            function () {
                // No
                return false;
            });
        } else {
            // If no changes, continue with reload
            $window.location.reload(true);
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
    initialize();
}]);