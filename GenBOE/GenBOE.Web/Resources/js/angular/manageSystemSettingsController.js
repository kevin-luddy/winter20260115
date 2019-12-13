(function () {
    'use strict';
    var app = angular.module('genboe');
    app.controller('manageSystemSettingsController', ['$scope', '$rootScope', 'ManageSystemSettingsModelView', '$http', 'uiGridConstants', function ($scope, $rootScope, ManageSystemSettingsModelView, $http, uiGridConstants) {
        $rootScope.errors = [];
        $scope.isInitializing = false;
        $scope.isDataLoading = true;
        $scope.data = [];
        $scope.filteredData = [];
        $scope.PageIsDirty = false;
        $scope.gridOptions = {};

        $scope.initialize = function () {
            $scope.isInitializing = false;
            $scope.setGridOptions();
            $scope.loadData();
        };

        $scope.cellClass = function (grid, row, col, rowRenderIndex, colRenderIndex) {
            // This is a row-entity column, since we only have one Dirty flag for the row,
            // I broke out each column so interface is consistant.
            if (row.entity.D === true && $scope.dirtyArray !== undefined) {
                var dirtyEditedRow = $scope.dirtyArray.filter(function (daRow) {
                    return daRow.Key === row.entity.Key;
                });
                if (dirtyEditedRow[0] && dirtyEditedRow[0][col.field] === true) {
                    return 'dirty-text';
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
                                return row.Key === rowEntity.Key;
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

        $scope.resetColumnDefinitions = function () {
            var editHeaderCellTemplate = '<div role="columnheader" data-ng-class="{ \'sortable\': sortable }" ui-grid-one-bind-aria-labelledby-grid="col.uid + \'-header-text\'  + col.uid + ' +
                '\'-sortdir-text\'" aria-sort="none" aria-labelledby="1498216693300-uiGrid-0004-header-text 1498216693300-uiGrid-0004-sortdir-text"><div role="button" tabindex="0" ' +
                'class="ui-grid-cell-contents ui-grid-header-cell-primary-focus fa fa-pencil edit-document-icon" col-index="renderIndex" style="padding-left:10px; padding-right:0px">' +
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
                        name: 'Edited', enableColumnMenu: false, headerCellTemplate: editHeaderCellTemplate, field: 'D', cellTemplate: '<span title="Row Contains Edits" class="fa fa-pencil edit-document-icon" data-ng-show="row.entity.D"></span>',
                        enableSorting: false, width: 30, cellClass: 'dirty-text center-icons center', enableCellEdit: true, exporterSuppressExport: true
                    },
                    { name: 'Key', field: 'Key', width: 200, cellEditableCondition: false, cellTooltip: true },
                    { name: 'Value', field: 'Value', width: 550, cellEditableCondition: true, cellClass: $scope.cellClass, cellTooltip: true }
            );

            tempColDef[0].visible = true; // Edits column
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

        $scope.reloadGrid = function () {
            $(document).trigger("SHOW_LOADING_BOX");
            $scope.isDataLoading = true;

            $http({
                method: 'POST',
                url: GenSession.CreateSystemAdminPostURL(ManageSystemSettingsModelView.controller, ManageSystemSettingsModelView.getSystemSettingsAction),
                data: {}
            }).then(function successCallback(response) {

                if (response.data.ValidationMessages !== undefined && response.data.ValidationMessages !== null) {
                    $rootScope.errors = response.data.ValidationMessages;
                } else {
                    $rootScope.errors = [];
                }

                $scope.data = response.data;
                $scope.gridOptions.data = [];
                $scope.gridOptions.columnDefs = new Array();
                $scope.resetColumnDefinitions();
                $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
                $scope.PageIsDirty = false;
                $scope.filterData();
                $scope.isDataLoading = false;
                $scope.initializeDirtyGrid();
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            });
        };

        $scope.showHideManageColumn = function () {
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
        };

        $scope.reloadPage = function (reload) {
            if (reload === true) {
                $scope.PageIsDirty = false;
                $window.location.reload(true);
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

        /* Saves the System Settings. */
        $scope.saveSystemSettings = function () {
            // clear error messages.
            $rootScope.errors = [];
            $(document).trigger("SHOW_LOADING_BOX");

            var saveUrl = GenSession.CreateSystemAdminPostURL(ManageSystemSettingsModelView.controller, ManageSystemSettingsModelView.saveSystemSettingsAction);
            var dirtyItems = $scope.data.filter(function (item) { return item.D; });  // return all items that are dirty

            return $http({
                method: 'POST',
                url: saveUrl,
                data: JSON.stringify(dirtyItems)
            }).then(function successCallback(response) {
                $scope.systemSettingsForm.$setPristine();
                $scope.loadData();
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $rootScope.errors = response.data.MessageList;
                $(document).trigger("HIDE_LOADING_BOX");
            });
        };

        $scope.initializeDirtyGrid = function () {
            var rowCount = $scope.data.length;
            $scope.dirtyArray = new Array(rowCount);
            for (var i = 0; i < rowCount; i++) {
                $scope.dirtyArray[i] = { 'Key': $scope.data[i].Key, 'Value': false };
            }
        };

        $scope.confirmCancel = function () {
            if ($scope.systemSettingsForm.$dirty) {
                Session.confirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                    // go to admin section
                    $scope.systemSettingsForm.$setPristine();
                    window.location.hash = '#';
                });
            } else {
                // If no changes, go to admin section
                window.location.hash = '#';
            }
        };

        window.onbeforeunload = function () {
            // Confirm leaving/refreshing page if the page is dirty
            if ($scope.PageIsDirty) {
                return "Changes have not been saved. Are you sure you want to navigate away?";
            }
        };

        // Start Initialization -----------------------------------------
        $scope.initialize();
    }]);
})();