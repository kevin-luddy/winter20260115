angular.module('RDM').controller('RateController', ['$scope', '$http', '$filter', '$window', '$rootScope', '$mdDialog', '$uibModal', '$log', 'lockService', 'utilityService', 'uiGridConstants', 'uiGridExporterConstants', 'RateFormatter', 'RATE_FORMATS', 'RateModel', function ($scope, $http, $filter, $window, $rootScope, $mdDialog, $uibModal, $log, lockService, utilityService, uiGridConstants, uiGridExporterConstants, RateFormatter, RATE_FORMATS, RateModel) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.isDataError = false;
    $scope.data = [];
    $scope.filteredData = [];
    $scope.selectedVersion = { Id: RateModel.SelectedRevisionId };
    $scope.isRdmAdminUser = false;
    $scope.isRdmCobraAdminUser = false;
    $scope.versions = [];
    $scope.rateCategories = [];
    $scope.sections = [];
    $scope.lockInfo;
    $scope.showEditLockTimeoutExpiration = false;
    $rootScope.errors = [];
    $rootScope.importRateCodesModalErrors = [];
    $scope.insertRateCodes = [];
    $scope.updateRateCodes = [];
    $rootScope.importRatesModalErrors = [];
    $scope.importRateCodesWorking = false;
    $scope.importRatesWorking = false;
    $scope.addedRateId = -1;
    $scope.zeroYear = new Date().getFullYear() - 1;
    $scope.PageIsDirty = false;
    $scope.canEdit = function () { return $scope.lockInfo !== undefined && !$scope.lockInfo.IsReadOnly; };
    $scope.gridOptions = {};
    $scope.isExportingRates = false;
    $scope.isExportingRateCodes = false;
    $scope.showImportResults = false;
    $scope.timeoutTime = 3000;
    $scope.defaultRateHistory = RateModel.defaultRateHistory;
    $scope.FormattingTarget = RateModel.FormattingTarget;
    $scope.defaultFormatPrecision = { precision: RateModel.DefaultFormatPrecision };
    $scope.ResourceTypeLabor = RateModel.ResourceTypeLabor;
    lockService.id = RateModel.Area;
    $scope.ReplicationErrors = [];
    $scope.cobraReminderRateList = [];

    $scope.FormatRates = function (grid, row, col, input) {
        var rateObjectRef = col.colDef.field.substring(0, col.colDef.field.indexOf('.Val'));
        grid.target = $scope.FormattingTarget;       // Tell the formatter what the target area is.
        if (rateObjectRef.length > 0) {
            // return RateFormatter.Format(grid, row, col, input);
            return RateFormatter.Format(grid.target, row, input);
        }
        return input;
    };

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
            // Category, Description and RateCode are required.
            var cellValue = grid.getCellValue(row, col);
            if (cellValue === undefined || cellValue.length === 0) {
                return 'value-required';
            }
            
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

    $scope.yearCellClass = function (grid, row, col, rowRenderIndex, colRenderIndex) {
        if (grid.appScope.lockInfo.IsReadOnly) {
            return 'right disabled-text';
        } else {
            var rateObjectRef = col.colDef.field.substring(0, col.colDef.field.indexOf('.Val'));
            // This is a rate column.
            var dirtyObject = eval('row.entity.' + rateObjectRef);

            // If we get this error, the RateDetailLoader did not fill in all the rate years.
            if (dirtyObject === undefined) {
                $rootScope.errors = [{ ValidationIssue: String.format('Rate information is missing for Rate Code {0}, for the year {1}. Please contact your administrator.', row.entity.Co, col.colDef.name) }];
                return 'right no-spinner';
            }

            if (dirtyObject !== undefined) {
                if (dirtyObject.isInValid !== undefined && dirtyObject.isInValid) {
                    return 'right invalid-text bold';
                } else if (dirtyObject.D !== undefined && dirtyObject.D) {
                    return 'right dirty-text bold';
                }
            }
        }
        return 'right no-spinner';    // Chrome is making number input elements show spinner.
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
            // Do not define exporterCsvFilename before the onRegisterApi or you can not modify the name for subsequent exports.
            // exporterCsvFilename: 'Rates_RDM_164_2017-06-29.csv',
            exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
            exporterFieldCallback: function (grid, row, col, input) {
                return $scope.FormatRates(grid, row, col, input);
            },
            rowHeight: 18,          // This causes a performance hit in IE (like everything) but makes the grid paint & scroll correctly.
            globeStyle: function (row) {  // Handles coloring mappings globe.
                if (row.entity.S && row.entity.S > 0) {
                    return 'glyphicon glyphicon-globe text-center text-success';
                } else {
                    return 'glyphicon glyphicon-globe text-center text-danger';
                }
            },

            data: $scope.filteredData
        };

        /* Get a reference to the gridApi so that we can auto-scroll later. */
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            $scope.gridApi.core.on.renderingComplete($scope,
                function () {
                    // For potential performance fix - We need to find events at the end of the grid refresh to take down
                    // the Loading... box. This event may help, but it may not be the first time 
                    // it is called. Left this here because we need to circle back.
                    // console.log("renderingComplete");
                });

            $scope.gridApi.edit.on.afterCellEdit($scope,
                function (rowEntity, colDef, newValue, oldValue) {
                    if (!(newValue === oldValue)) {
                        $scope.PageIsDirty = true;      // Mark page as dirty to turn on Save/Cancel buttons.
                        rowEntity.D = true;         // Mark the row entity as Dirty.

                        var rateObjectRef = colDef.field.substring(0, colDef.field.indexOf('.Val'));
                        if (rateObjectRef !== undefined && rateObjectRef.length > 0) {
                            // This is a rate column.
                            var dirtyObject = eval('rowEntity.' + rateObjectRef);
                            var numericVal = Number(newValue);
                            var format = $scope.defaultFormatPrecision;

                            if (rowEntity.RCD !== undefined) {
                                format = RATE_FORMATS[$scope.FormattingTarget + rowEntity.RCD];
                            }

                            if (dirtyObject !== undefined) {
                                dirtyObject.D = true;
                                if (!isNaN(numericVal) && isFinite(numericVal)) {
                                    var twoDecimalRegex = new RegExp('^\\d+(\\.\\d{0,' + format.precision + '})?$');
                                    var allowNegativeTwoDecimalRegex = new RegExp('^[+-]?\\d+(\\.\\d{0,' + format.precision + '})?$');
                                    var isNonLaborEscalation = rowEntity.RCD.startsWith('Non-Labor Escalation');

                                    if (!isNonLaborEscalation && twoDecimalRegex.test(numericVal) === false
                                        || isNonLaborEscalation && allowNegativeTwoDecimalRegex.test(numericVal) === false) {
                                        // Create format function if we haven't already.
                                        if (!String.format) {
                                            String.format = function (format) {
                                                var args = Array.prototype.slice.call(arguments, 1);
                                                return format.replace(/{(\d+)}/g, function (match, number) {
                                                    return typeof args[number] !== 'undefined'
                                                        ? args[number]
                                                        : match
                                                        ;
                                                });
                                            };
                                        }

                                        if (isNonLaborEscalation) {
                                            $rootScope.errors = [{ ValidationIssue: String.format(RateModel.RatePrecisionErrorAllowNegative, rowEntity.Co, colDef.name, newValue, rowEntity.RCD, format.precision) }];
                                        } else {
                                            $rootScope.errors = [{ ValidationIssue: String.format(RateModel.RatePrecisionError, rowEntity.Co, colDef.name, newValue, rowEntity.RCD, format.precision) }];
                                        }
                                        $scope.isDataValid = false;
                                        dirtyObject.isInValid = true;
                                    } else {
                                        $scope.isDataValid = true;
                                        dirtyObject.isInValid = false;
                                        $rootScope.errors = [];
                                    }
                                } else {
                                    $rootScope.errors = [{ ValidationIssue: newValue + ' - Rates must be numeric values.' }];
                                    $scope.isDataValid = false;
                                    dirtyObject.isInValid = true;
                                }
                            }
                        } else {
                            // This is a row entity column and uses dirtyArray.
                            var dirtyEditedRow = $scope.dirtyArray.filter(function (row) {
                                return row.Id === rowEntity.Id;
                            });
                            dirtyEditedRow[0][colDef.field] = true;

                            if (colDef.field === 'RCD' && rowEntity.RCD.length > 0) {
                                var rateCategory = $scope.rateCategories.filter(function (category) { return category.Label === rowEntity.RCD; });
                                if (rateCategory !== undefined) {
                                    rowEntity.RC = rateCategory[0].Id;
                                }
                            }

                            // If all required fields are filled in mark row as valid.
                            if (rowEntity.RC > 0 && rowEntity.Co.length > 0 && rowEntity.De.length > 0) {
                                dirtyEditedRow[0].IsDataValid = true;
                                $scope.isDataValid = true;
                            }
                        }
                        $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.EDIT);
                    }
                });
        };

        // setup column definitions for the Years
        $scope.resetGridYearColumns();

        $scope.gridOptions.columnVirtualizationThreshold = $scope.gridOptions.columnDefs.length;
    };

    // Page Functionality -------------------------------------------------
    $scope.addRate = function () {
        // Mark page as dirty and invalid on add.
        $scope.PageIsDirty = true;
        $scope.isDataValid = false;

        // Create the years from MinDate to MaxDate
        var addedYears = [];
        for (var i = $scope.selectedVersion.StartYear; i <= $scope.selectedVersion.EndYear; i++) {
            addedYears.push({
                Id: -1,
                RCId: -1, // RateCodeId
                Yr: i,
                Val: null,
                D: true, // Dirty
                isInValid: false
            });
        }

        // Create the row object.
        var newRate = {
            D: true, // Dirty
            C1: 0, // Code1
            De: '', // Description
            Gen: false, // GenerateAdditionalDirectLaborRates
            Id: $scope.addedRateId,
            Del: false, // IsDeleted
            RC: '', // Rate Category
            Co: '', // Rate Code
            RD: '', // Rate Description
            RD1: '', 
            RD2: '',
            RD3: '',
            RD4: '',
            RD5: '',
            RD6: '',
            RD7: '',
            RCId: 0, // Resource Class Ids
            RCId1: 0,
            RCId2: 0,
            RCId3: 0,
            RCId4: 0,
            RCId5: 0,
            RCId6: 0,
            RCId7: 0,
            RS: '', // RateSet
            Ra: 0,
            RT: 0,
            GBPId: 0,   // Burden Pool Ids
            CBPId: 0,
            R: $scope.selectedVersion.Id, // RevisionId
            S: 0,        // (Section) for now 0 will make the mappings globe red
            Updateable: 1,
            Values: addedYears
        };

        // Insert a new row into the top of the dirtyArray & decrement the new row id.
        var newDirtyRow = { 'Id': $scope.addedRateId--, 'RC': false, 'RCD': false, 'Co': false, 'D': true, 'IsDataValid': false };
        $scope.dirtyArray.splice(0, 0, newDirtyRow);
        $scope.data.splice(0, 0, newRate);
        $scope.filterData();
    };

    $scope.deleteRate = function (rate) {
        // clear error message
        $scope.isDataValid = true;
        rate.Del = true;
        $scope.PageIsDirty = true;
        rate.D = true;
        $scope.refreshLock();
        $scope.filterData();
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
        var getRatesurl = createPostURL(RateModel.controller, RateModel.getRatesByVersionAction, $scope.selectedVersion.Id);
        $http({
            method: 'POST',
            url: getRatesurl,
            data: { }
        }).then(function successCallback(response) {
            if (!$scope.versions.length) {
                $scope.versions = response.data.Versions;
                $scope.selectedVersion = $scope.findVersionById($scope.selectedVersion.Id, response.data.Versions);
            }
            $scope.isRdmAdminUser = response.data.AdminUser;
            $scope.isRdmCobraAdminUser = response.data.CobraAdminUser;
            $scope.rateCategories = response.data.RateCategories;
            $scope.sections = response.data.Sections;
            $scope.rateTypes = response.data.RateTypes;
            $scope.resourceTypes = response.data.ResourceTypes;
            $scope.resourceClasses = response.data.ResourceClasses;
            $scope.commercialBurdenPools = response.data.CommercialBurdenPools;
            $scope.governmentBurdenPools = response.data.GovernmentBurdenPools;
            $scope.lockInfo = response.data.LockInfo;
            $scope.ZeroedOut = false;
            // TEMPERAMENTAL GRID WARNING - Reloading data into an existing grid is no trivial matter and the code
            // and order of events below were hard won and if modified, even a little, must be tested with the following scenario 
            // to be sure it is working:
            // 1) WIP has a few years (2010 - 2015) defined in the Revision and loads with the Index() server method.
            // 2) A previous version (with different data) and a larger timeframe (2002 - 2040) is reloaded here.
            // 3) WIP is reloaded here - truncating the timeframe (2010-2015).
            // Error - The data can look right at a glance but be in the wrong year or be missing.
            // The keys here are - when notifyDataChange is called, that original data is wiped out, and that angular.copy
            // is used here and in resetGridYearColumns(), and gridOptions is created from scratch each time.
            // Questions: Deb Ray
            $scope.data = [];
            $scope.gridOptions.data = [];
            $scope.gridOptions.columnDefs = new Array();
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
            $scope.resetGridYearColumns();
            $scope.updateRates(angular.copy(response.data.Rates), isLock);
            $scope.initializeDirtyGrid();
            if (!isLock) {
                $scope.releaseLock();
            }

            if (response.data.ReplicationValidationMessages !== undefined && response.data.ReplicationValidationMessages !== null) {
                $scope.ReplicationValidationMessages = response.data.ReplicationValidationMessages;
                if (isLock) {
                    $rootScope.errors = $scope.ReplicationValidationMessages;
                }
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

    $scope.resetGridYearColumns = function () {
        // setup column definitions for the Years
        var firstVisibleYear = new Date().getFullYear() - $scope.defaultRateHistory; // User wants to hide earlier years.
        var index = 0;
        var targetArea = $scope.FormattingTarget;
        var cellFilterText = 'formattedRate: row: "' + targetArea + '"';

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

        // The Dirty traits for Category, Description, and RateCode are monitored in the dirtyArray.
        // If you add a new column, individual color change after an edit will require adding that column to the array.
        // columnDefs: $scope.columnDefinitions,
        var tempColDef = new Array(
                {
                    name: 'Manage', cellTemplate: '<span title="Delete Rate" role="button" data-ng-hide="!grid.appScope.canEdit()" data-ng-click="grid.appScope.showConfirmDelete(row.entity)" class="glyphicon glyphicon-remove text-danger text-center padRight"></span>' +
                    '<span title="Edit Mappings" role="button" data-ng-hide="row.entity.hidePopup" data-ng-click="grid.appScope.showMappings(row.entity, $event)" data-ng-class="grid.options.globeStyle(row)"></span>',
                    enableSorting: false, enableFiltering: false, pinnedLeft: true, width: 80, cellClass: 'manage-rates center-icons center', enableCellEdit: false, exporterSuppressExport: true
                },
                {
                    name: 'Edited', headerCellTemplate: editHeaderCellTemplate, headerCellClass: 'glyphicon glyphicon-pencil text-success text-center', field: 'D', visible: false, cellTemplate: '<span title="Row Contains Edits" class="glyphicon glyphicon-pencil text-success text-center" data-ng-show="row.entity.D"></span>',
                    enableSorting: true, enableFiltering: false, pinnedLeft: true, width: 55, cellClass: 'dirty-text center-icons center', enableCellEdit: false, exporterSuppressExport: true
                },
                {
                    name: 'Category', field: 'RCD', cellEditableCondition: $scope.canEdit, pinnedLeft: true, width: 150, editableCellTemplate: 'ui-grid/dropdownEditor',
                    editDropdownIdLabel: 'Label', editDropdownValueLabel: 'Label', editDropdownOptionsArray: $scope.rateCategories, cellClass: $scope.cellClass, cellTooltip: true
                },
                { name: 'Rate Code', field: 'Co', pinnedLeft: true, width: 145, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass },
                { name: 'Description', field: 'De', pinnedLeft: true, width: 250, cellEditableCondition: $scope.canEdit, cellClass: $scope.cellClass, cellTooltip: true }
            );

        for (var year = $scope.selectedVersion.StartYear; year <= $scope.selectedVersion.EndYear; year++) {
            tempColDef.push(
                {
                    name: year,
                    type: 'number',
                    field: 'Values[' + index + '].Val',
                    width: 75,
                    enableSorting: false,
                    enableFiltering: false,
                    cellEditableCondition: $scope.canEdit,
                    cellClass: $scope.yearCellClass,
                    cellFilter: cellFilterText,
                    visible: year >= firstVisibleYear
                });

            index++;
        }

        tempColDef[1].visible = $scope.canEdit(); // Edits column
        $scope.gridOptions.columnDefs = angular.copy(tempColDef);
    }

    $scope.saveRateGrid = function (releaseLock) {
        // clear error messages.
        $scope.isDataValid = true;
        $(document).trigger("SHOW_LOADING_BOX");

        // Clear lock timers so there's no pop-up in the middle of a save
        $scope.clearLockTimers();

        var saveUrl = createPostURL(RateModel.controller, RateModel.saveAction);

        var dirtyRates = $scope.data.filter(function (rate) { return rate.D === true && (rate.Del === false || rate.Id > 0); });

        return $http({
            method: 'POST',
            url: saveUrl,
            data: JSON.stringify(dirtyRates)
        }).then(function successCallback(response) {
            $scope.initializeDirtyGrid();
            $scope.updateRates(response.data, !releaseLock); 
            if (releaseLock) {
                $scope.releaseLock();
            } else {
                // start a new timer on Save & Continue.
                $scope.refreshLock();
            }

            $(document).trigger("HIDE_LOADING_BOX");

            // Remind user about cobra mapping for any new rates
            var newRates = dirtyRates.filter(function (rate) { return rate.Id < 0; });
            if (newRates.length > 0) {
                $scope.showCobraReminderDialog(newRates);
            }
        }, function errorCallback(response) {
            $scope.refreshLock(); // Refresh lock to give user time to correct errors
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.showCobraReminderDialog = function (newRates) {
        $scope.cobraReminderRateList = [];
        newRates.forEach(function (rate) {
            $scope.cobraReminderRateList.push(rate.Co);
        });

        $mdDialog.show({
            contentElement: '#cobraReminderDialog',
            parent: angular.element(document.body)
        });
    };

    $scope.closeCobraReminderDialog = function () {
        $mdDialog.hide();
        $scope.cobraReminderRateList = [];
    };

    $scope.showConfirmDelete = function (rate) {
        var confirmText = 'Do you want to delete this Rate?';

        if (rate.HM) { // has pro pricer burden rate mappings
            confirmText =
                'This rate is mapped to one or more Burden Pools. Deleting this rate will also delete the mappings. Do you want to delete this rate?';
        }

        ConfirmDialog("Delete Rate", confirmText, function () {
            $scope.deleteRate(rate);
        });
    };

    $scope.showMappings = function (rate) {
        var clonedContent = angular.copy(rate);
        $scope.openEditMappingsModal(clonedContent);
    };

    $scope.updateRates = function (newRates, isLock) {
        if (!$scope.canEdit() || !isLock) {
            newRates = utilityService.expandRateData(newRates);
        }

        $scope.data = newRates;
        $scope.PageIsDirty = false;
        $scope.filterData();
        $scope.isDataLoading = false;
        // Clear validation errors.
        $rootScope.errors = [];
        $scope.isDataValid = true;
    };

    /* Verifies the RateCode is unique against all other rate codes */
    $scope.verifyRateCodeUniqueness = function (updatedRate) {
        $rootScope.errors = [];
        var unique = true;
        $scope.data.forEach(function (rate) {
            if (updatedRate.Id !== rate.Id) {
                if (updatedRate.Co === rate.Co) {
                    unique = false;
                }
            }
        });

        if (!unique) {
            $rootScope.errors = [{ ValidationIssue: "RateCode is not unique: " + updatedRate.Co }];
        }
    };

    $scope.showHideManageColumn = function () {
        $scope.gridOptions.columnDefs[1].visible = $scope.canEdit(); // Edits column
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
            $scope.dirtyArray[i] = { 'Id': $scope.data[i].Id, 'RC': false, 'RCD': false, 'Co': false };
        }
    };

    // Modals --------------------------------------------------------------
    // Edit Mappings Modal

    $scope.openEditMappingsModal = function (content) {
        // open the Edit Modal
        var modalInstance = $uibModal.open({
            templateUrl: '/app/Rate/EditMappings.html',
            controller: 'EditMappingsController',
            windowTopClass: 'bootstrap edit-mappings-modal',
            windowClass: 'bootstrap',
            backdrop: 'static',
            backdropClass: 'bootstrap',
            size: 'lg',
            resolve: {
                model: [function () { return content; }],
                rateModel: [function () { return $scope; }]
            }
        });

        modalInstance.rendered.then(function () {
            utilityService.makeModalDraggableAndResizable('#editMappingsModal', 810, 800);
        });

        modalInstance.result.then(function (content) {
            if (content !== undefined) {
                // user hit OK, we will assume it is Dirty.
                $scope.PageIsDirty = true;
            }
            $scope.isDataValid = true;
            if ($scope.canEdit()) {
                $scope.refreshLock(RateModel.Area);
            }
            $scope.filterData();
        },
            function () { });
    };

    // Import Rate Codes Modal Dialog functions
    $scope.showImportRateCodesModal = function (ev) {
        // Clear error messages & initialize.
        $rootScope.importRateCodesModalErrors = [];
        $scope.insertRateCodes = [];
        $scope.updateRateCodes = [];
        $scope.importRateCodesFileName = null;
        $("#importRateCodesFile")[0].value = '';
        $scope.importRateCodesWorking = false;

        $mdDialog.show({
            contentElement: '#importRateCodesDialog',
            parent: angular.element(document.body)
        });
    };

    $scope.reshowImport = function () {
        $scope.showImportResults = false;
        $rootScope.importRateCodesModalErrors = [];
        $scope.insertRateCodes = [];
        $scope.updateRateCodes = [];
    };

    $scope.closeImportRateCodesModal = function () {
        $scope.showImportResults = false;
        $rootScope.importRateCodesModalErrors = [];
        $mdDialog.hide();
    };

    importRateCodesFileSelect = function () {
        if ($("#importRateCodesFile").val().endsWith('.xlsx') || $("#importRateCodesFile").val().endsWith('.xlsm')) {
            $('#validateRateCodes').prop('disabled', false);
            $('#importRateCodes').prop('disabled', false);
            $scope.importRateCodesFileName = $("#importRateCodesFile").val();
        }
        else {
            $('#validateRateCodes').prop('disabled', true);
            $('#importRateCodes').prop('disabled', true);
        }
    };

    // validates rate codes
    $scope.validateRateCodesClick = function () {
        $(document).trigger("SHOW_LOADING_BOX");

        // clear error messages.
        $rootScope.importRateCodesModalErrors = [];
        $scope.isDataValid = true;
        $scope.importRateCodesWorking = true;

        // Call the validate rate codes controller action.
        var validateRateCodesUrl = createPostURL(RateModel.controller, RateModel.validateRateCodesAction);
        var formData = new FormData($('#importRateCodesForm').get(0));

        // Get the current Dirty State for the page in case of an error.
        var pageIsDirty = $scope.PageIsDirty;

        // I tried using $http, but I could not get the file into the Request.Files object on the server.
        $.ajax({
            context: this,    // makes $scope available inside success/failure functions.
            type: "POST",
            url: validateRateCodesUrl,
            contentType: false,
            data: formData,
            dataType: 'json',
            encode: true,
            async: true,
            processData: false,
            cache: false,
            success: function (response) {
                // show the validation results
                $scope.showImportResults = true;
                $rootScope.importRateCodesModalErrors = response.ValidationErrors;
                $scope.insertRateCodes = response.InsertRateCodes;
                $scope.updateRateCodes = response.UpdateRateCodes;
                $scope.importRateCodesWorking = false;
                $scope.$digest();
                $(document).trigger("HIDE_LOADING_BOX");
            },
            error: function (response) {
                $scope.showImportResults = false;
                $scope.insertRateCodes = [];
                $scope.updateRateCodes = [];
                $scope.importRateCodesWorking = false;
                $scope.getRateCodeErrorMessages(response.responseJSON);
                $(document).trigger("HIDE_LOADING_BOX");
            }
        });
    };

    // imports rate codes
    $scope.importRateCodesClick = function () {
        $(document).trigger("SHOW_LOADING_BOX");

        // clear error messages.
        $rootScope.importRateCodesModalErrors = [];
        $scope.isDataValid = true;
        $scope.importRateCodesWorking = true;

        // Call the import rate codes controller action.
        var importRateCodesUrl = createPostURL(RateModel.controller, RateModel.importRateCodesAction);
        var formData = new FormData($('#importRateCodesForm').get(0));

        // Get the current Dirty State for the page in case of an error.
        var pageIsDirty = $scope.PageIsDirty;

        // I tried using $http, but I could not get the file into the Request.Files object on the server.
        $.ajax({
            context: this,    // makes $scope available inside success/failure functions.
            type: "POST",
            url: importRateCodesUrl,
            contentType: false,
            data: formData,
            dataType: 'json',
            encode: true,
            async: true,
            processData: false,
            cache: false,
            success: function (response) {
                // close the modal
                $scope.closeImportRateCodesModal();
                $scope.importRateCodesWorking = false;
                $scope.updateRates(response, false);
                $scope.releaseLock();
                $(document).trigger("DISPLAY_NOTIFICATION", 'Import successful and changes saved');
                $(document).trigger("HIDE_LOADING_BOX");

                // Remind user about cobra mapping for any new rates
                var newRates = [];
                $scope.insertRateCodes.forEach(function (rc) {
                    newRates.push({ Co: rc.RateCode });
                });

                if (newRates.length > 0) {
                    $scope.showCobraReminderDialog(newRates);
                }
            },
            error: function (response) {
                $scope.importRateCodesWorking = false;
                $scope.getRateCodeErrorMessages(response.responseJSON);
                $(document).trigger("HIDE_LOADING_BOX");
            }
        });
    };

    $scope.getRateCodeErrorMessages = function (responseJSON) {
        $rootScope.importRateCodesModalErrors = [];
        if (responseJSON) {
            if (responseJSON.MessageList && responseJSON.MessageList.length > 0) {
                $rootScope.importRateCodesModalErrors = responseJSON.MessageList;
            } else if (responseJSON.Details) {
                var idx = responseJSON.Details.indexOf('<br />');   // strip off trailing stack trace information
                if (idx > -1) {
                    $rootScope.importRateCodesModalErrors = [{ ValidationIssue: responseJSON.Details.substring(0, idx) }];
                } else {
                    $rootScope.importRateCodesModalErrors = [{ ValidationIssue: responseJSON.Details }];
                }
            }
            $scope.$digest();
        }
    };

    $scope.getRateErrorMessages = function (responseJSON) {
        $rootScope.importRatesModalErrors = [];
        if (responseJSON) {
            if (responseJSON.MessageList && responseJSON.MessageList.length > 0) {
                $rootScope.importRatesModalErrors = responseJSON.MessageList;
            } else if (responseJSON.Details) {
                var idx = responseJSON.Details.indexOf('<br />');   // strip off trailing stack trace information
                if (idx > -1) {
                    $rootScope.importRatesModalErrors = [{ ValidationIssue: responseJSON.Details.substring(0, idx) }];
                } else {
                    $rootScope.importRatesModalErrors = [{ ValidationIssue: responseJSON.Details }];
                }
            }
            $scope.$digest();
        }
    };

    $scope.exportRateCodes = function (ev) {
        $scope.isExportingRateCodes = true;
        var dateString = new Date().toLocaleDateString();
        dateString = dateString.replace('/', '-');

        $scope.gridOptions.exporterCsvFilename = 'RateCodes_RDM_' + $scope.selectedVersion.Label + '_' + dateString + '.xlsx';
        setTimeout(function () { $scope.timeoutFuncRateCodes(); }, $scope.timeoutTime);
        DownloadFile('ExportRateCodes', RateModel.controller, RateModel.exportRateCodesAction, $scope.selectedVersion.Id);
    };

    $scope.timeoutFuncRateCodes = function () { $scope.$apply(function () { $scope.isExportingRateCodes = false; }); }

    // Import Rates Modal Dialog functions
    $scope.showImportRatesModal = function (ev) {
        // Clear error messages & initialize.
        $rootScope.importRatesModalErrors = [];
        $scope.importRatesFileName = null;
        $("#importRatesFile")[0].value = '';
        $scope.importRatesWorking = false;

        $mdDialog.show({
            contentElement: '#importRatesDialog',
            parent: angular.element(document.body)
        });
    };

    $scope.closeImportRatesModal = function () {
        $rootScope.importRatesModalErrors = [];
        $mdDialog.hide();
    };

    importRatesFileSelect = function () {
        if ($("#importRatesFile").val().endsWith('.xlsx') || $("#importRatesFile").val().endsWith('.xlsm')) {
            $('#importRates').prop('disabled', false);
            $scope.importRatesFileName = $("#importRatesFile").val();
        }
        else {
            $('#importRates').prop('disabled', true);
        }
    };

    // imports rates
    $scope.importRatesClick = function () {
        $(document).trigger("SHOW_LOADING_BOX");

        // clear error messages.
        $rootScope.importRatesModalErrors = [];
        $scope.isDataValid = true;
        $scope.importRatesWorking = true;

        // Call the import rates controller action.
        var importRatesUrl = createPostURL(RateModel.controller, RateModel.importRatesAction);
        var formData = new FormData($('#importRatesForm').get(0));

        // Get the current Dirty State for the page in case of an error.
        var pageIsDirty = $scope.PageIsDirty;

        // I tried using $http, but I could not get the file into the Request.Files object on the server.
        $.ajax({
            context: this,    // makes $scope available inside success/failure functions.
            type: "POST",
            url: importRatesUrl,
            contentType: false,
            data: formData,
            dataType: 'json',
            encode: true,
            async: true,
            processData: false,
            cache: false,
            success: function (response) {
                // close the modal
                $scope.closeImportRatesModal();
                $scope.importRatesWorking = false;

                $scope.updateRates(response, false);
                $scope.releaseLock();
                $(document).trigger("DISPLAY_NOTIFICATION", 'Import successful and changes saved');
                $(document).trigger("HIDE_LOADING_BOX");
            },
            error: function (response) {
                $scope.importRatesWorking = false;
                $scope.getRateErrorMessages(response.responseJSON);
                $(document).trigger("HIDE_LOADING_BOX");
            }
        });
    };

    $scope.exportRates = function (ev) {
        $scope.isExportingRates = true;
        var dateString = new Date().toLocaleDateString();
        dateString = dateString.replace('/', '-');

        $scope.gridOptions.exporterCsvFilename = 'Rates_RDM_' + $scope.selectedVersion.Label + '_' + dateString + '.csv';
        setTimeout(function () { $scope.timeoutFuncRates(); }, $scope.timeoutTime);
        $scope.gridApi.exporter.csvExport(uiGridExporterConstants.VISIBLE, uiGridExporterConstants.ALL);
    };

    $scope.timeoutFuncRates = function () { $scope.$apply(function () { $scope.isExportingRates = false; }); }

    // Zero Out Modal Dialog functions
    $scope.showZeroOutModal = function (ev) {

        // Clear error messages.
        $scope.zeroOutModalErrors = [];

        $mdDialog.show({
            contentElement: '#ZeroOutDialog',
            parent: angular.element(document.body)
        });
        $('#zeroOutConfirmInput').focus();
    };

    $scope.zeroOutClick = function () {
        var valuesChanged = false;

        // Mark page as dirty on Zero Out.
        $scope.PageIsDirty = true;
        $scope.data.forEach(function (rate) {
            if (rate.RCD === 'Direct Labor' || rate.RCD === 'Service Center') {
                var valueRate = rate.Values.filter(function (valRate) {
                    return valRate.Yr === $scope.zeroYear;
                });
                if (valueRate.length > 0) {
                    rate.D = true; // Mark the row as edited to show pencil and allow sort.
                    valueRate[0].Val = 0.0;
                    valueRate[0].D = true;
                    valuesChanged = true;
                }
            }
        });
        // Hide the button & enable Save button.
        $scope.ZeroedOut = true;
        $scope.isDataValid = true;

        if (valuesChanged) {
            var catColumns = $scope.gridApi.grid.columns.filter(function (col) {
                return col.field === 'D';              // this has to match the column definitions for Dirty column
            });
            $scope.gridApi.grid.sortColumn(catColumns[0], uiGridConstants.DESC);
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.EDIT);
        }

        $scope.closeZeroOutModal();
    };

    $scope.closeZeroOutModal = function () {
        $mdDialog.hide();
    };

    $scope.confirmCancel = function () {
        if ($scope.PageIsDirty) {
            ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                // Yes
                $scope.reloadGrid(false);
            },
            function () {
                // No
                return false;
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
