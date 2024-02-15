angular.module('genboe').controller('permissionCtrl', ['$scope', '$http', 'WorkspacePermissionModel', function ($scope, $http, WorkspacePermissionModel) {
    // Get Data
    $scope.isLoading = true;
    
    // Initialize Models
    $scope.filter = {};
    $scope.filter.Users = '';
    $scope.filter.Roles = '';
    $scope.filter.GroupName = '';
    $scope.filter.Access = '';
    $scope.data = {};
    $scope.data.ntids = '';
    $scope.data.Roles = [];
    $scope.dialog = {};
    $scope.dialog.title = 'Add/Edit Permissions';
    $scope.dialog.open = false;
    $scope.dialog.errors = [];

    $scope.importExportDialog = {
        title: 'Import/Export Permissions',
        open: false,
        validBOEs: [],
        file: null,
        importWorking: false,
        disableImport: true,
        invalidData: false,
        importResults: [],
        importExport: '',
        exportValue: 'Export',
        importValue: 'Import',
        exportSelect: '',
    };


    // Perform filter
    $scope.filterPermissions = function (permission) {
        var found = true;

        var loopFound = false;
        for (var i = 0; i < permission.Users.length; i++) {
            if (permission.Users[i].DisplayName.toLowerCase().indexOf($scope.filter.Users.toLowerCase()) != -1 && permission.Users[i].DisplayName.toLowerCase().indexOf(".") == -1
                || $scope.filter.Users == "") {
                loopFound = true;
                break;
            }
        }
        found = found && loopFound;

        loopFound = false;
        for (var i = 0; i < permission.Roles.length; i++) {
            if (permission.Roles[i].RoleName.toLowerCase().indexOf($scope.filter.Roles.toLowerCase()) != -1) {
                loopFound = true;
                break;
            }
        }
        found = found && loopFound;

        loopFound = false;
        for (var i = 0; i < permission.Users.length; i++) {
            if ((permission.Users[i].DisplayName.toLowerCase().indexOf($scope.filter.GroupName.toLowerCase()) != -1 && permission.Users[i].DisplayName.toLowerCase().indexOf(".") != -1)
                || $scope.filter.GroupName == "") {
                loopFound = true;
                break;
            }
        }
        found = found && loopFound;

        loopFound = false;        
        if (permission.genBOEAccess.toLowerCase().indexOf($scope.filter.Access.toLowerCase()) != -1) {
            loopFound = true;
        }        
        found = found && loopFound;

        return found;
    }

    // Open a dialog for import/export
    $scope.toggleImportExport = function () {
        $scope.importExportDialog.importExport = '';
        $scope.importExportDialog.exportSelect = '';
        $scope.importExportDialog.disableImport = true;
        $scope.importExportDialog.importWorking = false;
        $scope.importExportDialog.open = !$scope.importExportDialog.open;
        $scope.importExportDialog.errors = [];

        // clear file input field
        resetUploadForm();
    };

    $scope.exportPermissions = function () {
        // remove old iframe
        $('#DownloadTarget-ExportPermission').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DownloadTarget-ExportPermission',
            'class': 'display-none',
            'src': CreatePostURL(WorkspacePermissionModel.workspace, WorkspacePermissionModel.controller, WorkspacePermissionModel.exportAction, '')
        });
        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');
        

        $scope.toggleImportExport();
    };

    $scope.importPermissions = function () {
        if (!$scope.importExportDialog.disableImport) {
            // create form data
            var fd = new FormData();
            fd.append("file", $scope.importExportDialog.file);

            // get url from form
            var url = $('#ImportExportPermissionsDialog-Form').attr('action');
            $scope.importExportDialog.importWorking = true;

            $http.post(url, fd, {
                headers: {
                    'Content-Type': undefined
                }
            }).then(function (response) {
                $scope.permissions = response.data.Permissions;
                $scope.importExportDialog.importWorking = false;
                $scope.importExportDialog.open = false;
                RaiseNotification('Import Successful');
            })
            .catch(function (response) {
                $scope.importExportDialog.errors = response.data.MessageList;
                $scope.importExportDialog.importWorking = false;
            });                ;
        }
    };

    $scope.fileUploadChange = function (element) {
        $scope.$apply(function ($scope) {
            $scope.importExportDialog.disableImport = element.value.endsWith('.xlsx') ? false : true;
            $scope.importExportDialog.file = $scope.importExportDialog.disableImport ? null : element.files[0];
        });
    };


    /*
     * **************************** NOTE *****************************
     * Functions below relate to the logic of the dialog import/export
     * ***************************************************************
     */
    
    var resetUploadForm = function () {
        $("#ImportExportPermissionsDialog-Form")[0].reset();
        $scope.importExportDialog.disableImport = true;
        $scope.importExportDialog.file = null;
    };

    // Open a blank dialog
    $scope.openNewDialog = function (isProjectMapWs) {
        $scope.data.ntids = "";
        $scope.data.isEdit = false;
        $scope.data.Author = false;
        // 'Workspace Admin' role is selected by default (isProjectMapWs = 4) for Project Map Workspaces.
        $scope.data.WorkspaceAdmin = isProjectMapWs;
        $scope.data.Approver = false;
        $scope.data.WorkspaceReviewer = false;
        $scope.data.SubcontractorAuthor = false;
        $scope.data.SubcontractAdmin = false;
        $scope.dialog.errors = [];
        $scope.dialog.open = true;
        $scope.dialog.title = 'Add Permissions';
    };

    // Open a dialog for edits
    $scope.openEditDialog = function (permission) {
        if (permission.isGroup) {
            $scope.dialog.title = 'Edit Group Permission';
        } else {
            $scope.dialog.title = 'Edit User Permission';
        }
        $scope.data.ntids = permission.Users[0].DisplayName;
        $scope.data.EntityID = permission.Users[0].UserID;
        $scope.data.type = "user";
        $scope.data.isEdit = true;
        $scope.data.Author = hasRole(permission, "Author");
        $scope.data.WorkspaceAdmin = hasRole(permission, "Workspace Administrator");
        $scope.data.WorkspaceReviewer = hasRole(permission, "Workspace Reviewer");
        $scope.data.Approver = hasRole(permission, "Approver");
        $scope.data.SubcontractorAuthor = hasRole(permission, "Subcontractor Author");
        $scope.data.SubcontractAdmin = hasRole(permission, "GSCO Administrator");
        
        $scope.dialog.errors = [];
        $scope.dialog.open = true;
    };

    // Close dialog
    $scope.closeDialog = function () {
        $scope.dialog.open = false;
    };

    // Delete a permission
    $scope.deletePermission = function (permission) {
        var deleteSelf = false;
        var dialog = {};
        dialog.title = 'Delete User Confirmation';
        dialog.text = 'Are you sure you want to delete this user?';

        $scope.adminPermissions = getAdminPermissions($scope.permissions, $scope.currentUserName, $scope.currentUserId);

        // if there's only one admin permission for the current user and it's current being deleted, display a dialog.
        if ($scope.adminPermissions.Groups.length + $scope.adminPermissions.Users.length == 1 &&
            ($scope.adminPermissions.Groups.length && permission.Users[0].UserID == $scope.adminPermissions.Groups[0] ||
            $scope.adminPermissions.Users.length && permission.Users[0].UserID == $scope.adminPermissions.Users[0])) {
            dialog.title = 'Please Confirm Action';
            dialog.text = 'Performing this action will remove role as a Workspace Administrator. You will no longer be able to perform any Workspace Administrator functions.  Are you sure you want to remove this permission?';
            deleteSelf = true;
        }

        GenSession.confirmDialog(dialog.title, dialog.text, function () {
            $scope.$apply(function () {
                $scope.errors = [];
                var saveData = {};
                saveData.inUserID = permission.Users[0].UserID;
                $http.post('DeleteUserPermissions', saveData)
                    .then(function () {
                        removePermission(permission);
                        if (deleteSelf) { //no permissions -> send home
                            window.location.assign('/');
                        }
                    })
                    .catch(function (response) {
                        $scope.errors = response.data.MessageList;
                    });
            });
        }, function () {
        });
    };

    // Save a permission
    $scope.savePermission = function (result) {
        populateRoles();
        var saveElement = $("#savePermissionsButton");
        var loaderElement = $("#permissionsLoader");
        loaderElement.removeClass('display-none');
        saveElement.addClass('display-none');

        var saveData = {};
        var url;

        if ($scope.data.isEdit) {
            saveData.inEntityId = $scope.data.EntityID;
            saveData.inRoles = $scope.data.Roles;
            saveData.inType = $scope.data.type;
            url = 'EditPermissions';
        } else {
            saveData.EntityIds = $scope.data.ntids;
            saveData.Roles = $scope.data.Roles;
            url = 'SaveNewPermissions';
        }

        $http.post(url, saveData).then(function (response) {
            $scope.permissions = response.data.Permissions;
            loaderElement.addClass('display-none');
            saveElement.removeClass('display-none');
            $scope.dialog.open = false;
        }).catch(function (response) {
            $scope.dialog.errors = response.data.MessageList;
            loaderElement.addClass('display-none');
            saveElement.removeClass('display-none');
        });
       
    }

    // Get all permissions where the user has admin
    var getAdminPermissions = function (permissions, userName, userId) {
        var adminPermissions = {};
        adminPermissions.Groups = [];
        adminPermissions.Users = [];

        // foreach permission
        for (var i = 0; i < permissions.length; i++) {
            var containsAdminRole = false;

            // determine if the role array contains workspace admin
            for (var j = 0; j < permissions[i].Roles.length; j++) {
                if (permissions[i].Roles[j].RoleID == 4) {
                    containsAdminRole = true;
                }
            }

            // if it does, determine if the user array contains the user
            if (containsAdminRole) {
                for (var k = 0; k < permissions[i].Users.length; k++) {
                    if (userId == permissions[i].Users[k].UserID) {
                        adminPermissions.Users.push(permissions[i].Users[k].UserID);
                    }
                    else if (permissions[i].isGroup == true) {
                        for (var j = 0; j < permissions[i].groupMembers.length; j++) {
                            if (userName == permissions[i].groupMembers[j]) {
                                adminPermissions.Groups.push(permissions[i].Users[k].UserID);
                            }
                        }
                    }
                }
            }
        }

        return adminPermissions;
    };

    // Determine if the user has the given role
    var hasRole = function (permission, role) {
        for (var i = 0; i < permission.Roles.length; i++) {
            if (permission.Roles[i].RoleName == role) {
                return permission.Roles[i].RoleID;
            }
        }
        return false;
    };

    // Populate the data.Roles property
    var populateRoles = function () {
        $scope.data.Roles = [];
        if (typeof ($scope.data.WorkspaceAdmin) !== 'undefined' && $scope.data.WorkspaceAdmin != false) {
            $scope.data.Roles.push($scope.data.WorkspaceAdmin);
        }
        if (typeof ($scope.data.Author) !== 'undefined' && $scope.data.Author != false) {
            $scope.data.Roles.push($scope.data.Author);
        }
        if (typeof ($scope.data.SubcontractorAuthor) !== 'undefined' && $scope.data.SubcontractorAuthor != false) {
            $scope.data.Roles.push($scope.data.SubcontractorAuthor);
        }
        if (typeof ($scope.data.WorkspaceReviewer) !== 'undefined' && $scope.data.WorkspaceReviewer != false) {
            $scope.data.Roles.push($scope.data.WorkspaceReviewer);
        }
        if (typeof ($scope.data.Approver) !== 'undefined' && $scope.data.Approver != false) {
            $scope.data.Roles.push($scope.data.Approver);
        }
        if (typeof ($scope.data.SubcontractAdmin) !== 'undefined' && $scope.data.SubcontractAdmin != false) {
            $scope.data.Roles.push($scope.data.SubcontractAdmin);
        }
        
    }

    // remove a permission from the grid
    var removePermission = function (permission) {
        var index;
        for (var i = 0; i < $scope.permissions.length; i++) {
            if ($scope.permissions[i].GroupID == permission.GroupID && $scope.permissions[i].Users[0].UserID == permission.Users[0].UserID) {
                index = i;
            }
        }

        $scope.permissions.splice(index, 1);
    };

    $scope.ViewUsersClick = function (permission, showUsers) {
        if (showUsers && permission.groupMembers.length<2) { //only for first click - 0 for empty group, 1 for group with current user added
            var sd = {};
            sd.groupName = permission.Users[0].DisplayName;
            permission.loadingGroups = true;
            $http.post('GetGroupMembers', sd)
                        .then(function (response) {
                            for (var i = 0; i < $scope.permissions.length; i++) {
                                if ($scope.permissions[i] === permission) {
                                    $scope.permissions[i].groupMembers = response.data;
                                }
                            }
                            permission.loadingGroups = false;
                        }
                        ).catch(function () {
                            $scope.permissions[i].groupMembers = ['Error loading group members'];
                            permission.loadingGroups = false;
                        });
        }
    }

    var loadPermissions = function () {
        $scope.isLoading = true;
        $http({
            method: 'POST',
            url: WorkspacePermissionModel.action
        }).then(function (response) {
            var data = response.data;
            $scope.permissions = data.Permissions;
            $scope.currentUserId = data.CurrentUserId;
            $scope.currentUserName = data.CurrentUserDisplayName
            $scope.adminPermissions = getAdminPermissions(data.Permissions, data.CurrentUserDisplayName, data.CurrentUserId);
            $scope.isLoading = false;
        });
    };

    loadPermissions();
}]);