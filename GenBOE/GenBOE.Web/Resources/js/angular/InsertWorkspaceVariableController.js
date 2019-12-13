// The controller for the Insert Workspace Variable modal.
moqEquationApp.controller('InsertWorkspaceVariableController', ['$scope', '$uibModalInstance', 'model', function ($scope, $uibModalInstance, model) {
    $scope.model = model;

    // determine if any of the variables are disabled
    $scope.hasDisabledVariable = false;
    for (var i = 0; i < $scope.model.length; i++) {
        if ($scope.model[i].Disabled) {
            $scope.hasDisabledVariable = true;
        }
    }

    // Called when a variable is clicked.
    $scope.VariableClicked = function (variable) {
        $uibModalInstance.close(variable);
    }
}]);