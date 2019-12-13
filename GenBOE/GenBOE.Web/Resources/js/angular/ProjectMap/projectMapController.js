(function () {
    'use strict';
    var app = angular.module('genboe');

    app.controller('projectMapController', ['$scope', 'uiGridConstants', 'projectMapService', '$uibModal', 'isReadOnly', 'allowGridEdit', 'isBucketized', 'resources', 'performingOrgs', 'legacyResources', 'pageSize', '$sce', '$timeout', function ($scope, uiGridConstants, projectMapService, $uibModal, isReadOnly, allowGridEdit, isBucketized, resources, performingOrgs, legacyResources, pageSize, $sce, $timeout) {
        $scope.PageIsDirty = false;
        $scope.pageSize = pageSize;
        $scope.isReadOnly = isReadOnly;
        $scope.allowGridEdit = allowGridEdit; // When set to false, the grid will paginate the data
        $scope.isBucketized = isBucketized;
        $scope.showValidationMessage = false; // Holds a bool that decides whether to hide or show the validation message box above the grid
        $scope.isInitializing = true;
        $scope.isExporting = false;
        $scope.isOffloadExporting = false;
        var rationaleDisplayName = "Rationale";
        var hasDiscreteData = false;
        $scope.numberDiscreteMonths = 0;
        $scope.displayImportedNotification = false;
        $scope.displayValidationMessages = false;  // used inside $scope.initialize to determine whether to show the validation messages after a reload of data
        $scope.totalHours = 0;
        $scope.totalDollars = 0;
        $scope.totalRows = 0;
        $scope.resources = resources;
        $scope.legacyResources = legacyResources;
        $scope.performingOrgs = performingOrgs;
        var paginationOptions = {
            pageNumber: 1
        };

        $scope.isGridEditAllowed = function () { return $scope.allowGridEdit; };

        $scope.gridOptions = {
            paginationPageSizes: [$scope.pageSize],
            paginationPageSize: $scope.pageSize,
            useExternalPagination: !$scope.allowGridEdit,
            useExternalSorting: false,
            enableGridMenu: true,
            enableColumnResizing: !$scope.isBucketized,
            enableSorting: $scope.allowGridEdit,
            enableFiltering: $scope.allowGridEdit,
            enableColumnMenus: true,
            enableHorizontalScrollbar: uiGridConstants.scrollbars.ALWAYS,
            enableCellEditOnFocus: $scope.isGridEditAllowed()
        };

        $scope.setGridOptions = function() {
            // Setup ui-grid options
            $scope.gridOptions.columnDefs = [];
            if ($scope.isGridEditAllowed()) {
                $scope.gridOptions.columnDefs.push({
                    name: 'Delete',
                    cellTemplate:
                        '<div class="bootstrap"><span title="Delete Row" role="button" data-ng-click="grid.appScope.deleteRow(row)" class="glyphicon bootstrap glyphicon-remove text-danger text-center padRight"></span></div>',
                    enableSorting: false,
                    enableFiltering: false,
                    pinnedLeft: true,
                    width: 55,
                    cellClass: 'grid-delete center-icons center',
                    enableCellEdit: false,
                    exporterSuppressExport: true
                });
            }
            
            $scope.gridOptions.columnDefs.push({ field: 'WbsNumber', cellEditableCondition: $scope.isGridEditAllowed, displayName: 'WBS Number', width: '100' });
            $scope.gridOptions.columnDefs.push({ field: 'ActivityID', cellEditableCondition: $scope.isGridEditAllowed, displayName: 'Activity ID', width: '100' });
            $scope.gridOptions.columnDefs.push({
                field: 'ActivityName', width: '120', cellEditableCondition: $scope.isGridEditAllowed, cellTemplate: '<div style="height:100%" data-ng-click="grid.appScope.showLargeText(row.entity.ActivityName, row.entity, \'ActivityName\', 500000)">{{row.entity.ActivityName}}</div>'
            });
            $scope.gridOptions.columnDefs.push({
                field: 'WbsElementTitle', displayName: 'WBS Element Title', width: '120', cellEditableCondition: $scope.isGridEditAllowed, cellTemplate: '<div style="height:100%" data-ng-click="grid.appScope.showLargeText(row.entity.WbsElementTitle, row.entity, \'WbsElementTitle\', 255)">{{row.entity.WbsElementTitle}}</div>'
            });
            $scope.gridOptions.columnDefs.push({
                field: 'InitialResource', height: '100%', width: '130', cellEditableCondition: $scope.isGridEditAllowed, displayName: 'Activity Type Code', editableCellTemplate: 'ui-grid/dropdownEditor',
                editDropdownIdLabel: 'Label', editDropdownValueLabel: 'Label', editDropdownOptionsArray: $scope.resources });
            $scope.gridOptions.columnDefs.push({
                field: 'CostCenter', width: '100', cellEditableCondition: $scope.isGridEditAllowed, editableCellTemplate: 'ui-grid/dropdownEditor',
                editDropdownIdLabel: 'Label', editDropdownValueLabel: 'Label', editDropdownOptionsArray: $scope.performingOrgs });
            $scope.gridOptions.columnDefs.push({
                field: 'LegacyID', displayName: 'Legacy Resource', width: '100', cellEditableCondition: $scope.isGridEditAllowed, editableCellTemplate: 'ui-grid/dropdownEditor',
                cellFilter: "griddropdown:editDropdownOptionsArray:editDropdownIdLabel:editDropdownValueLabel:row.entity.LegacyResourceID",
                editDropdownIdLabel: 'Id', editDropdownValueLabel: 'Label', editDropdownOptionsArray: $scope.legacyResources });
            $scope.gridOptions.columnDefs.push({ field: 'StartDate', width: '100', cellEditableCondition: $scope.isGridEditAllowed, editableCellTemplate: '<input jqdatepicker type="text" data-ng-model="row.entity.StartDate" style="width:75px;" />' });
            $scope.gridOptions.columnDefs.push({ field: 'EndDate', width: '100', cellEditableCondition: $scope.isGridEditAllowed, editableCellTemplate: '<input jqdatepicker type="text" data-ng-model="row.entity.EndDate" style="width:75px;" />' });
            $scope.gridOptions.columnDefs.push({ field: 'Clin', cellEditableCondition: $scope.isGridEditAllowed, displayName: 'CLIN', width: '100' });
            $scope.gridOptions.columnDefs.push({ field: 'SowNumber', cellEditableCondition: $scope.isGridEditAllowed, displayName: 'SOW Number', width: '100' });
            $scope.gridOptions.columnDefs.push({
                field: 'SowTitle', displayName: 'SOW Title', width: '100', cellEditableCondition: $scope.isGridEditAllowed, enableCellEdit: false, cellTemplate: '<div style="height:100%" data-ng-click="grid.appScope.showLargeText(row.entity.SowTitle, row.entity, \'SowTitle\', 255)">{{row.entity.SowTitle}}</div>'
            });
            $scope.gridOptions.columnDefs.push({ field: 'Task', width: '200', cellEditableCondition: $scope.isGridEditAllowed, enableCellEdit: false, cellTemplate: '<div style="height:100%" data-ng-click="grid.appScope.showLargeText(row.entity.Task, row.entity, \'Task\', 500000)">{{row.entity.Task}}</div>' });
            $scope.gridOptions.columnDefs.push({ field: 'Hours', width: '100', cellEditableCondition: $scope.isGridEditAllowed });
            $scope.gridOptions.columnDefs.push({ field: 'Dollars', width: '100', cellFilter: 'currency', cellEditableCondition: $scope.isGridEditAllowed });
            $scope.gridOptions.columnDefs.push({
                field: 'Rationale', displayName: rationaleDisplayName, width: '330', cellEditableCondition: $scope.isGridEditAllowed, enableCellEdit: false, cellTemplate: '<div style="height:100%" data-ng-click="grid.appScope.showLargeText(row.entity.Rationale, row.entity, \'Rationale\', 500000)">{{row.entity.Rationale}}</div>'
            });
            $scope.gridOptions.columnDefs.push({
                field: 'CamName', displayName: 'CAM Name', width: '100', cellEditableCondition: $scope.isGridEditAllowed, enableCellEdit: false, cellTemplate: '<div style="height:100%" data-ng-click="grid.appScope.showLargeText(row.entity.CamName, row.entity, \'CamName\', 255)">{{row.entity.CamName}}</div>'
            });
            $scope.gridOptions.columnDefs.push({
                field: 'Category', width: '100', cellEditableCondition: $scope.isGridEditAllowed, enableCellEdit: false, cellTemplate: '<div style="height:100%" data-ng-click="grid.appScope.showLargeText(row.entity.Category, row.entity, \'Category\', 255)">{{row.entity.Category}}</div>'
            });
            $scope.gridOptions.columnDefs.push({ field: 'Offload', width: '100', cellEditableCondition: $scope.isGridEditAllowed, cellTemplate: '<div class="grid-padding"><span>{{row.entity.Offload | uppercaseFilter}}</span><div title="{{row.entity.OffloadWarningText}}" data-ng-show="row.entity.OffloadWarning" class="warning-row-icon"></div></div>' });
            $scope.gridOptions.columnDefs.push({
                field: 'ClassOfCost', displayName: 'Class of Cost', width: '100', cellEditableCondition: $scope.isGridEditAllowed, editableCellTemplate: 'ui-grid/dropdownEditor',
                editDropdownIdLabel: 'id', editDropdownValueLabel: 'Label', editDropdownOptionsArray: [
                    { id: 'REC', Label: 'REC' },
                    { id: 'NRE', Label: 'NRE' },
                    { id: 'DNR', Label: 'DNR' }
                ] });
            $scope.gridOptions.columnDefs.push({
                field: 'AddDelete', displayName: 'Add/Delete', width: '100', cellEditableCondition: $scope.isGridEditAllowed, editableCellTemplate: 'ui-grid/dropdownEditor',
                editDropdownIdLabel: 'id', editDropdownValueLabel: 'Label', editDropdownOptionsArray: [
                    { id: 'A', Label: 'A' },
                    { id: 'D', Label: 'D' }
                ]
            });
            $scope.gridOptions.columnDefs.push({ field: 'TieredPercentage', displayName: '% Tier', width: '100', cellEditableCondition: $scope.isGridEditAllowed });

            if (hasDiscreteData) {
                var minM = 1;
                var maxM = $scope.numberDiscreteMonths;

                for (var m = minM; m <= maxM; m++) {
                    $scope.gridOptions.columnDefs.push({
                        enableFiltering: false,
                        enableSorting: false,
                        name: 'M' + m.toString(),
                        field: 'DiscreteMonths[' + (m - 1).toString() + ']',
                        width: '100',
                        cellEditableCondition: $scope.isGridEditAllowed
                    });
                }
            }
            $scope.gridOptions.columnVirtualizationThreshold = $scope.gridOptions.columnDefs.length;
        }

        $scope.setPageIsDirty = function (value)
        {
            $scope.PageIsDirty = value;
            //Copy workspace popup could not be handled via angular so using jquery to show/hide
            if (value) {
                $("#CopyFromExistingBOE-Button").hide();
            } else {
                $("#CopyFromExistingBOE-Button").show();
            }
        }

        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            $scope.gridApi.edit.on.afterCellEdit($scope,
                function (rowEntity, colDef, newValue, oldValue) {
                    if (!(newValue === oldValue)) {
                        $scope.setPageIsDirty(true); // Mark page as dirty to turn on Save/Cancel buttons.
                    }
                });
            if (!$scope.allowGridEdit) {
                $scope.gridApi.pagination.on.paginationChanged($scope, function (newPage, pageSize) {
                    paginationOptions.pageNumber = newPage;
                    $scope.loadPagedData();
                });
            }
        };

        // Page Functionality -------------------------------------------------
        $scope.isBusy = function() {
            return $scope.loading || $scope.isExporting || $scope.isSaving;
        }

        $scope.cancel = function () {
            $scope.setPageIsDirty(false);
            location.reload();
        }

        $scope.deleteRow = function (row) {
            var i = $scope.gridOptions.data.indexOf(row.entity);
            $scope.gridOptions.data.splice(i, 1);
            $scope.setPageIsDirty(true); // Mark page as dirty to turn on Save/Cancel buttons.
        };

        $scope.addRow = function () {

            var newRow = {
                ActivityID: '',
                ActivityName: '',
                AddDelete: '',
                CamName: '',
                Category: '',
                ClassOfCost: '',
                Clin: '',
                CostCenter: '',
                DiscreteMonths: hasDiscreteData ? new Array(204) : null,
                Dollars: null,
                EndDate: '',
                Hours: '',
                InitialResource: '',
                LegacyResourceID: '',
                Offload: false,
                OffloadWarning: false,
                OffloadWarningText: '',
                Rationale: '',
                SowNumber: '',
                SowTitle: '',
                StartDate: '',
                Task: '',
                WbsElementTitle: '',
                WbsNumber: '',
                TieredPercentage: ''
            };

            // Insert a new row
            $scope.gridOptions.data.splice(0, 0, newRow);
            $scope.setPageIsDirty(true);
        };

        $scope.hideValidationMessages = function () {
            $scope.showValidationMessage = false;
        };

        $scope.saveProjectMapGrid = function () {
            $scope.showValidationMessage = false;
            $("#CopyFromExistingBOE-Button").hide();
            $scope.isSaving = true;
            $scope.loading = true;

            projectMapService.saveProjectMapGrid(JSON.stringify($scope.gridOptions.data))
                .then(function (response) {
                    var validationResult = response.data;
                    if (validationResult.message) {
                        $scope.validationMessages = validationResult.message.split(/\n/gi).filter(Boolean);
                    }
                    if (validationResult.status === "error") {
                        $scope.validationTitle = "Save failed, fix errors and try again";
                        $scope.validationClass = "error-validation-box";
                        $scope.showValidationMessage = true;
                    }
                    else if (validationResult.status === "warning") {
                        $scope.validationTitle = "Save completed with warnings";
                        $scope.validationClass = "warning-validation-box";
                        $scope.showValidationMessage = true;
                    }
                    else if (validationResult.status === "success") {
                        $scope.displayImportedNotification = true;
                        $scope.setPageIsDirty(false);
                        // Update the grid
                        $scope.initialize();
                    }
                })
                .catch(function() {
                    $scope.validationMessages = ["An unexpected error occurred while saving your changes. Please try again."];
                    $scope.validationTitle = "Error Occurred";
                    $scope.validationClass = "error-validation-box";
                    $scope.showValidationMessage = true;
                })
                .finally(function() {
                    $scope.isSaving = false;
                    $scope.loading = false;
                });
        };

        // Calls projectMap service and sets grid data upon return
        $scope.initialize = function () {
            $scope.loading = true;
            $scope.isInitializing = false;
            if ($scope.allowGridEdit) {
                $scope.loadData();
            } else {
                $scope.loadPagedData();
            }
        };

        /* Gets a date string in MM/yyyy format from a JSON date. */
        $scope.getDateStringFromJsonDate = function (jsonDate) {
            var date = new Date(jsonDate);
            var dateMonth = date.getMonth() + 1;
            var leadingZero = (dateMonth < 10) ? '0' : '';
            return leadingZero + dateMonth + '/' + date.getFullYear();
        }

        /* Loads the Data */
        $scope.loadData = function () {
            if (!$scope.isInitializing) {
                $scope.isDataLoading = true;
                $scope.isDataError = false;
                $scope.loading = true;
                projectMapService.loadProjectMapData().then(function (response) {
                    var projectMapData = response.data;
                    // Convert the Dates from the JSON format to the proper string format.
                    for (var i = 0; i < projectMapData.length; i++) {
                        projectMapData[i].StartDate = $scope.getDateStringFromJsonDate(parseInt(projectMapData[i].StartDate.substr(6)));
                        projectMapData[i].EndDate = $scope.getDateStringFromJsonDate(parseInt(projectMapData[i].EndDate.substr(6)));
                    }

                    // If bucketized list then change name
                    if (projectMapData && projectMapData.length > 0 && projectMapData[0].DiscreteMonths) {
                        rationaleDisplayName = "Basis Of Estimate";
                        hasDiscreteData = true;
                        $scope.numberDiscreteMonths = projectMapData[0].DiscreteMonths.length;
                    }

                    $scope.CalculateTotalsForHeader(projectMapData);

                    $scope.setGridOptions();

                    $scope.gridOptions.data = projectMapData;

                    $scope.showValidationMessage = false;

                    if ($scope.displayImportedNotification) {
                        // Show success notification
                        RaiseNotification('Update successful -> ' + projectMapData.length + ' records were saved');
                        $scope.displayImportedNotification = false;
                    }

                    if ($scope.displayValidationMessages) {
                        $scope.showValidationMessage = true;
                        $scope.displayValidationMessages = false;
                    }
                })
                .catch(function () {
                    //handle error
                })
                .finally(function () {
                    $scope.loading = false;
                    $scope.isDataLoading = false;
                    $scope.isSaving = false;
                    $scope.setPageIsDirty(false);
                    $scope.refreshDirtyPageConfirmation();
                });
            }
        };

        /* Loads the Paged Data */
        $scope.loadPagedData = function () {
            if (!$scope.isInitializing) {
                $scope.isDataLoading = true;
                $scope.isDataError = false;
                $scope.loading = true;

                projectMapService.loadProjectMapPagedData(paginationOptions.pageNumber).then(function (response) {
                    $scope.gridOptions.totalItems = response.data.TotalRows;
                    $scope.totalRows = response.data.TotalRows;
                    $scope.totalHours = response.data.TotalHours;
                    $scope.totalDollars = response.data.TotalDollars;

                    var projectMapData = response.data.Data;
                    // Convert the Dates from the JSON format to the proper string format.
                    for (var i = 0; i < projectMapData.length; i++) {
                        projectMapData[i].StartDate = $scope.getDateStringFromJsonDate(parseInt(projectMapData[i].StartDate.substr(6)));
                        projectMapData[i].EndDate = $scope.getDateStringFromJsonDate(parseInt(projectMapData[i].EndDate.substr(6)));
                    }

                    // If bucketized list then change name
                    if (projectMapData && projectMapData.length > 0 && projectMapData[0].DiscreteMonths) {
                        rationaleDisplayName = "Basis Of Estimate";
                        hasDiscreteData = true;
                        $scope.numberDiscreteMonths = projectMapData[0].DiscreteMonths.length;
                    }

                    $scope.setGridOptions();

                    $scope.gridOptions.data = projectMapData;

                    $scope.showValidationMessage = false;

                    if ($scope.displayImportedNotification) {
                        // Show success notification
                        RaiseNotification('Update successful -> ' + $scope.totalRows + ' records were saved');
                        $scope.displayImportedNotification = false;
                    }
                })
                .catch(function() {
                    //handle error
                })
                .finally(function() {
                    $scope.loading = false;
                    $scope.isDataLoading = false;
                    $scope.isSaving = false;
                    $scope.setPageIsDirty(false);
                    $scope.refreshDirtyPageConfirmation();
                });
            }
        };

        //Overwrites any existing events bound to onbeforeunload (e.g. iboe)
        $scope.refreshDirtyPageConfirmation = function() {
            window.onbeforeunload = null;
            window.onbeforeunload = function () {
                if ($scope.PageIsDirty) {
                    return "Changes have not been saved. Are you sure you want to navigate away?";
                }
            };
        }

        /* Calculates Totals needed for the Header. */
        $scope.CalculateTotalsForHeader = function (projectMapData) {

            // Reset totals.
            $scope.totalHours = 0;
            $scope.totalDollars = 0;
            $scope.totalRows = 0;

            if (projectMapData && projectMapData.length > 0) {
                $scope.totalRows = projectMapData.length;
                for (var i = 0; i < projectMapData.length; i++) {
                    if (projectMapData[i]) {
                        // Calculate Hours Total.
                        if (projectMapData[i].Hours) {
                            $scope.totalHours += projectMapData[i].Hours;
                        }

                        // Calculate Dollars Total.
                        if (projectMapData[i].Dollars) {
                            $scope.totalDollars += projectMapData[i].Dollars;
                        }
                    }
                }
            }
        }
        
        // exports the grid into an excel download
        $scope.exportGrid = function (offload) {
            var timeoutTime = 2000;

            if (offload === 'true') {
                $scope.isOffloadExporting = true;
                timeoutTime = 3000;
            }
            else {
                $scope.isExporting = true;
            }
            var workspaceName = window.location.protocol + '//' + window.location.host + '/' + window.location.pathname;
            GenWidget.prototype.performExport(workspaceName + '/Workspace/ExportProjectMapData?offload=' + offload);


            // export is done via attaching an iframe, wait an arbitrary # of seconds (2-3) until showing the export button again to stop double-click
            $timeout(function () {
                if (offload == 'true') {
                    $scope.isOffloadExporting = false;
                }
                else {
                    $scope.isExporting = false;
                }
            }, timeoutTime);

            // this is needed so IE does not stop animating gif(s) on the page
            return false;
        };

        $scope.showImportDialog = function () {

            $scope.showValidationMessage = false;
            // open the Import Modal

            var modalInstance = $uibModal.open({
                templateUrl: '/Resources/ProjectMapImport.html',
                controller: 'projectMapImportController',
                windowTopClass: 'bootstrap project-map-dialog',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap',
                size: 'lg'
            });

            modalInstance.result.then(function (results) {
                if (results != undefined && results.Status) {
                    $scope.displayImportedNotification = true;

                    var showValidation = false;
                    if (results.Message != '' && results.Message != undefined) {
                        showValidation = true;
                        $scope.validationMessages = results.Message.split(/\n/gi).filter(Boolean);
                        $scope.validationTitle = "Save completed with warnings";
                        $scope.validationClass = "warning-validation-box";
                    }

                    // Update the grid
                    if ($scope.allowGridEdit) {
                        $scope.displayValidationMessages = showValidation;
                        $scope.initialize();
                    } else {
                        // there is a bug in pagination controls that does not reset the total items, just reload the page
                        // If the bug is every fixed, remove the else block and just have the innards of the if block above so that validation is shown
                        $scope.loading = true;
                        location.reload();
                    }
                }
            }, function () { });
        };

        // shows the large text in a popup modal
        $scope.showLargeText = function (text, rowValue, columnName, maxLength) {
            if (!$scope.allowGridEdit) {
                return;
            }

            var modalInstance = $uibModal.open({
                templateUrl: '/Resources/ProjectMapLargeText.html',
                controller: 'projectMapLargeTextController',
                windowTopClass: 'bootstrap project-map-dialog',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap',
                resolve: { model: [function () { return { 'text': text, 'maxLength': maxLength } }] }
            });

            modalInstance.result.then(function (results) {
                if (results != undefined) {
                    if (rowValue[columnName] !== results) {
                        rowValue[columnName] = results;
                        $scope.setPageIsDirty(true);
                    }
                }
            }, function () { });
        };

        $scope.initialize();

    }]);

    app.filter("dateFilter",
        function () {
            return function (item) {
                if (item != null) {
                    return new Date(parseInt(item.substr(6)));
                }
                return "";
            };
        });

    app.filter("uppercaseFilter",
        function () {
            return function (item) {
                if (item != null) {
                    return item.toString().toUpperCase();
                }
                return "";
            };
        });
})();


