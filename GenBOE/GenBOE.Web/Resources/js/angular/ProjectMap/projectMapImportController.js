// The controller for the Edit Content modal.
app.controller('projectMapImportController', ['$scope', '$uibModalInstance', '$rootScope', function ($scope, $uibModalInstance, $rootScope) {
    $rootScope.modalErrors = [];
    // Note: File uploader does not data-ng-bind, data-ng-model, data-ng-change correctly in angular
    $scope.importFile = "";
    $scope.showImportButton = false;
    $scope.importInProgress = false;
    $scope.formAction = "";
    
    initialize = function () {
        $('#ImportInstructions').removeClass('display-none');
        var workspaceName = window.location.protocol + '//' + window.location.host + '/' + window.location.pathname;
        $scope.formAction = workspaceName + "/Workspace/ImportProjectMapData";
    };

    $scope.fileImportChanged = function (event) {
        // Note:  event has the list of files picked as: event.target.files
        $scope.$apply(function () {
            $scope.importFile = "";
            if (event.target != undefined && event.target.files != undefined && event.target.files.length == 1) {
                $scope.importFile = event.target.files["0"].name;
            }

            $scope.validateFileInput();
        });
    }

    $scope.validateFileInput = function (event) {
        if ($scope.importFile.length == 0) {
            $scope.showImportFileValidation();
            $scope.showImportButton = false;
            return false;
        }
        else {
            $scope.hideImportFileValidation();
            $scope.showImportButton = true;
            return true;
        }
    };

    // exports the grid into an excel download
    $scope.exportGrid = function () {
        var workspaceName = window.location.protocol + '//' + window.location.host + '/' + window.location.pathname;
        GenWidget.prototype.performExport(workspaceName + '/Workspace/ExportProjectMapData?offload=false');
    };

    $scope.submit = function () {
        if ($scope.validateFileInput()) {
            $scope.hideImportDialogError();
            $scope.showImportButton = false;
            $scope.importInProgress = true;

            appendIFrameForUploadResponse();
            $("#ImportProjectMapDialog-Form").submit();
        }
    }

    appendIFrameForUploadResponse = function () {
        // Remove the old hidden iFrame, if it exists
        $('#ImportProjectMapDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportProjectMapDialog-Form').append('<iframe id="ImportProjectMapDialog-UploadTarget" name="ImportProjectMapDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportProjectMapDialog-UploadTarget').load(stopUpload);
    };

    stopUpload = function () { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportProjectMapDialog-UploadTarget").contents().find("body #UploadResponse");
        $scope.$apply(function () {
            $scope.importInProgress = false;
        });

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                $uibModalInstance.close(results);
            }
            else {
                if (results != undefined) {
                    $("#ImportProjectMapDialog-ErrorText").html(results.Message.replace(/\n/gi, "<br />"));
                    if (results.WarningMessage != undefined && results.WarningMessage != '') {
                        $("#ImportProjectMapDialog-WarningText").html(results.WarningMessage.replace(/\n/gi, "<br />"));
                    }
                }
                $scope.importFile = "";
                // Set the file uploader's value to nothing
                $("#ImportProjectMapDialog-File").val('');
                $scope.validateFileInput();
                $scope.showImportDialogError();
            }
        }
        else {
            // authorization exception or another unknown exception, show the title
            var pageTitle = $("#ImportProjectMapDialog-UploadTarget").contents()["0"].title;
            $("#ImportProjectMapDialog-ErrorText").html(pageTitle);
            $scope.importFile = "";
            // Set the file uploader's value to nothing
            $("#ImportProjectMapDialog-File").val('');
            $scope.validateFileInput();
            $scope.showImportDialogError();
        }
    }

    $scope.showImportDialogError = function () {
        $("#ImportProjectMapDialog-Error").slideDown("slow");
    };

    $scope.hideImportDialogError = function () {
        $("#ImportProjectMapDialog-Error").slideUp("slow");
    };

    $scope.showImportFileValidation = function () {
        $("#ImportProjectMapDialog-FileValidation").slideDown("slow");
    };

    $scope.hideImportFileValidation = function () {
        $("#ImportProjectMapDialog-FileValidation").slideUp("slow");
    };

    initialize();

}]);