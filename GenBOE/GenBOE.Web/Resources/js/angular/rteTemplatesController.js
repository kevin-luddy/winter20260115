angular.module('genboe').controller('RTETemplatesController', ['$scope', '$http', '$timeout', '$filter', 'RTETemplateModel', function ($scope, $http, $timeout, $filter, RTETemplateModel) {

    $scope.data = [];
    $scope.noDirty = true;
    $scope.isLoading = false;
    $scope.errors = [];
    $scope.deleteAll = {};
    $scope.deleteAll.deleteAll = false;
    $scope.newId = -1;
    $scope.templateSources = RTETemplateModel.templateSources;
    // Manage RTE Templates can be done when workspace state is Initialization or Working
    $scope.isWorkingState = RTETemplateModel.isWorkingState;

    $scope.getCurrentlyAssignedText = function (template) {
        var assigned = [];

        $scope.templateSources.forEach(function (item) {
            if (template.Assigned.includes(item.SourceId)) {
                assigned.push(item.Description);
            }
        });

        if (assigned.length > 0) {
            return assigned.join(', ');
        } else {
            return '';
        }
    };

    $scope.edit = {
        open: false,
        template: { questions: [] },
        isAdd: false,
        isDirty: false,
        errors: [],
        deletedQuestions: [],
    };

    $scope.assign = {
        open: false,
        template: { questions: [] },
        isDirty: false,
        sources:[],
        errors: []
    };

    $scope.search = {
        open: false,
        templates: [],
        showResults: false,
        errors: [],
        searchText: '',
        isLoading: false,
        currentPage: 0,
        pageSize: 20
    };

    addError = function (errorArray, error) {
        // Do not allow duplicates
        if (!errorArray.some(function (item) {
            if (item.ValidationIssue === error) {
                return true;
            }
        })) {
            errorArray.push({ ValidationIssue: error });
        }
    }

    /**** Assign Dialog *****/

    $scope.assignTemplate = function (template) {
        $scope.assign.isDirty = false;
        $scope.assign.errors = [];
        $scope.assign.sources = angular.copy($scope.templateSources);
        $scope.assign.template = angular.copy(template);

        $scope.assign.sources.forEach(function (item) {
            item.checked = $scope.assign.template.Assigned.includes(item.SourceId);
        });

        $scope.assign.open = true;
    };

    $scope.setAssignDirty = function () {
        $scope.assign.isDirty = true;
    };

    $scope.onAssignSave = function () {
        $scope.assign.errors = [];
        var confirmUnassign = false;
        var confirmReplace = false;

        var checkedSources = $scope.assign.sources.filter(function (s) { return s.checked; }).map(function (s) { return s.SourceId; });

        // confirm if unassigning in-use template
        if ($scope.assign.template.InUse) {
            $scope.assign.template.Assigned.forEach(function (item) {
                if (!checkedSources.includes(item)) {
                    confirmUnassign = true;
                }
            });
        }

        // confirm if replacing an in-use template
        $scope.data.forEach(function (item) {
            if (item.InUse && item.Id !== $scope.assign.template.Id && item.Assigned.filter(function (a) { return checkedSources.includes(a); }).length) {
                confirmReplace = true;
            }
        });

        if (confirmUnassign) {
            Session.confirmDialog("Unassign in use Template",
                "This template is currently in use. Unassigning this template will remove all answers for this template throughout the workspace. Are you sure you want to unassign it? This cannot be undone.",
                function () {
                    $timeout(function () {
                        if (confirmReplace) {
                            $scope.confirmReplace();
                        } else {
                            $scope.continueOnAssignSave();
                        }
                    }, 50);
                },
                function () { $timeout(function () { $scope.onAssignClose(); }, 50); });
        } else if (confirmReplace) {
            $scope.confirmReplace();
        } else {
            $scope.continueOnAssignSave();
        }
    };

    $scope.confirmReplace = function () {
        Session.confirmDialog("Replace in use Template",
            "This will replace another template that is currently in use. Replacing this template will remove all answers for that template throughout the workspace. Are you sure you want to replace it? This cannot be undone.",
            function () {
                $timeout(function () { $scope.continueOnAssignSave(); }, 50);
            },
            function () { $timeout(function () { $scope.onAssignClose(); }, 50); });
    };

    $scope.continueOnAssignSave = function () {
        // set assigned items
        $scope.assign.template.Assigned = [];
        $scope.assign.sources.forEach(function (item) {
            if (item.checked) {
                $scope.assign.template.Assigned.push(item.SourceId);
            }
        });

        // get a copy of the data, and replace the template with updated values
        var data = angular.copy($scope.data);
        var templateIndex = -1;
        data.forEach(function (item, index) {
            item.Deleted = false; // no accidental deletes
            if (item.Id === $scope.assign.template.Id) {
                templateIndex = index;
                $scope.assign.template.Deleted = false;
            } else {
                // Turn off in other Templates
                $scope.assign.template.Assigned.forEach(function (id) {
                    var index = item.Assigned.indexOf(id);
                    if (index > -1) {
                        item.Assigned.splice(index, 1);
                    }
                });
            }
        });

        if (templateIndex === -1) {
            // Error, template not found
            addError($scope.assign.errors, 'Template not found when updating, please refresh the page.');
        } else {
            data[templateIndex] = $scope.assign.template;

            save(data, $scope.assign.errors, function () {
                $scope.isLoading = true;
                loadData();
                $scope.onAssignClose();
            });

        }
    };

    $scope.onAssignClose = function () {
        $scope.assign.open = false;
        $scope.assign.isDirty = false;
        $scope.assign.errors = [];
    };

    /**** Edit Dialog *****/

    $scope.addRTETemplate = function () {
        $scope.edit.isAdd = true;
        $scope.edit.errors = [];
        $scope.edit.deletedQuestions = [];
        var date = new Date();
        
        var template = {
            Questions: [],
            Description: '',
            Author: '',
            CreationDate: ((date.getMonth() > 8) ? (date.getMonth() + 1) : ('0' + (date.getMonth() + 1))) + '/' + ((date.getDate() > 9) ? date.getDate() : ('0' + date.getDate())) + '/' + date.getFullYear(),
            InUse: false,
            Assigned: [],
            Id: $scope.newId--,
            WorkspaceId: RTETemplateModel.workspaceId,
        };
        $scope.edit.template = angular.copy(template);
        $scope.edit.open = true;
    }

    $scope.sortableOptions = {
        stop: function(e, ui) {
            $scope.edit.isDirty = true;
        }
    };

    $scope.onEditSave = function () {
        // set the order for the questions
        var template = angular.copy($scope.edit.template);
        for (var index in template.Questions) {
            template.Questions[index].SortOrder = index;
        }

        $scope.edit.deletedQuestions.forEach(function (question) {
            template.Questions.push(question);
        });

        var data = [];
        data.push(template);
        save(data, $scope.edit.errors, function () {
            $scope.isLoading = true;
            loadData();
            $scope.onEditClose();
        });

    };

    $scope.onEditClose = function () {
        $scope.edit.open = false;
        $scope.edit.isDirty = false;
        $scope.edit.errors = [];
    };

    $scope.setEditDirty = function () {
        $scope.edit.isDirty = true;
    };

    $scope.editTemplate = function (template) {
        $scope.edit.isAdd = false;
        $scope.edit.errors = [];
        $scope.edit.deletedQuestions = [];
        $scope.edit.template = angular.copy(template);
        $scope.edit.open = true;
    };

    $scope.deleteQuestion = function (index) {
        if ($scope.edit.template.InUse && $scope.edit.template.Questions[index].Id > 0) {
            Session.confirmDialog('Delete Template Prompt', 'This Template is in use. Deletion of this prompt will remove all answers throughout the workspace. Are you sure you want to delete this prompt? This cannot be undone.', function () {
                $timeout(function () {
                    $scope.edit.template.Questions[index].Updateable = 2; // deleted
                    $scope.edit.deletedQuestions.push($scope.edit.template.Questions[index]);
                    $scope.edit.template.Questions.splice(index, 1);
                }, 50);
            });
        } else {
            $scope.edit.isDirty = true;
            $scope.edit.template.Questions[index].Updateable = 2; // deleted
            $scope.edit.deletedQuestions.push($scope.edit.template.Questions[index]);
            $scope.edit.template.Questions.splice(index, 1);
        }
    };

    $scope.onEditBlur = function () {
        $scope.edit.isDirty = true;
    };

    $scope.addQuestion = function () {
        var question = {
            Id: $scope.newId--,
            Text: '',
            Required: true,
            SortOrder: 1000,
            Updateable: 0,
        };

        $scope.edit.isDirty = true;
        $scope.edit.template.Questions.push(question);
    }

    /**** Search Template *****/

    $scope.onSearchOpen = function () {
        $scope.search.showResults = false;
        $scope.search.errors = [];
        $scope.search.searchText = '';
        $scope.templates = [];
        $scope.search.isLoading = false;
        $scope.search.currentPage = 0;
        $scope.search.open = true;
    };

    $scope.onSearchClose = function () {
        $scope.search.open = false;
        $scope.search.isLoading = false;
    };

    $scope.searchButtonDisabled = function () {
        return $scope.search.isLoading || $scope.search.searchText === '';
    }

    $scope.onSearchTemplate = function () {
        if (!$scope.searchButtonDisabled()) {
            $scope.search.currentPage = 0;
            var data = {};
            data.search = $scope.search.searchText;
            $scope.search.isLoading = true;

            return $http({
                method: 'POST',
                data: data,
                url: CreatePostURL(RTETemplateModel.workspace, RTETemplateModel.controller, RTETemplateModel.searchAction, '')
            }).then(function (response) {
                if ($scope.search.isLoading) {
                    $scope.search.errors = [];

                    $scope.search.templates = response.data;
                    $scope.search.showResults = true;
                    $scope.search.isLoading = false;
                }

            }, function errorCallback(response) {
                if (response.data && response.data.MessageList) {
                    // put the errors into the appropriate error boxes
                    angular.forEach(response.data.MessageList, function (value) {
                        $scope.search.errors.push(value);
                    });
                } else {
                    $scope.search.errors = [];
                    addError($scope.search.errors, "Error searching for Templates.");
                }

                $scope.search.isLoading = false;
            });
        }
    }

    $scope.copyTemplate = function (template) {
        Session.confirmDialog('Copy Template', 'Copy the Template "' + template.Description + '" from Workspace "' + template.WorkspaceName + '" into the current Workspace?', function () {
            $timeout(function () {
                $scope.search.errors = [];
                $(document).trigger("SHOW_LOADING_BOX");
                var data = {
                    templateId: template.Id,
                };

                return $http({
                    method: 'POST',
                    data: data,
                    url: CreatePostURL(RTETemplateModel.workspace, RTETemplateModel.controller, RTETemplateModel.copyAction, '')
                }).then(function (response) {
                    $(document).trigger("HIDE_LOADING_BOX");
                    $scope.search.errors = [];

                    // alert template copied
                    RaiseNotification('The Template was copied successfully');

                    $scope.search.open = false;

                    loadData();
                }, function errorCallback(response) {
                    $scope.search.errors = [];
                    $(document).trigger("HIDE_LOADING_BOX");
                    if (response.data && response.data.MessageList) {
                        // put the errors into the appropriate error boxes
                        angular.forEach(response.data.MessageList, function (value) {
                            $scope.search.errors.push(value);
                        });
                    } else {
                        addError($scope.search.errors, "Error copying Template.");
                    }
                });
            }, 50);
        });
    };

    $scope.numberOfSearchPages = function () {
        var totalRows = angular.isDefined($scope.search.templates) ? $scope.search.templates.length : 0;
        return Math.ceil(totalRows / $scope.search.pageSize);
    }

    /**** Delete Template *****/

    $scope.toggleDeleteAll = function () {
        if ($scope.deleteAll.deleteAll === false) {
            // unmark ALL
            $scope.data.forEach(function (item) {
                item.Deleted = $scope.deleteAll.deleteAll;
            });
        } else {
            // get all visible Ids
            var Ids = $('input.delete-chck:not([disabled])').parents('tr').map(function () { return $(this).attr("pkid"); }).get();

            // mark items appropriately
            $scope.data.forEach(function (item) {
                item.Deleted = $.inArray(item.Id + '', Ids) !== -1;
            });
        }
    };

    $scope.disableDelete = function () {
        var disabled = true;
        $scope.data.forEach(function (item) {
            if (item.Deleted) {
                disabled = false;
            }
        });

        return disabled;
    };

    $scope.delete = function () {
        Session.confirmDialog('Delete Custom RTE Templates', 'Are you sure you want to delete these Custom RTE Templates? This cannot be undone.', function () {
            $timeout(function () {
                var data = {};
                data.templates = [];
                var inUse = false;
                $scope.data.forEach(function (item) {
                    if (item.Deleted) {
                        item.Updateable = 2; // deleted
                        data.templates.push(item);
                        if (item.InUse) {
                            inUse = true;
                        }
                    }
                });

                if (inUse) {
                    Session.confirmDialog('Delete In Use Custom RTE Templates', 'One or more Templates to be deleted is in use.  Deletion will remove all prompts and associated user responses. This cannot be undone.  Are you sure you want to delete these Custom RTE Templates?', function () {
                        $timeout(function () {
                            deleteTemplateConfirmed(data);
                        }, 50);
                    });
                } else {
                    deleteTemplateConfirmed(data);
                }
            }, 50);
        });
    };

    var deleteTemplateConfirmed = function (data) {
        var data = {};
        data.templates = [];

        $scope.data.forEach(function (item) {
            if (item.Deleted) {
                item.Updateable = 2; // deleted
                data.templates.push(item);
            }
        });

        $scope.errors = [];

        $(document).trigger("SHOW_LOADING_BOX");
        $http({
            method: 'POST',
            url: CreatePostURL(RTETemplateModel.workspace, RTETemplateModel.controller, RTETemplateModel.saveAction, ''),
            data: data
        }).then(function successCallback(response) {

            // remove the items from the data []
            data.templates.forEach(function (item) {
                removeTemplate(item);
            });

            loadData();
            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.errors = response.data.MessageList;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    var removeTemplate = function (template) {
        var index = -1;

        for (var i = 0; i < $scope.data.length; i++) {
            if ($scope.data[i].Id == template.Id) {
                index = i;
            }
        }

        if (index >= 0) {
            $scope.data.splice(index, 1);
        }
    };

    var save = function (updatedData, errors, callback) {
        $(document).trigger("SHOW_LOADING_BOX");

        var data = { templates: updatedData };

        return $http({
            method: 'POST',
            data: data,
            url: CreatePostURL(RTETemplateModel.workspace, RTETemplateModel.controller, RTETemplateModel.saveAction, '')
        }).then(function (response) {

            errors.length = 0;
            $(document).trigger("HIDE_LOADING_BOX");

            if (callback && typeof callback === 'function') {
                callback();
            }

        }, function errorCallback(response) {
            errors.length = 0;
            if (response.data && response.data.MessageList) {
                // put the errors into the appropriate error boxes
                angular.forEach(response.data.MessageList, function (value) {
                    errors.push(value);
                });
            } else {
                addError(errors, "Error saving Templates.");
            }

            $(document).trigger("HIDE_LOADING_BOX");
        });
    }
    
    var loadData = function () {
        $scope.isLoading = true;
        $scope.data = [];
        $scope.deleteAll.deleteAll = false;

        return $http({
            method: 'POST',
            url: CreatePostURL(RTETemplateModel.workspace, RTETemplateModel.controller, RTETemplateModel.action, '')
        }).then(function (response) {
            
            $scope.noDirty = true;
            $scope.data = response.data;
            $scope.isLoading = false;
        });
    }

    loadData();
}]);