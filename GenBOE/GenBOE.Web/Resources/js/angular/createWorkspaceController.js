angular.module('genboe').controller('createWorkspaceController', ['$scope', '$http', 'CreateWorkspaceModelView', '$q', '$window', '$timeout', function ($scope, $http, CreateWorkspaceModelView, $q, $window, $timeout) {
    $scope.isDataLoading = true;            // Indicates the page is still loading
    $scope.isWaitingForCallback = false;    // Indicates the page is waiting for an AJAX call to return
    $scope.step = 1;                        // Current step of the wizard
    $scope.errors = [];
    $scope.identificationPageSetup = false; // Has the identification page been setup yet.  Used to make sure the setup code is only run once when Step 3 is shown to user.
    
    // model houses the labels, dropdowns, etc for page setup
    $scope.model = {
        stepTitle: '',                      // Current Step Title for the page
        showBackButton: false,
        showNextButton: true,
        showButtonLoader: false,            // Button Loader used when clicking Create Workspace button
        hideCopyNonExactButton: false,      // RMS only used for ProjectMap
        showWorkspaceSearchBOELoader: false,// Show loader when loading BOE list for non-exact copy    
        showOCINote: false,                 // OCI note shown on Step 3 & 5 only
        showProjectMap: CreateWorkspaceModelView.ShowProjectMap,            // RMS only
        showEquivalentPersonsOption: CreateWorkspaceModelView.ShowEquivalentPersonsOption, // SSC only
        applicationUrl: CreateWorkspaceModelView.ApplicationUrl,            // used in Step 3 & 5
        lobs: CreateWorkspaceModelView.LOBs,                                // LOB array for dropdown (this one actually uses integers instead of strings for the ids)
        trackingNumbers: CreateWorkspaceModelView.TrackingNumbers,          // Tracking # array for dropdown
        copyTrackingNumbers: [],                                            // Tracking # array for copy workspace dropdown (includes original WS Tracking # if any)
        projectMapTypes: CreateWorkspaceModelView.ProjectMapTypes,          // Project Map array for dropdown
        proposalClassTypes: CreateWorkspaceModelView.ProposalClassTypes,    // Proposal Class array for dropdown
        contractTypes: CreateWorkspaceModelView.ContractTypes,              // Contract Type array for dropdown
        isSSC: CreateWorkspaceModelView.IsSSC,                              // True for SSC, false for RMS
        labelLeadPricer: CreateWorkspaceModelView.LabelLeadPricer,          // Label to use for Lead Pricer/Cost Volume Lead
        hoursLabel: 'Hours',                                                // Hours label (used in help popup), could be Hours/EPs when Equivalent Person is enabled
        originalDecimalPrecision: '',                                       // RMS only, ProjectMap
        originalCostDecimalPrecision: '',                                   // RMS only, ProjectMap
        workSpaceNameURL: CreateWorkspaceModelView.WorkSpaceNameURL,        // Url used for validation of unique workspace name
        workSpaceShortNameURL: CreateWorkspaceModelView.WorkSpaceShortNameURL,  // Url used for validation of unique workspace shortname/url
        workspaceToCopy: {
            WorkspaceName: ''
        }, // json object
        isPTMIntegrated: CreateWorkspaceModelView.IsPTMIntegrated && CreateWorkspaceModelView.IsSSC,    // SSC only
        ptmTrackingNumber: '',                      // SSC only
        selectedPtmTrackingNumber: '',              // SSC only
        isAdmin: CreateWorkspaceModelView.IsAdmin,  // SSC only
        ptmTrackingNumberNotRequired: CreateWorkspaceModelView.PtmTrackingNumberNotRequired || CreateWorkspaceModelView.IsAdmin, // SSC only TODO - remove admin part
        nextRevision: '' // SSC only, the next revision of the PTM tracking number
    };

    // data houses the data being saved and sent to the back-end
    $scope.data = {}; 
    
    $scope.resetData = function () {

        // reset all of the data
        $scope.model.workspaceToCopy = { WorkspaceName: '' };
        $scope.model.nextRevision = '';

        return {
            IsAttemptingToImport: null, // was called WorkspaceType but a better name is IsCopy (1 for copy, 0 for blank workspace)
            ResourceDecimalPrecision: null,
            CostDecimalPrecision: 2, // default is 2
            ApplicationURL: '',
            BOEsToCopy: [],                 // array of integers
            ContainsContentTemplates: null,
            ContainsOCI: null,
            ShareAndAllowSearch: null,
            ContractStartDate: '',
            ContractEndDate: '',
            CopyLaborSpreads: true,
            CopyPermissions: true,
            CopyTasks: true,
            CostVolumeLeadPricerNTID: CreateWorkspaceModelView.CostVolumeLeadPricerNTID,
            CostVolumeLeadPricerDisplayName: CreateWorkspaceModelView.CostVolumeLeadPricerDisplayName,
            Description: '',
            ProposalSubmittalDate: '',
            LineOfBusinessID: -1,                   // integer
            RFPNumber: '',
            Shortname: '',
            WorkspaceID: -1,                        // integer
            WorkspaceName: '',
            RevisionWorkspaceName: '',
            WorkspaceToCopyID: -1,                  // integer
            WSExactCopy: null,
            ProposalTitle: '',
            ProjectMapType: '1',                    // RMS only
            AllowGridEdit: false,                   // RMS only
            IsUsingEquivalentPerson: false,
            IsUsingTM: CreateWorkspaceModelView.IsSSC, // by default on for SSC, off for RMS
            TrackingNumber: $scope.model.ptmTrackingNumber, // SSC only
            ProposalClass: '-1',                    // SSC only
            SelectedContractTypes: []               // SSC only, array of strings
        };
    };

    // NOTE:  All scope variables that are used inside ng-model must be objects and not primitives so that they can be used inside of ng-switch
    // https://stackoverflow.com/a/15593652 

    /////////////////////////////////////// WIZARD STEPS /////////////////////////////////////////////

    $scope.goToStep = function (newStep) {
        $scope.errors = [];
        $('#urlValidationBox').html('');
        $scope.step = newStep;

        $scope.model.showNextButton = true;
        $scope.model.showBackButton = newStep !== 1;

        $scope.setStepSpecificElements(newStep);
    };

    $scope.setStepSpecificElements = function (newStep) {
        $scope.step = newStep;
        switch ($scope.step) {
            case 1:
                $scope.model.stepTitle = 'Workspace Type';
                $scope.model.showOCINote = false;
                $scope.model.showBackButton = false;
                break;
            case 2:
                $scope.model.stepTitle = 'Search & Copy Existing Workspace';
                $scope.model.showOCINote = false;
                $scope.model.showNextButton = true;
                break;
            case 3:
                $scope.model.stepTitle = 'Workspace Identification';
                $scope.model.showOCINote = true;
                $scope.identificationPageSetup = false;
                break;
            case 4:
                $scope.model.stepTitle = 'Share & Allow Search Settings';
                $scope.model.showOCINote = false;
                $scope.model.showNextButton = true;
                break;
            case 5:
                $scope.model.stepTitle = 'Verify';
                $scope.model.showOCINote = true;
                $scope.model.showNextButton = false;
                break;
        }
    };

    $scope.back = function () {
        $scope.errors = [];
        $('#urlValidationBox').html('');
        switch ($scope.step) {
            case 2:
                $scope.setStepSpecificElements(1);
                break;
            case 3:
                if ($scope.data.IsAttemptingToImport) {
                    $scope.setStepSpecificElements(2);
                } else {
                    $scope.setStepSpecificElements(1);
                }
                break;
            case 4:
                $scope.setStepSpecificElements(3);
                break;
            case 5:
                if ($scope.data.IsAttemptingToImport && $scope.data.WSExactCopy) {
                    // skip Workspace Identification and Share/allow search settings
                    $scope.setStepSpecificElements(2);
                } else {
                    $scope.setStepSpecificElements(4);
                }
                break;
        }

        ;
    };

    $scope.next = function () {
        if (!$scope.nextButtonDisabled()) {
            $scope.errors = [];
            $('#urlValidationBox').html('');
            switch ($scope.step) {
                case 1:
                    $scope.model.showBackButton = true;
                    if ($scope.data.IsAttemptingToImport) {
                        // clear out the PTM Tracking Number if Workspace is not selected
                        if ($scope.model.workspaceToCopy.WorkspaceName === '') {
                            $scope.model.ptmTrackingNumber = '';
                            $scope.model.selectedPtmTrackingNumber = '';
                        }
                        $scope.setStepSpecificElements(2);
                    } else {
                        // reset the data in case the user went back and forth
                        $scope.data = $scope.resetData();
                        $scope.data.IsAttemptingToImport = false;

                        if ($scope.model.isPTMIntegrated && ($scope.model.isAdmin || $scope.model.ptmTrackingNumberNotRequired) && $scope.model.ptmTrackingNumber === '') {
                            // show notification to System Admin that they did not (optionally) select a PTM Tracking Number
                            Session.confirmDialog("PTM Tracking Number", "A PTM Tracking Number was not set, are you sure you want to continue?",
                                function () { $scope.$apply(function () { $scope.setStepSpecificElements(3); }) },
                                null // user clicked cancel, do nothing
                            );
                        } else if ($scope.model.isPTMIntegrated) {
                            // if PTM is integrated, get the next tracking number revision
                            $scope.isWaitingForCallback = true;
                            var trackingPromise = $scope.getNextTrackingNumberRevision();
                            trackingPromise.then(
                                function (answer) {
                                    $scope.setStepSpecificElements(3);
                                    $scope.isWaitingForCallback = false;
                                }, function (error) {
                                    $scope.isWaitingForCallback = false;
                                });
                        } else {
                            // just go to the next step
                            $scope.setStepSpecificElements(3);
                        }
                    }
                    break;
                case 2:
                    $scope.isWaitingForCallback = true;
                    if ($scope.model.isPTMIntegrated && $scope.model.ptmTrackingNumber !== '') {
                        // If doing a copy, the user/admin has to choose a PTM Tracking # so we can safely get the new revision now
                        var trackingPromise2 = $scope.getNextTrackingNumberRevision();
                        trackingPromise2.then(
                            function (answer) {
                                $scope.step2Next();
                            }, function (error) {
                                $scope.isWaitingForCallback = false;
                            });
                    } else {
                        $scope.step2Next();
                    }

                    break;
                case 3:

                    if ($scope.model.isPTMIntegrated && $scope.model.ptmTrackingNumber !== '') {
                        // set the real workspacename if we are integrating with PTM (and admin hasn't overwritten with no PTM listed)
                        $scope.data.WorkspaceName = $scope.model.nextRevision + " " + $scope.data.RevisionWorkspaceName;
                    }

                    // need to validate the data
                    $scope.isWaitingForCallback = true;
                    var validatePromise = $scope.validateWorkspace();

                    validatePromise.then(
                        function (answer) {
                            $scope.setStepSpecificElements(4);
                            $scope.isWaitingForCallback = false;
                        }, function (error) {
                            $scope.isWaitingForCallback = false;
                        });
                    break;
                case 4:
                    $scope.setStepSpecificElements(5);
                    break;
            }
        }
    };

    $scope.step2Next = function () {
        if ($scope.data.WSExactCopy) {
            // skip Workspace Identification and Share/allow search settings steps, but still need to validate the data
            $scope.isWaitingForCallback = true;
            var promise = $scope.validateWorkspace();

            promise.then(
                function (answer) {
                    var copyPromise = $scope.copyExactDetails();
                    copyPromise.then(
                      function (answer) {
                          // go to step 5 -- Verify
                          $scope.setStepSpecificElements(5);
                          $scope.isWaitingForCallback = false;
                      }, function (error) {
                          $scope.isWaitingForCallback = false;
                      });
                }, function (error) {
                    $scope.isWaitingForCallback = false;
                });
        } else {
            // need to copy out the BOEs selected, this is old school used elsewhere so no angular
            $scope.data.BOEsToCopy = [];
            $('#BOEToCopyGrid tbody :input:checked').each(function () {
                $scope.data.BOEsToCopy.push($(this).parents('tr').attr('pkid'));
            });

            // just go to step 3
            $scope.setStepSpecificElements(3);
            $scope.isWaitingForCallback = false;
        }

        if ($scope.model.ptmTrackingNumberNotRequired && $scope.model.ptmTrackingNumber === '') {
            $scope.data.TrackingNumber = '';
            $scope.model.selectedPtmTrackingNumber = '';
        }
    };

    /////////////////////////////////////// Step 1 /////////////////////////////////////////////

    $scope.setProposalId = function () {
        var options = $('#' + $('#proposalSelect').attr('list') + ' option');

        $scope.model.ptmTrackingNumber = ''; //clear previous value

        for (var i = 0; i < options.length; i++) {
            var option = options.eq(i);
            if (option[0].value === $scope.model.selectedPtmTrackingNumber || option.attr('propId') === $scope.model.selectedPtmTrackingNumber) {
                $scope.model.ptmTrackingNumber = option.attr('propId');
                $scope.data.TrackingNumber = $scope.model.ptmTrackingNumber;
                break;
            }
        }
    };

    /////////////////////////////////////// Step 3 Identification Page /////////////////////////////////////////////

    $scope.setupIdentificationPage = function () {
        if (!$scope.identificationPageSetup) {
            $scope.identificationPageSetup = true;
            // setup cost volume pricer 
            $('#CostVolumeLeadPricerDisplayName').lookupUser({
                accountNameElementId: 'CostVolumeLeadPricerNTID',
                accountNameInitial: $scope.data.CostVolumeLeadPricerNTID,
                accountDisplayNameInitial: $scope.data.CostVolumeLeadPricerDisplayName,
                enabled: true,
                fieldDisplayName: $scope.model.labelLeadPricer,
                allowGroups: false,
                checkNameCallbackHandler: function (valid) { }
            });

            if (!$scope.model.isPTMIntegrated || $scope.model.ptmTrackingNumber === '') {
                // bind the workspacename and url for finding duplicates when no PTM Integration or Admin doesn't select Tracking Number
                CreateWorkspace.CommonBindings();
            }

            if ($scope.data.RteSizeLimit) {
                $scope.displayApproxPages($scope.data.RteSizeLimit);
            }
        }

        // create datepicker every time
        $timeout($scope.initAnticipatedDatepicker);
    };

    $scope.initAnticipatedDatepicker = function () {
        if (!$scope.model.isPTMIntegrated || $scope.model.isAdmin) {
            $("#ProposalSubmittalDate").datepicker({ dateFormat: 'mm/dd/yy' });
        }
    }

    $scope.workspaceNameKeyUp = function () {
        var workspaceNameText = $scope.data.WorkspaceName.toLowerCase();
        var workspaceNameCharArray = workspaceNameText.split("");
        var whiteList = "abcdefghijklmnopqrstuvwxyz0123456789-";
        for (index in workspaceNameCharArray) {
            if (whiteList.indexOf(workspaceNameCharArray[index]) < 0) {
                workspaceNameCharArray[index] = "_";
            }
        }
        var URLText = workspaceNameCharArray.join("");
        if (URLText.length > 15) {
            URLText = URLText.substr(0, 15);
        }
        $scope.data.Shortname = URLText;
    };

    $scope.rteSizeKeyUp = function () {
        var input = $scope.data.RteSizeLimit;
        $scope.displayApproxPages(input);
    };

    $scope.displayApproxPages = function (input) {
        if (!input) {
            // if nothing entered, remove text
            $("#approxPages").text("");
        } else {
            if (angular.isNumber(+input) && !isNaN(input) && input >= 0 && Number.isInteger(+input)) {
                // 3000 characters per page
                var approxPages = input / 3000;
                var pagesToDisplay = 0;

                if (approxPages < 0.1) {
                    // if less than 0.1 pages, display 0 (instead of 0.0)
                    pagesToDisplay = 0;
                } else if (approxPages < 0.5) {
                    // if less than half a page, round to nearest tenth
                    pagesToDisplay = approxPages.toFixed(1);
                } else {
                    // round to nearest .25 if more than half a page
                    pagesToDisplay = Math.round(approxPages * 4) / 4;
                }

                $("#approxPages").html("Approximately " + pagesToDisplay + " pages");
            } else {
                $("#approxPages").text("Invalid value");
            }
        }

    };

    $scope.warnPrecisionChange = function () {
        if ($scope.data.IsAttemptingToImport) {
            // if we are doing a workspace copy..
            Session.confirmDialog("Resource Decimal Precision", "Decimal precision has been modified for this workspace copy.  This will cause the entire workspace to recalculate.  This is a long running process.  You will be redirected to your new workspace once the recalculation is complete.  Are you sure you want to make the change?",
                 // Do nothing if they give postive confirmation.
                 null,
                 function () {
                     // Negative confirmation. Restore old Resource Decimal Precision value.
                     $scope.$apply(function () {
                         $scope.data.ResourceDecimalPrecision = $scope.model.originalDecimalPrecision;
                     });
                 });
        }
    };

    $scope.warnCostPrecisionChange = function () {
        if ($scope.data.IsAttemptingToImport) {
            // if we are doing a workspace copy..
            Session.confirmDialog("Cost Decimal Precision", "Cost decimal precision has been modified for this workspace copy.  This will cause the entire workspace to recalculate.  This is a long running process.  You will be redirected to your new workspace once the recalculation is complete.  Are you sure you want to make the change?",
                 // Do nothing if they give postive confirmation.
                 null,
                 function () {
                     // Negative confirmation. Restore old Resource Decimal Precision value.
                     $scope.$apply(function () {
                         $scope.data.CostDecimalPrecision = $scope.model.originalCostDecimalPrecision;
                     });
                 });
        }
    };

    /* Gets a date string in MM/yyyy format from a date. */
    $scope.getDateStringFromDate = function (date) {
        var dateMonth = date.getMonth() + 1;
        var leadingZero = (dateMonth < 10) ? '0' : '';
        return leadingZero + dateMonth + '/' + date.getFullYear();
    };

    $scope.setDecimalPrecisionDefaults = function () {
        if ($scope.data.ProjectMapType === '3' || $scope.data.ProjectMapType === '4') {

            $scope.data.CostDecimalPrecision = CreateWorkspaceModelView.ProjectMapPrecisionDefault;

            if ($scope.data.ResourceDecimalPrecision === null || $scope.data.ResourceDecimalPrecision === '') {
                $scope.data.ResourceDecimalPrecision = CreateWorkspaceModelView.ProjectMapPrecisionDefault;
            }

            var today = new Date();
            var nextMonth = new Date();
            nextMonth.setMonth(nextMonth.getMonth() + 1);

            $scope.data.ContractStartDate = $scope.getDateStringFromDate(today);
            $scope.data.ContractEndDate = $scope.getDateStringFromDate(nextMonth);
        }
        else {
            $scope.data.ContractStartDate = '';
            $scope.data.ContractEndDate = '';
        }
    };

    /////////////////////////////////////// Step 5 Verify Page /////////////////////////////////////////////

    $scope.getLOB = function () {
        var lob = $scope.model.lobs.find(function (item) {
            return item.Id === $scope.data.LineOfBusinessID;
        });

        if (lob) {
            return lob.Text;
        }

        return '';
    };

    $scope.getProposalClass = function () {
        var proposal = $scope.model.proposalClassTypes.find(function (item) {
            return item.Id === $scope.data.ProposalClass;
        });

        if (proposal) {
            return proposal.Text;
        }

        return '';
    };

    $scope.getSelectedContractTypes = function () {
        var selected = [];
        $scope.model.contractTypes.forEach(function (item) {
            var found = $scope.data.SelectedContractTypes.find(function (contract) {
                return contract === item.Id;
            });

            if (found) {
                selected.push(item.Text);
            }
        });

        return selected.join(", ");
    };

    $scope.getProjectMapTypeName = function () {
        var pm = $scope.model.projectMapTypes.find(function (item) {
            return item.Value === $scope.data.ProjectMapType;
        });

        if (pm) {
            return pm.Text;
        }

        return '';
    };

    $scope.convertBool = function (val) {
        if (val) {
            return "Yes";
        } else {
            return "No";
        }
    };

    // When doing an Exact copy, copy the details so that they are shown on the Verify Page
    $scope.copyExactDetails = function () {

        // retrieve exact copy details from server for duplicate name and cost volume pricer display name
        var deferred = $q.defer();
        var getExactDetailsUrl = CreateSystemAdminWithParmsPostURL(CreateWorkspaceModelView.Controller, CreateWorkspaceModelView.GetExactCopyDataAction, $scope.model.workspaceToCopy.WorkspaceID);

        $http({
            method: 'POST',
            url: getExactDetailsUrl,
            data: $scope.data
        }).then(function successCallback(response) {
            if (!$scope.model.isPTMIntegrated || ($scope.model.ptmTrackingNumberNotRequired && $scope.model.ptmTrackingNumber === '')) {
                // only copy if not PTM Integrated or original does not have a tracking number
                $scope.data.WorkspaceName = response.data.Name;
                $scope.data.Shortname = response.data.ShortName;
                $scope.data.LineOfBusinessID = response.data.LOBId;
                $scope.data.ProposalClass = response.data.ProposalClass;
                $scope.data.SelectedContractTypes = response.data.ContractTypes;
            }

            $scope.data.CostVolumeLeadPricerDisplayName = response.data.DisplayName;
            

            deferred.resolve();
        },
            function errorCallback(error) {
                if (error && error.data && error.data.MessageList) {
                    $scope.errors = error.data.MessageList;
                }
                deferred.reject(error);
            }
        );

        return deferred.promise;
    };

    $scope.create = function () {
        $scope.errors = [];
        $('#urlValidationBox').html('');
        $scope.model.showButtonLoader = true;
        // create the workspace
        var createUrl = CreateSystemAdminPostURL(CreateWorkspaceModelView.Controller, CreateWorkspaceModelView.CreateWorkspaceAction);
        $http({
            method: 'POST',
            url: createUrl,
            data: $scope.data
        }).then(function successCallback(response) {
                $window.location.href = '/' + $scope.data.Shortname;
            },
            function errorCallback(error) {
                $scope.model.showButtonLoader = false;
                if (error && error.data && error.data.MessageList) {
                    if (error.data.MessageList.length > 0) {
                        $scope.errors = error.data.MessageList;
                    } else if (error.data.Message) {
                        var newError = { ValidationIssue: error.data.Message };
                        $scope.errors.push(newError);
                    }
                }
            }
        );
    };

    $scope.getWorkspaceType = function () {
        if ($scope.data.IsAttemptingToImport) {
            return "Copy from an existing Workspace.";
        } else {
            return "Create a new blank Workspace.";
        }
    };

    /////////////////////////////////////// Navigation Buttons /////////////////////////////////////////////

    $scope.cancel = function () {
        Session.confirmDialog("Cancel", "Are you sure you want to cancel all changes?", $scope.cancelChanges, null);
    };

    $scope.cancelChanges = function () {
        // Gobbles up the "Are you sure you want to leave this page message" which shows up right after our own version of the message.
        window.onbeforeunload = null;
        $window.location.href = '/';
    };

    $scope.nextButtonDisabled = function () {
        if ($scope.isDataLoading || $scope.isWaitingForCallback) {
            return true;
        } else if ($scope.step === 1 && $scope.data.IsAttemptingToImport === null) { //here
            return true;
        } else if ($scope.step === 1 && $scope.model.isPTMIntegrated && !$scope.model.isAdmin && $scope.data.IsAttemptingToImport === false && $scope.model.ptmTrackingNumber === '') {
            // if we are on the first step, PTM is integrated, we are not system admin, chose blank workspace, and have not chosen a PTM Tracking number yet then disable the next button
            return true;
        } else if ($scope.step === 2 && ($scope.model.workspaceToCopy.WorkspaceName === '' || $scope.data.WSExactCopy === null || ($scope.model.isPTMIntegrated && $scope.model.ptmTrackingNumber === '' && !$scope.model.ptmTrackingNumberNotRequired))) {
            // if we are on the 2nd step and one of the following occurs, then disable the next button
                // Exact Copy radio button not selected
                // WorkspaceName not selected meaning no workspace to copy has been selected
                // If PTM integrated, no PTM Tracking number selected yet
            return true;
        } else if ($scope.step === 2 && $scope.model.showWorkspaceSearchBOELoader && $scope.data.WSExactCopy === false) {
            // If we are on the second step and Non-Exact has been selected we still need to wait for the BOE list to be loaded
            return true;
        } else if ($scope.step === 4 && ($scope.data.ContainsContentTemplates === null || $scope.data.ContainsOCI === null || $scope.data.ShareAndAllowSearch === null)) {
            return true;
        } else {
            return false;
        }
    };

    $scope.validateWorkspace = function () {
        var deferred = $q.defer();
        // use default for the workspace name since this is not a real workspace yet
        var validateUrl = CreateSystemAdminPostURL(CreateWorkspaceModelView.Controller, CreateWorkspaceModelView.ValidateIndentificationAction);
        $http({
            method: 'POST',
            url: validateUrl,
            data: $scope.data
        }).then(function successCallback(response) {
                deferred.resolve();
            },
            function errorCallback(error) {
                if (error && error.data && error.data.MessageList) {
                    $scope.errors = error.data.MessageList;
                }
                deferred.reject(error);
            }
        );

        return deferred.promise;
    };

    $scope.getNextTrackingNumberRevision = function () {
        var deferred = $q.defer();
        // use default for the workspace name since this is not a real workspace yet
        var nextRevisionUrl = CreateSystemAdminPostURL(CreateWorkspaceModelView.Controller, CreateWorkspaceModelView.GetNextTrackingNumberRevisionAction);
        var postData = { trackingNumber: $scope.model.ptmTrackingNumber };
        $http({
                method: 'POST',
                url: nextRevisionUrl,
                data: postData
        }).then(function successCallback(response) {
                $scope.model.nextRevision = response.data.TrackingNumberRevision;
                $scope.data.WorkspaceName = response.data.TrackingNumberRevision;
                $scope.data.Shortname = response.data.TrackingNumberRevision;
                $scope.data.RFPNumber = response.data.RFPNumber;
                $scope.data.ProposalTitle = response.data.Title;
                $scope.data.ProposalClass = response.data.ProposalClassId;
                $scope.data.LineOfBusinessID = response.data.LOBId;
                $scope.data.SelectedContractTypes = response.data.ContractTypes;
                $scope.data.ProposalSubmittalDate = response.data.AnticipatedDeliveryDate;
                $scope.data.RevisedSubmittalDate = response.data.RevisedSubmittalDate;
                deferred.resolve();
            },
            function errorCallback(error) {
                if (error && error.data && error.data.MessageList) {
                    $scope.errors = error.data.MessageList;
                }
                deferred.reject(error);
            }
        );

        return deferred.promise;
    };

    /////////////////////////////////////// Step 4 OCI/Share Page /////////////////////////////////////////////

    $scope.updateOCI = function () {
        if ($scope.data.ContainsOCI) {
            $scope.data.ContainsContentTemplates = false;
            $scope.data.ShareAndAllowSearch = false;
        }
    };

    $scope.updateContainsContentTemplates = function () {
        if ($scope.data.ContainsContentTemplates) {
            $scope.data.ShareAndAllowSearch = true;
        }
    };

    /////////////////////////////////////// Step 2 Copy Workspace Page /////////////////////////////////////////////

    $scope.updateCopy = function () {
        if ($scope.data.WSExactCopy && $scope.model.workspaceToCopy.WorkspaceName !== '') {
            $scope.data.ContainsOCI = $scope.model.workspaceToCopy.ContainsOCI;
            $scope.data.ContainsContentTemplates = $scope.model.workspaceToCopy.ContainsContentTemplates;
            $scope.data.ShareAndAllowSearch = $scope.model.workspaceToCopy.ShareAndAllowSearch;
        } else {
            $scope.data.ContainsOCI = null;
            $scope.data.ContainsContentTemplates = null;
            $scope.data.ShareAndAllowSearch = null;
        }
    };

    $scope.searchForWorkspace = function () {
        $(document).trigger('WorkspaceSearch');
        WorkspaceSearchWidget.clearValidationBox($("#WorkspaceSearchForm ul.validation-box"));
    };

    $scope.getWorkspaceToCopyBOEList = function (copyDetails) {
        var deferred = $q.defer();

        $scope.model.showWorkspaceSearchBOELoader = true;

        var postURL = CreateSystemAdminPostURL(CreateWorkspaceModelView.Controller, CreateWorkspaceModelView.GetWorkspaceToCopyBOEListAction);

        $http({
            method: 'POST',
            url: postURL,
            data: copyDetails
        }).then(function successCallback(response) {
                var SearchResults = $('#WorkspaceSearchBOEList');
                SearchResults.html(response.data);
                $scope.model.showWorkspaceSearchBOELoader = false;
                deferred.resolve();
            },
            function errorCallback(error) {
                if (error && error.data && error.data.MessageList) {
                    $scope.errors = error.data.MessageList;
                }
                deferred.reject(error);
            }
        );

        return deferred.promise;
    };

    $scope.getWorkspaceToCopyDetails = function (copyDetails) {
        var deferred = $q.defer();
        var postURL = CreateSystemAdminPostURL(CreateWorkspaceModelView.Controller, CreateWorkspaceModelView.GetWorkspaceToCopyDetailsAction);

        $http({
            method: 'POST',
            url: postURL,
            data: copyDetails
        }).then(function successCallback(response) {
            var result = response.data;
            $scope.model.workspaceToCopy = result;
            if (result.Description !== null) {
                $scope.data.Description = result.Description;
            }

            $scope.data.WorkspaceName = result.WorkspaceName;
            $scope.data.Shortname = result.ShortName;
            $scope.data.ResourceDecimalPrecision = result.ResourceDecimalPrecision;
            $scope.model.originalDecimalPrecision = result.ResourceDecimalPrecision;
            $scope.data.CostDecimalPrecision = result.CostDecimalPrecision;
            $scope.model.originalCostDecimalPrecision = result.CostDecimalPrecision;
            $scope.data.ContractStartDate = result.ContractStartDate;
            $scope.data.ContractEndDate = result.ContractEndDate;            

            if (result.ProposalSubmittalDate !== null) {
                $scope.data.ProposalSubmittalDate = result.ProposalSubmittalDate;
            }

            // RMS only for NonExact copy, copy the LOB
            // Exact for SSC & RMS copies the LOB in another retrieval 
            if (!$scope.model.isSSC) {
                $scope.data.LineOfBusinessID = result.LineOfBusinessID;
            }

            $scope.model.copyTrackingNumbers = $scope.model.trackingNumbers.slice();
            if (result.TrackingNumber !== null) {
                $scope.data.TrackingNumber = result.TrackingNumber;
                $scope.model.ptmTrackingNumber = result.TrackingNumber;
                $scope.model.selectedPtmTrackingNumber = result.TrackingNumber;

                // check to see if we need to add the tracking number to the array list
                var found = false;
                $scope.model.trackingNumbers.forEach(function (item) {
                    if (item.Value === result.TrackingNumber) {
                        found = true;
                    }
                });

                if (!found) {
                    var originalTrackingNumber = {
                        Text: result.TrackingNumber,
                        Value: result.TrackingNumber
                    }
                    $scope.model.copyTrackingNumbers.unshift(originalTrackingNumber);
                }
            }

            if (result.RFPNumber !== null) {
                $scope.data.RFPNumber = result.RFPNumber;
            }

            $scope.data.AllowGridEdit = result.AllowGridEdit;
            $scope.data.ProjectMapType = result.ProjectMapType.toString(); // comes in as integer, but we need string
            $scope.data.IsUsingEquivalentPerson = result.IsUsingEquivalentPerson;
            $scope.data.IsUsingTM = result.IsUsingTM;
            $scope.data.RteSizeLimit = result.RteSizeLimit;
                
            if ($scope.data.WSExactCopy) {
                $scope.data.ContainsOCI = result.ContainsOCI;
                $scope.data.ContainsContentTemplates = result.ContainsContentTemplates;
                $scope.data.ShareAndAllowSearch = result.ShareAndAllowSearch;
            }

            deferred.resolve();
        },
        function errorCallback(error) {
            if (error && error.data && error.data.MessageList) {
                $scope.errors = error.data.MessageList;
            }
            deferred.reject(error);
        });

        return deferred.promise;
    };

    $scope.workspaceToCopyChosen = function (event, data) {
        $scope.model.WorkspaceToCopy = data.workspaceName;
        $scope.data.WorkspaceToCopyID = data.workspaceID;
        
        if (!data.isProjectMap.isTrue()) {
            $scope.getWorkspaceToCopyBOEList(data);
        }

        $scope.getWorkspaceToCopyDetails(data);

        if (data.isProjectMap.isTrue()) {
            // Hide the second Radio button for non-exact copy
            $scope.model.hideCopyNonExactButton = true;

            // Click the first radio button since it is the only choice, this also fixes an issue if the second radio button was previously selected
            $scope.data.WSExactCopy = true;
        } else {
            // Show the second Radio button for non-exact copy
            $scope.model.hideCopyNonExactButton = false;
        }
    };

    $scope.initialize = function () {
        $scope.data = $scope.resetData();
        $scope.setStepSpecificElements(1);

        // add initial option for LOBs
        var lob = {
            IsActive: true,
            Id: -1,
            Text: 'Select Line of Business'
        };
        $scope.model.lobs.unshift(lob);

        $scope.setDecimalPrecisionDefaults();

        CreateWorkspace.registerForEvent('WorkspaceToCopyChosen', $scope.workspaceToCopyChosen);

        if ($scope.model.isSSC) {
            if ($scope.model.showEquivalentPersonsOption) {
                $scope.model.hoursLabel = 'Hours/EPs';
            }

            if (CreateWorkspaceModelView.TrackingNumber !== '') {
                $scope.data.IsAttemptingToImport = false;
                $scope.model.ptmTrackingNumber = CreateWorkspaceModelView.TrackingNumber;
                $scope.data.TrackingNumber = CreateWorkspaceModelView.TrackingNumber;
                var trackingPromise = $scope.getNextTrackingNumberRevision();
                trackingPromise.then(
                    function (answer) {
                        // Start the user at the Workspace Identification step if they pass in a TrackingNumber
                        $scope.setStepSpecificElements(3);
                        $scope.isDataLoading = false;
                        $scope.isWaitingForCallback = false;
                    }, function (error) {
                        // Something bad happened, so just start out at step 1
                        $scope.isDataLoading = false;
                        $scope.isWaitingForCallback = false;
                    });
            } else {
                $scope.isDataLoading = false;
            }
        } else {
            $scope.isDataLoading = false;
        }
    };

    $scope.initialize();
    
}]);