// The controller for the Who's Online modal.
angular.module('rdsb').controller('WhosOnlineController', ['$scope', '$uibModalInstance', '$rootScope', 'model', function ($scope, $uibModalInstance, $rootScope, model) {
    $scope.model = model;
    $rootScope.modalErrors = [];
}]);