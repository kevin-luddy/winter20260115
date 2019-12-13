angular.module('RDM').controller('MainMenuController', ['$scope', '$http', '$uibModal', 'utilityService', 'MainMenuModel', function ($scope, $http, $uibModal, utilityService, MainMenuModel) {

    $scope.isInitializing = true;
    $scope.menu = [];

    initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };

    /* Loads the Data */
    $scope.loadData = function () {
        if (!$scope.isInitializing) {

            $http({
                method: 'POST',
                url: createPostURL(MainMenuModel.controller, 'GetMenuOptions'),
                data: JSON.stringify({ menuOptionControllerName: MainMenuModel.menuOptionControllerName })
            }).then(function successCallback(response) {
                $scope.menu = response.data.menu;
            });
        }
    };

    /*
    Determines whether the user can access top-level menu options.  If sub menu options
    are present, access is automatically determined for the top-level menu option based upon
    the ability to access the sub options.
    */
    $scope.userCanAccessMenuOption = function (menuOption) {
        // If there are SubMenuOptions, the user must have access to at least one
        // to access the top-level menu option.
        if (menuOption.SubMenuOptions && menuOption.SubMenuOptions.length > 0)
        {
            for (var i = 0; i < menuOption.SubMenuOptions.length; i++)
            {
                if (menuOption.SubMenuOptions[i].UserCanAccess)
                {
                    return true;
                }
            }

            // There were SubMenuOptions, but the user did not have access to any.
            return false;
        }

        // If there were no SubMenuOptions, access is determined by the menu option's own setting.
        return menuOption.UserCanAccess;
    };

    $scope.openWhosOnlineModal = function () {
        // open the Who's online Modal
        var userData = {};
        $scope.loadWhosOnline().then(function successCallback(response) {
            userData = response.data;

            var modalInstance = $uibModal.open({
                templateUrl: MainMenuModel.baseUrl + 'app/Menus/WhosOnline.html',
                controller: 'WhosOnlineController',
                windowTopClass: 'bootstrap help-modal',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap',
                resolve: { model: [function () { return userData; }] }
            })

            modalInstance.rendered.then(function () {
                utilityService.makeModalDraggableAndResizable('#whosOnlineModal', 142, 640);
            });
        });
    }

    $scope.loadWhosOnline = function () {
        return $http({
            method: 'POST',
            url: createPostURL(MainMenuModel.controller, MainMenuModel.getWhosOnlineAction)
        });
    }
    
    initialize();
}]);