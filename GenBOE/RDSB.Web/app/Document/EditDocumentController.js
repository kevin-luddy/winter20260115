// The controller for the Editing of a Document
angular.module('rdsb').controller('editDocumentController', ['$scope', '$http', '$window', 'DocumentDetailModelView', '$q', '$rootScope', function ($scope, $http, $window, DocumentDetailModelView, $q, $rootScope) {
    $scope.model = DocumentDetailModelView.data;
    $scope.isDataLoading = true;
    $scope.start = new Date();
    $scope.end = new Date();
    $scope.allSections = [];
    $scope.flatSections = [];
    $scope.allRates = [];
    $scope.searchTerm = '';
    $scope.pastedText = '';
    $scope.filteredRates = [];
    $scope.selectedRates = [];
    $rootScope.errors = [];
    $scope.timeoutTime = 3000;
    $scope.isSaving = false;

    $scope.popupDatePickerStart = {
        opened: false
    };

    $scope.popupDatePickerEnd = {
        opened: false
    };

    $scope.saveDocument = function () {
        $scope.isSaving = true;
        $rootScope.errors = [];
        
        $scope.model.StartYear = $scope.start.getFullYear();
        $scope.model.EndYear = $scope.end.getFullYear();

        // set the selected sections
        $scope.model.SelectedSectionIds = [];
        $scope.flatSections.forEach(function (item) {
            if (item.IsSelected) {
                $scope.model.SelectedSectionIds.push(item.Id);
            }
        });

        // set the selcted rate codes
        $scope.model.SelectedRateCodeIds = [];
        $scope.allRates.forEach(function (item) {
            if (item.IsSelected) {
                $scope.model.SelectedRateCodeIds.push(item.Id);
            }
        });

        var url = createPostURL(DocumentDetailModelView.controller, DocumentDetailModelView.saveDocumentAction, $scope.model.ProposalId);

        $http({
            method: "POST",
            url: url,
            data: $scope.model
        }).then(function successCallback(response) {
            $(document).trigger("DISPLAY_NOTIFICATION", 'Document changes saved');
            $scope.documentForm.$setPristine();
            $window.location.reload(true);
        }, function errorCallback(response){
            $scope.isSaving = false;
        });
    };

    $scope.cancel = function () {
        $window.location.href = '/';
    };

    $scope.initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };

    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = true;
            if ($scope.documentForm) {
                $scope.documentForm.$setPristine();
            }

            // Load data.  
            if ($scope.model.StartYear > 0) {
                $scope.start = new Date($scope.model.StartYear, 1, 1, 1, 1, 1, 1);
            }

            if ($scope.model.EndYear > 0) {
                $scope.end = new Date($scope.model.EndYear, 1, 1, 1, 1, 1, 1);
            }

            $scope.model.AvailableRevisions.forEach(function (revision) {
                revision.Label = 'Rev. ' + revision.Revision + ' Published ' + revision.RevisionPublishedInfo;
            });

            var promise = $scope.getSections();

            promise.then(
              function (answer) {
                  $scope.isDataLoading = false;
                  $(document).trigger("HIDE_LOADING_BOX");
                  if (!$scope.model.IsUsingLatest) {
                      AlertDialog("Old PPR&D Revision", "This document is using an older PPR&D Revision.");
                  }
              },
              function (error) {
                  $scope.isDataLoading = false;
                  $(document).trigger("HIDE_LOADING_BOX");
                  if (!$scope.model.IsUsingLatest) {
                      AlertDialog("Old PPR&D Revision", "This document is using an older PPR&D Revision.");
                  }
              });
        }
    };

    $scope.getSections = function () {
        var deferred = $q.defer();
        $scope.allSections = [];
        $scope.flatSections = [];
        if ($scope.model.SelectedRevisionId) {
            var getSectionsUrl = createPostURL(DocumentDetailModelView.controller, DocumentDetailModelView.getSectionsForRevisionAction);
            $http({
                method: 'POST',
                url: getSectionsUrl,
                data: { revisionID: $scope.model.SelectedRevisionId }
            }).then(function successCallback(response) {
                $scope.allSections = response.data;
                $scope.flatSections = $scope.flatten($scope.allSections, 0);

                if ($scope.model.SelectedSectionIds) {
                    $scope.flatSections.forEach(function (section) {
                        section.IsSelected = $scope.model.SelectedSectionIds.includes(section.Id) || section.IsRdsbRequired;
                        $scope.checkboxUpdateForRequired(section);
                    });
                }

                var promise = $scope.getRates();
                promise.then(
                    function (answer) {
                        deferred.resolve();
                    },
              function (error) {
                  deferred.reject(error);
              });
            },
                function errorCallback(error) {
                    deferred.reject(error);
                });
        } else {
            deferred.resolve();
        }

        return deferred.promise;
    };

    $scope.flatten = function (items, level) {
        var flat = [];
        items.forEach(function (item) {
            item.Level = level;
            // HasRate indicates that there is an associated Rate Code
            item.HasRate = false;
            // HasTable indicates that the section has an associated Rate Table directly inside it
            item.Collapsed = false;
            // Locked is used to indicate that an associated Rate Code for this section has been selected
            item.Locked = false;
            item.HasChildren = false;
            flat.push(item);
            if (item.ChildNodes && item.ChildNodes.length > 0) {
                item.HasChildren = true;
                var childFlat = $scope.flatten(item.ChildNodes, level + 1);
                childFlat.forEach(function (allChildren) {
                    flat.push(allChildren);
                });

                item.ChildNodes.forEach(function (child) {
                    child.Parent = item;
                });
            } 
        });

        return flat;
    };

    $scope.getRates = function () {
        var deferred = $q.defer();
        $scope.allRates = [];

        if ($scope.model.SelectedRevisionId) {
            var getRatesUrl = createPostURL(DocumentDetailModelView.controller, DocumentDetailModelView.getRatesForRevisionAction);
            $http({
                method: 'POST',
                url: getRatesUrl,
                data: { revisionID: $scope.model.SelectedRevisionId }
            }).then(function successCallback(response) {
                $scope.allRates = response.data;
                
                if ($scope.model.SelectedRateCodeIds) {
                    $scope.allRates.forEach(function (rate) {
                        rate.IsSelected = $scope.model.SelectedRateCodeIds.includes(rate.Id);
                        var section = $.grep($scope.flatSections, function (e) { return e.Id == rate.Section; });

                        if (section.length == 0) {
                            // bad news bears, somehow we have a rate that is attached to a section Id that we don't have
                            deferred.reject("Rate found with a bad section Id of " + rate.Section);
                        } else if (section.length == 1) {
                            // found the section
                            section[0].HasRate = true;
                            if (rate.IsSelected) {
                                $scope.updateRateSelection(rate);
                            }
                        } else {
                            // more bad news, multiple sections found with the same id!!!
                            deferred.reject("Multiple sections found with the same id.");
                        }
                    });
                }

                $scope.searchRates();

                deferred.resolve();
            },
                function errorCallback(error) {
                    deferred.reject(error);
                });
        } else {
            deferred.resolve();
        }

        return deferred.promise;
    };

    $scope.updateRateSelection = function (rate) {
        $rootScope.errors = [];
        // find section
        var section = $.grep($scope.flatSections, function (e) { return e.Id == rate.Section; });

        if (rate.IsSelected) {
            section[0].IsSelected = true;
            if (section[0].Parent) {
                section[0].Parent.IsSelected = true;
                section[0].Parent.Locked = true;
                if (section[0].Parent.Parent) {
                    section[0].Parent.Parent.IsSelected = true;
                    section[0].Parent.Parent.Locked = true;
                }
            }
        } else {
            var otherRateSelected = false;
            // verify no other rates are selected for this section
            $scope.allRates.forEach(function (otherRate) {
                if (otherRate.IsSelected && otherRate.Section == section[0].Id){
                    otherRateSelected = true;
                }
            });

            if (!otherRateSelected) {
                // no other rates selected, unselect the section and parents if applicable
                section[0].IsSelected = false;

                // unlocking parents gets very tricky based on siblings 
                if (section[0].Parent) {
                    var selectedRateSiblingIndex = section[0].Parent.ChildNodes.findIndex($scope.checkRateSelected);
                    if (selectedRateSiblingIndex == -1) {
                        // no siblings selected that are rates, so unlock parent
                        section[0].Parent.Locked = false;

                        // now check for selected siblings to unselect parent
                        var selectedSiblingIndex = section[0].Parent.ChildNodes.findIndex($scope.checkSelected);
                        if (selectedSiblingIndex == -1) {
                            section[0].Parent.IsSelected = false;
                        }

                        if (section[0].Parent.Parent) {
                            // now check parent's parent for locked children
                            var selectedParentRateSiblingIndex = section[0].Parent.Parent.ChildNodes.findIndex($scope.checkRateLocked);
                            if (selectedParentRateSiblingIndex == -1) {
                                // no siblings of parent are locked, so unlock grandparent
                                section[0].Parent.Parent.Locked = false;

                                // if the parent is not selected, look at parent siblings for selected to see if grandparent becomes unselected
                                if (!section[0].Parent.IsSelected) {
                                    var selectedParentSelectedSiblingIndex = section[0].Parent.Parent.ChildNodes.findIndex($scope.checkSelected);
                                    if (selectedParentSelectedSiblingIndex == -1) {
                                        // no siblings of parent checked, unselect the grandparent
                                        section[0].Parent.Parent.IsSelected = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    $scope.checkRateLocked = function (section) {
        return section.Locked;
    }

    $scope.checkRateSelected = function (section) {
        return section.IsSelected && section.HasRate;
    }

    $scope.checkSelected = function (section) {
        return section.IsSelected;
    }

    $scope.checkboxUpdate = function(section){
        if (section.IsSelected) {
            // Make sure parent is selected
            if (section.Parent) {
                section.Parent.IsSelected = true;
                if (section.Parent.Parent) {
                    section.Parent.Parent.IsSelected = true;
                }
            }
        } else {
            // Unselect children and their children
            $scope.unselectChildren(section);
        }
    };

    $scope.checkboxUpdateForRequired = function(section) {
        if (section.IsSelected) {
            // Make sure parent is selected and required if needed
            if (section.Parent) {
                section.Parent.IsSelected = true;
                section.Parent.IsRdsbRequired = section.Parent.IsRdsbRequired || section.IsRdsbRequired;
                if (section.Parent.Parent) {
                    section.Parent.Parent.IsSelected = true;
                    section.Parent.Parent.IsRdsbRequired = section.Parent.Parent.IsRdsbRequired || section.Parent.IsRdsbRequired || section.IsRdsbRequired;
                }
            }
        }
    }

    $scope.unselectChildren = function (section) {
        if (section.ChildNodes) {
            section.ChildNodes.forEach(function (child) {
                child.IsSelected = false;
                $scope.unselectChildren(child);
            });
        }
    };

    $scope.openDatePickerStart = function () {
        $scope.popupDatePickerStart.opened = true;
    };

    $scope.openDatePickerEnd = function () {
        $scope.popupDatePickerEnd.opened = true;
    };

    $scope.dateOptions = {
        formatYear: 'yyyy',
        startingDay: 1,
        minMode: 'year'
    };

    /* toggle ui tree expand/collapse node */
    $scope.toggle = function (section) {
        section.Collapsed = !section.Collapsed;
    };

    $scope.expandAll = function () {
        $scope.flatSections.forEach(function (section) {
            section.Collapsed = false;
        });
    };

    $scope.collapseAll = function () {
        $scope.flatSections.forEach(function (section) {
            section.Collapsed = true;
        });
    };

    // Search for the rates using the search box
    $scope.searchRates = function () {
        $scope.filteredRates = [];

        if ($scope.searchTerm && $scope.searchTerm.length > 0) {
            var upperCaseSearch = $scope.searchTerm.toUpperCase();
            $scope.allRates.forEach(function (rate) {
                if (!rate.IsSelected){
                    if (rate.RateCode.indexOf(upperCaseSearch) >= 0) {
                        $scope.filteredRates.push(rate);
                    }
                }
            });
        } else {
            $scope.allRates.forEach(function (rate) {
                if (!rate.IsSelected) {
                    $scope.filteredRates.push(rate);
                }
            });
        }
    };

    // Add the selected rates 
    $scope.addRates = function () {
        if ($scope.selectedRates && $scope.selectedRates.length > 0) {
            $scope.selectedRates.forEach(function (rateCode) {
                var rate = $.grep($scope.filteredRates, function (e) { return e.RateCode == rateCode; });
                if (rate.length == 1) {
                    rate[0].IsSelected = true;
                    $scope.updateRateSelection(rate[0]);
                }
            });

            $scope.searchRates();
        }
    };

    // Add the pasted rate codes
    $scope.addPastedRates = function () {
        var invalidRates = [];
        $rootScope.errors = [];

        if ($scope.pastedText && $scope.pastedText.length > 0) {
            var rateCodes = $scope.pastedText.replace(/(?:\r\n|\r|\n|\t|,|;)/g, ' ');
            var rateArray = rateCodes.split(' ');
            rateArray.forEach(function (rateCode) {
                if (rateCode && rateCode.length > 0) {
                    rateCode = rateCode.toUpperCase().trim();

                    var searchCode = rateCode;
                    if (rateCode.length == 7 && (rateCode.endsWith("1") || rateCode.endsWith("2") || rateCode.endsWith("3") || rateCode.endsWith("4") || rateCode.endsWith("5") || rateCode.endsWith("6") || rateCode.endsWith("7"))) {
                        searchCode = rateCode.substr(0, 6);
                    }

                    var found = false;

                    $scope.allRates.some(function (rate) {
                        if (rate.RateCode === searchCode) {
                            if (!rate.IsSelected) {
                                rate.IsSelected = true;
                                $scope.updateRateSelection(rate);
                            }

                            found = true;
                            return true; // short-circuits the loop
                        }

                        return false;
                    });

                    if (!found) {
                        invalidRates.push(rateCode);
                    }
                }
            });
        }

        if (invalidRates.length > 0) {
            $rootScope.errors = [{ ValidationIssue: 'The following Rate Codes were not found: ' + invalidRates.join(', ') }];
        }

        $scope.pastedText = '';
        $scope.searchRates();
    };

    $scope.deselectRate = function (rate) {
        rate.IsSelected = false;
        $scope.updateRateSelection(rate);
        $scope.searchRates();
    }

    // Clear the selected rates
    $scope.clearRates = function () {
        $scope.allRates.forEach(function (rate) {
            if (rate.IsSelected) {
                rate.IsSelected = false;
                $scope.updateRateSelection(rate);
            }
        });
        $scope.searchRates();
    };

    $scope.revisionChanged = function() {
        $rootScope.errors = [];
        // update start/end years from revision
        var revision = $scope.model.AvailableRevisions.find(function(rev){ return rev.Id === $scope.model.SelectedRevisionId});

        var startYear = $scope.start.getFullYear();
        var endYear = $scope.end.getFullYear();
        if (startYear < revision.StartYear || startYear > revision.EndYear) {
            $scope.start = new Date(new Date().getFullYear(), 1, 1, 1, 1, 1, 1);
        }

        if (endYear < revision.StartYear || endYear > revision.EndYear) {

            if ($scope.start.getFullYear() + 5 > revision.EndYear) {
                $scope.end = new Date(revision.EndYear, 1, 1, 1, 1, 1, 1);
            } else {
                $scope.end = new Date($scope.start.getFullYear() + 5, 1, 1, 1, 1, 1, 1);
            }
        }

        // load the sections and rates
        $(document).trigger("SHOW_LOADING_BOX");

        $scope.isDataLoading = true;

        // save off the old rates
        var oldRates = $scope.allRates;

        var promise = $scope.getSections();

        promise.then(
          function (answer) {
              // try to match up the old rates with the new rates
              oldRates.forEach(function (oldRate) {
                  if (oldRate.IsSelected) {
                      var rate = $.grep($scope.filteredRates, function (e) { return e.RateCode == oldRate.RateCode; });
                      if (rate.length == 1) {
                          rate[0].IsSelected = true;
                          $scope.updateRateSelection(rate[0]);
                      }
                  }
              });

              $scope.searchRates();

              $scope.isDataLoading = false;
              $(document).trigger("HIDE_LOADING_BOX");
          },
          function (error) {
              $scope.isDataLoading = false;
              $(document).trigger("HIDE_LOADING_BOX");
          });
    }

    $scope.publishDocument = function (Document, portionMarkingRequired) {
        if ($scope.model.SelectedRevisionId == $scope.model.AvailableRevisions[0].Id) {
            $scope.export(Document, portionMarkingRequired);
        } else {
            // Only show the confirm dialog if user is not using latest
            ConfirmDialog("Old PPR&D Revision", "This document is using an older PPR&D Revision. Are you sure you want to continue publishing?", function () {
                $scope.export(Document, portionMarkingRequired);
            });
        }
    };

    $scope.export = function (Document, portionMarkingRequired) {
        $(document).trigger("SHOW_LOADING_BOX");
        setTimeout(function () { $scope.timeoutFuncRDD(); }, $scope.timeoutTime);
        DownloadFile('GenerateRDD', DocumentDetailModelView.controller, DocumentDetailModelView.publishAction, $scope.model.ProposalId, portionMarkingRequired);
    }

    $scope.timeoutFuncRDD = function () { $(document).trigger("HIDE_LOADING_BOX"); };

    window.onbeforeunload = function () {
        // Confirm leaving/refreshing page if the page is dirty
        if ($scope.documentForm.$dirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    // Initialization functions
    $scope.initialize();
}]);