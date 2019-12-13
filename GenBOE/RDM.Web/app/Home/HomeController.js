angular.module('RDM').controller('HomeController', ['$scope', '$http', '$timeout', '$rootScope', 'utilityService', 'uiGridExporterConstants', 'RevisionModel', function ($scope, $http, $timeout, $rootScope, utilityService, uiGridExporterConstants, RevisionModel) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.gridApi = null;
    $scope.revisions = [];
    $scope.isRdmAdminUser = false;
    $scope.isRdmCobraAdminUser = false;
    $scope.managePprdUrl;
    $scope.manageRatesUrl;
    $scope.manageBurdenPoolsUrl;
    $scope.manageCobraMappingsUrl;
    $rootScope.errors = [];
    $scope.isExportingRateDoc = false;
    $scope.isExportingPPRD = false;
    $scope.timeoutTime = 3000;

    initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
        utilityService.setRateGridOptions($scope, function () {
            // this event handler is called by the rateGridApi.core.on.rowsRendered event.
            if ($scope.rateGridApi.grid.rows.length > 0 && $scope.isExportingRateDoc === true) {
                // export the CSV data
                $scope.rateGridApi.exporter.csvExport(uiGridExporterConstants.VISIBLE, uiGridExporterConstants.ALL);
                $scope.isExportingRateDoc = false;
            }
        });
    };

    /* Loads the data. */
    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = true;

            $http({
                method: 'POST',
                url: createPostURL(RevisionModel.controller, RevisionModel.getAction)
            }).then(function successCallback(response) {
                $scope.revisions = response.data.Revisions;
                $scope.managePprdUrl = response.data.ManagePprdMenu.UserCanAccess ? response.data.ManagePprdMenu.Link : null;
                $scope.manageRatesUrl = response.data.ManageRatesMenu.UserCanAccess ? response.data.ManageRatesMenu.Link : null;
                $scope.manageBurdenPoolsUrl = response.data.ManageBurdenPoolsMenu.UserCanAccess ? response.data.ManageBurdenPoolsMenu.Link : null;
                $scope.manageCobraMappingsUrl = response.data.ManageCobraMappingsMenu.UserCanAccess ? response.data.ManageCobraMappingsMenu.Link : null;
                $scope.isRdmAdminUser = response.data.IsRdmAdminUser;
                $scope.isRdmCobraAdminUser = response.data.IsRdmCobraAdminUser;

                // Convert the Dates from the JSON format to the proper string format.
                for (var i = 0; i < $scope.revisions.length; i++) {
                    $scope.revisions[i].DatePublished = utilityService.getLocaleDateTimeStringFromJsonDate($scope.revisions[i].DatePublished);
                }

                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $(document).trigger("HIDE_LOADING_BOX");
            });
        }
    };

    // Generates Full PPR&D.  Call controller to redirect to export page w/version selector
    $scope.generatePPRDClick = function (id) {
        $scope.isExportingPPRD = true;
        setTimeout(function () { $scope.timeoutFuncPPRD(); }, $scope.timeoutTime);
        DownloadFile('GeneratePPRD', RevisionModel.wcReportsController, RevisionModel.wcActionGenerateFullPPRD, id);
    }

    // Generates Rates file.
    $scope.generateRateDocClick = function (id, versionNumber) {
        $scope.isExportingRateDoc = true;

        var getRatesurl = createPostURL(RevisionModel.wcRateController, RevisionModel.wcGetRatesByVersionAction, id);
        $http({
            method: 'POST',
            url: getRatesurl,
            data: {}
        }).then(function successCallback(response) {
            utilityService.updateRateGridData($scope.rateGridOptions, response.data.Rates, versionNumber);    // Years can change with each version, must be called for each export.
            $scope.rateGridOptions.data = response.data.Rates;       // triggers the rowsRendered event (handler to export CSV data defined above by setRateGridOptions method)
        }, function errorCallback(response) {
            $scope.isExportingRateDoc = false;
        });
    }

    $scope.timeoutFuncRateDoc = function () { $scope.$apply(function () { $scope.isExportingRateDoc = false; }); }
    $scope.timeoutFuncPPRD = function () { $scope.$apply(function () { $scope.isExportingPPRD = false; }); }

    initialize();
}]);