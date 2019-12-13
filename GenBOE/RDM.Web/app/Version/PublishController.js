// The controller for the Edit Content modal.
angular.module('RDM').controller('PublishController', ['$scope', '$uibModalInstance', '$rootScope', 'utilityService', 'model', function ($scope, $uibModalInstance, $rootScope, utilityService, model) {
    $scope.model = model;
    $rootScope.modalErrors = [];
    $scope.tinyMceOptionsHistory = utilityService.getTinyMceOptions();
    $scope.tinyMceOptionsHistory.height = "300";
    $scope.tinyMceOptionsReleaseNotes = utilityService.getTinyMceOptions();
    $scope.tinyMceOptionsReleaseNotes.height = "250";
    $scope.validateRates = function (){ utilityService.validateRates(model.validateRatesController, model.validateRatesAction, null, function () { $uibModalInstance.close($scope.model); }); }
}]);