// The controller for the Edit Section modal.
angular.module('RDM').controller('EditSectionController', ['$scope', '$uibModalInstance', '$rootScope', 'model', function ($scope, $uibModalInstance, $rootScope, model) {
    $scope.model = model;
    $rootScope.modalErrors = [];

    $scope.uncheckRequiredIfInternal = function () {
        if (model.IsInternalSection) {
            model.IsRdsbRequired = false;
        }
    };
}]);