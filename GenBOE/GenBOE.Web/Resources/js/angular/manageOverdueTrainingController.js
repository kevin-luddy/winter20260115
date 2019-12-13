(function () {
    'use strict';
    var app = angular.module('genboe');
    app.controller('manageOverdueTrainingController', ['$scope', 'ManageOverdueTrainingModelView', '$http', function ($scope, ManageOverdueTrainingModelView, $http) {
        $scope.errors = [];
        $scope.importWorking = false;
        $scope.data = [];
        $scope.disableImport = true;
        $scope.file = null;
        $scope.showImportResults = false;
        $scope.courseName = '';
        
        $scope.showImportError = function (message) {
            var error = {};
            error.Title = "Import Training Error";
            error.Message = message;
            error.Details = message;

            DisplayExceptionDialog(error);
        };

        $scope.import = function () {
            if (!$scope.disableImport) {
                // create form data
                var fd = new FormData();
                fd.append("file", $scope.file);
                fd.append("courseName", $scope.courseName);

                // get url from form
                var url = GenSession.CreateSystemAdminPostURL(ManageOverdueTrainingModelView.controller, ManageOverdueTrainingModelView.importOverdueTrainingAction);
                $scope.showImportResults = false;
                $scope.importWorking = true;
                $scope.errors = [];

                $http.post(url, fd, {
                    headers: {
                        'Content-Type': undefined
                    }
                }).then(function (response) {
                    if (response != undefined && response.data != undefined) {
                        var results = response.data;
                        if (results.Status) {
                            $scope.data = results.Data;

                            $scope.showImportResults = true;
                        } else {
                            $scope.showImportError(results.Message);
                        }
                    } else {
                        $scope.showImportError('A general import error occurred. Please check your import file for validity and try the import again.');
                    }

                    $scope.importWorking = false;
                }).catch(function () {
                    $scope.showImportError('A general import error occurred. Please check your import file for validity and try the import again.');
                    $scope.importWorking = false;
                });
            }
        };

        $scope.fileUploadChange = function (element) {
            $scope.$apply(function ($scope) {
                $scope.disableImport = element.value.endsWith('.xlsx') || element.value.endsWith('.xlsm') ? false : true;
                if ($scope.disableImport) {
                    $scope.showImportError("Please choose an Excel file ending in .xlsx or .xlsm");
                }

                $scope.file = $scope.disableImport ? null : element.files[0];
            });
        };
    }]);
})();