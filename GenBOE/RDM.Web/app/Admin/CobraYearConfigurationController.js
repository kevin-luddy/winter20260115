angular.module('RDM').controller('CobraYearConfigurationController', ['$scope', '$http', '$timeout', '$rootScope', 'uiGridConstants', 'utilityService', 'CobraYearConfigurationModel', function ($scope, $http, $timeout, $rootScope, uiGridConstants, utilityService, CobraYearConfigurationModel) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.gridApi = null;
    $scope.cobraYearMappings = [];
    $scope.gridOptions = {};
    $rootScope.errors = [];

    initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };

    $scope.datepickerOptions = {
        minDate: new Date(2000, 1, 1),
        maxDate: new Date(2150, 1, 1)
    };

    /* Loads the data. */
    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = true;

            $http({
                method: 'POST',
                url: createPostURL(CobraYearConfigurationModel.controller, CobraYearConfigurationModel.getAction)
            }).then(function successCallback(response) {
                $scope.cobraYearMappings = response.data.cobraYearMappings;

                $scope.setGridOptions();

                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $(document).trigger("HIDE_LOADING_BOX");
            });
        }
    };

    /* Saves the Cobra Year Configurations. */
    $scope.saveCobraYearConfigurations = function () {
        $(document).trigger("SHOW_LOADING_BOX");

        // Clear error messages.
        $rootScope.errors = [];

        var saveUrl = createPostURL(CobraYearConfigurationModel.controller, CobraYearConfigurationModel.saveAction);

        return $http({
            method: 'POST',
            url: saveUrl,
            data: JSON.stringify($scope.cobraYearMappings)
        }).then(function successCallback(response) {
            $scope.cobraYearForm.$setPristine();
            $scope.loadData();
            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $rootScope.errors = response.data.MessageList;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    /* Sets the options for the grid. */
    $scope.setGridOptions = function () {
        $scope.gridOptions = {
            enableRowHeaderSelection: false,
            enableCellEditOnFocus: true,
            enableSorting: false,
            enableFiltering: false,
            enableGridMenu: false,
            minRowsToShow: 50,
            rowHeight: 100,
            columnDefs: [
                { field: 'Year', width: 135, enableSorting: false, enableCellEdit: false, enableColumnMenu: false },
                {
                    field: 'CobraDateString', width: 135, enableSorting: false, cellEditableCondition: $scope.canEdit, enableColumnMenu: false, name: 'Cobra Date', cellFilter: 'textDate:"M/d/yyyy"',
                    editableCellTemplate: '<div><form name="inputForm"><div ui-grid-edit-datepicker datepicker-options="datepickerOptions" ng-class="\'colt\' + col.uid"></div></form></div>'
                }
            ],
            data: $scope.cobraYearMappings
        };
    };

    /* Get a reference to the gridApi so that we can auto-scroll later. */
    $scope.gridOptions.onRegisterApi = function(gridApi){
        $scope.gridApi = gridApi;
    };

    $scope.gridChanged = function (event) {
        $scope.cobraYearForm.$setDirty();
    };

    /* Adds a new Cobra Year to the grid (automatically calculating the next year) with a blank Cobra Date. */
    $scope.addCobraYearConfiguration = function () {
        // Get the max Cobra Year.
        var maxYear = $scope.cobraYearMappings.reduce(function (max, mapping) {
            return mapping.Year > max ? mapping.Year : max;
        }, 0);

        // Add the new Cobra Year with a blank Cobra Date.
        $scope.cobraYearMappings.push({ Year: maxYear + 1, CobraDateString: '' });

        // Auto-scroll to the new item and set focus to the Cobra Date cell.  The timeout delay is required to allow for the push to complete.
        $timeout(function(){
            $scope.gridApi.cellNav.scrollToFocus($scope.gridOptions.data[$scope.gridOptions.data.length - 1], $scope.gridOptions.columnDefs[1]);
        }, 100);
        $scope.cobraYearForm.$setDirty();
    };

    // Show delete Cobra Year confirmation dialog.
    $scope.showDeleteYearModal = function (ev) {
        var lastCobraYear = $scope.cobraYearMappings[$scope.cobraYearMappings.length - 1].Year;
        ConfirmDialog("Delete Cobra Year " + lastCobraYear, "Are you sure you want to delete Cobra Year " + lastCobraYear + "?", function () {
            $scope.cobraYearMappings.splice(-1, 1); // delete the last year in the list
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.EDIT);
            $scope.cobraYearForm.$setDirty();
        });
    };

    $scope.confirmCancel = function () {
        if ($scope.cobraYearForm.$dirty) {
            ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                // Yes
                $scope.cobraYearForm.$setPristine();
                initialize();
            },
            function () {
                // No
                return false;
            });
        }
    };

    window.onbeforeunload = function () {
        // Confirm leaving/refreshing page if the page is dirty
        if ($scope.cobraYearForm.$dirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    initialize();
}]);