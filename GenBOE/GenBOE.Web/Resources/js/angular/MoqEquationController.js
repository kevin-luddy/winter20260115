// The controller for the MOQ equation section.
moqEquationApp.controller('MoqEquationController', ['$scope', '$document', '$uibModal', '$window', 'ManageTaskModel', function ($scope, $document, $uibModal, $window, ManageTaskModel) {

    $scope.init = function () {
        $scope.model = $window.MOQEquationFieldModel;
        $scope.model.IsCostEquation = ($scope.model.MoqEquationType == 'Cost');
        $scope.model.insertWorkspaceModalOpen = false;
    }


    // Called when the Insert Workspace Variable dropdown item is clicked.
    $scope.InsertWorkspaceVariableClicked = function () {
        // show the modal
        var modalInstance = $uibModal.open({
            templateUrl: $scope.model.BaseUrl + 'Resources/InsertWorkspaceVariable.html',
            controller: 'InsertWorkspaceVariableController',
            windowTopClass: 'bootstrap insert-ws-var-modal',
            windowClass: 'bootstrap',
            backdropClass: 'bootstrap',
            resolve: { model: [function () { return $scope.model.WorkspaceVariables; }] }
        });

        $scope.model.insertWorkspaceModalOpen = true;

        // handle the result
        modalInstance.result.then(function (selectedVariable) {
            $scope.model.insertWorkspaceModalOpen = false;

            if (angular.isDefined(selectedVariable)) {
                // add the variable to the MOQ equation field
                var moqEquationField = $('#' + $scope.model.MoqEquationName + 'MOQEquationField #MOQEquation');
                var previousValue = moqEquationField.val();
                moqEquationField.val(previousValue + selectedVariable.WorkspaceVariableName);

                // re-validate the equation
                $(document).trigger('ValidateMOQEquation');
            }
        });
    };

    // Called when the Search Estimating Catalog dropdown item is clicked.
    $scope.SearchEstimatingCatalogClicked = function () {
        $(document).trigger('SEARCH_METRICS');
    }

    // Called when the Copy MOQ from BOE dropdown item is clicked.
    $scope.CopyMoqFromBoeClicked = function () {
        $('#CopyMoqFromBoeLink').data('moq-task-id', $scope.model.TaskElementId);
        $(document).trigger("COPY_MOQ_SELECT");
    }
}]);