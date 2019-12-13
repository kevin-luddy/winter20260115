angular.module('RDM').controller('RevisionConfigurationCtrl', ['$scope', '$http', '$rootScope', 'RevisionConfigurationModel', 'utilityService', function ($scope, $http, $rootScope, RevisionConfigurationModel, utilityService) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.start = new Date();
    $scope.end = new Date();
    $rootScope.errors = [];
    $scope.tinyMceOptionsHistory = utilityService.getTinyMceOptions();
    $scope.tinyMceOptionsHistory.height = "400";
    $scope.tinyMceOptionsReleaseNotes = utilityService.getTinyMceOptions();

    $scope.popupDatePickerStart = {
        opened: false
    };

    $scope.popupDatePickerEnd = {
        opened: false
    };

    $scope.openDatePickerStart = function () {
        $scope.popupDatePickerStart.opened = true;
    };

    $scope.openDatePickerEnd = function () {
        $scope.popupDatePickerEnd.opened = true;
    };

    $scope.dateOptions = {
        formatYear: 'yyyy',
        startingDay: 1,
        minMode: 'year',
        minDate: new Date(2000, 1, 1),
        maxDate: new Date(2150, 1, 1)
    };

    var ignoreOnChangeDate = false;

    initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };
    /* Loads the data. */
    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = true;

            // Load data.  
            $http({
                method: 'POST',
                url: createPostURL(RevisionConfigurationModel.controller, RevisionConfigurationModel.getAction)
            }).then(function successCallback(response) {
                // Load data.  
                if (response.data.StartYear > 0) {
                    $scope.start = new Date(response.data.StartYear, 1, 1, 1, 1, 1, 1);
                }

                if (response.data.EndYear > 0) {
                    $scope.end = new Date(response.data.EndYear, 1, 1, 1, 1, 1, 1);
                }

                $scope.releaseNotes = response.data.ReleaseNotes;
                $scope.history = response.data.History;

                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $(document).trigger("HIDE_LOADING_BOX");
            });
        }
    };

    /* Saves the Revision Configurations. */
    $scope.saveRevisionConfigurations = function () {
        $(document).trigger("SHOW_LOADING_BOX");

        // Clear error messages.
        $rootScope.errors = [];
        var startYear = $scope.start.getFullYear();
        var endYear = $scope.end.getFullYear();
        return $http({
            method: 'POST',
            url: createPostURL(RevisionConfigurationModel.controller, RevisionConfigurationModel.saveAction),
            data: JSON.stringify({ startYear: startYear, endYear: endYear, releaseNotes: $scope.releaseNotes, history: $scope.history })
        }).then(function successCallback(response) {
            $scope.inputForm.$setPristine();
            $(document).trigger("HIDE_LOADING_BOX");
        },
            function errorCallback(response) {
                $rootScope.errors = response.data.MessageList;
                $(document).trigger("HIDE_LOADING_BOX");
            });
    };

    $scope.cancel = function () {
        if ($scope.inputForm.$dirty) {
            ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                // Yes
                $scope.inputForm.$setPristine();
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
        if ($scope.inputForm.$dirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    initialize();
}]);