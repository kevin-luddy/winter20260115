angular.module('rdsb').controller('documentController', ['$scope', '$http', '$filter', '$window', '$uibModal', 'uiGridConstants', 'utilityService', 'DocumentGridModel',
    function ($scope, $http, $filter, $window, $uibModal, uiGridConstants, utilityService, DocumentGridModel) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.isDataError = false;
    $scope.showWhosOnlineButton = false;
    $scope.timeoutTime = 3000;
    $scope.isAdding = false;

    initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    }

    $scope.editDocument = function (Document) {
        var editUrl = createPostURL(DocumentGridModel.controller, DocumentGridModel.editAction, Document.ProposalId);

        $window.location.href = editUrl;
    }

    $scope.deleteDocument = function (Document) {
        ConfirmDialog("Delete Document", "Are you sure you want to delete this Document?  This can not be undone.", function () {
            $scope.isDataValid = true;
            $(document).trigger("SHOW_LOADING_BOX");

            var deleteUrl = createPostURL(DocumentGridModel.controller, DocumentGridModel.deleteAction);
            var DocumentToDelete = {};
            DocumentToDelete.id = Document.ProposalId;

            return $http({
                method: 'POST',
                url: deleteUrl,
                data: JSON.stringify(DocumentToDelete)
            }).then(function successCallback(response) {
                $window.location.reload();
            }, function errorCallback(response) {
            });
        });
    }

    $scope.publishDocument = function (Document, portionMarkingRequired) {
        if (Document.IsReadOnly || Document.IsUsingLatest) {
            $scope.export(Document, portionMarkingRequired);
        } else {
            // Only show the confirm dialog if user has Edit and not using latest
            ConfirmDialog("Old PPR&D Revision", "This document is using an older PPR&D Revision. Are you sure you want to continue publishing?", function () {
                $scope.export(Document, portionMarkingRequired);
            });
        }
    };

    $scope.export = function (Document, portionMarkingRequired) {
        $(document).trigger("SHOW_LOADING_BOX");
        setTimeout(function () { $scope.timeoutFuncRDD(); }, $scope.timeoutTime);
        DownloadFile('GenerateRDD', DocumentGridModel.controller, DocumentGridModel.publishAction, Document.ProposalId, portionMarkingRequired);
    }

    $scope.timeoutFuncRDD = function () { $(document).trigger("HIDE_LOADING_BOX"); }

    $scope.openAddDocumentModal = function () {
        $scope.isAdding = true;
        // open the Add Document Modal
        var url = createPostURL(DocumentGridModel.controller, DocumentGridModel.getProposalsForNewDocAction);

        $http({
            method: "POST",
            url: url
        }).then(function successCallback(response) {
            var proposals = response.data;

            var modalInstance = $uibModal.open({
                templateUrl: DocumentGridModel.baseUrl + 'app/Document/AddDocument.html',
                controller: 'AddDocumentController',
                windowTopClass: 'bootstrap help-modal',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap',
                resolve: { model: [function () { return proposals }], documentGridModel: [function () { return DocumentGridModel }] }
            });

            $scope.isAdding = false;

            modalInstance.rendered.then(function () {
                utilityService.makeModalDraggableAndResizable('#addDocumentModal', 142, 640);
            });
        });
    };

    /* Loads the Data */
    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $scope.isDataLoading = true;
            $scope.isDataError = false;
            DocumentGridModel.data.forEach(function (Document) {
                Document.url = createPostURL(DocumentGridModel.controller, DocumentGridModel.editAction, Document.ProposalId);
            });

            $scope.gridOptions.data = DocumentGridModel.data;
            $scope.PTMUrl = DocumentGridModel.PTMUrl;

            $scope.setShowWhosOnlineButton();

            $scope.isDataLoading = false;
        }
        };

    $scope.gridOptions = {
        enableSorting: true,
        enableFiltering: true,
        enableColumnMenus: false,
        headerTemplate: DocumentGridModel.baseUrl + "app/Document/gridHeaderTemplate.html",
        groupColumnDefs: [{
            name: 'prop',
            displayName: 'Proposal'
        }, {
            name: 'trads',
            displayName: 'Tailored Rates & Disclosures Section'
        }],
        columnDefs: [
            { field: 'ProposalTrackingNumber', displayName: 'Tracking Number', width: '133', groupColumn: 'prop' },
            { field: 'ProposalTitle', displayName: 'Title', groupColumn: 'prop' },
            {
                field: 'ProposalStatus', displayName: 'Status', groupColumn: 'prop', filter: {
                    term: 'In Progress',
                    type: uiGridConstants.filter.SELECT,
                    selectOptions: [
                        { value: '', label: 'All' }, { value: 'Completed', label: 'Completed' },
                        { value: 'In Progress', label: 'In Progress' }, { value: 'Submitted', label: 'Submitted' }
                    ]
                },
                headerCellClass: 'right-divider',
                cellClass: 'right-divider'
            },
            { field: 'DocumentCreatedBy', displayName: 'Created By', groupColumn: 'trads' },
            { field: 'DocumentCreated', displayName: 'Created', groupColumn: 'trads', enableFiltering: false, cellFilter: 'date:\'short\'' },
            {
                field: 'UpdateDate', displayName: 'Last Modified', groupColumn: 'trads', enableFiltering: false, cellFilter: 'date:\'short\'', sort: {
                direction: uiGridConstants.DESC,
                priority: 1} 
            },
            { field: 'PPRDVersion', displayName: 'PPR&D Revision', groupColumn: 'trads' },
            { field: 'PPRDVersionDate', displayName: 'PPR&D Revision Date', groupColumn: 'trads', enableFiltering: false, cellFilter: 'date:\'short\'' },
            {
                field: 'Manage', groupColumn: 'trads', headerCellClass: 'text-center', enableFiltering: false, enableSorting: false, width: '143',
                cellTemplate: "<div class=\"ui-grid-cell-contents pad-left\">\
                        <a ng-if=\"!grid.appScope.HideNonPortionMarking\" title=\"Export PPR&D\" class=\"glyphicon glyphicon-save-file publish-button text-success\" data-ng-click=\"grid.appScope.publishDocument(row.entity, false)\"></a>\r\n \
                        <a ng-if=\"!grid.appScope.HidePortionMarking\" title=\"Export Portion Marked PPR&D\" class=\"glyphicon glyphicon-save-file publish-button text-success\" data-ng-click=\"grid.appScope.publishDocument(row.entity, true)\"></a>\r\n \
                        <a class=\"btn btn-primary btn-xs proposal-button\" href=\"{{grid.appScope.PTMUrl}}proposal/DisplayProposalDetails/id/{{row.entity.ProposalId}}/#Proposal\" target=\"_blank\" title=\"Open proposal in PTM\"><span class=\"glyphicon\">P</span></a>\r\n \
                        <a class=\"btn btn-primary btn-xs edit-button\" title=\"Edit Document\" data-nodrag data-ng-hide=\"row.entity.IsReadOnly\" data-ng-click=\"grid.appScope.editDocument(row.entity)\"><span class=\"glyphicon glyphicon-pencil\"></span></a>\r\n \
                        <a class=\"btn btn-danger btn-xs delete-button\" title=\"Delete Document\" data-nodrag data-ng-hide=\"row.entity.IsReadOnly\" data-ng-click=\"grid.appScope.deleteDocument(row.entity)\"><span class=\"glyphicon glyphicon-remove\"></span></a>\r\n \
                    </div>"
            }
        ]
    };

    $scope.setShowWhosOnlineButton = function () {
        var url = createPostURL(DocumentGridModel.controller, DocumentGridModel.showWhosOnlineAction);
        
        $http({
            method: 'POST',
            url: url
        }).then(function successCallback(response) {
            $scope.showWhosOnlineButton = response.data;
        });
    };

    $scope.openWhosOnlineModal = function() {
        // open the Who's online Modal
        var userData = {};
        $scope.loadWhosOnline().then(function successCallback(response) {
            userData = response.data;

            var modalInstance = $uibModal.open({
                templateUrl: DocumentGridModel.baseUrl + 'app/WhosOnline/WhosOnline.html',
                controller: 'WhosOnlineController',
                windowTopClass: 'bootstrap help-modal',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap',
                resolve: { model: [function() { return userData; }] }
            });

            modalInstance.rendered.then(function () {
                utilityService.makeModalDraggableAndResizable('#whosOnlineModal', 142, 640);
            });
        });
    };

    $scope.loadWhosOnline = function() {
        return $http({
            method: 'POST',
            url: createPostURL(DocumentGridModel.controller, DocumentGridModel.getWhosOnlineAction)
        });
    };
        
    initialize();
}]);