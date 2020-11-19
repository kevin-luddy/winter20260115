angular.module('genboe').controller('ProPricerController', ['$scope', '$document', 'ProPricerModel', '$http', function ($scope, $document, ProPricerModel, $http) {
    $scope.dataIsLoading = true;
    $scope.model = ProPricerModel;
    $scope.model.sendToProPricerModalOpen = false;
    $scope.showInstanceLoader = true;
    $scope.errors = [];

    /* Send To Pro Pricer Dialog variables */
    $scope.proPricerDataIsLoading = false;
    $scope.showPreview = false;
    $scope.previewIsLoading = true;
    $scope.model.SendToProPricerData.resourceDuplicates = null;
    $scope.model.SendToProPricerData.taskDuplicates = null;
    $scope.model.SendToProPricerData.taskReplaceAll = false;
    $scope.model.SendToProPricerData.resourceReplaceAll = false;
    $scope.model.SendToProPricerData.showNextButton = true;
    $scope.model.SendToProPricerData.showButtonLoader = false;
    $scope.model.SendToProPricerData.step = 1;  // 1 is select proposal, 2 is preview
    $scope.rendered = false;
    $scope.apiErrors = [];
    $scope.taskHeaders = [];
    $scope.resourceHeaders = [];
    $scope.formatTaskHeaders = [];
    $scope.formatResourceHeaders = [];
    $scope.previewTaskRows = [];
    $scope.previewResourceRows = [];
    $scope.taskExportResults = [];
    $scope.resourceExportResults = [];
    $scope.filtering = false;
    $scope.showTaskResultsError = false;
    $scope.showResourceResultsError = false;

    $scope.initialize = function () {
        $scope.model.SendToProPricerData.ProPricerUrl = ProPricerModel.ProPricerUrl;
        $scope.model.SendToProPricerData.LastProposalAction = ProPricerModel.LastProposalAction;
        $scope.model.SendToProPricerData.LastInstanceAction = ProPricerModel.LastInstanceAction;
        $scope.model.SendToProPricerData.PreviewAction = ProPricerModel.PreviewAction;
        $scope.model.SendToProPricerData.ExportAction = ProPricerModel.ExportAction;
        $scope.model.SendToProPricerData.Controller = ProPricerModel.Controller;
        $scope.model.SendToProPricerData.Workspace = ProPricerModel.Workspace;
        $scope.model.SendToProPricerData.SelectedProposalName = '<Unselected>';

        $scope.getProposalInstances();
    };

    $scope.getProposalInstances = function () {
        
        $http({
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            withCredentials: true,
            url: $scope.model.ProPricerUrl + 'poolinstance'
        }).then(function (response) {
            $scope.model.SendToProPricerData.Instances = response.data;

            if (response.data[0].IsBackup) {
                $('#backupPropricer').show();
            }

            // auto-select the instance/schema if there is only one
            if (response.data.length == 1) {
                $scope.model.SendToProPricerData.SelectedInstance = response.data[0].Id;
            }

            // try to get the selected proposal's name
            if ($scope.model.SendToProPricerData.SelectedProposalId !== '') {
                $http({
                    method: 'GET',
                    headers: { 'Content-Type': 'application/json' },
                    withCredentials: true,
                    url: $scope.model.ProPricerUrl + 'proposals/' + $scope.model.SendToProPricerData.SelectedInstance + '/' + $scope.model.SendToProPricerData.SelectedProposalId
                }).then(function (response) {
                    if (response.data.Name !== undefined) {
                        $scope.model.SendToProPricerData.SelectedProposalName = response.data.Name;
                    } else {
                        // the name was not found...reset the selected proposal Id
                        $scope.model.SendToProPricerData.SelectedProposalId = '';
                    }
                }, function (response) {
                    // the name was not found...reset the selected proposal Id
                    $scope.model.SendToProPricerData.SelectedProposalId = '';
                });
            }

            $scope.showInstanceLoader = false;
        }, function (response) {
            // error handler
            var errors = [];
            var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
            errors.push(error);

            $scope.errors = errors;
            $scope.showInstanceLoader = false;
            $('.send').hide();
        });
    };

    $scope.SendToProPricerClicked = function (formatId, formatName, scope) {
        $scope.model.SendToProPricerData.formatId = formatId;
        $scope.model.SendToProPricerData.formatName = formatName;
        $scope.model.SendToProPricerData.scope = scope;

        $scope.model.SendToProPricerData.Proposals = [];

        $scope.model.sendToProPricerModalOpen = true;
    };

    $scope.CopyToSystemLevel = function (formatId) {
        GenSession.ShowLoadingBox();
        $scope.errors = [];

        $http({
            method: 'POST',
            url: GenSession.CreatePostURL($scope.model.SendToProPricerData.Workspace, $scope.model.SendToProPricerData.Controller, $scope.model.CopyToSystemFormatAction),
            data: { reportId: formatId }
        }).then(function (response) {
            ExportToProPricerWidget.ReloadExportGrid();
            GenSession.HideLoadingBox();
            RaiseNotification("Successful Copy to System Level.");
        }, function (response) {
            // error handler
            GenSession.HideLoadingBox();
            var errors = [];
            var error = { ValidationIssue: 'Error during copy: ' + response.data.Message};
            errors.push(error);

            $scope.errors = errors;
        });
    };

    $scope.closeDialog = function () {
        $scope.model.sendToProPricerModalOpen = false;
        $scope.model.SendToProPricerData.step = 2;
        $scope.previousStep();
    };

    /* Send To Pro Pricer Dialog functions */

    $scope.instanceSelected = function () {
        return $scope.model.SendToProPricerData.SelectedInstance !== null && $scope.model.SendToProPricerData.SelectedInstance !== -1 && $scope.model.SendToProPricerData.SelectedInstance !== '' && $scope.model.SendToProPricerData.SelectedInstance !== undefined;
    };

    $scope.dialogInitialize = function () {
        if (!$scope.proPricerDataIsLoading && $scope.model.SendToProPricerData.SelectedProposalId != '') {
            // the data has already loaded, expand the node
            $scope.collapseAll();
            expandNode($scope.model.SendToProPricerData.SelectedProposalId, $scope.model.SendToProPricerData.Proposals);
        }

        $scope.getProposalsForInstance();
    };

    /* toggle ui tree expand/collapse node */
    $scope.toggleFolder = function (scope) {
        scope.node.expanded = !scope.node.expanded;
        if (scope.node.expanded && scope.node.Children === undefined) {
            // copy the children over so UI shows them (lazy load)
            scope.node.Children = scope.node.Ce;
        }
    };

    /* Match all of the proposals to show them all */
    $scope.matchAll = function (items) {
        // Set the filter to match all of the items
        if (items !== undefined) {
            items.forEach(function (item) {
                item.matched = true;
                item.lowerName = item.Name.toLowerCase();
                if (!item.IsP && item.Ce != undefined && item.Ce != null && item.Ce.length > 0) {
                    $scope.matchAll(item.Ce);
                }
            });
        }
    };

    $scope.searchProposals = function () {
        var searchText = $scope.model.SendToProPricerData.searchText.toLowerCase();
        if ($scope.filtering) {
            if (searchText.length <= 2) {
                // disable filtering
                $scope.filtering = false;
                $scope.matchAll($scope.model.SendToProPricerData.Proposals);
                $scope.collapseAll();
            } else {
                $scope.filterSearch(searchText, $scope.model.SendToProPricerData.Proposals);
            }
        } else {
            // not currently filtering
            if (searchText.length > 2) {
                // enable filtering
                $scope.filtering = true;
                $scope.filterSearch(searchText, $scope.model.SendToProPricerData.Proposals);
            }
        }
    };

    /* Recursively search and match items in a tree */
    $scope.filterSearch = function (searchText, items) {
        var match = false;
        if (items !== undefined) {
            items.forEach(function (item) {
                var itemMatch = false;
                // look in children for match first
                if (!item.IsP && item.Ce != undefined && item.Ce != null && item.Ce.length > 0) {
                    itemMatch = $scope.filterSearch(searchText, item.Ce);
                }

                if (itemMatch || item.lowerName.contains(searchText)) {
                    item.matched = true;
                    match = true;

                    // special case to show children if the text matches a folder
                    if (!itemMatch && !item.IsP && item.matched && item.Ce != undefined && item.Ce != null && item.Ce.length > 0) {
                        $scope.matchAll(item.Ce);
                    }
                } else {
                    item.matched = false;
                }
            });
        }

        return match;
    };

    $scope.collapseAll = function () {
        $scope.collapseAllRecursive($scope.model.SendToProPricerData.Proposals);
    };

    $scope.collapseAllRecursive = function (items) {
        if (items !== undefined) {
            items.forEach(function (item) {
                item.expanded = false;
                if (!item.IsP && item.Children != undefined && item.Children != null && item.Children.length > 0) {
                    $scope.collapseAllRecursive(item.Children);
                }
            });
        }
    };

    $scope.expandAll = function () {
        $scope.expandAllRecursive($scope.model.SendToProPricerData.Proposals);
    };

    $scope.expandAllRecursive = function (items) {
        if (items !== undefined) {
            items.forEach(function (item) {
                if (item.matched && !item.expanded) {
                    item.expanded = true;

                    if (item.Children === undefined) {
                        // copy the children over so UI shows them (lazy load)
                        item.Children = item.Ce;
                    }

                    if (!item.IsP && item.Children != undefined && item.Children != null && item.Children.length > 0) {
                        $scope.expandAllRecursive(item.Children);
                    }
                }
            });
        }
    };

    $scope.selectedInstanceChanged = function () {
        $scope.model.SendToProPricerData.SelectedProposalId = '';
        $scope.model.SendToProPricerData.SelectedProposalName = '<Unselected>';
        $scope.getProposalsForInstance();

        $http({
            method: 'POST',
            url: GenSession.CreatePostURL($scope.model.SendToProPricerData.Workspace, $scope.model.SendToProPricerData.Controller, $scope.model.SendToProPricerData.LastInstanceAction),
            data: { selectedInstance: $scope.model.SendToProPricerData.SelectedInstance }
        }).then(function (response) {
            $http({
                method: 'POST',
                url: GenSession.CreatePostURL($scope.model.SendToProPricerData.Workspace, $scope.model.SendToProPricerData.Controller, $scope.model.SendToProPricerData.LastProposalAction),
                data: { proposalId: '' }
            }).then(function (response) {
            });
        });
    }

    $scope.getProposalsForInstance = function () {
        $scope.model.SendToProPricerData.Proposals = [];
        $scope.apiErrors = [];

        if ($scope.instanceSelected()) {
            $scope.proPricerDataIsLoading = true;

            $http({
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                withCredentials: true,
                url: $scope.model.SendToProPricerData.ProPricerUrl + 'proposals/' + $scope.model.SendToProPricerData.SelectedInstance
            }).then(function (response) {

                $scope.model.SendToProPricerData.Proposals = response.data;
                $scope.matchAll($scope.model.SendToProPricerData.Proposals);
                $scope.proPricerDataIsLoading = false;

                if ($scope.model.SendToProPricerData.SelectedProposalId !== '') {
                    if ($scope.rendered) {
                        // modal has been rendered, but we were still retrieving data so need to expand node here
                        expandNode($scope.model.SendToProPricerData.SelectedProposalId, $scope.model.SendToProPricerData.Proposals);
                    }
                }
            }, function (response) {
                // error handler
                var errors = [];
                var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
                errors.push(error);

                $scope.apiErrors = errors;
            });
        }
    };

    function expandNode(proposalId, items) {
        var found = false;

        if (items !== undefined) {
            items.some(function (item) {
                if (item.Id == proposalId) {
                    found = true;
                }

                if (!item.IsP && item.Ce != undefined && item.Ce != null && item.Ce.length > 0) {
                    var isParent = expandNode(proposalId, item.Ce);
                    if (isParent) {
                        item.expanded = true;
                        if (item.Children === undefined) {
                            // copy the children over so UI shows them (lazy load)
                            item.Children = item.Ce;
                        }

                        found = true;
                    }
                }

                return found;
            });
        }

        return found;
    }

    $scope.selectProposal = function (proposalId, proposalName) {
        $scope.model.SendToProPricerData.SelectedProposalId = proposalId;
        $scope.model.SendToProPricerData.SelectedProposalName = proposalName;

        $http({
            method: 'POST',
            url: GenSession.CreatePostURL($scope.model.SendToProPricerData.Workspace, $scope.model.SendToProPricerData.Controller, $scope.model.SendToProPricerData.LastProposalAction),
            data: { proposalId: proposalId }
        }).then(function (response) {
        });
    };

    $scope.getProposalName = function () {
        if ($scope.model.SendToProPricerData.SelectedProposalName === undefined) {
            return '';
        }

        return $scope.model.SendToProPricerData.SelectedProposalName;
    };

    /*
     * *********** NOTE ************
     * Wizard Steps
     * *****************************
     */

    $scope.previousStep = function () {
        $scope.apiErrors = [];
        $scope.showPreview = false;
        $scope.taskHeaders = [];
        $scope.resourceHeaders = [];
        $scope.formatTaskHeaders = [];
        $scope.formatResourceHeaders = [];
        $scope.previewTaskRows = [];
        $scope.previewResourceRows = [];
        $scope.model.SendToProPricerData.step = $scope.model.SendToProPricerData.step - 1;

        if ($scope.model.SendToProPricerData.step == 1) {
            // reset the selections
            $scope.resetSelections();
        }
    };

    $scope.sendToPropricer = function () {
        $scope.apiErrors = [];
        $scope.showTaskResultsError = false;
        $scope.showResourceResultsError = false;
        $scope.taskExportResults = [];
        $scope.resourceExportResults = [];
        $scope.model.SendToProPricerData.success = false;
        $scope.model.SendToProPricerData.showButtonLoader = true;
        $scope.model.SendToProPricerData.sendMessage = '';
        $scope.model.SendToProPricerData.step = 3;

        // retrieve the task and resource files as strings
        $http({
            method: 'POST',
            url: GenSession.CreatePostURL($scope.model.SendToProPricerData.Workspace, $scope.model.SendToProPricerData.Controller, $scope.model.SendToProPricerData.ExportAction),
            data: { formatId: $scope.model.SendToProPricerData.formatId, scope: $scope.model.SendToProPricerData.scope }
        }).then(function (response) {
            if ((response.data.tasks === undefined || response.data.tasks === null || response.data.tasks === '') && (response.data.resources === undefined || response.data.resources === null || response.data.resources === '')) {
                $scope.model.SendToProPricerData.sendMessage = 'There was no Export Data retrieved from genBOE to send.';
                $scope.model.SendToProPricerData.showButtonLoader = false;
            } else {
                $scope.sendToPropricerService(response.data.tasks, response.data.resources);
            }
        },
            function (error) {
                if (error && error.data && error.data.MessageList) {
                    $scope.apiErrors = error.data.MessageList;
                } else {
                    $scope.model.SendToProPricerData.sendMessage = 'Error retrieving Export Data from genBOE.';
                }
                $scope.model.SendToProPricerData.showButtonLoader = false;
        });
    };

    $scope.sendToPropricerService = function (tasks, resources) {
        $scope.taskExportResults = [];
        $scope.resourceExportResults = [];
        var tskExportOption;
        var resExportOption;

        if ($scope.model.SendToProPricerData.taskReplaceAll) {
            tskExportOption = 4;
        } else if ($scope.model.SendToProPricerData.taskDuplicates == 'file') {
            tskExportOption = 2;
        } else {
            tskExportOption = 1;
        }

        if ($scope.model.SendToProPricerData.resourceReplaceAll) {
            resExportOption = 4;
        } else if ($scope.model.SendToProPricerData.resourceDuplicates == 'file') {
            resExportOption = 2;
        } else if ($scope.model.resourceDuplicates == 'add') {
            resExportOption = 3;
        } else {
            resExportOption = 1;
        }

        $http({
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            withCredentials: true,
            data: {
                taskData: tasks,
                resourceData: resources,
                taskExportOption: tskExportOption,
                resourceExportOption: resExportOption
            },
            url: $scope.model.SendToProPricerData.ProPricerUrl + 'ProPricerDirectImport/' + $scope.model.SendToProPricerData.SelectedInstance + '/' + $scope.model.SendToProPricerData.SelectedProposalId
        }).then(function (response) {
            var results = response.data;
            if (results.ExportExceptionMessage !== null && results.ExportExceptionMessage !== undefined && results.ExportExceptionMessage !== '') {
                // there was an exception during the import
                $scope.model.SendToProPricerData.sendMessage = results.ExportExceptionMessage;
            } else {
                $scope.taskExportResults = results.TaskExportResults;
                $scope.resourceExportResults = results.ResourceExportResults;
                $scope.showTaskResultsError = $scope.taskExportResults.TaskIdErrorMessages !== undefined && $scope.taskExportResults.TaskIdErrorMessages !== null && Object.keys($scope.taskExportResults.TaskIdErrorMessages).length > 0;
                $scope.showResourceResultsError = $scope.resourceExportResults.TaskIdErrorMessages !== undefined && $scope.resourceExportResults.TaskIdErrorMessages !== null && Object.keys($scope.resourceExportResults.TaskIdErrorMessages).length > 0;

                $scope.model.SendToProPricerData.success = true;
                RaiseNotification('Export to ProPricer Successful');
            }

            $scope.model.SendToProPricerData.showButtonLoader = false;
        }, function (response) {
            $scope.model.SendToProPricerData.sendMessage = "Error communicating with ProPricer";
            $scope.model.SendToProPricerData.showButtonLoader = false;
        });
    }

    $scope.resetSelections = function () {
        $scope.model.SendToProPricerData.taskReplaceAll = false;
        $scope.model.SendToProPricerData.taskDuplicates = null;
        $scope.model.SendToProPricerData.resourceReplaceAll = false;
        $scope.model.SendToProPricerData.resourceDuplicates = null;
    }

    $scope.cancel = function () {
        // reset the selections
        $scope.resetSelections();
        $scope.apiErrors = [];
        $scope.closeDialog();
    };

    $scope.goToStep2 = function () {
        if (!$scope.nextButtonDisabled()) {
            $scope.model.SendToProPricerData.showButtonLoader = true;
            $scope.taskHeaders = [];
            $scope.resourceHeaders = [];
            $scope.formatTaskHeaders = [];
            $scope.formatResourceHeaders = [];
            $scope.previewTaskRows = [];
            $scope.previewResourceRows = [];
            $scope.model.SendToProPricerData.step = 2;
            $scope.apiErrors = [];

            $scope.model.SendToProPricerData.showButtonLoader = false;
        }
    };

    $scope.backButtonHidden = function () {
        return $scope.model.SendToProPricerData.step == 1 || ($scope.model.SendToProPricerData.step == 3 && !$scope.showTaskResultsError && !$scope.showResourceResultsError);
    };

    $scope.nextButtonDisabled = function () {
        return $scope.model.SendToProPricerData.SelectedProposalId === null || $scope.model.SendToProPricerData.SelectedProposalId === undefined || $scope.model.SendToProPricerData.SelectedProposalId === '' || $scope.proPricerDataIsLoading;
    };

    $scope.sendButtonDisabled = function () {
        // Check that radio buttons are selected or their respective checkbox is selected
        return (($scope.model.SendToProPricerData.resourceDuplicates == null && $scope.model.SendToProPricerData.resourceReplaceAll == false) ||
            ($scope.model.SendToProPricerData.taskDuplicates == null && $scope.model.SendToProPricerData.taskReplaceAll == false));
    };

    $scope.preview = function () {
        $scope.previewIsLoading = true;
        $scope.showPreview = true;
        $scope.apiErrors = [];
        $http({
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            withCredentials: true,
            url: $scope.model.SendToProPricerData.ProPricerUrl + 'SummaryFieldDefinitions/' + $scope.model.SendToProPricerData.SelectedInstance + '/' + $scope.model.SendToProPricerData.SelectedProposalId
        }).then(function (response) {
            if (Array.isArray(response.data)) {
                var taskHeaders = [];
                taskHeaders.push('ProPricer Header');
                taskHeaders.push('Row Number');
                taskHeaders.push('TASK ID');
                taskHeaders.push('Description');
                taskHeaders.push('Start Date');
                taskHeaders.push('End Date');
                taskHeaders.push('Indirect Pool');
                taskHeaders.push('Quantity');
                taskHeaders.push('Profit / Fee');

                response.data.forEach(function (item) {
                    taskHeaders.push(item.Name);
                });

                $scope.taskHeaders = taskHeaders;

                $scope.loadDates();
            } else {
                var errors = [];
                var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
                errors.push(error);

                $scope.apiErrors = errors;
            }
        }, function (response) {
            var errors = [];
            var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
            errors.push(error);

            $scope.apiErrors = errors;
        });
    };

    $scope.loadDates = function () {
        $scope.apiErrors = [];
        $http({
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            withCredentials: true,
            url: $scope.model.SendToProPricerData.ProPricerUrl + 'proposals/' + $scope.model.SendToProPricerData.SelectedInstance + '/' + $scope.model.SendToProPricerData.SelectedProposalId
        }).then(function (response) {
            var numMonths = response.data.NumberMonths;
            if (Number.isInteger(numMonths)) {
                $scope.loadResourceHeaders(numMonths);
            } else {
                var errors = [];
                var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
                errors.push(error);

                $scope.apiErrors = errors;
            }
        }, function (response) {
            var errors = [];
            var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
            errors.push(error);

            $scope.apiErrors = errors;
        });
    };

    $scope.loadResourceHeaders = function (numMonths) {
        $scope.apiErrors = [];

        // Load Resource Headers
        $http({
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            withCredentials: true,
            url: $scope.model.SendToProPricerData.ProPricerUrl + 'ResourceFieldDefinitions/' + $scope.model.SendToProPricerData.SelectedInstance + '/' + $scope.model.SendToProPricerData.SelectedProposalId
        }).then(function (response) {
            if (Array.isArray(response.data)) {
                var resourceHeaders = [];
                resourceHeaders.push('ProPricer Header');
                resourceHeaders.push('Row Number');
                resourceHeaders.push('TASK ID');
                resourceHeaders.push('Resource');
                response.data.forEach(function (item) {
                    resourceHeaders.push(item.Name);
                });

                resourceHeaders.push('Indirect Pool');
                resourceHeaders.push('Profit/Fee');
                resourceHeaders.push('Rate Band');
                resourceHeaders.push('Calendar');
                resourceHeaders.push('Hours / Heads');
                resourceHeaders.push('Curve');
                resourceHeaders.push('Spread Amount / Discrete Start Date');
                resourceHeaders.push('Spread Start Date / Value 1');
                if (numMonths == 1) {
                    resourceHeaders.push('Spread End Date');
                } else {
                    resourceHeaders.push('Spread End Date / Value 2');
                    for (var i = 3; i <= numMonths; i++) {
                        resourceHeaders.push('Value ' + i.toString());
                    }
                }

                $scope.resourceHeaders = resourceHeaders;
                $scope.loadPreview();
            } else {
                var errors = [];
                var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
                errors.push(error);

                $scope.apiErrors = errors;
            }
        }, function (response) {
            var errors = [];
            var error = { ValidationIssue: 'Could not connect to Pro Pricer Service.' };
            errors.push(error);

            $scope.apiErrors = errors;
        });
    }

    $scope.loadPreview = function () {
        $scope.apiErrors = [];

        $http({
            method: 'POST',
            url: GenSession.CreatePostURL($scope.model.SendToProPricerData.Workspace, $scope.model.SendToProPricerData.Controller, $scope.model.SendToProPricerData.PreviewAction),
            data: { formatId: $scope.model.SendToProPricerData.formatId, scope: $scope.model.SendToProPricerData.scope }
        }).then(function (response) {
            $scope.formatTaskHeaders = response.data.TaskHeaders;
            $scope.formatResourceHeaders = response.data.ResourceHeaders;
            $scope.previewTaskRows = response.data.TaskRows;
            $scope.previewResourceRows = response.data.ResourceRows;
            $scope.previewIsLoading = false;
        }, function (response) {
            var errors = [];
            var error = { ValidationIssue: 'Error retrieving Preview Data from genBOE.' };
            errors.push(error);

            $scope.apiErrors = errors;
        });
    };

    $scope.taskReplaceAllChanged = function () {
        if ($scope.model.SendToProPricerData.taskReplaceAll == true) {
            $scope.model.SendToProPricerData.taskDuplicates = null;
        }
    }

    $scope.resourceReplaceAllChanged = function () {
        if ($scope.model.SendToProPricerData.resourceReplaceAll == true) {
            $scope.model.SendToProPricerData.resourceDuplicates = null;
        }
    }

    $scope.initialize();
}]);