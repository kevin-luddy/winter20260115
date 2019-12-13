// The controller for the Editing of a Picklist
angular.module('portal').controller('editPickListController', ['$scope', '$http', '$window', 'PickListModelView', '$rootScope', function ($scope, $http, $window, PickListModelView, $rootScope) {
    $scope.pickLists = PickListModelView.data;
    $scope.selectedPickList = {};
    $scope.isDataLoading = true;
    $rootScope.errors = [];
    $scope.validationErrors = [];
    $scope.isSaving = false;
    $scope.showAllData = true;
    $scope.disableDeleteButton = true;
    $scope.deleteAll = false;
    $scope.showReadOnly = PickListModelView.showReadOnly;
    $scope.showParent = PickListModelView.showParent;
    $scope.allowMultipleParents = PickListModelView.allowMultipleParents;
    $scope.parents = PickListModelView.parents;
    $scope.predicate = $scope.showParent && !$scope.allowMultipleParents ? 'ParentIds[0]' : 'Text';

    $scope.updatePredicate = function (header) {
        if ($scope.predicate === header) {
            $scope.predicate = '-' + header;
        } else {
            $scope.predicate = header;
        }
    }

    $scope.updateDeleteStatus = function () {
        var disableDelete = true;
        $scope.pickLists.forEach(function (item) {
            if (item.delete) {
                disableDelete = false;
            }
        });

        $scope.disableDeleteButton = disableDelete;
    };

    $scope.getParentNames = function (parentIds) {
        var names = [];
        if (parentIds !== undefined && $scope.parents !== undefined && $scope.parents !== null) {
            $scope.parents.forEach(function (item) {
                parentIds.forEach(function (id) {
                    if (item.Value == id) {
                        names.push(item.Text);
                    }
                });
            });
        }

        return names.join(', ');
    };

    $scope.updateDeleteAll = function () {
        $scope.pickLists.forEach(function (item) {
            if (!item.InUse && !item.IsReadOnly) {
                item.delete = $scope.deleteAll;
            }
        });
        $scope.updateDeleteStatus();
    };

    $scope.delete = function () {
        ConfirmDialog("Delete PickList Item(s)", "Are you sure you want to delete this Pick List item(s)?  This can not be undone.", function () {
            $rootScope.errors = [];
            $(document).trigger("SHOW_LOADING_BOX");

            var deleteUrl = createPostURL(PickListModelView.controller, PickListModelView.saveAction);
            var pickListToDelete = [];
            $scope.pickLists.forEach(function (item) {
                if (item.delete) {
                    item.updateable = PickListModelView.deleteValue;
                    pickListToDelete.push(item);
                }
            });

            return $http({
                method: 'POST',
                url: deleteUrl,
                data: { pickListType: PickListModelView.pickListId, dataToSave: pickListToDelete }
            }).then(function successCallback(response) {
                $scope.pickListForm.$setPristine();
                $window.location.reload(true);
            }, function errorCallback(response) {
                $(document).trigger("HIDE_LOADING_BOX");
            });
        });
    };

    $scope.edit = function (id) {
        $scope.pickLists.forEach(function (item) {
            if (item.Id == id) {
                $scope.selectedPickList = angular.copy(item);
                if ($scope.showParent && !$scope.allowMultipleParents && $scope.selectedPickList.ParentIds !== undefined && $scope.selectedPickList.ParentIds.length === 1) {
                    $scope.selectedPickList.ParentId = $scope.selectedPickList.ParentIds[0];
                }
            }
        });

        if (!$scope.selectedPickList.IsReadOnly) {
            $rootScope.errors = [];
            $scope.showAllData = false;
        }
    };

    $scope.add = function () {
        $scope.selectedPickList = {
            Id: -1,
            BoeId: -1,
            PtmId: -1,
            InUse: false,
            IsActive: false,
            IsReadOnly: false
        };

        $rootScope.errors = [];
        $scope.showAllData = false;
    };

    $scope.savePickList = function () {
        if ($scope.pickListForm.$valid) {

            $scope.isSaving = true;
            $rootScope.errors = [];
            $(document).trigger("SHOW_LOADING_BOX");

            var url = createPostURL(PickListModelView.controller, PickListModelView.saveAction);
            $scope.selectedPickList.updateable = PickListModelView.upsertValue;

            if ($scope.showParent && !$scope.allowMultipleParents) {
                $scope.selectedPickList.ParentIds = [];
                $scope.selectedPickList.ParentIds.push($scope.selectedPickList.ParentId);
            }

            var data = [];
            data.push($scope.selectedPickList);

            $http({
                method: "POST",
                url: url,
                data: { pickListType: PickListModelView.pickListId, dataToSave: data }
            }).then(function successCallback(response) {
                $(document).trigger("DISPLAY_NOTIFICATION", 'Pick List changes saved');
                $scope.pickListForm.$setPristine();
                $window.location.reload(true);
            }, function errorCallback(response) {
                $scope.isSaving = false;
                $(document).trigger("HIDE_LOADING_BOX");
            });
        }
    };

    $scope.fixErrors = function () {
        $scope.isSaving = true;
        $rootScope.errors = [];
        $(document).trigger("SHOW_LOADING_BOX");

        var url = createPostURL(PickListModelView.controller, PickListModelView.fixErrorsAction);
        
        $http({
            method: "POST",
            url: url,
            data: { pickListType: PickListModelView.pickListId }
        }).then(function successCallback(response) {
            $(document).trigger("DISPLAY_NOTIFICATION", 'Pick List changes saved');
            $scope.pickListForm.$setPristine();
            $window.location.reload(true);
        }, function errorCallback(response) {
            $scope.isSaving = false;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.cancel = function () {
        $scope.pickListForm.$setPristine();
        $rootScope.errors = [];
        $scope.showAllData = true;
    };

    $scope.initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };

    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.validationErrors = PickListModelView.validationMessages;

            $scope.isDataLoading = true;

            // TODO Any data cleanup needed

            $(document).trigger("HIDE_LOADING_BOX");
            $scope.isDataLoading = false;
        }
    };

    window.onbeforeunload = function () {
        // Confirm leaving/refreshing page if the page is dirty
        if ($scope.pickListForm.$dirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    // Initialization functions
    $scope.initialize();
}]);