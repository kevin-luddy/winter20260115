// Controller for editing the Manage Offline Applications page
angular.module('portal').controller('editOfflineApplicationsController', ['$scope', '$http', '$window', 'OfflineApplicationsView', '$rootScope', function ($scope, $http, $window, OfflineApplicationsView, $rootScope) {
    $scope.IsInitializing = true;
    $scope.isDataLoading = false;
    $scope.isSaving = false;
    $scope.isPageDirty = false;
    $rootScope.errors = [];
    $scope.applications = [];

    $scope.initialize = function () {
        $scope.IsInitializing = false;
        $scope.loadData();
    };

    $scope.loadData = function () {
        if (!$scope.IsInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");
            
            $rootScope.errors = [];
            $scope.isDataLoading = true;
            
            angular.forEach(OfflineApplicationsView.data, function (value, key) {
                $scope.applications.push({ ApplicationName: value.ApplicationName, IsOffline: value.IsOffline, UpdateDate: value.UpdateDate });
            });

            $(document).trigger("HIDE_LOADING_BOX");
            $scope.isDataLoading = false;
        }
    };

    $scope.markPageDirty = function () {
        $scope.isPageDirty = true;
    }

    $scope.save = function () {
        if ($scope.isPageDirty) {
            $scope.isSaving = true;
            $rootScope.errors = [];

            var url = createPostURL(OfflineApplicationsView.controller, OfflineApplicationsView.saveAction);

            $http({
                method: "POST",
                url: url,
                data: $scope.applications
            }).then(function successCallback(response) {
                $(document).trigger("DISPLAY_NOTIFICATION", 'Offline Application changes saved');
                $scope.isPageDirty = false;
                $window.location.reload();
            }, function errorCallback(response) {
                $scope.isSaving = false;
            });
        }
    };

    $scope.cancel = function () {
        // Return to main Admin page
        $window.location.href = createPostURL('admin');
    };

    window.onbeforeunload = function () {
        // Confirm leaving/refreshing page if the page is dirty
        if ($scope.isPageDirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    $scope.initialize();
}]);