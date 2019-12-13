angular.module('RDM').controller('FileAttachmentController', ['$scope', '$http', '$rootScope', '$timeout', 'lockService', 'utilityService', 'uiGridConstants', 'FileAttachmentModel', function ($scope, $http, $rootScope, $timeout, lockService, utilityService, uiGridConstants, FileAttachmentModel) {

    $scope.isInitializing = true;
    $scope.isDataLoading = true;
    $scope.isDataError = false;
    $scope.selectedVersion;
    $scope.sections = [];
    $scope.versions = [];
    $scope.lockInfo;
    $scope.showEditLockTimeoutExpiration = false;
    $rootScope.errors = [];
    $scope.addedFileAttachmentId = -1;
    $scope.canEdit = function () { return $scope.lockInfo !== undefined && !$scope.lockInfo.IsReadOnly; };
    lockService.id = FileAttachmentModel.Area;

    $scope.initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };

    $scope.loadData = function() {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = true;
            $scope.clearLockTimers();
            if ($scope.canEdit()) {
                $scope.attachmentForm.$setPristine();
            }
         
            // Load data.  If SelectedRevisionId is null, then get WIP data.
            $scope.loadFileAttachments(FileAttachmentModel.SelectedRevisionId).then(function successCallback(response) {
                $scope.data = response.data.FileAttachments;
                $scope.versions = response.data.Versions;
                $scope.sections = response.data.Sections;
                $scope.selectedVersion = $scope.versions[0];    // default to WIP revision
                if (FileAttachmentModel.SelectedRevisionId) {
                    for (var i = 0; i < $scope.versions.length; i++) {
                        var version = $scope.versions[i];
                        if (version.Id == FileAttachmentModel.SelectedRevisionId) {
                            $scope.selectedVersion = version;
                        }
                    }
                }
                $scope.lockInfo = response.data.LockInfo;
                $scope.addSectionLabels();
                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $scope.isDataLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            });
        }
    };

    $scope.loadFileAttachments = function (versionId) {
        return $http({
            method: 'POST',
            data: { versionId: versionId },
            url: createPostURL(FileAttachmentModel.controller, FileAttachmentModel.getAction)
        });
    };

    $scope.deleteFileAttachment = function (row) {
        // clear error message
        $scope.isDataValid = true;
        row.IsDeleted = true;
        $scope.attachmentForm.$setDirty();
        row.D = true;

        $scope.refreshLock();
     };

    /* Adds a new (blank) row to the grid. */
    $scope.addFileAttachment = function() {
        // If a row is added, mark the page as dirty
        $scope.attachmentForm.$setDirty();
        // Insert a new row (at top of list).
        $scope.data.unshift({ Id: $scope.addedFileAttachmentId, IsDeleted: false, D: true, SectionId: 0 });
    };

    $scope.addSectionLabels = function() {
        $scope.data.forEach(function (row) {
            if (row.SectionId !== undefined && row.SectionId !== 0){
                row.Section = $.grep($scope.sections, function (e) { return e.Id == row.SectionId; })[0].Label;
            }
        });
    };

    $scope.saveFileAttachmentGrid = function (releaseLock) {
        // clear error message
        $rootScope.errors = [];
        $scope.isDataValid = true;
        $(document).trigger("SHOW_LOADING_BOX");

        // Clear lock timers so there's no pop-up in the middle of a save
        $scope.clearLockTimers();

        var saveUrl = createPostURL(FileAttachmentModel.controller, FileAttachmentModel.saveAction);

        var dirtyRows = $scope.data.filter(function (row) { return row.D === true; });

        return $http({
            method: 'POST',
            url: saveUrl,
            data: JSON.stringify(dirtyRows)
        }).then(function successCallback(response) {
            $scope.reloadGrid(releaseLock);
        }, function errorCallback(response) {
            $scope.refreshLock(); // Refresh lock to give user time to correct errors
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.showConfirmDelete = function (fileAttachment) {
        ConfirmDialog("Delete Attachment", "Are you sure you want to delete this File Attachment?", function () {
            $scope.deleteFileAttachment(fileAttachment);
        });
    };

    $scope.reloadGrid = function (releaseLock) {
        // If Save & Continue make a copy of the lockInfo.
        var tempLockInfo = null;
        if (!releaseLock && $scope.lockInfo !== undefined && $scope.lockInfo.InUse !== null) {
            tempLockInfo = $scope.lockInfo;
        }
        // AJAX call to backend to get JSON data
        $(document).trigger("SHOW_LOADING_BOX");
        // clear error message
        $rootScope.errors = [];
        $scope.isDataLoading = true;
        if ($scope.canEdit()) {
            $scope.attachmentForm.$setPristine();
        }

        // use $scope.selectedVersion.Id for the version id
        $scope.loadFileAttachments($scope.selectedVersion.Id).then(function successCallback(response) {
            $scope.data = response.data.FileAttachments;
	        $scope.sections = response.data.Sections;
            if (releaseLock !== undefined && !releaseLock && tempLockInfo !== null) {
                $scope.lockInfo = tempLockInfo;
                $scope.refreshLock();
            } else {
                $scope.lockInfo = response.data.LockInfo;
                $scope.releaseLock();
            }

            $scope.addSectionLabels();
            $scope.isDataLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.isDataLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.lock = function () {
        lockService.lock().then(
            function (response) {
                $scope.lockInfo = response.data.LockInfo;
                lockService.processResponse(response,
                    $scope.lockInfo.EditLockTimeoutWarningMinutes,
                    $scope.refreshLock,
                    $scope.lockInfo.EditLockTimeoutExpirationMinutes,
                    $scope.editLockTimeoutExpirationCallback,
                    $scope.clearLockTimers,
                    $scope.reloadGrid(false),
                    false);
            },
            function () {
                lockService.requestRejected();
            });
    };

    $scope.clearLockTimers = function () {
        $scope.showEditLockTimeoutExpiration = false;
        lockService.clearLockTimers();
    };

    $scope.editLockTimeoutExpirationCallback = function () {
        $scope.showEditLockTimeoutExpiration = true;
    };

    $scope.refreshLock = function () {
        lockService.refreshLock().then(
            function (response) {
                $scope.lockInfo = response.data.LockInfo;
                lockService.processResponse(response,
                    $scope.lockInfo.EditLockTimeoutWarningMinutes,
                    $scope.refreshLock,
                    $scope.lockInfo.EditLockTimeoutExpirationMinutes,
                    $scope.editLockTimeoutExpirationCallback,
                    $scope.clearLockTimers,
                    null,
                    true);
            },
            function () {
                lockService.requestRejected();
            });
    };

    $scope.releaseLock = function () {
        // See if we have a lock.
        if (lockService !== undefined && lockService.id !== undefined && $scope.canEdit()) {
            lockService.unlock().then(
                function (response) {
                    $scope.lockInfo = response.data.LockInfo;
                    lockService.processResponseRelease(response, $scope.clearLockTimers);
                },
                function () {
                    lockService.requestRejected();
                });
        }
    };

    $scope.confirmCancel = function () {
        if ($scope.attachmentForm.$dirty) {
            ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                // Yes
                $scope.reloadGrid(true);
            },
            function () {
                // No
                return false;
            });
       } else {
            // If no changes, continue with reloadGrid
            $scope.reloadGrid(true);
        }
    };

    window.onbeforeunload = function () {
        // Confirm leaving/refreshing page if the page is dirty
        if ($scope.canEdit() && $scope.attachmentForm.$dirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    window.onunload = function () {
        // If there is a lock and user owns it, remove the lock
        // Checks for this are done on the back end in the Home Controller Unlock() method
        if (lockService !== undefined && lockService.id !== undefined && $scope.canEdit()) {
            lockService.synchronousUnlock();
        }
    };

    // Initialization functions
    $scope.initialize();
}]);