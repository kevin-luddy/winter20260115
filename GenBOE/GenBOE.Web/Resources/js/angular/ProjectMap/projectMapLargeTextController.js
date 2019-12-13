// The controller for the Large Text modal.
app.controller('projectMapLargeTextController', ['$scope', '$uibModalInstance', 'model', function ($scope, $uibModalInstance, $model) {
    $scope.model = $model;

    $scope.closeModal = function () {
        $uibModalInstance.close($scope.model.text);
    }
}]);