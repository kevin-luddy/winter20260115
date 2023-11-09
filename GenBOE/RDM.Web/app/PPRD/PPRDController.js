angular.module('RDM').controller('PPRDController', ['$scope', '$rootScope', '$http', '$window', '$uibModal', '$sce', 'lockService', 'utilityService', 'PPRDModel',
    function ($scope, $rootScope, $http, $window, $uibModal, $sce, lockService, utilityService, PPRDModel) {

        $scope.isInitializing = true;
        $scope.isDataLoading = true;
        $scope.rates = [];
        $rootScope.errors = [];
        $scope.showEditLockTimeoutExpiration = false;
        $scope.isGeneratingPPRD = false;
        $scope.timeoutTime = 4000;
        $scope.PageIsDirty = false;
        $scope.canEdit = function () { return $scope.pprd && $scope.pprd.LockInfo && !$scope.pprd.LockInfo.IsReadOnly; };
        lockService.id = PPRDModel.Area;
        $scope.newIndex = -2;

        /* define the ui-tree events in a treeOptions collection */
        $scope.treeOptions = {
            accept: function (sourceNodeScope, destNodesScope, destIndex) {
                var sourceNode = sourceNodeScope.$nodeScope ? sourceNodeScope.$modelValue : null;
                var destNode = destNodesScope.$nodeScope ? destNodesScope.$nodeScope.$modelValue : null;
                if (destNode) {
                    if ($scope.IsSectionContent(destNode)) {
                        return false; // do not allow drop onto content nodes
                    }
                    if (sourceNode &&
                        sourceNode.ParentId !== destNode.Id &&
                        $scope.IsSectionTableContent(sourceNode) &&
                        $scope.countTablesInSection(destNode) > 0) {
                        return false; // do not allow drop of table onto another section that already has a table
                    }
                } else {
                    if (sourceNode && $scope.IsSectionContent(sourceNode)) {
                        return false; // do not allow drop of content onto document (top) level
                    }
                }
                return true;
            },

            dropped: function (e) {
                $scope.PageIsDirty = true;
                $scope.reorderSections($scope.pprd
                    .ChildNodes); // renumber the sections and content any time a node is drag/dropped
                $scope.refreshLock();
                return true;
            }
        };

        /* Helper method - returns true if node is a content node (i.e. text, or table); false if node is a section node. */
        $scope.IsSectionContent = function (node) {
            if (node.ContentType === PPRDModel.SectionContentTypeSection) {
                return false;
            } else {
                return true;
            }
        };

        /* Helper method - returns true if node is a Rate Table; false otherwise. */
        $scope.IsSectionTableContent = function (node) {
            if (node.ContentType === PPRDModel.SectionContentTypeRateTable) {
                return true;
            } else {
                return false;
            }
        };

        /* Helper method - returns true if node is address; false otherwise. */
        $scope.IsSectionAddressContent = function (node) {
            if (node.ContentType === PPRDModel.SectionContentTypeAddress) {
                return true;
            } else {
                return false;
            }
        };

        /* Helper method to count the table nodes in the section */
        $scope.countTablesInSection = function (targetNode) {
            var numTablesInSection = 0;
            if (!$scope.IsSectionContent(targetNode)) { // make sure this is a section node
                for (var i = 0; i < targetNode.ChildNodes.length; i++) {
                    var node = targetNode.ChildNodes[i];
                    if ($scope.IsSectionTableContent(node)) {
                        numTablesInSection++;
                    }
                }
            }
            return numTablesInSection;
        };

        /* Helper method to count the address content in the section */
        $scope.countAddressInSection = function (targetNode) {
            var numAddressInSection = 0;
            if (!$scope.IsSectionContent(targetNode)) { // make sure this is a section node
                for (var i = 0; i < targetNode.ChildNodes.length; i++) {
                    var node = targetNode.ChildNodes[i];
                    if ($scope.IsSectionAddressContent(node)) {
                        numAddressInSection++;
                    }
                }
            }
            return numAddressInSection;
        };

        /* sanitize html */
        $scope.trustedHtml = function (html) {
            return $sce.trustAsHtml(html);
        };

        /* toggle ui tree expand/collapse node */
        $scope.toggle = function (scope) {
            scope.toggle();
        };

        $scope.lock = function () {
            lockService.lock().then(
                function (response) {
                    $scope.pprd.LockInfo = response.data.LockInfo;
                    lockService.processResponse(response,
                        $scope.pprd.LockInfo.EditLockTimeoutWarningMinutes,
                        $scope.refreshLock,
                        $scope.pprd.LockInfo.EditLockTimeoutExpirationMinutes,
                        $scope.editLockTimeoutExpirationCallback,
                        $scope.clearLockTimers,
                        $scope.loadData,
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
                    $scope.pprd.LockInfo = response.data.LockInfo;
                    lockService.processResponse(response,
                        $scope.pprd.LockInfo.EditLockTimeoutWarningMinutes,
                        $scope.refreshLock,
                        $scope.pprd.LockInfo.EditLockTimeoutExpirationMinutes,
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
                        $scope.pprd.LockInfo = response.data.LockInfo;
                        lockService.processResponseRelease(response, $scope.clearLockTimers);
                    },
                    function () {
                        lockService.requestRejected();
                    });
            }
        };

        $scope.collapseAll = function () {
            $scope.$broadcast('angular-ui-tree:collapse-all');
        };

        $scope.expandAll = function () {
            $scope.$broadcast('angular-ui-tree:expand-all');
        };

        $scope.generatePPRDClick = function () {
            $scope.isGeneratingPPRD = true;
            setTimeout(function () { $scope.timeoutFuncPPRD(); }, $scope.timeoutTime);
            DownloadFile('GeneratePPRD', PPRDModel.reportsController, PPRDModel.generateFullPPRDAction, 'WIP');
        };

        $scope.timeoutFuncPPRD = function () { $scope.$apply(function () { $scope.isGeneratingPPRD = false; }); }

        $scope.showConfirmDelete = function (scope, $event) {
            // Clear error messages.
            $rootScope.errors = []; 
            var node = scope.$modelValue;
            var nodeType = $scope.IsSectionContent(node) ? 'Content' : 'Section';
            var confirmationMessage = 'Are you sure you want to remove ' +
                ($scope.IsSectionContent(node) ? 'this content?' : ('section "' + node.Title + (node.ChildNodes && node.ChildNodes.length > 0 ? '" and all its children?' : '"?')));
            if (!$scope.IsSectionContent(node)) {
                var rateCodes = [];
                var sectionIds = [];
                $scope.getSectionIds(node, sectionIds);    // get list of section (and sub-section) Ids for the section being deleted
                // send ids to backend to see if there are any File Attachments
                var validateDeleteUrl = createPostURL(PPRDModel.controller, PPRDModel.validateDeleteAction);
                $http({
                    method: 'POST',
                    url: validateDeleteUrl,
                    data: JSON.stringify(sectionIds)
                }).then(function successCallback(response) {
                    // Get list of associated rate codes (if any) 
                    for (var i = 0; i < $scope.rates.length; i++) {
                        var rate = $scope.rates[i];
                        if (sectionIds.indexOf(rate.Section) >= 0) {
                            rateCodes.push(rate.RateCode);
                        }
                    }
                    if (rateCodes.length > 0) {
                        confirmationMessage = confirmationMessage + '<br />Note: The following rate codes are associated with this section:<br /><strong>' + rateCodes.join(', ') + '</strong>';
                    }
                    
                    confirmDeletionDialog(scope, $event, confirmationMessage, nodeType)
                }, function errorCallback(response) {
                    $scope.refreshLock(); // Refresh lock to give user time to fix errors
                    $scope.refreshScrollableElementSizeWithDelay();
                });
            } else {
                confirmDeletionDialog(scope, $event, confirmationMessage, nodeType)
            }
        };

        confirmDeletionDialog = function (scope, $event, confirmationMessage, nodeType) {
            ConfirmDialog("Delete " + nodeType, confirmationMessage, function () {
                $scope.PageIsDirty = true;
                scope.remove();
                $scope.reorderSections($scope.pprd.ChildNodes);  // renumber the sections and content
                $scope.refreshLock();
            });
        };

        /* Recursively get section Ids for this section and all sub-sections. */
        $scope.getSectionIds = function (section, sectionIds) {
            sectionIds.push(section.Id);
            if (section.ChildNodes && section.ChildNodes.length > 0) {
                for (var i = 0; i < section.ChildNodes.length; i++) {
                    if (!$scope.IsSectionContent(section.ChildNodes[i])) {  // only process sections (not content)
                        $scope.getSectionIds(section.ChildNodes[i], sectionIds);
                    }
                }
            }
        };

        /* Adds the first section (to an empty tree) using the modal dialog */
        $scope.addFirstSection = function (scope) {
            var section = $scope.createNewSection(-1, 0, "1", "", false);
            $scope.openEditSectionModal($scope.pprd, section);
        };

        /* Adds a new section using the modal dialog */
        $scope.addSection = function (scope) {
            var targetNode = scope.$parentNodeScope ? scope.$parentNodeScope.$modelValue : $scope.pprd;

            // create a new section object
            var section = $scope.createNewSection(-1, targetNode.ChildNodes.length, targetNode.ReferenceNumber + '.' + (targetNode.ChildNodes.length + 1), '', targetNode.IsInternalSection);
            $scope.openEditSectionModal(targetNode, section);
        };

        /* Adds a new sub-section using the modal dialog */
        $scope.addSubSection = function (scope) {
            var targetNode = scope.$modelValue;
            if ($scope.IsSectionContent(targetNode)) {
                // Should never get here, but keep it as a fail-safe.
                targetNode = scope.$parentNodeScope.$modelValue;
            }

            // create a new section object
            var section = $scope.createNewSection(-1, targetNode.ChildNodes.length, targetNode.ReferenceNumber + '.' + (targetNode.ChildNodes.length + 1), '', targetNode.IsInternalSection);
            $scope.openEditSectionModal(targetNode, section);
        };

        /* Helper method for creating a section object */
        $scope.createNewSection = function (id, displayOrder, referenceNumber, title, isInternalSection) {
            var section = {
                "Id": id,
                "DisplayOrder": displayOrder,
                "ReferenceNumber": referenceNumber,
                "Title": title,
                "IsInternalSection": isInternalSection,
                "ContentType": PPRDModel.SectionContentTypeSection,
                "ChildNodes": [],
                "TextContent": ""
            };
            return section;
        };

        /* Edits the selected section, normally this would be called editSection but there were collisions inside angular */
        $scope.modifySection = function (scope) {
            // first clone the section, so that the edit dialog can cancel changes
            var clonedSection = angular.copy(scope.$modelValue);
            // get a handle to the parent node -- be careful in case scope points to a top-level node
            var parentNode = scope.$parentNodeScope ? scope.$parentNodeScope.$modelValue : $scope.pprd;
            $scope.openEditSectionModal(parentNode, clonedSection);
        };

        $scope.openHelpModal = function () {
            // open the Help Modal
            var modalInstance = $uibModal.open({
                templateUrl: PPRDModel.baseUrl + 'app/PPRD/Help.html',
                windowTopClass: 'bootstrap help-modal',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap'
            });
            
            modalInstance.rendered.then(function () {
                utilityService.makeModalDraggableAndResizable('#helpModal', 460, 640);
            });
        };

        $scope.openEditSectionModal = function (parentNode, section) {
            // open the Edit Modal
            var modalInstance = $uibModal.open({
                templateUrl: PPRDModel.baseUrl + 'app/PPRD/EditSection.html',
                controller: 'EditSectionController',
                windowTopClass: 'bootstrap edit-content-modal',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap',
                resolve: { model: [function () { return section; }] }
            });
            
            modalInstance.rendered.then(function () {
                utilityService.makeModalDraggableAndResizable('#editSectionModal', 160, 700);
            });

            modalInstance.result.then(function (section) {
                if (section !== undefined) {
                    // section was passed back inside OK button, now update the real section from the clone
                    $scope.PageIsDirty = true;
                    if (section.Id === -1) {
                        // this is a new section, add it to the end of the array
                        // Also, update the ID to signify that it has already been added to the table so that it is not added again if edited before the section is saved
                        section.Id = $scope.newIndex--; 
                        parentNode.ChildNodes.push(section);
                    } else {
                        // this is a cloned section, replace original with cloned
                        for (var i = 0; i < parentNode.ChildNodes.length; i++) {
                            if (parentNode.ChildNodes[i].Id === section.Id) {
                                parentNode.ChildNodes[i] = section;
                                break;
                            }
                        }
                    }

                    $scope.reorderSections($scope.pprd.ChildNodes);  // renumber the sections and content
                    $scope.refreshLock();
                }
            }, function () { });
        };

        /* Adds a new content using the modal dialog */
        $scope.addContent = function (scope) {
            var targetNode = scope.$modelValue;
            if ($scope.IsSectionContent(targetNode)) {
                targetNode = scope.$parentNodeScope.$modelValue;
            }

            // first create a new Content
            var content = {
                "Id": -1,
                "Order": targetNode.ChildNodes.length,
                "ContentType": PPRDModel.SectionContentTypeText,
                "TextContent": "",
                "IsInternalSection": false,
                "DisplayRateCode": false,
                "Office": "",
                "Agency": "",
                "LMBA": "",
                "Name": "",
                "Street": "",
                "CityST": "",
                "Phone": "",
                "Email": "",
                "Other": "",
                "IncludeInCoversheet": false,
                "NumTablesInSection": $scope.countTablesInSection(targetNode),
                "NumAddressInSection": $scope.countAddressInSection(targetNode)
            };
            $scope.openEditContentModal(targetNode, content, true);
        };

        /* Edits the selected content, normally this would be called editContent but there were collisions inside angular */
        $scope.modifyContent = function (scope) {
            // first clone the content, so that the edit dialog can cancel changes
            var clonedContent = angular.copy(scope.$modelValue);
            $scope.openEditContentModal(scope.$parentNodeScope.$modelValue, clonedContent, false);
        };

        $scope.openEditContentModal = function (parentNode, content, newNode) {
            // open the Edit Modal
            var modalInstance = $uibModal.open({
                templateUrl: PPRDModel.baseUrl + 'app/PPRD/EditContent.html',
                controller: 'EditContentController',
                windowTopClass: 'bootstrap edit-content-modal',
                windowClass: 'bootstrap',
                backdrop: 'static',
                backdropClass: 'bootstrap',
                resolve: { model: [function () { return content; }], PPRDModel: [function () { return PPRDModel; }], newNode: [function () { return newNode; }] }
            });
            
            modalInstance.rendered.then(function () {
                utilityService.makeModalDraggableAndResizable('#editContentModal', 400, 600);
            });

            modalInstance.result.then(function (content) {
                if (content !== undefined) {
                    // content was passed back inside OK button, now update the real content from the clone
                    $scope.PageIsDirty = true;
                    if (content.Id === -1) {
                        // this is a new content, add it to the end of the array
                        // Also, update the ID to signify that it has already been added to the table so that it is not added again if edited before the section is saved
                        content.Id = $scope.newIndex--;
                        parentNode.ChildNodes.push(content);
                    } else {
                        // this is a cloned content, replace original with cloned
                        for (var i = 0; i < parentNode.ChildNodes.length; i++) {
                            if (parentNode.ChildNodes[i].Id === content.Id) {
                                parentNode.ChildNodes[i] = content;
                                break;
                            }
                        }
                    }

                    $scope.reorderSections($scope.pprd.ChildNodes);  // renumber the sections and content
                    $scope.refreshLock();
                }
            }, function () { });
        };

        /* Recursively reset the display order of the tree sections and content. */
        $scope.reorderSections = function (sections, parent) {
            if (sections && sections.length) {
                // Need to order content entries before sibling sections. 
                // To do this, we need to loop through the entries twice.
                var i;
                var section;
                var displayOrder = 0;

                // Process content nodes
                for (i = 0; i < sections.length; i++) {
                    section = sections[i];
                    if ($scope.IsSectionContent(section)) {
                        section.DisplayOrder = displayOrder++;
                        section.ReferenceNumber = '';
                    }
                }

                // Process section nodes
                var sectionNumber = 0;
                for (i = 0; i < sections.length; i++) {
                    section = sections[i];
                    if (!$scope.IsSectionContent(section)) {
                        section.DisplayOrder = displayOrder++;
                        sectionNumber++;
                        section.ReferenceNumber = parent ? parent.ReferenceNumber + '.' + sectionNumber : sectionNumber + '';
                    }
                    $scope.reorderSections(section.ChildNodes, section);  // recursively reorder sub-nodes
                }

                // Now, re-order all the nodes by display order 
                sections.sort(function (a, b) {
                    return a.DisplayOrder - b.DisplayOrder;
                });
            }
        };

        /* Save the document */
        $scope.savePPRD = function (releaseLock) {
            // clear error message
            $scope.isDataValid = true;
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear lock timers so there's no pop-up in the middle of a save
            $scope.clearLockTimers();

            var saveUrl = createPostURL(PPRDModel.controller, PPRDModel.saveAction);
            return $http({
                method: 'POST',
                url: saveUrl,
                data: JSON.stringify($scope.pprd)
            }).then(function successCallback(response) {
                $scope.loadData(releaseLock);
                trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                $scope.refreshLock(); // Refresh lock to give user time to fix errors
                $(document).trigger("HIDE_LOADING_BOX");
            });
        };

        $scope.initialize = function () {
            $scope.isInitializing = false;
            $scope.loadData();
        };

        $scope.loadPPRD = function () {
            return $http({
                method: 'POST',
                url: createPostURL(PPRDModel.controller, PPRDModel.getAction)
            });
        };

        $scope.loadRates = function (revisionId) {
            return $http({
                method: 'POST',
                data: { id: revisionId },
                url: createPostURL(PPRDModel.rateController, PPRDModel.getRatesAction)
            });
        };

        /* Loads the Data - releaseLock optional to support Save & Continue */
        $scope.loadData = function (releaseLock) {
            if (!$scope.isInitializing) {
                $(document).trigger("SHOW_LOADING_BOX");

                // If Save & Continue make a copy of the lockInfo.
                var tempLockInfo = null;
                if (!releaseLock && $scope.pprd !== undefined && $scope.pprd.LockInfo !== undefined) {
                    tempLockInfo = $scope.pprd.LockInfo;
                }

                // Clear error messages.
                $rootScope.errors = [];
                $scope.isDataLoading = true;
                $scope.pprd = null;
                $scope.rates = [];
                $scope.PageIsDirty = false;

                // Load PPR&D and rates data by chaining the calls together
                $scope.loadPPRD().then(function successCallback(response) {
                    $scope.pprd = response.data;

                    // Save & Continue - restore lock.
                    if (!releaseLock && tempLockInfo !== null) {
                        $scope.pprd.LockInfo = tempLockInfo;
                    }

                    // Load Rates data using Revision ID from PPR&D
                    $scope.loadRates($scope.pprd.Revision.Id).then(function successCallback(response) {
                        $scope.rates = response.data;
                        $scope.isDataLoading = false;
                        if (releaseLock) {
                            $scope.releaseLock();
                        } else if (releaseLock === false) {
                            // Only do this if releaseLock specifically set to false so we don't 
                            // unnecessarily refresh the lock, such as when calling $scope.lock()
                            $scope.refreshLock();
                        }
                        $(document).trigger("HIDE_LOADING_BOX");
                        $scope.refreshScrollableElementSizeWithDelay();
                    }, function errorCallback(response) {
                        $(document).trigger("HIDE_LOADING_BOX");
                        $scope.refreshScrollableElementSizeWithDelay();
                    });
                }, function errorCallback(response) {
                    $(document).trigger("HIDE_LOADING_BOX");
                    $scope.refreshScrollableElementSizeWithDelay();
                });
            }
        };

        $scope.confirmCancel = function () {
            if ($scope.PageIsDirty) {
                ConfirmDialog("Confirm Cancel", "Are you sure you want to cancel all changes?", function () {
                    // Yes
                    $scope.loadData(true);
                },
                function () {
                    // No
                    return false;
                });
            } else {
                // If no changes, continue with loadData and releaseLock(true).
                $scope.loadData(true);
            }
        };

        window.onbeforeunload = function () {
            // Confirm leaving/refreshing page if the page is dirty
            if ($scope.PageIsDirty) {
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

        $scope.initialize();
    }]);