// The controller for the Add Document modal.
angular.module('rdsb').controller('AddDocumentController', ['$scope', '$http', '$window', '$uibModalInstance', '$sce', '$rootScope', 'model', 'documentGridModel', function ($scope, $http, $window, $uibModalInstance, $sce, $rootScope, model, documentGridModel) {
    $scope.model = model;
    $scope.documentGridModel = documentGridModel;
    $rootScope.modalErrors = [];
    $scope.helpText = $sce.trustAsHtml('Available proposals meet the following requirements:' +
        '<ul><li>You have a role of Lead Esitmator, Backup Lead Estimator, Cost Volume Lead, or System Admin with the Proposal</li>' +
        '<li>The Proposal is \"In Progress\"</li>' +
        '<li>The Proposal does not already have an RDD linked to it</li>' +
        '<li>The Proposal does not have Commercial or International Commercial End Customer Types</li></ul>');
    $scope.selectedProposalId = null;
    $scope.error = null;

    $scope.setProposalId = function () {
        var options = $('#' + $('#proposalSelect').attr('list') + ' option');

        $scope.selectedProposalId = null; //clear previous value

        for (var i = 0; i < options.length; i++) {
            var option = options.eq(i);
            if (option[0].value === $scope.selectedValue || option.attr('propId') === $scope.selectedValue) {
                $scope.selectedProposalId = option.attr('propId');
                break;
            }
        }

        if ($scope.selectedProposalId === null) {
            $('#saveButton-addDocument').attr('disabled', 'disabled');
        } else {
            $('#saveButton-addDocument').removeAttr('disabled');
        }
    };

    $scope.saveNewDocument = function () {
        $scope.error = null; // clear error message

        if ($scope.selectedProposalId != null) {
            var url = createPostURL($scope.documentGridModel.controller, $scope.documentGridModel.saveNewDocumentAction, $scope.selectedProposalId);

            $http({
                method: "POST",
                url: url
            }).then(function successCallback(response) {
                $scope.editDocument($scope.selectedProposalId);
            });
        } else {
            $scope.error = "Please select a valid proposal.";
            $('#saveButton-addDocument').attr('disabled', 'disabled');
        }
    };

    $scope.editDocument = function (proposalId) {
        var editUrl = createPostURL($scope.documentGridModel.controller, $scope.documentGridModel.editAction, proposalId);

        $window.location.href = editUrl;
    }
}]);