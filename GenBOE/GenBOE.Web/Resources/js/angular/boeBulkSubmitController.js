angular.module('genboe').controller('BoeBulkSubmitController', ['$scope', '$http', '$timeout', '$filter', 'BulkSubmitModel', function ($scope, $http, $timeout, $filter, BulkSubmitModel) {

    $scope.data = [];
    $scope.colSpan = 4;
    $scope.noDirty = true;
    $scope.isLoading = false;
    $scope.errors = [];

    $scope.updateDirty = function () {
        var boeStateEdits = [];
        $scope.data.forEach(function (item) {
            var isOriginallyAwaitingApproval = item.Status != 'Draft';
            if (item.isAwaitingApproval != isOriginallyAwaitingApproval) {
                // there was a change, add it to the list
                var edit = { key: item.Id, value: BulkSubmitModel.awaitingState };

                if (isOriginallyAwaitingApproval) {
                    edit.value = BulkSubmitModel.draftState;
                }
                boeStateEdits.push(edit);
            }
        });


        $scope.noDirty = (boeStateEdits.length == 0);
    };

    $scope.toggleAllDraft = function () {
        $scope.data.forEach(function (item) {
            if (!item.Invalid) {
                item.isAwaitingApproval = false;
            }
        });

        $scope.updateDirty();
    };

    $scope.toggleAllAwaitingApproval = function () {
        $scope.data.forEach(function (item) {
            if (!item.Invalid) {
                item.isAwaitingApproval = true;
            }
        });

        $scope.updateDirty();
    };

    $scope.saveChanges = function () {
        $scope.errors = [];
        var boeStateEdits = [];

        $scope.data.forEach(function (item) {
            var isOriginallyAwaitingApproval = item.Status != 'Draft';
            if (item.isAwaitingApproval != isOriginallyAwaitingApproval)
            {
                // there was a change, add it to the list
                var edit = { key: item.Id, value: BulkSubmitModel.awaitingState };

                if (isOriginallyAwaitingApproval) {
                    edit.value = BulkSubmitModel.draftState;
                }
                boeStateEdits.push(edit);
            }
        });

        if (boeStateEdits.length == 0) {
            // no changes
            $scope.noDirty = true;

            GenSession.alertDialog('Save Changes', 'There are no changes to save.');
        } else {

            // method re-used from Manage BOEs
            // Save the state changes
            $('#PageLoading').removeClass('display-none');
            $http({
                method: 'POST',
                url: CreatePostURL(BulkSubmitModel.workspace, BulkSubmitModel.controller, BulkSubmitModel.editAction, ''),
                data: { boeStates: boeStateEdits }
            }).then(function (response) {
                $('#PageLoading').addClass('display-none');

                if (response.data.ErrorMessages.length > 0) {
                    response.data.ErrorMessages.forEach(function (item) {
                        var newError = { ValidationIssue: item };
                        $scope.errors.push(newError);
                    });
                } else {
                    loadBOEs();
                }
            }).catch(function () {
                $('#PageLoading').addClass('display-none');

                $scope.errors.push({ ValidationIssue: 'Error(s) encountered during save.' });
            });
        }
    };
    
    var loadBOEs = function () {
        $scope.isLoading = true;
        $scope.data = [];

        return $http({
            method: 'POST',
            url: CreatePostURL(BulkSubmitModel.workspace, BulkSubmitModel.controller, BulkSubmitModel.action, '')
        }).then(function (response) {
            response.data.forEach(function (item) {
                item.url = CreatePostURL(BulkSubmitModel.workspace, BulkSubmitModel.boeController, BulkSubmitModel.editBOEAction, 'boe/') + item.Id;
            });
            $scope.noDirty = true;
            $scope.data = response.data
            $scope.isLoading = false;
        });
    }

    loadBOEs();
}]);