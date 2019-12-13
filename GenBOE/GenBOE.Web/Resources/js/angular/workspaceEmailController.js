(function () {
    'use strict';
    var app = angular.module('genboe');
    app.controller('workspaceEmailController', ['$scope', 'WorkspaceEmailModelView', '$http', '$timeout', function ($scope, WorkspaceEmailModelView, $http, $timeout) {
        $scope.errors = [];
        $scope.isInitializing = false;
        $scope.isDataLoading = true;
        $scope.isReadOnly = true;
        $scope.emails = [];
        $scope.emailGroups = [];
        $scope.allOn = false;
        $scope.toggleAllText = 'Turn All Emails On';

        $scope.initialize = function () {
            $scope.isInitializing = false;
            $scope.loadData();
        };

        $scope.loadData = function () {
            if (!$scope.isInitializing) {
                $(document).trigger("SHOW_LOADING_BOX");

                // Clear error messages.
                $scope.errors = [];
                $scope.isDataLoading = true;
                if ($scope.workspaceEmailForm) {
                    $scope.workspaceEmailForm.$setPristine();
                }

                var getEmailsUrl = GenSession.CreatePostURL(WorkspaceEmailModelView.workspace, WorkspaceEmailModelView.controller, WorkspaceEmailModelView.getWorkspaceEmailsAction);

                // Load data.  
                $http({
                    method: 'POST',
                    url: getEmailsUrl,
                    data: { }
                }).then(function successCallback(response) {
                    $scope.emails = response.data.emails;
                    $scope.isReadOnly = response.data.isReadOnly;
                    $scope.emailGroups = $scope.emails.reduce(function (r, a) {
                        r[a.Category] = r[a.Category] || [];
                        r[a.Category].push(a);
                        return r;
                    }, Object.create(null));

                    $timeout(WorkspaceSettings.refreshModule);
                    $scope.isDataLoading = false;
                    $(document).trigger("HIDE_LOADING_BOX");
                },
                function errorCallback(error) {
                    $scope.isDataLoading = false;
                    $(document).trigger("HIDE_LOADING_BOX");
                });
            }
        };    

        $scope.toggleSelectAll = function () {
            $scope.workspaceEmailForm.$setDirty();
            $scope.allOn = !$scope.allOn;

            var emails = $scope.emails;
            for (var i = 0; i < emails.length; i++) {
                emails[i].WorkspaceOverrideOn = $scope.allOn;
            }

            if ($scope.allOn === true) {
                $scope.toggleAllText = 'Turn All Emails Off';
            }
            else {
                $scope.toggleAllText = 'Turn All Emails On';
            }
        };

        $scope.selectCategoryAll = function (category, checked) {
            $scope.workspaceEmailForm.$setDirty();
            var emails = $scope.emailGroups[category];
            for (var i = 0; i < emails.length; i++) {
                emails[i].WorkspaceOverrideOn = checked;
            }
        };

        $scope.save = function () {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $scope.errors = [];

            var saveUrl = GenSession.CreatePostURL(WorkspaceEmailModelView.workspace, WorkspaceEmailModelView.controller, WorkspaceEmailModelView.saveAction);

            return $http({
                method: 'POST',
                url: saveUrl,
                data: { emails: $scope.emails }
            }).then(function successCallback(response) {
                $scope.workspaceEmailForm.$setPristine();
                $(document).trigger('DATA_CLEANED', 'workspaceEmailForm');
                $scope.loadData();
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $scope.errors = response.data.MessageList;
                $(document).trigger("HIDE_LOADING_BOX");
            });
        };

        $scope.confirmCancel = function () {
            if ($scope.workspaceEmailForm.$dirty) {
                Session.confirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                    // go to admin section
                    $scope.workspaceEmailForm.$setPristine();
                    window.location.hash = '#';
                });
            } else {
                // If no changes, go to admin section
                window.location.hash = '#';
            }
        };

        window.onbeforeunload = function () {
            // Confirm leaving/refreshing page if the page is dirty
            if ($scope.workspaceEmailForm.$dirty) {
                return "Changes have not been saved. Are you sure you want to navigate away?";
            }
        };

        $scope.initialize();
    }]);
})();