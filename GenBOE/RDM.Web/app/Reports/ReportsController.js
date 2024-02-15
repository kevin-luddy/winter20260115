angular.module('RDM').controller('ReportsController', ['$scope', '$http', '$rootScope', 'utilityService', 'uiGridExporterConstants', 'VersionIndexModel', function ($scope, $http, $rootScope, utilityService, uiGridExporterConstants, VersionIndexModel) {
    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.isDataError = false;
    $scope.data = [];
    $scope.selectedVersion = null;
    $rootScope.errors = [];
    $scope.isExportingProPricer = false;
    $scope.isExportingRevisionAsJson = false;
    $scope.isExportingCobra = false;
    $scope.isExportingRateDoc = false;
    $scope.isExportingExpandedRateDoc = false;
    $scope.isExportingPPRD = false;
    $scope.isExportingPortionMarkedPPRD = false;
    $scope.timeoutTime = 3000;
    $scope.rateGridOptions = {};
    $scope.rateData = [];

    initialize = function() {
        $scope.isInitializing = false;
        $scope.loadData();
        $scope.isDataLoading = false;
        $scope.selectedVersion = VersionIndexModel.data.AvailableVersions[0];
        if (VersionIndexModel.data.SelectedVersion) {
            for (var i = 0; i < VersionIndexModel.data.AvailableVersions.length; i++) {
                var version = VersionIndexModel.data.AvailableVersions[i];
                if (version.Id === VersionIndexModel.data.SelectedVersion) {
                    $scope.selectedVersion = version;
                    break;
                }
            }
        }

        utilityService.setRateGridOptions($scope, function () {
            // this event handler is called by the rateGridApi.core.on.rowsRendered event.
            if ($scope.rateGridApi.grid.rows.length > 0 && ($scope.isExportingRateDoc === true || $scope.isExportingExpandedRateDoc === true)) {
                // export the CSV data
                $scope.rateGridApi.exporter.csvExport(uiGridExporterConstants.VISIBLE, uiGridExporterConstants.ALL);
                $scope.isExportingRateDoc = false;
                $scope.isExportingExpandedRateDoc = false;
            }
        });
    };

    $scope.loadData = function() {
        if (!$scope.isInitializing) {
            $scope.isDataError = false;
            $scope.data = VersionIndexModel.data; 
        }
    };

    // Exports Cobra Data.
    $scope.exportCobraClick = function () {
        $scope.isExportingCobra = true;
        setTimeout(function () { $scope.timeoutFuncCobra(); }, $scope.timeoutTime);
        DownloadFile('ExportCobra', VersionIndexModel.wcController, VersionIndexModel.wcActionExportCobra, $scope.selectedVersion.Id);
    }

    // Exports the ProPricer Data.
    $scope.exportProPricerClick = function () {
        $scope.isExportingProPricer = true;
        setTimeout(function () { $scope.timeoutFuncProPricer(); }, $scope.timeoutTime);
        DownloadFile('ExportProPricer', VersionIndexModel.wcController, VersionIndexModel.wcActionExportProPricer, $scope.selectedVersion.Id);
    }

    // Generates Full PPR&D.  Call controller to redirect to export page w/version selector
    $scope.generatePPRDClick = function (portionMarkingRequired) {
        if (portionMarkingRequired) {
            $scope.isExportingPortionMarkedPPRD = true;
        } else {
            $scope.isExportingPPRD = true;
        }
        setTimeout(function () { $scope.timeoutFuncPPRD(); }, $scope.timeoutTime);
        DownloadFile('GeneratePPRD', VersionIndexModel.wcController, VersionIndexModel.wcActionGenerateFullPPRD, $scope.selectedVersion.Id, portionMarkingRequired);
    }

    // Exports Revision data as zipped XML files.
    $scope.exportRevisionAsJsonClick = function () {
        $scope.isExportingRevisionAsJson = true;
        setTimeout(function () { $scope.timeoutFuncRevisionAsJson(); }, $scope.timeoutTime);
        DownloadFile('ExportRevisionAsJson', VersionIndexModel.wcController, VersionIndexModel.wcActionExportRevisionAsJson, $scope.selectedVersion.Id);
    }

    // Generates Rates file.
    $scope.generateRateDocClick = function (expandRates) {
        if (expandRates) {
            $scope.isExportingExpandedRateDoc = true;
        } else {
            $scope.isExportingRateDoc = true;
        }

        var getRatesurl = createPostURL(VersionIndexModel.wcRateController, VersionIndexModel.wcGetRatesByVersionAction, $scope.selectedVersion.Id);
        $http({
            method: 'POST',
            url: getRatesurl,
            data: { }
        }).then(function successCallback(response) {

            if (expandRates) {
                response.data.Rates = utilityService.expandRateData(response.data.Rates);
            }

            utilityService.updateRateGridData($scope.rateGridOptions, response.data.Rates, $scope.selectedVersion.Revision);  // Years can change with each version, must be called for each export.
            $scope.rateGridOptions.data = response.data.Rates;    // triggers the rowsRendered event (handler to export CSV data defined above by setRateGridOptions method)
        }, function errorCallback(response) {
            $scope.isExportingRateDoc = false;
        });
    }

    $scope.timeoutFuncProPricer = function () { $scope.$apply(function () { $scope.isExportingProPricer = false; }); }
    $scope.timeoutFuncRevisionAsJson = function () { $scope.$apply(function () { $scope.isExportingRevisionAsJson = false; }); }
    $scope.timeoutFuncCobra = function () { $scope.$apply(function () { $scope.isExportingCobra = false; }); }
    $scope.timeoutFuncRateDoc = function () { $scope.$apply(function () { $scope.isExportingRateDoc = false; $scope.isExportingExpandedRateDoc = false; }); }
    $scope.timeoutFuncPPRD = function () { $scope.$apply(function () { $scope.isExportingPPRD = false; $scope.isExportingPortionMarkedPPRD = false }); }

    initialize();
}]);