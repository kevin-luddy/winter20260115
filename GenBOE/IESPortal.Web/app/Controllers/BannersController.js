// The controller for the showing of the Banners
angular.module('portal').controller('bannersController', ['$scope', '$http', '$window', 'BannerGridView', '$rootScope', function ($scope, $http, $window, BannerGridView, $rootScope) {
    $scope.model = BannerGridView.data;
    $scope.isDataLoading = true;
    $rootScope.errors = [];
    
    $scope.deleteBanner = function (id) {
        $rootScope.errors = [];
        
        ConfirmDialog("Delete Banner", "Are you sure you want to delete this Banner?  This can not be undone.", function () {
            $(document).trigger("SHOW_LOADING_BOX");

            var deleteUrl = createPostURL(BannerGridView.controller, BannerGridView.deleteAction, id);
            var BannerToDelete = {};
            
            return $http({
                method: 'POST',
                url: deleteUrl,
                data: JSON.stringify(BannerToDelete)
            }).then(function successCallback(response) {
                $window.location.reload();
            }, function errorCallback(response) {
                $(document).trigger("HIDE_LOADING_BOX");
            });
        });
    };

    $scope.addBanner = function () {
        $window.location.href = '/Admin/EditBanner/-1';
    }

    $scope.initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };

    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            
            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = false;
        }
    };

    // Initialization functions
    $scope.initialize();
}]);