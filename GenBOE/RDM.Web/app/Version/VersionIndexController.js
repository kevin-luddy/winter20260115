angular.module('RDM').controller('VersionIndexController', ['$scope', '$http', '$rootScope', '$mdDialog', '$uibModal', '$timeout', 'uiGridConstants', 'VersionIndexModel', 'utilityService', 'RateFormatter', function ($scope, $http, $rootScope, $mdDialog, $uibModal, $timeout, uiGridConstants, VersionIndexModel, utilityService, RateFormatter) {
    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.isDataError = false;
    $scope.versionData = [];
    $scope.rateData = null;
    $scope.controller;
    $scope.displayAction;
    $scope.tolerance = 50;
    $scope.workInProgressName;
    $scope.defaultRateHistory = VersionIndexModel.defaultRateHistory;
    $scope.pprdFormattingTarget = VersionIndexModel.pprdFormattingTarget;
    $scope.rateFormattingTarget = VersionIndexModel.rateFormattingTarget;
    $scope.activeLockMessages = [];
    $rootScope.modalErrors = [];
    $scope.ReplicationValidationMessages = VersionIndexModel.data.ReplicationValidationMessages || [];
    $scope.shouldShowHelp = false;

    initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
        $scope.controller = VersionIndexModel.controller;
        $scope.compareAction = VersionIndexModel.compareAction;
        $scope.workInProgressName = VersionIndexModel.workInProgressName;
        $scope.rollbackConfirmationText = '';
        $scope.setGridOptions();
    };

    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $scope.isDataLoading = true;
            $scope.isDataError = false;
            $scope.versionData = VersionIndexModel.data;
            $scope.updateTolerance();
            $scope.isDataLoading = false;
            $scope.WIPVersion = $scope.versionData.AvailableVersions[0];   // Should be OrderByDescending in RevisionLoader.
            $scope.refreshActiveLockMessages();
            $rootScope.errors = $scope.ReplicationValidationMessages;
        }
    };

    // PPRD Tab Methods
    $scope.getPPRD = function () {
        $("#ToleranceSlider").css("visibility", "hidden");
        $scope.updateDiff(true);
    };

    // Rate Tab Methods
    $scope.getRates = function () {
        $("#ToleranceSlider").css("visibility", "visible");
        $scope.updateDiff(true);
    };

    $scope.setGridOptions = function () {

        $scope.gridOptions = {
            enableCellEditOnFocus: false,
            enableSorting: true,
            enableFiltering: true,
            enableGridMenu: true,
            enableColumnResizing: true,
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
                if (row.entity.S) {
                    return 'glyphicon glyphicon-globe text-center text-success';
                } else {
                    return 'glyphicon glyphicon-globe text-center text-danger';
                }
            }
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
        };

        // setup column definitions for the Years
        $scope.resetGridYearColumns();

        $scope.gridOptions.columnVirtualizationThreshold = $scope.gridOptions.columnDefs.length;
    };

    $scope.cellClass = function (grid, row, col, rowRenderIndex, colRenderIndex) {

        var classString = "";
        var i;

        // Don't put a line separator on top row - there is an ugly gap.
        var firstVisibleEntityId = 0;
        for (i = 0; i < grid.rows.length; i++) {
            if (grid.rows[i].visible) {
                firstVisibleEntityId = grid.rows[i].entity.Id;
                break;
            }
        }

        // Add top line to Deleted rows or current revision rows.
        if (firstVisibleEntityId > 0 && row.entity.Id !== firstVisibleEntityId &&
            (row.entity.CS === 'Deleted' || row.entity.R === $scope.versionData.FirstSelectedRevision.Id)) {
            classString = 'top-grid-line';
        }

        if (row.entity.R !== $scope.versionData.FirstSelectedRevision.Id) {
            return classString + ' disabled-text';
        } else {
            var rateObjectRef = col.colDef.field.substring(0, col.colDef.field.indexOf('.Val'));
            if (rateObjectRef.length > 0) {
                // This is a rate column.
                var rateObject = eval('row.entity.' + rateObjectRef);
                if (rateObject !== undefined && $scope.isOverTolerance(rateObject.PC)) {
                    return classString + ' invalid-text bold';
                }
            } else {
                // Highlight the Rate Code if any of it's values exceed percent change, in case they are out of view.
                if (col.colDef.field === 'Co') {

                    // Save the highest percent change in the row.entity.
                    if (row.entity.highestPercentChangeInRow === undefined) {
                        for (i = 0; i < row.entity.Values.length; i++) {
                            if (row.entity.highestPercentChangeInRow === undefined || row.entity.Values[i].PC > row.entity.highestPercentChangeInRow) {
                                if (row.entity.Values[i].PC !== null) {
                                    row.entity.highestPercentChangeInRow = row.entity.Values[i].PC;
                                }
                            }
                        }
                    }

                    if ($scope.isOverTolerance(row.entity.highestPercentChangeInRow)) {
                        return classString + ' invalid-text bold';
                    }
                }
            }
        }
        return classString;
    };

    /* Helper method to check if a percentChange value > the set tolerance. */
    $scope.isOverTolerance = function (percentChange) {
        return percentChange !== null && percentChange > $scope.tolerance;
    }

    $scope.resetGridYearColumns = function (startYear, endYear) {
        // setup column definitions for the Years
        var index = 0;
        var targetArea = $scope.rateFormattingTarget;
        var cellFilterText = 'formattedRate: row: "' + targetArea + '"';

        // The Dirty traits for Category, Description, and RateCode are monitored in the dirtyArray.
        // If you add a new column, individual color change after an edit will require adding that column to the array.
        // columnDefs: $scope.columnDefinitions,
        var tempColDef = new Array(
            { name: 'Chg', field: 'CS', enableSorting: false, pinnedLeft: true, width: 55, cellClass: $scope.cellClass, enableCellEdit: false },
            { name: 'Ver', field: 'RevisionIdLabel', enableSorting: false, pinnedLeft: true, width: 55, cellClass: $scope.cellClass, enableCellEdit: false },
            { name: 'Category', field: 'RCD', enableSorting: false, pinnedLeft: true, width: 150, cellClass: $scope.cellClass, enableCellEdit: false },
            { name: 'Rate Code', field: 'Co', enableSorting: false, pinnedLeft: true, width: 95, cellClass: $scope.cellClass, enableCellEdit: false },
            { name: 'Description', field: 'De', enableSorting: false, pinnedLeft: true, width: 150, cellClass: $scope.cellClass, enableCellEdit: false },
            { name: '>' + $scope.tolerance + '%', field: 'IsOverTolerance', enableSorting: false, pinnedLeft: true, width: 65, cellClass: $scope.cellClass, enableCellEdit: false }
        );

        if ($scope.rateData !== null && startYear !== undefined && endYear !== undefined) {
            for (var year = startYear; year <= endYear; year++) {
                tempColDef.push(
                    {
                        name: year,
                        field: 'Values[' + index + '].Val',
                        width: 75,
                        enableSorting: false,
                        enableFiltering: false,
                        cellClass: $scope.cellClass,
                        cellFilter: cellFilterText,
                        enableCellEdit: false
                    });

                index++;
            }
        }

        $scope.gridOptions.columnDefs = angular.copy(tempColDef);
    }

    $scope.FormatRates = function (grid, row, col, input) {
        var rateObjectRef = col.colDef.field.substring(0, col.colDef.field.indexOf('.Val'));
        grid.target = $scope.rateFormattingTarget;       // Tell the formatter what the target area is.
        if (rateObjectRef.length > 0) {
            return RateFormatter.Format(grid.target, row, input);
        }
        return input;
    };

    $scope.reloadGrid = function () {
        $(document).trigger("SHOW_LOADING_BOX");

        var getRatesurl = createPostURL(VersionIndexModel.rateController, $scope.compareAction, $scope.versionData.FirstSelectedRevision.Id);

        $http({
            method: 'POST',
            url: getRatesurl,
            data: { secondId: $scope.getSecondId() }
        }).then(function successCallback(response) {
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
            if (response.data.length > 0) {
                $scope.rateData = [];
                $scope.gridOptions.data = [];
                $scope.gridOptions.columnDefs = new Array();
                $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);

                if ($scope.gridApi !== undefined) {
                    $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
                }

                var yearCount = response.data[0].Values.length;
                if (yearCount > 0) {
                    var startYear = response.data[0].Values[0].Yr;
                    var endYear = response.data[0].Values[yearCount - 1].Yr;
                    $scope.resetGridYearColumns(startYear, endYear);
                } else {
                    $scope.resetGridYearColumns();
                }

                $scope.updateRates(angular.copy(response.data));
            } else {
                $scope.rateData = null;
            }

            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.updateRates = function (newRates) {
        $scope.rateData = newRates;

        // Put RevisionIdLabels in each row
        for (var i = 0; i < newRates.length; i++) {
            var revision = $scope.versionData.AvailableVersions.filter(function (v) {
                return v.Id === newRates[i].R;
            });

            if (revision !== undefined && revision[0] !== undefined && revision[0].Label !== undefined) {
                if (isNaN(parseFloat(revision[0].Label))) {
                    newRates[i].RevisionIdLabel = "WIP";
                } else {
                    newRates[i].RevisionIdLabel = revision[0].Label;
                }
            }
        }

        $scope.updateTolerance();
        $scope.gridOptions.data = $scope.rateData;
        $scope.PageIsDirty = false;

        // Clear validation errors.
        $rootScope.errors = [];
        $scope.isDataValid = true;
    };

    // Page methods.

    $scope.refreshActiveLockMessages = function () {
        if ($scope.versionData.ActiveLocks.length) {
            document.getElementById("Publish").disabled = true;
            document.getElementById("Rollback").disabled = true;
            $scope.versionData.ActiveLocks.forEach(function (activeLock) {
                $scope.activeLockMessages.push(activeLock.AreaString +
                    " is locked by " +
                    activeLock.LockedBy.DisplayName);
            });
        } else {
            document.getElementById("Publish").disabled = false;
            document.getElementById("Rollback").disabled = false;
            $scope.activeLockMessages = [];
        }
    };

    // Update the grid when a new version is selected
    $scope.updateDiff = function () {
        $(document).trigger("SHOW_LOADING_BOX");

        $http({
            method: 'POST',
            url: createPostURL(
                $scope.controller,
                $scope.compareAction,
                $scope.versionData.FirstSelectedRevision.Id),
            data: { id: $scope.versionData.FirstSelectedRevision.Id, secondId: $scope.getSecondId() }
        }).then(function successCallback(response) {
            $scope.versionData = response.data;
            $scope.updateTolerance();
            $scope.refreshActiveLockMessages();
        }, function errorCallback(response) {
        }).finally(function () {
            // Get Rates
            $scope.reloadGrid(); // Hide loading box called within reloadGrid()
        });
    };

    // Get the second selected ID, use -1 if nothing or previous version selected
    $scope.getSecondId = function () {
        var secondId = $scope.versionData.SecondSelectedRevision === null ? -1 : $scope.versionData.SecondSelectedRevision.Id;
        var secondSelection = $scope.versionData.AvailableCompareToVersions.find(x => x.Id === secondId);

        if (secondSelection && secondSelection.Label === "Previous Revision") {
            secondId = -1;
        }

        return secondId;
    };

    // Compare numerical values of each row to determine if they are > the set tolerance
    $scope.updateTolerance = function () {
        // change IsOverTolerance column label to reflect new tolerance value.
        if ($scope.gridOptions && $scope.gridOptions.columnDefs) {
            for (var i = 0; i < $scope.gridOptions.columnDefs.length; i++) {
                if ($scope.gridOptions.columnDefs[i].field === 'IsOverTolerance') {
                    $scope.gridOptions.columnDefs[i].displayName = '>' + $scope.tolerance + '%';
                    break;
                }
            }
        }

        // update IsOverTolerance flag for each row
        if ($scope.rateData) {
            for (var rowIdx = 0; rowIdx < $scope.rateData.length; rowIdx++) {
                if ($scope.rateData[rowIdx].CS === 'Edit') {
                    var isOverTolerance = false;
                    for (var valIdx = 0; valIdx < $scope.rateData[rowIdx].Values.length; valIdx++) {
                        if ($scope.isOverTolerance($scope.rateData[rowIdx].Values[valIdx].PC)) {
                            isOverTolerance = true;
                            break;
                        }
                    }

                    // Rows marked "Edit" come in pairs - set IsOverTolerance flag for both rows in the pair
                    $scope.rateData[rowIdx].IsOverTolerance = isOverTolerance;
                    $scope.rateData[rowIdx + 1].IsOverTolerance = isOverTolerance;

                    rowIdx++ // skip second row in the pair
                } else {
                    // Added or Deleted rows
                    $scope.rateData[rowIdx].IsOverTolerance = false;
                }
            }
        }

        if ($scope.gridApi !== undefined) {
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
        }
    }

    //Publish Modal
    $scope.showPublishModal = function () {
        // Remove any existing editor with the same ID (elementName).  If we don't, it will sometimes cause problems during partial page refreshes.
        tinymce.EditorManager.execCommand('mceRemoveEditor', true, 'editor1');
        tinymce.EditorManager.execCommand('mceRemoveEditor', true, 'editor2');

        var model = {
            workInProgressName : $scope.workInProgressName,
            revision: $scope.versionData.FirstSelectedRevision.Revision,
            history : $scope.versionData.WorkInProgressHistory,
            releaseNotes: $scope.versionData.ReleaseNotes,
            validateRatesController: VersionIndexModel.controller,
            validateRatesAction: VersionIndexModel.validateRatesAction
        };

        // open the Edit Modal
        var modalInstance = $uibModal.open({
            templateUrl: VersionIndexModel.baseUrl + 'app/Version/Publish.html',
            controller: 'PublishController',
            windowTopClass: 'bootstrap publish-modal',
            windowClass: 'bootstrap',
            backdrop: 'static',
            backdropClass: 'bootstrap',
            resolve: { model: [function () { return model; }] }
        });
        
        modalInstance.rendered.then(function () {
            utilityService.makeModalDraggableAndResizable('#publishModal', 774, 1100);
        });

        modalInstance.result.then(function (model) {
            if (model !== undefined) {
                // model was passed back inside OK button, now update the real settings from the clone
                $scope.versionData.ReleaseNotes = model.releaseNotes;
                $scope.versionData.WorkInProgressHistory = model.history;

                $scope.publishPPRDClick();
            }
        }, function () { });
    };

    // Validates the rates to present any warnings before continuing with Publish.
    $scope.validateRates = function () {
        utilityService.validateRates(VersionIndexModel.controller,
            VersionIndexModel.validateRatesAction,
            null,
            $scope.showPublishModal);
    }

    // on publish click, redirect to report page
    $scope.publishPPRDClick = function () {
        // clear error messages.
        $scope.isDataValid = true;
        $(document).trigger("SHOW_LOADING_BOX");

        // Call the publish controller action.
        var publishUrl = createPostURL(VersionIndexModel.controller, VersionIndexModel.publishAction);

        return $http({
            method: 'POST',
            url: publishUrl,
            data: JSON.stringify({ revision: $scope.versionData.FirstSelectedRevision.Revision, history: $scope.versionData.WorkInProgressHistory, releaseNotes: $scope.versionData.ReleaseNotes })
        }).then(function successCallback(response) {
            $(document).trigger("DISPLAY_NOTIFICATION", response.data.Message);
            $(document).trigger("HIDE_LOADING_BOX");
            if (response.data.Status) {
                // on success, redirect to reports page
                window.location.href = createPostURL(VersionIndexModel.reportsController, 'Index');
            }
            $scope.versionData.ActiveLocks = response.data.ActiveLocks;
            $scope.refreshActiveLockMessages();
        }, function errorCallback(response) {
            $(document).trigger("DISPLAY_NOTIFICATION", 'Error publishing revision.');
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    // Rollback Modal
    $scope.showRollbackModal = function (ev) {
        $mdDialog.show({
            contentElement: '#rollbackDialog',
            parent: angular.element(document.body)
        });
        $('#rollbackConfirmInput').focus();
    };

    $scope.rollbackConfirmClick = function () {
        $mdDialog.hide();

        // clear error messages.
        $scope.isDataValid = true;
        $(document).trigger("SHOW_LOADING_BOX");

        // Call the rollback controller action.
        var rollbackUrl = createPostURL(VersionIndexModel.controller, VersionIndexModel.rollbackAction);

        return $http({
            method: 'POST',
            url: rollbackUrl,
            data: JSON.stringify({ revision: $scope.versionData.FirstSelectedRevision.Revision })
        }).then(function successCallback(response) {
            $(document).trigger("DISPLAY_NOTIFICATION", response.data.Message);
            $(document).trigger("HIDE_LOADING_BOX");
            if (response.data.Status) {
                // on success, redirect to rates page
                window.location.href = createPostURL(VersionIndexModel.rateController, 'Index');
            }
            $scope.versionData.ActiveLocks = response.data.ActiveLocks;
            $scope.refreshActiveLockMessages();
        }, function errorCallback(response) {
            $(document).trigger("DISPLAY_NOTIFICATION", 'Error rolling back revision.');
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };
    
    $scope.closeRollbackModal = function () {
        $mdDialog.hide();
    };

    $scope.showHelp = function (event) {
        $scope.shouldShowHelp = true;

        // use a short timeout to avoid popping up when mouse happens to pass over button quickly
        // shouldShowHelp gets set to false on mouseout event to help with this
        $timeout(function () {
            if ($scope.shouldShowHelp) {
                var helpButton = $(event.target);
                var helpDialog = helpButton.next();

                helpDialog.css("top", $(helpButton).position().top - helpDialog.height());
                helpDialog.css("left", $(helpButton).position().left + $(helpButton).width() + 5);

                helpDialog.removeClass('display-none');
            }
        }, 300);        
    };

    $scope.hideHelp = function (event) {
        $scope.shouldShowHelp = false;
        var helpDialog = $(event.target).next();
        helpDialog.addClass('display-none');
    };

    initialize();
}]);